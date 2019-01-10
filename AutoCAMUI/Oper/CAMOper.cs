using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    public enum levelsPosition
    {
        TopLevel,
        BottomLevel
    }
    /// <summary>
    /// 加工工序类
    /// </summary>
    public class CAMOper
    {
        protected NXOpen.UF.UFSession ufSession = NXOpen.UF.UFSession.GetUFSession();
        public string AUTOCAM_TYPE { get; set; }
        public string AUTOCAM_SUBTYPE { get; set; }
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
        public virtual void SetCutDepth(double depth, int param_index = NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH)
        {
            _SetCutDepth(depth, param_index);
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
        protected virtual void _SetReferenceCutter(CAMCutter cutter)
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
    }
}
