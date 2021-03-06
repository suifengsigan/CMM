﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMMToolEx
{
    public static class Program
    {
        public static void Main()
        {
            AssemblyLoader.Entry.InitAssembly();
            CMMTool.Program.Main();
        }

        /// <summary>
        /// 显示用户配置
        /// </summary>
        public static int ShowEactConfig()
        {
            AssemblyLoader.Entry.InitAssembly();
            CMMTool.Program.ShowEactConfig();
            return 0;
        }
        
        public static int GetUnloadOption(string arg)
        {
            //return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);
            return System.Convert.ToInt32(1);
            // return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
        }
    }
}
