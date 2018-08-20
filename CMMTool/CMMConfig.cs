using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMMTool
{
    public class CMMConfig
    {
        public List<ProbeData> ProbeDatas = new List<ProbeData>();
        public CMMConfig()
        {
            EntryPoint = 10;
            RetreatPoint = 10;
            SideFaceGetPointValue = 2;
            StepLength = 5;
        }
        /// <summary>
        /// 进点
        /// </summary>
        public double EntryPoint { get; set; }
        /// <summary>
        /// 退点
        /// </summary>
        public double RetreatPoint { get; set; }
        /// <summary>
        /// 电极侧面取点变量
        /// </summary>
        public double SideFaceGetPointValue { get; set; }
        /// <summary>
        /// 步长（用于动态增量干涉）
        /// </summary>
        public double StepLength { get; set; }
    }
}
