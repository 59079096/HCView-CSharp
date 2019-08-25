namespace EMRView
{
    partial class frmTemplate
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
            this.splitContainerDataElement = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvTemplate = new System.Windows.Forms.TreeView();
            this.pmTemplate = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniNewTemplate = new System.Windows.Forms.ToolStripMenuItem();
            this.mniDeleteTemplate = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertTemplate = new System.Windows.Forms.ToolStripMenuItem();
            this.mniTemplateProperty = new System.Windows.Forms.ToolStripMenuItem();
            this.tabTemplate = new System.Windows.Forms.TabControl();
            this.pmpg = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.关闭ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDataElement)).BeginInit();
            this.splitContainerDataElement.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pmTemplate.SuspendLayout();
            this.pmpg.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerDataElement
            // 
            this.splitContainerDataElement.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitContainerDataElement.Location = new System.Drawing.Point(716, 0);
            this.splitContainerDataElement.Name = "splitContainerDataElement";
            this.splitContainerDataElement.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainerDataElement.Size = new System.Drawing.Size(299, 598);
            this.splitContainerDataElement.SplitterDistance = 346;
            this.splitContainerDataElement.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.splitContainer1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(716, 598);
            this.panel1.TabIndex = 5;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvTemplate);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabTemplate);
            this.splitContainer1.Size = new System.Drawing.Size(716, 598);
            this.splitContainer1.SplitterDistance = 180;
            this.splitContainer1.TabIndex = 1;
            // 
            // tvTemplate
            // 
            this.tvTemplate.ContextMenuStrip = this.pmTemplate;
            this.tvTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvTemplate.Location = new System.Drawing.Point(0, 0);
            this.tvTemplate.Name = "tvTemplate";
            this.tvTemplate.Size = new System.Drawing.Size(180, 598);
            this.tvTemplate.TabIndex = 0;
            this.tvTemplate.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvTemplate_BeforeExpand);
            this.tvTemplate.DoubleClick += new System.EventHandler(this.tvTemplate_DoubleClick);
            // 
            // pmTemplate
            // 
            this.pmTemplate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniNewTemplate,
            this.mniDeleteTemplate,
            this.mniInsertTemplate,
            this.mniTemplateProperty});
            this.pmTemplate.Name = "pmTemplate";
            this.pmTemplate.Size = new System.Drawing.Size(101, 92);
            this.pmTemplate.Opening += new System.ComponentModel.CancelEventHandler(this.pmTemplate_Opening);
            // 
            // mniNewTemplate
            // 
            this.mniNewTemplate.Name = "mniNewTemplate";
            this.mniNewTemplate.Size = new System.Drawing.Size(100, 22);
            this.mniNewTemplate.Text = "新建";
            this.mniNewTemplate.Click += new System.EventHandler(this.mniNewTemplate_Click);
            // 
            // mniDeleteTemplate
            // 
            this.mniDeleteTemplate.Name = "mniDeleteTemplate";
            this.mniDeleteTemplate.Size = new System.Drawing.Size(100, 22);
            this.mniDeleteTemplate.Text = "删除";
            this.mniDeleteTemplate.Click += new System.EventHandler(this.mniDeleteTemplate_Click);
            // 
            // mniInsertTemplate
            // 
            this.mniInsertTemplate.Name = "mniInsertTemplate";
            this.mniInsertTemplate.Size = new System.Drawing.Size(100, 22);
            this.mniInsertTemplate.Text = "插入";
            this.mniInsertTemplate.Click += new System.EventHandler(this.mniInsertTemplate_Click);
            // 
            // mniTemplateProperty
            // 
            this.mniTemplateProperty.Name = "mniTemplateProperty";
            this.mniTemplateProperty.Size = new System.Drawing.Size(100, 22);
            this.mniTemplateProperty.Text = "属性";
            // 
            // tabTemplate
            // 
            this.tabTemplate.ContextMenuStrip = this.pmpg;
            this.tabTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabTemplate.Location = new System.Drawing.Point(0, 0);
            this.tabTemplate.Name = "tabTemplate";
            this.tabTemplate.SelectedIndex = 0;
            this.tabTemplate.Size = new System.Drawing.Size(532, 598);
            this.tabTemplate.TabIndex = 0;
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
            // frmTemplate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1015, 598);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitContainerDataElement);
            this.Name = "frmTemplate";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "模板制作";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frm_Template_Load);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDataElement)).EndInit();
            this.splitContainerDataElement.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pmTemplate.ResumeLayout(false);
            this.pmpg.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerDataElement;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabTemplate;
        private System.Windows.Forms.TreeView tvTemplate;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip pmTemplate;
        private System.Windows.Forms.ToolStripMenuItem mniNewTemplate;
        private System.Windows.Forms.ToolStripMenuItem mniDeleteTemplate;
        private System.Windows.Forms.ToolStripMenuItem mniInsertTemplate;
        private System.Windows.Forms.ToolStripMenuItem mniTemplateProperty;
        private System.Windows.Forms.ContextMenuStrip pmpg;
        private System.Windows.Forms.ToolStripMenuItem 关闭ToolStripMenuItem;
    }
}