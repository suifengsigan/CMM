using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 基准平面
    /// </summary>
    public class WsqAutoCAM_FACE_MILLING_BASE_Oper : WsqAutoCAM_Oper
    {
        public WsqAutoCAM_FACE_MILLING_BASE_Oper()
        {
            TmplateOper = E_TmplateOper.FACE_MILLING_BASE;
        }

        protected override void AutoSet(CAMElectrode ele)
        {
            SetBoundary(ele.Electrode);
        }

        public override void SetCutDepth(double depth)
        {
           _SetCutDepth(depth, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_DEPTH_PER_CUT);
        }

        /// <summary>
        /// 设置部件边界
        /// </summary>
        /// <param name="ele">电极</param>
        public void SetBoundary(ElecManage.Electrode ele)
        {
            Helper.SetBoundaryByFace(ele.BaseFace.NXOpenTag, NXOpen.UF.CamGeomType.CamBlank, OperTag, NXOpen.UF.CamMaterialSide.CamMaterialSideInLeft);
        }
    }
}
