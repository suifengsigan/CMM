using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 基准平面
    /// </summary>
    public class WsqAutoCAM_FACE_MILLING_BASE_Oper : CAMOper
    {
        public WsqAutoCAM_FACE_MILLING_BASE_Oper()
        {
            AUTOCAM_TYPE = "WsqAutoCAM";
            AUTOCAM_SUBTYPE = "FACE_MILLING_BASE";
        }

        public override void SetCutDepth(double depth, int param_index = NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_DEPTH_PER_CUT)
        {
            base.SetCutDepth(depth, param_index);
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
