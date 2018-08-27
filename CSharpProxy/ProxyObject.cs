﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CSharpProxy
{
    public class ProxyObject : MarshalByRefObject
    {
        public const int WM_DATA_TRANSFER = 0x0437;
        /// <summary>
        /// CopyDataStruct
        /// </summary>
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        // FindWindow method, using Windows API
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern int FindWindow(string lpClassName, string lpWindowName);

        // IsWindow method, using Windows API
        [DllImport("User32.dll", EntryPoint = "IsWindow")]
        private static extern bool IsWindow(int hWnd);

        // SendMessage method, using Windows API
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
            int hWnd,                   // handle to destination window
            int Msg,                    // message
            int wParam,                 // first message parameter
            ref COPYDATASTRUCT lParam   // second message parameter
        );

        // SendMessage method, using Windows API
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
            int hWnd,                   // handle to destination window
            int Msg,                    // message
            int wParam,                 // first message parameter
            string lParam               // second message parameter
        );


        Assembly assembly = null;
        public IntPtr WindowPH = IntPtr.Zero;
        public static ProxyObject Instance;
        public ProxyObject()
        {
            Instance = this;
        }

        /// <summary>
        ///type 0 普通信息 1 错误信息
        /// </summary>
        public void ShowMsg(string msg, int type = 0)
        {
            if (WindowPH != IntPtr.Zero)
            {
                String strSent = msg;
                byte[] arr = System.Text.Encoding.Default.GetBytes(strSent);
                int len = arr.Length;
                COPYDATASTRUCT cdata;
                cdata.dwData = (IntPtr)100;
                cdata.lpData = Marshal.StringToHGlobalAnsi(strSent);
                cdata.cbData = len + 1;
                SendMessage(WindowPH.ToInt32(), WM_DATA_TRANSFER, 0, ref cdata);
            }
        }
        public void LoadAssembly(string actionName)
        {
            assembly = Assembly.LoadFile(actionName);
        }
        public bool Invoke(string fullClassName, string methodName,string newMethodName, params string[] args)
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
                (obj as NxOpenHelper).Main(newMethodName,args);
            }
            else { method.Invoke(obj, args); }
            return true;
        }
        public static void ExecuteMothod(string actionName, string baseDirectory,string methodName= "Main")
        {
            var setup = new AppDomainSetup();
            setup.ApplicationBase = baseDirectory;
            AppDomain _appDomain = AppDomain.CreateDomain(PathCombine(actionName, ".dll"), null, setup);
            try
            {
                var args = new string[] { actionName };
                var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                ProxyObject po = (ProxyObject)_appDomain.CreateInstanceFromAndUnwrap(location, typeof(ProxyObject).FullName);
                IntPtr hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                po.WindowPH = hWnd;
                po.LoadAssembly(location);
                po.Invoke(typeof(NxOpenHelper).FullName, "Main", methodName, args);
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
