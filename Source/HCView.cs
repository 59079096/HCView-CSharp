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
using System.Text;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HC.View
{
    public delegate void LoadSectionProcHandler(ushort aFileVersion);

    public delegate void PaintEventHandler(HCCanvas aCanvas, PaintInfo aPaintInfo);
    public delegate bool HCCopyPasteEventHandler(int aFormat);

    // HCView必需是第一个类
    public class HCView : UserControl
    {
        private string FFileName;
        private string FPageNoFormat;
        private HCStyle FStyle;
        private List<HCSection> FSections;
        private HCUndoList FUndoList;
        private HCStatusScrollBar FHScrollBar;
        private HCRichScrollBar FVScrollBar;
        private IntPtr FDC = IntPtr.Zero;
        private Bitmap FDataBmp;
        private HCCanvas FDataBmpCanvas;
        private IntPtr FhImc = IntPtr.Zero;
        private int FActiveSectionIndex, FViewWidth, FViewHeight, FDisplayFirstSection, FDisplayLastSection;
        private uint FUpdateCount;
        private byte FPagePadding;
        private Single FZoom;
        private bool FAutoZoom;  // 自动缩放
        private bool FIsChanged;  // 是否发生了改变

        private HCAnnotatePre FAnnotatePre;  // 批注管理

        private HCViewModel FViewModel;  // 界面显示模式：页面、Web
        private HCCaret FCaret;
        private EventHandler FOnCaretChange, FOnVerScroll, FOnHorScroll, FOnSectionCreateItem, FOnSectionReadOnlySwitch,
            FOnSectionCurParaNoChange, FOnSectionActivePageChange;
        private StyleItemEventHandler FOnSectionCreateStyleItem;
        private SectionDataItemEventHandler FOnSectionCaretItemChanged;
        private FloatStyleItemEventHandler FOnSectionCreateFloatStyleItem;
        private OnCanEditEventHandler FOnSectionCanEdit;
        private TextEventHandler FOnSectionInsertTextBefor;
        private SectionDataItemEventHandler FOnSectionInsertItem, FOnSectionRemoveItem;
        private SectionDataItemNoFunEvent FOnSectionSaveItem;
        private SectionDataActionEventHandler FOnSectionAcceptAction;
        private SectionDrawItemPaintEventHandler FOnSectionDrawItemPaintAfter, FOnSectionDrawItemPaintBefor;

        private SectionPaintEventHandler FOnSectionPaintHeader, FOnSectionPaintFooter, FOnSectionPaintPage,
          FOnSectionPaintPaperBefor, FOnSectionPaintPaperAfter;
        private PaintEventHandler FOnPaintViewBefor, FOnPaintViewAfter;

        private EventHandler FOnChange, FOnChangedSwitch, FOnZoomChanged, FOnViewResize;

        private void SetPrintBySectionInfo(PageSettings aPageSettings, int aSectionIndex)
        {
            if (FSections[aSectionIndex].PaperSize == PaperKind.Custom)
            {
                aPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("Custom",
                    (int)Math.Round(FSections[aSectionIndex].PaperWidth / 25.4 * 100),
                    (int)Math.Round(FSections[aSectionIndex].PaperHeight / 25.4 * 100));
            }
            else
            {
                aPageSettings.PaperSize = new System.Drawing.Printing.PaperSize(FSections[aSectionIndex].PaperSize.ToString(),
                    (int)Math.Round(FSections[aSectionIndex].PaperWidth / 25.4 * 100),
                    (int)Math.Round(FSections[aSectionIndex].PaperHeight / 25.4 * 100));
            }

            aPageSettings.Margins.Left = 0;
            aPageSettings.Margins.Top = 0;
            aPageSettings.Margins.Right = 0;
            aPageSettings.Margins.Bottom = 0;
            //aPageSettings.Landscape = FSections[vSectionIndex].PaperOrientation == PaperOrientation.cpoLandscape;
        }

        private void GetViewWidth()
        {
            if (FVScrollBar.Visible)
                FViewWidth = Width - FVScrollBar.Width;
            else
                FViewWidth = Width;
        }

        private void GetViewHeight()
        {
            if (FHScrollBar.Visible)
                FViewHeight = Height - FHScrollBar.Height;
            else
                FViewHeight = Height;
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
                DoViewResize();
            }
        }

        private void DoVerScroll(object Sender, ScrollCode ScrollCode, int ScrollPos)
        {
            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret(false);
            CheckUpdateInfo();
            GetPagesAndActive();
            if (FOnVerScroll != null)
                FOnVerScroll(this, null);
        }

        private void DoHorScroll(object Sender, ScrollCode ScrollCode, int ScrollPos)
        {
            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret(false);
            CheckUpdateInfo();
            if (FOnHorScroll != null)
                FOnHorScroll(this, null);
        }

        private void DoSectionDataChange(Object Sender, EventArgs e)
        {
            DoChange();
        }

        private void DoSectionChangeTopLevelData(Object Sender, EventArgs e)
        {
            DoViewResize();
        }

        // 仅重绘和重建光标，不触发Change事件
        private void DoSectionDataCheckUpdateInfo(Object Sender, EventArgs e)
        {
            CheckUpdateInfo();
        }

        private void DoLoadFromStream(Stream aStream, HCStyle aStyle, LoadSectionProcHandler aLoadSectionProc)
        {
            aStream.Position = 0;
            string vFileExt = "";
            ushort vFileVersion = 0;
            byte vLang = 0;
            HC._LoadFileFormatAndVersion(aStream, ref vFileExt, ref vFileVersion, ref vLang);
            if (vFileExt != HC.HC_EXT)
                throw new Exception("加载失败，不是" + HC.HC_EXT + "文件！");

            DoLoadStreamBefor(aStream, vFileVersion);  // 触发加载前事件
            aStyle.LoadFromStream(aStream, vFileVersion);  // 加载样式表
            aLoadSectionProc(vFileVersion);  // 加载节数量、节数据
            DoLoadStreamAfter(aStream, vFileVersion);
            DoMapChanged();
        }

        private HCUndo DoUndoNew()
        {
            HCUndo Result = new HCSectionUndo();
            (Result as HCSectionUndo).SectionIndex = FActiveSectionIndex;
            (Result as HCSectionUndo).HScrollPos = FHScrollBar.Position;
            (Result as HCSectionUndo).VScrollPos = FVScrollBar.Position;
            Result.Data = ActiveSection.ActiveData;

            return Result;
        }

        private HCUndoGroupBegin DoUndoGroupBegin(int aItemNo, int aOffset)
        {
            HCUndoGroupBegin Result = new HCSectionUndoGroupBegin();
            (Result as HCSectionUndoGroupBegin).SectionIndex = FActiveSectionIndex;
            (Result as HCSectionUndoGroupBegin).HScrollPos = FHScrollBar.Position;
            (Result as HCSectionUndoGroupBegin).VScrollPos = FVScrollBar.Position;
            Result.Data = ActiveSection.ActiveData;
            Result.CaretDrawItemNo = ActiveSection.ActiveData.CaretDrawItemNo;

            return Result;
        }

        private HCUndoGroupEnd DoUndoGroupEnd(int aItemNo, int aOffset)
        {
            HCUndoGroupEnd Result = new HCSectionUndoGroupEnd();
            (Result as HCSectionUndoGroupEnd).SectionIndex = FActiveSectionIndex;
            (Result as HCSectionUndoGroupEnd).HScrollPos = FHScrollBar.Position;
            (Result as HCSectionUndoGroupEnd).VScrollPos = FVScrollBar.Position;
            Result.Data = ActiveSection.ActiveData;
            Result.CaretDrawItemNo = ActiveSection.ActiveData.CaretDrawItemNo;

            return Result;
        }

        private void DoUndo(HCUndo sender)
        {
            if (sender is HCSectionUndo)
            {
                if (FActiveSectionIndex != (sender as HCSectionUndo).SectionIndex)
                    SetActiveSectionIndex((sender as HCSectionUndo).SectionIndex);

                FHScrollBar.Position = (sender as HCSectionUndo).HScrollPos;
                FVScrollBar.Position = (sender as HCSectionUndo).VScrollPos;
            }
            else
            if (sender is HCSectionUndoGroupBegin)
            {
                if (FActiveSectionIndex != (sender as HCSectionUndoGroupBegin).SectionIndex)
                    SetActiveSectionIndex((sender as HCSectionUndoGroupBegin).SectionIndex);

                FHScrollBar.Position = (sender as HCSectionUndoGroupBegin).HScrollPos;
                FVScrollBar.Position = (sender as HCSectionUndoGroupBegin).VScrollPos;
            }

            ActiveSection.Undo(sender);
        }

        private void DoRedo(HCUndo sender)
        {
            if (sender is HCSectionUndo)
            {
                if (FActiveSectionIndex != (sender as HCSectionUndo).SectionIndex)
                    SetActiveSectionIndex((sender as HCSectionUndo).SectionIndex);

                FHScrollBar.Position = (sender as HCSectionUndo).HScrollPos;
                FVScrollBar.Position = (sender as HCSectionUndo).VScrollPos;
            }
            else
            if (sender is HCSectionUndoGroupEnd)
            {
                if (FActiveSectionIndex != (sender as HCSectionUndoGroupEnd).SectionIndex)
                    SetActiveSectionIndex((sender as HCSectionUndoGroupEnd).SectionIndex);

                FHScrollBar.Position = (sender as HCSectionUndoGroupEnd).HScrollPos;
                FVScrollBar.Position = (sender as HCSectionUndoGroupEnd).VScrollPos;
            }

            ActiveSection.Redo(sender);
        }

        private void DoUndoDestroy(HCUndo sender)
        {
            //sender的Data是ActiveSection.ActiveData，所以不需要释放
        }

        private void DoViewResize()
        {
            if (FOnViewResize != null)
                FOnViewResize(this, null);
        }

        /// <summary> 文档"背板"变动(数据无变化，如对称边距，缩放视图) </summary>
        private void DoMapChanged()
        {
            if (FUpdateCount == 0)
            {
                CalcScrollRang();
                CheckUpdateInfo();
            }
        }

        private void DoSectionReadOnlySwitch(Object sender, EventArgs e)
        {
            if (FOnSectionReadOnlySwitch != null)
                FOnSectionReadOnlySwitch(this, null);
        }

        private POINT DoSectionGetScreenCoord(int x, int y)
        {
            Point vPt = this.PointToScreen(new Point(x, y));
            return new POINT(vPt.X, vPt.Y);
        }

        private void DoSectionItemResize(HCCustomData aData, int aItemNo)
        {
            DoViewResize();
        }

        private void DoSectionPaintHeader(Object sender, int aPageIndex, RECT aRect, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            if (FOnSectionPaintHeader != null)
                FOnSectionPaintHeader(sender, aPageIndex, aRect, aCanvas, aPaintInfo);
        }

        private void DoSectionPaintFooter(Object sender, int aPageIndex, RECT aRect, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            HCSection vSection = sender as HCSection;
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

                string vS = string.Format(FPageNoFormat, vSectionStartPageIndex + vSection.PageNoFrom + aPageIndex, vAllPageCount);
                aCanvas.Brush.Style = HCBrushStyle.bsClear;

                aCanvas.Font.BeginUpdate();
                try
                {
                    aCanvas.Font.Size = 10;
                    aCanvas.Font.Family = "宋体";
                }
                finally
                {
                    aCanvas.Font.EndUpdate();
                }

                aCanvas.TextOut(aRect.Left + (aRect.Width - aCanvas.TextWidth(vS)) / 2, aRect.Top + vSection.Footer.Height, vS);
            }

            if (FOnSectionPaintFooter != null)
                FOnSectionPaintFooter(vSection, aPageIndex, aRect, aCanvas, aPaintInfo);
        }

        private void DoSectionPaintPage(Object sender, int aPageIndex, RECT aRect, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            if (FOnSectionPaintPage != null)
                FOnSectionPaintPage(sender, aPageIndex, aRect, aCanvas, aPaintInfo);
        }

        private void DoSectionPaintPaperBefor(object sender, int aPageIndex, RECT aRect, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            if (aPaintInfo.Print && (FAnnotatePre.DrawCount > 0))
                FAnnotatePre.ClearDrawAnnotate();

            if (FOnSectionPaintPaperBefor != null)
                FOnSectionPaintPaperBefor(sender, aPageIndex, aRect, aCanvas, aPaintInfo);
        }

        private void DoSectionPaintPaperAfter(object sender, int aPageIndex,
            RECT aRect, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            // HCView广告信息，如介意可以删掉
            if (!aPaintInfo.Print && (FViewModel == HCViewModel.hvmFilm) && ((sender as HCSection).PagePadding > 10))
            {
                aCanvas.Brush.Style = HCBrushStyle.bsClear;

                aCanvas.Font.BeginUpdate();
                try
                {
                    aCanvas.Font.Size = 10;
                    aCanvas.Font.Family = "宋体";
                    aCanvas.Font.Color = Color.FromArgb(0xD0, 0xD1, 0xD5);
                    aCanvas.Font.FontStyles.Value = 0;
                }
                finally
                {
                    aCanvas.Font.EndUpdate();
                }

                aCanvas.TextOut(aRect.Left, aRect.Bottom + 4, "编辑器由 HCView 提供，技术交流QQ群：649023932");
            }

            if (FAnnotatePre.Visible)  // 当前页有批注，绘制批注
                FAnnotatePre.PaintDrawAnnotate(sender, aRect, aCanvas, aPaintInfo);

            if (FOnSectionPaintPaperAfter != null)
                FOnSectionPaintPaperAfter(sender, aPageIndex, aRect, aCanvas, aPaintInfo);
        }

        private void DoSectionDrawItemAnnotate(object sender, HCCustomData aData,
            int aDrawItemNo, RECT aDrawRect, HCDataAnnotate aDataAnnotate)
        {
            HCDrawAnnotate vDrawAnnotate = new HCDrawAnnotate();
            vDrawAnnotate.Data = aData;
            vDrawAnnotate.DrawRect = aDrawRect;
            vDrawAnnotate.DataAnnotate = aDataAnnotate;
            FAnnotatePre.AddDrawAnnotate(vDrawAnnotate);
        }

        private HCUndoList DoSectionGetUndoList()
        {
            return FUndoList;
        }

        private void DoSectionInsertAnnotate(object sender, HCCustomData aData, HCDataAnnotate aDataAnnotate)
        {
            FAnnotatePre.InsertDataAnnotate(aDataAnnotate);
        }

        private void DoSectionRemoveAnnotate(object sender, HCCustomData aData, HCDataAnnotate aDataAnnotate)
        {
            FAnnotatePre.RemoveDataAnnotate(aDataAnnotate);
        }

        private void DoSectionCurParaNoChange(object sender, EventArgs e)
        {
            if (FOnSectionCurParaNoChange != null)
                FOnSectionCurParaNoChange(sender, e);
        }

        private void DoSectionActivePageChange(object sender, EventArgs e)
        {
            if (FOnSectionActivePageChange != null)
                FOnSectionActivePageChange(sender, e);
        }

        private delegate void SetStyleInvalidateRect(RECT aRect);

        private void DoStyleInvalidateRect(RECT aRect)
        {
            //if (this.InvokeRequired)
            //{
            //    if (this.IsHandleCreated)
            //    {
            //        SetStyleInvalidateRect srt = new SetStyleInvalidateRect(DoStyleInvalidateRect);
            //        this.Invoke(srt, new object[] { aRect });
            //    }
            //}
            //else
                UpdateView(aRect);
        }

        private void DoAnnotatePreUpdateView(object sender, EventArgs e)
        {
            if (FAnnotatePre.Visible)
            {
                FStyle.UpdateInfoRePaint();
                DoMapChanged();
            }
            else
                UpdateView();
        }

        private HCSection NewDefaultSection()
        {
            HCSection Result = new HCSection(FStyle);
            // 创建节后马上赋值事件（保证后续插入表格等需要这些事件的操作可获取到事件）
            Result.OnDataChange = DoSectionDataChange;
            Result.OnChangeTopLevelData = DoSectionChangeTopLevelData;
            Result.OnCheckUpdateInfo = DoSectionDataCheckUpdateInfo;
            Result.OnCreateItem = DoSectionCreateItem;
            Result.OnDataAcceptAction = DoSectionAcceptAction;
            Result.OnCreateItemByStyle = DoSectionCreateStyleItem;
            Result.OnCreateFloatItemByStyle = DoSectionCreateFloatStyleItem;
            Result.OnCanEdit = DoSectionCanEdit;
            Result.OnInsertTextBefor = DoSectionInsertTextBefor;
            Result.OnInsertItem = DoSectionInsertItem;
            Result.OnRemoveItem = DoSectionRemoveItem;
            Result.OnSaveItem = DoSectionSaveItem;
            Result.OnItemMouseDown = DoSectionItemMouseDown;
            Result.OnItemMouseUp = DoSectionItemMouseUp;
            Result.OnItemResize = DoSectionItemResize;
            Result.OnReadOnlySwitch = DoSectionReadOnlySwitch;
            Result.OnGetScreenCoord = DoSectionGetScreenCoord;
            Result.OnDrawItemPaintAfter = DoSectionDrawItemPaintAfter;
            Result.OnDrawItemPaintBefor = DoSectionDrawItemPaintBefor;
            Result.OnDrawItemPaintContent = DoSectionDrawItemPaintContent;
            Result.OnPaintHeader = DoSectionPaintHeader;
            Result.OnPaintFooter = DoSectionPaintFooter;
            Result.OnPaintPage = DoSectionPaintPage;
            Result.OnPaintPaperBefor = DoSectionPaintPaperBefor;
            Result.OnPaintPaperAfter = DoSectionPaintPaperAfter;
            Result.OnInsertAnnotate = DoSectionInsertAnnotate;
            Result.OnRemoveAnnotate = DoSectionRemoveAnnotate;
            Result.OnDrawItemAnnotate = DoSectionDrawItemAnnotate;
            Result.OnGetUndoList = DoSectionGetUndoList;
            Result.OnCurParaNoChange = DoSectionCurParaNoChange;
            Result.OnCaretItemChanged = DoSectionCaretItemChanged;
            Result.OnActivePageChange = DoSectionActivePageChange;

            return Result;
        }

        private RECT GetViewRect()
        {
            return HC.Bounds(0, 0, FViewWidth, FViewHeight);
        }

        private int GetPageIndexTop(int aPageIndex)
        {
            int vPageIndex = -1;
            int vSectionIndex = GetSectionPageIndexByPageIndex(aPageIndex, ref vPageIndex);
            int vResult = GetSectionTopFilm(vSectionIndex);

            if (vPageIndex > 0)
            {
                if (FSections[vSectionIndex].ViewModel == HCViewModel.hvmFilm)
                    vResult = vResult + vPageIndex * (FPagePadding + FSections[vSectionIndex].PaperHeightPix);
                else
                    vResult = vResult + vPageIndex * (FPagePadding + FSections[vSectionIndex].GetPageHeight());
            }

            return vResult;
        }

        private void DoPageUp(object sender, EventArgs e)
        {
            int vPageIndex = GetPagePreviewFirst();
            if (vPageIndex > 0)
                FVScrollBar.Position = GetPageIndexTop(vPageIndex - 1);
        }

        private void DoPageDown(object sender, EventArgs e)
        {
            int vPageIndex = GetPagePreviewFirst();
            if (vPageIndex < GetPageCount() - 1)
                FVScrollBar.Position = GetPageIndexTop(vPageIndex + 1);
        }

        private void ReBuildCaret()
        {
            if (FCaret == null)
                return;

            if ((!this.Focused && !FStyle.UpdateInfo.Draging) || ActiveSection.SelectExists())
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

            FVScrollBar.SetAreaPos(-1, vCaretInfo.Y, vCaretInfo.Height);

            FCaret.X = ZoomIn(GetSectionDrawLeft(FActiveSectionIndex) + vCaretInfo.X) - FHScrollBar.Position;
            FCaret.Y = ZoomIn(GetSectionTopFilm(FActiveSectionIndex) + vCaretInfo.Y) - FVScrollBar.Position;
            FCaret.Height = ZoomIn(vCaretInfo.Height);

            if (!FStyle.UpdateInfo.ReScroll)
            {
                if ((FCaret.X < 0) || (FCaret.X > FViewWidth))
                {
                    FCaret.Hide();
                    return;
                }

                if ((FCaret.Y + FCaret.Height < 0) || (FCaret.Y > FViewHeight))
                {
                    FCaret.Hide();
                    return;
                }
            }
            else  // 非滚动条(方向键、点击等)引起的光标位置变化
            {
                if (FCaret.Height < FViewHeight)
                {
                    if (!FCaret.VScroll)
                    {
                        FCaret.VScroll = true;
                        try
                        {
                            if (FCaret.Y < 0)
                                FVScrollBar.Position = FVScrollBar.Position + FCaret.Y - FPagePadding;
                            else
                            if (FCaret.Y + FCaret.Height + FPagePadding > FViewHeight)
                                FVScrollBar.Position = FVScrollBar.Position + FCaret.Y + FCaret.Height + FPagePadding - FViewHeight;
                        }
                        finally
                        {
                            FCaret.VScroll = false;
                        }
                    }

                    if (!FCaret.HScroll)
                    {
                        FCaret.HScroll = true;
                        try
                        {
                            if (FCaret.X < 0)
                                FHScrollBar.Position = FHScrollBar.Position + FCaret.X - FPagePadding;
                            else
                            if (FCaret.X + FPagePadding > FViewWidth)
                                FHScrollBar.Position = FHScrollBar.Position + FCaret.X + FPagePadding - FViewWidth;
                        }
                        finally
                        {
                            FCaret.HScroll = false;
                        }
                    }
                }
            }

            if (FCaret.VScroll || FCaret.HScroll)
                return;

            if (FCaret.Y + FCaret.Height > FViewHeight)
                FCaret.Height = FViewHeight - FCaret.Y;

            FCaret.Show();
            DoCaretChange();
        }

        private void GetSectionByCrood(int X, int Y, ref int aSectionIndex)
        {
            aSectionIndex = -1;
            int vY = 0;
            for (int i = 0; i <= FSections.Count - 1; i++)
            {
                vY = vY + FSections[i].GetFilmHeight();
                if (vY > Y)
                {
                    aSectionIndex = i;
                    break;
                }
            }
            if ((aSectionIndex < 0) && (vY + FPagePadding >= Y))
                aSectionIndex = FSections.Count - 1;

            if (aSectionIndex < 0)
                aSectionIndex = 0;  // throw new Exception("没有获取到正确的节序号！"); 只一页且高度小于视口高度时点击到底部按第0节处理
        }

        private void SetZoom(Single Value)
        {
            Single vValue = Value;
            if (vValue < 0.25)
                vValue = 0.25f;
            else
                if (vValue > 5)
                vValue = 5f;

            if (FZoom != vValue)
            {
                this.Focus();
                FZoom = vValue;
                FStyle.UpdateInfoRePaint();
                FStyle.UpdateInfoReCaret(false);
                if (FOnZoomChanged != null)
                    FOnZoomChanged(this, null);

                DoMapChanged();
                DoViewResize();
            }
        }

        private int GetHScrollValue()
        {
            return FHScrollBar.Position;
        }

        private int GetCurStyleNo()
        {
            return ActiveSection.CurStyleNo;
        }

        private int GetCurParaNo()
        {
            return ActiveSection.CurParaNo;
        }

        private bool GetShowLineActiveMark()
        {
            return FSections[0].Page.ShowLineActiveMark;
        }

        private void SetShowLineActiveMark(bool Value)
        {
            for (int i = 0; i <= FSections.Count - 1; i++)
                FSections[i].Page.ShowLineActiveMark = Value;

            UpdateView();
        }

        private bool GetShowLineNo()
        {
            return FSections[0].Page.ShowLineNo;
        }

        private void SetShowLineNo(bool Value)
        {
            for (int i = 0; i <= FSections.Count - 1; i++)
                FSections[i].Page.ShowLineNo = Value;

            UpdateView();
        }

        private bool GetShowUnderLine()
        {
            return FSections[0].Page.ShowUnderLine;
        }

        private void SetShowUnderLine(bool Value)
        {
            for (int i = 0; i <= FSections.Count - 1; i++)
                FSections[i].Page.ShowUnderLine = Value;

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

            UpdateView();
        }

        private void SetPageNoFormat(string value)
        {
            if (FPageNoFormat != value)
            {
                FPageNoFormat = value;
                UpdateView();
            }
        }

        private void SetViewModel(HCViewModel value)
        {
            if (FViewModel != value)
            {
                FViewModel = value;

                for (int i = 0; i <= FSections.Count - 1; i++)
                    FSections[i].ViewModel = value;

                if (value == HCViewModel.hvmFilm)
                    PagePadding = 20;
                else
                    PagePadding = 0;

                //FHRuler.Visible = FViewModel = HCViewModel.hvmFilm;
                //FVRuler.Visible = FHRuler.Visible;
            }
        }

        private void SetActiveSectionIndex(int value)
        {
            if (FActiveSectionIndex != value)
            {
                if (FActiveSectionIndex >= 0)
                    FSections[FActiveSectionIndex].DisActive();

                FActiveSectionIndex = value;
                DoViewResize();
            }
        }

        private void SetIsChanged(bool Value)
        {
            if (FIsChanged != Value)
            {
                FIsChanged = Value;
                if (FOnChangedSwitch != null)
                    FOnChangedSwitch(this, null);
            }
        }

        private void SetPagePadding(byte value)
        {
            if (FPagePadding != value)
            {
                FPagePadding = value;
                for (int i = 0; i <= FSections.Count - 1; i++)
                    FSections[i].PagePadding = FPagePadding;

                FStyle.UpdateInfoRePaint();
                FStyle.UpdateInfoReCaret(false);
                DoMapChanged();
                DoViewResize();
            }
        }

        /// <summary> 获取当前节对象 </summary>
        private HCSection GetActiveSection()
        {
            return FSections[FActiveSectionIndex];
        }

        private void AutoScrollTimer(bool aStart)
        {
            if (!aStart)
                User.KillTimer(Handle, 2);
            else
            {
                if (User.SetTimer(Handle, 2, 100, IntPtr.Zero) == 0)
                    throw new Exception(HC.HCS_EXCEPTION_TIMERRESOURCEOUTOF);
            }
        }

        private void GetPagesAndActive()
        {
            FHScrollBar.Statuses[0].Text = "预览" + (PagePreviewFirst + 1).ToString()
                + " 光标" + (ActivePageIndex + 1).ToString()
                + "/" + PageCount.ToString() + "页";
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();
            if (!DesignMode)
                FCaret = new HCCaret(this.Handle);

            if (FDC == IntPtr.Zero)
                FDC = User.GetDC(this.Handle);

            FhImc = Imm.ImmGetContext(this.Handle);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            User.ReleaseDC(this.Handle, FDC);

            Imm.ImmReleaseContext(this.Handle, FhImc);
            base.OnHandleDestroyed(e);
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);
            drgevent.Effect = DragDropEffects.All;
            FStyle.UpdateInfo.Draging = true;
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            FStyle.UpdateInfo.Draging = false;
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);
            Point vPt = PointToClient(new Point(drgevent.X, drgevent.Y));
            MouseEventArgs vMouseArgs = new MouseEventArgs(MouseButtons.None, 0, vPt.X, vPt.Y, 0);
            OnMouseMove(vMouseArgs);
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);
            FStyle.UpdateInfo.Draging = false;
            if (DoPasteRequest(User.CF_TEXT))
                this.InsertText(drgevent.Data.GetData(DataFormats.Text).ToString());
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
            {
                base.OnPaint(e);
                return;
            }

            GDI.BitBlt(FDC, 0, 0, FViewWidth, FViewHeight, FDataBmpCanvas.Handle, 0, 0, GDI.SRCCOPY);

            using (SolidBrush vBrush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(vBrush, new Rectangle(FVScrollBar.Left, FHScrollBar.Top, FVScrollBar.Width, FHScrollBar.Height));
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GetViewWidth();
            GetViewHeight();

            if ((FViewWidth > 0) && (FViewHeight > 0))
            {
                FDataBmp = new Bitmap(FViewWidth, FViewHeight);  // 设置为除滚动条外的大小
                if (FDataBmpCanvas == null)
                    FDataBmpCanvas = new HCCanvas();

                FDataBmpCanvas.Graphics = Graphics.FromImage(FDataBmp);
            }

            if (FAutoZoom)
            {
                if (FAnnotatePre.Visible)
                    FZoom = (FViewWidth - FPagePadding * 2) / (ActiveSection.PaperWidthPix + HC.AnnotationWidth);
                else
                    FZoom = (FViewWidth - FPagePadding * 2) / ActiveSection.PaperWidthPix;
            }

            CalcScrollRang();

            FStyle.UpdateInfoRePaint();
            if (FCaret != null)
                FStyle.UpdateInfoReCaret(false);

            CheckUpdateInfo();
            DoViewResize();
        }

        protected virtual void DoChange()
        {
            SetIsChanged(true);
            DoMapChanged();
            if (FOnChange != null)
                FOnChange(this, null);
        }

        protected virtual void DoCaretChange()
        {
            GetPagesAndActive();
            if (FOnCaretChange != null)
                FOnCaretChange(this, null);
        }

        protected virtual void DoKillFocus()
        {
            if (FCaret != null)
            {
                FCaret.Hide(true);
                UpdateView(HC.Bounds(FCaret.X - 1, FCaret.Y, FCaret.Width + 1, FCaret.Height));
            }
        }

        protected virtual void DoSectionCreateItem(Object sender, EventArgs e)
        {
            if (FOnSectionCreateItem != null)
                FOnSectionCreateItem(this, null);
        }

        protected virtual bool DoSectionAcceptAction(object sender, HCCustomData aData, int aItemNo, int aOffset, HCAction aAction)
        {
            if (FOnSectionAcceptAction != null)
                return FOnSectionAcceptAction(sender, aData, aItemNo, aOffset, aAction);
            else
                return true;
        }

        protected virtual HCCustomItem DoSectionCreateStyleItem(HCCustomData aData, int aStyleNo)
        {
            if (FOnSectionCreateStyleItem != null)
                return FOnSectionCreateStyleItem(aData, aStyleNo);
            else
                return null;
        }

        protected virtual void DoSectionCaretItemChanged(Object sender, HCCustomData data, HCCustomItem item)
        {
            if (FOnSectionCaretItemChanged != null)
                FOnSectionCaretItemChanged(sender, data, item);
        }

        protected virtual HCCustomFloatItem DoSectionCreateFloatStyleItem(HCSectionData aData, int aStyleNo)
        {
            if (FOnSectionCreateFloatStyleItem != null)
                return FOnSectionCreateFloatStyleItem(aData, aStyleNo);
            else
                return null;
        }

        protected virtual void DoSectionInsertItem(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            if (FOnSectionInsertItem != null)
                FOnSectionInsertItem(sender, aData, aItem);
        }

        protected virtual void DoSectionRemoveItem(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            if (FOnSectionRemoveItem != null)
                FOnSectionRemoveItem(sender, aData, aItem);
        }

        protected virtual bool DoSectionSaveItem(object sender, HCCustomData aData, int aItemNo)
        {
            if (FOnSectionSaveItem != null)
                return FOnSectionSaveItem(sender, aData, aItemNo);
            else
                return true;
        }

        protected virtual void DoSectionItemMouseDown(object sender, HCCustomData aData, int aItemNo, int aOffset, MouseEventArgs e)
        {

        }

        protected virtual void DoSectionItemMouseUp(object sender, HCCustomData aData, int aItemNo, int aOffset, MouseEventArgs e)
        {
            if (((Control.ModifierKeys & Keys.Shift) == Keys.Shift) && (aData.Items[aItemNo].HyperLink != ""))
                System.Diagnostics.Process.Start(aData.Items[aItemNo].HyperLink);
        }

        protected virtual bool DoSectionCanEdit(object sender)
        {
            if (FOnSectionCanEdit != null)
                return FOnSectionCanEdit(sender);
            else
                return true;
        }

        protected virtual bool DoSectionInsertTextBefor(HCCustomData aData, int aItemNo, int aOffset, string aText)
        {
            if (FOnSectionInsertTextBefor != null)
                return FOnSectionInsertTextBefor(aData, aItemNo, aOffset, aText);
            else
                return true;
        }

        protected virtual void DoSectionDrawItemPaintBefor(object sender, HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (FOnSectionDrawItemPaintBefor != null)
                FOnSectionDrawItemPaintBefor(this, aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight,
                    aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        protected virtual void DoSectionDrawItemPaintContent(HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect, RECT aClearRect, string aDrawText,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            // 背景处理完，绘制文本前触发，可处理高亮关键字
        }

        protected virtual void DoSectionDrawItemPaintAfter(object sender, HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (aData.Items[aItemNo].HyperLink != "")
            {
                aCanvas.Pen.BeginUpdate();
                try
                {
                    aCanvas.Pen.Style = HCPenStyle.psSolid;
                    aCanvas.Pen.Color = Color.Blue;
                    aCanvas.Pen.Width = 1;
                }
                finally
                {
                    aCanvas.Pen.EndUpdate();
                }

                aCanvas.DrawLine(aDrawRect.Left, aDrawRect.Bottom, aDrawRect.Right, aDrawRect.Bottom);
            }

            if (FOnSectionDrawItemPaintAfter != null)
                FOnSectionDrawItemPaintAfter(this, aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight,
                    aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        /// <summary> 实现插入文本 </summary>
        protected virtual bool DoInsertText(string aText)
        {
            return ActiveSection.InsertText(aText);
        }

        /// <summary> 复制前，便于控制是否允许复制 </summary>
        protected virtual bool DoCopyRequest(int aFormat)
        {
            return true;
        }

        /// <summary> 粘贴前，便于控制是否允许粘贴 </summary>
        protected virtual bool DoPasteRequest(int aFormat)
        {
            return true;
        }

        /// <summary> 复制前，便于订制特征数据如内容来源 </summary>
        protected virtual void DoCopyAsStream(Stream aStream) { }

        /// <summary> 粘贴前，便于确认订制特征数据如内容来源 </summary>
        protected virtual bool DoPasteFormatStream(Stream aStream)
        {
            return true;
        }

        /// <summary> 视图绘制开始 </summary>
        protected virtual void DoPaintViewBefor(HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (FOnPaintViewBefor != null)
                FOnPaintViewBefor(aCanvas, aPaintInfo);
        }

        /// <summary> 视图绘制完成 </summary>
        protected virtual void DoPaintViewAfter(HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (FOnPaintViewAfter != null)
                FOnPaintViewAfter(aCanvas, aPaintInfo);
        }

        /// <summary> 保存文档前触发事件，便于订制特征数据 </summary>
        protected virtual void DoSaveStreamBefor(Stream aStream) { }

        /// <summary> 保存文档后触发事件，便于订制特征数据 </summary>
        protected virtual void DoSaveStreamAfter(Stream aStream)
        {
            //SetIsChanged(false);
        }

        /// <summary> 读取文档前触发事件，便于确认订制特征数据 </summary>
        protected virtual void DoLoadStreamBefor(Stream aStream, ushort aFileVersion) { }

        /// <summary> 读取文档后触发事件，便于确认订制特征数据 </summary>
        protected virtual void DoLoadStreamAfter(Stream aStream, ushort aFileVersion) { }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int vSectionIndex = -1;
            GetSectionByCrood(ZoomOut(FHScrollBar.Position + e.X), ZoomOut(FVScrollBar.Position + e.Y), ref vSectionIndex);
            if (vSectionIndex != FActiveSectionIndex)
                SetActiveSectionIndex(vSectionIndex);

            if (FActiveSectionIndex < 0)
                return;

            int vSectionDrawLeft = GetSectionDrawLeft(FActiveSectionIndex);

            if (FAnnotatePre.DrawCount > 0)  // 有批注被绘制
                FAnnotatePre.MouseDown(ZoomOut(e.X), ZoomOut(e.Y));

            // 映射到节页面(白色区域)
            Point vPt = new Point();
            vPt.X = ZoomOut(FHScrollBar.Position + e.X) - vSectionDrawLeft;
            vPt.Y = ZoomOut(FVScrollBar.Position + e.Y) - GetSectionTopFilm(FActiveSectionIndex);
            //vPageIndex := FSections[FActiveSectionIndex].GetPageByFilm(vPt.Y);
            MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vPt.X, vPt.Y, e.Delta);
            FSections[FActiveSectionIndex].MouseDown(vMouseArgs);

            CheckUpdateInfo();  // 换光标、切换激活Item

            base.OnMouseDown(e);
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

                if (FStyle.UpdateInfo.Selecting)
                    AutoScrollTimer(true);
            }

            if (FAnnotatePre.DrawCount > 0)  // 有批注被绘制
                FAnnotatePre.MouseMove(ZoomOut(e.X), ZoomOut(e.Y));

            CheckUpdateInfo();

            if (FStyle.UpdateInfo.Draging)
                Cursor.Current = HC.GCursor;
            else
                this.Cursor = HC.GCursor;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (FStyle.UpdateInfo.Selecting)
                AutoScrollTimer(false);

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

            FStyle.UpdateInfo.Selecting = false;
            FStyle.UpdateInfo.Draging = false;

            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //base.OnMouseWheel(e);
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)  // 按下ctrl
            {
                if (e.Delta > 0)  // 向上
                    Zoom = Zoom + 0.1f;
                else
                    Zoom = Zoom - 0.1f;
            }
            else
            {
                if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)  // 按下shift
                    FHScrollBar.Position -= e.Delta;
                else
                    FVScrollBar.Position -= e.Delta;
            }
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
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)  // 按下ctrl
                return;

            ActiveSection.KeyPress(e);
        }

        protected virtual void UpdateImeComposition(int aLParam)
        {
            if ((aLParam & Imm.GCS_RESULTSTR) != 0)
            {
                //if (FhImc != IntPtr.Zero)
                {
                    int vSize = Imm.ImmGetCompositionString(FhImc, Imm.GCS_RESULTSTR, null, 0);
                    if (vSize > 0)
                    {
                        byte[] vBuffer = new byte[vSize];
                        Imm.ImmGetCompositionString(FhImc, Imm.GCS_RESULTSTR, vBuffer, vSize);
                        string vS = System.Text.Encoding.Default.GetString(vBuffer);
                        if (vS != "")
                            InsertText(vS);
                    }
                }
            }
        }

        protected virtual void UpdateImePosition()
        {
            // 全局 FhImc
            /*LOGFONT vLogFont = new LOGFONT();
            Imm.ImmGetCompositionFont(FhImc, ref vLogFont);
            vLogFont.lfHeight = 22;
            Imm.ImmSetCompositionFont(FhImc, ref vLogFont);*/

            // 告诉输入法当前光标位置信息
            COMPOSITIONFORM vCF = new COMPOSITIONFORM();
            vCF.ptCurrentPos = new POINT(FCaret.X, FCaret.Y + FCaret.Height + 4);  // 输入法弹出窗体位置
            vCF.dwStyle = 0x0020;

            Rectangle vr = this.ClientRectangle;
            vCF.rcArea = new RECT(vr.Left, vr.Top, vr.Right, vr.Bottom);
            Imm.ImmSetCompositionWindow(FhImc, ref vCF);
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
                    //case User.WM_NCPAINT:
                    base.WndProc(ref Message);
                    if (Message.HWnd != this.Handle)
                    {
                        FStyle.UpdateInfoReCaret(false);
                        FStyle.UpdateInfoRePaint();
                        //FStyle.UpdateInfoReScroll();
                        CheckUpdateInfo();
                    }
                    return;

                case User.WM_KILLFOCUS:
                    base.WndProc(ref Message);
                    if (Message.WParam != Handle)
                        DoKillFocus();

                    return;

                case User.WM_IME_SETCONTEXT:
                    if (Message.WParam.ToInt32() == 1)
                    {
                        Imm.ImmAssociateContext(this.Handle, FhImc);
                    }
                    break;

                case User.WM_IME_COMPOSITION:
                    UpdateImeComposition(Message.LParam.ToInt32());
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

                case User.WM_TIMER:
                    if (Message.WParam.ToInt32() == 2)  // 划选时自动滚动
                    {
                        POINT vPt = new POINT();
                        User.GetCursorPos(out vPt);
                        User.ScreenToClient(Handle, ref vPt);
                        if (vPt.Y > this.Height - FHScrollBar.Height)
                            FVScrollBar.Position = FVScrollBar.Position + 60;
                        else
                            if (vPt.Y < 0)
                                FVScrollBar.Position = FVScrollBar.Position - 60;

                        if (vPt.X > this.Width - FVScrollBar.Width)
                            FHScrollBar.Position = FHScrollBar.Position + 60;
                        else
                            if (vPt.X < 0)
                                FHScrollBar.Position = FHScrollBar.Position - 60;
                    }
                    break;

                case User.WM_IME_CHAR:
                    Message.Result = (IntPtr)1;
                    return;
            }

            base.WndProc(ref Message);
        }

        protected void CalcScrollRang()
        {
            int vVMax = 0, vHMax = 0, vWidth = 0;

            if (FViewModel == HCViewModel.hvmFilm)
            {
                vHMax = FSections[0].PaperWidthPix;
                for (int i = 0; i <= FSections.Count - 1; i++)  //  计算节垂直总和，以及节中最宽的页宽
                {
                    vVMax = vVMax + FSections[i].GetFilmHeight();

                    vWidth = FSections[i].PaperWidthPix;

                    if (vWidth > vHMax)
                        vHMax = vWidth;
                }
            }
            else
            {
                vHMax = FSections[0].GetPageWidth();
                for (int i = 0; i <= FSections.Count - 1; i++)  //  计算节垂直总和，以及节中最宽的页宽
                {
                    vVMax = vVMax + FSections[i].GetFilmHeight();

                    vWidth = FSections[i].GetPageWidth();

                    if (vWidth > vHMax)
                        vHMax = vWidth;
                }
            }

            if (FAnnotatePre.Visible)
                vHMax = vHMax + HC.AnnotationWidth;

            vVMax = ZoomIn(vVMax + FPagePadding);  // 补充最后一页后面的PagePadding
            vHMax = ZoomIn(vHMax + FPagePadding + FPagePadding);

            FVScrollBar.Max = vVMax;
            FHScrollBar.Max = vHMax;
        }

        /// <summary> 是否由滚动条位置变化引起的更新 </summary>
        protected void CheckUpdateInfo()
        {
            if (FUpdateCount > 0)
                return;

            if ((FCaret != null) && FStyle.UpdateInfo.ReCaret)
            {
                ReBuildCaret();
                FStyle.UpdateInfo.ReCaret = false;
                FStyle.UpdateInfo.ReStyle = false;
                FStyle.UpdateInfo.ReScroll = false;
                UpdateImePosition();
            }

            if (FStyle.UpdateInfo.RePaint)
            {
                FStyle.UpdateInfo.RePaint = false;
                UpdateView();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "HCView";
            this.ResumeLayout(false);
        }

        /// <summary> 便于子类在构造函数前执行 </summary>
        protected virtual void Create()
        {

        }

        protected IntPtr hImc
        {
            get { return FhImc; }
        }

        protected HCCaret Caret
        {
            get { return FCaret; }
        }

        public HCView()
        {
            HCUnitConversion.Initialization();
            //this.DoubleBuffered = true;
            Create();  // 便于子类在构造函数前执行
            HC.HCExtFormat = DataFormats.GetFormat(HC.HC_EXT);
            SetStyle(ControlStyles.Selectable, true);  // 可接收焦点
            this.BackColor = Color.FromArgb(82, 89, 107);

            FUndoList = new HCUndoList();
            FUndoList.OnUndo = DoUndo;
            FUndoList.OnRedo = DoRedo;
            FUndoList.OnUndoNew = DoUndoNew;
            FUndoList.OnUndoGroupStart = DoUndoGroupBegin;
            FUndoList.OnUndoGroupEnd = DoUndoGroupEnd;
            FUndoList.OnUndoDestroy = DoUndoDestroy;

            FAnnotatePre = new HCAnnotatePre();
            FAnnotatePre.OnUpdateView = DoAnnotatePreUpdateView;

            FFileName = "";
            FPageNoFormat = "{0}/{1}";
            FIsChanged = false;
            FZoom = 1;
            FAutoZoom = false;
            FViewModel = HCViewModel.hvmFilm;
            FPagePadding = 20;

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
            FVScrollBar.OnScroll = DoVerScroll;
            FVScrollBar.OnPageUpClick = DoPageUp;
            FVScrollBar.OnPageDownClick = DoPageDown;
            // 水平滚动条，范围在Resize中设置
            FHScrollBar = new HCStatusScrollBar();
            FHScrollBar.AddStatus(100);
            FHScrollBar.Orientation = Orientation.oriHorizontal;
            FHScrollBar.OnScroll = DoHorScroll;

            this.AllowDrop = true;
            this.Controls.Add(FHScrollBar);
            this.Controls.Add(FVScrollBar);
            this.ResumeLayout();

            CalcScrollRang();
        }

        ~HCView()
        {
            FStyle.States.Include(HCState.hosDestroying);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);

            FVScrollBar.Left = Width - FVScrollBar.Width;
            FVScrollBar.Height = Height - FHScrollBar.Height;
            //
            FHScrollBar.Top = Height - FHScrollBar.Height;
            FHScrollBar.Width = Width - FVScrollBar.Width;

            this.Refresh();
        }

        /// <summary> 删除不使用的文本样式 </summary>
        public static void DeleteUnUsedStyle(HCStyle aStyle, List<HCSection> aSections, HashSet<SectionArea> aParts)  //  = (saHeader, saPage, saFooter)
        {
            for (int i = 0; i <= aStyle.TextStyles.Count - 1; i++)
            {
                aStyle.TextStyles[i].CheckSaveUsed = false;
                aStyle.TextStyles[i].TempNo = HCStyle.Null;
            }

            for (int i = 0; i <= aStyle.ParaStyles.Count - 1; i++)
            {
                aStyle.ParaStyles[i].CheckSaveUsed = false;
                aStyle.ParaStyles[i].TempNo = HCStyle.Null;
            }

            for (int i = 0; i <= aSections.Count - 1; i++)
                aSections[i].MarkStyleUsed(true, aParts);
            
            int vUnCount = 0;
            for (int i = 0; i <= aStyle.TextStyles.Count - 1; i++)
            {
                if (aStyle.TextStyles[i].CheckSaveUsed)
                    aStyle.TextStyles[i].TempNo = i - vUnCount;
                else
                    vUnCount++;
            }
            
            vUnCount = 0;
            for (int i = 0; i <= aStyle.ParaStyles.Count - 1; i++)
            {
                if (aStyle.ParaStyles[i].CheckSaveUsed)
                    aStyle.ParaStyles[i].TempNo = i - vUnCount;
                else
                    vUnCount++;
            }

            HCCustomData vData = null;
            for (int i = 0; i <= aSections.Count - 1; i++)
            {
                aSections[i].MarkStyleUsed(false, aParts);

                vData = aSections[i].ActiveData.GetTopLevelData();
                if (vData.CurStyleNo > HCStyle.Null)
                    vData.CurStyleNo = aStyle.TextStyles[vData.CurStyleNo].TempNo;

                vData.CurParaNo = aStyle.ParaStyles[vData.CurParaNo].TempNo;
            }

            for (int i = aStyle.TextStyles.Count - 1; i >= 0; i--)
            {
                if (!aStyle.TextStyles[i].CheckSaveUsed)
                    aStyle.TextStyles.RemoveAt(i);
            }

            for (int i = aStyle.ParaStyles.Count - 1; i >= 0; i--)
            {
                if (!aStyle.ParaStyles[i].CheckSaveUsed)
                    aStyle.ParaStyles.RemoveAt(i);
            }
        }

        /// <summary> 重设当前节纸张边距 </summary>
        public void ResetActiveSectionMargin()
        {
            ActiveSection.ResetMargin();
            DoViewResize();
        }

        /// <summary> ActiveItem重新适应其环境(供外部直接修改Item属性后重新和其前后Item连接组合) </summary>
        public void ActiveItemReAdaptEnvironment()
        {
            ActiveSection.ActiveItemReAdaptEnvironment();
        }

        /// <summary> 全部清空(清除各节页眉、页脚、页面的Item及DrawItem) </summary>
        public void Clear()
        {
            FStyle.Initialize();  // 先清样式，防止Data初始化为EmptyData时空Item样式赋值为CurStyleNo
            FSections.RemoveRange(1, FSections.Count - 1);

            FUndoList.SaveState();
            try
            {
                FUndoList.Enable = false;
                FSections[0].Clear();
                FUndoList.Clear();
            }
            finally
            {
                FUndoList.RestoreState();
            }

            FHScrollBar.Position = 0;
            FVScrollBar.Position = 0;
            FActiveSectionIndex = 0;
            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret();
            DoMapChanged();
            DoViewResize();
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

        /// <summary> 删除当前域 </summary>
        public bool DeleteActiveDomain()
        {
            return ActiveSection.DeleteActiveDomain();
        }

        /// <summary> 删除当前Data指定范围内的Item </summary>
        public void DeleteActiveDataItems(int aStartNo, int aEndNo = -1, bool aKeepPara = true)
        {
            if (aEndNo < aStartNo)
                ActiveSection.DeleteActiveDataItems(aStartNo, aStartNo, aKeepPara);
            else
                ActiveSection.DeleteActiveDataItems(aStartNo, aEndNo, aKeepPara);
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
            {
                FSections[i].FormatData();
                FSections[i].BuildSectionPages(0);
            }

            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret();
            DoMapChanged();
        }

        /// <summary> 插入流 </summary>
        public bool InsertStream(Stream aStream)
        {
            bool vResult = false;
            this.BeginUpdate();
            try
            {
                using (HCStyle vStyle = new HCStyle())
                {
                    LoadSectionProcHandler vEvent = delegate (ushort aFileVersion)
                    {
                        byte[] vBuffer = new byte[1];
                        aStream.Read(vBuffer, 0, vBuffer.Length);
                        byte vByte = vBuffer[0];  // 节数量

                        using (MemoryStream vDataStream = new MemoryStream())
                        {
                            using (HCSection vSection = new HCSection(vStyle))
                            {
                                vSection.OnCreateItemByStyle = DoSectionCreateStyleItem;  // 样式要保持一致

                                // 不循环，只插入第一节的正文
                                vSection.LoadFromStream(aStream, vStyle, aFileVersion);
                                vDataStream.SetLength(0);
                                vSection.Page.SaveToStream(vDataStream);
                                vDataStream.Position = 0;

                                bool vShowUnderLine = false;
                                vBuffer = BitConverter.GetBytes(vShowUnderLine);
                                vDataStream.Read(vBuffer, 0, vBuffer.Length);
                                vShowUnderLine = BitConverter.ToBoolean(vBuffer, 0);

                                vResult = ActiveSection.InsertStream(vDataStream, vStyle, HC.HC_FileVersionInt);  // 只插入第一节的数据
                            }
                        }
                    };

                    DoLoadFromStream(aStream, vStyle, vEvent);
                }
            }
            finally
            {
                this.EndUpdate();
            }

            return vResult;
        }

        /// <summary> 插入文本(可包括\r\n) </summary>
        public bool InsertText(string aText)
        {
            this.BeginUpdate();
            try
            {
                return DoInsertText(aText);
            }
            finally
            {
                this.EndUpdate();
            }
        }

        /// <summary> 插入指定行列的表格 </summary>
        public bool InsertTable(int aRowCount, int aColCount)
        {
            this.BeginUpdate();
            try
            {
                return ActiveSection.InsertTable(aRowCount, aColCount);
            }
            finally
            {
                this.EndUpdate();
            }
        }

        /// <summary> 插入图片 </summary>
        public bool InsertImage(string aFile)
        {
            Bitmap vImage = new Bitmap(aFile);
            return InsertImage(vImage);
        }

        public bool InsertImage(Bitmap aImage)
        {
            this.BeginUpdate();
            try
            {
                return ActiveSection.InsertImage(aImage);
            }
            finally
            {
                this.EndUpdate();
            }
        }

        /// <summary> 插入GIF图片 </summary>
        public bool InsertGifImage(string aFile)
        {
            this.BeginUpdate();
            try
            {
                return ActiveSection.InsertGifImage(aFile);
            }
            finally
            {
                this.EndUpdate();
            }
        }

        /// <summary> 插入水平线 </summary>
        public bool InsertLine(int aLineHeight)
        {
            return ActiveSection.InsertLine(aLineHeight);
        }

        /// <summary> 插入一个Item </summary>
        public bool InsertItem(HCCustomItem aItem)
        {
            return ActiveSection.InsertItem(aItem);
        }

        /// <summary> 在指定的位置插入一个Item </summary>
        public bool InsertItem(int aIndex, HCCustomItem aItem)
        {
            return ActiveSection.InsertItem(aIndex, aItem);
        }

        /// <summary> 插入浮动Item </summary>
        public bool InsertFloatItem(HCCustomFloatItem aFloatItem)
        {
            return ActiveSection.InsertFloatItem(aFloatItem);
        }

        /// <summary> 插入批注 </summary>
        public bool InsertAnnotate(string aTitle, string aText)
        {
            return ActiveSection.InsertAnnotate(aTitle, aText);
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

        /// <summary> 插入域 </summary>
        public bool InsertDomain(HCDomainItem aMouldDomain)
        {
            return ActiveSection.InsertDomain(aMouldDomain);
        }

        public bool SetActiveImage(Stream aImageStream)
        {
            return ActiveSection.SetActiveImage(aImageStream);
        }

        public bool ActiveTableResetRowCol(int rowCount, int colCount)
        {
            return ActiveSection.ActiveTableResetRowCol(rowCount, colCount);
        }

        /// <summary> 当前表格选中行下面插入行 </summary>
        public bool ActiveTableInsertRowAfter(byte aRowCount)
        {
            return ActiveSection.ActiveTableInsertRowAfter(aRowCount);
        }

        /// <summary> 当前表格选中行上面插入行 </summary>
        public bool ActiveTableInsertRowBefor(byte aRowCount)
        {
            return ActiveSection.ActiveTableInsertRowBefor(aRowCount);
        }

        /// <summary> 当前表格删除选中的行 </summary>
        public bool ActiveTableDeleteCurRow()
        {
            return ActiveSection.ActiveTableDeleteCurRow();
        }

        /// <summary> 当前表格选中的单元格拆分行 </summary>
        public bool ActiveTableSplitCurRow()
        {
            return ActiveSection.ActiveTableSplitCurRow();
        }

        /// <summary> 当前表格选中的单元格拆分列 </summary>
        public bool ActiveTableSplitCurCol()
        {
            return ActiveSection.ActiveTableSplitCurCol();
        }

        /// <summary> 当前表格选中列左侧插入列 </summary>
        public bool ActiveTableInsertColBefor(byte aColCount)
        {
            return ActiveSection.ActiveTableInsertColBefor(aColCount);
        }

        /// <summary> 当前表格选中列右侧插入列 </summary>
        public bool ActiveTableInsertColAfter(byte aColCount)
        {
            return ActiveSection.ActiveTableInsertColAfter(aColCount);
        }

        /// <summary> 当前表格删除选中的列 </summary>
        public bool ActiveTableDeleteCurCol()
        {
            return ActiveSection.ActiveTableDeleteCurCol();
        }

        /// <summary> 修改当前光标所在段水平对齐方式 </summary>
        public void ApplyParaAlignHorz(ParaAlignHorz aAlign)
        {
            ActiveSection.ApplyParaAlignHorz(aAlign);
        }

        /// <summary> 修改当前光标所在段垂直对齐方式 </summary>
        public void ApplyParaAlignVert(ParaAlignVert aAlign)
        {
            ActiveSection.ApplyParaAlignVert(aAlign);
        }

        /// <summary> 修改当前光标所在段背景色 </summary>
        public void ApplyParaBackColor(Color aColor)
        {
            ActiveSection.ApplyParaBackColor(aColor);
        }

        /// <summary> 修改当前光标所在段换行截断方式 </summary>
        public void ApplyParaBreakRough(bool aRough)
        {
            ActiveSection.ApplyParaBreakRough(aRough);
        }

        /// <summary> 修改当前光标所在段行间距 </summary>
        public void ApplyParaLineSpace(ParaLineSpaceMode aSpaceMode, Single aSpace = 1)
        {
            ActiveSection.ApplyParaLineSpace(aSpaceMode, aSpace);
        }

        /// <summary> 修改当前光标所在段左缩进 </summary>
        public void ApplyParaLeftIndent(bool add = true)
        {
            if (add)
                ActiveSection.ApplyParaLeftIndent(FStyle.ParaStyles[CurParaNo].LeftIndent + HCUnitConversion.PixXToMillimeter(HC.TabCharWidth));
            else
                ActiveSection.ApplyParaLeftIndent(FStyle.ParaStyles[CurParaNo].LeftIndent - HCUnitConversion.PixXToMillimeter(HC.TabCharWidth));
        }

        /// <summary> 修改当前光标所在段左缩进 </summary>
        public void ApplyParaLeftIndent(Single aIndent)
        {
            ActiveSection.ApplyParaLeftIndent(aIndent);
        }

        /// <summary> 修改当前光标所在段右缩进 </summary>
        public void ApplyParaRightIndent(Single aIndent)
        {
            ActiveSection.ApplyParaRightIndent(aIndent);
        }

        /// <summary> 修改当前光标所在段首行缩进 </summary>
        public void ApplyParaFirstIndent(Single aIndent)
        {
            ActiveSection.ApplyParaFirstIndent(aIndent);
        }

        /// <summary> 修改当前选中文本的样式 </summary>
        public void ApplyTextStyle(HCFontStyle aFontStyle)
        {
            ActiveSection.ApplyTextStyle(aFontStyle);
        }

        /// <summary> 修改当前选中文本的字体 </summary>
        public void ApplyTextFontName(string aFontName)
        {
            ActiveSection.ApplyTextFontName(aFontName);
        }

        /// <summary> 修改当前选中文本的字号 </summary>
        public void ApplyTextFontSize(Single aFontSize)
        {
            ActiveSection.ApplyTextFontSize(aFontSize);
        }

        /// <summary> 修改当前选中文本的颜色 </summary>
        public void ApplyTextColor(Color aColor)
        {
            ActiveSection.ApplyTextColor(aColor);
        }

        /// <summary> 修改当前选中文本的背景颜色 </summary>
        public void ApplyTextBackColor(Color aColor)
        {
            ActiveSection.ApplyTextBackColor(aColor);
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
                FStyle.States.Include(HCState.hosCopying);
                try
                {
                    MemoryStream vStream = new MemoryStream();
                    try
                    {
                        HC._SaveFileFormatAndVersion(vStream);  // 保存文件格式和版本
                        DoCopyAsStream(vStream);  // 通知保存事件

                        HashSet<SectionArea> vSaveParts = new HashSet<SectionArea>() { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
                        FStyle.SaveToStream(vStream);
                        this.ActiveSectionTopLevelData().SaveSelectToStream(vStream);

                        //IDataObject vDataObj = new DataObject();
                        //vDataObj.SetData(HC.HC_EXT, vStream);
                        //vDataObj.SetData(DataFormats.Text, this.ActiveSectionTopLevelData().SaveSelectToText());  // 文本格式
                        //Clipboard.SetDataObject(vDataObj);

                        byte[] vBuffer = new byte[0];
                        IntPtr vMem = (IntPtr)Kernel.GlobalAlloc(Kernel.GMEM_MOVEABLE | Kernel.GMEM_DDESHARE, (int)vStream.Length);
                        try
                        {
                            if (vMem == IntPtr.Zero)
                                throw new Exception(HC.HCS_EXCEPTION_MEMORYLESS);
                            IntPtr vPtr = (IntPtr)Kernel.GlobalLock(vMem);
                            try
                            {
                                vStream.Position = 0;
                                vBuffer = vStream.ToArray();
                                System.Runtime.InteropServices.Marshal.Copy(vBuffer, 0, vPtr, vBuffer.Length);
                                //Kernel.CopyMemory(vPtr, vStream.ToArray(), (int)vStream.Length);
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

                        vBuffer = System.Text.Encoding.Unicode.GetBytes(this.ActiveSectionTopLevelData().SaveSelectToText());
                        IntPtr vMemText = (IntPtr)Kernel.GlobalAlloc(Kernel.GMEM_MOVEABLE | Kernel.GMEM_DDESHARE, vBuffer.Length + 2);
                        try
                        {
                            if (vMemText == IntPtr.Zero)
                                throw new Exception(HC.HCS_EXCEPTION_MEMORYLESS);

                            IntPtr vPtr = (IntPtr)Kernel.GlobalLock(vMemText);
                            try
                            {
                                System.Runtime.InteropServices.Marshal.Copy(vBuffer, 0, vPtr, vBuffer.Length);
                            }
                            finally
                            {
                                Kernel.GlobalUnlock(vMemText);
                            }
                        }
                        catch
                        {
                            Kernel.GlobalFree(vMemText);
                            return;
                        }

                        User.OpenClipboard(IntPtr.Zero);
                        try
                        {
                            User.EmptyClipboard();

                            if (DoCopyRequest(HC.HCExtFormat.Id))
                                User.SetClipboardData(HC.HCExtFormat.Id, vMem);  // HC格式

                            if (DoCopyRequest(User.CF_UNICODETEXT))
                                User.SetClipboardData(User.CF_UNICODETEXT, vMemText);  // 文本格式
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
                finally
                {
                    FStyle.States.Exclude(HCState.hosCopying);
                }
            }
        }

        /// <summary> 复制选中内容为文本 </summary>
        public void CopyAsText()
        {
            if (DoCopyRequest(User.CF_UNICODETEXT))
                Clipboard.SetText(this.ActiveSectionTopLevelData().SaveSelectToText());  // 文本格式
        }

        /// <summary> 粘贴剪贴板中的内容 </summary>
        public void Paste()
        {
            FStyle.States.Include(HCState.hosPasting);
            try
            {
                IDataObject vIData = Clipboard.GetDataObject();
                //string[] vFormats = vIData.GetFormats();

                if (vIData.GetDataPresent(HC.HC_EXT) && DoPasteRequest(HC.HCExtFormat.Id))
                {
                    MemoryStream vStream = (MemoryStream)vIData.GetData(HC.HC_EXT);
                    try
                    {
                        string vFileFormat = "";
                        ushort vFileVersion = 0;
                        byte vLang = 0;

                        vStream.Position = 0;
                        HC._LoadFileFormatAndVersion(vStream, ref vFileFormat, ref vFileVersion, ref vLang);  // 文件格式和版本
                        if (!DoPasteFormatStream(vStream))
                            return;

                        HCStyle vStyle = new HCStyle();
                        try
                        {
                            vStyle.LoadFromStream(vStream, vFileVersion);
                            this.BeginUpdate();
                            try
                            {
                                FStyle.States.Include(HCState.hosPasting);
                                try
                                {
                                    ActiveSection.InsertStream(vStream, vStyle, vFileVersion);
                                }
                                finally
                                {
                                    FStyle.States.Exclude(HCState.hosPasting);
                                }
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
                if (vIData.GetDataPresent(DataFormats.Rtf) && DoPasteRequest(User.CF_TEXT))
                {
                    string vs = vIData.GetData(DataFormats.Rtf).ToString();
                    HCRtfRW vRtfRW = new HCRtfRW();
                    vRtfRW.InsertString(this, vs);
                }
                else
                if (vIData.GetDataPresent(DataFormats.Text) && DoPasteRequest(User.CF_TEXT))
                    InsertText(Clipboard.GetText());
                else
                if (vIData.GetDataPresent(DataFormats.Text) && DoPasteRequest(User.CF_UNICODETEXT))
                    InsertText(Clipboard.GetText());
                else
                if (vIData.GetDataPresent(DataFormats.Bitmap) && DoPasteRequest(User.CF_BITMAP))
                {
                    Image vImage = (Image)vIData.GetData(typeof(Bitmap));

                    HCRichData vTopData = this.ActiveSectionTopLevelData() as HCRichData;
                    HCImageItem vImageItem = new HCImageItem(vTopData);

                    vImageItem.Image = new Bitmap(vImage);

                    vImageItem.Width = vImageItem.Image.Width;
                    vImageItem.Height = vImageItem.Image.Height;

                    vImageItem.RestrainSize(vTopData.Width, vImageItem.Height);

                    this.InsertItem(vImageItem);
                }
            }
            finally
            {
                FStyle.States.Exclude(HCState.hosPasting);
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
            UpdateView(GetViewRect());
        }

        #region UpdateView UpdateView子方法CalcDisplaySectionAndPage
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

            for (int i = 0; i <= FSections.Count - 1; i++)
            {
                for (int j = 0; j <= FSections[i].PageCount - 1; j++)
                {
                    if (FSections[i].ViewModel == HCViewModel.hvmFilm)
                        vPos = vPos + ZoomIn(FPagePadding + FSections[i].PaperHeightPix);
                    else
                        vPos = vPos + ZoomIn(FPagePadding + FSections[i].GetPageHeight());

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
                int vY = FVScrollBar.Position + FViewHeight;
                for (int i = FDisplayFirstSection; i <= FSections.Count - 1; i++)
                {
                    for (int j = vFirstPage; j <= FSections[i].PageCount - 1; j++)
                    {
                        if (vPos < vY)
                        {
                            if (FSections[i].ViewModel == HCViewModel.hvmFilm)
                                vPos = vPos + ZoomIn(FPagePadding + FSections[i].PaperHeightPix);
                            else
                                vPos = vPos + ZoomIn(FPagePadding + FSections[i].GetPageHeight());
                        }
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

                    vFirstPage = 0;  // 当前节没找到，下一节从0开始
                }

                if (FDisplayLastSection < 0)
                {
                    FDisplayLastSection = FSections.Count - 1;
                    FSections[FDisplayLastSection].DisplayLastPageIndex = FSections[FDisplayLastSection].PageCount - 1;
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
        public void UpdateView(RECT aRect)
        {
            if (FStyle.States.Contain(HCState.hosDestroying))
                return;

            if ((FUpdateCount == 0) && IsHandleCreated)
            {
                // 创建一个新的剪切区域，该区域是当前剪切区域和一个特定矩形的交集
                //GDI.IntersectClipRect(FDataBmpCanvas.Handle, aRect.Left, aRect.Top, aRect.Right, aRect.Bottom);
                IntPtr vRgn = GDI.CreateRectRgn(aRect.Left, aRect.Top, aRect.Right, aRect.Bottom);
                try
                {
                    GDI.SelectClipRgn(FDataBmpCanvas.Handle, vRgn);
                }
                finally
                {
                    GDI.DeleteObject(vRgn);
                }

                // 控件背景
                if (FViewModel == HCViewModel.hvmFilm)
                    FDataBmpCanvas.Brush.Color = this.BackColor;// $00E7BE9F;
                else
                    FDataBmpCanvas.Brush.Color = FStyle.BackgroundColor;

                FDataBmpCanvas.FillRect(new RECT(0, 0, FViewWidth, FViewHeight));
                // 因基于此计算当前页面数据起始结束，所以不能用ARect代替
                CalcDisplaySectionAndPage();  // 计算当前范围内可显示的起始节、页和结束节、页

                SectionPaintInfo vPaintInfo = new SectionPaintInfo();
                try
                {
                    vPaintInfo.ScaleX = FZoom;
                    vPaintInfo.ScaleY = FZoom;
                    vPaintInfo.Zoom = FZoom;
                    vPaintInfo.ViewModel = FViewModel;
                    vPaintInfo.WindowWidth = FViewWidth;
                    vPaintInfo.WindowHeight = FViewHeight;
                    vPaintInfo.DPI = HCUnitConversion.PixelsPerInchX;

                    ScaleInfo vScaleInfo = vPaintInfo.ScaleCanvas(FDataBmpCanvas);
                    try
                    {
                        DoPaintViewBefor(FDataBmpCanvas, vPaintInfo);

                        if (FAnnotatePre.DrawCount > 0)
                            FAnnotatePre.ClearDrawAnnotate();

                        int vOffsetY = 0;
                        for (int i = FDisplayFirstSection; i <= FDisplayLastSection; i++)
                        {
                            vPaintInfo.SectionIndex = i;

                            vOffsetY = ZoomOut(FVScrollBar.Position) - GetSectionTopFilm(i);  // 转为原始Y向偏移
                            FSections[i].PaintDisplayPage(GetSectionDrawLeft(i) - ZoomOut(FHScrollBar.Position),  // 原始X向偏移
                                vOffsetY, FDataBmpCanvas, vPaintInfo);
                        }

                        for (int i = 0; i <= vPaintInfo.TopItems.Count - 1; i++)  // 绘制顶层Item
                            vPaintInfo.TopItems[i].PaintTop(FDataBmpCanvas);

                        DoPaintViewAfter(FDataBmpCanvas, vPaintInfo);

                        if ((!vPaintInfo.Print) && (FCaret != null) && (FCaret.DisFocus))
                        {
                            FDataBmpCanvas.Pen.BeginUpdate();
                            try
                            {
                                FDataBmpCanvas.Pen.Color = Color.Gray;
                                FDataBmpCanvas.Pen.Style = HCPenStyle.psSolid;
                                FDataBmpCanvas.Pen.Width = FCaret.Width;
                            }
                            finally
                            {
                                FDataBmpCanvas.Pen.EndUpdate();
                            }

                            FDataBmpCanvas.MoveTo(FCaret.X, FCaret.Y);
                            FDataBmpCanvas.LineTo(FCaret.X, FCaret.Y + FCaret.Height);
                        }
                    }
                    finally
                    {
                        vPaintInfo.RestoreCanvasScale(FDataBmpCanvas, vScaleInfo);
                    }
                }
                finally
                {
                    vPaintInfo.Dispose();
                }

                GDI.BitBlt(FDC, 0, 0, FViewWidth, FViewHeight, FDataBmpCanvas.Handle, 0, 0, GDI.SRCCOPY);

                User.InvalidateRect(this.Handle, ref aRect, 0);  // 只更新变动区域，防止闪烁，解决BitBlt光标滞留问题
                User.UpdateWindow(this.Handle);  // 滚动条拖动时更新滚动条拖块
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
            if (FUpdateCount > 0)
                FUpdateCount--;

            DoMapChanged();
        }

        /// <summary> 开始一组撤销操作 </summary>
        public void UndoGroupBegin()
        {
            if (FUndoList.Enable)
            {
                HCRichData vData = ActiveSection.ActiveData;
                FUndoList.UndoGroupBegin(vData.SelectInfo.StartItemNo, vData.SelectInfo.StartItemOffset);
            }
        }

        /// <summary> 结束一组撤销操作 </summary>
        public void UndoGroupEnd()
        {
            if (FUndoList.Enable)
            {
                HCRichData vData = ActiveSection.ActiveData;
                FUndoList.UndoGroupEnd(vData.SelectInfo.StartItemNo, vData.SelectInfo.StartItemOffset);
            }
        }

        /// <summary> 返回当前节当前Item </summary>
        public HCCustomItem GetActiveItem()
        {
            return ActiveSection.GetActiveItem();
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
            
            return Result + ActiveSection.ActivePageIndex;
        }

        /// <summary> 返回当前预览起始页序号 </summary>
        public int GetPagePreviewFirst()
        {
            int Result = 0;
            for (int i = 0; i <= ActiveSectionIndex - 1; i++)
                Result = Result + FSections[i].PageCount;
    
            return Result + FSections[FActiveSectionIndex].DisplayFirstPageIndex;
        }

        /// <summary> 返回总页数 </summary>
        public int GetPageCount()
        {
            int Result = 0;
            for (int i = 0; i <= Sections.Count - 1; i++)
                Result = Result + FSections[i].PageCount;

            return Result;
        }

        public void PageUp()
        {
            DoPageUp(this, null);
        }

        public void PageDown()
        {
            DoPageDown(this, null);
        }

        /// <summary> 返回指定节页面绘制时Left位置 </summary>
        public int GetSectionDrawLeft(int aSectionIndex)
        {
            int Result = 0;
            if (FViewModel == HCViewModel.hvmFilm)
            {
                if (FAnnotatePre.Visible)
                    Result = Math.Max((FViewWidth - ZoomIn(FSections[aSectionIndex].PaperWidthPix + HC.AnnotationWidth)) / 2, ZoomIn(FPagePadding));
                else
                    Result = Math.Max((FViewWidth - ZoomIn(FSections[aSectionIndex].PaperWidthPix)) / 2, ZoomIn(FPagePadding));
            }

            Result = ZoomOut(Result);

            return Result;
        }

        /// <summary> 返回格式化位置相对当前页显示的窗体坐标 </summary>
        public POINT GetFormatPointToViewCoord(POINT aPoint)
        {
            POINT Result = new POINT(aPoint.X, aPoint.Y);
            HCSection vSection = this.ActiveSection;

            int vPageIndex = -1;
            if (vSection.ActiveData == vSection.Page)
                vPageIndex = vSection.GetPageIndexByFormat(Result.Y);
            else
                vPageIndex = vSection.ActivePageIndex;

            // 映射到节页面(白色区域)
            Result.X = ZoomIn(GetSectionDrawLeft(this.ActiveSectionIndex)
                + (vSection.GetPageMarginLeft(vPageIndex) + Result.X)) - FHScrollBar.Position;

            if (vSection.ActiveData == vSection.Header)  // 页眉
                Result.Y = ZoomIn(GetSectionTopFilm(this.ActiveSectionIndex)
                    + vSection.GetPageTopFilm(vPageIndex)  // 20
                    + vSection.GetHeaderPageDrawTop()
                    + Result.Y)
                    - FVScrollBar.Position;
            else
            if (vSection.ActiveData == vSection.Footer)  // 页脚
                Result.Y = ZoomIn(GetSectionTopFilm(this.ActiveSectionIndex)
                    + vSection.GetPageTopFilm(vPageIndex)  // 20
                    + vSection.PaperHeightPix - vSection.PaperMarginBottomPix
                    + Result.Y)
                    - FVScrollBar.Position;
            else
                Result.Y = ZoomIn(GetSectionTopFilm(this.ActiveSectionIndex)
                + vSection.GetPageTopFilm(vPageIndex)  // 20
                + vSection.GetHeaderAreaHeight() // 94
                + Result.Y
                - vSection.GetPageDataFmtTop(vPageIndex))  // 0
                - FVScrollBar.Position;

            return Result;
        }

        /// <summary> 返回光标处DrawItem相对当前页显示的窗体坐标 </summary>
        /// <returns>坐标</returns>
        public POINT GetTopLevelDrawItemViewCoord()
        {
            POINT vResult = this.ActiveSection.GetTopLevelDrawItemCoord();  // 有选中时，以选中结束位置的DrawItem格式化坐标
            vResult = this.GetFormatPointToViewCoord(vResult);
            return vResult;
        }

        public POINT GetTopLevelRectDrawItemViewCoord()
        {
            POINT vResult = this.ActiveSection.GetTopLevelRectDrawItemCoord();  // 有选中时，以选中结束位置的DrawItem格式化坐标
            vResult = this.GetFormatPointToViewCoord(vResult);
            return vResult;
        }

        /// <summary> 设置当前TextItem的文本内容 </summary>
        public void SetActiveItemText(string aText)
        {
            ActiveSection.SetActiveItemText(aText);
        }

        /// <summary> 格式化指定节的数据 </summary>
        public void FormatSection(int aSectionIndex)
        {
            FSections[aSectionIndex].FormatData();
            FSections[aSectionIndex].BuildSectionPages(0);
            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret();

            DoChange();
        }

        /// <summary> 获取当前节顶层Data </summary>
        public HCCustomData ActiveSectionTopLevelData()
        {
            return ActiveSection.ActiveData.GetTopLevelData();
        }

        /// <summary> 指定节在整个胶卷中的Top位置 </summary>
        public int GetSectionTopFilm(int aSectionIndex)
        {
            int Result = 0;
            for (int i = 0; i <= aSectionIndex - 1; i++)
                Result = Result + FSections[i].GetFilmHeight();

            return Result;
        }

        /// <summary> 文档保存为hcf格式 </summary>
        public void SaveToFile(string aFileName, bool aQuick = false)
        {
            FileStream vStream = new FileStream(aFileName, FileMode.Create, FileAccess.Write);
            try
            {
                HashSet<SectionArea> vParts = new HashSet<SectionArea> { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
                SaveToStream(vStream, aQuick, vParts);
            }
            finally
            {
                vStream.Close();
                vStream.Dispose();
            }
        }

        /// <summary> 读取hcf文件 </summary>
        public bool LoadFromFile(string aFileName)
        {
            bool vResult = false;            
            FileStream vStream = new FileStream(aFileName, FileMode.Open, FileAccess.Read);
            try
            {
                vResult = LoadFromStream(vStream);
                if (vResult)
                    FFileName = aFileName;
            }
            finally
            {
                vStream.Dispose();
            }

            return vResult;
        }

        /// <summary> 读取其他格式的文件 </summary>
        public void LoadFromDocumentFile(string aFileName, string aExt)
        {

        }

        /// <summary> 另存为其他格式的文件 </summary>
        public void SaveToDocumentFile(string aFileName, string aExt)
        {

        }

        /// <summary> 文档保存为PDF格式 </summary>
        public void SaveToPDF(string aFileName)
        {
            using (FileStream vStream = new FileStream(aFileName, FileMode.Create, FileAccess.Write))
            {
                SaveToPDFStream(vStream);
            }
        }

        [DllImport("HCExpPDF.dll", EntryPoint = "SetServiceCode")]
        public static extern object SetServiceCode(object obj);

        [DllImport("HCExpPDF.dll", EntryPoint = "SaveToPDFStream", CallingConvention = CallingConvention.StdCall)]
        public static extern void SaveToPDFStream_DLL(ref object inObj, out object outObj);
        public virtual void SaveToPDFStream(Stream stream)
        {
            using (MemoryStream vFileStream = new MemoryStream())
            {
                HashSet<SectionArea> vParts = new HashSet<SectionArea> { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
                this.SaveToStream(vFileStream, true, vParts);
                vFileStream.Position = 0;
                byte[] bytes = new byte[vFileStream.Length];
                vFileStream.Read(bytes, 0, bytes.Length);
                object vInObj = (object)bytes;

                object vOutObj = null;
                SaveToPDFStream_DLL(ref vInObj, out vOutObj);
                if (vOutObj != null)
                {
                    byte[] vOutBytes = vOutObj as byte[];
                    stream.Write(vOutBytes, 0, vOutBytes.Length);
                }
            }
        }

        /// <summary> 以字符串形式获取文档各节正文内容 </summary>
        public string SaveToText()
        {
            string vResult = "";
            for (int i = 0; i <= Sections.Count - 1; i++)
                vResult = vResult + HC.sLineBreak + FSections[i].SaveToText();

            return vResult;
        }

        /// <summary>
        /// 将文档每一页保存为图片
        /// </summary>
        /// <param name="aPath">图片路径</param>
        /// <param name="aPrefix">图片名称前缀</param>
        /// <param name="aImageType">图片格式如 BMP, JPG, PNG</param>
        public void SaveToImage(string aPath, string aPrefix, string aImageType, bool aOnePaper = true)
        {
            HCCanvas vBmpCanvas = new HCCanvas();
            SectionPaintInfo vPaintInfo = new SectionPaintInfo();
            try
            {
                vPaintInfo.ScaleX = 1;
                vPaintInfo.ScaleY = 1;
                vPaintInfo.Zoom = 1;
                vPaintInfo.Print = true;
                vPaintInfo.DPI = HCUnitConversion.PixelsPerInchX;
                vPaintInfo.ViewModel = HCViewModel.hvmFilm;

                int vWidth = 0, vHeight = 0;
                if (aOnePaper)
                {
                    for (int i = 0; i < FSections.Count; i++)
                    {
                        vHeight = vHeight + FSections[i].PaperHeightPix * FSections[i].PageCount;
                        if (vWidth < FSections[i].PaperWidthPix)
                            vWidth = FSections[i].PaperWidthPix;
                    }

                    vPaintInfo.WindowWidth = vWidth;
                    vPaintInfo.WindowHeight = vHeight;

                    using (Bitmap vBmp = new Bitmap(vWidth, vHeight))
                    {
                        vBmpCanvas.Graphics = Graphics.FromImage(vBmp);

                        int vSectionIndex = 0, vSectionPageIndex = 0, vTop = 0;
                        for (int i = 0; i < this.PageCount; i++)
                        {
                            vSectionIndex = GetSectionPageIndexByPageIndex(i, ref vSectionPageIndex);
                            //vWidth = FSections[vSectionIndex].PaperWidthPix;
                            vHeight = FSections[vSectionIndex].PaperHeightPix;

                            vBmpCanvas.Brush.Color = Color.White;
                            vBmpCanvas.FillRect(new RECT(0, vTop, vWidth, vTop + vHeight));

                            ScaleInfo vScaleInfo = vPaintInfo.ScaleCanvas(vBmpCanvas);
                            try
                            {
                                FSections[vSectionIndex].PaintPaper(vSectionPageIndex, 0, vTop, vBmpCanvas, vPaintInfo);
                                vTop = vTop + vHeight;
                            }
                            finally
                            {
                                vPaintInfo.RestoreCanvasScale(vBmpCanvas, vScaleInfo);
                            }                            
                        }

                        vBmpCanvas.Dispose();
                        if (aImageType == "BMP")
                            vBmp.Save(aPath + aPrefix + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                        else
                        if (aImageType == "JPG")
                            vBmp.Save(aPath + aPrefix + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                        else
                            vBmp.Save(aPath + aPrefix + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                else
                {
                    int vSectionIndex = 0, vSectionPageIndex = 0;
                    for (int i = 0; i < this.PageCount; i++)
                    {
                        vSectionIndex = GetSectionPageIndexByPageIndex(i, ref vSectionPageIndex);

                        using (Bitmap vBmp = new Bitmap(FSections[vSectionIndex].PaperWidthPix, FSections[vSectionIndex].PaperHeightPix))
                        {
                            vBmpCanvas.Graphics = Graphics.FromImage(vBmp);
                            vBmpCanvas.Brush.Color = Color.White;
                            vBmpCanvas.FillRect(new RECT(0, 0, vBmp.Width, vBmp.Height));

                            vPaintInfo.WindowWidth = vBmp.Width;
                            vPaintInfo.WindowHeight = vBmp.Height;
                            ScaleInfo vScaleInfo = vPaintInfo.ScaleCanvas(vBmpCanvas);
                            try
                            {
                                vBmpCanvas.Brush.Color = Color.White;
                                vBmpCanvas.FillRect(new RECT(0, 0, vBmp.Width, vBmp.Height));
                                FSections[vSectionIndex].PaintPaper(vSectionPageIndex, 0, 0, vBmpCanvas, vPaintInfo);
                            }
                            finally
                            {
                                vPaintInfo.RestoreCanvasScale(vBmpCanvas, vScaleInfo);
                            }

                            vBmpCanvas.Dispose();
                            if (aImageType == "BMP")
                                vBmp.Save(aPath + aPrefix + (i + 1).ToString() + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                            else
                            if (aImageType == "JPG")
                                vBmp.Save(aPath + aPrefix + (i + 1).ToString() + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                            else
                                vBmp.Save(aPath + aPrefix + (i + 1).ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            finally
            {
                vPaintInfo.Dispose();
            }
        }

        /// <summary> 读文本到第一节正文 </summary>
        public bool LoadFromText(string aText)
        {
            if (ReadOnly)
                return false;

            Clear();
            FStyle.Initialize();

            if (aText != "")
                return ActiveSection.InsertText(aText);
            else
                return false;
        }

        /// <summary> 文档各节正文字符串保存为文本格式文件 </summary>
        public void SaveToTextFile(string aFileName, System.Text.Encoding aEncoding)
        {
            using (FileStream vStream = new FileStream(aFileName, FileMode.Create))
            {
                SaveToTextStream(vStream, aEncoding);
            }
        }

        /// <summary> 读取文本文件内容到第一节正文 </summary>
        public bool LoadFromTextFile(string aFileName, System.Text.Encoding aEncoding)
        {
            bool vResult = false;
            using (FileStream vStream = new FileStream(aFileName, FileMode.Open))
            {
                vStream.Position = 0;
                vResult = LoadFromTextStream(vStream, aEncoding);
            }

            return vResult;
        }

        /// <summary> 文档各节正文字符串保存为文本格式流 </summary>
        public void SaveToTextStream(Stream aStream, System.Text.Encoding aEncoding)
        {
            string vText = SaveToText();
            byte[] vBuffer = aEncoding.GetBytes(vText);
            byte[] vPreamble = aEncoding.GetPreamble();
            if (vPreamble.Length > 0)
                aStream.Write(vPreamble, 0, vPreamble.Length);

            aStream.Write(vBuffer, 0, vBuffer.Length);
        }

        /// <summary> 读取文本文件流 </summary>
        public bool LoadFromTextStream(Stream aStream, System.Text.Encoding aEncoding)
        {
            long vSize = aStream.Length - aStream.Position;
            byte[] vBuffer = new byte[vSize];
            aStream.Read(vBuffer, 0, (int)vSize);
            string vS = aEncoding.GetString(vBuffer);
            return LoadFromText(vS);
        }

        /// <summary> 文档保存到流 </summary>
        public virtual void SaveToStream(Stream aStream, bool aQuick = false, HashSet<SectionArea> aAreas = null)
        {
            HC._SaveFileFormatAndVersion(aStream);  // 文件格式和版本
            DoSaveStreamBefor(aStream);

            HashSet<SectionArea> vArea = aAreas;
            if (vArea == null)
            {
                vArea = new HashSet<SectionArea>();
                vArea.Add(SectionArea.saHeader);
                vArea.Add(SectionArea.saFooter);
                vArea.Add(SectionArea.saPage);
            }

            if (!aQuick)
            {
                FUndoList.Clear();
                DeleteUnUsedStyle(FStyle, FSections, vArea);  // 删除不使用的样式(可否改为把有用的存了，加载时Item的StyleNo取有用)
            }

            FStyle.SaveToStream(aStream);
            // 节数量
            byte vByte = (byte)FSections.Count;
            aStream.WriteByte(vByte);
            // 各节数据
            for (int i = 0; i <= Sections.Count - 1; i++)
                FSections[i].SaveToStream(aStream, vArea);
            
            DoSaveStreamAfter(aStream);
        }

        /// <summary> 读取文件流 </summary>
        public virtual bool LoadFromStream(Stream aStream)
        {
            if (ReadOnly)
                return false;

            bool vResult = false;
            this.BeginUpdate();
            try
            {
                // 清除撤销恢复数据
                FUndoList.Clear();
                FUndoList.SaveState();
                try
                {
                    FUndoList.Enable = false;
                    this.Clear();

                    FStyle.States.Include(HCState.hosLoading);
                    try
                    {
                        aStream.Position = 0;
                        LoadSectionProcHandler vEvent = delegate (ushort aFileVersion)
                        {
                            byte vByte = 0;
                            vByte = (byte)aStream.ReadByte();  // 节数量
                            // 各节数据
                            FSections[0].LoadFromStream(aStream, FStyle, aFileVersion);
                            for (int i = 1; i <= vByte - 1; i++)
                            {
                                HCSection vSection = NewDefaultSection();
                                vSection.LoadFromStream(aStream, FStyle, aFileVersion);
                                FSections.Add(vSection);
                            }
                        };

                        DoLoadFromStream(aStream, FStyle, vEvent);
                    }
                    finally
                    {
                        FStyle.States.Exclude(HCState.hosLoading);
                    }

                    vResult = true;
                    DoViewResize();
                }
                finally
                {
                    FUndoList.RestoreState();
                }
            }
            finally
            {
                this.EndUpdate();
            }

            return vResult;
        }

        /// <summary> 文档保存为xml格式 </summary>
        public void SaveToXml(string aFileName, System.Text.Encoding aEncoding)
        {
            FileStream vStream = new FileStream(aFileName, FileMode.Create, FileAccess.Write);
            try
            {
                SaveToXmlStream(vStream, aEncoding);
            }
            finally
            {
                vStream.Close();
                vStream.Dispose();
            }
        }

        public void SaveToXmlStream(Stream stream, System.Text.Encoding encoding)
        {
            FUndoList.Clear();
            HashSet<SectionArea> vParts = new HashSet<SectionArea> { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
            DeleteUnUsedStyle(FStyle, FSections, vParts);

            XmlDocument vXml = new XmlDocument();
            vXml.PreserveWhitespace = true;
            //vXml. = "1.0";
            //vXml.DocumentElement
            XmlElement vElement = vXml.CreateElement("HCView");
            vElement.SetAttribute("EXT", HC.HC_EXT);
            vElement.SetAttribute("ver", HC.HC_FileVersion);
            vElement.SetAttribute("lang", HC.HC_PROGRAMLANGUAGE.ToString());
            vXml.AppendChild(vElement);

            XmlElement vNode = vXml.CreateElement("style");
            FStyle.ToXml(vNode);  // 样式表
            vElement.AppendChild(vNode);

            vNode = vXml.CreateElement("sections");
            vNode.SetAttribute("count", FSections.Count.ToString());  // 节数量
            vElement.AppendChild(vNode);

            for (int i = 0; i <= FSections.Count - 1; i++)  // 各节数据
            {
                XmlElement vSectionNode = vXml.CreateElement("sc");
                FSections[i].ToXml(vSectionNode);
                vNode.AppendChild(vSectionNode);
            }

            vXml.Save(stream);
        }

        /// <summary> 读取xml格式 </summary>
        public bool LoadFromXml(string aFileName)
        {
            bool vResult = false;
            FileStream vStream = new FileStream(aFileName, FileMode.Open, FileAccess.Read);
            try
            {
                vStream.Position = 0;
                vResult = LoadFromXmlStream(vStream);
                if (vResult)
                    FFileName = aFileName;
            }
            finally
            {
                vStream.Dispose();
            }

            return vResult;
        }

        public bool LoadFromXmlStream(Stream stream)
        {
            if (ReadOnly)
                return false;

            bool vResult = false;
            this.BeginUpdate();
            try
            {
                FUndoList.Clear();
                FUndoList.SaveState();
                try
                {
                    FUndoList.Enable = false;
                    this.Clear();

                    XmlDocument vXml = new XmlDocument();
                    vXml.PreserveWhitespace = true;
                    vXml.Load(stream);
                    if (vXml.DocumentElement.Name == "HCView")
                    {
                        if (vXml.DocumentElement.Attributes["EXT"].Value != HC.HC_EXT)
                            return false;

                        string vVersion = vXml.DocumentElement.Attributes["ver"].Value;
                        byte vLang = byte.Parse(vXml.DocumentElement.Attributes["lang"].Value);

                        FStyle.States.Include(HCState.hosLoading);
                        try
                        {
                            for (int i = 0; i <= vXml.DocumentElement.ChildNodes.Count - 1; i++)
                            {
                                XmlElement vNode = vXml.DocumentElement.ChildNodes[i] as XmlElement;
                                if (vNode.Name == "style")
                                    FStyle.ParseXml(vNode);
                                else
                                if (vNode.Name == "sections")
                                {
                                    FSections[0].ParseXml(vNode.ChildNodes[0] as XmlElement);
                                    for (int j = 1; j < vNode.ChildNodes.Count - 1; j++)
                                    {
                                        HCSection vSection = NewDefaultSection();
                                        vSection.ParseXml(vNode.ChildNodes[j] as XmlElement);
                                        FSections.Add(vSection);
                                    }
                                }
                            }

                            DoMapChanged();
                        }
                        finally
                        {
                            FStyle.States.Exclude(HCState.hosLoading);
                        }

                        vResult = true;
                        DoViewResize();
                    }
                }
                finally
                {
                    FUndoList.RestoreState();
                }
            }
            finally
            {
                this.EndUpdate();
            }

            return vResult;
        }

        /// <summary> 导出为html格式 </summary>
        /// <param name="aSeparateSrc">True：图片等保存到文件夹，False以base64方式存储到页面中</param>
        public void SaveToHtml(string aFileName, bool aSeparateSrc = false)
        {
            FUndoList.Clear();
            HashSet<SectionArea> vParts = new HashSet<SectionArea> { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
            DeleteUnUsedStyle(FStyle, FSections, vParts);

            FStyle.GetHtmlFileTempName(true);

            string vPath = "";
            if (aSeparateSrc)
                vPath = Path.GetDirectoryName(aFileName);

            StringBuilder vHtmlTexts = new StringBuilder();
            vHtmlTexts.Append("<!DOCTYPE HTML>");
            vHtmlTexts.Append("<html>");
            vHtmlTexts.Append("<head>");
            vHtmlTexts.Append("<title>");
            vHtmlTexts.Append("</title>");

            vHtmlTexts.Append(FStyle.ToCSS());

            vHtmlTexts.Append("</head>");

            vHtmlTexts.Append("<body>");
            for (int i = 0; i <= FSections.Count - 1; i++)
              vHtmlTexts.Append(FSections[i].ToHtml(vPath));

            vHtmlTexts.Append("</body>");
            vHtmlTexts.Append("</html>");

            System.IO.File.WriteAllText(aFileName, vHtmlTexts.ToString());
        }

        /// <summary> 获取指定页所在的节和相对此节的页序号 </summary>
        /// <param name="aPageIndex">页序号</param>
        /// <param name="aSectionPageIndex">返回相对所在节的序号</param>
        /// <returns>返回页序号所在的节序号</returns>
        public int GetSectionPageIndexByPageIndex(int aPageIndex, ref int aSectionPageIndex)
        {
            int Result = -1, vPageCount = 0;

            for (int i = 0; i <= FSections.Count - 1; i++)
            {
                if (vPageCount + FSections[i].PageCount > aPageIndex)
                {
                    Result = i;  // 找到节序号
                    aSectionPageIndex = aPageIndex - vPageCount;
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
        /// <param name="aPrinter">指定打印机</param>
        /// <param name="aCopies">打印份数</param>
        /// <returns>打印结果</returns>
        public PrintResult Print(string aPrinter, short aCopies = 1)
        {
            return Print(aPrinter, 0, PageCount - 1, aCopies);
        }

        /// <summary> 使用指定的打印机打印指定页序号范围内的页 </summary>
        /// <param name="aPrinter">指定打印机</param>
        /// <param name="aStartPageIndex">起始页序号</param>
        /// <param name="aEndPageIndex">结束页序号</param>
        /// <param name="aCopies">打印份数</param>
        /// <returns></returns>
        public PrintResult Print(string aPrinter, int aStartPageIndex, int aEndPageIndex, int aCopies)
        {
            int[] vPages = new int[aEndPageIndex - aStartPageIndex + 1];
            for (int i = aStartPageIndex; i <= aEndPageIndex; i++)
                vPages[i - aStartPageIndex] = i;
    
            return Print(aPrinter, aCopies, vPages);
        }

        /// <summary> 使用指定的打印机打印指定页 </summary>
        /// <param name="aPrinter">指定打印机</param>
        /// <param name="aCopies">打印份数</param>
        /// <param name="aPages">要打印的页序号数组</param>
        /// <returns>打印结果</returns>
        public PrintResult Print(string aPrinter, int aCopies, int[] aPages)
        {          
            PrintDocument vPrinter = new PrintDocument();
            if (aPrinter != "")
                vPrinter.PrinterSettings.PrinterName = aPrinter;
            
            if (!vPrinter.PrinterSettings.IsValid)
                return PrintResult.prError;
    
            vPrinter.DocumentName = FFileName;
            // 取打印机打印区域相关参数
            //int vPrintOffsetX = (int)vPrinter.PrinterSettings.DefaultPageSettings.HardMarginX;  // -GetDeviceCaps(Printer.Handle, PHYSICALOFFSETX);  // 73
            //int vPrintOffsetY = (int)vPrinter.PrinterSettings.DefaultPageSettings.HardMarginY;  // -GetDeviceCaps(Printer.Handle, PHYSICALOFFSETY);  // 37
            int vPrintOffsetX = -(int)Math.Round(HCUnitConversion.PixelsPerInchX * vPrinter.PrinterSettings.DefaultPageSettings.PrintableArea.X / 100);
            int vPrintOffsetY = -(int)Math.Round(HCUnitConversion.PixelsPerInchY * vPrinter.PrinterSettings.DefaultPageSettings.PrintableArea.Y / 100);

            vPrinter.PrinterSettings.Copies = (short)aCopies;

            SectionPaintInfo vPaintInfo = new SectionPaintInfo();
            try
            {
                vPaintInfo.Print = true;

                HCCanvas vPrintCanvas = new HCCanvas();
                int vFirstPageIndex = 0, vSectionIndex = 0, vPrintWidth = 0, vPrintHeight = 0, vPrintPageIndex = 0, i = 0;

                vPrintPageIndex = aPages[i];

                vPrinter.QueryPageSettings += (sender, e) =>
                {
                    vSectionIndex = GetSectionPageIndexByPageIndex(vPrintPageIndex, ref vFirstPageIndex);
                    if (vPaintInfo.SectionIndex != vSectionIndex)
                        SetPrintBySectionInfo(e.PageSettings, vSectionIndex);
                };

                vPrinter.PrintPage += (sender, e) =>
                {
                    //int vW = (int)Math.Round(FSections[vSectionIndex].PaperWidth / 25.4 * 100);
                    //int vH = (int)Math.Round(FSections[vSectionIndex].PaperHeight / 25.4 * 100);
                    //int vmarg = (int)Math.Round(FSections[vSectionIndex].PaperMarginLeft / 25.4 * 100);
                    //e.Graphics.DrawRectangle(Pens.Black, new Rectangle(vmarg, vmarg, vW - vmarg - vmarg, vH - vmarg - vmarg));
                    
                    if (vPrintCanvas.Graphics == null)
                        vPrintCanvas.Graphics = e.Graphics;                
                    
                    if (vPaintInfo.SectionIndex != vSectionIndex)
                    {
                        vPaintInfo.DPI = GDI.GetDeviceCaps(vPrintCanvas.Handle, GDI.LOGPIXELSX);
                        vPaintInfo.SectionIndex = vSectionIndex;

                        //HCPrinter.NewPage(False);
                        vPrintWidth = GDI.GetDeviceCaps(vPrintCanvas.Handle, GDI.PHYSICALWIDTH);  // 4961
                        vPrintHeight = GDI.GetDeviceCaps(vPrintCanvas.Handle, GDI.PHYSICALHEIGHT);  // 7016
                        
                        if (FSections[vSectionIndex].Page.DataAnnotates.Count > 0)
                        {
                            vPaintInfo.ScaleX = (float)vPrintWidth / (FSections[vSectionIndex].PaperWidthPix + HC.AnnotationWidth);
                            //vPaintInfo.ScaleY = (float)vPrintHeight / (FSections[vSectionIndex].PaperHeightPix + HC.AnnotationWidth * vPrintHeight / vPrintWidth);
                            vPaintInfo.ScaleY = vPaintInfo.ScaleX;
                            vPaintInfo.Zoom = FSections[vSectionIndex].PaperWidthPix / (FSections[vSectionIndex].PaperWidthPix + HC.AnnotationWidth);
                        }
                        else
                        {
                            vPaintInfo.ScaleX = (float)vPrintWidth / FSections[vSectionIndex].PaperWidthPix;  // GetDeviceCaps(Printer.Handle, LOGPIXELSX) / GetDeviceCaps(FStyle.DefCanvas.Handle, LOGPIXELSX);
                            //vPaintInfo.ScaleY = (float)vPrintHeight / FSections[vSectionIndex].PaperHeightPix;  // GetDeviceCaps(Printer.Handle, LOGPIXELSY) / GetDeviceCaps(FStyle.DefCanvas.Handle, LOGPIXELSY);
                            vPaintInfo.ScaleY = vPaintInfo.ScaleX;
                            vPaintInfo.Zoom = 1;
                        }

                        vPaintInfo.WindowWidth = vPrintWidth;  // FSections[vStartSection].PageWidthPix;
                        vPaintInfo.WindowHeight = vPrintHeight;  // FSections[vStartSection].PageHeightPix;

                        //vPrintOffsetX = -(int)Math.Round(vPrintOffsetX / vPaintInfo.ScaleX);
                        //vPrintOffsetY = -(int)Math.Round(vPrintOffsetY / vPaintInfo.ScaleY);
                        vPrintOffsetX = -(int)Math.Round(HCUnitConversion.PixelsPerInchX * e.PageSettings.PrintableArea.X / 100);
                        vPrintOffsetY = -(int)Math.Round(HCUnitConversion.PixelsPerInchY * e.PageSettings.PrintableArea.Y / 100);
                    }

                    ScaleInfo vScaleInfo = vPaintInfo.ScaleCanvas(vPrintCanvas);
                    try
                    {
                        vPaintInfo.PageIndex = aPages[i];
                        FSections[vSectionIndex].PaintPaper(vFirstPageIndex,
                            vPrintOffsetX, vPrintOffsetY, vPrintCanvas, vPaintInfo);
                    }
                    finally
                    {
                        vPaintInfo.RestoreCanvasScale(vPrintCanvas, vScaleInfo);
                    }

                    //e.Cancel = true;

                    if (i < aPages.Length - 1)
                    {
                        i++;
                        vPrintPageIndex = aPages[i];
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

        public PrintResult PrintOdd(string aPrinter)
        {
            List<int> vPages = new List<int>();
            for (int i = 0; i < PageCount; i++)
            {
                if (!HC.IsOdd(i))  // 偶数序号是奇数页
                    vPages.Add(i);
            }

            return Print(aPrinter, 1, vPages.ToArray());
        }

        public PrintResult PrintEven(string aPrinter)
        {
            List<int> vPages = new List<int>();
            for (int i = 0; i < PageCount; i++)
            {
                if (HC.IsOdd(i))  // 奇数序号是偶数页
                    vPages.Add(i);
            }

            return Print(aPrinter, 1, vPages.ToArray());
        }

        /// <summary> 从当前行打印当前页(仅限正文) </summary>
        /// <param name="aPrinter">指定打印机</param>
        /// <param name="aPrintHeader"> 是否打印页眉 </param>
        /// <param name="aPrintFooter"> 是否打印页脚 </param>
        public PrintResult PrintCurPageByActiveLine(string aPrinter, bool aPrintHeader, bool aPrintFooter)
        {
            PrintDocument vPrinter = new PrintDocument();
            PrintResult Result = PrintResult.prError;

            if (aPrinter != "")
                vPrinter.PrinterSettings.PrinterName = aPrinter;

            if (!vPrinter.PrinterSettings.IsValid)
                return Result;

            vPrinter.DocumentName = FFileName;

            // 取打印机打印区域相关参数
            //int vPrintOffsetX = (int)vPrinter.PrinterSettings.DefaultPageSettings.HardMarginX;  // -GetDeviceCaps(Printer.Handle, PHYSICALOFFSETX);  // 73
            //int vPrintOffsetY = (int)vPrinter.PrinterSettings.DefaultPageSettings.HardMarginY;  // -GetDeviceCaps(Printer.Handle, PHYSICALOFFSETY);  // 37
            int vPrintOffsetX = -(int)Math.Round(HCUnitConversion.PixelsPerInchX * vPrinter.PrinterSettings.DefaultPageSettings.PrintableArea.X / 100);
            int vPrintOffsetY = -(int)Math.Round(HCUnitConversion.PixelsPerInchY * vPrinter.PrinterSettings.DefaultPageSettings.PrintableArea.Y / 100);

            SectionPaintInfo vPaintInfo = new SectionPaintInfo();
            try
            {
                vPaintInfo.Print = true;

                HCCanvas vPrintCanvas = new HCCanvas();

                vPrinter.QueryPageSettings += (sender, e) =>
                {
                    SetPrintBySectionInfo(e.PageSettings, FActiveSectionIndex);
                };

                vPrinter.PrintPage += (sender, e) =>
                {
                    vPrintCanvas.Graphics = e.Graphics;
                    vPaintInfo.DPI = GDI.GetDeviceCaps(vPrintCanvas.Handle, GDI.LOGPIXELSX);
                    vPaintInfo.SectionIndex = this.ActiveSectionIndex;
                    vPaintInfo.PageIndex = this.ActiveSection.ActivePageIndex;

                    int vPrintWidth = GDI.GetDeviceCaps(vPrintCanvas.Handle, GDI.PHYSICALWIDTH);
                    int vPrintHeight = GDI.GetDeviceCaps(vPrintCanvas.Handle, GDI.PHYSICALHEIGHT);

                    if (this.ActiveSection.Page.DataAnnotates.Count > 0)
                    {
                        vPaintInfo.ScaleX = (float)vPrintWidth / (this.ActiveSection.PaperWidthPix + HC.AnnotationWidth);
                        //vPaintInfo.ScaleY = (float)vPrintHeight / (this.ActiveSection.PaperHeightPix + HC.AnnotationWidth * vPrintHeight / vPrintWidth);
                        vPaintInfo.ScaleY = vPaintInfo.ScaleX;
                        vPaintInfo.Zoom = this.ActiveSection.PaperWidthPix / (this.ActiveSection.PaperWidthPix + HC.AnnotationWidth);
                    }
                    else
                    {
                        vPaintInfo.ScaleX = (float)vPrintWidth / this.ActiveSection.PaperWidthPix;  // GetDeviceCaps(Printer.Handle, LOGPIXELSX) / GetDeviceCaps(FStyle.DefCanvas.Handle, LOGPIXELSX);
                        //vPaintInfo.ScaleY = (float)vPrintHeight / this.ActiveSection.PaperHeightPix;  // GetDeviceCaps(Printer.Handle, LOGPIXELSY) / GetDeviceCaps(FStyle.DefCanvas.Handle, LOGPIXELSY);
                        vPaintInfo.ScaleY = vPaintInfo.ScaleX;
                        vPaintInfo.Zoom = 1;
                    }

                    vPaintInfo.WindowWidth = vPrintWidth;  // FSections[vStartSection].PageWidthPix;
                    vPaintInfo.WindowHeight = vPrintHeight;  // FSections[vStartSection].PageHeightPix;

                    //vPrintOffsetX = -(int)Math.Round(vPrintOffsetX / vPaintInfo.ScaleX);
                    //vPrintOffsetY = -(int)Math.Round(vPrintOffsetY / vPaintInfo.ScaleY);
                    vPrintOffsetX = -(int)Math.Round(HCUnitConversion.PixelsPerInchX * e.PageSettings.PrintableArea.X / 100);
                    vPrintOffsetY = -(int)Math.Round(HCUnitConversion.PixelsPerInchY * e.PageSettings.PrintableArea.Y / 100);

                    ScaleInfo vScaleInfo = vPaintInfo.ScaleCanvas(vPrintCanvas);
                    try
                    {                       
                        this.ActiveSection.PaintPaper(this.ActiveSection.ActivePageIndex, 
                            vPrintOffsetX, vPrintOffsetY, vPrintCanvas, vPaintInfo);

                        POINT vPt;
                        if (this.ActiveSection.ActiveData == this.ActiveSection.Page)
                        {
                            vPt = this.ActiveSection.GetTopLevelDrawItemCoord();
                            vPt.Y = vPt.Y - ActiveSection.GetPageDataFmtTop(this.ActiveSection.ActivePageIndex);
                        }
                        else
                        {
                            Result = PrintResult.prNoSupport;
                            return;
                        }

                        // "抹"掉不需要显示的地方
                        vPrintCanvas.Brush.Color = Color.White;

                        RECT vRect = new RECT();
                        if (aPrintHeader)
                            vRect = HC.Bounds(vPrintOffsetX,
                                vPrintOffsetY + this.ActiveSection.GetHeaderAreaHeight(),  // 页眉下边
                                this.ActiveSection.PaperWidthPix, vPt.Y);
                        else  // 不打印页眉
                            vRect = HC.Bounds(vPrintOffsetX,// + vMarginLeft,防止页眉浮动Item在页边距中
                                vPrintOffsetY, this.ActiveSection.PaperWidthPix,// - vMarginLeft - vMarginRight,
                                this.ActiveSection.GetHeaderAreaHeight() + vPt.Y);

                        vPrintCanvas.FillRect(vRect);
                        if (!aPrintFooter)
                        {
                            vRect = HC.Bounds(vPrintOffsetX,// + vMarginLeft,防止页脚浮动Item在页边距中
                                vPrintOffsetY + this.ActiveSection.PaperHeightPix - this.ActiveSection.PaperMarginBottomPix,
                                this.ActiveSection.PaperWidthPix,// - vMarginLeft - vMarginRight, 
                                this.ActiveSection.PaperMarginBottomPix);
                            
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
        /// <param name="aPrinter">指定打印机</param>
        /// <param name="aPrintHeader"> 是否打印页眉 </param>
        /// <param name="aPrintFooter"> 是否打印页脚 </param>
        public PrintResult PrintCurPageByItemRange(string aPrinter, bool aPrintHeader, bool aPrintFooter, 
            int aStartItemNo, int aStartOffset, int aEndItemNo, int aEndOffset)
        {
            PrintDocument vPrinter = new PrintDocument();
            PrintResult Result = PrintResult.prError;

            if (aPrinter != "")
                vPrinter.PrinterSettings.PrinterName = aPrinter;

            if (!vPrinter.PrinterSettings.IsValid)
                return Result;

            vPrinter.DocumentName = FFileName;

            // 取打印机打印区域相关参数
            //int vPrintOffsetX = (int)vPrinter.PrinterSettings.DefaultPageSettings.HardMarginX;  // -GetDeviceCaps(Printer.Handle, PHYSICALOFFSETX);  // 73
            //int vPrintOffsetY = (int)vPrinter.PrinterSettings.DefaultPageSettings.HardMarginY;  // -GetDeviceCaps(Printer.Handle, PHYSICALOFFSETY);  // 37
            int vPrintOffsetX = -(int)Math.Round(HCUnitConversion.PixelsPerInchX * vPrinter.PrinterSettings.DefaultPageSettings.PrintableArea.X / 100);
            int vPrintOffsetY = -(int)Math.Round(HCUnitConversion.PixelsPerInchY * vPrinter.PrinterSettings.DefaultPageSettings.PrintableArea.Y / 100);

            SectionPaintInfo vPaintInfo = new SectionPaintInfo();
            try
            {
                vPaintInfo.Print = true;

                HCCanvas vPrintCanvas = new HCCanvas();

                vPrinter.QueryPageSettings += (sender, e) =>
                {
                    SetPrintBySectionInfo(e.PageSettings, FActiveSectionIndex);
                };

                vPrinter.PrintPage += (sender, e) =>
                {
                    vPrintCanvas.Graphics = e.Graphics;
                    vPaintInfo.DPI = GDI.GetDeviceCaps(vPrintCanvas.Handle, GDI.LOGPIXELSX);
                    vPaintInfo.SectionIndex = this.ActiveSectionIndex;
                    vPaintInfo.PageIndex = this.ActiveSection.ActivePageIndex;

                    int vPrintWidth = GDI.GetDeviceCaps(vPrintCanvas.Handle, GDI.PHYSICALWIDTH);
                    int vPrintHeight = GDI.GetDeviceCaps(vPrintCanvas.Handle, GDI.PHYSICALHEIGHT);

                    if (this.ActiveSection.Page.DataAnnotates.Count > 0)
                    {
                        vPaintInfo.ScaleX = (float)vPrintWidth / (this.ActiveSection.PaperWidthPix + HC.AnnotationWidth);
                        //vPaintInfo.ScaleY = (float)vPrintHeight / (this.ActiveSection.PaperHeightPix + HC.AnnotationWidth * vPrintHeight / vPrintWidth);
                        vPaintInfo.ScaleY = vPaintInfo.ScaleX;
                        vPaintInfo.Zoom = this.ActiveSection.PaperWidthPix / (this.ActiveSection.PaperWidthPix + HC.AnnotationWidth);
                    }
                    else
                    {
                        vPaintInfo.ScaleX = (float)vPrintWidth / this.ActiveSection.PaperWidthPix;  // GetDeviceCaps(Printer.Handle, LOGPIXELSX) / GetDeviceCaps(FStyle.DefCanvas.Handle, LOGPIXELSX);
                        //vPaintInfo.ScaleY = (float)vPrintHeight / this.ActiveSection.PaperHeightPix;  // GetDeviceCaps(Printer.Handle, LOGPIXELSY) / GetDeviceCaps(FStyle.DefCanvas.Handle, LOGPIXELSY);
                        vPaintInfo.ScaleY = vPaintInfo.ScaleX;
                        vPaintInfo.Zoom = 1;
                    }

                    vPaintInfo.WindowWidth = vPrintWidth;  // FSections[vStartSection].PageWidthPix;
                    vPaintInfo.WindowHeight = vPrintHeight;  // FSections[vStartSection].PageHeightPix;

                    //vPrintOffsetX = -(int)Math.Round(vPrintOffsetX / vPaintInfo.ScaleX);
                    //vPrintOffsetY = -(int)Math.Round(vPrintOffsetY / vPaintInfo.ScaleY);
                    vPrintOffsetX = -(int)Math.Round(HCUnitConversion.PixelsPerInchX * e.PageSettings.PrintableArea.X / 100);
                    vPrintOffsetY = -(int)Math.Round(HCUnitConversion.PixelsPerInchY * e.PageSettings.PrintableArea.Y / 100);

                    ScaleInfo vScaleInfo = vPaintInfo.ScaleCanvas(vPrintCanvas);
                    try
                    {                       
                        this.ActiveSection.PaintPaper(this.ActiveSection.ActivePageIndex, 
                            vPrintOffsetX, vPrintOffsetY, vPrintCanvas, vPaintInfo);

                        POINT vPt;
                        HCRichData vData = null;
                        int vDrawItemNo = -1;
                        if (this.ActiveSection.ActiveData == this.ActiveSection.Page)
                        {
                            vData = this.ActiveSection.ActiveData;
                            vDrawItemNo = vData.GetDrawItemNoByOffset(aStartItemNo, aStartOffset);
                            vPt = vData.DrawItems[vDrawItemNo].Rect.TopLeft();
                            vPt.Y = vPt.Y - ActiveSection.GetPageDataFmtTop(this.ActiveSection.ActivePageIndex);
                        }
                        else
                        {
                            Result = PrintResult.prNoSupport;
                            return;
                        }

                        int vMarginLeft = -1, vMarginRight = -1;
                        this.ActiveSection.GetPageMarginLeftAndRight(this.ActiveSection.ActivePageIndex, 
                            ref vMarginLeft, ref vMarginRight);
                        // "抹"掉不需要显示的地方
                        vPrintCanvas.Brush.Color = Color.White;

                        RECT vRect = new RECT();
                        if (aPrintHeader)
                            vRect = HC.Bounds(vPrintOffsetX,
                                vPrintOffsetY + this.ActiveSection.GetHeaderAreaHeight(),  // 页眉下边
                                this.ActiveSection.PaperWidthPix, vPt.Y);
                        else  // 不打印页眉
                            vRect = HC.Bounds(vPrintOffsetX,// + vMarginLeft, 防止页眉浮动Item在页边距中
                                vPrintOffsetY, 
                                this.ActiveSection.PaperWidthPix,
                                this.ActiveSection.GetHeaderAreaHeight() + vPt.Y);

                        vPrintCanvas.FillRect(vRect);
                        
                        vRect = HC.Bounds(vPrintOffsetX + vMarginLeft, 
                            vPrintOffsetY + this.ActiveSection.GetHeaderAreaHeight() + vPt.Y,
                            vData.DrawItems[vDrawItemNo].Rect.Left + vData.GetDrawItemOffsetWidth(vDrawItemNo, aStartOffset - vData.DrawItems[vDrawItemNo].CharOffs + 1),
                            vData.DrawItems[vDrawItemNo].Rect.Height);

                        vPrintCanvas.FillRect(vRect);

                        //
                        vDrawItemNo = vData.GetDrawItemNoByOffset(aEndItemNo, aEndOffset);
                        vPt = vData.DrawItems[vDrawItemNo].Rect.TopLeft();
                        vPt.Y = vPt.Y - ActiveSection.GetPageDataFmtTop(this.ActiveSection.ActivePageIndex);

                        vRect = new RECT(vPrintOffsetX + vMarginLeft + vData.DrawItems[vDrawItemNo].Rect.Left +
                            vData.GetDrawItemOffsetWidth(vDrawItemNo, aEndOffset - vData.DrawItems[vDrawItemNo].CharOffs + 1),
                            vPrintOffsetY + this.ActiveSection.GetHeaderAreaHeight() + vPt.Y,
                            vPrintOffsetX + this.ActiveSection.PaperWidthPix - vMarginRight,
                            vPrintOffsetY + this.ActiveSection.GetHeaderAreaHeight() + vPt.Y + vData.DrawItems[vDrawItemNo].Rect.Height);
                        
                        vPrintCanvas.FillRect(vRect);

                        if (!aPrintFooter)
                        {
                            vRect = new RECT(vPrintOffsetX,// + vMarginLeft, 防止页脚浮动Item在页边距中
                                vPrintOffsetY + + this.ActiveSection.GetHeaderAreaHeight() + vPt.Y + vData.DrawItems[vDrawItemNo].Rect.Height,
                                vPrintOffsetX + this.ActiveSection.PaperWidthPix,// - vMarginRight,
                                vPrintOffsetY + this.ActiveSection.PaperHeightPix);

                            vPrintCanvas.FillRect(vRect);
                        }
                        else  // 打印页脚
                        {
                            vRect = new RECT(vPrintOffsetX,
                                vPrintOffsetY + this.ActiveSection.GetHeaderAreaHeight() + vPt.Y + vData.DrawItems[vDrawItemNo].Rect.Height,
                                vPrintOffsetX + this.ActiveSection.PaperWidthPix,
                                vPrintOffsetY + this.ActiveSection.PaperHeightPix - this.ActiveSection.PaperMarginBottomPix);

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

        /// <summary> 打印当前页选中的起始、结束Item(仅限正文) </summary>
        /// <param name="aPrinter">指定打印机</param>
        /// <param name="aPrintHeader"> 是否打印页眉 </param>
        /// <param name="aPrintFooter"> 是否打印页脚 </param>
        public PrintResult PrintCurPageSelected(string aPrinter, bool aPrintHeader, bool aPrintFooter)
        {
            if (this.ActiveSection.ActiveData.SelectExists(false))
                return PrintCurPageByItemRange(aPrinter, aPrintHeader, aPrintFooter,
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

        public bool TableApplyContentAlign(HCContentAlign aAlign)
        {
            return ActiveSection.TableApplyContentAlign(aAlign);
        }

        /// <summary> 撤销 </summary>
        public void Undo()
        {
            FStyle.States.Include(HCState.hosUndoing);
            try
            {
                if (FUndoList.Enable)
                {
                    try
                    {
                        FUndoList.Enable = false;

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
                    finally
                    {
                        FUndoList.Enable = true;
                    }
                }
            }
            finally
            {
                FStyle.States.Exclude(HCState.hosUndoing);
            }
        }

        /// <summary> 恢复/重做 </summary>
        public void Redo()
        {
            FStyle.States.Include(HCState.hosRedoing);
            try
            {
                if (FUndoList.Enable)
                {
                    try
                    {
                        FUndoList.Enable = false;

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
                    finally
                    {
                        FUndoList.Enable = true;
                    }
                }
            }
            finally
            {
                FStyle.States.Exclude(HCState.hosRedoing);
            }
        }

        /// <summary> 当前位置开始查找指定的内容 </summary>
        /// <param name="aKeyword">要查找的关键字</param>
        /// <param name="aForward">True：向前，False：向后</param>
        /// <param name="aMatchCase">True：区分大小写，False：不区分大小写</param>
        /// <returns>True：找到</returns>
        public bool Search(string aKeyword, bool aForward = false, bool aMatchCase = false)
        {
            bool Result = this.ActiveSection.Search(aKeyword, aForward, aMatchCase);
            if (Result)
            {
                POINT vPt = GetTopLevelDrawItemViewCoord();  // 返回光标处DrawItem相对当前页显示的窗体坐标，有选中时，以选中结束位置的DrawItem格式化坐标
                HCCustomData vTopData = ActiveSectionTopLevelData();

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
                if (vStartDrawRect.Bottom > FViewHeight)
                    this.FVScrollBar.Position = this.FVScrollBar.Position + vStartDrawRect.Bottom - FViewHeight;
                
                if (vStartDrawRect.Left < 0)
                    this.FHScrollBar.Position = this.FHScrollBar.Position + vStartDrawRect.Left;
                else
                if (vStartDrawRect.Right > FViewWidth)
                    this.FHScrollBar.Position = this.FHScrollBar.Position + vStartDrawRect.Right - FViewWidth;
            }

            return Result;
        }

        /// <summary> 替换已经通过查找选中的内容 </summary>
        /// <param name="aText">要替换为的内容</param>
        /// <returns>是否替换成功</returns>
        public bool Replace(string aText)
        {
            return this.ActiveSection.Replace(aText);
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

        /// <summary> 当前光标所在的节 </summary>
        public HCSection ActiveSection
        {
            get { return GetActiveSection(); }
        }

        /// <summary> 水平滚动条 </summary>
        public HCStatusScrollBar HScrollBar
        {
            get { return FHScrollBar; }
        }

        /// <summary> 垂直滚动条 </summary>
        public HCRichScrollBar VScrollBar
        {
            get { return FVScrollBar; }
        }

        /// <summary> 当前光标处的文本样式 </summary>
        public int CurStyleNo
        {
            get { return GetCurStyleNo(); }
        }

        /// <summary> 当前光标处的段样式 </summary>
        public int CurParaNo
        {
            get { return GetCurParaNo(); }
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

        /// <summary> 当前文档胶卷视图时页之间的间距 </summary>
        public byte PagePadding
        {
            get { return FPagePadding; }
            set { SetPagePadding(value); }
        }

        /// <summary> 当前文档显示出来的批注 </summary>
        public HCAnnotatePre AnnotatePre
        {
            get { return FAnnotatePre; }
        }

        /// <summary> 当前文档可显示的宽度 </summary>
        public int ViewWidth
        {
            get { return FViewWidth; }
        }

        /// <summary> 当前文档可显示的高度 </summary>
        public int ViewHeight
        {
            get { return FViewHeight; }
        }

        /// <summary> 节有新的Item创建时触发 </summary>
        public EventHandler OnSectionCreateItem
        {
            get { return FOnSectionCreateItem; }
            set { FOnSectionCreateItem = value; }
        }

        /// <summary> 节有新的Item插入时触发 </summary>
        public SectionDataItemEventHandler OnSectionItemInsert
        {
            get { return FOnSectionInsertItem; }
            set { FOnSectionInsertItem = value; }
        }

        /// <summary> 节有新的Item删除时触发 </summary>
        public SectionDataItemEventHandler OnSectionRemoveItem
        {
            get { return FOnSectionRemoveItem; }
            set { FOnSectionRemoveItem = value; }
        }

        public SectionDataItemNoFunEvent OnSectionSaveItem
        {
            get { return FOnSectionSaveItem; }
            set { FOnSectionSaveItem = value; }
        }

        public SectionDataActionEventHandler OnSectionAcceptAction
        {
            get { return FOnSectionAcceptAction; }
            set { FOnSectionAcceptAction = value; }
        }

        /// <summary> Item绘制开始前触发 </summary>
        public SectionDrawItemPaintEventHandler OnSectionDrawItemPaintBefor
        {
            get { return FOnSectionDrawItemPaintBefor; }
            set { FOnSectionDrawItemPaintBefor = value; }
        }

        /// <summary> DrawItem绘制完成后触发 </summary>
        public SectionDrawItemPaintEventHandler OnSectionDrawItemPaintAfter
        {
            get { return FOnSectionDrawItemPaintAfter; }
            set { FOnSectionDrawItemPaintAfter = value; }
        }

        /// <summary> 节页眉绘制时触发 </summary>
        public SectionPaintEventHandler OnSectionPaintHeader
        {
            get { return FOnSectionPaintHeader; }
            set { FOnSectionPaintHeader = value; }
        }

        /// <summary> 节页脚绘制时触发 </summary>
        public SectionPaintEventHandler OnSectionPaintFooter
        {
            get { return FOnSectionPaintFooter; }
            set { FOnSectionPaintFooter = value; }
        }

        /// <summary> 节页面绘制时触发 </summary>
        public SectionPaintEventHandler OnSectionPaintPage
        {
            get { return FOnSectionPaintPage; }
            set { FOnSectionPaintPage = value; }
        }

        /// <summary> 节整页绘制时触发 </summary>
        public SectionPaintEventHandler OnSectionPaintPaperBefor
        {
            get { return FOnSectionPaintPaperBefor; }
            set { FOnSectionPaintPaperBefor = value; }
        }

        /// <summary> 节整页绘制后触发 </summary>
        public SectionPaintEventHandler OnSectionPaintPaperAfter
        {
            get { return FOnSectionPaintPaperAfter; }
            set { FOnSectionPaintPaperAfter = value; }
        }

        /// <summary> 节只读属性有变化时触发 </summary>
        public EventHandler OnSectionReadOnlySwitch
        {
            get { return FOnSectionReadOnlySwitch; }
            set { FOnSectionReadOnlySwitch = value; }
        }

        /// <summary> 界面显示模式：胶卷、页面 </summary>
        public HCViewModel ViewModel
        {
            get { return FViewModel; }
            set { SetViewModel(value); }
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

        /// <summary> 页码的格式 </summary>
        public string PageNoFormat
        {
            get { return FPageNoFormat; }
            set { SetPageNoFormat(value); }
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

        /// <summary> 水平滚动条滚动时触发 </summary>
        public EventHandler OnHorScroll
        {
            get { return FOnHorScroll; }
            set { FOnHorScroll = value; }
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

        /// <summary> 文档Zoom缩放变化后触发 </summary>
        public EventHandler OnZoomChanged
        {
            get { return FOnZoomChanged; }
            set { FOnZoomChanged = value; }
        }

        /// <summary> 窗口重绘开始时触发 </summary>
        public PaintEventHandler OnPaintViewBefor
        {
            get { return FOnPaintViewBefor; }
            set { FOnPaintViewBefor = value; }
        }

        /// <summary> 窗口重绘结束后触发 </summary>
        public PaintEventHandler OnPaintViewAfter
        {
            get { return FOnPaintViewAfter; }
            set { FOnPaintViewAfter = value; }
        }

        /// <summary> 创建指定样式的Item时触发 </summary>
        public StyleItemEventHandler OnSectionCreateStyleItem
        {
            get { return FOnSectionCreateStyleItem; }
            set { FOnSectionCreateStyleItem = value; }
        }

        /// <summary> 创建指定样式的FloatItem时触发 </summary>
        public FloatStyleItemEventHandler OnSectionCreateFloatStyleItem
        {
            get { return FOnSectionCreateFloatStyleItem; }
            set { FOnSectionCreateFloatStyleItem = value; }
        }

        /// <summary> 当编辑只读状态的Data时触发 </summary>
        public OnCanEditEventHandler OnSectionCanEdit
        {
            get { return FOnSectionCanEdit; }
            set { FOnSectionCanEdit = value; }
        }

        public TextEventHandler OnSectionInsertTextBefor
        {
            get { return FOnSectionInsertTextBefor; }
            set { FOnSectionInsertTextBefor = value; }
        }

        /// <summary> 节当前位置段样式和上一次不一样时触发 </summary>
        public EventHandler OnSectionCurParaNoChange
        {
            get { return FOnSectionCurParaNoChange; }
            set { FOnSectionCurParaNoChange = value; }
        }

        public SectionDataItemEventHandler OnSectionCaretItemChanged
        {
            get { return FOnSectionCaretItemChanged; }
            set { FOnSectionCaretItemChanged = value; }
        }

        /// <summary> 节当前位置文本样式和上一次不一样时触发 </summary>
        public EventHandler OnSectionActivePageChange
        {
            get { return FOnSectionActivePageChange; }
            set { FOnSectionActivePageChange = value; }
        }

        /// <summary> 文档视图有变动时触发 </summary>
        public EventHandler OnViewResize
        {
            get { return FOnViewResize; }
            set { FOnViewResize = value; }
        }
    }

    public class HCDrawAnnotate : HCDrawItemAnnotate
    {
        public HCCustomData Data;
        public RECT Rect;
    }

    public class HCDrawAnnotateDynamic : HCDrawAnnotate
    {
        public string Title;
        public string Text;
    }

    public class HCAnnotatePre : HCObject
    {
        private RECT FDrawRect;
        private int FCount;
        private bool FMouseIn, FVisible;
        private List<HCDrawAnnotate> FDrawAnnotates;
        private int FActiveDrawAnnotateIndex;
        private EventHandler FOnUpdateView;

        private int GetDrawCount()
        {
            return FDrawAnnotates.Count;
        }

        private int GetDrawAnnotateAt(int x, int y)
        {
            return GetDrawAnnotateAt(new POINT(x, y));
        }

        private int GetDrawAnnotateAt(POINT aPoint)
        {
            int Result = -1;
            for (int i = 0; i <= FDrawAnnotates.Count - 1; i++)
            {
                if (HC.PtInRect(FDrawAnnotates[i].Rect, aPoint))
                {
                    Result = i;
                    break;
                }
            }

            return Result;
        }

        private void DoUpdateView()
        {
            if (FOnUpdateView != null)
                FOnUpdateView(this, null);
        }

        private void SetMouseIn(bool value)
        {
            if (FMouseIn != value)
            {
                FMouseIn = value;
                DoUpdateView();
            }
        }

        protected bool MouseIn
        {
            get { return FMouseIn; }
            set { SetMouseIn(value); }
        }

        public HCAnnotatePre()
        {
            FDrawAnnotates = new List<HCDrawAnnotate>();
            FCount = 0;
            FVisible = false;
            FMouseIn = false;
            FActiveDrawAnnotateIndex = -1;
        }

        ~HCAnnotatePre()
        {
            FDrawAnnotates.Clear();
        }

        /// <summary> 绘制批注尾巴 </summary>
        /// <param name="Sender"></param>
        /// <param name="APageRect"></param>
        /// <param name="ACanvas"></param>
        /// <param name="APaintInfo"></param>
        public void PaintDrawAnnotate(object sender, RECT aPageRect, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            FDrawRect = new RECT(aPageRect.Right, aPageRect.Top, aPageRect.Right + HC.AnnotationWidth, aPageRect.Bottom);

            if (!aPaintInfo.Print)
            {
                if (FMouseIn)
                    aCanvas.Brush.Color = Color.FromArgb(0xD0, 0xD1, 0xD5);
                else
                    aCanvas.Brush.Color = Color.FromArgb(0xF4, 0xF4, 0xF4);  // 填充批注区域

                aCanvas.FillRect(FDrawRect);
            }

            if (FDrawAnnotates.Count > 0)  // 有批注
            {
                int vFirst = -1, vLast = -1;
                HCDrawAnnotate vDrawAnnotate = null;
                string vText = "";
                HCSection vSection = sender as HCSection;
                int vHeaderAreaHeight = vSection.GetHeaderAreaHeight();  // 页眉高度
                //正文中的批注
                int vVOffset = aPageRect.Top + vHeaderAreaHeight - aPaintInfo.PageDataFmtTop;
                int vTop = aPaintInfo.PageDataFmtTop + vVOffset;
                int vBottom = vTop + vSection.PaperHeightPix - vHeaderAreaHeight - vSection.PaperMarginBottomPix;

                for (int i = 0; i <= FDrawAnnotates.Count - 1; i++)  // 找本页的起始和结束批
                {
                    vDrawAnnotate = FDrawAnnotates[i];
                    if (vDrawAnnotate.DrawRect.Top > vBottom)
                        break;
                    else
                        if (vDrawAnnotate.DrawRect.Bottom > vTop)
                        {
                            vLast = i;
                            if (vFirst < 0)
                                vFirst = i;
                        }
                }

                if (vFirst >= 0)
                {
                    aCanvas.Font.BeginUpdate();
                    try
                    {
                        aCanvas.Font.Size = 8;
                        aCanvas.Font.Family = "宋体";
                        aCanvas.Font.Color = Color.Black;
                    }
                    finally
                    {
                        aCanvas.Font.EndUpdate();
                    }
                    // 计算本页各批注显示位置
                    vTop = FDrawAnnotates[vFirst].DrawRect.Top;
                    for (int i = vFirst; i <= vLast; i++)
                    {
                        vDrawAnnotate = FDrawAnnotates[i];
                        if (vDrawAnnotate.DrawRect.Top > vTop)
                            vTop = vDrawAnnotate.DrawRect.Top;

                        if (vDrawAnnotate is HCDrawAnnotateDynamic)
                            vText = (vDrawAnnotate as HCDrawAnnotateDynamic).Title + ":" + (vDrawAnnotate as HCDrawAnnotateDynamic).Text;
                        else
                            vText = vDrawAnnotate.DataAnnotate.Title + ":" + vDrawAnnotate.DataAnnotate.Text;

                        vDrawAnnotate.Rect = new RECT(0, 0, HC.AnnotationWidth - 30, 0);

                        User.DrawTextEx(aCanvas.Handle, vText, -1, ref vDrawAnnotate.Rect,
                            User.DT_TOP | User.DT_LEFT | User.DT_WORDBREAK | User.DT_CALCRECT, IntPtr.Zero);  // 计算区域
                        if (vDrawAnnotate.Rect.Right < HC.AnnotationWidth - 30)
                            vDrawAnnotate.Rect.Right = HC.AnnotationWidth - 30;

                        vDrawAnnotate.Rect.Offset(aPageRect.Right + 20, vTop + 5);
                        vDrawAnnotate.Rect.Inflate(5, 5);

                        vTop = vDrawAnnotate.Rect.Bottom + 5;
                    }

                    if (FDrawAnnotates[vLast].Rect.Bottom > aPageRect.Bottom)
                    {
                        vVOffset = FDrawAnnotates[vLast].Rect.Bottom - aPageRect.Bottom + 5;  // 需要上移这么大的空间可放下

                        int vSpace = 0;  // 各批注之间除固定间距外的空隙
                        int vRePlace = -1;  // 从哪一个开始调整
                        vTop = FDrawAnnotates[vLast].Rect.Top;
                        for (int i = vLast; i >= vFirst; i--)  // 紧凑排列，去掉中间的空
                        {
                            vSpace = vTop - FDrawAnnotates[i].Rect.Bottom - 5;
                            vVOffset = vVOffset - vSpace;  // 消减后的剩余
                            if (vVOffset <= 0)
                            {
                                vRePlace = i + 1;
                                if (vVOffset < 0)
                                    vSpace = vSpace + vVOffset;  // vRePlace处实际需要偏移的量

                                break;
                            }

                            vTop = FDrawAnnotates[i].Rect.Top;
                        }

                        if (vRePlace < 0)
                        {
                            vRePlace = vFirst;
                            vSpace = FDrawAnnotates[vFirst].Rect.Top - aPageRect.Top - 5;
                            if (vSpace > vVOffset)
                                vSpace = vVOffset;  // 只调整到需要的位置
                        }

                        FDrawAnnotates[vRePlace].Rect.Offset(0, -vSpace);
                        vTop = FDrawAnnotates[vRePlace].Rect.Bottom + 5;
                        for (int i = vRePlace; i <= vLast; i++)
                        {
                            vVOffset = vTop - FDrawAnnotates[i].Rect.Top;
                            FDrawAnnotates[i].Rect.Offset(0, vVOffset);
                            vTop = FDrawAnnotates[i].Rect.Bottom + 5;
                        }
                    }

                    HCAnnotateData vData;
                    RECT vTextRect;
                    aCanvas.Pen.Color = Color.Red;
                    for (int i = vFirst; i <= vLast; i++)  // 绘制批
                    {
                        vDrawAnnotate = FDrawAnnotates[i];
                        if (vDrawAnnotate is HCDrawAnnotateDynamic)
                        {
                            vText = (vDrawAnnotate as HCDrawAnnotateDynamic).Title + ":" + (vDrawAnnotate as HCDrawAnnotateDynamic).Text;
                            aCanvas.Pen.Style = HCPenStyle.psDot;
                            aCanvas.Pen.Width = 1;
                            aCanvas.Brush.Color = HC.AnnotateBKColor;
                        }
                        else
                        {
                            vText = vDrawAnnotate.DataAnnotate.Title + ":" + vDrawAnnotate.DataAnnotate.Text;
                            vData = vDrawAnnotate.Data as HCAnnotateData;

                            if (vDrawAnnotate.DataAnnotate == vData.HotAnnotate)
                            {
                                aCanvas.Pen.Style = HCPenStyle.psSolid;
                                aCanvas.Pen.Width = 1;
                                aCanvas.Brush.Color = HC.AnnotateBKActiveColor;
                            }
                            else
                                if (vDrawAnnotate.DataAnnotate == vData.ActiveAnnotate)
                                {
                                    aCanvas.Pen.Style = HCPenStyle.psSolid;
                                    aCanvas.Pen.Width = 2;
                                    aCanvas.Brush.Color = HC.AnnotateBKActiveColor;
                                }
                                else
                                {
                                    aCanvas.Pen.Style = HCPenStyle.psDot;
                                    aCanvas.Pen.Width = 1;
                                    aCanvas.Brush.Color = HC.AnnotateBKColor;
                                }
                        }

                        if (aPaintInfo.Print)
                            aCanvas.Brush.Style = HCBrushStyle.bsClear;

                        aCanvas.RoundRect(vDrawAnnotate.Rect, 5, 5);  // 填充批注区域
                        vTextRect = vDrawAnnotate.Rect;
                        vTextRect.Inflate(-5, -5);

                        User.DrawTextEx(aCanvas.Handle, i.ToString() + vText, -1, ref vTextRect,
                            User.DT_VCENTER | User.DT_LEFT | User.DT_WORDBREAK, IntPtr.Zero);

                        // 绘制指向线
                        aCanvas.Brush.Style = HCBrushStyle.bsClear;
                        aCanvas.MoveTo(vDrawAnnotate.DrawRect.Right, vDrawAnnotate.DrawRect.Bottom);
                        aCanvas.LineTo(aPageRect.Right, vDrawAnnotate.DrawRect.Bottom);
                        aCanvas.LineTo(vDrawAnnotate.Rect.Left, vTextRect.Top);
                    }
                }
            }
        }

        /// <summary> 有批注插入 </summary>
        public void InsertDataAnnotate(HCDataAnnotate aDataAnnotate)
        {
            FCount++;
            FVisible = true;
        }

        public void RemoveDataAnnotate(HCDataAnnotate aDataAnnotate)
        {
            FCount--;
        }

        public void AddDrawAnnotate(HCDrawAnnotate aDrawAnnotate)
        {
            FDrawAnnotates.Add(aDrawAnnotate);
        }

        public void ClearDrawAnnotate()
        {
            FDrawAnnotates.Clear();
        }

        public HCDataAnnotate ActiveAnnotate()
        {
            if (FActiveDrawAnnotateIndex < 0)
                return null;
            else
                return FDrawAnnotates[FActiveDrawAnnotateIndex].DataAnnotate;
        }

        public void DeleteDataAnnotateByDraw(int aIndex)
        {
            if (aIndex >= 0)
            {
                (FDrawAnnotates[aIndex].Data as HCAnnotateData).DataAnnotates.DeleteByID(FDrawAnnotates[aIndex].DataAnnotate.ID);
                DoUpdateView();
            }
        }

        public void MouseDown(int x, int y)
        {
            FActiveDrawAnnotateIndex = GetDrawAnnotateAt(x, y);
        }

        public void MouseMove(int x, int y)
        {
            POINT vPt = new POINT(x, y);
            MouseIn = HC.PtInRect(FDrawRect, vPt);
            //vIndex = GetDrawAnnotateAt(vPt);
        }

        public int DrawCount
        {
            get { return GetDrawCount(); }
        }

        public List<HCDrawAnnotate> DrawAnnotates
        {
            get { return FDrawAnnotates; }
        }

        public bool Visible
        {
            get { return FVisible; }
            set { FVisible = value; }
        }

        public int Count
        {
            get { return FCount; }
        }

        public RECT DrawRect
        {
            get { return FDrawRect; }
        }

        public int ActiveDrawAnnotateIndex
        {
            get { return FActiveDrawAnnotateIndex; }
        }

        public EventHandler OnUpdateView
        {
            get { return FOnUpdateView; }
            set { FOnUpdateView = value; }
        }
    }
}
