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
    public partial class MainForm : Form
    {
        public MainForm()
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
            CMM.Entry.Test();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
           
        }
    }
}
