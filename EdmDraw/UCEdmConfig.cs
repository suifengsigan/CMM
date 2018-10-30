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

            dataGridView2.DataSource = config.DraftViewLocations;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            var config = GetInstance();
            config.DraftViewLocations = dataGridView2.DataSource as List<EdmConfig.DraftViewLocation> ?? new List<EdmConfig.DraftViewLocation>();
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
                return Newtonsoft.Json.JsonConvert.DeserializeObject<EdmConfig>(json) ?? new EdmConfig();
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
            if (e.ClickedItem.Text == "新增")
            {
                datasource.Add(new EdmConfig.DraftViewLocation { });
                dataGridViewPSelection.DataSource = datasource.ToList();
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
