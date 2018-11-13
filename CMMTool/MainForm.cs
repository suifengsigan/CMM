using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CMMTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitDgv(dataGridView1);
            InitDgv(dataGridView2);
            dataGridView2.ReadOnly = false;
            InitDgv(dataGridViewExtensionBar);
            dataGridViewExtensionBar.ReadOnly = false;
            this.Load += MainForm_Load;
            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            this.FormClosing += MainForm_FormClosing;
            dataGridView2.MouseDown += DataGridView2_MouseDown;
            //dataGridView2.SelectionChanged += DataGridView2_SelectionChanged;
            dataGridViewExtensionBar.MouseDown += DataGridViewExtensionBar_MouseDown;
            //dataGridViewExtensionBar.SelectionChanged += DataGridViewExtensionBar_SelectionChanged;
            btnSelAutoCmmDir.Click += BtnSelAutoCmmDir_Click;
            btnSelGetPointFilePath.Click += BtnSelGetPointFilePath_Click;
            btnAutoPrtToolDir.Click += BtnAutoPrtToolDir_Click;
            dataGridView1.CellMouseDown += DataGridView1_CellMouseDown;
            dataGridView1.CellPainting += DataGridView1_CellPainting;
        }

        private void BtnSelGetPointFilePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                txtGetPointFilePath.Text = folderBrowser.SelectedPath;
            }
        }

        private void DataGridViewExtensionBar_SelectionChanged(object sender, EventArgs e)
        {
            var dataGridViewPSelection = dataGridViewExtensionBar;
            var list = (dataGridViewPSelection.DataSource as List<ProbeData.ExtensionBarData>) ?? new List<ProbeData.ExtensionBarData>();
            bool temp = dataGridViewPSelection.CurrentRow != null && dataGridViewPSelection.CurrentRow.Index >= 0 && dataGridViewPSelection.CurrentRow.Index < list.Count;
            if (temp)
            {
                var probeData = dataGridViewPSelection.CurrentRow.DataBoundItem as ProbeData.ExtensionBarData;
                //txt_Height.Text = probeData.Height.ToString();
                //txt_ED1.Text = probeData.D1.ToString();
                //txt_ED2.Text = probeData.D2.ToString();
            }
        }

        private void DataGridViewExtensionBar_MouseDown(object sender, MouseEventArgs e)
        {
            var dataGridViewPSelection = dataGridViewExtensionBar;
            if (e.Button == MouseButtons.Right)
            {
                _cms = new ContextMenuStrip();
                _cms.Items.Add("新增加长杆");
                var list = (dataGridViewPSelection.DataSource as List<ProbeData.ExtensionBarData>) ?? new List<ProbeData.ExtensionBarData>();
                bool temp = dataGridViewPSelection.CurrentRow != null && dataGridViewPSelection.CurrentRow.Index >= 0 && dataGridViewPSelection.CurrentRow.Index < list.Count;
                if (temp)
                {
                    //_cms.Items.Add("修改加长杆");
                    _cms.Items.Add("删除加长杆");
                }

                _cms.ItemClicked += _cms_ItemClicked;
                //弹出操作菜单
                _cms.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void DataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            var dataGridViewPSelection = dataGridView1;
            if (e.RowIndex > -1)
            {
                var obj = dataGridViewPSelection.Rows[e.RowIndex].DataBoundItem as ProbeData;
                if (obj.IsBaseFaceProbe)
                {
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
                }
                else
                {
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                }
            }
        }

        private void DataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            var dataGridViewPSelection = dataGridView1;
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0)
                {
                    var obj = dataGridViewPSelection.Rows[e.RowIndex].DataBoundItem as ProbeData;
                    _cms = new ContextMenuStrip();
                    _cms.Items.Add("设置基准面测针");
                    _cms.ItemClicked += _cms_ItemClicked;
                    //弹出操作菜单
                    _cms.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        private void BtnAutoPrtToolDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                txtAutoPrtToolDir.Text = folderBrowser.SelectedPath;
            }
        }

        private void BtnSelAutoCmmDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                txtAutoCmmDir.Text = folderBrowser.SelectedPath;
            }
        }

        private void DataGridView2_MouseDown(object sender, MouseEventArgs e)
        {
            var dataGridViewPSelection = dataGridView2;
            if (e.Button == MouseButtons.Right)
            {
                _cms = new ContextMenuStrip();
                _cms.Items.Add("新增");
                var list = (dataGridView2.DataSource as List<ProbeData.AB>) ?? new List<ProbeData.AB>();
                bool temp = dataGridView2.CurrentRow != null && dataGridView2.CurrentRow.Index >= 0 && dataGridView2.CurrentRow.Index < list.Count;
                if (temp)
                {
                    //_cms.Items.Add("修改");
                    _cms.Items.Add("删除");
                }
              
                _cms.ItemClicked += _cms_ItemClicked;
                //弹出操作菜单
                _cms.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void DataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            var list = (dataGridView2.DataSource as List<ProbeData.AB>) ?? new List<ProbeData.AB>();
            bool temp = dataGridView2.CurrentRow != null && dataGridView2.CurrentRow.Index >= 0 && dataGridView2.CurrentRow.Index < list.Count;
            if (temp)
            {
                var probeData = dataGridView2.CurrentRow.DataBoundItem as ProbeData.AB;
                //txtA.Text = probeData.A.ToString();
                //txtB.Text = probeData.B.ToString();
            }
        }

        ContextMenuStrip _cms;

        private void _cms_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var dataGridViewPSelection = dataGridView2;
            double A = 0;
            double B = 0;
            //double.TryParse(txtA.Text, out A);
            //double.TryParse(txtB.Text, out B);
            var datasource= dataGridViewPSelection.DataSource as List<ProbeData.AB> ?? new List<ProbeData.AB>();
            double Height = 0;
            double ED1 = 0;
            double ED2 = 0;
            //double.TryParse(txt_Height.Text, out Height);
            //double.TryParse(txt_ED1.Text, out ED1);
            //double.TryParse(txt_ED2.Text, out ED2);
            var dataGridViewPSelection1 = dataGridViewExtensionBar;
            var datasource1 = dataGridViewPSelection1.DataSource as List<ProbeData.ExtensionBarData> ?? new List<ProbeData.ExtensionBarData>();
            if (e.ClickedItem.Text == "新增")
            {
                datasource.Add(new ProbeData.AB { A = A, B = B });
                dataGridViewPSelection.DataSource = datasource.ToList();
            }
            else if (e.ClickedItem.Text == "修改")
            {
                if (dataGridViewPSelection.CurrentRow != null)
                {
                    var obj = dataGridViewPSelection.CurrentRow.DataBoundItem as ProbeData.AB;
                    if (obj != null)
                    {
                        obj.A = A;
                        obj.B = B;
                    }
                    dataGridViewPSelection.Refresh();
                }
            }
            else if (e.ClickedItem.Text == "新增加长杆")
            {
                datasource1.Add(new ProbeData.ExtensionBarData { Height = Height, D1 = ED1, D2 = ED2 });
                dataGridViewPSelection1.DataSource = datasource1.ToList();
            }
            else if (e.ClickedItem.Text == "修改加长杆")
            {
                if (dataGridViewPSelection1.CurrentRow != null)
                {
                    var obj = dataGridViewPSelection1.CurrentRow.DataBoundItem as ProbeData.ExtensionBarData;
                    if (obj != null)
                    {
                        obj.Height = Height;
                        obj.D1 = ED1;
                        obj.D2 = ED2;
                    }
                    dataGridViewPSelection.Refresh();
                }
            }
            else if (e.ClickedItem.Text == "删除加长杆")
            {
                if (dataGridViewPSelection.CurrentRow != null)
                {
                    var obj = dataGridViewPSelection1.CurrentRow.DataBoundItem as ProbeData.ExtensionBarData;
                    if (obj != null)
                    {
                        datasource1.Remove(obj);

                        dataGridViewPSelection1.DataSource = datasource1.ToList();
                    }
                }
            }
            else if (e.ClickedItem.Text == "设置基准面测针")
            {
                dataGridView1.Rows.Cast<DataGridViewRow>().ToList().ForEach(u => {
                    var data = u.DataBoundItem as ProbeData;
                    var currentRow = dataGridView1.CurrentRow;
                    if (currentRow == u)
                    {
                        data.IsBaseFaceProbe = true;
                    }
                    else
                    {
                        data.IsBaseFaceProbe = false;
                    }
                });

                dataGridView1.Refresh();
            }
            else
            {
                if (dataGridViewPSelection.CurrentRow != null)
                {
                    var obj = dataGridViewPSelection.CurrentRow.DataBoundItem as ProbeData.AB;
                    if (obj != null)
                    {
                        datasource.Remove(obj);

                        dataGridViewPSelection.DataSource = datasource.ToList();
                    }
                }

            }
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var datasource = dataGridView1.DataSource as List<ProbeData>;
            var result = new CMMConfig { ProbeDatas = datasource };
            result.AutoCmmDir = txtAutoCmmDir.Text;
            result.AutoPrtToolDir = txtAutoPrtToolDir.Text;
            result.EntryPoint = double.Parse(txtEntryPoint.Text);
            result.RetreatPoint = double.Parse(txtRetreatPoint.Text);
            result.SafeDistance = double.Parse(txtSafeDistance.Text);
            result.VerticalValue = double.Parse(txtVerticalValue.Text);
            result.IsInspectionPath = cbIsInspectionPath.Checked;
            result.IsAutoCmmFtpDir = cbIsAutoCmmFtpDir.Checked;
            result.IsSelGetPointFilePath = cbIsSelGetPointFilePath.Checked;
            result.GetPointFilePath = txtGetPointFilePath.Text;
            result.IsEDMFaceGetPoint = cbIsEDMFaceGetPoint.Checked;
            result.IsInitConfig = cbIsInitConfig.Checked;
            result.IsUploadDataBase = cbIsUploadDatabase.Checked;
            result.IsBaseRoundInt = cbIsBaseRoundInt.Checked;
            result.IsGetTowPointArea = cbIsGetTowPointArea.Checked;
            result.IsMinGetPointArea = cbIsMinGetPointArea.Checked;
            result.GetTowPointArea = double.Parse(txtGetTowPointArea.Text);
            result.MinGetPointArea = double.Parse(txtMinGetPointArea.Text);
            result.MinEdgeDistance = double.Parse(txtMinEdgeDistance.Text);
            CMMConfig.WriteConfig(result);
            if (result.IsInitConfig)
            {
                CMMTool.Business.InitConfig();
            }
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            var list = (dataGridView1.DataSource as List<ProbeData>) ?? new List<ProbeData>();
            bool temp = dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Index >= 0 && dataGridView1.CurrentRow.Index < list.Count;
            if (temp)
            {
                var probeData = dataGridView1.CurrentRow.DataBoundItem as ProbeData;
                txtProbeName.Text = probeData.ProbeName;
                probeData.ProbeName = txtProbeName.Text;
                txtD.Text = probeData.D.ToString();
                txtL.Text = probeData.L.ToString();
                txt_d.Text = probeData.d.ToString();
                txtD1.Text = probeData.D1.ToString();
                txtD2.Text = probeData.D2.ToString();
                txtD3.Text = probeData.D3.ToString();
                txtL1.Text = probeData.L1.ToString();
                txtL2.Text = probeData.L2.ToString();
                dataGridView2.DataSource = probeData.GetABList().ToList();
                dataGridViewExtensionBar.DataSource = probeData.ExtensionBarDataList;
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            bool temp = dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Index >= 0;
            if (temp)
            {
                var probeData = dataGridView1.CurrentRow.DataBoundItem as ProbeData;
                var datasource = dataGridView1.DataSource as List<ProbeData>;
                datasource.Remove(probeData);
                dataGridView1.DataSource = datasource.ToList();
                System.Windows.Forms.MessageBox.Show("删除成功");
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var probeData = new ProbeData();
            probeData.ProbeName = txtProbeName.Text;
            probeData.D = double.Parse(txtD.Text);
            probeData.L = double.Parse(txtL.Text);
            probeData.d = double.Parse(txt_d.Text);
            probeData.D1 = double.Parse(txtD1.Text);
            probeData.D2 = double.Parse(txtD2.Text);
            probeData.D3 = double.Parse(txtD3.Text);
            probeData.L1 = double.Parse(txtL1.Text);
            probeData.L2 = double.Parse(txtL2.Text);
            probeData.ABList = dataGridView2.DataSource as List<ProbeData.AB> ?? new List<ProbeData.AB>();
            probeData.ExtensionBarDataList= dataGridViewExtensionBar.DataSource as List<ProbeData.ExtensionBarData> ?? new List<ProbeData.ExtensionBarData>();
            var datasource = dataGridView1.DataSource as List<ProbeData>;
            datasource.Add(probeData);
            dataGridView1.DataSource = datasource.ToList();
            System.Windows.Forms.MessageBox.Show("新增成功");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var result = CMMConfig.GetInstance();
            var data = result.ProbeDatas ?? new List<ProbeData>();
            cbIsUploadDatabase.Checked = result.IsUploadDataBase;
            txtAutoCmmDir.Text = result.AutoCmmDir;
            txtAutoPrtToolDir.Text = result.AutoPrtToolDir;
            txtEntryPoint.Text = result.EntryPoint.ToString();
            txtRetreatPoint.Text = result.RetreatPoint.ToString();
            txtSafeDistance.Text = result.SafeDistance.ToString();
            txtVerticalValue.Text = result.VerticalValue.ToString();
            cbIsInspectionPath.Checked = result.IsInspectionPath;
            cbIsAutoCmmFtpDir.Checked = result.IsAutoCmmFtpDir;
            cbIsEDMFaceGetPoint.Checked = result.IsEDMFaceGetPoint;
            cbIsInitConfig.Checked = result.IsInitConfig;
            cbIsBaseRoundInt.Checked = result.IsBaseRoundInt;
            cbIsGetTowPointArea.Checked = result.IsGetTowPointArea;
            cbIsMinGetPointArea.Checked = result.IsMinGetPointArea;
            txtGetTowPointArea.Text = result.GetTowPointArea.ToString();
            txtMinGetPointArea.Text = result.MinGetPointArea.ToString();
            txtGetPointFilePath.Text = result.GetPointFilePath;
            cbIsSelGetPointFilePath.Checked = result.IsSelGetPointFilePath;
            dataGridView1.DataSource = data;
            txtMinEdgeDistance.Text = result.MinEdgeDistance.ToString();
        }

        void InitDgv(DataGridView view)
        {
            view.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            view.ReadOnly = true;
            //view.ColumnHeadersVisible = false;
            view.RowHeadersVisible = false;
            view.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            view.AllowUserToResizeRows = false;
            view.MultiSelect = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool temp = dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Index >= 0;
            if (temp)
            {
                var probeData = dataGridView1.CurrentRow.DataBoundItem as ProbeData;
                probeData.ProbeName = txtProbeName.Text;
                probeData.D = double.Parse(txtD.Text);
                probeData.L = double.Parse(txtL.Text);
                probeData.d = double.Parse(txt_d.Text);
                probeData.D1 = double.Parse(txtD1.Text);
                probeData.D2 = double.Parse(txtD2.Text);
                probeData.D3 = double.Parse(txtD3.Text);
                probeData.L1 = double.Parse(txtL1.Text);
                probeData.L2 = double.Parse(txtL2.Text);
                probeData.ABList = dataGridView2.DataSource as List<ProbeData.AB> ?? new List<ProbeData.AB>();
                probeData.ExtensionBarDataList = dataGridViewExtensionBar.DataSource as List<ProbeData.ExtensionBarData> ?? new List<ProbeData.ExtensionBarData>();
                var datasource = dataGridView1.DataSource as List<ProbeData>;
                dataGridView1.DataSource = datasource.ToList();
                System.Windows.Forms.MessageBox.Show("保存成功");
            }

        }
    }
}
