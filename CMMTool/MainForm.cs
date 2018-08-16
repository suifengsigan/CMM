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
        static string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("Config", "ProbeData.json"));
        public static void WriteConfig(CMMConfig data)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            File.WriteAllText(_path, json);
        }
        public static CMMConfig GetInstance()
        {
            var json = string.Empty;
            if (File.Exists(_path))
            {
                json = File.ReadAllText(_path);
            }

            if (!string.IsNullOrEmpty(json))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<CMMConfig> (json) ?? new CMMConfig();
            }
            return new CMMConfig();
        }
        public MainForm()
        {
            InitializeComponent();
            InitDgv(dataGridView1);
            this.Load += MainForm_Load;
            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var datasource = dataGridView1.DataSource as List<ProbeData>;
            WriteConfig(new CMMConfig { ProbeDatas = datasource });
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
                textAB.Text = probeData.ProbeAB;
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
            probeData.ProbeAB = textAB.Text;
            var datasource = dataGridView1.DataSource as List<ProbeData>;
            datasource.Add(probeData);
            dataGridView1.DataSource = datasource.ToList();
            System.Windows.Forms.MessageBox.Show("新增成功");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var data=GetInstance().ProbeDatas;
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
                probeData.ProbeAB = textAB.Text;
                var datasource = dataGridView1.DataSource as List<ProbeData>;
                dataGridView1.DataSource = datasource.ToList();
                System.Windows.Forms.MessageBox.Show("保存成功");
            }

        }
    }
}
