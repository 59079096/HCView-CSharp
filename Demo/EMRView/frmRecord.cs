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
using System.Drawing.Printing;
using System.Runtime.Remoting.Channels;

namespace EMRView
{
    public delegate void DeItemInsertEventHandler(HCEmrView aEmrView, HCSection aSection,
        HCCustomData aData, HCCustomItem aItem);

    public delegate bool DeItemPopupEventHandler(DeItem aDeItem);

    public delegate string DeItemGetSyncValue(int aDesID, DeItem aDeItem);

    public partial class frmRecord : Form
    {
        private bool FPopupFormShow = false;  // 防止PopupForm在不必要时创建
        private bool FMouseInElementFire = false;
        private HCEmrView FEmrView;
        private frmRecordPop frmRecordPop;
        private EventHandler FOnSave, FOnSaveStructure, FOnChangedSwitch, FOnReadOnlySwitch;
        private DeItemInsertEventHandler FOnInsertDeItem;
        private DeItemSetTextEventHandler FOnSetDeItemText;
        private DeItemPopupEventHandler FOnDeItemPopup;
        private EventHandler FOnPrintPreview;
        private DeItemGetSyncValue FOnDeItemGetSyncValue;

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
                    FEmrView.SetDataDeGroupText(vTopData, vDomain.BeginNo, vText);
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

