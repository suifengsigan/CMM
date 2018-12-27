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

        public void CreateOper(NXOpen.Tag WorkGeometryGroup, NXOpen.Tag ProgramGroup, NXOpen.Tag MethodGroupRoot, CAMCutter CAMCutter)
        {
            this.WorkGeometryGroup = WorkGeometryGroup;
            this.ProgramGroup = ProgramGroup;
            this.MethodGroupRoot = MethodGroupRoot;
            this.CAMCutter = CAMCutter;

            //TODO 创建工序
            NXOpen.Tag operTag;
            ufSession.Oper.Create(AUTOCAM_TYPE, AUTOCAM_SUBTYPE, out operTag);
            ufSession.Ncgroup.AcceptMember(WorkGeometryGroup, operTag);
            ufSession.Ncgroup.AcceptMember(ProgramGroup, operTag);
            ufSession.Ncgroup.AcceptMember(MethodGroupRoot, operTag);
            ufSession.Ncgroup.AcceptMember(CAMCutter.CutterTag, operTag);
            OperTag = operTag;

            int count;
            NXOpen.Tag[] list;
            ufSession.Ncgroup.AskMemberList(ProgramGroup, out count, out list);
            int index = -1;
            string name;
            for (int i = 0; i < count; i++)
            {
                ufSession.Obj.AskName(list[i], out name);
                if (name.Contains(AUTOCAM_SUBTYPE)&& operTag!=list[i])
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
        }

        /// <summary>
        /// 创建工序
        /// </summary>
        public void CreateOper()
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
        }

        /// <summary>
        /// 设置切深/步距
        /// </summary>
        /// <param name="depth"></param>
        public virtual void SetCutDepth(double depth)
        {
            _SetCutDepth(depth);
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
            Helper.SetFeedRate(OperTag, NXOpen.UF.UFConstants.UF_PARAM_FEED_ENGAGE, feedRate);
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
    }
}
