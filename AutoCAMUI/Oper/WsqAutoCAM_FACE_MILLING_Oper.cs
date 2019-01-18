using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 平面
    /// </summary>
    public class WsqAutoCAM_FACE_MILLING_Oper : WsqAutoCAM_Oper
    {
        public WsqAutoCAM_FACE_MILLING_Oper()
        {
            TmplateOper = E_TmplateOper.FACE_MILLING;
        }

        protected override void AutoSet(CAMElectrode ele)
        {
            SetBoundary(ele.HorizontalFaces);
        }

        protected override bool AnalysisOperIsValid(CAMElectrode ele)
        {
            return ele.HorizontalFaces.Count > 0;
        }

        public override void SetCutDepth(double depth, int param_index = NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_DEPTH_PER_CUT)
        {
            if (OperIsValid)
            {
                base.SetCutDepth(depth, param_index);
            }  
        }

        /// <summary>
        /// 设置部件边界
        /// </summary>
        public void SetBoundary(List<CAMFace> faces)
        {
            if (OperIsValid)
            {
                faces.ForEach(u => {
                    Helper.SetBoundaryByFace(u.FaceTag, NXOpen.UF.CamGeomType.CamBlank, OperTag, NXOpen.UF.CamMaterialSide.CamMaterialSideInLeft);
                });
            }  
        }
    }
}
