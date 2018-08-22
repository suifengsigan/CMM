using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSharpProxy
{
    public class NxOpenHelper
    {
        public void Main(string newMethodName, string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Show(newMethodName, args);
        }

        static void Show(string newMethodName,string[] args)
        {
            var arg = args.Count() > 0 ? args.First() : string.Empty;
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            try
            {
                var loader = new ManagedLoader();
                var assembly = loader.Load(arg);
                string outArg = string.Empty;
                //int result = 0;
                //loader.Run(newMethodName, arg, out outArg, out result);

                #region oldCode
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(u => u.Location == Path.Combine(AppDomain.CurrentDomain.BaseDirectory, arg));
                Type[] types = assemblies.FirstOrDefault().GetTypes();
                foreach (Type type in types)
                {
                    foreach (MethodInfo info in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (info.Name == newMethodName)
                        {
                            ParameterInfo[] parameters = info.GetParameters();
                            if (parameters.Length == 1)
                            {
                                Type parameterType = parameters[0].ParameterType;
                                Type returnType = info.ReturnType;
                                if ((parameterType.IsArray && (parameterType.GetElementType() == typeof(string))) && (returnType.IsArray && (returnType.GetElementType() == typeof(string))))
                                {
                                    var result = (string[])info.Invoke(null, new object[] { args });
                                }
                            }
                            else if (parameters.Length == 0)
                            {
                                var result = (string[])info.Invoke(null, null);
                            }
                        }
                    }
                }
                #endregion
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
