using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 等高角度
    /// </summary>
    public class WsqAutoCAM_ZLEVEL_PROFILE_STEEP_Oper:WsqAutoCAM_Oper
    {
        public WsqAutoCAM_ZLEVEL_PROFILE_STEEP_Oper()
        {
            TmplateOper = E_TmplateOper.ZLEVEL_PROFILE_STEEP;
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
            //设置切削区域
            Helper.SetCamgeom(NXOpen.UF.CamGeomType.CamCutArea, OperTag, Enumerable.Select(ele.ElecHeadFaces, u => u.NXOpenTag).ToList());
            //指定检查体
            Helper.SetCamgeom(NXOpen.UF.CamGeomType.CamCheck, OperTag, Enumerable.Select(
                new List<Snap.NX.Face> {ele.BaseFace }, 
                u => u.NXOpenTag).ToList());
        }
    }
}
