using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// 图纸放置层
        /// </summary>
        public int EdmDrfLayer = 254;
        /// <summary>
        /// 字体
        /// </summary>
        public string TextMpi88;
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
        public string DimensionMpi85;
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
        /// <summary>
        /// 放电图纸视图
        /// </summary>
        public List<DraftViewLocation> DraftViewLocations = new List<DraftViewLocation>();
        /// <summary>
        /// 电极信息
        /// </summary>
        public List<PropertyInfo> PropertyInfos = new List<PropertyInfo>();
        /// <summary>
        /// 表格信息
        /// </summary>
        public TableInfo Table = new TableInfo();

        /// <summary>
        /// 表格信息
        /// </summary>
        public class TableInfo
        {
            /// <summary>
            /// 位置X
            /// </summary>
            public double locationX { get; set; }
            /// <summary>
            /// 位置Y
            /// </summary>
            public double locationY { get; set; }
            /// <summary>
            /// 列宽
            /// </summary>
            public double ColumnWidth { get; set; }
            /// <summary>
            /// 行高
            /// </summary>
            public double RowHeight { get; set; }
            /// <summary>
            /// 列信息
            /// </summary>
            public List<ColumnInfo> ColumnInfos = new List<ColumnInfo>();
        }

        /// <summary>
        /// 表格列信息
        /// </summary>
        public class ColumnInfo
        {
            /// <summary>
            /// 显示名称
            /// </summary>
            [DisplayName("属性名称")]
            public string DisplayName { get;set; }
            [DisplayName("类型")]
            public string Ex { get; set; }
        }

        /// <summary>
        /// 放电图纸视图位置
        /// </summary>
        public class DraftViewLocation
        {
            /// <summary>
            /// 视图类型
            /// </summary>
            [DisplayName("视图类型")]
            public string ViewType { get; set; }
            /// <summary>
            /// 长
            /// </summary>
            [DisplayName("长")]
            public double SizeX { get; set; }
            /// <summary>
            /// 宽
            /// </summary>
            [DisplayName("宽")]
            public double SizeY { get; set; }
            /// <summary>
            /// 位置X
            /// </summary>
            [DisplayName("位置X")]
            public double LocationX { get; set; }
            /// <summary>
            /// 位置Y
            /// </summary>
            [DisplayName("位置Y")]
            public double LocationY { get; set; }
        }

        /// <summary>
        /// 属性信息
        /// </summary>
        public class PropertyInfo
        {
            /// <summary>
            /// 显示名称(特性)
            /// </summary>
            [DisplayName("名称")]
            public string DisplayName { get; set; }
            /// <summary>
            /// 位置X
            /// </summary>
            [DisplayName("位置X")]
            public double LocationX { get; set; }
            /// <summary>
            /// 位置Y
            /// </summary>
            [DisplayName("位置Y")]
            public double LocationY { get; set; }
            /// <summary>
            /// 拓展字段（用于判断是否是勾选）
            /// </summary>
            [DisplayName("类型")]
            public string Ex { get; set; }
        }
    }
}
