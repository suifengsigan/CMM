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
            InitEvent();
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

        void InitEvent()
        {
            dataGridView2.MouseDown += DataGridView2_MouseDown;
            this.Disposed += CAMConfigUserControl_Disposed;
        }
        private void _cms_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var dataGridViewPSelection = dataGridView2;
            var datasource1 = dataGridViewPSelection.DataSource as List<CAMConfig.OperationInfo>;
            if (e.ClickedItem.Text == "删除工序")
            {
                if (dataGridViewPSelection.CurrentRow != null)
                {
                    var obj = dataGridViewPSelection.CurrentRow.DataBoundItem as CAMConfig.OperationInfo;
                    if (obj != null)
                    {
                        datasource1.Remove(obj);
                        dataGridViewPSelection.DataSource = datasource1;
                        dataGridViewPSelection.Refresh();
                    }
                }

            }
            else if (e.ClickedItem.Text == "新增工序")
            {
                datasource1.Add(new CAMConfig.OperationInfo { });
                dataGridViewPSelection.Refresh();
                dataGridViewPSelection.DataSource = datasource1;
            }
        }

        private void CAMConfigUserControl_Disposed(object sender, EventArgs e)
        {
            CAMConfig.WriteConfig(_camConfig);
        }

        ContextMenuStrip _cms;
        private void DataGridView2_MouseDown(object sender, MouseEventArgs e)
        {
            var dataGridViewPSelection = dataGridView2;
            if (e.Button == MouseButtons.Right)
            {
                _cms = new ContextMenuStrip();
                _cms.Items.Add("新增工序");
                var list = dataGridViewPSelection.DataSource as List<EactConfig.ConfigData.PopertySelection> ?? new List<EactConfig.ConfigData.PopertySelection>();
                bool temp = dataGridViewPSelection.CurrentRow != null && dataGridViewPSelection.CurrentRow.Index >= 0 && dataGridViewPSelection.CurrentRow.Index < list.Count;
                if (temp)
                {
                    _cms.Items.Add("删除工序");
                }
                _cms.ItemClicked += _cms_ItemClicked;
                //弹出操作菜单
                _cms.Show(MousePosition.X, MousePosition.Y);
            }
        }
    }
}
