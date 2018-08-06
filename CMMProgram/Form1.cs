using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CMMProgram
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitEvent();
        }


        void InitEvent()
        {
            btnStart.Click += BtnStart_Click;
            btnEnd.Click += BtnEnd_Click;
        }

        private void BtnEnd_Click(object sender, EventArgs e)
        {
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            var session = NXOpen.Session.GetSession();
            if (session != null)
            {
                System.Windows.Forms.MessageBox.Show("非空");
            }
        }
    }
}
