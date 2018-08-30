using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// 显示用户配置
        /// </summary>
        public static void ShowEactConfig()
        {
            AssemblyLoader.Entry.InitAssembly();
            _ShowEactConfig();
        }
        
        public static void _ShowEactConfig()
        {
            Snap.NX.Part basePart = null;
            try
            {
                if (NXOpen.Session.GetSession().Parts.Work == null)
                {
                    var filePath = Path.Combine(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config"), "blank.prt");
                    basePart = Snap.NX.Part.OpenPart(filePath);
                    Snap.Globals.WorkPart = basePart;
                }
                else
                {
                    basePart = NXOpen.Session.GetSession().Parts.Work;
                }
                EactConfig.Unload.Main();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (basePart != null)
                {
                    basePart.Close(true, true);
                }
            }
        }
    }
}
