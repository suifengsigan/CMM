using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 加工工序类
    /// </summary>
    public class CAMOper : ICAMOper
    {
        protected NXOpen.UF.UFSession ufSession = NXOpen.UF.UFSession.GetUFSession();
        public string AUTOCAM_TYPE { get;protected set; }
        public string AUTOCAM_SUBTYPE { get { return GetEnumNameByKey(TmplateOper); } }
        public E_TmplateOper TmplateOper { get; protected set; }
        public CAMCutter CAMCutter { get; set; }
        public NXOpen.Tag WorkGeometryGroup { get; set; }
        public NXOpen.Tag ProgramGroup { get; set; }
        public NXOpen.Tag MethodGroupRoot { get; set; }
        public NXOpen.Tag OperTag { get; protected set; }
        
        /// <summary>
        /// 工序是否可用
        /// </summary>
        public virtual bool OperIsValid { get { return OperTag != NXOpen.Tag.Null; } }
        /// <summary>
        /// 工序名称
        /// </summary>
        protected string _operName {
            get
            {
                if (OperIsValid)
                {
                    string name;
                    ufSession.Obj.AskName(OperTag, out name);
                    return name;
                }
                return string.Empty;
            }
        }
        /// <summary>
        /// 火花位
        /// </summary>
        public double FRIENUM { get; set; }

        public static List<CAMOper> CreateCamOper(
            NXOpen.Tag WorkGeometryGroup,
            NXOpen.Tag ProgramGroup, 
            NXOpen.Tag MethodGroupRoot,
            NXOpen.Tag cutterGroupRootTag,
            CAMElectrode ele,
            CNCConfig.CAMConfig.ProjectInfo project,
            List<CAMCutter> cutterDetails
            )
        {
            var result = new List<CAMOper>();
            foreach (var item in project.Details)
            {
                var operConfig = ele.CamConfig.Operations.FirstOrDefault(m => m.显示名称 == item.工序);
                var cutter = cutterDetails.FirstOrDefault(m => m.CutterName == item.刀具);
                var refCutter = cutterDetails.FirstOrDefault(m => m.CutterName == item.参考刀具);

                if (operConfig == null)
                {
                    throw new Exception("配置工具方案工序配置异常！");
                }
                ICAMOper camOper = null;
                switch (operConfig.模版类型)
                {
                    case CNCConfig.CAMConfig.S_OperationTemplate.EACT_AUTOCAM:
                        {
                            break;
                        }
                    default:
                        {
                            switch (GetEnumByKey(operConfig.模板名称))
                            {
                                case E_TmplateOper.FACE_MILLING_TOP:
                                    {
                                        camOper = new WsqAutoCAM_FACE_MILLING_TOP_Oper();
                                        break;
                                    }
                                case E_TmplateOper.CAVITY_MILL_C:
                                    {
                                        camOper = new WsqAutoCAM_CAVITY_MILL_C_Oper();
                                        break;
                                    }
                                case E_TmplateOper.CAVITY_PLANAR_MILL:
                                    {
                                        camOper = new WsqAutoCAM_CAVITY_PLANAR_MILL_Oper();
                                        break;
                                    }
                                case E_TmplateOper.CAVITY_MILL_REF:
                                    {
                                        camOper = new WsqAutoCAM_CAVITY_MILL_REF_Oper();
                                        break;
                                    }
                                case E_TmplateOper.FACE_MILLING_BASE:
                                    {
                                        camOper = new WsqAutoCAM_FACE_MILLING_BASE_Oper();
                                        break;
                                    }
                                case E_TmplateOper.FACE_MILLING:
                                    {
                                        camOper = new WsqAutoCAM_FACE_MILLING_Oper();
                                        break;
                                    }
                                case E_TmplateOper.PLANAR_MILL_BASE:
                                    {
                                        camOper = new WsqAutoCAM_PLANAR_MILL_BASE_Oper();
                                        break;
                                    }
                                case E_TmplateOper.PLANAR_MILL:
                                    {
                                        camOper = new WsqAutoCAM_PLANAR_MILL_Oper();
                                        break;
                                    }
                                case E_TmplateOper.ZLEVEL_PROFILE_STEEP:
                                    {
                                        camOper = new WsqAutoCAM_ZLEVEL_PROFILE_STEEP_Oper();
                                        break;
                                    }
                                case E_TmplateOper.ZLEVEL_CORNER:
                                    {
                                        camOper = new WsqAutoCAM_ZLEVEL_CORNER_Oper();
                                        break;
                                    }
                                case E_TmplateOper.CONTOUR_AREA_NON_STEEP:
                                    {
                                        camOper=new WsqAutoCAM_CONTOUR_AREA_NON_STEEP_Oper();
                                        break;
                                    }
                                case E_TmplateOper.FLOWCUT_REF_TOOL:
                                    {
                                        camOper = new WsqAutoCAM_FLOWCUT_REF_TOOL_Oper();
                                        break;
                                    }
                                case E_TmplateOper.FACE_MILLING_CORNER:
                                    {
                                        camOper = new WsqAutoCAM_FACE_MILLING_CORNER_Oper();
                                        break;
                                    }
                                case E_TmplateOper.CONTOUR_TEXT:
                                    {
                                        camOper = new WsqAutoCAM_CONTOUR_TEXT_Oper();
                                        break;
                                    }
                            }
                            break;
                        }
                }


                if (camOper != null)
                {
                    camOper.AutoAnalysis(ele, WorkGeometryGroup, ProgramGroup, MethodGroupRoot,cutter, refCutter);
                    if (item.切深 > 0)
                    {
                        camOper.SetCutDepth(item.切深);
                    }

                    if (item.进给 > 0)
                    {
                        camOper.SetFeedRate(item.进给);
                    } 
                }
            }
            return result;
        }

        /// <summary>
        /// 设置进给
        /// </summary>
        public virtual void SetFeedRate(double feedRate)
        {
            if (OperIsValid)
            {
                Helper.SetFeedRate(OperTag, feedRate);
            }
        }

        /// <summary>
        /// 根据电极分析数据设置工序相关参数
        /// </summary>
        protected virtual void AutoSet(CAMElectrode ele)
        {

        }

        /// <summary>
        /// 根据电极分析数据判断该工序是否可用
        /// </summary>
        protected virtual bool AnalysisOperIsValid(CAMElectrode ele)
        {
            return true;
        }

        /// <summary>
        /// 根据类型自动设置相关参数
        /// </summary>
        public virtual void AutoAnalysis(CAMElectrode ele, NXOpen.Tag WorkGeometryGroup, NXOpen.Tag ProgramGroup, NXOpen.Tag MethodGroupRoot
            , CAMCutter CAMCutter, CAMCutter refCAMCutter
            )
        {
            if (AnalysisOperIsValid(ele))
            {
                CreateOper(WorkGeometryGroup, ProgramGroup, MethodGroupRoot, CAMCutter);
                if (refCAMCutter != null)
                {
                    SetReferenceCutter(refCAMCutter);
                }
                AutoSet(ele);
            }
        }

       

        public void CreateOper(NXOpen.Tag WorkGeometryGroup, NXOpen.Tag ProgramGroup, NXOpen.Tag MethodGroupRoot, CAMCutter CAMCutter,bool operIsValid=true)
        {
            this.WorkGeometryGroup = WorkGeometryGroup;
            this.ProgramGroup = ProgramGroup;
            this.MethodGroupRoot = MethodGroupRoot;
            this.CAMCutter = CAMCutter;

            if (operIsValid)
            {
                //TODO 创建工序
                var operTag = CreateOper();

                int count;
                NXOpen.Tag[] list;
                ufSession.Ncgroup.AskMemberList(ProgramGroup, out count, out list);
                int index = -1;
                string name;
                for (int i = 0; i < count; i++)
                {
                    ufSession.Obj.AskName(list[i], out name);
                    if (name.Contains(AUTOCAM_SUBTYPE) && operTag != list[i])
                    {
                        var strIndex = name.Split('_').LastOrDefault();
                        var tempInt = -1;
                        int.TryParse(strIndex, out tempInt);
                        if (tempInt > index)
                        {
                            index = tempInt;
                        }
                    }
                }

                //设置名称
                ufSession.Obj.SetName(operTag, string.Format("{0}_{1}", AUTOCAM_SUBTYPE, index + 1));

                //设置进给率和速度
                _SetFeedRate(CAMCutter.FeedRate, CAMCutter.Speed);

                //设置切深
                SetCutDepth(CAMCutter.CutDepth);

                //设置横越(移刀)
                Helper.SetFeedTraversal(OperTag, CAMCutter.FEED_TRAVERSAL);
            }
        }

        /// <summary>
        /// 创建工序
        /// </summary>
        NXOpen.Tag CreateOper()
        {
            //TODO 创建工序
            NXOpen.Tag operTag;
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            ufSession.Oper.Create(AUTOCAM_TYPE, AUTOCAM_SUBTYPE, out operTag);
            ufSession.Ncgroup.AcceptMember(WorkGeometryGroup, operTag);
            ufSession.Ncgroup.AcceptMember(ProgramGroup, operTag);
            ufSession.Ncgroup.AcceptMember(MethodGroupRoot, operTag);
            ufSession.Ncgroup.AcceptMember(CAMCutter.CutterTag, operTag);
            OperTag = operTag;
            return operTag;
        }

        /// <summary>
        /// 设置切深/步距
        /// </summary>
        /// <param name="depth"></param>
        public virtual void SetCutDepth(double depth)
        {
            _SetCutDepth(depth, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH);
        }

        /// <summary>
        /// 生成路径
        /// </summary>
        public virtual string PathGenerate()
        {
            if (OperIsValid)
            {
                return Helper.PathGenerate(OperTag);
            }
            return string.Empty;
        }

        /// <summary>
        /// 是否过切
        /// </summary>
        public virtual bool IsPathGouged()
        {
            if (OperIsValid)
            {
                return Helper.IsPathGouged(OperTag);
            }
            return false;
        }

        /// <summary>
        /// 生成程序
        /// </summary>
        public virtual void GenerateProgram(string postName,string path,string extension)
        {
            var name = _operName;
            //TODO 判断是否有无刀路
            try
            {
                //生成nc程式
                ufSession.Setup.GenerateProgram(
                    Snap.Globals.WorkPart.NXOpenPart.CAMSetup.Tag,
                   OperTag
                    , postName
                    , System.IO.Path.Combine(path, string.Format(@"{0}.{1}",name, extension))
                    , NXOpen.UF.UFSetup.OutputUnits.OutputUnitsOutputDefined
                    );
            }
            catch (Exception ex)
            {
                Helper.ShowInfoWindow(string.Format("{0}:{1}", name, ex.Message));
            }
        }

        /// <summary>
        /// 设置切深/步距
        /// </summary>
        /// <param name="depth"></param>
        protected void _SetCutDepth(double depth, int param_index = NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH)
        {
            ufSession.Param.SetDoubleValue(OperTag, param_index, depth);
        }

        /// <summary>
        /// 设置进给率和主轴转速
        /// </summary>
        /// <param name="feedRate">进给率</param>
        /// <param name="speedValue">主轴转速</param>
        protected void _SetFeedRate(double feedRate,double speedValue)
        {
            Helper.SetFeedRate(OperTag, feedRate);
            Helper.SetSpeedValue(OperTag, speedValue);
            Helper.SetSpeedValue(OperTag, 0, NXOpen.UF.UFConstants.UF_PARAM_SURFACE_SPEED);
            Helper.SetSpeedValue(OperTag, 0, NXOpen.UF.UFConstants.UF_PARAM_FEED_PER_TOOTH);
        }

        /// <summary>
        /// 设置切削层
        /// </summary>
        protected virtual void _SetCutLevels(NXOpen.Tag faceTag, levelsPosition levelsPosition= levelsPosition.BottomLevel)
        {
            Helper.SetCutLevels(OperTag, faceTag, (int)levelsPosition);
        }

        /// <summary>
        /// 设置参考刀具
        /// </summary>
        /// <param name="cutter"></param>
        public virtual void SetReferenceCutter(CAMCutter cutter)
        {
            Helper.SetReferenceCutter(OperTag, cutter.CutterTag);
        }

        /// <summary>
        /// 设置部件余量及底部余量
        /// </summary>
        /// <param name="sideStock">部件余量</param>
        /// <param name="floorStock">底部余量</param>
        protected virtual void _SetPartStockAndFloorStock(double sideStock, double floorStock)
        {
            Helper.SetPartStockAndFloorStock(OperTag, sideStock, floorStock);
        }
        public static E_TmplateOper GetEnumByKey(string key)
        {
            return (E_TmplateOper)Enum.Parse(typeof(E_TmplateOper), key);
        }
        public static string GetEnumNameByKey(E_TmplateOper enumKey)
        {

            return Enum.GetName(typeof(E_TmplateOper), enumKey);
        }
    }

    public enum levelsPosition
    {
        TopLevel,
        BottomLevel
    }

    public enum E_TmplateOper
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unkown,
        /// <summary>
        /// 杀顶
        /// </summary>
        FACE_MILLING_TOP,
        /// <summary>
        /// 铜开粗
        /// </summary>
        CAVITY_MILL_C,
        /// <summary>
        /// 石墨开粗
        /// </summary>
        CAVITY_MILL_G,
        /// <summary>
        /// 2D开粗基准
        /// </summary>
        CAVITY_PLANAR_MILL,
        /// <summary>
        /// 残料开粗
        /// </summary>
        CAVITY_MILL_REF,
        /// <summary>
        /// 基准平面
        /// </summary>
        FACE_MILLING_BASE,
        /// <summary>
        /// 基准侧面
        /// </summary>
        PLANAR_MILL_BASE,
        /// <summary>
        /// 平面
        /// </summary>
        FACE_MILLING,
        /// <summary>
        /// 直身位
        /// </summary>
        PLANAR_MILL,
        /// <summary>
        /// 等高角度
        /// </summary>
        ZLEVEL_PROFILE_STEEP,
        /// <summary>
        /// 等高清角
        /// </summary>
        ZLEVEL_CORNER,
        /// <summary>
        /// 曲面加工
        /// </summary>
        CONTOUR_AREA,
        /// <summary>
        /// 曲面角度
        /// </summary>
        CONTOUR_AREA_NON_STEEP,
        /// <summary>
        /// 曲面清角
        /// </summary>
        FLOWCUT_REF_TOOL,
        /// <summary>
        /// 刻字
        /// </summary>
        CONTOUR_TEXT,
        /// <summary>
        /// 平面清角
        /// </summary>
        FACE_MILLING_CORNER
    }
}
