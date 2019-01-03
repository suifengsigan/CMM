using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;

namespace AutoCAMUI
{
    /// <summary>
    /// 刻字
    /// </summary>
    public class WsqAutoCAM_CONTOUR_TEXT_Oper:CAMOper
    {
        public WsqAutoCAM_CONTOUR_TEXT_Oper()
        {
            AUTOCAM_TYPE = "WsqAutoCAM";
            AUTOCAM_SUBTYPE = "CONTOUR_TEXT";
        }

        /// <summary>
        /// 设置刻字内容
        /// </summary>
        public void SetText(string text,ElecManage.Electrode ele)
        {
            ele.GetChamferFace();
            if (ele.ChamferFace != null)
            {
                List<NXOpen.Tag> peripheral;
                List<List<NXOpen.Tag>> innerCircumference;
                Helper.GetOutlineCurve(ele.BaseFace, out peripheral, out innerCircumference);
                if (!(peripheral.Count > 0 && ele.BaseSideFaces.Count > 0))
                {
                    return;
                }
                var list = peripheral.ToList();
                if (list.Count > 2)
                {
                    list = list.OrderByDescending(u => Snap.NX.Edge.Wrap(u).ArcLength).Take(2).ToList();
                }
                var edge = Snap.NX.Edge.Wrap(list.OrderByDescending(u => Snap.Compute.Distance(ele.ChamferFace, Snap.NX.NXObject.Wrap(u))).FirstOrDefault());
                var minDistance = double.MaxValue;
                innerCircumference.ForEach(u => {
                    u.ForEach(m => {
                        var tmpDistance = Snap.Compute.Distance(edge, Snap.NX.NXObject.Wrap(m));
                        if (minDistance > tmpDistance)
                        {
                            minDistance = tmpDistance;
                        }
                    });
                });

                var textCenterPoint = (edge.StartPoint + edge.EndPoint) / 2;
                var face = ele.BaseSideFaces.OrderBy(u => Snap.Compute.Distance(textCenterPoint, u)).FirstOrDefault();
                var yDir = Snap.Vector.Unit(-face.GetFaceDirection());
                textCenterPoint = textCenterPoint.Copy(Snap.Geom.Transform.CreateTranslation((minDistance / 3) * yDir));
                var textOri = new Snap.Orientation(Snap.Vector.Cross(yDir, Snap.Orientation.Identity.AxisZ), yDir);
                var trans = Snap.Geom.Transform.CreateRotation(textOri, Snap.Orientation.Identity);
                trans = Snap.Geom.Transform.Composition(trans, Snap.Geom.Transform.CreateTranslation(textCenterPoint - Snap.Position.Origin));
                var textNxObject = Snap.NX.NXObject.Wrap(SnapEx.Create.CreateNode(text, Snap.Position.Origin));
                textNxObject.Move(trans);
                Helper.SetCamText(OperTag, new List<NXOpen.Tag> { textNxObject.NXOpenTag });
            }
        }
    }
}
