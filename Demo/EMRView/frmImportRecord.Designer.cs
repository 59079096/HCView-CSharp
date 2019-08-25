namespace EMRView
{
    partial class frmImportRecord
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
            this.pnlRecord = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnImportAll = new System.Windows.Forms.Button();
            this.btnImportSelect = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlRecord
            // 
            this.pnlRecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRecord.Location = new System.Drawing.Point(0, 36);
            this.pnlRecord.Name = "pnlRecord";
            this.pnlRecord.Size = new System.Drawing.Size(800, 414);
            this.pnlRecord.TabIndex = 9;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnImportAll);
            this.panel1.Controls.Add(this.btnImportSelect);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 36);
            this.panel1.TabIndex = 8;
            // 
            // btnImportAll
            // 
            this.btnImportAll.Location = new System.Drawing.Point(18, 7);
            this.btnImportAll.Name = "btnImportAll";
            this.btnImportAll.Size = new System.Drawing.Size(75, 23);
            this.btnImportAll.TabIndex = 1;
            this.btnImportAll.Text = "导入全部";
            this.btnImportAll.UseVisualStyleBackColor = true;
            this.btnImportAll.Click += new System.EventHandler(this.BtnImportAll_Click);
            // 
            // btnImportSelect
            // 
            this.btnImportSelect.Location = new System.Drawing.Point(112, 7);
            this.btnImportSelect.Name = "btnImportSelect";
            this.btnImportSelect.Size = new System.Drawing.Size(97, 23);
            this.btnImportSelect.TabIndex = 0;
            this.btnImportSelect.Text = "导入选中内容";
            this.btnImportSelect.UseVisualStyleBackColor = true;
            this.btnImportSelect.Click += new System.EventHandler(this.BtnImportSelect_Click);
            // 
            // frmImportRecord
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pnlRecord);
            this.Controls.Add(this.panel1);
            this.Name = "frmImportRecord";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "frmImportRecord";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlRecord;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnImportAll;
        private System.Windows.Forms.Button btnImportSelect;
    }
}