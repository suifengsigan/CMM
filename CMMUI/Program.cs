using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CMMUI
{
    public static class Program
    {
        public static void Main()
        {
            AssemblyLoader.Entry.InitAssembly();
            
            Execute();
        }

        static void Execute()
        {
            CSharpProxy.ProxyObject.Instance.ShowMsg("开始");
            CMM.Entry.Test();
            CSharpProxy.ProxyObject.Instance.ShowMsg(string.Format("{0}  结束",DateTime.Now));
        }
    }
}
