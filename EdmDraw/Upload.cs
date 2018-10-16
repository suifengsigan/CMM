using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdmDraw
{
    public class Upload
    {
        public static void Main()
        {
            var configFrm = new System.Windows.Forms.Form();
            configFrm.Width = 600;
            configFrm.Height = 600;
            configFrm.Text = "图纸设置";
            var uc = new UCEdmConfig();
            uc.Dock = System.Windows.Forms.DockStyle.Fill;
            configFrm.Controls.Add(uc);
            configFrm.ShowDialog();
            new EdmDrawUI().Show();
        }
    }
}
