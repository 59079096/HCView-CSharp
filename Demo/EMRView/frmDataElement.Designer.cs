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
            this.mniNew = new System.Windows.Forms.ToolStripMenuItem();
            this.mniEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mniDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mniDomain = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mniInsertAsDeItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsDeGroup = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsCombobox = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsDateTime = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsRadioGroup = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsCheckBox = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsFloatBarCode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.mniRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsImage = new System.Windows.Forms.ToolStripMenuItem();
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
            this.label1.Location = new System.Drawing.Point(170, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "<- 名称或简码回车检索";
            this.label1.Click += new System.EventHandler(this.Label1_Click);
            // 
            // tbxPY
            // 
            this.tbxPY.Location = new System.Drawing.Point(4, 4);
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
            this.dgvDE.ContextMenuStrip = this.pmde;
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
            this.dgvDE.SelectionChanged += new System.EventHandler(this.DgvDE_SelectionChanged);
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
            this.mniNew,
            this.mniEdit,
            this.mniDelete,
            this.toolStripSeparator3,
            this.mniDomain,
            this.toolStripSeparator2,
            this.mniInsertAsDeItem,
            this.mniInsertAsDeGroup,
            this.mniInsertAsEdit,
            this.mniInsertAsCombobox,
            this.mniInsertAsDateTime,
            this.mniInsertAsRadioGroup,
            this.mniInsertAsCheckBox,
            this.mniInsertAsImage,
            this.mniInsertAsFloatBarCode,
            this.toolStripSeparator4,
            this.mniRefresh});
            this.pmde.Name = "pmde";
            this.pmde.Size = new System.Drawing.Size(196, 352);
            this.pmde.Opening += new System.ComponentModel.CancelEventHandler(this.Pmde_Opening);
            // 
            // mniNew
            // 
            this.mniNew.Name = "mniNew";
            this.mniNew.Size = new System.Drawing.Size(195, 22);
            this.mniNew.Text = "添加";
            this.mniNew.Click += new System.EventHandler(this.MniNew_Click);
            // 
            // mniEdit
            // 
            this.mniEdit.Name = "mniEdit";
            this.mniEdit.Size = new System.Drawing.Size(195, 22);
            this.mniEdit.Text = "修改";
            this.mniEdit.Click += new System.EventHandler(this.MniEdit_Click);
            // 
            // mniDelete
            // 
            this.mniDelete.Name = "mniDelete";
            this.mniDelete.Size = new System.Drawing.Size(195, 22);
            this.mniDelete.Text = "删除";
            this.mniDelete.Click += new System.EventHandler(this.MniDelete_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(192, 6);
            // 
            // mniDomain
            // 
            this.mniDomain.Name = "mniDomain";
            this.mniDomain.Size = new System.Drawing.Size(195, 22);
            this.mniDomain.Text = "值域管理";
            this.mniDomain.Click += new System.EventHandler(this.MniDomain_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(192, 6);
            // 
            // mniInsertAsDeItem
            // 
            this.mniInsertAsDeItem.Name = "mniInsertAsDeItem";
            this.mniInsertAsDeItem.Size = new System.Drawing.Size(195, 22);
            this.mniInsertAsDeItem.Text = "插入（数据元）";
            this.mniInsertAsDeItem.Click += new System.EventHandler(this.mniInsertAsDeItem_Click);
            // 
            // mniInsertAsDeGroup
            // 
            this.mniInsertAsDeGroup.Name = "mniInsertAsDeGroup";
            this.mniInsertAsDeGroup.Size = new System.Drawing.Size(195, 22);
            this.mniInsertAsDeGroup.Text = "插入（数据组）";
            this.mniInsertAsDeGroup.Click += new System.EventHandler(this.MniInsertAsDeGroup_Click);
            // 
            // mniInsertAsEdit
            // 
            this.mniInsertAsEdit.Name = "mniInsertAsEdit";
            this.mniInsertAsEdit.Size = new System.Drawing.Size(195, 22);
            this.mniInsertAsEdit.Text = "插入（Edit）";
            this.mniInsertAsEdit.Click += new System.EventHandler(this.MniInsertAsEdit_Click);
            // 
            // mniInsertAsCombobox
            // 
            this.mniInsertAsCombobox.Name = "mniInsertAsCombobox";
            this.mniInsertAsCombobox.Size = new System.Drawing.Size(195, 22);
            this.mniInsertAsCombobox.Text = "插入（Combobox）";
            this.mniInsertAsCombobox.Click += new System.EventHandler(this.MniInsertAsCombobox_Click);
            // 
            // mniInsertAsDateTime
            // 
            this.mniInsertAsDateTime.Name = "mniInsertAsDateTime";
            this.mniInsertAsDateTime.Size = new System.Drawing.Size(195, 22);
            this.mniInsertAsDateTime.Text = "插入（DateTime）";
            this.mniInsertAsDateTime.Click += new System.EventHandler(this.MniInsertAsDateTime_Click);
            // 
            // mniInsertAsRadioGroup
            // 
            this.mniInsertAsRadioGroup.Name = "mniInsertAsRadioGroup";
            this.mniInsertAsRadioGroup.Size = new System.Drawing.Size(195, 22);
            this.mniInsertAsRadioGroup.Text = "插入（RadioGroup）";
            this.mniInsertAsRadioGroup.Click += new System.EventHandler(this.MniInsertAsRadioGroup_Click);
            // 
            // mniInsertAsCheckBox
            // 
            this.mniInsertAsCheckBox.Name = "mniInsertAsCheckBox";
            this.mniInsertAsCheckBox.Size = new System.Drawing.Size(195, 22);
            this.mniInsertAsCheckBox.Text = "插入（CheckBox）";
            this.mniInsertAsCheckBox.Click += new System.EventHandler(this.MniInsertAsCheckBox_Click);
            // 
            // mniInsertAsFloatBarCode
            // 
            this.mniInsertAsFloatBarCode.Name = "mniInsertAsFloatBarCode";
            this.mniInsertAsFloatBarCode.Size = new System.Drawing.Size(195, 22);
            this.mniInsertAsFloatBarCode.Text = "插入(浮动一维码)";
            this.mniInsertAsFloatBarCode.Click += new System.EventHandler(this.MniInsertAsFloatBarCode_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(192, 6);
            // 
            // mniRefresh
            // 
            this.mniRefresh.Name = "mniRefresh";
            this.mniRefresh.Size = new System.Drawing.Size(195, 22);
            this.mniRefresh.Text = "刷新";
            this.mniRefresh.Click += new System.EventHandler(this.MniRefresh_Click);
            // 
            // mniInsertAsImage
            // 
            this.mniInsertAsImage.Name = "mniInsertAsImage";
            this.mniInsertAsImage.Size = new System.Drawing.Size(195, 22);
            this.mniInsertAsImage.Text = "插入(Image)";
            this.mniInsertAsImage.Click += new System.EventHandler(this.mniInsertAsImage_Click);
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
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "插入数据元";
            this.Load += new System.EventHandler(this.FrmDataElement_Load);
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
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsDeItem;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsDeGroup;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsEdit;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsCombobox;
        private System.Windows.Forms.ToolStripMenuItem mniNew;
        private System.Windows.Forms.ToolStripMenuItem mniEdit;
        private System.Windows.Forms.ToolStripMenuItem mniDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mniDomain;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsDateTime;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsRadioGroup;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsCheckBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem mniRefresh;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsFloatBarCode;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsImage;
    }
}