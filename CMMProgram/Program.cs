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
            Application.Run(new Form1());
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            if (assemblyName.Name.Contains("NXOpen"))
            {
                var dir = UGBASEDIRUGII;
                var info = new System.IO.DirectoryInfo(dir);
                var UGII_BASE_DIR = info.Parent.FullName;

                var UGMANAGEDPATH = Path.Combine(dir, "managed", assemblyName.Name + ".dll");
                if (!File.Exists(UGMANAGEDPATH))
                {
                    UGMANAGEDPATH = Path.Combine(UGII_BASE_DIR, "NXBIN", "managed", assemblyName.Name + ".dll");
                }
                if (File.Exists(UGMANAGEDPATH))
                {
                    return Assembly.LoadFile(UGMANAGEDPATH);
                }
            }
            else
            {
                var fileName = Path.Combine(UGBASEDIRUGII, "CMMProg", assemblyName.Name + ".dll");
                if (File.Exists(fileName))
                {
                    return Assembly.LoadFile(fileName);
                }
            }
            return null;
        }
    }
}
