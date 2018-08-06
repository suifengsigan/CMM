using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMMProgram
{
    /// <summary>
    /// 探针数据
    /// </summary>
    public class ProbeData
    {
        public ProbeData() 
        {
            VerticalValue = 2.5;
        }
        /// <summary>
        /// 侧面值
        /// </summary>
        public double VerticalValue { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string ProbeName { get; set; }
        /// <summary>
        /// 探针角度
        /// </summary>
        public string ProbeAB { get; set; }
        /// <summary>
        /// 球半径
        /// </summary>
        public double SphereRadius { get; set; }
        /// <summary>
        /// 测针半径
        /// </summary>
        public double ArrowRadius { get; set; }
        /// <summary>
        /// 测针长度
        /// </summary>
        public double ArrowLength { get; set; }
        /// <summary>
        /// 加长杆半径
        /// </summary>
        public double ExtensionBarRadius { get; set; }
        /// <summary>
        /// 加长杆长度
        /// </summary>
        public double ExtensionBarLength { get; set; }
        /// <summary>
        /// 测头半径
        /// </summary>
        public double HeadRadius { get; set; }
        /// <summary>
        /// 测头长度
        /// </summary>
        public double HeadLength { get; set; }

        public Snap.NX.Body Body { get; set; }

        public List<AB> ABList
        {
            get
            {
                var list = new List<AB>();
                try
                {
                    ProbeAB.Split('|').ToList().ForEach(u =>
                    {
                        var strs = new List<string>();
                        u.Split('B').ToList().ForEach(m =>
                        {
                            strs.Add(m.Replace("A", string.Empty));
                        });
                        if (strs.Count == 2)
                        {
                            double A, B;
                            if (double.TryParse(strs[0], out A) && double.TryParse(strs[1], out B))
                            {
                                list.Add(new AB { A = A, B = B });
                            }
                        }
                    });
                }
                catch { }
                

                if (list.Count == 0)
                {
                    list.Add(new AB());
                }
                return list;
            }
        }
    }
}
