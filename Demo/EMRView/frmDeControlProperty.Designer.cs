namespace EMRView
{
    partial class frmDeControlProperty
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
            this.pnlSize = new System.Windows.Forms.Panel();
            this.tbxHeight = new System.Windows.Forms.TextBox();
            this.tbxWidth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbxAutoSize = new System.Windows.Forms.CheckBox();
            this.pnlEdit = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.dgvEdit = new System.Windows.Forms.DataGridView();
            this.Key = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlBorder = new System.Windows.Forms.Panel();
            this.cbxBorderRight = new System.Windows.Forms.CheckBox();
            this.cbxBorderLeft = new System.Windows.Forms.CheckBox();
            this.cbxBorderBottom = new System.Windows.Forms.CheckBox();
            this.cbxBorderTop = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pnlCombobox = new System.Windows.Forms.Panel();
            this.lstCombobox = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnMod = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.tbxValue = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dgvCombobox = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlRadioGroup = new System.Windows.Forms.Panel();
            this.lstRadioItem = new System.Windows.Forms.ListBox();
            this.btnDeleteRadioItem = new System.Windows.Forms.Button();
            this.btnModRadioItem = new System.Windows.Forms.Button();
            this.btnAddRadioItem = new System.Windows.Forms.Button();
            this.tbxRadioValue = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.pnlDateTime = new System.Windows.Forms.Panel();
            this.cbbDTFormat = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.tbxText = new System.Windows.Forms.TextBox();
            this.pnlSize.SuspendLayout();
            this.pnlEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEdit)).BeginInit();
            this.pnlBorder.SuspendLayout();
            this.pnlCombobox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCombobox)).BeginInit();
            this.pnlRadioGroup.SuspendLayout();
            this.pnlDateTime.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSize
            // 
            this.pnlSize.Controls.Add(this.tbxText);
            this.pnlSize.Controls.Add(this.label9);
            this.pnlSize.Controls.Add(this.tbxHeight);
            this.pnlSize.Controls.Add(this.tbxWidth);
            this.pnlSize.Controls.Add(this.label2);
            this.pnlSize.Controls.Add(this.label1);
            this.pnlSize.Controls.Add(this.cbxAutoSize);
            this.pnlSize.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSize.Location = new System.Drawing.Point(0, 0);
            this.pnlSize.Name = "pnlSize";
            this.pnlSize.Size = new System.Drawing.Size(304, 98);
            this.pnlSize.TabIndex = 0;
            // 
            // tbxHeight
            // 
            this.tbxHeight.Location = new System.Drawing.Point(175, 43);
            this.tbxHeight.Name = "tbxHeight";
            this.tbxHeight.Size = new System.Drawing.Size(80, 21);
            this.tbxHeight.TabIndex = 4;
            // 
            // tbxWidth
            // 
            this.tbxWidth.Location = new System.Drawing.Point(53, 43);
            this.tbxWidth.Name = "tbxWidth";
            this.tbxWidth.Size = new System.Drawing.Size(80, 21);
            this.tbxWidth.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(152, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "高";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "宽";
            // 
            // cbxAutoSize
            // 
            this.cbxAutoSize.AutoSize = true;
            this.cbxAutoSize.Location = new System.Drawing.Point(14, 14);
            this.cbxAutoSize.Name = "cbxAutoSize";
            this.cbxAutoSize.Size = new System.Drawing.Size(96, 16);
            this.cbxAutoSize.TabIndex = 0;
            this.cbxAutoSize.Text = "自动计算宽高";
            this.cbxAutoSize.UseVisualStyleBackColor = true;
            // 
            // pnlEdit
            // 
            this.pnlEdit.Controls.Add(this.label3);
            this.pnlEdit.Controls.Add(this.dgvEdit);
            this.pnlEdit.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEdit.Location = new System.Drawing.Point(0, 98);
            this.pnlEdit.Name = "pnlEdit";
            this.pnlEdit.Size = new System.Drawing.Size(304, 141);
            this.pnlEdit.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(209, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "DeEdit属性（第一列无值时不会存储）";
            // 
            // dgvEdit
            // 
            this.dgvEdit.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEdit.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Key,
            this.value});
            this.dgvEdit.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvEdit.Location = new System.Drawing.Point(0, 0);
            this.dgvEdit.Name = "dgvEdit";
            this.dgvEdit.RowHeadersVisible = false;
            this.dgvEdit.RowTemplate.Height = 23;
            this.dgvEdit.Size = new System.Drawing.Size(304, 110);
            this.dgvEdit.TabIndex = 0;
            // 
            // Key
            // 
            this.Key.HeaderText = "键";
            this.Key.Name = "Key";
            // 
            // value
            // 
            this.value.HeaderText = "值";
            this.value.Name = "value";
            // 
            // pnlBorder
            // 
            this.pnlBorder.Controls.Add(this.cbxBorderRight);
            this.pnlBorder.Controls.Add(this.cbxBorderLeft);
            this.pnlBorder.Controls.Add(this.cbxBorderBottom);
            this.pnlBorder.Controls.Add(this.cbxBorderTop);
            this.pnlBorder.Controls.Add(this.label4);
            this.pnlBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBorder.Location = new System.Drawing.Point(0, 239);
            this.pnlBorder.Name = "pnlBorder";
            this.pnlBorder.Size = new System.Drawing.Size(304, 58);
            this.pnlBorder.TabIndex = 2;
            // 
            // cbxBorderRight
            // 
            this.cbxBorderRight.AutoSize = true;
            this.cbxBorderRight.Location = new System.Drawing.Point(238, 33);
            this.cbxBorderRight.Name = "cbxBorderRight";
            this.cbxBorderRight.Size = new System.Drawing.Size(36, 16);
            this.cbxBorderRight.TabIndex = 4;
            this.cbxBorderRight.Text = "右";
            this.cbxBorderRight.UseVisualStyleBackColor = true;
            // 
            // cbxBorderLeft
            // 
            this.cbxBorderLeft.AutoSize = true;
            this.cbxBorderLeft.Location = new System.Drawing.Point(168, 33);
            this.cbxBorderLeft.Name = "cbxBorderLeft";
            this.cbxBorderLeft.Size = new System.Drawing.Size(36, 16);
            this.cbxBorderLeft.TabIndex = 3;
            this.cbxBorderLeft.Text = "左";
            this.cbxBorderLeft.UseVisualStyleBackColor = true;
            // 
            // cbxBorderBottom
            // 
            this.cbxBorderBottom.AutoSize = true;
            this.cbxBorderBottom.Location = new System.Drawing.Point(95, 33);
            this.cbxBorderBottom.Name = "cbxBorderBottom";
            this.cbxBorderBottom.Size = new System.Drawing.Size(36, 16);
            this.cbxBorderBottom.TabIndex = 2;
            this.cbxBorderBottom.Text = "下";
            this.cbxBorderBottom.UseVisualStyleBackColor = true;
            // 
            // cbxBorderTop
            // 
            this.cbxBorderTop.AutoSize = true;
            this.cbxBorderTop.Location = new System.Drawing.Point(26, 33);
            this.cbxBorderTop.Name = "cbxBorderTop";
            this.cbxBorderTop.Size = new System.Drawing.Size(36, 16);
            this.cbxBorderTop.TabIndex = 1;
            this.cbxBorderTop.Text = "上";
            this.cbxBorderTop.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 7);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "边框";
            // 
            // pnlCombobox
            // 
            this.pnlCombobox.Controls.Add(this.lstCombobox);
            this.pnlCombobox.Controls.Add(this.label6);
            this.pnlCombobox.Controls.Add(this.btnDelete);
            this.pnlCombobox.Controls.Add(this.btnMod);
            this.pnlCombobox.Controls.Add(this.btnAdd);
            this.pnlCombobox.Controls.Add(this.tbxValue);
            this.pnlCombobox.Controls.Add(this.label5);
            this.pnlCombobox.Controls.Add(this.dgvCombobox);
            this.pnlCombobox.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCombobox.Location = new System.Drawing.Point(0, 297);
            this.pnlCombobox.Name = "pnlCombobox";
            this.pnlCombobox.Size = new System.Drawing.Size(304, 194);
            this.pnlCombobox.TabIndex = 3;
            // 
            // lstCombobox
            // 
            this.lstCombobox.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstCombobox.FormattingEnabled = true;
            this.lstCombobox.ItemHeight = 12;
            this.lstCombobox.Location = new System.Drawing.Point(0, 74);
            this.lstCombobox.Name = "lstCombobox";
            this.lstCombobox.Size = new System.Drawing.Size(304, 76);
            this.lstCombobox.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(289, 167);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(233, 12);
            this.label6.TabIndex = 7;
            this.label6.Text = "DeCombobox属性（第一列无值时不会存储）";
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(237, 163);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(45, 23);
            this.btnDelete.TabIndex = 6;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnMod
            // 
            this.btnMod.Location = new System.Drawing.Point(186, 163);
            this.btnMod.Name = "btnMod";
            this.btnMod.Size = new System.Drawing.Size(45, 23);
            this.btnMod.TabIndex = 5;
            this.btnMod.Text = "修改";
            this.btnMod.UseVisualStyleBackColor = true;
            this.btnMod.Click += new System.EventHandler(this.btnMod_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(135, 163);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(45, 23);
            this.btnAdd.TabIndex = 4;
            this.btnAdd.Text = "添加";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // tbxValue
            // 
            this.tbxValue.Location = new System.Drawing.Point(29, 164);
            this.tbxValue.Name = "tbxValue";
            this.tbxValue.Size = new System.Drawing.Size(100, 21);
            this.tbxValue.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 168);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 12);
            this.label5.TabIndex = 2;
            this.label5.Text = "值";
            // 
            // dgvCombobox
            // 
            this.dgvCombobox.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCombobox.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2});
            this.dgvCombobox.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvCombobox.Location = new System.Drawing.Point(0, 0);
            this.dgvCombobox.Name = "dgvCombobox";
            this.dgvCombobox.RowHeadersVisible = false;
            this.dgvCombobox.RowTemplate.Height = 23;
            this.dgvCombobox.Size = new System.Drawing.Size(304, 74);
            this.dgvCombobox.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "键";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "值";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // pnlRadioGroup
            // 
            this.pnlRadioGroup.Controls.Add(this.lstRadioItem);
            this.pnlRadioGroup.Controls.Add(this.btnDeleteRadioItem);
            this.pnlRadioGroup.Controls.Add(this.btnModRadioItem);
            this.pnlRadioGroup.Controls.Add(this.btnAddRadioItem);
            this.pnlRadioGroup.Controls.Add(this.tbxRadioValue);
            this.pnlRadioGroup.Controls.Add(this.label7);
            this.pnlRadioGroup.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRadioGroup.Location = new System.Drawing.Point(0, 491);
            this.pnlRadioGroup.Name = "pnlRadioGroup";
            this.pnlRadioGroup.Size = new System.Drawing.Size(304, 149);
            this.pnlRadioGroup.TabIndex = 4;
            // 
            // lstRadioItem
            // 
            this.lstRadioItem.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstRadioItem.FormattingEnabled = true;
            this.lstRadioItem.ItemHeight = 12;
            this.lstRadioItem.Location = new System.Drawing.Point(0, 0);
            this.lstRadioItem.Name = "lstRadioItem";
            this.lstRadioItem.Size = new System.Drawing.Size(304, 112);
            this.lstRadioItem.TabIndex = 12;
            // 
            // btnDeleteRadioItem
            // 
            this.btnDeleteRadioItem.Location = new System.Drawing.Point(241, 119);
            this.btnDeleteRadioItem.Name = "btnDeleteRadioItem";
            this.btnDeleteRadioItem.Size = new System.Drawing.Size(45, 23);
            this.btnDeleteRadioItem.TabIndex = 11;
            this.btnDeleteRadioItem.Text = "删除";
            this.btnDeleteRadioItem.UseVisualStyleBackColor = true;
            this.btnDeleteRadioItem.Click += new System.EventHandler(this.btnDeleteRadioItem_Click);
            // 
            // btnModRadioItem
            // 
            this.btnModRadioItem.Location = new System.Drawing.Point(190, 119);
            this.btnModRadioItem.Name = "btnModRadioItem";
            this.btnModRadioItem.Size = new System.Drawing.Size(45, 23);
            this.btnModRadioItem.TabIndex = 10;
            this.btnModRadioItem.Text = "修改";
            this.btnModRadioItem.UseVisualStyleBackColor = true;
            this.btnModRadioItem.Click += new System.EventHandler(this.btnModRadioItem_Click);
            // 
            // btnAddRadioItem
            // 
            this.btnAddRadioItem.Location = new System.Drawing.Point(139, 119);
            this.btnAddRadioItem.Name = "btnAddRadioItem";
            this.btnAddRadioItem.Size = new System.Drawing.Size(45, 23);
            this.btnAddRadioItem.TabIndex = 9;
            this.btnAddRadioItem.Text = "添加";
            this.btnAddRadioItem.UseVisualStyleBackColor = true;
            this.btnAddRadioItem.Click += new System.EventHandler(this.btnAddRadioItem_Click);
            // 
            // tbxRadioValue
            // 
            this.tbxRadioValue.Location = new System.Drawing.Point(33, 120);
            this.tbxRadioValue.Name = "tbxRadioValue";
            this.tbxRadioValue.Size = new System.Drawing.Size(100, 21);
            this.tbxRadioValue.TabIndex = 8;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 124);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 12);
            this.label7.TabIndex = 7;
            this.label7.Text = "值";
            // 
            // pnlDateTime
            // 
            this.pnlDateTime.Controls.Add(this.cbbDTFormat);
            this.pnlDateTime.Controls.Add(this.label8);
            this.pnlDateTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDateTime.Location = new System.Drawing.Point(0, 640);
            this.pnlDateTime.Name = "pnlDateTime";
            this.pnlDateTime.Size = new System.Drawing.Size(304, 39);
            this.pnlDateTime.TabIndex = 5;
            // 
            // cbbDTFormat
            // 
            this.cbbDTFormat.FormattingEnabled = true;
            this.cbbDTFormat.Items.AddRange(new object[] {
            "yyyy年M月d日",
            "yyyy年MM月dd日",
            "yyyy-M-d",
            "yyyy-MM-dd",
            "yyyy/M/d",
            "yyyy/MM/dd"});
            this.cbbDTFormat.Location = new System.Drawing.Point(43, 10);
            this.cbbDTFormat.Name = "cbbDTFormat";
            this.cbbDTFormat.Size = new System.Drawing.Size(147, 20);
            this.cbbDTFormat.TabIndex = 6;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 13);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 0;
            this.label8.Text = "格式";
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.btnSave);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBottom.Location = new System.Drawing.Point(0, 679);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(304, 32);
            this.pnlBottom.TabIndex = 7;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(115, 5);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(18, 74);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 12);
            this.label9.TabIndex = 5;
            this.label9.Text = "文本";
            // 
            // tbxText
            // 
            this.tbxText.Location = new System.Drawing.Point(53, 71);
            this.tbxText.Name = "tbxText";
            this.tbxText.Size = new System.Drawing.Size(202, 21);
            this.tbxText.TabIndex = 6;
            // 
            // frmDeControlProperty
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(304, 750);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlDateTime);
            this.Controls.Add(this.pnlRadioGroup);
            this.Controls.Add(this.pnlCombobox);
            this.Controls.Add(this.pnlBorder);
            this.Controls.Add(this.pnlEdit);
            this.Controls.Add(this.pnlSize);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDeControlProperty";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ControlItem属性";
            this.pnlSize.ResumeLayout(false);
            this.pnlSize.PerformLayout();
            this.pnlEdit.ResumeLayout(false);
            this.pnlEdit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEdit)).EndInit();
            this.pnlBorder.ResumeLayout(false);
            this.pnlBorder.PerformLayout();
            this.pnlCombobox.ResumeLayout(false);
            this.pnlCombobox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCombobox)).EndInit();
            this.pnlRadioGroup.ResumeLayout(false);
            this.pnlRadioGroup.PerformLayout();
            this.pnlDateTime.ResumeLayout(false);
            this.pnlDateTime.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSize;
        private System.Windows.Forms.TextBox tbxHeight;
        private System.Windows.Forms.TextBox tbxWidth;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbxAutoSize;
        private System.Windows.Forms.Panel pnlEdit;
        private System.Windows.Forms.DataGridView dgvEdit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Key;
        private System.Windows.Forms.DataGridViewTextBoxColumn value;
        private System.Windows.Forms.Panel pnlBorder;
        private System.Windows.Forms.CheckBox cbxBorderRight;
        private System.Windows.Forms.CheckBox cbxBorderLeft;
        private System.Windows.Forms.CheckBox cbxBorderBottom;
        private System.Windows.Forms.CheckBox cbxBorderTop;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel pnlCombobox;
        private System.Windows.Forms.DataGridView dgvCombobox;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnMod;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.TextBox tbxValue;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel pnlRadioGroup;
        private System.Windows.Forms.ListBox lstRadioItem;
        private System.Windows.Forms.Button btnDeleteRadioItem;
        private System.Windows.Forms.Button btnModRadioItem;
        private System.Windows.Forms.Button btnAddRadioItem;
        private System.Windows.Forms.TextBox tbxRadioValue;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel pnlDateTime;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbbDTFormat;
        private System.Windows.Forms.ListBox lstCombobox;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox tbxText;
        private System.Windows.Forms.Label label9;
    }
}