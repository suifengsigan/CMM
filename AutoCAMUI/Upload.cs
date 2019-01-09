using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    public static class Upload
    {
        public static void Main()
        {
            new EleAutoCamUI().Show();
        }

        /// <summary>
        /// 自动编程
        /// </summary>
        public static void AutoCam()
        {
            AutoCamBusiness.AutoCAM();
        }
        public static int GetUnloadOption(string arg)
        {
            //return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);
            return System.Convert.ToInt32(1);
            // return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
        }
    }
}
