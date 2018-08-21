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

        /// <summary>
        /// 自动选点
        /// </summary>
        public static void AutoSelPoint()
        {
            AutoSelPoint(@"C:\Users\PENGHUI\Desktop\prt1\test\SX-1833-4201-1002.prt");
        }

        /// <summary>
        /// 自动选点
        /// </summary>
        public static void AutoSelPoint(string filename)
        {
            //导入探针
            var part = NXOpen.Session.GetSession().Parts.Work;
            if (part != null)
            {
                Snap.NX.Part.Wrap(part.Tag).Close(true, true);
            }

            Snap.NX.Part snapPart = Snap.NX.Part.OpenPart(filename);
            var name = Path.GetFileNameWithoutExtension(filename);
            Snap.Globals.WorkPart = snapPart;
            try
            {
                var config = CMMTool.CMMConfig.GetInstance();
                foreach (var item in config.ProbeDatas)
                {
                    foreach (var ab in item.GetABList())
                    {
                        var fileName = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION"), string.Format("{0}A{1}B{2}.prt", item.ProbeName, ab.A, ab.B));
                        Helper.ImportPart(fileName);
                    }
                }
                Helper.ShowMsg(string.Format("{0}开始取点...", name));
                CMMBusiness.AutoSelPoint(snapPart.Bodies.FirstOrDefault(), config);
                Helper.ShowMsg(string.Format("{0}取点成功", name));
            }
            catch (Exception ex)
            {
                Helper.ShowMsg(string.Format("{0}取点错误【{1}】", name, ex.Message));
                Console.WriteLine("AutoSelPoint错误:{0}", ex.Message);
            }
            snapPart.Close(true, true);
        }

        static bool IsInit = false;

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            if (!IsInit)
            { 
                //初始化探针数据
                var config = CMMTool.CMMConfig.GetInstance();
                CMMTool.Business.DeleteProbe();
                foreach (var item in config.ProbeDatas.ToList())
                {
                    CMMTool.Business.CreateProbe(item);
                }
            }
            IsInit = true;
        }
    }
}
