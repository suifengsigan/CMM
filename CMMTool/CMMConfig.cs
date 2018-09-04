using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CMMTool
{
    public class CMMConfig
    {
        static string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("Config", "ProbeData.json"));
        public List<ProbeData> ProbeDatas = new List<ProbeData>();
        public CMMConfig()
        {
            EntryPoint = 10;
            RetreatPoint = 10;
            StepLength = 5;
            SafeDistance = 10;
            VerticalValue = 4;
        }

        public static void WriteConfig(CMMConfig data)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            File.WriteAllText(_path, json);
        }

        public static CMMConfig GetInstance()
        {
            var json = string.Empty;
            if (File.Exists(_path))
            {
                json = File.ReadAllText(_path);
            }

            if (!string.IsNullOrEmpty(json))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<CMMConfig>(json) ?? new CMMConfig();
            }
            return new CMMConfig();
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
        /// 步长（用于动态增量干涉）
        /// </summary>
        public double StepLength { get; set; }
        /// <summary>
        /// 安全距离
        /// </summary>
        public double SafeDistance { get; set; }
        /// <summary>
        /// 侧面取值范围
        /// </summary>
        public double VerticalValue { get; set; }
        /// <summary>
        /// 图档工具路径
        /// </summary>
        public string AutoPrtToolDir { get; set; }
        /// <summary>
        /// AutoCMM路径
        /// </summary>
        public string AutoCmmDir { get; set; }
        /// <summary>
        /// 是否启用干涉点检测
        /// </summary>
        public bool IsInspectionPath = false;
        /// <summary>
        /// 放电面取点
        /// </summary>
        public bool IsEDMFaceGetPoint = false;
        /// <summary>
        /// 是否初始化配置
        /// </summary>
        public bool IsInitConfig = false;

    }
}
