﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace CMMProgram
{
    public static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        public static void Main()
        {
            var path = System.Configuration.ConfigurationManager.AppSettings.Get("PATH");
            if (!string.IsNullOrEmpty(path))
            {
                path += ";" + System.Environment.GetEnvironmentVariable("PATH");
            }
            else
            {
                path += System.Configuration.ConfigurationManager.AppSettings.Get("UGII_ROOT_DIR")+";" + System.Environment.GetEnvironmentVariable("PATH");
            }
            System.Environment.SetEnvironmentVariable("Path", path);
            System.Environment.SetEnvironmentVariable("UGII_ROOT_DIR", System.Configuration.ConfigurationManager.AppSettings.Get("UGII_ROOT_DIR"));
            System.Environment.SetEnvironmentVariable("UGII_BASE_DIR", System.Configuration.ConfigurationManager.AppSettings.Get("UGII_BASE_DIR"));
            System.Environment.SetEnvironmentVariable("UGII_CAM_POST_DIR", System.IO.Path.Combine(Application.StartupPath, @"MACH\resource\postprocessor\"));
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

        public static int GetUnloadOption(string arg)
        {
            //return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);
            return System.Convert.ToInt32(1);
            // return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            if (assemblyName.Name == "CSharpProxy"|| assemblyName.Name == "Newtonsoft.Json")
            {
                var programPath = System.Configuration.ConfigurationManager.AppSettings.Get("ProgramPath");
                if (programPath == null)
                {
                    programPath = "Application";
                }
                if (Directory.Exists(programPath))
                {
                    DirectoryInfo info = new DirectoryInfo(programPath);
                    programPath = info.FullName;
                }
                else
                {
                    programPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, programPath);
                }
                var UGMANAGEDPATH = PathCombine(programPath, assemblyName.Name + ".dll");
                if (File.Exists(UGMANAGEDPATH))
                {
                    return Assembly.LoadFile(UGMANAGEDPATH);
                }
            }
            else if (assemblyName.Name == "ManagedLoader")
            {
                var UGMANAGEDPATH = Path.Combine(System.Environment.GetEnvironmentVariable("UGII_BASE_DIR") ?? string.Empty, "UGII", "managed", assemblyName.Name + ".dll");


                if (!File.Exists(UGMANAGEDPATH))
                {
                    UGMANAGEDPATH = Path.Combine(System.Environment.GetEnvironmentVariable("UGII_BASE_DIR") ?? string.Empty, "NXBIN", "managed", assemblyName.Name + ".dll");
                }

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
