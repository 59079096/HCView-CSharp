namespace EMRView
{
    partial class frmTableBorderBackColor
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
            this.label3 = new System.Windows.Forms.Label();
            this.cbxLeft = new System.Windows.Forms.CheckBox();
            this.cbxTop = new System.Windows.Forms.CheckBox();
            this.cbxRight = new System.Windows.Forms.CheckBox();
            this.cbxLTRB = new System.Windows.Forms.CheckBox();
            this.cbxBottom = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.cbbRang = new System.Windows.Forms.ComboBox();
            this.cbxRTLB = new System.Windows.Forms.CheckBox();
            this.btnSelectColor = new System.Windows.Forms.Button();
            this.pnlBackColor = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "应用于";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(55, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "边框";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(55, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "背景色";
            // 
            // cbxLeft
            // 
            this.cbxLeft.AutoSize = true;
            this.cbxLeft.Location = new System.Drawing.Point(90, 44);
            this.cbxLeft.Name = "cbxLeft";
            this.cbxLeft.Size = new System.Drawing.Size(36, 16);
            this.cbxLeft.TabIndex = 3;
            this.cbxLeft.Text = "左";
            this.cbxLeft.UseVisualStyleBackColor = true;
            // 
            // cbxTop
            // 
            this.cbxTop.AutoSize = true;
            this.cbxTop.Location = new System.Drawing.Point(135, 44);
            this.cbxTop.Name = "cbxTop";
            this.cbxTop.Size = new System.Drawing.Size(36, 16);
            this.cbxTop.TabIndex = 4;
            this.cbxTop.Text = "上";
            this.cbxTop.UseVisualStyleBackColor = true;
            // 
            // cbxRight
            // 
            this.cbxRight.AutoSize = true;
            this.cbxRight.Location = new System.Drawing.Point(180, 43);
            this.cbxRight.Name = "cbxRight";
            this.cbxRight.Size = new System.Drawing.Size(36, 16);
            this.cbxRight.TabIndex = 5;
            this.cbxRight.Text = "右";
            this.cbxRight.UseVisualStyleBackColor = true;
            // 
            // cbxLTRB
            // 
            this.cbxLTRB.AutoSize = true;
            this.cbxLTRB.Location = new System.Drawing.Point(90, 72);
            this.cbxLTRB.Name = "cbxLTRB";
            this.cbxLTRB.Size = new System.Drawing.Size(102, 16);
            this.cbxLTRB.TabIndex = 6;
            this.cbxLTRB.Text = "左上-右下斜线";
            this.cbxLTRB.UseVisualStyleBackColor = true;
            // 
            // cbxBottom
            // 
            this.cbxBottom.AutoSize = true;
            this.cbxBottom.Location = new System.Drawing.Point(228, 44);
            this.cbxBottom.Name = "cbxBottom";
            this.cbxBottom.Size = new System.Drawing.Size(36, 16);
            this.cbxBottom.TabIndex = 7;
            this.cbxBottom.Text = "下";
            this.cbxBottom.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(125, 145);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 9;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // cbbRang
            // 
            this.cbbRang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbRang.FormattingEnabled = true;
            this.cbbRang.Items.AddRange(new object[] {
            "单元格",
            "整个表格"});
            this.cbbRang.Location = new System.Drawing.Point(57, 10);
            this.cbbRang.Name = "cbbRang";
            this.cbbRang.Size = new System.Drawing.Size(207, 20);
            this.cbbRang.TabIndex = 10;
            // 
            // cbxRTLB
            // 
            this.cbxRTLB.AutoSize = true;
            this.cbxRTLB.Location = new System.Drawing.Point(200, 72);
            this.cbxRTLB.Name = "cbxRTLB";
            this.cbxRTLB.Size = new System.Drawing.Size(102, 16);
            this.cbxRTLB.TabIndex = 11;
            this.cbxRTLB.Text = "右上-左下斜线";
            this.cbxRTLB.UseVisualStyleBackColor = true;
            // 
            // btnSelectColor
            // 
            this.btnSelectColor.Location = new System.Drawing.Point(135, 102);
            this.btnSelectColor.Name = "btnSelectColor";
            this.btnSelectColor.Size = new System.Drawing.Size(41, 23);
            this.btnSelectColor.TabIndex = 21;
            this.btnSelectColor.Text = "...";
            this.btnSelectColor.UseVisualStyleBackColor = true;
            this.btnSelectColor.Click += new System.EventHandler(this.btnSelectColor_Click);
            // 
            // pnlBackColor
            // 
            this.pnlBackColor.BackColor = System.Drawing.SystemColors.Control;
            this.pnlBackColor.Location = new System.Drawing.Point(99, 105);
            this.pnlBackColor.Name = "pnlBackColor";
            this.pnlBackColor.Size = new System.Drawing.Size(35, 18);
            this.pnlBackColor.TabIndex = 20;
            // 
            // frmTableBorderBackColor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(314, 192);
            this.Controls.Add(this.btnSelectColor);
            this.Controls.Add(this.pnlBackColor);
            this.Controls.Add(this.cbxRTLB);
            this.Controls.Add(this.cbbRang);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cbxBottom);
            this.Controls.Add(this.cbxLTRB);
            this.Controls.Add(this.cbxRight);
            this.Controls.Add(this.cbxTop);
            this.Controls.Add(this.cbxLeft);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmTableBorderBackColor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "边框及背景色";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbxLeft;
        private System.Windows.Forms.CheckBox cbxTop;
        private System.Windows.Forms.CheckBox cbxRight;
        private System.Windows.Forms.CheckBox cbxLTRB;
        private System.Windows.Forms.CheckBox cbxBottom;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ComboBox cbbRang;
        private System.Windows.Forms.CheckBox cbxRTLB;
        private System.Windows.Forms.Button btnSelectColor;
        private System.Windows.Forms.Panel pnlBackColor;
    }
}