namespace EACT_Start
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnSelUG = new System.Windows.Forms.Button();
            this.txtUgPath = new System.Windows.Forms.TextBox();
            this.btnRegUG = new System.Windows.Forms.Button();
            this.btnStartUG = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnGetPoint = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSelUG
            // 
            this.btnSelUG.Location = new System.Drawing.Point(631, 14);
            this.btnSelUG.Name = "btnSelUG";
            this.btnSelUG.Size = new System.Drawing.Size(75, 23);
            this.btnSelUG.TabIndex = 0;
            this.btnSelUG.Text = "选择UG";
            this.btnSelUG.UseVisualStyleBackColor = true;
            this.btnSelUG.Click += new System.EventHandler(this.btnSelUG_Click);
            // 
            // txtUgPath
            // 
            this.txtUgPath.Location = new System.Drawing.Point(28, 14);
            this.txtUgPath.Name = "txtUgPath";
            this.txtUgPath.Size = new System.Drawing.Size(563, 21);
            this.txtUgPath.TabIndex = 1;
            // 
            // btnRegUG
            // 
            this.btnRegUG.Location = new System.Drawing.Point(12, 134);
            this.btnRegUG.Name = "btnRegUG";
            this.btnRegUG.Size = new System.Drawing.Size(116, 81);
            this.btnRegUG.TabIndex = 2;
            this.btnRegUG.Text = "注册UG";
            this.btnRegUG.UseVisualStyleBackColor = true;
            this.btnRegUG.Click += new System.EventHandler(this.btnRegUG_Click);
            // 
            // btnStartUG
            // 
            this.btnStartUG.Location = new System.Drawing.Point(12, 6);
            this.btnStartUG.Name = "btnStartUG";
            this.btnStartUG.Size = new System.Drawing.Size(116, 81);
            this.btnStartUG.TabIndex = 3;
            this.btnStartUG.Text = "启动UG";
            this.btnStartUG.UseVisualStyleBackColor = true;
            this.btnStartUG.Click += new System.EventHandler(this.btnStartUG_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.txtUgPath);
            this.panel2.Controls.Add(this.btnSelUG);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(731, 57);
            this.panel2.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnGetPoint);
            this.panel1.Controls.Add(this.btnStartUG);
            this.panel1.Controls.Add(this.btnRegUG);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 57);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(147, 365);
            this.panel1.TabIndex = 6;
            // 
            // btnGetPoint
            // 
            this.btnGetPoint.Location = new System.Drawing.Point(12, 258);
            this.btnGetPoint.Name = "btnGetPoint";
            this.btnGetPoint.Size = new System.Drawing.Size(116, 81);
            this.btnGetPoint.TabIndex = 4;
            this.btnGetPoint.Text = "后台工具";
            this.btnGetPoint.UseVisualStyleBackColor = true;
            this.btnGetPoint.Click += new System.EventHandler(this.btnGetPoint_Click);
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(147, 57);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(584, 365);
            this.listBox1.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 422);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Eact_益模";
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSelUG;
        private System.Windows.Forms.TextBox txtUgPath;
        private System.Windows.Forms.Button btnRegUG;
        private System.Windows.Forms.Button btnStartUG;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button btnGetPoint;
    }
}

