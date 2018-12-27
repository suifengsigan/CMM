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
    }
}
