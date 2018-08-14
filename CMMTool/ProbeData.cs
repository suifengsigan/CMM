using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CMMTool
{
    public class ProbeData
    {
        /// <summary>
        /// 名称
        /// </summary>
        [DisplayName("探针名称")]
        public string ProbeName { get; set; }
        /// <summary>
        /// 探针角度
        /// </summary>
        public string ProbeAB = string.Empty;
        /// <summary>
        /// 探球直径
        /// </summary>
        public double D = 0;
        /// <summary>
        /// 测杆直径
        /// </summary>
        public double d = 0;
        /// <summary>
        /// 测头工作长度
        /// </summary>
        public double L = 0;
        public double D1 = 0;
        public double L1 = 0;
        public double L2 = 0;
        public double D2 = 0;
        public double D3 = 0;
    }
}
