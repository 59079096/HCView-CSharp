/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-8-17             }
{                                                       }
{                  文档节基类实现单元                   }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.IO;
using HC.Win32;
using System.Xml;

namespace HC.View
{
    public enum PrintResult : byte 
    {
        prOk, prNoPrinter, prNoSupport, prError
    }

    public class SectionPaintInfo : PaintInfo
    {
        private int FSectionIndex, FPageIndex, FPageDataFmtTop;

        public SectionPaintInfo()
        {
            FSectionIndex = -1;
            FPageIndex  = -1;
            FPageDataFmtTop = 0;
        }

        public int SectionIndex
        {
            get { return FSectionIndex; }
            set { FSectionIndex = value; }
        }

        public int PageIndex
        {
            get { return FPageIndex; }
            set { FPageIndex = value; }
        }

        public int PageDataFmtTop
        {
            get { return FPageDataFmtTop; }
            set { FPageDataFmtTop = value; }
        }
    }

    public delegate void SectionPaintEventHandler(object sender, int aPageIndex,
        RECT aRect, HCCanvas aCanvas, SectionPaintInfo aPaintInfo);

    public delegate void SectionDrawItemPaintEventHandler(object sender, HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
        int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo);

    public delegate void SectionDataItemEventHandler(object sender, HCCustomData aData, HCCustomItem aItem);

    public delegate void SectionDataFloatItemEventHandler(object sender, HCSectionData aData, HCCustomFloatItem aItem);

    public delegate bool SectionDataItemFunEvent(object sender, HCCustomData aData, HCCustomItem aItem);

    public delegate void SectionDrawItemAnnotateEventHandler(object sender, HCCustomData aData, int aDrawItemNo, RECT aDrawRect,
        HCDataAnnotate aDataAnnotate);

    public delegate void SectionAnnotateEventHandler(object sender, HCCustomData aData, HCDataAnnotate aDataAnnotate);

    public delegate void SectionDataItemMouseEventHandler(object sender, HCCustomData aData, int aItemNo, int aOffset, MouseEventArgs e);

    public class HCCustomSection : HCObject
    {
        private HCStyle FStyle;
        private HCPages FPages;  // 所有页面
        private HCPaper FPaper;
        private PaperOrientation FPaperOrientation;
        private HCHeaderData FHeader;
        private HCFooterData FFooter;
        private HCPageData FPage;
        private HCViewModel FViewModel;
        private HCSectionData FActiveData;  // 页眉、正文、页脚
        private HCSectionData FMoveData;

        /// <summary> 是否对称边距 </summary>
        bool FSymmetryMargin;
        bool FPageNoVisible;  // 是否显示页码

        byte FPagePadding;

        int FPageNoFrom,  // 页码从几开始
            FActivePageIndex,  // 当前激活的页
            FMousePageIndex,  // 当前鼠标所在页
            FDisplayFirstPageIndex,  // 屏显第一页
            FDisplayLastPageIndex,   // 屏显最后一页
            FHeaderOffset;  // 页眉顶部偏移

        EventHandler
            FOnDataChange,  // 页眉、页脚、页面某一个修改时触发
            FOnCheckUpdateInfo,  // 当前Data需要UpdateInfo更新时触发
            FOnReadOnlySwitch,  // 页眉、页脚、页面只读状态发生变化时触发
            FOnChangeTopLevelData;  // 切换页眉、页脚、正文、表格单元格时触发

        GetScreenCoordEventHandler FOnGetScreenCoord;

        SectionPaintEventHandler FOnPaintHeader, FOnPaintFooter, FOnPaintPage, FOnPaintPaperBefor, FOnPaintPaperAfter;

        SectionDrawItemPaintEventHandler FOnDrawItemPaintBefor, FOnDrawItemPaintAfter;

        SectionAnnotateEventHandler FOnInsertAnnotate, FOnRemoveAnnotate;
        SectionDrawItemAnnotateEventHandler FOnDrawItemAnnotate;

        DrawItemPaintContentEventHandler FOnDrawItemPaintContent;
        SectionDataFloatItemEventHandler FOnInsertFloatItem;
        SectionDataItemEventHandler FOnInsertItem, FOnRemoveItem;
        SectionDataItemFunEvent FOnSaveItem, FOnDeleteItem;
        SectionDataItemMouseEventHandler FOnItemMouseDown, FOnItemMouseUp;
        DataItemNoEventHandler FOnItemResize;
        EventHandler FOnCreateItem, FOnCurParaNoChange, FOnActivePageChange;
        StyleItemEventHandler FOnCreateItemByStyle;
        FloatStyleItemEventHandler FOnCreateFloatItemByStyle;
        OnCanEditEventHandler FOnCanEdit;
        GetUndoListEventHandler FOnGetUndoList;

        private int GetPageIndexByFilm(int aVOffset)
        {
            int Result = -1;
            int vPos = 0;
            for (int i = 0; i <= FPages.Count - 1; i++)
            {
                if (FViewModel == HCViewModel.hvmFilm)
                    vPos = vPos + FPagePadding + FPaper.HeightPix;
                else
                    vPos = vPos + GetPageHeight();

                if (vPos >= aVOffset)
                {
                    Result = i;
                    break;
                }
            }

            if ((Result < 0) && (aVOffset > vPos))
                Result = FPages.Count - 1;

            return Result;
        }

        /// <summary> 当前Data需要UpdateInfo更新 </summary>
        protected void DoActiveDataCheckUpdateInfo()
        {
            if (FOnCheckUpdateInfo != null)
                FOnCheckUpdateInfo(this, null);
        }

        private void DoDataReadOnlySwitch(object sender, EventArgs e)
        {
            if (FOnReadOnlySwitch != null)
                FOnReadOnlySwitch(this, null);
        }

        private POINT DoGetScreenCoordEvent(int x, int y)
        {
            if (FOnGetScreenCoord != null)
                return FOnGetScreenCoord(x, y);
            else
                return new POINT(0, 0);
        }

        private void DoDataDrawItemPaintBefor(HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (FOnDrawItemPaintBefor != null)
                FOnDrawItemPaintBefor(this, aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight,
                    aDataDrawBottom, aDataScreenTop, aDataScreenBottom, ACanvas, APaintInfo);
        }

