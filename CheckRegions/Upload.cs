using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckRegions
{
    public class Upload
    {
        public static void Main()
        {
            AssemblyLoader.Entry.InitAssembly();
            Show();
        }

        private static void Show()
        {
            CheckRegionsUI theCheckRegions = null;
            try
            {
                theCheckRegions = new CheckRegionsUI();
                theCheckRegions.Show();
            }
            catch (Exception ex)
            {
                NXOpen.UI.GetUI().NXMessageBox.Show("Block Styler", NXOpen.NXMessageBox.DialogType.Error, ex.ToString());
            }
            finally
            {
                if (theCheckRegions != null)
                    theCheckRegions.Dispose();
                theCheckRegions = null;
            }
        }
    }
}
