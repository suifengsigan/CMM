using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                var textNxObject = Snap.NX.NXObject.Wrap(SnapEx.Create.CreateNode(text, ele.GetElecBasePos()));
                Helper.SetCamText(OperTag, new List<NXOpen.Tag> { textNxObject.NXOpenTag });
            }
        }
    }
}
