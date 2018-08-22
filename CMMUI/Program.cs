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

        public static void CMMInit()
        {
            AssemblyLoader.Entry.InitAssembly();
            Init();
          
        }

        static void Init()
        {
            CSharpProxy.ProxyObject.Instance.ShowMsg("初始化配置数据...");
            CMM.Entry.Init();
            CSharpProxy.ProxyObject.Instance.ShowMsg("完成初始化配置数据");
        }

        static void Execute()
        {
            CSharpProxy.ProxyObject.Instance.ShowMsg("开始自动取点");
            CMM.Entry.AutoSelPoint();
            CSharpProxy.ProxyObject.Instance.ShowMsg("结束自动取点");
        }
    }
}
