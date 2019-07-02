namespace EMRView
{
    partial class frmItemContent
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmItemContent));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tbxPY = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dgvDE = new System.Windows.Forms.DataGridView();
            this.Key = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlEdit = new System.Windows.Forms.Panel();
            this.tlbTool = new System.Windows.Forms.ToolStrip();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnUndo = new System.Windows.Forms.ToolStripButton();
            this.btnRedo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.cbbFont = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cbbFontSize = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.btnBold = new System.Windows.Forms.ToolStripButton();
            this.btnItalic = new System.Windows.Forms.ToolStripButton();
            this.btnUnderLine = new System.Windows.Forms.ToolStripButton();
            this.btnStrikeOut = new System.Windows.Forms.ToolStripButton();
            this.btnSuperScript = new System.Windows.Forms.ToolStripButton();
            this.btnSubScript = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRightIndent = new System.Windows.Forms.ToolStripButton();
            this.btnLeftIndent = new System.Windows.Forms.ToolStripButton();
            this.btnAlignLeft = new System.Windows.Forms.ToolStripButton();
            this.btnAlignCenter = new System.Windows.Forms.ToolStripButton();
            this.btnAlignRight = new System.Windows.Forms.ToolStripButton();
            this.btnAlignJustify = new System.Windows.Forms.ToolStripButton();
            this.btnAlignScatter = new System.Windows.Forms.ToolStripButton();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDE)).BeginInit();
            this.tlbTool.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.tbxPY);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(925, 36);
            this.panel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label1.Location = new System.Drawing.Point(146, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "<- 输入名称或简拼回车检索";
            // 
            // tbxPY
            // 
            this.tbxPY.Location = new System.Drawing.Point(14, 8);
            this.tbxPY.Name = "tbxPY";
            this.tbxPY.Size = new System.Drawing.Size(123, 21);
            this.tbxPY.TabIndex = 2;
            this.tbxPY.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbxPY_KeyDown);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 36);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dgvDE);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pnlEdit);
            this.splitContainer1.Panel2.Controls.Add(this.tlbTool);
            this.splitContainer1.Size = new System.Drawing.Size(925, 397);
            this.splitContainer1.SplitterDistance = 311;
            this.splitContainer1.TabIndex = 1;
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
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDE.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDE.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDE.Location = new System.Drawing.Point(0, 0);
            this.dgvDE.MultiSelect = false;
            this.dgvDE.Name = "dgvDE";
            this.dgvDE.ReadOnly = true;
            this.dgvDE.RowHeadersVisible = false;
            this.dgvDE.RowTemplate.Height = 23;
            this.dgvDE.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDE.Size = new System.Drawing.Size(311, 397);
            this.dgvDE.TabIndex = 6;
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
            // pnlEdit
            // 
            this.pnlEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEdit.Location = new System.Drawing.Point(0, 25);
            this.pnlEdit.Name = "pnlEdit";
            this.pnlEdit.Size = new System.Drawing.Size(610, 372);
            this.pnlEdit.TabIndex = 6;
            // 
            // tlbTool
            // 
            this.tlbTool.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSave,
            this.toolStripSeparator1,
            this.btnUndo,
            this.btnRedo,
            this.toolStripSeparator3,
            this.cbbFont,
            this.toolStripSeparator2,
            this.cbbFontSize,
            this.toolStripSplitButton1,
            this.btnBold,
            this.btnItalic,
            this.btnUnderLine,
            this.btnStrikeOut,
            this.btnSuperScript,
            this.btnSubScript,
            this.toolStripSeparator4,
            this.btnRightIndent,
            this.btnLeftIndent,
            this.btnAlignLeft,
            this.btnAlignCenter,
            this.btnAlignRight,
            this.btnAlignJustify,
            this.btnAlignScatter});
            this.tlbTool.Location = new System.Drawing.Point(0, 0);
            this.tlbTool.Name = "tlbTool";
            this.tlbTool.Size = new System.Drawing.Size(610, 25);
            this.tlbTool.TabIndex = 5;
            this.tlbTool.Text = "toolStrip1";
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSave.Image = global::EMRView.Properties.Resources._00003;
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(23, 22);
            this.btnSave.Text = "保存";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnUndo
            // 
            this.btnUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnUndo.Image = global::EMRView.Properties.Resources._00128;
            this.btnUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(23, 22);
            this.btnUndo.Text = "撤销";
            // 
            // btnRedo
            // 
            this.btnRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRedo.Image = global::EMRView.Properties.Resources._00129;
            this.btnRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRedo.Name = "btnRedo";
            this.btnRedo.Size = new System.Drawing.Size(23, 22);
            this.btnRedo.Text = "恢复";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // cbbFont
            // 
            this.cbbFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbFont.Name = "cbbFont";
            this.cbbFont.Size = new System.Drawing.Size(75, 25);
            this.cbbFont.DropDownClosed += new System.EventHandler(this.cbbFont_DropDownClosed);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // cbbFontSize
            // 
            this.cbbFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbFontSize.Items.AddRange(new object[] {
            "初号",
            "小初",
            "一号",
            "小一",
            "二号",
            "小二",
            "三号",
            "小三",
            "四号",
            "小四",
            "五号",
            "小五"});
            this.cbbFontSize.Name = "cbbFontSize";
            this.cbbFontSize.Size = new System.Drawing.Size(75, 25);
            this.cbbFontSize.DropDownClosed += new System.EventHandler(this.cbbFontSize_DropDownClosed);
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton1.Image")));
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(32, 22);
            this.toolStripSplitButton1.Text = "toolStripSplitButton1";
            // 
            // btnBold
            // 
            this.btnBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnBold.Image = global::EMRView.Properties.Resources._00113;
            this.btnBold.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBold.Name = "btnBold";
            this.btnBold.Size = new System.Drawing.Size(23, 22);
            this.btnBold.Tag = "0";
            this.btnBold.Text = "加粗";
            this.btnBold.Click += new System.EventHandler(this.btnBold_Click);
            // 
            // btnItalic
            // 
            this.btnItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnItalic.Image = global::EMRView.Properties.Resources._00114;
            this.btnItalic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnItalic.Name = "btnItalic";
            this.btnItalic.Size = new System.Drawing.Size(23, 22);
            this.btnItalic.Tag = "1";
            this.btnItalic.Text = "倾斜";
            this.btnItalic.Click += new System.EventHandler(this.btnBold_Click);
            // 
            // btnUnderLine
            // 
            this.btnUnderLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnUnderLine.Image = global::EMRView.Properties.Resources._00115;
            this.btnUnderLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUnderLine.Name = "btnUnderLine";
            this.btnUnderLine.Size = new System.Drawing.Size(23, 22);
            this.btnUnderLine.Tag = "2";
            this.btnUnderLine.Text = "下划线";
            this.btnUnderLine.Click += new System.EventHandler(this.btnBold_Click);
            // 
            // btnStrikeOut
            // 
            this.btnStrikeOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStrikeOut.Image = global::EMRView.Properties.Resources._00115U;
            this.btnStrikeOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStrikeOut.Name = "btnStrikeOut";
            this.btnStrikeOut.Size = new System.Drawing.Size(23, 22);
            this.btnStrikeOut.Tag = "3";
            this.btnStrikeOut.Text = "中划线";
            this.btnStrikeOut.Click += new System.EventHandler(this.btnBold_Click);
            // 
            // btnSuperScript
            // 
            this.btnSuperScript.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSuperScript.Image = global::EMRView.Properties.Resources._00057;
            this.btnSuperScript.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSuperScript.Name = "btnSuperScript";
            this.btnSuperScript.Size = new System.Drawing.Size(23, 22);
            this.btnSuperScript.Tag = "4";
            this.btnSuperScript.Text = "上标";
            this.btnSuperScript.Click += new System.EventHandler(this.btnBold_Click);
            // 
            // btnSubScript
            // 
            this.btnSubScript.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSubScript.Image = global::EMRView.Properties.Resources._00058;
            this.btnSubScript.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSubScript.Name = "btnSubScript";
            this.btnSubScript.Size = new System.Drawing.Size(23, 22);
            this.btnSubScript.Tag = "5";
            this.btnSubScript.Text = "下标";
            this.btnSubScript.Click += new System.EventHandler(this.btnBold_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // btnRightIndent
            // 
            this.btnRightIndent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRightIndent.Image = global::EMRView.Properties.Resources._00015;
            this.btnRightIndent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRightIndent.Name = "btnRightIndent";
            this.btnRightIndent.Size = new System.Drawing.Size(23, 22);
            this.btnRightIndent.Tag = "5";
            this.btnRightIndent.Text = "toolStripButton1";
            this.btnRightIndent.Click += new System.EventHandler(this.btnRightIndent_Click);
            // 
            // btnLeftIndent
            // 
            this.btnLeftIndent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnLeftIndent.Image = global::EMRView.Properties.Resources._00014;
            this.btnLeftIndent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLeftIndent.Name = "btnLeftIndent";
            this.btnLeftIndent.Size = new System.Drawing.Size(23, 22);
            this.btnLeftIndent.Tag = "6";
            this.btnLeftIndent.Text = "toolStripButton2";
            this.btnLeftIndent.Click += new System.EventHandler(this.btnRightIndent_Click);
            // 
            // btnAlignLeft
            // 
            this.btnAlignLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAlignLeft.Image = global::EMRView.Properties.Resources.段00120;
            this.btnAlignLeft.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAlignLeft.Name = "btnAlignLeft";
            this.btnAlignLeft.Size = new System.Drawing.Size(23, 22);
            this.btnAlignLeft.Tag = "0";
            this.btnAlignLeft.Text = "左对齐";
            this.btnAlignLeft.Click += new System.EventHandler(this.btnRightIndent_Click);
            // 
            // btnAlignCenter
            // 
            this.btnAlignCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAlignCenter.Image = global::EMRView.Properties.Resources.段00122;
            this.btnAlignCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAlignCenter.Name = "btnAlignCenter";
            this.btnAlignCenter.Size = new System.Drawing.Size(23, 22);
            this.btnAlignCenter.Tag = "1";
            this.btnAlignCenter.Text = "居中对齐";
            this.btnAlignCenter.Click += new System.EventHandler(this.btnRightIndent_Click);
            // 
            // btnAlignRight
            // 
            this.btnAlignRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAlignRight.Image = global::EMRView.Properties.Resources.段00121;
            this.btnAlignRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAlignRight.Name = "btnAlignRight";
            this.btnAlignRight.Size = new System.Drawing.Size(23, 22);
            this.btnAlignRight.Tag = "2";
            this.btnAlignRight.Text = "右对齐";
            this.btnAlignRight.Click += new System.EventHandler(this.btnRightIndent_Click);
            // 
            // btnAlignJustify
            // 
            this.btnAlignJustify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAlignJustify.Image = global::EMRView.Properties.Resources.段00123;
            this.btnAlignJustify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAlignJustify.Name = "btnAlignJustify";
            this.btnAlignJustify.Size = new System.Drawing.Size(23, 22);
            this.btnAlignJustify.Tag = "3";
            this.btnAlignJustify.Text = "分散对齐";
            this.btnAlignJustify.Click += new System.EventHandler(this.btnRightIndent_Click);
            // 
            // btnAlignScatter
            // 
            this.btnAlignScatter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAlignScatter.Image = global::EMRView.Properties.Resources.段落分散对齐09246;
            this.btnAlignScatter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAlignScatter.Name = "btnAlignScatter";
            this.btnAlignScatter.Size = new System.Drawing.Size(23, 22);
            this.btnAlignScatter.Tag = "4";
            this.btnAlignScatter.Text = "两端对齐";
            this.btnAlignScatter.Click += new System.EventHandler(this.btnRightIndent_Click);
            // 
            // frmItemContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(925, 433);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmItemContent";
            this.Text = "frmItemContent";
            this.Load += new System.EventHandler(this.frmItemContent_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDE)).EndInit();
            this.tlbTool.ResumeLayout(false);
            this.tlbTool.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbxPY;
        private System.Windows.Forms.DataGridView dgvDE;
        private System.Windows.Forms.DataGridViewTextBoxColumn Key;
        private System.Windows.Forms.DataGridViewTextBoxColumn value;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.ToolStrip tlbTool;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnUndo;
        private System.Windows.Forms.ToolStripButton btnRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripComboBox cbbFont;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripComboBox cbbFontSize;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripButton btnBold;
        private System.Windows.Forms.ToolStripButton btnItalic;
        private System.Windows.Forms.ToolStripButton btnUnderLine;
        private System.Windows.Forms.ToolStripButton btnStrikeOut;
        private System.Windows.Forms.ToolStripButton btnSuperScript;
        private System.Windows.Forms.ToolStripButton btnSubScript;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnRightIndent;
        private System.Windows.Forms.ToolStripButton btnLeftIndent;
        private System.Windows.Forms.ToolStripButton btnAlignLeft;
        private System.Windows.Forms.ToolStripButton btnAlignCenter;
        private System.Windows.Forms.ToolStripButton btnAlignRight;
        private System.Windows.Forms.ToolStripButton btnAlignJustify;
        private System.Windows.Forms.ToolStripButton btnAlignScatter;
        private System.Windows.Forms.Panel pnlEdit;
    }
}