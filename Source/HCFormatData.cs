/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2019-3-11             }
{                                                       }
{            支持格式化的文档对象管理单元               }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;

namespace HC.View
{
    public class HCFormatData : HCCustomData
    {
        private int FWidth;
        private int FFormatCount,
            /// <summary> Item含行高的高度 </summary>
            FItemFormatHeight,
            /// <summary> 格式化的起始DrawItem </summary>
            FFormatStartDrawItemNo,
            FFormatStartTop,
            FFormatEndBottom,
            FLastFormatParaNo;
        /// <summary> 当次格式化DrawItem数量是否发生变化 </summary>
        private bool FFormatDrawItemCountChange;
        /// <summary> 当次格式化Data高度是否发生变化 </summary>
        private bool FFormatHeightChange;
        /// <summary> 多次格式化是否有变动，外部由此决定是否重新计算分页起始结束DrawItemNo </summary>
        private bool FFormatChange;

        private void FormatRange(int aStartDrawItemNo, int aLastItemNo)
        {
            int vPrioDrawItemNo = -1, vStartItemNo = -1, vStartOffset = -1;
            HCParaStyle vParaStyle = null;
            POINT vPos = new POINT();

            FFormatStartDrawItemNo = aStartDrawItemNo;

            // 获取起始DrawItem的上一个序号及格式化开始位置
            if (aStartDrawItemNo > 0)
            {
                vPrioDrawItemNo = aStartDrawItemNo - 1;  // 上一个最后的DrawItem
                vStartItemNo = DrawItems[aStartDrawItemNo].ItemNo;
                vStartOffset = DrawItems[aStartDrawItemNo].CharOffs;
                vParaStyle = Style.ParaStyles[Items[vStartItemNo].ParaNo];
                if (DrawItems[aStartDrawItemNo].LineFirst)
                {
                    vPos.X = vParaStyle.LeftIndentPix;
                    vPos.Y = DrawItems[vPrioDrawItemNo].Rect.Bottom;
                }
                else
                {
                    vPos.X = DrawItems[vPrioDrawItemNo].Rect.Right;
                    vPos.Y = DrawItems[vPrioDrawItemNo].Rect.Top;
                }
            }
            else  // 是第一个
            {
                vPrioDrawItemNo = -1;
                vStartItemNo = 0;
                vStartOffset = 1;
                vParaStyle = Style.ParaStyles[Items[vStartItemNo].ParaNo];
                vPos.X = vParaStyle.LeftIndentPix;
                vPos.Y = 0;
            }

            Style.ApplyTempStyle(HCStyle.Null);
            FormatItemToDrawItems(vStartItemNo, vStartOffset, vParaStyle.LeftIndentPix,
                FWidth - vParaStyle.RightIndentPix, FWidth, ref vPos, ref vPrioDrawItemNo);

            for (int i = vStartItemNo + 1; i <= aLastItemNo; i++)  // 格式
            {
                if (Items[i].ParaFirst)
                {
                    vParaStyle = Style.ParaStyles[Items[i].ParaNo];
                    vPos.X = vParaStyle.LeftIndentPix;
                }

                FormatItemToDrawItems(i, 1, vParaStyle.LeftIndentPix,
                    FWidth - vParaStyle.RightIndentPix, FWidth, ref vPos, ref vPrioDrawItemNo);
            }

            DrawItems.DeleteFormatMark();
        }

        private void CalcItemFormatHeigh(HCCustomItem AItem)
        {
            if (Style.TempStyleNo != AItem.StyleNo)
            {
                Style.ApplyTempStyle(AItem.StyleNo);
                FLastFormatParaNo = AItem.ParaNo;
                FItemFormatHeight = CalculateLineHeight(Style.TempCanvas,
                    Style.TextStyles[AItem.StyleNo], Style.ParaStyles[AItem.ParaNo].LineSpaceMode);
            }
            else
            if (FLastFormatParaNo != AItem.ParaNo)
            {
                FLastFormatParaNo = AItem.ParaNo;
                FItemFormatHeight = CalculateLineHeight(Style.TempCanvas,
                    Style.TextStyles[AItem.StyleNo], Style.ParaStyles[AItem.ParaNo].LineSpaceMode);
            }
        }

