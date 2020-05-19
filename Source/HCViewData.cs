/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{             文档内各类对象高级管理单元                }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HC.Win32;
using System.Drawing;
using System.Reflection;
using System.IO;

namespace HC.View
{
    public delegate HCCustomItem StyleItemEventHandler(HCCustomData aData, int aStyleNo);
    public delegate bool OnCanEditEventHandler(object sender);
    public delegate bool TextEventHandler(HCCustomData aData, int aItemNo, int aOffset, string aText);

    public class HCViewData : HCViewDevData  // 富文本数据类，可做为其他显示富文本类的基类
    {
        private List<int> FDomainStartDeletes;  // 仅用于选中删除时，当域起始结束都选中时，删除了结束后标明起始的可删除
        private bool FCaretItemChanged = false;
        private HCDomainInfo FHotDomain,  // 当前高亮域
            FActiveDomain;  // 当前激活域

        private IntPtr FHotDomainRGN, FActiveDomainRGN;
        private StyleItemEventHandler FOnCreateItemByStyle;
        private OnCanEditEventHandler FOnCanEdit;
        private TextEventHandler FOnInsertTextBefor;
        private DataItemEventHandler FOnCaretItemChanged;
        private DataItemNoFunEventHandler FOnPaintDomainRegion;

        protected override bool DoAcceptAction(int aItemNo, int aOffset, HCAction aAction)
        {
            if (Style.States.Contain(HCState.hosLoading)
                || Style.States.Contain(HCState.hosUndoing)
                || Style.States.Contain(HCState.hosRedoing))
                return true;

            bool Result = true;
            if (aAction == HCAction.actDeleteItem)
            {
                if (Items[aItemNo].StyleNo == HCStyle.Domain)
                {
                    if ((Items[aItemNo] as HCDomainItem).MarkType == MarkType.cmtEnd)
                    {
                        int vItemNo = GetDomainAnother(aItemNo);  // 找起始
                        Result = (vItemNo >= SelectInfo.StartItemNo) && (vItemNo <= SelectInfo.EndItemNo);
                        if (Result)
                            FDomainStartDeletes.Add(vItemNo);  // 记录下来
                    }
                    else  // 域起始标记
                        Result = FDomainStartDeletes.IndexOf(aItemNo) >= 0;  // 结束标识已经删除了
                }
            }

            if (Result)
                Result = base.DoAcceptAction(aItemNo, aOffset, aAction);

            return Result;
        }

        protected override bool DoSaveItem(int aItemNo)
        {
            bool vResult = base.DoSaveItem(aItemNo);
            if (vResult && this.Style.States.Contain(HCState.hosCopying))  // 复制保存
            {
                if (Items[aItemNo].StyleNo == HCStyle.Domain)  // 是域标识
                {
                    int vItemNo = GetDomainAnother(aItemNo);  // 找起始
                    vResult = (vItemNo >= SelectInfo.StartItemNo) && (vItemNo <= SelectInfo.EndItemNo);
                }
            }

            return vResult;
        }

        /// <summary> 用于从流加载完Items后，检查不合格的Item并删除 </summary>
        protected override int CheckInsertItemCount(int aStartNo, int aEndNo)
        {
            int vResult = base.CheckInsertItemCount(aStartNo, aEndNo);
            if (this.Loading)
                return vResult;

            int vLevel = -1;
            HCDomainInfo vDomainInfo = new HCDomainInfo();
            GetDomainFrom(aStartNo, 0, vDomainInfo);
            if (vDomainInfo.BeginNo >= 0)
                vLevel = (Items[vDomainInfo.BeginNo] as HCDomainItem).Level;

            for (int i = aStartNo; i <= aEndNo; i++)
            {
                if (Items[i] is HCDomainItem)  // 域标识
                {
                    if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtBeg)  // 是起始
                        vLevel++;

                    (Items[i] as HCDomainItem).Level = (byte)vLevel;

                    if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtEnd)  // 是结束
                        vLevel--;
                }
            }

