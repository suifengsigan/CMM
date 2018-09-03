using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CMMProgram
{
    static class Program
    {
        private static string UGBASEDIRUGII = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.IO.Directory.SetCurrentDirectory(UGBASEDIRUGII);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new s());
        }
    }
}