        private void DoDataDrawItemPaintContent(HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect, RECT aClearRect, string aDrawText,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (FOnDrawItemPaintContent != null)
                FOnDrawItemPaintContent(aData, aItemNo, aDrawItemNo, aDrawRect, aClearRect, aDrawText,
                    aDataDrawLeft, aDataDrawRight, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        private void DoDataDrawItemPaintAfter(HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (FOnDrawItemPaintAfter != null)
                FOnDrawItemPaintAfter(this, aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight,
                    aDataDrawBottom, aDataScreenTop, aDataScreenBottom, ACanvas, APaintInfo);
        }

        private void DoDataInsertAnnotate(HCCustomData aData, HCDataAnnotate aDataAnnotate)
        {
            if (FOnInsertAnnotate != null)
                FOnInsertAnnotate(this, aData, aDataAnnotate);
        }

        private void DoDataRemoveAnnotate(HCCustomData aData, HCDataAnnotate aDataAnnotate)
        {
            if (FOnRemoveAnnotate != null)
                FOnRemoveAnnotate(this, aData, aDataAnnotate);
        }

        private void DoDataDrawItemAnnotate(HCCustomData aData, int aDrawItemNo, RECT aDrawRect, HCDataAnnotate aDataAnnotate)
        {
            if (FOnDrawItemAnnotate != null)
                FOnDrawItemAnnotate(this, aData, aDrawItemNo, aDrawRect, aDataAnnotate);
        }

        private void DoDataInsertFloatItem(HCSectionData aData, HCCustomFloatItem aItem)
        {
            if (FOnInsertFloatItem != null)
                FOnInsertFloatItem(this, aData, aItem);
        }
        private void DoDataInsertItem(HCCustomData aData, HCCustomItem aItem)
        {
            if (FOnInsertItem != null)
                FOnInsertItem(this, aData, aItem);
        }

        private void DoDataRemoveItem(HCCustomData aData, HCCustomItem aItem)
        {
            if (FOnRemoveItem != null)
                FOnRemoveItem(this, aData, aItem);
        }

        private bool DoDataSaveItem(HCCustomData aData, HCCustomItem aItem)
        {
            if (FOnSaveItem != null)
                return FOnSaveItem(this, aData, aItem);
            else
                return true;
        }

        private bool DoDataDeleteItem(HCCustomData aData, HCCustomItem aItem)
        {
            if (FOnDeleteItem != null)
                return FOnDeleteItem(this, aData, aItem);
            else
                return true;
        }

        private void DoDataItemMouseDown(HCCustomData aData, int aItemNo, int aOffset, MouseEventArgs e)
        {
            if (FOnItemMouseDown != null)
                FOnItemMouseDown(this, aData, aItemNo, aOffset, e);
        }

        private void DoDataItemMouseUp(HCCustomData aData, int aItemNo, int aOffset, MouseEventArgs e)
        {
            if (FOnItemMouseUp != null)
                FOnItemMouseUp(this, aData, aItemNo, aOffset, e);
        }

        protected void DoDataChanged(object sender)
        {
            if (FOnDataChange != null)
                FOnDataChange(sender, null);
        }

        /// <summary> 缩放Item约束不要超过整页宽、高 </summary>
        private void DoDataItemResized(HCCustomData aData, int aItemNo)
        {
            HCResizeRectItem vResizeItem = aData.Items[aItemNo] as HCResizeRectItem;
            int vWidth = GetPageWidth();  // 页宽
            int vHeight = 0;
            HCCustomData vData = aData.GetRootData();  // 获取是哪一部分的ResizeItem
            if (vData == FHeader)
                vHeight = GetHeaderAreaHeight();
            else
                if (vData == FFooter)
                    vHeight = FPaper.MarginBottomPix;
                else
                    vHeight = GetPageHeight();// - FStyle.ParaStyles[vResizeItem.ParaNo].LineSpace;

            vResizeItem.RestrainSize(vWidth, vHeight);

            if (FOnItemResize != null)
                FOnItemResize(aData, aItemNo);
        }

        private HCCustomItem DoDataCreateStyleItem(HCCustomData aData, int aStyleNo)
        {
            if (FOnCreateItemByStyle != null)
                return FOnCreateItemByStyle(aData, aStyleNo);
            else
                return null;
        }

        private HCCustomFloatItem DoDataCreateFloatStyleItem(HCSectionData aData, int aStyleNo)
        {
            if (FOnCreateFloatItemByStyle != null)
                return FOnCreateFloatItemByStyle(aData, aStyleNo);
            else
                return null;
        }

        private bool DoDataCanEdit(object sender)
        {
            if (FOnCanEdit != null)
                return FOnCanEdit(sender);
            else
                return true;
        }

        private void DoDataCreateItem(object sender, EventArgs e)
        {
            if (FOnCreateItem != null)
                FOnCreateItem(sender, null);
        }

        private void DoDataCurParaNoChange(object sender, EventArgs e)
        {
            if (FOnCurParaNoChange != null)
                FOnCurParaNoChange(sender, e);
        }

        private HCUndoList DoDataGetUndoList()
        {
            if (FOnGetUndoList != null)
                return FOnGetUndoList();
            else
                return null;
        }

        /// <summary> 返回页面Data指定DrawItem所在的页(跨页的按最后位置所在页) </summary>
        /// <param name="aDrawItemNo"></param>
        /// <returns></returns>
        private int GetPageIndexByPageDataDrawItem(int aDrawItemNo)
        {
            if (aDrawItemNo < 0)
                return 0;

            int Result = FPages.Count - 1;
            for (int i = 0; i <= FPages.Count - 1; i++)
            {
                if (FPages[i].EndDrawItemNo >= aDrawItemNo)
                {
                    Result = i;
                    break;
                }
            }

            return Result;
        }

        /// <summary> 将某一页的坐标转换到页指定Data的坐标(此方法需要AX、AY在此页上的前提) </summary>
        /// <param name="aPageIndex"></param>
        /// <param name="aData"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="ARestrain">是否约束到Data的绝对区域中</param>
        private void PaperCoordToData(int aPageIndex, HCViewData aData, ref int aX, ref int aY,
          bool ARestrain = true)
        {
            if (FViewModel != HCViewModel.hvmFilm)
                return;

            int viTemp = -1;
            int vMarginLeft = GetPageMarginLeft(aPageIndex);
            aX = aX - vMarginLeft;

            // 为避免边界(激活正文，在页眉页脚点击时判断仍是在正文位置造成光标错误)约束后都偏移1
            if (aData == FHeader)
            {
                aY = aY - GetHeaderPageDrawTop();  // 相对页眉绘制位置
                if (ARestrain)
                {
                    if (aY < 0)
                        aY = 1;
                    else
                    {
                        viTemp = FHeader.Height;
                        if (aY > viTemp)
                            aY = viTemp - 1;
                    }
                }
            }
            else
            if (aData == FFooter)
            {
                aY = aY - FPaper.HeightPix + FPaper.MarginBottomPix;
                if (ARestrain)
                {
                    if (aY < 0)
                        aY = 1;
                    else
                        if (aY > FPaper.MarginBottomPix)
                            aY = FPaper.MarginBottomPix - 1;
                }
            }
            else
            if (aData == FPage)
            {
                aY = aY - GetHeaderAreaHeight();
                if (ARestrain)
                {
                    if (aY < 0)
                        aY = 1;  // 处理激活正文，在页眉页脚中点击
                    else
                    {
                        viTemp = GetPageHeight();
                        if (aY > viTemp)
                            aY = viTemp - 1;
                    }
                }
            }
        }

        private bool GetReadOnly()
        {
            return FHeader.ReadOnly && FFooter.ReadOnly && FPage.ReadOnly;
        }

        private void SetReadOnly(bool value)
        {
            FHeader.ReadOnly = value;
            FFooter.ReadOnly = value;
            FPage.ReadOnly = value;
        }

        private void SetActivePageIndex(int Value)
        {
            if (FActivePageIndex != Value)
            {
                FActivePageIndex = Value;
                if (FOnActivePageChange != null)
                    FOnActivePageChange(this, null);
            }
        }

        private int GetCurStyleNo()
        {
            return FActiveData.GetTopLevelData().CurStyleNo;
        }

        private int GetCurParaNo()
        {
            return FActiveData.GetTopLevelData().CurParaNo;
        }

        protected void KillFocus()
        {
            FActiveData.KillFocus();
        }

        // 纸张信息
        protected PaperKind GetPaperSize()
        {
            return FPaper.Size;
        }

        protected void SetPaperSize(PaperKind Value)
        {
            FPaper.Size = Value;
        }

        // 边距信息
        protected Single GetPaperWidth()
        {
            return FPaper.Width;
        }

        protected Single GetPaperHeight()
        {
            return FPaper.Height;
        }

        protected Single GetPaperMarginTop()
        {
            return FPaper.MarginTop;
        }

        protected Single GetPaperMarginLeft()
        {
            return FPaper.MarginLeft;
        }

        protected Single GetPaperMarginRight()
        {
            return FPaper.MarginRight;
        }

        protected Single GetPaperMarginBottom()
        {
            return FPaper.MarginBottom;
        }

        protected void SetPaperWidth(Single value)
        {
            FPaper.Width = value;
        }

        protected void SetPaperHeight(Single value)
        {
            FPaper.Height = value;
        }

        protected void SetPaperMarginTop(Single value)
        {
            FPaper.MarginTop = value;
        }

        protected void SetPaperMarginLeft(Single value)
        {
            FPaper.MarginLeft = value;
        }

        protected void SetPaperMarginRight(Single value)
        {
            FPaper.MarginRight = value;
        }

        protected void SetPaperMarginBottom(Single value)
        {
            FPaper.MarginBottom = value;
        }

        protected int GetPaperWidthPix()
        {
            return FPaper.WidthPix;
        }

        protected int GetPaperHeightPix()
        {
            return FPaper.HeightPix;
        }

        protected int GetPaperMarginTopPix()
        {
            return FPaper.MarginTopPix;
        }

        protected int GetPaperMarginLeftPix()
        {
            return FPaper.MarginLeftPix;
        }

        protected int GetPaperMarginRightPix()
        {
            return FPaper.MarginRightPix;
        }

        protected int GetPaperMarginBottomPix()
        {
            return FPaper.MarginBottomPix;
        }

        protected void SetHeaderOffset(int value)
        {
            if (FHeaderOffset != value)
            {
                FHeaderOffset = value;
                BuildSectionPages(0);
                DoDataChanged(this);
            }
        }

        protected HCPage NewEmptyPage()
        {
            HCPage Result = new HCPage();
            FPages.Add(Result);
            return Result;
        }

        protected int GetPageCount()
        {
            return FPages.Count;  // 所有页面
        }

        protected HCSectionData GetSectionDataAt(int x, int y)
        {
            int vPageIndex = GetPageIndexByFilm(y);
            int vMarginLeft = -1, vMarginRight = -1;
            GetPageMarginLeftAndRight(vPageIndex, ref vMarginLeft, ref vMarginRight);
            // 确定点击页面显示区域
            if (x < 0)
            {
                return FActiveData;
            }
            if (x > FPaper.WidthPix)
            {
                return FActiveData;
            }
            if (y < 0)
            {
                return FActiveData;
            }
            if (y > FPaper.HeightPix)
            {
                return FActiveData;
            }
            // 边距信息，先上下，再左右
            if (y > FPaper.HeightPix - FPaper.MarginBottomPix)
                return FFooter;
            // 页眉区域实际高(页眉内容高度>上边距时，取页眉内容高度)
            if (y < GetHeaderAreaHeight())
                return FHeader;
            //if X > FPageSize.PageWidthPix - vMarginRight then Exit;  // 点击在页右边距区域TEditArea.eaMarginRight
            //if X < vMarginLeft then Exit;  // 点击在页左边距区域TEditArea.eaMarginLeft
            //如果要区分左、右边距不是正文，注意双击等判断ActiveData为nil
            return FPage;
        }

        protected SectionArea GetActiveArea()
        {
            if (FActiveData == FHeader)
                return SectionArea.saHeader;
            else
                if (FActiveData == FFooter)
                    return SectionArea.saFooter;
                else
                    return SectionArea.saPage;
        }

        protected void SetActiveData(HCSectionData value)
        {
            if (FActiveData != value)
            {
                if (FActiveData != null)
                {
                    FActiveData.DisSelect();
                    FActiveData.DisActive();  // 旧的取消激活
                }

                FActiveData = value;
                FStyle.UpdateInfoReScroll();
            }
        }

        /// <summary> 返回数据格式化AVertical位置在胶卷中的位置 </summary>
        /// <param name="aVertical"></param>
        /// <returns></returns>
        protected int GetDataFmtTopFilm(int aVertical)
        {
            int Result = 0, vTop = 0;
            int vPageHeight = GetPageHeight();

            for (int i = 0; i <= FPages.Count - 1; i++)
            {
                vTop = vTop + vPageHeight;
                if (vTop >= aVertical)
                {
                    vTop = aVertical - (vTop - vPageHeight);
                    break;
                }
                else
                    Result = Result + FPagePadding + FPaper.HeightPix;
            }
            Result = Result + FPagePadding + GetHeaderAreaHeight() + vTop;

            return Result;
        }

        protected bool ActiveDataChangeByAction(HCFunction aFunction)
        {
            if (!FActiveData.CanEdit())
                return false;

            if (FActiveData.FloatItemIndex >= 0)
                return false;

            bool Result = aFunction();  // 处理变动

            if (FActiveData.FormatChange)
            {
                FActiveData.FormatChange = false;

                if (FActiveData == FPage)
                    BuildSectionPages(FActiveData.FormatStartDrawItemNo);
                else
                    BuildSectionPages(0);
            }

            DoDataChanged(this);

            return Result;
        }

        protected HCStyle Style
        {
            get { return FStyle; }
        }

        // HCCustomSection子方法
        protected void SetDataProperty(int vWidth, HCSectionData aData)
        {
            aData.Width = vWidth;
            aData.OnInsertFloatItem = DoDataInsertFloatItem;
            aData.OnInsertItem = DoDataInsertItem;
            aData.OnRemoveItem = DoDataRemoveItem;
            aData.OnSaveItem = DoDataSaveItem;
            aData.OnDeleteItem = DoDataDeleteItem;
            aData.OnItemResized = DoDataItemResized;
            aData.OnItemMouseDown = DoDataItemMouseDown;
            aData.OnItemMouseUp = DoDataItemMouseUp;
            aData.OnCreateItemByStyle = DoDataCreateStyleItem;
            aData.OnCreateFloatItemByStyle = DoDataCreateFloatStyleItem;
            aData.OnCanEdit = DoDataCanEdit;
            aData.OnCreateItem = DoDataCreateItem;
            aData.OnReadOnlySwitch = DoDataReadOnlySwitch;
            aData.OnGetScreenCoord = DoGetScreenCoordEvent;
            aData.OnDrawItemPaintBefor = DoDataDrawItemPaintBefor;
            aData.OnDrawItemPaintAfter = DoDataDrawItemPaintAfter;
            aData.OnDrawItemPaintContent = DoDataDrawItemPaintContent;
            aData.OnInsertAnnotate = DoDataInsertAnnotate;
            aData.OnRemoveAnnotate = DoDataRemoveAnnotate;
            aData.OnDrawItemAnnotate = DoDataDrawItemAnnotate;
            aData.OnGetUndoList = DoDataGetUndoList;
            aData.OnCurParaNoChange = DoDataCurParaNoChange;
        }

        public HCCustomSection(HCStyle aStyle)
        {
            FStyle = aStyle;
            FActiveData = null;
            FMoveData = null;
            FPageNoVisible = true;
            FPageNoFrom = 1;
            FHeaderOffset = 20;
            FViewModel = HCViewModel.hvmFilm;
            FPagePadding = 20;
            FDisplayFirstPageIndex = -1;
            FDisplayLastPageIndex = -1;

            FPaper = new HCPaper();
            FPaperOrientation = PaperOrientation.cpoPortrait;
            int vWidth = GetPageWidth();

            FPage = new HCPageData(aStyle);
            SetDataProperty(vWidth, FPage);

            // FData.PageHeight := PageHeightPix - PageMarginBottomPix - GetHeaderAreaHeight;
            // 在ReFormatSectionData中处理了FData.PageHeight
            FHeader = new HCHeaderData(aStyle);
            SetDataProperty(vWidth, FHeader);

            FFooter = new HCFooterData(aStyle);
            SetDataProperty(vWidth, FFooter);

            FActiveData = FPage;
            FSymmetryMargin = true;  // 对称页边距 debug

            FPages = new HCPages();
            NewEmptyPage();           // 创建空白页
            FPages[0].StartDrawItemNo = 0;
            FPages[0].EndDrawItemNo = 0;
        }

        ~HCCustomSection()
        {

        }

        public override void Dispose()
        {
            FHeader.Dispose();
            FFooter.Dispose();
            FPage.Dispose();
            FPaper.Dispose();
            base.Dispose();
        }

        /// <summary> 修改纸张边距 </summary>
        public void ResetMargin()
        {
            FPage.Width = GetPageWidth();

            FHeader.Width = FPage.Width;
            FFooter.Width = FPage.Width;

            FormatData();

            BuildSectionPages(0);

            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret(false);

            DoDataChanged(this);
        }

        public void ReAdaptActiveItem()
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ReAdaptActiveItem();
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void DisActive()
        {
            FActiveData.DisSelect();

            FHeader.InitializeField();
            FFooter.InitializeField();
            FPage.InitializeField();
            FActiveData = FPage;
        }

        public bool SelectExists()
        {
            return FActiveData.SelectExists();
        }

        public void SelectAll()
        {
            FActiveData.SelectAll();
        }

        public string GetHint()
        {
            return (FActiveData.GetTopLevelData() as HCRichData).GetHint();
        }

        public HCCustomItem GetActiveItem()
        {
            return FActiveData.GetActiveItem();
        }

        public HCCustomItem GetTopLevelItem()
        {
            return FActiveData.GetTopLevelItem();
        }

        public HCCustomDrawItem GetTopLevelDrawItem()
        {
            return FActiveData.GetTopLevelDrawItem();
        }

        public POINT GetActiveDrawItemCoord()
        {
            return FActiveData.GetActiveDrawItemCoord();
        }

        /// <summary> 返回光标或选中结束位置所在页序号 </summary>
        public int GetPageIndexByCurrent()
        {
            int Result = -1, vCaretDrawItemNo = -1;
            if (FActiveData != FPage)
                Result = FActivePageIndex;
            else
            {
                if (FPage.CaretDrawItemNo < 0)
                {
                    vCaretDrawItemNo = FPage.GetDrawItemNoByOffset(FPage.SelectInfo.StartItemNo,
                        FPage.SelectInfo.StartItemOffset);
                }
                else
                    vCaretDrawItemNo = FPage.CaretDrawItemNo;

                HCCaretInfo vCaretInfo = new HCCaretInfo();
                for (int i = 0; i <= FPages.Count - 1; i++)
                {
                    if (FPages[i].EndDrawItemNo >= vCaretDrawItemNo)
                    {
                        if ((i < FPages.Count - 1) && (FPages[i + 1].StartDrawItemNo == vCaretDrawItemNo))
                        {
                            if (FPage.SelectInfo.StartItemNo >= 0)
                            {
                                vCaretInfo.Y = 0;
                                (FPage as HCCustomData).GetCaretInfo(FPage.SelectInfo.StartItemNo,
                                    FPage.SelectInfo.StartItemOffset, ref vCaretInfo);

                                Result = GetPageIndexByFormat(vCaretInfo.Y);
                            }
                            else
                                Result = GetPageIndexByPageDataDrawItem(vCaretDrawItemNo);
                        }
                        else
                            Result = i;

                        break;
                    }
                }
            }

            return Result;
        }

        /// <summary> 返回正文格式位置所在页序号 </summary>
        public int GetPageIndexByFormat(int aVOffset)
        {
            return aVOffset / GetPageHeight();
        }

        /// <summary> 直接设置当前TextItem的Text值 </summary>
        public void SetActiveItemText(string aText)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.SetActiveItemText(aText);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void PaintDisplayPage(int aFilmOffsetX, int aFilmOffsetY, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            for (int i = FDisplayFirstPageIndex; i <= FDisplayLastPageIndex; i++)
            {
                aPaintInfo.PageIndex = i;
                int vPaperFilmTop = 0;
                if (aPaintInfo.ViewModel == HCViewModel.hvmFilm)
                    vPaperFilmTop = GetPageTopFilm(i);
                else
                    vPaperFilmTop = GetPageTop(i);

                int vPageDrawTop = vPaperFilmTop - aFilmOffsetY;  // 映射到当前页面为原点的屏显起始位置(可为负数)
                PaintPaper(i, aFilmOffsetX, vPageDrawTop, aCanvas, aPaintInfo);
            }
        }

        public void KeyPress(KeyPressEventArgs e)
        {
            if (!FActiveData.CanEdit())
                return;

            if (HC.IsKeyPressWant(e))
            {
                Char vKey = e.KeyChar;

                HCFunction vEvent = delegate()
                {
                    FActiveData.KeyPress(ref vKey);
                    return true;
                };

                ActiveDataChangeByAction(vEvent);

                e.KeyChar = vKey;
            }
            else
                e.KeyChar = (Char)0;
        }

        public void KeyDown(KeyEventArgs e)
        {
            if (!FActiveData.CanEdit())
                return;

            if (FActiveData.KeyDownFloatItem(e))
            {
                DoActiveDataCheckUpdateInfo();
                return;
            }

            int Key = e.KeyValue;
            if (HC.IsKeyDownWant(Key))
            {
                switch (Key)
                {
                    case User.VK_BACK:
                    case User.VK_DELETE:
                    case User.VK_RETURN:
                    case User.VK_TAB:
                        {
                            HCFunction vEvent = delegate()
                            {
                                FActiveData.KeyDown(e);
                                return true;
                            };
                            ActiveDataChangeByAction(vEvent);

                            //Key = vKey;
                        }
                        break;

                    case User.VK_LEFT:
                    case User.VK_RIGHT:
                    case User.VK_UP:
                    case User.VK_DOWN:
                    case User.VK_HOME:
                    case User.VK_END:
                        FActiveData.KeyDown(e);
                        SetActivePageIndex(GetPageIndexByCurrent());  // 方向键可能移动到了其他页
                        DoActiveDataCheckUpdateInfo();
                        break;
                }
            }
        }

        public void KeyUp(KeyEventArgs e)
        {
            if (!FActiveData.CanEdit())
                return;

            FActiveData.KeyUp(e);
        }

        //
        public void ApplyTextStyle(HCFontStyle aFontStyle)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyTextStyle(aFontStyle);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void ApplyTextFontName(string aFontName)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyTextFontName(aFontName);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void ApplyTextFontSize(Single aFontSize)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyTextFontSize(aFontSize);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void ApplyTextColor(Color aColor)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyTextColor(aColor);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void ApplyTextBackColor(Color aColor)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyTextBackColor(aColor);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void ApplyTableCellAlign(HCContentAlign aAlign)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyTableCellAlign(aAlign);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public bool InsertText(string aText)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertText(aText);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool InsertTable(int aRowCount, int aColCount)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertTable(aRowCount, aColCount);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool InsertImage(Bitmap aImage)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertImage(aImage);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool InsertGifImage(string aFile)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertGifImage(aFile);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool InsertLine(int aLineHeight)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertLine(aLineHeight);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool InsertItem(HCCustomItem aItem)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertItem(aItem);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool InsertItem(int aIndex, HCCustomItem aItem)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertItem(aIndex, aItem);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        /// <summary> 从当前位置后换行 </summary>
        public bool InsertBreak()
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertBreak();
            };

            return ActiveDataChangeByAction(vEvent);
        }

