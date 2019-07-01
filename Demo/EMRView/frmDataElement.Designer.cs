namespace EMRView
{
    partial class frmDataElement
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tbxPY = new System.Windows.Forms.TextBox();
            this.dgvDE = new System.Windows.Forms.DataGridView();
            this.Key = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pmde = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mniInsertAsDE = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsDG = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsCombobox = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDE)).BeginInit();
            this.pmde.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.tbxPY);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(358, 30);
            this.panel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(173, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(179, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "<- 输入名称或简拼回车开始检索";
            // 
            // tbxPY
            // 
            this.tbxPY.Location = new System.Drawing.Point(7, 4);
            this.tbxPY.Name = "tbxPY";
            this.tbxPY.Size = new System.Drawing.Size(160, 21);
            this.tbxPY.TabIndex = 0;
            this.tbxPY.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbxPY_KeyDown);
            // 
            // dgvDE
            // 
            this.dgvDE.AllowUserToAddRows = false;
            this.dgvDE.AllowUserToDeleteRows = false;
            this.dgvDE.AllowUserToResizeRows = false;
            this.dgvDE.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvDE.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDE.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Key,
            this.value,
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4});
            this.dgvDE.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDE.Location = new System.Drawing.Point(0, 30);
            this.dgvDE.MultiSelect = false;
            this.dgvDE.Name = "dgvDE";
            this.dgvDE.ReadOnly = true;
            this.dgvDE.RowHeadersVisible = false;
            this.dgvDE.RowTemplate.Height = 23;
            this.dgvDE.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDE.Size = new System.Drawing.Size(358, 286);
            this.dgvDE.TabIndex = 1;
            this.dgvDE.DoubleClick += new System.EventHandler(this.dgvDE_DoubleClick);
            // 
            // Key
            // 
            this.Key.HeaderText = "序";
            this.Key.Name = "Key";
            this.Key.ReadOnly = true;
            this.Key.Width = 30;
            // 
            // value
            // 
            this.value.HeaderText = "名称";
            this.value.Name = "value";
            this.value.ReadOnly = true;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "编码";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 60;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "拼音";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 40;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "类型";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 60;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "值域";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 60;
            // 
            // pmde
            // 
            this.pmde.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.mniInsertAsDE,
            this.mniInsertAsDG,
            this.mniInsertAsEdit,
            this.mniInsertAsCombobox});
            this.pmde.Name = "pmde";
            this.pmde.Size = new System.Drawing.Size(190, 98);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(186, 6);
            // 
            // mniInsertAsDE
            // 
            this.mniInsertAsDE.Name = "mniInsertAsDE";
            this.mniInsertAsDE.Size = new System.Drawing.Size(189, 22);
            this.mniInsertAsDE.Text = "插入（数据元）";
            this.mniInsertAsDE.Click += new System.EventHandler(this.mniInsertAsDE_Click);
            // 
            // mniInsertAsDG
            // 
            this.mniInsertAsDG.Name = "mniInsertAsDG";
            this.mniInsertAsDG.Size = new System.Drawing.Size(189, 22);
            this.mniInsertAsDG.Text = "插入（数据组）";
            // 
            // mniInsertAsEdit
            // 
            this.mniInsertAsEdit.Name = "mniInsertAsEdit";
            this.mniInsertAsEdit.Size = new System.Drawing.Size(189, 22);
            this.mniInsertAsEdit.Text = "插入（Edit）";
            // 
            // mniInsertAsCombobox
            // 
            this.mniInsertAsCombobox.Name = "mniInsertAsCombobox";
            this.mniInsertAsCombobox.Size = new System.Drawing.Size(189, 22);
            this.mniInsertAsCombobox.Text = "插入（Combobox）";
            // 
            // frmDataElement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 316);
            this.Controls.Add(this.dgvDE);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDataElement";
            this.Text = "插入数据元";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDE)).EndInit();
            this.pmde.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbxPY;
        private System.Windows.Forms.DataGridView dgvDE;
        private System.Windows.Forms.DataGridViewTextBoxColumn Key;
        private System.Windows.Forms.DataGridViewTextBoxColumn value;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.ContextMenuStrip pmde;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsDE;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsDG;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsEdit;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsCombobox;
    }
}