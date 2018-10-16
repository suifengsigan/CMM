using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdmDraw
{
    /// <summary>
    /// Edm图纸配置
    /// </summary>
    public class EdmConfig
    {
        /// <summary>
        ///  是否使用系统参数
        /// </summary>
        public bool IsUseSystemConfig = false;
        /// <summary>
        /// 字体
        /// </summary>
        public int TextMpi88;
        /// <summary>
        /// 字大小
        /// </summary>
        public double TextMpr44;
        /// <summary>
        /// 字间距
        /// </summary>
        public double TextMpr46;
        /// <summary>
        /// 纵横比
        /// </summary>
        public double TextMpr45;
        /// <summary>
        /// 字的粗细类型
        /// </summary>
        public int TextMpi89;
        /// <summary>
        /// 尺寸字体
        /// </summary>
        public int DimensionMpi85;
        /// <summary>
        /// 尺寸小数点位数
        /// </summary>
        public int DimensionMpi3;
        /// <summary>
        /// 尺寸字大小
        /// </summary>
        public double DimensionMpr32;
        /// <summary>
        /// 尺寸纵横比
        /// </summary>
        public double DimensionMpr33;
        /// <summary>
        /// 尺寸间距因子
        /// </summary>
        public double DimensionMpr34;
        /// <summary>
        /// 尺寸粗细类型
        /// </summary>
        public int DimensionMpi86;
        /// <summary>
        /// 抑制尾零
        /// </summary>
        public int DimensionMpi90;
        /// <summary>
        /// Edm图纸模板
        /// </summary>
        public string EdmTemplate;
    }
}