        /// <summary> 从当前位置后分页 </summary>
        public bool InsertPageBreak()
        {
            HCFunction vEvent = delegate()
            {
                return FPage.InsertPageBreak();
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool InsertDomain(HCDomainItem aMouldDomain)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertDomain(aMouldDomain);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        /// <summary> 当前选中的内容添加批注 </summary>
        public bool InsertAnnotate(string aTitle, string aText)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertAnnotate(aTitle, aText);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        //
        public bool ActiveTableInsertRowAfter(byte aRowCount)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.TableInsertRowAfter(aRowCount);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool ActiveTableInsertRowBefor(byte aRowCount)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.TableInsertRowBefor(aRowCount);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool ActiveTableDeleteCurRow()
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.ActiveTableDeleteCurRow();
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool ActiveTableSplitCurRow()
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.ActiveTableSplitCurRow();
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool ActiveTableSplitCurCol()
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.ActiveTableSplitCurCol();
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool ActiveTableInsertColAfter(byte aColCount)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.TableInsertColAfter(aColCount);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool ActiveTableInsertColBefor(byte aColCount)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.TableInsertColBefor(aColCount);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool ActiveTableDeleteCurCol()
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.ActiveTableDeleteCurCol();
            };

            return ActiveDataChangeByAction(vEvent);
        }

        //// <summary>  节坐标转换到指定页坐标 </summary>
        public void SectionCoordToPaper(int aPageIndex, int x, int y, ref int aPageX, ref int aPageY)
        {
            aPageX = x;// - vMarginLeft;

            int vPageFilmTop = 0;
            if (FViewModel == HCViewModel.hvmFilm)
                vPageFilmTop = GetPageTopFilm(aPageIndex);
            else
                vPageFilmTop = GetPageTop(aPageIndex);

            aPageY = y - vPageFilmTop;  // 映射到当前页面为原点的屏显起始位置(可为负数)
        }

        /// <summary> 为段应用对齐方式 </summary>
        /// <param name="aAlign">对方方式</param>
        public void ApplyParaAlignHorz(ParaAlignHorz aAlign)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyParaAlignHorz(aAlign);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void ApplyParaAlignVert(ParaAlignVert aAlign)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyParaAlignVert(aAlign);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void ApplyParaBackColor(Color aColor)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyParaBackColor(aColor);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void ApplyParaLineSpace(ParaLineSpaceMode aSpaceMode, Single aSpace)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyParaLineSpace(aSpaceMode, aSpace);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void ApplyParaLeftIndent(Single indent)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyParaLeftIndent(indent);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void ApplyParaRightIndent(Single indent)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyParaRightIndent(indent);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void ApplyParaFirstIndent(Single indent)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyParaFirstIndent(indent);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        /// <summary> 获取光标在Dtat中的位置信息并映射到指定页面 </summary>
        /// <param name="APageIndex">要映射到的页序号</param>
        /// <param name="aCaretInfo">光标位置信息</param>
        public virtual void GetPageCaretInfo(ref HCCaretInfo aCaretInfo)
        {
            int vMarginLeft = -1, vPageIndex = -1;
            if (FStyle.UpdateInfo.Draging)
                vPageIndex = FMousePageIndex;
            else
                vPageIndex = FActivePageIndex;

            if ((FActiveData.SelectInfo.StartItemNo < 0) || (vPageIndex < 0))
            {
                aCaretInfo.Visible = false;
                return;
            }

            aCaretInfo.PageIndex = vPageIndex;  // 鼠标点击处所在的页
            FActiveData.GetCaretInfoCur(ref aCaretInfo);

            if (aCaretInfo.Visible)
            {
                if (FActiveData == FPage)
                {
                    vMarginLeft = GetPageIndexByFormat(aCaretInfo.Y);  // 借用变量vMarginLeft表示页序号
                    if (vPageIndex != vMarginLeft)
                    {
                        vPageIndex = vMarginLeft;
                        SetActivePageIndex(vPageIndex);
                    }
                }

                if (FViewModel == HCViewModel.hvmFilm)
                {
                    vMarginLeft = GetPageMarginLeft(vPageIndex);
                    aCaretInfo.X = aCaretInfo.X + vMarginLeft;
                    aCaretInfo.Y = aCaretInfo.Y + GetPageTopFilm(vPageIndex);

                    if (FActiveData == FHeader)
                        aCaretInfo.Y = aCaretInfo.Y + GetHeaderPageDrawTop(); // 页在节中的Top位置
                    else
                        if (FActiveData == FPage)
                            aCaretInfo.Y = aCaretInfo.Y + GetHeaderAreaHeight() - GetPageDataFmtTop(vPageIndex);  // - 页起始数据在Data中的位置
                        else
                            if (FActiveData == FFooter)
                                aCaretInfo.Y = aCaretInfo.Y + FPaper.HeightPix - FPaper.MarginBottomPix;
                }
                else
                {
                    aCaretInfo.Y = aCaretInfo.Y + GetPageTop(vPageIndex);
                    if (FActiveData == FPage)
                        aCaretInfo.Y = aCaretInfo.Y - GetPageDataFmtTop(vPageIndex);  // - 页起始数据在Data中的位置
                }
            }
        }

        #region 绘制页眉数据
        private void PaintHeader(int vPaperDrawTop, int vPageDrawTop, int vPageDrawLeft, int vPageDrawRight, int vMarginLeft, int vMarginRight,
            int vHeaderAreaHeight, int aPageIndex, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            int vHeaderDataDrawTop = vPaperDrawTop + GetHeaderPageDrawTop();

            FHeader.PaintData(vPageDrawLeft, vHeaderDataDrawTop, vPageDrawRight,
                vPageDrawTop, Math.Max(vHeaderDataDrawTop, 0),
                Math.Min(vPageDrawTop, aPaintInfo.WindowHeight), 0, aCanvas, aPaintInfo);

            if (FOnPaintHeader != null)
            {
                int vDCState = GDI.SaveDC(aCanvas.Handle);
                try
                {
                    FOnPaintHeader(this, aPageIndex,
                        new RECT(vPageDrawLeft, vHeaderDataDrawTop, vPageDrawRight, vPageDrawTop), aCanvas, aPaintInfo);
                }
                finally
                {
                    GDI.RestoreDC(aCanvas.Handle, vDCState);
                }
            }
        }
        #endregion

