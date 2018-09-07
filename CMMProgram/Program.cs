using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace CMMProgram
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            #region 系统只运行一次
            bool bCreatedNew;
            Mutex ltt = new Mutex(false, Path.GetFileNameWithoutExtension(Application.ExecutablePath), out bCreatedNew);
            if (!bCreatedNew)
            {
                MessageBox.Show("已有程序运行中！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            #endregion
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new s());
        }
    }
}
