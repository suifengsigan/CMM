using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAM
{
    public static class Upload
    {
        public static void Main()
        {
            AssemblyLoader.Entry.InitAssembly();
            AutoCAMUI.Upload.Main();
        }
        public static int GetUnloadOption(string arg)
        {
            //return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);
            return System.Convert.ToInt32(1);
            // return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
        }
    }
}
