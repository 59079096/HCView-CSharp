namespace EMRView
{
    partial class frmDeInfo
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
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.cbbFrmtp = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnSaveClose = new System.Windows.Forms.Button();
            this.tbxName = new System.Windows.Forms.TextBox();
            this.tbxCode = new System.Windows.Forms.TextBox();
            this.tbxPY = new System.Windows.Forms.TextBox();
            this.tbxDefine = new System.Windows.Forms.TextBox();
            this.tbxType = new System.Windows.Forms.TextBox();
            this.tbxFormat = new System.Windows.Forms.TextBox();
            this.tbxUnit = new System.Windows.Forms.TextBox();
            this.tbxDomainID = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "名称";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(169, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "编码";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(329, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "拼音";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 42);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "定义";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 74);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "类型";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(171, 73);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 5;
            this.label6.Text = "格式";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(329, 74);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 6;
            this.label7.Text = "类别";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(17, 109);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 7;
            this.label8.Text = "单位";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(171, 109);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 12);
            this.label9.TabIndex = 8;
            this.label9.Text = "值域";
            // 
            // cbbFrmtp
            // 
            this.cbbFrmtp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbFrmtp.FormattingEnabled = true;
            this.cbbFrmtp.Items.AddRange(new object[] {
            "文本",
            "单选",
            "多选",
            "数值",
            "日期",
            "时间",
            "日期时间"});
            this.cbbFrmtp.Location = new System.Drawing.Point(367, 71);
            this.cbbFrmtp.Name = "cbbFrmtp";
            this.cbbFrmtp.Size = new System.Drawing.Size(98, 20);
            this.cbbFrmtp.TabIndex = 9;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(171, 150);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnSaveClose
            // 
            this.btnSaveClose.Location = new System.Drawing.Point(272, 150);
            this.btnSaveClose.Name = "btnSaveClose";
            this.btnSaveClose.Size = new System.Drawing.Size(75, 23);
            this.btnSaveClose.TabIndex = 11;
            this.btnSaveClose.Text = "保存并关闭";
            this.btnSaveClose.UseVisualStyleBackColor = true;
            this.btnSaveClose.Click += new System.EventHandler(this.btnSaveClose_Click);
            // 
            // tbxName
            // 
            this.tbxName.Location = new System.Drawing.Point(49, 9);
            this.tbxName.Name = "tbxName";
            this.tbxName.Size = new System.Drawing.Size(100, 21);
            this.tbxName.TabIndex = 12;
            // 
            // tbxCode
            // 
            this.tbxCode.Location = new System.Drawing.Point(205, 9);
            this.tbxCode.Name = "tbxCode";
            this.tbxCode.Size = new System.Drawing.Size(100, 21);
            this.tbxCode.TabIndex = 13;
            // 
            // tbxPY
            // 
            this.tbxPY.Location = new System.Drawing.Point(365, 9);
            this.tbxPY.Name = "tbxPY";
            this.tbxPY.Size = new System.Drawing.Size(100, 21);
            this.tbxPY.TabIndex = 14;
            // 
            // tbxDefine
            // 
            this.tbxDefine.Location = new System.Drawing.Point(49, 38);
            this.tbxDefine.Name = "tbxDefine";
            this.tbxDefine.Size = new System.Drawing.Size(416, 21);
            this.tbxDefine.TabIndex = 15;
            // 
            // tbxType
            // 
            this.tbxType.Location = new System.Drawing.Point(49, 70);
            this.tbxType.Name = "tbxType";
            this.tbxType.Size = new System.Drawing.Size(100, 21);
            this.tbxType.TabIndex = 16;
            // 
            // tbxFormat
            // 
            this.tbxFormat.Location = new System.Drawing.Point(205, 70);
            this.tbxFormat.Name = "tbxFormat";
            this.tbxFormat.Size = new System.Drawing.Size(100, 21);
            this.tbxFormat.TabIndex = 17;
            // 
            // tbxUnit
            // 
            this.tbxUnit.Location = new System.Drawing.Point(49, 105);
            this.tbxUnit.Name = "tbxUnit";
            this.tbxUnit.Size = new System.Drawing.Size(100, 21);
            this.tbxUnit.TabIndex = 18;
            // 
            // tbxDomainID
            // 
            this.tbxDomainID.Location = new System.Drawing.Point(205, 105);
            this.tbxDomainID.Name = "tbxDomainID";
            this.tbxDomainID.Size = new System.Drawing.Size(100, 21);
            this.tbxDomainID.TabIndex = 19;
            this.tbxDomainID.Text = "0";
            // 
            // frmDeInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 189);
            this.Controls.Add(this.tbxDomainID);
            this.Controls.Add(this.tbxUnit);
            this.Controls.Add(this.tbxFormat);
            this.Controls.Add(this.tbxType);
            this.Controls.Add(this.tbxDefine);
            this.Controls.Add(this.tbxPY);
            this.Controls.Add(this.tbxCode);
            this.Controls.Add(this.tbxName);
            this.Controls.Add(this.btnSaveClose);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.cbbFrmtp);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDeInfo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "数据元维护";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cbbFrmtp;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnSaveClose;
        private System.Windows.Forms.TextBox tbxName;
        private System.Windows.Forms.TextBox tbxCode;
        private System.Windows.Forms.TextBox tbxPY;
        private System.Windows.Forms.TextBox tbxDefine;
        private System.Windows.Forms.TextBox tbxType;
        private System.Windows.Forms.TextBox tbxFormat;
        private System.Windows.Forms.TextBox tbxUnit;
        private System.Windows.Forms.TextBox tbxDomainID;
    }
}