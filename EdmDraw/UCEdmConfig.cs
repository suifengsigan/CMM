using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace EdmDraw
{
    public partial class UCEdmConfig : UserControl
    {
        static string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("Config", "EdmConfig.json"));
        public UCEdmConfig()
        {
            InitializeComponent();
            InitEvent();
            Init();
        }

        public void Init()
        {
            InitDgv(dataGridView2);
            InitDgv(dataGridView1);

            List<string> _paramFileList = new List<string>();
            var members = new List<string>();
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EdmTemplate");
            if (System.IO.Directory.Exists(path))
            {
                _paramFileList = System.IO.Directory.GetFiles(path).ToList();
                _paramFileList.ForEach(u =>
                {
                    cbbEdmTemplate.Items.Add(System.IO.Path.GetFileNameWithoutExtension(u));
                });
            }

            //读取配置文件信息
            var config = GetInstance();

            dataGridView2.DataSource = config.DraftViewLocations ?? new List<EdmConfig.DraftViewLocation>();
            var tableInfo = config.Table ?? new EdmConfig.TableInfo();
            dataGridView1.DataSource = tableInfo.ColumnInfos;
            txtTableInfoX.Text = tableInfo.locationX.ToString();
            txtTableInfoY.Text = tableInfo.locationY.ToString();
            txtTableInfoColW.Text = tableInfo.ColumnWidth.ToString();
            txtTableInfoRowH.Text = tableInfo.RowHeight.ToString();
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            var config = GetInstance();
            config.DraftViewLocations = dataGridView2.DataSource as List<EdmConfig.DraftViewLocation> ?? new List<EdmConfig.DraftViewLocation>();
            config.Table = config.Table ?? new EdmConfig.TableInfo();
            config.Table.locationX = double.Parse(txtTableInfoX.Text);
            config.Table.locationY = double.Parse(txtTableInfoY.Text);
            config.Table.ColumnWidth = double.Parse(txtTableInfoColW.Text);
            config.Table.RowHeight = double.Parse(txtTableInfoRowH.Text);
            config.Table.ColumnInfos = dataGridView1.DataSource as List<EdmConfig.ColumnInfo> ?? new List<EdmConfig.ColumnInfo>();
            WriteConfig(config);
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

        public static EdmConfig GetInstance()
        {
            var json = string.Empty;
            if (File.Exists(_path))
            {
                json = File.ReadAllText(_path);
            }

            if (!string.IsNullOrEmpty(json))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<EdmConfig>(json) ?? new EdmConfig();
                result.Table = result.Table ?? new EdmConfig.TableInfo();
                return result;
            }
            return new EdmConfig();
        }

        public static void WriteConfig(EdmConfig data)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            File.WriteAllText(_path, json);
        }

        public void InitEvent()
        {
            dataGridView2.MouseDown += DataGridView2_MouseDown;
            dataGridView1.MouseDown += DataGridView1_MouseDown;
        }

        private void DataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            var dataGridViewPSelection = dataGridView1;
            if (e.Button == MouseButtons.Right)
            {
                _cms = new ContextMenuStrip();
                _cms.Items.Add("新增列信息");
                var list = dataGridViewPSelection.DataSource as List<EdmConfig.ColumnInfo> ?? new List<EdmConfig.ColumnInfo>();
                bool temp = dataGridViewPSelection.CurrentRow != null && dataGridViewPSelection.CurrentRow.Index >= 0 && dataGridViewPSelection.CurrentRow.Index < list.Count;
                if (temp)
                {
                    //_cms.Items.Add("修改加长杆");
                    _cms.Items.Add("删除列信息");
                }

                _cms.ItemClicked += _cms_ItemClicked;
                //弹出操作菜单
                _cms.Show(MousePosition.X, MousePosition.Y);
            }
        }


        private void DataGridView2_MouseDown(object sender, MouseEventArgs e)
        {
            var dataGridViewPSelection = dataGridView2;
            if (e.Button == MouseButtons.Right)
            {
                _cms = new ContextMenuStrip();
                _cms.Items.Add("新增");
                var list = dataGridViewPSelection.DataSource as List<EdmConfig.DraftViewLocation> ?? new List<EdmConfig.DraftViewLocation>();
                bool temp = dataGridViewPSelection.CurrentRow != null && dataGridViewPSelection.CurrentRow.Index >= 0 && dataGridViewPSelection.CurrentRow.Index < list.Count;
                if (temp)
                {
                    //_cms.Items.Add("修改加长杆");
                    _cms.Items.Add("删除");
                }

                _cms.ItemClicked += _cms_ItemClicked;
                //弹出操作菜单
                _cms.Show(MousePosition.X, MousePosition.Y);
            }
        }

        ContextMenuStrip _cms;

        private void _cms_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var dataGridViewPSelection = dataGridView2;
            var datasource = dataGridViewPSelection.DataSource as List<EdmConfig.DraftViewLocation> ?? new List<EdmConfig.DraftViewLocation>();

            var dataGridViewPSelection1 = dataGridView1;
            var datasource1= dataGridViewPSelection1.DataSource as List<EdmConfig.ColumnInfo> ?? new List<EdmConfig.ColumnInfo>();
            if (e.ClickedItem.Text == "新增")
            {
                datasource.Add(new EdmConfig.DraftViewLocation { });
                dataGridViewPSelection.DataSource = datasource.ToList();
            }
            else if (e.ClickedItem.Text == "新增列信息")
            {
                datasource1.Add(new EdmConfig.ColumnInfo { });
                dataGridViewPSelection1.DataSource = datasource1.ToList();
            }
            else if (e.ClickedItem.Text == "删除列信息")
            {
                if (dataGridViewPSelection1.CurrentRow != null)
                {
                    var obj = dataGridViewPSelection1.CurrentRow.DataBoundItem as EdmConfig.ColumnInfo;
                    if (obj != null)
                    {
                        datasource1.Remove(obj);
                        dataGridViewPSelection1.DataSource = datasource1.ToList();
                    }
                }

            }
            else
            {
                if (dataGridViewPSelection.CurrentRow != null)
                {
                    var obj = dataGridViewPSelection.CurrentRow.DataBoundItem as EdmConfig.DraftViewLocation;
                    if (obj != null)
                    {
                        datasource.Remove(obj);
                        dataGridViewPSelection.DataSource = datasource.ToList();
                    }
                }

            }
        }

    }
}
