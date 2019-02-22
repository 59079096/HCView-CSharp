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

namespace EMRView
{
    public partial class frmRecord : Form
    {
        private EventHandler FOnSave, FOnChangedSwitch, FOnReadOnlySwitch, FOnDeComboboxGetItem;
        public EmrView emrView;
        public frmRecordPop frmRecordPop;

        public frmRecord()
        {
            InitializeComponent();
        }

        private void DoComboboxPopupItem(object sender, EventArgs e)
        {
            if (FOnDeComboboxGetItem != null)
                FOnDeComboboxGetItem(sender, e);
        }

        private void DoItemInsert(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            if (aItem is DeCombobox)
                (aItem as DeCombobox).OnPopupItem = DoComboboxPopupItem;
        }

        private void PopupFormClose(object sender, EventArgs e)
        {
            if ((frmRecordPop != null) && frmRecordPop.Visible)
                frmRecordPop.Close();
        }

        private void DoMouseDown(object sender, MouseEventArgs e)
        {
            PopupFormClose(this, null);
        }

        private void DoActiveItemChange(object sender, EventArgs e)
        {
            emrView.ActiveSection.ReFormatActiveItem();
        }

        private frmRecordPop PopupForm()
        {
            if (frmRecordPop == null)
            {
                frmRecordPop = new frmRecordPop();
                frmRecordPop.OnActiveItemChange = DoActiveItemChange;
            }

            return frmRecordPop;
        }