        #region 绘制页脚数据
        private void PaintFooter(int vPaperDrawBottom, int vPageDrawLeft, int vPageDrawRight, int vPageDrawBottom, int vMarginLeft,
            int vMarginRight, int aPageIndex, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            FFooter.PaintData(vPageDrawLeft, vPageDrawRight, vPageDrawBottom, vPaperDrawBottom,
                Math.Max(vPageDrawBottom, 0), Math.Min(vPaperDrawBottom, aPaintInfo.WindowHeight),
                0, aCanvas, aPaintInfo);

            if (FOnPaintFooter != null)
            {
                int vDCState = GDI.SaveDC(aCanvas.Handle);
                try
                {
                    FOnPaintFooter(this, aPageIndex,
                        new RECT(vPageDrawLeft, vPageDrawBottom, vPageDrawRight, vPaperDrawBottom), aCanvas, aPaintInfo);
                }
                finally
                {
                    GDI.RestoreDC(aCanvas.Handle, vDCState);
                }
            }
        }
        #endregion

        #region 绘制页面数据
        void PaintPage(int vPageDrawLeft, int vPageDrawTop, int vPageDrawRight, int vPageDrawBottom,
            int vMarginLeft, int vMarginRight, int vHeaderAreaHeight, int vPageDataScreenTop, int vPageDataScreenBottom,
            int aPageIndex, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            if ((FPages[aPageIndex].StartDrawItemNo < 0) || (FPages[aPageIndex].EndDrawItemNo < 0))
                return;

            /* 绘制数据，把Data中指定位置的数据，绘制到指定的页区域中，并按照可显示出来的区域约束 }*/
            FPage.PaintData(vPageDrawLeft,  // 当前页数据要绘制到的Left
                vPageDrawTop,     // 当前页数据要绘制到的Top
                vPageDrawRight,   // 当前页数据要绘制到的Right
                vPageDrawBottom,  // 当前页数据要绘制的Bottom
                vPageDataScreenTop,     // 界面呈现当前页数据的Top位置
                vPageDataScreenBottom,  // 界面呈现当前页数据Bottom位置
                aPaintInfo.PageDataFmtTop,  // 指定从哪个位置开始的数据绘制到页数据起始位置
                FPages[aPageIndex].StartDrawItemNo,
                FPages[aPageIndex].EndDrawItemNo,
                aCanvas,
                aPaintInfo);

            if (FOnPaintPage != null)
            {
                int vDCState = GDI.SaveDC(aCanvas.Handle);
                try
                {
                    FOnPaintPage(this, aPageIndex,
                        new RECT(vPageDrawLeft, vPageDrawTop, vPageDrawRight, vPageDrawBottom), aCanvas, aPaintInfo);
                }
                finally
                {
                    GDI.RestoreDC(aCanvas.Handle, vDCState);
                }
            }
        }
        #endregion


        /// <summary> 绘制指定页到指定的位置，为配合打印，开放ADisplayWidth, ADisplayHeight参数 </summary>
        /// <param name="aPageIndex">要绘制的页码</param>
        /// <param name="aLeft">绘制X偏移</param>
        /// <param name="aTop">绘制Y偏移</param>
        /// <param name="aCanvas"></param>
        public void PaintPaper(int aPageIndex, int aLeft, int aTop, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            int vHeaderAreaHeight, vMarginLeft = -1, vMarginRight = -1,
              vPaperDrawLeft, vPaperDrawTop, vPaperDrawRight, vPaperDrawBottom,  
              vPageDrawLeft, vPageDrawTop, vPageDrawRight, vPageDrawBottom,  // 页区域各位置
              vPageDataScreenTop, vPageDataScreenBottom,  // 页数据屏幕位置
              vScaleWidth, vScaleHeight;

            IntPtr vPaintRegion = IntPtr.Zero;
            RECT vClipBoxRect = new RECT();

            vScaleWidth = (int)Math.Round(aPaintInfo.WindowWidth / aPaintInfo.ScaleX);
            vScaleHeight = (int)Math.Round(aPaintInfo.WindowHeight / aPaintInfo.ScaleY);

            if (aPaintInfo.ViewModel == HCViewModel.hvmFilm)
            {
                vPaperDrawLeft = aLeft;
                vPaperDrawRight = aLeft + FPaper.WidthPix;
                vPaperDrawTop = aTop;
                vPaperDrawBottom = vPaperDrawTop + FPaper.HeightPix;

                GetPageMarginLeftAndRight(aPageIndex, ref vMarginLeft, ref vMarginRight);  // 获取页左右边距绘制位置
                vPageDrawLeft = vPaperDrawLeft + vMarginLeft;
                vPageDrawRight = vPaperDrawRight - vMarginRight;
                vHeaderAreaHeight = GetHeaderAreaHeight();  // 页眉区域实际高 = 页眉数据顶部偏移 + 内容高度，大于上边距时以此为准
                vPageDrawTop = vPaperDrawTop + vHeaderAreaHeight;  // 映射到当前页面左上角为原点的起始位置(可为负数)
                vPageDrawBottom = vPaperDrawBottom - FPaper.MarginBottomPix;  // 页面结束位置(可为负数)
            }
            else
            {
                vPaperDrawLeft = aLeft;
                vPaperDrawRight = aLeft + GetPageWidth();
                vPaperDrawTop = aTop;
                vPaperDrawBottom = aTop + GetPageHeight();

                vPageDrawLeft = vPaperDrawLeft;
                vPageDrawRight = vPaperDrawRight;
                vHeaderAreaHeight = 0;
                vMarginLeft = 0;
                vMarginRight = 0;
                vPageDrawTop = vPaperDrawTop;  // 映射到当前页面左上角为原点的起始位置(可为负数)
                vPageDrawBottom = vPaperDrawBottom - 1;  // 页面结束位置(可为负数)
            }

            // 当前页数据能显示出来的区域边界
            vPageDataScreenTop = Math.Max(vPageDrawTop, 0);
            vPageDataScreenBottom = Math.Min(vPageDrawBottom, vScaleHeight);

            aPaintInfo.PageDataFmtTop = GetPageDataFmtTop(aPageIndex);

            GDI.GetClipBox(aCanvas.Handle, ref vClipBoxRect);  // 保存当前的绘图区域

            if (!aPaintInfo.Print)
            {

                #region 非打印时填充页面背景
                aCanvas.Brush.Color = FStyle.BackgroudColor;
                aCanvas.FillRect(new RECT(vPaperDrawLeft, vPaperDrawTop, Math.Min(vPaperDrawRight, vScaleWidth),  // 约束边界
                    Math.Min(vPaperDrawBottom, vScaleHeight)));
                #endregion

                if (aPaintInfo.ViewModel == HCViewModel.hvmFilm)
                {
                    #region 页眉边距指示符
                    if (vPageDrawTop > 0)
                    {
                        if (vHeaderAreaHeight > FPaper.MarginTopPix)
                        {
                            aCanvas.Pen.BeginUpdate();
                            try
                            {
                                aCanvas.Pen.Style = HCPenStyle.psDot;
                                aCanvas.Pen.Color = Color.Gray;
                            }
                            finally
                            {
                                aCanvas.Pen.EndUpdate();
                            }

                            aPaintInfo.DrawNoScaleLine(aCanvas, new Point[2] { new Point(vPageDrawLeft, vPageDrawTop - 1), new Point(vPageDrawRight, vPageDrawTop - 1) });
                        }

                        if (FActiveData == FHeader)
                        {
                            aCanvas.Pen.BeginUpdate();
                            try
                            {
                                aCanvas.Pen.Width = 1;
                                aCanvas.Pen.Color = Color.Blue;
                                aCanvas.Pen.Style = HCPenStyle.psSolid;
                            }
                            finally
                            {
                                aCanvas.Pen.EndUpdate();
                            }

                            aCanvas.DrawLine(vPageDrawLeft, vPageDrawTop, vPageDrawRight, vPageDrawTop);
                        }
                        else
                        {
                            aCanvas.Pen.BeginUpdate();
                            try
                            {
                                aCanvas.Pen.Width = 1;
                                aCanvas.Pen.Color = Color.Gray;
                                aCanvas.Pen.Style = HCPenStyle.psSolid;
                            }
                            finally
                            {
                                aCanvas.Pen.EndUpdate();
                            }
                        }

                        // 左上， 左-原-上
                        aPaintInfo.DrawNoScaleLine(aCanvas, new Point[3] { new Point(vPageDrawLeft - HC.PMSLineHeight, vPageDrawTop),
                            new Point(vPageDrawLeft, vPageDrawTop), new Point(vPageDrawLeft, vPageDrawTop - HC.PMSLineHeight) });
                        // 右上，右-原-上
                        aPaintInfo.DrawNoScaleLine(aCanvas, new Point[3] { new Point(vPageDrawRight + HC.PMSLineHeight, vPageDrawTop),
                            new Point(vPageDrawRight, vPageDrawTop), new Point(vPageDrawRight, vPageDrawTop - HC.PMSLineHeight) });
                    }
                    #endregion

                    #region 页脚边距指示符
                    if (vPageDrawBottom < aPaintInfo.WindowHeight)
                    {
                        if (FActiveData == FFooter)
                        {
                            aCanvas.Pen.BeginUpdate();
                            try
                            {
                                aCanvas.Pen.Width = 1;
                                aCanvas.Pen.Color = Color.Blue;
                                aCanvas.Pen.Style = HCPenStyle.psSolid;
                            }
                            finally
                            {
                                aCanvas.Pen.EndUpdate();
                            }

                            aCanvas.DrawLine(vPageDrawLeft, vPageDrawBottom, vPageDrawRight, vPageDrawBottom);
                        }
                        else
                        {
                            aCanvas.Pen.BeginUpdate();
                            try
                            {
                                aCanvas.Pen.Width = 1;
                                aCanvas.Pen.Color = Color.Gray;
                                aCanvas.Pen.Style = HCPenStyle.psSolid;
                            }
                            finally
                            {
                                aCanvas.Pen.EndUpdate();
                            }
                        }

                        // 左下，左-原-下
                        aPaintInfo.DrawNoScaleLine(aCanvas, new Point[3] { new Point(vPageDrawLeft - HC.PMSLineHeight, vPageDrawBottom),
                            new Point(vPageDrawLeft, vPageDrawBottom), new Point(vPageDrawLeft, vPageDrawBottom + HC.PMSLineHeight) });
                        // 右下，右-原-下
                        aPaintInfo.DrawNoScaleLine(aCanvas, new Point[3] { new Point(vPageDrawRight + HC.PMSLineHeight, vPageDrawBottom),
                            new Point(vPageDrawRight, vPageDrawBottom), new Point(vPageDrawRight, vPageDrawBottom + HC.PMSLineHeight) });
                    }
                    #endregion
                }
                else
                {
                    if (vPageDrawBottom < aPaintInfo.WindowHeight)  // 页脚结束可显示
                    {
                        aCanvas.Pen.BeginUpdate();
                        try
                        {
                            aCanvas.Pen.Width = 1;
                            aCanvas.Pen.Color = Color.Gray;
                            aCanvas.Pen.Style = HCPenStyle.psDashDot;
                        }
                        finally
                        {
                            aCanvas.Pen.EndUpdate();
                        }

                        aCanvas.DrawLine(vPaperDrawLeft, vPageDrawBottom, vPaperDrawRight, vPageDrawBottom);
                    }
                }
            }

            if (FOnPaintPaperBefor != null)  // 公开页面绘制前事件
            {
                int vDCState = GDI.SaveDC(aCanvas.Handle);
                try
                {
                    FOnPaintPaperBefor(this, aPageIndex,
                        new RECT(vPaperDrawLeft, vPaperDrawTop, vPaperDrawRight, vPaperDrawBottom), aCanvas, aPaintInfo);
                }
                finally
                {
                    GDI.RestoreDC(aCanvas.Handle, vDCState);
                }
            }

            if (aPaintInfo.ViewModel == HCViewModel.hvmFilm)
            {
                #region 绘制页眉
                if (vPageDrawTop > 0)
                {
                    vPaintRegion = (IntPtr)GDI.CreateRectRgn(aPaintInfo.GetScaleX(vPageDrawLeft),
                        Math.Max(aPaintInfo.GetScaleY(vPaperDrawTop + FHeaderOffset), 0),
                        aPaintInfo.GetScaleX(vPaperDrawRight),  // 表格有时候会拖宽到页面外面vPageDrawRight
                        Math.Min(aPaintInfo.GetScaleY(vPageDrawTop), aPaintInfo.WindowHeight));

                    try
                    {
                        GDI.SelectClipRgn(aCanvas.Handle, vPaintRegion);  // 设置绘制有效区域
                        PaintHeader(vPaperDrawTop, vPageDrawTop, vPageDrawLeft, vPageDrawRight, vMarginLeft, vMarginRight,
                            vHeaderAreaHeight, aPageIndex, aCanvas, aPaintInfo);
                    }
                    finally
                    {
                        GDI.DeleteObject(vPaintRegion);
                    }
                }
                #endregion

                #region 绘制页脚
                if (aPaintInfo.GetScaleY(vPageDrawBottom) < aPaintInfo.WindowHeight)  // 页脚可显示
                {
                    vPaintRegion = (IntPtr)GDI.CreateRectRgn(aPaintInfo.GetScaleX(vPageDrawLeft),
                        Math.Max(aPaintInfo.GetScaleY(vPageDrawBottom), 0),
                        aPaintInfo.GetScaleX(vPaperDrawRight),  // 表格有时候会拖宽到页面外面vPageDrawRight
                        Math.Min(aPaintInfo.GetScaleY(vPaperDrawBottom), aPaintInfo.WindowHeight));

                    try
                    {
                        GDI.SelectClipRgn(aCanvas.Handle, vPaintRegion);  // 设置绘制有效区域
                        PaintFooter(vPaperDrawBottom, vPageDrawLeft, vPageDrawRight, vPageDrawBottom, vMarginLeft, vMarginRight,
                            aPageIndex, aCanvas, aPaintInfo);
                    }
                    finally
                    {
                        GDI.DeleteObject(vPaintRegion);
                    }
                }
                #endregion
            }

            #region 绘制页面
            if (vPageDataScreenBottom > vPageDataScreenTop)  // 能露出数据则绘制当前页，绘制正文
            {
                vPaintRegion = (IntPtr)GDI.CreateRectRgn(aPaintInfo.GetScaleX(vPaperDrawLeft),  // 有行号或行指示符所以从纸张左边
                        aPaintInfo.GetScaleY(Math.Max(vPageDrawTop, vPageDataScreenTop)),
                        aPaintInfo.GetScaleX(vPaperDrawRight),  // 表格有时候会拖宽到页面外面vPageDrawRight
                        aPaintInfo.GetScaleY(Math.Min(vPageDrawBottom, vPageDataScreenBottom)) + 1);

                try
                {
                    GDI.SelectClipRgn(aCanvas.Handle, vPaintRegion);  // 设置绘制有效区域
                    PaintPage(vPageDrawLeft, vPageDrawTop, vPageDrawRight, vPageDrawBottom,
                        vMarginLeft, vMarginRight, vHeaderAreaHeight, vPageDataScreenTop, vPageDataScreenBottom,
                        aPageIndex, aCanvas, aPaintInfo);
                }
                finally
                {
                    GDI.DeleteObject(vPaintRegion);
                }
            }
            #endregion

            // 恢复区域，准备给整页绘制用(各部分浮动Item)
            vPaintRegion = (IntPtr)GDI.CreateRectRgn(
                aPaintInfo.GetScaleX(vPaperDrawLeft),
                aPaintInfo.GetScaleX(vPaperDrawTop),
                aPaintInfo.GetScaleX(vPaperDrawRight),
                aPaintInfo.GetScaleX(vPaperDrawBottom));

            try
            {
                GDI.SelectClipRgn(aCanvas.Handle, vPaintRegion);

                FHeader.PaintFloatItems(aPageIndex, vPageDrawLeft,
                    vPaperDrawTop + GetHeaderPageDrawTop(), 0, aCanvas, aPaintInfo);

                FFooter.PaintFloatItems(aPageIndex, vPageDrawLeft,
                    vPageDrawBottom, 0, aCanvas, aPaintInfo);

                FPage.PaintFloatItems(aPageIndex, vPageDrawLeft,  // 当前页绘制到的Left
                    vPageDrawTop,     // 当前页绘制到的Top
                    GetPageDataFmtTop(aPageIndex),  // 指定从哪个位置开始的数据绘制到页数据起始位置
                    aCanvas,
                    aPaintInfo);
            }
            finally
            {
                GDI.DeleteObject(vPaintRegion);
            }

            // 恢复区域，准备给外部绘制用
            vPaintRegion = (IntPtr)GDI.CreateRectRgn(
                aPaintInfo.GetScaleX(vClipBoxRect.Left),
                aPaintInfo.GetScaleX(vClipBoxRect.Top),
                aPaintInfo.GetScaleX(vClipBoxRect.Right),
                aPaintInfo.GetScaleX(vClipBoxRect.Bottom));
            try
            {
                GDI.SelectClipRgn(aCanvas.Handle, vPaintRegion);
            }
            finally
            {
                GDI.DeleteObject(vPaintRegion);
            }

            if (FOnPaintPaperAfter != null)  // 公开页面绘制后事件
            {
                int vDCState = GDI.SaveDC(aCanvas.Handle);
                try
                {
                    FOnPaintPaperAfter(this, aPageIndex,
                        new RECT(vPaperDrawLeft, vPaperDrawTop, vPaperDrawRight, vPaperDrawBottom),
                        aCanvas, aPaintInfo);
                }
                finally
                {
                    GDI.RestoreDC(aCanvas.Handle, vDCState);
                }
            }
        }

