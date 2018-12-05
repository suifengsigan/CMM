using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    public static class Helper
    {
        private static NXOpen.UF.UFSession ufSession = NXOpen.UF.UFSession.GetUFSession();

        /// <summary>
        /// 加载模板
        /// </summary>
        public static void ReinitOpt()
        {
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            var curBaseDir = new System.IO.DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;
            var curTemplatePrtDir = System.IO.Path.Combine(curBaseDir
                , @"MACH\resource\template_part\metric\");
            var curTemplateSetDir = System.IO.Path.Combine(curBaseDir
                , @"MACH\resource\template_set\");
            string UGII_CAM_TEMPLATE_SET_DIR = string.Empty;
            ufSession.UF.TranslateVariable("UGII_CAM_TEMPLATE_SET_DIR", out UGII_CAM_TEMPLATE_SET_DIR);
            var optFile = System.IO.Path.Combine(UGII_CAM_TEMPLATE_SET_DIR, "cam_general.opt");
            ufSession.UF.SetVariable("EACT_AUTOCAM_ELE_TEMPLATEPRT_DIR", curTemplatePrtDir);
            var eact_cam_general_optFile = System.IO.Path.Combine(curTemplateSetDir, "eact_cam_general.opt");
            if (System.IO.File.Exists(optFile))
            {
                var optFileInfo = System.IO.File.ReadAllText(optFile);
                StringBuilder str = new StringBuilder();
                str.Append(optFileInfo);
                System.IO.Directory.GetFiles(curTemplatePrtDir).ToList().ForEach(u => {
                    string tempV = "${EACT_AUTOCAM_ELE_TEMPLATEPRT_DIR}" + System.IO.Path.GetFileName(u);
                    if (!optFileInfo.Contains(tempV))
                    {
                        str.AppendLine();
                        str.AppendLine(tempV);
                    }
                });

                System.IO.File.WriteAllText(eact_cam_general_optFile, str.ToString());
            }

            ufSession.Cam.ReinitOpt(eact_cam_general_optFile);
        }

        /// <summary>
        /// 过切检查
        /// </summary>
        public static bool IsPathGouged(NXOpen.Tag oper)
        {
            var result = true;
            ufSession.Oper.IsPathGouged(oper, out result);
            return result;
        }

        /// <summary>
        /// 获取拔模角度
        /// </summary>
        public static double GetDraftAngle(this Snap.NX.Face face, Snap.Vector vector)
        {
            double[] param = new double[2], point = new double[3], u1 = new double[3], v1 = new double[3], u2 = new double[3], v2 = new double[3], unitNorm = new double[3], radii = new double[2];
            ufSession.Modl.AskFaceProps(face.NXOpenTag, param, point, u1, v1, u2, v2, unitNorm, radii);
            var angle = Snap.Vector.Angle(unitNorm, vector);
            var draftAngle = 90 - angle;
            return draftAngle;
        }
        public static void AutoCAM(Snap.NX.Body body)
        {
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

            //TODO 创建坐标系和几何体
            //加工坐标系
            NXOpen.Tag workMcsGroupTag;
            ufSession.Ncgeom.Create(AUTOCAM_TYPE.mill_planar, AUTOCAM_SUBTYPE.MCS, out workMcsGroupTag);
            ufSession.Obj.SetName(workMcsGroupTag, AUTOCAM_ROOTNAME.GEOM_EACT);
            ufSession.Ncgroup.AcceptMember(geometryGroupRootTag, workMcsGroupTag);

            //TODO 设置安全平面
            var normal = new Snap.Vector(0, 0, 1);
            var origin = new Snap.Position();
            ufSession.Cam.SetClearPlaneData(workMcsGroupTag, origin.Array, normal.Array);

            //TODO 创建几何体
            ufSession.Ncgeom.Create(AUTOCAM_TYPE.mill_planar, AUTOCAM_SUBTYPE.WORKPIECE, out workGeometryGroupTag);
            ufSession.Obj.SetName(workGeometryGroupTag, AUTOCAM_ROOTNAME.WORKPIECE_EACT);
            ufSession.Ncgroup.AcceptMember(workMcsGroupTag, workGeometryGroupTag);

            //TODO 创建程序
            NXOpen.Tag programGroupTag;
            ufSession.Ncprog.Create(AUTOCAM_TYPE.mill_planar, AUTOCAM_SUBTYPE.PROGRAM, out programGroupTag);
            ufSession.Obj.SetName(programGroupTag, AUTOCAM_ROOTNAME.PROGRAM_EACT);
            ufSession.Ncgroup.AcceptMember(orderGroupRootTag, programGroupTag);

            //TODO 添加Body作为工作几何体
            SetMillArea(NXOpen.UF.CamGeomType.CamPart, workGeometryGroupTag, new List<NXOpen.Tag> { body.NXOpenTag });

            //TODO 设置毛坯为自动块
            ufSession.Cam.SetAutoBlank(workGeometryGroupTag, NXOpen.UF.UFCam.BlankGeomType.AutoBlockType, new double[] { 0, 0, 0, 0, 0, 0 });
        }

        /// <summary>
        /// 创建刀具
        /// </summary>
        /// <returns></returns>
        public static List<CAMCutter> CreateCutter(List<CAMCutter> cutters, NXOpen.Tag cutterGroupRootTag)
        {
            var result = new List<CAMCutter>();
            foreach (var item in cutters)
            {
                NXOpen.Tag cutterTag;
                ufSession.Cutter.Create(item.AUTOCAM_TYPE, item.AUTOCAM_SUBTYPE, out cutterTag);
                ufSession.Ncgroup.AcceptMember(cutterGroupRootTag, cutterTag);
                ufSession.Obj.SetName(cutterTag, item.CutterName);
                ufSession.Param.SetDoubleValue(cutterTag, NXOpen.UF.UFConstants.UF_PARAM_TL_DIAMETER, item.TL_DIAMETER);
                item.CutterTag = cutterTag;
                result.Add(item);
            }
            return result;
        }
        
        /// <summary>
        /// 设置区域
        /// </summary>
        public static void SetMillArea(NXOpen.UF.CamGeomType camGeomType, NXOpen.Tag operTag, List<NXOpen.Tag> cutAreaGeometryTags)
        {
            var appDatas = new List<NXOpen.UF.UFCamgeom.AppData>();
            cutAreaGeometryTags.ForEach(u =>
            {
                var appData = new NXOpen.UF.UFCamgeom.AppData();
                appData.has_stock = 0;
                appData.has_cut_stock = 0;
                appData.has_tolerances = 0;
                appData.has_feedrate = 0;
                appData.has_offset = 0;
                appData.has_avoidance_type = 0;
                appData.offset = 0.1;
                appDatas.Add(appData);
            });
            ufSession.Camgeom.DeleteGeometry(operTag, camGeomType);
            ufSession.Camgeom.AppendItems(operTag, camGeomType, cutAreaGeometryTags.Count, cutAreaGeometryTags.ToArray(), appDatas.ToArray());
        }

        /// <summary>
        /// 路径生成
        /// </summary>
        public static void PathGenerate(List<NXOpen.Tag> operTags)
        {
            bool generated;
            operTags.ForEach(u =>
            {
                ufSession.Param.Generate(u, out generated);
            });
        }

        /// <summary>
        /// 显示刀具路径
        /// </summary>
        public static void ShowCutterPath(NXOpen.Tag operTag)
        {
            ufSession.Param.ReplayPath(operTag);
        }

        /// <summary>
        /// 设置参考刀具
        /// </summary>
        public static void SetReferenceCutter(NXOpen.Tag operTag,NXOpen.Tag refCutterTag)
        {
            ufSession.Oper.SetRefCutter(operTag, refCutterTag);
        }


        /// <summary>
        /// 显示编辑的对话框
        /// </summary>
        public static void ShowEditDialog(NXOpen.Tag tag, Action action = null)
        {
            int dialogResponse;
            ufSession.UiParam.EditObject(tag, out dialogResponse);
            if (dialogResponse == NXOpen.UF.UFConstants.UF_UI_APPLY || dialogResponse == NXOpen.UF.UFConstants.UF_UI_OK)
            {
                if (action != null)
                {
                    action();
                }
            }
        }

        /// <summary>
        /// 设置面边界
        /// </summary>
        public static void SetBoundaryByFace(NXOpen.UF.CamGeomType camGeomType, NXOpen.Tag operTag, NXOpen.Tag faceTag, NXOpen.UF.CamMaterialSide materialSide)
        {
            var boundary_data = new NXOpen.UF.UFCambnd.BoundaryData();
            boundary_data.boundary_type = NXOpen.UF.CamBoundaryType.CamBoundaryTypeClosed;
            boundary_data.plane_type = 1;
            boundary_data.material_side = materialSide;
            boundary_data.ignore_holes = 0;
            boundary_data.ignore_islands = 0;
            boundary_data.ignore_chamfers = 0;
            boundary_data.app_data = new NXOpen.UF.UFCambnd.AppData[] { };
            ufSession.Cambnd.AppendBndFromFace(operTag, camGeomType, faceTag, ref boundary_data);
        }
    }

    public class AUTOCAM_TYPE
    {
        public const string mill_planar = "mill_planar";
    }

    public class AUTOCAM_ROOTNAME
    {
        public const string GEOM_EACT = "GEOM_EACT";
        public const string WORKPIECE_EACT = "WORKPIECE_EACT";
        public const string PROGRAM_EACT = "PROGRAM_EACT";
    }

    public class AUTOCAM_SUBTYPE
    {
        public const string MCS = "MCS";
        public const string WORKPIECE = "WORKPIECE";
        public const string PROGRAM = "PROGRAM";
    }
}
