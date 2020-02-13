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
            this.tbxText = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
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
            this.pnlDateTime = new System.Windows.Forms.Panel();
            this.cbbDTFormat = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.pnlSize.SuspendLayout();
            this.pnlEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEdit)).BeginInit();
            this.pnlBorder.SuspendLayout();
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
            this.pnlSize.Size = new System.Drawing.Size(476, 38);
            this.pnlSize.TabIndex = 0;
            // 
            // tbxText
            // 
            this.tbxText.Location = new System.Drawing.Point(314, 9);
            this.tbxText.Name = "tbxText";
            this.tbxText.Size = new System.Drawing.Size(151, 21);
            this.tbxText.TabIndex = 6;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(282, 12);
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
            // pnlEdit
            // 
            this.pnlEdit.Controls.Add(this.label3);
            this.pnlEdit.Controls.Add(this.dgvEdit);
            this.pnlEdit.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEdit.Location = new System.Drawing.Point(0, 38);
            this.pnlEdit.Name = "pnlEdit";
            this.pnlEdit.Size = new System.Drawing.Size(476, 113);
            this.pnlEdit.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.Location = new System.Drawing.Point(0, 100);
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
            this.dgvEdit.Size = new System.Drawing.Size(476, 100);
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
            this.value.Width = 200;
            // 
            // pnlBorder
            // 
            this.pnlBorder.Controls.Add(this.cbxBorderRight);
            this.pnlBorder.Controls.Add(this.cbxBorderLeft);
            this.pnlBorder.Controls.Add(this.cbxBorderBottom);
            this.pnlBorder.Controls.Add(this.cbxBorderTop);
            this.pnlBorder.Controls.Add(this.label4);
            this.pnlBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBorder.Location = new System.Drawing.Point(0, 151);
            this.pnlBorder.Name = "pnlBorder";
            this.pnlBorder.Size = new System.Drawing.Size(476, 26);
            this.pnlBorder.TabIndex = 2;
            // 
            // cbxBorderRight
            // 
            this.cbxBorderRight.AutoSize = true;
            this.cbxBorderRight.Location = new System.Drawing.Point(254, 5);
            this.cbxBorderRight.Name = "cbxBorderRight";
            this.cbxBorderRight.Size = new System.Drawing.Size(36, 16);
            this.cbxBorderRight.TabIndex = 4;
            this.cbxBorderRight.Text = "右";
            this.cbxBorderRight.UseVisualStyleBackColor = true;
            // 
            // cbxBorderLeft
            // 
            this.cbxBorderLeft.AutoSize = true;
            this.cbxBorderLeft.Location = new System.Drawing.Point(184, 5);
            this.cbxBorderLeft.Name = "cbxBorderLeft";
            this.cbxBorderLeft.Size = new System.Drawing.Size(36, 16);
            this.cbxBorderLeft.TabIndex = 3;
            this.cbxBorderLeft.Text = "左";
            this.cbxBorderLeft.UseVisualStyleBackColor = true;
            // 
            // cbxBorderBottom
            // 
            this.cbxBorderBottom.AutoSize = true;
            this.cbxBorderBottom.Location = new System.Drawing.Point(111, 5);
            this.cbxBorderBottom.Name = "cbxBorderBottom";
            this.cbxBorderBottom.Size = new System.Drawing.Size(36, 16);
            this.cbxBorderBottom.TabIndex = 2;
            this.cbxBorderBottom.Text = "下";
            this.cbxBorderBottom.UseVisualStyleBackColor = true;
            // 
            // cbxBorderTop
            // 
            this.cbxBorderTop.AutoSize = true;
            this.cbxBorderTop.Location = new System.Drawing.Point(42, 5);
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
            // pnlDateTime
            // 
            this.pnlDateTime.Controls.Add(this.cbbDTFormat);
            this.pnlDateTime.Controls.Add(this.label8);
            this.pnlDateTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDateTime.Location = new System.Drawing.Point(0, 177);
            this.pnlDateTime.Name = "pnlDateTime";
            this.pnlDateTime.Size = new System.Drawing.Size(476, 37);
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
            this.pnlBottom.Location = new System.Drawing.Point(0, 214);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(476, 32);
            this.pnlBottom.TabIndex = 7;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(184, 5);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // frmDeControlProperty
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(476, 302);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlDateTime);
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
        private System.Windows.Forms.Panel pnlBorder;
        private System.Windows.Forms.CheckBox cbxBorderRight;
        private System.Windows.Forms.CheckBox cbxBorderLeft;
        private System.Windows.Forms.CheckBox cbxBorderBottom;
        private System.Windows.Forms.CheckBox cbxBorderTop;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel pnlDateTime;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbbDTFormat;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox tbxText;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Key;
        private System.Windows.Forms.DataGridViewTextBoxColumn value;
    }
}