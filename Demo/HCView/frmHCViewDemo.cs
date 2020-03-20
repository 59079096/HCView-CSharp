using HC.View;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace HCViewDemo
{
    public partial class frmHCViewDemo : Form
    {
        HCView FHCView;

        public frmHCViewDemo()
        {
            InitializeComponent();
        }

        private void GetPagesAndActive()
        {
            tssPage.Text = "预览" + (FHCView.PagePreviewFirst + 1).ToString()
                + "页 光标" + (FHCView.ActivePageIndex + 1).ToString()
                + "页 共" + FHCView.PageCount.ToString() + "页";
        }

        private void CurTextStyleChange(int aNewStyleNo)
        {
            if (aNewStyleNo >= 0)
            {
                cbbFont.SelectedIndex = cbbFont.Items.IndexOf(FHCView.Style.TextStyles[aNewStyleNo].Family);
                cbbFontSize.SelectedIndex = cbbFontSize.Items.IndexOf(HC.View.HC.GetFontSizeStr(FHCView.Style.TextStyles[aNewStyleNo].Size));
                btnBold.Checked = FHCView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsBold);
                btnItalic.Checked = FHCView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsItalic);
                btnUnderLine.Checked = FHCView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsUnderline);
                btnStrikeOut.Checked = FHCView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsStrikeOut);
                btnSuperScript.Checked = FHCView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsSuperscript);
                btnSubScript.Checked = FHCView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsSubscript);
            }
            else
            {
                btnBold.Checked = false;
                btnItalic.Checked = false;
                btnUnderLine.Checked = false;
                btnStrikeOut.Checked = false;
                btnSuperScript.Checked = false;
                btnSubScript.Checked = false;
            }
        }

        private void CurParaStyleChange(int aNewParaNo)
        {
            if (aNewParaNo >= 0)
            {
                ParaAlignHorz vAlignHorz = FHCView.Style.ParaStyles[aNewParaNo].AlignHorz;

                btnAlignLeft.Checked = vAlignHorz == ParaAlignHorz.pahLeft;
                btnAlignRight.Checked = vAlignHorz == ParaAlignHorz.pahRight;
                btnAlignCenter.Checked = vAlignHorz == ParaAlignHorz.pahCenter;
                btnAlignJustify.Checked = vAlignHorz == ParaAlignHorz.pahJustify;
                btnAlignScatter.Checked = vAlignHorz == ParaAlignHorz.pahScatter;
            }
        }

        private void DoCaretChange(object sender, EventArgs e)
        {
            GetPagesAndActive();

            CurTextStyleChange(FHCView.CurStyleNo);
            CurParaStyleChange(FHCView.CurParaNo);
        }

        private void DoZoomChanged(object sender, EventArgs e)
        {
            string vZoom = Math.Round(FHCView.Zoom * 100).ToString();
            int vIndex = cbbZoom.Items.IndexOf(vZoom);
            if (vIndex < 0)
            {
                cbbZoom.Items[cbbZoom.Items.Count - 1] = vZoom;
                vIndex = cbbZoom.Items.Count - 1;
            }

            cbbZoom.SelectedIndex = vIndex;
        }

        private void DoVerScroll(object sender, EventArgs e)
        {
            GetPagesAndActive();
        }

        private void frmHCViewDemo_Load(object sender, EventArgs e)
        {
            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily family in fonts.Families)
            {
                cbbFont.Items.Add(family.Name);
            }
            cbbFont.Text = "宋体";
            cbbFontSize.Text = "五号";
            cbbZoom.SelectedIndex = 3;

            this.Text = "HCViewDemo " + Application.ProductVersion.ToString();
            FHCView = new HCView();
            FHCView.OnCaretChange = DoCaretChange;
            FHCView.OnZoomChanged = DoZoomChanged;
            FHCView.OnVerScroll = DoVerScroll;
            FHCView.ContextMenuStrip = pmHCView;
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
                                //FHCView.SaveAsPDF(vDlg.FileName);
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
                vOpenDlg.Filter = "图像文件|*.bmp; *.jpg; *.jpeg; *.png|Windows Bitmap|*.bmp|JPEG 文件|*.jpg; *.jpge|可移植网络图形|*.png";
                if (vOpenDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (vOpenDlg.FileName != "")
                    {
                        HCRichData vTopData = FHCView.ActiveSectionTopLevelData() as HCRichData;
                        HCImageItem vImageItem = new HCImageItem(vTopData);
                        vImageItem.LoadFromBmpFile(vOpenDlg.FileName);
                        vImageItem.RestrainSize(vTopData.Width, vImageItem.Height);
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
                FHCView.InsertAnnotate("title", "text");
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

        private void comboboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCComboboxItem vCombobox = new HCComboboxItem(FHCView.ActiveSectionTopLevelData(), "默认值");
            HCCbbItem vItem = new HCCbbItem("选项1");
            vCombobox.Items.Add(vItem);
            vItem = new HCCbbItem("选项2");
            vCombobox.Items.Add(vItem);
            vItem = new HCCbbItem("选项3");
            vCombobox.Items.Add(vItem);
            vItem = new HCCbbItem("选项4");
            vCombobox.Items.Add(vItem);
            vItem = new HCCbbItem("选项5");
            vCombobox.Items.Add(vItem);
            vItem = new HCCbbItem("选项6");
            vCombobox.Items.Add(vItem);
            //vCombobox.OnPopupItem = DoComboboxPopupItem;
            //vCombobox.ItemIndex := 0;
            FHCView.InsertItem(vCombobox);
        }

        private void 分数分子分母ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCFractionItem vFractionItem = new HCFractionItem(FHCView.ActiveSectionTopLevelData(), "12", "2018");
            FHCView.InsertItem(vFractionItem);
        }

        private void 分数上下左右ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCExpressItem vExpressItem = new HCExpressItem(FHCView.ActiveSectionTopLevelData(),
                "12", "5-6", "2017-6-3", "28-30");
            FHCView.InsertItem(vExpressItem);
        }

        private void 上下标ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCSupSubScriptItem vSupSubScriptItem = new HCSupSubScriptItem(FHCView.ActiveSectionTopLevelData(), "20g", "先煎");
            FHCView.InsertItem(vSupSubScriptItem);
        }

        private void checkBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCCheckBoxItem vCheckBox = new HCCheckBoxItem(FHCView.ActiveSectionTopLevelData(), "勾选框", false);
            FHCView.InsertItem(vCheckBox);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCEditItem vEdit = new HCEditItem(FHCView.ActiveSectionTopLevelData(), "文本框");
            FHCView.InsertItem(vEdit);
        }

        private void dateTimePickerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCDateTimePicker vHCDateTimePicker = new HCDateTimePicker(FHCView.ActiveSectionTopLevelData(), DateTime.Now);
            FHCView.InsertItem(vHCDateTimePicker);
        }

        private void radioGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCRadioGroup vHCRadioGroup = new HCRadioGroup(FHCView.ActiveSectionTopLevelData());
            vHCRadioGroup.AddItem("选项1");
            vHCRadioGroup.AddItem("选项2");
            vHCRadioGroup.AddItem("选项3");
            FHCView.InsertItem(vHCRadioGroup);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            FHCView.Undo();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            FHCView.Redo();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            mniOpen_Click(sender, e);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            FHCView.FileName = "";
            FHCView.Clear();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            mniSave_Click(sender, e);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            直接打印ToolStripMenuItem_Click(sender, e);
        }

        private void btnSymmetryMargin_Click(object sender, EventArgs e)
        {
            FHCView.SymmetryMargin = !FHCView.SymmetryMargin;
        }

        private void btnBold_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripButton).Tag.ToString())
            {
                case "0":
                    FHCView.ApplyTextStyle(HCFontStyle.tsBold);
                    break;

                case "1":
                    FHCView.ApplyTextStyle(HCFontStyle.tsItalic);
                    break;

                case "2":
                    FHCView.ApplyTextStyle(HCFontStyle.tsUnderline);
                    break;

                case "3":
                    FHCView.ApplyTextStyle(HCFontStyle.tsStrikeOut);
                    break;

                case "4":
                    FHCView.ApplyTextStyle(HCFontStyle.tsSuperscript);
                    break;

                case "5":
                    FHCView.ApplyTextStyle(HCFontStyle.tsSubscript);
                    break;
            }
        }

        private void btnAlignLeft_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripButton).Tag.ToString())
            {
                case "0":
                    FHCView.ApplyParaAlignHorz(ParaAlignHorz.pahLeft);
                    break;

                case "1":
                    FHCView.ApplyParaAlignHorz(ParaAlignHorz.pahCenter);
                    break;

                case "2":
                    FHCView.ApplyParaAlignHorz(ParaAlignHorz.pahRight);
                    break;

                case "3":
                    FHCView.ApplyParaAlignHorz(ParaAlignHorz.pahJustify);  // 两端
                    break;

                case "4":
                    FHCView.ApplyParaAlignHorz(ParaAlignHorz.pahScatter);  // 分散
                    break;

                case "5":
                    FHCView.ApplyParaLeftIndent();
                    break;

                case "6":
                    FHCView.ApplyParaLeftIndent(false);
                    break;
            }
        }

        private void mniLS100_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripMenuItem).Tag.ToString())
            {
                case "0":
                    FHCView.ApplyParaLineSpace(ParaLineSpaceMode.pls100);   // 单倍
                    break;

                case "1":
                    FHCView.ApplyParaLineSpace(ParaLineSpaceMode.pls115);  // 1.15倍
                    break;

                case "2":
                    FHCView.ApplyParaLineSpace(ParaLineSpaceMode.pls150);  // 1.5倍
                    break;

                case "3":
                    FHCView.ApplyParaLineSpace(ParaLineSpaceMode.pls200);  // 双倍
                    break;
            }
        }

        private void cbbFontSize_DropDownClosed(object sender, EventArgs e)
        {
            FHCView.ApplyTextFontSize(HC.View.HC.GetFontSize(cbbFontSize.Text));
            if (!FHCView.Focused)
                FHCView.Focus();
        }

        private void cbbFont_DropDownClosed(object sender, EventArgs e)
        {
            FHCView.ApplyTextFontName(cbbFont.Text);
            if (!FHCView.Focused)
                FHCView.Focus();
        }

        private void pmHCView_Opening(object sender, CancelEventArgs e)
        {
            if (FHCView.AnnotatePre.ActiveDrawAnnotateIndex >= 0)
            {
                for (int i = 0; i <= pmHCView.Items.Count - 1; i++)
                    pmHCView.Items[i].Visible = false;

                mniModAnnotate.Visible = true;
                mniDelAnnotate.Visible = true;

                return;
            }
            else
            {
                for (int i = 0; i <= pmHCView.Items.Count - 1; i++)
                    pmHCView.Items[i].Visible = true;

                mniModAnnotate.Visible = false;
                mniDelAnnotate.Visible = false;
            }

            HCCustomData vActiveData = FHCView.ActiveSection.ActiveData;
            HCCustomItem vActiveItem = vActiveData.GetActiveItem();

            HCCustomData vTopData = null;
            HCCustomItem vTopItem = vActiveItem;

            while (vTopItem is HCCustomRectItem)
            {
                if ((vTopItem as HCCustomRectItem).GetActiveData() != null)
                {
                    if (vTopData != null)
                    {
                        vActiveData = vTopData;
                        vActiveItem = vTopItem;
                    }

                    vTopData = (vTopItem as HCCustomRectItem).GetTopLevelData();
                    vTopItem = vTopData.GetActiveItem();
                }
                else
                    break;
            }

            if (vTopData == null)
                vTopData = vActiveData;

            mniTable.Enabled = vActiveItem.StyleNo == HCStyle.Table;
            if (mniTable.Enabled)
            {
                HCTableItem vTableItem = vActiveItem as HCTableItem;
                mniInsertRowTop.Enabled = vTableItem.GetEditCell() != null;
                mniInsertRowBottom.Enabled = mniInsertRowTop.Enabled;
                mniInsertColLeft.Enabled = mniInsertRowTop.Enabled;
                mniInsertColRight.Enabled = mniInsertRowTop.Enabled;
                mniSplitRow.Enabled = mniInsertRowTop.Enabled;
                mniSplitCol.Enabled = mniInsertRowTop.Enabled;

                mniDeleteCurRow.Enabled = vTableItem.CurRowCanDelete();
                mniDeleteCurCol.Enabled = vTableItem.CurColCanDelete();
                mniMerge.Enabled = vTableItem.SelectedCellCanMerge();

                if (vTableItem.BorderVisible)
                    mniDisBorder.Text = "隐藏边框";
                else
                    mniDisBorder.Text = "显示边框";
            }

            mniCut.Enabled = (!FHCView.ActiveSection.ReadOnly && vTopData.SelectExists());
            mniCopy.Enabled = mniCut.Enabled;

            IDataObject vIData = Clipboard.GetDataObject();
            mniPaste.Enabled = ((!FHCView.ActiveSection.ReadOnly)
                && ((vIData.GetDataPresent(HC.View.HC.HC_EXT))
                        || (vIData.GetDataPresent(DataFormats.Text))
                        || (vIData.GetDataPresent(DataFormats.Bitmap))
                    ));
            mniControlItem.Visible = ((!FHCView.ActiveSection.ReadOnly)
                                        && (!vTopData.SelectExists())
                                        && (vTopItem is HCControlItem)
                                        && vTopItem.Active
                                      );
            if (mniControlItem.Visible)
                mniControlItem.Text = "属性(" + (vTopItem as HCControlItem).GetType().Name + ")";
        }

        private void mniCut_Click(object sender, EventArgs e)
        {
            FHCView.Cut();
        }

        private void mniCopy_Click(object sender, EventArgs e)
        {
            FHCView.Copy();
        }

        private void mniPaste_Click(object sender, EventArgs e)
        {
            FHCView.Paste();
        }

        private void mniInsertRowTop_Click(object sender, EventArgs e)
        {
            FHCView.ActiveTableInsertRowBefor(1);
        }

        private void mniInsertRowBottom_Click(object sender, EventArgs e)
        {
            FHCView.ActiveTableInsertRowAfter(1);
        }

        private void mniInsertColLeft_Click(object sender, EventArgs e)
        {
            FHCView.ActiveTableInsertColBefor(1);
        }

        private void mniInsertColRight_Click(object sender, EventArgs e)
        {
            FHCView.ActiveTableInsertColAfter(1);
        }

        private void mniMerge_Click(object sender, EventArgs e)
        {
            FHCView.MergeTableSelectCells();
        }

        private void mniSplitRow_Click(object sender, EventArgs e)
        {
            FHCView.ActiveTableSplitCurRow();
        }

        private void mniSplitCol_Click(object sender, EventArgs e)
        {
            FHCView.ActiveTableSplitCurCol();
        }

        private void mniDeleteCurRow_Click(object sender, EventArgs e)
        {
            FHCView.ActiveTableDeleteCurRow();
        }

        private void mniDeleteCurCol_Click(object sender, EventArgs e)
        {
            FHCView.ActiveTableDeleteCurCol();
        }

        private void mniDisBorder_Click(object sender, EventArgs e)
        {
            if (FHCView.ActiveSection.ActiveData.GetActiveItem() is HCTableItem)
            {
                HCTableItem vTable = FHCView.ActiveSection.ActiveData.GetActiveItem() as HCTableItem;
                vTable.BorderVisible = !vTable.BorderVisible;
                FHCView.UpdateView();
            }
        }

        private void cbbZoom_DropDownClosed(object sender, EventArgs e)
        {
            Single vOut = 0;
            if (float.TryParse(cbbZoom.Text, out vOut))
                FHCView.Zoom = vOut / 100;
            else
                FHCView.Zoom = 1.0f;
        }

        private void 超连接ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCCustomData vTopData = FHCView.ActiveSectionTopLevelData();
            HCTextItem vTextItem = vTopData.CreateDefaultTextItem() as HCTextItem;
            vTextItem.Text = "打开百度";
            vTextItem.HyperLink = "www.baidu.com";
            FHCView.InsertItem(vTextItem);
        }

        private void 域ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FHCView.InsertDomain(null);
        }

        private void 一维码ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            HCFloatBarCodeItem vFloatBarCodeItem = new HCFloatBarCodeItem(FHCView.ActiveSection.ActiveData);
            FHCView.InsertFloatItem(vFloatBarCodeItem);
        }
    }
}
