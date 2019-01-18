using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 等高清角
    /// </summary>
    public class WsqAutoCAM_ZLEVEL_CORNER_Oper:WsqAutoCAM_Oper
    {
        public WsqAutoCAM_ZLEVEL_CORNER_Oper()
        {
            TmplateOper = E_TmplateOper.ZLEVEL_CORNER;
        }

        protected override void AutoSet(CAMElectrode ele)
        {
            SetMillArea(ele.Electrode);
        }

        /// <summary>
        /// 设置加工区域
        /// </summary>
        /// <param name="ele">电极</param>
        public void SetMillArea(ElecManage.Electrode ele)
        {
            Helper.SetCamgeom(NXOpen.UF.CamGeomType.CamCutArea, OperTag, Enumerable.Select(ele.ElecHeadFaces, u => u.NXOpenTag).ToList());
            //指定检查体
            Helper.SetCamgeom(NXOpen.UF.CamGeomType.CamCheck, OperTag, Enumerable.Select(
                new List<Snap.NX.Face> { ele.BaseFace },
                u => u.NXOpenTag).ToList());
        }
    }
}
