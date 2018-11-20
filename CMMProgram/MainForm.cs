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
        private IntPtr _windowPtr = IntPtr.Zero;
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
                    var msgInfo=CSharpProxy.EactMsgInfo.DeserializeObject(msg);
                    if (msgInfo.Type == 0)
                    {
                        DispMsg(msgInfo.Msg);
                    }
                    else
                    {
                        DispMsg(msgInfo.Msg, listBox2);
                    }
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
            DispMsg(strMsg, listBox1);
        }

        private void DispMsg(string strMsg,ListBox box)
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

        public object Excute(string action,string methodName="Main")
        {
            string actionNameStr = action;
            var programPath = System.Configuration.ConfigurationManager.AppSettings.Get("ProgramPath");
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Application");
            if (Directory.Exists(programPath))
            {
                DirectoryInfo info = new DirectoryInfo(programPath);
                path = info.FullName;
            }
            else
            {
                if (!Directory.Exists(path))
                {
                    path = Path.Combine(path, "CMMProg");
                }
            }
            actionNameStr = Path.Combine(path, actionNameStr);
            if (System.Configuration.ConfigurationManager.AppSettings.Get("IsAppDomain") == "0")
            {
                return ExecuteMothod(actionNameStr, path, _windowPtr, methodName);
            }
            else
            {
                return CSharpProxy.ProxyObject.ExecuteMothod(actionNameStr, path, _windowPtr, methodName);
            }
        }

        AppDomain _appDomain;
        EactThread _eactThread = new EactThread();
        public object ExecuteMothod(string actionName, string baseDirectory, IntPtr hWnd, string methodName = "Main")
        {
            object result = null;
            if (_appDomain == null)
            {
                _appDomain = (AppDomain)_eactThread.Run(() => {
                    var setup = new AppDomainSetup();
                    setup.ApplicationBase = baseDirectory;
                    var appDomain = AppDomain.CreateDomain(actionName, null, setup);
                    return appDomain;
                });
            }
            result = _eactThread.Run(() => {
                return CSharpProxy.ProxyObject.ExecuteMothod(_appDomain, actionName, baseDirectory, hWnd, methodName);
            });
            return result;
        }


        public object ExecuteMothodEx(string actionName, string baseDirectory, IntPtr hWnd, string methodName = "Main")
        {
            object result = null;
            var setup = new AppDomainSetup();
            setup.ApplicationBase = baseDirectory;
            AppDomain _appDomain = AppDomain.CreateDomain(actionName, null, setup);
            try
            {
                var args = new string[] { actionName, methodName };
                var location = typeof(CSharpProxy.ProxyObject).Assembly.Location;
                CSharpProxy.ProxyObject po = (CSharpProxy.ProxyObject)_appDomain.CreateInstanceFromAndUnwrap(location, typeof(CSharpProxy.ProxyObject).FullName);
                po.WindowPH = hWnd;
                result=_appDomain.ExecuteAssembly(Path.Combine(baseDirectory, "CSharpEntry.exe"), args);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                AppDomain.Unload(_appDomain);
            }
            return result;
        }



        void InitEvent()
        {
            btnStart.Click += BtnStart_Click;
            //btnCMMConfig.Click += BtnConfig_Click;
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
            _windowPtr = this.Handle;
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
                int str = (int)Excute("CMM.dll", "Verification");
                if (str == 2)
                {
                    invokeAction(() => {
                        System.Windows.Forms.MessageBox.Show("加密锁验证失败");
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

                    Thread.Sleep(2000);
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

                        Thread.Sleep(2000);
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
