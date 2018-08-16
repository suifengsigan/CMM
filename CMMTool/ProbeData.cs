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
          /// <summary>
        /// 探针角度
        /// </summary>
        public List<AB> ABList = new List<AB>();
        public List<AB> GetABList()
        {
            var list = ABList ?? new List<AB>();
            if (list.Where(u => u.A == 0 && u.B == 0).Count() == 0)
            {
                list.Add(new AB { });
            }
            return list;
        }
        public class AB
        {
            public double A { get; set; }
            public double B { get; set; }
        }
    }
}
