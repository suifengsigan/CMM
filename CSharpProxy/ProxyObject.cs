using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSharpProxy
{
    public class ProxyObject : MarshalByRefObject
    {
        public Action<int, string> ShowMsgHandler;
        Assembly assembly = null;

        public void ShowMsg(string msg, int type = 0)
        {
            if (ShowMsgHandler != null)
            {
                ShowMsgHandler(type, msg);
            }
        }
        public void LoadAssembly(string actionName)
        {
            assembly = Assembly.LoadFile(actionName);
        }
        public bool Invoke(string fullClassName, string methodName, params string[] args)
        {
            if (assembly == null)
                return false;
            Type tp = assembly.GetType(fullClassName);
            if (tp == null)
                return false;
            MethodInfo method = tp.GetMethod(methodName);
            if (method == null)
                return false;
            Object obj = Activator.CreateInstance(tp);
            if (obj is NxOpenHelper)
            {
                (obj as NxOpenHelper).Main(args);
            }
            else { method.Invoke(obj, args); }
            return true;
        }
        public static void ExecuteMothod(string actionName, string baseDirectory)
        {
            var setup = new AppDomainSetup();
            setup.ApplicationBase = baseDirectory;
            AppDomain _appDomain = AppDomain.CreateDomain(PathCombine(actionName, ".dll"), null, setup);
            try
            {
                var args = new string[] { actionName };
                var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                ProxyObject po = (ProxyObject)_appDomain.CreateInstanceFromAndUnwrap(location, typeof(ProxyObject).FullName);
                po.LoadAssembly(location);
                po.Invoke(typeof(NxOpenHelper).FullName, "Main", args);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                AppDomain.Unload(_appDomain);
            }
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
