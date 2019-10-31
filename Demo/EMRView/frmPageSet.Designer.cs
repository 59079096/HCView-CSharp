namespace EMRView
{
    partial class frmPageSet
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
            this.cbxPaper = new System.Windows.Forms.ComboBox();
            this.cbxPaperOrientation = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.tbxWidth = new System.Windows.Forms.TextBox();
            this.tbxHeight = new System.Windows.Forms.TextBox();
            this.tbxTop = new System.Windows.Forms.TextBox();
            this.tbxLeft = new System.Windows.Forms.TextBox();
            this.tbxBottom = new System.Windows.Forms.TextBox();
            this.tbxRight = new System.Windows.Forms.TextBox();
            this.cbxSymmetryMargin = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.cbxPageNoVisible = new System.Windows.Forms.CheckBox();
            this.cbxParaLastMark = new System.Windows.Forms.CheckBox();
            this.cbxShowLineActiveMark = new System.Windows.Forms.CheckBox();
            this.cbxShowLineNo = new System.Windows.Forms.CheckBox();
            this.cbxShowUnderLine = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbxPaper
            // 
            this.cbxPaper.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxPaper.FormattingEnabled = true;
            this.cbxPaper.Location = new System.Drawing.Point(50, 15);
            this.cbxPaper.Name = "cbxPaper";
            this.cbxPaper.Size = new System.Drawing.Size(77, 20);
            this.cbxPaper.TabIndex = 2;
            this.cbxPaper.DropDownClosed += new System.EventHandler(this.cbxPaper_DropDownClosed);
            // 
            // cbxPaperOrientation
            // 
            this.cbxPaperOrientation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxPaperOrientation.FormattingEnabled = true;
            this.cbxPaperOrientation.Items.AddRange(new object[] {
            "纵向",
            "横向"});
            this.cbxPaperOrientation.Location = new System.Drawing.Point(52, 138);
            this.cbxPaperOrientation.Name = "cbxPaperOrientation";
            this.cbxPaperOrientation.Size = new System.Drawing.Size(77, 20);
            this.cbxPaperOrientation.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "纸张";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(162, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "宽(mm)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(260, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "高(mm)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "边距(mm)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(53, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "上";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(189, 80);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 12);
            this.label6.TabIndex = 9;
            this.label6.Text = "下";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(53, 108);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 12);
            this.label7.TabIndex = 10;
            this.label7.Text = "左";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(189, 108);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 12);
            this.label8.TabIndex = 11;
            this.label8.Text = "右";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(17, 143);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 12);
            this.label9.TabIndex = 12;
            this.label9.Text = "方向";
            // 
            // tbxWidth
            // 
            this.tbxWidth.Location = new System.Drawing.Point(204, 15);
            this.tbxWidth.Name = "tbxWidth";
            this.tbxWidth.Size = new System.Drawing.Size(50, 21);
            this.tbxWidth.TabIndex = 13;
            // 
            // tbxHeight
            // 
            this.tbxHeight.Location = new System.Drawing.Point(301, 14);
            this.tbxHeight.Name = "tbxHeight";
            this.tbxHeight.Size = new System.Drawing.Size(50, 21);
            this.tbxHeight.TabIndex = 14;
            // 
            // tbxTop
            // 
            this.tbxTop.Location = new System.Drawing.Point(73, 76);
            this.tbxTop.Name = "tbxTop";
            this.tbxTop.Size = new System.Drawing.Size(70, 21);
            this.tbxTop.TabIndex = 15;
            this.tbxTop.Text = "35";
            // 
            // tbxLeft
            // 
            this.tbxLeft.Location = new System.Drawing.Point(73, 104);
            this.tbxLeft.Name = "tbxLeft";
            this.tbxLeft.Size = new System.Drawing.Size(70, 21);
            this.tbxLeft.TabIndex = 16;
            this.tbxLeft.Text = "20";
            // 
            // tbxBottom
            // 
            this.tbxBottom.Location = new System.Drawing.Point(207, 76);
            this.tbxBottom.Name = "tbxBottom";
            this.tbxBottom.Size = new System.Drawing.Size(70, 21);
            this.tbxBottom.TabIndex = 17;
            this.tbxBottom.Text = "15";
            // 
            // tbxRight
            // 
            this.tbxRight.Location = new System.Drawing.Point(207, 104);
            this.tbxRight.Name = "tbxRight";
            this.tbxRight.Size = new System.Drawing.Size(70, 21);
            this.tbxRight.TabIndex = 18;
            this.tbxRight.Text = "15";
            // 
            // cbxSymmetryMargin
            // 
            this.cbxSymmetryMargin.AutoSize = true;
            this.cbxSymmetryMargin.Location = new System.Drawing.Point(76, 49);
            this.cbxSymmetryMargin.Name = "cbxSymmetryMargin";
            this.cbxSymmetryMargin.Size = new System.Drawing.Size(96, 16);
            this.cbxSymmetryMargin.TabIndex = 19;
            this.cbxSymmetryMargin.Text = "对称边距显示";
            this.cbxSymmetryMargin.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(142, 107);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(29, 12);
            this.label10.TabIndex = 20;
            this.label10.Text = "(mm)";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(276, 108);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(29, 12);
            this.label11.TabIndex = 21;
            this.label11.Text = "(mm)";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(142, 79);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(29, 12);
            this.label12.TabIndex = 22;
            this.label12.Text = "(mm)";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(276, 80);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(29, 12);
            this.label13.TabIndex = 23;
            this.label13.Text = "(mm)";
            // 
            // cbxPageNoVisible
            // 
            this.cbxPageNoVisible.AutoSize = true;
            this.cbxPageNoVisible.Location = new System.Drawing.Point(19, 173);
            this.cbxPageNoVisible.Name = "cbxPageNoVisible";
            this.cbxPageNoVisible.Size = new System.Drawing.Size(72, 16);
            this.cbxPageNoVisible.TabIndex = 24;
            this.cbxPageNoVisible.Text = "显示页码";
            this.cbxPageNoVisible.UseVisualStyleBackColor = true;
            // 
            // cbxParaLastMark
            // 
            this.cbxParaLastMark.AutoSize = true;
            this.cbxParaLastMark.Location = new System.Drawing.Point(125, 173);
            this.cbxParaLastMark.Name = "cbxParaLastMark";
            this.cbxParaLastMark.Size = new System.Drawing.Size(84, 16);
            this.cbxParaLastMark.TabIndex = 25;
            this.cbxParaLastMark.Text = "显示换行符";
            this.cbxParaLastMark.UseVisualStyleBackColor = true;
            // 
            // cbxShowLineActiveMark
            // 
            this.cbxShowLineActiveMark.AutoSize = true;
            this.cbxShowLineActiveMark.Location = new System.Drawing.Point(125, 195);
            this.cbxShowLineActiveMark.Name = "cbxShowLineActiveMark";
            this.cbxShowLineActiveMark.Size = new System.Drawing.Size(144, 16);
            this.cbxShowLineActiveMark.TabIndex = 26;
            this.cbxShowLineActiveMark.Text = "显示当前编辑行指示符";
            this.cbxShowLineActiveMark.UseVisualStyleBackColor = true;
            // 
            // cbxShowLineNo
            // 
            this.cbxShowLineNo.AutoSize = true;
            this.cbxShowLineNo.Location = new System.Drawing.Point(19, 195);
            this.cbxShowLineNo.Name = "cbxShowLineNo";
            this.cbxShowLineNo.Size = new System.Drawing.Size(72, 16);
            this.cbxShowLineNo.TabIndex = 27;
            this.cbxShowLineNo.Text = "显示行号";
            this.cbxShowLineNo.UseVisualStyleBackColor = true;
            // 
            // cbxShowUnderLine
            // 
            this.cbxShowUnderLine.AutoSize = true;
            this.cbxShowUnderLine.Location = new System.Drawing.Point(19, 217);
            this.cbxShowUnderLine.Name = "cbxShowUnderLine";
            this.cbxShowUnderLine.Size = new System.Drawing.Size(84, 16);
            this.cbxShowUnderLine.TabIndex = 28;
            this.cbxShowUnderLine.Text = "显示下划线";
            this.cbxShowUnderLine.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(144, 254);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 29;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // frmPageSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 289);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cbxShowUnderLine);
            this.Controls.Add(this.cbxShowLineNo);
            this.Controls.Add(this.cbxShowLineActiveMark);
            this.Controls.Add(this.cbxParaLastMark);
            this.Controls.Add(this.cbxPageNoVisible);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.cbxSymmetryMargin);
            this.Controls.Add(this.tbxRight);
            this.Controls.Add(this.tbxBottom);
            this.Controls.Add(this.tbxLeft);
            this.Controls.Add(this.tbxTop);
            this.Controls.Add(this.tbxHeight);
            this.Controls.Add(this.tbxWidth);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbxPaperOrientation);
            this.Controls.Add(this.cbxPaper);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPageSet";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "页面设置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxPaper;
        private System.Windows.Forms.ComboBox cbxPaperOrientation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbxWidth;
        private System.Windows.Forms.TextBox tbxHeight;
        private System.Windows.Forms.TextBox tbxTop;
        private System.Windows.Forms.TextBox tbxLeft;
        private System.Windows.Forms.TextBox tbxBottom;
        private System.Windows.Forms.TextBox tbxRight;
        private System.Windows.Forms.CheckBox cbxSymmetryMargin;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox cbxPageNoVisible;
        private System.Windows.Forms.CheckBox cbxParaLastMark;
        private System.Windows.Forms.CheckBox cbxShowLineActiveMark;
        private System.Windows.Forms.CheckBox cbxShowLineNo;
        private System.Windows.Forms.CheckBox cbxShowUnderLine;
        private System.Windows.Forms.Button btnOK;
    }
}