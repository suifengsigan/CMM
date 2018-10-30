using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 加工刀具类
    /// </summary>
    public class CAMCutter
    {
        public string AUTOCAM_TYPE { get; set; }
        public string AUTOCAM_SUBTYPE { get; set; }
        public string CutterName { get; set; }
        public double TL_DIAMETER { get; set; }
    }
}