        public virtual void Clear()
        {
            FHeader.Clear();
            FFooter.Clear();
            FPage.Clear();
            FPages.ClearEx();
            FActivePageIndex = 0;
        }

        public virtual void MouseDown(MouseEventArgs e)
        {
            bool vChangeActiveData = false;
            HCCustomData vOldTopData = FActiveData.GetTopLevelData();
            int vPageIndex = GetPageIndexByFilm(e.Y);  // 鼠标点击处所在的页(和光标所在页可能并不是同一页，如表格跨页时，空单元格第二页点击时，光标回前一页)
            if (FActivePageIndex != vPageIndex)
                FActivePageIndex = vPageIndex;

            int vX = -1, vY = -1;
            SectionCoordToPaper(FActivePageIndex, e.X, e.Y, ref vX, ref vY);  // X，Y转换到指定页的坐标vX,vY
            HCSectionData vNewActiveData = GetSectionDataAt(vX, vY);

            if ((vNewActiveData != FActiveData) && (e.Clicks == 2))
            {
                SetActiveData(vNewActiveData);
                vChangeActiveData = true;
            }

            #region 有FloatItem时短路
            if (FActiveData.FloatItems.Count > 0)
            {
                int vX2 = vX;  // 使用另外的变量，防止FloatItem不处理时影响下面的正常计算
                int vY2 = vY;

                PaperCoordToData(FActivePageIndex, FActiveData, ref vX2, ref vY2, false);
                if (FActiveData == FPage)
                    vY2 = vY2 + GetPageDataFmtTop(FActivePageIndex);

                MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX2, vY2, e.Delta);
                if (FActiveData.MouseDownFloatItem(vMouseArgs))
                    return;
            }
            #endregion

            PaperCoordToData(FActivePageIndex, FActiveData, ref vX, ref vY);

            if (FActiveData == FPage)
                vY = vY + GetPageDataFmtTop(FActivePageIndex);

            if ((e.Clicks == 2) && (!vChangeActiveData))
                FActiveData.DblClick(vX, vY);
            else
            {
                MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
                FActiveData.MouseDown(vMouseArgs);
            }

            if (vOldTopData != FActiveData.GetTopLevelData())
            {
                if (FOnChangeTopLevelData != null)
                    FOnChangeTopLevelData(this, null);
            }
        }

        public virtual void MouseMove(MouseEventArgs e)
        {
            int vMarginLeft = -1, vMarginRight = -1;

            GetPageMarginLeftAndRight(FMousePageIndex, ref vMarginLeft, ref vMarginRight);

            if (e.X < vMarginLeft)
                HC.GCursor = Cursors.Default;
            else
                if (e.X > FPaper.WidthPix - vMarginRight)
                    HC.GCursor = Cursors.Default;
                else
                    HC.GCursor = Cursors.IBeam;

            FMousePageIndex = GetPageIndexByFilm(e.Y);
            //Assert(FMousePageIndex >= 0, "不应该出现鼠标移动到空页面上的情况！");
            //if FMousePageIndex < 0 then Exit;  应该永远不会出现

            int vX = -1, vY = -1;
            MouseEventArgs vEventArgs = null;

            #region 有FloatItem时短路
            if (FActiveData.FloatItems.Count > 0)
            {
                if ((e.Button == MouseButtons.Left) && (FActiveData.FloatItemIndex >= 0))
                {
                    if (!FActiveData.ActiveFloatItem.Resizing)
                        FActiveData.ActiveFloatItem.PageIndex = FMousePageIndex;
                }

                if (FActiveData == FPage)
                {
                    if ((FActiveData.FloatItemIndex >= 0) && (FActiveData.ActiveFloatItem.Resizing))
                    {
                        SectionCoordToPaper(FActiveData.ActiveFloatItem.PageIndex, e.X, e.Y, ref vX, ref vY);
                        PaperCoordToData(FActiveData.ActiveFloatItem.PageIndex, FActiveData, ref vX, ref vY, false);
                        vY = vY + GetPageDataFmtTop(FActiveData.ActiveFloatItem.PageIndex);
                    }
                    else
                    {
                        SectionCoordToPaper(FMousePageIndex, e.X, e.Y, ref vX, ref vY);
                        PaperCoordToData(FMousePageIndex, FActiveData, ref vX, ref vY, false);
                        vY = vY + GetPageDataFmtTop(FMousePageIndex);
                    }
                }
                else  // FloatItem在Header或Footer
                {
                    if ((FActiveData.FloatItemIndex >= 0) && (FActiveData.ActiveFloatItem.Resizing))
                    {
                        SectionCoordToPaper(FActivePageIndex, e.X, e.Y, ref vX, ref vY);
                        PaperCoordToData(FActivePageIndex, FActiveData, ref vX, ref vY, false);
                    }
                    else
                    {
                        SectionCoordToPaper(FMousePageIndex, e.X, e.Y, ref vX, ref vY);
                        PaperCoordToData(FMousePageIndex, FActiveData, ref vX, ref vY, false);
                    }
                }

                vEventArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
                if (FActiveData.MouseMoveFloatItem(vEventArgs))
                    return;
            }
            #endregion

            SectionCoordToPaper(FMousePageIndex, e.X, e.Y, ref vX, ref vY);

            HCSectionData vMoveData = GetSectionDataAt(vX, vY);
            if (vMoveData != FMoveData)
            {
                if (FMoveData != null)
                    FMoveData.MouseLeave();

                FMoveData = vMoveData;
            }

            PaperCoordToData(FMousePageIndex, FActiveData, ref vX, ref vY, e.Button != MouseButtons.None);

            if (FActiveData == FPage)
                vY = vY + GetPageDataFmtTop(FMousePageIndex);

            vEventArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
            FActiveData.MouseMove(vEventArgs);
        }

        public virtual void MouseUp(MouseEventArgs e)
        {
            int vPageIndex = GetPageIndexByFilm(e.Y);

            int vX = -1, vY = -1;
            MouseEventArgs vEventArgs = null;

            #region 有FloatItem时短路
            if ((FActiveData.FloatItems.Count > 0) && (FActiveData.FloatItemIndex >= 0))
            {
                if (FActiveData == FPage)
                {
                    SectionCoordToPaper(FActiveData.ActiveFloatItem.PageIndex, e.X, e.Y, ref vX, ref vY);
                    PaperCoordToData(FActiveData.ActiveFloatItem.PageIndex, FActiveData, ref vX, ref vY, false);
                    vY = vY + GetPageDataFmtTop(FActiveData.ActiveFloatItem.PageIndex);
                }
                else  // FloatItem在Header或Footer
                {
                    SectionCoordToPaper(vPageIndex, e.X, e.Y, ref vX, ref vY);
                    PaperCoordToData(vPageIndex, FActiveData, ref vX, ref vY, false);
                }

                vEventArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
                if (FActiveData.MouseUpFloatItem(vEventArgs))
                    return;
            }
            #endregion

            SectionCoordToPaper(vPageIndex, e.X, e.Y, ref vX, ref vY);
            PaperCoordToData(vPageIndex, FActiveData, ref vX, ref vY);

            if (FActiveData == FPage)
                vY = vY + GetPageDataFmtTop(vPageIndex);

            // RectItem的缩放在MouseUp中处理，所以需要判断是否需要改变
            if (FActiveData.SelectedResizing())
            {
                HCFunction vEvent = delegate()
                {
                    vEventArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
                    FActiveData.MouseUp(vEventArgs);
                    return true;
                };

                ActiveDataChangeByAction(vEvent);
            }
            else
            {
                vEventArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
                FActiveData.MouseUp(vEventArgs);
            }
        }

