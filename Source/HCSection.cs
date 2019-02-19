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

    public delegate void SectionPagePaintEventHandler(object sender, int aPageIndex,
        RECT aRect, HCCanvas aCanvas, SectionPaintInfo aPaintInfo);

    public delegate void SectionDrawItemPaintEventHandler(object sender, HCCustomData aData, int aDrawItemNo, RECT aDrawRect,
        int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo);

    public delegate void SectionDataItemNotifyEventHandler(object sender, HCCustomData aData, HCCustomItem aItem);

    public delegate void SectionDrawItemAnnotateEventHandler(object sender, HCCustomData aData, int aDrawItemNo, RECT aDrawRect,
        HCDataAnnotate aDataAnnotate);

    public delegate void SectionAnnotateEventHandler(object sender, HCCustomData aData, HCDataAnnotate aDataAnnotate);

    public delegate void SectionDataItemMouseEventHandler(object sender, HCCustomData aData, int aItemNo, MouseEventArgs e);

    public class HCCustomSection : HCObject
    {
        private HCStyle FStyle;
        private bool FSymmetryMargin;  // 是否对称边距
        private HCPages FPages;  // 所有页面
        private HCPageSize FPageSize;
        private PageOrientation FPageOrientation;
        private HCHeaderData FHeader;
        private HCFooterData FFooter;
        private HCPageData FPageData;
        private HCSectionData FActiveData;  // 页眉、正文、页脚
        private HCSectionData FMoveData;

        bool FPageNoVisible;  // 是否显示页码

        int FPageNoFrom,  // 页码从几开始
            FActivePageIndex,  // 当前激活的页
            FMousePageIndex,  // 当前鼠标所在页
            FDisplayFirstPageIndex,  // 屏显第一页
            FDisplayLastPageIndex,   // 屏显最后一页
            FHeaderOffset;  // 页眉顶部偏移

        EventHandler
            FOnDataChanged,  // 页眉、页脚、页面某一个修改时触发
            FOnCheckUpdateInfo,  // 当前Data需要UpdateInfo更新时触发
            FOnReadOnlySwitch;  // 页眉、页脚、页面只读状态发生变化时触发

        GetScreenCoordEventHandler FOnGetScreenCoord;

        SectionPagePaintEventHandler FOnPaintHeader, FOnPaintFooter, FOnPaintPage, FOnPaintWholePageBefor, FOnPaintWholePageAfter;

        SectionDrawItemPaintEventHandler FOnDrawItemPaintBefor, FOnDrawItemPaintAfter;

        SectionAnnotateEventHandler FOnInsertAnnotate, FOnRemoveAnnotate;
        SectionDrawItemAnnotateEventHandler FOnDrawItemAnnotate;

        DrawItemPaintContentEventHandler FOnDrawItemPaintContent;

        SectionDataItemNotifyEventHandler FOnInsertItem, FOnRemoveItem;
        SectionDataItemMouseEventHandler FOnItemMouseUp;
        DataItemEventHandler FOnItemResized;
        EventHandler FOnCreateItem;
        StyleItemEventHandler FOnCreateItemByStyle;
        OnCanEditEventHandler FOnCanEdit;
        GetUndoListEventHandler FOnGetUndoList;

        private int GetPageIndexByFilm(int aVOffset)
        {
            int Result = -1;
            int vPos = 0;
            for (int i = 0; i <= FPages.Count - 1; i++)
            {
                vPos = vPos + HC.PagePadding + FPageSize.PageHeightPix;
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

        private void DoDataDrawItemPaintBefor(HCCustomData aData, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (FOnDrawItemPaintBefor != null)
                FOnDrawItemPaintBefor(this, aData, aDrawItemNo, aDrawRect, aDataDrawLeft,
                    aDataDrawBottom, aDataScreenTop, aDataScreenBottom, ACanvas, APaintInfo);
        }

        private void DoDataDrawItemPaintContent(HCCustomData aData, int aDrawItemNo, RECT aDrawRect, RECT aClearRect, string aDrawText,
            int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (FOnDrawItemPaintContent != null)
                FOnDrawItemPaintContent(aData, aDrawItemNo, aDrawRect, aClearRect, aDrawText,
                    aDataDrawLeft, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        private void DoDataDrawItemPaintAfter(HCCustomData aData, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (FOnDrawItemPaintAfter != null)
                FOnDrawItemPaintAfter(this, aData, aDrawItemNo, aDrawRect, aDataDrawLeft,
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

        private void DoDataItemMouseUp(HCCustomData aData, int aItemNo, MouseEventArgs e)
        {
            if (FOnItemMouseUp != null)
                FOnItemMouseUp(this, aData, aItemNo, e);
        }

        protected void DoDataChanged(object sender)
        {
            if (FOnDataChanged != null)
                FOnDataChanged(sender, null);
        }

        /// <summary> 缩放Item约束不要超过整页宽、高 </summary>
        private void DoDataItemResized(HCCustomData aData, int aItemNo)
        {
            HCResizeRectItem vResizeItem = aData.Items[aItemNo] as HCResizeRectItem;
            int vWidth = GetContentWidth();  // 页宽
            int vHeight = 0;
            HCCustomData vData = aData.GetRootData();  // 获取是哪一部分的ResizeItem
            if (vData == FHeader)
                vHeight = GetHeaderAreaHeight();
            else
                if (vData == FFooter)
                    vHeight = FPageSize.PageMarginBottomPix;
                else
                    if (vData == FPageData)
                        vHeight = GetContentHeight();// - FStyle.ParaStyles[vResizeItem.ParaNo].LineSpace;

            vResizeItem.RestrainSize(vWidth, vHeight);
            if (FOnItemResized != null)
                FOnItemResized(aData, aItemNo);
        }

        private HCCustomItem DoDataCreateStyleItem(HCCustomData aData, int aStyleNo)
        {
            if (FOnCreateItemByStyle != null)
                return FOnCreateItemByStyle(aData, aStyleNo);
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
            int Result = 0;
            if (aDrawItemNo < 0)
                Result = FPages.Count - 1;
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
        private void PageCoordToData(int aPageIndex, HCViewData aData, ref int aX, ref int aY,
          bool ARestrain = false)
        {
            int vMarginLeft = -1, vMarginRight = -1, viTemp = -1; ;
            GetPageMarginLeftAndRight(aPageIndex, ref vMarginLeft, ref vMarginRight);
            aX = aX - vMarginLeft;
            if (ARestrain)
            {
                if (aX < 0)
                    aX = 0;
                else
                {
                    viTemp = FPageSize.PageWidthPix - vMarginLeft - vMarginRight;
                    if (aX > viTemp)
                        aX = viTemp;
                }
            }

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
                aY = aY - FPageSize.PageHeightPix + FPageSize.PageMarginBottomPix;
                if (ARestrain)
                {
                    if (aY < 0)
                        aY = 1;
                    else
                        if (aY > FPageSize.PageMarginBottomPix)
                            aY = FPageSize.PageMarginBottomPix - 1;
                }
            }
            else
            if (aData == FPageData)
            {
                viTemp = GetHeaderAreaHeight();
                aY = aY - GetHeaderAreaHeight();
                if (ARestrain)
                {
                    if (aY < 0)
                        aY = 1;  // 处理激活正文，在页眉页脚中点击
                    else
                    {
                        viTemp = FPageSize.PageHeightPix - GetHeaderAreaHeight() - FPageSize.PageMarginBottomPix;
                        if (aY > viTemp)
                            aY = viTemp - 1;
                    }
                }
            }
        }

        private bool GetReadOnly()
        {
            return FHeader.ReadOnly && FFooter.ReadOnly && FPageData.ReadOnly;
        }

        private void SetReadOnly(bool value)
        {
            FHeader.ReadOnly = value;
            FFooter.ReadOnly = value;
            FPageData.ReadOnly = value;
        }

        /// <summary> 获取光标在Dtat中的位置信息并映射到指定页面 </summary>
        /// <param name="APageIndex">要映射到的页序号</param>
        /// <param name="aCaretInfo">光标位置信息</param>
        public virtual void GetPageCaretInfo(ref HCCaretInfo aCaretInfo)
        {
            int vMarginLeft = -1, vMarginRight = -1, vPageIndex = -1;
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
                if (FActiveData == FPageData)
                {
                    vMarginLeft = GetPageIndexByFormat(aCaretInfo.Y);  // 借用变量vMarginLeft表示页序号
                    if (vPageIndex != vMarginLeft)
                    {
                        vPageIndex = vMarginLeft;
                        FActivePageIndex = vPageIndex;
                    }
                }

                GetPageMarginLeftAndRight(vPageIndex, ref vMarginLeft, ref vMarginRight);
                aCaretInfo.X = aCaretInfo.X + vMarginLeft;
                aCaretInfo.Y = aCaretInfo.Y + GetPageTopFilm(vPageIndex);

                if (FActiveData == FHeader)
                    aCaretInfo.Y = aCaretInfo.Y + GetHeaderPageDrawTop(); // 页在节中的Top位置
                else
                    if (FActiveData == FPageData)
                        aCaretInfo.Y = aCaretInfo.Y + GetHeaderAreaHeight() - GetPageDataFmtTop(vPageIndex);  // - 页起始数据在Data中的位置
                    else
                        if (FActiveData == FFooter)
                            aCaretInfo.Y = aCaretInfo.Y + FPageSize.PageHeightPix - FPageSize.PageMarginBottomPix;
            }
        }

#region 绘制页眉数据
        private void PaintHeader(int vPageDrawTop, int vPageDrawLeft, int vPageDrawRight, int vMarginLeft, int vMarginRight,
            int vHeaderAreaHeight, int aPageIndex, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            int vHeaderDataDrawTop = vPageDrawTop + GetHeaderPageDrawTop();
            int vHeaderDataDrawBottom = vPageDrawTop + vHeaderAreaHeight;
        
            FHeader.PaintData(vPageDrawLeft + vMarginLeft, vHeaderDataDrawTop,
                vHeaderDataDrawBottom, Math.Max(vHeaderDataDrawTop, 0),
                Math.Min(vHeaderDataDrawBottom, aPaintInfo.WindowHeight), 0, aCanvas, aPaintInfo);
            
            if ((!aPaintInfo.Print) && (FActiveData == FHeader))
            {
                aCanvas.Pen.Color = Color.Blue;
                aCanvas.DrawLines(new Point[2] {
                    new Point(vPageDrawLeft, vHeaderDataDrawBottom - 1),
                    new Point(vPageDrawRight, vHeaderDataDrawBottom - 1) });
            }
            if (FOnPaintHeader != null)
            {
                int vDCState = GDI.SaveDC(aCanvas.Handle);
                try
                {
                    FOnPaintHeader(this, aPageIndex, 
                        new RECT(vPageDrawLeft + vMarginLeft, vHeaderDataDrawTop,
                                vPageDrawRight - vMarginRight, vHeaderDataDrawBottom), 
                        aCanvas, aPaintInfo);
                }
                finally
                {
                    GDI.RestoreDC(aCanvas.Handle, vDCState);
                }
            }
        }
#endregion  
        
#region 绘制页脚数据
        private void PaintFooter(int vPageDrawLeft, int vPageDrawRight, int vPageDrawBottom, int vMarginLeft, 
            int vMarginRight, int aPageIndex, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            int vFooterDataDrawTop = vPageDrawBottom - FPageSize.PageMarginBottomPix;
            FFooter.PaintData(vPageDrawLeft + vMarginLeft, vFooterDataDrawTop, vPageDrawBottom,
                Math.Max(vFooterDataDrawTop, 0), Math.Min(vPageDrawBottom, aPaintInfo.WindowHeight), 
                0, aCanvas, aPaintInfo);

            if ((!aPaintInfo.Print) && (FActiveData == FFooter))
            {
                aCanvas.Pen.Color = Color.Blue;
                aCanvas.DrawLines(new Point[2] {
                    new Point(vPageDrawLeft, vFooterDataDrawTop),
                    new Point(vPageDrawRight, vFooterDataDrawTop) });
            }

            if (FOnPaintFooter != null)
            {
                int vDCState = GDI.SaveDC(aCanvas.Handle);
                try
                {
                    FOnPaintFooter(this, aPageIndex, 
                        new RECT(vPageDrawLeft + vMarginLeft, vFooterDataDrawTop,
                        vPageDrawRight - vMarginRight, vPageDrawBottom), aCanvas, aPaintInfo);
                }
                finally
                {
                    GDI.RestoreDC(aCanvas.Handle, vDCState);
                }
            }
        }
#endregion

#region 绘制页面数据
        void PaintPageData(int vPageDrawLeft, int vPageDrawTop, int vPageDrawRight, int vPageDrawBottom,
            int vMarginLeft, int vMarginRight, int vHeaderAreaHeight, int vPageDataScreenTop, int vPageDataScreenBottom, 
            int aPageIndex, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            if ((FPages[aPageIndex].StartDrawItemNo < 0) || (FPages[aPageIndex].EndDrawItemNo < 0))
                return;

            /* 绘制数据，把Data中指定位置的数据，绘制到指定的页区域中，并按照可显示出来的区域约束 }*/
            FPageData.PaintData(vPageDrawLeft + vMarginLeft,  // 当前页数据要绘制到的Left
                vPageDrawTop + vHeaderAreaHeight,     // 当前页数据要绘制到的Top
                vPageDrawBottom - PageMarginBottomPix,  // 当前页数据要绘制的Bottom
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
                        new RECT(vPageDrawLeft + vMarginLeft,
                            vPageDrawTop + vHeaderAreaHeight, 
                            vPageDrawRight - vMarginRight,
                            vPageDrawBottom - PageMarginBottomPix), aCanvas, aPaintInfo);
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
        public virtual void PaintPage(int aPageIndex, int  aLeft, int  aTop, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            int vX = -1, vY = -1, vHeaderAreaHeight, vMarginLeft = -1, vMarginRight = -1,
              vPageDrawLeft, vPageDrawRight, vPageDrawTop, vPageDrawBottom,  // 页区域各位置
              vPageDataScreenTop, vPageDataScreenBottom,  // 页数据屏幕位置
              vScaleWidth, vScaleHeight;

            IntPtr vPaintRegion = IntPtr.Zero;
            RECT vClipBoxRect = new RECT();

            vScaleWidth = (int)Math.Round(aPaintInfo.WindowWidth / aPaintInfo.ScaleX);
            vScaleHeight = (int)Math.Round(aPaintInfo.WindowHeight / aPaintInfo.ScaleY);
            
            vPageDrawLeft = aLeft;
            vPageDrawRight = vPageDrawLeft + FPageSize.PageWidthPix;
            
            vHeaderAreaHeight = GetHeaderAreaHeight();  // 页眉区域实际高 = 页眉数据顶部偏移 + 内容高度，大于上边距时以此为准
            GetPageMarginLeftAndRight(aPageIndex, ref vMarginLeft, ref vMarginRight);  // 获取页左右边距绘制位置
            
            vPageDrawTop = aTop;  // 映射到当前页面左上角为原点的起始位置(可为负数)
            vPageDrawBottom = vPageDrawTop + FPageSize.PageHeightPix;  // 页面结束位置(可为负数)
            // 当前页数据能显示出来的区域边界
            vPageDataScreenTop = Math.Max(vPageDrawTop + vHeaderAreaHeight, 0);
            vPageDataScreenBottom = Math.Min(vPageDrawBottom - FPageSize.PageMarginBottomPix, vScaleHeight);

            aPaintInfo.PageDataFmtTop = GetPageDataFmtTop(aPageIndex);
            
            GDI.GetClipBox(aCanvas.Handle, ref vClipBoxRect);  // 保存当前的绘图区域
            
            if (!aPaintInfo.Print)
            {

#region 非打印时填充页面背景
                aCanvas.Brush.Color = FStyle.BackgroudColor;
                aCanvas.FillRect(new RECT(vPageDrawLeft, vPageDrawTop, Math.Min(vPageDrawRight, vScaleWidth),  // 约束边界
                    Math.Min(vPageDrawBottom, vScaleHeight)));
#endregion

#region 页眉边距指示符
                if (vPageDrawTop + vHeaderAreaHeight > 0)
                {
                    vY = vPageDrawTop + vHeaderAreaHeight;
                    if (vHeaderAreaHeight > FPageSize.PageMarginTopPix)
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

                        aPaintInfo.DrawNoScaleLine(aCanvas, new Point[2] { new Point(vPageDrawLeft, vY - 1), new Point(vPageDrawRight, vY - 1) });
                    }

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

                    // 左上， 左-原-上
                    vX = vPageDrawLeft + vMarginLeft;
                    vY = vPageDrawTop + FPageSize.PageMarginTopPix;
                    aPaintInfo.DrawNoScaleLine(aCanvas, new Point[3] { new Point(vX - HC.PMSLineHeight, vY),
                        new Point(vX, vY), new Point(vX, vY - HC.PMSLineHeight) });
                    // 右上，右-原-上
                    vX = vPageDrawLeft + FPageSize.PageWidthPix - vMarginRight;
                    aPaintInfo.DrawNoScaleLine(aCanvas, new Point[3] { new Point(vX + HC.PMSLineHeight, vY), new Point(vX, vY), new Point(vX, vY - HC.PMSLineHeight) });
                }
#endregion

#region 页脚边距指示符
                vY = vPageDrawBottom - FPageSize.PageMarginBottomPix;
                if (vY < aPaintInfo.WindowHeight)
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

                    // 左下，左-原-下
                    vX = vPageDrawLeft + vMarginLeft;
                    aPaintInfo.DrawNoScaleLine(aCanvas, new Point[3] { new Point(vX - HC.PMSLineHeight, vY),
                        new Point(vX, vY), new Point(vX, vY + HC.PMSLineHeight) });
                    // 右下，右-原-下
                    vX = vPageDrawRight - vMarginRight;
                    aPaintInfo.DrawNoScaleLine(aCanvas, new Point[3] { new Point(vX + HC.PMSLineHeight, vY),
                        new Point(vX, vY), new Point(vX, vY + HC.PMSLineHeight) });
                }
#endregion

            }

            if (FOnPaintWholePageBefor != null)
                FOnPaintWholePageBefor(this, aPageIndex,
                    new RECT(vPageDrawLeft, vPageDrawTop, vPageDrawRight, vPageDrawBottom),
                    aCanvas, aPaintInfo);

#region 绘制页眉
            if (vPageDrawTop + vHeaderAreaHeight > 0)
            {
                vPaintRegion = (IntPtr)GDI.CreateRectRgn(aPaintInfo.GetScaleX(vPageDrawLeft),
                    Math.Max(aPaintInfo.GetScaleY(vPageDrawTop + FHeaderOffset), 0),
                    aPaintInfo.GetScaleX(vPageDrawRight),
                Math.Min(aPaintInfo.GetScaleY(vPageDrawTop + vHeaderAreaHeight), aPaintInfo.WindowHeight));
                try
                {
                    GDI.SelectClipRgn(aCanvas.Handle, vPaintRegion);  // 设置绘制有效区域
                    PaintHeader(vPageDrawTop, vPageDrawLeft, vPageDrawRight, vMarginLeft, vMarginRight,
                        vHeaderAreaHeight, aPageIndex, aCanvas, aPaintInfo);
                }
                finally
                {
                    GDI.DeleteObject(vPaintRegion);
                }
            }
#endregion
            
#region 绘制页脚
            if (aPaintInfo.GetScaleY(vPageDrawBottom - FPageSize.PageMarginBottomPix) < aPaintInfo.WindowHeight)  // 页脚可显示
            {
                vPaintRegion = (IntPtr)GDI.CreateRectRgn(aPaintInfo.GetScaleX(vPageDrawLeft),
                    Math.Max(aPaintInfo.GetScaleY(vPageDrawBottom - FPageSize.PageMarginBottomPix), 0),
                    aPaintInfo.GetScaleX(vPageDrawRight),
                    Math.Min(aPaintInfo.GetScaleY(vPageDrawBottom), aPaintInfo.WindowHeight));
                try
                {
                    GDI.SelectClipRgn(aCanvas.Handle, vPaintRegion);  // 设置绘制有效区域
                    PaintFooter(vPageDrawLeft, vPageDrawRight, vPageDrawBottom, vMarginLeft, vMarginRight,
                        aPageIndex, aCanvas, aPaintInfo);
                }
                finally
                {
                    GDI.DeleteObject(vPaintRegion);
                }
            }
#endregion

#region 绘制页面
            if (vPageDataScreenBottom > vPageDataScreenTop)  // 能露出数据则绘制当前页，绘制正文
            {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
                vPaintRegion = (IntPtr)GDI.CreateRectRgn(aPaintInfo.GetScaleX(vPageDrawLeft),
                        aPaintInfo.GetScaleY(Math.Max(vPageDrawTop + vHeaderAreaHeight, vPageDataScreenTop)),
                        aPaintInfo.GetScaleX(vPageDrawRight),
                        aPaintInfo.GetScaleY(Math.Min(vPageDrawBottom - FPageSize.PageMarginBottomPix, vPageDataScreenBottom)) + 1);
                try
                {
                    GDI.SelectClipRgn(aCanvas.Handle, vPaintRegion);  // 设置绘制有效区域
                    PaintPageData(vPageDrawLeft, vPageDrawTop, vPageDrawRight, vPageDrawBottom, 
                        vMarginLeft, vMarginRight, vHeaderAreaHeight, vPageDataScreenTop, vPageDataScreenBottom,
                        aPageIndex, aCanvas, aPaintInfo);
                }
                finally
                {
                    GDI.DeleteObject(vPaintRegion);
                }
            }
#endregion
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    
            vPaintRegion = (IntPtr)GDI.CreateRectRgn(
                aPaintInfo.GetScaleX(vPageDrawLeft),
                aPaintInfo.GetScaleX(vPageDrawTop),
                aPaintInfo.GetScaleX(vPageDrawRight),
                aPaintInfo.GetScaleX(vPageDrawBottom));
            try
            {
                GDI.SelectClipRgn(aCanvas.Handle, vPaintRegion);
            
                FHeader.PaintFloatItems(aPageIndex, vPageDrawLeft + vMarginLeft,
                    vPageDrawTop + GetHeaderPageDrawTop(), 0, aCanvas, aPaintInfo);
            
                FFooter.PaintFloatItems(aPageIndex, vPageDrawLeft + vMarginLeft,
                    vPageDrawBottom - FPageSize.PageMarginBottomPix, 0, aCanvas, aPaintInfo);

                FPageData.PaintFloatItems(aPageIndex, vPageDrawLeft + vMarginLeft,  // 当前页绘制到的Left
                vPageDrawTop + vHeaderAreaHeight,     // 当前页绘制到的Top
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

            if (FOnPaintWholePageAfter != null)  // 公开页面绘制后事件
            {
                FOnPaintWholePageAfter(this, aPageIndex, 
                    new RECT(vPageDrawLeft, vPageDrawTop, vPageDrawRight, vPageDrawBottom),
                    aCanvas, aPaintInfo);
            }
        }

        public virtual void Clear()
        {
            FHeader.Clear();
            FFooter.Clear();
            FPageData.Clear();
            FPages.ClearEx();
            FActivePageIndex = 0;
        }

        public virtual void MouseDown(MouseEventArgs e)
        {
            bool vChangeActiveData = false;
            int vPageIndex = GetPageIndexByFilm(e.Y);  // 鼠标点击处所在的页(和光标所在页可能并不是同一页，如表格跨页时，空单元格第二页点击时，光标回前一页)
            if (FActivePageIndex != vPageIndex)
                FActivePageIndex = vPageIndex;
            
            int vX = -1, vY = -1;
            if (FActiveData.FloatItems.Count > 0)
            {
                SectionCoordToPage(FActivePageIndex, e.X, e.Y, ref vX, ref vY);
                PageCoordToData(FActivePageIndex, FActiveData, ref vX, ref vY);
                if (FActiveData == FPageData)
                    vY = vY + GetPageDataFmtTop(FActivePageIndex);

                MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
                if (FActiveData.MouseDownFloatItem(vMouseArgs))
                    return;
            }
           
            SectionCoordToPage(FActivePageIndex, e.X, e.Y, ref vX, ref vY);  // X，Y转换到指定页的坐标vX,vY
            
            HCSectionData vNewActiveData = GetSectionDataAt(vX, vY);
            if ((vNewActiveData != FActiveData) && (e.Clicks == 2))
            {
                SetActiveData(vNewActiveData);
                vChangeActiveData = true;
            }

            if ((vNewActiveData != FActiveData) && (FActiveData == FPageData))
                PageCoordToData(FActivePageIndex, FActiveData, ref vX, ref vY, true);  // 约束到Data中，防止点页脚认为是下一页
            else
                PageCoordToData(FActivePageIndex, FActiveData, ref vX, ref vY);
            
            if (FActiveData == FPageData)
                vY = vY + GetPageDataFmtTop(FActivePageIndex);
            
            if ((e.Clicks == 2) && (!vChangeActiveData))
                FActiveData.DblClick(vX, vY);
            else
            {
                MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
                FActiveData.MouseDown(vMouseArgs);
            }
        }

        public virtual void MouseMove(MouseEventArgs e)
        {
            int vMarginLeft = -1, vMarginRight = -1;

            GetPageMarginLeftAndRight(FMousePageIndex, ref vMarginLeft, ref vMarginRight);

            if (e.X < vMarginLeft)
                HC.GCursor = Cursors.Default;
            else
            if (e.X > FPageSize.PageWidthPix - vMarginRight)
                HC.GCursor = Cursors.Default;
            else
                HC.GCursor = Cursors.IBeam;

            FMousePageIndex = GetPageIndexByFilm(e.Y);
            //Assert(FMousePageIndex >= 0, "不应该出现鼠标移动到空页面上的情况！");
            //if FMousePageIndex < 0 then Exit;  应该永远不会出现
            
            int vX = -1, vY = -1;
            MouseEventArgs vEventArgs = null;

            if (FActiveData.FloatItems.Count > 0)
            {
                if ((e.Button == MouseButtons.Left) && (FActiveData.FloatItemIndex >= 0))
                {
                    if (!FActiveData.ActiveFloatItem.Resizing)
                        FActiveData.ActiveFloatItem.PageIndex = FMousePageIndex;
                }
                
                if (FActiveData == FPageData)
                {
                    if ((FActiveData.FloatItemIndex >= 0) && (FActiveData.ActiveFloatItem.Resizing))
                    {
                        SectionCoordToPage(FActiveData.ActiveFloatItem.PageIndex, e.X, e.Y, ref vX, ref vY);
                        PageCoordToData(FActiveData.ActiveFloatItem.PageIndex, FActiveData, ref vX, ref vY);
                        vY = vY + GetPageDataFmtTop(FActiveData.ActiveFloatItem.PageIndex);
                    }
                    else
                    {
                        SectionCoordToPage(FMousePageIndex, e.X, e.Y, ref vX, ref vY);
                        PageCoordToData(FMousePageIndex, FActiveData, ref vX, ref vY);
                        vY = vY + GetPageDataFmtTop(FMousePageIndex);
                    }
                }
                else  // FloatItem在Header或Footer
                {
                    if ((FActiveData.FloatItemIndex >= 0) && (FActiveData.ActiveFloatItem.Resizing))
                    {
                        SectionCoordToPage(FActivePageIndex, e.X, e.Y, ref vX, ref vY);
                        PageCoordToData(FActivePageIndex, FActiveData, ref vX, ref vY);
                    }
                    else
                    {
                        SectionCoordToPage(FMousePageIndex, e.X, e.Y, ref vX, ref vY);
                        PageCoordToData(FMousePageIndex, FActiveData, ref vX, ref vY);
                    }
                }

                vEventArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
                if (FActiveData.MouseMoveFloatItem(vEventArgs))
                    return;
            }

            SectionCoordToPage(FMousePageIndex, e.X, e.Y, ref vX, ref vY);
            
            HCSectionData vMoveData = GetSectionDataAt(vX, vY);
            if (vMoveData != FMoveData)
            {
                if (FMoveData != null)
                    FMoveData.MouseLeave();
            
                FMoveData = vMoveData;
            }

            PageCoordToData(FMousePageIndex, FActiveData, ref vX, ref vY, FActiveData.Selecting);  // 划选时约束到Data中
            
            if (FActiveData == FPageData)
                vY = vY + GetPageDataFmtTop(FMousePageIndex);
            
            vEventArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
            FActiveData.MouseMove(vEventArgs);
        }

        public virtual void MouseUp(MouseEventArgs e)
        {
            int vPageIndex = GetPageIndexByFilm(e.Y);

            int vX = -1, vY = -1;
            MouseEventArgs vEventArgs = null;
            if ((FActiveData.FloatItems.Count > 0) && (FActiveData.FloatItemIndex >= 0))
            {
                if (FActiveData == FPageData)
                {
                    SectionCoordToPage(FActiveData.ActiveFloatItem.PageIndex, e.X, e.Y, ref vX, ref vY);
                    PageCoordToData(FActiveData.ActiveFloatItem.PageIndex, FActiveData, ref vX, ref vY);
                    vY = vY + GetPageDataFmtTop(FActiveData.ActiveFloatItem.PageIndex);
                }
                else  // FloatItem在Header或Footer
                {
                    SectionCoordToPage(vPageIndex, e.X, e.Y, ref vX, ref vY);
                    PageCoordToData(vPageIndex, FActiveData, ref vX, ref vY);
                }

                vEventArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
                if (FActiveData.MouseUpFloatItem(vEventArgs))
                    return;
            }
            
            SectionCoordToPage(vPageIndex, e.X, e.Y, ref vX, ref vY);
                
            //if  <> FActiveData then Exit;  // 不在当前激活的Data上
            if ((GetSectionDataAt(vX, vY) != FActiveData) && (FActiveData == FPageData))
                PageCoordToData(vPageIndex, FActiveData, ref vX, ref vY, true);
            else
                PageCoordToData(vPageIndex, FActiveData, ref vX, ref vY);

            if (FActiveData == FPageData)
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

        protected void KillFocus()
        {
            FActiveData.KillFocus();
        }

        // 纸张信息
        protected PaperKind GetPaperKind()
        {
            return FPageSize.PaperKind;
        }

        protected void SetPaperKind(PaperKind Value)
        {
            FPageSize.PaperKind = Value;
        }
        // 边距信息
        protected Single GetPaperWidth()
        {
            return FPageSize.PaperWidth;
        }

        protected Single GetPaperHeight()
        {
            return FPageSize.PaperHeight;
        }

        protected Single GetPaperMarginTop()
        {
            return FPageSize.PaperMarginTop;
        }

        protected Single GetPaperMarginLeft()
        {
            return FPageSize.PaperMarginLeft;
        }

        protected Single GetPaperMarginRight()
        {
            return FPageSize.PaperMarginRight;
        }

        protected Single GetPaperMarginBottom()
        {
            return FPageSize.PaperMarginBottom;
        }

        protected void SetPaperWidth(Single value)
        {
            FPageSize.PaperWidth = value;
            FPageSize.PaperKind = System.Drawing.Printing.PaperKind.Custom;
        }

        protected void SetPaperHeight(Single value)
        {
            FPageSize.PaperHeight = value;
            FPageSize.PaperKind = System.Drawing.Printing.PaperKind.Custom;
        }

        protected void SetPaperMarginTop(Single value)
        {
            FPageSize.PaperMarginTop = value;
        }

        protected void SetPaperMarginLeft(Single value)
        {
            FPageSize.PaperMarginLeft = value;
        }

        protected void SetPaperMarginRight(Single value)
        {
            FPageSize.PaperMarginRight = value;
        }

        protected void SetPaperMarginBottom(Single value)
        {
            FPageSize.PaperMarginBottom = value;
        }

        protected void SetPageOrientation(PageOrientation value)
        {
            if (FPageOrientation != value)
            {
                FPageOrientation = value;
                Single vfW = FPageSize.PaperWidth;
                FPageSize.PaperWidth = FPageSize.PaperHeight;
                FPageSize.PaperHeight = vfW;
            }
        }

        protected int GetPageWidthPix()
        {
            return FPageSize.PageWidthPix;
        }

        protected int GetPageHeightPix()
        {
            return FPageSize.PageHeightPix;
        }

        protected int GetPageMarginTopPix()
        {
            return FPageSize.PageMarginTopPix;
        }

        protected int GetPageMarginLeftPix()
        {
            return FPageSize.PageMarginLeftPix;
        }

        protected int GetPageMarginRightPix()
        {
            return FPageSize.PageMarginRightPix;
        }

        protected int GetPageMarginBottomPix()
        {
            return FPageSize.PageMarginBottomPix;
        }

        protected void SetPageWidthPix(int value)
        {
            if (FPageSize.PageWidthPix != value)
                FPageSize.PageWidthPix = value;
        }

        protected void SetPageHeightPix(int value)
        {
            if (FPageSize.PageHeightPix != value)
                FPageSize.PageHeightPix = value;
        }

        protected void SetPageMarginTopPix(int value)
        {
            if (FPageSize.PageMarginTopPix != value)
                FPageSize.PageMarginTopPix = value;
        }

        protected void SetPageMarginLeftPix(int value)
        {
            if (FPageSize.PageMarginLeftPix != value)
                FPageSize.PageMarginLeftPix = value;
        }

        protected void SetPageMarginRightPix(int value)
        {
            if (FPageSize.PageMarginRightPix != value)
                FPageSize.PageMarginRightPix = value;
        }

        protected void SetPageMarginBottomPix(int value)
        {
            if (FPageSize.PageMarginBottomPix != value)
                FPageSize.PageMarginBottomPix = value;
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
            if (x > FPageSize.PageWidthPix)
            {
                return FActiveData;
            }
            if (y < 0)
            {
                return FActiveData;
            }
            if (y > FPageSize.PageHeightPix)
            {
                return FActiveData;
            }
            // 边距信息，先上下，再左右
            if (y > FPageSize.PageHeightPix - FPageSize.PageMarginBottomPix)
                return FFooter;
            // 页眉区域实际高(页眉内容高度>上边距时，取页眉内容高度)
            if (y < GetHeaderAreaHeight())
                return FHeader;
            //if X > FPageSize.PageWidthPix - vMarginRight then Exit;  // 点击在页右边距区域TEditArea.eaMarginRight
            //if X < vMarginLeft then Exit;  // 点击在页左边距区域TEditArea.eaMarginLeft
            //如果要区分左、右边距不是正文，注意双击等判断ActiveData为nil
            return FPageData;
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
            int vContentHeight = GetContentHeight();

            for (int i = 0; i <= FPages.Count - 1; i++)
            {
                vTop = vTop + vContentHeight;
                if (vTop >= aVertical)
                {
                    vTop = aVertical - (vTop - vContentHeight);
                    break;
                }
                else
                    Result = Result + HC.PagePadding + FPageSize.PageHeightPix;
            }
            Result = Result + HC.PagePadding + GetHeaderAreaHeight() + vTop;

            return Result;
        }

        protected bool ActiveDataChangeByAction(HCFunction aFunction)
        {
            int vHeight, vDrawItemCount, vCurItemNo, vNewItemNo;
            bool Result = false;

            if (!FActiveData.CanEdit())
                return false;

            if (FActiveData.FloatItemIndex >= 0)
                return false;

            // 记录变动前的状态
            vHeight = FActiveData.Height;
            // 应用选中文本样式等操作，并不引起高度变化，但会引起DrawItem数量变化
            // 也需要重新计算各页起始结束DrawItem
            vDrawItemCount = FActiveData.DrawItems.Count;  // 变动前的DrawItem数量
            vCurItemNo = FActiveData.GetCurItemNo();

            Result = aFunction();  // 处理变动

            // 变动后的状态
            vNewItemNo = FActiveData.GetCurItemNo();  // 变动后的当前ItemNo
            if (vNewItemNo < 0)  // 如果变动后小于0，修正为第0个
                vNewItemNo = 0;

            if ((vDrawItemCount != FActiveData.DrawItems.Count)  // DrawItem数量变化了
              || (vHeight != FActiveData.Height))  // 数据高度变化了
            {
                if (FActiveData == FPageData)
                {
                    BuildSectionPages(Math.Min(Math.Min(vCurItemNo, vNewItemNo), FPageData.ReFormatStartItemNo));
                }
                else
                {
                    BuildSectionPages(0);
                }
            }

            DoDataChanged(this);

            return Result;
        }

        protected HCStyle Style
        {
            get { return FStyle; }
        }

        protected void SetDataProperty(int vWidth, HCSectionData aData)
        {
            aData.Width = vWidth;
            aData.OnInsertItem = DoDataInsertItem;
            aData.OnRemoveItem = DoDataRemoveItem;
            aData.OnItemResized = DoDataItemResized;
            aData.OnItemMouseUp = DoDataItemMouseUp;
            aData.OnCreateItemByStyle = DoDataCreateStyleItem;
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
        }

        public HCCustomSection(HCStyle aStyle)
        {
            FStyle = aStyle;
            FActiveData = null;
            FMoveData = null;
            FPageNoVisible = true;
            FPageNoFrom = 1;
            FHeaderOffset = 20;
            FDisplayFirstPageIndex = -1;
            FDisplayLastPageIndex = -1;
            FPageSize = new HCPageSize(aStyle.PixelsPerMMX, aStyle.PixelsPerMMY);
            FPageOrientation = PageOrientation.cpoPortrait;
            int vWidth = GetContentWidth();
            FPageData = new HCPageData(aStyle);
            SetDataProperty(vWidth, FPageData);
            // FData.PageHeight := PageHeightPix - PageMarginBottomPix - GetHeaderAreaHeight;
            // 在ReFormatSectionData中处理了FData.PageHeight
            FHeader = new HCHeaderData(aStyle);
            SetDataProperty(vWidth, FHeader);
            FFooter = new HCFooterData(aStyle);
            SetDataProperty(vWidth, FFooter);
            FActiveData = FPageData;
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
            base.Dispose();
            FHeader.Dispose();
            FFooter.Dispose();
            FPageData.Dispose();
            FPageSize.Dispose();
            //FPages.Free;
        }

        /// <summary> 修改纸张边距 </summary>
        public void ResetMargin()
        {
            FPageData.Width = GetContentWidth();
            FHeader.Width = FPageData.Width;
            FFooter.Width = FPageData.Width;
            FormatData();
            BuildSectionPages(0);
            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret(false);
            DoDataChanged(this);
        }

        public void DisActive()
        {
            FActiveData.DisSelect();
            FHeader.InitializeField();
            FFooter.InitializeField();
            FPageData.InitializeField();
            FActiveData = FPageData;
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
            return FActiveData.GetTopLevelData().GetHint();
        }

        public HCCustomItem GetCurItem()
        {
            return FActiveData.GetCurItem();
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
            if (FActiveData != FPageData)
                Result = FActivePageIndex;
            else
            {
                if (FPageData.CaretDrawItemNo < 0)
                {
                    vCaretDrawItemNo = FPageData.GetDrawItemNoByOffset(FPageData.SelectInfo.StartItemNo,
                        FPageData.SelectInfo.StartItemOffset);
                }
                else
                    vCaretDrawItemNo = FPageData.CaretDrawItemNo;
            
                HCCaretInfo vCaretInfo = new HCCaretInfo();
                for (int i = 0; i <= FPages.Count - 1; i++)
                {
                    if (FPages[i].EndDrawItemNo >= vCaretDrawItemNo)
                    {
                        if ((i < FPages.Count - 1) && (FPages[i + 1].StartDrawItemNo == vCaretDrawItemNo))
                        {
                            if (FPageData.SelectInfo.StartItemNo >= 0)
                            {
                                vCaretInfo.Y = 0;
                                (FPageData as HCCustomData).GetCaretInfo(FPageData.SelectInfo.StartItemNo,
                                    FPageData.SelectInfo.StartItemOffset, ref vCaretInfo);
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
            return aVOffset / GetContentHeight();
        }

        public void PaintDisplayPage(int aFilmOffsetX, int aFilmOffsetY, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            for (int i = FDisplayFirstPageIndex; i <= FDisplayLastPageIndex; i++)
            {
                aPaintInfo.PageIndex = i;
                int vPageFilmTop = GetPageTopFilm(i);
                int vPageDrawTop = vPageFilmTop - aFilmOffsetY;  // 映射到当前页面为原点的屏显起始位置(可为负数)
                PaintPage(i, aFilmOffsetX, vPageDrawTop, aCanvas, aPaintInfo);
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
                        FActivePageIndex = GetPageIndexByCurrent();  // 方向键可能移动到了其他页
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

        public bool InsertText(string aText)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertText(aText);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        public bool InsertTable(int aRowCount, int  aColCount)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertTable(aRowCount, aColCount);
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

        public bool InsertDomain(HCDomainItem aMouldDomain)
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.InsertDomain(aMouldDomain);
            };

            return ActiveDataChangeByAction(vEvent);
        }

        /// <summary> 从当前位置后分页 </summary>
        public bool InsertPageBreak()
        {
            HCFunction vEvent = delegate()
            {
                return FPageData.InsertPageBreak();
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

        //
        //// <summary>  节坐标转换到指定页坐标 </summary>
        public void SectionCoordToPage(int aPageIndex, int x, int y, ref int aPageX, ref int aPageY)
        {
            aPageX = x;// - vMarginLeft;
            int vPageFilmTop = GetPageTopFilm(aPageIndex);
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

        public void ApplyParaLineSpace(ParaLineSpaceMode aSpaceMode)
        {
            HCFunction vEvent = delegate()
            {
                FActiveData.ApplyParaLineSpace(aSpaceMode);
                return true;
            };

            ActiveDataChangeByAction(vEvent);
        }

        /// <summary> 某页在整个节中的Top位置 </summary>
        /// <param name="aPageIndex"></param>
        /// <returns></returns>
        public int GetPageTopFilm(int aPageIndex)
        {
            int Result = HC.PagePadding;
            for (int i = 0; i <= aPageIndex - 1; i++)
                Result = Result + FPageSize.PageHeightPix + HC.PagePadding;  // 每一页和其上面的分隔计为一整个处理单元
            
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
                int vContentHeight = GetContentHeight();
                for (int i = 0; i <= aPageIndex - 1; i++)
                    Result = Result + vContentHeight;
            }

            return Result;
        }

        /// <summary> 页眉内容在页中绘制时的起始位置 </summary>
        /// <returns></returns>
        public int GetHeaderPageDrawTop()
        {
            int Result = FHeaderOffset;
            int vHeaderHeight = FHeader.Height;
            if (vHeaderHeight < (FPageSize.PageMarginTopPix - FHeaderOffset))
                Result = Result + (FPageSize.PageMarginTopPix - FHeaderOffset - vHeaderHeight) / 2;

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
                aMarginLeft = FPageSize.PageMarginRightPix;
                aMarginRight = FPageSize.PageMarginLeftPix;
            }
            else
            {
                aMarginLeft = FPageSize.PageMarginLeftPix;
                aMarginRight = FPageSize.PageMarginRightPix;
            }
        }

#region 内部方法
        private void _FormatNewPage(ref int vPageIndex, int aPrioEndDItemNo, int  aNewStartDItemNo)
        {
            FPages[vPageIndex].EndDrawItemNo = aPrioEndDItemNo;
            HCPage vPage = new HCPage();
            vPage.StartDrawItemNo = aNewStartDItemNo;
            FPages.Insert(vPageIndex + 1, vPage);
            vPageIndex++;
        }

        private void _RectItemCheckPage(int ADrawItemNo, int vPageDataFmtBottom, int AStartSeat,
            int vContentHeight, HCCustomRectItem vRectItem, ref int vPageIndex, 
            ref int vBreakSeat, ref int vSuplus, ref int vPageDataFmtTop)
        {
            int vFmtHeightInc = -1, vFmtOffset = -1;

            if (FPageData.GetDrawItemStyle(ADrawItemNo) == HCStyle.PageBreak)
            {
                vFmtOffset = vPageDataFmtBottom - FPageData.DrawItems[ADrawItemNo].Rect.Top;
                
                vSuplus = vSuplus + vFmtOffset;
                if (vFmtOffset > 0)
                    HC.OffsetRect(ref FPageData.DrawItems[ADrawItemNo].Rect, 0, vFmtOffset);

                vPageDataFmtTop = vPageDataFmtBottom;
                vPageDataFmtBottom = vPageDataFmtTop + vContentHeight;
        
                _FormatNewPage(ref vPageIndex, ADrawItemNo - 1, ADrawItemNo);  // 新建页
            }
            else
            if (FPageData.DrawItems[ADrawItemNo].Rect.Bottom > vPageDataFmtBottom)
            {
                if ((FPages[vPageIndex].StartDrawItemNo == ADrawItemNo)
                    && (AStartSeat == 0)
                    && (!vRectItem.CanPageBreak))
                {
                    vFmtHeightInc = vPageDataFmtBottom - FPageData.DrawItems[ADrawItemNo].Rect.Bottom;
                    vSuplus = vSuplus + vFmtHeightInc;
                    FPageData.DrawItems[ADrawItemNo].Rect.Bottom =  // 扩充格式化高度
                    FPageData.DrawItems[ADrawItemNo].Rect.Bottom + vFmtHeightInc;
                    vRectItem.Height = vRectItem.Height + vFmtHeightInc;  // 是在这里处理呢还是在RectItem内部更合适？
        
                    return;
                }
        
                RECT vDrawRect = FPageData.DrawItems[ADrawItemNo].Rect;
                
                //if vSuplus = 0 then  // 第一次计算分页
                HC.InflateRect(ref vDrawRect, 0, -FPageData.GetLineBlankSpace(ADrawItemNo) / 2);  // 减掉行间距，为了达到去年行间距能放下不换页的效果
        
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
                    vFmtOffset = vFmtOffset + FPageData.GetLineBlankSpace(ADrawItemNo) / 2;  // 整体向下移动增加上面减掉的行间距
                    vSuplus = vSuplus + vFmtOffset + vFmtHeightInc;
                    HC.OffsetRect(ref FPageData.DrawItems[ADrawItemNo].Rect, 0, vFmtOffset);
                    vPageDataFmtTop = vPageDataFmtBottom;
                    vPageDataFmtBottom = vPageDataFmtTop + vContentHeight;
                    _FormatNewPage(ref vPageIndex, ADrawItemNo - 1, ADrawItemNo);  // 新建页
                    _RectItemCheckPage(ADrawItemNo, vPageDataFmtBottom, AStartSeat, vContentHeight,
                        vRectItem, ref vPageIndex, ref vBreakSeat, ref vSuplus, ref vPageDataFmtTop);
                 }
                else  // 跨页，但未整体下移
                {
                    vSuplus = vSuplus + vFmtHeightInc;
                    FPageData.DrawItems[ADrawItemNo].Rect.Bottom =  // 扩充格式化高度
                        FPageData.DrawItems[ADrawItemNo].Rect.Bottom + vFmtHeightInc;
                    vRectItem.Height = vRectItem.Height + vFmtHeightInc;  // 是在这里处理呢还是在RectItem内部更合适？
    
                    vPageDataFmtTop = vPageDataFmtBottom;
                    vPageDataFmtBottom = vPageDataFmtTop + vContentHeight;
                    _FormatNewPage(ref vPageIndex, ADrawItemNo, ADrawItemNo);  // 新建页
                    
                    _RectItemCheckPage(ADrawItemNo, vPageDataFmtBottom, AStartSeat, vContentHeight,
                        vRectItem, ref vPageIndex, ref vBreakSeat, ref vSuplus, ref vPageDataFmtTop);  // 从分页位置后面继续检查是否分页
                }
            }
        }

        private void _FormatRectItemCheckPageBreak(int vPageDataFmtBottom, int vContentHeight,
            ref int vPageIndex, ref int vPageDataFmtTop, int ADrawItemNo)
        {
            HCCustomRectItem vRectItem;
            int vSuplus = 0;  // 所有因换页向下偏移量的总和
            int vBreakSeat = 0;  // 分页位置，不同RectItem的含义不同，表格表示 vBreakRow

            vRectItem = FPageData.Items[FPageData.DrawItems[ADrawItemNo].ItemNo] as HCCustomRectItem;
            vSuplus = 0;
            vBreakSeat = 0;
            
            vRectItem.CheckFormatPageBreakBefor();
            _RectItemCheckPage(ADrawItemNo, vPageDataFmtBottom, 0, vContentHeight, vRectItem, 
                ref vPageIndex, ref vBreakSeat, ref vSuplus, ref vPageDataFmtTop);  // 从最开始位置，检测表格各行内容是否能显示在当前页
            
            if (vSuplus != 0)
            {
                for (int i = ADrawItemNo + 1; i <= FPageData.DrawItems.Count - 1; i++)
                    HC.OffsetRect(ref FPageData.DrawItems[i].Rect, 0, vSuplus);
            }
        }

        private  void _FormatTextItemCheckPageBreak(int vContentHeight, int ADrawItemNo,
            ref int vPageDataFmtTop, ref int vPageDataFmtBottom, ref int vPageIndex)
        {
            //if not DrawItems[ADrawItemNo].LineFirst then Exit; // 注意如果文字环绕时这里就不能只判断行第1个
            if (FPageData.DrawItems[ADrawItemNo].Rect.Bottom > vPageDataFmtBottom)
            {
                int vH = vPageDataFmtBottom - FPageData.DrawItems[ADrawItemNo].Rect.Top;
                for (int i = ADrawItemNo; i <= FPageData.DrawItems.Count - 1; i++)
                    HC.OffsetRect(ref FPageData.DrawItems[i].Rect, 0, vH);
            
                vPageDataFmtTop = vPageDataFmtBottom;
                vPageDataFmtBottom = vPageDataFmtTop + vContentHeight;
                _FormatNewPage(ref vPageIndex, ADrawItemNo - 1, ADrawItemNo); // 新建页
            }
    }
#endregion

        /// <summary> 从正文指定Item开始重新计算页 </summary>
        /// <param name="aStartItemNo"></param>
        public void BuildSectionPages(int aStartItemNo)
        {
            int vPrioDrawItemNo = -1;
            HCPage vPage = null;                

            if (aStartItemNo > 0)
                vPrioDrawItemNo = FPageData.GetItemLastDrawItemNo(aStartItemNo - 1);  // 上一个最后的DItem
            else
                vPrioDrawItemNo = -1;
            
            // 上一个DrawItemNo所在页作为格式化起始页
            int vPageIndex = -1;
            if (vPrioDrawItemNo < 0)
                vPageIndex = 0;
            else  // 指定了DrawItemNo
            {
                for (int i = FPages.Count - 1; i >= 0; i--)  // 对于跨页的，按最后位置所在页，所以倒序
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

            // 因为行首可能是分页，所以需要从行首开始判断跨页
            for (int i = FPageData.Items[aStartItemNo].FirstDItemNo; i >= 0; i--)
            {
                if (FPageData.DrawItems[i].LineFirst)
                {
                    vPrioDrawItemNo = i;
                    break;
                }
            }

            if (vPrioDrawItemNo == FPages[vPageIndex].StartDrawItemNo)
            {
                FPages.RemoveRange(vPageIndex, FPages.Count - vPageIndex);  // 删除当前页一直到最后
                
                // 从上一页最后开始计算分页
                vPageIndex--;
                if (vPageIndex >= 0)
                    FPages[vPageIndex].EndDrawItemNo = -1;
            }
            else  // 行首不是页的第一个DrawItem
                FPages.RemoveRange(vPageIndex + 1, FPages.Count - vPageIndex - 1);  // 删除当前页后面的，准备格式化
            
            if (FPages.Count == 0)
            {
            
                vPage = new HCPage();
                vPage.StartDrawItemNo = vPrioDrawItemNo;
                FPages.Add(vPage);
                vPageIndex = 0;
            }

            int vPageDataFmtTop = GetPageDataFmtTop(vPageIndex);
            int vContentHeight = GetContentHeight();
            int vPageDataFmtBottom = vPageDataFmtTop + vContentHeight;
            
            for (int i = vPrioDrawItemNo; i <= FPageData.DrawItems.Count - 1; i++)
            {
                if (FPageData.DrawItems[i].LineFirst)
                {
                    if (FPageData.Items[FPageData.DrawItems[i].ItemNo].StyleNo < HCStyle.Null)
                        _FormatRectItemCheckPageBreak(vPageDataFmtBottom, vContentHeight, ref vPageIndex, ref vPageDataFmtTop, i);
                    else
                        _FormatTextItemCheckPageBreak(vContentHeight, i, ref vPageDataFmtTop, ref vPageDataFmtBottom, ref vPageIndex);
                }
            }

            FPages[vPageIndex].EndDrawItemNo = FPageData.DrawItems.Count - 1;
            FActivePageIndex = GetPageIndexByCurrent();
            
            for (int i = FPageData.FloatItems.Count - 1; i >= 0; i--)  // 正文中删除页序号超过页总数的FloatItem
            {
                if (FPageData.FloatItems[i].PageIndex > FPages.Count - 1)
                    FPageData.FloatItems.RemoveAt(i);
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

        public bool MergeTableSelectCells()
        {
            HCFunction vEvent = delegate()
            {
                return FActiveData.MergeTableSelectCells();
            };

            return ActiveDataChangeByAction(vEvent);
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
            if (Result < FPageSize.PageMarginTopPix)
                Result = FPageSize.PageMarginTopPix;

            return Result;
        }

        public int GetContentHeight()
        {
            return FPageSize.PageHeightPix  // 节页面正文区域高度，即页面除页眉、页脚后净高
                - FPageSize.PageMarginBottomPix - GetHeaderAreaHeight();
        }

        public int GetContentWidth()
        {
            return FPageSize.PageWidthPix - FPageSize.PageMarginLeftPix - FPageSize.PageMarginRightPix;
        }

        public int GetFilmHeight()
        {
            return FPages.Count * (HC.PagePadding + FPageSize.PageHeightPix);
        }

        public int GetFilmWidth()
        {
            return FPages.Count * (HC.PagePadding + FPageSize.PageWidthPix);
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
                FPageData.MarkStyleUsed(aMark);
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

                aStream.WriteByte((byte)FPageOrientation);  // 纸张方向
                
                vBuffer = BitConverter.GetBytes(FPageNoVisible);
                aStream.Write(vBuffer, 0, vBuffer.Length);  // 是否显示页码
                
                FPageSize.SaveToStream(aStream);  // 页面参数
                
                bool vArea = aSaveParts.Contains(SectionArea.saHeader);  // 存页眉
                vBuffer = BitConverter.GetBytes(vArea);
                aStream.Write(vBuffer, 0, vBuffer.Length); 

                vArea = aSaveParts.Contains(SectionArea.saFooter);  // 存页脚
                vBuffer = BitConverter.GetBytes(vArea);
                aStream.Write(vBuffer, 0, vBuffer.Length); 

                vArea = aSaveParts.Contains(SectionArea.saPage);  // 存页面
                vBuffer = BitConverter.GetBytes(vArea);
                aStream.Write(vBuffer, 0, vBuffer.Length); 

                if (aSaveParts.Contains(SectionArea.saHeader))
                {
                    vBuffer = BitConverter.GetBytes(FHeaderOffset);
                    aStream.Write(vBuffer, 0, vBuffer.Length);
            
                    FHeader.SaveToStream(aStream);
                }
                
                if (aSaveParts.Contains(SectionArea.saFooter))
                    FFooter.SaveToStream(aStream);
                
                if (aSaveParts.Contains(SectionArea.saPage))
                    FPageData.SaveToStream(aStream);
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

        public void SaveToTextFile(string aFileName, System.Text.Encoding aEncoding)
        {
            FPageData.SaveToTextFile(aFileName, aEncoding);
        }

        public void LoadFromTextFile(string aFileName, Encoding aEncoding)
        {
            FPageData.LoadFromTextFile(aFileName, aEncoding);
            BuildSectionPages(0);
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
                FPageOrientation = (View.PageOrientation)vBuffer[0];  // 纸张方向

                vBuffer = BitConverter.GetBytes(FPageNoVisible);
                aStream.Read(vBuffer, 0, vBuffer.Length);  // 是否显示页码
                FPageNoVisible = BitConverter.ToBoolean(vBuffer, 0);
            }

            FPageSize.LoadToStream(aStream, aFileVersion);  // 页面参数
            FPageData.Width = GetContentWidth();

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
                FHeader.Width = FPageData.Width;
                FHeader.LoadFromStream(aStream, FStyle, aFileVersion);
            }

            if (vLoadParts.Contains(SectionArea.saFooter))
            {
                FFooter.Width = FPageData.Width;
                FFooter.LoadFromStream(aStream, FStyle, aFileVersion);
            }

            if (vLoadParts.Contains(SectionArea.saPage))
                FPageData.LoadFromStream(aStream, FStyle, aFileVersion);

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
            FHeader.ReFormat(0);
            Footer.ReFormat(0);
            FPageData.ReFormat(0);
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
        public System.Drawing.Printing.PaperKind PaperKind
        {
            get { return GetPaperKind(); }
            set { SetPaperKind(value); }
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

        public PageOrientation PageOrientation
        {
            get { return FPageOrientation; }
            set { SetPageOrientation(value); }
        }

        //
        public int PageWidthPix
        {
            get { return GetPageWidthPix(); }
            set { SetPageWidthPix(value); }
        }

        public int PageHeightPix
        {
            get { return GetPageHeightPix(); }
            set { SetPageHeightPix(value); }
        }

        public int PageMarginTopPix
        {
            get { return GetPageMarginTopPix(); }
            set { SetPageMarginTopPix(value); }
        }

        public int PageMarginLeftPix
        {
            get { return GetPageMarginLeftPix(); }
            set { SetPageMarginLeftPix(value); }
        }

        public int PageMarginRightPix
        {
            get { return GetPageMarginRightPix(); }
            set { SetPageMarginRightPix(value); }
        }

        public int PageMarginBottomPix
        {
            get { return GetPageMarginBottomPix(); }
            set { SetPageMarginBottomPix(value); }
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

        public HCPageData PageData
        {
            get { return FPageData; }
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

        /// <summary> 文档所有部分是否只读 </summary>
        public bool ReadOnly
        {
            get { return GetReadOnly(); }
            set { SetReadOnly(value); }
        }

        public EventHandler OnDataChanged
        {
            get { return FOnDataChanged; }
            set { FOnDataChanged = value; }
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

        public SectionDataItemNotifyEventHandler OnInsertItem
        {
            get { return FOnInsertItem; }
            set { FOnInsertItem = value; }
        }

        public SectionDataItemNotifyEventHandler OnRemoveItem
        {
            get { return FOnRemoveItem; }
            set { FOnRemoveItem = value; }
        }

        public DataItemEventHandler OnItemResized
        {
            get { return FOnItemResized; }
            set { FOnItemResized = value; }
        }

        public SectionDataItemMouseEventHandler OnItemMouseUp
        {
            get { return FOnItemMouseUp; }
            set { FOnItemMouseUp = value; }
        }

        public SectionPagePaintEventHandler OnPaintHeader
        {
            get { return FOnPaintHeader; }
            set { FOnPaintHeader = value; }
        }

        public SectionPagePaintEventHandler OnPaintFooter
        {
            get { return FOnPaintFooter; }
            set { FOnPaintFooter = value; }
        }

        public SectionPagePaintEventHandler OnPaintPage
        {
            get { return FOnPaintPage; }
            set { FOnPaintPage = value; }
        }

        public SectionPagePaintEventHandler OnPaintWholePageBefor
        {
            get { return FOnPaintWholePageBefor; }
            set { FOnPaintWholePageBefor = value; }
        }

        public SectionPagePaintEventHandler OnPaintWholePageAfter
        {
            get { return FOnPaintWholePageAfter; }
            set { FOnPaintWholePageAfter = value; }
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

        public StyleItemEventHandler OnCreateItemByStyle
        {
            get { return FOnCreateItemByStyle; }
            set { FOnCreateItemByStyle = value; }
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

        protected HCPageSize PageSize
        {
            get { return FPageSize; }
            set { FPageSize = value; }
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
            return PageData.ToHtml(aPath);
        }

        public void ToXml(XmlElement aNode)
        {
            aNode.Attributes["symmargin"].Value = SymmetryMargin.ToString(); // 是否对称页边距
            aNode.Attributes["ori"].Value = ((byte)PageOrientation).ToString();  // 纸张方向
            aNode.Attributes["pagenovisible"].Value = PageNoVisible.ToString();  // 是否显示页码

            aNode.Attributes["pagesize"].Value =  // 纸张大小
                ((int)PageSize.PaperKind).ToString()
                + "," + string.Format("#.#", PageSize.PaperWidth)
                + "," + string.Format("#.#", PageSize.PaperHeight) ;

            aNode.Attributes["margin"].Value =  // 边距
                string.Format("#.#", PageSize.PaperMarginLeft) + ","
                + string.Format("#.#", PageSize.PaperMarginTop) + ","
                + string.Format("#.#", PageSize.PaperMarginRight) + ","
                + string.Format("#.#", PageSize.PaperMarginBottom);

            // 存页眉
            XmlElement vNode = aNode.OwnerDocument.CreateElement("header");
            vNode.Attributes["offset"].Value = HeaderOffset.ToString();
            Header.ToXml(vNode);
            aNode.AppendChild(vNode);

            // 存页脚
            vNode = aNode.OwnerDocument.CreateElement("footer");
            Footer.ToXml(vNode);
            aNode.AppendChild(vNode);

            // 存页面
            vNode = aNode.OwnerDocument.CreateElement("page");
            PageData.ToXml(vNode);
            aNode.AppendChild(vNode);
        }

        public void ParseXml(XmlElement aNode)
        {
            SymmetryMargin = bool.Parse(aNode.Attributes["symmargin"].Value);  // 是否对称页边距
            PageOrientation = (PageOrientation)(byte.Parse(aNode.Attributes["ori"].Value));  // 纸张方向

            PageNoVisible = bool.Parse(aNode.Attributes["pagenovisible"].Value);  // 是否对称页边距
            
            string[] vStrings = aNode.Attributes["pagesize"].Value.Split(new string[] { "," }, StringSplitOptions.None);
            PageSize.PaperKind = (PaperKind)(int.Parse(vStrings[0]));
            PageSize.PaperWidth = int.Parse(vStrings[1]);
            PageSize.PaperHeight = int.Parse(vStrings[2]);

            vStrings = aNode.Attributes["margin"].Value.Split(new string[] { "," }, StringSplitOptions.None);
            PageSize.PaperMarginLeft = int.Parse(vStrings[0]);
            PageSize.PaperMarginTop = int.Parse(vStrings[1]);
            PageSize.PaperMarginRight = int.Parse(vStrings[2]);
            PageSize.PaperMarginBottom = int.Parse(vStrings[3]);

            for (int i = 0; i <= aNode.ChildNodes.Count - 1; i++)
            {
                if (aNode.ChildNodes[i].Name == "header")
                {
                    HeaderOffset = int.Parse(aNode.ChildNodes[i].Attributes["offset"].Value);
                    Header.ParseXml(aNode.ChildNodes[i] as XmlElement);
                }
                else
                if (aNode.ChildNodes[i].Name == "footer")
                    Footer.ParseXml(aNode.ChildNodes[i] as XmlElement);
                else
                if (aNode.ChildNodes[i].Name == "page")
                    PageData.ParseXml(aNode.ChildNodes[i] as XmlElement);
            }

            BuildSectionPages(0);
        }
    }
}
