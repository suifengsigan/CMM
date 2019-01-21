using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    public class JYToolsOper : CAMOper
    {
        public JYToolsOper()
        {
            AUTOCAM_TYPE = CNCConfig.CAMConfig.S_OperationTemplate.EACT_AUTOCAM;
        }

        protected override void AutoSet(CAMElectrode ele)
        {
            SetBoundary(ele.Electrode);
        }

        public override void SetCutDepth(double depth)
        {
            _SetCutDepth(depth, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_DEPTH_PER_CUT);
        }

        /// <summary>
        /// 设置边界
        /// </summary>
        public void SetBoundary(ElecManage.Electrode electrode)
        {
            List<NXOpen.Tag> peripheral = new List<NXOpen.Tag>();
            var box3d = electrode.ElecBody.Box;
            var p1 = new Snap.Position(box3d.MinX, box3d.MaxY, box3d.MaxZ);
            var p2 = new Snap.Position(box3d.MinX, box3d.MinY, box3d.MaxZ);
            var p3 = new Snap.Position(box3d.MaxX, box3d.MinY, box3d.MaxZ);
            var p4 = new Snap.Position(box3d.MaxX, box3d.MaxY, box3d.MaxZ);
            var workPart = NXOpen.Session.GetSession().Parts.Work;
            peripheral.Add(Helper.Create_SO_Curve(p1, p2));
            peripheral.Add(Helper.Create_SO_Curve(p2, p3));
            peripheral.Add(Helper.Create_SO_Curve(p3, p4));
            peripheral.Add(Helper.Create_SO_Curve(p4, p1));
            Helper.SetBoundaryByCurves(
                peripheral.ToList()
                , NXOpen.UF.CamGeomType.CamBlank, OperTag, NXOpen.UF.CamMaterialSide.CamMaterialSideInLeft);
        }
    }
}
