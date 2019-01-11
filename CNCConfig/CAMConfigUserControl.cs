using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CNCConfig
{
    public partial class CAMConfigUserControl : UserControl
    {
        public CAMConfigUserControl()
        {
            InitializeComponent();
            InitUI();
        }

        CAMConfig _camConfig = CAMConfig.GetInstance();

        void InitUI()
        {
            InitDgv(dataGridView1);
            var config=EactConfig.ConfigData.GetInstance();
            var poperty = config.Poperties.FirstOrDefault(u => u.DisplayName == "电极材质");
            if (poperty != null)
            {
                poperty.Selections.ForEach(s => {
                    cbCutterType.Items.Add(new ComboBoxItem { Text = s.Value + "电极刀具", Value = s.Value });
                });
            }
        }

        void InitDgv(DataGridView view)
        {
            view.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            view.ReadOnly = false;
            //view.ColumnHeadersVisible = false;
            view.RowHeadersVisible = false;
            view.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            view.AllowUserToResizeRows = false;
            view.MultiSelect = false;
        }
    }
}
