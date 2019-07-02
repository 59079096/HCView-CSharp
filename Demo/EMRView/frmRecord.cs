/*******************************************************}
{                                                       }
{         基于HCView的电子病历程序  作者：荆通          }
{                                                       }
{ 此代码仅做学习交流使用，不可用于商业目的，由此引发的  }
{ 后果请使用者承担，加入QQ群 649023932 来获取更多的技术 }
{ 交流。                                                }
{                                                       }
{*******************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HC.View;
using HC.Win32;
using System.IO;

namespace EMRView
{
    public delegate void DeItemInsertEventHandler(HCEmrView aEmrView, HCSection aSection,
        HCCustomData aData, HCCustomItem aItem);

    public partial class frmRecord : Form
    {
        private int FMouseDownTick;
        private HCEmrView FEmrView;
        private frmRecordPop frmRecordPop;
        private EventHandler FOnSave, FOnSaveStructure, FOnChangedSwitch, FOnReadOnlySwitch;
        private DeItemInsertEventHandler FOnInsertDeItem;

        private void btnOpen_Click(object sender, EventArgs e)
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
                        FEmrView.LoadFromFile(vOpenDlg.FileName);
                    }
                }
            }
            finally
            {
                vOpenDlg.Dispose();
                GC.Collect();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DoSave();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            using (PrintDialog vPrintDlg = new PrintDialog())
            {
                vPrintDlg.PrinterSettings.MaximumPage = FEmrView.PageCount;
                if (vPrintDlg.ShowDialog() == DialogResult.OK)
                    FEmrView.Print(vPrintDlg.PrinterSettings.PrinterName);
            }
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            FEmrView.Undo();
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            FEmrView.Redo();
        }

        private void btnSymmetryMargin_Click(object sender, EventArgs e)
        {
            FEmrView.SymmetryMargin = !FEmrView.SymmetryMargin;
        }

        private void cbbFont_DropDownClosed(object sender, EventArgs e)
        {
            FEmrView.ApplyTextFontName(cbbFont.Text);
            if (!FEmrView.Focused)
                FEmrView.Focus();
        }

        private void cbbZoom_DropDownClosed(object sender, EventArgs e)
        {
            FEmrView.Zoom = float.Parse(cbbZoom.Text) / 100;
        }

        private void cbbFontSize_DropDownClosed(object sender, EventArgs e)
        {
            FEmrView.ApplyTextFontSize(HC.View.HC.GetFontSize(cbbFontSize.Text));
            if (!FEmrView.Focused)
                FEmrView.Focus();
        }

        private void btnBold_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripButton).Tag.ToString())
            {
                case "0":
                    FEmrView.ApplyTextStyle(HCFontStyle.tsBold);
                    break;

                case "1":
                    FEmrView.ApplyTextStyle(HCFontStyle.tsItalic);
                    break;

                case "2":
                    FEmrView.ApplyTextStyle(HCFontStyle.tsUnderline);
                    break;

                case "3":
                    FEmrView.ApplyTextStyle(HCFontStyle.tsStrikeOut);
                    break;

                case "4":
                    FEmrView.ApplyTextStyle(HCFontStyle.tsSuperscript);
                    break;

                case "5":
                    FEmrView.ApplyTextStyle(HCFontStyle.tsSubscript);
                    break;
            }
        }

        private void btnAlignLeft_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripButton).Tag.ToString())
            {
                case "0":
                    FEmrView.ApplyParaAlignHorz(ParaAlignHorz.pahLeft);
                    break;

                case "1":
                    FEmrView.ApplyParaAlignHorz(ParaAlignHorz.pahCenter);
                    break;

                case "2":
                    FEmrView.ApplyParaAlignHorz(ParaAlignHorz.pahRight);
                    break;

                case "3":
                    FEmrView.ApplyParaAlignHorz(ParaAlignHorz.pahJustify);  // 两端
                    break;

                case "4":
                    FEmrView.ApplyParaAlignHorz(ParaAlignHorz.pahScatter);  // 分散
                    break;

                case "5":
                    FEmrView.ApplyParaLeftIndent();
                    break;

                case "6":
                    FEmrView.ApplyParaLeftIndent(false);
                    break;
            }
        }

        private void mniLS100_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripMenuItem).Tag.ToString())
            {
                case "0":
                    FEmrView.ApplyParaLineSpace(ParaLineSpaceMode.pls100);   // 单倍
                    break;

                case "1":
                    FEmrView.ApplyParaLineSpace(ParaLineSpaceMode.pls115);  // 1.15倍
                    break;

                case "2":
                    FEmrView.ApplyParaLineSpace(ParaLineSpaceMode.pls150);  // 1.5倍
                    break;

                case "3":
                    FEmrView.ApplyParaLineSpace(ParaLineSpaceMode.pls200);  // 双倍
                    break;
            }
        }

        private void mniCut_Click(object sender, EventArgs e)
        {
            FEmrView.Cut();
        }

        private void mniCopy_Click(object sender, EventArgs e)
        {
            FEmrView.Copy();
        }

        private void mniPaste_Click(object sender, EventArgs e)
        {
            FEmrView.Paste();
        }

        private void mniInsertRowTop_Click(object sender, EventArgs e)
        {
            FEmrView.ActiveTableInsertRowBefor(1);
        }

        private void mniInsertRowBottom_Click(object sender, EventArgs e)
        {
            FEmrView.ActiveTableInsertRowAfter(1);
        }

        private void mniInsertColLeft_Click(object sender, EventArgs e)
        {
            FEmrView.ActiveTableInsertColBefor(1);
        }

        private void mniInsertColRight_Click(object sender, EventArgs e)
        {
            FEmrView.ActiveTableInsertColAfter(1);
        }

        private void mniMerge_Click(object sender, EventArgs e)
        {
            FEmrView.MergeTableSelectCells();
        }

        private void mniSplitRow_Click(object sender, EventArgs e)
        {
            FEmrView.ActiveTableSplitCurRow();
        }

        private void mniSplitCol_Click(object sender, EventArgs e)
        {
            FEmrView.ActiveTableSplitCurCol();
        }

        private void mniDeleteCurRow_Click(object sender, EventArgs e)
        {
            FEmrView.ActiveTableDeleteCurRow();
        }

        private void mniDeleteCurCol_Click(object sender, EventArgs e)
        {
            FEmrView.ActiveTableDeleteCurCol();
        }

        private void mniDisBorder_Click(object sender, EventArgs e)
        {
            if (FEmrView.ActiveSection.ActiveData.GetActiveItem() is HCTableItem)
            {
                HCTableItem vTable = FEmrView.ActiveSection.ActiveData.GetActiveItem() as HCTableItem;
                vTable.BorderVisible = !vTable.BorderVisible;
                FEmrView.UpdateView();
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCRichData vTopData = FEmrView.ActiveSectionTopLevelData() as HCRichData;
            FEmrView.DeleteActiveDataItems(vTopData.SelectInfo.StartItemNo);
        }

        private void 更新引用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCViewData vTopData = FEmrView.ActiveSectionTopLevelData() as HCViewData;
            HCDomainInfo vDomain = vTopData.ActiveDomain;
            HCViewData vPageData = FEmrView.ActiveSection.Page;

            string vText = "";
            if (vTopData == vPageData)
                vText = FEmrView.GetDataForwardDeGroupText(vPageData, vDomain.BeginNo);
            else
                vText = "";

            if (vText != "")
            {
                FEmrView.BeginUpdate();
                try
                {
                    FEmrView.SetDeGroupText(vTopData, vDomain.BeginNo, vText);
                    FEmrView.FormatSection(FEmrView.ActiveSectionIndex);
                }
                finally
                {
                    FEmrView.EndUpdate();
                }
            }
        }

        private void 删除ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FEmrView.DeleteActiveDomain();
        }

        public frmRecord()
        {
            InitializeComponent();

            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily family in fonts.Families)
            {
                cbbFont.Items.Add(family.Name);
            }
            cbbFont.Text = "宋体";
            cbbFontSize.Text = "五号";
            cbbZoom.SelectedIndex = 3;

            if (FEmrView == null)
            {
                FEmrView = new HCEmrView();
                FEmrView.OnSectionItemInsert = DoItemInsert;
                FEmrView.MouseDown += DoMouseDown;
                FEmrView.MouseUp += DoMouseUp;
                FEmrView.OnCaretChange = DoCaretChange;
                FEmrView.OnVerScroll = DoVerScroll;
                FEmrView.OnChangedSwitch = DoChangedSwitch;
                FEmrView.OnSectionReadOnlySwitch = DoReadOnlySwitch;
                FEmrView.OnCanNotEdit = DoCanNotEdit;
                FEmrView.OnSectionPaintPaperBefor = DoPaintPaperBefor;
                FEmrView.ContextMenuStrip = this.pmView;
                //
                this.pnlView.Controls.Add(FEmrView);
                //FEmrView.Parent = this;
                FEmrView.Dock = DockStyle.Fill;
                FEmrView.Show();
            }
        }

        private void GetPagesAndActive()
        {
            tssPage.Text = "预览" + (FEmrView.PagePreviewFirst + 1).ToString()
                + "页 光标" + (FEmrView.ActivePageIndex + 1).ToString()
                + "页 共" + FEmrView.PageCount.ToString() + "页";
        }

        private void DoCaretChange(object sender, EventArgs e)
        {
            GetPagesAndActive();

            CurTextStyleChange(FEmrView.CurStyleNo);
            CurParaStyleChange(FEmrView.CurParaNo);
        }

        private void DoChangedSwitch(object sender, EventArgs e)
        {
            if (FOnChangedSwitch != null)
                FOnChangedSwitch(this, e);
        }

        private void DoCanNotEdit(object sender, EventArgs e)
        {
            MessageBox.Show("当前位置只读、不可编辑！");
        }

        private void DoReadOnlySwitch(object sender, EventArgs e)
        {
            if (FOnReadOnlySwitch != null)
                FOnReadOnlySwitch(sender, e);
        }

        private void DoVerScroll(object sender, EventArgs e)
        {
            GetPagesAndActive();
        }

        private void DoPaintPaperBefor(object sender, int aPageIndex, RECT aRect,
            HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            if ((!aPaintInfo.Print) && (sender as HCSection).ReadOnly)
            {
                aCanvas.Font.BeginUpdate();
                try
                {
                    aCanvas.Font.Size = 48;
                    aCanvas.Font.Color = Color.Gray;
                    aCanvas.Font.Family = "隶书";
                }
                finally
                {
                    aCanvas.Font.EndUpdate();
                }

                aCanvas.TextOut(aRect.Left + 10, aRect.Top + 10, "只读");
            }
        }

        private void DoSetActiveDeItemText(string aText)
        {
            FEmrView.SetActiveItemText(aText);
        }

        private void DoSetActiveDeItemExtra(Stream aStream)
        {
            FEmrView.SetActiveItemExtra(aStream);
        }

        private void CurTextStyleChange(int aNewStyleNo)
        {
            if (aNewStyleNo >= 0)
            {
                cbbFont.SelectedIndex = cbbFont.Items.IndexOf(FEmrView.Style.TextStyles[aNewStyleNo].Family);
                cbbFontSize.SelectedIndex = cbbFontSize.Items.IndexOf(HC.View.HC.GetFontSizeStr(FEmrView.Style.TextStyles[aNewStyleNo].Size));
                btnBold.Checked = FEmrView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsBold);
                btnItalic.Checked = FEmrView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsItalic);
                btnUnderLine.Checked = FEmrView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsUnderline);
                btnStrikeOut.Checked = FEmrView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsStrikeOut);
                btnSuperScript.Checked = FEmrView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsSuperscript);
                btnSubScript.Checked = FEmrView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsSubscript);
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
                ParaAlignHorz vAlignHorz = FEmrView.Style.ParaStyles[aNewParaNo].AlignHorz;

                btnAlignLeft.Checked = vAlignHorz == ParaAlignHorz.pahLeft;
                btnAlignRight.Checked = vAlignHorz == ParaAlignHorz.pahRight;
                btnAlignCenter.Checked = vAlignHorz == ParaAlignHorz.pahCenter;
                btnAlignJustify.Checked = vAlignHorz == ParaAlignHorz.pahJustify;
                btnAlignScatter.Checked = vAlignHorz == ParaAlignHorz.pahScatter;
            }
        }

        private frmRecordPop PopupForm()
        {
            frmRecordPop = new frmRecordPop();
            frmRecordPop.OnSetActiveItemText = DoSetActiveDeItemText;
            frmRecordPop.OnSetActiveItemExtra = DoSetActiveDeItemExtra;

            return frmRecordPop;
        }

        private void PopupFormClose(object sender, EventArgs e)
        {
            if ((frmRecordPop != null) && frmRecordPop.Visible)
                frmRecordPop.Close();
        }

        protected void DoMouseDown(object sender, MouseEventArgs e)
        {
            PopupFormClose(this, null);
            FMouseDownTick = Environment.TickCount;
        }

        protected void DoMouseUp(object sender, MouseEventArgs e)
        {
            string vInfo = "";

            if (FEmrView.ActiveSection.ActiveData.ReadOnly)
            {
                tssDeInfo.Text = "";
                return;
            }

            HCCustomItem vActiveItem = FEmrView.GetTopLevelItem();
            if (vActiveItem != null)
            {
                if (FEmrView.ActiveSection.ActiveData.ActiveDomain.BeginNo >= 0)
                {
                    DeGroup vDeGroup = FEmrView.ActiveSection.ActiveData.Items[
                        FEmrView.ActiveSection.ActiveData.ActiveDomain.BeginNo] as DeGroup;

                    vInfo = vDeGroup[DeProp.Name];
                }

                if (vActiveItem is DeItem)
                {
                    DeItem vDeItem = vActiveItem as DeItem;
                    if (vDeItem.Active
                            && (vDeItem[DeProp.Index] != "")
                            && (!vDeItem.IsSelectComplate)
                            && (!vDeItem.IsSelectPart)
                            && (Environment.TickCount - FMouseDownTick < 500)
                        )
                    {
                        vInfo = vInfo + "元素(" + vDeItem[DeProp.Index] + ")";

                        if (FEmrView.ActiveSection.ActiveData.ReadOnly)
                        {
                            tssDeInfo.Text = "";
                            return;
                        }

                        POINT vPt = FEmrView.GetActiveDrawItemClientCoord();
                        HCCustomDrawItem vActiveDrawItem = FEmrView.GetTopLevelDrawItem();
                        RECT vDrawItemRect = vActiveDrawItem.Rect;
                        vDrawItemRect = HC.View.HC.Bounds(vPt.X, vPt.Y, vDrawItemRect.Width, vDrawItemRect.Height);
                        
                        if (HC.View.HC.PtInRect(vDrawItemRect, new POINT(e.X, e.Y)))
                        {
                            vPt.Y = vPt.Y + FEmrView.ZoomIn(vActiveDrawItem.Height());
                            vPt.Offset(FEmrView.Left, FEmrView.Top);
                            HC.Win32.User.ClientToScreen(FEmrView.Handle, ref vPt);

                            PopupForm().PopupDeItem(vDeItem, vPt);
                        }
                    }
                }
                else
                    if (vActiveItem is DeEdit)
                    {

                    }
                    else
                        if (vActiveItem is DeCombobox)
                        {

                        }
                        else
                            if (vActiveItem is DeDateTimePicker)
                            {

                            }
            }

            tssDeInfo.Text = vInfo;
        }

        protected void DoItemInsert(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            if (FOnInsertDeItem != null)
                FOnInsertDeItem(FEmrView, sender as HCSection, aData, aItem);
        }

        protected void DoSave()
        {
            if (FOnSave != null)
                FOnSave(this, null);
        }

        protected void DoSaveStructure()
        {
            if (FOnSaveStructure != null)
                FOnSaveStructure(this, null);
        }

        public object ObjectData;

        public frmDataElement FfrmDataElement;

        public void HideToolbar()
        {
            tlbTool.Visible = false;
        }

        public void InsertDeItem(string aIndex, string aName)
        {
            DeItem vDeItem = FEmrView.NewDeItem(aName);
            vDeItem[DeProp.Index] = aIndex;
            vDeItem[DeProp.Name] = aName;
            FEmrView.InsertDeItem(vDeItem);
        }

        // 属性
        public HCEmrView EmrView
        {
            get { return FEmrView; }
        }

        public EventHandler OnSave
        {
            get { return FOnSave; }
            set { FOnSave = value; }
        }

        public EventHandler OnSaveStructure
        {
            get { return FOnSaveStructure; }
            set { FOnSaveStructure = value; }
        }

        public EventHandler OnChangedSwitch
        {
            get { return FOnChangedSwitch; }
            set { FOnChangedSwitch = value; }
        }

        public EventHandler OnReadOnlySwitch
        {
            get { return FOnReadOnlySwitch; }
            set { FOnReadOnlySwitch = value; }
        }

        public DeItemInsertEventHandler OnInsertDeItem
        {
            get { return FOnInsertDeItem; }
            set { FOnInsertDeItem = value; }
        }

        private void pmView_Opening(object sender, CancelEventArgs e)
        {
            HCCustomData vActiveData = FEmrView.ActiveSection.ActiveData;
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

                    vTopData = (vTopItem as HCCustomRectItem).GetActiveData();
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
                DeTable vTable = vActiveItem as DeTable;
                mniInsertRowTop.Enabled = vTable.GetEditCell() != null;
                mniInsertRowBottom.Enabled = mniInsertRowTop.Enabled;
                mniInsertColLeft.Enabled = mniInsertRowTop.Enabled;
                mniInsertColRight.Enabled = mniInsertRowTop.Enabled;
                mniSplitRow.Enabled = mniInsertRowTop.Enabled;
                mniSplitCol.Enabled = mniInsertRowTop.Enabled;

                mniDeleteCurRow.Enabled = vTable.CurRowCanDelete();
                mniDeleteCurCol.Enabled = vTable.CurColCanDelete();
                mniMerge.Enabled = vTable.SelectedCellCanMerge();
            }

            mniCut.Enabled = (!FEmrView.ActiveSection.ReadOnly) && vTopData.SelectExists();
            mniCopy.Enabled = mniCut.Enabled;

            IDataObject vIData = Clipboard.GetDataObject();

            mniPaste.Enabled = ((!(vTopData as HCRichData).ReadOnly)
                && (    (vIData.GetDataPresent(HC.View.HC.HC_EXT)) 
                        || vIData.GetDataPresent(DataFormats.Text) 
                        || (vIData.GetDataPresent(DataFormats.Bitmap))));

            mniControlItem.Visible = ((!(vTopData as HCRichData).ReadOnly) && (!vTopData.SelectExists())
                && (vTopItem is HCControlItem) && (vTopItem.Active));
            if (mniControlItem.Visible)
                mniControlItem.Text = "属性(" + (vTopItem as HCControlItem).GetType().Name + ")";

            mniDeItem.Visible = false;
            mniDeleteProtect.Visible = false;

            if (vTopItem is DeItem)
            {
                if ((vTopItem as DeItem).IsElement)
                {
                    mniDeItem.Visible = true;
                    mniDeItem.Text = (vTopItem as DeItem)[DeProp.Name];
                }

                if (FEmrView.DesignModeEx)  // 文档设计模式
                {
                    if (vTopData.SelectExists())
                    {
                        mniDeleteProtect.Text = "只读";
                        mniDeleteProtect.Visible = true;
                    }
                    else
                        if ((vTopItem as DeItem).EditProtect)
                        {
                            mniDeleteProtect.Text = "取消只读";
                            mniDeleteProtect.Visible = true;
                        }
                }
            }

            if ((vTopData as HCViewData).ActiveDomain.BeginNo >= 0)
            {
                mniDeGroup.Visible = true;
                mniDeGroup.Text = (vTopData.Items[(vTopData as HCViewData).ActiveDomain.BeginNo] as DeGroup)[DeProp.Name];
            }
            else
                mniDeGroup.Visible = false;

            mniSplit.Visible = mniControlItem.Visible || mniDeItem.Visible || mniDeGroup.Visible;
        }

        private void mniDeItemProp_Click(object sender, EventArgs e)
        {
            frmDeProperty vFrmDeProperty = new frmDeProperty();
            vFrmDeProperty.SetHCView(FEmrView);
        }

        private void mniControlItem_Click(object sender, EventArgs e)
        {
            frmDeControlProperty vFrmDeControlProperty = new frmDeControlProperty();
            vFrmDeControlProperty.SetHCView(FEmrView);
        }

        private void mniDeleteProtect_Click(object sender, EventArgs e)
        {
            HCCustomData vTopData = FEmrView.ActiveSectionTopLevelData();

            if (vTopData.SelectExists())
            {
                string vS = vTopData.GetSelectText();
                vS = vS.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                DeItem vDeItem = FEmrView.NewDeItem(vS);
                vDeItem.EditProtect = true;
                FEmrView.InsertDeItem(vDeItem);
            }
            else
            {
                DeItem vDeItem = vTopData.GetActiveItem() as DeItem;
                if (vDeItem.EditProtect)
                {
                    vDeItem.EditProtect = false;
                    FEmrView.ReAdaptActiveItem();
                }
            }
        }

        private void mniInsertTable_Click(object sender, EventArgs e)
        {
            frmInsertTable vFrmInsertTable = new frmInsertTable();
            vFrmInsertTable.ShowDialog();
            if (vFrmInsertTable.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                HCRichData vTopData = FEmrView.ActiveSectionTopLevelData() as HCRichData;
                DeTable vTable = new DeTable(vTopData, vFrmInsertTable.RowCount, vFrmInsertTable.ColCount, vTopData.Width);
                FEmrView.InsertItem(vTable);
            }
        }

        private void mniInsertImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog vOpenDlg = new OpenFileDialog();
            vOpenDlg.Filter = "bmp文件|*.bmp";
            if (vOpenDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (vOpenDlg.FileName != "")
                {
                    HCImageItem vImageItem = new HCImageItem(FEmrView.ActiveSectionTopLevelData());
                    vImageItem.LoadFromBmpFile(vOpenDlg.FileName);
                    FEmrView.InsertItem(vImageItem);
                }
            }
        }

        private void mniInsertGif_Click(object sender, EventArgs e)
        {
            OpenFileDialog vOpenDlg = new OpenFileDialog();
            vOpenDlg.Filter = "GIF动画文件|*.gif";
            if (vOpenDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (vOpenDlg.FileName != "")
                {
                    HCGifItem vGifItem = new HCGifItem(FEmrView.ActiveSectionTopLevelData());
                    vGifItem.LoadFromFile(vOpenDlg.FileName);
                    FEmrView.InsertItem(vGifItem);
                }
            }
        }

        private void mniCheckbox_Click(object sender, EventArgs e)
        {
            DeCheckBox vCheckBox = new DeCheckBox(FEmrView.ActiveSectionTopLevelData(), "勾选框", false);
            FEmrView.InsertItem(vCheckBox);
        }

        private void mniEditItem_Click(object sender, EventArgs e)
        {
            DeEdit vDeEdit = new DeEdit(FEmrView.ActiveSectionTopLevelData(), "文本框");
            FEmrView.InsertItem(vDeEdit);
        }

        private void mniCombobox_Click(object sender, EventArgs e)
        {
            DeCombobox vDeCombobox = new DeCombobox(FEmrView.ActiveSectionTopLevelData(), "下拉框");
            vDeCombobox.SaveItem = false;
            vDeCombobox[DeProp.Index] = "1002";  // 控件的数据元属性
            //vDeCombobox.Items.Add("选项1");
            FEmrView.InsertItem(vDeCombobox);
        }

        private void mniInsertLine_Click(object sender, EventArgs e)
        {
            FEmrView.InsertLine(1);
        }

        private void mniPageBreak_Click(object sender, EventArgs e)
        {
            FEmrView.InsertPageBreak();
        }

        private void mniSection_Click(object sender, EventArgs e)
        {
            FEmrView.InsertSectionBreak();
        }

        private void mniYuejing_Click(object sender, EventArgs e)
        {
            EmrYueJingItem vYueJingItem = new EmrYueJingItem(FEmrView.ActiveSectionTopLevelData(),
                "12", "5-6", string.Format("{0:yyyy-MM-dd}", DateTime.Now), "28-30");
            FEmrView.InsertItem(vYueJingItem);
        }

        private void mniTooth_Click(object sender, EventArgs e)
        {
            EmrToothItem vToothItem = new EmrToothItem(FEmrView.ActiveSectionTopLevelData(),
                "XX", "XX", "XX", "XX");
            FEmrView.InsertItem(vToothItem);
        }

        private void mniFangJiao_Click(object sender, EventArgs e)
        {
            EmrFangJiaoItem vFangJiaoItem = new EmrFangJiaoItem(FEmrView.ActiveSectionTopLevelData(), "", "", "", "");
            FEmrView.InsertItem(vFangJiaoItem);
        }

        private void mniInsertDeItem_Click(object sender, EventArgs e)
        {
            frmDataElement vFrmDataElement = new frmDataElement();
            vFrmDataElement.OnInsertAsDE = InsertDeItem;
            vFrmDataElement.ShowDialog();
        }

        private void mniOpen_Click(object sender, EventArgs e)
        {
            if (FEmrView.ReadOnly)
            {
                MessageBox.Show("当前文档只读！");
                return;
            }

            OpenFileDialog vOpenDlg = new OpenFileDialog();
            vOpenDlg.Filter = "文件|*" + HC.View.HC.HC_EXT;
            if (vOpenDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (vOpenDlg.FileName != "")
                    FEmrView.LoadFromFile(vOpenDlg.FileName);
            }
        }

        private void mniSave_Click(object sender, EventArgs e)
        {
            DoSave();
        }

        private void mniSaveStructure_Click(object sender, EventArgs e)
        {
            DoSaveStructure();
        }

        private void mniClear_Click(object sender, EventArgs e)
        {
            FEmrView.Clear();
        }

        private void mniFastPrint_Click(object sender, EventArgs e)
        {
            btnPrint_Click(sender, e);
        }

        private void mniPrintCurLine_Click(object sender, EventArgs e)
        {
            FEmrView.PrintCurPageByActiveLine(false, false);
        }

        private void mniPrintSelect_Click(object sender, EventArgs e)
        {
            FEmrView.PrintCurPageSelected(false, false);
        }

        private void mniPageSet_Click(object sender, EventArgs e)
        {
            frmPageSet vFrmPageSet = new frmPageSet();
            vFrmPageSet.SetHCView(FEmrView);
        }

        private void mniTableProperty_Click(object sender, EventArgs e)
        {
            frmDeTableProperty vFrmDeTableProperty = new frmDeTableProperty();
            vFrmDeTableProperty.SetHCView(FEmrView);
        }

        private void mniBorder_Click(object sender, EventArgs e)
        {
            frmTableBorderBackColor vFrmBorderBackColor = new frmTableBorderBackColor();
            vFrmBorderBackColor.SetView(FEmrView);
        }

        private void mniPara_Click(object sender, EventArgs e)
        {
            frmParagraph vFrmParagraph = new frmParagraph();
            vFrmParagraph.SetView(FEmrView);
        }
    }
}
