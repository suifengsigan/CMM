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
            InitEvent();
        }

        private void DispMsg(string strMsg)
        {
            Action action = () => {
                listBox1.Items.Insert(0, string.Format("信息:{0} - [ {1} ]", strMsg, DateTime.Now.ToString()));
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

        public void Excute(string action)
        {
            try
            {
                string actionNameStr = action;
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMMProg");
                path = Path.Combine(path, "Application");
                actionNameStr = Path.Combine(path, actionNameStr);
                CSharpProxy.ProxyObject.ExecuteMothod(actionNameStr, path);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }


        void InitEvent()
        {
            btnStart.Click += BtnStart_Click;
            btnCMMConfig.Click += BtnConfig_Click;
            btnUserConfig.Click += BtnUserConfig_Click;
        }

        private void BtnUserConfig_Click(object sender, EventArgs e)
        {
            Excute("EactConfig.dll");
        }

        private void BtnConfig_Click(object sender, EventArgs e)
        {
            Excute("CMMTool.dll");
        }
        

        private void BtnEnd_Click(object sender, EventArgs e)
        {
            
        }

       
        private void BtnStart_Click(object sender, EventArgs e)
        {
            Excute("CMMUI.dll");
        }
    }
}
