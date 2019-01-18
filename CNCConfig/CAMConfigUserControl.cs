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
            InitDgv(dgv_Data);
            InitDgv(dataGridView4);

            var 模版类型_Column = new DataGridViewComboBoxColumn();
            模版类型_Column.Name = "模版类型";
            模版类型_Column.DataPropertyName = "模版类型";
            模版类型_Column.Items.Add(CAMConfig.S_OperationTemplate.Default);
            模版类型_Column.Items.Add(CAMConfig.S_OperationTemplate.EACT_AUTOCAM);
            dataGridView2.Columns.Add(模版类型_Column);

            var 工序_Column = new DataGridViewComboBoxColumn();
            工序_Column.Name = "工序";
            工序_Column.DataPropertyName = "工序";
            var operations = _camConfig.Operations.ToList();

            operations.ForEach(u => { 工序_Column.Items.Add(u.显示名称 ?? string.Empty); });

            _camConfig.Projects.ForEach(p => {
                p.Details.ForEach(u => {
                    if (operations.Where(m => m.显示名称 == u.工序).Count() <= 0)
                    {
                        工序_Column.Items.Add(u.工序 ?? string.Empty);
                    }
                });
            });
            dgv_Data.Columns.Add(工序_Column);

            var 刀具_Column = new DataGridViewComboBoxColumn();
            刀具_Column.Name = "刀具";
            刀具_Column.DataPropertyName = "刀具";
            var cutterDetails = new List<CAMConfig.CutterDetail>();
            if (_camConfig.Cutters.Count > 0)
            {
                cutterDetails = _camConfig.Cutters.First().Details;
                cutterDetails.ForEach(u =>
                {
                    刀具_Column.Items.Add(u.刀具名称 ?? string.Empty);
                });
            }
            _camConfig.Projects.ForEach(p =>
            {
                p.Details.ForEach(u =>
                {
                    if (cutterDetails.Where(m => m.刀具名称 == u.刀具).Count() <= 0)
                    {
                        刀具_Column.Items.Add(u.刀具 ?? string.Empty);
                    }
                });
            });
            dgv_Data.Columns.Add(刀具_Column);

            var 参考刀具_Column = new DataGridViewComboBoxColumn();
            参考刀具_Column.Name = "参考刀具";
            参考刀具_Column.DataPropertyName = "参考刀具";
            cutterDetails.ForEach(u => {
                参考刀具_Column.Items.Add(u.刀具名称 ?? string.Empty);
            });
            _camConfig.Projects.ForEach(p => {
                p.Details.ForEach(u => {
                    if (cutterDetails.Where(m => m.刀具名称 == u.参考刀具).Count() <= 0)
                    {
                        参考刀具_Column.Items.Add(u.参考刀具 ?? string.Empty);
                    }
                });
            });
            dgv_Data.Columns.Add(参考刀具_Column);
            Action<DataGridViewComboBoxColumn> distinctAction = (c) => {
                c.Items.Add(string.Empty);
                var result = c.Items.Cast<string>().ToList().Distinct();
                c.Items.Clear();
                result.ToList().ForEach(u => {
                    c.Items.Add(u);
                });
            };
            distinctAction(工序_Column);
            distinctAction(参考刀具_Column);
            distinctAction(刀具_Column);
            dataGridView2.DataSource = _camConfig.Operations;
            dataGridView4.DataSource = _camConfig.Projects;
            var config=EactConfig.ConfigData.GetInstance();
            var poperty = config.Poperties.FirstOrDefault(u => u.DisplayName == "电极材质");
            if (poperty != null)
            {
                poperty.Selections.ForEach(s => {
                    cbCutterType.Items.Add(new ComboBoxItem { Text = s.Value + "电极刀具", Value = s.Value });
                });

                if (cbCutterType.Items.Count > 0)
                {
                    cbCutterType.SelectedItem = cbCutterType.Items[0];
                }
            }

            txtCAVITYPartStock.Text = _camConfig.CAVITYPartStock.ToString();
            txtCAVITYFloorStock.Text = _camConfig.CAVITYFloorStock.ToString();

            //火花位
            cbbSparkPosition.Items.Add(new ComboBoxItem { Text = "余量", Value = CAMConfig.E_SparkPosition.Stock });
            cbbSparkPosition.Items.Add(new ComboBoxItem { Text = "骗刀Z", Value = CAMConfig.E_SparkPosition.CheatKnifeZ });
            cbbSparkPosition.Items.Add(new ComboBoxItem { Text = "骗刀", Value = CAMConfig.E_SparkPosition.CheatKnife });

            var items = cbbSparkPosition.Items.Cast<ComboBoxItem>().ToList();
            cbbSparkPosition.SelectedIndex = items.IndexOf(items.FirstOrDefault(u => (CAMConfig.E_SparkPosition)u.Value == _camConfig.SparkPosition));
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
            dgv_Data.MouseDown += DataGridView3_MouseDown;
            dataGridView1.MouseDown += DataGridView1_MouseDown;
            dataGridView4.SelectionChanged += DataGridView4_SelectionChanged;
            cbCutterType.SelectedIndexChanged += CbCutterType_SelectedIndexChanged;
            btnUp.Click += BtnUp_Click;
            btnDown.Click += BtnDown_Click;
        }

        private void BtnDown_Click(object sender, EventArgs e)
        {
            var dataGridViewPSelection3 = dgv_Data;
            var datasource3 = dataGridViewPSelection3.DataSource as List<CAMConfig.ProjectDetail> ?? new List<CAMConfig.ProjectDetail>();
            bool temp = dataGridViewPSelection3.CurrentRow != null && dataGridViewPSelection3.CurrentRow.Index >= 0 && dataGridViewPSelection3.CurrentRow.Index < datasource3.Count;
            var dataGridViewPSelection2 = dataGridView4;
            if (dataGridViewPSelection2.CurrentRow != null && true)
            {
                var obj1 = dataGridViewPSelection2.CurrentRow.DataBoundItem as CAMConfig.ProjectInfo;
                var obj = dataGridViewPSelection3.CurrentRow.DataBoundItem as CAMConfig.ProjectDetail;
                if (obj != null)
                {
                    var dataIndex = datasource3.IndexOf(obj);
                    dataIndex++;
                    if (dataIndex <= datasource3.Count - 1)
                    {
                        datasource3.Remove(obj);
                        datasource3.Insert(dataIndex, obj);
                        obj1.Details = datasource3.ToList();
                        dataGridViewPSelection3.DataSource = obj1.Details;
                    }
                }
            }
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {
            var dataGridViewPSelection3 = dgv_Data;
            var datasource3 = dataGridViewPSelection3.DataSource as List<CAMConfig.ProjectDetail> ?? new List<CAMConfig.ProjectDetail>();
            bool temp = dataGridViewPSelection3.CurrentRow != null && dataGridViewPSelection3.CurrentRow.Index >= 0 && dataGridViewPSelection3.CurrentRow.Index < datasource3.Count;
            var dataGridViewPSelection2 = dataGridView4;
            if (dataGridViewPSelection2.CurrentRow != null && true)
            {
                var obj1 = dataGridViewPSelection2.CurrentRow.DataBoundItem as CAMConfig.ProjectInfo;
                var obj = dataGridViewPSelection3.CurrentRow.DataBoundItem as CAMConfig.ProjectDetail;
                if (obj != null)
                {
                    var dataIndex = datasource3.IndexOf(obj);
                    dataIndex--;
                    if (dataIndex >= 0)
                    {
                        datasource3.Remove(obj);
                        datasource3.Insert(dataIndex, obj);
                        obj1.Details = datasource3.ToList();
                        dataGridViewPSelection3.DataSource = obj1.Details;
                    }
                }
            }
        }

        private void CbCutterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = cbCutterType.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                dataGridView1.DataSource = _camConfig.GetCutters(item.Value.ToString()).Details;
            }
        }

        private void DataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            var item = cbCutterType.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                var dataGridViewPSelection = dataGridView1;
                if (e.Button == MouseButtons.Right)
                {
                    _cms = new ContextMenuStrip();
                    _cms.Items.Add("新增刀具");
                    var list = dataGridViewPSelection.DataSource as List<CAMConfig.CutterDetail> ?? new List<CAMConfig.CutterDetail>();
                    bool temp = dataGridViewPSelection.CurrentRow != null && dataGridViewPSelection.CurrentRow.Index >= 0 && dataGridViewPSelection.CurrentRow.Index < list.Count;
                    if (temp)
                    {
                        _cms.Items.Add("删除刀具");
                    }
                    _cms.ItemClicked += _cms_ItemClicked;
                    //弹出操作菜单
                    _cms.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        private void DataGridView4_SelectionChanged(object sender, EventArgs e)
        {
            var dataGridViewPSelection = dataGridView4;
            var list = dataGridViewPSelection.DataSource as List<CAMConfig.ProjectInfo> ?? new List<CAMConfig.ProjectInfo>();
            bool temp = dataGridViewPSelection.CurrentRow != null && dataGridViewPSelection.CurrentRow.Index >= 0 && dataGridViewPSelection.CurrentRow.Index < list.Count;
            if (temp)
            {
                var obj = dataGridViewPSelection.CurrentRow.DataBoundItem as CAMConfig.ProjectInfo;
                dgv_Data.DataSource = obj.Details;
            }
        }

        private void DataGridView3_MouseDown(object sender, MouseEventArgs e)
        {
            var dataGridViewPSelection = dgv_Data;
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

            var dataGridViewPSelection3 = dgv_Data;
            var datasource3 = dataGridViewPSelection3.DataSource as List<CAMConfig.ProjectDetail>;

            var dataGridViewPSelection4 = dataGridView1;
            var datasource4 = dataGridViewPSelection4.DataSource as List<CAMConfig.CutterDetail>;

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
                        dataGridViewPSelection3.DataSource = obj1.Details;
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
                    dataGridViewPSelection3.DataSource = obj1.Details;
                }
            }
            else if (e.ClickedItem.Text == "删除刀具")
            {
                var item = cbCutterType.SelectedItem as ComboBoxItem;
                if (item != null)
                {
                    var info = _camConfig.GetCutters(item.Value.ToString());
                    if (dataGridViewPSelection4.CurrentRow != null)
                    {
                        var obj = dataGridViewPSelection4.CurrentRow.DataBoundItem as CAMConfig.CutterDetail;
                        if (obj != null)
                        {
                            datasource4.Remove(obj);
                            info.Details = datasource4.ToList();
                            dataGridViewPSelection4.DataSource = info.Details;
                        }
                    }
                }
               

            }
            else if (e.ClickedItem.Text == "新增刀具")
            {
                var item = cbCutterType.SelectedItem as ComboBoxItem;
                if (item != null)
                {
                    var info = _camConfig.GetCutters(item.Value.ToString());
                    datasource4.Add(new CAMConfig.CutterDetail { });
                    info.Details = datasource4.ToList();
                    dataGridViewPSelection4.DataSource = info.Details;
                }
            }
        }

        private void CAMConfigUserControl_Disposed(object sender, EventArgs e)
        {
            _camConfig.SparkPosition = (CAMConfig.E_SparkPosition)(cbbSparkPosition.SelectedItem as ComboBoxItem).Value;
            _camConfig.CAVITYPartStock = double.Parse(txtCAVITYPartStock.Text);
            _camConfig.CAVITYFloorStock = double.Parse(txtCAVITYFloorStock.Text);
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

        private void btnExportCutter_Click(object sender, EventArgs e)
        {
            var item = cbCutterType.SelectedItem as ComboBoxItem;
            var dataGridViewPSelection4 = dataGridView1;
            var datasource4 = dataGridViewPSelection4.DataSource as List<CAMConfig.CutterDetail>;
            if (item != null)
            {
                var file = new OpenFileDialog();
                file.Filter = "表格文件(*.xls)|*.xls";
                if (file.ShowDialog() == DialogResult.OK)
                {
                    var fileName = file.FileName;
                    var dataTable = Common.ExcelToDataTableEx(fileName, 0, string.Empty, true);
                    var list = new DbTableConvertor<XKCutterInfo>().ConvertToList(dataTable);
                    list = list.Where(u => !string.IsNullOrEmpty(u.刀具名称)).ToList();
                    var info = _camConfig.GetCutters(item.Value.ToString());
                    list.ForEach(u => {
                        var detail = Newtonsoft.Json.JsonConvert.DeserializeObject<CAMConfig.CutterDetail>(Newtonsoft.Json.JsonConvert.SerializeObject(u));
                        datasource4.Add(detail);
                    });
                    info.Details = datasource4.ToList();
                    dataGridViewPSelection4.DataSource = info.Details;
                }
               
            }
            
        }

        private void btnClearCutter_Click(object sender, EventArgs e)
        {
            var item = cbCutterType.SelectedItem as ComboBoxItem;
            var dataGridViewPSelection4 = dataGridView1;
            var datasource4 = dataGridViewPSelection4.DataSource as List<CAMConfig.CutterDetail>;
            if (item != null)
            {
                var info = _camConfig.GetCutters(item.Value.ToString());
                datasource4.Clear();
                info.Details = datasource4.ToList();
                dataGridViewPSelection4.DataSource = info.Details;

            }
        }
    }
}
