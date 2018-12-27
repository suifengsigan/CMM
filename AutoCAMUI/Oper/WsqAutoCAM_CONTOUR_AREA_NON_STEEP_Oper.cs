using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 曲面角度
    /// </summary>
    public class WsqAutoCAM_CONTOUR_AREA_NON_STEEP_Oper:CAMOper
    {
        public WsqAutoCAM_CONTOUR_AREA_NON_STEEP_Oper()
        {
            AUTOCAM_TYPE = "WsqAutoCAM";
            AUTOCAM_SUBTYPE = "CONTOUR_AREA_NON_STEEP";
        }
    }
}
