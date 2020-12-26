namespace EMRView
{
    partial class frmDeProperty
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
            this.cbxCanCopy = new System.Windows.Forms.CheckBox();
            this.cbxCanEdit = new System.Windows.Forms.CheckBox();
            this.dgvProperty = new System.Windows.Forms.DataGridView();
            this.Key = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.cbxDeleteAllow = new System.Windows.Forms.CheckBox();
            this.cbxAllocOnly = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProperty)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbxAllocOnly);
            this.panel1.Controls.Add(this.cbxDeleteAllow);
            this.panel1.Controls.Add(this.cbxCanCopy);
            this.panel1.Controls.Add(this.cbxCanEdit);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(334, 48);
            this.panel1.TabIndex = 0;
            // 
            // cbxCanCopy
            // 
            this.cbxCanCopy.AutoSize = true;
            this.cbxCanCopy.Location = new System.Drawing.Point(13, 26);
            this.cbxCanCopy.Name = "cbxCanCopy";
            this.cbxCanCopy.Size = new System.Drawing.Size(72, 16);
            this.cbxCanCopy.TabIndex = 1;
            this.cbxCanCopy.Text = "允许复制";
            this.cbxCanCopy.UseVisualStyleBackColor = true;
            // 
            // cbxCanEdit
            // 
            this.cbxCanEdit.AutoSize = true;
            this.cbxCanEdit.Location = new System.Drawing.Point(13, 6);
            this.cbxCanEdit.Name = "cbxCanEdit";
            this.cbxCanEdit.Size = new System.Drawing.Size(96, 16);
            this.cbxCanEdit.TabIndex = 0;
            this.cbxCanEdit.Text = "允许修改内容";
            this.cbxCanEdit.UseVisualStyleBackColor = true;
            // 
            // dgvProperty
            // 
            this.dgvProperty.AllowUserToResizeRows = false;
            this.dgvProperty.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProperty.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Key,
            this.value});
            this.dgvProperty.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvProperty.Location = new System.Drawing.Point(0, 48);
            this.dgvProperty.Name = "dgvProperty";
            this.dgvProperty.RowHeadersVisible = false;
            this.dgvProperty.RowTemplate.Height = 23;
            this.dgvProperty.Size = new System.Drawing.Size(334, 193);
            this.dgvProperty.TabIndex = 1;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label1.Location = new System.Drawing.Point(11, 250);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "行中第一列为空则属性无效";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(127, 265);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "确  定";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // chkDeleteAllow
            // 
            this.cbxDeleteAllow.AutoSize = true;
            this.cbxDeleteAllow.Location = new System.Drawing.Point(115, 26);
            this.cbxDeleteAllow.Name = "chkDeleteAllow";
            this.cbxDeleteAllow.Size = new System.Drawing.Size(72, 16);
            this.cbxDeleteAllow.TabIndex = 2;
            this.cbxDeleteAllow.Text = "允许删除";
            this.cbxDeleteAllow.UseVisualStyleBackColor = true;
            // 
            // chkAllocOnly
            // 
            this.cbxAllocOnly.AutoSize = true;
            this.cbxAllocOnly.Location = new System.Drawing.Point(115, 6);
            this.cbxAllocOnly.Name = "chkAllocOnly";
            this.cbxAllocOnly.Size = new System.Drawing.Size(192, 16);
            this.cbxAllocOnly.TabIndex = 3;
            this.cbxAllocOnly.Text = "修改时整体赋值不允许部分修改";
            this.cbxAllocOnly.UseVisualStyleBackColor = true;
            // 
            // frmDeProperty
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 300);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgvProperty);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDeProperty";
            this.Text = "属性";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProperty)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cbxCanEdit;
        private System.Windows.Forms.DataGridView dgvProperty;
        private System.Windows.Forms.DataGridViewTextBoxColumn Key;
        private System.Windows.Forms.DataGridViewTextBoxColumn value;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox cbxCanCopy;
        private System.Windows.Forms.CheckBox cbxAllocOnly;
        private System.Windows.Forms.CheckBox cbxDeleteAllow;
    }
}