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
            CMM.Entry.Init();
        }

        static void Execute()
        {
            CMM.Entry.AutoSelPoint();
        }
    }
}
