using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CMMTool
{
    public static class Program
    {
        public static void Main()
        {
            AssemblyLoader.Entry.InitAssembly();
            new MainForm().ShowDialog();
        }
    }
}
