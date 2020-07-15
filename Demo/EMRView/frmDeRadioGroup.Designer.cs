namespace EMRView
{
    partial class frmDeRadioGroup
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
            this.cbxMulSelect = new System.Windows.Forms.CheckBox();
            this.cbbRadioStyle = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbxDeleteAllow = new System.Windows.Forms.CheckBox();
            this.tbxHeight = new System.Windows.Forms.TextBox();
            this.tbxWidth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbxAutoSize = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.dgvItem = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.dgvRadioGroup = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label6 = new System.Windows.Forms.Label();
            this.tbxColume = new System.Windows.Forms.TextBox();
            this.cbxColumnAlign = new System.Windows.Forms.CheckBox();
            this.cbxItemHit = new System.Windows.Forms.CheckBox();
            this.pnlSize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItem)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRadioGroup)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlSize
            // 
            this.pnlSize.Controls.Add(this.cbxItemHit);
            this.pnlSize.Controls.Add(this.cbxColumnAlign);
            this.pnlSize.Controls.Add(this.tbxColume);
            this.pnlSize.Controls.Add(this.label6);
            this.pnlSize.Controls.Add(this.cbxMulSelect);
            this.pnlSize.Controls.Add(this.cbbRadioStyle);
            this.pnlSize.Controls.Add(this.label4);
            this.pnlSize.Controls.Add(this.label3);
            this.pnlSize.Controls.Add(this.cbxDeleteAllow);
            this.pnlSize.Controls.Add(this.tbxHeight);
            this.pnlSize.Controls.Add(this.tbxWidth);
            this.pnlSize.Controls.Add(this.label2);
            this.pnlSize.Controls.Add(this.label1);
            this.pnlSize.Controls.Add(this.cbxAutoSize);
            this.pnlSize.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSize.Location = new System.Drawing.Point(0, 0);
            this.pnlSize.Name = "pnlSize";
            this.pnlSize.Size = new System.Drawing.Size(329, 122);
            this.pnlSize.TabIndex = 2;
            // 
            // cbxMulSelect
            // 
            this.cbxMulSelect.AutoSize = true;
            this.cbxMulSelect.Location = new System.Drawing.Point(200, 44);
            this.cbxMulSelect.Name = "cbxMulSelect";
            this.cbxMulSelect.Size = new System.Drawing.Size(48, 16);
            this.cbxMulSelect.TabIndex = 18;
            this.cbxMulSelect.Text = "多选";
            this.cbxMulSelect.UseVisualStyleBackColor = true;
            // 
            // cbbRadioStyle
            // 
            this.cbbRadioStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbRadioStyle.FormattingEnabled = true;
            this.cbbRadioStyle.Items.AddRange(new object[] {
            "RadioButton",
            "CheckBox"});
            this.cbbRadioStyle.Location = new System.Drawing.Point(100, 40);
            this.cbbRadioStyle.Name = "cbbRadioStyle";
            this.cbbRadioStyle.Size = new System.Drawing.Size(85, 20);
            this.cbbRadioStyle.TabIndex = 17;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 12);
            this.label4.TabIndex = 16;
            this.label4.Text = "RadioItem样式";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(209, 12);
            this.label3.TabIndex = 15;
            this.label3.Text = "（属性保存时丢弃第一列为空的属性）";
            // 
            // cbxDeleteAllow
            // 
            this.cbxDeleteAllow.AutoSize = true;
            this.cbxDeleteAllow.Location = new System.Drawing.Point(254, 42);
            this.cbxDeleteAllow.Name = "cbxDeleteAllow";
            this.cbxDeleteAllow.Size = new System.Drawing.Size(72, 16);
            this.cbxDeleteAllow.TabIndex = 12;
            this.cbxDeleteAllow.Text = "允许删除";
            this.cbxDeleteAllow.UseVisualStyleBackColor = true;
            // 
            // tbxHeight
            // 
            this.tbxHeight.Location = new System.Drawing.Point(216, 9);
            this.tbxHeight.Name = "tbxHeight";
            this.tbxHeight.Size = new System.Drawing.Size(50, 21);
            this.tbxHeight.TabIndex = 4;
            // 
            // tbxWidth
            // 
            this.tbxWidth.Location = new System.Drawing.Point(135, 9);
            this.tbxWidth.Name = "tbxWidth";
            this.tbxWidth.Size = new System.Drawing.Size(50, 21);
            this.tbxWidth.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(198, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "高";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(118, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "宽";
            // 
            // cbxAutoSize
            // 
            this.cbxAutoSize.AutoSize = true;
            this.cbxAutoSize.Location = new System.Drawing.Point(14, 11);
            this.cbxAutoSize.Name = "cbxAutoSize";
            this.cbxAutoSize.Size = new System.Drawing.Size(96, 16);
            this.cbxAutoSize.TabIndex = 0;
            this.cbxAutoSize.Text = "自动计算宽高";
            this.cbxAutoSize.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(123, 392);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 19;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // dgvItem
            // 
            this.dgvItem.AllowUserToResizeRows = false;
            this.dgvItem.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvItem.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4});
            this.dgvItem.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvItem.Location = new System.Drawing.Point(0, 268);
            this.dgvItem.Name = "dgvItem";
            this.dgvItem.RowHeadersVisible = false;
            this.dgvItem.RowTemplate.Height = 23;
            this.dgvItem.Size = new System.Drawing.Size(329, 110);
            this.dgvItem.TabIndex = 18;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "键";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "值";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.Width = 200;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label5);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 234);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(329, 34);
            this.panel1.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "选项";
            // 
            // dgvRadioGroup
            // 
            this.dgvRadioGroup.AllowUserToResizeRows = false;
            this.dgvRadioGroup.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRadioGroup.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2});
            this.dgvRadioGroup.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvRadioGroup.Location = new System.Drawing.Point(0, 122);
            this.dgvRadioGroup.Name = "dgvRadioGroup";
            this.dgvRadioGroup.RowHeadersVisible = false;
            this.dgvRadioGroup.RowTemplate.Height = 23;
            this.dgvRadioGroup.Size = new System.Drawing.Size(329, 112);
            this.dgvRadioGroup.TabIndex = 16;
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
            this.dataGridViewTextBoxColumn2.Width = 200;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 74);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 12);
            this.label6.TabIndex = 19;
            this.label6.Text = "列";
            // 
            // tbxColume
            // 
            this.tbxColume.Location = new System.Drawing.Point(40, 70);
            this.tbxColume.Name = "tbxColume";
            this.tbxColume.Size = new System.Drawing.Size(45, 21);
            this.tbxColume.TabIndex = 20;
            this.tbxColume.Text = "0";
            // 
            // cbxColumnAlign
            // 
            this.cbxColumnAlign.AutoSize = true;
            this.cbxColumnAlign.Location = new System.Drawing.Point(100, 73);
            this.cbxColumnAlign.Name = "cbxColumnAlign";
            this.cbxColumnAlign.Size = new System.Drawing.Size(84, 16);
            this.cbxColumnAlign.TabIndex = 21;
            this.cbxColumnAlign.Text = "列自动对齐";
            this.cbxColumnAlign.UseVisualStyleBackColor = true;
            // 
            // cbxItemHit
            // 
            this.cbxItemHit.AutoSize = true;
            this.cbxItemHit.Location = new System.Drawing.Point(200, 72);
            this.cbxItemHit.Name = "cbxItemHit";
            this.cbxItemHit.Size = new System.Drawing.Size(120, 16);
            this.cbxItemHit.TabIndex = 22;
            this.cbxItemHit.Text = "点击文本切换选中";
            this.cbxItemHit.UseVisualStyleBackColor = true;
            // 
            // frmDeRadioGroup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 423);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.dgvItem);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.dgvRadioGroup);
            this.Controls.Add(this.pnlSize);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDeRadioGroup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DeRadioGroup属性";
            this.pnlSize.ResumeLayout(false);
            this.pnlSize.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItem)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRadioGroup)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbxDeleteAllow;
        private System.Windows.Forms.TextBox tbxHeight;
        private System.Windows.Forms.TextBox tbxWidth;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbxAutoSize;
        private System.Windows.Forms.CheckBox cbxMulSelect;
        private System.Windows.Forms.ComboBox cbbRadioStyle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridView dgvItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridView dgvRadioGroup;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.CheckBox cbxItemHit;
        private System.Windows.Forms.CheckBox cbxColumnAlign;
        private System.Windows.Forms.TextBox tbxColume;
        private System.Windows.Forms.Label label6;
    }
}