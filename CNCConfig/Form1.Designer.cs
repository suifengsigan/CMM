namespace CNCConfig
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.camConfigUserControl1 = new CNCConfig.CAMConfigUserControl();
            this.SuspendLayout();
            // 
            // camConfigUserControl1
            // 
            this.camConfigUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.camConfigUserControl1.Location = new System.Drawing.Point(0, 0);
            this.camConfigUserControl1.Name = "camConfigUserControl1";
            this.camConfigUserControl1.Size = new System.Drawing.Size(960, 499);
            this.camConfigUserControl1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 499);
            this.Controls.Add(this.camConfigUserControl1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EACT_自动编程配置工具";
            this.ResumeLayout(false);

        }

        #endregion

        private CAMConfigUserControl camConfigUserControl1;
    }
}