        /// <summary> 某页在整个节中的Top位置 </summary>
        /// <param name="aPageIndex"></param>
        /// <returns></returns>
        public int GetPageTopFilm(int aPageIndex)
        {
            int Result = FPagePadding;
            for (int i = 0; i <= aPageIndex - 1; i++)
                Result = Result + FPaper.HeightPix + FPagePadding;  // 每一页和其上面的分隔计为一整个处理单元

            return Result;
        }

        public int GetPageTop(int aPageIndex)
        {
            int Result = 0;
            int vPageHeight = GetPageHeight();
            for (int i = 0; i <= aPageIndex - 1; i++)
                Result = Result + vPageHeight;

            return Result;
        }

        /// <summary> 返回指定页数据起始位置在整个Data中的Top，注意 20161216001 </summary>
        /// <param name="aPageIndex"></param>
        /// <returns></returns>
        public int GetPageDataFmtTop(int aPageIndex)
        {
            int Result = 0;
            if (aPageIndex > 0)
            {
                int vPageHeight = GetPageHeight();

                for (int i = 0; i <= aPageIndex - 1; i++)
                    Result = Result + vPageHeight;
            }

            return Result;
        }

        /// <summary> 页眉内容在页中绘制时的起始位置 </summary>
        /// <returns></returns>
        public int GetHeaderPageDrawTop()
        {
            int Result = FHeaderOffset;
            int vHeaderHeight = FHeader.Height;
            if (vHeaderHeight < (FPaper.MarginTopPix - FHeaderOffset))
                Result = Result + (FPaper.MarginTopPix - FHeaderOffset - vHeaderHeight) / 2;

            return Result;
        }

        public int GetPageMarginLeft(int aPageIndex)
        {
            int Result = -1, vMarginRight = -1;
            GetPageMarginLeftAndRight(aPageIndex, ref Result, ref vMarginRight);

            return Result;
        }

        /// <summary> 根据页面对称属性，获取指定页的左右边距 </summary>
        /// <param name="aPageIndex"></param>
        /// <param name="aMarginLeft"></param>
        /// <param name="aMarginRight"></param>
        public void GetPageMarginLeftAndRight(int aPageIndex, ref int aMarginLeft, ref int aMarginRight)
        {
            if (FSymmetryMargin && HC.IsOdd(aPageIndex))
            {
                aMarginLeft = FPaper.MarginRightPix;
                aMarginRight = FPaper.MarginLeftPix;
            }
            else
            {
                aMarginLeft = FPaper.MarginLeftPix;
                aMarginRight = FPaper.MarginRightPix;
            }
        }

        #region BuildSectionPages内部方法
        private void _FormatNewPage(ref int vPageIndex, int aPrioEndDItemNo, int aNewStartDItemNo)
        {
            FPages[vPageIndex].EndDrawItemNo = aPrioEndDItemNo;
            HCPage vPage = new HCPage();
            vPage.StartDrawItemNo = aNewStartDItemNo;
            FPages.Insert(vPageIndex + 1, vPage);
            vPageIndex++;
        }

        private void _RectItemCheckPage(int ADrawItemNo, int AStartSeat,
            int vPageHeight, HCCustomRectItem vRectItem, ref int vPageIndex,
            ref int vBreakSeat, ref int vSuplus, ref int vPageDataFmtTop, ref int vPageDataFmtBottom)
        {
            int vFmtHeightInc = -1, vFmtOffset = -1;

            //if (FPage.GetDrawItemStyle(ADrawItemNo) == HCStyle.PageBreak)
            //{
            //    vFmtOffset = vPageDataFmtBottom - FPage.DrawItems[ADrawItemNo].Rect.Top;

            //    vSuplus = vSuplus + vFmtOffset;
            //    if (vFmtOffset > 0)
            //        HC.OffsetRect(ref FPage.DrawItems[ADrawItemNo].Rect, 0, vFmtOffset);

            //    vPageDataFmtTop = vPageDataFmtBottom;
            //    vPageDataFmtBottom = vPageDataFmtTop + vPageHeight;

            //    _FormatNewPage(ref vPageIndex, ADrawItemNo - 1, ADrawItemNo);  // 新建页
            //}
            //else
                if (FPage.DrawItems[ADrawItemNo].Rect.Bottom > vPageDataFmtBottom)
                {
                    if ((FPages[vPageIndex].StartDrawItemNo == ADrawItemNo)
                        && (AStartSeat == 0)
                        && (!vRectItem.CanPageBreak))
                    {
                        vFmtHeightInc = vPageDataFmtBottom - FPage.DrawItems[ADrawItemNo].Rect.Bottom;
                        vSuplus = vSuplus + vFmtHeightInc;
                        FPage.DrawItems[ADrawItemNo].Rect.Bottom =  // 扩充格式化高度
                            FPage.DrawItems[ADrawItemNo].Rect.Bottom + vFmtHeightInc;
                        vRectItem.Height = vRectItem.Height + vFmtHeightInc;  // 是在这里处理呢还是在RectItem内部更合适？

                        return;
                    }

                    RECT vDrawRect = FPage.DrawItems[ADrawItemNo].Rect;

                    //if vSuplus = 0 then  // 第一次计算分页
                    HC.InflateRect(ref vDrawRect, 0, -FPage.GetLineBlankSpace(ADrawItemNo) / 2);  // 减掉行间距，为了达到去年行间距能放下不换页的效果

                    vRectItem.CheckFormatPageBreak(  // 去除行间距后，判断表格跨页位置
                        FPages.Count - 1,
                        vDrawRect.Top,  // 表格的顶部位置 FPageData.DrawItems[ADrawItemNo].Rect.Top,
                        vDrawRect.Bottom,  // 表格的底部位置 FPageData.DrawItems[ADrawItemNo].Rect.Bottom,
                        vPageDataFmtTop,
                        vPageDataFmtBottom,  // 当前页的数据底部位置
                        AStartSeat,  // 起始位置
                        ref vBreakSeat,  // 当前页分页的行(位置)
                        ref vFmtOffset,  // 当前RectItem为了避开分页位置整体向下偏移的高度
                        ref vFmtHeightInc  // 当前行各列为了避开分页位置单元格内容额外偏移的最大高度
                        );

                    if (vBreakSeat < 0)
                    {
                        vSuplus = vSuplus + vPageDataFmtBottom - vDrawRect.Bottom;
                    }
                    else  // vBreakSeat >= 0 从vBreakSeat位置跨页
                        if (vFmtOffset > 0)
                        {
                            vFmtOffset = vFmtOffset + FPage.GetLineBlankSpace(ADrawItemNo) / 2;  // 整体向下移动增加上面减掉的行间距
                            vSuplus = vSuplus + vFmtOffset + vFmtHeightInc;

                            HC.OffsetRect(ref FPage.DrawItems[ADrawItemNo].Rect, 0, vFmtOffset);

                            vPageDataFmtTop = vPageDataFmtBottom;
                            vPageDataFmtBottom = vPageDataFmtTop + vPageHeight;
                            _FormatNewPage(ref vPageIndex, ADrawItemNo - 1, ADrawItemNo);  // 新建页
                            _RectItemCheckPage(ADrawItemNo, AStartSeat, vPageHeight,
                                vRectItem, ref vPageIndex, ref vBreakSeat, ref vSuplus, ref vPageDataFmtTop, ref vPageDataFmtBottom);
                        }
                        else  // 跨页，但未整体下移
                        {
                            vSuplus = vSuplus + vFmtHeightInc;
                            FPage.DrawItems[ADrawItemNo].Rect.Bottom =  // 扩充格式化高度
                                FPage.DrawItems[ADrawItemNo].Rect.Bottom + vFmtHeightInc;
                            vRectItem.Height = vRectItem.Height + vFmtHeightInc;  // 是在这里处理呢还是在RectItem内部更合适？

                            vPageDataFmtTop = vPageDataFmtBottom;
                            vPageDataFmtBottom = vPageDataFmtTop + vPageHeight;
                            _FormatNewPage(ref vPageIndex, ADrawItemNo, ADrawItemNo);  // 新建页

                            _RectItemCheckPage(ADrawItemNo, AStartSeat, vPageHeight,
                                vRectItem, ref vPageIndex, ref vBreakSeat, ref vSuplus, ref vPageDataFmtTop, ref vPageDataFmtBottom);  // 从分页位置后面继续检查是否分页
                        }
                }
        }

        private void _FormatRectItemCheckPageBreak(int ADrawItemNo, int vPageHeight,
            ref int vPageIndex, ref int vPageDataFmtTop, ref int vPageDataFmtBottom)
        {
            int vSuplus = 0;  // 所有因换页向下偏移量的总和
            int vBreakSeat = 0;  // 分页位置，不同RectItem的含义不同，表格表示 vBreakRow

            HCCustomRectItem vRectItem = FPage.Items[FPage.DrawItems[ADrawItemNo].ItemNo] as HCCustomRectItem;

            vRectItem.CheckFormatPageBreakBefor();
            _RectItemCheckPage(ADrawItemNo, 0, vPageHeight, vRectItem,
                ref vPageIndex, ref vBreakSeat, ref vSuplus, ref vPageDataFmtTop, ref vPageDataFmtBottom);  // 从最开始位置，检测表格各行内容是否能显示在当前页

            if (vSuplus != 0)
            {
                for (int i = ADrawItemNo + 1; i <= FPage.DrawItems.Count - 1; i++)
                    HC.OffsetRect(ref FPage.DrawItems[i].Rect, 0, vSuplus);
            }
        }

        private void _FormatTextItemCheckPageBreak(int vPageHeight, int ADrawItemNo,
            ref int vPageDataFmtTop, ref int vPageDataFmtBottom, ref int vPageIndex)
        {
            //if not DrawItems[ADrawItemNo].LineFirst then Exit; // 注意如果文字环绕时这里就不能只判断行第1个
            if (FPage.DrawItems[ADrawItemNo].Rect.Bottom > vPageDataFmtBottom)
            {
                int vH = vPageDataFmtBottom - FPage.DrawItems[ADrawItemNo].Rect.Top;
                for (int i = ADrawItemNo; i <= FPage.DrawItems.Count - 1; i++)
                    HC.OffsetRect(ref FPage.DrawItems[i].Rect, 0, vH);

                vPageDataFmtTop = vPageDataFmtBottom;
                vPageDataFmtBottom = vPageDataFmtTop + vPageHeight;
                _FormatNewPage(ref vPageIndex, ADrawItemNo - 1, ADrawItemNo); // 新建页
            }
        }
        #endregion

