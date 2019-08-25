namespace EMRView
{
    partial class frmDataElementDomain
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvCV = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pmCV = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniNewItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mniEditItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mniDeleteItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mniEditItemLink = new System.Windows.Forms.ToolStripMenuItem();
            this.mniDeleteItemLink = new System.Windows.Forms.ToolStripMenuItem();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblDE = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCV)).BeginInit();
            this.pmCV.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvCV
            // 
            this.dgvCV.AllowUserToAddRows = false;
            this.dgvCV.AllowUserToDeleteRows = false;
            this.dgvCV.AllowUserToResizeRows = false;
            this.dgvCV.BackgroundColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCV.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvCV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5});
            this.dgvCV.ContextMenuStrip = this.pmCV;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvCV.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvCV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCV.Location = new System.Drawing.Point(0, 30);
            this.dgvCV.MultiSelect = false;
            this.dgvCV.Name = "dgvCV";
            this.dgvCV.ReadOnly = true;
            this.dgvCV.RowHeadersVisible = false;
            this.dgvCV.RowTemplate.Height = 23;
            this.dgvCV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCV.Size = new System.Drawing.Size(360, 420);
            this.dgvCV.TabIndex = 8;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "值";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "编码";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Width = 40;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "拼音";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Width = 60;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "id";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.Width = 40;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "扩展";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            this.dataGridViewTextBoxColumn5.Width = 35;
            // 
            // pmCV
            // 
            this.pmCV.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniNewItem,
            this.mniEditItem,
            this.mniDeleteItem,
            this.toolStripSeparator3,
            this.mniEditItemLink,
            this.mniDeleteItemLink});
            this.pmCV.Name = "pmCV";
            this.pmCV.Size = new System.Drawing.Size(181, 142);
            this.pmCV.Opening += new System.ComponentModel.CancelEventHandler(this.PmCV_Opening);
            // 
            // mniNewItem
            // 
            this.mniNewItem.Name = "mniNewItem";
            this.mniNewItem.Size = new System.Drawing.Size(180, 22);
            this.mniNewItem.Text = "添加";
            this.mniNewItem.Click += new System.EventHandler(this.MniNewItem_Click);
            // 
            // mniEditItem
            // 
            this.mniEditItem.Name = "mniEditItem";
            this.mniEditItem.Size = new System.Drawing.Size(180, 22);
            this.mniEditItem.Text = "修改";
            this.mniEditItem.Click += new System.EventHandler(this.MniEditItem_Click);
            // 
            // mniDeleteItem
            // 
            this.mniDeleteItem.Name = "mniDeleteItem";
            this.mniDeleteItem.Size = new System.Drawing.Size(180, 22);
            this.mniDeleteItem.Text = "删除";
            this.mniDeleteItem.Click += new System.EventHandler(this.MniDeleteItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // mniEditItemLink
            // 
            this.mniEditItemLink.Name = "mniEditItemLink";
            this.mniEditItemLink.Size = new System.Drawing.Size(180, 22);
            this.mniEditItemLink.Text = "编辑扩展内容";
            this.mniEditItemLink.Click += new System.EventHandler(this.MniEditItemLink_Click);
            // 
            // mniDeleteItemLink
            // 
            this.mniDeleteItemLink.Name = "mniDeleteItemLink";
            this.mniDeleteItemLink.Size = new System.Drawing.Size(180, 22);
            this.mniDeleteItemLink.Text = "删除扩展内容";
            this.mniDeleteItemLink.Click += new System.EventHandler(this.MniDeleteItemLink_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.lblDE);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(360, 30);
            this.panel3.TabIndex = 7;
            // 
            // lblDE
            // 
            this.lblDE.AutoSize = true;
            this.lblDE.Location = new System.Drawing.Point(7, 8);
            this.lblDE.Name = "lblDE";
            this.lblDE.Size = new System.Drawing.Size(113, 12);
            this.lblDE.TabIndex = 0;
            this.lblDE.Text = "值域选项(点击刷新)";
            this.lblDE.Click += new System.EventHandler(this.LblDE_Click);
            // 
            // frmDataElementDomain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 450);
            this.Controls.Add(this.dgvCV);
            this.Controls.Add(this.panel3);
            this.Name = "frmDataElementDomain";
            this.Text = "frmDataElementDomain";
            ((System.ComponentModel.ISupportInitialize)(this.dgvCV)).EndInit();
            this.pmCV.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvCV;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lblDE;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.ContextMenuStrip pmCV;
        private System.Windows.Forms.ToolStripMenuItem mniNewItem;
        private System.Windows.Forms.ToolStripMenuItem mniEditItem;
        private System.Windows.Forms.ToolStripMenuItem mniDeleteItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mniEditItemLink;
        private System.Windows.Forms.ToolStripMenuItem mniDeleteItemLink;
    }
}