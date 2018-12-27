using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;

namespace AutoCAMUI
{
    /// <summary>
    /// 基准侧面
    /// </summary>
    public class WsqAutoCAM_PLANAR_MILL_BASE_Oper:CAMOper
    {
        public WsqAutoCAM_PLANAR_MILL_BASE_Oper()
        {
            AUTOCAM_TYPE = "WsqAutoCAM";
            AUTOCAM_SUBTYPE = "PLANAR_MILL_BASE";
        }

        /// <summary>
        /// 设置切深
        /// </summary>
        public override void SetCutDepth(double depth, int param_index = NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_MAX_DEPTH)
        {
            base.SetCutDepth(depth, param_index);
        }

        /// <summary>
        /// 设置边界和加工底面
        /// </summary>
        /// <param name="ele">电极</param>
        public void SetBoundaryAndCutFloor(ElecManage.Electrode ele)
        {
            Helper.SetBoundary(ele.BaseFace.GetCenterPointEx(), ele.BaseFace.NXOpenTag, NXOpen.UF.CamGeomType.CamPart, OperTag, NXOpen.UF.CamMaterialSide.CamMaterialSideInLeft);
            Helper.SetCutFloor(OperTag, ele.TopFace.GetCenterPointEx());
        }
    }
}
