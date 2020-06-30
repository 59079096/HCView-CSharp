/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                 文档对象基本管理单元                  }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using HC.Win32;
using System.Runtime.InteropServices;
using System.Xml;

namespace HC.View
{
    public class HCDomainInfo : HCObject
    {
        private int FBeginNo, FEndNo;
        public HCCustomData Data;

        public HCDomainInfo()
        {
            Clear();
        }

        public virtual void Clear()
        {
            Data = null;
            FBeginNo = -1;
            FEndNo = -1;
        }

        public virtual void Assign(HCDomainInfo source)
        {
            this.Data = source.Data;
            this.FBeginNo = source.BeginNo;
            this.FEndNo = source.EndNo;
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

    public delegate void TraverseItemEventHandle(HCCustomData aData, int aItemNo, int aTag, Stack<HCDomainInfo> aDomainStack, ref bool aStop);

    public class HCItemTraverse : Object
    {
        public HashSet<SectionArea> Areas;
        public int Tag;
        public bool Stop;
        public TraverseItemEventHandle Process;
        public Stack<HCDomainInfo> DomainStack;

        public HCItemTraverse()
        {
            Tag = 0;
            Stop = false;
            Areas = new HashSet<SectionArea>();
            DomainStack = new Stack<HCDomainInfo>();
        }
    }

    public class SelectInfo : object
    {
        private int FStartItemNo,  // 不能使用DrawItem记录，因为内容变动时Item的指定Offset对应的DrawItem，可能和变动前不一样
            FStartItemOffset,  // 选中起始在第几个字符后面，0表示在Item最前面
            FEndItemNo,
            FEndItemOffset;  // 选中结束在第几个字符后面

        private bool FStartRestrain;

        public SelectInfo()
        {
            this.Initialize();
        }

        ~SelectInfo()
        {

        }

        public virtual void Initialize()
        {
            FStartItemNo = -1;
            FStartItemOffset = -1;
            FStartRestrain = false;
            FEndItemNo = -1;
            FEndItemOffset = -1;
        }

        /// <summary> 选中起始Item序号 </summary>
        public int StartItemNo
        {
            get { return FStartItemNo; }
            set { FStartItemNo = value; }
        }

        public int StartItemOffset
        {
            get { return FStartItemOffset; }
            set { FStartItemOffset = value; }
        }

        public bool StartRestrain
        {
            get { return FStartRestrain; }
            set { FStartRestrain = value; }
        }

        /// <summary> 选中结束Item序号 </summary>
        public int EndItemNo
        {
            get { return FEndItemNo; }
            set { FEndItemNo = value; }
        }

        public int EndItemOffset
        {
            get { return FEndItemOffset; }
            set { FEndItemOffset = value; }
        }
    }

    public delegate void DataDomainItemNoEventHandler(HCCustomData aData, Stack<HCDomainInfo> aDomainStack, int aItemNo);
    public delegate void DataItemEventHandler(HCCustomData aData, HCCustomItem aItem);
    public delegate void DataItemNoEventHandler(HCCustomData aData, int aItemNo);
    public delegate bool DataItemNoFunEventHandler(HCCustomData aData, int aItemNo);
    public delegate bool DataActionEventHandler(HCCustomData aData, int aItemNo, int aOffset, HCAction aAction);

    public delegate void DrawItemPaintEventHandler(HCCustomData aData, int aItemNo,
      int aDrawItemNo, RECT aDrawRect, int aDataDrawLeft, int aDataDrawRight,
      int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
      HCCanvas aCanvas, PaintInfo aPaintInfo);

    public delegate void DrawItemPaintContentEventHandler(HCCustomData aData, int aItemNo,
      int aDrawItemNo, RECT aDrawRect, RECT aClearRect, string aDrawText,
      int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
      HCCanvas aCanvas, PaintInfo aPaintInfo);

    public class HCCustomData : HCObject
    {
        HCCustomData FParentData;
        private HCStyle FStyle;
        int FCurStyleNo, FCurParaNo;
        HCItems FItems;
        HCDrawItems FDrawItems;
        SelectInfo FSelectInfo;
        HashSet<DrawOption> FDrawOptions;
        bool FLoading;
        int FCaretDrawItemNo;  // 当前Item光标处的DrawItem限定其只在相关的光标处理中使用(解决同一Item分行后Offset为行尾时不能区分是上行尾还是下行始)

        DataItemEventHandler FOnInsertItem, FOnRemoveItem;
        DataItemNoFunEventHandler FOnSaveItem;
        GetUndoListEventHandler FOnGetUndoList;
        EventHandler FOnCurParaNoChange;
        DrawItemPaintEventHandler FOnDrawItemPaintBefor, FOnDrawItemPaintAfter;
        DrawItemPaintContentEventHandler FOnDrawItemPaintContent;

        private void DrawItemPaintBefor(HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            int vDCState = ACanvas.Save();
            try
            {
                this.DoDrawItemPaintBefor(aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight, aDataDrawBottom, 
                    aDataScreenTop, aDataScreenBottom, ACanvas, APaintInfo);
            }
            finally
            {
                ACanvas.Restore(vDCState);
            }
        }

        private void DrawItemPaintAfter(HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            int vDCState = aCanvas.Save();
            try
            {
                this.DoDrawItemPaintAfter(aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight, aDataDrawBottom, 
                    aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
            }
            finally
            {
                aCanvas.Restore(vDCState);
            }
        }

        private void DrawItemPaintContent(HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
            RECT aClearRect, string aDrawText, int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop,
            int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            int vDCState = aCanvas.Save();
            try
            {
                this.DoDrawItemPaintContent(aData, aItemNo, aDrawItemNo, aDrawRect, aClearRect, aDrawText,
                    aDataDrawLeft, aDataDrawRight, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
            }
            finally
            {
                aCanvas.Restore(vDCState);
            }
        }

        private void SetCurStyleNo(int value)
        {
            if (FCurStyleNo != value)
                FCurStyleNo = value;
        }

        private void SetCurParaNo(int value)
        {
            if (FCurParaNo != value)
            {
                FCurParaNo = value;
                if (FOnCurParaNoChange != null)
                    FOnCurParaNoChange(this, null);
            }
        }

        protected bool MergeItemText(HCCustomItem aDestItem, HCCustomItem aSrcItem)
        {
            bool Result = aDestItem.CanConcatItems(aSrcItem);
            if (Result)
                aDestItem.Text = aDestItem.Text + aSrcItem.Text;

            return Result;
        }

        /// <summary> Item成功合并到同段前一个Item </summary>
        protected bool MergeItemToPrio(int aItemNo)
        {
            return (aItemNo > 0) && (!Items[aItemNo].ParaFirst)
                && MergeItemText(Items[aItemNo - 1], Items[aItemNo]);
        }

        /// <summary> Item成功合并到同段后一个Item </summary>
        protected bool MergeItemToNext(int aItemNo)
        {
            return (aItemNo < Items.Count - 1) && (!Items[aItemNo + 1].ParaFirst)
                && MergeItemText(Items[aItemNo], Items[aItemNo + 1]);
        }

        protected int CalcContentHeight()
        {
            if (FDrawItems.Count > 0)
                return FDrawItems[DrawItems.Count - 1].Rect.Bottom - FDrawItems[0].Rect.Top;
            else
                return 0;
        }

        #region
        private void CheckItemSelectedState(int aItemNo)
        {
            if ((aItemNo > SelectInfo.StartItemNo) && (aItemNo < SelectInfo.EndItemNo))
                Items[aItemNo].SelectComplate();
            else
            if (aItemNo == SelectInfo.StartItemNo)
            {
                if (aItemNo == SelectInfo.EndItemNo)
                {
                    if (Items[aItemNo].StyleNo < HCStyle.Null)
                    {
                        if ((SelectInfo.StartItemOffset == HC.OffsetInner)
                          || (SelectInfo.EndItemOffset == HC.OffsetInner))
                            Items[aItemNo].SelectPart();
                        else
                            Items[aItemNo].SelectComplate();
                    
                    }
                    else  // TextItem
                    {
                        if ((SelectInfo.StartItemOffset == 0)
                          && (SelectInfo.EndItemOffset == Items[aItemNo].Length)) 
                            Items[aItemNo].SelectComplate();
                        else
                            Items[aItemNo].SelectPart();
                    }
                }
                else  // 选中在不同的Item，当前是起始
                {
                    if (SelectInfo.StartItemOffset == 0)
                        Items[aItemNo].SelectComplate();
                    else
                        Items[aItemNo].SelectPart();
                }
            }
            else  // 选中在不同的Item，当前是结尾 if AItemNo = SelectInfo.EndItemNo) then
            {
                if (Items[aItemNo].StyleNo < HCStyle.Null)
                {
                    if (SelectInfo.EndItemOffset == HC.OffsetAfter)
                        Items[aItemNo].SelectComplate();
                    else
                        Items[aItemNo].SelectPart();
                
                }
                else  // TextItem
                {
                    if (SelectInfo.EndItemOffset == Items[aItemNo].Length)
                        Items[aItemNo].SelectComplate();
                    else
                        Items[aItemNo].SelectPart();
                }
            }
        }
        #endregion

        /// <summary> 处理选中范围内Item的全选中、部分选中状态 </summary>
        protected void MatchItemSelectState()
        {
            if (SelectExists())
            {
                for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)  // 起始结束之间的按全选中处
                    CheckItemSelectedState(i);
            }
        }

        protected void GetParaItemRang(int aItemNo, ref int aFirstItemNo, ref int aLastItemNo)
        {
            aFirstItemNo = aItemNo;
            while (aFirstItemNo > 0)
            {
                if (FItems[aFirstItemNo].ParaFirst)
                    break;
                else
                    aFirstItemNo--;
            }

            aLastItemNo = aItemNo + 1;
            while (aLastItemNo < FItems.Count)
            {
                if (FItems[aLastItemNo].ParaFirst)
                    break;
                else
                    aLastItemNo++;
            }

            aLastItemNo--;
        }

        protected int GetParaFirstItemNo(int aItemNo)
        {
            int Result = aItemNo;
            while (Result > 0)
            {
                if (FItems[Result].ParaFirst)
                    break;
                else
                    Result--;
            }

            return Result;
        }

        protected int GetParaLastItemNo(int aItemNo)
        {
            // 目前需要外部自己约束AItemNo < FItems.Count
            int Result = aItemNo + 1;
            while (Result < FItems.Count)
            {
                if (FItems[Result].ParaFirst)
                    break;
                else
                    Result++;
            }

            Result--;

            return Result;
        }

        /// <summary> 取行第一个DrawItem对应的ItemNo(用于格式化时计算一个较小的ItemNo范围) </summary>
        protected int GetLineFirstItemNo(int aItemNo, int aOffset)
        {
            //int Result = aItemNo;
            int Result = GetDrawItemNoByOffset(aItemNo, aOffset);

            while (Result > 0)
            {
                if (DrawItems[Result].LineFirst)
                    break;
                else
                    Result--;
            }

            return Result;
        }

        /// <summary> 取行最后一个DrawItem对应的ItemNo(用于格式化时计算一个较小的ItemNo范围) </summary>
        protected int GetLineLastItemNo(int aItemNo, int aOffset)
        {
            int Result = aItemNo;
            int vLastDItemNo = GetDrawItemNoByOffset(aItemNo, aOffset) + 1;  // 下一个开始，否则行第一个获取最后一个时还是行第一个
            while (vLastDItemNo < FDrawItems.Count)
            {
                if (FDrawItems[vLastDItemNo].LineFirst)
                    break;
                else
                    vLastDItemNo++;
            }

            vLastDItemNo--;
            Result = DrawItems[vLastDItemNo].ItemNo;

            return Result;
        }

        /// <summary> 根据指定Item获取其所在行的起始和结束DrawItemNo </summary>
        /// <param name="AFirstItemNo1">指定</param>
        /// <param name="AFirstItemNo">起始</param>
        /// <param name="ALastItemNo">结束</param>
        protected virtual void GetLineDrawItemRang(ref int aFirstDItemNo, ref int aLastDItemNo)
        {
            while (aFirstDItemNo > 0)
            {
                if (FDrawItems[aFirstDItemNo].LineFirst)
                    break;
                else
                    aFirstDItemNo--;
            }

            aLastDItemNo = aFirstDItemNo + 1;
            while (aLastDItemNo < FDrawItems.Count)
            {
                if (FDrawItems[aLastDItemNo].LineFirst)
                    break;
                else
                    aLastDItemNo++;
            }

            aLastDItemNo--;
        }

        /// <summary> 返回字符串AText的分散分隔数量和各分隔的起始位置 </summary>
        /// <param name="aText">要计算的字符串</param>
        /// <param name="aCharIndexs">记录各分隔的起始位置</param>
        /// <returns>分散分隔数量</returns>
        protected int GetJustifyCount(string aText, List<int> aCharIndexs)
        {
            int Result = 0;
            if (aText == "")
                throw new Exception("异常：不能对空字符串计算分散!");

            if (aCharIndexs != null)
                aCharIndexs.Clear();

            for (int i = 1; i <= aText.Length; i++)
            {
                #if UNPLACEHOLDERCHAR
                if (HC.UnPlaceholderChar.IndexOf(aText[i - 1]) < 0)
                #endif
                {
                    Result++;
                    if (aCharIndexs != null)
                        aCharIndexs.Add(i);
                }
            }

            if (aCharIndexs != null)
                aCharIndexs.Add(aText.Length + 1);

            return Result;
        }

        protected void SetCaretDrawItemNo(int value)
        {
            int vItemNo;

            if (FCaretDrawItemNo != value)
            {
                if ((FCaretDrawItemNo >= 0) && (FCaretDrawItemNo < FDrawItems.Count))
                {
                    vItemNo = FDrawItems[FCaretDrawItemNo].ItemNo;
                    if ((value >= 0) && (vItemNo != FDrawItems[value].ItemNo))
                        FItems[vItemNo].Active = false;
                }
                else
                {
                    vItemNo = -1;
                }

                FCaretDrawItemNo = value;

                if (FStyle.States.Contain(HCState.hosLoading))
                    return;

                SetCurStyleNo(FItems[FDrawItems[FCaretDrawItemNo].ItemNo].StyleNo);
                SetCurParaNo(FItems[FDrawItems[FCaretDrawItemNo].ItemNo].ParaNo);

                if ((FCaretDrawItemNo >= 0) && (FDrawItems[FCaretDrawItemNo].ItemNo != vItemNo))
                {
                    if (FItems[FDrawItems[FCaretDrawItemNo].ItemNo].StyleNo < HCStyle.Null)
                    {
                        if (FSelectInfo.StartItemOffset == HC.OffsetInner)
                            FItems[FDrawItems[FCaretDrawItemNo].ItemNo].Active = true;
                    }
                    else
                    //if ((FSelectInfo.StartItemOffset > 0)  // 在Item上
                    //    && (FSelectInfo.StartItemOffset < FItems[FDrawItems[FCaretDrawItemNo].ItemNo].Length))
                    {
                        FItems[FDrawItems[FCaretDrawItemNo].ItemNo].Active = true;
                    }

                    DoCaretItemChanged();
                }
            }
        }

        protected int CalculateLineHeight(HCTextStyle aTextStyle, HCParaStyle aParaStyle)
        {
            int Result = aTextStyle.FontHeight;// THCStyle.GetFontHeight(ACanvas);  // 行高
            if (aParaStyle.LineSpaceMode == ParaLineSpaceMode.plsMin)
                return Result;

            if (aParaStyle.LineSpaceMode == ParaLineSpaceMode.plsFix)
            {
                int vLineSpacing = HCUnitConversion.MillimeterToPixY(aParaStyle.LineSpace * 0.3527f);
                if (vLineSpacing < Result)
                    return Result;
                else
                    return vLineSpacing;
            }

            if (FStyle.FormatVersion == 2)
            {
                switch (aParaStyle.LineSpaceMode)
                {
                    case ParaLineSpaceMode.pls115:
                        Result = Result + (int)Math.Round(aTextStyle.TextMetric_tmHeight * 0.15);
                        break;

                    case ParaLineSpaceMode.pls150:
                        Result = Result + (int)Math.Round(aTextStyle.TextMetric_tmHeight * 0.5);
                        break;

                    case ParaLineSpaceMode.pls200:
                        Result = Result + aTextStyle.TextMetric_tmHeight;
                        break;

                    case ParaLineSpaceMode.plsMult:
                        Result = Result + (int)Math.Round(aTextStyle.TextMetric_tmHeight * aParaStyle.LineSpace);
                        break;
                }

                return Result;
            }

            ushort vAscent = 0, vDescent = 0;
            if ((aTextStyle.OutMetSize > 0) && aTextStyle.CJKFont)
            {
                if ((aTextStyle.OutlineTextmetric_otmfsSelection & 128) != 0)
                {
                    vAscent = (ushort)aTextStyle.OutlineTextmetric_otmAscent;
                    vDescent = (ushort)(-aTextStyle.OutlineTextmetric_otmDescent);
                }
                else
                {
                    vAscent = HC.SwapBytes((ushort)aTextStyle.FontHeader_Ascender);
                    vDescent = (ushort)(-HC.SwapBytes((ushort)aTextStyle.FontHeader_Descender)); // 基线向下
                    Single vSizeScale = aTextStyle.Size / HCUnitConversion.FontSizeScale / aTextStyle.OutlineTextmetric_otmEMSquare;
                    vAscent = (ushort)Math.Ceiling(vAscent * vSizeScale);
                    vDescent = (ushort)Math.Ceiling(vDescent * vSizeScale);
                    int vLineSpacing = (ushort)Math.Ceiling(1.3 * (vAscent + vDescent));
                    int vDelta = vLineSpacing - (vAscent + vDescent);
                    int vLeading = vDelta / 2;
                    int vOtherLeading = vDelta - vLeading;
                    vAscent = (ushort)(vAscent + vLeading);
                    vDescent = (ushort)(vDescent + vOtherLeading);
                    Result = vAscent + vDescent;
                    switch (aParaStyle.LineSpaceMode)
                    {
                        case ParaLineSpaceMode.pls115: 
                            Result = Result + (int)Math.Truncate(3 * Result / 20f);
                            break;

                        case ParaLineSpaceMode.pls150: 
                            Result = (int)Math.Truncate(3 * Result / 2f);
                            break;

                        case ParaLineSpaceMode.pls200: 
                            Result = Result * 2;
                            break;

                        case ParaLineSpaceMode.plsMult:
                            Result = (int)Math.Truncate(Result * aParaStyle.LineSpace);
                            break;
                    }
                }
            }
            else
            {
                switch (aParaStyle.LineSpaceMode)
                {
                    case ParaLineSpaceMode.pls100:
                        Result = Result + aTextStyle.TextMetric_tmExternalLeading; // Round(vTextMetric.tmHeight * 0.2);
                        break;

                    case ParaLineSpaceMode.pls115:
                        Result = Result + aTextStyle.TextMetric_tmExternalLeading + (int)Math.Round((aTextStyle.TextMetric_tmHeight + aTextStyle.TextMetric_tmExternalLeading) * 0.15);
                        break;

                    case ParaLineSpaceMode.pls150:
                        Result = Result + aTextStyle.TextMetric_tmExternalLeading + (int)Math.Round((aTextStyle.TextMetric_tmHeight + aTextStyle.TextMetric_tmExternalLeading) * 0.5);
                        break;

                    case ParaLineSpaceMode.pls200:
                        Result = Result + aTextStyle.TextMetric_tmExternalLeading + aTextStyle.TextMetric_tmHeight + aTextStyle.TextMetric_tmExternalLeading;
                        break;

                    case ParaLineSpaceMode.plsMult:
                        Result = Result + aTextStyle.TextMetric_tmExternalLeading + (int)Math.Round((aTextStyle.TextMetric_tmHeight + aTextStyle.TextMetric_tmExternalLeading) * aParaStyle.LineSpace);
                        break;
                }
            }

            return Result;
        }

        protected virtual HCUndoList GetUndoList()
        {
            if (FOnGetUndoList != null)
                return FOnGetUndoList();
            else
                return null;
        }

        protected virtual bool DoSaveItem(int aItemNo)
        {
            if (FOnSaveItem != null)
                return FOnSaveItem(this, aItemNo);
            else
                return true;
        }

        protected virtual void DoInsertItem(HCCustomItem aItem)
        {
            if (FOnInsertItem != null)
                FOnInsertItem(this, aItem);
        }

        protected virtual void DoRemoveItem(HCCustomItem aItem)
        {
            if ((FOnRemoveItem != null) && (!FStyle.States.Contain(HCState.hosDestroying)))
                FOnRemoveItem(this, aItem);
        }

        protected virtual void DoItemAction(int aItemNo, int aOffset, HCAction aAction) { }

        protected virtual void DoDrawItemPaintBefor(HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (FOnDrawItemPaintBefor != null)
            {
                FOnDrawItemPaintBefor(aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight,
                    aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
            }
        }

        protected virtual void DoDrawItemPaintContent(HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
            RECT aClearRect, string aDrawText, int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop,
            int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (FOnDrawItemPaintContent != null)
            {
                FOnDrawItemPaintContent(aData, aItemNo, aDrawItemNo, aDrawRect, aClearRect, aDrawText,
                    aDataDrawLeft, aDataDrawRight, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
            }
        }

        protected virtual void DoDrawItemPaintAfter(HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (FOnDrawItemPaintAfter != null)
            {
                FOnDrawItemPaintAfter(aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight,
                    aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
            }
        }

        protected virtual void DoLoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            Clear();
        }

        protected virtual void DoCaretItemChanged() { }

        protected bool Loading
        {
            get { return FLoading; }
        }

        public HCCustomData(HCStyle aStyle)
        {
            FParentData = null;
            FStyle = aStyle;
            FDrawItems = new HCDrawItems();
            FItems = new HCItems();
            FItems.OnInsertItem += DoInsertItem;
            FItems.OnRemoveItem += DoRemoveItem;

            FLoading = false;
            FCurStyleNo = 0;
            FCurParaNo = 0;
            FCaretDrawItemNo = -1;
            FSelectInfo = new SelectInfo();
            FDrawOptions = new HashSet<DrawOption>();
        }

        ~HCCustomData()
        {
                    
        }

        public virtual bool CanEdit()
        {
            if (this.ParentData != null)
                return this.ParentData.CanEdit();
            else
                return true;
        }

        /// <summary> 全选 </summary>
        public virtual void SelectAll()
        {
            if (FItems.Count > 0)
            {
                FSelectInfo.StartItemNo = 0;
                FSelectInfo.StartItemOffset = 0;

                if (!IsEmptyData())
                {
                    FSelectInfo.EndItemNo = FItems.Count - 1;
                    FSelectInfo.EndItemOffset = GetItemOffsetAfter(FSelectInfo.EndItemNo);
                }
                else
                {
                    FSelectInfo.EndItemNo = -1;
                    FSelectInfo.EndItemOffset = -1;
                }

                MatchItemSelectState();
            }
        }

        /// <summary> 当前内容是否全选中了 </summary>
        public virtual bool SelectedAll()
        {
            return ((FSelectInfo.StartItemNo == 0)
                    && (FSelectInfo.StartItemOffset == 0)
                    && (FSelectInfo.EndItemNo == FItems.Count - 1)
                    && (FSelectInfo.EndItemOffset == GetItemOffsetAfter(FItems.Count - 1)));
        }

        public virtual void Clear()
        {
            FSelectInfo.Initialize();
            FCaretDrawItemNo = -1;
            FDrawItems.Clear();
            FItems.Clear();
            FCurStyleNo = 0;
            FCurParaNo = 0;
        }

        public virtual void InitializeField()
        {
            FCaretDrawItemNo = -1;
        }

        public virtual void SilenceChange() { }

        /// <summary> 嵌套时获取根级Data </summary>
        public virtual HCCustomData GetRootData()
        {
            return this;
        }

        public virtual POINT GetScreenCoord(int x, int y)
        {
            return this.GetRootData().GetScreenCoord(x, y);
        }

        public virtual HCCustomItem CreateDefaultTextItem()
        {
            Type[] vTypes = new Type[1];
            vTypes[0] = typeof(string);
            object[] vobj = new object[1];
            vobj[0] = (object)"";
            Type t = HCTextItem.HCDefaultTextItemClass;
            HCCustomItem vItem = t.GetConstructor(vTypes).Invoke(vobj) as HCCustomItem;

            if (FCurStyleNo < HCStyle.Null)
                vItem.StyleNo = FStyle.GetStyleNo(FStyle.DefaultTextStyle, true);
            else
                vItem.StyleNo = FCurStyleNo;

            vItem.ParaNo = FCurParaNo;

            return vItem;
        }

        public virtual HCCustomItem CreateDefaultDomainItem()
        {
            Type[] vTypes = new Type[1];
            vTypes[0] = this.GetType();
            object[] vobj = new object[1];
            vobj[0] = this;
            Type t = HCDomainItem.HCDefaultDomainItemClass;
            HCCustomItem Result = t.GetConstructor(vTypes).Invoke(vobj) as HCCustomItem;
            Result.ParaNo = FCurParaNo;
            return Result;
        }

        public virtual HCCustomItem CreateItemByStyle(int aStyleNo)
        {
            return null;
        }

        public virtual void GetCaretInfo(int aItemNo, int aOffset, ref HCCaretInfo aCaretInfo)
        {
            int vDrawItemNo, vStyleItemNo;

            /* 注意：为处理RectItem往外迭代，这里位置处理为叠加，而不是直接赋值 }*/
            if (FCaretDrawItemNo < 0)
            {
                if (FItems[aItemNo].StyleNo < HCStyle.Null)
                    vDrawItemNo = FItems[aItemNo].FirstDItemNo;
                else
                    vDrawItemNo = GetDrawItemNoByOffset(aItemNo, aOffset);  // AOffset处对应的DrawItemNo
            }
            else
                vDrawItemNo = FCaretDrawItemNo;

            HCCustomDrawItem vDrawItem = FDrawItems[vDrawItemNo];
            aCaretInfo.Height = vDrawItem.Height;  // 光标高度

            if (FStyle.UpdateInfo.ReStyle)
            {
                vStyleItemNo = aItemNo;
                if (aOffset == 0)
                {
                    if ((!FItems[aItemNo].ParaFirst)
                    && (aItemNo > 0)
                    && (Items[aItemNo - 1].StyleNo > HCStyle.Null))
                        vStyleItemNo = aItemNo - 1;
                }

                if ((Items[vStyleItemNo] is HCTextRectItem) && (FSelectInfo.StartItemOffset == HC.OffsetInner))
                    this.CurStyleNo = (Items[vStyleItemNo] as HCTextRectItem).TextStyleNo;
                else
                    this.CurStyleNo = Items[vStyleItemNo].StyleNo;

                this.CurParaNo = Items[vStyleItemNo].ParaNo;
            }

            if (FItems[aItemNo].StyleNo < HCStyle.Null)
            {
                HCCustomRectItem vRectItem = FItems[aItemNo] as HCCustomRectItem;

                if (aOffset == HC.OffsetBefor)
                {
                    if (vRectItem.CanPageBreak)
                        GetRectItemInnerCaretInfo(vRectItem, aItemNo, vDrawItemNo, vDrawItem, ref aCaretInfo);

                    aCaretInfo.X = aCaretInfo.X + vDrawItem.Rect.Left;
                }
                else
                if (aOffset == HC.OffsetInner)
                {
                    GetRectItemInnerCaretInfo(vRectItem, aItemNo, vDrawItemNo, vDrawItem, ref aCaretInfo);
                    aCaretInfo.X = aCaretInfo.X + vDrawItem.Rect.Left;
                }
                else  // 在其右侧
                {
                    if (vRectItem.CanPageBreak)
                        GetRectItemInnerCaretInfo(vRectItem, aItemNo, vDrawItemNo, vDrawItem, ref aCaretInfo);

                    aCaretInfo.X = aCaretInfo.X + vDrawItem.Rect.Right;
                }

                if (vRectItem.JustifySplit())
                {
                    if ( ((FStyle.ParaStyles[vRectItem.ParaNo].AlignHorz == ParaAlignHorz.pahJustify) && (!IsParaLastDrawItem(vDrawItemNo)) )  // 两端对齐且不是段最后)
                    || (FStyle.ParaStyles[vRectItem.ParaNo].AlignHorz == ParaAlignHorz.pahScatter))  // 分散对齐
                    {
                        if (IsLineLastDrawItem(vDrawItemNo))
                            aCaretInfo.X = aCaretInfo.X + vDrawItem.Width - vRectItem.Width;
                    }
                    else
                        aCaretInfo.X = aCaretInfo.X + vDrawItem.Width - vRectItem.Width;
                }
            }
            else  // TextItem
                aCaretInfo.X = aCaretInfo.X + vDrawItem.Rect.Left + GetDrawItemOffsetWidth(vDrawItemNo, aOffset - vDrawItem.CharOffs + 1);

            aCaretInfo.Y = aCaretInfo.Y + vDrawItem.Rect.Top;
        }

        /// <summary>
        /// 返回指定坐标下的Item和Offset
        /// </summary>
        /// <param name="x">水平坐标值X</param>
        /// <param name="y">垂直坐标值Y</param>
        /// <param name="aItemNo">坐标处的Item</param>
        /// <param name="aOffset">坐标在Item中的位置</param>
        /// <param name="ARestrain">True并不是在AItemNo范围内(在行最右侧或最后一行底部，通过约束坐标找到的)</param>
        public virtual void GetItemAt(int x, int y, ref int aItemNo, ref int aOffset, ref int aDrawItemNo,
            ref bool ARestrain)
        {
            aItemNo = -1;
            aOffset = -1;
            aDrawItemNo = -1;
            ARestrain = true;  // 默认为约束找到(不在Item上面)

            if (IsEmptyData())
            {
                aItemNo = 0;
                aOffset = 0;
                aDrawItemNo = 0;
                return;
            }

            /* 获取对应位置最接近的起始DrawItem }*/
            int vStartDItemNo, vEndDItemNo = -1, vi;
            RECT vDrawRect;

            if (y < 0)
                vStartDItemNo = 0;
            else  // 判断在哪一行
            {
                vDrawRect = FDrawItems[FDrawItems.Count - 1].Rect;
                if (y > vDrawRect.Bottom)
                    vStartDItemNo = FDrawItems.Count - 1;
                else  // 二分法查找哪个Item
                {
                    vStartDItemNo = 0;
                    vEndDItemNo = FDrawItems.Count - 1;

                    while (true)
                    {
                        if (vEndDItemNo - vStartDItemNo > 1)
                        {
                            vi = vStartDItemNo + (vEndDItemNo - vStartDItemNo) / 2;
                            if (y > FDrawItems[vi].Rect.Bottom)
                            {
                                vStartDItemNo = vi + 1;  // 中间位置下一个
                                continue;
                            }
                            else
                            if (y < FDrawItems[vi].Rect.Top)
                            {
                                vEndDItemNo = vi - 1;  // 中间位置上一个
                                continue;
                            }
                            else
                            {
                                vStartDItemNo = vi;  // 正好是中间位置的
                                break;
                            }
                        }
                        else  // 相差1
                        {
                            if (y >= FDrawItems[vEndDItemNo].Rect.Bottom)
                                vStartDItemNo = vEndDItemNo;
                            else
                                if (y > FDrawItems[vEndDItemNo].Rect.Top)
                                    vStartDItemNo = vEndDItemNo;

                            //else 不处理即第一个
                            break;
                        }
                    }
                }

                if (y < FDrawItems[vStartDItemNo].Rect.Top)
                    vStartDItemNo--;
            }

            // 判断是指定行中哪一个Item
            GetLineDrawItemRang(ref vStartDItemNo, ref vEndDItemNo);  // 行起始和结束DrawItem

            if (x <= FDrawItems[vStartDItemNo].Rect.Left)
            {
                aDrawItemNo = vStartDItemNo;
                aItemNo = FDrawItems[vStartDItemNo].ItemNo;
                if (FItems[aItemNo].StyleNo < HCStyle.Null)
                    aOffset = GetDrawItemOffsetAt(vStartDItemNo, x);
                else
                    aOffset = FDrawItems[vStartDItemNo].CharOffs - 1;  // DrawItem起始
            }
            else
            if (x >= FDrawItems[vEndDItemNo].Rect.Right)
            {
                aDrawItemNo = vEndDItemNo;
                aItemNo = FDrawItems[vEndDItemNo].ItemNo;
                if (FItems[aItemNo].StyleNo < HCStyle.Null)
                    aOffset = GetDrawItemOffsetAt(vEndDItemNo, x);
                else
                    aOffset = FDrawItems[vEndDItemNo].CharOffs + FDrawItems[vEndDItemNo].CharLen - 1;  // DrawItem最后
            }
            else
            {
                for (int i = vStartDItemNo; i <= vEndDItemNo; i++)  // 行中间
                {
                    vDrawRect = FDrawItems[i].Rect;
                    if ((x >= vDrawRect.Left) && (x < vDrawRect.Right))
                    {
                        ARestrain = ((y < vDrawRect.Top) || (y > vDrawRect.Bottom));

                        aDrawItemNo = i;
                        aItemNo = FDrawItems[i].ItemNo;
                        if (FItems[aItemNo].StyleNo < HCStyle.Null)
                            aOffset = GetDrawItemOffsetAt(i, x);
                        else  // TextItem
                            aOffset = FDrawItems[i].CharOffs + GetDrawItemOffsetAt(i, x) - 1;

                        break;
                    }
                }
            }
        }

        /// <summary> 坐标是否在AItem的选中区域中 </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="aItemNo">X、Y处的Item</param>
        /// <param name="aOffset">X、Y处的Item偏移(供在RectItem上时计算)</param>
        /// <param name="aRestrain">AItemNo, AOffset是X、Y位置约束后的(此参数为方便单元格Data处理)</param>
        public virtual bool CoordInSelect(int x, int y, int aItemNo, int aOffset, bool aRestrain)
        {
            bool Result = false;

            if ((aItemNo < 0) || (aOffset < 0))
                return Result;

            if (aRestrain)
                return Result;

            // 判断坐标是否在AItemNo对应的AOffset上
            int vDrawItemNo = GetDrawItemNoByOffset(aItemNo, aOffset);
            RECT vDrawRect = DrawItems[vDrawItemNo].Rect;
            Result = HC.PtInRect(vDrawRect, x, y);
            if (Result)
            {
                if (FItems[aItemNo].StyleNo < HCStyle.Null)
                {
                    int vX = x - vDrawRect.Left;
                    int vY = y - vDrawRect.Top - GetLineBlankSpace(vDrawItemNo) / 2;

                    Result = (FItems[aItemNo] as HCCustomRectItem).CoordInSelect(vX, vY);
                }
                else
                    Result = OffsetInSelect(aItemNo, aOffset);  // 对应的AOffset在选中内容中
            }

            return Result;
        }

        public string GetDrawItemText(int aDrawItemNo)
        {
            HCCustomDrawItem vDrawItem = FDrawItems[aDrawItemNo];
            return FItems[vDrawItem.ItemNo].Text.Substring(vDrawItem.CharOffs - 1, vDrawItem.CharLen);
        }

        #region GetDrawItemOffsetWidth子方法
        private int _GetNorAlignDrawItemOffsetWidth(int aDrawItemNo, int aDrawOffset, HCCanvas aCanvas)
        {
            int Result = 0;
            
            #if UNPLACEHOLDERCHAR
            string vText = GetDrawItemText(aDrawItemNo);
            if (vText != "")
            {
                int vLen = vText.Length;
                int[] vCharWArr = new int[vLen];
                SIZE vSize = new SIZE(0, 0);
                aCanvas.GetTextExtentExPoint(vText, vLen, vCharWArr, ref vSize);
                Result = vCharWArr[aDrawOffset - 1];
            }
            #else
            HCCustomDrawItem vDrawItem = FDrawItems[aDrawItemNo];
            vText = FItems[vDrawItem.ItemNo].Text.Substring(vDrawItem.CharOffs - 1, aDrawOffset);
            if (vText != ")
                Result = aCanvas.TextWidth(vText);
            #endif

            return Result;
        }
        #endregion

        /// <summary> 获取DItem中指定偏移处的内容绘制宽度 </summary>
        /// <param name="aDrawItemNo"></param>
        /// <param name="aDrawOffs">相对与DItem的CharOffs的Offs</param>
        /// <returns></returns>
        public int GetDrawItemOffsetWidth(int aDrawItemNo, int aDrawOffs, HCCanvas aStyleCanvas = null)
        {
            int Result = 0;
            if (aDrawOffs == 0)
                return Result;

            HCCustomDrawItem vDrawItem = FDrawItems[aDrawItemNo];
            int vStyleNo = FItems[vDrawItem.ItemNo].StyleNo;
            if (vStyleNo < HCStyle.Null)
            {
                if (aDrawOffs > HC.OffsetBefor)
                    Result = FDrawItems[aDrawItemNo].Width;
            }
            else
            {
                HCCanvas vCanvas = null;
                if (aStyleCanvas != null)
                    vCanvas = aStyleCanvas;
                else
                {
                    vCanvas = FStyle.TempCanvas;
                    FStyle.ApplyTempStyle(vStyleNo);
                }

                ParaAlignHorz vAlignHorz = FStyle.ParaStyles[GetDrawItemParaStyle(aDrawItemNo)].AlignHorz;
                switch (vAlignHorz)
                {
                    case ParaAlignHorz.pahLeft:
                    case ParaAlignHorz.pahRight:
                    case ParaAlignHorz.pahCenter:
                        Result = _GetNorAlignDrawItemOffsetWidth(aDrawItemNo, aDrawOffs, vCanvas);
                        break;

                    case ParaAlignHorz.pahJustify:
                    case ParaAlignHorz.pahScatter:  // 20170220001 两端、分散对齐相关处理
                        if (vAlignHorz == ParaAlignHorz.pahJustify)
                        {
                            if (IsParaLastDrawItem(aDrawItemNo))
                            {
                                Result = _GetNorAlignDrawItemOffsetWidth(aDrawItemNo, aDrawOffs, vCanvas);
                                return Result;
                                //break;
                            }
                        }

                        string vText = GetDrawItemText(aDrawItemNo);
                        int vLen = vText.Length;
                        int[] vCharWArr = new int[vLen];
                        SIZE vSize = new SIZE(0, 0);
                        vCanvas.GetTextExtentExPoint(vText, vLen, vCharWArr, ref vSize);

                        int viSplitW = vDrawItem.Width - vCharWArr[vLen - 1];  // 当前DItem的Rect中用于分散的空间
                        int vMod = 0;

                        // 计算当前Ditem内容分成几份，每一份在内容中的起始位置
                        List<int> vSplitList = new List<int>();
                        int vSplitCount = GetJustifyCount(vText, vSplitList);
                        bool vLineLast = IsLineLastDrawItem(aDrawItemNo);
                        if (vLineLast && (vSplitCount > 0))
                            vSplitCount--;

                        if (vSplitCount > 0)
                        {
                            vMod = viSplitW % vSplitCount;
                            viSplitW = viSplitW / vSplitCount;
                        }

                        int vExtra = 0, vInnerOffs = 0;
                        for (int i = 0; i <= vSplitList.Count - 2; i++)  // vSplitList最后一个是字符串长度所以多减1
                        {
                            // 计算结束位置
                            if (vLineLast && (i == vSplitList.Count - 2))
                            {

                            }
                            else
                            if (vMod > 0)
                            {
                                vExtra += viSplitW + 1;
                                vMod--;
                            }
                            else
                                vExtra += viSplitW;

                            vInnerOffs = vSplitList[i + 1] - 1;
                            if (vInnerOffs == aDrawOffs)
                            {
                                Result = vCharWArr[vInnerOffs - 1] + vExtra;
                                break;
                            }
                            else
                            if (vInnerOffs > aDrawOffs)
                            {
                                Result = vCharWArr[aDrawOffs - vSplitList[i]] + vExtra;
                                break;
                            }
                        }
                        break;
                    }
                }

            return Result;
        }

        private void GetRectItemInnerCaretInfo(HCCustomRectItem aRectItem, int aItemNo, int aDrawItemNo, HCCustomDrawItem aDrawItem, ref HCCaretInfo aCaretInfo)
        {
            aRectItem.GetCaretInfo(ref aCaretInfo);

            RECT vDrawRect = aDrawItem.Rect;

            int vLineSpaceHalf = GetLineBlankSpace(aDrawItemNo) / 2;
            vDrawRect.Inflate(0, -vLineSpaceHalf);

            switch (FStyle.ParaStyles[FItems[aItemNo].ParaNo].AlignVert)  // 垂直对齐方式
            {
                case ParaAlignVert.pavCenter: 
                    aCaretInfo.Y = aCaretInfo.Y + vLineSpaceHalf + (vDrawRect.Height - aRectItem.Height) / 2;
                    break;

                case ParaAlignVert.pavTop: 
                    aCaretInfo.Y = aCaretInfo.Y + vLineSpaceHalf;
                    break;
                
                default:
                    aCaretInfo.Y = aCaretInfo.Y + vLineSpaceHalf + vDrawRect.Height - aRectItem.Height;
                    break;
            }
        }

        #if UNPLACEHOLDERCHAR
        public int GetItemActualOffset(int aItemNo, int aOffset, bool aAfter = false)
        {
            return HC.GetTextActualOffset(FItems[aItemNo].Text, aOffset, aAfter);
        }
        #endif

        /// <summary> 获取指定的Item最后面位置 </summary>
        /// <param name="aItemNo">指定的Item</param>
        /// <returns>最后面位置</returns>
        public int GetItemOffsetAfter(int aItemNo)
        {
            if (FItems[aItemNo].StyleNo < HCStyle.Null)
                return HC.OffsetAfter;
            else
                return FItems[aItemNo].Length;
        }

        /// <summary>
        /// 根据给定的位置获取在此范围内的起始和结束DItem
        /// </summary>
        /// <param name="aTop"></param>
        /// <param name="aBottom"></param>
        /// <param name="AFristDItemNo"></param>
        /// <param name="aLastDItemNo"></param>
        public void GetDataDrawItemRang(int aTop, int aBottom, ref int aFirstDItemNo, ref int aLastDItemNo)
        {
            aFirstDItemNo = -1;
            aLastDItemNo = -1;
            // 获取第一个可显示的DrawItem
            for (int i = 0; i <= FDrawItems.Count - 1; i++)
            {
                if ((FDrawItems[i].LineFirst)
                  && (FDrawItems[i].Rect.Bottom > aTop)  // 底部超过区域上边
                  && (FDrawItems[i].Rect.Top < aBottom))  // 顶部没超过区域下边
                {
                    aFirstDItemNo = i;
                    break;
                }
            }

            if (aFirstDItemNo < 0)
                return;

            // 获取最后一个可显示的DrawItem
            for (int i = aFirstDItemNo; i <= FDrawItems.Count - 1; i++)
            {
                if ((FDrawItems[i].LineFirst) && (FDrawItems[i].Rect.Top >= aBottom))
                {
                    aLastDItemNo = i - 1;
                    break;
                }
            }

            if (aLastDItemNo < 0)
                aLastDItemNo = FDrawItems.Count - 1;
        }

        // Item和DItem互查 
        /// <summary>
        /// 获取Item对应的最后一个DItem
        /// </summary>
        /// <param name="aItemNo"></param>
        /// <returns></returns>
        public int GetItemLastDrawItemNo(int aItemNo)
        {
            int Result = -1;
            if (FItems[aItemNo].FirstDItemNo < 0)                
                return Result;  // 还没有格式化过

            Result = FItems[aItemNo].FirstDItemNo + 1;
            while (Result < FDrawItems.Count)
            {
                if (FDrawItems[Result].ParaFirst || (FDrawItems[Result].ItemNo != aItemNo))
                    break;
                else
                    Result++;
            }
            Result--;

            return Result;
        }

        /// <summary>
        /// Item指定偏移位置是否被选中(仅用于文本Item和粗略Rect)
        /// </summary>
        /// <param name="aItemNo"></param>
        /// <param name="aOffset"></param>
        /// <returns></returns>
        public bool OffsetInSelect(int aItemNo, int aOffset)
        {
            bool Result = false;

            if ((aItemNo < 0) || (aOffset < 0))
                return Result;

            if (FItems[aItemNo].StyleNo < HCStyle.Null)
            {
                if ((aOffset == HC.OffsetInner) && FItems[aItemNo].IsSelectComplate)
                    Result = true;

                return Result;
            }

            if (SelectExists())
            {
                if ((aItemNo > FSelectInfo.StartItemNo) && (aItemNo < FSelectInfo.EndItemNo))
                    Result = true;
                else
                if (aItemNo == FSelectInfo.StartItemNo)
                {
                    if (aItemNo == FSelectInfo.EndItemNo)
                        Result = (aOffset >= FSelectInfo.StartItemOffset) && (aOffset <= FSelectInfo.EndItemOffset);
                    else
                        Result = (aOffset >= FSelectInfo.StartItemOffset);
                }
                else
                if (aItemNo == FSelectInfo.EndItemNo)
                    Result = (aOffset <= FSelectInfo.EndItemOffset);
            }

            return Result;
       }

        /// <summary>
        /// 获取Data中的坐标X、Y处的Item和Offset，并返回X、Y相对DrawItem的坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="aItemNo"></param>
        /// <param name="aOffset"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        public void CoordToItemOffset(int x, int y, int aItemNo, int aOffset, ref int aX, ref int aY)
        {
            aX = x;
            aY = y;
            if (aItemNo < 0)
                return;

            int vDrawItemNo = GetDrawItemNoByOffset(aItemNo, aOffset);
            RECT vDrawRect = FDrawItems[vDrawItemNo].Rect;

            vDrawRect.Inflate(0, -GetLineBlankSpace(vDrawItemNo) / 2);

            aX = aX - vDrawRect.Left;
            aY = aY - vDrawRect.Top;
            if (FItems[aItemNo].StyleNo < HCStyle.Null)
            {
                switch (FStyle.ParaStyles[FItems[aItemNo].ParaNo].AlignVert)  // 垂直对齐方式
                {
                    case ParaAlignVert.pavCenter: 
                        aY = aY - (vDrawRect.Height - (FItems[aItemNo] as HCCustomRectItem).Height) / 2;
                        break;
                    case ParaAlignVert.pavTop: 
                        break;

                    default:
                        aY = aY - (vDrawRect.Height - (FItems[aItemNo] as HCCustomRectItem).Height);
                        break;
                }
            }
        }

        /// <summary>
        /// 返回Item中指定Offset处的DrawItem序号
        /// </summary>
        /// <param name="aItemNo">指定Item</param>
        /// <param name="aOffset">Item中指定Offset</param>
        /// <returns>Offset处的DrawItem序号</returns>
        public int GetDrawItemNoByOffset(int aItemNo, int aOffset)
        {
            int Result = FItems[aItemNo].FirstDItemNo;
            if (FItems[aItemNo].StyleNo > HCStyle.Null)  // TextItem
            {
                if (FItems[aItemNo].Length > 0)
                {
                    for (int i = FItems[aItemNo].FirstDItemNo; i <= FDrawItems.Count - 1; i++)
                    {
                        HCCustomDrawItem vDrawItem = FDrawItems[i];
                        if (vDrawItem.ItemNo != aItemNo)
                            break;

                        if (aOffset - vDrawItem.CharOffs < vDrawItem.CharLen)
                        {
                            Result = i;
                            break;
                        }
                    }
                }
            }

            return Result;
        }

        public bool IsLineLastDrawItem(int aDrawItemNo)
        {
            // 不能在格式化进行中使用，因为DrawItems.Count可能只是当前格式化到的Item
            return ((aDrawItemNo == FDrawItems.Count - 1) || (FDrawItems[aDrawItemNo + 1].LineFirst));
        }
        
        public bool IsParaLastDrawItem(int aDrawItemNo)
        {
            bool Result = false;
            int vItemNo = FDrawItems[aDrawItemNo].ItemNo;
            if (vItemNo < FItems.Count - 1)
            {
                if (FItems[vItemNo + 1].ParaFirst)
                    Result = (FDrawItems[aDrawItemNo].CharOffsetEnd() == FItems[vItemNo].Length);  // 是Item最后一个DrawItem
            }
            else  // 是最后一个Item
                Result = (FDrawItems[aDrawItemNo].CharOffsetEnd() == FItems[vItemNo].Length);  // 是Item最后一个DrawItem

            return Result;
        }

        public bool IsParaLastItem(int aItemNo)
        {
            return ((aItemNo == FItems.Count - 1) || FItems[aItemNo + 1].ParaFirst);
        }

        /// <summary> 返回当前光标处的顶层Data </summary>
        public HCCustomData GetTopLevelData()
        {
            HCCustomData Result = null;
            if ((SelectInfo.StartItemNo >= 0) && (SelectInfo.EndItemNo < 0))
            {
                if ((Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null)
                    && (SelectInfo.StartItemOffset == HC.OffsetInner))
                    Result = (Items[SelectInfo.StartItemNo] as HCCustomRectItem).GetActiveData() as HCRichData;
            }

            if (Result == null)
                Result = this;

            return Result;
        }

        public HCCustomData GetTopLevelDataAt(int X, int Y)
        {
            HCCustomData Result = null;

            int vItemNo = -1, vOffset = 0, vDrawItemNo = -1;
            bool vRestrain = false;
            GetItemAt(X, Y, ref vItemNo, ref vOffset, ref vDrawItemNo, ref vRestrain);
            if ((!vRestrain) && (vItemNo >= 0))
            {
                if (FItems[vItemNo].StyleNo < HCStyle.Null)
                {
                    int vX = 0, vY = 0;
                    CoordToItemOffset(X, Y, vItemNo, vOffset, ref vX, ref vY);
                    Result = (FItems[vItemNo] as HCCustomRectItem).GetTopLevelDataAt(vX, vY);
                }
            }
        
            if (Result == null)
                Result = this;

            return Result;
        }

        public int GetActiveDrawItemNo()
        {
            if (FCaretDrawItemNo >= 0)
                return FCaretDrawItemNo;

            int Result = -1;

            if (FSelectInfo.StartItemNo < 0)
            {

            }
            else
            {
                int vItemNo = -1;

                if (SelectExists())
                {
                    if (FSelectInfo.EndItemNo >= 0)
                        vItemNo = FSelectInfo.EndItemNo;
                    else
                        vItemNo = FSelectInfo.StartItemNo;
                }
                else
                    vItemNo = FSelectInfo.StartItemNo;

                if (FItems[vItemNo].StyleNo < HCStyle.Null)
                    Result = FItems[vItemNo].FirstDItemNo;
                else  // 文本
                {
                    for (int i = FItems[vItemNo].FirstDItemNo; i <= FDrawItems.Count - 1; i++)
                    {
                        HCCustomDrawItem vDrawItem = FDrawItems[i];
                        if (FSelectInfo.StartItemOffset - vDrawItem.CharOffs + 1 <= vDrawItem.CharLen)
                        {
                            Result = i;
                            break;
                        }
                    }
                }
            }

            return Result;
        }

        public HCCustomDrawItem GetActiveDrawItem()
        {
            int vDrawItemNo = GetActiveDrawItemNo();
            if (vDrawItemNo < 0)
                return null;
            else
                return FDrawItems[vDrawItemNo];
        }

        public virtual int GetActiveItemNo()
        {
            return FSelectInfo.StartItemNo;
        }

        public virtual HCCustomItem GetActiveItem()
        {
            int vItemNo = GetActiveItemNo();
            if (vItemNo < 0)
                return null;
            else
                return FItems[vItemNo];
        }

        public HCCustomItem GetTopLevelItem()
        {
            HCCustomItem Result = GetActiveItem();
            if ((Result != null) && (Result.StyleNo < HCStyle.Null))
                Result = (Result as HCCustomRectItem).GetTopLevelItem();

            return Result;
        }

        public HCCustomDrawItem GetTopLevelDrawItem()
        {
            HCCustomDrawItem Result = null;
            HCCustomItem vItem = GetActiveItem();
            if (vItem.StyleNo < HCStyle.Null)
                Result = (vItem as HCCustomRectItem).GetTopLevelDrawItem();
            if (Result == null)
                Result = GetActiveDrawItem();

            return Result;
        }

        public POINT GetTopLevelDrawItemCoord()
        {
            POINT Result = new POINT(0, 0);
            POINT vPt = new POINT(0, 0);
            HCCustomDrawItem vDrawItem = GetActiveDrawItem();
            if (vDrawItem != null)
            {
                Result = vDrawItem.Rect.TopLeft();
                HCCustomItem vItem = GetActiveItem();
                if (vItem.StyleNo < HCStyle.Null)
                {
                    vPt = (vItem as HCCustomRectItem).GetTopLevelDrawItemCoord();
                    vPt.Y = vPt.Y + FStyle.LineSpaceMin / 2;
                }

                Result.X = Result.X + vPt.X;
                Result.Y = Result.Y + vPt.Y;
            }

            return Result;
        }

        public HCCustomDrawItem GetTopLevelRectDrawItem()
        {
            HCCustomDrawItem vResult = null;

            HCCustomItem vItem = GetActiveItem();
            if (vItem.StyleNo < HCStyle.Null)
            {
                vResult = (vItem as HCCustomRectItem).GetTopLevelRectDrawItem();
                if (vResult == null)
                    vResult = GetActiveDrawItem();
            }

            return vResult;
        }

        public POINT GetTopLevelRectDrawItemCoord()
        {
            POINT vResult = new POINT(-1, -1);
            HCCustomItem vItem = GetActiveItem();
            if ((vItem != null) && (vItem.StyleNo < HCStyle.Null))
            {
                vResult = FDrawItems[vItem.FirstDItemNo].Rect.TopLeft();
                POINT vPt = (vItem as HCCustomRectItem).GetTopLevelRectDrawItemCoord();
                if (vPt.X >= 0)
                {
                    vPt.Y = vPt.Y + FStyle.LineSpaceMin / 2;
                    vResult.X = vResult.X + vPt.X;
                    vResult.Y = vResult.Y + vPt.Y;
                }
            }

            return vResult;
        }

        /// <summary> 返回Item的文本样式 </summary>
        public int GetItemStyle(int aItemNo)
        {
            return FItems[aItemNo].StyleNo;
        }

        /// <summary> 返回DDrawItem对应的Item的文本样式 </summary>
        public int GetDrawItemStyle(int aDrawItemNo)
        {
            return GetItemStyle(FDrawItems[aDrawItemNo].ItemNo);
        }

        /// <summary> 返回Item对应的段落样式 </summary>
        public int GetItemParaStyle(int aItemNo)
        {
            return FItems[aItemNo].ParaNo;
        }

        /// <summary> 返回DDrawItem对应的Item的段落样式 </summary>
        public int GetDrawItemParaStyle(int aDrawItemNo)
        {
            return GetItemParaStyle(FDrawItems[aDrawItemNo].ItemNo);
        }

        /// <summary> 得到指定横坐标X处，是DItem内容的第几个字符 </summary>
        /// <param name="aDrawItemNo">指定的DItem</param>
        /// <param name="x">在Data中的横坐标</param>
        /// <returns>第几个字符</returns>
        public int GetDrawItemOffsetAt(int aDrawItemNo, int x)
        {
            int Result = 0;

            HCCustomDrawItem vDrawItem = FDrawItems[aDrawItemNo];
            HCCustomItem vItem = FItems[vDrawItem.ItemNo];

            if (vItem.StyleNo < HCStyle.Null)
                Result = (vItem as HCCustomRectItem).GetOffsetAt(x - vDrawItem.Rect.Left);
            else  // 文本
            {
                Result = vDrawItem.CharLen;  // 赋值为最后，为方便行最右侧点击时返回为最后一个
                string vText = (vItem as HCTextItem).SubString(vDrawItem.CharOffs, vDrawItem.CharLen);
                FStyle.ApplyTempStyle(vItem.StyleNo);
                HCParaStyle vParaStyle = FStyle.ParaStyles[vItem.ParaNo];
                int vWidth = x - vDrawItem.Rect.Left;

                switch (vParaStyle.AlignHorz)
                {
                    case ParaAlignHorz.pahLeft:
                    case ParaAlignHorz.pahRight:
                    case ParaAlignHorz.pahCenter:
                        Result = HC.GetNorAlignCharOffsetAt(FStyle.TempCanvas, vText, vWidth);
                        break;

                    case ParaAlignHorz.pahJustify:
                    case ParaAlignHorz.pahScatter:  // 20170220001 两端、分散对齐相关处理
                        if (vParaStyle.AlignHorz == ParaAlignHorz.pahJustify)
                        {
                            if (IsParaLastDrawItem(aDrawItemNo))
                            {
                                Result = HC.GetNorAlignCharOffsetAt(FStyle.TempCanvas, vText, vWidth);
                                return Result;
                            }
                        }

                        int vLen = vText.Length;
                        int[] vCharWArr = new int[vLen];
                        SIZE vSize = new SIZE(0, 0);
                        FStyle.TempCanvas.GetTextExtentExPoint(vText, vLen, vCharWArr, ref vSize);
                        int viSplitW = vDrawItem.Width - vCharWArr[vLen - 1];  // 当前DItem的Rect中用于分散的空间
                        int vMod = 0;
                        
                        // 计算当前Ditem内容分成几份，每一份在内容中的起始位置
                        List<int> vSplitList = new List<int>();
                        int vSplitCount = GetJustifyCount(vText, vSplitList);
                        bool vLineLast = IsLineLastDrawItem(aDrawItemNo);

                        if (vLineLast && (vSplitCount > 0))
                            vSplitCount--;

                        if (vSplitCount > 0)
                        {
                            vMod = viSplitW % vSplitCount;
                            viSplitW = viSplitW / vSplitCount;
                        }

                        int vRight = 0, vExtraAll = 0, vExtra = 0;

                        //vSplitCount := 0;
                        for (int i = 0; i <= vSplitList.Count - 2; i++)  // vSplitList最后一个是字符串长度所以多减1
                        {
                            // 计算结束位置
                            if (vLineLast && (i == vSplitList.Count - 2))
                                vExtra = 0;
                            else
                            {
                                if (vMod > 0)
                                {
                                    vExtra = viSplitW + 1;
                                    vMod--;
                                }
                                else
                                    vExtra = viSplitW;

                                vExtraAll += vExtra;
                            }
                            
                            vRight = vCharWArr[(vSplitList[i + 1] - 1) - 1] + vExtraAll;  // 下一个分段的前一个字符

                            if (vRight > vWidth)
                            {
                                int j = vSplitList[i];
                                while (j < vSplitList[i + 1])
                                {
                                    #if UNPLACEHOLDERCHAR
                                    j = HC.GetTextActualOffset(vText, j, true);
                                    #endif

                                    if (vCharWArr[j - 1] + vExtraAll > vWidth)
                                    {
                                        vRight = vExtraAll - vExtra / 2 + HC.GetCharHalfFarfrom(
                                            #if UNPLACEHOLDERCHAR
                                            vText, 
                                            #endif
                                            j, vCharWArr);  // 中间位置

                                        if (vWidth > vRight)
                                            Result = j;
                                        else
                                        {
                                            Result = j - 1;
                                            #if UNPLACEHOLDERCHAR
                                            if (HC.IsUnPlaceHolderChar(vText[Result + 1 - 1]))
                                                Result = HC.GetTextActualOffset(vText, Result) - 1;
                                            #endif
                                        }

                                        break;
                                    }
                                }

                                break;
                            }
                        }
                        break;
                }
            }

            return Result;
        }

        // 获取选中相关信息 
        /// <summary> 当前选中起始DItemNo </summary>
        /// <returns></returns>
        public int GetSelectStartDrawItemNo()
        {
            int Result = -1;
            if (FSelectInfo.StartItemNo < 0)
                return Result;
            else
            {
                Result = GetDrawItemNoByOffset(FSelectInfo.StartItemNo, FSelectInfo.StartItemOffset);

                if ((FSelectInfo.EndItemNo >= 0) && (Result < FItems.Count - 1))
                {
                    if (FItems[FSelectInfo.StartItemNo].StyleNo < HCStyle.Null)
                    {
                        if (FSelectInfo.StartItemOffset == HC.OffsetAfter)
                            Result++;
                    }
                    else
                    if (FDrawItems[Result].CharOffsetEnd() == FSelectInfo.StartItemOffset)
                        Result++;
                }
            }

            return Result;
        }

        /// <summary> 当前选中结束DItemNo </summary>
        /// <returns></returns>
        public int GetSelectEndDrawItemNo()
        {
            int Result = -1;
            
            if (FSelectInfo.EndItemNo < 0)
                return Result;
            else
                Result = GetDrawItemNoByOffset(FSelectInfo.EndItemNo, 
                    FSelectInfo.EndItemOffset);

            return Result;
        }

        /// <summary> 获取选中内容是否在同一个DrawItem中 </summary>
        /// <returns></returns>
        public bool SelectInSameDrawItem()
        {
            int vStartDNo = GetSelectStartDrawItemNo();

            if (vStartDNo < 0)
                return false;
            else
            {
                if (GetDrawItemStyle(vStartDNo) < HCStyle.Null)
                    return (FItems[FDrawItems[vStartDNo].ItemNo].IsSelectComplate && (FSelectInfo.EndItemNo < 0));
                else
                    return (vStartDNo == GetSelectEndDrawItemNo());
            }
        }

        /// <summary> 取消选中 </summary>
        /// <returns>取消时当前是否有选中，True：有选中；False：无选中</returns>
        public virtual bool DisSelect()
        {
            bool Result = SelectExists();

            if (Result)
            {
                // 如果选中是在RectItem中进，下面循环SelectInfo.EndItemNo<0，不能取消选中，所以单独处理StartItemNo
                HCCustomItem vItem = FItems[SelectInfo.StartItemNo];
                vItem.DisSelect();
                vItem.Active = false;

                for (int i = SelectInfo.StartItemNo + 1; i <= SelectInfo.EndItemNo; i++)  // 遍历选中的其他Item
                {
                    vItem = FItems[i];
                    vItem.DisSelect();
                    vItem.Active = false;
                }
                SelectInfo.EndItemNo = -1;
                SelectInfo.EndItemOffset = -1;
            }
            else  // 没有选中
            if (SelectInfo.StartItemNo >= 0)
            {
                HCCustomItem vItem = FItems[SelectInfo.StartItemNo];
                vItem.DisSelect();
                //vItem.Active = false;
            }

            SelectInfo.StartItemNo = -1;
            SelectInfo.StartItemOffset = -1;

            return Result;
        }

        /// <summary> 当前选中内容允许拖动 </summary>
        /// <returns></returns>
        public bool SelectedCanDrag()
        {
            bool Result = true;

            if (FSelectInfo.EndItemNo < 0)
            {
                if (FSelectInfo.StartItemNo >= 0)
                    Result = FItems[FSelectInfo.StartItemNo].CanDrag();
                }
            else
            {
                for (int i = FSelectInfo.StartItemNo; i <= FSelectInfo.EndItemNo; i++)
                {
                    if (FItems[i].StyleNo < HCStyle.Null)
                    {
                        if (!FItems[i].IsSelectComplate)
                        {
                            Result = false;
                            break;
                        }
                    }

                    if (!FItems[i].CanDrag())
                    {
                        Result = false;
                        break;
                    }
                }
            }

            return Result;
        }

        /// <summary> 当前选中内容只有RectItem且正处于缩放状态 </summary>
        /// <returns></returns>
        public bool SelectedResizing()
        {
            if ((FSelectInfo.StartItemNo >= 0) 
              && (FSelectInfo.EndItemNo < 0) 
              && (FItems[FSelectInfo.StartItemNo] is HCResizeRectItem))
                return (FItems[FSelectInfo.StartItemNo] as HCResizeRectItem).Resizing;
            else
                return false;
        }

        /// <summary>
        /// 当前Data是不是无内容(仅有一个Item且内容为空)
        /// </summary>
        /// <returns></returns>
        public bool IsEmptyData()
        {
            return ((FItems.Count == 1) && IsEmptyLine(0));
        }

        public bool IsEmptyLine(int aItemNo)
        {
            return (FItems[aItemNo].StyleNo > HCStyle.Null) && (Items[aItemNo].Text == "");
        }

        /// <summary> 为段应用对齐方式 </summary>
        /// <param name="aAlign">对方方式</param>
        public virtual void ApplyParaAlignHorz(ParaAlignHorz aAlign)
        {
            ParaAlignHorzMatch vMatchStyle = new ParaAlignHorzMatch();
            vMatchStyle.Align = aAlign;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaAlignVert(ParaAlignVert aAlign)
        {
            ParaAlignVertMatch vMatchStyle = new ParaAlignVertMatch();
            vMatchStyle.Align = aAlign;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaBackColor(Color aColor)
        {
            ParaBackColorMatch vMatchStyle = new ParaBackColorMatch();
            vMatchStyle.BackColor = aColor;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaBreakRough(bool aRough)
        {
            ParaBreakRoughMatch vMatchStyle = new ParaBreakRoughMatch();
            vMatchStyle.BreakRough = aRough;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaLineSpace(ParaLineSpaceMode aSpaceMode, Single aSpace)
        {
            ParaLineSpaceMatch vMatchStyle = new ParaLineSpaceMatch();
            vMatchStyle.SpaceMode = aSpaceMode;
            vMatchStyle.Space = aSpace;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaLeftIndent(Single indent)
        {
            ParaLeftIndentMatch vMatchStyle = new ParaLeftIndentMatch();
            vMatchStyle.Indent = indent;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaRightIndent(Single indent)
        {
            ParaRightIndentMatch vMatchStyle = new ParaRightIndentMatch();
            vMatchStyle.Indent = indent;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaFirstIndent(Single indent)
        {
            ParaFirstIndentMatch vMatchStyle = new ParaFirstIndentMatch();
            vMatchStyle.Indent = indent;
            ApplySelectParaStyle(vMatchStyle);
        }

        // 选中内容应用样式
        public virtual void ApplySelectTextStyle(HCStyleMatch aMatchStyle) { }

        public virtual void ApplySelectParaStyle(HCParaMatch aMatchStyle)  { }

        public virtual void ApplyTableCellAlign(HCContentAlign aAlign) { }

        /// <summary> 删除选中 </summary>
        public virtual bool DeleteSelected()
        {
            return false;
        }

        /// <summary> 为选中文本使用指定的文本样式 </summary>
        /// <param name="aFontStyle">文本样式</param>
        public virtual void ApplyTextStyle(HCFontStyle aFontStyle)
        {
            TextStyleMatch vMatchStyle = new TextStyleMatch();
            vMatchStyle.FontStyle = aFontStyle;
            ApplySelectTextStyle(vMatchStyle);
        }

        public virtual void ApplyTextFontName(string aFontName)
        {
            FontNameStyleMatch vMatchStyle = new FontNameStyleMatch();
            vMatchStyle.FontName = aFontName;
            ApplySelectTextStyle(vMatchStyle);
        }

        public virtual void ApplyTextFontSize(Single aFontSize)
        {
            FontSizeStyleMatch vMatchStyle = new FontSizeStyleMatch();
            vMatchStyle.FontSize = aFontSize;
            ApplySelectTextStyle(vMatchStyle);
        }

        public virtual void ApplyTextColor(Color aColor)
        {
            ColorStyleMatch vMatchStyle = new ColorStyleMatch();
            vMatchStyle.Color = aColor;
            ApplySelectTextStyle(vMatchStyle);
        }

        public virtual void ApplyTextBackColor(Color aColor)
        {
            BackColorStyleMatch vMatchStyle = new BackColorStyleMatch();
            vMatchStyle.Color = aColor;
            ApplySelectTextStyle(vMatchStyle);
        }

#region 当前显示范围内要绘制的DrawItem是否全选
    private bool DrawItemSelectAll(int aFristDItemNo, int aLastDItemNo)
    {
        int vSelStartDItemNo = GetSelectStartDrawItemNo();
        int vSelEndDItemNo = GetSelectEndDrawItemNo();

        return  // 当前页是否全选中了
            (
                (vSelStartDItemNo < aFristDItemNo)
                ||
                (
                    (vSelStartDItemNo == aFristDItemNo)
                    && (SelectInfo.StartItemOffset == FDrawItems[vSelStartDItemNo].CharOffsetStart())
                )
            )
            &&
            (
                (vSelEndDItemNo > aLastDItemNo)
                ||
                (
                    (vSelEndDItemNo == aLastDItemNo)
                    && (SelectInfo.EndItemOffset == FDrawItems[vSelEndDItemNo].CharOffsetEnd())
                )
            );
    }
#endregion

#region DrawTextJsutify 20170220001 分散对齐相关处理
        private void DrawTextJsutify(HCCanvas aCanvas, RECT aRect, string aText, bool aLineLast, int vTextDrawTop)
        {
            int vX = aRect.Left;
            int vLen = aText.Length;
            int[] vCharWArr = new int[vLen];
            SIZE vSize = new SIZE(0, 0);
            aCanvas.GetTextExtentExPoint(aText, vLen, vCharWArr, ref vSize);

            int viSplitW = aRect.Width - vCharWArr[vLen - 1];
            if (viSplitW > 0)
            {
                List<int> vSplitList = new List<int>();
                int vSplitCount = GetJustifyCount(aText, vSplitList);
                if (aLineLast && (vSplitCount > 0))  // 行最后DrawItem，少分一个
                    vSplitCount--;

                int vMod = 0;
                if (vSplitCount > 0)
                {
                    vMod = viSplitW % vSplitCount;
                    viSplitW = viSplitW / vSplitCount;
                }

                vX = 0;
                int vExtra = 0;

                for (int i = 0; i <= vSplitList.Count - 2; i++)  // vSplitList最后一个是字符串长度所以多减1
                {
                    vLen = vSplitList[i + 1] - vSplitList[i];
                    string vS = aText.Substring(vSplitList[i] - 1, vLen);

                    if (i > 0)
                        vX = vCharWArr[vSplitList[i] - 2] + vExtra;

                    GDI.ExtTextOut(aCanvas.Handle, aRect.Left + vX, vTextDrawTop, 0, IntPtr.Zero, vS, vLen, IntPtr.Zero);

                    // 计算结束位置
                    if (aLineLast && (i == vSplitList.Count - 2))
                    {

                    }
                    else
                    if (vMod > 0)
                    {
                        vExtra += viSplitW + 1;
                        vMod--;
                    }
                    else
                        vExtra += viSplitW;
                }
            }
            else
                GDI.ExtTextOut(aCanvas.Handle, vX, vTextDrawTop, 0, IntPtr.Zero, aText, vLen, IntPtr.Zero);
        }
#endregion

        /// <summary> 绘制数据 </summary>
        /// <param name="aDataDrawLeft">绘制目标区域Left</param>
        /// <param name="aDataDrawTop">绘制目标区域的Top</param>
        /// <param name="aDataDrawBottom">绘制目标区域的Bottom</param>
        /// <param name="aDataScreenTop">屏幕区域Top</param>
        /// <param name="aDataScreenBottom">屏幕区域Bottom</param>
        /// <param name="aVOffset">指定从哪个位置开始的数据绘制到目标区域的起始位置</param>
        /// <param name="aCanvas">画布</param>
        public virtual void PaintData(int aDataDrawLeft, int aDataDrawTop, int aDataDrawRight, int aDataDrawBottom,
            int aDataScreenTop, int aDataScreenBottom, int aVOffset, int aFirstDItemNo,
            int aLastDItemNo, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if ((aFirstDItemNo < 0) || (aLastDItemNo < 0))
                return;

            int vSelStartDNo = -1, vSelStartDOffs = -1, vSelEndDNo = -1, vSelEndDOffs = -1;
            bool vDrawsSelectAll = false;

            if (!aPaintInfo.Print)
            {
                vSelStartDNo = GetSelectStartDrawItemNo();  // 选中起始DItem
                if (vSelStartDNo < 0)
                    vSelStartDOffs = -1;
                else
                    vSelStartDOffs = FSelectInfo.StartItemOffset - FDrawItems[vSelStartDNo].CharOffs + 1;

                vSelEndDNo = GetSelectEndDrawItemNo();      // 选中结束DrawItem
                if (vSelEndDNo < 0)
                    vSelEndDOffs = -1;
                else
                    vSelEndDOffs = FSelectInfo.EndItemOffset - FDrawItems[vSelEndDNo].CharOffs + 1;

                vDrawsSelectAll = DrawItemSelectAll(aFirstDItemNo, aLastDItemNo);
            }

            int vPrioStyleNo = HCStyle.Null;
            int vPrioParaNo = HCStyle.Null;
            int vTextHeight = 0;

            int vVOffset = aDataDrawTop - aVOffset;  // 将数据起始位置映射到绘制位置

            int vDCState = aCanvas.Save();
            try
            {
                int vLineSpace = -1;
                if (!FDrawItems[aFirstDItemNo].LineFirst)
                    vLineSpace = GetLineBlankSpace(aFirstDItemNo);

                HCCustomDrawItem vDrawItem;
                RECT vDrawRect, vClearRect;
                ParaAlignHorz vAlignHorz = ParaAlignHorz.pahLeft;
                HCCustomItem vItem;
                HCCustomRectItem vRectItem;

                for (int i = aFirstDItemNo; i <= aLastDItemNo; i++)  // 遍历要绘制的数据
                {
                    vDrawItem = FDrawItems[i];
                    vItem = FItems[vDrawItem.ItemNo];
                    vDrawRect = vDrawItem.Rect;
                    vDrawRect.Offset(aDataDrawLeft, vVOffset);  // 偏移到指定的画布绘制位置(SectionData时为页数据在格式化中可显示起始位置)

                    if (FDrawItems[i].LineFirst)
                        vLineSpace = GetLineBlankSpace(i);

                    // 绘制内容前
                    DrawItemPaintBefor(this, vDrawItem.ItemNo, i, vDrawRect, aDataDrawLeft, aDataDrawRight, 
                        aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);

                    if (vPrioParaNo != vItem.ParaNo)  // 水平对齐方式
                    {
                        vPrioParaNo = vItem.ParaNo;
                        vAlignHorz = FStyle.ParaStyles[vItem.ParaNo].AlignHorz;  // 段内容水平对齐方式
                    }

                    vClearRect = vDrawRect;
                    vClearRect.Inflate(0, -vLineSpace / 2);  // 除去行间距净Rect，即内容的显示区域
                    if (vItem.StyleNo < HCStyle.Null)  // RectItem自行处理绘制
                    {
                        vRectItem = vItem as HCCustomRectItem;
                        vPrioStyleNo = vRectItem.StyleNo;

                        if (vRectItem.JustifySplit())  // 分散占空间
                        {
                            if (((vAlignHorz == ParaAlignHorz.pahJustify)
                                     && (!IsLineLastDrawItem(i))
                                  )
                                  ||
                                  (vAlignHorz == ParaAlignHorz.pahScatter)  // 分散对齐
                                )
                            {
                                if (IsLineLastDrawItem(i))
                                    vClearRect.Offset(vClearRect.Width - vRectItem.Width, 0);
                            }
                            else
                                vClearRect.Right = vClearRect.Left + vRectItem.Width;
                        }

                        switch (FStyle.ParaStyles[vItem.ParaNo].AlignVert)  // 垂直对齐方式
                        {
                            case ParaAlignVert.pavCenter:
                                vClearRect.Inflate(0, -(vClearRect.Height - vRectItem.Height) / 2);
                                break;

                            case ParaAlignVert.pavTop:
                                break;

                            default:
                                vClearRect.Top = vClearRect.Bottom - vRectItem.Height;
                                break;
                        }

                        DrawItemPaintContent(this, vDrawItem.ItemNo, i, vDrawRect, vClearRect, "", aDataDrawLeft, aDataDrawRight, aDataDrawBottom,
                            aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);

                        if (vRectItem.IsSelectComplate)  // 选中背景区域
                        {
                            aCanvas.Brush.Color = FStyle.SelColor;
                            aCanvas.FillRect(vDrawRect);
                        }

                        vItem.PaintTo(FStyle, vClearRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
                    }
                    else  // 文本Item
                    {
                        if (vItem.StyleNo != vPrioStyleNo)
                        {
                            vPrioStyleNo = vItem.StyleNo;
                            FStyle.TextStyles[vPrioStyleNo].ApplyStyle(aCanvas, aPaintInfo.ScaleY / aPaintInfo.Zoom);
                            FStyle.ApplyTempStyle(vPrioStyleNo);//, APaintInfo.ScaleY / APaintInfo.Zoom);

                            vTextHeight = FStyle.TextStyles[vPrioStyleNo].FontHeight;
                            if (FStyle.TextStyles[vPrioStyleNo].FontStyles.Contains((byte)HCFontStyle.tsSuperscript)
                                || FStyle.TextStyles[vPrioStyleNo].FontStyles.Contains((byte)HCFontStyle.tsSubscript))
                            {
                                vTextHeight = vTextHeight + vTextHeight;
                            }

                            if (vItem.HyperLink != "")
                            {
                                aCanvas.Font.Color = HC.HyperTextColor;
                                aCanvas.Font.FontStyles.InClude((byte)HCFontStyle.tsUnderline);
                            }
                        }

                        int vTextDrawTop;
                        switch (FStyle.ParaStyles[vItem.ParaNo].AlignVert)  // 垂直对齐方式
                        {
                            case ParaAlignVert.pavCenter:
                                vTextDrawTop = vClearRect.Top + (vClearRect.Bottom - vClearRect.Top - vTextHeight) / 2;
                                break;

                            case ParaAlignVert.pavTop:
                                vTextDrawTop = vClearRect.Top;
                                break;

                            default:
                                vTextDrawTop = vClearRect.Bottom - vTextHeight;
                                break;
                        }

                        if (FStyle.TextStyles[vPrioStyleNo].FontStyles.Contains((byte)HCFontStyle.tsSubscript))
                            vTextDrawTop = vTextDrawTop + vTextHeight / 2;

                        // 文字背景
                        if (FStyle.TextStyles[vPrioStyleNo].BackColor != HC.HCTransparentColor)
                        {
                            aCanvas.Brush.Color = FStyle.TextStyles[vPrioStyleNo].BackColor;
                            aCanvas.FillRect(new RECT(vClearRect.Left, vClearRect.Top, vClearRect.Left + vDrawItem.Width, vClearRect.Bottom));
                        }

                        string vText = vItem.Text.Substring(vDrawItem.CharOffs - 1, vDrawItem.CharLen);  // 为减少判断，没有直接使用GetDrawItemText(i)
                        DrawItemPaintContent(this, vDrawItem.ItemNo, i, vDrawRect, vClearRect, vText, aDataDrawLeft, aDataDrawRight,
                            aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);

                        // 绘制优先级更高的选中情况下的背景
                        if (!aPaintInfo.Print)
                        {
                            if (vDrawsSelectAll)
                            {
                                aCanvas.Brush.Color = FStyle.SelColor;
                                aCanvas.FillRect(new RECT(vDrawRect.Left, vDrawRect.Top, vDrawRect.Left + vDrawItem.Width, Math.Min(vDrawRect.Bottom, aDataScreenBottom)));
                            }
                            else  // 处理一部分选中
                            if (vSelEndDNo >= 0)  // 有选中内容，部分背景为选中
                            {
                                aCanvas.Brush.Color = FStyle.SelColor;
                                if ((vSelStartDNo == vSelEndDNo) && (i == vSelStartDNo))  // 选中内容都在当前DrawItem
                                {
                                    aCanvas.FillRect(new RECT(vDrawRect.Left + GetDrawItemOffsetWidth(i, vSelStartDOffs),
                                        vDrawRect.Top,
                                        vDrawRect.Left + GetDrawItemOffsetWidth(i, vSelEndDOffs, FStyle.TempCanvas),
                                        Math.Min(vDrawRect.Bottom, aDataScreenBottom)));
                                }
                                else
                                if (i == vSelStartDNo)  // 选中在不同DrawItem，当前是起始
                                {
                                    aCanvas.FillRect(new RECT(vDrawRect.Left + GetDrawItemOffsetWidth(i, vSelStartDOffs, FStyle.TempCanvas),
                                        vDrawRect.Top,
                                        vDrawRect.Right,
                                        Math.Min(vDrawRect.Bottom, aDataScreenBottom)));
                                }
                                else
                                if (i == vSelEndDNo)  // 选中在不同的DrawItem，当前是结束
                                {
                                    aCanvas.FillRect(new RECT(vDrawRect.Left,
                                        vDrawRect.Top,
                                        vDrawRect.Left + GetDrawItemOffsetWidth(i, vSelEndDOffs, FStyle.TempCanvas),
                                        Math.Min(vDrawRect.Bottom, aDataScreenBottom)));
                                }
                                else
                                if ((i > vSelStartDNo) && (i < vSelEndDNo))  // 选中起始和结束DrawItem之间的DrawItem
                                    aCanvas.FillRect(vDrawRect);
                            }
                        }

                        vItem.PaintTo(FStyle, vClearRect, aDataDrawTop, aDataDrawBottom,
                            aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);  // 触发Item绘制事件

                        // 绘制文本                      
                        if (vText != "")
                        {
                            if (!(aPaintInfo.Print && vItem.PrintInvisible))
                            {
                                aCanvas.Brush.Style = HCBrushStyle.bsClear;
                                switch (vAlignHorz)  // 水平对齐方式
                                {
                                    case ParaAlignHorz.pahLeft:
                                    case ParaAlignHorz.pahRight:
                                    case ParaAlignHorz.pahCenter:  // 一般对齐
                                        int vLen = vText.Length;
                                        GDI.ExtTextOut(aCanvas.Handle, vClearRect.Left, vTextDrawTop,
                                            GDI.ETO_OPAQUE, IntPtr.Zero, vText, vLen, IntPtr.Zero);
                                        break;

                                    case ParaAlignHorz.pahJustify:
                                    case ParaAlignHorz.pahScatter:  // 两端、分散对齐
                                        DrawTextJsutify(aCanvas, vClearRect, vText, IsLineLastDrawItem(i), vTextDrawTop);
                                        break;
                                }
                            }
                        }
                        else  // 空行
                        {
                            if (!vItem.ParaFirst)
                                throw new Exception(HC.HCS_EXCEPTION_NULLTEXT);
                        }
                    }

                    DrawItemPaintAfter(this, vDrawItem.ItemNo, i, vClearRect, aDataDrawLeft, aDataDrawRight, aDataDrawBottom,
                        aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);  // 绘制内容后
                }
            }
            finally
            {
                aCanvas.Restore(vDCState);
            }
        }

        public virtual void PaintData(int aDataDrawLeft, int aDataDrawTop, int aDataDrawRight, int aDataDrawBottom,
            int aDataScreenTop, int aDataScreenBottom, int aVOffset, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (FItems.Count == 0)
                return;

            int vFirstDItemNo = -1, vLastDItemNo = -1;
            int vVOffset = aDataDrawTop - aVOffset;  // 将数据起始位置映射到绘制位置
           
            GetDataDrawItemRang(Math.Max(aDataDrawTop, aDataScreenTop) - vVOffset,  // 可显示出来的DrawItem范围
                Math.Min(aDataDrawBottom, aDataScreenBottom) - vVOffset, ref vFirstDItemNo, ref vLastDItemNo);

            PaintData(aDataDrawLeft, aDataDrawTop, aDataDrawRight, aDataDrawBottom, aDataScreenTop,
              aDataScreenBottom, aVOffset, vFirstDItemNo, vLastDItemNo, aCanvas, aPaintInfo);
        }

        /// <summary> 根据行中某DrawItem获取当前行间距(行中除文本外的空白空间) </summary>
        /// <param name="aDrawNo">行中指定的DrawItem</param>
        /// <returns>行间距</returns>
        public int GetLineBlankSpace(int aDrawNo)
        {
            int vFirst = aDrawNo;
            int vLast = -1;
            GetLineDrawItemRang(ref vFirst, ref vLast);

            // 找行中最高的DrawItem
            int vMaxHi = 0;
            int vMaxDrawItemNo;

            vMaxDrawItemNo = vFirst;
            for (int i = vFirst; i <= vLast; i++)
            {
                int vHi;
                if (GetDrawItemStyle(i) < HCStyle.Null)
                    vHi = (FItems[FDrawItems[i].ItemNo] as HCCustomRectItem).Height;
                else
                    vHi = FStyle.TextStyles[FItems[FDrawItems[i].ItemNo].StyleNo].FontHeight;

                if (vHi > vMaxHi)
                {
                    vMaxHi = vHi;  // 记下最大的高度
                    vMaxDrawItemNo = i;  // 记下最高的DrawItemNo
                }
            }

            if (GetDrawItemStyle(vMaxDrawItemNo) < HCStyle.Null)
                return FStyle.LineSpaceMin;
            else
                return GetDrawItemLineSpace(vMaxDrawItemNo) - vMaxHi;  // 根据最高的DrawItem取行间距
        }

        /// <summary> 获取指定DrawItem的行间距 </summary>
        /// <param name="aDrawNo">指定的DrawItem</param>
        /// <returns>DrawItem的行间距</returns>
        public int GetDrawItemLineSpace(int aDrawNo)
        {
            int Result = FStyle.LineSpaceMin;

            if (GetDrawItemStyle(aDrawNo) >= HCStyle.Null)
            {
                //HCCanvas vCanvas = HCStyle.CreateStyleCanvas();
                //try
                //{
                Result = CalculateLineHeight(FStyle.TextStyles[GetDrawItemStyle(aDrawNo)],
                    FStyle.ParaStyles[GetDrawItemParaStyle(aDrawNo)]);
                //}
                //finally
                //{
                //    HCStyle.DestroyStyleCanvas(vCanvas);
                //}
            }

            return Result;
        }

        /// <summary> 是否有选中 </summary>
        public bool SelectExists(bool aIfRectItem = true)
        {
            bool Result = false;

            if (FSelectInfo.StartItemNo >= 0)
            {
                if (FSelectInfo.EndItemNo >= 0)
                {
                    if (FSelectInfo.StartItemNo != FSelectInfo.EndItemNo)
                        Result = true;
                    else  // 在同一Item
                        Result = (FSelectInfo.StartItemOffset != FSelectInfo.EndItemOffset);  // 同一Item不同位置
                }
                else  // 当前光标仅在一个Item中(在Rect中即使有选中，相对当前层的Data也算在一个Item)
                {
                    if (aIfRectItem && (FItems[FSelectInfo.StartItemNo].StyleNo < HCStyle.Null))
                    {
                        Result = (FItems[FSelectInfo.StartItemNo] as HCCustomRectItem).SelectExists();
                    }
                }
            }

            return Result;
        }

        public void MarkStyleUsed(bool aMark)
        {
            for (int i = 0; i <= FItems.Count - 1; i++)
            {
                HCCustomItem vItem = FItems[i];
                if (aMark)
                {
                    FStyle.ParaStyles[vItem.ParaNo].CheckSaveUsed = true;
                    if (vItem.StyleNo < HCStyle.Null)
                        (vItem as HCCustomRectItem).MarkStyleUsed(aMark);
                    else
                        FStyle.TextStyles[vItem.StyleNo].CheckSaveUsed = true;
                }
                else  // 重新赋值
                {
                    vItem.ParaNo = FStyle.ParaStyles[vItem.ParaNo].TempNo;
                    if (vItem.StyleNo < HCStyle.Null)
                        (vItem as HCCustomRectItem).MarkStyleUsed(aMark);
                    else
                        vItem.StyleNo = FStyle.TextStyles[vItem.StyleNo].TempNo;
                }
            }
        }

        public void SaveItemToStream(Stream aStream, int aStartItemNo, int aStartOffset, int aEndItemNo, int aEndOffset)
        {
            Int64 vBegPos = aStream.Position;
            byte[] vBuffer = System.BitConverter.GetBytes(vBegPos);
            aStream.Write(vBuffer, 0, vBuffer.Length);  // 数据大小占位，便于越过

            int vCount = aEndItemNo - aStartItemNo + 1;
            vBuffer = System.BitConverter.GetBytes(vCount);
            aStream.Write(vBuffer, 0, vBuffer.Length);  // 数量

            int vCountAct = 0;
            if (vCount > 0)
            {
                if (aStartItemNo != aEndItemNo)
                {
                    if (DoSaveItem(aStartItemNo))
                    {
                        FItems[aStartItemNo].SaveToStream(aStream, aStartOffset, FItems[aStartItemNo].Length);
                        vCountAct++;
                    }

                    for (int i = aStartItemNo + 1; i <= aEndItemNo - 1; i++)
                    {
                        if (DoSaveItem(i))
                        {
                            FItems[i].SaveToStream(aStream);
                            vCountAct++;
                        }
                    }

                    if (DoSaveItem(aEndItemNo))
                    {
                        FItems[aEndItemNo].SaveToStream(aStream, 0, aEndOffset);
                        vCountAct++;
                    }
                }
                else
                if (DoSaveItem(aStartItemNo))
                {
                    FItems[aStartItemNo].SaveToStream(aStream, aStartOffset, aEndOffset);
                    vCountAct++;
                }
            }
            //
            Int64 vEndPos = aStream.Position;
            aStream.Position = vBegPos;
            vBegPos = vEndPos - vBegPos - Marshal.SizeOf(vBegPos);
            vBuffer = System.BitConverter.GetBytes(vBegPos);
            aStream.Write(vBuffer, 0, vBuffer.Length);  // 当前页数据大小
            if (vCount != vCountAct)  // 实际数量
            {
                vBuffer = System.BitConverter.GetBytes(vCountAct);
                aStream.Write(vBuffer, 0, vBuffer.Length);
            }

            aStream.Position = vEndPos;
        }

        public virtual void SaveToStream(Stream aStream)
        {
            SaveToStream(aStream, 0, 0, FItems.Count - 1, Items.Last.Length);
        }

        public virtual void SaveToStream(Stream aStream, int aStartItemNo, int aStartOffset,
            int aEndItemNo, int aEndOffset)
        {
            SaveItemToStream(aStream, aStartItemNo, aStartOffset, aEndItemNo, aEndOffset);
        }

        public string SaveToText()
        {
            return SaveToText(0, 0, FItems.Count - 1, FItems.Last.Length);
        }

        public string SaveToText(int aStartItemNo, int aStartOffset, int aEndItemNo, int aEndOffset)
        {
            string Result = "";

            int vi = aEndItemNo - aStartItemNo + 1;

            if (vi > 0)
            {
                if (aStartItemNo != aEndItemNo)
                {
                    if (DoSaveItem(aStartItemNo))
                    {
                        if (FItems[aStartItemNo].StyleNo > HCStyle.Null)
                            Result = (FItems[aStartItemNo] as HCTextItem).SubString(aStartOffset + 1, FItems[aStartItemNo].Length - aStartOffset);
                        else
                            Result = (FItems[aStartItemNo] as HCCustomRectItem).SaveSelectToText();
                    }

                    for (int i = aStartItemNo + 1; i <= aEndItemNo - 1; i++)
                    {
                        if (DoSaveItem(i))
                        {
                            if (FItems[i].ParaFirst)
                                Result = Result + HC.sLineBreak + FItems[i].Text;
                            else
                                Result = Result + FItems[i].Text;
                        }
                    }

                    if (DoSaveItem(aEndItemNo))
                    {
                        if (FItems[aEndItemNo].StyleNo > HCStyle.Null)
                        {
                            if (FItems[aEndItemNo].ParaFirst)
                                Result = Result + HC.sLineBreak;

                            Result = Result + (FItems[aEndItemNo] as HCTextItem).SubString(1, aEndOffset);
                        }
                        else
                        {
                            if (FItems[aEndItemNo].ParaFirst)
                                Result = Result + HC.sLineBreak;

                            Result = (FItems[aEndItemNo] as HCCustomRectItem).SaveSelectToText();
                        }
                    }
                }
                else  // 选中在同一Item
                {
                    if (DoSaveItem(aStartItemNo))
                    {
                        if (FItems[aStartItemNo].StyleNo > HCStyle.Null)
                            Result = (FItems[aStartItemNo] as HCTextItem).SubString(aStartOffset + 1, aEndOffset - aStartOffset);
                    }
                }
            }

            return Result;
        }
        
        /// <summary> 保存选中内容到流 </summary>
        public virtual void SaveSelectToStream(Stream aStream)
        {
            if (SelectExists())
            {
                if ((FSelectInfo.EndItemNo < 0)
                  && (FItems[FSelectInfo.StartItemNo].StyleNo < HCStyle.Null))
                {
                    if ((FItems[FSelectInfo.StartItemNo] as HCCustomRectItem).IsSelectComplateTheory())
                    {
                        this.SaveToStream(aStream, FSelectInfo.StartItemNo, HC.OffsetBefor, FSelectInfo.StartItemNo, HC.OffsetAfter);
                    }
                    else
                        (FItems[FSelectInfo.StartItemNo] as HCCustomRectItem).SaveSelectToStream(aStream);
                }
                else
                {
                    this.SaveToStream(aStream, FSelectInfo.StartItemNo, FSelectInfo.StartItemOffset, FSelectInfo.EndItemNo, FSelectInfo.EndItemOffset);
                }
            }
        }

        public string SaveSelectToText()
        {
            string Result = "";

            if (SelectExists())
            {
                if ((FSelectInfo.EndItemNo < 0) && (FItems[FSelectInfo.StartItemNo].StyleNo < HCStyle.Null))
                    Result = (FItems[FSelectInfo.StartItemNo] as HCCustomRectItem).SaveSelectToText();
                else
                {
                    Result = this.SaveToText(FSelectInfo.StartItemNo, FSelectInfo.StartItemOffset,
                        FSelectInfo.EndItemNo, FSelectInfo.EndItemOffset);
                }
            }

            return Result;
        }

        public string GetSelectText()
        {
            return SaveSelectToText();
        }

        public virtual bool InsertStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            return false;
        }

        public void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            FLoading = true;
            try
            {
                DoLoadFromStream(aStream, aStyle, aFileVersion);
            }
            finally
            {
                FLoading = false;
            }
        }

        public string ToHtml(string aPath)
        {
            string Result = "";
            for (int i = 0; i <= Items.Count - 1; i++)
            {
                if (Items[i].ParaFirst)
                {
                    if (i != 0)
                        Result = Result + HC.sLineBreak + "</p>";

                    Result = Result + HC.sLineBreak + "<p class=\"ps" + Items[i].ParaNo.ToString() + "\">";
                }

                Result = Result + HC.sLineBreak + Items[i].ToHtml(aPath);
            }

            return Result + HC.sLineBreak + "</p>";
        }

        public virtual void ToXml(XmlElement aNode)
        {
            aNode.SetAttribute("itemcount", FItems.Count.ToString());
            for (int i = 0; i <= FItems.Count - 1; i++)
            {
                XmlElement vNode = aNode.OwnerDocument.CreateElement("item");
                FItems[i].ToXml(vNode);
                aNode.AppendChild(vNode);
            }
        }

        public virtual void ParseXml(XmlElement aNode)
        {
            Clear();

            for (int i = 0; i <= aNode.ChildNodes.Count - 1; i++)
            {
                XmlElement vItemNode = aNode.ChildNodes[i] as XmlElement;
                HCCustomItem vItem = CreateItemByStyle(int.Parse(vItemNode.Attributes["sno"].Value));
                vItem.ParseXml(vItemNode);
                FItems.Add(vItem);
            }

            if (FItems[0].Length == 0)  // 删除Clear后默认的第一个空行Item
                FItems.Delete(0);
        }

        public HCCustomData ParentData
        {
            get { return FParentData; }
            set { FParentData = value; }
        }

        public HCStyle Style
        {
            get { return FStyle; }
        }

        public int CurStyleNo
        {
            get { return FCurStyleNo; }
            set { SetCurStyleNo(value); }
        }

        public int CurParaNo
        {
            get { return FCurParaNo; }
            set { SetCurParaNo(value); }
        }

        public HCItems Items
        {
            get { return FItems; }
        }

        public HCDrawItems DrawItems
        {
            get { return FDrawItems; }
        }

        public SelectInfo SelectInfo
        {
            get { return FSelectInfo; }
        }

        public HashSet<DrawOption> DrawOptions
        {
            get { return FDrawOptions; }
            set { FDrawOptions = value; }
        }

        public int CaretDrawItemNo
        {
            get { return FCaretDrawItemNo; }
            set { SetCaretDrawItemNo(value); }
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

        public DrawItemPaintEventHandler OnDrawItemPaintBefor
        {
            get { return FOnDrawItemPaintBefor; }
            set { FOnDrawItemPaintBefor = value; }
        }

        public DrawItemPaintEventHandler OnDrawItemPaintAfter
        {
            get { return FOnDrawItemPaintAfter; }
            set { FOnDrawItemPaintAfter = value; }
        }

        public DrawItemPaintContentEventHandler OnDrawItemPaintContent
        {
            get { return FOnDrawItemPaintContent; }
            set { FOnDrawItemPaintContent = value; }
        }

        public DataItemEventHandler OnInsertItem
        {
            get { return FOnInsertItem; }
            set { FOnInsertItem = value; }
        }

        public DataItemEventHandler OnRemoveItem
        {
            get { return FOnRemoveItem; }
            set { FOnRemoveItem = value; }
        }

        public DataItemNoFunEventHandler OnSaveItem
        {
            get { return FOnSaveItem; }
            set { FOnSaveItem = value; }
        }
    }
}
