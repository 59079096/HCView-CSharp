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
    public class HCDomain : HCObject
    {

        private int FBeginNo, FEndNo;

        public HCDomain()
        {
            Clear();
        }

        public void Clear()
        {
            FBeginNo = -1;
            FEndNo = -1;
        }

        public bool Contain(int AItemNo)
        {
            return (AItemNo >= FBeginNo) && (AItemNo <= FEndNo);
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

    public delegate HCCustomItem StyleItemEventHandler(HCCustomData AData, int AStyleNo);

    public class HCRichData : HCUndoRichData  // 富文本数据类，可做为其他显示富文本类的基类
    {
        private List<int> FDomainStartDeletes;  // 仅用于选中删除时，当域起始结束都选中时，删除了结束后标明起始的可删除

        private HCDomain FHotDomain,  // 当前高亮域
            FActiveDomain;  // 当前激活域

        private IntPtr FHotDomainRGN, FActiveDomainRGN;

        private bool FDrawActiveDomainRegion, FDrawHotDomainRegion;  // 是否绘制域边框

        private StyleItemEventHandler FOnCreateItemByStyle;

        private void GetDomainFrom(int AItemNo, int AOffset, HCDomain ADomain)
        {
            ADomain.Clear();
            if ((AItemNo < 0) || (AOffset < 0))
                return;

            /* 找起始标识 }*/
            int vCount = 0;
            // 确定往前找的起始位置
            int vStartNo = AItemNo;
            int vEndNo = AItemNo;
            if (Items[AItemNo] is HCDomainItem)
            {
                if ((Items[AItemNo] as HCDomainItem).MarkType == MarkType.cmtBeg)
                {
                    if (AOffset == HC.OffsetAfter)
                    {
                        ADomain.BeginNo = AItemNo;  // 当前即为起始标识
                        vEndNo = AItemNo + 1;
                    }
                    else  // 光标在前面
                    {
                        if (AItemNo > 0)
                            vStartNo = AItemNo - 1; // 从前一个往前
                        else  // 是在第一个前面
                            return;  // 不用找了
                    }
                }
                else  // 查找位置是结束标记
                {
                    if (AOffset == HC.OffsetAfter)
                    {
                        if (AItemNo < Items.Count - 1)
                            vEndNo = AItemNo + 1;
                        else  // 是最后一个后面
                            return;  // 不用找了
                    }
                    else  // 光标在前面
                    {
                        ADomain.EndNo = AItemNo;
                        vStartNo = AItemNo - 1;
                    }
                }
            }
                
            if (ADomain.BeginNo < 0)
            {
                for (int i = vStartNo; i >= 0; i--)  // 找
                {
                    if (Items[i] is HCDomainItem)
                    {
                        if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtBeg)
                        {
                            if (vCount != 0)
                                vCount--;
                            else
                            {
                                ADomain.BeginNo = i;
                                break;
                            }
                        }
                        else  // 结束标记
                            vCount++;  // 有嵌套
                    }
                }
            }
                
            /* 找结束标识 }*/
            if ((ADomain.BeginNo >= 0) && (ADomain.EndNo < 0))
            {
                vCount = 0;
                for (int i = vEndNo; i <= Items.Count - 1; i++)
                {
                    if (Items[i] is HCDomainItem)
                    {
                        if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtEnd)
                        {
                            if (vCount != 0)
                                vCount--;
                            else
                            {
                                ADomain.EndNo = i;
                                break;
                            }
                        }
                        else  // 是起始标记
                            vCount++;  // 有嵌套
                    }
                }
                    
                if (ADomain.EndNo < 0)
                    throw new Exception("异常：获取数据组结束出错！");
            }
        }

        private HCDomain GetActiveDomain()
        {
            HCDomain Result = null;
            if (FActiveDomain.BeginNo >= 0)
                Result = FActiveDomain;

            return Result;
        }

        protected override HCCustomItem CreateItemByStyle(int AStyleNo)
        {
            HCCustomItem Result = null;

            if (FOnCreateItemByStyle != null)
                Result = FOnCreateItemByStyle(this, AStyleNo);

            if (Result == null)
                Result = base.CreateItemByStyle(AStyleNo);

            return Result;
        }

        protected override bool CanDeleteItem(int AItemNo)
        {
            bool Result = base.CanDeleteItem(AItemNo);
            if (Result)
            {
                if (Items[AItemNo].StyleNo == HCStyle.Domain)
                {
                    if ((Items[AItemNo] as HCDomainItem).MarkType == MarkType.cmtEnd)
                    {
                        int vItemNo = GetDomainAnother(AItemNo);  // 找起始
                        Result = (vItemNo >= SelectInfo.StartItemNo) && (vItemNo <= SelectInfo.EndItemNo);
                        if (Result)
                            FDomainStartDeletes.Add(vItemNo);  // 记录下来
                    }
                    else  // 域起始标记
                        Result = FDomainStartDeletes.IndexOf(AItemNo) >= 0;  // 结束标识已经删除了
                }
            }

            return Result;
        }

        /// <summary> 用于从流加载完Items后，检查不合格的Item并删除 </summary>
        protected override int CheckInsertItemCount(int AStartNo, int  AEndNo)
        {
            int Result = base.CheckInsertItemCount(AStartNo, AEndNo);
            
            // 检查加载或粘贴等从流插入Items不匹配的域起始结束标识并删除
            int vDelCount = 0;
            for (int i = AStartNo; i <= AEndNo; i++)  // 从前往后找没有插入起始标识的域，删除单独的域结束标识
            {
                if (Items[i] is HCDomainItem)
                {
                    if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtEnd)
                    {
                        if (i < AEndNo)
                            Items[i + 1].ParaFirst = Items[i].ParaFirst;
                            
                        Items.RemoveAt(i);
                        vDelCount++;
                        
                        if ((i > AStartNo) && (i <= AEndNo - vDelCount))
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

            for (int i = AEndNo - vDelCount; i >= AStartNo; i--)  // 从后往前，找没有插入结束标识的域
            {
                if (Items[i] is HCDomainItem)
                {
                    if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtBeg)
                    {
                        if (i < AEndNo - vDelCount)
                            Items[i + 1].ParaFirst = Items[i].ParaFirst;
            
                        Items.RemoveAt(i);
                        vDelCount++;

                        if ((i > AStartNo) && (i <= AEndNo - vDelCount))
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

        protected override void DoDrawItemPaintBefor(HCCustomData AData, int ADrawItemNo, 
            RECT ADrawRect, int ADataDrawLeft, int ADataDrawBottom, int ADataScreenTop,
            int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoDrawItemPaintBefor(AData, ADrawItemNo, ADrawRect, ADataDrawLeft,
                ADataDrawBottom, ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);
            
            if (!APaintInfo.Print)
            {
                bool vDrawHotDomainBorde = false;
                bool vDrawActiveDomainBorde = false;
                int vItemNo = DrawItems[ADrawItemNo].ItemNo;
                
                if (FHotDomain.BeginNo >= 0)
                    vDrawHotDomainBorde = FHotDomain.Contain(vItemNo);
                
                if (FActiveDomain.BeginNo >= 0)
                    vDrawActiveDomainBorde = FActiveDomain.Contain(vItemNo);
                
                if (vDrawHotDomainBorde || vDrawActiveDomainBorde)
                {
                    IntPtr vDliRGN = (IntPtr)GDI.CreateRectRgn(ADrawRect.Left, ADrawRect.Top, ADrawRect.Right, ADrawRect.Bottom);
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

        #region
        private void DrawLineLastMrak(HCCanvas ACanvas, RECT ADrawRect, PaintInfo APaintInfo)
        {
            ACanvas.Pen.BeginUpdate();
            try
            {
                ACanvas.Pen.Width = 1;
                ACanvas.Pen.Style = HCPenStyle.psSolid;
                ACanvas.Pen.Color = HC.ColorActiveBorder;
            }
            finally
            {
                ACanvas.Pen.EndUpdate();
            }

            SIZE vPt = new SIZE();
            GDI.SetViewportExtEx(ACanvas.Handle, APaintInfo.WindowWidth, APaintInfo.WindowHeight, ref vPt);
            try
            {
                ACanvas.MoveTo(APaintInfo.GetScaleX(ADrawRect.Right) + 4,
                APaintInfo.GetScaleY(ADrawRect.Bottom) - 8);
                ACanvas.LineTo(APaintInfo.GetScaleX(ADrawRect.Right) + 6, APaintInfo.GetScaleY(ADrawRect.Bottom) - 8);
                ACanvas.LineTo(APaintInfo.GetScaleX(ADrawRect.Right) + 6, APaintInfo.GetScaleY(ADrawRect.Bottom) - 3);
                ACanvas.MoveTo(APaintInfo.GetScaleX(ADrawRect.Right),     APaintInfo.GetScaleY(ADrawRect.Bottom) - 3);
                ACanvas.LineTo(APaintInfo.GetScaleX(ADrawRect.Right) + 6, APaintInfo.GetScaleY(ADrawRect.Bottom) - 3);
                ACanvas.MoveTo(APaintInfo.GetScaleX(ADrawRect.Right) + 1, APaintInfo.GetScaleY(ADrawRect.Bottom) - 4);
                ACanvas.LineTo(APaintInfo.GetScaleX(ADrawRect.Right) + 1, APaintInfo.GetScaleY(ADrawRect.Bottom) - 1);
                ACanvas.MoveTo(APaintInfo.GetScaleX(ADrawRect.Right) + 2, APaintInfo.GetScaleY(ADrawRect.Bottom) - 5);
                ACanvas.LineTo(APaintInfo.GetScaleX(ADrawRect.Right) + 2, APaintInfo.GetScaleY(ADrawRect.Bottom));
            }
            finally
            {
                GDI.SetViewportExtEx(ACanvas.Handle, APaintInfo.GetScaleX(APaintInfo.WindowWidth),
                    APaintInfo.GetScaleY(APaintInfo.WindowHeight), ref vPt);
            }
        }
        #endregion

        protected override void DoDrawItemPaintAfter(HCCustomData AData, int ADrawItemNo, 
            RECT ADrawRect, int ADataDrawLeft, int ADataDrawBottom, int ADataScreenTop,
            int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoDrawItemPaintAfter(AData, ADrawItemNo, ADrawRect, ADataDrawLeft,
                ADataDrawBottom, ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);
            
            if (!APaintInfo.Print)
            {
                if (AData.Style.ShowLineLastMark)
                {
                    if ((ADrawItemNo < DrawItems.Count - 1) && DrawItems[ADrawItemNo + 1].ParaFirst)
                        DrawLineLastMrak(ACanvas, ADrawRect, APaintInfo);  // 段尾的换行符
                    else
                        if (ADrawItemNo == DrawItems.Count - 1)
                            DrawLineLastMrak(ACanvas, ADrawRect, APaintInfo);  // 段尾的换行符
                }
            }
        }

        public HCRichData(HCStyle AStyle) : base(AStyle)
        {
            FDomainStartDeletes = new List<int>();
            FHotDomain = new HCDomain();
            FActiveDomain = new HCDomain();
        }

        ~HCRichData()
        {

        }

        public override void Dispose()
        {
            base.Dispose();
            FHotDomain.Dispose();
            FActiveDomain.Dispose();
            //FDomainStartDeletes.Free;
        }

        public override HCCustomItem CreateDefaultDomainItem()
        {
            //return new HCDefaultDomainItemClass(this);
            object[] vobj = new object[1];
            vobj[0] = this;
            Type t = HCDomainItem.HCDefaultDomainItemClass;
            return t.GetConstructor(null).Invoke(vobj) as HCCustomItem;
        }

        public override HCCustomItem CreateDefaultTextItem()
        {
            //HCCustomItem Result = new HCDefaultTextItemClass();
            Type[] vTypes = new Type[1];
            vTypes[0] = typeof(string);
            object[] vobj = new object[1];
            vobj[0] = (object)"";
            Type t = HCTextItem.HCDefaultTextItemClass;

            HCCustomItem Result = t.GetConstructor(vTypes).Invoke(vobj) as HCCustomItem;

            if (Style.CurStyleNo < HCStyle.Null)
                Result.StyleNo = 0;
            else
                Result.StyleNo = Style.CurStyleNo;

            Result.ParaNo = Style.CurParaNo;

            if (OnCreateItem != null)
                OnCreateItem(Result, null);

            return Result;
        }

        public override void PaintData(int ADataDrawLeft, int ADataDrawTop, int ADataDrawBottom, 
            int ADataScreenTop, int ADataScreenBottom, int AVOffset, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (!APaintInfo.Print)
            {
                if (FDrawHotDomainRegion)
                    FHotDomainRGN = (IntPtr)GDI.CreateRectRgn(0, 0, 0, 0);
                
                if (FDrawActiveDomainRegion)
                    FActiveDomainRGN = (IntPtr)GDI.CreateRectRgn(0, 0, 0, 0);
            }
            
            base.PaintData(ADataDrawLeft, ADataDrawTop, ADataDrawBottom,
                ADataScreenTop, ADataScreenBottom, AVOffset, ACanvas, APaintInfo);
            
            if (!APaintInfo.Print)
            {
                Color vOldColor = ACanvas.Brush.Color;  // 因为使用Brush绘制边框所以需要缓存原颜色
                try
                {
                    if (FDrawHotDomainRegion)
                    {
                        ACanvas.Brush.Color = HC.ColorActiveBorder;
                        //FieldInfo vField = typeof(Brush).GetField("nativeBrush", BindingFlags.NonPublic | BindingFlags.Instance);
                        //IntPtr hbrush = (IntPtr)vField.GetValue(ACanvas.Brush);
                        GDI.FrameRgn(ACanvas.Handle, FHotDomainRGN, ACanvas.Brush.Handle, 1, 1);
                        GDI.DeleteObject(FHotDomainRGN);
                    }
                
                    if (FDrawActiveDomainRegion)
                    {
                        ACanvas.Brush.Color = Color.Blue;

                        //FieldInfo vField = typeof(Brush).GetField("nativeBrush", BindingFlags.NonPublic | BindingFlags.Instance);
                        //IntPtr hbrush = (IntPtr)vField.GetValue(ACanvas.Brush);

                        GDI.FrameRgn(ACanvas.Handle, FActiveDomainRGN, ACanvas.Brush.Handle, 1, 1);
                        GDI.DeleteObject(FActiveDomainRGN);
                    }
                }
                finally
                {
                    ACanvas.Brush.Color = vOldColor;
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

        public override void GetCaretInfo(int AItemNo, int AOffset, ref HCCaretInfo ACaretInfo)
        {
            base.GetCaretInfo(AItemNo, AOffset, ref ACaretInfo);
            
            // 赋值激活Group信息，清除在 MouseDown
            if (this.SelectInfo.StartItemNo >= 0)
            {
                HCCustomRichData vTopData = GetTopLevelData();
                if (vTopData == this)
                {
                    if (FActiveDomain.BeginNo >= 0)
                    {
                        FActiveDomain.Clear();
                        FDrawActiveDomainRegion = false;
                        Style.UpdateInfoRePaint();
                        }
                    // 获取当前光标处ActiveDeGroup信息
                    this.GetDomainFrom(this.SelectInfo.StartItemNo, this.SelectInfo.StartItemOffset, FActiveDomain);
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

                HCRichData vTopData = this.GetTopLevelDataAt(e.X, e.Y) as HCRichData;
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

        public override bool InsertItem(HCCustomItem AItem)
        {
            bool Result = base.InsertItem(AItem);
            if (Result)
            {
                Style.UpdateInfoRePaint();
                Style.UpdateInfoReCaret();
                Style.UpdateInfoReScroll();
            }

            return Result;
        }

        public override bool InsertItem(int AIndex, HCCustomItem AItem, bool AOffsetBefor = true)
        {
            bool Result = base.InsertItem(AIndex, AItem, AOffsetBefor);
            if (Result)
            {
                Style.UpdateInfoRePaint();
                Style.UpdateInfoReCaret();
                Style.UpdateInfoReScroll();
            }

            return Result;
        }

        /// <summary> 设置选中范围，仅供外部使用内部不使用 </summary>
        public void SetSelectBound(int AStartNo, int  AStartOffset, int  AEndNo, int  AEndOffset)
        {
            int vStartNo = -1, vEndNo = -1, vStartOffset = -1, vEndOffset = -1;
            if (AEndNo < 0)
            {
                vStartNo = AStartNo;
                vStartOffset = AStartOffset;
                vEndNo = -1;
                vEndOffset = -1;
            }
            else
            if (AEndNo >= AStartNo)
            {
                vStartNo = AStartNo;
                vEndNo = AEndNo;
                
                if (AEndNo == AStartNo)
                {
                    if (AEndOffset >= AStartOffset)
                    {
                        vStartOffset = AStartOffset;
                        vEndOffset = AEndOffset;
                    }
                    else  // 结束位置在起始前面
                    {
                        vStartOffset = AEndOffset;
                        vEndOffset = AStartOffset;
                    }
                }
                else  // 不在同一个Item
                {
                    vStartOffset = AStartOffset;
                    vEndOffset = AEndOffset;
                }
            }
            else  // AEndNo < AStartNo 从后往前选择
            {
                vStartNo = AEndNo;
                vStartOffset = AEndOffset;
                vEndNo = AStartNo;
                vEndOffset = vStartOffset;
            }
            
            SelectInfo.StartItemNo = AStartNo;
            SelectInfo.StartItemOffset = AStartOffset;
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
        }

        /// <summary> 光标选到指定Item的最后面 </summary>
        public void SelectItemAfterWithCaret(int AItemNo)
        {
            ReSetSelectAndCaret(AItemNo);
        }

        /// <summary> 光村选到最后一个Item的最后面 </summary>
        public void SelectLastItemAfterWithCaret()
        {
            SelectItemAfterWithCaret(Items.Count - 1);
        }

        /// <summary> 获取DomainItem配对的另一个ItemNo </summary>
        public int GetDomainAnother(int AItemNo)
        {
            int Result = -1;
            int vIgnore = 0;
            
            // 请外部保证AItemNo对应的是THCDomainItem
            HCDomainItem vDomain = this.Items[AItemNo] as HCDomainItem;
            if (vDomain.MarkType == MarkType.cmtEnd)
            {
                for (int i = AItemNo - 1; i >= 0; i--)  // 找起始标识
                {
                    if (Items[i].StyleNo == HCStyle.Domain)
                    {
                        if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtBeg)
                        {
                            if (vIgnore == 0)
                            {
                                Result = i;
                                break;
                            }
                            else
                                vIgnore--;
                        }
                        else
                            vIgnore++;
                        }
                    }
                }
            else  // 是起始标识
            {
                for (int i = AItemNo + 1; i <= this.Items.Count - 1; i++)  // 找结束标识
                {
                    if (Items[i].StyleNo == HCStyle.Domain)
                    {
                        if ((Items[i] as HCDomainItem).MarkType == MarkType.cmtEnd)
                        {
                            if (vIgnore == 0)
                            {
                                Result = i;
                                break;
                            }
                            else
                                vIgnore--;
                        }
                        else
                            vIgnore++;
                    }
                }
            }

            return Result;
        }

        #region

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
                    vText = (this.Items[AItemNo] as HCTextItem).GetTextPart(1, AOffset);
                    if (!AMatchCase)
                        vText = vText.ToUpper();
            
                    vPos = ReversePos(vKeyword, vText);  // 一个字符串在另一个字符串中最后出现的位置(用LastDelimiter不区分大小写)
                }
                else  // 向后找
                {
                    vText = (this.Items[AItemNo] as HCTextItem).GetTextPart(AOffset + 1,
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
        /// <param name="AKeyword">要查找的关键字</param>
        /// <param name="AForward">True：向前，False：向后</param>
        /// <param name="AMatchCase">True：区分大小写，False：不区分大小写</param>
        /// <returns>True：找到</returns>
        public bool Search(string AKeyword, bool AForward, bool  AMatchCase)
        {
            bool Result = false;
            string vKeyword = "";
            if (!AMatchCase)
                vKeyword = AKeyword.ToUpper();
            else
                vKeyword = AKeyword;
            
            int vItemNo = -1, vOffset = -1;
            if (AForward)
            {
                vItemNo = this.SelectInfo.StartItemNo;
                vOffset = this.SelectInfo.StartItemOffset;
            }
            else  // 向后找
            {
                if (this.SelectInfo.EndItemNo < 0)
                {
                    vItemNo = this.SelectInfo.StartItemNo;
                    vOffset = this.SelectInfo.StartItemOffset;
                }
                else  // 没有选中结束，从选中起始往后
                {
                    vItemNo = this.SelectInfo.EndItemNo;
                    vOffset = this.SelectInfo.EndItemOffset;
                }
            }

            Result = DoSearchByOffset(AKeyword, vKeyword, AForward, AMatchCase, vItemNo, vOffset);
            
            if (!Result)
            {
                if (AForward)
                {
                    for (int i = vItemNo - 1; i >= 0; i--)
                    {
                        if (DoSearchByOffset(AKeyword, vKeyword, AForward, AMatchCase, i, GetItemAfterOffset(i)))
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
                        if (DoSearchByOffset(AKeyword, vKeyword, AForward, AMatchCase, i, 0))
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
                    if (!AForward)
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

        public void GetCaretInfoCur(ref HCCaretInfo ACaretInfo)
        {
            if (Style.UpdateInfo.Draging)
                this.GetCaretInfo(this.MouseMoveItemNo, this.MouseMoveItemOffset, ref ACaretInfo);
            else
                this.GetCaretInfo(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, ref ACaretInfo);
        }

        public void TraverseItem(HCItemTraverse ATraverse)
        {
            if (ATraverse != null)
            {
                for (int i = 0; i <= Items.Count - 1; i++)
                {
                    if (ATraverse.Stop)
                        break;

                    ATraverse.Process(this, i, ATraverse.Tag, ref ATraverse.Stop);
                    if (Items[i].StyleNo < HCStyle.Null)
                        (Items[i] as HCCustomRectItem).TraverseItem(ATraverse);
                }
            }
        }

        public HCDomain HotDomain
        {
            get { return FHotDomain; }
        }

        public HCDomain ActiveDomain
        {
            get { return GetActiveDomain(); }
        }

        public StyleItemEventHandler OnCreateItemByStyle
        {
            get { return FOnCreateItemByStyle; }
            set { FOnCreateItemByStyle = value; }
        }
    }
}
