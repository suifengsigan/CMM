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
        /// 设置切削层
        /// </summary>
        protected virtual void _SetCutLevels(NXOpen.Tag faceTag, levelsPosition levelsPosition= levelsPosition.BottomLevel)
        {
            Helper.SetCutLevels(OperTag, faceTag, (int)levelsPosition);
        }
    }
}
