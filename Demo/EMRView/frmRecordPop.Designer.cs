namespace EMRView
{
    partial class frmRecordPop
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
            this.tabPop = new System.Windows.Forms.TabControl();
            this.tabDomain = new System.Windows.Forms.TabPage();
            this.dgvDomain = new System.Windows.Forms.DataGridView();
            this.Key = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnDomainOk = new System.Windows.Forms.Button();
            this.tbxSpliter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabNumber = new System.Windows.Forms.TabPage();
            this.tabQk = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btn42 = new System.Windows.Forms.Button();
            this.btn41 = new System.Windows.Forms.Button();
            this.btn39 = new System.Windows.Forms.Button();
            this.btn40 = new System.Windows.Forms.Button();
            this.btn38 = new System.Windows.Forms.Button();
            this.btn37 = new System.Windows.Forms.Button();
            this.btn35 = new System.Windows.Forms.Button();
            this.btn36 = new System.Windows.Forms.Button();
            this.btnMul = new System.Windows.Forms.Button();
            this.btnDiv = new System.Windows.Forms.Button();
            this.btn1 = new System.Windows.Forms.Button();
            this.btn3 = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDec = new System.Windows.Forms.Button();
            this.btnC = new System.Windows.Forms.Button();
            this.btn7 = new System.Windows.Forms.Button();
            this.btn9 = new System.Windows.Forms.Button();
            this.btnResult = new System.Windows.Forms.Button();
            this.btn6 = new System.Windows.Forms.Button();
            this.btnDot = new System.Windows.Forms.Button();
            this.btn8 = new System.Windows.Forms.Button();
            this.btn5 = new System.Windows.Forms.Button();
            this.btn0 = new System.Windows.Forms.Button();
            this.btn2 = new System.Windows.Forms.Button();
            this.btn4 = new System.Windows.Forms.Button();
            this.btnNumberOk = new System.Windows.Forms.Button();
            this.btnCE = new System.Windows.Forms.Button();
            this.cbxHideUnit = new System.Windows.Forms.CheckBox();
            this.cbbUnit = new System.Windows.Forms.ComboBox();
            this.tbxValue = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabMemo = new System.Windows.Forms.TabPage();
            this.tbxMemo = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnMemoOk = new System.Windows.Forms.Button();
            this.tabDateTime = new System.Windows.Forms.TabPage();
            this.pnlTime = new System.Windows.Forms.Panel();
            this.cbbTime = new System.Windows.Forms.ComboBox();
            this.dtpTime = new System.Windows.Forms.DateTimePicker();
            this.pnlDate = new System.Windows.Forms.Panel();
            this.cbbDate = new System.Windows.Forms.ComboBox();
            this.dtpDate = new System.Windows.Forms.DateTimePicker();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnDateTimeOk = new System.Windows.Forms.Button();
            this.btnNow = new System.Windows.Forms.Button();
            this.tabPop.SuspendLayout();
            this.tabDomain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDomain)).BeginInit();
            this.panel1.SuspendLayout();
            this.tabNumber.SuspendLayout();
            this.tabQk.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabMemo.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabDateTime.SuspendLayout();
            this.pnlTime.SuspendLayout();
            this.pnlDate.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPop
            // 
            this.tabPop.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.tabPop.Controls.Add(this.tabDomain);
            this.tabPop.Controls.Add(this.tabNumber);
            this.tabPop.Controls.Add(this.tabMemo);
            this.tabPop.Controls.Add(this.tabDateTime);
            this.tabPop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPop.Location = new System.Drawing.Point(0, 0);
            this.tabPop.Name = "tabPop";
            this.tabPop.SelectedIndex = 0;
            this.tabPop.Size = new System.Drawing.Size(259, 466);
            this.tabPop.TabIndex = 0;
            // 
            // tabDomain
            // 
            this.tabDomain.Controls.Add(this.dgvDomain);
            this.tabDomain.Controls.Add(this.panel1);
            this.tabDomain.Location = new System.Drawing.Point(4, 25);
            this.tabDomain.Name = "tabDomain";
            this.tabDomain.Padding = new System.Windows.Forms.Padding(3);
            this.tabDomain.Size = new System.Drawing.Size(251, 437);
            this.tabDomain.TabIndex = 0;
            this.tabDomain.Text = "tabDomain";
            this.tabDomain.UseVisualStyleBackColor = true;
            // 
            // dgvDomain
            // 
            this.dgvDomain.AllowUserToAddRows = false;
            this.dgvDomain.AllowUserToDeleteRows = false;
            this.dgvDomain.AllowUserToResizeRows = false;
            this.dgvDomain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDomain.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Key,
            this.value,
            this.Column1,
            this.Column2,
            this.Column3});
            this.dgvDomain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDomain.Location = new System.Drawing.Point(3, 30);
            this.dgvDomain.MultiSelect = false;
            this.dgvDomain.Name = "dgvDomain";
            this.dgvDomain.ReadOnly = true;
            this.dgvDomain.RowHeadersVisible = false;
            this.dgvDomain.RowTemplate.Height = 23;
            this.dgvDomain.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDomain.Size = new System.Drawing.Size(245, 404);
            this.dgvDomain.TabIndex = 2;
            this.dgvDomain.DoubleClick += new System.EventHandler(this.dgvDomain_DoubleClick);
            // 
            // Key
            // 
            this.Key.HeaderText = "值";
            this.Key.Name = "Key";
            this.Key.ReadOnly = true;
            // 
            // value
            // 
            this.value.HeaderText = "编码";
            this.value.Name = "value";
            this.value.ReadOnly = true;
            this.value.Width = 40;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "ID";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 30;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "拼音";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 30;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "扩展";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 30;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnDomainOk);
            this.panel1.Controls.Add(this.tbxSpliter);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(245, 27);
            this.panel1.TabIndex = 0;
            // 
            // btnDomainOk
            // 
            this.btnDomainOk.Location = new System.Drawing.Point(147, 2);
            this.btnDomainOk.Name = "btnDomainOk";
            this.btnDomainOk.Size = new System.Drawing.Size(75, 23);
            this.btnDomainOk.TabIndex = 2;
            this.btnDomainOk.Text = "确定";
            this.btnDomainOk.UseVisualStyleBackColor = true;
            this.btnDomainOk.Click += new System.EventHandler(this.btnDomainOk_Click);
            // 
            // tbxSpliter
            // 
            this.tbxSpliter.Location = new System.Drawing.Point(36, 3);
            this.tbxSpliter.Name = "tbxSpliter";
            this.tbxSpliter.Size = new System.Drawing.Size(100, 21);
            this.tbxSpliter.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "检索";
            // 
            // tabNumber
            // 
            this.tabNumber.Controls.Add(this.tabQk);
            this.tabNumber.Controls.Add(this.btnMul);
            this.tabNumber.Controls.Add(this.btnDiv);
            this.tabNumber.Controls.Add(this.btn1);
            this.tabNumber.Controls.Add(this.btn3);
            this.tabNumber.Controls.Add(this.btnAdd);
            this.tabNumber.Controls.Add(this.btnDec);
            this.tabNumber.Controls.Add(this.btnC);
            this.tabNumber.Controls.Add(this.btn7);
            this.tabNumber.Controls.Add(this.btn9);
            this.tabNumber.Controls.Add(this.btnResult);
            this.tabNumber.Controls.Add(this.btn6);
            this.tabNumber.Controls.Add(this.btnDot);
            this.tabNumber.Controls.Add(this.btn8);
            this.tabNumber.Controls.Add(this.btn5);
            this.tabNumber.Controls.Add(this.btn0);
            this.tabNumber.Controls.Add(this.btn2);
            this.tabNumber.Controls.Add(this.btn4);
            this.tabNumber.Controls.Add(this.btnNumberOk);
            this.tabNumber.Controls.Add(this.btnCE);
            this.tabNumber.Controls.Add(this.cbxHideUnit);
            this.tabNumber.Controls.Add(this.cbbUnit);
            this.tabNumber.Controls.Add(this.tbxValue);
            this.tabNumber.Controls.Add(this.label2);
            this.tabNumber.Location = new System.Drawing.Point(4, 25);
            this.tabNumber.Name = "tabNumber";
            this.tabNumber.Padding = new System.Windows.Forms.Padding(3);
            this.tabNumber.Size = new System.Drawing.Size(251, 437);
            this.tabNumber.TabIndex = 1;
            this.tabNumber.Text = "tabNumber";
            this.tabNumber.UseVisualStyleBackColor = true;
            // 
            // tabQk
            // 
            this.tabQk.Controls.Add(this.tabPage1);
            this.tabQk.Location = new System.Drawing.Point(2, 209);
            this.tabQk.Name = "tabQk";
            this.tabQk.SelectedIndex = 0;
            this.tabQk.Size = new System.Drawing.Size(180, 91);
            this.tabQk.TabIndex = 23;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btn42);
            this.tabPage1.Controls.Add(this.btn41);
            this.tabPage1.Controls.Add(this.btn39);
            this.tabPage1.Controls.Add(this.btn40);
            this.tabPage1.Controls.Add(this.btn38);
            this.tabPage1.Controls.Add(this.btn37);
            this.tabPage1.Controls.Add(this.btn35);
            this.tabPage1.Controls.Add(this.btn36);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(172, 65);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "体温";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btn42
            // 
            this.btn42.Location = new System.Drawing.Point(131, 32);
            this.btn42.Name = "btn42";
            this.btn42.Size = new System.Drawing.Size(38, 23);
            this.btn42.TabIndex = 15;
            this.btn42.Tag = "42";
            this.btn42.Text = "42.";
            this.btn42.UseVisualStyleBackColor = true;
            this.btn42.Click += new System.EventHandler(this.btn35_Click);
            // 
            // btn41
            // 
            this.btn41.Location = new System.Drawing.Point(87, 32);
            this.btn41.Name = "btn41";
            this.btn41.Size = new System.Drawing.Size(38, 23);
            this.btn41.TabIndex = 14;
            this.btn41.Tag = "41";
            this.btn41.Text = "41.";
            this.btn41.UseVisualStyleBackColor = true;
            this.btn41.Click += new System.EventHandler(this.btn35_Click);
            // 
            // btn39
            // 
            this.btn39.Location = new System.Drawing.Point(-1, 32);
            this.btn39.Name = "btn39";
            this.btn39.Size = new System.Drawing.Size(38, 23);
            this.btn39.TabIndex = 13;
            this.btn39.Tag = "39";
            this.btn39.Text = "39.";
            this.btn39.UseVisualStyleBackColor = true;
            this.btn39.Click += new System.EventHandler(this.btn35_Click);
            // 
            // btn40
            // 
            this.btn40.Location = new System.Drawing.Point(43, 32);
            this.btn40.Name = "btn40";
            this.btn40.Size = new System.Drawing.Size(38, 23);
            this.btn40.TabIndex = 12;
            this.btn40.Tag = "40";
            this.btn40.Text = "40.";
            this.btn40.UseVisualStyleBackColor = true;
            this.btn40.Click += new System.EventHandler(this.btn35_Click);
            // 
            // btn38
            // 
            this.btn38.Location = new System.Drawing.Point(131, 3);
            this.btn38.Name = "btn38";
            this.btn38.Size = new System.Drawing.Size(38, 23);
            this.btn38.TabIndex = 11;
            this.btn38.Tag = "38";
            this.btn38.Text = "38.";
            this.btn38.UseVisualStyleBackColor = true;
            this.btn38.Click += new System.EventHandler(this.btn35_Click);
            // 
            // btn37
            // 
            this.btn37.Location = new System.Drawing.Point(87, 3);
            this.btn37.Name = "btn37";
            this.btn37.Size = new System.Drawing.Size(38, 23);
            this.btn37.TabIndex = 10;
            this.btn37.Tag = "37";
            this.btn37.Text = "37.";
            this.btn37.UseVisualStyleBackColor = true;
            this.btn37.Click += new System.EventHandler(this.btn35_Click);
            // 
            // btn35
            // 
            this.btn35.Location = new System.Drawing.Point(-1, 3);
            this.btn35.Name = "btn35";
            this.btn35.Size = new System.Drawing.Size(38, 23);
            this.btn35.TabIndex = 9;
            this.btn35.Tag = "35";
            this.btn35.Text = "35.";
            this.btn35.UseVisualStyleBackColor = true;
            this.btn35.Click += new System.EventHandler(this.btn35_Click);
            // 
            // btn36
            // 
            this.btn36.Location = new System.Drawing.Point(43, 3);
            this.btn36.Name = "btn36";
            this.btn36.Size = new System.Drawing.Size(38, 23);
            this.btn36.TabIndex = 8;
            this.btn36.Tag = "36";
            this.btn36.Text = "36.";
            this.btn36.UseVisualStyleBackColor = true;
            this.btn36.Click += new System.EventHandler(this.btn35_Click);
            // 
            // btnMul
            // 
            this.btnMul.Location = new System.Drawing.Point(137, 146);
            this.btnMul.Name = "btnMul";
            this.btnMul.Size = new System.Drawing.Size(38, 23);
            this.btnMul.TabIndex = 21;
            this.btnMul.Text = "*";
            this.btnMul.UseVisualStyleBackColor = true;
            this.btnMul.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDiv
            // 
            this.btnDiv.Location = new System.Drawing.Point(137, 177);
            this.btnDiv.Name = "btnDiv";
            this.btnDiv.Size = new System.Drawing.Size(38, 23);
            this.btnDiv.TabIndex = 20;
            this.btnDiv.Text = "/";
            this.btnDiv.UseVisualStyleBackColor = true;
            this.btnDiv.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btn1
            // 
            this.btn1.Location = new System.Drawing.Point(5, 84);
            this.btn1.Name = "btn1";
            this.btn1.Size = new System.Drawing.Size(38, 23);
            this.btn1.TabIndex = 19;
            this.btn1.Tag = "1";
            this.btn1.Text = "1";
            this.btn1.UseVisualStyleBackColor = true;
            this.btn1.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btn3
            // 
            this.btn3.Location = new System.Drawing.Point(93, 84);
            this.btn3.Name = "btn3";
            this.btn3.Size = new System.Drawing.Size(38, 23);
            this.btn3.TabIndex = 18;
            this.btn3.Tag = "3";
            this.btn3.Text = "3";
            this.btn3.UseVisualStyleBackColor = true;
            this.btn3.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(137, 84);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(38, 23);
            this.btnAdd.TabIndex = 17;
            this.btnAdd.Text = "+";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDec
            // 
            this.btnDec.Location = new System.Drawing.Point(137, 115);
            this.btnDec.Name = "btnDec";
            this.btnDec.Size = new System.Drawing.Size(38, 23);
            this.btnDec.TabIndex = 16;
            this.btnDec.Text = "-";
            this.btnDec.UseVisualStyleBackColor = true;
            this.btnDec.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnC
            // 
            this.btnC.Location = new System.Drawing.Point(49, 53);
            this.btnC.Name = "btnC";
            this.btnC.Size = new System.Drawing.Size(38, 23);
            this.btnC.TabIndex = 15;
            this.btnC.Text = "C";
            this.btnC.UseVisualStyleBackColor = true;
            this.btnC.Click += new System.EventHandler(this.btnC_Click);
            // 
            // btn7
            // 
            this.btn7.Location = new System.Drawing.Point(5, 146);
            this.btn7.Name = "btn7";
            this.btn7.Size = new System.Drawing.Size(38, 23);
            this.btn7.TabIndex = 14;
            this.btn7.Tag = "7";
            this.btn7.Text = "7";
            this.btn7.UseVisualStyleBackColor = true;
            this.btn7.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btn9
            // 
            this.btn9.Location = new System.Drawing.Point(93, 146);
            this.btn9.Name = "btn9";
            this.btn9.Size = new System.Drawing.Size(38, 23);
            this.btn9.TabIndex = 13;
            this.btn9.Tag = "9";
            this.btn9.Text = "9";
            this.btn9.UseVisualStyleBackColor = true;
            this.btn9.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btnResult
            // 
            this.btnResult.Location = new System.Drawing.Point(93, 177);
            this.btnResult.Name = "btnResult";
            this.btnResult.Size = new System.Drawing.Size(38, 23);
            this.btnResult.TabIndex = 12;
            this.btnResult.Text = "=";
            this.btnResult.UseVisualStyleBackColor = true;
            this.btnResult.Click += new System.EventHandler(this.btnResult_Click);
            // 
            // btn6
            // 
            this.btn6.Location = new System.Drawing.Point(93, 115);
            this.btn6.Name = "btn6";
            this.btn6.Size = new System.Drawing.Size(38, 23);
            this.btn6.TabIndex = 11;
            this.btn6.Tag = "6";
            this.btn6.Text = "6";
            this.btn6.UseVisualStyleBackColor = true;
            this.btn6.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btnDot
            // 
            this.btnDot.Location = new System.Drawing.Point(49, 177);
            this.btnDot.Name = "btnDot";
            this.btnDot.Size = new System.Drawing.Size(38, 23);
            this.btnDot.TabIndex = 10;
            this.btnDot.Text = ".";
            this.btnDot.UseVisualStyleBackColor = true;
            this.btnDot.Click += new System.EventHandler(this.btnDot_Click);
            // 
            // btn8
            // 
            this.btn8.Location = new System.Drawing.Point(49, 146);
            this.btn8.Name = "btn8";
            this.btn8.Size = new System.Drawing.Size(38, 23);
            this.btn8.TabIndex = 9;
            this.btn8.Tag = "8";
            this.btn8.Text = "8";
            this.btn8.UseVisualStyleBackColor = true;
            this.btn8.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btn5
            // 
            this.btn5.Location = new System.Drawing.Point(49, 115);
            this.btn5.Name = "btn5";
            this.btn5.Size = new System.Drawing.Size(38, 23);
            this.btn5.TabIndex = 8;
            this.btn5.Tag = "5";
            this.btn5.Text = "5";
            this.btn5.UseVisualStyleBackColor = true;
            this.btn5.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btn0
            // 
            this.btn0.Location = new System.Drawing.Point(5, 177);
            this.btn0.Name = "btn0";
            this.btn0.Size = new System.Drawing.Size(38, 23);
            this.btn0.TabIndex = 7;
            this.btn0.Tag = "0";
            this.btn0.Text = "0";
            this.btn0.UseVisualStyleBackColor = true;
            this.btn0.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btn2
            // 
            this.btn2.Location = new System.Drawing.Point(49, 84);
            this.btn2.Name = "btn2";
            this.btn2.Size = new System.Drawing.Size(38, 23);
            this.btn2.TabIndex = 6;
            this.btn2.Tag = "2";
            this.btn2.Text = "2";
            this.btn2.UseVisualStyleBackColor = true;
            this.btn2.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btn4
            // 
            this.btn4.Location = new System.Drawing.Point(5, 115);
            this.btn4.Name = "btn4";
            this.btn4.Size = new System.Drawing.Size(38, 23);
            this.btn4.TabIndex = 5;
            this.btn4.Tag = "4";
            this.btn4.Text = "4";
            this.btn4.UseVisualStyleBackColor = true;
            this.btn4.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btnNumberOk
            // 
            this.btnNumberOk.Location = new System.Drawing.Point(93, 31);
            this.btnNumberOk.Name = "btnNumberOk";
            this.btnNumberOk.Size = new System.Drawing.Size(82, 47);
            this.btnNumberOk.TabIndex = 4;
            this.btnNumberOk.Text = "确定";
            this.btnNumberOk.UseVisualStyleBackColor = true;
            this.btnNumberOk.Click += new System.EventHandler(this.btnNumberOk_Click);
            // 
            // btnCE
            // 
            this.btnCE.Location = new System.Drawing.Point(5, 53);
            this.btnCE.Name = "btnCE";
            this.btnCE.Size = new System.Drawing.Size(38, 23);
            this.btnCE.TabIndex = 3;
            this.btnCE.Text = "CE";
            this.btnCE.UseVisualStyleBackColor = true;
            this.btnCE.Click += new System.EventHandler(this.btnCE_Click);
            // 
            // cbxHideUnit
            // 
            this.cbxHideUnit.AutoSize = true;
            this.cbxHideUnit.Location = new System.Drawing.Point(7, 32);
            this.cbxHideUnit.Name = "cbxHideUnit";
            this.cbxHideUnit.Size = new System.Drawing.Size(72, 16);
            this.cbxHideUnit.TabIndex = 2;
            this.cbxHideUnit.Text = "隐藏单位";
            this.cbxHideUnit.UseVisualStyleBackColor = true;
            // 
            // cbbUnit
            // 
            this.cbbUnit.FormattingEnabled = true;
            this.cbbUnit.Location = new System.Drawing.Point(102, 4);
            this.cbbUnit.Name = "cbbUnit";
            this.cbbUnit.Size = new System.Drawing.Size(58, 20);
            this.cbbUnit.TabIndex = 1;
            this.cbbUnit.DropDown += new System.EventHandler(this.CbbUnit_DropDown);
            this.cbbUnit.DropDownClosed += new System.EventHandler(this.CbbUnit_DropDownClosed);
            // 
            // tbxValue
            // 
            this.tbxValue.Location = new System.Drawing.Point(6, 4);
            this.tbxValue.Name = "tbxValue";
            this.tbxValue.Size = new System.Drawing.Size(90, 21);
            this.tbxValue.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 194);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 12);
            this.label2.TabIndex = 22;
            this.label2.Text = "____________________________";
            // 
            // tabMemo
            // 
            this.tabMemo.Controls.Add(this.tbxMemo);
            this.tabMemo.Controls.Add(this.panel2);
            this.tabMemo.Location = new System.Drawing.Point(4, 25);
            this.tabMemo.Name = "tabMemo";
            this.tabMemo.Padding = new System.Windows.Forms.Padding(3);
            this.tabMemo.Size = new System.Drawing.Size(251, 437);
            this.tabMemo.TabIndex = 2;
            this.tabMemo.Text = "tabMemo";
            this.tabMemo.UseVisualStyleBackColor = true;
            // 
            // tbxMemo
            // 
            this.tbxMemo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxMemo.Location = new System.Drawing.Point(3, 34);
            this.tbxMemo.Multiline = true;
            this.tbxMemo.Name = "tbxMemo";
            this.tbxMemo.Size = new System.Drawing.Size(245, 400);
            this.tbxMemo.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnMemoOk);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(245, 31);
            this.panel2.TabIndex = 0;
            // 
            // btnMemoOk
            // 
            this.btnMemoOk.Location = new System.Drawing.Point(122, 4);
            this.btnMemoOk.Name = "btnMemoOk";
            this.btnMemoOk.Size = new System.Drawing.Size(75, 23);
            this.btnMemoOk.TabIndex = 0;
            this.btnMemoOk.Text = "确定";
            this.btnMemoOk.UseVisualStyleBackColor = true;
            this.btnMemoOk.Click += new System.EventHandler(this.btnMemoOk_Click);
            // 
            // tabDateTime
            // 
            this.tabDateTime.Controls.Add(this.pnlTime);
            this.tabDateTime.Controls.Add(this.pnlDate);
            this.tabDateTime.Controls.Add(this.panel3);
            this.tabDateTime.Location = new System.Drawing.Point(4, 25);
            this.tabDateTime.Name = "tabDateTime";
            this.tabDateTime.Padding = new System.Windows.Forms.Padding(3);
            this.tabDateTime.Size = new System.Drawing.Size(251, 437);
            this.tabDateTime.TabIndex = 3;
            this.tabDateTime.Text = "tabDateTime";
            this.tabDateTime.UseVisualStyleBackColor = true;
            // 
            // pnlTime
            // 
            this.pnlTime.Controls.Add(this.cbbTime);
            this.pnlTime.Controls.Add(this.dtpTime);
            this.pnlTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTime.Location = new System.Drawing.Point(3, 81);
            this.pnlTime.Name = "pnlTime";
            this.pnlTime.Size = new System.Drawing.Size(245, 32);
            this.pnlTime.TabIndex = 8;
            // 
            // cbbTime
            // 
            this.cbbTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbTime.FormattingEnabled = true;
            this.cbbTime.Items.AddRange(new object[] {
            "H时m分",
            "H时m分s秒",
            "HH:mm:ss",
            "HH:mm",
            "H:m:s",
            "H:mm"});
            this.cbbTime.Location = new System.Drawing.Point(132, 7);
            this.cbbTime.Name = "cbbTime";
            this.cbbTime.Size = new System.Drawing.Size(82, 20);
            this.cbbTime.TabIndex = 7;
            // 
            // dtpTime
            // 
            this.dtpTime.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dtpTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpTime.Location = new System.Drawing.Point(42, 5);
            this.dtpTime.Name = "dtpTime";
            this.dtpTime.Size = new System.Drawing.Size(87, 23);
            this.dtpTime.TabIndex = 6;
            // 
            // pnlDate
            // 
            this.pnlDate.Controls.Add(this.cbbDate);
            this.pnlDate.Controls.Add(this.dtpDate);
            this.pnlDate.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDate.Location = new System.Drawing.Point(3, 50);
            this.pnlDate.Name = "pnlDate";
            this.pnlDate.Size = new System.Drawing.Size(245, 31);
            this.pnlDate.TabIndex = 7;
            // 
            // cbbDate
            // 
            this.cbbDate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbDate.FormattingEnabled = true;
            this.cbbDate.Items.AddRange(new object[] {
            "yyyy年M月d日",
            "yyyy年MM月dd日",
            "yyyy-M-d",
            "yyyy-MM-dd",
            "yyyy/M/d",
            "yyyy/MM/dd"});
            this.cbbDate.Location = new System.Drawing.Point(132, 4);
            this.cbbDate.Name = "cbbDate";
            this.cbbDate.Size = new System.Drawing.Size(97, 20);
            this.cbbDate.TabIndex = 5;
            // 
            // dtpDate
            // 
            this.dtpDate.CustomFormat = "yyyy-MM-dd";
            this.dtpDate.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dtpDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpDate.Location = new System.Drawing.Point(15, 4);
            this.dtpDate.Name = "dtpDate";
            this.dtpDate.Size = new System.Drawing.Size(114, 23);
            this.dtpDate.TabIndex = 4;
            this.dtpDate.Value = new System.DateTime(2019, 6, 22, 17, 43, 0, 0);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnDateTimeOk);
            this.panel3.Controls.Add(this.btnNow);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(245, 47);
            this.panel3.TabIndex = 6;
            // 
            // btnDateTimeOk
            // 
            this.btnDateTimeOk.Location = new System.Drawing.Point(108, 13);
            this.btnDateTimeOk.Name = "btnDateTimeOk";
            this.btnDateTimeOk.Size = new System.Drawing.Size(75, 23);
            this.btnDateTimeOk.TabIndex = 3;
            this.btnDateTimeOk.Text = "确定";
            this.btnDateTimeOk.UseVisualStyleBackColor = true;
            this.btnDateTimeOk.Click += new System.EventHandler(this.btnDateTimeOk_Click);
            // 
            // btnNow
            // 
            this.btnNow.Location = new System.Drawing.Point(14, 13);
            this.btnNow.Name = "btnNow";
            this.btnNow.Size = new System.Drawing.Size(75, 23);
            this.btnNow.TabIndex = 2;
            this.btnNow.Text = "当前时间";
            this.btnNow.UseVisualStyleBackColor = true;
            this.btnNow.Click += new System.EventHandler(this.btnNow_Click);
            // 
            // frmRecordPop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(259, 466);
            this.Controls.Add(this.tabPop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmRecordPop";
            this.Text = "frmRecordPop";
            this.Deactivate += new System.EventHandler(this.frmRecordPop_Deactivate);
            this.Load += new System.EventHandler(this.frmRecordPop_Load);
            this.Shown += new System.EventHandler(this.frmRecordPop_Shown);
            this.Leave += new System.EventHandler(this.frmRecordPop_Leave);
            this.tabPop.ResumeLayout(false);
            this.tabDomain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDomain)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabNumber.ResumeLayout(false);
            this.tabNumber.PerformLayout();
            this.tabQk.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabMemo.ResumeLayout(false);
            this.tabMemo.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tabDateTime.ResumeLayout(false);
            this.pnlTime.ResumeLayout(false);
            this.pnlDate.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabPop;
        private System.Windows.Forms.TabPage tabDomain;
        private System.Windows.Forms.TabPage tabNumber;
        private System.Windows.Forms.TabPage tabMemo;
        private System.Windows.Forms.TabPage tabDateTime;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDomainOk;
        private System.Windows.Forms.TextBox tbxSpliter;
        private System.Windows.Forms.DataGridView dgvDomain;
        private System.Windows.Forms.ComboBox cbbUnit;
        private System.Windows.Forms.TextBox tbxValue;
        private System.Windows.Forms.CheckBox cbxHideUnit;
        private System.Windows.Forms.Button btnMul;
        private System.Windows.Forms.Button btnDiv;
        private System.Windows.Forms.Button btn1;
        private System.Windows.Forms.Button btn3;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDec;
        private System.Windows.Forms.Button btnC;
        private System.Windows.Forms.Button btn7;
        private System.Windows.Forms.Button btn9;
        private System.Windows.Forms.Button btnResult;
        private System.Windows.Forms.Button btn6;
        private System.Windows.Forms.Button btnDot;
        private System.Windows.Forms.Button btn8;
        private System.Windows.Forms.Button btn5;
        private System.Windows.Forms.Button btn0;
        private System.Windows.Forms.Button btn2;
        private System.Windows.Forms.Button btn4;
        private System.Windows.Forms.Button btnNumberOk;
        private System.Windows.Forms.Button btnCE;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl tabQk;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btn38;
        private System.Windows.Forms.Button btn37;
        private System.Windows.Forms.Button btn35;
        private System.Windows.Forms.Button btn36;
        private System.Windows.Forms.Button btn42;
        private System.Windows.Forms.Button btn41;
        private System.Windows.Forms.Button btn39;
        private System.Windows.Forms.Button btn40;
        private System.Windows.Forms.TextBox tbxMemo;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnMemoOk;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnDateTimeOk;
        private System.Windows.Forms.Button btnNow;
        private System.Windows.Forms.Panel pnlTime;
        private System.Windows.Forms.ComboBox cbbTime;
        private System.Windows.Forms.DateTimePicker dtpTime;
        private System.Windows.Forms.Panel pnlDate;
        private System.Windows.Forms.ComboBox cbbDate;
        private System.Windows.Forms.DateTimePicker dtpDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn Key;
        private System.Windows.Forms.DataGridViewTextBoxColumn value;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
    }
}