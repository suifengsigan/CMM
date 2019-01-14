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

        public void Save()
        {
            CAMConfigUserControl_Disposed(null, null);
        }

        void InitUI()
        {
            InitDgv(dataGridView1);
            InitDgv(dataGridView2);
            InitDgv(dataGridView3);
            InitDgv(dataGridView4);
            dataGridView2.DataSource = _camConfig.Operations;
            dataGridView4.DataSource = _camConfig.Projects;
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
            //this.Disposed += CAMConfigUserControl_Disposed;
            dataGridView2.MouseDown += DataGridView2_MouseDown;
            dataGridView4.MouseDown += DataGridView4_MouseDown;
            dataGridView3.MouseDown += DataGridView3_MouseDown;
            dataGridView4.SelectionChanged += DataGridView4_SelectionChanged;
        }

        private void DataGridView4_SelectionChanged(object sender, EventArgs e)
        {
            var dataGridViewPSelection = dataGridView4;
            var list = dataGridViewPSelection.DataSource as List<CAMConfig.ProjectInfo> ?? new List<CAMConfig.ProjectInfo>();
            bool temp = dataGridViewPSelection.CurrentRow != null && dataGridViewPSelection.CurrentRow.Index >= 0 && dataGridViewPSelection.CurrentRow.Index < list.Count;
            if (temp)
            {
                var obj = dataGridViewPSelection.CurrentRow.DataBoundItem as CAMConfig.ProjectInfo;
                dataGridView3.DataSource = obj.Details;
            }
        }

        private void DataGridView3_MouseDown(object sender, MouseEventArgs e)
        {
            var dataGridViewPSelection = dataGridView3;
            if (e.Button == MouseButtons.Right)
            {
                _cms = new ContextMenuStrip();
                _cms.Items.Add("新增方案工序");
                var list = dataGridViewPSelection.DataSource as List<CAMConfig.ProjectDetail> ?? new List<CAMConfig.ProjectDetail>();
                bool temp = dataGridViewPSelection.CurrentRow != null && dataGridViewPSelection.CurrentRow.Index >= 0 && dataGridViewPSelection.CurrentRow.Index < list.Count;
                if (temp)
                {
                    _cms.Items.Add("删除方案工序");
                }
                _cms.ItemClicked += _cms_ItemClicked;
                //弹出操作菜单
                _cms.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void DataGridView4_MouseDown(object sender, MouseEventArgs e)
        {
            var dataGridViewPSelection = dataGridView4;
            if (e.Button == MouseButtons.Right)
            {
                _cms = new ContextMenuStrip();
                _cms.Items.Add("新增方案");
                var list = dataGridViewPSelection.DataSource as List<CAMConfig.ProjectInfo> ?? new List<CAMConfig.ProjectInfo>();
                bool temp = dataGridViewPSelection.CurrentRow != null && dataGridViewPSelection.CurrentRow.Index >= 0 && dataGridViewPSelection.CurrentRow.Index < list.Count;
                if (temp)
                {
                    _cms.Items.Add("删除方案");
                }
                _cms.ItemClicked += _cms_ItemClicked;
                //弹出操作菜单
                _cms.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void _cms_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var dataGridViewPSelection = dataGridView2;
            var datasource1 = dataGridViewPSelection.DataSource as List<CAMConfig.OperationInfo>;

            var dataGridViewPSelection2 = dataGridView4;
            var datasource2 = dataGridViewPSelection2.DataSource as List<CAMConfig.ProjectInfo>;

            var dataGridViewPSelection3 = dataGridView3;
            var datasource3 = dataGridViewPSelection3.DataSource as List<CAMConfig.ProjectDetail>;

            if (e.ClickedItem.Text == "删除工序")
            {
                if (dataGridViewPSelection.CurrentRow != null)
                {
                    var obj = dataGridViewPSelection.CurrentRow.DataBoundItem as CAMConfig.OperationInfo;
                    if (obj != null)
                    {
                        datasource1.Remove(obj);
                        _camConfig.Operations = datasource1.ToList();
                        dataGridViewPSelection.DataSource = _camConfig.Operations;
                    }
                }

            }
            else if (e.ClickedItem.Text == "新增工序")
            {
                datasource1.Add(new CAMConfig.OperationInfo { });
                _camConfig.Operations = datasource1.ToList();
                dataGridViewPSelection.DataSource = _camConfig.Operations;
            }
            else if (e.ClickedItem.Text == "删除方案")
            {
                if (dataGridViewPSelection2.CurrentRow != null)
                {
                    var obj = dataGridViewPSelection2.CurrentRow.DataBoundItem as CAMConfig.ProjectInfo;
                    if (obj != null)
                    {
                        datasource2.Remove(obj);
                        _camConfig.Projects = datasource2.ToList();
                        dataGridViewPSelection2.DataSource = _camConfig.Projects;
                    }
                }

            }
            else if (e.ClickedItem.Text == "新增方案")
            {
                datasource2.Add(new CAMConfig.ProjectInfo { });
                _camConfig.Projects = datasource2.ToList();
                dataGridViewPSelection2.DataSource = _camConfig.Projects;
            }
            else if (e.ClickedItem.Text == "删除方案工序")
            {
                if (dataGridViewPSelection2.CurrentRow != null && datasource3 != null)
                {
                    var obj1 = dataGridViewPSelection2.CurrentRow.DataBoundItem as CAMConfig.ProjectInfo;
                    var obj = dataGridViewPSelection3.CurrentRow.DataBoundItem as CAMConfig.ProjectDetail;
                    if (obj != null)
                    {
                        datasource3.Remove(obj);
                        obj1.Details = datasource3.ToList();
                        dataGridViewPSelection2.DataSource = obj1.Details;
                    }
                }
            }
            else if (e.ClickedItem.Text == "新增方案工序")
            {
                if (dataGridViewPSelection2.CurrentRow != null && datasource3 != null)
                {
                    var obj1 = dataGridViewPSelection2.CurrentRow.DataBoundItem as CAMConfig.ProjectInfo;
                    datasource3.Add(new CAMConfig.ProjectDetail { });
                    obj1.Details = datasource3.ToList();
                    dataGridViewPSelection2.DataSource = obj1.Details;
                }
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
                var list = dataGridViewPSelection.DataSource as List<CAMConfig.OperationInfo> ?? new List<CAMConfig.OperationInfo>();
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
