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

        public static int CMMInit()
        {
            AssemblyLoader.Entry.InitAssembly();
            Init();
            return 0;
        }

        public static int InitUG()
        {
            AssemblyLoader.Entry.InitAssembly();
            _InitUG();
            return 0;
        }

        public static int Verification()
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

        public static int GetUnloadOption(string arg)
        {
            //return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);
            return System.Convert.ToInt32(1);
            // return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
        }
    }
}
