using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CMMManual
{
    public class Unload
    {
        public static void Main()
        {
            Show();
        }

        private static void Show()
        {
            var mark = Snap.Globals.SetUndoMark(Snap.Globals.MarkVisibility.Invisible, "CMMManualShow");
            try
            {
                //导入探针数据
                var ui = new CMMProgramUI();
                ui.Show();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                try
                {
                    Snap.Globals.UndoToMark(mark, null);
                }
                catch (Exception ex1)
                {
                    System.Windows.Forms.MessageBox.Show(ex1.Message);
                }
            }
        }

    }
}
