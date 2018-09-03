using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMMManual
{
    public class Unload
    {
        public static void Main()
        {
            try
            {
                Show();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private static void Show()
        {
            var ui = new CMMProgramUI();
            ui.Show();
        }

    }
}
