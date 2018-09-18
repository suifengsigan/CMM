using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMMManualUI
{
    public class Unload
    {
        public static void Main()
        {
            AssemblyLoader.Entry.InitAssembly();
            Show();
        }

        private static void Show()
        {
            CMMManual.Unload.Main();
        }

        public static int GetUnloadOption(string arg)
        {
            //return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);
            return System.Convert.ToInt32(1);
            // return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
        }
    }
}
