using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EdmDraw
{
    public class EDMTableInfo
    {
        [DisplayName("跑位")]
        public string N { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
        public string ZTOP { get; set; }
        [DisplayName("旋转")]
        public string C { get; set; }
        [DisplayName("方向")]
        public string ROCDIRECTION { get; set; }

        /// <summary>
        /// 中文处理
        /// </summary>
        public static string ChineseHandle(string info, int size = 2)
        {
            if (IsHasChinese(info))
            {
                return string.Format("<F{0}>{1}<F{0}>", size, info);
            }
            return info;
        }

        /// <summary>
        /// 是否有中文
        /// </summary>
        public static bool IsHasChinese(string info)
        {
            Regex rx = new Regex("[\u4E00-\u9FA5]+");
            return rx.IsMatch(info);
        }
    }
}
