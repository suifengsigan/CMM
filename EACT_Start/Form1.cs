using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EACT_Start
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            txtUgPath.ReadOnly = true;
            var baseDir = System.Configuration.ConfigurationManager.AppSettings["UGII_BASE_DIR"];
            if (!string.IsNullOrEmpty(GetUgrafPath(baseDir)))
            {
                txtUgPath.Text = baseDir;
            }
            else
            {
                var ugiibaseDir = System.Environment.GetEnvironmentVariable("UGII_BASE_DIR");
                if (!string.IsNullOrEmpty(GetUgrafPath(ugiibaseDir)))
                {
                    txtUgPath.Text = baseDir;
                }
            }
            txtUgPath.TextChanged += TxtUgPath_TextChanged;
        }

        private void TxtUgPath_TextChanged(object sender, EventArgs e)
        {
            var regUGPath = GetRegUGPath(txtUgPath.Text);
            if (string.IsNullOrEmpty(txtUgPath.Text) || string.IsNullOrEmpty(regUGPath))
            {
                return;
            }
            var dirInfo = new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(regUGPath));
            System.Configuration.ConfigurationManager.AppSettings["UGII_BASE_DIR"] = dirInfo.Parent.FullName;
            System.Configuration.ConfigurationManager.AppSettings["UGII_ROOT_DIR"] = dirInfo.FullName;
        }

        private void DispMsg(string strMsg)
        {
            Action action = () => { DispMsg(strMsg, listBox1); };

            var box = listBox1;
            if (box.InvokeRequired == false)
            {
                action();
            }
            else
            {
                box.Invoke(action);
            }
        }

        private void DispMsg(string strMsg, ListBox box)
        {
            Action action = () => {
                box.Items.Insert(0, string.Format("{0}:{1}", DateTime.Now.ToString(), strMsg));
                var maxItem = 1000;
                if (box.Items.Count > maxItem)
                {
                    box.Items.RemoveAt(maxItem);
                }
            };
            if (box.InvokeRequired == false)
            {
                action();
            }
            else
            {
                box.Invoke(action);
            }
        }

        private string GetRegUGPath(string UGII_BASE_DIR)
        {
            var result = string.Empty;
            var dir = System.IO.Path.GetDirectoryName(GetUgrafPath(UGII_BASE_DIR));
            var path = System.IO.Path.Combine(dir, "SignDotNet.exe");
            if (System.IO.File.Exists(path))
            {
                return path;
            }

            path = System.IO.Path.Combine(dir, "SignLibrary.exe");
            if (System.IO.File.Exists(path))
            {
                return path;
            }
            return result;
        }

        private string GetUgrafPath(string UGII_BASE_DIR)
        {
            var result = string.Empty;
            var path = System.IO.Path.Combine(UGII_BASE_DIR, "UGII", "ugraf.exe");
            if (System.IO.File.Exists(path))
            {
                return path;
            }

            path = System.IO.Path.Combine(UGII_BASE_DIR, "NXBIN", "ugraf.exe");
            if (System.IO.File.Exists(path))
            {
                return path;
            }

            path = System.IO.Path.Combine(UGII_BASE_DIR, "ugraf.exe");
            if (System.IO.File.Exists(path))
            {
                return path;
            }
            return result;
        }

        private void btnStartUG_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUgPath.Text))
            {
                System.Windows.Forms.MessageBox.Show("路径无法识别，请重新选择！");
                return;
            }

            var startupPath = Application.StartupPath;

            Helper.ExecBatCommand(p => {
                p(string.Format("set UGII_USER_DIR={0}", startupPath));
                p(string.Format("set UGII_MOLDDM_DIR={0}", startupPath));
                p(string.Format("set UGII_EACT_DIR={0}", startupPath));
                p(string.Format("set UGII_BITMAP_PATH={0}\\Application\\EActBitmap", startupPath));
                p("set UGII_MODL_DEMO=1");
                p("set UGII_DISPLAY_DEBUG=1");
                p("set PRINT_DIALOG_BITMAP_NAMES=1");
                p(string.Format("set UPOINT_DEVELOP_DIR={0}", startupPath));
                p(string.Format("set UGII_CAM_POST_DIR={0}\\MACH\\resource\\postprocessor\\", startupPath));
                p("set UGII_LANG=simpl_chinese");
                p(string.Format("\"{0}\"", GetUgrafPath(txtUgPath.Text)));
                p("exit 0");
            },null,false);
        }

        private void btnSelUG_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.Description = "请选择UG路径";
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var path = f.SelectedPath;
                if (string.IsNullOrEmpty(GetUgrafPath(path)))
                {
                    System.Windows.Forms.MessageBox.Show("路径无法识别，请重新选择！");
                }
                else
                {
                    txtUgPath.Text = path;
                }
            }
        }

        private void btnRegUG_Click(object sender, EventArgs e)
        {
            var regUGPath = GetRegUGPath(txtUgPath.Text);
            if (string.IsNullOrEmpty(txtUgPath.Text) || string.IsNullOrEmpty(regUGPath))
            {
                System.Windows.Forms.MessageBox.Show("路径无法识别，请重新选择！");
                return;
            }
            var EACT_UG_SIGNDOTNET = regUGPath;
            var EACT_UG_USER_APPLICATION = System.IO.Path.Combine(Application.StartupPath, "Application");
            var list=Helper.ExecBatCommand(p => {
                p(string.Format("\"{0}\"  {1}\\EactConfig.dll", EACT_UG_SIGNDOTNET, EACT_UG_USER_APPLICATION));
                p(string.Format("\"{0}\"  {1}\\EactBom.dll", EACT_UG_SIGNDOTNET, EACT_UG_USER_APPLICATION));
                p(string.Format("\"{0}\"  {1}\\SetProperty.dll", EACT_UG_SIGNDOTNET, EACT_UG_USER_APPLICATION));
                p(string.Format("\"{0}\"  {1}\\EactTool.dll", EACT_UG_SIGNDOTNET, EACT_UG_USER_APPLICATION));
                p(string.Format("\"{0}\"  {1}\\CSharpProxy.dll", EACT_UG_SIGNDOTNET, EACT_UG_USER_APPLICATION));
                p(string.Format("\"{0}\"  {1}\\CMM.dll", EACT_UG_SIGNDOTNET, EACT_UG_USER_APPLICATION));
                p(string.Format("\"{0}\"  {1}\\AutoPrtTool.dll", EACT_UG_SIGNDOTNET, EACT_UG_USER_APPLICATION));
                p(string.Format("\"{0}\"  {1}\\CMMManual.dll", EACT_UG_SIGNDOTNET, EACT_UG_USER_APPLICATION));
                p(string.Format("\"{0}\"  {1}\\CMMTool.dll", EACT_UG_SIGNDOTNET, EACT_UG_USER_APPLICATION));
                p("exit 0");
            });

            list.ForEach(u => {
                DispMsg(u);
            });
        }

        private void btnGetPoint_Click(object sender, EventArgs e)
        {
            var startupPath = Application.StartupPath;
            Application.Exit();
            Helper.Send(new List<string> { "CMMProgram" });
        }
    }
}
