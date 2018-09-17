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
        public object Main(string newMethodName, string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            return Show(newMethodName, args);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                ProxyObject.Instance.ShowMsg(ex.Message, 1);
            }
        }

        static object Show(string newMethodName,string[] args)
        {
            var arg = args.Count() > 0 ? args.First() : string.Empty;
            object result = null;
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
                                    result = (string[])info.Invoke(null, new object[] { args });
                                }
                            }
                            else if (parameters.Length == 0)
                            {
                                result = (string[])info.Invoke(null, null);
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw ex;
            }

            return result;
        }
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            if (assemblyName.Name == "ManagedLoader")
            {
                var UGII_BASE_DIR = string.Empty;
                var dir = string.Empty;
                if (System.IO.Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName).ToUpper() == "UGRAF")
                {
                    string fileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    dir = System.IO.Path.GetDirectoryName(fileName);
                    var info = new System.IO.DirectoryInfo(dir);
                    UGII_BASE_DIR = info.Parent.FullName;
                }
                else
                {
                    dir = System.Configuration.ConfigurationManager.AppSettings.Get("UGII_ROOT_DIR");
                    var info = new System.IO.DirectoryInfo(dir);
                    UGII_BASE_DIR = info.Parent.FullName;
                }

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
