using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 平面清角
    /// </summary>
    public class WsqAutoCAM_FACE_MILLING_CORNER_Oper : CAMOper
    {
        public WsqAutoCAM_FACE_MILLING_CORNER_Oper()
        {
            AUTOCAM_TYPE = "WsqAutoCAM";
            AUTOCAM_SUBTYPE = "FACE_MILLING_CORNER";
        }

        public override void SetCutDepth(double depth, int param_index = NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_DEPTH_PER_CUT)
        {
            base.SetCutDepth(depth, param_index);
        }

        /// <summary>
        /// 设置加工区域
        /// </summary>
        public void SetMillArea(List<NXOpen.Tag> faces)
        {
            Helper.SetMillArea(NXOpen.UF.CamGeomType.CamCutArea, OperTag, faces);
        }
    }
}
