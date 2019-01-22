﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;

namespace AutoCAMUI
{
    public class WsqAutoCAM_PLANAR_MILL_Oper:WsqAutoCAM_Oper
    {
        public WsqAutoCAM_PLANAR_MILL_Oper()
        {
            TmplateOper = E_TmplateOper.PLANAR_MILL;
        }

        protected override void AutoSet(CAMElectrode ele)
        {
            SetBoundaryAndCutFloor(ele.Electrode);
        }

        /// <summary>
        /// 设置切深
        /// </summary>
        public override void SetCutDepth(double depth)
        {
            _SetCutDepth(depth, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_MAX_DEPTH);
        }

        /// <summary>
        /// 设置边界和加工底面
        /// </summary>
        /// <param name="ele">电极</param>
        public void SetBoundaryAndCutFloor(ElecManage.Electrode ele)
        {
            List<NXOpen.Tag> peripheral;
            List<List<NXOpen.Tag>> innerCircumference;
            Helper.GetOutlineCurve(ele.BaseFace, out peripheral, out innerCircumference);
            innerCircumference.ForEach(u => {
                Helper.SetBoundaryByCurves(u
               , NXOpen.UF.CamGeomType.CamPart, OperTag, NXOpen.UF.CamMaterialSide.CamMaterialSideInLeft);
            });

            Helper.SetCutFloor(OperTag, ele.BaseFace.GetCenterPointEx());
        }
    }
}