using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdmDrawUIEx2
{
    public static class Program
    {
        public static void Main()
        {
            AssemblyLoader.Entry.InitAssembly();
            EdmDraw.Upload.Main();
        }

        public static int GetUnloadOption(string arg)
        {
            //return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);
            return System.Convert.ToInt32(1);
            // return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
        }
    }
}
