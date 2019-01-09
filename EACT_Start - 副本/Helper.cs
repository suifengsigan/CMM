using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace EACT_Start
{
    public abstract class Helper
    {
        public static void Send(List<string> args)
        {
            args.Insert(0, System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            string format = "\"{0}\"";
            for (int i = 1; i < args.Count; i++)
            {
                format += " \"{" + i + "}\"";
            }

            string str = string.Format(format
                , args.ToArray()
                );

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(str + "&exit");

            p.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令
            //获取cmd窗口的输出信息
          
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
        }

        public static List<string> ExecBatCommand(Action<Action<string>> inputAction, Action<string> showMsg=null,bool isRead=true)
        {
            Process pro = null;
            StreamWriter sIn = null;
            StreamReader sOut = null;
            List<string> outPutString = new List<string>();
            try
            {
                pro = new Process();
                pro.StartInfo.FileName = "cmd.exe";
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.CreateNoWindow = true;
                pro.StartInfo.RedirectStandardInput = true;
                pro.StartInfo.RedirectStandardOutput = true;
                pro.StartInfo.RedirectStandardError = true;

                pro.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outPutString.Add(e.Data);
                        if (showMsg != null)
                        {
                            showMsg(e.Data);
                        }
                    }
                };

                pro.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outPutString.Add(e.Data);
                        if (showMsg != null)
                        {
                            showMsg(e.Data);
                        }
                    }
                };

                pro.Exited += Pro_Exited;

                pro.Start();
                sIn = pro.StandardInput;
                sIn.AutoFlush = true;
                inputAction(value => sIn.WriteLine(value));
                if (isRead)
                {
                    pro.BeginOutputReadLine();
                    pro.BeginErrorReadLine();
                }
                pro.WaitForExit();
            }
            catch (Exception ex)
            {
                outPutString.Add(ex.Message);
            }
            finally
            {
                if (pro != null && !pro.HasExited)
                    pro.Kill();

                if (sIn != null)
                    sIn.Close();
                if (sOut != null)
                    sOut.Close();
                if (pro != null)
                    pro.Close();
            }

            return outPutString;
        }

        private static void Pro_Exited(object sender, EventArgs e)
        {
            
        }

        public static string ExecBat(string batPath, ref string errMsg)
        {
            string outPutString = string.Empty;
            using (Process pro = new Process())
            {
                FileInfo file = new FileInfo(batPath);
                pro.StartInfo.WorkingDirectory = file.Directory.FullName;
                pro.StartInfo.FileName = batPath;
                pro.StartInfo.CreateNoWindow = false;
                pro.StartInfo.RedirectStandardOutput = true;
                pro.StartInfo.RedirectStandardError = true;
                pro.StartInfo.UseShellExecute = false;

                pro.Start();
                pro.WaitForExit();

                outPutString = pro.StandardOutput.ReadToEnd();
                errMsg = pro.StandardError.ReadToEnd();
            }
            return outPutString;
        }
        
    }
}
