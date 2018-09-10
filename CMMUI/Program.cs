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

        public static void InitUG()
        {
            AssemblyLoader.Entry.InitAssembly();
            _InitUG();
        }

        public static string[] Verification()
        {
            return CMM.Entry.Verification();
        }

        static void _InitUG()
        {
            CMM.Entry.InitUG();
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
