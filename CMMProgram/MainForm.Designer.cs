﻿namespace CMMProgram
{
    partial class s
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(s));
            this.btnStart = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnCMMConfig = new System.Windows.Forms.Button();
            this.btnUserConfig = new System.Windows.Forms.Button();
            this.btnAutoPrt = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(29, 43);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(109, 79);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "开启检测";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.listBox1);
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(814, 525);
            this.panel2.TabIndex = 8;
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 17;
            this.listBox1.Location = new System.Drawing.Point(185, 0);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(629, 525);
            this.listBox1.TabIndex = 3;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnAutoPrt);
            this.panel4.Controls.Add(this.btnCMMConfig);
            this.panel4.Controls.Add(this.btnUserConfig);
            this.panel4.Controls.Add(this.btnStart);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(185, 525);
            this.panel4.TabIndex = 2;
            // 
            // btnCMMConfig
            // 
            this.btnCMMConfig.Location = new System.Drawing.Point(29, 405);
            this.btnCMMConfig.Name = "btnCMMConfig";
            this.btnCMMConfig.Size = new System.Drawing.Size(109, 79);
            this.btnCMMConfig.TabIndex = 7;
            this.btnCMMConfig.Text = "EACT配置";
            this.btnCMMConfig.UseVisualStyleBackColor = true;
            // 
            // btnUserConfig
            // 
            this.btnUserConfig.Location = new System.Drawing.Point(29, 282);
            this.btnUserConfig.Name = "btnUserConfig";
            this.btnUserConfig.Size = new System.Drawing.Size(109, 79);
            this.btnUserConfig.TabIndex = 8;
            this.btnUserConfig.Text = "用户配置";
            this.btnUserConfig.UseVisualStyleBackColor = true;
            // 
            // btnAutoPrt
            // 
            this.btnAutoPrt.Location = new System.Drawing.Point(29, 162);
            this.btnAutoPrt.Name = "btnAutoPrt";
            this.btnAutoPrt.Size = new System.Drawing.Size(109, 79);
            this.btnAutoPrt.TabIndex = 9;
            this.btnAutoPrt.Text = "图档工具";
            this.btnAutoPrt.UseVisualStyleBackColor = true;
            // 
            // s
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 525);
            this.Controls.Add(this.panel2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "s";
            this.Text = "EAct_益模";
            this.panel2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnUserConfig;
        private System.Windows.Forms.Button btnCMMConfig;
        private System.Windows.Forms.Button btnAutoPrt;
    }
}

