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
                textCenterPoint = textCenterPoint.Copy(Snap.Geom.Transform.CreateTranslation((minDistance*2 / 5) * yDir));
                var textOri = new Snap.Orientation(Snap.Vector.Cross(yDir, Snap.Orientation.Identity.AxisZ), yDir);
                var trans = Snap.Geom.Transform.CreateRotation(textOri, Snap.Orientation.Identity);
                trans = Snap.Geom.Transform.Composition(trans, Snap.Geom.Transform.CreateTranslation(textCenterPoint - Snap.Position.Origin));
                var textNxObject = Snap.NX.NXObject.Wrap(SnapEx.Create.CreateNode(text, Snap.Position.Origin));
                textNxObject.Move(trans);
                var eData = new EdmDraw.DraftingEnvironmentData();
                var mpi = eData.mpi;
                var mpr = eData.mpr;
                var ufSession = NXOpen.UF.UFSession.GetUFSession();
                ufSession.Drf.AskObjectPreferences(textNxObject.NXOpenTag, eData.mpi, eData.mpr, out eData.radiusValue, out eData.diameterValue);
                //设置尺寸
                var textStyle = new Snap.NX.TextStyle();
                textStyle.SetFont("chinesef", Snap.NX.TextStyle.FontType.NX);
                //文字对齐位置 首选项→公共→文字→对齐位置
                mpi[30] = (int)textStyle.AlignmentPosition;
                //文字样式 首选项→公共→文字→文本参数→字体(将字体设置为blockfont)
                mpi[88] = textStyle.FontIndex;
                //文字样式 首选项→公共→文字→文本参数→设置字宽(粗细)
                mpi[89] = 0;
                //字大小
                mpr[44] = 3.5;
                //文本长宽比
                mpr[45] = 0.5;
                //字体间距
                mpr[46] = 0.1;
                textStyle.AlignmentPosition = Snap.NX.TextStyle.AlignmentPositions.MidCenter;
                ufSession.Drf.SetObjectPreferences(textNxObject.NXOpenTag, eData.mpi, eData.mpr, eData.radiusValue, eData.diameterValue);
                Helper.SetCamText(OperTag, new List<NXOpen.Tag> { textNxObject.NXOpenTag });
            }
        }
    }
}
