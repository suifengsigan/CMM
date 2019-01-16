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
        /// <summary>
        /// 开粗部件余量
        /// </summary>
        public double CAVITYPartStock = 0.1;
        /// <summary>
        /// 开粗底部余量
        /// </summary>
        public double CAVITYFloorStock = 0.1;
        /// <summary>
        /// 火花位类型
        /// </summary>
        public E_SparkPosition SparkPosition = E_SparkPosition.Stock;
        /// <summary>
        /// 刀具列表
        /// </summary>
        public List<CutterInfo> Cutters = new List<CutterInfo>();
        public CutterInfo GetCutters(string key)
        {
            CutterInfo result;
            var info = Cutters.Where(u => u.刀具类型 == key).FirstOrDefault();
            if (info != null)
            {
                result = info;
            }
            else
            {
                result = new CutterInfo { 刀具类型 = key, Details = new List<CutterDetail>()};
                Cutters.Add(result);
            }
            return result;
        }
        /// <summary>
        /// 工序列表
        /// </summary>
        public List<OperationInfo> Operations = new List<OperationInfo>();
        /// <summary>
        /// 方案列表
        /// </summary>
        public List<ProjectInfo> Projects = new List<ProjectInfo>();

        public class CutterInfo
        {
            public string 刀具类型 { get; set; }
            public List<CutterDetail> Details = new List<CutterDetail>();
        }

        public class CutterDetail
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
        }

        public class OperationInfo
        {
            public OperationInfo()
            {
                模版类型 = S_OperationTemplate.Default;
            }
            //public int 序号 { get; set; }
            public string 显示名称 { get; set; }
            public string 模板名称 { get; set; }
            public string 模版类型 { get; set; }
            public string 操作类型 { get; set; }
            
        }

        public class ProjectInfo
        {
            public string 方案名称 { get; set; }
            public List<ProjectDetail> Details = new List<ProjectDetail>();
        }

        public class ProjectDetail
        {
            public string 工序 { get; set; }
            public string 刀具 { get; set; }
            public string 参考刀具 { get; set; }
            public double 切深 { get; set; }
            public double 进给 { get; set; }
        }

        public enum E_SparkPosition
        {
            /// <summary>
            /// 余量
            /// </summary>
            Stock,
            /// <summary>
            /// 骗刀Z
            /// </summary>
            CheatKnifeZ,
            /// <summary>
            /// 骗刀
            /// </summary>
            CheatKnife
        }

        public struct S_OperationTemplate
        {
            /// <summary>
            /// 默认
            /// </summary>
            public const string Default = "默认";
            /// <summary>
            /// EACT_AUTOCAM
            /// </summary>
            public const string EACT_AUTOCAM = "EACT_AUTOCAM";
        }
    }

   
    
}
