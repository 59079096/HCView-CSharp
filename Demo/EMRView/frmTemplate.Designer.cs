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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
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
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mniViewItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mniDomain = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mniInsertAsDE = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsDG = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mniInsertAsCombobox = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mniRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tbxPY = new System.Windows.Forms.TextBox();
            this.dgvCV = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pmCV = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniNewItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mniEditItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mniDeleteItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mniEditItemLink = new System.Windows.Forms.ToolStripMenuItem();
            this.mniDeleteItemLink = new System.Windows.Forms.ToolStripMenuItem();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblDE = new System.Windows.Forms.Label();
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
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDE)).BeginInit();
            this.pmde.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCV)).BeginInit();
            this.pmCV.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pmTemplate.SuspendLayout();
            this.pmpg.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitContainer2.Location = new System.Drawing.Point(716, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dgvDE);
            this.splitContainer2.Panel1.Controls.Add(this.panel2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dgvCV);
            this.splitContainer2.Panel2.Controls.Add(this.panel3);
            this.splitContainer2.Size = new System.Drawing.Size(299, 598);
            this.splitContainer2.SplitterDistance = 346;
            this.splitContainer2.TabIndex = 4;
            // 
            // dgvDE
            // 
            this.dgvDE.AllowUserToAddRows = false;
            this.dgvDE.AllowUserToDeleteRows = false;
            this.dgvDE.AllowUserToResizeRows = false;
            this.dgvDE.BackgroundColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDE.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDE.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDE.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Key,
            this.value,
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4});
            this.dgvDE.ContextMenuStrip = this.pmde;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDE.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDE.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDE.Location = new System.Drawing.Point(0, 30);
            this.dgvDE.MultiSelect = false;
            this.dgvDE.Name = "dgvDE";
            this.dgvDE.ReadOnly = true;
            this.dgvDE.RowHeadersVisible = false;
            this.dgvDE.RowTemplate.Height = 23;
            this.dgvDE.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDE.Size = new System.Drawing.Size(299, 316);
            this.dgvDE.TabIndex = 5;
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
            this.Column3.Width = 30;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "值域";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 20;
            // 
            // pmde
            // 
            this.pmde.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniNew,
            this.mniEdit,
            this.mniDelete,
            this.toolStripMenuItem1,
            this.mniViewItem,
            this.mniDomain,
            this.toolStripSeparator1,
            this.mniInsertAsDE,
            this.mniInsertAsDG,
            this.mniInsertAsEdit,
            this.mniInsertAsCombobox,
            this.toolStripSeparator2,
            this.mniRefresh});
            this.pmde.Name = "pmde";
            this.pmde.Size = new System.Drawing.Size(190, 242);
            // 
            // mniNew
            // 
            this.mniNew.Name = "mniNew";
            this.mniNew.Size = new System.Drawing.Size(189, 22);
            this.mniNew.Text = "添加";
            this.mniNew.Click += new System.EventHandler(this.mniNew_Click);
            // 
            // mniEdit
            // 
            this.mniEdit.Name = "mniEdit";
            this.mniEdit.Size = new System.Drawing.Size(189, 22);
            this.mniEdit.Text = "修改";
            this.mniEdit.Click += new System.EventHandler(this.mniEdit_Click);
            // 
            // mniDelete
            // 
            this.mniDelete.Name = "mniDelete";
            this.mniDelete.Size = new System.Drawing.Size(189, 22);
            this.mniDelete.Text = "删除";
            this.mniDelete.Click += new System.EventHandler(this.mniDelete_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(186, 6);
            // 
            // mniViewItem
            // 
            this.mniViewItem.Name = "mniViewItem";
            this.mniViewItem.Size = new System.Drawing.Size(189, 22);
            this.mniViewItem.Text = "查看值域选项";
            this.mniViewItem.Click += new System.EventHandler(this.mniViewItem_Click);
            // 
            // mniDomain
            // 
            this.mniDomain.Name = "mniDomain";
            this.mniDomain.Size = new System.Drawing.Size(189, 22);
            this.mniDomain.Text = "值域管理";
            this.mniDomain.Click += new System.EventHandler(this.mniDomain_Click);
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
            this.mniInsertAsDG.Click += new System.EventHandler(this.mniInsertAsDG_Click);
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
            this.mniInsertAsCombobox.Click += new System.EventHandler(this.mniInsertAsCombobox_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(186, 6);
            // 
            // mniRefresh
            // 
            this.mniRefresh.Name = "mniRefresh";
            this.mniRefresh.Size = new System.Drawing.Size(189, 22);
            this.mniRefresh.Text = "刷新";
            this.mniRefresh.Click += new System.EventHandler(this.mniRefresh_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.tbxPY);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(299, 30);
            this.panel2.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label1.Location = new System.Drawing.Point(139, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "<- 输入名称或简拼回车检索";
            this.toolTip1.SetToolTip(this.label1, "单击刷新数据元");
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // tbxPY
            // 
            this.tbxPY.Location = new System.Drawing.Point(7, 4);
            this.tbxPY.Name = "tbxPY";
            this.tbxPY.Size = new System.Drawing.Size(123, 21);
            this.tbxPY.TabIndex = 0;
            this.tbxPY.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbxPY_KeyDown);
            // 
            // dgvCV
            // 
            this.dgvCV.AllowUserToAddRows = false;
            this.dgvCV.AllowUserToDeleteRows = false;
            this.dgvCV.AllowUserToResizeRows = false;
            this.dgvCV.BackgroundColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCV.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvCV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5});
            this.dgvCV.ContextMenuStrip = this.pmCV;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvCV.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvCV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCV.Location = new System.Drawing.Point(0, 30);
            this.dgvCV.MultiSelect = false;
            this.dgvCV.Name = "dgvCV";
            this.dgvCV.ReadOnly = true;
            this.dgvCV.RowHeadersVisible = false;
            this.dgvCV.RowTemplate.Height = 23;
            this.dgvCV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCV.Size = new System.Drawing.Size(299, 218);
            this.dgvCV.TabIndex = 6;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "值";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "编码";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Width = 40;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "拼音";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Width = 60;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "id";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.Width = 40;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "扩展";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            this.dataGridViewTextBoxColumn5.Width = 30;
            // 
            // pmCV
            // 
            this.pmCV.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniNewItem,
            this.mniEditItem,
            this.mniDeleteItem,
            this.toolStripSeparator3,
            this.mniEditItemLink,
            this.mniDeleteItemLink});
            this.pmCV.Name = "pmCV";
            this.pmCV.Size = new System.Drawing.Size(149, 120);
            // 
            // mniNewItem
            // 
            this.mniNewItem.Name = "mniNewItem";
            this.mniNewItem.Size = new System.Drawing.Size(148, 22);
            this.mniNewItem.Text = "添加";
            this.mniNewItem.Click += new System.EventHandler(this.mniNewItem_Click);
            // 
            // mniEditItem
            // 
            this.mniEditItem.Name = "mniEditItem";
            this.mniEditItem.Size = new System.Drawing.Size(148, 22);
            this.mniEditItem.Text = "修改";
            this.mniEditItem.Click += new System.EventHandler(this.mniEditItem_Click);
            // 
            // mniDeleteItem
            // 
            this.mniDeleteItem.Name = "mniDeleteItem";
            this.mniDeleteItem.Size = new System.Drawing.Size(148, 22);
            this.mniDeleteItem.Text = "删除";
            this.mniDeleteItem.Click += new System.EventHandler(this.mniDeleteItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(145, 6);
            // 
            // mniEditItemLink
            // 
            this.mniEditItemLink.Name = "mniEditItemLink";
            this.mniEditItemLink.Size = new System.Drawing.Size(148, 22);
            this.mniEditItemLink.Text = "编辑扩展内容";
            this.mniEditItemLink.Click += new System.EventHandler(this.mniEditItemLink_Click);
            // 
            // mniDeleteItemLink
            // 
            this.mniDeleteItemLink.Name = "mniDeleteItemLink";
            this.mniDeleteItemLink.Size = new System.Drawing.Size(148, 22);
            this.mniDeleteItemLink.Text = "删除扩展内容";
            this.mniDeleteItemLink.Click += new System.EventHandler(this.mniDeleteItemLink_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.lblDE);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(299, 30);
            this.panel3.TabIndex = 0;
            // 
            // lblDE
            // 
            this.lblDE.AutoSize = true;
            this.lblDE.Location = new System.Drawing.Point(7, 8);
            this.lblDE.Name = "lblDE";
            this.lblDE.Size = new System.Drawing.Size(149, 12);
            this.lblDE.TabIndex = 0;
            this.lblDE.Text = "数据元值域选项(点击刷新)";
            this.lblDE.Click += new System.EventHandler(this.lblDE_Click);
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
            this.Controls.Add(this.splitContainer2);
            this.Name = "frmTemplate";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "模板制作";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frm_Template_Load);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDE)).EndInit();
            this.pmde.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCV)).EndInit();
            this.pmCV.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
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

        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dgvDE;
        private System.Windows.Forms.DataGridViewTextBoxColumn Key;
        private System.Windows.Forms.DataGridViewTextBoxColumn value;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbxPY;
        private System.Windows.Forms.DataGridView dgvCV;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lblDE;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabTemplate;
        private System.Windows.Forms.TreeView tvTemplate;
        private System.Windows.Forms.ContextMenuStrip pmde;
        private System.Windows.Forms.ToolStripMenuItem mniNew;
        private System.Windows.Forms.ToolStripMenuItem mniEdit;
        private System.Windows.Forms.ToolStripMenuItem mniDelete;
        private System.Windows.Forms.ToolStripMenuItem mniViewItem;
        private System.Windows.Forms.ToolStripMenuItem mniDomain;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsDE;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsDG;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsEdit;
        private System.Windows.Forms.ToolStripMenuItem mniInsertAsCombobox;
        private System.Windows.Forms.ToolStripMenuItem mniRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.ContextMenuStrip pmCV;
        private System.Windows.Forms.ToolStripMenuItem mniNewItem;
        private System.Windows.Forms.ToolStripMenuItem mniEditItem;
        private System.Windows.Forms.ToolStripMenuItem mniDeleteItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mniEditItemLink;
        private System.Windows.Forms.ToolStripMenuItem mniDeleteItemLink;
        private System.Windows.Forms.ContextMenuStrip pmTemplate;
        private System.Windows.Forms.ToolStripMenuItem mniNewTemplate;
        private System.Windows.Forms.ToolStripMenuItem mniDeleteTemplate;
        private System.Windows.Forms.ToolStripMenuItem mniInsertTemplate;
        private System.Windows.Forms.ToolStripMenuItem mniTemplateProperty;
        private System.Windows.Forms.ContextMenuStrip pmpg;
        private System.Windows.Forms.ToolStripMenuItem 关闭ToolStripMenuItem;

    }
}