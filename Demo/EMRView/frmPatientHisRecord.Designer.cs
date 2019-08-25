namespace EMRView
{
    partial class frmPatientHisRecord
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
            this.tvRecord = new System.Windows.Forms.TreeView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.pnlRecord = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // tvRecord
            // 
            this.tvRecord.Dock = System.Windows.Forms.DockStyle.Left;
            this.tvRecord.Location = new System.Drawing.Point(0, 0);
            this.tvRecord.Name = "tvRecord";
            this.tvRecord.Size = new System.Drawing.Size(194, 450);
            this.tvRecord.TabIndex = 4;
            this.tvRecord.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.TvRecord_BeforeExpand);
            this.tvRecord.DoubleClick += new System.EventHandler(this.TvRecord_DoubleClick);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(194, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 450);
            this.splitter1.TabIndex = 5;
            this.splitter1.TabStop = false;
            // 
            // pnlRecord
            // 
            this.pnlRecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRecord.Location = new System.Drawing.Point(197, 0);
            this.pnlRecord.Name = "pnlRecord";
            this.pnlRecord.Size = new System.Drawing.Size(603, 450);
            this.pnlRecord.TabIndex = 6;
            // 
            // frmPatientHisRecord
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pnlRecord);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.tvRecord);
            this.MinimizeBox = false;
            this.Name = "frmPatientHisRecord";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "历次病历";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmPatientHisInchRecord_FormClosed);
            this.Shown += new System.EventHandler(this.FrmPatientHisInchRecord_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView tvRecord;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel pnlRecord;
    }
}