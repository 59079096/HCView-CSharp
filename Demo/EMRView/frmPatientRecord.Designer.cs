namespace EMRView
{
    partial class frmPatientRecord
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
            this.components = new System.ComponentModel.Container();
            this.pmRecord = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.新建ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.编辑ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.签名ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mniHisRecord = new System.Windows.Forms.ToolStripMenuItem();
            this.mniMergeRecord = new System.Windows.Forms.ToolStripMenuItem();
            this.导出XML结构ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblPatientInfo = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvRecord = new System.Windows.Forms.TreeView();
            this.tabRecord = new System.Windows.Forms.TabControl();
            this.pmpg = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.关闭ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pmRecord.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pmpg.SuspendLayout();
            this.SuspendLayout();
            // 
            // pmRecord
            // 
            this.pmRecord.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新建ToolStripMenuItem,
            this.查看ToolStripMenuItem,
            this.编辑ToolStripMenuItem,
            this.签名ToolStripMenuItem,
            this.删除ToolStripMenuItem,
            this.toolStripSeparator1,
            this.mniHisRecord,
            this.mniMergeRecord,
            this.导出XML结构ToolStripMenuItem});
            this.pmRecord.Name = "contextMenuStrip1";
            this.pmRecord.Size = new System.Drawing.Size(151, 186);
            // 
            // 新建ToolStripMenuItem
            // 
            this.新建ToolStripMenuItem.Name = "新建ToolStripMenuItem";
            this.新建ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.新建ToolStripMenuItem.Text = "新建";
            this.新建ToolStripMenuItem.Click += new System.EventHandler(this.新建ToolStripMenuItem_Click);
            // 
            // 查看ToolStripMenuItem
            // 
            this.查看ToolStripMenuItem.Name = "查看ToolStripMenuItem";
            this.查看ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.查看ToolStripMenuItem.Text = "查看";
            this.查看ToolStripMenuItem.Click += new System.EventHandler(this.查看ToolStripMenuItem_Click);
            // 
            // 编辑ToolStripMenuItem
            // 
            this.编辑ToolStripMenuItem.Name = "编辑ToolStripMenuItem";
            this.编辑ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.编辑ToolStripMenuItem.Text = "编辑";
            this.编辑ToolStripMenuItem.Click += new System.EventHandler(this.编辑ToolStripMenuItem_Click);
            // 
            // 签名ToolStripMenuItem
            // 
            this.签名ToolStripMenuItem.Name = "签名ToolStripMenuItem";
            this.签名ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.签名ToolStripMenuItem.Text = "签名";
            this.签名ToolStripMenuItem.Click += new System.EventHandler(this.签名ToolStripMenuItem_Click);
            // 
            // 删除ToolStripMenuItem
            // 
            this.删除ToolStripMenuItem.Name = "删除ToolStripMenuItem";
            this.删除ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.删除ToolStripMenuItem.Text = "删除";
            this.删除ToolStripMenuItem.Click += new System.EventHandler(this.删除ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(147, 6);
            // 
            // mniHisRecord
            // 
            this.mniHisRecord.Name = "mniHisRecord";
            this.mniHisRecord.Size = new System.Drawing.Size(150, 22);
            this.mniHisRecord.Text = "历史病历";
            this.mniHisRecord.Click += new System.EventHandler(this.MniHisRecord_Click);
            // 
            // mniMergeRecord
            // 
            this.mniMergeRecord.Name = "mniMergeRecord";
            this.mniMergeRecord.Size = new System.Drawing.Size(150, 22);
            this.mniMergeRecord.Text = "病历打印预览";
            this.mniMergeRecord.Click += new System.EventHandler(this.mniMergeRecord_Click);
            // 
            // 导出XML结构ToolStripMenuItem
            // 
            this.导出XML结构ToolStripMenuItem.Name = "导出XML结构ToolStripMenuItem";
            this.导出XML结构ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.导出XML结构ToolStripMenuItem.Text = "导出XML结构";
            this.导出XML结构ToolStripMenuItem.Click += new System.EventHandler(this.导出XML结构ToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblPatientInfo);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1079, 33);
            this.panel1.TabIndex = 1;
            // 
            // lblPatientInfo
            // 
            this.lblPatientInfo.AutoSize = true;
            this.lblPatientInfo.Location = new System.Drawing.Point(12, 9);
            this.lblPatientInfo.Name = "lblPatientInfo";
            this.lblPatientInfo.Size = new System.Drawing.Size(89, 12);
            this.lblPatientInfo.TabIndex = 0;
            this.lblPatientInfo.Text = "lblPatientInfo";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 33);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvRecord);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabRecord);
            this.splitContainer1.Size = new System.Drawing.Size(1079, 508);
            this.splitContainer1.SplitterDistance = 185;
            this.splitContainer1.TabIndex = 2;
            // 
            // tvRecord
            // 
            this.tvRecord.ContextMenuStrip = this.pmRecord;
            this.tvRecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvRecord.Location = new System.Drawing.Point(0, 0);
            this.tvRecord.Name = "tvRecord";
            this.tvRecord.Size = new System.Drawing.Size(185, 508);
            this.tvRecord.TabIndex = 0;
            this.tvRecord.DoubleClick += new System.EventHandler(this.tvRecord_DoubleClick_1);
            // 
            // tabRecord
            // 
            this.tabRecord.ContextMenuStrip = this.pmpg;
            this.tabRecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabRecord.Location = new System.Drawing.Point(0, 0);
            this.tabRecord.Name = "tabRecord";
            this.tabRecord.SelectedIndex = 0;
            this.tabRecord.Size = new System.Drawing.Size(890, 508);
            this.tabRecord.TabIndex = 0;
            // 
            // pmpg
            // 
            this.pmpg.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.关闭ToolStripMenuItem});
            this.pmpg.Name = "pmpg";
            this.pmpg.Size = new System.Drawing.Size(101, 26);
            // 
            // 关闭ToolStripMenuItem
            // 
            this.关闭ToolStripMenuItem.Name = "关闭ToolStripMenuItem";
            this.关闭ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.关闭ToolStripMenuItem.Text = "关闭";
            this.关闭ToolStripMenuItem.Click += new System.EventHandler(this.关闭ToolStripMenuItem_Click);
            // 
            // frmPatientRecord
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1079, 541);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Name = "frmPatientRecord";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "病历书写";
            this.Load += new System.EventHandler(this.frmPatientRecord_Load);
            this.pmRecord.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pmpg.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip pmRecord;
        private System.Windows.Forms.ToolStripMenuItem 新建ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 编辑ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除ToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblPatientInfo;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView tvRecord;
        private System.Windows.Forms.TabControl tabRecord;
        private System.Windows.Forms.ToolStripMenuItem 签名ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mniMergeRecord;
        private System.Windows.Forms.ToolStripMenuItem 导出XML结构ToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip pmpg;
        private System.Windows.Forms.ToolStripMenuItem 关闭ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mniHisRecord;
    }
}