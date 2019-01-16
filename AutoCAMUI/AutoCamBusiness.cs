using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    public abstract class AutoCamBusiness
    {
        static NXOpen.UF.UFSession ufSession = NXOpen.UF.UFSession.GetUFSession();
        /// <summary>
        /// 自动编程
        /// </summary>
        public static void AutoCAM()
        {
            Helper.ShowMsg("正在匹配图档...");
            var ConfigData = EactConfig.ConfigData.GetInstance();
            EactTool.FileHelper.InitFileMode(true ? 1 : 0, ConfigData.FTP.Address, "", ConfigData.FTP.User, ConfigData.FTP.Pass, false, "/Eact_AutoCNC", @"Temp\Eact_AutoCNC", @"Temp\Eact_AutoCNC_Error");
            var path = string.Empty;
            var fileName = EactTool.FileHelper.FindFile(path);
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    AutoCAM(fileName);
                    EactTool.FileHelper.DeleteFile(path, fileName);
                }
                catch (Exception ex)
                {
                    Helper.ShowMsg("自动编程异常:" + ex.Message, 1);
                    EactTool.FileHelper.WriteErrorFile(path, fileName, ex.Message);
                }
            }
        }

        /// <summary>
        /// 自动编程
        /// </summary>
        static void AutoCAM(string filename)
        {
            
            var part = NXOpen.Session.GetSession().Parts.Work;
            if (part != null)
            {
                Snap.NX.Part.Wrap(part.Tag).Close(true, true);
            }
            
            Snap.NX.Part snapPart = Snap.NX.Part.OpenPart(filename);
            AutoCAMUI.Helper.InitCAMSession("WsqAutoCAM");
            var name = Path.GetFileNameWithoutExtension(filename);
            Snap.Globals.WorkPart = snapPart;
            try
            {
                var body = snapPart.Bodies.FirstOrDefault();
                Helper.ShowMsg(string.Format("{0}开始自动编程...", name));
                var ele = ElecManage.Electrode.GetElectrode(body);
                if (ele != null)
                {
                    ele.InitAllFace();
                    AutoCAMUI.Helper.AutoCAM(ele);
                    Helper.ShowMsg(string.Format("{0}自动编程完成", name));
                }
            }
            catch (Exception ex)
            {
                Helper.ShowMsg(string.Format("{0}自动编程错误【{1}】", name, ex.Message));
                Console.WriteLine("自动编程错误:{0}", ex.Message);
                throw ex;
            }
            finally
            {
                snapPart.Close(true, true);
            }
        }


        /// <summary>
        /// 自动编程
        /// </summary>
        public static void AutoCam(CAMElectrode ele, CNCConfig.CAMConfig camConfig)
        {
            //安全距离
            var safeDistance = 10;
            var autoBlankOffset = new double[] { 2, 2, 2, 2, 2, 0 };
            var eleInfo = ele.Electrode.GetElectrodeInfo();
            var bodyBox = ele.BodyBox;

            //几何组根节点
            NXOpen.Tag geometryGroupRootTag;
            //程序组根节点
            NXOpen.Tag orderGroupRootTag;
            //刀具组根节点
            NXOpen.Tag cutterGroupRootTag;
            //方法组根节点
            NXOpen.Tag methodGroupRootTag;
            //几何体组
            NXOpen.Tag workGeometryGroupTag;


            //TODO 初始化对象
            NXOpen.Tag setup_tag;
            ufSession.Setup.AskSetup(out setup_tag);
            ufSession.Setup.AskGeomRoot(setup_tag, out geometryGroupRootTag);
            ufSession.Setup.AskProgramRoot(setup_tag, out orderGroupRootTag);
            ufSession.Setup.AskMctRoot(setup_tag, out cutterGroupRootTag);
            ufSession.Setup.AskMthdRoot(setup_tag, out methodGroupRootTag);

            //根据配置文件创建刀具
            CreateCutter(ele, camConfig, cutterGroupRootTag);

            //TODO删除旧的程序

            //TODO 创建坐标系和几何体
            //加工坐标系
            NXOpen.Tag workMcsGroupTag;
            ufSession.Ncgeom.Create(AUTOCAM_TYPE.mill_planar, AUTOCAM_SUBTYPE.MCS, out workMcsGroupTag);
            ufSession.Obj.SetName(workMcsGroupTag, AUTOCAM_ROOTNAME.GEOM_EACT);
            ufSession.Ncgroup.AcceptMember(geometryGroupRootTag, workMcsGroupTag);

            //TODO 设置安全平面
            var normal = new Snap.Vector(0, 0, 1);
            var origin = new Snap.Position((bodyBox.MinX + bodyBox.MaxX) / 2, (bodyBox.MinY + bodyBox.MaxY) / 2, bodyBox.MaxZ + safeDistance);
            ufSession.Cam.SetClearPlaneData(workMcsGroupTag, origin.Array, normal.Array);

            //TODO 创建几何体
            ufSession.Ncgeom.Create(AUTOCAM_TYPE.mill_planar, AUTOCAM_SUBTYPE.WORKPIECE, out workGeometryGroupTag);
            ufSession.Obj.SetName(workGeometryGroupTag, eleInfo.Elec_Name);
            ufSession.Ncgroup.AcceptMember(workMcsGroupTag, workGeometryGroupTag);

            //TODO 添加Body作为工作几何体
            Helper.SetCamgeom(NXOpen.UF.CamGeomType.CamPart, workGeometryGroupTag, new List<NXOpen.Tag> { ele.Electrode.ElecBody.NXOpenTag });

            //TODO 设置毛坯为自动块
            ufSession.Cam.SetAutoBlank(workGeometryGroupTag, NXOpen.UF.UFCam.BlankGeomType.AutoBlockType, autoBlankOffset);

            Action<string, double,int> action = (type, fireNum, number) => {
                //TODO 创建程序
                NXOpen.Tag programGroupTag;
                ufSession.Ncprog.Create(AUTOCAM_TYPE.mill_planar, AUTOCAM_SUBTYPE.PROGRAM, out programGroupTag);
                ufSession.Obj.SetName(programGroupTag,string.Format("{0}-{1}", eleInfo.Elec_Name,type));
                ufSession.Ncgroup.AcceptMember(orderGroupRootTag, programGroupTag);
            };

            if (eleInfo.FINISH_NUMBER > 0)  //精
            {
                action("F", eleInfo.FINISH_SPACE, eleInfo.FINISH_NUMBER);
            }

            if (eleInfo.MIDDLE_NUMBER > 0)  //中
            {
                action("M", eleInfo.MIDDLE_SPACE, eleInfo.MIDDLE_NUMBER);
            }

            if (eleInfo.ROUGH_NUMBER > 0)  //粗
            {
                action("R", eleInfo.ROUGH_SPACE, eleInfo.ROUGH_NUMBER);
            }
        }

        public static void CreateCutter(CAMElectrode ele, CNCConfig.CAMConfig camConfig, NXOpen.Tag cutterGroupRootTag)
        {
            var info = ele.Electrode.GetElectrodeInfo();
            var cutterConfigs = camConfig.FindCutterInfo(info.MAT_NAME);
            var project = camConfig.Projects.Where(u => u.方案名称 == "自动").FirstOrDefault() ?? camConfig.Projects.Where(u => u.方案名称 == "自动").FirstOrDefault();
            if (project == null)
            {
                throw new Exception("配置工具未配置方案!");
            }

            var cutterStrs = new List<string>();
            project.Details.ForEach(u => {
                cutterStrs.Add(u.刀具);
                cutterStrs.Add(u.参考刀具);
            });
            cutterStrs = cutterStrs.Where(u => !string.IsNullOrEmpty(u)).ToList();
            cutterStrs = cutterStrs.Distinct().ToList();
            foreach (var item in cutterStrs)
            {
                var cutterConfig = cutterConfigs.FirstOrDefault(m => m.刀具名称 == item);
                if (cutterConfig != null)
                {
                    var camCutter = new CAMCutter();
                    camCutter.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
                    camCutter.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
                    camCutter.CutterName = cutterConfig.刀具名称;
                    camCutter.TL_DIAMETER = double.Parse(cutterConfig.直径);
                    camCutter.TL_COR1_RAD = double.Parse(cutterConfig.R角);
                    camCutter.TL_HEIGHT = double.Parse(cutterConfig.刀长);
                    camCutter.TL_FLUTE_LN = double.Parse(cutterConfig.刃长);
                    camCutter.TL_NUMBER = int.Parse(cutterConfig.刀号);
                    camCutter.Speed = double.Parse(cutterConfig.转速);
                    camCutter.FeedRate = double.Parse(cutterConfig.进给);
                    camCutter.FEED_TRAVERSAL = double.Parse(cutterConfig.横越);
                    camCutter.CutDepth = double.Parse(cutterConfig.切深);
                    Helper.CreateCutter(new List<CAMCutter> { camCutter }, cutterGroupRootTag);
                }
                else
                {
                    throw new Exception("配置工具方案刀具配置异常!");
                }
            }
        }
    }
}
