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
    }
}
