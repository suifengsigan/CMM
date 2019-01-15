using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNCConfig
{
    public class XKCutterInfo
    {
        public string 刀具名称 { get; set; }
        public string 直径 { get; set; }
        public string R角 { get; set; }
        [PropertieName("长度")]
        public string 刀长 { get; set; }
        public string 刃长 { get; set; }
        [PropertieName("刀号(普)")]
        public string 刀号 { get; set; }
        [PropertieName("补正号(普)")]
        public string 补正号 { get; set; }
        [PropertieName("转速(普)")]
        public string 转速 { get; set; }
        [PropertieName("进刀(普)")]
        public string 进刀 { get; set; }
        [PropertieName("进给(普)")]
        public string 进给 { get; set; }
        [PropertieName("横越(普)")]
        public string 横越 { get; set; }
        [PropertieName("切深(普)")]
        public string 切深 { get; set; }
        [PropertieName("刀柄(普通机)")]
        public string 刀柄 { get; set; }
    }
}   
