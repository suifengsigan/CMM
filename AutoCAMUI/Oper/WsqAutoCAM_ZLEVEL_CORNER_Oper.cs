using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 等高清角
    /// </summary>
    public class WsqAutoCAM_ZLEVEL_CORNER_Oper:CAMOper
    {
        public WsqAutoCAM_ZLEVEL_CORNER_Oper()
        {
            AUTOCAM_TYPE = "WsqAutoCAM";
            AUTOCAM_SUBTYPE = "ZLEVEL_CORNER";
        }
    }
}