        private void DoMouseUp(object sender, MouseEventArgs e)
        {
            string vInfo = "";

            if (emrView.ActiveSection.ActiveData.ReadOnly)
                return;

            HCCustomItem vActiveItem = emrView.GetTopLevelItem();
            if (vActiveItem != null)
            {
                if (emrView.ActiveSection.ActiveData.ActiveDomain.BeginNo >= 0)
                {
                    DeGroup vDeGroup = emrView.ActiveSection.ActiveData.Items[
                        emrView.ActiveSection.ActiveData.ActiveDomain.BeginNo] as DeGroup;
                    vInfo = vDeGroup[DeProp.Name];
                }

                if (vActiveItem is DeItem)
                {
                    DeItem vDeItem = vActiveItem as DeItem;
                    if (vDeItem.Active
                            && (vDeItem[DeProp.Index] != "")
                            && (!vDeItem.IsSelectComplate)
                            && (!vDeItem.IsSelectPart)
                        )
                    {
                        vInfo = vInfo + "元素(" + vDeItem[DeProp.Index] + ")";

                        POINT vPt = emrView.GetActiveDrawItemClientCoord();
                        HCCustomDrawItem vActiveDrawItem = emrView.GetTopLevelDrawItem();
                        RECT vDrawItemRect = vActiveDrawItem.Rect;
                        vDrawItemRect = HC.View.HC.Bounds(vPt.X, vPt.Y, vDrawItemRect.Width, vDrawItemRect.Height);
                        if (HC.View.HC.PtInRect(vDrawItemRect, new POINT(e.X, e.Y)))
                        {
                            vPt.Y = vPt.Y + emrView.ZoomIn(vActiveDrawItem.Height());
                            vPt.Offset(emrView.Left, emrView.Top);
                            HC.Win32.User.ClientToScreen(Handle, ref vPt);

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

        private void GetPagesAndActive()
        {
            tssPage.Text = "预览" + (emrView.PagePreviewFirst + 1).ToString()
                + "页 光标" + (emrView.ActivePageIndex + 1).ToString()
                + "页 共" + emrView.PageCount.ToString() + "页";
        }

        private void CurTextStyleChange(int aNewStyleNo)
        {
            if (aNewStyleNo >= 0)
            {
                cbbFont.SelectedIndex = cbbFont.Items.IndexOf(emrView.Style.TextStyles[aNewStyleNo].Family);
                cbbFontSize.SelectedIndex = cbbFontSize.Items.IndexOf(HC.View.HC.GetFontSizeStr(emrView.Style.TextStyles[aNewStyleNo].Size));
                btnBold.Checked = emrView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsBold);
                btnItalic.Checked = emrView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsItalic);
                btnUnderLine.Checked = emrView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsUnderline);
                btnStrikeOut.Checked = emrView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsStrikeOut);
                btnSuperScript.Checked = emrView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsSuperscript);
                btnSubScript.Checked = emrView.Style.TextStyles[aNewStyleNo].FontStyles.Contains((byte)HCFontStyle.tsSubscript);
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
                ParaAlignHorz vAlignHorz = emrView.Style.ParaStyles[aNewParaNo].AlignHorz;

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

            CurTextStyleChange(emrView.Style.CurStyleNo);
            CurParaStyleChange(emrView.Style.CurParaNo);
        }

        private void DoVerScroll(object sender, EventArgs e)
        {
            GetPagesAndActive();
        }

        private void DoChangedSwitch(object sender, EventArgs e)
        {
            if (FOnChangedSwitch != null)
                FOnChangedSwitch(sender, e);
        }

        private void DoReadOnlySwitch(object sender, EventArgs e)
        {
            if (FOnReadOnlySwitch != null)
                FOnReadOnlySwitch(sender, e);
        }

        private void frmRecord_Load(object sender, EventArgs e)
        {
            if (emrView == null)
            {
                emrView = new EmrView();
                emrView.OnSectionItemInsert = DoItemInsert;
                emrView.MouseDown += DoMouseDown;
                emrView.MouseUp += DoMouseUp;
                emrView.OnCaretChange = DoCaretChange;
                emrView.OnVerScroll = DoVerScroll;
                emrView.OnChangedSwitch = DoChangedSwitch;
                emrView.OnSectionReadOnlySwitch = DoReadOnlySwitch;
                emrView.ContextMenuStrip = this.pmRichEdit;
                //
                this.Controls.Add(emrView);
                //FEmrView.Parent = this;
                emrView.Dock = DockStyle.Fill;
                emrView.Show();
            }
        }

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
                        emrView.LoadFromFile(vOpenDlg.FileName);
                    }
                }
            }
            finally
            {
                vOpenDlg.Dispose();
                GC.Collect();
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            emrView.FileName = "";
            emrView.Clear();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //DoSave();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            using (PrintDialog vPrintDlg = new PrintDialog())
            {
                vPrintDlg.PrinterSettings.MaximumPage = emrView.PageCount;
                if (vPrintDlg.ShowDialog() == DialogResult.OK)
                    emrView.Print(vPrintDlg.PrinterSettings.PrinterName);
            }
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            emrView.Undo();
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            emrView.Redo();
        }

        private void btnSymmetryMargin_Click(object sender, EventArgs e)
        {
            emrView.SymmetryMargin = !emrView.SymmetryMargin;
        }

        private void cbbFont_DropDownClosed(object sender, EventArgs e)
        {
            emrView.ApplyTextFontName(cbbFont.Text);
            if (!emrView.Focused)
                emrView.Focus();
        }

        private void cbbZoom_DropDownClosed(object sender, EventArgs e)
        {
            emrView.Zoom = float.Parse(cbbZoom.Text) / 100;
        }

        private void cbbFontSize_DropDownClosed(object sender, EventArgs e)
        {
            emrView.ApplyTextFontSize(HC.View.HC.GetFontSize(cbbFontSize.Text));
            if (!emrView.Focused)
                emrView.Focus();
        }

        private void btnBold_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripButton).Tag.ToString())
            {
                case "0":
                    emrView.ApplyTextStyle(HCFontStyle.tsBold);
                    break;

                case "1":
                    emrView.ApplyTextStyle(HCFontStyle.tsItalic);
                    break;

                case "2":
                    emrView.ApplyTextStyle(HCFontStyle.tsUnderline);
                    break;

                case "3":
                    emrView.ApplyTextStyle(HCFontStyle.tsStrikeOut);
                    break;

                case "4":
                    emrView.ApplyTextStyle(HCFontStyle.tsSuperscript);
                    break;

                case "5":
                    emrView.ApplyTextStyle(HCFontStyle.tsSubscript);
                    break;
            }
        }

        private void btnAlignLeft_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripButton).Tag.ToString())
            {
                case "0":
                    emrView.ApplyParaAlignHorz(ParaAlignHorz.pahLeft);
                    break;

                case "1":
                    emrView.ApplyParaAlignHorz(ParaAlignHorz.pahCenter);
                    break;

                case "2":
                    emrView.ApplyParaAlignHorz(ParaAlignHorz.pahRight);
                    break;

                case "3":
                    emrView.ApplyParaAlignHorz(ParaAlignHorz.pahJustify);  // 两端
                    break;

                case "4":
                    emrView.ApplyParaAlignHorz(ParaAlignHorz.pahScatter);  // 分散
                    break;

                case "5":
                    emrView.ApplyParaLeftIndent();
                    break;

                case "6":
                    emrView.ApplyParaLeftIndent(false);
                    break;
            }
        }

        private void mniLS100_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripMenuItem).Tag.ToString())
            {
                case "0":
                    emrView.ApplyParaLineSpace(ParaLineSpaceMode.pls100);   // 单倍
                    break;

                case "1":
                    emrView.ApplyParaLineSpace(ParaLineSpaceMode.pls115);  // 1.15倍
                    break;

                case "2":
                    emrView.ApplyParaLineSpace(ParaLineSpaceMode.pls150);  // 1.5倍
                    break;

                case "3":
                    emrView.ApplyParaLineSpace(ParaLineSpaceMode.pls200);  // 双倍
                    break;
            }
        }

        private void mniCut_Click(object sender, EventArgs e)
        {
            emrView.Cut();
        }

        private void mniCopy_Click(object sender, EventArgs e)
        {
            emrView.Copy();
        }

        private void mniPaste_Click(object sender, EventArgs e)
        {
            emrView.Paste();
        }

        private void mniInsertRowTop_Click(object sender, EventArgs e)
        {
            emrView.ActiveTableInsertRowBefor(1);
        }

        private void mniInsertRowBottom_Click(object sender, EventArgs e)
        {
            emrView.ActiveTableInsertRowAfter(1);
        }

        private void mniInsertColLeft_Click(object sender, EventArgs e)
        {
            emrView.ActiveTableInsertColBefor(1);
        }

        private void mniInsertColRight_Click(object sender, EventArgs e)
        {
            emrView.ActiveTableInsertColAfter(1);
        }

        private void mniMerge_Click(object sender, EventArgs e)
        {
            emrView.MergeTableSelectCells();
        }

        private void mniSplitRow_Click(object sender, EventArgs e)
        {
            emrView.ActiveTableSplitCurRow();
        }

        private void mniSplitCol_Click(object sender, EventArgs e)
        {
            emrView.ActiveTableSplitCurCol();
        }

        private void mniDeleteCurRow_Click(object sender, EventArgs e)
        {
            emrView.ActiveTableDeleteCurRow();
        }

        private void mniDeleteCurCol_Click(object sender, EventArgs e)
        {
            emrView.ActiveTableDeleteCurCol();
        }

        private void mniDisBorder_Click(object sender, EventArgs e)
        {
            if (emrView.ActiveSection.ActiveData.GetCurItem() is HCTableItem)
            {
                HCTableItem vTable = emrView.ActiveSection.ActiveData.GetCurItem() as HCTableItem;
                vTable.BorderVisible = !vTable.BorderVisible;
                emrView.UpdateView();
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCRichData vTopData = emrView.ActiveSectionTopLevelData() as HCRichData;
            vTopData.DeleteItems(vTopData.SelectInfo.StartItemNo);
            emrView.FormatSection(emrView.ActiveSectionIndex);
        }

        private void 更新引用ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HCViewData vTopData = emrView.ActiveSectionTopLevelData() as HCViewData;
            HCDomainInfo vDomain = vTopData.ActiveDomain;
            HCViewData vPageData = emrView.ActiveSection.PageData;

            string vText = "";
            if (vTopData == vPageData)
                vText = emrView.GetDataForwardDeGroupText(vPageData, vDomain.BeginNo);
            else
                vText = "";

            if (vText != "")
            {
                emrView.SetDataDeGroupText(vTopData, vDomain.BeginNo, vText);
                emrView.FormatSection(emrView.ActiveSectionIndex);
            }
        }

        private void 删除ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            HCViewData vTopData = emrView.ActiveSectionTopLevelData() as HCViewData;
            HCDomainInfo vDomain = vTopData.ActiveDomain;
            vTopData.DeleteItems(vDomain.BeginNo, vDomain.EndNo);
            emrView.FormatSection(emrView.ActiveSectionIndex);
        }
    }
}
