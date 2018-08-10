using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CMMProgram
{
    public class NxOpenHelper
    {
        public void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Show(args);
        }

        static void Show(string[] args)
        {
            var arg = args.Count() > 0 ? args.First() : string.Empty;
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            try
            {
                var loader = new ManagedLoader();
                var assembly = loader.Load(arg);
                string outArg = string.Empty;
                int result = 0;
                loader.Run("Main", args, out outArg, out result);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            if (assemblyName.Name == "ManagedLoader")
            {
                string fileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                var dir = System.IO.Path.GetDirectoryName(fileName);
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
            else if (assemblyName.Name.Contains("NXOpen"))
            {
                string fileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                var dir = System.IO.Path.GetDirectoryName(fileName);
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
