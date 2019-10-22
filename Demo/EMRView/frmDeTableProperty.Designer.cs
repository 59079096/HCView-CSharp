namespace EMRView
{
    partial class frmDeTableProperty
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
            this.tabTableInfo = new System.Windows.Forms.TabControl();
            this.tabTable = new System.Windows.Forms.TabPage();
            this.tbxFixColLast = new System.Windows.Forms.TextBox();
            this.tbxFixColFirst = new System.Windows.Forms.TextBox();
            this.tbxFixRowLast = new System.Windows.Forms.TextBox();
            this.tbxFixRowFirst = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.dgvTable = new System.Windows.Forms.DataGridView();
            this.Key = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnBorderBackColor = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.tbxBorderWidth = new System.Windows.Forms.TextBox();
            this.tbxCellVPadding = new System.Windows.Forms.TextBox();
            this.tbxCellHPadding = new System.Windows.Forms.TextBox();
            this.cbxBorderVisible = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabRow = new System.Windows.Forms.TabPage();
            this.tbxRowHeight = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabCell = new System.Windows.Forms.TabPage();
            this.cbbCellAlignVert = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.tabTableInfo.SuspendLayout();
            this.tabTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTable)).BeginInit();
            this.tabRow.SuspendLayout();
            this.tabCell.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabTableInfo
            // 
            this.tabTableInfo.Controls.Add(this.tabTable);
            this.tabTableInfo.Controls.Add(this.tabRow);
            this.tabTableInfo.Controls.Add(this.tabCell);
            this.tabTableInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabTableInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tabTableInfo.Location = new System.Drawing.Point(0, 0);
            this.tabTableInfo.Name = "tabTableInfo";
            this.tabTableInfo.SelectedIndex = 0;
            this.tabTableInfo.Size = new System.Drawing.Size(404, 355);
            this.tabTableInfo.TabIndex = 0;
            // 
            // tabTable
            // 
            this.tabTable.Controls.Add(this.tbxFixColLast);
            this.tabTable.Controls.Add(this.tbxFixColFirst);
            this.tabTable.Controls.Add(this.tbxFixRowLast);
            this.tabTable.Controls.Add(this.tbxFixRowFirst);
            this.tabTable.Controls.Add(this.label15);
            this.tabTable.Controls.Add(this.label14);
            this.tabTable.Controls.Add(this.label13);
            this.tabTable.Controls.Add(this.label12);
            this.tabTable.Controls.Add(this.label11);
            this.tabTable.Controls.Add(this.label10);
            this.tabTable.Controls.Add(this.label9);
            this.tabTable.Controls.Add(this.label8);
            this.tabTable.Controls.Add(this.dgvTable);
            this.tabTable.Controls.Add(this.btnBorderBackColor);
            this.tabTable.Controls.Add(this.label5);
            this.tabTable.Controls.Add(this.tbxBorderWidth);
            this.tabTable.Controls.Add(this.tbxCellVPadding);
            this.tabTable.Controls.Add(this.tbxCellHPadding);
            this.tabTable.Controls.Add(this.cbxBorderVisible);
            this.tabTable.Controls.Add(this.label4);
            this.tabTable.Controls.Add(this.label3);
            this.tabTable.Controls.Add(this.label2);
            this.tabTable.Controls.Add(this.label1);
            this.tabTable.Location = new System.Drawing.Point(4, 22);
            this.tabTable.Name = "tabTable";
            this.tabTable.Padding = new System.Windows.Forms.Padding(3);
            this.tabTable.Size = new System.Drawing.Size(396, 329);
            this.tabTable.TabIndex = 0;
            this.tabTable.Text = "表格";
            this.tabTable.UseVisualStyleBackColor = true;
            // 
            // tbxFixColLast
            // 
            this.tbxFixColLast.Location = new System.Drawing.Point(176, 155);
            this.tbxFixColLast.Name = "tbxFixColLast";
            this.tbxFixColLast.Size = new System.Drawing.Size(40, 21);
            this.tbxFixColLast.TabIndex = 23;
            this.tbxFixColLast.TextChanged += new System.EventHandler(this.tbxCellHPadding_TextChanged);
            // 
            // tbxFixColFirst
            // 
            this.tbxFixColFirst.Location = new System.Drawing.Point(86, 155);
            this.tbxFixColFirst.Name = "tbxFixColFirst";
            this.tbxFixColFirst.Size = new System.Drawing.Size(40, 21);
            this.tbxFixColFirst.TabIndex = 22;
            this.tbxFixColFirst.TextChanged += new System.EventHandler(this.tbxCellHPadding_TextChanged);
            // 
            // tbxFixRowLast
            // 
            this.tbxFixRowLast.Location = new System.Drawing.Point(176, 111);
            this.tbxFixRowLast.Name = "tbxFixRowLast";
            this.tbxFixRowLast.Size = new System.Drawing.Size(40, 21);
            this.tbxFixRowLast.TabIndex = 21;
            this.tbxFixRowLast.TextChanged += new System.EventHandler(this.tbxCellHPadding_TextChanged);
            // 
            // tbxFixRowFirst
            // 
            this.tbxFixRowFirst.Location = new System.Drawing.Point(86, 111);
            this.tbxFixRowFirst.Name = "tbxFixRowFirst";
            this.tbxFixRowFirst.Size = new System.Drawing.Size(40, 21);
            this.tbxFixRowFirst.TabIndex = 20;
            this.tbxFixRowFirst.TextChanged += new System.EventHandler(this.tbxCellHPadding_TextChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(222, 159);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(17, 12);
            this.label15.TabIndex = 19;
            this.label15.Text = "列";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(222, 115);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(17, 12);
            this.label14.TabIndex = 18;
            this.label14.Text = "行";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(132, 159);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(41, 12);
            this.label13.TabIndex = 17;
            this.label13.Text = "列到第";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(51, 159);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(29, 12);
            this.label12.TabIndex = 16;
            this.label12.Text = "从第";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(32, 137);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(41, 12);
            this.label11.TabIndex = 15;
            this.label11.Text = "固定列";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(132, 115);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(41, 12);
            this.label10.TabIndex = 14;
            this.label10.Text = "行到第";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(51, 115);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 12);
            this.label9.TabIndex = 13;
            this.label9.Text = "从第";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(30, 92);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 12);
            this.label8.TabIndex = 12;
            this.label8.Text = "固定行";
            // 
            // dgvTable
            // 
            this.dgvTable.AllowUserToResizeRows = false;
            this.dgvTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Key,
            this.value});
            this.dgvTable.Location = new System.Drawing.Point(30, 182);
            this.dgvTable.Name = "dgvTable";
            this.dgvTable.RowHeadersVisible = false;
            this.dgvTable.RowTemplate.Height = 23;
            this.dgvTable.Size = new System.Drawing.Size(339, 110);
            this.dgvTable.TabIndex = 11;
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
            // btnBorderBackColor
            // 
            this.btnBorderBackColor.Location = new System.Drawing.Point(272, 61);
            this.btnBorderBackColor.Name = "btnBorderBackColor";
            this.btnBorderBackColor.Size = new System.Drawing.Size(97, 23);
            this.btnBorderBackColor.TabIndex = 9;
            this.btnBorderBackColor.Text = "边框及背景色";
            this.btnBorderBackColor.UseVisualStyleBackColor = true;
            this.btnBorderBackColor.Click += new System.EventHandler(this.btnBorderBackColor_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(107, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "边框宽度(像素)";
            // 
            // tbxBorderWidth
            // 
            this.tbxBorderWidth.Location = new System.Drawing.Point(201, 61);
            this.tbxBorderWidth.Name = "tbxBorderWidth";
            this.tbxBorderWidth.Size = new System.Drawing.Size(65, 21);
            this.tbxBorderWidth.TabIndex = 7;
            this.tbxBorderWidth.TextChanged += new System.EventHandler(this.tbxCellHPadding_TextChanged);
            // 
            // tbxCellVPadding
            // 
            this.tbxCellVPadding.Location = new System.Drawing.Point(202, 29);
            this.tbxCellVPadding.Name = "tbxCellVPadding";
            this.tbxCellVPadding.Size = new System.Drawing.Size(65, 21);
            this.tbxCellVPadding.TabIndex = 6;
            this.tbxCellVPadding.TextChanged += new System.EventHandler(this.tbxCellHPadding_TextChanged);
            // 
            // tbxCellHPadding
            // 
            this.tbxCellHPadding.Location = new System.Drawing.Point(82, 29);
            this.tbxCellHPadding.Name = "tbxCellHPadding";
            this.tbxCellHPadding.Size = new System.Drawing.Size(65, 21);
            this.tbxCellHPadding.TabIndex = 5;
            this.tbxCellHPadding.TextChanged += new System.EventHandler(this.tbxCellHPadding_TextChanged);
            // 
            // cbxBorderVisible
            // 
            this.cbxBorderVisible.AutoSize = true;
            this.cbxBorderVisible.Location = new System.Drawing.Point(30, 65);
            this.cbxBorderVisible.Name = "cbxBorderVisible";
            this.cbxBorderVisible.Size = new System.Drawing.Size(72, 16);
            this.cbxBorderVisible.TabIndex = 4;
            this.cbxBorderVisible.Text = "显示边框";
            this.cbxBorderVisible.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(32, 295);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "第一列无值时不会存储";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(167, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "上下";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(49, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "左右";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "单元格边距";
            // 
            // tabRow
            // 
            this.tabRow.Controls.Add(this.tbxRowHeight);
            this.tabRow.Controls.Add(this.label6);
            this.tabRow.Location = new System.Drawing.Point(4, 22);
            this.tabRow.Name = "tabRow";
            this.tabRow.Padding = new System.Windows.Forms.Padding(3);
            this.tabRow.Size = new System.Drawing.Size(396, 329);
            this.tabRow.TabIndex = 1;
            this.tabRow.Text = "行(0)";
            this.tabRow.UseVisualStyleBackColor = true;
            // 
            // tbxRowHeight
            // 
            this.tbxRowHeight.Location = new System.Drawing.Point(100, 17);
            this.tbxRowHeight.Name = "tbxRowHeight";
            this.tbxRowHeight.Size = new System.Drawing.Size(100, 21);
            this.tbxRowHeight.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(31, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "行高(像素)";
            // 
            // tabCell
            // 
            this.tabCell.Controls.Add(this.cbbCellAlignVert);
            this.tabCell.Controls.Add(this.label16);
            this.tabCell.Location = new System.Drawing.Point(4, 22);
            this.tabCell.Name = "tabCell";
            this.tabCell.Padding = new System.Windows.Forms.Padding(3);
            this.tabCell.Size = new System.Drawing.Size(396, 329);
            this.tabCell.TabIndex = 2;
            this.tabCell.Text = "单元格";
            this.tabCell.UseVisualStyleBackColor = true;
            // 
            // cbbCellAlignVert
            // 
            this.cbbCellAlignVert.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbCellAlignVert.FormattingEnabled = true;
            this.cbbCellAlignVert.Items.AddRange(new object[] {
            "顶",
            "居中",
            "底"});
            this.cbbCellAlignVert.Location = new System.Drawing.Point(80, 16);
            this.cbbCellAlignVert.Name = "cbbCellAlignVert";
            this.cbbCellAlignVert.Size = new System.Drawing.Size(100, 20);
            this.cbbCellAlignVert.TabIndex = 5;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(21, 19);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(53, 12);
            this.label16.TabIndex = 4;
            this.label16.Text = "垂直对齐";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(154, 361);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // frmDeTableProperty
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 396);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.tabTableInfo);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDeTableProperty";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "表格属性";
            this.Load += new System.EventHandler(this.frmDeTableProperty_Load);
            this.tabTableInfo.ResumeLayout(false);
            this.tabTable.ResumeLayout(false);
            this.tabTable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTable)).EndInit();
            this.tabRow.ResumeLayout(false);
            this.tabRow.PerformLayout();
            this.tabCell.ResumeLayout(false);
            this.tabCell.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabTableInfo;
        private System.Windows.Forms.TabPage tabTable;
        private System.Windows.Forms.TabPage tabRow;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TabPage tabCell;
        private System.Windows.Forms.Button btnBorderBackColor;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbxBorderWidth;
        private System.Windows.Forms.TextBox tbxCellVPadding;
        private System.Windows.Forms.TextBox tbxCellHPadding;
        private System.Windows.Forms.CheckBox cbxBorderVisible;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgvTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn Key;
        private System.Windows.Forms.DataGridViewTextBoxColumn value;
        private System.Windows.Forms.TextBox tbxRowHeight;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbxFixColLast;
        private System.Windows.Forms.TextBox tbxFixColFirst;
        private System.Windows.Forms.TextBox tbxFixRowLast;
        private System.Windows.Forms.TextBox tbxFixRowFirst;
        private System.Windows.Forms.ComboBox cbbCellAlignVert;
        private System.Windows.Forms.Label label16;
    }
}