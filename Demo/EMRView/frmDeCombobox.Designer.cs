namespace EMRView
{
    partial class frmDeCombobox
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
            this.label3 = new System.Windows.Forms.Label();
            this.cbxPrintOnlyText = new System.Windows.Forms.CheckBox();
            this.cbxDeleteAllow = new System.Windows.Forms.CheckBox();
            this.cbxBorderRight = new System.Windows.Forms.CheckBox();
            this.cbxBorderLeft = new System.Windows.Forms.CheckBox();
            this.cbxBorderBottom = new System.Windows.Forms.CheckBox();
            this.cbxBorderTop = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbxText = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbxHeight = new System.Windows.Forms.TextBox();
            this.tbxWidth = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbxAutoSize = new System.Windows.Forms.CheckBox();
            this.dgvCombobox = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbxSaveItem = new System.Windows.Forms.CheckBox();
            this.dgvItem = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSave = new System.Windows.Forms.Button();
            this.pnlSize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCombobox)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItem)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlSize
            // 
            this.pnlSize.Controls.Add(this.label3);
            this.pnlSize.Controls.Add(this.cbxPrintOnlyText);
            this.pnlSize.Controls.Add(this.cbxDeleteAllow);
            this.pnlSize.Controls.Add(this.cbxBorderRight);
            this.pnlSize.Controls.Add(this.cbxBorderLeft);
            this.pnlSize.Controls.Add(this.cbxBorderBottom);
            this.pnlSize.Controls.Add(this.cbxBorderTop);
            this.pnlSize.Controls.Add(this.label4);
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
            this.pnlSize.Size = new System.Drawing.Size(324, 149);
            this.pnlSize.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(209, 12);
            this.label3.TabIndex = 15;
            this.label3.Text = "（属性保存时丢弃第一列为空的属性）";
            // 
            // cbxPrintOnlyText
            // 
            this.cbxPrintOnlyText.AutoSize = true;
            this.cbxPrintOnlyText.Location = new System.Drawing.Point(92, 97);
            this.cbxPrintOnlyText.Name = "cbxPrintOnlyText";
            this.cbxPrintOnlyText.Size = new System.Drawing.Size(180, 16);
            this.cbxPrintOnlyText.TabIndex = 13;
            this.cbxPrintOnlyText.Text = "打印时仅打印文本不打印边框";
            this.cbxPrintOnlyText.UseVisualStyleBackColor = true;
            // 
            // cbxDeleteAllow
            // 
            this.cbxDeleteAllow.AutoSize = true;
            this.cbxDeleteAllow.Location = new System.Drawing.Point(14, 97);
            this.cbxDeleteAllow.Name = "cbxDeleteAllow";
            this.cbxDeleteAllow.Size = new System.Drawing.Size(72, 16);
            this.cbxDeleteAllow.TabIndex = 12;
            this.cbxDeleteAllow.Text = "允许删除";
            this.cbxDeleteAllow.UseVisualStyleBackColor = true;
            // 
            // cbxBorderRight
            // 
            this.cbxBorderRight.AutoSize = true;
            this.cbxBorderRight.Location = new System.Drawing.Point(231, 38);
            this.cbxBorderRight.Name = "cbxBorderRight";
            this.cbxBorderRight.Size = new System.Drawing.Size(36, 16);
            this.cbxBorderRight.TabIndex = 11;
            this.cbxBorderRight.Text = "右";
            this.cbxBorderRight.UseVisualStyleBackColor = true;
            // 
            // cbxBorderLeft
            // 
            this.cbxBorderLeft.AutoSize = true;
            this.cbxBorderLeft.Location = new System.Drawing.Point(174, 38);
            this.cbxBorderLeft.Name = "cbxBorderLeft";
            this.cbxBorderLeft.Size = new System.Drawing.Size(36, 16);
            this.cbxBorderLeft.TabIndex = 10;
            this.cbxBorderLeft.Text = "左";
            this.cbxBorderLeft.UseVisualStyleBackColor = true;
            // 
            // cbxBorderBottom
            // 
            this.cbxBorderBottom.AutoSize = true;
            this.cbxBorderBottom.Location = new System.Drawing.Point(117, 38);
            this.cbxBorderBottom.Name = "cbxBorderBottom";
            this.cbxBorderBottom.Size = new System.Drawing.Size(36, 16);
            this.cbxBorderBottom.TabIndex = 9;
            this.cbxBorderBottom.Text = "下";
            this.cbxBorderBottom.UseVisualStyleBackColor = true;
            // 
            // cbxBorderTop
            // 
            this.cbxBorderTop.AutoSize = true;
            this.cbxBorderTop.Location = new System.Drawing.Point(56, 38);
            this.cbxBorderTop.Name = "cbxBorderTop";
            this.cbxBorderTop.Size = new System.Drawing.Size(36, 16);
            this.cbxBorderTop.TabIndex = 8;
            this.cbxBorderTop.Text = "上";
            this.cbxBorderTop.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "边框";
            // 
            // tbxText
            // 
            this.tbxText.Location = new System.Drawing.Point(47, 65);
            this.tbxText.Name = "tbxText";
            this.tbxText.Size = new System.Drawing.Size(219, 21);
            this.tbxText.TabIndex = 6;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 70);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 12);
            this.label9.TabIndex = 5;
            this.label9.Text = "文本";
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
            // dgvCombobox
            // 
            this.dgvCombobox.AllowUserToResizeRows = false;
            this.dgvCombobox.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCombobox.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2});
            this.dgvCombobox.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvCombobox.Location = new System.Drawing.Point(0, 149);
            this.dgvCombobox.Name = "dgvCombobox";
            this.dgvCombobox.RowHeadersVisible = false;
            this.dgvCombobox.RowTemplate.Height = 23;
            this.dgvCombobox.Size = new System.Drawing.Size(324, 112);
            this.dgvCombobox.TabIndex = 12;
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
            // panel1
            // 
            this.panel1.Controls.Add(this.cbxSaveItem);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 261);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(324, 34);
            this.panel1.TabIndex = 13;
            // 
            // cbxSaveItem
            // 
            this.cbxSaveItem.AutoSize = true;
            this.cbxSaveItem.Location = new System.Drawing.Point(14, 9);
            this.cbxSaveItem.Name = "cbxSaveItem";
            this.cbxSaveItem.Size = new System.Drawing.Size(252, 16);
            this.cbxSaveItem.TabIndex = 0;
            this.cbxSaveItem.Text = "保存选项（保存时丢弃第一列为空的选项）";
            this.cbxSaveItem.UseVisualStyleBackColor = true;
            this.cbxSaveItem.CheckedChanged += new System.EventHandler(this.cbxSaveItem_CheckedChanged);
            // 
            // dgvItem
            // 
            this.dgvItem.AllowUserToResizeRows = false;
            this.dgvItem.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvItem.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4});
            this.dgvItem.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvItem.Location = new System.Drawing.Point(0, 295);
            this.dgvItem.Name = "dgvItem";
            this.dgvItem.RowHeadersVisible = false;
            this.dgvItem.RowTemplate.Height = 23;
            this.dgvItem.Size = new System.Drawing.Size(324, 110);
            this.dgvItem.TabIndex = 14;
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
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(120, 415);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 15;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // frmDeCombobox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 450);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.dgvItem);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.dgvCombobox);
            this.Controls.Add(this.pnlSize);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDeCombobox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DeCombobox属性";
            this.pnlSize.ResumeLayout(false);
            this.pnlSize.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCombobox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItem)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSize;
        private System.Windows.Forms.TextBox tbxText;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbxHeight;
        private System.Windows.Forms.TextBox tbxWidth;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbxAutoSize;
        private System.Windows.Forms.CheckBox cbxPrintOnlyText;
        private System.Windows.Forms.CheckBox cbxDeleteAllow;
        private System.Windows.Forms.CheckBox cbxBorderRight;
        private System.Windows.Forms.CheckBox cbxBorderLeft;
        private System.Windows.Forms.CheckBox cbxBorderBottom;
        private System.Windows.Forms.CheckBox cbxBorderTop;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dgvCombobox;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cbxSaveItem;
        private System.Windows.Forms.DataGridView dgvItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.Button btnSave;
    }
}