        /// <summary> 从正文指定Item开始重新计算页 </summary>
        /// <param name="aStartItemNo"></param>
        public void BuildSectionPages(int aStartDrawItemNo)
        {
            int vPrioDrawItemNo = aStartDrawItemNo;
            HCPage vPage = null;

            while (vPrioDrawItemNo > 0)
            {
                if (FPage.DrawItems[vPrioDrawItemNo].LineFirst)
                    break;

                vPrioDrawItemNo--;
            }

            vPrioDrawItemNo--;  // 上一行末尾
            
            int vPageIndex = 0;
            if (vPrioDrawItemNo > 0)
            {
                for (int i = FPages.Count - 1; i >= 0; i--)  // 对于跨页的，按最后位置所在页，所以倒
                {
                    vPage = FPages[i];
                    if ((vPrioDrawItemNo >= vPage.StartDrawItemNo)
                        && (vPrioDrawItemNo <= vPage.EndDrawItemNo))
                    {
                        vPageIndex = i;
                        break;
                    }
                }
            }

            FPages.RemoveRange(vPageIndex + 1, FPages.Count - vPageIndex - 1);  // 删除当前页后面的，准备格式化

            if (FPages.Count == 0)
            {
                vPage = new HCPage();
                vPage.StartDrawItemNo = 0;
                FPages.Add(vPage);
                vPageIndex = 0;
            }

            int vPageDataFmtTop = GetPageDataFmtTop(vPageIndex);
            int vPageHeight = GetPageHeight();
            int vPageDataFmtBottom = vPageDataFmtTop + vPageHeight;
            int vFmtPageOffset = 0;
            HCCustomItem vItem = null;
            for (int i = vPrioDrawItemNo + 1; i <= FPage.DrawItems.Count - 1; i++)
            {
                vItem = FPage.Items[FPage.DrawItems[i].ItemNo];
                if (vItem.PageBreak && (vItem.FirstDItemNo == i))
                {
                    vFmtPageOffset = vPageDataFmtBottom - FPage.DrawItems[i].Rect.Top;
                    if (vFmtPageOffset > 0)
                    {
                        for (int j = i; j <= FPage.DrawItems.Count - 1; j++)
                            FPage.DrawItems[j].Rect.Offset(0, vFmtPageOffset);
                    }

                    vPageDataFmtTop = vPageDataFmtBottom;
                    vPageDataFmtBottom = vPageDataFmtTop + vPageHeight;

                    _FormatNewPage(ref vPageIndex, i - 1, i);
                }

                if (FPage.DrawItems[i].LineFirst)
                {
                    if (FPage.GetDrawItemStyle(i) < HCStyle.Null)
                        _FormatRectItemCheckPageBreak(i, vPageHeight, ref vPageIndex, ref vPageDataFmtTop, ref vPageDataFmtBottom);
                    else
                        _FormatTextItemCheckPageBreak(vPageHeight, i, ref vPageDataFmtTop, ref vPageDataFmtBottom, ref vPageIndex);
                }
            }

            FPages[vPageIndex].EndDrawItemNo = FPage.DrawItems.Count - 1;
            SetActivePageIndex(GetPageIndexByCurrent());

            for (int i = FPage.FloatItems.Count - 1; i >= 0; i--)  // 正文中删除页序号超过页总数的FloatItem
            {
                if (FPage.FloatItems[i].PageIndex > FPages.Count - 1)
                    FPage.FloatItems.RemoveAt(i);
            }
        }

        public bool DeleteSelected()
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.DeleteSelected();
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public void DisSelect()
        {
            FActiveData.GetTopLevelData().DisSelect();
        }

        public bool DeleteActiveDomain()
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.DeleteActiveDomain();
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool DeleteActiveDataItems(int aStartNo, int aEndNo, bool aKeepPara)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.DeleteActiveDataItems(aStartNo, aEndNo, aKeepPara);
                return true;
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool MergeTableSelectCells()
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.MergeTableSelectCells();
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool TableApplyContentAlign(HCContentAlign aAlign)
        {
            HCFunction vEvent = delegate ()
            {
                return FActiveData.TableApplyContentAlign(aAlign);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public void ReFormatActiveParagraph()
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ReFormatActiveParagraph();
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public void ReFormatActiveItem()
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ReFormatActiveItem();
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        public int GetHeaderAreaHeight()
        {
            int Result = FHeaderOffset + FHeader.Height;
            if (Result < FPaper.MarginTopPix)
                Result = FPaper.MarginTopPix;

            return Result;
        }

        public int GetPageHeight()
        {
            return FPaper.HeightPix - GetHeaderAreaHeight() - FPaper.MarginBottomPix;
        }

        public int GetPageWidth()
        {
            return FPaper.WidthPix - FPaper.MarginLeftPix - FPaper.MarginRightPix;
        }

        public int GetFilmHeight()
        {
            if (FViewModel == HCViewModel.hvmFilm)
                return FPages.Count * (FPagePadding + FPaper.HeightPix);
            else
                return FPages.Count * GetPageHeight();
        }

        public int GetFilmWidth()
        {
            return FPages.Count * (FPagePadding + FPaper.WidthPix);
        }

        /// <summary> 标记样式是否在用或删除不使用的样式后修正样式序号 </summary>
        /// <param name="aMark">True:标记样式是否在用，Fasle:修正原样式因删除不使用样式后的新序号</param>
        public void MarkStyleUsed(bool aMark, HashSet<SectionArea> aParts)
        {
            if (aParts.Contains(SectionArea.saHeader))
                FHeader.MarkStyleUsed(aMark);

            if (aParts.Contains(SectionArea.saFooter))
                FFooter.MarkStyleUsed(aMark);

            if (aParts.Contains(SectionArea.saPage))
                FPage.MarkStyleUsed(aMark);
        }

        public void SaveToStream(Stream aStream, HashSet<SectionArea> aSaveParts)
        {
            long vBegPos = 0, vEndPos = 0;
            byte[] vBuffer = new byte[0];

            vBegPos = aStream.Position;
            vBuffer = BitConverter.GetBytes(vBegPos);
            aStream.Write(vBuffer, 0, vBuffer.Length);  // 数据大小占位，便于越过
            //
            if (aSaveParts.Count > 0)
            {
                vBuffer = BitConverter.GetBytes(FSymmetryMargin);
                aStream.Write(vBuffer, 0, vBuffer.Length);  // 是否对称页边距

                aStream.WriteByte((byte)FPaperOrientation);  // 纸张方向

                vBuffer = BitConverter.GetBytes(FPageNoVisible);
                aStream.Write(vBuffer, 0, vBuffer.Length);  // 是否显示页码

                FPaper.SaveToStream(aStream);  // 页面参数

                bool vArea = aSaveParts.Contains(SectionArea.saHeader);  // 存页眉
                vBuffer = BitConverter.GetBytes(vArea);
                aStream.Write(vBuffer, 0, vBuffer.Length);

                vArea = aSaveParts.Contains(SectionArea.saFooter);  // 存页脚
                vBuffer = BitConverter.GetBytes(vArea);
                aStream.Write(vBuffer, 0, vBuffer.Length);

                vArea = aSaveParts.Contains(SectionArea.saPage);  // 存页面
                vBuffer = BitConverter.GetBytes(vArea);
                aStream.Write(vBuffer, 0, vBuffer.Length);

                if (aSaveParts.Contains(SectionArea.saHeader))  // 存页眉
                {
                    vBuffer = BitConverter.GetBytes(FHeaderOffset);
                    aStream.Write(vBuffer, 0, vBuffer.Length);

                    FHeader.SaveToStream(aStream);
                }

                if (aSaveParts.Contains(SectionArea.saFooter))  // 存页脚
                    FFooter.SaveToStream(aStream);

                if (aSaveParts.Contains(SectionArea.saPage))  // 存页面
                    FPage.SaveToStream(aStream);
            }
            //
            vBuffer = BitConverter.GetBytes(vBegPos);

            vEndPos = aStream.Position;
            aStream.Position = vBegPos;
            vBegPos = vEndPos - vBegPos - vBuffer.Length;

            vBuffer = BitConverter.GetBytes(vBegPos);
            aStream.Write(vBuffer, 0, vBuffer.Length);  // 当前节数据大小
            aStream.Position = vEndPos;
        }

        public string SaveToText()
        {
            return FPage.SaveToText();
        }

        public void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            long vDataSize = 0;
            bool vArea = false;
            HashSet<SectionArea> vLoadParts = new HashSet<SectionArea>();
            byte[] vBuffer = BitConverter.GetBytes(vDataSize);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            vDataSize = BitConverter.ToInt64(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FSymmetryMargin);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FSymmetryMargin = BitConverter.ToBoolean(vBuffer, 0);  // 是否对称页边距

            if (aFileVersion > 11)
            {
                vBuffer = new byte[1];
                aStream.Read(vBuffer, 0, 1);
                FPaperOrientation = (View.PaperOrientation)vBuffer[0];  // 纸张方向

                vBuffer = BitConverter.GetBytes(FPageNoVisible);
                aStream.Read(vBuffer, 0, vBuffer.Length);  // 是否显示页码
                FPageNoVisible = BitConverter.ToBoolean(vBuffer, 0);
            }

            FPaper.LoadToStream(aStream, aFileVersion);  // 页面参数
            FPage.Width = GetPageWidth();

            // 文档都有哪些部件的数据
            vLoadParts.Clear();

            vBuffer = BitConverter.GetBytes(vArea);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vArea = BitConverter.ToBoolean(vBuffer, 0);
            if (vArea)
                vLoadParts.Add(SectionArea.saHeader);

            //vBuffer = BitConverter.GetBytes(vArea);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vArea = BitConverter.ToBoolean(vBuffer, 0);
            if (vArea)
                vLoadParts.Add(SectionArea.saFooter);

            //vBuffer = BitConverter.GetBytes(vArea);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vArea = BitConverter.ToBoolean(vBuffer, 0);
            if (vArea)
                vLoadParts.Add(SectionArea.saPage);

            if (vLoadParts.Contains(SectionArea.saHeader))
            {
                vBuffer = BitConverter.GetBytes(FHeaderOffset);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FHeaderOffset = BitConverter.ToInt32(vBuffer, 0);
                FHeader.Width = FPage.Width;
                FHeader.LoadFromStream(aStream, FStyle, aFileVersion);
            }

            if (vLoadParts.Contains(SectionArea.saFooter))
            {
                FFooter.Width = FPage.Width;
                FFooter.LoadFromStream(aStream, FStyle, aFileVersion);
            }

            if (vLoadParts.Contains(SectionArea.saPage))
                FPage.LoadFromStream(aStream, FStyle, aFileVersion);

            BuildSectionPages(0);
        }

        public bool InsertStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertStream(aStream, aStyle, aFileVersion);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public void FormatData()
        {
            FActiveData.DisSelect();  // 先清选中，防止格式化后选中位置不存在
            FHeader.ReFormat();
            Footer.ReFormat();
            FPage.ReFormat();
        }

        /// <summary> 设置选中范围(如不需要更新界面可直接调用Data的SetSelectBound) </summary>
        public void ActiveDataSetSelectBound(int aStartNo, int aStartOffset, int aEndNo, int aEndOffset)
        {
            FActiveData.SetSelectBound(aStartNo, aStartOffset, aEndNo, aEndOffset, false);
            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret();
            FStyle.UpdateInfoReScroll();

            DoActiveDataCheckUpdateInfo();
        }

        public void Undo(HCUndo aUndo)
        {
            HCUndoList vUndoList = DoDataGetUndoList();
            if (!vUndoList.GroupWorking)
            {
                if (FActiveData != aUndo.Data)
                    SetActiveData(aUndo.Data as HCSectionData);

                HCFunction vEvent = delegate()
                {
                    FActiveData.Undo(aUndo);
                    return true;
                };

                ActiveDataChangeByAction(vEvent);
            }
            else
                (aUndo.Data as HCSectionData).Undo(aUndo);
        }

        public void Redo(HCUndo aRedo)
        {
            HCUndoList vUndoList = DoDataGetUndoList();
            if (!vUndoList.GroupWorking)
            {
                if (FActiveData != aRedo.Data)
                    SetActiveData(aRedo.Data as HCSectionData);

                HCFunction vEvent = delegate()
                {
                    FActiveData.Redo(aRedo);
                    return true;
                };

                ActiveDataChangeByAction(vEvent);
            }
            else
                (aRedo.Data as HCSectionData).Redo(aRedo);
        }

        // 属性
        // 页面
        public System.Drawing.Printing.PaperKind PaperSize
        {
            get { return GetPaperSize(); }
            set { SetPaperSize(value); }
        }

        public Single PaperWidth
        {
            get { return GetPaperWidth(); }
            set { SetPaperWidth(value); }
        }

        public Single PaperHeight
        {
            get { return GetPaperHeight(); }
            set { SetPaperHeight(value); }
        }

        public Single PaperMarginTop
        {
            get { return GetPaperMarginTop(); }
            set { SetPaperMarginTop(value); }
        }

        public Single PaperMarginLeft
        {
            get { return GetPaperMarginLeft(); }
            set { SetPaperMarginLeft(value); }
        }

        public Single PaperMarginRight
        {
            get { return GetPaperMarginRight(); }
            set { SetPaperMarginRight(value); }
        }

        public Single PaperMarginBottom
        {
            get { return GetPaperMarginBottom(); }
            set { SetPaperMarginBottom(value); }
        }

        public PaperOrientation PaperOrientation
        {
            get { return FPaperOrientation; }
            set { FPaperOrientation = value; }
        }

        //
        public int PaperWidthPix
        {
            get { return GetPaperWidthPix(); }
        }

        public int PaperHeightPix
        {
            get { return GetPaperHeightPix(); }
        }

        public int PaperMarginTopPix
        {
            get { return GetPaperMarginTopPix(); }
        }

        public int PaperMarginLeftPix
        {
            get { return GetPaperMarginLeftPix(); }
        }

        public int PaperMarginRightPix
        {
            get { return GetPaperMarginRightPix(); }
        }

        public int PaperMarginBottomPix
        {
            get { return GetPaperMarginBottomPix(); }
        }

        public int HeaderOffset
        {
            get { return FHeaderOffset; }
            set { SetHeaderOffset(value); }
        }

        public HCHeaderData Header
        {
            get { return FHeader; }
        }

        public HCFooterData Footer
        {
            get { return FFooter; }
        }

        public HCPageData Page
        {
            get { return FPage; }
        }

        public int CurStyleNo
        {
            get { return GetCurStyleNo(); }
        }

        public int CurParaNo
        {
            get { return GetCurParaNo(); }
        }

        /// <summary> 当前文档激活区域(页眉、页脚、页面)的数据对象 </summary>
        public HCSectionData ActiveData
        {
            get { return FActiveData; }
            set { SetActiveData(value); }
        }

        /// <summary> 当前文档激活区域页眉、页脚、页面 </summary>
        public SectionArea ActiveArea
        {
            get { return GetActiveArea(); }
        }

        public int ActivePageIndex
        {
            get { return FActivePageIndex; }
        }

        public HCViewModel ViewModel
        {
            get { return FViewModel; }
            set { FViewModel = value; }
        }

        /// <summary> 是否对称边距 </summary>
        public bool SymmetryMargin
        {
            get { return FSymmetryMargin; }
            set { FSymmetryMargin = value; }
        }

        public int DisplayFirstPageIndex
        {
            get { return FDisplayFirstPageIndex; }
            set { FDisplayFirstPageIndex = value; }
        }

        public int DisplayLastPageIndex
        {
            get { return FDisplayLastPageIndex; }
            set { FDisplayLastPageIndex = value; }
        }

        public int PageCount
        {
            get { return GetPageCount(); }
        }

        public bool PageNoVisible
        {
            get { return FPageNoVisible; }
            set { FPageNoVisible = value; }
        }

        public int PageNoFrom
        {
            get { return FPageNoFrom; }
            set { FPageNoFrom = value; }
        }

        public byte PagePadding
        {
            get { return FPagePadding; }
            set { FPagePadding = value; }
        }

        /// <summary> 文档所有部分是否只读 </summary>
        public bool ReadOnly
        {
            get { return GetReadOnly(); }
            set { SetReadOnly(value); }
        }

        public EventHandler OnDataChange
        {
            get { return FOnDataChange; }
            set { FOnDataChange = value; }
        }

        public EventHandler OnChangeTopLevelData
        {
            get { return FOnChangeTopLevelData; }
            set { FOnChangeTopLevelData = value; }
        }

        public EventHandler OnReadOnlySwitch
        {
            get { return FOnReadOnlySwitch; }
            set { FOnReadOnlySwitch = value; }
        }

        public GetScreenCoordEventHandler OnGetScreenCoord
        {
            get { return FOnGetScreenCoord; }
            set { FOnGetScreenCoord = value; }
        }

        public EventHandler OnCheckUpdateInfo
        {
            get { return FOnCheckUpdateInfo; }
            set { FOnCheckUpdateInfo = value; }
        }

        public SectionDataFloatItemEventHandler OnInsertFloatItem
        {
            get { return FOnInsertFloatItem; }
            set { FOnInsertFloatItem = value; }
        }

        public SectionDataItemEventHandler OnInsertItem
        {
            get { return FOnInsertItem; }
            set { FOnInsertItem = value; }
        }

        public SectionDataItemEventHandler OnRemoveItem
        {
            get { return FOnRemoveItem; }
            set { FOnRemoveItem = value; }
        }

        public SectionDataItemFunEvent OnSaveItem
        {
            get { return FOnSaveItem; }
            set { FOnSaveItem = value; }
        }

        public DataItemNoEventHandler OnItemResize
        {
            get { return FOnItemResize; }
            set { FOnItemResize = value; }
        }

        public SectionDataItemMouseEventHandler OnItemMouseDown
        {
            get { return FOnItemMouseDown; }
            set { FOnItemMouseDown = value; }
        }

        public SectionDataItemMouseEventHandler OnItemMouseUp
        {
            get { return FOnItemMouseUp; }
            set { FOnItemMouseUp = value; }
        }

        public SectionPaintEventHandler OnPaintHeader
        {
            get { return FOnPaintHeader; }
            set { FOnPaintHeader = value; }
        }

        public SectionPaintEventHandler OnPaintFooter
        {
            get { return FOnPaintFooter; }
            set { FOnPaintFooter = value; }
        }

        public SectionPaintEventHandler OnPaintPage
        {
            get { return FOnPaintPage; }
            set { FOnPaintPage = value; }
        }

        public SectionPaintEventHandler OnPaintPaperBefor
        {
            get { return FOnPaintPaperBefor; }
            set { FOnPaintPaperBefor = value; }
        }

        public SectionPaintEventHandler OnPaintPaperAfter
        {
            get { return FOnPaintPaperAfter; }
            set { FOnPaintPaperAfter = value; }
        }

        public SectionDrawItemPaintEventHandler OnDrawItemPaintBefor
        {
            get { return FOnDrawItemPaintBefor; }
            set { FOnDrawItemPaintBefor = value; }
        }

        public SectionDrawItemPaintEventHandler OnDrawItemPaintAfter
        {
            get { return FOnDrawItemPaintAfter; }
            set { FOnDrawItemPaintAfter = value; }
        }

        public DrawItemPaintContentEventHandler OnDrawItemPaintContent
        {
            get { return FOnDrawItemPaintContent; }
            set { FOnDrawItemPaintContent = value; }
        }

        public SectionAnnotateEventHandler OnInsertAnnotate
        {
            get { return FOnInsertAnnotate; }
            set { FOnInsertAnnotate = value; }
        }

        public SectionAnnotateEventHandler OnRemoveAnnotate
        {
            get { return FOnRemoveAnnotate; }
            set { FOnRemoveAnnotate = value; }
        }

        public SectionDrawItemAnnotateEventHandler OnDrawItemAnnotate
        {
            get { return FOnDrawItemAnnotate; }
            set { FOnDrawItemAnnotate = value; }
        }

        public EventHandler OnCreateItem
        {
            get { return FOnCreateItem; }
            set { FOnCreateItem = value; }
        }

        public SectionDataItemFunEvent OnDeleteItem
        {
            get { return FOnDeleteItem; }
            set { FOnDeleteItem = value; }
        }

        public StyleItemEventHandler OnCreateItemByStyle
        {
            get { return FOnCreateItemByStyle; }
            set { FOnCreateItemByStyle = value; }
        }

        public FloatStyleItemEventHandler OnCreateFloatItemByStyle
        {
            get { return FOnCreateFloatItemByStyle; }
            set { FOnCreateFloatItemByStyle = value; }
        }

        public OnCanEditEventHandler OnCanEdit
        {
            get { return FOnCanEdit; }
            set { FOnCanEdit = value; }
        }

        public GetUndoListEventHandler OnGetUndoList
        {
            get { return FOnGetUndoList; }
            set { FOnGetUndoList = value; }
        }

        public EventHandler OnCurParaNoChange
        {
            get { return FOnCurParaNoChange; }
            set { FOnCurParaNoChange = value; }
        }

        public EventHandler OnActivePageChange
        {
            get { return FOnActivePageChange; }
            set { FOnActivePageChange = value; }
        }
    }

