using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 平面清角
    /// </summary>
    public class WsqAutoCAM_FACE_MILLING_CORNER_Oper : CAMOper
    {
        public WsqAutoCAM_FACE_MILLING_CORNER_Oper()
        {
            AUTOCAM_TYPE = "WsqAutoCAM";
            AUTOCAM_SUBTYPE = "FACE_MILLING_CORNER";
        }
    }
}
