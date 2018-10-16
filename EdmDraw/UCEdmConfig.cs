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
            Init();
        }

        public void Init()
        {
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
    }
}
