namespace EMRView
{
    partial class frmDomain
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
            this.dgvDomain = new System.Windows.Forms.DataGridView();
            this.Key = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pm = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniNew = new System.Windows.Forms.ToolStripMenuItem();
            this.mniEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mniDelete = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDomain)).BeginInit();
            this.pm.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvDomain
            // 
            this.dgvDomain.AllowUserToAddRows = false;
            this.dgvDomain.AllowUserToDeleteRows = false;
            this.dgvDomain.AllowUserToResizeRows = false;
            this.dgvDomain.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvDomain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDomain.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Key,
            this.Column1,
            this.value});
            this.dgvDomain.ContextMenuStrip = this.pm;
            this.dgvDomain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDomain.Location = new System.Drawing.Point(0, 0);
            this.dgvDomain.MultiSelect = false;
            this.dgvDomain.Name = "dgvDomain";
            this.dgvDomain.ReadOnly = true;
            this.dgvDomain.RowHeadersVisible = false;
            this.dgvDomain.RowTemplate.Height = 23;
            this.dgvDomain.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDomain.Size = new System.Drawing.Size(531, 293);
            this.dgvDomain.TabIndex = 2;
            // 
            // Key
            // 
            this.Key.HeaderText = "ID";
            this.Key.Name = "Key";
            this.Key.ReadOnly = true;
            this.Key.Width = 60;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "编码";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 80;
            // 
            // value
            // 
            this.value.HeaderText = "名称";
            this.value.Name = "value";
            this.value.ReadOnly = true;
            this.value.Width = 360;
            // 
            // pm
            // 
            this.pm.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniNew,
            this.mniEdit,
            this.mniDelete});
            this.pm.Name = "pm";
            this.pm.Size = new System.Drawing.Size(153, 92);
            // 
            // mniNew
            // 
            this.mniNew.Name = "mniNew";
            this.mniNew.Size = new System.Drawing.Size(152, 22);
            this.mniNew.Text = "添加";
            this.mniNew.Click += new System.EventHandler(this.mniNew_Click);
            // 
            // mniEdit
            // 
            this.mniEdit.Name = "mniEdit";
            this.mniEdit.Size = new System.Drawing.Size(152, 22);
            this.mniEdit.Text = "修改";
            this.mniEdit.Click += new System.EventHandler(this.mniEdit_Click);
            // 
            // mniDelete
            // 
            this.mniDelete.Name = "mniDelete";
            this.mniDelete.Size = new System.Drawing.Size(152, 22);
            this.mniDelete.Text = "删除";
            this.mniDelete.Click += new System.EventHandler(this.mniDelete_Click);
            // 
            // frmDomain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(531, 293);
            this.Controls.Add(this.dgvDomain);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDomain";
            this.Text = "值域";
            this.Load += new System.EventHandler(this.frmDomain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDomain)).EndInit();
            this.pm.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvDomain;
        private System.Windows.Forms.DataGridViewTextBoxColumn Key;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn value;
        private System.Windows.Forms.ContextMenuStrip pm;
        private System.Windows.Forms.ToolStripMenuItem mniNew;
        private System.Windows.Forms.ToolStripMenuItem mniEdit;
        private System.Windows.Forms.ToolStripMenuItem mniDelete;
    }
}