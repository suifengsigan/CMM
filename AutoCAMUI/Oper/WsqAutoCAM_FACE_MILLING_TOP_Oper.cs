using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 杀顶
    /// </summary>
    public class WsqAutoCAM_FACE_MILLING_TOP_Oper: WsqAutoCAM_Oper
    {
        public WsqAutoCAM_FACE_MILLING_TOP_Oper()
        {
            AUTOCAM_SUBTYPE = "FACE_MILLING_TOP";
        }

        public override void SetCutDepth(double depth, int param_index = NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_DEPTH_PER_CUT)
        {
            base.SetCutDepth(depth, param_index);
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
