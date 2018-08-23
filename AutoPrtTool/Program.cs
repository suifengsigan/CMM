using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoPrtTool
{
    public static class Program
    {
        public static void Main()
        {
            AssemblyLoader.Entry.InitAssembly();
            Excute();
        }

        static void Excute()
        {
            ShowMsg("正在匹配图档...");
            var path = CMMTool.CMMConfig.GetInstance().AutoPrtToolDir;
            var fileName=EactTool.FileHelper.FindFile(path);
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    EactBomUI.AutoPartBusiness.Start(fileName, ShowMsg);
                    EactTool.FileHelper.DeleteFile(path, fileName);
                }
                catch(Exception ex)
                {
                    EactTool.FileHelper.WriteErrorFile(path, fileName, ex.Message);
                }
            }
            
        }

        static void ShowMsg(string msg)
        {
            CSharpProxy.ProxyObject.Instance.ShowMsg(msg);
        }
    }
}
