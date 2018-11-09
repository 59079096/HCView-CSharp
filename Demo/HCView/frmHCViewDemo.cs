using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HC.View;
using System.IO;

namespace HCViewDemo
{
    public partial class frmHCViewDemo : Form
    {
        HCView FHCView;

        public frmHCViewDemo()
        {
            InitializeComponent();
        }

        private void frmHCViewDemo_Load(object sender, EventArgs e)
        {
            this.Text = "HCViewDemo " + Application.ProductVersion.ToString();
            FHCView = new HCView();
            //FHCView.OnCaretChange := DoCaretChange;
            //FHCView.OnVerScroll := DoVerScroll;
            //FHCView.PopupMenu := pmRichEdit;
            this.Controls.Add(FHCView);
            FHCView.Dock = DockStyle.Fill;
            FHCView.BringToFront();
            //this.ResumeLayout(true);
        }

        private void mniOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog vOpenDlg = new OpenFileDialog();
            try
            {
                vOpenDlg.Filter = "文件|*" + HC.View.HC.HC_EXT;
                if (vOpenDlg.ShowDialog() == DialogResult.OK)
                {
                    if (vOpenDlg.FileName != "")
                    {
                        Application.DoEvents();
                        FHCView.LoadFromFile(vOpenDlg.FileName);
                    }
                }
            }
            finally
            {
                vOpenDlg.Dispose();
                GC.Collect();
            }
        }

        private void mniSave_Click(object sender, EventArgs e)
        {
            if (FHCView.FileName != "")
                FHCView.SaveToFile(FHCView.FileName);
            else
            {
                SaveFileDialog vDlg = new SaveFileDialog();
                try
                {
                    vDlg.Filter = "文件|*" + HC.View.HC.HC_EXT;
                    if (vDlg.ShowDialog() == DialogResult.OK)
                    {
                        if (vDlg.FileName != "")
                        {
                            if (System.IO.Path.GetExtension(vDlg.FileName) != HC.View.HC.HC_EXT)
                                vDlg.FileName += HC.View.HC.HC_EXT;

                            FHCView.SaveToFile(vDlg.FileName);
                        }
                    }
                }
                finally
                {
                    vDlg.Dispose();
                    GC.Collect();
                }
            }
        }

        private void mniSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog vDlg = new SaveFileDialog();
            try
            {
                vDlg.Filter = HC.View.HC.HC_EXT + "|*" + HC.View.HC.HC_EXT + "|pdf|*.pdf";
                if (vDlg.ShowDialog() == DialogResult.OK)
                {
                    if (vDlg.FileName != "")
                    {
                        string vExt = "";
                        switch (vDlg.FilterIndex)
                        {
                            case 1:
                                vExt = HC.View.HC.HC_EXT;
                                break;

                            case 2:
                                vExt = ".pdf";
                                break;

                            default:
                                return;
                        }

                        if (System.IO.Path.GetExtension(vDlg.FileName) != vExt)
                            vDlg.FileName = vDlg.FileName + vExt;

                        switch (vDlg.FilterIndex)
                        {
                            case 1:
                                FHCView.SaveToFile(vDlg.FileName);
                                FHCView.FileName = vDlg.FileName;
                                break;

                            case 2:
                                FHCView.SaveAsPDF(vDlg.FileName);
                                break;
                        }
                    }
                }
            }
            finally
            {
                vDlg.Dispose();
                GC.Collect();
            }
        }

        private void 表格ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (frmInsertTable vFrmInsertTable = new frmInsertTable())
            {
                vFrmInsertTable.ShowDialog();
                if (vFrmInsertTable.DialogResult == DialogResult.OK)
                    FHCView.InsertTable(int.Parse(vFrmInsertTable.txbRow.Text),
                        int.Parse(vFrmInsertTable.txbCol.Text));
            }
        }

        private void 剪切ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FHCView.Cut();
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FHCView.Copy();
        }

        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FHCView.Paste();
        }

        private void 选中ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FHCView.DeleteSelected();
        }

        private void 当前节ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FHCView.DeleteActiveSection();
        }

        private void frmHCViewDemo_Shown(object sender, EventArgs e)
        {
            FHCView.UpdateView();  // 解决窗体显示后，HCView没有显示重绘结果的问题
        }

        private void 图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog vOpenDlg = new OpenFileDialog())
            {
                vOpenDlg.Filter = "图像文件|*.bmp";
                if (vOpenDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (vOpenDlg.FileName != "")
                    {
                        HCImageItem vImageItem = new HCImageItem(FHCView.ActiveSectionTopLevelData());
                        vImageItem.LoadFromBmpFile(vOpenDlg.FileName);
                        Application.DoEvents();
                        FHCView.InsertItem(vImageItem);
                    }
                }
            }
        }

        private void 横线ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FHCView.InsertLine(1);
        }

        private void 分页符ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FHCView.InsertPageBreak();
        }

        private void 分节符ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FHCView.InsertSectionBreak();
        }

        private void 直线ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCFloatLineItem vFloatLineItem = new HCFloatLineItem(FHCView.ActiveSection.ActiveData);
            FHCView.InsertFloatItem(vFloatLineItem);
        }

        private void 文档ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog vOpenDlg = new OpenFileDialog())
            {
                vOpenDlg.Filter = "文件|*" + HC.View.HC.HC_EXT;
                if (vOpenDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (vOpenDlg.FileName != "")
                    {
                        using (FileStream vStream = new FileStream(vOpenDlg.FileName, FileMode.Open))
                        {
                            FHCView.InsertStream(vStream);
                        }
                    }
                }
            }
        }

        private void 文本ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FHCView.InsertText("这是InsertText插入的内容^_^");
        }

        private void 批注ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FHCView.ActiveSection.ActiveData.SelectExists())
                FHCView.InsertAnnotate("aaaa");
        }

        private void 一维码ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCBarCodeItem vHCBarCode = new HCBarCodeItem(FHCView.ActiveSectionTopLevelData(), "123");
            FHCView.InsertItem(vHCBarCode);
        }

        private void 二维码ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCQRCodeItem vQRCode = new HCQRCodeItem(FHCView.ActiveSectionTopLevelData(), "123");
            FHCView.InsertItem(vQRCode);
        }

        private void 直接打印ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (PrintDialog vPrintDlg = new PrintDialog())
            {
                vPrintDlg.PrinterSettings.MaximumPage = FHCView.PageCount;
                if (vPrintDlg.ShowDialog() == DialogResult.OK)
                    FHCView.Print(vPrintDlg.PrinterSettings.PrinterName);
            }
        }
    }
}
