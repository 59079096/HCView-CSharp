namespace EMRView
{
    partial class frmRecordSet
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbxPageBlankTip = new System.Windows.Forms.TextBox();
            this.tbxPageNo = new System.Windows.Forms.TextBox();
            this.tbxPageNoFmt = new System.Windows.Forms.TextBox();
            this.cbxPageBlankTip = new System.Windows.Forms.CheckBox();
            this.btnShow = new System.Windows.Forms.Button();
            this.cbxShowTrace = new System.Windows.Forms.CheckBox();
            this.dgvRecord = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.pnlRecord = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecord)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.dgvRecord);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(250, 535);
            this.panel1.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.tbxPageBlankTip);
            this.panel2.Controls.Add(this.tbxPageNo);
            this.panel2.Controls.Add(this.tbxPageNoFmt);
            this.panel2.Controls.Add(this.cbxPageBlankTip);
            this.panel2.Controls.Add(this.btnShow);
            this.panel2.Controls.Add(this.cbxShowTrace);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 335);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(250, 200);
            this.panel2.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 115);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 11;
            this.label2.Text = "页码格式";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 10;
            this.label1.Text = "起始页码";
            // 
            // tbxPageBlankTip
            // 
            this.tbxPageBlankTip.Location = new System.Drawing.Point(29, 57);
            this.tbxPageBlankTip.Name = "tbxPageBlankTip";
            this.tbxPageBlankTip.Size = new System.Drawing.Size(215, 21);
            this.tbxPageBlankTip.TabIndex = 9;
            this.tbxPageBlankTip.Text = "--------本页以下内容空白--------";
            this.tbxPageBlankTip.TextChanged += new System.EventHandler(this.TbxPageBlankTip_TextChanged);
            // 
            // tbxPageNo
            // 
            this.tbxPageNo.Location = new System.Drawing.Point(71, 84);
            this.tbxPageNo.Name = "tbxPageNo";
            this.tbxPageNo.Size = new System.Drawing.Size(40, 21);
            this.tbxPageNo.TabIndex = 8;
            this.tbxPageNo.Text = "1";
            // 
            // tbxPageNoFmt
            // 
            this.tbxPageNoFmt.Location = new System.Drawing.Point(71, 111);
            this.tbxPageNoFmt.Name = "tbxPageNoFmt";
            this.tbxPageNoFmt.Size = new System.Drawing.Size(100, 21);
            this.tbxPageNoFmt.TabIndex = 7;
            // 
            // cbxPageBlankTip
            // 
            this.cbxPageBlankTip.AutoSize = true;
            this.cbxPageBlankTip.Location = new System.Drawing.Point(12, 35);
            this.cbxPageBlankTip.Name = "cbxPageBlankTip";
            this.cbxPageBlankTip.Size = new System.Drawing.Size(180, 16);
            this.cbxPageBlankTip.TabIndex = 6;
            this.cbxPageBlankTip.Text = "另起页时上一页添加结束语句";
            this.cbxPageBlankTip.UseVisualStyleBackColor = true;
            this.cbxPageBlankTip.CheckedChanged += new System.EventHandler(this.CbxPageBlankTip_CheckedChanged);
            // 
            // btnShow
            // 
            this.btnShow.Location = new System.Drawing.Point(80, 151);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(75, 23);
            this.btnShow.TabIndex = 5;
            this.btnShow.Text = "显示";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Click += new System.EventHandler(this.BtnShow_Click);
            // 
            // cbxShowTrace
            // 
            this.cbxShowTrace.AutoSize = true;
            this.cbxShowTrace.Location = new System.Drawing.Point(12, 13);
            this.cbxShowTrace.Name = "cbxShowTrace";
            this.cbxShowTrace.Size = new System.Drawing.Size(72, 16);
            this.cbxShowTrace.TabIndex = 4;
            this.cbxShowTrace.Text = "显示痕迹";
            this.cbxShowTrace.UseVisualStyleBackColor = true;
            this.cbxShowTrace.CheckedChanged += new System.EventHandler(this.CbxShowTrace_CheckedChanged);
            // 
            // dgvRecord
            // 
            this.dgvRecord.AllowUserToAddRows = false;
            this.dgvRecord.AllowUserToResizeRows = false;
            this.dgvRecord.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRecord.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column5});
            this.dgvRecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRecord.Location = new System.Drawing.Point(0, 0);
            this.dgvRecord.MultiSelect = false;
            this.dgvRecord.Name = "dgvRecord";
            this.dgvRecord.RowHeadersVisible = false;
            this.dgvRecord.Size = new System.Drawing.Size(250, 535);
            this.dgvRecord.TabIndex = 1;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "选择";
            this.Column1.Name = "Column1";
            this.Column1.Width = 40;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "另起页";
            this.Column2.Name = "Column2";
            this.Column2.Width = 50;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "名称";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "时间";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 80;
            // 
            // Column5
            // 
            this.Column5.HeaderText = "ID";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            this.Column5.Width = 30;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(250, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 535);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // pnlRecord
            // 
            this.pnlRecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRecord.Location = new System.Drawing.Point(253, 0);
            this.pnlRecord.Name = "pnlRecord";
            this.pnlRecord.Size = new System.Drawing.Size(931, 535);
            this.pnlRecord.TabIndex = 3;
            // 
            // frmRecordSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 535);
            this.Controls.Add(this.pnlRecord);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.MinimizeBox = false;
            this.Name = "frmRecordSet";
            this.ShowInTaskbar = false;
            this.Text = "病历集合";
            this.Load += new System.EventHandler(this.FrmRecordSet_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecord)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dgvRecord;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbxPageBlankTip;
        private System.Windows.Forms.TextBox tbxPageNo;
        private System.Windows.Forms.TextBox tbxPageNoFmt;
        private System.Windows.Forms.CheckBox cbxPageBlankTip;
        private System.Windows.Forms.Button btnShow;
        private System.Windows.Forms.CheckBox cbxShowTrace;
        private System.Windows.Forms.Panel pnlRecord;
    }
}