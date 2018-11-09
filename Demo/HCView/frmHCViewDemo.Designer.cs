namespace HCViewDemo
{
    partial class frmHCViewDemo
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mniOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.mniSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mniSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.页面设置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打印ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.预览ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.从当前行打印ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.当前页选中内容ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.编辑ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.剪切ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.复制ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.粘贴ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.选中ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.当前节ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查找ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.替换ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.插入ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.表格ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.图片ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gif动画ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.公式ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.分数分子分母ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.分数上下左右ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.上下标ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.横线ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.控件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.comboboxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dateTimePickerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.radioGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.分页符ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.分节符ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.文档ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.文本ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.批注ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.条码ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.一维码ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.二维码ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.形状ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.直线ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.直接打印ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.文件ToolStripMenuItem,
            this.编辑ToolStripMenuItem,
            this.插入ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(903, 25);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 文件ToolStripMenuItem
            // 
            this.文件ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniOpen,
            this.mniSave,
            this.mniSaveAs,
            this.页面设置ToolStripMenuItem,
            this.打印ToolStripMenuItem});
            this.文件ToolStripMenuItem.Name = "文件ToolStripMenuItem";
            this.文件ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.文件ToolStripMenuItem.Text = "文件";
            // 
            // mniOpen
            // 
            this.mniOpen.Name = "mniOpen";
            this.mniOpen.Size = new System.Drawing.Size(152, 22);
            this.mniOpen.Text = "打开";
            this.mniOpen.Click += new System.EventHandler(this.mniOpen_Click);
            // 
            // mniSave
            // 
            this.mniSave.Name = "mniSave";
            this.mniSave.Size = new System.Drawing.Size(152, 22);
            this.mniSave.Text = "保存";
            this.mniSave.Click += new System.EventHandler(this.mniSave_Click);
            // 
            // mniSaveAs
            // 
            this.mniSaveAs.Name = "mniSaveAs";
            this.mniSaveAs.Size = new System.Drawing.Size(152, 22);
            this.mniSaveAs.Text = "另存为";
            this.mniSaveAs.Click += new System.EventHandler(this.mniSaveAs_Click);
            // 
            // 页面设置ToolStripMenuItem
            // 
            this.页面设置ToolStripMenuItem.Name = "页面设置ToolStripMenuItem";
            this.页面设置ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.页面设置ToolStripMenuItem.Text = "页面设置";
            // 
            // 打印ToolStripMenuItem
            // 
            this.打印ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.直接打印ToolStripMenuItem,
            this.预览ToolStripMenuItem,
            this.从当前行打印ToolStripMenuItem,
            this.当前页选中内容ToolStripMenuItem});
            this.打印ToolStripMenuItem.Name = "打印ToolStripMenuItem";
            this.打印ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.打印ToolStripMenuItem.Text = "打印";
            // 
            // 预览ToolStripMenuItem
            // 
            this.预览ToolStripMenuItem.Name = "预览ToolStripMenuItem";
            this.预览ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.预览ToolStripMenuItem.Text = "预览";
            // 
            // 从当前行打印ToolStripMenuItem
            // 
            this.从当前行打印ToolStripMenuItem.Name = "从当前行打印ToolStripMenuItem";
            this.从当前行打印ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.从当前行打印ToolStripMenuItem.Text = "从当前行";
            // 
            // 当前页选中内容ToolStripMenuItem
            // 
            this.当前页选中内容ToolStripMenuItem.Name = "当前页选中内容ToolStripMenuItem";
            this.当前页选中内容ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.当前页选中内容ToolStripMenuItem.Text = "当前页选中内容";
            // 
            // 编辑ToolStripMenuItem
            // 
            this.编辑ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.剪切ToolStripMenuItem,
            this.复制ToolStripMenuItem,
            this.粘贴ToolStripMenuItem,
            this.删除ToolStripMenuItem,
            this.查找ToolStripMenuItem,
            this.替换ToolStripMenuItem});
            this.编辑ToolStripMenuItem.Name = "编辑ToolStripMenuItem";
            this.编辑ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.编辑ToolStripMenuItem.Text = "编辑";
            // 
            // 剪切ToolStripMenuItem
            // 
            this.剪切ToolStripMenuItem.Name = "剪切ToolStripMenuItem";
            this.剪切ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.剪切ToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.剪切ToolStripMenuItem.Text = "剪切";
            this.剪切ToolStripMenuItem.Click += new System.EventHandler(this.剪切ToolStripMenuItem_Click);
            // 
            // 复制ToolStripMenuItem
            // 
            this.复制ToolStripMenuItem.Name = "复制ToolStripMenuItem";
            this.复制ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.复制ToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.复制ToolStripMenuItem.Text = "复制";
            this.复制ToolStripMenuItem.Click += new System.EventHandler(this.复制ToolStripMenuItem_Click);
            // 
            // 粘贴ToolStripMenuItem
            // 
            this.粘贴ToolStripMenuItem.Name = "粘贴ToolStripMenuItem";
            this.粘贴ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.粘贴ToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.粘贴ToolStripMenuItem.Text = "粘贴";
            this.粘贴ToolStripMenuItem.Click += new System.EventHandler(this.粘贴ToolStripMenuItem_Click);
            // 
            // 删除ToolStripMenuItem
            // 
            this.删除ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.选中ToolStripMenuItem,
            this.当前节ToolStripMenuItem});
            this.删除ToolStripMenuItem.Name = "删除ToolStripMenuItem";
            this.删除ToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.删除ToolStripMenuItem.Text = "删除";
            // 
            // 选中ToolStripMenuItem
            // 
            this.选中ToolStripMenuItem.Name = "选中ToolStripMenuItem";
            this.选中ToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.选中ToolStripMenuItem.Text = "选中";
            this.选中ToolStripMenuItem.Click += new System.EventHandler(this.选中ToolStripMenuItem_Click);
            // 
            // 当前节ToolStripMenuItem
            // 
            this.当前节ToolStripMenuItem.Name = "当前节ToolStripMenuItem";
            this.当前节ToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.当前节ToolStripMenuItem.Text = "当前节";
            this.当前节ToolStripMenuItem.Click += new System.EventHandler(this.当前节ToolStripMenuItem_Click);
            // 
            // 查找ToolStripMenuItem
            // 
            this.查找ToolStripMenuItem.Name = "查找ToolStripMenuItem";
            this.查找ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.查找ToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.查找ToolStripMenuItem.Text = "查找";
            // 
            // 替换ToolStripMenuItem
            // 
            this.替换ToolStripMenuItem.Name = "替换ToolStripMenuItem";
            this.替换ToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.替换ToolStripMenuItem.Text = "替换";
            // 
            // 插入ToolStripMenuItem
            // 
            this.插入ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.表格ToolStripMenuItem,
            this.图片ToolStripMenuItem,
            this.gif动画ToolStripMenuItem,
            this.公式ToolStripMenuItem,
            this.横线ToolStripMenuItem,
            this.控件ToolStripMenuItem,
            this.分页符ToolStripMenuItem,
            this.分节符ToolStripMenuItem,
            this.文档ToolStripMenuItem,
            this.文本ToolStripMenuItem,
            this.批注ToolStripMenuItem,
            this.条码ToolStripMenuItem,
            this.形状ToolStripMenuItem});
            this.插入ToolStripMenuItem.Name = "插入ToolStripMenuItem";
            this.插入ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.插入ToolStripMenuItem.Text = "插入";
            // 
            // 表格ToolStripMenuItem
            // 
            this.表格ToolStripMenuItem.Name = "表格ToolStripMenuItem";
            this.表格ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.表格ToolStripMenuItem.Text = "表格";
            this.表格ToolStripMenuItem.Click += new System.EventHandler(this.表格ToolStripMenuItem_Click);
            // 
            // 图片ToolStripMenuItem
            // 
            this.图片ToolStripMenuItem.Name = "图片ToolStripMenuItem";
            this.图片ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.图片ToolStripMenuItem.Text = "图片";
            this.图片ToolStripMenuItem.Click += new System.EventHandler(this.图片ToolStripMenuItem_Click);
            // 
            // gif动画ToolStripMenuItem
            // 
            this.gif动画ToolStripMenuItem.Name = "gif动画ToolStripMenuItem";
            this.gif动画ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.gif动画ToolStripMenuItem.Text = "gif动画";
            // 
            // 公式ToolStripMenuItem
            // 
            this.公式ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.分数分子分母ToolStripMenuItem,
            this.分数上下左右ToolStripMenuItem,
            this.上下标ToolStripMenuItem});
            this.公式ToolStripMenuItem.Name = "公式ToolStripMenuItem";
            this.公式ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.公式ToolStripMenuItem.Text = "公式";
            // 
            // 分数分子分母ToolStripMenuItem
            // 
            this.分数分子分母ToolStripMenuItem.Name = "分数分子分母ToolStripMenuItem";
            this.分数分子分母ToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.分数分子分母ToolStripMenuItem.Text = "分数(分子/分母)";
            // 
            // 分数上下左右ToolStripMenuItem
            // 
            this.分数上下左右ToolStripMenuItem.Name = "分数上下左右ToolStripMenuItem";
            this.分数上下左右ToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.分数上下左右ToolStripMenuItem.Text = "分数(上下左右)";
            // 
            // 上下标ToolStripMenuItem
            // 
            this.上下标ToolStripMenuItem.Name = "上下标ToolStripMenuItem";
            this.上下标ToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.上下标ToolStripMenuItem.Text = "上下标";
            // 
            // 横线ToolStripMenuItem
            // 
            this.横线ToolStripMenuItem.Name = "横线ToolStripMenuItem";
            this.横线ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.横线ToolStripMenuItem.Text = "横线";
            this.横线ToolStripMenuItem.Click += new System.EventHandler(this.横线ToolStripMenuItem_Click);
            // 
            // 控件ToolStripMenuItem
            // 
            this.控件ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkBoxToolStripMenuItem,
            this.editToolStripMenuItem,
            this.comboboxToolStripMenuItem,
            this.dateTimePickerToolStripMenuItem,
            this.radioGroupToolStripMenuItem});
            this.控件ToolStripMenuItem.Name = "控件ToolStripMenuItem";
            this.控件ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.控件ToolStripMenuItem.Text = "控件";
            // 
            // checkBoxToolStripMenuItem
            // 
            this.checkBoxToolStripMenuItem.Name = "checkBoxToolStripMenuItem";
            this.checkBoxToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.checkBoxToolStripMenuItem.Text = "CheckBox";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // comboboxToolStripMenuItem
            // 
            this.comboboxToolStripMenuItem.Name = "comboboxToolStripMenuItem";
            this.comboboxToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.comboboxToolStripMenuItem.Text = "Combobox";
            // 
            // dateTimePickerToolStripMenuItem
            // 
            this.dateTimePickerToolStripMenuItem.Name = "dateTimePickerToolStripMenuItem";
            this.dateTimePickerToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.dateTimePickerToolStripMenuItem.Text = "DateTimePicker";
            // 
            // radioGroupToolStripMenuItem
            // 
            this.radioGroupToolStripMenuItem.Name = "radioGroupToolStripMenuItem";
            this.radioGroupToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.radioGroupToolStripMenuItem.Text = "RadioGroup";
            // 
            // 分页符ToolStripMenuItem
            // 
            this.分页符ToolStripMenuItem.Name = "分页符ToolStripMenuItem";
            this.分页符ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.分页符ToolStripMenuItem.Text = "分页符";
            this.分页符ToolStripMenuItem.Click += new System.EventHandler(this.分页符ToolStripMenuItem_Click);
            // 
            // 分节符ToolStripMenuItem
            // 
            this.分节符ToolStripMenuItem.Name = "分节符ToolStripMenuItem";
            this.分节符ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.分节符ToolStripMenuItem.Text = "分节符";
            this.分节符ToolStripMenuItem.Click += new System.EventHandler(this.分节符ToolStripMenuItem_Click);
            // 
            // 文档ToolStripMenuItem
            // 
            this.文档ToolStripMenuItem.Name = "文档ToolStripMenuItem";
            this.文档ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.文档ToolStripMenuItem.Text = "文档";
            this.文档ToolStripMenuItem.Click += new System.EventHandler(this.文档ToolStripMenuItem_Click);
            // 
            // 文本ToolStripMenuItem
            // 
            this.文本ToolStripMenuItem.Name = "文本ToolStripMenuItem";
            this.文本ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.文本ToolStripMenuItem.Text = "文本";
            this.文本ToolStripMenuItem.Click += new System.EventHandler(this.文本ToolStripMenuItem_Click);
            // 
            // 批注ToolStripMenuItem
            // 
            this.批注ToolStripMenuItem.Name = "批注ToolStripMenuItem";
            this.批注ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.批注ToolStripMenuItem.Text = "批注";
            this.批注ToolStripMenuItem.Click += new System.EventHandler(this.批注ToolStripMenuItem_Click);
            // 
            // 条码ToolStripMenuItem
            // 
            this.条码ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.一维码ToolStripMenuItem,
            this.二维码ToolStripMenuItem});
            this.条码ToolStripMenuItem.Name = "条码ToolStripMenuItem";
            this.条码ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.条码ToolStripMenuItem.Text = "条码";
            // 
            // 一维码ToolStripMenuItem
            // 
            this.一维码ToolStripMenuItem.Name = "一维码ToolStripMenuItem";
            this.一维码ToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.一维码ToolStripMenuItem.Text = "一维码";
            this.一维码ToolStripMenuItem.Click += new System.EventHandler(this.一维码ToolStripMenuItem_Click);
            // 
            // 二维码ToolStripMenuItem
            // 
            this.二维码ToolStripMenuItem.Name = "二维码ToolStripMenuItem";
            this.二维码ToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.二维码ToolStripMenuItem.Text = "二维码";
            this.二维码ToolStripMenuItem.Click += new System.EventHandler(this.二维码ToolStripMenuItem_Click);
            // 
            // 形状ToolStripMenuItem
            // 
            this.形状ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.直线ToolStripMenuItem});
            this.形状ToolStripMenuItem.Name = "形状ToolStripMenuItem";
            this.形状ToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.形状ToolStripMenuItem.Text = "形状";
            // 
            // 直线ToolStripMenuItem
            // 
            this.直线ToolStripMenuItem.Name = "直线ToolStripMenuItem";
            this.直线ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.直线ToolStripMenuItem.Text = "直线";
            this.直线ToolStripMenuItem.Click += new System.EventHandler(this.直线ToolStripMenuItem_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(452, 2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 21);
            this.textBox1.TabIndex = 2;
            // 
            // 直接打印ToolStripMenuItem
            // 
            this.直接打印ToolStripMenuItem.Name = "直接打印ToolStripMenuItem";
            this.直接打印ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.直接打印ToolStripMenuItem.Text = "直接打印";
            this.直接打印ToolStripMenuItem.Click += new System.EventHandler(this.直接打印ToolStripMenuItem_Click);
            // 
            // frmHCViewDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(903, 514);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmHCViewDemo";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.frmHCViewDemo_Load);
            this.Shown += new System.EventHandler(this.frmHCViewDemo_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 编辑ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 插入ToolStripMenuItem;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolStripMenuItem mniOpen;
        private System.Windows.Forms.ToolStripMenuItem mniSave;
        private System.Windows.Forms.ToolStripMenuItem mniSaveAs;
        private System.Windows.Forms.ToolStripMenuItem 页面设置ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 打印ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 预览ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 从当前行打印ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 当前页选中内容ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 剪切ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 复制ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 粘贴ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查找ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 替换ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 表格ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 图片ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gif动画ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 公式ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 分数分子分母ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 分数上下左右ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 上下标ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 横线ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 控件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkBoxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem comboboxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dateTimePickerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem radioGroupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 分页符ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 分节符ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 文档ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 文本ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 批注ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 条码ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 一维码ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 二维码ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 形状ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 直线ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 选中ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 当前节ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 直接打印ToolStripMenuItem;
    }
}