        #region FormatItemToDrawItems子方法
        /// <summary> 重整行 </summary>
        /// <param name="AEndDItemNo">行最后一个DItem</param>
        /// <param name="aRemWidth">行剩余宽度</param>
        private void FinishLine(int aItemNo, int aLineEndDItemNo, int aRemWidth)
        {
            int vLineBegDItemNo,  // 行第一个DItem
                vMaxBottom,
                viSplitW, vW,
                vLineSpaceCount,   // 当前行分几份
                vDItemSpaceCount,  // 当前DrawItem分几份
                vExtraW,
                viSplitMod,
                vDLen; 

            // 根据行中最高的DrawItem处理其他DrawItem的高度 
            vLineBegDItemNo = aLineEndDItemNo;
            for (int i = aLineEndDItemNo; i >= 0; i--)  // 得到行起始DItemNo
            {
                if (DrawItems[i].LineFirst)
                {
                    vLineBegDItemNo = i;
                    break;
                }
            }
            //Assert((vLineBegDItemNo >= 0), '断言失败：行起始DItemNo小于0！');
            // 找行DrawItem中最高的
            vMaxBottom = DrawItems[aLineEndDItemNo].Rect.Bottom;  // 先默认行最后一个DItem的Rect底位置最大
            for (int i = aLineEndDItemNo - 1; i >= vLineBegDItemNo; i--)
            {
                if (DrawItems[i].Rect.Bottom > vMaxBottom)
                    vMaxBottom = DrawItems[i].Rect.Bottom;  // 记下最大的Rect底位置
            }

            // 根据最高的处理行间距，并影响到同行DrawItem
            for (int i = aLineEndDItemNo; i >= vLineBegDItemNo; i--)
                DrawItems[i].Rect.Bottom = vMaxBottom;

            // 处理对齐方式，放在这里，是因为方便计算行起始和结束DItem，避免绘制时的运算
            HCParaStyle vParaStyle = Style.ParaStyles[Items[aItemNo].ParaNo];
            switch (vParaStyle.AlignHorz)  // 段内容水平对齐方式
            {
                case ParaAlignHorz.pahLeft:  // 默认
                    break;

                case ParaAlignHorz.pahRight:
                    {
                        for (int i = aLineEndDItemNo; i >= vLineBegDItemNo; i--)
                            DrawItems[i].Rect.Offset(aRemWidth, 0);
                    }
                    break;

                case ParaAlignHorz.pahCenter:
                    {
                        viSplitW = aRemWidth / 2;
                        for (int i = aLineEndDItemNo; i >= vLineBegDItemNo; i--)
                            DrawItems[i].Rect.Offset(viSplitW, 0);
                    }
                    break;

                case ParaAlignHorz.pahJustify:  // 20170220001 两端、分散对齐相关处理
                case ParaAlignHorz.pahScatter:
                    {
                        if (vParaStyle.AlignHorz == ParaAlignHorz.pahJustify)  // 两端对齐
                        {
                            if (IsParaLastDrawItem(aLineEndDItemNo))  // 两端对齐，段最后一行不处理
                                return;
                        }
                        else  // 分散对齐，空行或只有一个字符时居中
                        {
                            if (vLineBegDItemNo == aLineEndDItemNo)  // 行只有一个DrawItem
                            {
                                if (Items[DrawItems[vLineBegDItemNo].ItemNo].Length < 2)  // 此DrawItem对应的内容长度不足2个按居中处理
                                {
                                    viSplitW = aRemWidth / 2;
                                    DrawItems[vLineBegDItemNo].Rect.Offset(viSplitW, 0);
                                    return;
                                }
                            }
                        }

                        vLineSpaceCount = 0;
                        vExtraW = 0;
                        viSplitMod = 0;
                        viSplitW = aRemWidth;
                        ushort[] vDrawItemSplitCounts = new ushort[aLineEndDItemNo - vLineBegDItemNo + 1];  // 当前行各DItem分几份

                        for (int i = vLineBegDItemNo; i <= aLineEndDItemNo; i++)  // 计算空余分成几份
                        {
                            if (GetDrawItemStyle(i) < HCStyle.Null)  // RectItem
                            {
                                if ((Items[DrawItems[i].ItemNo] as HCCustomRectItem).JustifySplit()
                                    && (vLineBegDItemNo != aLineEndDItemNo))
                                {  // 分散对齐占间距
                                    if (i != aLineEndDItemNo)
                                        vDItemSpaceCount = 1;  // Graphic等占间距
                                    else
                                        vDItemSpaceCount = 0;
                                }
                                else
                                    vDItemSpaceCount = 0; // Tab等不占间距
                            }
                            else  // TextItem
                            {
                                vDItemSpaceCount = GetJustifyCount(GetDrawItemText(i), null);  // 当前DItem分了几份
                                if ((i == aLineEndDItemNo) && (vDItemSpaceCount > 0))  // 行尾的DItem，少分一个
                                    vDItemSpaceCount--;
                            }

                            vDrawItemSplitCounts[i - vLineBegDItemNo] = (ushort)vDItemSpaceCount;  // 记录当前DItem分几份
                            vLineSpaceCount = vLineSpaceCount + vDItemSpaceCount;  // 记录行内总共分几份
                        }

                        if (vLineSpaceCount > 1)  // 份数大于1
                        {
                            viSplitW = aRemWidth / vLineSpaceCount;  // 每一份的大小
                            viSplitMod = aRemWidth % vLineSpaceCount;  // 余数
                        }

                        // 行中第一个DrawItem增加的空间 
                        if (vDrawItemSplitCounts[0] > 0)
                        {
                            DrawItems[vLineBegDItemNo].Rect.Width += vDrawItemSplitCounts[0] * viSplitW;

                            if (viSplitMod > 0)  // 额外的没有分完
                            {
                                vDLen = DrawItems[vLineBegDItemNo].CharLen;
                                if (viSplitMod > vDLen)  // 足够分
                                {
                                    DrawItems[vLineBegDItemNo].Rect.Right += vDLen;
                                    viSplitMod = viSplitMod - vDLen;
                                }
                                else // 不够分
                                {
                                    DrawItems[vLineBegDItemNo].Rect.Right += viSplitMod;
                                    viSplitMod = 0;
                                }
                            }
                        }

                        for (int i = vLineBegDItemNo + 1; i <= aLineEndDItemNo; i++)  // 以第一个为基准，其余各DrawItem增加的空间
                        {
                            vW = DrawItems[i].Width;  // DrawItem原来Width
                            if (vDrawItemSplitCounts[i - vLineBegDItemNo] > 0)  // 有分到间距
                            {
                                vExtraW = vDrawItemSplitCounts[i - vLineBegDItemNo] * viSplitW;  // 多分到的width
                                if (viSplitMod > 0)  // 额外的没有分完
                                {
                                    if (GetDrawItemStyle(i) < HCStyle.Null)
                                    {
                                        if ((Items[DrawItems[i].ItemNo] as HCCustomRectItem).JustifySplit())
                                        {
                                            vExtraW++;  // 当前DrawItem多分一个像素
                                            viSplitMod--;  // 额外的减少一个像素
                                        }
                                    }
                                    else
                                    {
                                        vDLen = DrawItems[i].CharLen;
                                        if (viSplitMod > vDLen)
                                        {
                                            vExtraW = vExtraW + vDLen;
                                            viSplitMod = viSplitMod - vDLen;
                                        }
                                        else
                                        {
                                            vExtraW = vExtraW + viSplitMod;
                                            viSplitMod = 0;
                                        }
                                    }
                                }
                            }
                            else  // 没有分到间距
                                vExtraW = 0;

                            DrawItems[i].Rect.Left = DrawItems[i - 1].Rect.Right;
                            DrawItems[i].Rect.Right = DrawItems[i].Rect.Left + vW + vExtraW;
                        }
                    }
                    break;
            }
        }

