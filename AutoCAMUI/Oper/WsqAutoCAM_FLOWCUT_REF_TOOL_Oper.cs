using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 曲面清角
    /// </summary>
    public class WsqAutoCAM_FLOWCUT_REF_TOOL_Oper:WsqAutoCAM_Oper
    {
        public WsqAutoCAM_FLOWCUT_REF_TOOL_Oper()
        {
            TmplateOper = E_TmplateOper.FLOWCUT_REF_TOOL;
        }

        protected override void AutoSet(CAMElectrode ele)
        {
            SetMillArea(ele.Electrode);
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
            var faces = ele.ElecHeadFaces.Where(u => u.ObjectSubType != Snap.NX.ObjectTypes.SubType.FacePlane).ToList();
            Helper.SetCamgeom(NXOpen.UF.CamGeomType.CamCutArea, OperTag, Enumerable.Select(faces, u => u.NXOpenTag).ToList());
        }
    }
}
