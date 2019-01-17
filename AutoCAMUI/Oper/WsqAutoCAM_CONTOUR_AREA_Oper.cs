using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 曲面加工
    /// </summary>
    public class WsqAutoCAM_CONTOUR_AREA_Oper:WsqAutoCAM_Oper
    {
        public WsqAutoCAM_CONTOUR_AREA_Oper()
        {
            TmplateOper = E_TmplateOper.CONTOUR_AREA;
        }

        public override void SetCutDepth(double depth, int param_index = NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH)
        {
            //base.SetCutDepth(depth, param_index);
        }

        /// <summary>
        /// 设置加工区域
        /// </summary>
        /// <param name="ele">电极</param>
        public void SetMillArea(ElecManage.Electrode ele)
        {
            Helper.SetCamgeom(NXOpen.UF.CamGeomType.CamCutArea, OperTag, Enumerable.Select(ele.ElecHeadFaces, u => u.NXOpenTag).ToList());
        }
    }
}
