using NXOpen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EdmDraw
{
    public static class Helper
    {
        public static string ExportPDF(NXOpen.Drawings.DrawingSheet ds,string dsName)
        {
            return SnapEx.Create.ExportPDF(ds, dsName);
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public static object GetPropertyValue<T>(T obj,string name)
        {
            foreach (var item in obj.GetType().GetProperties())
            {
                if (item.Name != name)
                {
                    var v = ((System.ComponentModel.DisplayNameAttribute[])item.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), false)).ToList();
                    if (!(v.Count > 0 && v.First().DisplayName == name))
                    {
                        continue;
                    }
                }

                return item.GetValue(obj, null)??string.Empty;

            }

            return string.Empty;
        }
    }
}
