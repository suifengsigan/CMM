using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CMM
{
    public static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Show(args);
        }

        static void Show(string[] args)
        {
            System.Windows.Forms.MessageBox.Show("123");
        }

     
    }
}
