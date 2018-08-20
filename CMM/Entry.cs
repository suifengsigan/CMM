using ElecManage;
using NXOpen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CMM
{
    public class Entry
    {
        public static void Main()
        {

        }
        public static void AutoSelPoint(string filename)
        {
            //导入探针
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            //初始化探针数据
            var config = CMMTool.CMMConfig.GetInstance();
            CMMTool.Business.DeleteProbe();
            foreach (var item in config.ProbeDatas.ToList())
            {
                CMMTool.Business.CreateProbe(item);
            }
        }
    }
}
