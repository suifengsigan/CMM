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
            configFrm.Controls.Add(new UCEdmConfig());
            configFrm.ShowDialog();
            new EdmDrawUI().Show();
        }
    }
}
