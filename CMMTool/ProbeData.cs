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
        [NonSerialized]
        public string ProbeAB = string.Empty;
        /// <summary>
        /// 探球直径
        /// </summary>
        [NonSerialized]
        public double D = 0;
        /// <summary>
        /// 测杆直径
        /// </summary>
        [NonSerialized]
        public double d = 0;
        /// <summary>
        /// 测头工作长度
        /// </summary>
        [NonSerialized]
        public double L = 0;
        [NonSerialized]
        public double D1 = 0;
        [NonSerialized]
        public double L1 = 0;
        [NonSerialized]
        public double L2 = 0;
        [NonSerialized]
        public double D2 = 0;
        [NonSerialized]
        public double D3 = 0;
    }
}
