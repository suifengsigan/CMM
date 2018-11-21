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
            InitDgv(dataGridView3);

            List<string> _paramFileList = new List<string>();
            var members = new List<string>();
            var path = EdmConfig.GetEdmTemplatePath();
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.GetFiles(path).ToList().ForEach(u =>
                {
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(u);
                    cbbEdmTemplate.Items.Add(fileName);
                    _paramFileList.Add(fileName);
                });
            }

            //读取配置文件信息
            var config = GetInstance();

            dataGridView2.DataSource = config.DraftViewLocations ?? new List<EdmConfig.DraftViewLocation>();
            dataGridView3.DataSource = config.PropertyInfos ?? new List<EdmConfig.PropertyInfo>();
            var tableInfo = config.Table ?? new EdmConfig.TableInfo();
            dataGridView1.DataSource = tableInfo.ColumnInfos;
            txtTableInfoX.Text = tableInfo.locationX.ToString();
            txtTableInfoY.Text = tableInfo.locationY.ToString();
            txtTableInfoColW.Text = tableInfo.ColumnWidth.ToString();
            txtTableInfoRowH.Text = tableInfo.RowHeight.ToString();

            cbbEdmTemplate.SelectedIndex = _paramFileList.IndexOf(config.EdmTemplate);

            cbIsUseSystemParam.Checked = config.IsUseSystemConfig;
            txtPageCount.Text = config.PageCount.ToString();
            TextMpi88.Text = config.TextMpi88;
            TextMpr44.Text = config.TextMpr44.ToString();
            TextMpr46.Text = config.TextMpr46.ToString();
            TextMpr45.Text = config.TextMpr45.ToString();
            TextMpi89.Text = config.TextMpi89.ToString();
            DimensionMpi85.Text = config.DimensionMpi85;
            DimensionMpi3.Text = config.DimensionMpi3.ToString();
            DimensionMpr32.Text = config.DimensionMpr32.ToString();
            DimensionMpr33.Text = config.DimensionMpr33.ToString();
            DimensionMpr34.Text = config.DimensionMpr34.ToString();
            DimensionMpi86.Text = config.DimensionMpi86.ToString();
            txtEdmDrfLayer.Text = config.EdmDrfLayer.ToString();
            cbDimensionMpi90.Checked = config.DimensionMpi90 == 1;
            
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            var config = GetInstance();
            config.PropertyInfos= dataGridView3.DataSource as List<EdmConfig.PropertyInfo> ?? new List<EdmConfig.PropertyInfo>();
            config.DraftViewLocations = dataGridView2.DataSource as List<EdmConfig.DraftViewLocation> ?? new List<EdmConfig.DraftViewLocation>();
            config.Table = config.Table ?? new EdmConfig.TableInfo();
            config.Table.locationX = double.Parse(txtTableInfoX.Text);
            config.Table.locationY = double.Parse(txtTableInfoY.Text);
            config.Table.ColumnWidth = double.Parse(txtTableInfoColW.Text);
            config.Table.RowHeight = double.Parse(txtTableInfoRowH.Text);
            config.Table.ColumnInfos = dataGridView1.DataSource as List<EdmConfig.ColumnInfo> ?? new List<EdmConfig.ColumnInfo>();

            config.IsUseSystemConfig = cbIsUseSystemParam.Checked;
            config.PageCount = int.Parse(txtPageCount.Text);
            config.TextMpi88 = TextMpi88.Text;
            config.TextMpr44 = double.Parse(TextMpr44.Text);
            config.TextMpr46 = double.Parse(TextMpr46.Text);
            config.TextMpr45 = double.Parse(TextMpr45.Text);
            config.TextMpi89 = int.Parse(TextMpi89.Text);

            config.DimensionMpi85 = DimensionMpi85.Text;
            config.DimensionMpi3 = int.Parse(DimensionMpi3.Text);
            config.DimensionMpr32 = double.Parse(DimensionMpr32.Text);
            config.DimensionMpr33 = double.Parse(DimensionMpr33.Text);
            config.DimensionMpr34 = double.Parse(DimensionMpr34.Text);
            config.DimensionMpi86 = int.Parse(DimensionMpi86.Text);
            config.DimensionMpi90 = cbDimensionMpi90.Checked ? 1 : 0;

            config.EdmDrfLayer = int.Parse(txtEdmDrfLayer.Text);

            config.EdmTemplate = cbbEdmTemplate.SelectedItem.ToString();

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
            dataGridView3.MouseDown += DataGridView3_MouseDown;
        }

        private void DataGridView3_MouseDown(object sender, MouseEventArgs e)
        {
            var dataGridViewPSelection = dataGridView3;
            if (e.Button == MouseButtons.Right)
            {
                _cms = new ContextMenuStrip();
                _cms.Items.Add("新增属性");
                var list = dataGridViewPSelection.DataSource as List<EdmConfig.PropertyInfo> ?? new List<EdmConfig.PropertyInfo>();
                bool temp = dataGridViewPSelection.CurrentRow != null && dataGridViewPSelection.CurrentRow.Index >= 0 && dataGridViewPSelection.CurrentRow.Index < list.Count;
                if (temp)
                {
                    //_cms.Items.Add("修改加长杆");
                    _cms.Items.Add("删除属性");
                }

                _cms.ItemClicked += _cms_ItemClicked;
                //弹出操作菜单
                _cms.Show(MousePosition.X, MousePosition.Y);
            }
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

            var dataGridViewPSelection2 = dataGridView3;
            var datasource2 = dataGridViewPSelection2.DataSource as List<EdmConfig.PropertyInfo> ?? new List<EdmConfig.PropertyInfo>();
            if (e.ClickedItem.Text == "新增")
            {
                datasource.Add(new EdmConfig.DraftViewLocation { });
                dataGridViewPSelection.DataSource = datasource.ToList();
            }
            else if (e.ClickedItem.Text == "新增属性")
            {
                datasource2.Add(new EdmConfig.PropertyInfo { });
                dataGridViewPSelection2.DataSource = datasource2.ToList();
            }
            else if (e.ClickedItem.Text == "删除属性")
            {
                if (dataGridViewPSelection2.CurrentRow != null)
                {
                    var obj = dataGridViewPSelection2.CurrentRow.DataBoundItem as EdmConfig.PropertyInfo;
                    if (obj != null)
                    {
                        datasource2.Remove(obj);
                        dataGridViewPSelection2.DataSource = datasource2.ToList();
                    }
                }

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