            PrintToolVisible = false;

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
                FEmrView.MouseDown += DoEmrViewMouseDown;
                FEmrView.MouseUp += DoEmrViewMouseUp;
                FEmrView.OnSectionDrawItemMouseMove += DoSectionDrawItemMouseMove;
                FEmrView.OnCaretChange = DoCaretChange;
                FEmrView.OnVerScroll = DoVerScroll;
                FEmrView.OnChangedSwitch = DoChangedSwitch;
                FEmrView.OnSectionReadOnlySwitch = DoReadOnlySwitch;
                FEmrView.OnCanNotEdit = DoCanNotEdit;
                FEmrView.OnSectionPaintPaperBefor = DoPaintPaperBefor;
                FEmrView.OnSectionInsertTextBefor = DoInsertTextBefor;
                FEmrView.OnPasteRequest = DoPasteRequest;
                FEmrView.OnSyntaxPaint = DoSyntaxPaint;
                FEmrView.ContextMenuStrip = this.pmView;
                #if VIEWTOOL
                FEmrView.OnTableToolPropertyClick = mniTableProperty_Click;
                #endif
                //
                this.Controls.Add(FEmrView);
                FEmrView.Dock = DockStyle.Fill;
                FEmrView.Show();
                FEmrView.BringToFront();
            }
        }

        /// <summary> 遍历处理痕迹隐藏或显示 </summary>
        private void DoHideTraceTraverse(HCCustomData aData, int aItemNo, int aTags, Stack<HCDomainInfo> aDomainStack, ref bool aStop)
        {
            if (!(aData.Items[aItemNo] is DeItem))  // 只对元素生效
                return;

            DeItem vDeItem = aData.Items[aItemNo] as DeItem;
            if (vDeItem.StyleEx == StyleExtra.cseDel)
                vDeItem.Visible = !(aTags == TTravTag.HideTrace);  // 隐藏/显示痕迹
        }

        /// <summary> 设置当前是否隐藏痕迹 </summary>
        private void SetHideTrace(bool value)
        {
            if (FEmrView.HideTrace != value)
            {
                FEmrView.HideTrace = value;
                HashSet<SectionArea> vAreas = new HashSet<SectionArea>();
                vAreas.Add(SectionArea.saPage);

                if (value)
                {
                    //FEmrView.AnnotatePre.Visible = false;
                    TraverseElement(DoHideTraceTraverse, vAreas, TTravTag.HideTrace);
                }
                else
                {
                    //if ((FEmrView.TraceCount > 0) && (!FEmrView.AnnotatePre.Visible))
                    //    FEmrView.AnnotatePre.Visible = true;
                    TraverseElement(DoHideTraceTraverse, vAreas, 0);
                }

                if (value && (!FEmrView.ReadOnly))
                    FEmrView.ReadOnly = true;
            }
        }

        /// <summary> 文档光标位置发生变化时触发 </summary>
        private void DoCaretChange(object sender, EventArgs e)
        {
            CurTextStyleChange(FEmrView.CurStyleNo);
            CurParaStyleChange(FEmrView.CurParaNo);
        }

        /// <summary> 文档变动状态发生变化时触发 </summary>
        private void DoChangedSwitch(object sender, EventArgs e)
        {
            if (FOnChangedSwitch != null)
                FOnChangedSwitch(this, e);
        }

        /// <summary> 文档编辑时只读或当前位置不可编辑时触发 </summary>
        private void DoCanNotEdit(object sender, EventArgs e)
        {
            MessageBox.Show("当前位置只读、不可编辑！");
        }

        /// <summary> 文档只读状态发生变化时触发 </summary>
        private void DoReadOnlySwitch(object sender, EventArgs e)
        {
            if (FOnReadOnlySwitch != null)
                FOnReadOnlySwitch(sender, e);
        }

        /// <summary> 文档垂直滚动条滚动时触发 </summary>
        private void DoVerScroll(object sender, EventArgs e)
        {
            if (FPopupFormShow)
            {
                PopupForm().Close();
                FPopupFormShow = false;
            }
        }

        /// <summary> 节整页绘制前事件 </summary>
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

        private bool DoInsertTextBefor(HCCustomData aData, int aItemNo, int aOffset, string aText)
        {
            HCCustomItem vItem = aData.Items[aItemNo];
            if (vItem is DeItem)
            {
                DeItem vDeItem = vItem as DeItem;
                if (vDeItem.IsElement && !vDeItem.AllocValue && vItem.IsSelectComplate)  // 数据元没赋过值且全选中了（无弹出框时处理为全选中、手动全选中）
                {
                    FEmrView.UndoGroupBegin();
                    try
                    {
                        FEmrView.SetActiveItemText(aText);
                        (aData as HCRichData).UndoItemMirror(aItemNo, aOffset);
                        vDeItem.Propertys.Remove(DeProp.CMVVCode);
                    }
                    finally
                    {
                        FEmrView.UndoGroupEnd();
                    }

                    return false;
                }
            }

            return true;
        }

        private void DoSyntaxPaint(HCCustomData aData, int aItemNo, string aDrawText, EmrSyntax aSyntax, RECT aRect, HCCanvas aCanvas)
        {
            if (aSyntax.Problem == EmrSyntaxProblem.espContradiction)  // 矛盾
                aCanvas.Brush.Color = Color.Red;
            else
                aCanvas.Brush.Color = Color.Blue;

            aCanvas.FillRect(aRect);
        }

        private bool DoPasteRequest(int aFormat)
        {
            return true;  // 控制能否粘贴指定格式的内容
        }

        /// <summary> 设置当前数据元的文本内容，为能提前预处理一下DeItem，所以取一下DeItem </summary>
        private void DoSetActiveDeItemText(DeItem aDeItem, string aText, ref bool aReject)
        {
            if (FOnSetDeItemText != null)
            {
                string vText = aText;
                FOnSetDeItemText(this, aDeItem, ref vText, ref aReject);
                if (!aReject)
                    FEmrView.SetActiveItemText(vText);
            }
            else
                FEmrView.SetActiveItemText(aText);
        }

        /// <summary> 设置当前数据元的内容为扩展内容 </summary>
        private void DoSetActiveDeItemExtra(DeItem aDeItem, Stream aStream)
        {
            FEmrView.SetActiveItemExtra(aStream);
        }

        private bool ActiveDeItemSync(DeItem activeDeItem)
        {
            bool vResult = false;
            DeItem vSameDeItem = FEmrView.FindSameDeItem(activeDeItem);
            if (vSameDeItem != null)
            {
                activeDeItem[DeProp.CMVVCode] = vSameDeItem[DeProp.CMVVCode];
                FEmrView.SetActiveItemText(vSameDeItem.Text);
                vResult = true;
            }

            return vResult;
        }

        private void DoDeComboboxGetItem(object sender, EventArgs e)
        {
            DeCombobox vCombobox = sender as DeCombobox;
            //if (DoDeItemPopup(vCombobox))
            PopupForm().PopupDeCombobox(vCombobox);
        }

        /// <summary> 当前位置文本样式和上一位置不一样时事件 </summary>
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

        /// <summary> 当前位置段样式和上一位置不一样时事件 </summary>
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

        /// <summary> 获取数据元值处理窗体 </summary>
        private frmRecordPop PopupForm()
        {
            frmRecordPop = new frmRecordPop();
            frmRecordPop.OnSetActiveItemText = DoSetActiveDeItemText;
            frmRecordPop.OnSetActiveItemExtra = DoSetActiveDeItemExtra;

            return frmRecordPop;
        }

        /// <summary> 据元值处理窗体关闭事件 </summary>
        private void PopupFormClose(object sender, EventArgs e)
        {
            if ((frmRecordPop != null) && frmRecordPop.Visible)
                frmRecordPop.Close();
        }

        private void CheckPrintHeaderFooterPosition()
        {
            if (tlbPrint.Visible)
            {
                cbxPrintHeader.Top = tlbPrint.Top + 4;
                cbxPrintHeader.BringToFront();

                cbxPrintFooter.Top = tlbPrint.Top + 4;
                cbxPrintFooter.BringToFront();
            }
        }

        private void SetPrintToolVisible(bool value)
        {
            if (value)
            {
                cbbPrinter.Items.Clear();
                PrintDocument print = new PrintDocument();
                string sDefault = print.PrinterSettings.PrinterName;  // 默认打印机名

                foreach (string sPrint in PrinterSettings.InstalledPrinters)  // 获取所有打印机名称
                {
                    cbbPrinter.Items.Add(sPrint);
                    if (sPrint == sDefault)
                        cbbPrinter.SelectedIndex = cbbPrinter.Items.IndexOf(sPrint);
                }
            }

            tlbPrint.Visible = value;
            cbxPrintHeader.Visible = value;
            cbxPrintFooter.Visible = value;

            CheckPrintHeaderFooterPosition();
        }

        private void SetEditToolVisible(bool value)
        {
            tlbEditTool.Visible = value;
            CheckPrintHeaderFooterPosition();
        }

        protected bool DoDeItemPopup(DeItem aDeItem)
        {
            if (FOnDeItemPopup != null)
                return FOnDeItemPopup(aDeItem);
            else
                return true;
        }

        protected void DoSectionDrawItemMouseMove(object sender, HCCustomData data,
            int itemNo, int offset, int drawItemNo, MouseEventArgs e)
        {
            FMouseInElementFire = false;
            if (data.Items[itemNo].StyleNo < HCStyle.Null)
                return;

            if (!(data.Items[itemNo] as DeItem).IsElement)
                return;

            string vText = data.GetDrawItemText(drawItemNo);
            int vLen = vText.Length;
            if (vLen == 1)
            {
                RECT vRect = new RECT(data.DrawItems[drawItemNo].Rect);
                vRect.Offset(-vRect.Left, -vRect.Top);
                int vWf = FEmrView.Style.TempCanvas.TextWidth(vText);
                if ((e.X > vWf / 3) && (e.X < vRect.Right - vWf / 3))
                {
                    HC.View.HC.GCursor = Cursors.Arrow;
                    FMouseInElementFire = true;
                }
            }
            else
            if (vLen > 1)
            {
                RECT vRect = new RECT(data.DrawItems[drawItemNo].Rect);
                vRect.Offset(-vRect.Left, -vRect.Top);
                FEmrView.Style.TextStyles[data.Items[itemNo].StyleNo].ApplyStyle(FEmrView.Style.TempCanvas);
                int vWf = FEmrView.Style.TempCanvas.TextWidth(vText[0]);
                int vWl = FEmrView.Style.TempCanvas.TextWidth(vText[vLen - 1]);
                if ((e.X > vWf / 2) && (e.X < vRect.Right - vWl / 2))
                {
                    HC.View.HC.GCursor = Cursors.Arrow;
                    FMouseInElementFire = true;
                }
            }
        }

        protected void DoEmrViewMouseDown(object sender, MouseEventArgs e)
        {
            PopupFormClose(this, null);
            //FMouseDownTick = Environment.TickCount;
        }

        protected void DoEmrViewMouseUp(object sender, MouseEventArgs e)
        {
            HCCustomItem vActiveItem = FEmrView.GetTopLevelItem();
            if (vActiveItem is DeItem)
            {
                DeItem vDeItem = vActiveItem as DeItem;
                if (FEmrView.ActiveSection.ActiveData.ReadOnly || vDeItem.EditProtect)
                    return;

                if (vDeItem.StyleEx != StyleExtra.cseNone)
                { 
                    
                }
                else
                if (vDeItem.Active
                    && (vDeItem[DeProp.Index] != "")
                    && (!vDeItem.IsSelectComplate)
                    && (!vDeItem.IsSelectPart)
                    && (FMouseInElementFire)
                    )
                {
                    if (((Control.ModifierKeys & Keys.Control) == Keys.Control) && ActiveDeItemSync(vDeItem))
                        return;

                    POINT vPt = FEmrView.GetTopLevelDrawItemViewCoord();  // 得到相对EmrView的坐标
                    HCCustomDrawItem vActiveDrawItem = FEmrView.GetTopLevelDrawItem();
                    RECT vDrawItemRect = HC.View.HC.Bounds(vPt.X, vPt.Y, vActiveDrawItem.Rect.Width, vActiveDrawItem.Rect.Height);

                    if (HC.View.HC.PtInRect(vDrawItemRect, new POINT(e.X, e.Y)))
                    {
                        vPt.Y = vPt.Y + FEmrView.ZoomIn(vActiveDrawItem.Height);
                        //vPt.Offset(FEmrView.Left, FEmrView.Top);
                        HC.Win32.User.ClientToScreen(FEmrView.Handle, ref vPt);

                        if (DoDeItemPopup(vDeItem))
                        {
                            if (!PopupForm().PopupDeItem(vDeItem, vPt))  // 不用弹出框处理值时，判断首次输入直接替换原内容
                            {
                                FPopupFormShow = false;
                                HCViewData vData = FEmrView.ActiveSectionTopLevelData() as HCViewData;
                                if (vData.SelectExists())
                                    return;

                                if (!vDeItem.AllocValue)  // 没有处理过值
                                {
                                    vData.SetSelectBound(vData.SelectInfo.StartItemNo, 0,
                                        vData.SelectInfo.StartItemNo, vData.GetItemOffsetAfter(vData.SelectInfo.StartItemNo), false);
                                }
                            }
                            else
                                FPopupFormShow = true;
                        }
                    }
                }
            }
        }

        /// <summary> 病历有新的Item插入时触发 </summary>
        protected void DoItemInsert(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            if (aItem is DeCombobox)
                (aItem as DeCombobox).OnPopupItem = DoDeComboboxGetItem;

            if (FOnInsertDeItem != null)
                FOnInsertDeItem(FEmrView, sender as HCSection, aData, aItem);
        }

        /// <summary> 调用保存病历方法 </summary>
        protected void DoSave()
        {
            if (FOnSave != null)
                FOnSave(this, null);
        }

        /// <summary> 调用保存病历结构方法 </summary>
        protected void DoSaveStructure()
        {
            if (FOnSaveStructure != null)
                FOnSaveStructure(this, null);
        }

        public object ObjectData;

        public frmDataElement FfrmDataElement;

        /// <summary> 隐藏工具栏 </summary>
        public void HideToolbar()
        {
            tlbEditTool.Visible = false;
        }

        /// <summary> 插入一个数据元(文本形式) </summary>
        /// <param name="aIndex">数据元唯一标识</param>
        /// <param name="aName">数据元名称</param>
        public DeItem InsertDeItem(string aIndex, string aName)
        {
            if ((aIndex == "") || (aName == ""))
            {
                MessageBox.Show("要插入的数据元索引和名称不能为空！");
                return null;
            }

            DeItem vDeItem = FEmrView.NewDeItem(aName);
            vDeItem[DeProp.Index] = aIndex;
            vDeItem[DeProp.Name] = aName;
            FEmrView.InsertDeItem(vDeItem);
            return vDeItem;
        }

        /// <summary> 插入一个数据组 </summary>
        public void InsertDeGroup(string aIndex, string aName)
        {
            if ((aIndex == "") || (aName == ""))
            {
                MessageBox.Show("要插入的数据组索引和名称不能为空！");
                return;
            }

            using (DeGroup vDeGroup = new DeGroup(FEmrView.ActiveSectionTopLevelData()))
            {
                vDeGroup[DeProp.Index] = aIndex;
                vDeGroup[DeProp.Name] = aName;
                FEmrView.InsertDeGroup(vDeGroup);
            }
        }

        /// <summary> 插入一个数据元(Edit形式) </summary>
        public DeEdit InsertDeEdit(string aIndex, string aName)
        {
            if ((aIndex == "") || (aName == ""))
            {
                MessageBox.Show("要插入的Edit索引和名称不能为空！");
                return null;
            }

            DeEdit vDeEdit = new DeEdit(FEmrView.ActiveSectionTopLevelData(), aName);
            vDeEdit[DeProp.Index] = aIndex;
            vDeEdit[DeProp.Name] = aName;
            FEmrView.InsertItem(vDeEdit);
            return vDeEdit;
        }

        /// <summary> 插入一个数据元(Combobox形式) </summary>
        public DeCombobox InsertDeCombobox(string aIndex, string aName)
        {
            if ((aIndex == "") || (aName == ""))
            {
                MessageBox.Show("要插入的Combobox索引和名称不能为空！");
                return null;
            }

            DeCombobox vCombobox = new DeCombobox(FEmrView.ActiveSectionTopLevelData(), aName);
            vCombobox.SaveItem = false;
            vCombobox[DeProp.Index] = aIndex;
            vCombobox[DeProp.Name] = aName;
            FEmrView.InsertItem(vCombobox);
            return vCombobox;
        }

        /// <summary> 插入一个数据元(DateTime形式) </summary>
        public DeDateTimePicker InsertDeDateTime(string aIndex, string aName)
        {
            if ((aIndex == "") || (aName == ""))
            {
                MessageBox.Show("要插入的DateTiem索引和名称不能为空！");
                return null;
            }

            DeDateTimePicker vDateTime = new DeDateTimePicker(FEmrView.ActiveSectionTopLevelData(), DateTime.Now);
            vDateTime[DeProp.Index] = aIndex;
            vDateTime[DeProp.Name] = aName;
            FEmrView.InsertItem(vDateTime);
            return vDateTime;
        }

        /// <summary> 插入一个数据元(RadioGroup形式) </summary>
        public DeRadioGroup InsertDeRadioGroup(string aIndex, string aName)
        {
            if ((aIndex == "") || (aName == ""))
            {
                MessageBox.Show("要插入的RadioGroup索引和名称不能为空！");
                return null;
            }

            DeRadioGroup vRadioGroup = new DeRadioGroup(FEmrView.ActiveSectionTopLevelData());
            vRadioGroup[DeProp.Index] = aIndex;
            vRadioGroup[DeProp.Name] = aName;
            // 取数据元的选项，选项太多时提示是否都插入
            vRadioGroup.AddItem("选项1");
            vRadioGroup.AddItem("选项2");
            vRadioGroup.AddItem("选项3");
            FEmrView.InsertItem(vRadioGroup);
            return vRadioGroup;
        }

        public DeFloatBarCodeItem InsertDeFloatBarCode(string aIndex, string aName)
        {
            if (aIndex == "")
            {
                MessageBox.Show("要插入的FloatBarCode索引不能为空！！");
                return null;
            }

            DeFloatBarCodeItem vResult = new DeFloatBarCodeItem(FEmrView.ActiveSection.ActiveData);
            vResult[DeProp.Index] = aIndex;
            FEmrView.InsertFloatItem(vResult);
            return vResult;
        }

        public DeImageItem InsertDeImage(string aIndex, string aName)
        {
            if (aIndex == "")
            {
                MessageBox.Show("要插入的DeImage索引不能为空！！");
                return null;
            }

            DeImageItem vResult = new DeImageItem(FEmrView.ActiveSection.ActiveData);
            vResult[DeProp.Index] = aIndex;
            FEmrView.InsertItem(vResult);
            return vResult;
        }

        /// <summary> 插入一个数据元(CheckBox形式) </summary>
        public DeCheckBox InsertDeCheckBox(string aIndex, string aName)
        {
            if ((aIndex == "") || (aName == ""))
            {
                MessageBox.Show("要插入的CheckBox索引和名称不能为空！");
                return null;
            }

            DeCheckBox vCheckBox = new DeCheckBox(FEmrView.ActiveSectionTopLevelData(), aName, false);
            vCheckBox[DeProp.Index] = aIndex;
            vCheckBox[DeProp.Name] = aName;
            FEmrView.InsertItem(vCheckBox);
            return vCheckBox;
        }

        /// <summary> 遍历文档指定Data的Item </summary>
        /// <param name="aTravEvent">每遍历到一个Item时触发的事件</param>
        /// <param name="aAreas">要遍历的Data</param>
        /// <param name="aTag">遍历标识</param>
        public void TraverseElement(TraverseItemEventHandle aTravEvent, HashSet<SectionArea> aAreas = null, int aTag = 0)
        {
            if (aTravEvent == null)
                return;

            HashSet<SectionArea> vArea = aAreas;
            if (vArea == null)
            {
                vArea = new HashSet<SectionArea>();
                vArea.Add(SectionArea.saHeader);
                vArea.Add(SectionArea.saFooter);
                vArea.Add(SectionArea.saPage);
            }
            else
            if (vArea.Count == 0)
                return;

            HCItemTraverse vItemTraverse = new HCItemTraverse();
            vItemTraverse.Tag = aTag;
            vItemTraverse.Areas = vArea;
            vItemTraverse.Process = aTravEvent;
            FEmrView.TraverseItem(vItemTraverse);

            FEmrView.FormatData();
        }

        /// <summary>
        /// 将文档每一页保存为图片
        /// </summary>
        /// <param name="aPath">图片路径</param>
        /// <param name="aPrefix">图片名称前缀</param>
        /// <param name="aImageType">图片格式</param>
        public void SaveToImage(string aPath, string aPrefix, string aImageType = "PNG")
        {
            //if (FEmrView.TraceCount > 0)
            //    this.HideTrace = true;  // 隐藏痕迹
            FEmrView.SaveToImage(aPath, aPrefix, aImageType);
        }

        /// <summary> 病历编辑器 </summary>
        public HCEmrView EmrView
        {
            get { return FEmrView; }
        }

        /// <summary> 保存病历时调用的方法 </summary>
        public EventHandler OnSave
        {
            get { return FOnSave; }
            set { FOnSave = value; }
        }

        /// <summary> 保存病历结构时调用的方法 </summary>
        public EventHandler OnSaveStructure
        {
            get { return FOnSaveStructure; }
            set { FOnSaveStructure = value; }
        }

        /// <summary> 文档Change状态切换时调用的方法 </summary>
        public EventHandler OnChangedSwitch
        {
            get { return FOnChangedSwitch; }
            set { FOnChangedSwitch = value; }
        }

        /// <summary> 节只读属性有变化时调用的方法 </summary>
        public EventHandler OnReadOnlySwitch
        {
            get { return FOnReadOnlySwitch; }
            set { FOnReadOnlySwitch = value; }
        }

        /// <summary> 节有新的Item插入时调用的方法 </summary>
        public DeItemInsertEventHandler OnInsertDeItem
        {
            get { return FOnInsertDeItem; }
            set { FOnInsertDeItem = value; }
        }

        public DeItemSetTextEventHandler OnSetDeItemText
        {
            get { return FOnSetDeItemText; }
            set { FOnSetDeItemText = value; }
        }

        public DeItemPopupEventHandler OnDeItemPopup
        {
            get { return FOnDeItemPopup; }
            set { FOnDeItemPopup = value; }
        }

        public EventHandler OnPrintPreview
        {
            get { return FOnPrintPreview; }
            set { FOnPrintPreview = value; }
        }

        public DeItemGetSyncValue OnDeItemGetSyncValue
        {
            get { return FOnDeItemGetSyncValue; }
            set { FOnDeItemGetSyncValue = value; }
        }


        /// <summary> 复制内容前触发 </summary>
        public HCCopyPasteEventHandler OnCopyRequest
        {
            get { return FEmrView.OnCopyRequest; }
            set { FEmrView.OnCopyRequest = value; }
        }

        /// <summary> 粘贴内容前触发 </summary>
        public HCCopyPasteEventHandler OnPasteRequest
        {
            get { return FEmrView.OnPasteRequest; }
            set { FEmrView.OnPasteRequest = value; }
        }

        public HCCopyPasteStreamEventHandler OnCopyAsStream
        {
            get { return FEmrView.OnCopyAsStream; }
            set { FEmrView.OnCopyAsStream = value; }
        }

        public HCCopyPasteStreamEventHandler OnPasteFromStream
        {
            get { return FEmrView.OnPasteFromStream; }
            set { FEmrView.OnPasteFromStream = value; }
        }

        public bool PrintToolVisible
        {
            get { return tlbPrint.Visible; }
            set { SetPrintToolVisible(value); }
        }

        public DataDomainItemNoEventHandler OnSyntaxCheck
        {
            get { return FEmrView.OnSyntaxCheck; }
            set { FEmrView.OnSyntaxCheck = value; }
        }

        public SyntaxPaintEventHandler OnSyntaxPaint
        {
            get { return FEmrView.OnSyntaxPaint; }
            set { FEmrView.OnSyntaxPaint = value; }
        }

        public bool EditToolVisible
        {
            get { return tlbEditTool.Visible; }
            set { SetEditToolVisible(value); }
        }

        public bool HideTrace
        {
            get { return FEmrView.HideTrace; }
            set { SetHideTrace(value); }
        }
        private void pmView_Opening(object sender, CancelEventArgs e)
        {
            HCCustomData vActiveData = FEmrView.ActiveSection.ActiveData;
            HCCustomFloatItem vActiveFloatItem = (vActiveData as HCSectionData).GetActiveFloatItem();
            bool vReadOnly = (vActiveData as HCRichData).ReadOnly;

            if (vActiveFloatItem != null)
            {
                for (int i = 0; i < pmView.Items.Count; i++)
                    pmView.Items[i].Visible = false;

                if (!vReadOnly)
                {
                    mniFloatItemProperty.Visible = FEmrView.DesignModeEx;
                    if (vActiveFloatItem is DeFloatBarCodeItem)
                      mniFloatItemProperty.Text = "浮动条码";
                }

                return;
            }
            else
                mniFloatItemProperty.Visible = false;

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
                    if ((vTopData as HCRichData).ReadOnly)
                        vReadOnly = true;
                }
                else
                    break;
            }

            if (vTopData == null)
                vTopData = vActiveData;

            if (vReadOnly)
            {
                for (int i = 0; i < pmView.Items.Count; i++)
                    pmView.Items[i].Visible = false;

                mniCopy.Visible = vTopData.SelectExists();
                return;
            }

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
            mniCopyProtect.Visible = false;

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
                        mniDeleteProtect.Text = "运行时禁止编辑";
                        mniDeleteProtect.Visible = true;

                        mniCopyProtect.Text = "运行时禁止复制";
                        mniCopyProtect.Visible = true;
                    }
                    else
                    {
                        if ((vTopItem as DeItem).EditProtect)
                            mniDeleteProtect.Text = "运行时允许编辑";
                        else
                            mniDeleteProtect.Text = "运行时禁止编辑";

                        mniDeleteProtect.Visible = true;

                        if ((vTopItem as DeItem).CopyProtect)
                            mniCopyProtect.Text = "运行时允许复制";
                        else
                            mniCopyProtect.Text = "运行时禁止复制";

                        mniCopyProtect.Visible = true;
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
            HCCustomItem vControlItem = FEmrView.ActiveSectionTopLevelData().GetActiveItem();
            if (vControlItem is DeCombobox)  // ComboboxItem
            {
                frmDeCombobox vFrmDeCombobox = new frmDeCombobox();
                vFrmDeCombobox.SetHCView(FEmrView, vControlItem as DeCombobox);
            }
            else
            if (vControlItem is DeRadioGroup)  // DeRadioGroup
            {
                frmDeRadioGroup vFrmDeRadioGroup = new frmDeRadioGroup();
                vFrmDeRadioGroup.SetHCView(FEmrView, vControlItem as DeRadioGroup);
            }
            else
            {
                frmDeControlProperty vFrmDeControlProperty = new frmDeControlProperty();
                vFrmDeControlProperty.SetHCView(FEmrView);
            }
        }

        private void mniDeleteProtect_Click(object sender, EventArgs e)
        {
            HCCustomData vTopData = FEmrView.ActiveSectionTopLevelData();

            if (vTopData.SelectExists())
            {
                for (int i = vTopData.SelectInfo.StartItemNo; i <= vTopData.SelectInfo.EndItemNo; i++)
                {
                    if (vTopData.Items[i].StyleNo < HCStyle.Null)
                    {
                        MessageBox.Show("禁止编辑只能应用于文本内容，选中内容中存在非文本对象！");
                        return;
                    }
                }

                if ((vTopData.SelectInfo.StartItemNo == vTopData.SelectInfo.EndItemNo)
                    && (vTopData.SelectInfo.StartItemOffset == 0)
                    && (vTopData.SelectInfo.EndItemOffset == vTopData.GetItemOffsetAfter(vTopData.SelectInfo.StartItemNo)))  // 在同一个Item
                {
                    (vTopData.Items[vTopData.SelectInfo.StartItemNo] as DeItem).EditProtect = true;
                    return;
                }

                for (int i = vTopData.SelectInfo.StartItemNo; i <= vTopData.SelectInfo.EndItemNo; i++)
                        (vTopData.Items[i] as DeItem).EditProtect = false;

                string vS = vTopData.GetSelectText();
                vS = vS.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                DeItem vDeItem = FEmrView.NewDeItem(vS);
                vDeItem.EditProtect = true;
                FEmrView.InsertDeItem(vDeItem);
            }
            else
            {
                DeItem vDeItem = vTopData.GetActiveItem() as DeItem;
                vDeItem.EditProtect = !vDeItem.EditProtect;
                FEmrView.ActiveItemReAdaptEnvironment();
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
            vOpenDlg.Filter = "图像文件|*.bmp; *.jpg; *.jpeg; *.png|Windows Bitmap|*.bmp|JPEG 文件|*.jpg; *.jpge|可移植网络图形|*.png";
            FEmrView.Enabled = false;
            try
            {
                if (vOpenDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (vOpenDlg.FileName != "")
                    {
                        HCRichData vTopData = FEmrView.ActiveSectionTopLevelData() as HCRichData;
                        DeImageItem vImageItem = new DeImageItem(vTopData);
                        vImageItem.LoadGraphicFile(vOpenDlg.FileName);
                        vImageItem.RestrainSize(vTopData.Width, vImageItem.Height);
                        Application.DoEvents();
                        FEmrView.InsertItem(vImageItem);
                    }
                }
            }
            finally
            {
                FEmrView.Enabled = true;
            }
        }

        private void mniInsertGif_Click(object sender, EventArgs e)
        {
            OpenFileDialog vOpenDlg = new OpenFileDialog();
            vOpenDlg.Filter = "GIF动画文件|*.gif";
            FEmrView.Enabled = false;
            try
            {
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
            finally
            {
                FEmrView.Enabled = true;
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
            //vFrmDataElement.OnInsertAsDE = InsertDeItem;
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
            vOpenDlg.Filter = "支持的文件|*" + HC.View.HC.HC_EXT + "; *.xml|HCView (*.hcf)|*" + HC.View.HC.HC_EXT + "|HCView xml (*.xml)|*.xml";
            if (vOpenDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (vOpenDlg.FileName != "")
                {
                    Application.DoEvents();
                    string vExt = System.IO.Path.GetExtension(vOpenDlg.FileName);
                    if (vExt == HC.View.HC.HC_EXT)
                        FEmrView.LoadFromFile(vOpenDlg.FileName);
                    else
                    if (vExt == ".xml")
                        FEmrView.LoadFromXml(vOpenDlg.FileName);
                }
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
            FEmrView.PrintCurPageByActiveLine(cbbPrinter.Text, false, false);
        }

        private void mniPrintSelect_Click(object sender, EventArgs e)
        {
            FEmrView.PrintCurPageSelected(cbbPrinter.Text, false, false);
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

        private void MniOdd_Click(object sender, EventArgs e)
        {
            FEmrView.PrintOdd(cbbPrinter.Text);
        }

        private void MniEven_Click(object sender, EventArgs e)
        {
            FEmrView.PrintEven(cbbPrinter.Text);
        }

        private void BtnPrintAll_Click(object sender, EventArgs e)
        {
            FEmrView.Print(cbbPrinter.Text);
        }

        private void BtnPrintCurLine_Click(object sender, EventArgs e)
        {
            PrintDocument vPrinter = new PrintDocument();
            vPrinter.PrinterSettings.PrinterName = cbbPrinter.Text;
            FEmrView.PrintCurPageByActiveLine(cbbPrinter.Text, cbxPrintHeader.Checked, cbxPrintFooter.Checked);
        }

        private void BtnPrintSelect_Click(object sender, EventArgs e)
        {
            PrintDocument vPrinter = new PrintDocument();
            vPrinter.PrinterSettings.PrinterName = cbbPrinter.Text;
            FEmrView.PrintCurPageSelected(cbbPrinter.Text, cbxPrintHeader.Checked, cbxPrintFooter.Checked);
        }

        private void BtnPrintRange_Click(object sender, EventArgs e)
        {
            int vStartPage = int.Parse(tbxPageStart.Text);
            if (vStartPage < 0)
                vStartPage = 0;

            int vEndPage = int.Parse(tbxPageEnd.Text);
            if (vEndPage > FEmrView.PageCount - 1)
                vEndPage = FEmrView.PageCount - 1;

            FEmrView.Print(cbbPrinter.Text, vStartPage, vEndPage, 1);
        }

        private void MniViewFilm_Click(object sender, EventArgs e)
        {
            FEmrView.ViewModel = HCViewModel.hvmFilm;
            //FHRuler.Visible = true;
            //FVRuler.Visible = true;
        }

        private void MniViewPage_Click(object sender, EventArgs e)
        {
            FEmrView.ViewModel = HCViewModel.hvmPage;
            //FHRuler.Visible = False;
            //FVRuler.Visible = False;
        }

        private void MniInputHelp_Click(object sender, EventArgs e)
        {
            #if VIEWINPUTHELP
            FEmrView.InputHelpEnable = !FEmrView.InputHelpEnable;
            #endif
        }

        private void MniShapeLine_Click(object sender, EventArgs e)
        {
            HCFloatLineItem vFloatLineItem = new HCFloatLineItem(FEmrView.ActiveSection.ActiveData);
            FEmrView.InsertFloatItem(vFloatLineItem);
        }

        private void MniBarCode_Click(object sender, EventArgs e)
        {
            string vS = "123456897";
            HCBarCodeItem vHCBarCode = new HCBarCodeItem(FEmrView.ActiveSectionTopLevelData(), vS);
            FEmrView.InsertItem(vHCBarCode);
        }

        private void MniQRCode_Click(object sender, EventArgs e)
        {
            string vS = "HCView使用了DelphiZXingQRCode二维码控件";
            HCQRCodeItem vQRCode = new HCQRCodeItem(FEmrView.ActiveSectionTopLevelData(), vS);
            FEmrView.InsertItem(vQRCode);
        }

        private void MniCopyProtect_Click(object sender, EventArgs e)
        {
            HCCustomData vTopData = FEmrView.ActiveSectionTopLevelData();

            if (vTopData.SelectExists())
            {
                for (int i = vTopData.SelectInfo.StartItemNo; i <= vTopData.SelectInfo.EndItemNo; i++)
                {
                    if (vTopData.Items[i].StyleNo < HCStyle.Null)
                    {
                        MessageBox.Show("禁止编辑只能应用于文本内容，选中内容中存在非文本对象！");
                        return;
                    }
                }

                if ((vTopData.SelectInfo.StartItemNo == vTopData.SelectInfo.EndItemNo)
                    && (vTopData.SelectInfo.StartItemOffset == 0)
                    && (vTopData.SelectInfo.EndItemOffset == vTopData.GetItemOffsetAfter(vTopData.SelectInfo.StartItemNo)))  // 在同一个Item
                {
                    (vTopData.Items[vTopData.SelectInfo.StartItemNo] as DeItem).EditProtect = true;
                    return;
                }

                for (int i = vTopData.SelectInfo.StartItemNo; i <= vTopData.SelectInfo.EndItemNo; i++)
                    (vTopData.Items[i] as DeItem).EditProtect = false;

                string vS = vTopData.GetSelectText();
                vS = vS.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                DeItem vDeItem = FEmrView.NewDeItem(vS);
                vDeItem.CopyProtect = true;
                FEmrView.InsertDeItem(vDeItem);
            }
            else
            {
                DeItem vDeItem = vTopData.GetActiveItem() as DeItem;
                vDeItem.CopyProtect = !vDeItem.CopyProtect;
                FEmrView.ActiveItemReAdaptEnvironment();
            }
        }

        private void MniCellVTHL_Click(object sender, EventArgs e)
        {
            FEmrView.TableApplyContentAlign(HCContentAlign.tcaTopLeft);
        }

        private void MniCellVTHM_Click(object sender, EventArgs e)
        {
            FEmrView.TableApplyContentAlign(HCContentAlign.tcaTopCenter);
        }

        private void MniCellVTHR_Click(object sender, EventArgs e)
        {
            FEmrView.TableApplyContentAlign(HCContentAlign.tcaTopRight);
        }

        private void MniCellVMHL_Click(object sender, EventArgs e)
        {
            FEmrView.TableApplyContentAlign(HCContentAlign.tcaCenterLeft);
        }

        private void MniCellVMHM_Click(object sender, EventArgs e)
        {
            FEmrView.TableApplyContentAlign(HCContentAlign.tcaCenterCenter);
        }

        private void MniCellVMHR_Click(object sender, EventArgs e)
        {
            FEmrView.TableApplyContentAlign(HCContentAlign.tcaCenterRight);
        }

        private void MniCellVBHL_Click(object sender, EventArgs e)
        {
            FEmrView.TableApplyContentAlign(HCContentAlign.tcaBottomLeft);
        }

        private void MniCellVBHM_Click(object sender, EventArgs e)
        {
            FEmrView.TableApplyContentAlign(HCContentAlign.tcaBottomCenter);
        }

        private void MniCellVBHR_Click(object sender, EventArgs e)
        {
            FEmrView.TableApplyContentAlign(HCContentAlign.tcaBottomRight);
        }

        private void MniFloatBarCode_Click(object sender, EventArgs e)
        {
            DeFloatBarCodeItem vFloatBarCodeItem = new DeFloatBarCodeItem(FEmrView.ActiveSection.ActiveData);
            FEmrView.InsertFloatItem(vFloatBarCodeItem);
        }

        private void MniFloatItemProperty_Click(object sender, EventArgs e)
        {
            frmDeFloatItemProperty vfrmFloatItemProperty = new frmDeFloatItemProperty();
            vfrmFloatItemProperty.SetHCView(FEmrView);
        }

        private void frmRecord_Load(object sender, EventArgs e)
        {
            CheckPrintHeaderFooterPosition();
        }

        private void mniPrintPreview_Click(object sender, EventArgs e)
        {
            if (FOnPrintPreview != null)
                FOnPrintPreview(sender, e);
        }

        private void btnPrintCurLineToPage_Click(object sender, EventArgs e)
        {
            PrintDocument vPrinter = new PrintDocument();
            vPrinter.PrinterSettings.PrinterName = cbbPrinter.Text;
            FEmrView.PrintCurPageByActiveLine(cbbPrinter.Text, cbxPrintHeader.Checked, cbxPrintFooter.Checked);

            if (FEmrView.ActivePageIndex < FEmrView.PageCount - 1)
                FEmrView.Print(cbbPrinter.Text, FEmrView.ActivePageIndex + 1, FEmrView.PageCount - 1, 1);
        }

        private void mniReSyncDeItem_Click(object sender, EventArgs e)
        {
            if (FOnDeItemGetSyncValue != null)
            {
                HCCustomData vData = FEmrView.ActiveSectionTopLevelData();
                DeItem vDeItem = vData.GetActiveItem() as DeItem;  // 需保证点击处是DeItem
                string vValue = FOnDeItemGetSyncValue((this.ObjectData as RecordInfo).DesID, vDeItem);  // 取数据元的同步值
                if (vValue != "")
                {
                    bool vCancel = false;
                    this.DoSetActiveDeItemText(vDeItem, vValue, ref vCancel);
                }
            }
        }

        private void mniSyntax_Click(object sender, EventArgs e)
        {
            FEmrView.SyntaxCheck();
        }

        private void mniViewText_Click(object sender, EventArgs e)
        {
            FEmrView.ViewModel = HCViewModel.hvmEdit;
        }

        private void mniHideTrace_Click(object sender, EventArgs e)
        {
            SetHideTrace(!FEmrView.HideTrace);
        }

        private void btnFile_DropDownOpened(object sender, EventArgs e)
        {
            mniHideTrace.Visible = FEmrView.TraceCount > 0;

            if (mniHideTrace.Visible)
            {
                if (FEmrView.HideTrace)
                    mniHideTrace.Text = "显示痕迹";
                else
                    mniHideTrace.Text = "不显示痕迹";
            }

#if VIEWINPUTHELP
            if (FEmrView.InputHelpEnable)
                mniInputHelp.Text = "关闭辅助输入";
            else
                mniInputHelp.Text = "开启辅助输入";
#else
            mniInputHelp.Visible = false;
#endif
        }

        private void mniSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog vDlg = new SaveFileDialog();
            vDlg.Filter = "文件|*" + HC.View.HC.HC_EXT + "|HCView xml|*.xml" + "|pdf文件|*.pdf" + "|html页面|*.html";
            if (vDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
                            vExt = ".xml";
                            break;

                        case 3:
                            vExt = ".pdf";
                            break;

                        case 4:
                            vExt = ".html";
                            break;

                        default:
                            return;
                    }

                    if (System.IO.Path.GetExtension(vDlg.FileName) != vExt)
                        vDlg.FileName = vDlg.FileName + vExt;

                    switch (vDlg.FilterIndex)
                    {
                        case 1:
                            FEmrView.SaveToFile(vDlg.FileName);
                            break;

                        case 2:
                            FEmrView.SaveToXml(vDlg.FileName, Encoding.UTF8);
                            break;

                        case 3:
                            FEmrView.SaveToPDF(vDlg.FileName);
                            break;

                        case 4:
                            FEmrView.SaveToHtml(vDlg.FileName);
                            break;
                    }
                }
            }
        }
    }

    public static class TTravTag
    {
        public const byte WriteTraceInfo = 1;  // 遍历内容，为新痕迹增加痕迹信息
        public const byte HideTrace = 1 << 1;  // 隐藏痕迹内容
        //public const byte DataSetElement 1 << 2;  // 检查数据集需要的数据元

        public static bool Contains(int aTags, int aTag)
        {
            return (aTags & aTag) == aTag;
        }
    }
}
