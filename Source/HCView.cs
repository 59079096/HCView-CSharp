/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                 文档内容按页呈现控件                  }
{                                                       }
{*******************************************************/

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections.Generic;
using HC.Win32;
using System.IO;
using System.Xml;
using System.Reflection;

namespace HC.View
{
    public class HCView : UserControl
    {
        private string FFileName;
        private HCStyle FStyle;
        private List<HCSection> FSections;
        private HCUndoList FUndoList;
        private HCScrollBar FHScrollBar;
        private HCRichScrollBar FVScrollBar;
        private IntPtr FDC = IntPtr.Zero;
        private IntPtr FMemDC = IntPtr.Zero;
        private IntPtr FhImc = IntPtr.Zero;
        private int FActiveSectionIndex, FDisplayFirstSection, FDisplayLastSection, FUpdateCount;
        private Single FZoom;
        private bool FAutoZoom;  // 自动缩放
        private bool FShowAnnotation;  // 显示批注
        private bool FIsChanged;  // 是否发生了改变
        private HCAnnotates FAnnotates;  // 批注
        private HCViewModel FViewModel;  // 界面显示模式：页面、Web
        private HCPageScrollModel FPageScrollModel;  // 页面滚动显示模式：纵向、横向
        private HCCaret FCaret;
        private DataFormats.Format FHCExtFormat;
        private EventHandler FOnCaretChange, FOnVerScroll, FOnSectionCreateItem, FOnSectionReadOnlySwitch;
        private StyleItemEventHandler FOnSectionCreateStyleItem;
        private ItemNotifyEventHandler FOnSectionInsertItem;
        private DrawItemPaintEventHandler FOnSectionDrawItemPaintAfter, FOnSectionDrawItemPaintBefor;

        private SectionPagePaintEventHandler FOnSectionPaintHeader, FOnSectionPaintFooter, FOnSectionPaintPage,
          FOnSectionPaintWholePage;
        private PaintEventHandler FOnUpdateViewBefor, FOnUpdateViewAfter;

        private EventHandler FOnChange, FOnChangedSwitch;

        private void SetPrintBySectionInfo(PageSettings PageSettings, int ASectionIndex)
        {
            //PageSettings.PaperSize.Kind = (PaperKind)FSections[ASectionIndex].PaperSize;

            if (PageSettings.PaperSize.Kind == PaperKind.Custom)  // 自定义纸张
            {
                PageSettings.PaperSize.Height = (int)Math.Round(FSections[ASectionIndex].PaperHeight * 10); //纸长你可用变量获得纸张的长、宽。
                PageSettings.PaperSize.Width = (int)Math.Round(FSections[ASectionIndex].PaperWidth * 10);   //纸宽
                //vPDMode^.dmFields := vPDMode^.dmFields or DM_PAPERSIZE or DM_PAPERLENGTH or DM_PAPERWIDTH;
            }

            if (FSections[ASectionIndex].PageOrientation == PageOrientation.cpoPortrait)
                PageSettings.Landscape = false;
            else
                PageSettings.Landscape = true;
        }

        private int GetDisplayWidth()
        {
            if (FVScrollBar.Visible)
                return Width - FVScrollBar.Width;
            else
                return Width;
        }

        private int GetDisplayHeight()
        {
            if (FHScrollBar.Visible)
                return Height - FHScrollBar.Height;
            else
                return Height;
        }
        
        private bool GetSymmetryMargin()
        {
            return ActiveSection.SymmetryMargin;
        }
        
        private void SetSymmetryMargin(bool Value)
        {
            if (ActiveSection.SymmetryMargin != Value)
            {
                ActiveSection.SymmetryMargin = Value;
                FStyle.UpdateInfoRePaint();
                FStyle.UpdateInfoReCaret(false);
                DoMapChanged();
            }
        }

        private void DoVScrollChange(object Sender, ScrollCode ScrollCode, int ScrollPos)
        {
            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret(false);
            CheckUpdateInfo();
            if (FOnVerScroll != null)
                FOnVerScroll(this, null);
        }

        private HCCustomItem DoSectionCreateStyleItem(HCCustomData AData, int AStyleNo)
        {
            if (FOnSectionCreateStyleItem != null)
                return FOnSectionCreateStyleItem(AData, AStyleNo);
            else
                return null;
        }
        
        private HCSection NewDefaultSection()
        {
            HCSection Result = new HCSection(FStyle);
            // 创建节后马上赋值事件（保证后续插入表格等需要这些事件的操作可获取到事件）
            Result.OnDataChanged = DoSectionDataChanged;
            Result.OnCheckUpdateInfo = DoSectionDataCheckUpdateInfo;
            Result.OnCreateItem = DoSectionCreateItem;
            Result.OnCreateItemByStyle = DoSectionCreateStyleItem;
            Result.OnInsertItem = DoSectionInsertItem;
            Result.OnReadOnlySwitch = DoSectionReadOnlySwitch;
            Result.OnGetScreenCoord = DoSectionGetScreenCoord;
            Result.OnDrawItemPaintAfter = DoSectionDrawItemPaintAfter;
            Result.OnDrawItemPaintBefor = DoSectionDrawItemPaintBefor;
            Result.OnPaintHeader = DoSectionPaintHeader;
            Result.OnPaintFooter = DoSectionPaintFooter;
            Result.OnPaintPage = DoSectionPaintPage;
            Result.OnPaintWholePage = DoSectionPaintWholePage;
            Result.OnGetUndoList = DoSectionGetUndoList;

            return Result;
        }

        private RECT GetDisplayRect()
        {
            return HC.Bounds(0, 0, GetDisplayWidth(), GetDisplayHeight());
        }

        private void ReBuildCaret()
        {
            if (FCaret == null)
                return;

            if ((!this.Focused) || ((!Style.UpdateInfo.Draging) && ActiveSection.SelectExists()))
            {
                FCaret.Hide();
                return;
            }

            // 初始化光标信息，为处理表格内往外迭代，只能放在这里
            HCCaretInfo vCaretInfo = new HCCaretInfo();
            vCaretInfo.X = 0;
            vCaretInfo.Y = 0;
            vCaretInfo.Height = 0;
            vCaretInfo.Visible = true;
            
            ActiveSection.GetPageCaretInfo(ref vCaretInfo);
            if (!vCaretInfo.Visible)
            {
                FCaret.Hide();
                return;
            }
            FCaret.X = ZoomIn(GetSectionDrawLeft(FActiveSectionIndex) + vCaretInfo.X) - FHScrollBar.Position;
            FCaret.Y = ZoomIn(GetSectionTopFilm(FActiveSectionIndex) + vCaretInfo.Y) - FVScrollBar.Position;
            FCaret.Height = ZoomIn(vCaretInfo.Height);
            
            int vDisplayHeight = GetDisplayHeight();
            if (!FStyle.UpdateInfo.ReScroll)
            {
                if ((FCaret.X < 0) || (FCaret.X > GetDisplayWidth()))
                {
                    FCaret.Hide();
                    return;
                }

                if ((FCaret.Y + FCaret.Height < 0) || (FCaret.Y > vDisplayHeight))
                {
                    FCaret.Hide();
                    return;
                }
            }
            else  // 非滚动条(方向键、点击等)引起的光标位置变化
            {
                if (FCaret.Height < vDisplayHeight)
                {
                    if (FCaret.Y < 0)
                        FVScrollBar.Position = FVScrollBar.Position + FCaret.Y - HC.PagePadding;
                    else
                    if (FCaret.Y + FCaret.Height + HC.PagePadding > vDisplayHeight)
                        FVScrollBar.Position = FVScrollBar.Position + FCaret.Y + FCaret.Height + HC.PagePadding - vDisplayHeight;
                    
                    if (FCaret.X < 0)
                        FHScrollBar.Position = FHScrollBar.Position + FCaret.X - HC.PagePadding;
                    else
                    if (FCaret.X + HC.PagePadding > GetDisplayWidth())
                        FHScrollBar.Position = FHScrollBar.Position + FCaret.X + HC.PagePadding - GetDisplayWidth();
                }
            }

            if (FCaret.Y + FCaret.Height > vDisplayHeight)
                FCaret.Height = vDisplayHeight - FCaret.Y;
            
            FCaret.Show();
            DoCaretChange();
        }
        
        private void GetSectionByCrood(int X, int Y, ref int ASectionIndex)
        {
            ASectionIndex = -1;
            int vY = 0;
            for (int i = 0; i <= FSections.Count - 1; i++)
            {
                vY = vY + FSections[i].GetFilmHeight();
                if (vY > Y)
                {
                    ASectionIndex = i;
                    break;
                }
            }
            if ((ASectionIndex < 0) && (vY + HC.PagePadding >= Y))
                ASectionIndex = FSections.Count - 1;
        }

        private void SetZoom(Single Value)
        {
            if (FZoom != Value)
            {
                this.Focus();
                FZoom = Value;
                FStyle.UpdateInfoRePaint();
                FStyle.UpdateInfoReCaret(false);
                DoMapChanged();
            }
        }

        /// <summary> 删除不使用的文本样式 </summary>
        private void _DeleteUnUsedStyle(HashSet<SectionArea> AParts)  //  = (saHeader, saPage, saFooter)
        {
            for (int i = 0; i <= FStyle.TextStyles.Count - 1; i++)
            {
                FStyle.TextStyles[i].CheckSaveUsed = false;
                FStyle.TextStyles[i].TempNo = HCStyle.Null;
            }
            for (int i = 0; i <= FStyle.ParaStyles.Count - 1; i++)
            {
                FStyle.ParaStyles[i].CheckSaveUsed = false;
                FStyle.ParaStyles[i].TempNo = HCStyle.Null;
            }

            for (int i = 0; i <= FSections.Count - 1; i++)
                FSections[i].MarkStyleUsed(true, AParts);
            
            int vUnCount = 0;
            for (int i = 0; i <= FStyle.TextStyles.Count - 1; i++)
            {
                if (FStyle.TextStyles[i].CheckSaveUsed)
                    FStyle.TextStyles[i].TempNo = i - vUnCount;
                else
                    vUnCount++;
            }
            
            vUnCount = 0;
            for (int i = 0; i <= FStyle.ParaStyles.Count - 1; i++)
            {
                if (FStyle.ParaStyles[i].CheckSaveUsed)
                    FStyle.ParaStyles[i].TempNo = i - vUnCount;
                else
                    vUnCount++;
            }

            for (int i = 0; i <= FSections.Count - 1; i++)
                FSections[i].MarkStyleUsed(false, AParts);

            for (int i = FStyle.TextStyles.Count - 1; i >= 0; i--)
            {
                if (!FStyle.TextStyles[i].CheckSaveUsed)
                    FStyle.TextStyles.RemoveAt(i);
            }

            for (int i = FStyle.ParaStyles.Count - 1; i >= 0; i--)
            {
                if (!FStyle.ParaStyles[i].CheckSaveUsed)
                    FStyle.ParaStyles.RemoveAt(i);
            }
        }

        private int GetHScrollValue()
        {
            return FHScrollBar.Position;
        }

        private int GetVScrollValue()
        {
            return FVScrollBar.Position;
        }

        private bool GetShowLineActiveMark()
        {
            return FSections[0].PageData.ShowLineActiveMark;
        }

        private void SetShowLineActiveMark(bool Value)
        {
            for (int i = 0; i <= FSections.Count - 1; i++)
                FSections[i].PageData.ShowLineActiveMark = Value;

            UpdateView();
        }

        private bool GetShowLineNo()
        {
            return FSections[0].PageData.ShowLineNo;
        }

        private void SetShowLineNo(bool Value)
        {
            for (int i = 0; i <= FSections.Count - 1; i++)
                FSections[i].PageData.ShowLineNo = Value;

            UpdateView();
        }

        private bool GetShowUnderLine()
        {
            return FSections[0].PageData.ShowUnderLine;
        }

        private void SetShowUnderLine(bool Value)
        {
            for (int i = 0; i <= FSections.Count - 1; i++)
                FSections[i].PageData.ShowUnderLine = Value;

            UpdateView();
        }

        private bool GetReadOnly()
        {
            for (int i = 0; i <= FSections.Count - 1; i++)
            {
                if (!FSections[i].ReadOnly)
                {
                    return false;
                }
            }

            return true;
        }

        private void SetReadOnly(bool Value)
        {
            for (int i = 0; i <= FSections.Count - 1; i++)
                FSections[i].ReadOnly = Value;
        }

        // Imm
        private void UpdateImmPosition()
        {
            LOGFONT vLogFont = new LOGFONT();
            Imm.ImmGetCompositionFont(FhImc, ref vLogFont);
            vLogFont.lfHeight = 22;
            Imm.ImmSetCompositionFont(FhImc, ref vLogFont);
            // 告诉输入法当前光标位置信息
            COMPOSITIONFORM vCF = new COMPOSITIONFORM();
            vCF.ptCurrentPos = new POINT(FCaret.X, FCaret.Y + 5);  // 输入法弹出窗体位置
            vCF.dwStyle = 1;

            Rectangle vr = this.ClientRectangle;

            vCF.rcArea = new RECT(vr.Left, vr.Top, vr.Right, vr.Bottom);
            Imm.ImmSetCompositionWindow(FhImc, ref vCF);
        }  
     
        // =============== private end =============== //
         protected void DoCaretChange()
        {
             if (FOnCaretChange != null)
                 FOnCaretChange(this, null);
        }

        protected void DoSectionDataChanged(Object Sender, EventArgs e)
        {
            DoChange();
        }

        // 仅重绘和重建光标，不触发Change事件
        protected void DoSectionDataCheckUpdateInfo(Object Sender, EventArgs e)
        {
            if (FUpdateCount == 0)
                CheckUpdateInfo();
        }

        protected  void DoLoadFromStream(Stream AStream, HCStyle AStyle, LoadSectionProcHandler ALoadSectionProc)
        {
            AStream.Position = 0;
            string vFileExt = "";
            ushort vFileVersion = 0;
            byte vLan = 0;
            HC._LoadFileFormatAndVersion(AStream, ref vFileExt, ref vFileVersion, ref vLan);
            if (vFileExt != HC.HC_EXT)
                throw new Exception("加载失败，不是" + HC.HC_EXT + "文件！");



            DoLoadBefor(AStream, vFileVersion);  // 触发加载前事件
            AStyle.LoadFromStream(AStream, vFileVersion);  // 加载样式表
            ALoadSectionProc(vFileVersion);  // 加载节数量、节数据
            DoMapChanged();
        }

        protected void DoNewUndo(HCUndo Sender)
        {
            Sender.SectionIndex = FActiveSectionIndex;
            Sender.Data = ActiveSection.ActiveData;
        }

        protected void DoUndo(HCUndo Sender)
        {
            if (FActiveSectionIndex != Sender.SectionIndex)
                SetActiveSectionIndex(Sender.SectionIndex);

            ActiveSection.Undo(Sender);
        }

        protected void DoRedo(HCUndo Sender)
        {
            if (FActiveSectionIndex != Sender.SectionIndex)
                SetActiveSectionIndex(Sender.SectionIndex);

            ActiveSection.Redo(Sender);
        }

        /// <summary> 文档"背板"变动(数据无变化，如对称边距，缩放视图) </summary>
        protected void DoMapChanged()
        {
            if (FUpdateCount == 0)
            {
                CalcScrollRang();
                CheckUpdateInfo();
            }
        }

        protected virtual void DoChange()
        {
            SetIsChanged(true);
            DoMapChanged();
            if (FOnChange != null)
                FOnChange(this, null);
        }

        protected void DoSectionCreateItem(Object Sender, EventArgs e)
        {
            if (FOnSectionCreateItem != null)
                FOnSectionCreateItem(this, null);
        }

        protected void DoSectionReadOnlySwitch(Object Sender, EventArgs e)
        {
            if (FOnSectionReadOnlySwitch != null)
                FOnSectionReadOnlySwitch(this, null);
        }

        protected POINT DoSectionGetScreenCoord(int X, int  Y)
        {
            Point vPt = this.PointToScreen(new Point(X, Y));
            return new POINT(vPt.X , vPt.Y);
        }

        protected  void DoSectionInsertItem(HCCustomItem AItem)
        {
            if (FOnSectionInsertItem != null)
                FOnSectionInsertItem(AItem);
        }

        protected  void DoSectionDrawItemPaintBefor(HCCustomData AData, int ADrawItemNo, RECT ADrawRect, 
            int ADataDrawLeft, int ADataDrawBottom, int  ADataScreenTop, int ADataScreenBottom, 
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (FOnSectionDrawItemPaintBefor != null)
                FOnSectionDrawItemPaintBefor(AData, ADrawItemNo, ADrawRect, ADataDrawLeft,
                    ADataDrawBottom, ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);
        }

        protected virtual void DoSectionDrawItemPaintAfter(HCCustomData AData, int ADrawItemNo, RECT ADrawRect, 
            int ADataDrawLeft, int ADataDrawBottom, int ADataScreenTop, int ADataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (FOnSectionDrawItemPaintAfter != null)
                FOnSectionDrawItemPaintAfter(AData, ADrawItemNo, ADrawRect, ADataDrawLeft,
                    ADataDrawBottom, ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);
        }

        protected void DoSectionPaintHeader(Object Sender, int APageIndex, RECT ARect, HCCanvas ACanvas, SectionPaintInfo APaintInfo)
        {
            if (FOnSectionPaintHeader != null)
                FOnSectionPaintHeader(Sender, APageIndex, ARect, ACanvas, APaintInfo);
        }

        protected void DoSectionPaintFooter(Object Sender, int APageIndex, RECT ARect, HCCanvas ACanvas, SectionPaintInfo APaintInfo)
        {
            HCSection vSection = Sender as HCSection;
            if (vSection.PageNoVisible)
            {
                int vSectionIndex = FSections.IndexOf(vSection);
                int vSectionStartPageIndex = 0;
                int vAllPageCount = 0;
                for (int i = 0; i <= FSections.Count - 1; i++)
                {
                    if (i == vSectionIndex)
                        vSectionStartPageIndex = vAllPageCount;
                
                    vAllPageCount = vAllPageCount + FSections[i].PageCount;
                }
                    
                string vS = string.Format("{0}/{1}", 
                    vSectionStartPageIndex + vSection.PageNoFrom + APageIndex, vAllPageCount);
                ACanvas.Brush.Style = HCBrushStyle.bsClear;

                ACanvas.Font.BeginUpdate();
                try
                {
                    ACanvas.Font.Size = 10;
                    ACanvas.Font.Family = "宋体";
                }
                finally
                {
                    ACanvas.Font.EndUpdate();
                }

                ACanvas.TextOut(ARect.Left + (ARect.Width - ACanvas.TextWidth(vS)) / 2, ARect.Top + 20, vS);
            }

            if (FOnSectionPaintFooter != null)
                FOnSectionPaintFooter(vSection, APageIndex, ARect, ACanvas, APaintInfo);
        }

        protected void DoSectionPaintPage(Object Sender, int APageIndex, RECT ARect, HCCanvas ACanvas, SectionPaintInfo APaintInfo)
        {
            if (FOnSectionPaintPage != null)
                FOnSectionPaintPage(Sender, APageIndex, ARect, ACanvas, APaintInfo);
        }

        protected void DoSectionPaintWholePage(Object Sender, int APageIndex, RECT ARect, HCCanvas ACanvas, SectionPaintInfo APaintInfo)
        {
            if (FShowAnnotation)  // 绘制批注
                FAnnotates.PaintTo(ACanvas, ARect, APaintInfo);

            if (FOnSectionPaintWholePage != null)
                FOnSectionPaintWholePage(Sender, APageIndex, ARect, ACanvas, APaintInfo);
        }

        protected HCUndoList DoSectionGetUndoList()
        {
            return FUndoList;
        }

        protected void DoStyleInvalidateRect(RECT ARect)
        {
            UpdateView(ARect);
        }


        /// <summary> 是否上屏输入法输入的词条屏词条ID和词条 </summary>
        protected virtual bool DoProcessIMECandi(string ACandi)
        {
            return true;
        }

        /// <summary> 实现插入文本 </summary>
        protected virtual bool DoInsertText(string AText)
        {
            return ActiveSection.InsertText(AText);
        }

        /// <summary> 复制前，便于订制特征数据如内容来源 </summary>
        protected virtual void DoCopyDataBefor(Stream AStream) { }

        /// <summary> 粘贴前，便于确认订制特征数据如内容来源 </summary>
        protected virtual void DoPasteDataBefor(Stream AStream, ushort AVersion) { }

        /// <summary> 保存文档前触发事件，便于订制特征数据 </summary>
        protected virtual void DoSaveBefor(Stream AStream) { }

        /// <summary> 保存文档后触发事件，便于订制特征数据 </summary>
        protected virtual void DoSaveAfter(Stream AStream)
        {
            //SetIsChanged(false);
        }

        /// <summary> 读取文档前触发事件，便于确认订制特征数据 </summary>
        protected virtual void DoLoadBefor(Stream AStream, ushort AFileVersion) { }

        /// <summary> 读取文档后触发事件，便于确认订制特征数据 </summary>
        protected virtual void DoLoadAfter(Stream AStream, ushort AFileVersion) { }

        //
        protected override void OnMouseDown(MouseEventArgs e)
        {
 	        base.OnMouseDown(e);
            int vSectionIndex = -1;
            GetSectionByCrood(ZoomOut(FHScrollBar.Position + e.X), ZoomOut(FVScrollBar.Position + e.Y), ref vSectionIndex);
            if (vSectionIndex != FActiveSectionIndex)
                SetActiveSectionIndex(vSectionIndex);
            if (FActiveSectionIndex < 0)
                return;

            int vSectionDrawLeft = GetSectionDrawLeft(FActiveSectionIndex);
            
            if (FShowAnnotation)
            {
                if ((e.X > vSectionDrawLeft + FSections[FActiveSectionIndex].PageWidthPix)
                  && (e.X < vSectionDrawLeft + FSections[FActiveSectionIndex].PageWidthPix + HC.AnnotationWidth))
                {
                    FAnnotates.MouseDown(e.X, e.Y);
                    FStyle.UpdateInfoRePaint();
                    DoSectionDataCheckUpdateInfo(this, null);
                    return;
                }
            }
            
            // 映射到节页面(白色区域)
            Point vPt = new Point();
            vPt.X = ZoomOut(FHScrollBar.Position + e.X) - vSectionDrawLeft;
            vPt.Y = ZoomOut(FVScrollBar.Position + e.Y) - GetSectionTopFilm(FActiveSectionIndex);
            //vPageIndex := FSections[FActiveSectionIndex].GetPageByFilm(vPt.Y);
            MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vPt.X, vPt.Y, e.Delta);
            FSections[FActiveSectionIndex].MouseDown(vMouseArgs);
            
            CheckUpdateInfo();  // 换光标、切换激活Item
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
 	        base.OnMouseMove(e);
            if (FActiveSectionIndex >= 0)
            {
                MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks,
                    ZoomOut(FHScrollBar.Position + e.X) - GetSectionDrawLeft(FActiveSectionIndex),
                    ZoomOut(FVScrollBar.Position + e.Y) - GetSectionTopFilm(FActiveSectionIndex), 
                    e.Clicks);
                FSections[FActiveSectionIndex].MouseMove(vMouseArgs);

                //if (ShowHint)
                //    ProcessHint();
            }

            CheckUpdateInfo();

            if (FStyle.UpdateInfo.Draging)
                Cursor.Current = HC.GCursor;
            else
                this.Cursor = HC.GCursor;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                return;

            if (FActiveSectionIndex >= 0)
            {
                MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks,
                    ZoomOut(FHScrollBar.Position + e.X) - GetSectionDrawLeft(FActiveSectionIndex),
                    ZoomOut(FVScrollBar.Position + e.Y) - GetSectionTopFilm(FActiveSectionIndex),
                    e.Delta);
                FSections[FActiveSectionIndex].MouseUp(vMouseArgs);
            }

            if (FStyle.UpdateInfo.Draging)
                HC.GCursor = Cursors.Default;

            this.Cursor = HC.GCursor;

            CheckUpdateInfo();

            base.OnMouseUp(e);

            FStyle.UpdateInfo.Selecting = false;
            FStyle.UpdateInfo.Draging = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //base.OnMouseWheel(e);
            if (FPageScrollModel == HCPageScrollModel.psmVertical)
                FVScrollBar.Position -= e.Delta / 1;
            else
                FHScrollBar.Position -= e.Delta / 1;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
              if ((e.Control) && (e.Shift) && (e.KeyCode == Keys.C))
                this.CopyAsText();
              else
              if ((e.Control) && (e.KeyCode == Keys.C))
                this.Copy();
              else
              if ((e.Control) && (e.KeyCode == Keys.X))
                this.Cut();
              else
              if ((e.Control) && (e.KeyCode == Keys.V))
                this.Paste();
              else
              if ((e.Control) && (e.KeyCode == Keys.A))
                this.SelectAll();
              else
              if ((e.Control) && (e.KeyCode == Keys.Z))
                this.Undo();
              else
              if ((e.Control) && (e.KeyCode == Keys.Y))
                this.Redo();
              else
                ActiveSection.KeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            ActiveSection.KeyUp(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            ActiveSection.KeyPress(e);
        }

        protected override void WndProc(ref Message Message)
        {
            switch (Message.Msg)
            {
                case User.WM_GETDLGCODE:
                    Message.Result = (IntPtr)(User.DLGC_WANTTAB | User.DLGC_WANTARROWS);
                    return;

                case User.WM_ERASEBKGND:
                    Message.Result = (IntPtr)1;
                    return;

                case User.WM_SETFOCUS:
                    base.WndProc(ref Message);
                    ReBuildCaret();
                    return;

                case User.WM_KILLFOCUS:
                    base.WndProc(ref Message);
                    if (Message.WParam != Handle)
                        FCaret.Hide();
                    return;

                case User.WM_IME_SETCONTEXT:
                    if (Message.WParam.ToInt32() == 1)
                    {
                        Imm.ImmAssociateContext(this.Handle, FhImc);
                    }
                    break;

                case User.WM_IME_COMPOSITION:
                    if ((Message.LParam.ToInt32() & Imm.GCS_RESULTSTR) != 0)
                    {
                        
                        if (FhImc != IntPtr.Zero)
                        {
                            int vSize = Imm.ImmGetCompositionString(FhImc, Imm.GCS_RESULTSTR, null, 0);
                            if (vSize > 0)
                            {
                                byte[] vBuffer = new byte[vSize];
                                Imm.ImmGetCompositionString(FhImc, Imm.GCS_RESULTSTR, vBuffer, vSize);
                                string vS = System.Text.Encoding.Default.GetString(vBuffer);
                                if (vS != "")
                                {
                                    if (DoProcessIMECandi(vS))
                                        InsertText(vS);
                                }
                            }
                            
                            Message.Result = IntPtr.Zero;
                        }
                    }
                    break;

                case User.WM_LBUTTONDOWN:
                case User.WM_LBUTTONDBLCLK:
                    if ((!DesignMode) && (!this.Focused))
                    {
                        User.SetFocus(Handle);
                        if (!Focused)
                            return;
                    }
                    break;
            }

            base.WndProc(ref Message);
        }
        
        protected void CalcScrollRang()
        {
            int vVMax = 0, vWidth = 0;
            int vHMax = FSections[0].PageWidthPix;
            for (int i = 0; i <= Sections.Count - 1; i++)  //  计算节垂直总和，以及节中最宽的页宽
            {
                vVMax = vVMax + FSections[i].GetFilmHeight();
                
                vWidth = FSections[i].PageWidthPix;
                
                if (vWidth > vHMax)
                    vHMax = vWidth;
            }
            
            if (FShowAnnotation)
                vHMax = vHMax + HC.AnnotationWidth;
            
            vVMax = ZoomIn(vVMax + HC.PagePadding);  // 补充最后一页后面的PagePadding
            vHMax = ZoomIn(vHMax + HC.PagePadding + HC.PagePadding);
            
            FVScrollBar.Max = vVMax;
            FHScrollBar.Max = vHMax;
        }

        /// <summary> 是否由滚动条位置变化引起的更新 </summary>
        protected void CheckUpdateInfo()
        {
            if ((FCaret != null) && FStyle.UpdateInfo.ReCaret)
            {
                FStyle.UpdateInfo.ReCaret = false;
                ReBuildCaret();
                FStyle.UpdateInfo.ReStyle = false;
                FStyle.UpdateInfo.ReScroll = false;
                UpdateImmPosition();
            }

            if (FStyle.UpdateInfo.RePaint)
            {
                FStyle.UpdateInfo.RePaint = false;
                UpdateView();
            }
        }

        //
        protected  void SetPageScrollModel(HCPageScrollModel Value)
        {
            if (FViewModel == HCViewModel.vmWeb)
                return;

            if (FPageScrollModel != Value)
                FPageScrollModel = Value;
        }

        protected  void SetViewModel(HCViewModel Value)
        {
            if (FPageScrollModel == HCPageScrollModel.psmHorizontal)
                return;
            if (FViewModel != Value)
                FViewModel = Value;
        }

        protected  void SetShowAnnotation(bool Value)
        {
            if (FShowAnnotation != Value)
            {
                if (FAnnotates != null)
                    FAnnotates = new HCAnnotates();

                FShowAnnotation = Value;
                FStyle.UpdateInfoRePaint();
                FStyle.UpdateInfoReCaret(false);
                DoMapChanged();
            }
        }

        protected  void SetActiveSectionIndex(int Value)
        {
            if (FActiveSectionIndex != Value)
            {
                if (FActiveSectionIndex >= 0)
                FSections[FActiveSectionIndex].DisActive();
                FActiveSectionIndex = Value;
            }
        }

        //
        protected  void SetIsChanged(bool Value)
        {
            if (FIsChanged != Value)
            {
                FIsChanged = Value;
        
                if (FOnChangedSwitch != null)
                    FOnChangedSwitch(this, null);
            }
        }

        public HCView()
        {
            FHCExtFormat = DataFormats.GetFormat(HC.HC_EXT);
            SetStyle(ControlStyles.Selectable, true);
            this.BackColor = Color.FromArgb(82, 89, 107);
            FUndoList = new HCUndoList();
            FUndoList.OnUndo = DoUndo;
            FUndoList.OnRedo = DoRedo;
            FUndoList.OnNewUndo = DoNewUndo;

            FFileName = "";
            FIsChanged = false;
            FZoom = 1;
            FShowAnnotation = false;
            FAutoZoom = false;
            FViewModel = HCViewModel.vmPage;
            FPageScrollModel = HCPageScrollModel.psmVertical;

            FStyle = new HCStyle(true, true);
            FStyle.OnInvalidateRect = DoStyleInvalidateRect;
            FSections = new List<HCSection>();
            FSections.Add(NewDefaultSection());
            FActiveSectionIndex = 0;
            FDisplayFirstSection = 0;
            FDisplayLastSection = 0;
            // 垂直滚动条，范围在Resize中设置
            FVScrollBar = new HCRichScrollBar();
            FVScrollBar.Orientation = Orientation.oriVertical;
            FVScrollBar.OnScroll = DoVScrollChange;
            // 水平滚动条，范围在Resize中设置
            FHScrollBar = new HCScrollBar();
            FHScrollBar.Orientation = Orientation.oriHorizontal;
            FHScrollBar.OnScroll = DoVScrollChange;

            this.Controls.Add(FHScrollBar);
            this.Controls.Add(FVScrollBar);
            this.ResumeLayout();

            CalcScrollRang();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "HCView";
        }

        protected override void CreateHandle()
        {
 	        base.CreateHandle();
            if (!DesignMode)
                FCaret = new HCCaret(this.Handle);
            if (FDC == IntPtr.Zero)
            {
                FDC = User.GetDC(this.Handle);
                //FMemDC = (IntPtr)GDI.CreateCompatibleDC(FDC);
            }

            FhImc = Imm.ImmGetContext(this.Handle);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            GDI.DeleteDC(FMemDC);
            User.ReleaseDC(this.Handle, FDC);
            base.OnHandleDestroyed(e);
        }

        protected override void OnResize(EventArgs e)
        {
 	        base.OnResize(e);

            int vDisplayWidth = GetDisplayWidth();
            int vDisplayHeight = GetDisplayHeight();

            if (FAutoZoom)
            {
                if (FShowAnnotation)
                    FZoom = (vDisplayWidth - HC.PagePadding * 2) / (ActiveSection.PageWidthPix + HC.AnnotationWidth);
                else
                    FZoom = (vDisplayWidth - HC.PagePadding * 2) / ActiveSection.PageWidthPix;
            }

            CalcScrollRang();

            FStyle.UpdateInfoRePaint();
            if (FCaret != null)
                FStyle.UpdateInfoReCaret(false);

            CheckUpdateInfo();
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);

            FVScrollBar.Left = Width - FVScrollBar.Width;
            FVScrollBar.Height = Height - FHScrollBar.Height;
            //
            FHScrollBar.Top = Height - FHScrollBar.Height;
            FHScrollBar.Width = Width - FVScrollBar.Width;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            GDI.BitBlt(FDC, 0, 0, GetDisplayWidth(), GetDisplayHeight(), FMemDC, 0, 0, GDI.SRCCOPY);
 	        
            using (SolidBrush vBrush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(vBrush, new Rectangle(FVScrollBar.Left, FHScrollBar.Top, FVScrollBar.Width, FHScrollBar.Height));
            }
        }

        /// <summary> 重设当前节纸张边距 </summary>
        public void ResetActiveSectionMargin()
        {
            ActiveSection.ResetMargin();
        }

        /// <summary> 全部清空(清除各节页眉、页脚、页面的Item及DrawItem) </summary>
        public void Clear()
        {
            FStyle.Initialize();  // 先清样式，防止Data初始化为EmptyData时空Item样式赋值为CurStyleNo
            FSections.RemoveRange(1, FSections.Count - 1);
            FSections[0].Clear();
            FUndoList.Clear();
            FHScrollBar.Position = 0;
            FVScrollBar.Position = 0;
            FActiveSectionIndex = 0;
            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret();
            DoMapChanged();
        }

        /// <summary> 取消选中 </summary>
        public void DisSelect()
        {
            ActiveSection.DisSelect();
            DoSectionDataCheckUpdateInfo(this, null);
        }

        /// <summary> 删除选中内容 </summary>
        public void DeleteSelected()
        {
            ActiveSection.DeleteSelected();
        }

        /// <summary> 删除当前节 </summary>
        public void DeleteActiveSection()
        {
            if (FActiveSectionIndex > 0)
            {
                FSections.RemoveAt(FActiveSectionIndex);
                FActiveSectionIndex = FActiveSectionIndex - 1;
                FDisplayFirstSection = -1;
                FDisplayLastSection = -1;
                FStyle.UpdateInfoRePaint();
                FStyle.UpdateInfoReCaret();
                DoChange();
            }
        }

        /// <summary> 各节重新计算排版 </summary>
        public void FormatData()
        {
            for (int i = 0; i <= FSections.Count - 1; i++)
                FSections[i].FormatData();

            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret();
            DoMapChanged();
        }

        /// <summary> 插入流 </summary>
        public bool InsertStream(Stream AStream)
        {
            bool vResult = false;
            this.BeginUpdate();
            try
            {
                HCStyle vStyle = new HCStyle();
                try
                {
                    LoadSectionProcHandler vEvent = delegate(ushort AFileVersion)
                    {
                        byte[] vBuffer = new byte[1];
                        AStream.Read(vBuffer, 0, vBuffer.Length);
                        byte vByte = vBuffer[0];  // 节数量

                        MemoryStream vDataStream = new MemoryStream();
                        try
                        {
                            HCSection vSection = new HCSection(vStyle);
                            try
                            {
                                // 不循环，只插入第一节的正文
                                vSection.LoadFromStream(AStream, vStyle, AFileVersion);
                                vDataStream.SetLength(0);
                                vSection.PageData.SaveToStream(vDataStream);
                                vDataStream.Position = 0;

                                bool vShowUnderLine = false;
                                vBuffer = BitConverter.GetBytes(vShowUnderLine);
                                vDataStream.Read(vBuffer, 0, vBuffer.Length);
                                vShowUnderLine = BitConverter.ToBoolean(vBuffer, 0);

                                vResult = ActiveSection.InsertStream(vDataStream, vStyle, AFileVersion);  // 只插入第一节的数据
                            }
                            finally
                            {
                                vSection.Dispose();
                            }
                        }
                        finally
                        {
                            vDataStream.Close();
                            vDataStream.Dispose();
                        }
                    };

                    DoLoadFromStream(AStream, vStyle, vEvent);
                }
                finally
                {
                    vStyle.Dispose();
                }
            }
            finally
            {
                this.EndUpdate();
            }
            
            return vResult;
        }

        /// <summary> 插入文本(可包括#13#10) </summary>
        public bool InsertText(string AText)
        {
            this.BeginUpdate();
            try
            {
                return DoInsertText(AText);
            }
            finally
            {
                this.EndUpdate();
            }
        }

        /// <summary> 插入指定行列的表格 </summary>
        public bool InsertTable(int ARowCount, int  AColCount)
        {
            this.BeginUpdate();
            try
            {
                return ActiveSection.InsertTable(ARowCount, AColCount);
            }
            finally
            {
                this.EndUpdate();
            }
        }

        /// <summary> 插入水平线 </summary>
        public bool InsertLine(int ALineHeight)
        {
            return ActiveSection.InsertLine(ALineHeight);
        }

        /// <summary> 插入一个Item </summary>
        public bool InsertItem(HCCustomItem AItem)
        {
            return ActiveSection.InsertItem(AItem);
        }

        /// <summary> 在指定的位置插入一个Item </summary>
        public bool InsertItem(int AIndex, HCCustomItem AItem)
        {
            return ActiveSection.InsertItem(AIndex, AItem);
        }

        /// <summary> 插入浮动Item </summary>
        public bool InsertFloatItem(HCFloatItem AFloatItem)
        {
            return ActiveSection.InsertFloatItem(AFloatItem);
        }

        /// <summary> 插入批注(暂未实现) </summary>
        public bool InsertAnnotate(string AText)
        {
            return ActiveSection.InsertAnnotate(AText);
        }

        /// <summary> 从当前位置后换行 </summary>
        public bool InsertBreak()
        {
            return ActiveSection.InsertBreak();
        }

        /// <summary> 从当前位置后分页 </summary>
        public bool InsertPageBreak()
        {
            return ActiveSection.InsertPageBreak();
        }

        /// <summary> 从当前位置后分节 </summary>
        public bool InsertSectionBreak()
        {
            bool Result = false;
            HCSection vSection = NewDefaultSection();
            FSections.Insert(FActiveSectionIndex + 1, vSection);
            FActiveSectionIndex = FActiveSectionIndex + 1;
            Result = true;
            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret();
            DoChange();

            return Result;
        }

        /// <summary> 当前表格选中行下面插入行 </summary>
        public bool ActiveTableInsertRowAfter(byte ARowCount)
        {
            return ActiveSection.ActiveTableInsertRowAfter(ARowCount);
        }

        /// <summary> 当前表格选中行上面插入行 </summary>
        public bool ActiveTableInsertRowBefor(byte ARowCount)
        {
            return ActiveSection.ActiveTableInsertRowBefor(ARowCount);
        }

        /// <summary> 当前表格删除选中的行 </summary>
        public bool ActiveTableDeleteCurRow()
        {
            return ActiveSection.ActiveTableDeleteCurRow();
        }

        public bool ActiveTableSplitCurRow()
        {
            return ActiveSection.ActiveTableSplitCurRow();
        }

        public bool ActiveTableSplitCurCol()
        {
            return ActiveSection.ActiveTableSplitCurCol();
        }

        /// <summary> 当前表格选中列左侧插入列 </summary>
        public bool ActiveTableInsertColBefor(byte AColCount)
        {
            return ActiveSection.ActiveTableInsertColBefor(AColCount);
        }

        /// <summary> 当前表格选中列右侧插入列 </summary>
        public bool ActiveTableInsertColAfter(byte AColCount)
        {
            return ActiveSection.ActiveTableInsertColAfter(AColCount);
        }

        /// <summary> 当前表格删除选中的列 </summary>
        public bool ActiveTableDeleteCurCol()
        {
            return ActiveSection.ActiveTableDeleteCurCol();
        }

        /// <summary> 修改当前光标所在段水平对齐方式 </summary>
        public void ApplyParaAlignHorz(ParaAlignHorz AAlign)
        {
            ActiveSection.ApplyParaAlignHorz(AAlign);
        }

        /// <summary> 修改当前光标所在段垂直对齐方式 </summary>
        public void ApplyParaAlignVert(ParaAlignVert AAlign)
        {
            ActiveSection.ApplyParaAlignVert(AAlign);
        }

        /// <summary> 修改当前光标所在段背景色 </summary>
        public void ApplyParaBackColor(Color AColor)
        {
            ActiveSection.ApplyParaBackColor(AColor);
        }

        /// <summary> 修改当前光标所在段行间距 </summary>
        public void ApplyParaLineSpace(ParaLineSpaceMode ASpaceMode)
        {
            ActiveSection.ApplyParaLineSpace(ASpaceMode);
        }

        /// <summary> 修改当前选中文本的样式 </summary>
        public void ApplyTextStyle(HCFontStyle AFontStyle)
        {
            ActiveSection.ApplyTextStyle(AFontStyle);
        }

        /// <summary> 修改当前选中文本的字体 </summary>
        public void ApplyTextFontName(string AFontName)
        {
            ActiveSection.ApplyTextFontName(AFontName);
        }

        /// <summary> 修改当前选中文本的字号 </summary>
        public void ApplyTextFontSize(Single AFontSize)
        {
            ActiveSection.ApplyTextFontSize(AFontSize);
        }

        /// <summary> 修改当前选中文本的颜色 </summary>
        public void ApplyTextColor(Color AColor)
        {
            ActiveSection.ApplyTextColor(AColor);
        }

        /// <summary> 修改当前选中文本的背景颜色 </summary>
        public void ApplyTextBackColor(Color AColor)
        {
            ActiveSection.ApplyTextBackColor(AColor);
        }

        /// <summary> 全选(所有节内容) </summary>
        public void SelectAll()
        {
            for (int i = 0; i <= Sections.Count - 1; i++)
                FSections[i].SelectAll();

            FStyle.UpdateInfoRePaint();
            CheckUpdateInfo();
        }

        /// <summary> 剪切选中内容 </summary>
        public void Cut()
        {
            Copy();
            ActiveSection.DeleteSelected();
        }

        /// <summary> 复制选中内容(tcf格式) </summary>
        public void Copy()
        {
            if (ActiveSection.SelectExists())
            {
                //Clipboard.Clear();
                
                MemoryStream vStream = new MemoryStream();
                try
                {
                    HC._SaveFileFormatAndVersion(vStream);  // 保存文件格式和版本
                    DoCopyDataBefor(vStream);  // 通知保存事件

                    HashSet<SectionArea> vSaveParts = new HashSet<SectionArea>() { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
                    _DeleteUnUsedStyle(vSaveParts);  // 保存已使用的样式
                    FStyle.SaveToStream(vStream);
                    this.ActiveSectionTopLevelData().SaveSelectToStream(vStream);

                    //IDataObject vDataObj = new DataObject();
                    //vDataObj.SetData(HC.HC_EXT, vStream);
                    //vDataObj.SetData(DataFormats.Text, this.ActiveSectionTopLevelData().SaveSelectToText());  // 文本格式
                    //Clipboard.SetDataObject(vDataObj);

                    byte[] vBuffer = new byte[0];
                    IntPtr vMemExt = (IntPtr)Kernel.GlobalAlloc(Kernel.GMEM_MOVEABLE | Kernel.GMEM_DDESHARE, (int)vStream.Length);
                    try
                    {
                        if (vMemExt == IntPtr.Zero)
                            throw new Exception(HC.HCS_EXCEPTION_MEMORYLESS);
                        IntPtr vPtr = (IntPtr)Kernel.GlobalLock(vMemExt);
                        try
                        {
                            vStream.Position = 0;
                            vBuffer = vStream.ToArray();
                            System.Runtime.InteropServices.Marshal.Copy(vBuffer, 0, vPtr, vBuffer.Length);
                            //Kernel.CopyMemory(vPtr, vStream.ToArray(), (int)vStream.Length);
                        }
                        finally
                        {
                            Kernel.GlobalUnlock(vMemExt);
                        }
                    }
                    catch
                    {
                        Kernel.GlobalFree(vMemExt);
                        return;
                    }

                    //vBuffer = new byte[System.Text.Encoding.Default.GetByteCount() + 1];
                    vBuffer = System.Text.Encoding.Default.GetBytes(this.ActiveSectionTopLevelData().SaveSelectToText());
                    IntPtr vMem = (IntPtr)Kernel.GlobalAlloc(Kernel.GMEM_MOVEABLE | Kernel.GMEM_DDESHARE, vBuffer.Length + 1);
                    try
                    {
                        if (vMem == IntPtr.Zero)
                            throw new Exception(HC.HCS_EXCEPTION_MEMORYLESS);

                        IntPtr vPtr = (IntPtr)Kernel.GlobalLock(vMem);
                        try
                        {
                            string vs = "";
                            System.Runtime.InteropServices.Marshal.Copy(vBuffer, 0, vPtr, vBuffer.Length);
                        }
                        finally
                        {
                            Kernel.GlobalUnlock(vMem);
                        }
                    }
                    catch
                    {
                        Kernel.GlobalFree(vMem);
                        return;
                    }

                    User.OpenClipboard(IntPtr.Zero);
                    try
                    {
                        User.EmptyClipboard();
                        User.SetClipboardData(FHCExtFormat.Id, vMemExt);
                        User.SetClipboardData(User.CF_TEXT, vMem);  // 文本格式
                    }
                    finally
                    {
                        User.CloseClipboard();
                    }
                }
                finally
                {
                    vStream.Close();
                    vStream.Dispose();
                }
            }
        }

        /// <summary> 复制选中内容为文本 </summary>
        public void CopyAsText()
        {
            Clipboard.SetText(this.ActiveSectionTopLevelData().SaveSelectToText());  // 文本格式
        }

        /// <summary> 粘贴剪贴板中的内容 </summary>
        public void Paste()
        {
            IDataObject vIData = Clipboard.GetDataObject();
            //string[] vFormats = vIData.GetFormats();

            if (vIData.GetDataPresent(HC.HC_EXT))
            {
                MemoryStream vStream = (MemoryStream)vIData.GetData(HC.HC_EXT);
                try
                {
                    string vFileFormat = "";
                    ushort vFileVersion = 0;
                    byte vLan = 0;

                    vStream.Position = 0;
                    HC._LoadFileFormatAndVersion(vStream, ref vFileFormat, ref vFileVersion, ref vLan);  // 文件格式和版本
                    DoPasteDataBefor(vStream, vFileVersion);
                    HCStyle vStyle = new HCStyle();
                    try
                    {
                        vStyle.LoadFromStream(vStream, vFileVersion);
                        this.BeginUpdate();
                        try
                        {
                            ActiveSection.InsertStream(vStream, vStyle, vFileVersion);
                        }
                        finally
                        {
                            this.EndUpdate();
                        }
                    }
                    finally
                    {
                        vStyle.Dispose();
                    }
                }
                finally
                {
                    vStream.Close();
                    vStream.Dispose();
                }
            }
            else
            if (vIData.GetDataPresent(DataFormats.Text))
                InsertText(Clipboard.GetText());
            else
            if (vIData.GetDataPresent(DataFormats.Bitmap))
            {
                MemoryStream vStream = (MemoryStream)vIData.GetData(HC.HC_EXT);
                vStream.Position = 0;
                HCImageItem vImageItem = new HCImageItem(ActiveSectionTopLevelData());
                vImageItem.Image = new Bitmap(vStream);

                vImageItem.Width = vImageItem.Image.Width;
                vImageItem.Height = vImageItem.Image.Height;

                HCSection vSection = ActiveSection;
                int vContentWidth = vSection.PageWidthPix - vSection.PageMarginLeftPix - vSection.PageMarginRightPix;
                int vContentHeight = vSection.PageHeightPix - vSection.GetHeaderAreaHeight() - vSection.PageMarginBottomPix;
                vImageItem.RestrainSize(vContentWidth, vContentHeight);

                this.InsertItem(vImageItem);
            }
        }

        /// <summary> 放大视图 </summary>
        public int ZoomIn(int Value)
        {
            return (int)Math.Round(Value * FZoom);
        }

        /// <summary> 缩小视图 </summary>
        public int ZoomOut(int Value)
        {
            return (int)Math.Round(Value / FZoom);
        }

        /// <summary> 重绘客户区域 </summary>
        public void UpdateView()
        {
            UpdateView(new RECT(0, 0, GetDisplayWidth(), GetDisplayHeight()));
        }

#region
        private void CalcDisplaySectionAndPage()
        {
            if (FDisplayFirstSection >= 0)
            {
                FSections[FDisplayFirstSection].DisplayFirstPageIndex = -1;
                FSections[FDisplayFirstSection].DisplayLastPageIndex = -1;
                FDisplayFirstSection = -1;
            }

            if (FDisplayLastSection >= 0)
            {
                FSections[FDisplayLastSection].DisplayFirstPageIndex = -1;
                FSections[FDisplayLastSection].DisplayLastPageIndex = -1;
                FDisplayLastSection = -1;
            }
            
            int vFirstPage = -1;
            int vLastPage = -1;
            int vPos = 0;
            if (FPageScrollModel == HCPageScrollModel.psmVertical)
            {
                for (int i = 0; i <= Sections.Count - 1; i++)
                {
                    for (int j = 0; j <= Sections[i].PageCount - 1; j++)
                    {
                        vPos = vPos + ZoomIn(HC.PagePadding + FSections[i].PageHeightPix);
                        if (vPos > FVScrollBar.Position)
                        {
                            vFirstPage = j;
                            break;
                        }
                    }
                    if (vFirstPage >= 0)
                    {
                        FDisplayFirstSection = i;
                        FSections[FDisplayFirstSection].DisplayFirstPageIndex = vFirstPage;
                        break;
                    }
                }
                if (FDisplayFirstSection >= 0)
                {
                    int vY = FVScrollBar.Position + GetDisplayHeight();
                    for (int i = FDisplayFirstSection; i <= Sections.Count - 1; i++)
                    {
                        for (int j = vFirstPage; j <= Sections[i].PageCount - 1; j++)
                        {
                            if (vPos < vY)
                                vPos = vPos + ZoomIn(HC.PagePadding + FSections[i].PageHeightPix);
                            else
                            {
                                vLastPage = j;
                                break;
                            }
                        }
                        if (vLastPage >= 0)
                        {
                            FDisplayLastSection = i;
                            FSections[FDisplayLastSection].DisplayLastPageIndex = vLastPage;
                            break;
                    }
                }
                    if (FDisplayLastSection < 0)
                    {
                        FDisplayLastSection = FSections.Count - 1;
                        FSections[FDisplayLastSection].DisplayLastPageIndex = FSections[FDisplayLastSection].PageCount - 1;
                    }
                }
            }

            if ((FDisplayFirstSection < 0) || (FDisplayLastSection < 0))
                throw new Exception("异常：获取当前显示起始页和结束页失败！");
            else
            {
                if (FDisplayFirstSection != FDisplayLastSection)
                {
                    FSections[FDisplayFirstSection].DisplayLastPageIndex = FSections[FDisplayFirstSection].PageCount - 1;
                    FSections[FDisplayLastSection].DisplayFirstPageIndex = 0;
                }
            }
        }
#endregion

        /// <summary> 重绘客户区指定区域 </summary>
        public void UpdateView(RECT ARect)
        {
            if ((FUpdateCount == 0) && IsHandleCreated)
            {
                int vDisplayWidth = GetDisplayWidth();
                int vDisplayHeight = GetDisplayHeight();

                if (FMemDC != IntPtr.Zero)
                    GDI.DeleteDC(FMemDC);
                FMemDC = (IntPtr)GDI.CreateCompatibleDC(FDC);
                IntPtr vBitmap = (IntPtr)GDI.CreateCompatibleBitmap(FDC, vDisplayWidth, vDisplayHeight);
                GDI.SelectObject(FMemDC, vBitmap);
                try
                {
                    using (HCCanvas vDataBmpCanvas = new HCCanvas(FMemDC))
                    {
                        if (FShowAnnotation)
                            FAnnotates.Clear();

                        // 创建一个新的剪切区域，该区域是当前剪切区域和一个特定矩形的交集
                        GDI.IntersectClipRect(vDataBmpCanvas.Handle, ARect.Left, ARect.Top, ARect.Right, ARect.Bottom);

                        // 控件背景
                        vDataBmpCanvas.Brush.Color = this.BackColor;// $00E7BE9F;
                        vDataBmpCanvas.FillRect(new RECT(0, 0, vDisplayWidth, vDisplayHeight));
                        // 因基于此计算当前页面数据起始结束，所以不能用ARect代替
                        CalcDisplaySectionAndPage();  // 计算当前范围内可显示的起始节、页和结束节、页

                        SectionPaintInfo vPaintInfo = new SectionPaintInfo();
                        try
                        {
                            vPaintInfo.ScaleX = FZoom;
                            vPaintInfo.ScaleY = FZoom;
                            vPaintInfo.Zoom = FZoom;
                            vPaintInfo.WindowWidth = vDisplayWidth;
                            vPaintInfo.WindowHeight = vDisplayHeight;

                            ScaleInfo vScaleInfo = vPaintInfo.ScaleCanvas(vDataBmpCanvas);
                            try
                            {
                                if (FOnUpdateViewBefor != null)
                                    FOnUpdateViewBefor(vDataBmpCanvas);

                                int vOffsetY = 0;
                                for (int i = FDisplayFirstSection; i <= FDisplayLastSection; i++)
                                {
                                    vPaintInfo.SectionIndex = i;
                                    vOffsetY = ZoomOut(FVScrollBar.Position) - GetSectionTopFilm(i);  // 转为原始Y向偏移
                                    FSections[i].PaintDisplayPage(GetSectionDrawLeft(i) - ZoomOut(FHScrollBar.Position),  // 原始X向偏移
                                        vOffsetY, vDataBmpCanvas, vPaintInfo);
                                }

                                for (int i = 0; i <= vPaintInfo.TopItems.Count - 1; i++)  // 绘制顶层Ite
                                    vPaintInfo.TopItems[i].PaintTop(vDataBmpCanvas);

                                if (FOnUpdateViewAfter != null)
                                    FOnUpdateViewAfter(vDataBmpCanvas);
                            }
                            finally
                            {
                                vPaintInfo.RestoreCanvasScale(vDataBmpCanvas, vScaleInfo);
                            }
                        }
                        finally
                        {
                            vPaintInfo.Dispose();
                        }

                        GDI.BitBlt(FDC, ARect.Left, ARect.Top, ARect.Width, ARect.Height,
                            FMemDC, ARect.Left, ARect.Top, GDI.SRCCOPY);
                    }
                }
                finally
                {
                    GDI.DeleteObject(vBitmap);
                }

                User.InvalidateRect(this.Handle, ref ARect, 0);  // 只更新变动区域，防止闪烁，解决BitBlt光标滞留问题
            }
        }

        /// <summary> 开始批量重绘 </summary>
        public void BeginUpdate()
        {
            FUpdateCount++;
        }

        /// <summary> 结束批量重绘 </summary>
        public void EndUpdate()
        {
            FUpdateCount--;
            DoMapChanged();
        }

        //
        /// <summary> 返回当前节当前Item </summary>
        public HCCustomItem GetCurItem()
        {
            return ActiveSection.GetCurItem();
        }

        /// <summary> 返回当前节顶层Item </summary>
        public HCCustomItem GetTopLevelItem()
        {
            return ActiveSection.GetTopLevelItem();
        }

        /// <summary> 返回当前节顶层DrawItem </summary>
        public HCCustomDrawItem GetTopLevelDrawItem()
        {
            return ActiveSection.GetTopLevelDrawItem();
        }

        /// <summary> 返回当前光标所在页序号 </summary>
        public int GetActivePageIndex()
        {
            int Result = 0;
            for (int i = 0; i <= ActiveSectionIndex - 1; i++)
                Result = Result + FSections[i].PageCount;
            
            Result = Result + ActiveSection.ActivePageIndex;

            return Result;
        }

        /// <summary> 返回当前预览起始页序号 </summary>
        public int GetPagePreviewFirst()
        {
            int Result = 0;
            for (int i = 0; i <= ActiveSectionIndex - 1; i++)
                Result = Result + FSections[i].PageCount;
    
            Result = Result + FSections[FActiveSectionIndex].DisplayFirstPageIndex;

            return Result;
        }

        /// <summary> 返回总页数 </summary>
        public int GetPageCount()
        {
            int Result = 0;
            for (int i = 0; i <= Sections.Count - 1; i++)
                Result = Result + FSections[i].PageCount;

            return Result;
        }

        /// <summary> 返回指定节页面绘制时Left位置 </summary>
        public int GetSectionDrawLeft(int ASectionIndex)
        {
            int Result = 0;
             if (FShowAnnotation)
                Result = Math.Max((GetDisplayWidth() - ZoomIn(FSections[ASectionIndex].PageWidthPix + HC.AnnotationWidth)) / 2, ZoomIn(HC.PagePadding));
            else
                Result = Math.Max((GetDisplayWidth() - ZoomIn(FSections[ASectionIndex].PageWidthPix)) / 2, ZoomIn(HC.PagePadding));
    
            Result = ZoomOut(Result);

            return Result;
        }

        /// <summary> 返回光标处DrawItem相对当前页显示的窗体坐标 </summary>
        /// <returns>坐标</returns>
        public POINT GetActiveDrawItemClientCoord()
        {
            POINT Result = ActiveSection.GetActiveDrawItemCoord();  // 有选中时，以选中结束位置的DrawItem格式化坐标
            int vPageIndex = ActiveSection.GetPageIndexByFormat(Result.Y);
            
            // 映射到节页面(白色区域)
            Result.X = ZoomIn(GetSectionDrawLeft(this.ActiveSectionIndex)
                + (ActiveSection.GetPageMarginLeft(vPageIndex) + Result.X)) - this.HScrollValue;
            
            if (ActiveSection.ActiveData == ActiveSection.Header)
                Result.Y = ZoomIn(GetSectionTopFilm(this.ActiveSectionIndex)
                    + ActiveSection.GetPageTopFilm(vPageIndex)  // 20
                    + ActiveSection.GetHeaderPageDrawTop()
                    + Result.Y
                    - ActiveSection.GetPageDataFmtTop(vPageIndex))  // 0
                    - this.VScrollValue;
            else
            if (ActiveSection.ActiveData == ActiveSection.Footer)
                Result.Y = ZoomIn(GetSectionTopFilm(this.ActiveSectionIndex)
                    + ActiveSection.GetPageTopFilm(vPageIndex)  // 20
                    + ActiveSection.PageHeightPix - ActiveSection.PageMarginBottomPix
                    + Result.Y
                    - ActiveSection.GetPageDataFmtTop(vPageIndex))  // 0
                    - this.VScrollValue;
            else
                Result.Y = ZoomIn(GetSectionTopFilm(this.ActiveSectionIndex)
                + ActiveSection.GetPageTopFilm(vPageIndex)  // 20
                + ActiveSection.GetHeaderAreaHeight() // 94
                + Result.Y
                - ActiveSection.GetPageDataFmtTop(vPageIndex))  // 0
                - this.VScrollValue;

            return Result;
        }

        /// <summary> 格式化指定节的数据 </summary>
        public void FormatSection(int ASectionIndex)
        {
            FSections[ASectionIndex].FormatData();
            FSections[ASectionIndex].BuildSectionPages(0);
            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret();

            DoChange();
        }

        /// <summary> 获取当前节对象 </summary>
        public HCSection GetActiveSection()
        {
            return FSections[FActiveSectionIndex];
        }

        /// <summary> 获取当前节顶层Data </summary>
        public HCCustomRichData ActiveSectionTopLevelData()
        {
            return ActiveSection.ActiveData.GetTopLevelData();
        }

        /// <summary> 指定节在整个胶卷中的Top位置 </summary>
        public int GetSectionTopFilm(int ASectionIndex)
        {
            int Result = 0;
            for (int i = 0; i <= ASectionIndex - 1; i++)
                Result = Result + FSections[i].GetFilmHeight();

            return Result;
        }

        private void StyleSaveToXML(HCStyle AStyle, XmlElement ANode)
        {
        }

        private void SectionSaveToXML(List<HCSection> ASections, XmlElement ANode)
        {
        }

        // 保存文档
        /// <summary> 文档保存为xml格式 </summary>
        public void SaveAsXML(string AFileName)
        {
            HashSet<SectionArea> vParts = new HashSet<SectionArea> { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
            _DeleteUnUsedStyle(vParts);
            XmlDocument vXml = new XmlDocument();
            //vXml. = "1.0";
            //vXml.DocumentElement
            XmlElement vElement = vXml.CreateElement("HCView");
            vElement.SetAttribute("Version", HC.HC_FileVersion);
            StyleSaveToXML(FStyle, vElement);  // 样式表
            SectionSaveToXML(FSections, vElement);  // 节数据
            vXml.Save(AFileName);
        }

        /// <summary> 文档保存为hcf格式 </summary>
        public void SaveToFile(string AFileName)
        {
            FileStream vStream = new FileStream(AFileName, FileMode.Create, FileAccess.Write);
            try
            {
                HashSet<SectionArea> vParts = new HashSet<SectionArea> { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
                SaveToStream(vStream, vParts);
            }
            finally
            {
                vStream.Close();
                vStream.Dispose();
            }
        }

        /// <summary> 文档保存为PDF格式 </summary>
        public void SaveAsPDF(string AFileName)
        {

        }

        /// <summary> 文档保存为PDF格式 </summary>
        public void SaveAsText(string AFileName, System.Text.Encoding AEncoding)
        {
            for (int i = 0; i <= Sections.Count - 1; i++)
                FSections[i].SaveToText(AFileName, AEncoding);
        }

        /// <summary> 文档保存到流 </summary>
        public virtual void SaveToStream(Stream AStream, HashSet<SectionArea> ASaveParts)
        {
            HC._SaveFileFormatAndVersion(AStream);  // 文件格式和版本
            DoSaveBefor(AStream);
            _DeleteUnUsedStyle(ASaveParts);  // 删除不使用的样式(可否改为把有用的存了，加载时Item的StyleNo取有用)
            FStyle.SaveToStream(AStream);
            // 节数量
            byte vByte = (byte)FSections.Count;
            AStream.WriteByte(vByte);
            // 各节数据
            for (int i = 0; i <= Sections.Count - 1; i++)
                FSections[i].SaveToStream(AStream, ASaveParts);
            DoSaveAfter(AStream);
        }

        // 读取文档
        /// <summary> 读取Txt文件 </summary>
        public void LoadFromText(string AFileName, System.Text.Encoding AEncoding)
        {
            Clear();
            FStyle.Initialize();
            ActiveSection.LoadFromText(AFileName, AEncoding);
        }

        /// <summary> 读取hcf文件 </summary>
        public void LoadFromFile(string AFileName)
        {
            FFileName = AFileName;
            FileStream vStream = new FileStream(AFileName, FileMode.Open, FileAccess.Read);
            try
            {
                LoadFromStream(vStream);
            }
            finally
            {
                vStream.Dispose();
            }
        }

        /// <summary> 读取文件流 </summary>
        public virtual void LoadFromStream(Stream AStream)
        {
            this.BeginUpdate();
            try
            {
                this.Clear();
                AStream.Position = 0;
                LoadSectionProcHandler vEvent = delegate(ushort AFileVersion)
                {
                    byte vByte = 0;
                    vByte = (byte)AStream.ReadByte();  // 节数量
                    // 各节数据
                    FSections[0].LoadFromStream(AStream, FStyle, AFileVersion);
                    for (int i = 1; i <= vByte - 1; i++)
                    {
                        HCSection vSection = NewDefaultSection();
                        vSection.LoadFromStream(AStream, FStyle, AFileVersion);
                        FSections.Add(vSection);
                    }
                };
            
                DoLoadFromStream(AStream, FStyle, vEvent);
            }
            finally
            {
                this.EndUpdate();
            }
        }

        /// <summary> 获取指定页所在的节和相对此节的页序号 </summary>
        /// <param name="APageIndex">页序号</param>
        /// <param name="ASectionPageIndex">返回相对所在节的序号</param>
        /// <returns>返回页序号所在的节序号</returns>
        public int GetSectionPageIndexByPageIndex(int APageIndex, ref int ASectionPageIndex)
        {
            int Result = -1, vPageCount = 0;

            for (int i = 0; i <= FSections.Count - 1; i++)
            {
                if (vPageCount + FSections[i].PageCount > APageIndex)
                {
                    Result = i;  // 找到节序号
                    ASectionPageIndex = APageIndex - vPageCount;  // FSections[i].PageCount;
                    break;
                }
                else
                    vPageCount = vPageCount + FSections[i].PageCount;
            }

            return Result;
        }

        // 打印
        /// <summary> 使用默认打印机打印所有页 </summary>
        /// <returns>打印结果</returns>
        public PrintResult Print()
        {
            return Print("");
        }

        /// <summary> 使用指定的打印机打印所有页 </summary>
        /// <param name="APrinter">指定打印机</param>
        /// <param name="ACopies">打印份数</param>
        /// <returns>打印结果</returns>
        public PrintResult Print(string APrinter, short ACopies = 1)
        {
            return Print(APrinter, 0, PageCount - 1, ACopies);
        }

        /// <summary> 使用指定的打印机打印指定页序号范围内的页 </summary>
        /// <param name="APrinter">指定打印机</param>
        /// <param name="AStartPageIndex">起始页序号</param>
        /// <param name="AEndPageIndex">结束页序号</param>
        /// <param name="ACopies">打印份数</param>
        /// <returns></returns>
        public PrintResult Print(string APrinter, int AStartPageIndex, int  AEndPageIndex, short ACopies)
        {
            int[] vPages = new int[AEndPageIndex - AStartPageIndex + 1];
            for (int i = AStartPageIndex; i <= AEndPageIndex; i++)
                vPages[i] = i;
    
            return Print(APrinter, ACopies, vPages);
        }

        /// <summary> 使用指定的打印机打印指定页 </summary>
        /// <param name="APrinter">指定打印机</param>
        /// <param name="ACopies">打印份数</param>
        /// <param name="APages">要打印的页序号数组</param>
        /// <returns>打印结果</returns>
        public PrintResult Print(string APrinter, short ACopies, int[] APages)
        {          
            PrintDocument vPrinter = new PrintDocument();
            if (APrinter != "")
                vPrinter.PrinterSettings.PrinterName = APrinter;
            
            if (!vPrinter.PrinterSettings.IsValid)
                return PrintResult.prError;
    
            vPrinter.DocumentName = FFileName;
            // 取打印机打印区域相关参数
            int vPrintOffsetX = (int)vPrinter.PrinterSettings.DefaultPageSettings.HardMarginX;  // -GetDeviceCaps(Printer.Handle, PHYSICALOFFSETX);  // 73
            int vPrintOffsetY = (int)vPrinter.PrinterSettings.DefaultPageSettings.HardMarginY;  // -GetDeviceCaps(Printer.Handle, PHYSICALOFFSETY);  // 37
            
            vPrinter.PrinterSettings.Copies = ACopies;

            SectionPaintInfo vPaintInfo = new SectionPaintInfo();
            try
            {
                vPaintInfo.Print = true;

                HCCanvas vPrintCanvas = new HCCanvas();
                int vPageIndex = 0, vSectionIndex = 0, vPrintWidth = 0, vPrintHeight = 0, vPrintPageIndex = 0, i = 0;

                vPrintPageIndex = APages[i];

                vPrinter.PrintPage += (sender, e) =>
                {
                    if (vPrintCanvas.Graphics == null)
                        vPrintCanvas.Graphics = e.Graphics;
            
                    // 根据页码获取起始节和结束节
                    vSectionIndex = GetSectionPageIndexByPageIndex(APages[vPrintPageIndex], ref vPageIndex);
                    if (vPaintInfo.SectionIndex != vSectionIndex)
                    {
                        vPaintInfo.SectionIndex = vSectionIndex;
                        SetPrintBySectionInfo(vPrinter.PrinterSettings.DefaultPageSettings, vSectionIndex);
                        vPrintWidth = GDI.GetDeviceCaps(vPrintCanvas.Handle, GDI.PHYSICALWIDTH);  // 4961
                        vPrintHeight = GDI.GetDeviceCaps(vPrintCanvas.Handle, GDI.PHYSICALHEIGHT);  // 7016
                        vPaintInfo.ScaleX = (float)vPrintWidth / FSections[vSectionIndex].PageWidthPix;  // GetDeviceCaps(Printer.Handle, LOGPIXELSX) / GetDeviceCaps(FStyle.DefCanvas.Handle, LOGPIXELSX);
                        vPaintInfo.ScaleY = (float)vPrintHeight / FSections[vSectionIndex].PageHeightPix;  // GetDeviceCaps(Printer.Handle, LOGPIXELSY) / GetDeviceCaps(FStyle.DefCanvas.Handle, LOGPIXELSY);
                        vPaintInfo.WindowWidth = vPrintWidth;  // FSections[vStartSection].PageWidthPix;
                        vPaintInfo.WindowHeight = vPrintHeight;  // FSections[vStartSection].PageHeightPix;
                        vPrintOffsetX = (int)Math.Round(vPrintOffsetX / vPaintInfo.ScaleX);
                        vPrintOffsetY = (int)Math.Round(vPrintOffsetY / vPaintInfo.ScaleY);
                    }

                    ScaleInfo vScaleInfo = vPaintInfo.ScaleCanvas(vPrintCanvas);
                    try
                    {
                        vPaintInfo.PageIndex = APages[vPrintPageIndex];
                        
                        FSections[vSectionIndex].PaintPage(APages[vPrintPageIndex], vPrintOffsetX, vPrintOffsetY,
                            vPrintCanvas, vPaintInfo);
                    }
                    finally
                    {
                        vPaintInfo.RestoreCanvasScale(vPrintCanvas, vScaleInfo);
                    }

                    if (i < APages.Length - 1)
                    {
                        i++;
                        vPrintPageIndex = APages[i];
                        e.HasMorePages = true;
                    }
                    else
                        e.HasMorePages = false;
                };

                vPrinter.Print();
            }
            finally
            {
                vPaintInfo.Dispose();
            }

            return PrintResult.prOk;
        }

        /// <summary> 从当前行打印当前页(仅限正文) </summary>
        /// <param name="APrintHeader"> 是否打印页眉 </param>
        /// <param name="APrintFooter"> 是否打印页脚 </param>
        public PrintResult PrintCurPageByActiveLine(bool APrintHeader, bool  APrintFooter)
        {
            PrintDocument vPrinter = new PrintDocument();
            PrintResult Result = PrintResult.prError;

            // 取打印机打印区域相关参数
            int vPrintOffsetX = (int)vPrinter.PrinterSettings.DefaultPageSettings.HardMarginX;  // -GetDeviceCaps(Printer.Handle, PHYSICALOFFSETX);  // 73
            int vPrintOffsetY = (int)vPrinter.PrinterSettings.DefaultPageSettings.HardMarginY;  // -GetDeviceCaps(Printer.Handle, PHYSICALOFFSETY);  // 37
            
            SectionPaintInfo vPaintInfo = new SectionPaintInfo();
            try
            {
                vPaintInfo.Print = true;
                vPaintInfo.SectionIndex = this.ActiveSectionIndex;
                vPaintInfo.PageIndex = this.ActiveSection.ActivePageIndex;

                SetPrintBySectionInfo(vPrinter.PrinterSettings.DefaultPageSettings, FActiveSectionIndex);

                int vPrintWidth = vPrinter.PrinterSettings.DefaultPageSettings.Bounds.Width;  //  GetDeviceCaps(Printer.Handle, PHYSICALWIDTH);
                int vPrintHeight = vPrinter.PrinterSettings.DefaultPageSettings.Bounds.Height;  // GetDeviceCaps(Printer.Handle, PHYSICALHEIGHT);

                vPaintInfo.ScaleX = vPrintWidth / this.ActiveSection.PageWidthPix;  // GetDeviceCaps(Printer.Handle, LOGPIXELSX) / GetDeviceCaps(FStyle.DefCanvas.Handle, LOGPIXELSX);
                vPaintInfo.ScaleY = vPrintHeight / this.ActiveSection.PageHeightPix;  // GetDeviceCaps(Printer.Handle, LOGPIXELSY) / GetDeviceCaps(FStyle.DefCanvas.Handle, LOGPIXELSY);
                vPaintInfo.WindowWidth = vPrintWidth;  // FSections[vStartSection].PageWidthPix;
                vPaintInfo.WindowHeight = vPrintHeight;  // FSections[vStartSection].PageHeightPix;

                vPrintOffsetX = (int)Math.Round(vPrintOffsetX / vPaintInfo.ScaleX);
                vPrintOffsetY = (int)Math.Round(vPrintOffsetY / vPaintInfo.ScaleY);

                HCCanvas vPrintCanvas = new HCCanvas();

                vPrinter.PrintPage += (sender, e) =>
                {
                    vPrintCanvas.Graphics = e.Graphics;


                    ScaleInfo vScaleInfo = vPaintInfo.ScaleCanvas(vPrintCanvas);
                    try
                    {                       
                        this.ActiveSection.PaintPage(this.ActiveSection.ActivePageIndex, vPrintOffsetX, vPrintOffsetY,
                            vPrintCanvas, vPaintInfo);

                        POINT vPt;
                         if (this.ActiveSection.ActiveData == this.ActiveSection.PageData)
                        {
                            vPt = this.ActiveSection.GetActiveDrawItemCoord();
                            vPt.Y = vPt.Y - ActiveSection.GetPageDataFmtTop(this.ActiveSection.ActivePageIndex);
                        }
                        else
                        {
                            Result = PrintResult.prNoSupport;
                            return;
                        }

                        int vMarginLeft = -1, vMarginRight = -1;
                        this.ActiveSection.GetPageMarginLeftAndRight(this.ActiveSection.ActivePageIndex, ref vMarginLeft, ref vMarginRight);
                        // "抹"掉不需要显示的地方
                        vPrintCanvas.Brush.Color = Color.White;

                        RECT vRect = new RECT();
                        if (APrintHeader)
                            vRect = HC.Bounds(vPrintOffsetX + vMarginLeft, vPrintOffsetY + this.ActiveSection.GetHeaderAreaHeight(),  // 页眉下边
                                this.ActiveSection.PageWidthPix - vMarginLeft - vMarginRight, vPt.Y);
                        else  // 不打印页眉
                            vRect = HC.Bounds(vPrintOffsetX + vMarginLeft, vPrintOffsetY, this.ActiveSection.PageWidthPix - vMarginLeft - vMarginRight,
                                this.ActiveSection.GetHeaderAreaHeight() + vPt.Y);
                        vPrintCanvas.FillRect(vRect);
                        if (!APrintFooter)
                        {
                            vRect = HC.Bounds(vPrintOffsetX + vMarginLeft, vPrintOffsetY + this.ActiveSection.PageHeightPix - this.ActiveSection.PageMarginBottomPix,
                                this.ActiveSection.PageWidthPix - vMarginLeft - vMarginRight, this.ActiveSection.PageMarginBottomPix);
                            vPrintCanvas.FillRect(vRect);
                        }
                    }
                    finally
                    {
                        vPaintInfo.RestoreCanvasScale(vPrintCanvas, vScaleInfo);
                    }
                };

                vPrinter.Print();
            }
            finally
            {
                vPaintInfo.Dispose();
            }

            if (Result != PrintResult.prError)
                return Result;
            else
                return PrintResult.prOk;
        }

        /// <summary> 打印当前页指定的起始、结束Item(仅限正文) </summary>
        /// <param name="APrintHeader"> 是否打印页眉 </param>
        /// <param name="APrintFooter"> 是否打印页脚 </param>
        public PrintResult PrintCurPageByItemRange(bool APrintHeader, bool  APrintFooter, int AStartItemNo, int  AStartOffset, int  AEndItemNo, int  AEndOffset)
        {
            PrintDocument vPrinter = new PrintDocument();
            PrintResult Result = PrintResult.prError;

            // 取打印机打印区域相关参数
            int vPrintOffsetX = (int)vPrinter.PrinterSettings.DefaultPageSettings.HardMarginX;  // -GetDeviceCaps(Printer.Handle, PHYSICALOFFSETX);  // 73
            int vPrintOffsetY = (int)vPrinter.PrinterSettings.DefaultPageSettings.HardMarginY;  // -GetDeviceCaps(Printer.Handle, PHYSICALOFFSETY);  // 37
            
            SectionPaintInfo vPaintInfo = new SectionPaintInfo();
            try
            {
                vPaintInfo.Print = true;
                vPaintInfo.SectionIndex = this.ActiveSectionIndex;
                vPaintInfo.PageIndex = this.ActiveSection.ActivePageIndex;

                SetPrintBySectionInfo(vPrinter.PrinterSettings.DefaultPageSettings, FActiveSectionIndex);

                int vPrintWidth = vPrinter.PrinterSettings.DefaultPageSettings.Bounds.Width;  //  GetDeviceCaps(Printer.Handle, PHYSICALWIDTH);
                int vPrintHeight = vPrinter.PrinterSettings.DefaultPageSettings.Bounds.Height;  // GetDeviceCaps(Printer.Handle, PHYSICALHEIGHT);

                vPaintInfo.ScaleX = vPrintWidth / this.ActiveSection.PageWidthPix;  // GetDeviceCaps(Printer.Handle, LOGPIXELSX) / GetDeviceCaps(FStyle.DefCanvas.Handle, LOGPIXELSX);
                vPaintInfo.ScaleY = vPrintHeight / this.ActiveSection.PageHeightPix;  // GetDeviceCaps(Printer.Handle, LOGPIXELSY) / GetDeviceCaps(FStyle.DefCanvas.Handle, LOGPIXELSY);
                vPaintInfo.WindowWidth = vPrintWidth;  // FSections[vStartSection].PageWidthPix;
                vPaintInfo.WindowHeight = vPrintHeight;  // FSections[vStartSection].PageHeightPix;

                vPrintOffsetX = (int)Math.Round(vPrintOffsetX / vPaintInfo.ScaleX);
                vPrintOffsetY = (int)Math.Round(vPrintOffsetY / vPaintInfo.ScaleY);

                HCCanvas vPrintCanvas = new HCCanvas();

                vPrinter.PrintPage += (sender, e) =>
                {
                    vPrintCanvas.Graphics = e.Graphics;


                    ScaleInfo vScaleInfo = vPaintInfo.ScaleCanvas(vPrintCanvas);
                    try
                    {                       
                        this.ActiveSection.PaintPage(this.ActiveSection.ActivePageIndex, vPrintOffsetX, vPrintOffsetY,
                            vPrintCanvas, vPaintInfo);

                        POINT vPt;
                        HCCustomRichData vData = null;
                        int vDrawItemNo = -1;
                         if (this.ActiveSection.ActiveData == this.ActiveSection.PageData)
                        {
                            vData = this.ActiveSection.ActiveData;
                            vDrawItemNo = vData.GetDrawItemNoByOffset(AStartItemNo, AStartOffset);
                            vPt = vData.DrawItems[vDrawItemNo].Rect.TopLeft();
                            vPt.Y = vPt.Y - ActiveSection.GetPageDataFmtTop(this.ActiveSection.ActivePageIndex);
                        }
                        else
                        {
                            Result = PrintResult.prNoSupport;
                            return;
                        }

                        int vMarginLeft = -1, vMarginRight = -1;
                        this.ActiveSection.GetPageMarginLeftAndRight(this.ActiveSection.ActivePageIndex, ref vMarginLeft, ref vMarginRight);
                        // "抹"掉不需要显示的地方
                        vPrintCanvas.Brush.Color = Color.White;

                        RECT vRect = new RECT();
                        if (APrintHeader)
                            vRect = HC.Bounds(vPrintOffsetX + vMarginLeft,
                                vPrintOffsetY + this.ActiveSection.GetHeaderAreaHeight(),  // 页眉下边
                                this.ActiveSection.PageWidthPix - vMarginLeft - vMarginRight, vPt.Y);
                        else  // 不打印页眉
                            vRect = HC.Bounds(vPrintOffsetX + vMarginLeft, vPrintOffsetY, this.ActiveSection.PageWidthPix - vMarginLeft - vMarginRight,
                                this.ActiveSection.GetHeaderAreaHeight() + vPt.Y);
                        vPrintCanvas.FillRect(vRect);
                        
                        vRect = HC.Bounds(vPrintOffsetX + vMarginLeft, vPrintOffsetY + this.ActiveSection.GetHeaderAreaHeight() + vPt.Y,
                            vData.GetDrawItemOffsetWidth(vDrawItemNo, AStartOffset - vData.DrawItems[vDrawItemNo].CharOffs + 1),
                            vData.DrawItems[vDrawItemNo].Rect.Height);
                        vPrintCanvas.FillRect(vRect);

                        //
                        vDrawItemNo = vData.GetDrawItemNoByOffset(AEndItemNo, AEndOffset);
                        vPt = vData.DrawItems[vDrawItemNo].Rect.TopLeft();
                        vPt.Y = vPt.Y - ActiveSection.GetPageDataFmtTop(this.ActiveSection.ActivePageIndex);

                        vRect = new RECT(vPrintOffsetX + vMarginLeft + vData.GetDrawItemOffsetWidth(vDrawItemNo, AEndOffset - vData.DrawItems[vDrawItemNo].CharOffs + 1),
                            vPrintOffsetY + this.ActiveSection.GetHeaderAreaHeight() + vPt.Y, vPrintOffsetX + this.ActiveSection.PageWidthPix - vMarginRight,
                            vPrintOffsetY + this.ActiveSection.GetHeaderAreaHeight() + vPt.Y + vData.DrawItems[vDrawItemNo].Rect.Height);
                        vPrintCanvas.FillRect(vRect);

                        if (!APrintFooter)
                        {
                            vRect = new RECT(vPrintOffsetX + vMarginLeft, vPrintOffsetY + + this.ActiveSection.GetHeaderAreaHeight() + vPt.Y + vData.DrawItems[vDrawItemNo].Rect.Height,
                                vPrintOffsetX + this.ActiveSection.PageWidthPix - vMarginRight, vPrintOffsetY + this.ActiveSection.PageHeightPix);
                            vPrintCanvas.FillRect(vRect);
                        }
                        else  // 打印页脚
                        {
                            vRect = new RECT(vPrintOffsetX + vMarginLeft, vPrintOffsetY + this.ActiveSection.GetHeaderAreaHeight() + vPt.Y + vData.DrawItems[vDrawItemNo].Rect.Height,
                                vPrintOffsetX + this.ActiveSection.PageWidthPix - vMarginRight, vPrintOffsetY + this.ActiveSection.PageHeightPix - this.ActiveSection.PageMarginBottomPix);
                        }
                    }
                    finally
                    {
                        vPaintInfo.RestoreCanvasScale(vPrintCanvas, vScaleInfo);
                    }
                };

                vPrinter.Print();
            }
            finally
            {
                vPaintInfo.Dispose();
            }

            if (Result != PrintResult.prError)
                return Result;
            else
                return PrintResult.prOk;
        }

        /// <summary> 打印当前页选中的起始、结束Item(仅限正文) </summary>
        /// <param name="APrintHeader"> 是否打印页眉 </param>
        /// <param name="APrintFooter"> 是否打印页脚 </param>
        public PrintResult PrintCurPageSelected(bool APrintHeader, bool  APrintFooter)
        {
            if (this.ActiveSection.ActiveData.SelectExists(false))
                return PrintCurPageByItemRange(APrintHeader, APrintFooter,
                    this.ActiveSection.ActiveData.SelectInfo.StartItemNo,
                    this.ActiveSection.ActiveData.SelectInfo.StartItemOffset,
                    this.ActiveSection.ActiveData.SelectInfo.EndItemNo,
                    this.ActiveSection.ActiveData.SelectInfo.EndItemOffset);
            else
                return PrintResult.prNoSupport;
        }

        /// <summary> 合并表格选中的单元格 </summary>
        public bool MergeTableSelectCells()
        {
            return ActiveSection.MergeTableSelectCells();
        }

        /// <summary> 撤销 </summary>
        public void Undo()
        {
            BeginUpdate();
            try
            {
                FUndoList.Undo();
            }
            finally
            {
                EndUpdate();
            }
        }

        /// <summary> 重做 </summary>
        public void Redo()
        {
            BeginUpdate();
            try
            {
                FUndoList.Redo();
            }
            finally
            {
                EndUpdate();
            }
        }

        /// <summary> 当前位置开始查找指定的内容 </summary>
        /// <param name="AKeyword">要查找的关键字</param>
        /// <param name="AForward">True：向前，False：向后</param>
        /// <param name="AMatchCase">True：区分大小写，False：不区分大小写</param>
        /// <returns>True：找到</returns>
        public bool Search(string AKeyword, bool AForward = false, bool AMatchCase = false)
        {
            bool Result = this.ActiveSection.Search(AKeyword, AForward, AMatchCase);
            if (Result)
            {
                POINT vPt = GetActiveDrawItemClientCoord();  // 返回光标处DrawItem相对当前页显示的窗体坐标，有选中时，以选中结束位置的DrawItem格式化坐标
                HCCustomRichData vTopData = ActiveSectionTopLevelData();

                int vStartDrawItemNo = vTopData.GetDrawItemNoByOffset(vTopData.SelectInfo.StartItemNo, vTopData.SelectInfo.StartItemOffset);
                int vEndDrawItemNo = vTopData.GetDrawItemNoByOffset(vTopData.SelectInfo.EndItemNo, vTopData.SelectInfo.EndItemOffset);
                 
                RECT vStartDrawRect = new RECT();
                RECT vEndDrawRect = new RECT();

                if (vStartDrawItemNo == vEndDrawItemNo)
                {
                    vStartDrawRect.Left = vPt.X + ZoomIn(vTopData.GetDrawItemOffsetWidth(vStartDrawItemNo,
                    vTopData.SelectInfo.StartItemOffset - vTopData.DrawItems[vStartDrawItemNo].CharOffs + 1));
                    vStartDrawRect.Top = vPt.Y;
                    vStartDrawRect.Right = vPt.X + ZoomIn(vTopData.GetDrawItemOffsetWidth(vEndDrawItemNo,
                    vTopData.SelectInfo.EndItemOffset - vTopData.DrawItems[vEndDrawItemNo].CharOffs + 1));
                    vStartDrawRect.Bottom = vPt.Y + ZoomIn(vTopData.DrawItems[vEndDrawItemNo].Rect.Height);
                    vEndDrawRect = vStartDrawRect;
                }
                else  // 选中不在同一个DrawItem
                {
                    vStartDrawRect.Left = vPt.X + ZoomIn(vTopData.DrawItems[vStartDrawItemNo].Rect.Left - vTopData.DrawItems[vEndDrawItemNo].Rect.Left
                        + vTopData.GetDrawItemOffsetWidth(vStartDrawItemNo, vTopData.SelectInfo.StartItemOffset - vTopData.DrawItems[vStartDrawItemNo].CharOffs + 1));
                    vStartDrawRect.Top = vPt.Y + ZoomIn(vTopData.DrawItems[vStartDrawItemNo].Rect.Top - vTopData.DrawItems[vEndDrawItemNo].Rect.Top);
                    vStartDrawRect.Right = vPt.X + ZoomIn(vTopData.DrawItems[vStartDrawItemNo].Rect.Left - vTopData.DrawItems[vEndDrawItemNo].Rect.Left
                        + vTopData.DrawItems[vStartDrawItemNo].Rect.Width);
                    vStartDrawRect.Bottom = vStartDrawRect.Top + ZoomIn(vTopData.DrawItems[vStartDrawItemNo].Rect.Height);
                    vEndDrawRect.Left = vPt.X;
                    vEndDrawRect.Top = vPt.Y;
                    vEndDrawRect.Right = vPt.X + ZoomIn(vTopData.GetDrawItemOffsetWidth(vEndDrawItemNo,
                    vTopData.SelectInfo.EndItemOffset - vTopData.DrawItems[vEndDrawItemNo].CharOffs + 1));
                    vEndDrawRect.Bottom = vPt.Y + ZoomIn(vTopData.DrawItems[vEndDrawItemNo].Rect.Height);
                }
            
                if (vStartDrawRect.Top < 0)
                    this.FVScrollBar.Position = this.FVScrollBar.Position + vStartDrawRect.Top;
                else
                if (vStartDrawRect.Bottom > GetDisplayHeight())
                    this.FVScrollBar.Position = this.FVScrollBar.Position + vStartDrawRect.Bottom - GetDisplayHeight();
                if (vStartDrawRect.Left < 0)
                    this.FHScrollBar.Position = this.FHScrollBar.Position + vStartDrawRect.Left;
                else
                if (vStartDrawRect.Right > GetDisplayWidth())
                    this.FHScrollBar.Position = this.FHScrollBar.Position + vStartDrawRect.Right - GetDisplayWidth();
            }

            return Result;
        }

        // 属性部分
        /// <summary> 当前文档名称 </summary>
        public string FileName
        {
            get { return FFileName; }
            set { FFileName = value; }
        }

        /// <summary> 当前文档样式表 </summary>
        public HCStyle Style
        {
            get { return FStyle; }
        }

        public HCSection ActiveSection
        {
            get { return GetActiveSection(); }
        }

        /// <summary> 是否对称边距 </summary>
        public bool SymmetryMargin
        {
            get { return GetSymmetryMargin(); }
            set { SetSymmetryMargin(value); }
        }

        /// <summary> 当前光标所在页的序号 </summary>
        public int ActivePageIndex
        {
            get { return GetActivePageIndex(); }
        }

        /// <summary> 当前预览的页序号 </summary>
        public int PagePreviewFirst
        {
            get { return GetPagePreviewFirst(); }
        }

        /// <summary> 总页数 </summary>
        public int PageCount
        {
            get { return GetPageCount(); }
        }

        /// <summary> 当前光标所在节的序号 </summary>
        public int ActiveSectionIndex
        {
            get { return FActiveSectionIndex; }
            set { SetActiveSectionIndex(value); }
        }

        /// <summary> 水平滚动条的值 </summary>
        public int HScrollValue
        {
            get { return GetHScrollValue(); }
        }

        /// <summary> 垂直滚动条的值 </summary>
        public int VScrollValue
        {
            get { return GetVScrollValue(); }
        }

        /// <summary> 缩放值 </summary>
        public Single Zoom
        {
            get { return FZoom; }
            set { SetZoom(value); }
        }

        /// <summary> 当前文档所有节 </summary>
        public List<HCSection> Sections
        {
            get { return FSections; }
        }

        /// <summary> 是否显示当前行指示符 </summary>
        public bool ShowLineActiveMark
        {
            get { return GetShowLineActiveMark(); }
            set { SetShowLineActiveMark(value); }
        }

        /// <summary> 是否显示行号 </summary>
        public bool ShowLineNo
        {
            get { return GetShowLineNo(); }
            set { SetShowLineNo(value); }
        }

        /// <summary> 是否显示下划线 </summary>
        public bool ShowUnderLine
        {
            get { return GetShowUnderLine(); }
            set { SetShowUnderLine(value); }
        }

        /// <summary> 当前文档是否有变化 </summary>
        public bool IsChanged
        {
            get { return FIsChanged; }
            set { SetIsChanged(value); }
        }

        /// <summary> 当前文档所有批注 </summary>
        public HCAnnotates Annotates
        {
            get { return FAnnotates; }
        }

        /// <summary> 节有新的Item创建时触发 </summary>
        public EventHandler OnSectionCreateItem
        {
            get { return FOnSectionCreateItem; }
            set { FOnSectionCreateItem = value; }
        }

        /// <summary> 节有新的Item插入时触发 </summary>
        public ItemNotifyEventHandler OnSectionItemInsert
        {
            get { return FOnSectionInsertItem; }
            set { FOnSectionInsertItem = value; }
        }

        /// <summary> Item绘制开始前触发 </summary>
        public DrawItemPaintEventHandler OnSectionDrawItemPaintBefor
        {
            get { return FOnSectionDrawItemPaintBefor; }
            set { FOnSectionDrawItemPaintBefor = value; }
        }

        /// <summary> DrawItem绘制完成后触发 </summary>
        public DrawItemPaintEventHandler OnSectionDrawItemPaintAfter
        {
            get { return FOnSectionDrawItemPaintAfter; }
            set { FOnSectionDrawItemPaintAfter = value; }
        }

        /// <summary> 节页眉绘制时触发 </summary>
        public SectionPagePaintEventHandler OnSectionPaintHeader
        {
            get { return FOnSectionPaintHeader; }
            set { FOnSectionPaintHeader = value; }
        }

        /// <summary> 节页脚绘制时触发 </summary>
        public SectionPagePaintEventHandler OnSectionPaintFooter
        {
            get { return FOnSectionPaintFooter; }
            set { FOnSectionPaintFooter = value; }
        }

        /// <summary> 节页面绘制时触发 </summary>
        public SectionPagePaintEventHandler OnSectionPaintPage
        {
            get { return FOnSectionPaintPage; }
            set { FOnSectionPaintPage = value; }
        }

        /// <summary> 节整页绘制时触发 </summary>
        public SectionPagePaintEventHandler OnSectionPaintWholePage
        {
            get { return FOnSectionPaintWholePage; }
            set { FOnSectionPaintWholePage = value; }
        }

        /// <summary> 节只读属性有变化时触发 </summary>
        public EventHandler OnSectionReadOnlySwitch
        {
            get { return FOnSectionReadOnlySwitch; }
            set { FOnSectionReadOnlySwitch = value; }
        }

        /// <summary> 页面滚动显示模式：纵向、横向 </summary>
        public HCPageScrollModel PageScrollModel
        {
            get { return FPageScrollModel; }
            set { SetPageScrollModel(value); }
        }

        /// <summary> 界面显示模式：页面、Web </summary>
        public HCViewModel ViewModel
        {
            get { return FViewModel; }
            set { SetViewModel(value); }
        }

        /// <summary> 是否显示批注 </summary>
        public bool ShowAnnotation
        {
            get { return FShowAnnotation; }
            set { SetShowAnnotation(value); }
        }

        /// <summary> 是否根据宽度自动计算缩放比例 </summary>
        public bool AutoZoom
        {
            get { return FAutoZoom; }
            set { FAutoZoom = value; }
        }

        /// <summary> 所有Section是否只读 </summary>
        public bool ReadOnly
        {
            get { return GetReadOnly(); }
            set { SetReadOnly(value); }
        }

        /// <summary> 光标位置改变时触发 </summary>
        public EventHandler OnCaretChange
        {
            get { return FOnCaretChange; }
            set { FOnCaretChange = value; }
        }

        /// <summary> 垂直滚动条滚动时触发 </summary>
        public EventHandler OnVerScroll
        {
            get { return FOnVerScroll; }
            set { FOnVerScroll = value; }
        }

        /// <summary> 文档内容变化时触发 </summary>
        public EventHandler OnChange
        {
            get { return FOnChange; }
            set { FOnChange = value; }
        }

        /// <summary> 文档Change状态切换时触发 </summary>
        public EventHandler OnChangedSwitch
        {
            get { return FOnChangedSwitch; }
            set { FOnChangedSwitch = value; }
        }

        /// <summary> 窗口重绘开始时触发 </summary>
        public PaintEventHandler OnUpdateViewBefor
        {
            get { return FOnUpdateViewBefor; }
            set { FOnUpdateViewBefor = value; }
        }

        /// <summary> 窗口重绘结束后触发 </summary>
        public PaintEventHandler OnUpdateViewAfter
        {
            get { return FOnUpdateViewAfter; }
            set { FOnUpdateViewAfter = value; }
        }

        public StyleItemEventHandler OnSectionCreateStyleItem
        {
            get { return FOnSectionCreateStyleItem; }
            set { FOnSectionCreateStyleItem = value; }
        }
    }

    public delegate void LoadSectionProcHandler(ushort AFileVersion);

    public enum HCPageScrollModel : byte
    {
        psmVertical, psmHorizontal
    }

    public delegate void PaintEventHandler(object sender);

    public class Annotate : Object  // 批注
    {
        private RECT FDrawItemRect, FPaintRect;

        private string FText;

        public RECT DrawItemRect
        {
            get { return FDrawItemRect; }
            set { FDrawItemRect = value; }
        }

        public RECT PaintRect
        {
            get { return FPaintRect; }
            set { FPaintRect = value; }
        }

        public string Text
        {
            get { return FText; }
            set { FText = value; }
        }
    }

    public class HCAnnotates : List<Annotate>  // 批注s
    {
        private int FIndex;

        public HCAnnotates()
        {
            FIndex = -1;
        }

        public void PaintTo(HCCanvas ACanvas, RECT ARect, SectionPaintInfo APaintInfo)
        {
            if (APaintInfo.Print)
                return;

            ACanvas.Brush.Color = Color.FromArgb(0xF4, 0xF4, 0xF4);

            ACanvas.FillRect(new RECT(ARect.Right, ARect.Top, HC.AnnotationWidth, ARect.Height));

            if (this.Count > 0)
            {
                int vPos = 0;
                Annotate vAnnotation;
                ACanvas.Font.Size = 8;

                for (int i = 0; i < this.Count; i++)
                {
                    ACanvas.Pen.BeginUpdate();
                    try
                    {
                        if (i != FIndex)
                        {
                            ACanvas.Pen.Style = HCPenStyle.psDot;
                            ACanvas.Pen.Color = Color.Red;
                        }
                        else
                        {
                            ACanvas.Pen.Style = HCPenStyle.psSolid;
                            ACanvas.Pen.Color = Color.Maroon;
                        }
                    }
                    finally
                    {
                        ACanvas.Pen.EndUpdate();
                    }

                    vAnnotation = this[i];

                    if (vPos < vAnnotation.DrawItemRect.Top)
                    {
                        vPos = vAnnotation.DrawItemRect.Top;
                    }
                    else
                    {
                        if (vAnnotation.DrawItemRect.Top <= vPos)
                            vPos = vPos + (vAnnotation.DrawItemRect.Bottom - vAnnotation.DrawItemRect.Top);
                    }

                    // 计算批注文本显示区域
                    RECT vTextRect = new RECT(ARect.Right + 30, vPos, ARect.Right + HC.AnnotationWidth - 10, vAnnotation.DrawItemRect.Bottom);

                    DRAWTEXTPARAMS lpDrawTextParams = new DRAWTEXTPARAMS();
                    User.DrawTextEx(ACanvas.Handle, vAnnotation.Text, -1, ref vTextRect,
                        User.DT_TOP | User.DT_LEFT | User.DT_WORDBREAK | User.DT_EDITCONTROL | User.DT_CALCRECT, ref lpDrawTextParams);  // 计算区域

                    // 填充批注区域
                    //ACanvas.Brush.Style = bsSolid;
                    ACanvas.Brush.Color = Color.Yellow;
                    RECT vPaintRect = vTextRect;
                    User.InflateRect(ref vPaintRect, 5, 5);

                    ACanvas.RoundRect(vPaintRect, 5, 5);
                    vAnnotation.PaintRect = vPaintRect;  // 记录 PaintRect

                    // 绘制指向线
                    //ACanvas.Brush.Style = bsClear;
                    Point[] vPoints = new Point[3] {
                        new Point(vAnnotation.DrawItemRect.Right, vAnnotation.DrawItemRect.Bottom + 2),
                        new Point(ARect.Right, vAnnotation.DrawItemRect.Bottom + 2),
                        new Point(ARect.Right + 30, vPos) };
                    
                    ACanvas.DrawLines(vPoints);

                    // 绘制批注文本
                    User.DrawTextEx(ACanvas.Handle, vAnnotation.Text, -1, ref vTextRect, User.DT_TOP | User.DT_LEFT | User.DT_WORDBREAK, ref lpDrawTextParams);
                    vPos = vTextRect.Bottom + 5;
                }
            }
        }

        public void AddAnnotation(RECT ADrawItemRect, string AText)
        {
            Annotate vAnnotation = new Annotate();
            vAnnotation.DrawItemRect = ADrawItemRect;
            vAnnotation.Text = AText;
            this.Add(vAnnotation);
        }

        public void MouseDown(int X, int Y)
        {
            FIndex = -1;
            //            POINT vPt = new POINT(X, Y);
            RECT vRect;
            for (int i = 0; i < this.Count; i++)
            {
                vRect = this[i].PaintRect;
                if (User.PtInRect(ref vRect, X, Y) != 0)
                {
                    FIndex = i;
                    break;
                }
            }
        }
    }
}