            return vResult;
        }

        protected override void DoCaretItemChanged()
        {
            FCaretItemChanged = true;
        }

        protected override void DoDrawItemPaintBefor(HCCustomData aData, int aItemNo, int aDrawItemNo, 
            RECT aDrawRect, int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop,
            int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoDrawItemPaintBefor(aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight,
                aDataDrawBottom, aDataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);

            if (!APaintInfo.Print)  // 拼接域范围
            {
                bool vDrawHotDomainBorde = false;
                bool vDrawActiveDomainBorde = false;
                
                if (this.Style.DrawHotDomainRegion && (FHotDomain.BeginNo >= 0))
                    vDrawHotDomainBorde = FHotDomain.Contain(aItemNo);
                
                if (this.Style.DrawActiveDomainRegion && (FActiveDomain.BeginNo >= 0))
                    vDrawActiveDomainBorde = FActiveDomain.Contain(aItemNo);

                if (vDrawHotDomainBorde || vDrawActiveDomainBorde)  // 在Hot域或激活域中
                {
                    IntPtr vDliRGN = (IntPtr)GDI.CreateRectRgn(aDrawRect.Left, aDrawRect.Top, 
                        aDrawRect.Left == aDrawRect.Right ? aDrawRect.Right + 3 : aDrawRect.Right, aDrawRect.Bottom);
                    try
                    {
                        if (vDrawHotDomainBorde)
                            GDI.CombineRgn(FHotDomainRGN, FHotDomainRGN, vDliRGN, GDI.RGN_OR);

                        if (vDrawActiveDomainBorde)
                            GDI.CombineRgn(FActiveDomainRGN, FActiveDomainRGN, vDliRGN, GDI.RGN_OR);
                    }
                    finally
                    {
                        GDI.DeleteObject(vDliRGN);
                    }
                }
            }
        }

        #region DoDrawItemPaintAfter 子方法
        private void DrawLineLastMrak(HCCanvas aCanvas, RECT aDrawRect, PaintInfo aPaintInfo)
        {
            aCanvas.Pen.BeginUpdate();
            try
            {
                aCanvas.Pen.Width = 1;
                aCanvas.Pen.Style = HCPenStyle.psSolid;
                aCanvas.Pen.Color = HC.clActiveBorder;
            }
            finally
            {
                aCanvas.Pen.EndUpdate();
            }

            if (aPaintInfo.ScaleX != 1)
            {
                SIZE vPt = new SIZE();
                GDI.SetViewportExtEx(aCanvas.Handle, aPaintInfo.WindowWidth, aPaintInfo.WindowHeight, ref vPt);
                try
                {
                    aCanvas.MoveTo(aPaintInfo.GetScaleX(aDrawRect.Right) + 4, aPaintInfo.GetScaleY(aDrawRect.Bottom) - 8);
                    aCanvas.LineTo(aPaintInfo.GetScaleX(aDrawRect.Right) + 6, aPaintInfo.GetScaleY(aDrawRect.Bottom) - 8);
                    aCanvas.LineTo(aPaintInfo.GetScaleX(aDrawRect.Right) + 6, aPaintInfo.GetScaleY(aDrawRect.Bottom) - 3);

                    aCanvas.MoveTo(aPaintInfo.GetScaleX(aDrawRect.Right), aPaintInfo.GetScaleY(aDrawRect.Bottom) - 3);
                    aCanvas.LineTo(aPaintInfo.GetScaleX(aDrawRect.Right) + 6, aPaintInfo.GetScaleY(aDrawRect.Bottom) - 3);

                    aCanvas.MoveTo(aPaintInfo.GetScaleX(aDrawRect.Right) + 1, aPaintInfo.GetScaleY(aDrawRect.Bottom) - 4);
                    aCanvas.LineTo(aPaintInfo.GetScaleX(aDrawRect.Right) + 1, aPaintInfo.GetScaleY(aDrawRect.Bottom) - 1);

                    aCanvas.MoveTo(aPaintInfo.GetScaleX(aDrawRect.Right) + 2, aPaintInfo.GetScaleY(aDrawRect.Bottom) - 5);
                    aCanvas.LineTo(aPaintInfo.GetScaleX(aDrawRect.Right) + 2, aPaintInfo.GetScaleY(aDrawRect.Bottom));
                }
                finally
                {
                    GDI.SetViewportExtEx(aCanvas.Handle, aPaintInfo.GetScaleX(aPaintInfo.WindowWidth),
                        aPaintInfo.GetScaleY(aPaintInfo.WindowHeight), ref vPt);
                }
            }
            else
            {
                aCanvas.MoveTo(aDrawRect.Right + 4, aDrawRect.Bottom - 8);
                aCanvas.LineTo(aDrawRect.Right + 6, aDrawRect.Bottom - 8);
                aCanvas.LineTo(aDrawRect.Right + 6, aDrawRect.Bottom - 3);

                aCanvas.MoveTo(aDrawRect.Right, aDrawRect.Bottom - 3);
                aCanvas.LineTo(aDrawRect.Right + 6, aDrawRect.Bottom - 3);

                aCanvas.MoveTo(aDrawRect.Right + 1, aDrawRect.Bottom - 4);
                aCanvas.LineTo(aDrawRect.Right + 1, aDrawRect.Bottom - 1);

                aCanvas.MoveTo(aDrawRect.Right + 2, aDrawRect.Bottom - 5);
                aCanvas.LineTo(aDrawRect.Right + 2, aDrawRect.Bottom);
            }
        }
        #endregion

        protected override void DoDrawItemPaintAfter(HCCustomData aData, int aItemNo, int aDrawItemNo, 
            RECT aDrawRect, int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop,
            int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoDrawItemPaintAfter(aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight,
                aDataDrawBottom, aDataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);
            
            if (!APaintInfo.Print)
            {
                if (aData.Style.ShowParaLastMark)
                {
                    if ((aDrawItemNo < DrawItems.Count - 1) && DrawItems[aDrawItemNo + 1].ParaFirst)
                        DrawLineLastMrak(ACanvas, aDrawRect, APaintInfo);  // 段尾的换行符
                    else
                        if (aDrawItemNo == DrawItems.Count - 1)
                            DrawLineLastMrak(ACanvas, aDrawRect, APaintInfo);  // 段尾的换行符
                }
            }
        }

        protected bool DoPaintDomainRegion(int itemNo)
        {
            if (itemNo < 0)
                return false;
            else
            if (FOnPaintDomainRegion != null)
                return FOnPaintDomainRegion(this, itemNo);
            else
                return true;
        }

        public HCViewData(HCStyle aStyle) : base(aStyle)
        {
            FCaretItemChanged = false;
            FDomainStartDeletes = new List<int>();
            FHotDomain = new HCDomainInfo();
            FHotDomain.Data = this;
            FActiveDomain = new HCDomainInfo();
            FActiveDomain.Data = this;
        }

        ~HCViewData()
        {
            FHotDomain.Dispose();
            FActiveDomain.Dispose();
            FDomainStartDeletes.Clear();
        }

        public override void Dispose()
        {
            base.Dispose();
            FHotDomain.Dispose();
            FActiveDomain.Dispose();
            //FDomainStartDeletes.Free;
        }

        public override HCCustomItem CreateItemByStyle(int aStyleNo)
        {
            HCCustomItem Result = null;

            if (FOnCreateItemByStyle != null)
                Result = FOnCreateItemByStyle(this, aStyleNo);

            if (Result == null)
                Result = base.CreateItemByStyle(aStyleNo);

            return Result;
        }
        public override void PaintData(int aDataDrawLeft, int aDataDrawTop, int aDataDrawRight, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, int aVOffset, int aFristDItemNo, int aLastDItemNo,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (!aPaintInfo.Print)
            {
                if (this.Style.DrawHotDomainRegion)
                    FHotDomainRGN = (IntPtr)GDI.CreateRectRgn(0, 0, 0, 0);
                
                if (this.Style.DrawActiveDomainRegion)
                    FActiveDomainRGN = (IntPtr)GDI.CreateRectRgn(0, 0, 0, 0);
            }
            
            base.PaintData(aDataDrawLeft, aDataDrawTop, aDataDrawRight, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, 
                aVOffset, aFristDItemNo, aLastDItemNo, aCanvas, aPaintInfo);
            
            if (!aPaintInfo.Print)
            {
                Color vOldColor = aCanvas.Brush.Color;  // 因为使用Brush绘制边框所以需要缓存原颜色
                try
                {
                    if (Style.DrawHotDomainRegion)
                    {
                        if (DoPaintDomainRegion(FHotDomain.BeginNo))
                        {
                            aCanvas.Brush.Color = HC.clActiveBorder;
                            //FieldInfo vField = typeof(Brush).GetField("nativeBrush", BindingFlags.NonPublic | BindingFlags.Instance);
                            //IntPtr hbrush = (IntPtr)vField.GetValue(ACanvas.Brush);
                            GDI.FrameRgn(aCanvas.Handle, FHotDomainRGN, aCanvas.Brush.Handle, 1, 1);
                        }

                        GDI.DeleteObject(FHotDomainRGN);
                    }

                    if (Style.DrawActiveDomainRegion)
                    {
                        if (DoPaintDomainRegion(FActiveDomain.BeginNo))
                        {
                            aCanvas.Brush.Color = Color.Blue;
                            //FieldInfo vField = typeof(Brush).GetField("nativeBrush", BindingFlags.NonPublic | BindingFlags.Instance);
                            //IntPtr hbrush = (IntPtr)vField.GetValue(ACanvas.Brush);
                            GDI.FrameRgn(aCanvas.Handle, FActiveDomainRGN, aCanvas.Brush.Handle, 1, 1);
                        }

                        GDI.DeleteObject(FActiveDomainRGN);
                    }
                }
                finally
                {
                    aCanvas.Brush.Color = vOldColor;
                }
            }
        }

        public override void InitializeField()
        {
            base.InitializeField();
            if (FActiveDomain != null)
                FActiveDomain.Clear();
           
            if (FHotDomain != null)
                FHotDomain.Clear();
        }

        public override void GetCaretInfo(int aItemNo, int aOffset, ref HCCaretInfo aCaretInfo)
        {
            base.GetCaretInfo(aItemNo, aOffset, ref aCaretInfo);

            bool vRePaint = false;
            // 赋值激活Group信息，清除在 MouseDown
            if (this.SelectInfo.StartItemNo >= 0)
            {
                HCCustomData vTopData = GetTopLevelData();
                if (vTopData == this)
                {
                    if (this.Style.DrawActiveDomainRegion && (FActiveDomain.BeginNo >= 0))
                        vRePaint = true;

                    // 获取当前光标处ActiveDeGroup信息
                    GetDomainFrom(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, FActiveDomain);

                    if (this.Style.DrawActiveDomainRegion && (FActiveDomain.BeginNo >= 0))
                        vRePaint = true;
                }
            }
            else
            if (this.Style.DrawActiveDomainRegion && (FActiveDomain.BeginNo >= 0))
            {
                FActiveDomain.Clear();
                vRePaint = true;
            }

            if (vRePaint)
                Style.UpdateInfoRePaint();

            if (FCaretItemChanged)
            {
                FCaretItemChanged = false;
                if (FOnCaretItemChanged != null)
                    FOnCaretItemChanged(this, Items[SelectInfo.StartItemNo]);
            }
        }

        public override bool DeleteSelected()
        {
            FDomainStartDeletes.Clear();
            return base.DeleteSelected();
        }

        public bool DeleteActiveDomain()
        {
            if (SelectExists())
                return false;

            bool Result = false;
            if (FActiveDomain.BeginNo >= 0)
                Result = DeleteDomain(FActiveDomain);
            else
            if (Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null)
            {
                Result = (Items[SelectInfo.StartItemNo] as HCCustomRectItem).DeleteActiveDomain();
                if (Result)
                {
                    int vFirstDrawItemNo = -1, vLastItemNo = -1;
                    GetFormatRange(ref vFirstDrawItemNo, ref vLastItemNo);
                    FormatPrepare(vFirstDrawItemNo, vLastItemNo);
                    ReFormatData(vFirstDrawItemNo, vLastItemNo);

                    Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();
                }
            }

            return Result;
        }

        public bool DeleteDomain(HCDomainInfo aDomain)
        {
            return DeleteDomainByItemNo(aDomain.BeginNo, aDomain.EndNo);
        }

        public bool DeleteDomainByItemNo(int aStartNo, int aEndNo)
        {
            if (aStartNo < 0)
                return false;

            Undo_New();

            int vFirstDrawItemNo = GetFormatFirstDrawItem(Items[aStartNo].FirstDItemNo);
            int vParaLastItemNo = GetParaLastItemNo(aEndNo);

            if (Items[aStartNo].ParaFirst)
            {
                if (aEndNo == vParaLastItemNo)
                {
                    if (aStartNo > 0)
                        vFirstDrawItemNo = GetFormatFirstDrawItem(Items[aStartNo].FirstDItemNo - 1);
                }
                else  // 域结束不是段尾，起始是段首
                {
                    UndoAction_ItemParaFirst(aEndNo + 1, 0, true);
                    Items[aEndNo + 1].ParaFirst = true;
                }
            }

            FormatPrepare(vFirstDrawItemNo, vParaLastItemNo);

            int vDelCount = 0;
            bool vBeginPageBreak = Items[aStartNo].PageBreak;

            for (int i = aEndNo; i >= aStartNo; i--)  // 删除域及域范围内的Ite
            {
                UndoAction_DeleteItem(i, 0);
                Items.Delete(i);
                vDelCount++;
            }

            FActiveDomain.Clear();

            if (aStartNo == 0)  // 删除完了
            {
                HCCustomItem vItem = CreateDefaultTextItem();
                vItem.ParaFirst = true;
                vItem.PageBreak = vBeginPageBreak;

                Items.Insert(aStartNo, vItem);
                UndoAction_InsertItem(aStartNo, 0);
                vDelCount--;
            }

            ReFormatData(vFirstDrawItemNo, vParaLastItemNo - vDelCount, -vDelCount);

            this.InitializeField();
            if (aStartNo > Items.Count - 1)
                ReSetSelectAndCaret(aStartNo - 1);
            else
                ReSetSelectAndCaret(aStartNo, 0);

            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();

            return true;
        }

        public override void MouseMove(MouseEventArgs e)
        {
            bool vRePaint = this.Style.DrawHotDomainRegion && (FHotDomain.BeginNo >= 0);
            FHotDomain.Clear();
            base.MouseMove(e);
            if (!this.MouseMoveRestrain)
            {
                this.GetDomainFrom(this.MouseMoveItemNo, this.MouseMoveItemOffset, FHotDomain);
                HCViewData vTopData = this.GetTopLevelDataAt(e.X, e.Y) as HCViewData;
                if ((vTopData == this) || (vTopData.HotDomain.BeginNo < 0))
                {
                    if (this.Style.DrawHotDomainRegion && (FHotDomain.BeginNo >= 0))
                        vRePaint = true;
                }
            }

            if (vRePaint)
                Style.UpdateInfoRePaint();
        }

        public override bool InsertItem(HCCustomItem aItem)
        {
            bool Result = base.InsertItem(aItem);
            if (Result)
            {
                Style.UpdateInfoRePaint();
                Style.UpdateInfoReCaret();
                Style.UpdateInfoReScroll();
            }

            return Result;
        }

        public override bool InsertItem(int aIndex, HCCustomItem aItem, bool aOffsetBefor = true)
        {
            bool Result = base.InsertItem(aIndex, aItem, aOffsetBefor);
            if (Result)
            {
                Style.UpdateInfoRePaint();
                Style.UpdateInfoReCaret();
                Style.UpdateInfoReScroll();
            }

            return Result;
        }

        public override bool CanEdit()
        {
            bool Result = base.CanEdit();
            if ((Result) && (FOnCanEdit != null))
                Result = FOnCanEdit(this);

            return Result;
        }

        public override bool DoInsertTextBefor(int aItemNo, int aOffset, string aText)
        {
            bool vResult = base.DoInsertTextBefor(aItemNo, aOffset, aText);
            if (vResult && (FOnInsertTextBefor != null))
                vResult = FOnInsertTextBefor(this, aItemNo, aOffset, aText);

            return vResult;
        }

        public bool InsertDomain(HCDomainItem aMouldDomain)
        {
            if (!CanEdit())
                return false;

            bool Result = false;
            HCDomainItem vDomainItem = null;
            Undo_GroupBegin(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            try
            {
                this.Style.States.Include(HCState.hosBatchInsert);
                try
                {
                    // 插入头
                    vDomainItem = CreateDefaultDomainItem() as HCDomainItem;
                    if (aMouldDomain != null)
                        vDomainItem.Assign(aMouldDomain);

                    vDomainItem.MarkType = MarkType.cmtBeg;
                    if (FActiveDomain.BeginNo >= 0)
                        vDomainItem.Level = (byte)((Items[FActiveDomain.BeginNo] as HCDomainItem).Level + 1);

                    Result = InsertItem(vDomainItem);

                    if (Result)  // 插入尾
                    {
                        vDomainItem = CreateDefaultDomainItem() as HCDomainItem;
                        if (aMouldDomain != null)
                            vDomainItem.Assign(aMouldDomain);

                        vDomainItem.MarkType = MarkType.cmtEnd;
                        if (FActiveDomain.BeginNo >= 0)
                            vDomainItem.Level = (byte)((Items[FActiveDomain.BeginNo] as HCDomainItem).Level + 1);

                        Result = InsertItem(vDomainItem);
                    }
                }
                finally
                {
                    this.Style.States.Exclude(HCState.hosBatchInsert);
                }
            }
            finally
            {
                Undo_GroupEnd(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            }

            return Result;
        }

        public void GetDomainStackFrom(int aItemNo, int aOffset, Stack<HCDomainInfo> aDomainStack)
        {
            HCDomainInfo vDomainInfo;
            for (int i = 0; i < aItemNo; i++)
            {
                if (Items[i] is HCDomainItem)
                {
                    if (HCDomainItem.IsBeginMark(Items[i]))
                    {
                        vDomainInfo = new HCDomainInfo();
                        vDomainInfo.Data = this;
                        vDomainInfo.BeginNo = i;
                        aDomainStack.Push(vDomainInfo);
                    }
                    else
                        aDomainStack.Pop();
                }
            }
        }

        public void GetDomainFrom(int aItemNo, int aOffset, HCDomainInfo aDomainInfo)
        {
            aDomainInfo.Clear();

            if ((aItemNo < 0) || (aOffset < 0))
                return;

            /* 找起始标识 }*/
            int vCount = 0;
            byte vLevel = 0;
            // 确定往前找的起始位置
            int vStartNo = aItemNo;
            int vEndNo = aItemNo;
            if (Items[aItemNo] is HCDomainItem)
            {
                if ((Items[aItemNo] as HCDomainItem).MarkType == MarkType.cmtBeg)
                {
                    if (aOffset == HC.OffsetAfter)
                    {
                        aDomainInfo.Data = this;
                        aDomainInfo.BeginNo = aItemNo;  // 当前即为起始标识
                        vLevel = (Items[aItemNo] as HCDomainItem).Level;
                        vEndNo = aItemNo + 1;
                    }
                    else  // 光标在前面
                    {
                        if (aItemNo > 0)
                            vStartNo = aItemNo - 1; // 从前一个往前
                        else  // 是在第一个前面
                            return;  // 不用找了
                    }
                }
                else  // 查找位置是结束标记
                {
                    if (aOffset == HC.OffsetAfter)
                    {
                        if (aItemNo < Items.Count - 1)
                            vEndNo = aItemNo + 1;
                        else  // 是最后一个后面
                            return;  // 不用找了
                    }
                    else  // 光标在前面
                    {
                        aDomainInfo.EndNo = aItemNo;
                        vStartNo = aItemNo - 1;
                    }
                }
            }

            if (aDomainInfo.BeginNo < 0)
            {
                vCount = 0;

                if (vStartNo < Items.Count / 2)  // 在前半程
                {
                    for (int i = vStartNo; i >= 0; i--)  // 先往前找起始
                    {
                        if (Items[i] is HCDomainItem)
                        {
                            if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtBeg)  // 起始标记
                            {
                                if (vCount != 0)
                                    vCount--;
                                else
                                {
                                    aDomainInfo.BeginNo = i;
                                    vLevel = (Items[i] as HCDomainItem).Level;
                                    break;
                                }
                            }
                            else  // 结束标记
                                vCount++;  // 有嵌套
                        }
                    }

                    if ((aDomainInfo.BeginNo >= 0) && (aDomainInfo.EndNo < 0))  // 找结束标识
                    {
                        for (int i = vEndNo; i <= Items.Count - 1; i++)
                        {
                            if (Items[i] is HCDomainItem)
                            {
                                if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtEnd)  // 是结尾
                                {
                                    if ((Items[i] as HCDomainItem).Level == vLevel)
                                    {
                                        aDomainInfo.EndNo = i;
                                        break;
                                    }
                                }
                            }
                        }

                        if (aDomainInfo.EndNo < 0)
                            throw new Exception("异常：获取数据组结束出错！");
                    }
                }
                else  // 在后半程
                {
                    for (int i = vEndNo; i <= this.Items.Count - 1; i++)  // 先往后找结
                    {
                        if (Items[i] is HCDomainItem)
                        {
                            if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtEnd)
                            {
                                if (vCount > 0)
                                    vCount--;
                                else
                                {
                                    aDomainInfo.EndNo = i;
                                    vLevel = (Items[i] as HCDomainItem).Level;
                                    break;
                                }
                            }
                            else
                                vCount++;
                        }
                    }

                    if ((aDomainInfo.EndNo >= 0) && (aDomainInfo.BeginNo < 0))
                    {
                        for (int i = vStartNo; i >= 0; i--)
                        {
                            if (Items[i] is HCDomainItem)
                            {
                                if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtBeg)
                                {
                                    if ((Items[i] as HCDomainItem).Level == vLevel)
                                    {
                                        aDomainInfo.BeginNo = i;
                                        break;
                                    }
                                }
                            }
                        }

                        if (aDomainInfo.BeginNo < 0)
                            throw new Exception("异常：获取域起始位置出错！");
                    }
                }
            }
            else
            if (aDomainInfo.EndNo < 0)
            {
                for (int i = vEndNo; i <= this.Items.Count - 1; i++)
                {
                    if (Items[i] is HCDomainItem)
                    {
                        if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtEnd)  // 是结尾
                        {
                            if ((Items[i] as HCDomainItem).Level == vLevel)
                            {
                                aDomainInfo.EndNo = i;
                                break;
                            }
                        }
                    }
                }

                if (aDomainInfo.EndNo < 0)
                    throw new Exception("异常：获取域起始位置出错！");
            }
        }

        /// <summary> 设置选中范围，仅供外部使用内部不使用 </summary>
        public void SetSelectBound(int aStartNo, int aStartOffset, int aEndNo, int aEndOffset, bool aSilence = true)
        {
            int vStartNo = -1, vEndNo = -1, vStartOffset = -1, vEndOffset = -1;
            if (aEndNo < 0)
            {
                vStartNo = aStartNo;
                vStartOffset = aStartOffset;
                vEndNo = -1;
                vEndOffset = -1;
            }
            else
            if (aEndNo >= aStartNo)
            {
                vStartNo = aStartNo;
                vEndNo = aEndNo;

                if (aEndNo == aStartNo)  // 同一个Item
                {
                    if (aEndOffset >= aStartOffset)  // 结束位置在起始后面
                    {
                        vStartOffset = aStartOffset;
                        vEndOffset = aEndOffset;
                    }
                    else  // 结束位置在起始前面
                    {
                        vStartOffset = aEndOffset;
                        vEndOffset = aStartOffset;
                    }
                }
                else  // 不在同一个Item
                {
                    vStartOffset = aStartOffset;
                    vEndOffset = aEndOffset;
                }
            }
            else  // AEndNo < AStartNo 从后往前选择
            {
                vStartNo = aEndNo;
                vStartOffset = aEndOffset;

                vEndNo = aStartNo;
                vEndOffset = vStartOffset;
            }

            this.AdjustSelectRange(ref vStartNo, ref vStartOffset, ref vEndNo, ref vEndOffset);
            this.MatchItemSelectState();

            if (!aSilence)
            {
                ReSetSelectAndCaret(vStartNo, aStartOffset, true);
                this.Style.UpdateInfoRePaint();
            }
        }

        /// <summary> 光标选到指定Item的最后面 </summary>
        public void SelectItemAfterWithCaret(int aItemNo)
        {
            ReSetSelectAndCaret(aItemNo);
        }

        /// <summary> 光标选到最后一个Item的最后面 </summary>
        public void SelectLastItemAfterWithCaret()
        {
            SelectItemAfterWithCaret(Items.Count - 1);
        }

        public void SelectFirstItemBeforWithCaret()
        {
            ReSetSelectAndCaret(0, 0);
        }

        /// <summary> 获取DomainItem配对的另一个ItemNo </summary>
        public int GetDomainAnother(int aItemNo)
        {
            int Result = -1;
            
            // 请外部保证AItemNo对应的是THCDomainItem
            HCDomainItem vDomainItem = this.Items[aItemNo] as HCDomainItem;
            if (vDomainItem.MarkType == MarkType.cmtEnd)
            {
                for (int i = aItemNo - 1; i >= 0; i--)  // 找起始标识
                {
                    if (Items[i].StyleNo == HCStyle.Domain)
                    {
                        if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtBeg)
                        {
                            if ((Items[i] as HCDomainItem).Level == vDomainItem.Level)
                            {
                                Result = i;
                                break;
                            }
                        }
                    }
                }
            }
            else  // 是起始标识
            {
                for (int i = aItemNo + 1; i <= this.Items.Count - 1; i++)  // 找结束标识
                {
                    if (Items[i].StyleNo == HCStyle.Domain)
                    {
                        if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtEnd)
                        {
                            if ((Items[i] as HCDomainItem).Level == vDomainItem.Level)
                            {
                                Result = i;
                                break;
                            }
                        }
                    }
                }
            }

            return Result;
        }

        #region Search子方法

        private int ReversePos(string SubStr, string  S)
        {
            int Result = 0;

            char[] arr = SubStr.ToCharArray();
            Array.Reverse(arr);
            string vSubStr = new string(arr);

            arr = S.ToCharArray();
            Array.Reverse(arr);
            string vS = new string(arr);


            int i = vS.IndexOf(vSubStr);
            if (i > 0)
            {
                i = S.Length - i - SubStr.Length + 2;
                Result = i;
            }

            return Result;
        }

        private bool DoSearchByOffset(string AKeyword, string vKeyword, bool AForward, bool AMatchCase, 
            int AItemNo, int AOffset)
        {
            string vText, vConcatText, vOverText;
            int vPos = -1, vItemNo = -1;

            bool Result = false;
            if (this.Items[AItemNo].StyleNo < HCStyle.Null)
            {
                Result = (this.Items[AItemNo] as HCCustomRectItem).Search(AKeyword, AForward, AMatchCase);
                
                if (Result)
                {
                    this.SelectInfo.StartItemNo = AItemNo;
                    this.SelectInfo.StartItemOffset = HC.OffsetInner;
                    this.SelectInfo.EndItemNo = -1;
                    this.SelectInfo.EndItemOffset = -1;
                }
            }
            else
            {
                if (AForward)
                {
                    vText = (this.Items[AItemNo] as HCTextItem).SubString(1, AOffset);
                    if (!AMatchCase)
                        vText = vText.ToUpper();
            
                    vPos = ReversePos(vKeyword, vText);  // 一个字符串在另一个字符串中最后出现的位置(用LastDelimiter不区分大小写)
                }
                else  // 向后找
                {
                    vText = (this.Items[AItemNo] as HCTextItem).SubString(AOffset + 1,
                    this.Items[AItemNo].Length - AOffset);
                    if (!AMatchCase)
                        vText = vText.ToUpper();
                        
                    vPos = vText.IndexOf(vKeyword);
                }
               
                if (vPos > 0)
                {
                    this.SelectInfo.StartItemNo = AItemNo;
                
                    if (AForward)
                        this.SelectInfo.StartItemOffset = vPos - 1;
                    else  // 向后找
                        this.SelectInfo.StartItemOffset = AOffset + vPos - 1;
            
                    this.SelectInfo.EndItemNo = AItemNo;
                    this.SelectInfo.EndItemOffset = this.SelectInfo.StartItemOffset + vKeyword.Length;
            
                    Result = true;
                }
                else  // 没找到匹配，尝试在同段相邻的TextItem合并后查找
                if ((vText != "") && (vKeyword.Length > 1))
                {
                    if (AForward)
                    {
                        vItemNo = AItemNo;
                        vConcatText = vText;
                        vOverText = "";

                        while ((vItemNo > 0)
                            && (!this.Items[vItemNo].ParaFirst)
                            && (this.Items[vItemNo - 1].StyleNo > HCStyle.Null))
                        {
                            vText = this.Items[vItemNo - 1].Text.Substring(vKeyword.Length - 1);  // 取后面比关键字少一个字符长度的，以便和当前末尾最后一个拼接
                            vOverText = vOverText + vText;  // 记录拼接了多少个字符
                            vConcatText = vText + vConcatText;  // 拼接后的字符
                            if (!AMatchCase)
                                vConcatText = vConcatText.ToUpper();
            
                            vPos = vConcatText.IndexOf(vKeyword);
                            if (vPos > 0)
                            {
                                this.SelectInfo.StartItemNo = vItemNo - 1;
                                this.SelectInfo.StartItemOffset = this.Items[vItemNo - 1].Length - (vText.Length - vPos) - 1;
                            
                                this.SelectInfo.EndItemNo = AItemNo;
                                this.SelectInfo.EndItemOffset = vPos + vKeyword.Length - 1  // 关键字最后字符的偏移位置
                                    - vText.Length;  // 减去最前面Item占的宽度
                                while (vItemNo < AItemNo)  // 减去中间Item的宽度
                                {
                                    this.SelectInfo.EndItemOffset = this.SelectInfo.EndItemOffset - this.Items[vItemNo].Length;
                                    vItemNo++;
                                }
            
                                Result = true;
                                break;
                            }
                            else  // 当前接着的没找到
                            {
                                if (vOverText.Length >= vKeyword.Length - 1)
                                    break;
                            }
            
                            vItemNo--;
                        }
                    }
                    else  // 向后，在同段中找
                    {
                        vItemNo = AItemNo;
                        vConcatText = vText;
                        vOverText = "";
            
                        while ((vItemNo < this.Items.Count - 1)
                            && (!this.Items[vItemNo + 1].ParaFirst)
                            && (this.Items[vItemNo + 1].StyleNo > HCStyle.Null))  // 同段后面的TextItem
                        {
                            vText = this.Items[vItemNo + 1].Text.Substring(0, vKeyword.Length - 1);  // 取后面比关键字少一个字符长度的，以便和当前末尾最后一个拼接
                            vOverText = vOverText + vText;  // 记录拼接了多少个字符
                            vConcatText = vConcatText + vText;  // 拼接后的字符
                            if (!AMatchCase)
                                vConcatText = vConcatText.ToUpper();
            
                            vPos = vConcatText.IndexOf(vKeyword);
                            if (vPos > 0)
                            {
                                this.SelectInfo.StartItemNo = AItemNo;
                                this.SelectInfo.StartItemOffset = AOffset + vPos - 1;
                            
                                this.SelectInfo.EndItemNo = vItemNo + 1;
                                this.SelectInfo.EndItemOffset = vPos + vKeyword.Length - 1  // 关键字最后字符的偏移位置
                                    - (this.Items[AItemNo].Length - AOffset);  // 减去最前面Item占的宽度
            
                                while (vItemNo >= AItemNo + 1)  // 减去中间Item的宽度
                                {
                                    this.SelectInfo.EndItemOffset = this.SelectInfo.EndItemOffset - this.Items[vItemNo].Length;
                                    vItemNo--;
                                }
            
                                Result = true;
                                break;
                            }
                            else  // 当前接着的没找到
                            {
                                if (vOverText.Length >= vKeyword.Length - 1)
                                    break;
                            }
            
                            vItemNo++;
                        }
                    }
                }
            }

            return Result;
        }

        #endregion

        /// <summary> 当前位置开始查找指定的内容 </summary>
        /// <param name="aKeyword">要查找的关键字</param>
        /// <param name="aForward">True：向前，False：向后</param>
        /// <param name="aMatchCase">True：区分大小写，False：不区分大小写</param>
        /// <returns>True：找到</returns>
        public bool Search(string aKeyword, bool aForward, bool  aMatchCase)
        {
            bool Result = false;
            string vKeyword = "";
            if (!aMatchCase)
                vKeyword = aKeyword.ToUpper();
            else
                vKeyword = aKeyword;
            
            int vItemNo = -1, vOffset = -1;
            if (this.SelectInfo.StartItemNo < 0)
            {
                vItemNo = 0;
                vOffset = 0;
            }
            else
            if (this.SelectInfo.EndItemNo >= 0)
            {
                vItemNo = this.SelectInfo.EndItemNo;
                vOffset = this.SelectInfo.EndItemOffset;
            }
            else
            {
                vItemNo = this.SelectInfo.StartItemNo;
                vOffset = this.SelectInfo.StartItemOffset;
            }
            
            Result = DoSearchByOffset(aKeyword, vKeyword, aForward, aMatchCase, vItemNo, vOffset);
            
            if (!Result)
            {
                if (aForward)
                {
                    for (int i = vItemNo - 1; i >= 0; i--)
                    {
                        if (DoSearchByOffset(aKeyword, vKeyword, aForward, aMatchCase, i, GetItemOffsetAfter(i)))
                        {
                            Result = true;
                            break;
                         }
                    }
                }
                else  // 向后找
                {
                    for (int i = vItemNo + 1; i <= this.Items.Count - 1; i++)
                    {
                        if (DoSearchByOffset(aKeyword, vKeyword, aForward, aMatchCase, i, 0))
                        {
                            Result = true;
                            break;
                        }
                    }
                }
            }

            if (!Result)
            {
                if (this.SelectInfo.EndItemNo >= 0)
                {
                    if (!aForward)
                    {
                        this.SelectInfo.StartItemNo = this.SelectInfo.EndItemNo;
                        this.SelectInfo.StartItemOffset = this.SelectInfo.EndItemOffset;
                    }

                    this.SelectInfo.EndItemNo = -1;
                    this.SelectInfo.EndItemOffset = -1;
                }
            }

            this.Style.UpdateInfoRePaint();
            this.Style.UpdateInfoReCaret();

            return Result;
        }

        public bool Replace(string aText)
        {
            return InsertText(aText);
        }

        public void GetCaretInfoCur(ref HCCaretInfo aCaretInfo)
        {
            if (Style.UpdateInfo.Draging)
                this.GetCaretInfo(this.MouseMoveItemNo, this.MouseMoveItemOffset, ref aCaretInfo);
            else
                this.GetCaretInfo(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, ref aCaretInfo);
        }

        public void TraverseItem(HCItemTraverse aTraverse)
        {
            if (aTraverse != null)
            {
                HCDomainInfo vDomainInfo;
                for (int i = 0; i <= Items.Count - 1; i++)
                {
                    if (aTraverse.Stop)
                        break;

                    if (Items[i] is HCDomainItem)
                    {
                        if (HCDomainItem.IsBeginMark(Items[i]))
                        {
                            vDomainInfo = new HCDomainInfo();
                            GetDomainFrom(i, HC.OffsetAfter, vDomainInfo);
                            aTraverse.DomainStack.Push(vDomainInfo);
                        }
                        else
                            aTraverse.DomainStack.Pop();
                    }

                    aTraverse.Process(this, i, aTraverse.Tag, aTraverse.DomainStack, ref aTraverse.Stop);
                    if (Items[i].StyleNo < HCStyle.Null)
                        (Items[i] as HCCustomRectItem).TraverseItem(aTraverse);
                }
            }
        }

        public void SaveDomainToStream(Stream aStream, int aDomainItemNo)
        {
            int vGroupBeg = -1;
            int vGroupEnd = GetDomainAnother(aDomainItemNo);
            if (vGroupEnd > aDomainItemNo)
                vGroupBeg = aDomainItemNo;
            else
            {
                vGroupBeg = vGroupEnd;
                vGroupEnd = aDomainItemNo;
            }

            HC._SaveFileFormatAndVersion(aStream);
            this.Style.SaveToStream(aStream);
            SaveItemToStream(aStream, vGroupBeg + 1, 0, vGroupEnd - 1, GetItemOffsetAfter(vGroupEnd - 1));
        }

        public DataItemEventHandler OnCaretItemChanged
        {
            get { return FOnCaretItemChanged; }
            set { FOnCaretItemChanged = value; }
        }

        public HCDomainInfo HotDomain
        {
            get { return FHotDomain; }
        }

        public HCDomainInfo ActiveDomain
        {
            get { return FActiveDomain; }
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

        public TextEventHandler OnInsertTextBefor
        {
            get { return FOnInsertTextBefor; }
            set { FOnInsertTextBefor = value; }
        }

        public DataItemNoFunEventHandler OnPaintDomainRegion
        {
            get { return FOnPaintDomainRegion; }
            set { FOnPaintDomainRegion = value; }
        }
    }
}
