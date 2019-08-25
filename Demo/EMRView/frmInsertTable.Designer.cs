namespace EMRView
{
    partial class frmInsertTable
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbxRow = new System.Windows.Forms.TextBox();
            this.tbxCol = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "行数";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(50, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "列数";
            // 
            // tbxRow
            // 
            this.tbxRow.Location = new System.Drawing.Point(85, 26);
            this.tbxRow.Name = "tbxRow";
            this.tbxRow.Size = new System.Drawing.Size(100, 21);
            this.tbxRow.TabIndex = 2;
            this.tbxRow.Text = "2";
            // 
            // tbxCol
            // 
            this.tbxCol.Location = new System.Drawing.Point(85, 53);
            this.tbxCol.Name = "tbxCol";
            this.tbxCol.Size = new System.Drawing.Size(100, 21);
            this.tbxCol.TabIndex = 3;
            this.tbxCol.Text = "2";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(75, 96);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // frmInsertTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(234, 149);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.tbxCol);
            this.Controls.Add(this.tbxRow);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmInsertTable";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "表格信息";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbxRow;
        private System.Windows.Forms.TextBox tbxCol;
        private System.Windows.Forms.Button btnOK;
    }
}