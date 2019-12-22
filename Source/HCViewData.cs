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

namespace HC.View
{
    public class HCDomainInfo : HCObject
    {
        private int FBeginNo, FEndNo;

        public HCDomainInfo()
        {
            Clear();
        }

        public void Clear()
        {
            FBeginNo = -1;
            FEndNo = -1;
        }

        public bool Contain(int aItemNo)
        {
            return (aItemNo >= FBeginNo) && (aItemNo <= FEndNo);
        }

        public int BeginNo
        {
            get { return FBeginNo; }
            set { FBeginNo = value; }
        }

        public int EndNo
        {
            get { return FEndNo; }
            set { FEndNo = value; }
        }
    }

    public delegate HCCustomItem StyleItemEventHandler(HCCustomData aData, int aStyleNo);
    public delegate bool OnCanEditEventHandler(object sender);

    public class HCViewData : HCViewDevData  // 富文本数据类，可做为其他显示富文本类的基类
    {
        private List<int> FDomainStartDeletes;  // 仅用于选中删除时，当域起始结束都选中时，删除了结束后标明起始的可删除

        private HCDomainInfo FHotDomain,  // 当前高亮域
            FActiveDomain;  // 当前激活域

        private IntPtr FHotDomainRGN, FActiveDomainRGN;

        private bool FDrawActiveDomainRegion, FDrawHotDomainRegion;  // 是否绘制域边框

        private StyleItemEventHandler FOnCreateItemByStyle;
        private OnCanEditEventHandler FOnCanEdit;

        private void GetDomainFrom(int aItemNo, int aOffset, HCDomainInfo aDomainInfo)
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

        protected override bool CanDeleteItem(int aItemNo)
        {
            bool Result = base.CanDeleteItem(aItemNo);
            if (Result)
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
                else
                    Result = Items[aItemNo].CanAccept(0, HCItemAction.hiaRemove);
            }

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
        protected override int CheckInsertItemCount(int aStartNo, int  aEndNo)
        {
            int Result = base.CheckInsertItemCount(aStartNo, aEndNo);
            return Result;  // 目前的稳定性应该不会出现不匹配的问题了
            // 检查加载或粘贴等从流插入Items不匹配的域起始结束标识并删除
            int vDelCount = 0;
            for (int i = aStartNo; i <= aEndNo; i++)  // 从前往后找没有插入起始标识的域，删除单独的域结束标识
            {
                if (Items[i] is HCDomainItem)
                {
                    if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtEnd)
                    {
                        if (i < aEndNo)
                            Items[i + 1].ParaFirst = Items[i].ParaFirst;
                            
                        Items.RemoveAt(i);
                        vDelCount++;
                        
                        if ((i > aStartNo) && (i <= aEndNo - vDelCount))
                        {
                            if ((!Items[i - 1].ParaFirst)
                                && (!Items[i].ParaFirst)
                                && MergeItemText(Items[i - 1], Items[i]))  // 前后都不是段首，且能合并
                            {
                                Items.RemoveAt(i);
                                vDelCount++;
                            }
                        }
            
                        break;
                    }
                    else  // 是起始域标记，不用担心了
                        break;
                }
            }

            for (int i = aEndNo - vDelCount; i >= aStartNo; i--)  // 从后往前，找没有插入结束标识的域
            {
                if (Items[i] is HCDomainItem)
                {
                    if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtBeg)
                    {
                        if (i < aEndNo - vDelCount)
                            Items[i + 1].ParaFirst = Items[i].ParaFirst;
            
                        Items.RemoveAt(i);
                        vDelCount++;

                        if ((i > aStartNo) && (i <= aEndNo - vDelCount))
                        {
                            if ((!Items[i - 1].ParaFirst)
                                && (!Items[i].ParaFirst)
                                && MergeItemText(Items[i - 1], Items[i]))  // 前后都不是段首，且能合并
                            {
                                Items.RemoveAt(i);
                                vDelCount++;
                            }
                        }
            
                        break;
                    }
                    else  // 是结束域标记，不用担心了
                        break;
                }
            }

