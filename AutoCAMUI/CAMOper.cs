using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 加工工序类
    /// </summary>
    public class CAMOper
    {
        public string AUTOCAM_TYPE { get; set; }
        public string AUTOCAM_SUBTYPE { get; set; }
        public NXOpen.Tag CAMCutter { get; set; }
        public NXOpen.Tag WorkGeometryGroup { get; set; }
        public NXOpen.Tag ProgramGroup { get; set; }
        public NXOpen.Tag MethodGroupRoot { get; set; }
    }
}
