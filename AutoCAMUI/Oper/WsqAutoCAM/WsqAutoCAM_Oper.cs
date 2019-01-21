using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    public class WsqAutoCAM_Oper:CAMOper
    {
        public WsqAutoCAM_Oper()
        {
            AUTOCAM_TYPE = CNCConfig.CAMConfig.S_OperationTemplate.Default; 
        }
    }
}
