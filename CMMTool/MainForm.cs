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
            this.Load += MainForm_Load;
            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            this.FormClosing += MainForm_FormClosing;
            dataGridView2.MouseDown += DataGridView2_MouseDown;
            dataGridView2.SelectionChanged += DataGridView2_SelectionChanged;
            btnSelAutoCmmDir.Click += BtnSelAutoCmmDir_Click;
            btnAutoPrtToolDir.Click += BtnAutoPrtToolDir_Click;
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
                    _cms.Items.Add("修改");
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
                txtA.Text = probeData.A.ToString();
                txtB.Text = probeData.B.ToString();
            }
        }

        ContextMenuStrip _cms;

        private void _cms_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var dataGridViewPSelection = dataGridView2;
            double A = 0;
            double B = 0;
            double.TryParse(txtA.Text, out A);
            double.TryParse(txtB.Text, out B);
            var datasource= dataGridViewPSelection.DataSource as List<ProbeData.AB> ?? new List<ProbeData.AB>(); 
            if (e.ClickedItem.Text == "新增")
            {
                datasource.Insert(0, new ProbeData.AB { A = A, B = B });
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
            CMMConfig.WriteConfig(result);
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
            var datasource = dataGridView1.DataSource as List<ProbeData>;
            datasource.Add(probeData);
            dataGridView1.DataSource = datasource.ToList();
            System.Windows.Forms.MessageBox.Show("新增成功");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var result = CMMConfig.GetInstance();
            var data = result.ProbeDatas ?? new List<ProbeData>();
            txtAutoCmmDir.Text = result.AutoCmmDir;
            txtAutoPrtToolDir.Text = result.AutoPrtToolDir;
            txtEntryPoint.Text = result.EntryPoint.ToString();
            txtRetreatPoint.Text = result.RetreatPoint.ToString();
            txtSafeDistance.Text = result.SafeDistance.ToString();
            txtVerticalValue.Text = result.VerticalValue.ToString();
            dataGridView1.DataSource = data;
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
                var datasource = dataGridView1.DataSource as List<ProbeData>;
                dataGridView1.DataSource = datasource.ToList();
                System.Windows.Forms.MessageBox.Show("保存成功");
            }

        }
    }
}
