using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;

namespace AutoCAMUI
{
    /// <summary>
    /// 基准台开粗
    /// </summary>
    public class WsqAutoCAM_CAVITY_PLANAR_MILL_Oper : CAMOper
    {
        public WsqAutoCAM_CAVITY_PLANAR_MILL_Oper()
        {
            AUTOCAM_TYPE = "WsqAutoCAM";
            AUTOCAM_SUBTYPE = "CAVITY_PLANAR_MILL";
        }

        /// <summary>
        /// 设置切深步距
        /// </summary>
        public override void SetCutDepth(double depth,int param_index= NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_MAX_DEPTH)
        {
            base.SetCutDepth(depth, param_index);
        }

        /// <summary>
        /// 设置边界和底面
        /// </summary>
        public void SetBoundaryAndCutFloor(ElecManage.Electrode electrode)
        {
            List<NXOpen.Tag> peripheral;
            List<List<NXOpen.Tag>> innerCircumference;
            Helper.GetOutlineCurve(electrode.BaseFace, out peripheral, out innerCircumference);
            Helper.SetBoundaryByCurves(
                peripheral.ToList()
                , NXOpen.UF.CamGeomType.CamPart, OperTag, NXOpen.UF.CamMaterialSide.CamMaterialSideInLeft);
            Helper.SetCutFloor(OperTag, electrode.TopFace.GetCenterPointEx());
        }
    }
}
