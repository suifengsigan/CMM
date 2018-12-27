using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 曲面清角
    /// </summary>
    public class WsqAutoCAM_FLOWCUT_REF_TOOL_Oper:CAMOper
    {
        public WsqAutoCAM_FLOWCUT_REF_TOOL_Oper()
        {
            AUTOCAM_TYPE = "WsqAutoCAM";
            AUTOCAM_SUBTYPE = "FLOWCUT_REF_TOOL";
        }
    }
}
