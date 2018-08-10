using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CMMProgram
{
    static class Program
    {
        private static string UGBASEDIRUGII = @"I:\UG\NX 9.0-64bit\UGII";
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.IO.Directory.SetCurrentDirectory(UGBASEDIRUGII);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static string PathCombine(params string[] str)
        {
            var result = string.Empty;
            str.ToList().ForEach(u => {
                result = Path.Combine(result, u);
            });
            return result;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            if (assemblyName.Name.Contains("NXOpen"))
            {
                var dir = UGBASEDIRUGII;
                var info = new System.IO.DirectoryInfo(dir);
                var UGII_BASE_DIR = info.Parent.FullName;

                var UGMANAGEDPATH = PathCombine(dir, "managed", assemblyName.Name + ".dll");
                if (!File.Exists(UGMANAGEDPATH))
                {
                    UGMANAGEDPATH = PathCombine(UGII_BASE_DIR, "NXBIN", "managed", assemblyName.Name + ".dll");
                }
                if (File.Exists(UGMANAGEDPATH))
                {
                    return Assembly.LoadFile(UGMANAGEDPATH);
                }
            }
            else if (assemblyName.Name.Contains("PHSnap"))
            {
                Console.WriteLine("Resolving...");
                var assemblbyName = args.Name.Split(',').FirstOrDefault();
                var version = "UG9.0";
                var path = string.Format("{0}\\SnapDll\\{1}", PathCombine(UGBASEDIRUGII, "CMMProg", "Application"), version);
                var file = System.IO.Directory.GetFiles(path).FirstOrDefault(u => u == string.Format("{0}.dll", System.IO.Path.Combine(path, assemblbyName)));
                return Assembly.LoadFile(file);
            }
            else
            {
                var fileName = PathCombine(UGBASEDIRUGII, "CMMProg", "Application", assemblyName.Name + ".dll");
                if (File.Exists(fileName))
                {
                    return Assembly.LoadFile(fileName);
                }
            }
            return null;
        }
    }
}
