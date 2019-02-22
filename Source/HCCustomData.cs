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
    public delegate void TraverseItemEventHandle(HCCustomData aData, int aItemNo, int aTag, ref bool aStop);

    public class HCItemTraverse
    {
        public HCSet Area;
        public int Tag;
        public bool Stop;
        public TraverseItemEventHandle Process;
    }

    public class SelectInfo : object
    {
        private int FStartItemNo,  // 不能使用DrawItem记录，因为内容变动时Item的指定Offset对应的DrawItem，可能和变动前不一样
            FStartItemOffset,  // 选中起始在第几个字符后面，0表示在Item最前面
            FEndItemNo,
            FEndItemOffset;  // 选中结束在第几个字符后面

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

    public class HCCustomData : HCObject
    {
        private HCStyle FStyle;
        HCItems FItems;
        HCDrawItems FDrawItems;
        SelectInfo FSelectInfo;
        HashSet<DrawOption> FDrawOptions;
        int FCaretDrawItemNo;  // 当前Item光标处的DrawItem限定其只在相关的光标处理中使用(解决同一Item分行后Offset为行尾时不能区分是上行尾还是下行始)
        GetUndoListEventHandler FOnGetUndoList;

        private void DrawItemPaintBefor(HCCustomData aData, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            int vDCState = ACanvas.Save();
            try
            {
                this.DoDrawItemPaintBefor(aData, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawBottom, aDataScreenTop,
                    aDataScreenBottom, ACanvas, APaintInfo);
            }
            finally
            {
                ACanvas.Restore(vDCState);
            }
        }

        private void DrawItemPaintAfter(HCCustomData aData, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            int vDCState = aCanvas.Save();
            try
            {
                this.DoDrawItemPaintAfter(aData, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawBottom, aDataScreenTop,
                    aDataScreenBottom, aCanvas, aPaintInfo);
            }
            finally
            {
                aCanvas.Restore(vDCState);
            }
        }

        private void DrawItemPaintContent(HCCustomData aData, int aDrawItemNo, RECT aDrawRect,
            RECT aClearRect, string aDrawText, int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop,
            int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            int vDCState = aCanvas.Save();
            try
            {
                this.DoDrawItemPaintContent(aData, aDrawItemNo, aDrawRect, aClearRect, aDrawText,
                    aDataDrawLeft, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
            }
            finally
            {
                aCanvas.Restore(vDCState);
            }
        }

        private const int MS_HHEA_TAG = 0x61656868;
        private const int CJK_CODEPAGE_BITS = (1 << 17) | (1 << 18) | (1 << 19) | (1 << 20) | (1 << 21);

        private ushort SwapBytes(ushort value)
        {
            return (ushort)((value >> 8) | (ushort)(value << 8));
        }

        /// <summary> 计算行高(文本高+行间距) </summary>
        private int _CalculateLineHeight(HCCanvas aCanvas, HCTextStyle aTextStyle, ParaLineSpaceMode aLineSpaceMode)
        {
            int Result = 0;
            aTextStyle.ApplyStyle(aCanvas);

            Result = HCStyle.GetFontHeight(aCanvas);  // 行高
            IntPtr vDC = aCanvas.Handle;

            Win32.OUTLINETEXTMETRICW vOutlineTextmetric = new OUTLINETEXTMETRICW();
            vOutlineTextmetric.otmSize = Marshal.SizeOf(vOutlineTextmetric);
            if (GDI.GetOutlineTextMetrics(vDC, vOutlineTextmetric.otmSize, ref vOutlineTextmetric) != 0)
            {
                TT_HHEA vHorizontalHeader = new TT_HHEA();
                //ZeroMemory(@vHorizontalHeader, SizeOf(vHorizontalHeader));
                if (GDI.GetFontData(vDC, MS_HHEA_TAG, 0, ref vHorizontalHeader, Marshal.SizeOf(vHorizontalHeader)) == GDI.GDI_ERROR)  // 取字体度量信息
                  return Result;

                ushort vAscent = SwapBytes((ushort)vHorizontalHeader.Ascender);
                ushort vDescent = (ushort)-SwapBytes((ushort)vHorizontalHeader.Descender);
                ushort vLineGap = SwapBytes((ushort)vHorizontalHeader.LineGap);
                int vLineSpacing = vAscent + vDescent + vLineGap;

                Single vSizeScale = aTextStyle.Size / FStyle.FontSizeScale;
                vSizeScale = vSizeScale / vOutlineTextmetric.otmEMSquare;
                vAscent = (ushort)Math.Ceiling(vAscent * vSizeScale);
                vDescent = (ushort)Math.Ceiling(vDescent * vSizeScale);
                vLineSpacing = (int)Math.Ceiling(vLineSpacing * vSizeScale);

                Win32.FONTSIGNATURE vFontSignature = new FONTSIGNATURE();
                if ((GDI.GetTextCharsetInfo(vDC, ref vFontSignature, 0) != GDI.DEFAULT_CHARSET)
                  && ((vFontSignature.fsCsb[0] & CJK_CODEPAGE_BITS) != 0))
                {  // CJK Font
                    if ((vOutlineTextmetric.otmfsSelection & 128) != 0)
                    {
                        vAscent = (ushort)vOutlineTextmetric.otmAscent;
                        vDescent = (ushort)-vOutlineTextmetric.otmDescent;
                        vLineSpacing = (int)(vAscent + vDescent + vOutlineTextmetric.otmLineGap);
                    }
                    else
                    {
                        //vUnderlinePosition := Ceil(vAscent * 1.15 + vDescent * 0.85);
                        vLineSpacing = (int)Math.Ceiling(1.3 * (vAscent + vDescent));
                        int vDelta = vLineSpacing - (vAscent + vDescent);
                        int vLeading = vDelta / 2;
                        int vOtherLeading = vDelta - vLeading;
                        vAscent = (ushort)(vAscent + vLeading);
                        vDescent = (ushort)(vDescent + vOtherLeading);

                        Result = vAscent + vDescent;

                        switch (aLineSpaceMode)
                        {
                            case ParaLineSpaceMode.pls115:
                                Result = Result + (int)Math.Truncate(3 * Result / 20.0f);
                                break;

                            case ParaLineSpaceMode.pls150:
                                Result = (int)Math.Truncate(3 * Result / 2.0f);
                                break;

                            case ParaLineSpaceMode.pls200:
                                Result = Result * 2;
                                break;

                            case ParaLineSpaceMode.plsFix:
                                Result = Result + HC.LineSpaceMin;
                                break;
                        }
                    }
                }
            }
            else
            {
                TEXTMETRICW vTextMetric = aTextStyle.TextMetric;

                switch (aLineSpaceMode)
                {
                    case ParaLineSpaceMode.pls100: 
                        Result = Result + vTextMetric.tmExternalLeading; // Round(vTextMetric.tmHeight * 0.2);
                        break;

                    case ParaLineSpaceMode.pls115: 
                        Result = Result + vTextMetric.tmExternalLeading + (int)Math.Round((vTextMetric.tmHeight + vTextMetric.tmExternalLeading) * 0.15);
                        break;

                    case ParaLineSpaceMode.pls150: 
                        Result = Result + vTextMetric.tmExternalLeading + (int)Math.Round((vTextMetric.tmHeight + vTextMetric.tmExternalLeading) * 0.5);
                        break;

                    case ParaLineSpaceMode.pls200: 
                        Result = Result + vTextMetric.tmExternalLeading + vTextMetric.tmHeight + vTextMetric.tmExternalLeading;
                        break;

                    case ParaLineSpaceMode.plsFix: 
                        Result = Result + HC.LineSpaceMin;
                        break;
                }
            }

            return Result;
        }
    

        /// <summary> 处理选中范围内Item的全选中、部分选中状态 </summary>
        protected void MatchItemSelectState()
        {
            if (SelectExists())
            {
                #region CheckItemSelectedState检测某个Item的选中状态
                var CheckItemSelectedState = new Action<int>((int AItemNo) =>
                {
                    if ((AItemNo > SelectInfo.StartItemNo) && (AItemNo < SelectInfo.EndItemNo))  // 在选中范围之间
                    {
                        Items[AItemNo].SelectComplate();
                    }
                    else
                    {
                        if (AItemNo == SelectInfo.StartItemNo)  // 选中起始
                        {
                            if (AItemNo == SelectInfo.EndItemNo)  // 选中在同一个Item
                            {
                                if (Items[AItemNo].StyleNo < HCStyle.Null)  // RectItem
                                {
                                    if ((SelectInfo.StartItemOffset == HC.OffsetInner) || (SelectInfo.EndItemOffset == HC.OffsetInner))
                                    {
                                        Items[AItemNo].SelectPart();
                                    }
                                    else
                                    {
                                        Items[AItemNo].SelectComplate();
                                    }
                                }
                                else  // TextItem
                                {
                                    if ((SelectInfo.StartItemOffset == 0) && (SelectInfo.EndItemOffset == Items[AItemNo].Length))
                                    {
                                        Items[AItemNo].SelectComplate();
                                    }
                                    else
                                    {
                                        Items[AItemNo].SelectPart();
                                    }
                                }
                            }
                            else  // 选中在不同的Item，当前是起始
                            {
                                if (SelectInfo.StartItemOffset == 0)
                                {
                                    Items[AItemNo].SelectComplate();
                                }
                                else
                                {
                                    Items[AItemNo].SelectPart();
                                }
                            }
                        }
                        else  // 选中在不同的Item，当前是结尾 if AItemNo = SelectInfo.EndItemNo) then
                        {
                            if (Items[AItemNo].StyleNo < HCStyle.Null)  // RectItem
                            {
                                if (SelectInfo.EndItemOffset == HC.OffsetAfter)
                                {
                                    Items[AItemNo].SelectComplate();
                                }
                                else
                                {
                                    Items[AItemNo].SelectPart();
                                }
                            }
                            else  // TextItem
                            {
                                if (SelectInfo.EndItemOffset == Items[AItemNo].Length)
                                {
                                    Items[AItemNo].SelectComplate();
                                }
                                else
                                {
                                    Items[AItemNo].SelectPart();
                                }
                            }
                        }
                    }
                });
                #endregion

                for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)  // 起始结束之间的按全选中处理
                {
                    CheckItemSelectedState(i);
                }
            }
        }

        protected virtual HCCustomItem CreateItemByStyle(int aStyleNo)
        {
            return null;
        }

        /// <summary> 准备格式化参数 </summary>
        /// <param name="AStartItemNo">开始格式化的Item</param>
        /// <param name="APrioDItemNo">上一个Item的最后一个DrawItemNo</param>
        /// <param name="APos">开始格式化位置</param>
        protected virtual void _FormatReadyParam(int aStartItemNo, ref int aPrioDrawItemNo, ref POINT aPos) { }

        // Format仅负责格式化Item，ReFormat负责格式化后对后面Item和DrawItem的关联处理
        protected virtual void _ReFormatData(int aStartItemNo, int aLastItemNo = -1, int aExtraItemCount = 0) { }

        /// <summary> 当前Item对应的格式化起始Item和结束Item(段最后一个Item) </summary>
        /// <param name="AFirstItemNo">起始ItemNo</param>
        /// <param name="ALastItemNo">结束ItemNo</param>
        protected void GetReformatItemRange(ref int aFirstItemNo, ref int aLastItemNo)
        {
            GetReformatItemRange(ref aFirstItemNo, ref aLastItemNo, FSelectInfo.StartItemNo, FSelectInfo.StartItemOffset);
        }

        /// <summary> 指定Item对应的格式化起始Item和结束Item(段最后一个Item) </summary>
        /// <param name="AFirstItemNo">起始ItemNo</param>
        /// <param name="ALastItemNo">结束ItemNo</param>
        protected void GetReformatItemRange(ref int aFirstItemNo, ref int aLastItemNo, int aItemNo, int aItemOffset)
        {
            if ((aItemNo > 0) && FDrawItems[FItems[aItemNo].FirstDItemNo].LineFirst && (aItemOffset == 0))  // 在开头
            {
                if (!FItems[aItemNo].ParaFirst)  // 不是段首
                    aFirstItemNo = GetLineFirstItemNo(aItemNo - 1, FItems[aItemNo - 1].Length);
                else  // 是段首
                    aFirstItemNo = aItemNo;
            }
            else
                aFirstItemNo = GetLineFirstItemNo(aItemNo, 0);  // 取行第一个DrawItem对应的ItemNo

            aLastItemNo = GetParaLastItemNo(aItemNo);
        }

        /// <summary> 式化时，记录起始DrawItem和段最后的DrawItem </summary>
        /// <param name="aStartItemNo"></param>
        protected void _FormatItemPrepare(int aStartItemNo, int aEndItemNo = -1)
        {
            int vLastDrawItemNo = -1;
            int vFirstDrawItemNo = FItems[aStartItemNo].FirstDItemNo;
            if (aEndItemNo < 0)
            {
                vLastDrawItemNo = GetItemLastDrawItemNo(aStartItemNo);
            }
            else
            {
                vLastDrawItemNo = GetItemLastDrawItemNo(aEndItemNo);
            }

            FDrawItems.MarkFormatDelete(vFirstDrawItemNo, vLastDrawItemNo);
            FDrawItems.FormatBeforBottom = FDrawItems[vLastDrawItemNo].Rect.Bottom;
        }

        private static bool IsCharSameType(Char a, char b)
        {
            //if A = B then
            //  Result := True
            //else
              return false;
        }
        /// <summary> 返回字符串AText的分散分隔数量和各分隔的起始位置 </summary>
        /// <param name="aText">要计算的字符串</param>
        /// <param name="aCharIndexs">记录各分隔的起始位置</param>
        /// <returns>分散分隔数量</returns>
        private static int GetJustifyCount(string aText, List<int> aCharIndexs)
        {
            int Result = 0;
            if (aText == "")
            {
                throw new Exception("异常：不能对空字符串计算分散!");
            }

            if (aCharIndexs != null)
            {
                aCharIndexs.Clear();
            }

            Char vProvChar = (Char)0;

            for (int i = 1; i <= aText.Length; i++)
            {
                if (!IsCharSameType(vProvChar, aText[i - 1]))
                {
                    Result++;
                    if (aCharIndexs != null)
                    {
                        aCharIndexs.Add(i);
                    }
                }
                
                vProvChar = aText[i - 1];
            }

            if (aCharIndexs != null)
            {
                aCharIndexs.Add(aText.Length + 1);
            }

            return Result;
        }



#region FindLineBreak

        //GetHeadTailBreak 根据行首、尾对字符的约束条件，获取截断位置
        private void GetHeadTailBreak(string aText, ref int aPos)
        {
            if (aPos < 1)
                return;

            Char vChar = aText[aPos + 1 - 1];  // 因为是要处理截断，所以APos肯定是小于Length(AText)的，不用考虑越界
            if (HC.PosCharHC(vChar, HC.DontLineFirstChar) > 0)  // 下一个是不能放在行首的字符
            {
                aPos--;  // 当前要移动到下一行，往前一个截断重新判断
                GetHeadTailBreak(aText, ref aPos);
            }
            else  // 下一个可以放在行首，当前位置能否放置到行尾
            {
                vChar = aText[aPos - 1 ];  // 当前位置字符
                if (HC.PosCharHC(vChar, HC.DontLineLastChar) > 0)  // 是不能放在行尾的字符
                {
                    aPos--;  // 再往前寻找截断位置
                    GetHeadTailBreak(aText, ref aPos);
                }
            }
        }

        /// <summary>
        /// 获取字符串排版时截断到下一行的位置
        /// </summary>
        /// <param name="aText"></param>
        /// <param name="aPos">在第X个后面断开 X > 0</param>
        private void FindLineBreak(string aText, int aStartPos, ref int aPos)
        {
            GetHeadTailBreak(aText, ref aPos);  // 根据行首、尾的约束条件找APos不符合时应该在哪一个位置并重新赋值给APos
            if (aPos < 1)
                return;

            CharType vPosCharType = HC.GetUnicodeCharType((ushort)(aText[aPos - 1]));  // 当前类型
            CharType vNextCharType = HC.GetUnicodeCharType((ushort)(aText[aPos + 1 - 1]));  // 下一个字符类型

            if (HC.MatchBreak(vPosCharType, vNextCharType, aText, aPos + 1) != BreakPosition.jbpPrev)  // 不能在当前截断，当前往前找截断
            {
                if (vPosCharType != CharType.jctBreak)
                {
                    bool vFindBreak = false;
                    CharType vPrevCharType;
                    for (int i = aPos - 1; i >= aStartPos; i--)
                    {
                        vPrevCharType = HC.GetUnicodeCharType((ushort)(aText[i - 1]));
                        if (HC.MatchBreak(vPrevCharType, vPosCharType, aText, i + 1) == BreakPosition.jbpPrev)
                        {
                            aPos = i;
                            vFindBreak = true;
                            break;
                        }

                        vPosCharType = vPrevCharType;
                    }

                    if (!vFindBreak)  // 没找到
                        aPos = 0;
                }
            }
        }
#endregion

        /// <summary> 从指定偏移和指定位置开始格式化Text </summary>
        /// <param name="aCharOffset">文本格式化的起始偏移</param>
        /// <param name="aPlaceWidth">呈放文本的宽度</param>
        /// <param name="aBasePos">vCharWidths中对应偏移的起始位置</param>
        private void DoFormatTextItemToDrawItems(string vText, int aCharOffset, int aPlaceWidth, int aBasePos,
            int aItemNo, int viLen, int vItemHeight, int aFmtLeft, int aContentWidth, int aFmtRight, 
            int[] vCharWidths, ref bool vParaFirst, ref bool vLineFirst, ref POINT aPos, ref RECT vRect, 
            ref int vRemainderWidth, ref int aLastDrawItemNo)
        {
            int viPlaceOffset,  // 能放下第几个字符
            viBreakOffset,  // 第几个字符放不下
            vFirstCharWidth;  // 第一个字符的宽度

            vLineFirst = (aPos.X == aFmtLeft);
            viBreakOffset = 0;  // 换行位置，第几个字符放不下
            vFirstCharWidth = vCharWidths[aCharOffset - 1] - aBasePos;  // 第一个字符的宽度

            if (aPlaceWidth < 0)
                viBreakOffset = 1;
            else
            {
                for (int i = aCharOffset - 1; i <= viLen - 1; i++)
                {
                    if (vCharWidths[i] - aBasePos > aPlaceWidth)
                    {
                        viBreakOffset = i + 1;
                        break;
                    }
                }
            }

            if (viBreakOffset < 1)  // 当前行剩余空间把vText全放置下了
            {
                vRect.Left = aPos.X;
                vRect.Top = aPos.Y;
                vRect.Width = vCharWidths[viLen - 1] - aBasePos;  // 使用自定义测量的结果
                vRect.Height = vItemHeight;
                NewDrawItem(aItemNo, aCharOffset, viLen - aCharOffset + 1, vRect, vParaFirst, vLineFirst, ref aLastDrawItemNo);
                vParaFirst = false;

                vRemainderWidth = aFmtRight - vRect.Right;  // 放入最多后的剩余量
            }
            else
            if (viBreakOffset == 1)  // 当前行剩余空间连第一个字符也放不下
            {
                if (vFirstCharWidth > aFmtRight - aFmtLeft)  // Data的宽度不足一个字符
                {
                    vRect.Left = aPos.X;
                    vRect.Top = aPos.Y;
                    vRect.Width = vCharWidths[viLen - 1] - aBasePos;  // 使用自定义测量的结果
                    vRect.Height = vItemHeight;
                    NewDrawItem(aItemNo, aCharOffset, 1, vRect, vParaFirst, vLineFirst, ref aLastDrawItemNo);
                    vParaFirst = false;

                    vRemainderWidth = aFmtRight - vRect.Right;  // 放入最多后的剩余量
                    FinishLine(aItemNo, aLastDrawItemNo, vRemainderWidth);

                    // 偏移到下一行顶端，准备另起一行
                    aPos.X = aFmtLeft;
                    aPos.Y = FDrawItems[aLastDrawItemNo].Rect.Bottom;  // 不使用 vRect.Bottom 因为如果行中间有高的，会修正vRect.Bottom

                    if (aCharOffset < viLen)
                    {
                        DoFormatTextItemToDrawItems(vText, aCharOffset + 1, aFmtRight - aPos.X, vCharWidths[aCharOffset - 1],
                            aItemNo, viLen, vItemHeight, aFmtLeft, aContentWidth, aFmtRight, vCharWidths, 
                            ref vParaFirst, ref vLineFirst, ref aPos, ref vRect, ref vRemainderWidth, ref aLastDrawItemNo);
                    }
                }
                else  // Data的宽度足够一个字符
                if ((HC.PosCharHC(vText[aCharOffset - 1], HC.DontLineFirstChar) > 0)
                    && (FItems[aItemNo - 1].StyleNo > HCStyle.Null)
                    && (FDrawItems[aLastDrawItemNo].CharLen > 1))
                {
                    _FormatBreakTextDrawItem(aItemNo, aFmtLeft, aFmtRight, ref aLastDrawItemNo, ref aPos, ref vRect, ref vRemainderWidth, ref vParaFirst);  // 上一个重新分裂
                    DoFormatTextItemToDrawItems(vText, aCharOffset, aFmtRight - aPos.X, aBasePos, aItemNo, viLen, vItemHeight, aFmtLeft, aContentWidth, 
                        aFmtRight, vCharWidths, ref vParaFirst, ref vLineFirst, ref aPos, ref vRect, ref vRemainderWidth, ref aLastDrawItemNo);
                }
                else  // 整体下移到下一行
                {
                    vRemainderWidth = aPlaceWidth;
                    FinishLine(aItemNo, aLastDrawItemNo, vRemainderWidth);
                    // 偏移到下一行开始计算
                    aPos.X = aFmtLeft;
                    aPos.Y = FDrawItems[aLastDrawItemNo].Rect.Bottom;
                    DoFormatTextItemToDrawItems(vText, aCharOffset, aFmtRight - aPos.X, aBasePos,
                        aItemNo, viLen, vItemHeight, aFmtLeft, aContentWidth, aFmtRight, vCharWidths, 
                        ref vParaFirst, ref vLineFirst, ref aPos, ref vRect, ref vRemainderWidth, ref aLastDrawItemNo);
                }
            }
            else  // 当前行剩余宽度能放下当前Text的一部分
            {
                if (vFirstCharWidth > aFmtRight - aFmtLeft)  // Data的宽度不足一个字符
                {
                    viPlaceOffset = viBreakOffset;
                }
                else
                {
                    viPlaceOffset = viBreakOffset - 1;  // 第viBreakOffset个字符放不下，前一个能放下
                }

                FindLineBreak(vText, aCharOffset, ref viPlaceOffset);  // 判断从viPlaceOffset后打断是否合适

                if ((viPlaceOffset == 0) && (!vLineFirst))  // 能放下的都不合适放到当前行且不是行首格式化，整体下移
                {
                    vRemainderWidth = aPlaceWidth;
                    FinishLine(aItemNo, aLastDrawItemNo, vRemainderWidth);
                    aPos.X = aFmtLeft;  // 偏移到下一行开始计算
                    aPos.Y = FDrawItems[aLastDrawItemNo].Rect.Bottom;
                    DoFormatTextItemToDrawItems(vText, aCharOffset, aFmtRight - aPos.X, aBasePos, aItemNo, viLen, vItemHeight, 
                        aFmtLeft, aContentWidth, aFmtRight, vCharWidths, ref vParaFirst, ref vLineFirst, ref aPos, ref vRect, ref vRemainderWidth, ref aLastDrawItemNo);
                }
                else  // 有适合放到当前行的内容
                {
                    if (viPlaceOffset < aCharOffset)  // 找不到截断位置，就在原位置截断(如整行文本都是逗号)
                    {
                        if (vFirstCharWidth > aFmtRight - aFmtLeft)  // Data的宽度不足一个字符
                        {
                            viPlaceOffset = viBreakOffset;
                        }
                        else
                        {
                            viPlaceOffset = viBreakOffset - 1;
                        }
                    }

                    vRect.Left = aPos.X;
                    vRect.Top = aPos.Y;
                    vRect.Width = vCharWidths[viPlaceOffset - 1] - aBasePos;  // 使用自定义测量的结果
                    vRect.Height = vItemHeight;

                    NewDrawItem(aItemNo, aCharOffset, viPlaceOffset - aCharOffset + 1, vRect, vParaFirst, vLineFirst, ref aLastDrawItemNo);
                    vParaFirst = false;

                    vRemainderWidth = aFmtRight - vRect.Right;  // 放入最多后的剩余量
                    FinishLine(aItemNo, aLastDrawItemNo, vRemainderWidth);

                    // 偏移到下一行顶端，准备另起一行
                    aPos.X = aFmtLeft;
                    aPos.Y = FDrawItems[aLastDrawItemNo].Rect.Bottom;  // 不使用 vRect.Bottom 因为如果行中间有高的，会修正vRect.Bottom

                    if (viPlaceOffset < viLen)
                    {
                        DoFormatTextItemToDrawItems(vText, viPlaceOffset + 1, aFmtRight - aPos.X, vCharWidths[viPlaceOffset - 1],
                            aItemNo, viLen, vItemHeight, aFmtLeft, aContentWidth, aFmtRight, vCharWidths, ref vParaFirst, ref vLineFirst, ref aPos,
                            ref vRect, ref vRemainderWidth, ref aLastDrawItemNo);
                    }
                }
            }
        }
        #region FinishLine
        /// <summary> 重整行 </summary>
        /// <param name="AEndDItemNo">行最后一个DItem</param>
        /// <param name="aRemWidth">行剩余宽度</param>
        private void FinishLine(int aItemNo, int aLineEndDItemNo, int aRemWidth)
        {
            int vLineBegDItemNo,  // 行第一个DItem
                vMaxBottom,
                viSplitW, vExtraW, vW, vMaxHiDrawItem,
                vLineSpaceCount,   // 当前行分几份
                vDItemSpaceCount,  // 当前DrawItem分几份
                vDWidth,
                vModWidth,
                vCountWillSplit;  // 当前行有几个DItem参与分份

            // 根据行中最高的DrawItem处理其他DrawItem的高度 
            vLineBegDItemNo = aLineEndDItemNo;
            for (int i = aLineEndDItemNo; i >= 0; i--)  // 得到行起始DItemNo
            {
                if (FDrawItems[i].LineFirst)
                {
                    vLineBegDItemNo = i;
                    break;
                }
            }
            //Assert((vLineBegDItemNo >= 0), '断言失败：行起始DItemNo小于0！');
            // 找行DrawItem中最高的
            vMaxHiDrawItem = aLineEndDItemNo;  // 默认最后一个最高
            vMaxBottom = FDrawItems[aLineEndDItemNo].Rect.Bottom;  // 先默认行最后一个DItem的Rect底位置最大
            for (int i = aLineEndDItemNo - 1; i >= vLineBegDItemNo; i--)
            {
                if (FDrawItems[i].Rect.Bottom > vMaxBottom)
                {
                    vMaxBottom = FDrawItems[i].Rect.Bottom;  // 记下最大的Rect底位置
                    vMaxHiDrawItem = i;
                }
            }

            // 根据最高的处理行间距，并影响到同行DrawItem
            for (int i = aLineEndDItemNo; i >= vLineBegDItemNo; i--)
            {
                FDrawItems[i].Rect.Height = vMaxBottom - FDrawItems[i].Rect.Top;
            }

            // 处理对齐方式，放在这里，是因为方便计算行起始和结束DItem，避免绘制时的运算
            HCParaStyle vParaStyle = FStyle.ParaStyles[FItems[aItemNo].ParaNo];
            switch (vParaStyle.AlignHorz)  // 段内容水平对齐方式
            {
                case ParaAlignHorz.pahLeft:  // 默认
                    break;

                case ParaAlignHorz.pahRight:
                    {
                        for (int i = aLineEndDItemNo; i >= vLineBegDItemNo; i--)
                        {
                            FDrawItems[i].Rect.Offset(aRemWidth, 0);
                        }
                    }
                    break;

                case ParaAlignHorz.pahCenter:
                    {
                        viSplitW = aRemWidth / 2;
                        for (int i = aLineEndDItemNo; i >= vLineBegDItemNo; i--)
                        {
                            FDrawItems[i].Rect.Offset(viSplitW, 0);
                        }
                    }
                    break;

                case ParaAlignHorz.pahJustify:  // 20170220001 两端、分散对齐相关处理
                case ParaAlignHorz.pahScatter:
                    {
                        if (vParaStyle.AlignHorz == ParaAlignHorz.pahJustify)  // 两端对齐
                        {
                            if (IsParaLastDrawItem(aLineEndDItemNo))  // 两端对齐，段最后一行不处理
                            {
                                return;
                            }
                        }
                        else  // 分散对齐，空行或只有一个字符时居中
                        {
                            if (vLineBegDItemNo == aLineEndDItemNo)  // 行只有一个DrawItem
                            {
                                if (FItems[FDrawItems[vLineBegDItemNo].ItemNo].Length < 2)  // 此DrawItem对应的内容长度不足2个按居中处理
                                {
                                    viSplitW = aRemWidth / 2;
                                    FDrawItems[vLineBegDItemNo].Rect.Offset(viSplitW, 0);
                                    return;
                                }
                            }
                        }

                        vCountWillSplit = 0;
                        vLineSpaceCount = 0;
                        vExtraW = 0;
                        vModWidth = 0;
                        viSplitW = aRemWidth;
                        ushort[] vDrawItemSplitCounts = new ushort[aLineEndDItemNo - vLineBegDItemNo + 1];  // 当前行各DItem分几份

                        for (int i = vLineBegDItemNo; i <= aLineEndDItemNo; i++)  // 计算空余分成几份
                        {
                            if (GetDrawItemStyle(i) < HCStyle.Null)  // RectItem
                            {
                                if ((FItems[FDrawItems[i].ItemNo] as HCCustomRectItem).JustifySplit())  // 分散对齐占间距
                                {
                                    vDItemSpaceCount = 1;  // Graphic等占间距
                                }
                                else
                                {
                                    vDItemSpaceCount = 0; // Tab等不占间距
                                }
                            }
                            else  // TextItem
                            {
                                vDItemSpaceCount = GetJustifyCount(GetDrawItemText(i), null);  // 当前DItem分了几份
                                if ((i == aLineEndDItemNo) && (vDItemSpaceCount > 0))  // 行尾的DItem，少分一个
                                {
                                    vDItemSpaceCount--;
                                }
                            }

                            vDrawItemSplitCounts[i - vLineBegDItemNo] = (ushort)vDItemSpaceCount;  // 记录当前DItem分几份
                            vLineSpaceCount = vLineSpaceCount + vDItemSpaceCount;  // 记录行内总共分几份
                            if (vDItemSpaceCount > 0)  // 当前DItem有分到间距
                            {
                                vCountWillSplit++;  // 增加分到间距的DItem数量
                            }
                        }

                        if (vLineSpaceCount > 1)  // 份数大于1
                        {
                            viSplitW = aRemWidth / vLineSpaceCount;  // 每一份的大小
                            vDItemSpaceCount = aRemWidth % vLineSpaceCount;  // 余数，借用变量
                            if (vDItemSpaceCount > vCountWillSplit)  // 余数大于行中参与分的DItem的数量
                            {
                                vExtraW = vDItemSpaceCount / vCountWillSplit;  // 参与分的每一个DItem额外再分的量
                                vModWidth = vDItemSpaceCount % vCountWillSplit;  // 额外分完后剩余(小于行参与分DItem个数)
                            }
                            else  // 余数小于行中参与分的DItem数量
                            {
                                vModWidth = vDItemSpaceCount;
                            }
                        }

                        // 行中第一个DrawItem增加的空间 
                        if (vDrawItemSplitCounts[0] > 0)
                        {
                            FDrawItems[vLineBegDItemNo].Rect.Width += vDrawItemSplitCounts[0] * viSplitW + vExtraW;
                            if (vModWidth > 0)  // 额外的没有分完
                            {
                                FDrawItems[vLineBegDItemNo].Rect.Width++;  // 当前DrawItem多分一个像素
                                vModWidth--;  // 额外的减少一个像素
                            }
                        }

                        for (int i = vLineBegDItemNo + 1; i <= aLineEndDItemNo; i++)  // 以第一个为基准，其余各DrawItem增加的空间
                        {
                            vW = FDrawItems[i].Width();  // DrawItem原来Width
                            if (vDrawItemSplitCounts[i - vLineBegDItemNo] > 0)  // 有分到间距
                            {
                                vDWidth = vDrawItemSplitCounts[i - vLineBegDItemNo] * viSplitW + vExtraW;  // 多分到的width
                                if (vModWidth > 0)  // 额外的没有分完
                                {
                                    if (GetDrawItemStyle(i) < HCStyle.Null)
                                    {
                                        if ((FItems[FDrawItems[i].ItemNo] as HCCustomRectItem).JustifySplit())
                                        {
                                            vDWidth++;  // 当前DrawItem多分一个像素
                                            vModWidth--;  // 额外的减少一个像素
                                        }
                                    }
                                    else
                                    {
                                        vDWidth++;  // 当前DrawItem多分一个像素
                                        vModWidth--;  // 额外的减少一个像素
                                    }
                                }
                            }
                            else  // 没有分到间距
                            {
                                vDWidth = 0;
                            }

                            FDrawItems[i].Rect.Left = FDrawItems[i - 1].Rect.Right;

                            if (GetDrawItemStyle(i) < HCStyle.Null)  // RectItem
                            {
                                if ((FItems[FDrawItems[i].ItemNo] as HCCustomRectItem).JustifySplit())  // 分散对齐占间距
                                {
                                    FDrawItems[i].Rect.Width = vW + vDWidth;
                                }
                                else
                                {
                                    FDrawItems[i].Rect.Width = vW;
                                }
                            }
                            else  // TextItem
                            {
                                FDrawItems[i].Rect.Width = vW + vDWidth;
                            }
                        }
                    }
                    break;
            }
        }
        #endregion

        #region NewDrawItem
        private void NewDrawItem(int aItemNo, int aOffs, int aCharLen, RECT aRect, bool aParaFirst, bool aLineFirst,
            ref int vLastDrawItemNo)
        {
            HCCustomDrawItem vDrawItem = new HCCustomDrawItem();
            vDrawItem.ItemNo = aItemNo;
            vDrawItem.CharOffs = aOffs;
            vDrawItem.CharLen = aCharLen;
            vDrawItem.ParaFirst = aParaFirst;
            vDrawItem.LineFirst = aLineFirst;
            vDrawItem.Rect = aRect;
            vLastDrawItemNo++;
            FDrawItems.Insert(vLastDrawItemNo, vDrawItem);
            if (aOffs == 1)
                FItems[aItemNo].FirstDItemNo = vLastDrawItemNo;    
        }
        #endregion

        #region
        private void DoFormatRectItemToDrawItem(HCCustomRectItem vRectItem, int aItemNo, int aFmtLeft, int aContentWidth, int aFmtRight, int aOffset, 
            bool vParaFirst, ref POINT aPos, ref RECT vRect, ref bool vLineFirst, ref int aLastDrawItemNo, ref int vRemainderWidth)
        {
            vRectItem.FormatToDrawItem(this, aItemNo);
            int vWidth = aFmtRight - aPos.X;
            if ((vRectItem.Width > vWidth) && (!vLineFirst))  // 当前行剩余宽度放不下且不是行首
            {
                // 偏移到下一行
                FinishLine(aItemNo, aLastDrawItemNo, vWidth);
                aPos.X = aFmtLeft;
                aPos.Y = FDrawItems[aLastDrawItemNo].Rect.Bottom;
                vLineFirst = true;  // 作为行首
            }

            // 当前行空余宽度能放下或放不下但已经是行首了
            vRect.Left = aPos.X;
            vRect.Top = aPos.Y;
            vRect.Width = vRectItem.Width;
            vRect.Height = vRectItem.Height + HC.LineSpaceMin;

            NewDrawItem(aItemNo, aOffset, 1, vRect, vParaFirst, vLineFirst, ref aLastDrawItemNo);

            vRemainderWidth = aFmtRight - vRect.Right;  // 放入后的剩余量
        }
        #endregion

        #region
        private void _FormatBreakTextDrawItem(int aItemNo, int aFmtLeft, int aFmtRight, ref int aDrawItemNo,
            ref POINT aPos, ref RECT vRect, ref int vRemainderWidth, ref bool vParaFirst)
        {
            HCCanvas vCanvas = HCStyle.CreateStyleCanvas();
            try
            {
                HCCustomDrawItem vDrawItem = DrawItems[aDrawItemNo];
                HCCustomItem vItem = FItems[vDrawItem.ItemNo];
                int vLen = vItem.Text.Length;
                int vItemHeight = _CalculateLineHeight(vCanvas,
                    FStyle.TextStyles[vItem.StyleNo], FStyle.ParaStyles[vItem.ParaNo].LineSpaceMode);
                int vWidth = vCanvas.TextWidth(vItem.Text[vLen - 1]);
                // 分裂前
                vDrawItem.CharLen = vDrawItem.CharLen - 1;
                vDrawItem.Rect.Right = vDrawItem.Rect.Right - vWidth;
                vRemainderWidth = aFmtRight - vDrawItem.Rect.Right;
                FinishLine(aItemNo, aDrawItemNo, vRemainderWidth);
                // 分裂后
                aPos.X = aFmtLeft;
                aPos.Y = vDrawItem.Rect.Bottom;
                vRect.Left = aPos.X;
                vRect.Top = aPos.Y;
                vRect.Right = vRect.Left + vWidth;
                vRect.Bottom = vRect.Top + vItemHeight;
                NewDrawItem(vDrawItem.ItemNo, vLen - 1, 1, vRect, false, true, ref aDrawItemNo);
                vParaFirst = false;
                aPos.X = vRect.Right;

                vRemainderWidth = aFmtRight - vRect.Right;  // 放入最多后的剩余量
            }
            finally
            {
                HCStyle.DestroyStyleCanvas(vCanvas);
            }
        }
        #endregion

        /// <summary>
        /// 转换指定Item指定Offs格式化为DItem
        /// </summary>
        /// <param name="aItemNo">指定的Item</param>
        /// <param name="aOffset">指定的格式化起始位置</param>
        /// <param name="aContentWidth">当前Data格式化宽度</param>
        /// <param name="APageContenBottom">当前页格式化底部位置</param>
        /// <param name="aPos">起始位置</param>
        /// <param name="ALastDNo">起始DItemNo前一个值</param>
        /// <param name="vPageBoundary">数据页底部边界</param>
        protected void _FormatItemToDrawItems(int aItemNo, int aOffset, int aFmtLeft, int aFmtRight, int aContentWidth,
          ref POINT aPos, ref int aLastDrawItemNo)
        {
            if (!FItems[aItemNo].Visible) return;

            RECT vRect = new RECT();
            int vItemHeight = 0;
            bool vParaFirst, vLineFirst;
            int vRemainderWidth = 0;
            int vLastDrawItemNo = aLastDrawItemNo;
            HCCustomRectItem vRectItem = null;
            HCCustomItem vItem = FItems[aItemNo];
            HCParaStyle vParaStyle = FStyle.ParaStyles[vItem.ParaNo];

            if ((aOffset == 1) && (vItem.ParaFirst))  // 第一次处理段第一个Item
            {
                vParaFirst = true;
                vLineFirst = true;
                aPos.X = aPos.X + vParaStyle.FirstIndent;
            }
            else  // 非段第1个
            {
                vParaFirst = false;
                vLineFirst = (aPos.X == aFmtLeft);
            }

            if (vItem.StyleNo < HCStyle.Null)  // 是RectItem
            {
                vRectItem = vItem as HCCustomRectItem;
                DoFormatRectItemToDrawItem(vRectItem, aItemNo, aFmtLeft, aContentWidth, aFmtRight, aOffset, vParaFirst, 
                    ref aPos, ref vRect, ref vLineFirst, ref aLastDrawItemNo, ref vRemainderWidth);
            }
            else  // 文本
            {
                vItemHeight = _CalculateLineHeight(FStyle.DefCanvas, FStyle.TextStyles[vItem.StyleNo], 
                    FStyle.ParaStyles[vItem.ParaNo].LineSpaceMode);

                //FStyle.TextStyles[vItem.StyleNo].ApplyStyle(FStyle.DefCanvas);
                //vItemHeight = HCStyle.GetFontHeight(FStyle.DefCanvas);  // + vParaStyle.LineSpace;  // 行高

                //TEXTMETRIC vTextMetric = new TEXTMETRIC();
                //FStyle.DefCanvas.GetTextMetrics(ref vTextMetric);
                

                //switch (FStyle.ParaStyles[vItem.ParaNo].LineSpaceMode)
                //{
                //    case ParaLineSpaceMode.pls100: 
                //        vItemHeight = vItemHeight + vTextMetric.tmExternalLeading; // Round(vTextMetric.tmHeight * 0.2);
                //        break;

                //    case ParaLineSpaceMode.pls115: 
                //        vItemHeight = vItemHeight + vTextMetric.tmExternalLeading + (int)Math.Round((vTextMetric.tmHeight + vTextMetric.tmExternalLeading) * 0.15);
                //        break;

                //    case ParaLineSpaceMode.pls150: 
                //        vItemHeight = vItemHeight + vTextMetric.tmExternalLeading + (int)Math.Round((vTextMetric.tmHeight + vTextMetric.tmExternalLeading) * 0.5);
                //        break;

                //    case ParaLineSpaceMode.pls200: 
                //        vItemHeight = vItemHeight + vTextMetric.tmExternalLeading + vTextMetric.tmHeight + vTextMetric.tmExternalLeading;
                //        break;

                //    case ParaLineSpaceMode.plsFix: 
                //        vItemHeight = vItemHeight + HC.LineSpaceMin;
                //        break;
                //}

                vRemainderWidth = aFmtRight - aPos.X;
                string vText = vItem.Text;

                if (vText == "")  // 空item(肯定是空行)
                {
                    //Assert(vItem.ParaFirst, HCS_EXCEPTION_NULLTEXT);
                    vRect.Left = aPos.X;
                    vRect.Top = aPos.Y;
                    vRect.Width = 0;
                    vRect.Height = vItemHeight;  //DefaultCaretHeight;
                    vParaFirst = true;
                    vLineFirst = true;
                    vLastDrawItemNo = aLastDrawItemNo;
                    NewDrawItem(aItemNo, aOffset, 0, vRect, vParaFirst, vLineFirst, ref vLastDrawItemNo);
                    aLastDrawItemNo = vLastDrawItemNo;
                    vParaFirst = false;
                }
                else  // 非空Item
                {
                    int viLen = vText.Length;

                    if (viLen > 65535)
                        throw new Exception(HC.HCS_EXCEPTION_STRINGLENGTHLIMIT);

                    int[] vCharWidths = new int[viLen];
 
                    SIZE vSize = new SIZE();
                    FStyle.DefCanvas.GetTextExtentExPoint(vText, viLen, vCharWidths, ref vSize);  // 超过65535数组元素取不到值

                    DoFormatTextItemToDrawItems(vText, aOffset, aFmtRight - aPos.X, 0, aItemNo, 
                        viLen, vItemHeight, aFmtLeft, aContentWidth, aFmtRight, vCharWidths, 
                        ref vParaFirst, ref vLineFirst, ref aPos, ref vRect, ref vRemainderWidth, ref aLastDrawItemNo);
                }
            }

            // 计算下一个的位置
            if (aItemNo == FItems.Count - 1)
            {
                FinishLine(aItemNo, aLastDrawItemNo, vRemainderWidth);
            }
            else  // 不是最后一个，则为下一个Item准备位置
            {
                if (FItems[aItemNo + 1].ParaFirst)   // 下一个是段起始
                {
                    FinishLine(aItemNo, aLastDrawItemNo, vRemainderWidth);
                    // 偏移到下一行顶端，准备另起一行
                    aPos.X = 0;
                    aPos.Y = FDrawItems[aLastDrawItemNo].Rect.Bottom;  // 不使用 vRect.Bottom 因为如果行中间有高的，会修正其bottom
                }
                else  // 下一个不是段起始
                {
                    aPos.X = vRect.Right;  // 下一个的起始坐标
                }
            }
        }

        /// <summary> 根据指定Item获取其所在段的起始和结束ItemNo </summary>
        /// <param name="AFirstItemNo1">指定</param>
        /// <param name="aFirstItemNo">起始</param>
        /// <param name="aLastItemNo">结束</param>
        protected void GetParaItemRang(int aItemNo, ref int aFirstItemNo, ref int aLastItemNo)
        {
            aFirstItemNo = aItemNo;

            while (aFirstItemNo > 0)
            {
                if (FItems[aFirstItemNo].ParaFirst)
                {
                    break;
                }
                else
                {
                    aFirstItemNo--;
                }
            }

            aLastItemNo = aItemNo + 1;
            while (aLastItemNo < FItems.Count)
            {
                if (FItems[aLastItemNo].ParaFirst)
                {
                    break;
                }
                else
                {
                    aLastItemNo++;
                }
            }

            aLastItemNo--;
        }

        protected int GetParaFirstItemNo(int aItemNo)
        {
            int Result = aItemNo;
            while (Result > 0)
            {
                if (FItems[Result].ParaFirst)
                {
                    break;
                }
                else
                {
                    Result--;
                }
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
                {
                    break;
                }
                else
                {
                    Result++;
                }
            }

            Result--;

            return Result;
        }

        /// <summary> 取行第一个DrawItem对应的ItemNo(用于格式化时计算一个较小的ItemNo范围) </summary>
        protected int GetLineFirstItemNo(int aItemNo, int aOffset)
        {
            int Result = aItemNo;
            int vFirstDItemNo = GetDrawItemNoByOffset(aItemNo, aOffset);

            while (vFirstDItemNo > 0)
            {
                if (DrawItems[vFirstDItemNo].LineFirst)
                {
                    break;
                }
                else
                {
                    vFirstDItemNo--;
                }
            }

            Result = DrawItems[vFirstDItemNo].ItemNo;

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
                {
                    break;
                }
                else
                {
                    vLastDItemNo++;
                }
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
                {
                    break;
                }
                else
                {
                    aFirstDItemNo--;
                }
            }

            aLastDItemNo = aFirstDItemNo + 1;
            while (aLastDItemNo < FDrawItems.Count)
            {
                if (FDrawItems[aLastDItemNo].LineFirst)
                {
                    break;
                }
                else
                {
                    aLastDItemNo++;
                }
            }

            aLastDItemNo--;
        }

        /// <summary> 获取指定DrawItem对应的Text </summary>
        /// <param name="aDrawItemNo"></param>
        /// <returns></returns>
        protected string GetDrawItemText(int aDrawItemNo)
        {
            HCCustomDrawItem vDrawItem = FDrawItems[aDrawItemNo];
            string vText = FItems[vDrawItem.ItemNo].Text;
            if (vText != "")
            {
                vText = vText.Substring(vDrawItem.CharOffs - 1, vDrawItem.CharLen);
            }
            return vText;
        }

        protected void SetCaretDrawItemNo(int value)
        {
            int vItemNo;

            if (FCaretDrawItemNo != value)
            {
                if ((FCaretDrawItemNo >= 0) && (FCaretDrawItemNo < FDrawItems.Count))
                {
                    vItemNo = FDrawItems[FCaretDrawItemNo].ItemNo;
                    FItems[vItemNo].Active = false;
                }
                else
                {
                    vItemNo = -1;
                }

                FCaretDrawItemNo = value;

                if ((FCaretDrawItemNo >= 0) && (FDrawItems[FCaretDrawItemNo].ItemNo != vItemNo))
                {
                    if (FItems[FDrawItems[FCaretDrawItemNo].ItemNo].StyleNo < HCStyle.Null)
                    {
                        if (FSelectInfo.StartItemOffset == HC.OffsetInner)
                        {
                            FItems[FDrawItems[FCaretDrawItemNo].ItemNo].Active = true;
                        }
                    }
                    else
                    {
                        FItems[FDrawItems[FCaretDrawItemNo].ItemNo].Active = true;
                    }
                }
            }
        }

        protected HCUndoList GetUndoList()
        {
            if (FOnGetUndoList != null)
            {
                return FOnGetUndoList();
            }
            else
            {
                return null;
            }
        }

        protected virtual void DoItemAction(int aItemNo, int aOffset, HCItemAction aAction) { }

        protected virtual void DoDrawItemPaintBefor(HCCustomData aData, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        { }

        protected virtual void DoDrawItemPaintContent(HCCustomData aData, int aDrawItemNo, RECT aDrawRect,
            RECT aClearRect, string aDrawText, int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop,
            int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        { }

        protected virtual void DoDrawItemPaintAfter(HCCustomData aData, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        { }

        public HCCustomData(HCStyle aStyle)
        {
            FStyle = aStyle;
            FDrawItems = new HCDrawItems();
            FItems = new HCItems();
            FCaretDrawItemNo = -1;
            FSelectInfo = new SelectInfo();
            FDrawOptions = new HashSet<DrawOption>();
        }

        ~HCCustomData()
        {
                    
        }

        /// <summary>
        /// 当前Data是不是无内容(仅有一个Item且内容为空)
        /// </summary>
        /// <returns></returns>
        public bool IsEmptyData()
        {
            return ((FItems.Count == 1) && (FItems[0].StyleNo > HCStyle.Null) && (FItems[0].Text == ""));
        }

        public virtual void Clear()
        {
            FSelectInfo.Initialize();
            FCaretDrawItemNo = -1;
            FDrawItems.Clear();
            FItems.Clear();
        }

        public virtual void InitializeField()
        {
            FCaretDrawItemNo = -1;
        }

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

            if (FStyle.CurStyleNo < HCStyle.Null)
                vItem.StyleNo = 0;
            else
                vItem.StyleNo = FStyle.CurStyleNo;

            vItem.ParaNo = FStyle.CurParaNo;

            return vItem;
        }

        public virtual HCCustomItem CreateDefaultDomainItem()
        {
            object[] vobj = new object[1];
            vobj[0] = this;
            Type t = HCDomainItem.HCDefaultDomainItemClass;
            HCCustomItem Result = t.GetConstructor(null).Invoke(vobj) as HCCustomItem;
            Result.ParaNo = FStyle.CurParaNo;
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
            aCaretInfo.Height = vDrawItem.Height();  // 光标高度

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
                    FStyle.CurStyleNo = (Items[vStyleItemNo] as HCTextRectItem).TextStyleNo;
                else
                    FStyle.CurStyleNo = Items[vStyleItemNo].StyleNo;

                FStyle.CurParaNo = Items[vStyleItemNo].ParaNo;
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
            }
            else  // TextItem
                aCaretInfo.X = aCaretInfo.X + vDrawItem.Rect.Left + GetDrawItemOffsetWidth(vDrawItemNo, aOffset - vDrawItem.CharOffs + 1);

            aCaretInfo.Y = aCaretInfo.Y + vDrawItem.Rect.Top;
        }

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
                    Result = FDrawItems[aDrawItemNo].Width();
            }
            else
            {
                HCCanvas vCanvas = null;
                if (aStyleCanvas != null)
                    vCanvas = aStyleCanvas;
                else
                {
                    vCanvas = FStyle.DefCanvas;
                    FStyle.TextStyles[vStyleNo].ApplyStyle(vCanvas);
                }

                ParaAlignHorz vAlignHorz = FStyle.ParaStyles[GetDrawItemParaStyle(aDrawItemNo)].AlignHorz;
                switch (vAlignHorz)
                {
                    case ParaAlignHorz.pahLeft:
                    case ParaAlignHorz.pahRight:
                    case ParaAlignHorz.pahCenter:
                        Result = vCanvas.TextWidth(FItems[vDrawItem.ItemNo].Text.Substring(vDrawItem.CharOffs - 1, aDrawOffs));
                        break;

                    case ParaAlignHorz.pahJustify:
                    case ParaAlignHorz.pahScatter:  // 20170220001 两端、分散对齐相关处理
                        if (vAlignHorz == ParaAlignHorz.pahJustify)
                        {
                            if (IsParaLastDrawItem(aDrawItemNo))
                            {
                                Result = vCanvas.TextWidth(FItems[vDrawItem.ItemNo].Text.Substring(vDrawItem.CharOffs - 1, aDrawOffs));
                                return Result;
                                //break;
                            }
                        }

                        string vText = GetDrawItemText(aDrawItemNo);
                        int viSplitW = vDrawItem.Width() - vCanvas.TextWidth(vText);  // 当前DItem的Rect中用于分散的空间
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
                            
                        //vSplitCount := 0;  // 借用变量
                        for (int i = 0; i <= vSplitList.Count - 2; i++)  // vSplitList最后一个是字符串长度所以多减1
                        {
                            string vS = vText.Substring(vSplitList[i] - 1, vSplitList[i + 1] - vSplitList[i]);  // 当前分隔的一个字符串
                            int vCharWidth = vCanvas.TextWidth(vS);
                            if (vMod > 0)
                            {
                                vCharWidth++;  // 多分的余数
                                vSplitCount = 1;
                                vMod--;
                            }
                            else
                                vSplitCount = 0;

                            int vDOffset = vSplitList[i] + vS.Length - 1;
                            if (vDOffset <= aDrawOffs)
                            {
                                // 增加间距
                                if (i != vSplitList.Count - 2)
                                    vCharWidth = vCharWidth + viSplitW;  // 分隔间距
                                else  // 是当前DItem分隔的最后一个
                                {
                                    if (!vLineLast)
                                        vCharWidth = vCharWidth + viSplitW;  // 分隔间距
                                }
                                Result = Result + vCharWidth;

                                if (vDOffset == aDrawOffs)
                                    break;
                            }
                            else  // 当前字符结束位置在AOffs后，找具体位置
                            {
                                // 准备处理  a b c d e fgh ijklm n opq的形式(多个字符为一个分隔串)
                                for (int j = 1; j <= vS.Length; j++)  // 找在当前分隔的这串字符串中哪一个位置
                                {
                                    vCharWidth = vCanvas.TextWidth(vS[j - 1].ToString());
                                    vDOffset = vSplitList[i] - 1 + j;

                                    if (vDOffset == vDrawItem.CharLen)
                                    {
                                        if (!vLineLast)
                                            vCharWidth = vCharWidth + viSplitW + vSplitCount;  // 当前DItem最后一个字符享受分隔间距和多分的余数
                                            //else 行最后一个DItem的最后一个字符不享受分隔间距和多分的余数，因为串格式化时最后一个分隔字符串右侧就不分间距
                                    }
                                    Result = Result + vCharWidth;

                                    if (vDOffset == aDrawOffs)
                                        break;
                                }

                                break;
                            }
                        }
                        break;
                    }
                }

            return Result;
        }

        /// <summary> 获取指定的Item最后面位置 </summary>
        /// <param name="aItemNo">指定的Item</param>
        /// <returns>最后面位置</returns>
        public int GetItemAfterOffset(int aItemNo)
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
                        if (y > FDrawItems[vEndDItemNo].Rect.Bottom)
                            vStartDItemNo = vEndDItemNo;
                        else
                        if (y >= FDrawItems[vEndDItemNo].Rect.Top)
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
                    aOffset = HC.OffsetBefor;  // GetDrawItemOffsetAt(vStartDItemNo, X)
                else
                    aOffset = FDrawItems[vStartDItemNo].CharOffs - 1;  // DrawItem起始
            }
            else
            if (x >= FDrawItems[vEndDItemNo].Rect.Right)
            {
                aDrawItemNo = vEndDItemNo;
                aItemNo = FDrawItems[vEndDItemNo].ItemNo;
                if (FItems[aItemNo].StyleNo < HCStyle.Null)
                    aOffset = HC.OffsetAfter;  // GetDrawItemOffsetAt(vEndDItemNo, X)
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
                        {
                            if (ARestrain)
                            {
                                if (x < vDrawRect.Left + vDrawRect.Width / 2)
                                    aOffset = HC.OffsetBefor;
                                else
                                    aOffset = HC.OffsetAfter;
                            }
                            else
                                aOffset = GetDrawItemOffsetAt(i, x);
                        }
                        else  // TextItem
                            aOffset = FDrawItems[i].CharOffs + GetDrawItemOffsetAt(i, x) - 1;

                        break;
                    }
                }
            }
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
            int Result = -1;

            if (FItems[aItemNo].StyleNo < HCStyle.Null)
                Result = FItems[aItemNo].FirstDItemNo;
            else  // TextItem
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

        public int GetCurDrawItemNo()
        {
            int vItemNo, Result = -1;

            if (SelectInfo.StartItemNo < 0)
            {

            }
            else
            {
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

                        if (SelectInfo.StartItemOffset - vDrawItem.CharOffs + 1 <= vDrawItem.CharLen)
                        {
                            Result = i;
                            break;
                        }
                    }
                }
            }

            return Result;
        }

        public HCCustomDrawItem GetCurDrawItem()
        {
            int vCurDItemNo = GetCurDrawItemNo();
            if (vCurDItemNo < 0)
                return null;
            else
                return FDrawItems[vCurDItemNo];
        }

        public int GetCurItemNo()
        {
            return FSelectInfo.StartItemNo;
        }

        public HCCustomItem GetCurItem()
        {
            int vItemNo = GetCurItemNo();
            if (vItemNo < 0)
                return null;
            else
                return FItems[vItemNo];
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
                string vText = (vItem as HCTextItem).GetTextPart(vDrawItem.CharOffs, vDrawItem.CharLen);
                FStyle.TextStyles[vItem.StyleNo].ApplyStyle(FStyle.DefCanvas);
                HCParaStyle vParaStyle = FStyle.ParaStyles[vItem.ParaNo];
                int vX = vDrawItem.Rect.Left;

                switch (vParaStyle.AlignHorz)
                {
                    case ParaAlignHorz.pahLeft:
                    case ParaAlignHorz.pahRight:
                    case ParaAlignHorz.pahCenter:
                        Result = HC.GetCharOffsetAt(FStyle.DefCanvas, vText, x - vX);
                        break;

                    case ParaAlignHorz.pahJustify:
                    case ParaAlignHorz.pahScatter:  // 20170220001 两端、分散对齐相关处理
                        if (vParaStyle.AlignHorz == ParaAlignHorz.pahJustify)
                        {
                            if (IsParaLastDrawItem(aDrawItemNo))
                            {
                                Result = HC.GetCharOffsetAt(FStyle.DefCanvas, vText, x - vX);
                                return Result;
                            }
                        }
                        int vMod = 0;
                        int viSplitW = vDrawItem.Width() - FStyle.DefCanvas.TextWidth(vText);  // 当前DItem的Rect中用于分散的空间
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

                        //vSplitCount := 0;
                        for (int i = 0; i <= vSplitList.Count - 2; i++)  // vSplitList最后一个是字符串长度所以多减1
                        {
                            string vS = vText.Substring(vSplitList[i] - 1, vSplitList[i + 1] - vSplitList[i]);  // 当前分隔的一个字符串
                            int vCharWidth = FStyle.DefCanvas.TextWidth(vS);

                            if (vMod > 0)
                            {
                                vCharWidth++;  // 多分的余数
                                vSplitCount = 1;
                                vMod--;
                            }
                            else
                                vSplitCount = 0;

                            // 增加间距
                            if (i != vSplitList.Count - 2)
                                vCharWidth = vCharWidth + viSplitW;  // 分隔间距
                            else  // 是当前DItem分隔的最后一个
                            {
                                if (!vLineLast)
                                    vCharWidth = vCharWidth + viSplitW;  // 分隔间距
                            }

                            if (vX + vCharWidth > x)
                            {
                                vMod = vS.Length;  // 借用变量，准备处理  a b c d e fgh ijklm n opq的形式(多个字符为一个分隔串)

                                for (int j = 1; j <= vMod; j++)  // 找在当前分隔的一个字符串中哪一个位置
                                {
                                    vCharWidth = FStyle.DefCanvas.TextWidth(vS[j - 1]);
                                    if (i != vSplitList.Count - 2)
                                    {
                                        if (j == vMod)
                                            vCharWidth = vCharWidth + viSplitW + vSplitCount;
                                    }
                                    else  // 是当前DItem分隔的最后一个
                                    {
                                        if (!vLineLast)
                                            vCharWidth = vCharWidth + viSplitW + vSplitCount;  // 分隔间距

                                    }

                                    vX = vX + vCharWidth;
                                    if (vX > x)
                                    {
                                        if (vX - vCharWidth / 2 > x)
                                            Result = vSplitList[i] - 1 + j - 1;  // 计为前一个后面
                                        else
                                            Result = vSplitList[i] - 1 + j;

                                        break;
                                    }
                                }
                                break;
                            }

                            vX = vX + vCharWidth;
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

                if ((FSelectInfo.EndItemNo >= 0) && (Result < FItems.Count - 1) && (FDrawItems[Result].CharOffsetEnd() == FSelectInfo.StartItemOffset))
                    Result++;
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
                Result = GetDrawItemNoByOffset(FSelectInfo.EndItemNo, FSelectInfo.EndItemOffset);

            return Result;
        }

        /// <summary> 获取选中内容是否在同一个DItem中 </summary>
        /// <returns></returns>
        public bool SelectInSameDItem()
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
                vItem.Active = false;
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
            if ((FSelectInfo.StartItemNo >= 0) && (FSelectInfo.EndItemNo < 0) && (FItems[FSelectInfo.StartItemNo] is HCResizeRectItem))
                return (FItems[FSelectInfo.StartItemNo] as HCResizeRectItem).Resizing;
            else
                return false;
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
                    FSelectInfo.EndItemOffset = GetItemAfterOffset(FSelectInfo.EndItemNo);
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
                    && (FSelectInfo.EndItemOffset == GetItemAfterOffset(FItems.Count - 1)));
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

        public virtual void ApplyParaLineSpace(ParaLineSpaceMode aSpaceMode)
        {
            ParaLineSpaceMatch vMatchStyle = new ParaLineSpaceMatch();
            vMatchStyle.SpaceMode = aSpaceMode;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaLeftIndent(bool add)
        {
            ParaLeftIndentMatch vMatchStyle = new ParaLeftIndentMatch();
            vMatchStyle.Append = add;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaRightIndent(bool add)
        {
            ParaRightIndentMatch vMatchStyle = new ParaRightIndentMatch();
            vMatchStyle.Append = add;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaFirstIndent(bool add)
        {
            ParaFirstIndentMatch vMatchStyle = new ParaFirstIndentMatch();
            vMatchStyle.Append = add;
            ApplySelectParaStyle(vMatchStyle);
        }

        // 选中内容应用样式
        public virtual int ApplySelectTextStyle(HCStyleMatch aMatchStyle)
        {
            return -1;
        }

        public virtual int ApplySelectParaStyle(HCParaMatch aMatchStyle)
        {
            return -1;
        }

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
                    && (SelectInfo.StartItemOffset == FDrawItems[vSelStartDItemNo].CharOffs)
                )
            )
            &&
            (
                (vSelEndDItemNo > aLastDItemNo)
                ||
                (
                    (vSelEndDItemNo == aLastDItemNo)
                    && (SelectInfo.EndItemOffset == FDrawItems[vSelEndDItemNo].CharOffs + FDrawItems[vSelEndDItemNo].CharLen)
                )
            );
    }
#endregion

#region DrawTextJsutify 20170220001 分散对齐相关处理
        private void DrawTextJsutify(HCCanvas aCanvas, RECT aRect, string aText, bool aLineLast, int vTextDrawTop)
        {         
            int vMod = 0;
            int vX = aRect.Left;
            int viSplitW = (aRect.Right - aRect.Left) - FStyle.DefCanvas.TextWidth(aText);
            // 计算当前Ditem内容分成几份，每一份在内容中的起始位置
            List<int> vSplitList = new List<int>();
            int vSplitCount = GetJustifyCount(aText, vSplitList);
            if (aLineLast && (vSplitCount > 0))  // 行最后DItem，少分一个
                vSplitCount--;
              if (vSplitCount > 0)  // 有分到间距
              {
                  vMod = viSplitW % vSplitCount;
                  viSplitW = viSplitW / vSplitCount;
              }

              for (int i = 0; i <= vSplitList.Count - 2; i++)  // vSplitList最后一个是字符串长度所以多减1
              {
                  int vLen = vSplitList[i + 1] - vSplitList[i];
                  string vS = aText.Substring(vSplitList[i] - 1, vLen);

                  GDI.ExtTextOut(aCanvas.Handle, vX, vTextDrawTop, GDI.ETO_OPAQUE, IntPtr.Zero, vS, vLen, IntPtr.Zero);
                  vX = vX + FStyle.DefCanvas.TextWidth(vS) + viSplitW;
                  if (vMod > 0)
                  {
                      vX++;
                      vMod--;
                  }
              }
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
        public virtual void PaintData(int aDataDrawLeft, int aDataDrawTop, int aDataDrawBottom,
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
                    DrawItemPaintBefor(this, i, vDrawRect, aDataDrawLeft, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);

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
                                vClearRect.Inflate(-(vClearRect.Width - vRectItem.Width) / 2, 0);
                            else
                                vClearRect.Width = vClearRect.Width;
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

                        DrawItemPaintContent(this, i, vDrawRect, vClearRect, "", aDataDrawLeft, aDataDrawBottom, 
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
                            FStyle.TextStyles[vPrioStyleNo].ApplyStyle(FStyle.DefCanvas);//, APaintInfo.ScaleY / APaintInfo.Zoom);

                            vTextHeight = HCStyle.GetFontHeight(FStyle.DefCanvas);
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
                        if (FStyle.TextStyles[vPrioStyleNo].BackColor.A != 0)
                        {
                            aCanvas.Brush.Color = FStyle.TextStyles[vPrioStyleNo].BackColor;
                            aCanvas.FillRect(new RECT(vClearRect.Left, vClearRect.Top, vClearRect.Left + vDrawItem.Width(), vClearRect.Bottom));
                        }

                        string vText = vItem.Text.Substring(vDrawItem.CharOffs - 1, vDrawItem.CharLen);  // 为减少判断，没有直接使用GetDrawItemText(i)
                        DrawItemPaintContent(this, i, vDrawRect, vClearRect, vText, aDataDrawLeft,
                            aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);

                        // 绘制优先级更高的选中情况下的背景
                        if (!aPaintInfo.Print)
                        {
                            if (vDrawsSelectAll)
                            {
                                aCanvas.Brush.Color = FStyle.SelColor;
                                aCanvas.FillRect(new RECT(vDrawRect.Left, vDrawRect.Top, vDrawRect.Left + vDrawItem.Width(), Math.Min(vDrawRect.Bottom, aDataScreenBottom)));
                            }
                            else  // 处理一部分选中
                                if (vSelEndDNo >= 0)  // 有选中内容，部分背景为选中
                                {
                                    aCanvas.Brush.Color = FStyle.SelColor;
                                    if ((vSelStartDNo == vSelEndDNo) && (i == vSelStartDNo))  // 选中内容都在当前DrawItem
                                    {
                                        aCanvas.FillRect(new RECT(vDrawRect.Left + GetDrawItemOffsetWidth(i, vSelStartDOffs),
                                            vDrawRect.Top, vDrawRect.Left + GetDrawItemOffsetWidth(i, vSelEndDOffs, FStyle.DefCanvas),
                                            Math.Min(vDrawRect.Bottom, aDataScreenBottom)));
                                    }
                                    else
                                        if (i == vSelStartDNo)  // 选中在不同DrawItem，当前是起始
                                        {
                                            aCanvas.FillRect(new RECT(vDrawRect.Left + GetDrawItemOffsetWidth(i, vSelStartDOffs, FStyle.DefCanvas),
                                            vDrawRect.Top, vDrawRect.Right, Math.Min(vDrawRect.Bottom, aDataScreenBottom)));
                                        }
                                        else
                                            if (i == vSelEndDNo)  // 选中在不同的DrawItem，当前是结束
                                            {
                                                aCanvas.FillRect(new RECT(vDrawRect.Left,
                                                vDrawRect.Top, vDrawRect.Left + GetDrawItemOffsetWidth(i, vSelEndDOffs, FStyle.DefCanvas),
                                                Math.Min(vDrawRect.Bottom, aDataScreenBottom)));
                                            }
                                            else
                                                if ((i > vSelStartDNo) && (i < vSelEndDNo))  // 选中起始和结束DrawItem之间的DrawItem
                                                    aCanvas.FillRect(vDrawRect);
                                }
                        }

                        vItem.PaintTo(FStyle, vDrawRect, aDataDrawTop, aDataDrawBottom,
                            aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);  // 触发Item绘制事件

                        // 绘制文本                      
                        if (vText != "")
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
                        else  // 空行
                        {
                            if (!vItem.ParaFirst)
                                throw new Exception(HC.HCS_EXCEPTION_NULLTEXT);
                        }
                    }

                    DrawItemPaintAfter(this, i, vClearRect, aDataDrawLeft, aDataDrawBottom,
                        aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);  // 绘制内容后
                }
            }
            finally
            {
                aCanvas.Restore(vDCState);
            }
        }

        public virtual void PaintData(int aDataDrawLeft, int aDataDrawTop, int aDataDrawBottom,
            int aDataScreenTop, int aDataScreenBottom, int aVOffset, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (FItems.Count == 0)
                return;

            int vFirstDItemNo = -1, vLastDItemNo = -1;
            int vVOffset = aDataDrawTop - aVOffset;  // 将数据起始位置映射到绘制位置
            GetDataDrawItemRang(Math.Max(aDataDrawTop, aDataScreenTop) - vVOffset,  // 可显示出来的DrawItem范围
                Math.Min(aDataDrawBottom, aDataScreenBottom) - vVOffset, ref vFirstDItemNo, ref vLastDItemNo);

            PaintData(aDataDrawLeft, aDataDrawTop, aDataDrawBottom, aDataScreenTop,
              aDataScreenBottom, aVOffset, vFirstDItemNo, vLastDItemNo, aCanvas, aPaintInfo);
        }

        /// <summary> 根据行中某DrawItem获取当前行间距(行中除文本外的空白空间) </summary>
        /// <param name="aDrawNo">行中指定的DrawItem</param>
        /// <returns>行间距</returns>
        public int GetLineBlankSpace(int aDrawNo)
        {
            int vStyleNo = HCStyle.Null;
            int vFirst = aDrawNo;
            int vLast = -1;
            GetLineDrawItemRang(ref vFirst, ref vLast);

            // 找行中最高的DrawItem
            int vMaxHi = 0;
            int vMaxDrawItemNo;

            using (HCCanvas vCanvas = HCStyle.CreateStyleCanvas())
            {
                vMaxDrawItemNo = vFirst;
                for (int i = vFirst; i <= vLast; i++)
                {
                    int vHi;
                    if (GetDrawItemStyle(i) < HCStyle.Null)
                        vHi = (FItems[FDrawItems[i].ItemNo] as HCCustomRectItem).Height;
                    else
                    {
                        if (FItems[FDrawItems[i].ItemNo].StyleNo != vStyleNo)
                        {
                            vStyleNo = FItems[FDrawItems[i].ItemNo].StyleNo;
                            FStyle.TextStyles[vStyleNo].ApplyStyle(vCanvas);  // APaintInfo.ScaleY / APaintInfo.Zoom);
                        }
                        
                        vHi = HCStyle.GetFontHeight(vCanvas);
                    }

                    if (vHi > vMaxHi)
                    {
                        vMaxHi = vHi;  // 记下最大的高度
                        vMaxDrawItemNo = i;  // 记下最高的DrawItemNo
                    }
                }
            }

            if (GetDrawItemStyle(vMaxDrawItemNo) < HCStyle.Null)
                return HC.LineSpaceMin;
            else
                return GetDrawItemLineSpace(vMaxDrawItemNo) - vMaxHi;  // 根据最高的DrawItem取行间距
        }

        /// <summary> 获取指定DrawItem的行间距 </summary>
        /// <param name="aDrawNo">指定的DrawItem</param>
        /// <returns>DrawItem的行间距</returns>
        public int GetDrawItemLineSpace(int aDrawNo)
        {
            int Result = HC.LineSpaceMin;

            if (GetDrawItemStyle(aDrawNo) >= HCStyle.Null)
            {
                HCCanvas vCanvas = HCStyle.CreateStyleCanvas();

                try
                {
                    Result = _CalculateLineHeight(vCanvas, FStyle.TextStyles[GetDrawItemStyle(aDrawNo)],
                        FStyle.ParaStyles[GetDrawItemParaStyle(aDrawNo)].LineSpaceMode);
                    //FStyle.TextStyles[GetDrawItemStyle(aDrawNo)].ApplyStyle(vCanvas);
                    //TEXTMETRIC vTextMetric = new TEXTMETRIC();
                    //Win32.GDI.GetTextMetrics(vCanvas.Handle, ref vTextMetric);  // 得到字体信息

                    //switch (FStyle.ParaStyles[GetDrawItemParaStyle(aDrawNo)].LineSpaceMode)
                    //{
                    //    case ParaLineSpaceMode.pls100:
                    //        Result = vTextMetric.tmExternalLeading; // Round(vTextMetric.tmHeight * 0.2);
                    //        break;

                    //    case ParaLineSpaceMode.pls115:
                    //        Result = vTextMetric.tmExternalLeading + (int)Math.Round((vTextMetric.tmHeight + vTextMetric.tmExternalLeading) * 0.15);
                    //        break;

                    //    case ParaLineSpaceMode.pls150:
                    //        Result = vTextMetric.tmExternalLeading + (int)Math.Round((vTextMetric.tmHeight + vTextMetric.tmExternalLeading) * 0.5);
                    //        break;

                    //    case ParaLineSpaceMode.pls200:
                    //        Result = vTextMetric.tmExternalLeading + vTextMetric.tmHeight + vTextMetric.tmExternalLeading;
                    //        break;

                    //    case ParaLineSpaceMode.plsFix:
                    //        Result = HC.LineSpaceMin;
                    //        break;
                    //}
                }
                finally
                {
                    HCStyle.DestroyStyleCanvas(vCanvas);
                }
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

        public virtual void SaveToStream(Stream aStream)
        {
            SaveToStream(aStream, 0, 0, FItems.Count - 1, Items[FItems.Count - 1].Length);
        }

        public virtual void SaveToStream(Stream aStream, int aStartItemNo, int aStartOffset,
            int aEndItemNo, int aEndOffset)
        {
            Int64 vBegPos = aStream.Position;
            byte[] vBuffer = System.BitConverter.GetBytes(vBegPos);
            aStream.Write(vBuffer, 0, vBuffer.Length);  // 数据大小占位，便于越过

            int vi = aEndItemNo - aStartItemNo + 1;
            vBuffer = System.BitConverter.GetBytes(vi);
            aStream.Write(vBuffer, 0, vBuffer.Length);  // 数量

            if (vi > 0)
            {
                if (aStartItemNo != aEndItemNo)
                {
                    FItems[aStartItemNo].SaveToStream(aStream, aStartOffset, FItems[aStartItemNo].Length);
                    for (int i = aStartItemNo + 1; i <= aEndItemNo - 1; i++)
                        FItems[i].SaveToStream(aStream);

                    FItems[aEndItemNo].SaveToStream(aStream, 0, aEndOffset);

                }
                else
                    FItems[aStartItemNo].SaveToStream(aStream, aStartOffset, aEndOffset);

            }
            //
            Int64 vEndPos = aStream.Position;

            aStream.Position = vBegPos;

            vBegPos = vEndPos - vBegPos - Marshal.SizeOf(vBegPos);
            vBuffer = System.BitConverter.GetBytes(vBegPos);
            aStream.Write(vBuffer, 0, vBuffer.Length);  // 当前页数据大小

            aStream.Position = vEndPos;
        }

        public string SaveToText()
        {
            return SaveToText(0, 0, FItems.Count - 1, FItems[FItems.Count - 1].Length);
        }

        public string SaveToText(int aStartItemNo, int aStartOffset, int aEndItemNo, int aEndOffset)
        {
            string Result = "";

            int vi = aEndItemNo - aStartItemNo + 1;

            if (vi > 0)
            {
                if (aStartItemNo != aEndItemNo)
                {
                    if (FItems[aStartItemNo].StyleNo > HCStyle.Null)
                        Result = (FItems[aStartItemNo] as HCTextItem).GetTextPart(aStartOffset + 1, FItems[aStartItemNo].Length - aStartOffset);
                    else
                        Result = (FItems[aStartItemNo] as HCCustomRectItem).SaveSelectToText();

                    for (int i = aStartItemNo + 1; i <= aEndItemNo - 1; i++)
                        Result = Result + FItems[i].Text;

                    if (FItems[aEndItemNo].StyleNo > HCStyle.Null)
                        Result = Result + (FItems[aEndItemNo] as HCTextItem).GetTextPart(1, aEndOffset);
                    else
                        Result = (FItems[aEndItemNo] as HCCustomRectItem).SaveSelectToText();
                }
                else  // 选中在同一Item
                {
                    if (FItems[aStartItemNo].StyleNo > HCStyle.Null)
                        Result = (FItems[aStartItemNo] as HCTextItem).GetTextPart(aStartOffset + 1, aEndOffset - aStartOffset);
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

        public virtual bool InsertStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            return false;
        }

        public virtual void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            Clear();
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
            aNode.Attributes["itemcount"].Value = FItems.Count.ToString();
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
                FItems.RemoveAt(0);
        }

        public HCStyle Style
        {
            get { return FStyle; }
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
    }
}