            Result = Result - vDelCount;

            return Result;
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
                int vItemNo = DrawItems[aDrawItemNo].ItemNo;
                
                if (FHotDomain.BeginNo >= 0)
                    vDrawHotDomainBorde = FHotDomain.Contain(vItemNo);
                
                if (FActiveDomain.BeginNo >= 0)
                    vDrawActiveDomainBorde = FActiveDomain.Contain(vItemNo);

                if (vDrawHotDomainBorde || vDrawActiveDomainBorde)  // 在Hot域或激活域中
                {
                    IntPtr vDliRGN = (IntPtr)GDI.CreateRectRgn(aDrawRect.Left, aDrawRect.Top, aDrawRect.Right, aDrawRect.Bottom);
                    try
                    {
                        if ((FHotDomain.BeginNo >= 0) && vDrawHotDomainBorde)
                            GDI.CombineRgn(FHotDomainRGN, FHotDomainRGN, vDliRGN, GDI.RGN_OR);

                        if ((FActiveDomain.BeginNo >= 0) && vDrawActiveDomainBorde)
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

        public HCViewData(HCStyle aStyle) : base(aStyle)
        {
            FDomainStartDeletes = new List<int>();
            FHotDomain = new HCDomainInfo();
            FActiveDomain = new HCDomainInfo();
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
                if (FDrawHotDomainRegion)
                    FHotDomainRGN = (IntPtr)GDI.CreateRectRgn(0, 0, 0, 0);
                
                if (FDrawActiveDomainRegion)
                    FActiveDomainRGN = (IntPtr)GDI.CreateRectRgn(0, 0, 0, 0);
            }
            
            base.PaintData(aDataDrawLeft, aDataDrawTop, aDataDrawRight, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, 
                aVOffset, aFristDItemNo, aLastDItemNo, aCanvas, aPaintInfo);
            
            if (!aPaintInfo.Print)
            {
                Color vOldColor = aCanvas.Brush.Color;  // 因为使用Brush绘制边框所以需要缓存原颜色
                try
                {
                    if (FDrawHotDomainRegion)
                    {
                        aCanvas.Brush.Color = HC.clActiveBorder;
                        //FieldInfo vField = typeof(Brush).GetField("nativeBrush", BindingFlags.NonPublic | BindingFlags.Instance);
                        //IntPtr hbrush = (IntPtr)vField.GetValue(ACanvas.Brush);
                        GDI.FrameRgn(aCanvas.Handle, FHotDomainRGN, aCanvas.Brush.Handle, 1, 1);
                        GDI.DeleteObject(FHotDomainRGN);
                    }
                
                    if (FDrawActiveDomainRegion)
                    {
                        aCanvas.Brush.Color = Color.Blue;

                        //FieldInfo vField = typeof(Brush).GetField("nativeBrush", BindingFlags.NonPublic | BindingFlags.Instance);
                        //IntPtr hbrush = (IntPtr)vField.GetValue(ACanvas.Brush);

                        GDI.FrameRgn(aCanvas.Handle, FActiveDomainRGN, aCanvas.Brush.Handle, 1, 1);
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
            
            // 赋值激活Group信息，清除在 MouseDown
            if (this.SelectInfo.StartItemNo >= 0)
            {
                HCCustomData vTopData = GetTopLevelData();
                if (vTopData == this)
                {
                    if (FActiveDomain.BeginNo >= 0)
                    {
                        FActiveDomain.Clear();
                        FDrawActiveDomainRegion = false;
                        Style.UpdateInfoRePaint();
                        }
                    // 获取当前光标处ActiveDeGroup信息
                    GetDomainFrom(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, FActiveDomain);

                    if (FActiveDomain.BeginNo >= 0)
                    {
                        FDrawActiveDomainRegion = true;
                        Style.UpdateInfoRePaint();
                    }
                }
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
            if (aDomain.BeginNo < 0)
                return false;
        
            Undo_New();
        
            int vBeginItemNo = aDomain.BeginNo;

            int vFirstDrawItemNo = GetFormatFirstDrawItem(Items[aDomain.BeginNo].FirstDItemNo);
            int vParaLastItemNo = GetParaLastItemNo(aDomain.EndNo);
        
            if (Items[aDomain.BeginNo].ParaFirst)
            {
                if (aDomain.EndNo == vParaLastItemNo)
                {
                    if (aDomain.BeginNo > 0)
                        vFirstDrawItemNo = GetFormatFirstDrawItem(Items[aDomain.BeginNo].FirstDItemNo - 1);
                }
                else  // 域结束不是段尾，起始是段首
                {
                    UndoAction_ItemParaFirst(aDomain.EndNo + 1, 0, true);
                    Items[aDomain.EndNo + 1].ParaFirst = true;
                }
            }
        
            FormatPrepare(vFirstDrawItemNo, vParaLastItemNo);
        
            int vDelCount = 0;
            bool vBeginPageBreak = Items[vBeginItemNo].PageBreak;

            for (int i = aDomain.EndNo; i >= aDomain.BeginNo; i--)  // 删除域及域范围内的Ite
            {
                UndoAction_DeleteItem(i, 0);
                Items.Delete(i);
                vDelCount++;
            }

            FActiveDomain.Clear();

            if (vBeginItemNo == 0)  // 删除完了
            {
                HCCustomItem vItem = CreateDefaultTextItem();
                vItem.ParaFirst = true;
                vItem.PageBreak = vBeginPageBreak;

                Items.Insert(vBeginItemNo, vItem);
                UndoAction_InsertItem(vBeginItemNo, 0);
                vDelCount--;
            }

            ReFormatData(vFirstDrawItemNo, vParaLastItemNo - vDelCount, -vDelCount);
        
            this.InitializeField();
            if (vBeginItemNo > Items.Count - 1)
                ReSetSelectAndCaret(vBeginItemNo - 1);
            else
                ReSetSelectAndCaret(vBeginItemNo, 0);

            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();

            return true;
        }


        public override void MouseDown(MouseEventArgs e)
        {
            if (FActiveDomain.BeginNo >= 0)
                Style.UpdateInfoRePaint();

            FActiveDomain.Clear();
            FDrawActiveDomainRegion = false;

            base.MouseDown(e);

            if (e.Button == MouseButtons.Right)
                Style.UpdateInfoReCaret();
        }

        public override void MouseMove(MouseEventArgs e)
        {
            if (FHotDomain.BeginNo >= 0)
                Style.UpdateInfoRePaint();

            FHotDomain.Clear();
            FDrawHotDomainRegion = false;

            base.MouseMove(e);

            if (!this.MouseMoveRestrain)
            {
                this.GetDomainFrom(this.MouseMoveItemNo, this.MouseMoveItemOffset, FHotDomain);

                HCViewData vTopData = this.GetTopLevelDataAt(e.X, e.Y) as HCViewData;
                if ((vTopData == this) || (!vTopData.FDrawHotDomainRegion))
                {
                    if (FHotDomain.BeginNo >= 0)
                    {
                        FDrawHotDomainRegion = true;
                        Style.UpdateInfoRePaint();
                    }
                }
            }
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
            
            SelectInfo.StartItemNo = aStartNo;
            SelectInfo.StartItemOffset = aStartOffset;

            if ((vEndNo < 0) || ((vEndNo == vStartNo) && (vEndOffset == vStartOffset)))
            {
                SelectInfo.EndItemNo = -1;
                SelectInfo.EndItemOffset = -1;
            }
            else
            {
                SelectInfo.EndItemNo = vEndNo;
                SelectInfo.EndItemOffset = vEndOffset;
            }

            if (!aSilence)
                ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, true);
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
                for (int i = 0; i <= Items.Count - 1; i++)
                {
                    if (aTraverse.Stop)
                        break;

                    aTraverse.Process(this, i, aTraverse.Tag, ref aTraverse.Stop);
                    if (Items[i].StyleNo < HCStyle.Null)
                        (Items[i] as HCCustomRectItem).TraverseItem(aTraverse);
                }
            }
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
    }
}
