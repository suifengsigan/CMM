using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CNCConfig
{
    /// <summary>
    /// 加工参数配置
    /// </summary>
    public class CAMConfig
    {
        static string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("Config", "CamConfig.json"));
        public static CAMConfig GetInstance()
        {
            var json = string.Empty;
            if (File.Exists(_path))
            {
                json = File.ReadAllText(_path);
            }

            if (!string.IsNullOrEmpty(json))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<CAMConfig>(json) ?? new CAMConfig();
            }
            return new CAMConfig();
        }

        public static void WriteConfig(CAMConfig data)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            File.WriteAllText(_path, json);
        }

        /// <summary>
        /// 基准面颜色ID
        /// </summary>
        public int BaseFaceColor = 176;
        /// <summary>
        /// 水平面颜色ID
        /// </summary>
        public int HorizontalPlaneColor = 175;
        /// <summary>
        /// 垂直面颜色ID
        /// </summary>
        public int VerticalPlaneColor = 181;
        /// <summary>
        /// 陡峭面颜色ID
        /// </summary>
        public int CurveSurfaceColor = 36;
        /// <summary>
        /// 平缓斜面颜色ID
        /// </summary>
        public int GentlePlaneColor = 41;
        /// <summary>
        /// 倒扣面颜色ID
        /// </summary>
        public int ButtonedFaceColor = 186;

        public List<CutterInfo> Cutters = new List<CutterInfo>();

        public class CutterInfo
        {
            public string 刀具名称 { get; set; }
            public string 直径 { get; set; }
            public string R角 { get; set; }
            public string 刀长 { get; set; }
            public string 刃长 { get; set; }
            public string 刀号 { get; set; }
            public string 补正号 { get; set; }
            public string 转速 { get; set; }
            public string 进刀 { get; set; }
            public string 进给 { get; set; }
            public string 横越 { get; set; }
            public string 切深 { get; set; }
            public string 刀柄 { get; set; }
            public string 刀具类型 { get; set; }
        }

    }

   
    
}
