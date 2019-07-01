namespace EMRView
{
    partial class frmParagraph
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
            this.cbxSpaceMode = new System.Windows.Forms.ComboBox();
            this.cbxAlignHorz = new System.Windows.Forms.ComboBox();
            this.cbbAlignVert = new System.Windows.Forms.ComboBox();
            this.tbxFirstIndent = new System.Windows.Forms.TextBox();
            this.tbxLeftIndent = new System.Windows.Forms.TextBox();
            this.tbxRightIndent = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSelectColor = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbxSpaceMode
            // 
            this.cbxSpaceMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSpaceMode.FormattingEnabled = true;
            this.cbxSpaceMode.Items.AddRange(new object[] {
            "单倍",
            "1.5倍",
            "2倍",
            "固定值"});
            this.cbxSpaceMode.Location = new System.Drawing.Point(84, 11);
            this.cbxSpaceMode.Name = "cbxSpaceMode";
            this.cbxSpaceMode.Size = new System.Drawing.Size(77, 20);
            this.cbxSpaceMode.TabIndex = 0;
            // 
            // cbxAlignHorz
            // 
            this.cbxAlignHorz.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAlignHorz.FormattingEnabled = true;
            this.cbxAlignHorz.Items.AddRange(new object[] {
            "左",
            "居中",
            "右",
            "两端",
            "分散"});
            this.cbxAlignHorz.Location = new System.Drawing.Point(84, 41);
            this.cbxAlignHorz.Name = "cbxAlignHorz";
            this.cbxAlignHorz.Size = new System.Drawing.Size(77, 20);
            this.cbxAlignHorz.TabIndex = 1;
            // 
            // cbbAlignVert
            // 
            this.cbbAlignVert.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbAlignVert.FormattingEnabled = true;
            this.cbbAlignVert.Items.AddRange(new object[] {
            "上",
            "居中",
            "下"});
            this.cbbAlignVert.Location = new System.Drawing.Point(232, 40);
            this.cbbAlignVert.Name = "cbbAlignVert";
            this.cbbAlignVert.Size = new System.Drawing.Size(77, 20);
            this.cbbAlignVert.TabIndex = 2;
            // 
            // tbxFirstIndent
            // 
            this.tbxFirstIndent.Location = new System.Drawing.Point(84, 73);
            this.tbxFirstIndent.Name = "tbxFirstIndent";
            this.tbxFirstIndent.Size = new System.Drawing.Size(40, 21);
            this.tbxFirstIndent.TabIndex = 3;
            this.tbxFirstIndent.Text = "8";
            // 
            // tbxLeftIndent
            // 
            this.tbxLeftIndent.Location = new System.Drawing.Point(84, 102);
            this.tbxLeftIndent.Name = "tbxLeftIndent";
            this.tbxLeftIndent.Size = new System.Drawing.Size(40, 21);
            this.tbxLeftIndent.TabIndex = 4;
            this.tbxLeftIndent.Text = "10";
            // 
            // tbxRightIndent
            // 
            this.tbxRightIndent.Location = new System.Drawing.Point(220, 102);
            this.tbxRightIndent.Name = "tbxRightIndent";
            this.tbxRightIndent.Size = new System.Drawing.Size(40, 21);
            this.tbxRightIndent.TabIndex = 5;
            this.tbxRightIndent.Text = "10";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "行间距";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(176, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "背景色";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "首行缩进";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(176, 107);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "右缩进";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(130, 105);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "毫米";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(130, 77);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 12;
            this.label7.Text = "毫米";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(266, 107);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 13;
            this.label8.Text = "毫米";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(39, 107);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(41, 12);
            this.label9.TabIndex = 14;
            this.label9.Text = "左缩进";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(27, 44);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(53, 12);
            this.label10.TabIndex = 15;
            this.label10.Text = "水平对齐";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(176, 44);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(53, 12);
            this.label11.TabIndex = 16;
            this.label11.Text = "垂直对齐";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(142, 142);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 17;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(232, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(35, 18);
            this.panel1.TabIndex = 18;
            // 
            // btnSelectColor
            // 
            this.btnSelectColor.Location = new System.Drawing.Point(268, 9);
            this.btnSelectColor.Name = "btnSelectColor";
            this.btnSelectColor.Size = new System.Drawing.Size(41, 23);
            this.btnSelectColor.TabIndex = 19;
            this.btnSelectColor.Text = "...";
            this.btnSelectColor.UseVisualStyleBackColor = true;
            // 
            // frmParagraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 181);
            this.Controls.Add(this.btnSelectColor);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbxRightIndent);
            this.Controls.Add(this.tbxLeftIndent);
            this.Controls.Add(this.tbxFirstIndent);
            this.Controls.Add(this.cbbAlignVert);
            this.Controls.Add(this.cbxAlignHorz);
            this.Controls.Add(this.cbxSpaceMode);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmParagraph";
            this.Text = "段落属性设置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxSpaceMode;
        private System.Windows.Forms.ComboBox cbxAlignHorz;
        private System.Windows.Forms.ComboBox cbbAlignVert;
        private System.Windows.Forms.TextBox tbxFirstIndent;
        private System.Windows.Forms.TextBox tbxLeftIndent;
        private System.Windows.Forms.TextBox tbxRightIndent;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnSelectColor;
    }
}