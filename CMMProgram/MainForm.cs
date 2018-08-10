using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace CMMProgram
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitEvent();
        }

        public void Excute(string action)
        {
            try
            {
                string actionNameStr = action;
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMMProg");
                path = Path.Combine(path, "Application");
                actionNameStr = Path.Combine(path, actionNameStr);
                ProxyObject.ExecuteMothod(actionNameStr, path);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }


        void InitEvent()
        {
            btnStart.Click += BtnStart_Click;
            btnEnd.Click += BtnEnd_Click;
            btnSelectFile.Click += BtnSelectFile_Click;
            btnConfig.Click += BtnConfig_Click;
        }

        private void BtnConfig_Click(object sender, EventArgs e)
        {
            Excute("CMMTool.dll");
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtFile.Text = openFileDialog1.FileName;
            }
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