    public class HCSection : HCCustomSection
    {
        public HCSection(HCStyle AStyle) : base(AStyle)
        {

        }

        /// <summary> 当前位置开始查找指定的内容 </summary>
        /// <param name="aKeyword">要查找的关键字</param>
        /// <param name="aForward">True：向前，False：向后</param>
        /// <param name="aMatchCase">True：区分大小写，False：不区分大小写</param>
        /// <returns>True：找到</returns>
        public bool Search(string aKeyword, bool aForward, bool aMatchCase)
        {
            bool Result = ActiveData.Search(aKeyword, aForward, aMatchCase);
            DoActiveDataCheckUpdateInfo();
            return Result;
        }

        public bool Replace(string aText)
        {
            HCFunction vEvent = delegate()
            {
                return ActiveData.Replace(aText);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool ParseHtml(string aHtmlText)
        {
            HCFunction vEvent = delegate()
            {
                return true;
            };

            ActiveDataChangeByAction(vEvent);
            return true;
        }

        public bool InsertFloatItem(HCCustomFloatItem aFloatItem)
        {
            if (!ActiveData.CanEdit())
                return false;

            aFloatItem.PageIndex = ActivePageIndex;
            bool Result = ActiveData.InsertFloatItem(aFloatItem);
            DoDataChanged(this);

            return Result;
        }

        public string ToHtml(string aPath)
        {
            return Page.ToHtml(aPath);
        }

        public void ToXml(XmlElement aNode)
        {
            aNode.SetAttribute("symmargin", SymmetryMargin.ToString()); // 是否对称页边距
            aNode.SetAttribute("ori", ((byte)PaperOrientation).ToString());  // 纸张方向
            aNode.SetAttribute("pagenovisible", PageNoVisible.ToString());  // 是否显示页码

            aNode.SetAttribute("pagesize",  // 纸张大小
                ((int)this.PaperSize).ToString()
                + "," + string.Format("{0:0.#}", this.PaperWidth)
                + "," + string.Format("{0:0.#}", this.PaperHeight));

            aNode.SetAttribute("margin",  // 边距
                string.Format("{0:0.#}", this.PaperMarginLeft) + ","
                + string.Format("{0:0.#}", this.PaperMarginTop) + ","
                + string.Format("{0:0.#}", this.PaperMarginRight) + ","
                + string.Format("{0:0.#}", this.PaperMarginBottom));

            // 存页眉
            XmlElement vNode = aNode.OwnerDocument.CreateElement("header");
            vNode.SetAttribute("offset", HeaderOffset.ToString());
            Header.ToXml(vNode);
            aNode.AppendChild(vNode);

            // 存页脚
            vNode = aNode.OwnerDocument.CreateElement("footer");
            Footer.ToXml(vNode);
            aNode.AppendChild(vNode);

            // 存页面
            vNode = aNode.OwnerDocument.CreateElement("page");
            this.Page.ToXml(vNode);
            aNode.AppendChild(vNode);
        }

        public void ParseXml(XmlElement aNode)
        {
            SymmetryMargin = bool.Parse(aNode.Attributes["symmargin"].Value);  // 是否对称页边距
            this.PaperOrientation = (PaperOrientation)(byte.Parse(aNode.Attributes["ori"].Value));  // 纸张方向

            PageNoVisible = bool.Parse(aNode.Attributes["pagenovisible"].Value);  // 是否显示页码

            // GetXmlPaper_
            string[] vStrings = aNode.Attributes["pagesize"].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            this.PaperSize = (PaperKind)(int.Parse(vStrings[0]));
            this.PaperWidth = int.Parse(vStrings[1]);
            this.PaperHeight = int.Parse(vStrings[2]);
            // GetXmlPaperMargin_
            vStrings = aNode.Attributes["margin"].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            this.PaperMarginLeft = int.Parse(vStrings[0]);
            this.PaperMarginTop = int.Parse(vStrings[1]);
            this.PaperMarginRight = int.Parse(vStrings[2]);
            this.PaperMarginBottom = int.Parse(vStrings[3]);

            Page.Width = this.GetPageWidth();

            for (int i = 0; i <= aNode.ChildNodes.Count - 1; i++)
            {
                if (aNode.ChildNodes[i].Name == "header")
                {
                    HeaderOffset = int.Parse(aNode.ChildNodes[i].Attributes["offset"].Value);
                    Header.Width = Page.Width;
                    Header.ParseXml(aNode.ChildNodes[i] as XmlElement);
                }
                else
                if (aNode.ChildNodes[i].Name == "footer")
                {
                    Footer.Width = Page.Width;
                    Footer.ParseXml(aNode.ChildNodes[i] as XmlElement);
                }
                else
                if (aNode.ChildNodes[i].Name == "page")
                    Page.ParseXml(aNode.ChildNodes[i] as XmlElement);
            }

            BuildSectionPages(0);
        }
    }
}