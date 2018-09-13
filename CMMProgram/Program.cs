using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace CMMProgram
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            #region 系统只运行一次
            bool bCreatedNew;
            Mutex ltt = new Mutex(false, Path.GetFileNameWithoutExtension(Application.ExecutablePath), out bCreatedNew);
            if (!bCreatedNew)
            {
                MessageBox.Show("已有程序运行中！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            #endregion

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new s());
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            if (assemblyName.Name == "CSharpProxy")
            {
                var programPath = System.Configuration.ConfigurationManager.AppSettings.Get("ProgramPath");
                if (Directory.Exists(programPath))
                {
                    DirectoryInfo info = new DirectoryInfo(programPath);
                    programPath = info.FullName;
                }
                var UGMANAGEDPATH = PathCombine(programPath, assemblyName.Name + ".dll");
                if (File.Exists(UGMANAGEDPATH))
                {
                    return Assembly.LoadFile(UGMANAGEDPATH);
                }
            }
            return null; 
        }

        static string PathCombine(params string[] str)
        {
            var result = string.Empty;
            str.ToList().ForEach(u => {
                result = Path.Combine(result, u);
            });
            return result;
        }
    }
}
