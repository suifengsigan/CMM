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
            var cmmConfig = CMMTool.CMMConfig.GetInstance();
            var path = CMMTool.CMMConfig.GetInstance().AutoPrtToolDir;
            var ConfigData = EactConfig.ConfigData.GetInstance();
            EactTool.FileHelper.InitFileMode(cmmConfig.IsAutoPrtFtpDir ? 1 : 0, ConfigData.FTP.Address, "", ConfigData.FTP.User, ConfigData.FTP.Pass, false
                , "/Eact_AutoPrtTool", @"Temp\Eact_AutoPrtTool", @"Temp\Eact_AutoPrtTool_Error");
            var fileName=EactTool.FileHelper.FindFile(path);
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    EactBomUI.AutoPartBusiness.Start(fileName, ShowMsg);
                    EactTool.FileHelper.DeleteFile(path, fileName);
                }
                catch (Exception ex)
                {
                    var error = string.Format("【{0}】{1}   {2}", System.IO.Path.GetFileNameWithoutExtension(fileName), ex.Message, ex.StackTrace);
                    ShowMsg(error, 1);
                    EactTool.FileHelper.WriteErrorFile(path, fileName, error);
                }
            }
            else
            {
                System.Threading.Thread.Sleep(5000);
            }
        }

        static void ShowMsg(string msg)
        {
           ShowMsg(msg,0);
        }
        static void ShowMsg(string msg, int type)
        {
            CSharpProxy.ProxyObject.Instance.ShowMsg(msg, type);
        }
    }
}
