using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CMMProgram
{
    public partial class s : Form
    {
        /// <summary>
        /// Override the DefWndProc function, in order to receive the message through it.
        /// </summary>
        /// <param name="m">message</param>
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                // Here, we use WM_COPYDATA message to receive the COPYDATASTRUCT
                case CSharpProxy.ProxyObject.WM_DATA_TRANSFER:
                    CSharpProxy.ProxyObject.COPYDATASTRUCT cds = new CSharpProxy.ProxyObject.COPYDATASTRUCT();
                    cds = (CSharpProxy.ProxyObject.COPYDATASTRUCT)m.GetLParam(typeof(CSharpProxy.ProxyObject.COPYDATASTRUCT));
                    byte[] bytData = new byte[cds.cbData];
                    Marshal.Copy(cds.lpData, bytData, 0, bytData.Length);
                    var msg = Encoding.Default.GetString(bytData);
                    DispMsg(msg);
                    break;

                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }
        public s()
        {
            InitializeComponent();
            Enabled = false;
            InitEvent();
        }

        private void DispMsg(string strMsg)
        {
            Action action = () => {
                listBox1.Items.Insert(0, string.Format("{0}:{1}", DateTime.Now.ToString(), strMsg));
                var maxItem = 1000;
                if (listBox1.Items.Count > maxItem)
                {
                    listBox1.Items.RemoveAt(maxItem);
                }
            };
            if (this.listBox1.InvokeRequired == false)
            {
                action();
            }
            else
            {
                this.listBox1.Invoke(action);
            }
        }

        public object Excute(string action,string methodName="Main")
        {
            string actionNameStr = action;
            var programPath = System.Configuration.ConfigurationManager.AppSettings.Get("ProgramPath");
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMMProg");
            path = Path.Combine(path, "Application");
            if (Directory.Exists(programPath))
            {
                DirectoryInfo info = new DirectoryInfo(programPath);
                path = info.FullName;
            }
            actionNameStr = Path.Combine(path, actionNameStr);
            return CSharpProxy.ProxyObject.ExecuteMothod(actionNameStr, path, methodName);
        }


        void InitEvent()
        {
            btnStart.Click += BtnStart_Click;
            btnCMMConfig.Click += BtnConfig_Click;
            btnUserConfig.Click += BtnUserConfig_Click;
            btnAutoPrt.Click += BtnAutoPrt_Click;
            this.Shown += S_Shown;
            notifyIcon1.MouseDoubleClick += NotifyIcon1_MouseDoubleClick;
        }
    
        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = !this.Visible;
            if (this.Visible)
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.Show();
                    this.WindowState = FormWindowState.Maximized;
                    this.Activate();
                }
                this.BringToFront();
            }
        }

        private void S_Shown(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((o) => {
                Action<Action> invokeAction = (action) => {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(action);
                    }
                    else
                    {
                        action();
                    }
                };
                string[] str = (string[])Excute("CMM.dll", "Verification");
                if (str[0] == "2")
                {
                    invokeAction(() => {
                        System.Windows.Forms.MessageBox.Show(str[1]);
                        Application.Exit();
                    });

                }
                else
                {
                    Excute("CMM.dll", "InitUG");
                    Action action = () => {
                        this.Enabled = true;
                    };
                    invokeAction(action);
                }

            }));

        }

        private void BtnAutoPrt_Click(object sender, EventArgs e)
        {
            panel4.Visible = false;
            Text = "Eact_图档工具";
            ThreadPool.QueueUserWorkItem(new WaitCallback((o) => {
                while (true)
                {
                    try
                    {
                        Excute("AutoPrtTool.dll");
                    }
                    catch (Exception ex)
                    {
                        DispMsg(string.Format("启动AutoPrt程序错误【{0}】", ex.Message));
                        Console.WriteLine(ex.Message);
                    }

                    Thread.Sleep(1000);
                }

            }));
        }

        private void BtnUserConfig_Click(object sender, EventArgs e)
        {
            Excute("CMMTool.dll", "ShowEactConfig");
        }

        private void BtnConfig_Click(object sender, EventArgs e)
        {
            Excute("CMMTool.dll");
        }
       
        private void BtnStart_Click(object sender, EventArgs e)
        {
            panel4.Visible = false;
            Text = "Eact_自动取点工具";
            this.notifyIcon1.Icon = this.Icon;
            this.notifyIcon1.Text = this.Text;
            this.Visible = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback((o) => {
                try
                {
                    Excute("CMM.dll", "CMMInit");

                    while (true)
                    {
                        try
                        {
                            Excute("CMM.dll");
                        }
                        catch (Exception ex)
                        {
                            DispMsg(string.Format("CMM程序错误【{0}】", ex.Message));
                            Console.WriteLine(ex.Message);
                        }

                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    DispMsg(string.Format("启动CMM程序错误【{0}】", ex.Message));
                    Console.WriteLine(ex.Message);
                } 
            }));
        }
    }
}
