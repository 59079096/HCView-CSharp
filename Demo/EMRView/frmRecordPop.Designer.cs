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
            this.button24 = new System.Windows.Forms.Button();
            this.button25 = new System.Windows.Forms.Button();
            this.button26 = new System.Windows.Forms.Button();
            this.button27 = new System.Windows.Forms.Button();
            this.button23 = new System.Windows.Forms.Button();
            this.button22 = new System.Windows.Forms.Button();
            this.button21 = new System.Windows.Forms.Button();
            this.button20 = new System.Windows.Forms.Button();
            this.button19 = new System.Windows.Forms.Button();
            this.button18 = new System.Windows.Forms.Button();
            this.button17 = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.btnNumberOk = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
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
            this.tabNumber.Controls.Add(this.button19);
            this.tabNumber.Controls.Add(this.button18);
            this.tabNumber.Controls.Add(this.button17);
            this.tabNumber.Controls.Add(this.button16);
            this.tabNumber.Controls.Add(this.button15);
            this.tabNumber.Controls.Add(this.button14);
            this.tabNumber.Controls.Add(this.button13);
            this.tabNumber.Controls.Add(this.button12);
            this.tabNumber.Controls.Add(this.button11);
            this.tabNumber.Controls.Add(this.button10);
            this.tabNumber.Controls.Add(this.button9);
            this.tabNumber.Controls.Add(this.button8);
            this.tabNumber.Controls.Add(this.button7);
            this.tabNumber.Controls.Add(this.button6);
            this.tabNumber.Controls.Add(this.button5);
            this.tabNumber.Controls.Add(this.button4);
            this.tabNumber.Controls.Add(this.button3);
            this.tabNumber.Controls.Add(this.btnNumberOk);
            this.tabNumber.Controls.Add(this.button1);
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
            this.tabPage1.Controls.Add(this.button24);
            this.tabPage1.Controls.Add(this.button25);
            this.tabPage1.Controls.Add(this.button26);
            this.tabPage1.Controls.Add(this.button27);
            this.tabPage1.Controls.Add(this.button23);
            this.tabPage1.Controls.Add(this.button22);
            this.tabPage1.Controls.Add(this.button21);
            this.tabPage1.Controls.Add(this.button20);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(172, 65);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "体温";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // button24
            // 
            this.button24.Location = new System.Drawing.Point(131, 32);
            this.button24.Name = "button24";
            this.button24.Size = new System.Drawing.Size(38, 23);
            this.button24.TabIndex = 15;
            this.button24.Text = "42.";
            this.button24.UseVisualStyleBackColor = true;
            // 
            // button25
            // 
            this.button25.Location = new System.Drawing.Point(87, 32);
            this.button25.Name = "button25";
            this.button25.Size = new System.Drawing.Size(38, 23);
            this.button25.TabIndex = 14;
            this.button25.Text = "41.";
            this.button25.UseVisualStyleBackColor = true;
            // 
            // button26
            // 
            this.button26.Location = new System.Drawing.Point(-1, 32);
            this.button26.Name = "button26";
            this.button26.Size = new System.Drawing.Size(38, 23);
            this.button26.TabIndex = 13;
            this.button26.Text = "39.";
            this.button26.UseVisualStyleBackColor = true;
            // 
            // button27
            // 
            this.button27.Location = new System.Drawing.Point(43, 32);
            this.button27.Name = "button27";
            this.button27.Size = new System.Drawing.Size(38, 23);
            this.button27.TabIndex = 12;
            this.button27.Text = "40.";
            this.button27.UseVisualStyleBackColor = true;
            // 
            // button23
            // 
            this.button23.Location = new System.Drawing.Point(131, 3);
            this.button23.Name = "button23";
            this.button23.Size = new System.Drawing.Size(38, 23);
            this.button23.TabIndex = 11;
            this.button23.Text = "38.";
            this.button23.UseVisualStyleBackColor = true;
            // 
            // button22
            // 
            this.button22.Location = new System.Drawing.Point(87, 3);
            this.button22.Name = "button22";
            this.button22.Size = new System.Drawing.Size(38, 23);
            this.button22.TabIndex = 10;
            this.button22.Text = "37.";
            this.button22.UseVisualStyleBackColor = true;
            // 
            // button21
            // 
            this.button21.Location = new System.Drawing.Point(-1, 3);
            this.button21.Name = "button21";
            this.button21.Size = new System.Drawing.Size(38, 23);
            this.button21.TabIndex = 9;
            this.button21.Text = "35.";
            this.button21.UseVisualStyleBackColor = true;
            // 
            // button20
            // 
            this.button20.Location = new System.Drawing.Point(43, 3);
            this.button20.Name = "button20";
            this.button20.Size = new System.Drawing.Size(38, 23);
            this.button20.TabIndex = 8;
            this.button20.Text = "36.";
            this.button20.UseVisualStyleBackColor = true;
            // 
            // button19
            // 
            this.button19.Location = new System.Drawing.Point(137, 146);
            this.button19.Name = "button19";
            this.button19.Size = new System.Drawing.Size(38, 23);
            this.button19.TabIndex = 21;
            this.button19.Text = "*";
            this.button19.UseVisualStyleBackColor = true;
            // 
            // button18
            // 
            this.button18.Location = new System.Drawing.Point(137, 177);
            this.button18.Name = "button18";
            this.button18.Size = new System.Drawing.Size(38, 23);
            this.button18.TabIndex = 20;
            this.button18.Text = "/";
            this.button18.UseVisualStyleBackColor = true;
            // 
            // button17
            // 
            this.button17.Location = new System.Drawing.Point(5, 84);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(38, 23);
            this.button17.TabIndex = 19;
            this.button17.Text = "1";
            this.button17.UseVisualStyleBackColor = true;
            // 
            // button16
            // 
            this.button16.Location = new System.Drawing.Point(93, 84);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(38, 23);
            this.button16.TabIndex = 18;
            this.button16.Text = "3";
            this.button16.UseVisualStyleBackColor = true;
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(137, 84);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(38, 23);
            this.button15.TabIndex = 17;
            this.button15.Text = "+";
            this.button15.UseVisualStyleBackColor = true;
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(137, 115);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(38, 23);
            this.button14.TabIndex = 16;
            this.button14.Text = "-";
            this.button14.UseVisualStyleBackColor = true;
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(49, 53);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(38, 23);
            this.button13.TabIndex = 15;
            this.button13.Text = "C";
            this.button13.UseVisualStyleBackColor = true;
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(5, 146);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(38, 23);
            this.button12.TabIndex = 14;
            this.button12.Text = "7";
            this.button12.UseVisualStyleBackColor = true;
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(93, 146);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(38, 23);
            this.button11.TabIndex = 13;
            this.button11.Text = "9";
            this.button11.UseVisualStyleBackColor = true;
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(93, 177);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(38, 23);
            this.button10.TabIndex = 12;
            this.button10.Text = "=";
            this.button10.UseVisualStyleBackColor = true;
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(93, 115);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(38, 23);
            this.button9.TabIndex = 11;
            this.button9.Text = "6";
            this.button9.UseVisualStyleBackColor = true;
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(49, 177);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(38, 23);
            this.button8.TabIndex = 10;
            this.button8.Text = ".";
            this.button8.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(49, 146);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(38, 23);
            this.button7.TabIndex = 9;
            this.button7.Text = "8";
            this.button7.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(49, 115);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(38, 23);
            this.button6.TabIndex = 8;
            this.button6.Text = "5";
            this.button6.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(5, 177);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(38, 23);
            this.button5.TabIndex = 7;
            this.button5.Text = "0";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(49, 84);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(38, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "2";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(5, 115);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(38, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "4";
            this.button3.UseVisualStyleBackColor = true;
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
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(5, 53);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(38, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "CE";
            this.button1.UseVisualStyleBackColor = true;
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
        private System.Windows.Forms.Button button19;
        private System.Windows.Forms.Button button18;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button btnNumberOk;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl tabQk;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button button23;
        private System.Windows.Forms.Button button22;
        private System.Windows.Forms.Button button21;
        private System.Windows.Forms.Button button20;
        private System.Windows.Forms.Button button24;
        private System.Windows.Forms.Button button25;
        private System.Windows.Forms.Button button26;
        private System.Windows.Forms.Button button27;
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