        private void NewDrawItem(int aItemNo, int aCharOffs, int aCharLen, RECT aRect, bool aParaFirst, bool aLineFirst,
            ref int vLastDrawItemNo)
        {
            HCCustomDrawItem vDrawItem = new HCCustomDrawItem();
            vDrawItem.ItemNo = aItemNo;
            vDrawItem.CharOffs = aCharOffs;
            vDrawItem.CharLen = aCharLen;
            vDrawItem.ParaFirst = aParaFirst;
            vDrawItem.LineFirst = aLineFirst;
            vDrawItem.Rect = aRect;
            vLastDrawItemNo++;
            DrawItems.Insert(vLastDrawItemNo, vDrawItem);
            if (aCharOffs == 1)
                Items[aItemNo].FirstDItemNo = vLastDrawItemNo;
        }

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
                vChar = aText[aPos - 1];  // 当前位置字符
                if (HC.PosCharHC(vChar, HC.DontLineLastChar) > 0)  // 是不能放在行尾的字符
                {
                    aPos--;  // 再往前寻找截断位置
                    GetHeadTailBreak(aText, ref aPos);
                }
            }
        }

        private BreakPosition MatchBreak(CharType aPrevType, CharType aPosType, string aText, int aIndex)
        {
            switch (aPosType)
            {
                case CharType.jctHZ:
                    {
                        if ((aPrevType == CharType.jctZM)
                            || (aPrevType == CharType.jctSZ)
                            || (aPrevType == CharType.jctHZ)
                            || (aPrevType == CharType.jctFH))  // 当前位置是汉字，前一个是字母、数字、汉字
                        {
                            return BreakPosition.jbpPrev;
                        }
                    }
                    break;

                case CharType.jctZM:
                    {
                        if ((aPrevType != CharType.jctZM) && (aPrevType != CharType.jctSZ)) // 当前是字母，前一个不是数字、字母
                        {
                            return BreakPosition.jbpPrev;
                        }
                    }
                    break;

                case CharType.jctSZ:
                    {
                        switch (aPrevType)
                        {
                            case CharType.jctZM:
                            case CharType.jctSZ:
                                break;

                            case CharType.jctFH:
                                {
                                    string vChar = aText.Substring(aIndex - 1 - 1, 1);

                                    if (vChar == "￠")
                                    {

                                    }
                                    else
                                    {
                                        if ((vChar != ".") && (vChar != ":") && (vChar != "-") && (vChar != "^") && (vChar != "*") && (vChar != "/"))
                                            return BreakPosition.jbpPrev;
                                    }
                                }
                                break;

                            default:
                                return BreakPosition.jbpPrev;
                        }
                    }
                    break;

                case CharType.jctFH:
                    {
                        switch (aPrevType)
                        {
                            case CharType.jctFH:
                                break;

                            case CharType.jctSZ:
                                {
                                    string vChar = aText.Substring(aIndex - 1, 1);
                                    if ((vChar != ".") && (vChar != ":") && (vChar != "-") && (vChar != "^") && (vChar != "*") && (vChar != "/"))
                                        return BreakPosition.jbpPrev;
                                }
                                break;

                            case CharType.jctZM:
                                {
                                    if (aText.Substring(aIndex - 1, 1) != ":")
                                        return BreakPosition.jbpPrev;
                                }
                                break;

                            default:
                                return BreakPosition.jbpPrev;
                        }
                    }
                    break;
            }

            return BreakPosition.jbpNone;
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

            if (MatchBreak(vPosCharType, vNextCharType, aText, aPos + 1) != BreakPosition.jbpPrev)  // 不能在当前截断，当前往前找截断
            {
                if (vPosCharType != CharType.jctBreak)
                {
                    bool vFindBreak = false;
                    CharType vPrevCharType;
                    for (int i = aPos - 1; i >= aStartPos; i--)
                    {
                        vPrevCharType = HC.GetUnicodeCharType((ushort)(aText[i - 1]));
                        if (MatchBreak(vPrevCharType, vPosCharType, aText, i + 1) == BreakPosition.jbpPrev)
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
                aPos.Y = DrawItems[aLastDrawItemNo].Rect.Bottom;
                vLineFirst = true;  // 作为行首
            }

            // 当前行空余宽度能放下或放不下但已经是行首了
            vRect.Left = aPos.X;
            vRect.Top = aPos.Y;
            vRect.Right = vRect.Left + vRectItem.Width;
            vRect.Bottom = vRect.Top + vRectItem.Height + Style.LineSpaceMin;
            NewDrawItem(aItemNo, aOffset, 1, vRect, vParaFirst, vLineFirst, ref aLastDrawItemNo);

            vRemainderWidth = aFmtRight - vRect.Right;  // 放入后的剩余量
        }

        private void _FormatBreakTextDrawItem(int aItemNo, int aFmtLeft, int aFmtRight, ref int aDrawItemNo,
            ref POINT aPos, ref RECT vRect, ref int vRemainderWidth, ref bool vParaFirst)
        {
            HCCanvas vCanvas = HCStyle.CreateStyleCanvas();
            try
            {
                HCCustomDrawItem vDrawItem = DrawItems[aDrawItemNo];
                HCCustomItem vItemBK = Items[vDrawItem.ItemNo];
                int vLen = vItemBK.Text.Length;

                CalcItemFormatHeigh(vItemBK);

                int vWidth = Style.TempCanvas.TextWidth(vItemBK.Text[vLen - 1]);
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
                vRect.Bottom = vRect.Top + FItemFormatHeight;
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

        /// <summary> 从指定偏移和指定位置开始格式化Text </summary>
        /// <param name="aCharOffset">文本格式化的起始偏移</param>
        /// <param name="aPlaceWidth">呈放文本的宽度</param>
        /// <param name="aBasePos">vCharWidths中对应偏移的起始位置</param>
        private void DoFormatTextItemToDrawItems(HCCustomItem vItem, int aOffset, string vText, int aCharOffset, int aPlaceWidth, int aBasePos,
            int aItemNo, int vItemLen, int aFmtLeft, int aContentWidth, int aFmtRight,
            int[] vCharWidths, ref bool vParaFirst, ref bool vLineFirst, ref POINT aPos, ref RECT vRect,
            ref int vRemainderWidth, ref int aLastDrawItemNo)
        {
            int viPlaceOffset,  // 能放下第几个字符
            viBreakOffset,  // 第几个字符放不下
            vFirstCharWidth;  // 第一个字符的宽度

            vLineFirst = vParaFirst || ((aPos.X == aFmtLeft) && (DrawItems[aLastDrawItemNo].Width != 0));
            viBreakOffset = 0;  // 换行位置，第几个字符放不下
            vFirstCharWidth = vCharWidths[aCharOffset - 1] - aBasePos;  // 第一个字符的宽度

            if (aPlaceWidth < 0)
                viBreakOffset = 1;
            else
            {
                for (int i = aCharOffset - 1; i <= vItemLen - 1; i++)
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
                vRect.Right = vRect.Left + vCharWidths[vItemLen - 1] - aBasePos;  // 使用自定义测量的结果
                vRect.Bottom = vRect.Top + FItemFormatHeight;
                NewDrawItem(aItemNo, aOffset + aCharOffset - 1, vItemLen - aCharOffset + 1, vRect, vParaFirst, vLineFirst, ref aLastDrawItemNo);
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
                    vRect.Right = vRect.Left + vCharWidths[vItemLen - 1] - aBasePos;  // 使用自定义测量的结果
                    vRect.Bottom = vRect.Top + FItemFormatHeight;
                    NewDrawItem(aItemNo, aOffset + aCharOffset - 1, 1, vRect, vParaFirst, vLineFirst, ref aLastDrawItemNo);
                    vParaFirst = false;

                    vRemainderWidth = aFmtRight - vRect.Right;  // 放入最多后的剩余量
                    FinishLine(aItemNo, aLastDrawItemNo, vRemainderWidth);

                    // 偏移到下一行顶端，准备另起一行
                    aPos.X = aFmtLeft;
                    aPos.Y = DrawItems[aLastDrawItemNo].Rect.Bottom;  // 不使用 vRect.Bottom 因为如果行中间有高的，会修正vRect.Bottom

                    if (aCharOffset < vItemLen)
                    {
                        DoFormatTextItemToDrawItems(vItem, aOffset, vText, aCharOffset + 1, aFmtRight - aPos.X, vCharWidths[aCharOffset - 1],
                            aItemNo, vItemLen, aFmtLeft, aContentWidth, aFmtRight, vCharWidths,
                            ref vParaFirst, ref vLineFirst, ref aPos, ref vRect, ref vRemainderWidth, ref aLastDrawItemNo);
                    }
                }
                else  // Data的宽度足够一个字符
                    if ((HC.PosCharHC(vText[aCharOffset - 1], HC.DontLineFirstChar) > 0)
                        && (Items[aItemNo - 1].StyleNo > HCStyle.Null)
                        && (DrawItems[aLastDrawItemNo].CharLen > 1))
                    {
                        _FormatBreakTextDrawItem(aItemNo, aFmtLeft, aFmtRight, ref aLastDrawItemNo, ref aPos, ref vRect, ref vRemainderWidth, ref vParaFirst);  // 上一个重新分裂
                        CalcItemFormatHeigh(vItem);

                        DoFormatTextItemToDrawItems(vItem, aOffset, vText, aCharOffset, aFmtRight - aPos.X, aBasePos, aItemNo, vItemLen, aFmtLeft, aContentWidth,
                            aFmtRight, vCharWidths, ref vParaFirst, ref vLineFirst, ref aPos, ref vRect, ref vRemainderWidth, ref aLastDrawItemNo);
                    }
                    else  // 整体下移到下一行
                    {
                        vRemainderWidth = aPlaceWidth;
                        FinishLine(aItemNo, aLastDrawItemNo, vRemainderWidth);
                        // 偏移到下一行开始计算
                        aPos.X = aFmtLeft;
                        aPos.Y = DrawItems[aLastDrawItemNo].Rect.Bottom;
                        DoFormatTextItemToDrawItems(vItem, aOffset, vText, aCharOffset, aFmtRight - aPos.X, aBasePos,
                            aItemNo, vItemLen, aFmtLeft, aContentWidth, aFmtRight, vCharWidths,
                            ref vParaFirst, ref vLineFirst, ref aPos, ref vRect, ref vRemainderWidth, ref aLastDrawItemNo);
                    }
            }
            else  // 当前行剩余宽度能放下当前Text的一部分
            {
                if (vFirstCharWidth > aFmtRight - aFmtLeft)  // Data的宽度不足一个字符
                    viPlaceOffset = viBreakOffset;
                else
                    viPlaceOffset = viBreakOffset - 1;  // 第viBreakOffset个字符放不下，前一个能放下

                FindLineBreak(vText, aCharOffset, ref viPlaceOffset);  // 判断从viPlaceOffset后打断是否合适

                if ((viPlaceOffset == 0) && (!vLineFirst))  // 能放下的都不合适放到当前行且不是行首格式化，整体下移
                {
                    vRemainderWidth = aPlaceWidth;
                    FinishLine(aItemNo, aLastDrawItemNo, vRemainderWidth);
                    aPos.X = aFmtLeft;  // 偏移到下一行开始计算
                    aPos.Y = DrawItems[aLastDrawItemNo].Rect.Bottom;
                    DoFormatTextItemToDrawItems(vItem, aOffset, vText, aCharOffset, aFmtRight - aPos.X, aBasePos, aItemNo, vItemLen,
                        aFmtLeft, aContentWidth, aFmtRight, vCharWidths, ref vParaFirst, ref vLineFirst, ref aPos, ref vRect, ref vRemainderWidth, ref aLastDrawItemNo);
                }
                else  // 有适合放到当前行的内容
                {
                    if (viPlaceOffset < aCharOffset)  // 找不到截断位置，就在原位置截断(如整行文本都是逗号)
                    {
                        if (vFirstCharWidth > aFmtRight - aFmtLeft)  // Data的宽度不足一个字符
                            viPlaceOffset = viBreakOffset;
                        else
                            viPlaceOffset = viBreakOffset - 1;
                    }

                    vRect.Left = aPos.X;
                    vRect.Top = aPos.Y;
                    vRect.Right = vRect.Left + vCharWidths[viPlaceOffset - 1] - aBasePos;  // 使用自定义测量的结果
                    vRect.Bottom = vRect.Top + FItemFormatHeight;

                    NewDrawItem(aItemNo, aOffset + aCharOffset - 1, viPlaceOffset - aCharOffset + 1, vRect, vParaFirst, vLineFirst, ref aLastDrawItemNo);
                    vParaFirst = false;

                    vRemainderWidth = aFmtRight - vRect.Right;  // 放入最多后的剩余量

                    FinishLine(aItemNo, aLastDrawItemNo, vRemainderWidth);

                    // 偏移到下一行顶端，准备另起一行
                    aPos.X = aFmtLeft;
                    aPos.Y = DrawItems[aLastDrawItemNo].Rect.Bottom;  // 不使用 vRect.Bottom 因为如果行中间有高的，会修正vRect.Bottom

                    if (viPlaceOffset < vItemLen)
                    {
                        DoFormatTextItemToDrawItems(vItem, aOffset, vText, viPlaceOffset + 1, aFmtRight - aPos.X, vCharWidths[viPlaceOffset - 1],
                            aItemNo, vItemLen, aFmtLeft, aContentWidth, aFmtRight, vCharWidths, ref vParaFirst, ref vLineFirst, ref aPos,
                            ref vRect, ref vRemainderWidth, ref aLastDrawItemNo);
                    }
                }
            }
        }
        #endregion

        private const int FormatTextCut = 8192;

        /// <summary> 转换指定Item指定Offs格式化为DItem </summary>
        /// <param name="AItemNo">指定的Item</param>
        /// <param name="AOffset">指定的格式化起始位置</param>
        /// <param name="AContentWidth">当前Data格式化宽度</param>
        /// <param name="APageContenBottom">当前页格式化底部位置</param>
        /// <param name="APos">起始位置</param>
        /// <param name="ALastDNo">起始DItemNo前一个值</param>
        /// <param name="vPageBoundary">数据页底部边界</param>
        private void FormatItemToDrawItems(int aItemNo, int aOffset, int aFmtLeft, int aFmtRight, int aContentWidth, ref POINT aPos, ref int aLastDrawItemNo)
        {
            if (!Items[aItemNo].Visible)
                return;

            bool vParaFirst = false, vLineFirst = false;
            HCCustomRectItem vRectItem = null;
            string vText = "";
            RECT vRect = new RECT();

            int vRemainderWidth = 0;
            HCCustomItem vItem = Items[aItemNo];
            HCParaStyle vParaStyle = Style.ParaStyles[vItem.ParaNo];

            if (vItem.ParaFirst && (aOffset == 1))
            {
                vParaFirst = true;
                vLineFirst = true;
                aPos.X += vParaStyle.FirstIndentPix;
            }
            else  // 非段第1个
            {
                vParaFirst = false;
                vLineFirst = (aPos.X == aFmtLeft) && (DrawItems[aLastDrawItemNo].Width != 0);
            }

            if (!vItem.Visible)  // 不显示的Item
            {
                vRect.Left = aPos.X;
                vRect.Top = aPos.Y;
                vRect.Right = vRect.Left;
                vRect.Bottom = vRect.Top + 5;
                NewDrawItem(aItemNo, aOffset, vItem.Length, vRect, vParaFirst, vLineFirst, ref aLastDrawItemNo);
            }
            else
            if (vItem.StyleNo < HCStyle.Null)
            {
                vRectItem = vItem as HCCustomRectItem;
                DoFormatRectItemToDrawItem(vRectItem, aItemNo, aFmtLeft, aContentWidth, aFmtRight, aOffset, 
                    vParaFirst, ref aPos, ref vRect, ref vLineFirst, ref aLastDrawItemNo, ref vRemainderWidth);
                // 如果进入表格前是样式1，进入表格里有把Style的全局TempStyleNo改成0，表格后面
                // 是样式0的格式化时，由于此时Data的FItemFormatHeight还是样式1的，应用样式0的
                // StyleNo时和全局的并没有变化，并不能应用修改FItemFormatHeight，所以需要清除一下。
                Style.ApplyTempStyle(HCStyle.Null);
            }
            else  // 文本
            {
                CalcItemFormatHeigh(vItem);
                vRemainderWidth = aFmtRight - aPos.X;

                if (aOffset != 1)
                    vText = vItem.Text.Substring(aOffset - 1, vItem.Length - aOffset + 1);
                else
                    vText = vItem.Text;

                if (vText == "")
                {
                    vRect.Left = aPos.X;
                    vRect.Top = aPos.Y;
                    vRect.Right = vRect.Left;
                    vRect.Bottom = vRect.Top + FItemFormatHeight;  //DefaultCaretHeight;
                    vParaFirst = true;
                    vLineFirst = true;
                    NewDrawItem(aItemNo, aOffset, 0, vRect, vParaFirst, vLineFirst, ref aLastDrawItemNo);
                    vParaFirst = false;
                }
                else  // 非空Item
                {
                    int vItemLen = vText.Length;
                    if (vItemLen > 38347922)
                        throw new Exception(HC.HCS_EXCEPTION_STRINGLENGTHLIMIT);

                    int[] vCharWidths = new int[vItemLen];

                    int[] vCharWArr = null;
                    int viLen = vItemLen;
                    if (viLen > FormatTextCut)
                        vCharWArr = new int[FormatTextCut];
                        
                    int vIndex = 0, viBase = 0;
                    SIZE vSize = new SIZE();
                    while (viLen > FormatTextCut)
                    {
                        Style.TempCanvas.GetTextExtentExPoint(vText.Substring(vIndex, FormatTextCut), FormatTextCut, vCharWArr, ref vSize);  // 超过65535数组元素取不到值
                        for (int i = 0; i <= FormatTextCut - 1; i++)
                            vCharWidths[vIndex + i] = vCharWArr[i] + viBase;

                        viLen -= FormatTextCut;
                        vIndex += FormatTextCut;
                        viBase = vCharWidths[vIndex - 1];
                    }

                    vCharWArr = new int[viLen];
                    Style.TempCanvas.GetTextExtentExPoint(vText.Substring(vIndex, viLen), viLen, vCharWArr, ref vSize);  // 超过65535数组元素取不到值
            
                    
                    for (int i = 0; i <= viLen - 1; i++)
                        vCharWidths[vIndex + i] = vCharWArr[i] + viBase;
            
                    //SetLength(vCharWArr, 0);
                    DoFormatTextItemToDrawItems(vItem, aOffset, vText, 1, aFmtRight - aPos.X, 0, aItemNo, vItemLen, aFmtLeft, aContentWidth, aFmtRight, vCharWidths,
                        ref vParaFirst, ref vLineFirst, ref aPos, ref vRect, ref vRemainderWidth, ref aLastDrawItemNo);
                    //SetLength(vCharWidths, 0);
                }
            }

            // 计算下一个的位置
            if (aItemNo == Items.Count - 1)
                FinishLine(aItemNo, aLastDrawItemNo, vRemainderWidth);
            else  // 不是最后一个，则为下一个Item准备位置
            {
                if (Items[aItemNo + 1].ParaFirst)
                {
                    FinishLine(aItemNo, aLastDrawItemNo, vRemainderWidth);
                    // 偏移到下一行顶端，准备另起一行
                    aPos.X = 0;
                    aPos.Y = DrawItems[aLastDrawItemNo].Rect.Bottom;  // 不使用 vRect.Bottom 因为如果行中间有高的，会修正其bottom
                }
                else  // 下一个不是段起始
                    aPos.X = vRect.Right;  // 下一个的起始坐标
            }
        }

        protected void FormatInit()
        {
            FFormatHeightChange = false;
            FFormatDrawItemCountChange = false;
            FFormatStartTop = 0;
            FFormatEndBottom = 0;
            FFormatStartDrawItemNo = -1;
            FLastFormatParaNo = HCStyle.Null;
        }

        protected void ReSetSelectAndCaret(int aItemNo)
        {
            ReSetSelectAndCaret(aItemNo, GetItemOffsetAfter(aItemNo));
        }

        protected void ReSetSelectAndCaret(int aItemNo, int aOffset, bool aNextWhenMid = false)
        {
            SelectInfo.StartItemNo = aItemNo;
            SelectInfo.StartItemOffset = aOffset;

            if (FFormatCount != 0)
                return;

            int vOffset = 0;
            if (Items[aItemNo].StyleNo > HCStyle.Null)
            {
                if (SelectInfo.StartItemOffset > Items[aItemNo].Length)
                    vOffset = Items[aItemNo].Length;
                else
                    vOffset = aOffset;
            }
            else
                vOffset = aOffset;
        
            int vDrawItemNo = GetDrawItemNoByOffset(aItemNo, vOffset);
            if (aNextWhenMid
              && (vDrawItemNo < DrawItems.Count - 1)
              && (DrawItems[vDrawItemNo + 1].ItemNo == aItemNo)
              && (DrawItems[vDrawItemNo + 1].CharOffs == vOffset + 1))
                vDrawItemNo++;

            CaretDrawItemNo = vDrawItemNo;
        }

        protected void GetFormatRange(ref int aFirstDrawItemNo, ref int aLastItemNo)
        {
            GetFormatRange(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, ref aFirstDrawItemNo, ref aLastItemNo);
        }

        protected void GetFormatRange(int aItemNo, int aItemOffset, ref int aFirstDrawItemNo, ref int aLastItemNo)
        {
            if (FFormatCount != 0)
                return;

            aFirstDrawItemNo = GetFormatFirstDrawItem(aItemNo, aItemOffset);
            aLastItemNo = GetParaLastItemNo(aItemNo);
        }

        protected int GetFormatFirstDrawItem(int aItemNo, int aItemOffset)
        {
            int vDrawItemNo = GetDrawItemNoByOffset(aItemNo, aItemOffset);
            return GetFormatFirstDrawItem(vDrawItemNo);
        }

        protected int GetFormatFirstDrawItem(int aDrawItemNo)
        {
            int Result = aDrawItemNo;
            if (!DrawItems[Result].ParaFirst)
            {
                if (DrawItems[Result].LineFirst)
                    Result--;

                while (Result > 0)
                {
                    if (DrawItems[Result].LineFirst)
                        break;
                    else
                        Result--;
                }
            }

            return Result;
        }

        protected void FormatPrepare(int AFirstDrawItemNo, int ALastItemNo = -1)
        {
            if (FFormatCount != 0)
                return;
        
            FormatInit();

            if ((AFirstDrawItemNo > 0) && (!DrawItems[AFirstDrawItemNo].LineFirst))
                throw new Exception("行中格式化必需从行首开始，否则会影响分散的计算！");
            //vFirstDrawItemNo := Items[AStartItemNo].FirstDItemNo;

            int vLastItemNo = -1, vFmtTopOffset = -1;
            if (ALastItemNo < 0)
                vLastItemNo = DrawItems[AFirstDrawItemNo].ItemNo;
            else
                vLastItemNo = ALastItemNo;

            int vLastDrawItemNo = GetItemLastDrawItemNo(vLastItemNo);
            DrawItems.MarkFormatDelete(AFirstDrawItemNo, vLastDrawItemNo);
            if (AFirstDrawItemNo > 0)
            {
                FFormatStartTop = DrawItems[AFirstDrawItemNo - 1].Rect.Bottom;
                vFmtTopOffset = DrawItems[AFirstDrawItemNo].Rect.Top - FFormatStartTop;
            }
            else
            {
                FFormatStartTop = 0;
                vFmtTopOffset = 0;
            }

            for (int i = AFirstDrawItemNo + 1; i <= vLastDrawItemNo; i++)
            {
                if (DrawItems[i].LineFirst)
                    vFmtTopOffset = vFmtTopOffset + DrawItems[i].Rect.Top - DrawItems[i - 1].Rect.Bottom;
            }

            if (vFmtTopOffset != 0)
                FFormatEndBottom = -1;
            else
                FFormatEndBottom = DrawItems[vLastDrawItemNo].Rect.Bottom - vFmtTopOffset;
        }

        protected void ReFormatData(int AFirstDrawItemNo, int ALastItemNo = -1, int AExtraItemCount = 0, bool AForceClearExtra = false)
        {
            if (FFormatCount != 0)
                return;
        
            int vLastItemNo = -1;
            if (ALastItemNo < 0)
                vLastItemNo = DrawItems[AFirstDrawItemNo].ItemNo;
            else
                vLastItemNo = ALastItemNo;

            int vDrawItemCount = DrawItems.Count;  // 格式化前的DrawItem数量
            FormatRange(AFirstDrawItemNo, vLastItemNo);  // 格式化指定范围内的Item
            FFormatDrawItemCountChange = DrawItems.Count != vDrawItemCount;  // 格式化前后DrawItem数量有变化
      
            // 计算格式化后段的底部位置变化
            int vLastDrawItemNo = GetItemLastDrawItemNo(vLastItemNo);
            if ((Items[vLastItemNo] is HCCustomRectItem) && (Items[vLastItemNo] as HCCustomRectItem).SizeChanged)
                FFormatHeightChange = true;
            else
                FFormatHeightChange = AForceClearExtra
                    || ((DrawItems[AFirstDrawItemNo].Rect.Top != FFormatStartTop)  // 段格式化后，高度的增量
                    || (DrawItems[vLastDrawItemNo].Rect.Bottom != FFormatEndBottom));

            if (FFormatHeightChange || (AExtraItemCount != 0) || FFormatDrawItemCountChange)
            {
                FFormatChange = true;
                vLastItemNo = -1;
                int vFmtTopOffset = 0;
                int vClearFmtHeight = 0;

                for (int i = vLastDrawItemNo + 1; i <= DrawItems.Count - 1; i++)  // 从格式化变动段的下一段开
                {
                    if ((AExtraItemCount != 0) || FFormatDrawItemCountChange)
                    {
                        // 处理格式化后面各DrawItem对应的ItemNo偏移
                        DrawItems[i].ItemNo = DrawItems[i].ItemNo + AExtraItemCount;
                        if (vLastItemNo != DrawItems[i].ItemNo)
                        {
                            vLastItemNo = DrawItems[i].ItemNo;
                            Items[vLastItemNo].FirstDItemNo = i;
                        }
                    }

                    if (FFormatHeightChange)
                    {
                        // 将原格式化因分页等原因引起的整体下移或增加的高度恢复回来
                        // 如果不考虑上面处理ItemNo的偏移，可将TTableCellData.ClearFormatExtraHeight方法写到基类，这里直接调用
                        if (DrawItems[i].LineFirst)
                            vFmtTopOffset = DrawItems[i - 1].Rect.Bottom - DrawItems[i].Rect.Top;

                        DrawItems[i].Rect.Offset(0, vFmtTopOffset);

                        if (Items[DrawItems[i].ItemNo].StyleNo < HCStyle.Null)
                        {
                            vClearFmtHeight = (Items[DrawItems[i].ItemNo] as HCCustomRectItem).ClearFormatExtraHeight();
                            DrawItems[i].Rect.Bottom = DrawItems[i].Rect.Bottom - vClearFmtHeight;
                        }
                    }
                }
            }
        }

        public HCFormatData(HCStyle aStyle)
            : base(aStyle)
        {
            FFormatCount = 0;
            FFormatChange = false;
            FormatInit();
        }

        public virtual void ReFormat()
        {
            if (FFormatCount == 0)
            {
                DrawItems.Clear();
                InitializeField();

                DrawItems.MarkFormatDelete(0, DrawItems.Count - 1);
                
                FormatInit();
                FormatRange(0, Items.Count - 1);
                
                FFormatHeightChange = true;
            }

            if ((SelectInfo.StartItemNo >= 0) && (SelectInfo.StartItemNo < Items.Count))
                ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);  // 防止清空后格式化完成后没有选中起始访问出错
            else
                ReSetSelectAndCaret(0, 0);
        }

        public virtual void ReFormatActiveParagraph()
        {
            if (SelectInfo.StartItemNo >= 0)
            {
                int vFirstItemNo = -1, vLastItemNo = -1;
                GetParaItemRang(SelectInfo.StartItemNo, ref vFirstItemNo, ref vLastItemNo);
                FormatPrepare(Items[vFirstItemNo].FirstDItemNo, vLastItemNo);
                ReFormatData(Items[vFirstItemNo].FirstDItemNo, vLastItemNo);

                Style.UpdateInfoRePaint();
                Style.UpdateInfoReCaret();

                ReSetSelectAndCaret(SelectInfo.StartItemNo);
            }
        }

        public virtual void ReFormatActiveItem()
        {
            if (this.SelectExists())
                return;

            if (SelectInfo.StartItemNo >= 0)
            {
                if (Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null)
                    (Items[SelectInfo.StartItemNo] as HCCustomRectItem).ReFormatActiveItem();

                int vFirstDrawItemNo = -1, vLastItemNo = -1;
                GetFormatRange(ref vFirstDrawItemNo, ref vLastItemNo);
                FormatPrepare(vFirstDrawItemNo, vLastItemNo);
                ReFormatData(vFirstDrawItemNo, vLastItemNo);

                Style.UpdateInfoRePaint();
                Style.UpdateInfoReCaret();

                if (SelectInfo.StartItemOffset > Items[SelectInfo.StartItemNo].Length)
                    ReSetSelectAndCaret(SelectInfo.StartItemNo);
            }
        }

        public void BeginFormat()
        {
            FFormatCount++;
        }

        public void EndFormat(bool aReformat = true)
        {
            if (FFormatCount > 0)
                FFormatCount--;

            if ((FFormatCount == 0) && aReformat)
                ReFormat();
        }

        public int Width
        {
            get { return FWidth; }
            set { FWidth = value; }
        }

        public int FormatStartDrawItemNo
        {
            get { return FFormatStartDrawItemNo; }
        }

        public bool FormatHeightChange
        {
            get { return FFormatHeightChange; }
        }

        public bool FormatDrawItemCountChange
        {
            get { return FFormatDrawItemCountChange; }
        }

        public bool FormatChange
        {
            get { return FFormatChange; }
            set { FFormatChange = value; }
        }
    }
}
