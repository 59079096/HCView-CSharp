namespace EMRView
{
    partial class frmTemplateList
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
            this.btnOK = new System.Windows.Forms.Button();
            this.dtpDT = new System.Windows.Forms.DateTimePicker();
            this.tbxRecordName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvTemplate = new System.Windows.Forms.TreeView();
            this.dgvTempList = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTempList)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Controls.Add(this.dtpDT);
            this.panel1.Controls.Add(this.tbxRecordName);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 359);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(665, 80);
            this.panel1.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(383, 41);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // dtpDT
            // 
            this.dtpDT.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dtpDT.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpDT.Location = new System.Drawing.Point(225, 43);
            this.dtpDT.Name = "dtpDT";
            this.dtpDT.Size = new System.Drawing.Size(152, 21);
            this.dtpDT.TabIndex = 3;
            // 
            // tbxRecordName
            // 
            this.tbxRecordName.Location = new System.Drawing.Point(225, 13);
            this.tbxRecordName.Name = "tbxRecordName";
            this.tbxRecordName.Size = new System.Drawing.Size(233, 21);
            this.tbxRecordName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(166, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "病历时间";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(166, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "病历名称";
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
            this.splitContainer1.Panel2.Controls.Add(this.dgvTempList);
            this.splitContainer1.Size = new System.Drawing.Size(665, 359);
            this.splitContainer1.SplitterDistance = 221;
            this.splitContainer1.TabIndex = 1;
            // 
            // tvTemplate
            // 
            this.tvTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvTemplate.Location = new System.Drawing.Point(0, 0);
            this.tvTemplate.Name = "tvTemplate";
            this.tvTemplate.Size = new System.Drawing.Size(221, 359);
            this.tvTemplate.TabIndex = 1;
            this.tvTemplate.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvTemplate_AfterSelect);
            // 
            // dgvTempList
            // 
            this.dgvTempList.AllowUserToAddRows = false;
            this.dgvTempList.AllowUserToDeleteRows = false;
            this.dgvTempList.AllowUserToResizeRows = false;
            this.dgvTempList.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvTempList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTempList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4});
            this.dgvTempList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTempList.Location = new System.Drawing.Point(0, 0);
            this.dgvTempList.MultiSelect = false;
            this.dgvTempList.Name = "dgvTempList";
            this.dgvTempList.ReadOnly = true;
            this.dgvTempList.RowHeadersVisible = false;
            this.dgvTempList.RowTemplate.Height = 23;
            this.dgvTempList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTempList.Size = new System.Drawing.Size(440, 359);
            this.dgvTempList.TabIndex = 0;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "名称";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 120;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "所属";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 40;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "所属ID";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 70;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "ID";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 40;
            // 
            // frmTemplateList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(665, 439);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmTemplateList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "选择模板";
            this.Load += new System.EventHandler(this.frmTemplateList_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTempList)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgvTempList;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DateTimePicker dtpDT;
        private System.Windows.Forms.TextBox tbxRecordName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TreeView tvTemplate;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
    }
}