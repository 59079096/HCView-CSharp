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

namespace HC.View
{
    public delegate void TraverseItemEventHandle(HCCustomData AData, int AItemNo, int ATag, ref bool AStop);

    public class HCItemTraverse
    {
        public SectionArea Area;
        public int Tag;
        public bool Stop;
        public TraverseItemEventHandle Process;
    }

    public class SelectInfo
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
    
        public void Initialize()
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

        private void DrawItemPaintBefor(HCCustomData AData, int ADrawItemNo, RECT ADrawRect,
            int ADataDrawLeft, int ADataDrawBottom, int ADataScreenTop, int ADataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            int vDCState = ACanvas.Save();
            try
            {
                this.DoDrawItemPaintBefor(AData, ADrawItemNo, ADrawRect, ADataDrawLeft, ADataDrawBottom, ADataScreenTop,
                    ADataScreenBottom, ACanvas, APaintInfo);
            }
            finally
            {
                ACanvas.Restore(vDCState);
            }
        }

        private void DrawItemPaintAfter(HCCustomData AData, int ADrawItemNo, RECT ADrawRect,
            int ADataDrawLeft, int ADataDrawBottom, int ADataScreenTop, int ADataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            int vDCState = ACanvas.Save();
            try
            {
                this.DoDrawItemPaintAfter(AData, ADrawItemNo, ADrawRect, ADataDrawLeft, ADataDrawBottom, ADataScreenTop,
                    ADataScreenBottom, ACanvas, APaintInfo);
            }
            finally
            {
                ACanvas.Restore(vDCState);
            }
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

        /// <summary> 式化时，记录起始DrawItem和段最后的DrawItem </summary>
        /// <param name="AStartItemNo"></param>
        protected void FormatItemPrepare(int AStartItemNo, int AEndItemNo = -1)
        {
            int vLastDrawItemNo = -1;
            int vFirstDrawItemNo = FItems[AStartItemNo].FirstDItemNo;
            if (AEndItemNo < 0)
            {
                vLastDrawItemNo = GetItemLastDrawItemNo(AStartItemNo);
            }
            else
            {
                vLastDrawItemNo = GetItemLastDrawItemNo(AEndItemNo);
            }

            FDrawItems.MarkFormatDelete(vFirstDrawItemNo, vLastDrawItemNo);
            FDrawItems.FormatBeforBottom = FDrawItems[vLastDrawItemNo].Rect.Bottom;
        }

        private static bool IsCharSameType(Char A, Char B)
        {
            //if A = B then
            //  Result := True
            //else
              return false;
        }
        /// <summary> 返回字符串AText的分散分隔数量和各分隔的起始位置 </summary>
        /// <param name="AText">要计算的字符串</param>
        /// <param name="ACharIndexs">记录各分隔的起始位置</param>
        /// <returns>分散分隔数量</returns>
        private static int GetJustifyCount(string AText, List<int> ACharIndexs)
        {
            int Result = 0;
            if (AText == "")
            {
                throw new Exception("异常：不能对空字符串计算分散!");
            }

            if (ACharIndexs != null)
            {
                ACharIndexs.Clear();
            }

            Char vProvChar = (Char)0;

            for (int i = 1; i <= AText.Length; i++)
            {
                if (!IsCharSameType(vProvChar, AText[i - 1]))
                {
                    Result++;
                    if (ACharIndexs != null)
                    {
                        ACharIndexs.Add(i);
                    }
                }
                
                vProvChar = AText[i - 1];
            }

            if (ACharIndexs != null)
            {
                ACharIndexs.Add(AText.Length + 1);
            }

            return Result;
        }



#region FindLineBreak

        //GetHeadTailBreak 根据行首、尾对字符的约束条件，获取截断位置
        private void GetHeadTailBreak(string AText, ref int APos)
        {
            Char vChar = AText[APos + 1 - 1];  // 因为是要处理截断，所以APos肯定是小于Length(AText)的，不用考虑越界
            if (HC.PosCharHC(vChar, HC.DontLineFirstChar) > 0)  // 下一个是不能放在行首的字符
            {
                APos--;  // 当前要移动到下一行，往前一个截断重新判断
                GetHeadTailBreak(AText, ref APos);
            }
            else  // 下一个可以放在行首，当前位置能否放置到行尾
            {
                vChar = AText[APos -1 ];  // 当前位置字符
                if (HC.PosCharHC(vChar, HC.DontLineLastChar) > 0)  // 是不能放在行尾的字符
                {
                    APos--;  // 再往前寻找截断位置
                    GetHeadTailBreak(AText, ref APos);
                }
            }
        }

        /// <summary>
        /// 获取字符串排版时截断到下一行的位置
        /// </summary>
        /// <param name="AText"></param>
        /// <param name="APos">在第X个后面断开 X > 0</param>
        private void FindLineBreak(string AText, int AStartPos, ref int APos)
        {
            CharType vPosType, vPrevType, vNextType;
            GetHeadTailBreak(AText, ref APos);  // 根据行首、尾的约束条件找APos不符合时应该在哪一个位置并重新赋值给APos
            vPosType = HC.GetCharType((ushort)(AText[APos - 1]));  // 当前类型
            vNextType = HC.GetCharType((ushort)(AText[APos + 1 - 1]));  // 下一个字符类型

            if (HC.MatchBreak(vPosType, vNextType) != BreakPosition.jbpPrev)  // 不能在当前截断，当前往前找截断
            {
                if (vPosType != CharType.jctBreak)
                {
                    for (int i = APos - 1; i >= AStartPos + 1; i--)
                    {
                        vPrevType = HC.GetCharType((ushort)(AText[i - 1]));
                        if (HC.MatchBreak(vPrevType, vPosType) == BreakPosition.jbpPrev)
                        {
                            APos = i;
                            break;
                        }

                        vPosType = vPrevType;
                    }
                }
            }
        }
#endregion

        /// <summary> 从指定偏移和指定位置开始格式化Text </summary>
        /// <param name="ACharOffset">文本格式化的起始偏移</param>
        /// <param name="APlaceWidth">呈放文本的宽度</param>
        /// <param name="ABasePos">vCharWidths中对应偏移的起始位置</param>
        private void DoFormatTextItemToDrawItems(string vText, int ACharOffset, int APlaceWidth, int ABasePos,
            int AItemNo, int viLen, int vItemHeight, int AContentWidth, int[] vCharWidths, ref bool vParaFirst, 
            ref bool vLineFirst, ref POINT APos,  ref RECT vRect, ref int vRemainderWidth,
            ref int ALastDrawItemNo)
        {
            int viPlaceOffset,  // 能放下第几个字符
            viBreakOffset,  // 第几个字符放不下
            vFirstCharWidth;  // 第一个字符的宽度

            vLineFirst = (APos.X == 0);
            viBreakOffset = 0;  // 换行位置，第几个字符放不下
            vFirstCharWidth = vCharWidths[ACharOffset - 1] - ABasePos;  // 第一个字符的宽度

            for (int i = ACharOffset - 1; i <= viLen - 1; i++)
            {
                if (vCharWidths[i] - ABasePos > APlaceWidth)
                {
                    viBreakOffset = i + 1;
                    break;
                }
            }

            if (viBreakOffset < 1)  // 当前行剩余空间把vText全放置下了
            {
                vRect.Left = APos.X;
                vRect.Top = APos.Y;
                vRect.Width = vCharWidths[viLen - 1] - ABasePos;  // 使用自定义测量的结果
                vRect.Height = vItemHeight;
                NewDrawItem(AItemNo, ACharOffset, viLen - ACharOffset + 1, vRect, vParaFirst, vLineFirst, ref ALastDrawItemNo);
                vParaFirst = false;

                vRemainderWidth = AContentWidth - vRect.Right;  // 放入最多后的剩余量
            }
            else
            if (viBreakOffset == 1)  // 当前行剩余空间连第一个字符也放不下
            {
                if (vFirstCharWidth > AContentWidth)  // Data的宽度不足一个字符
                {
                    vRect.Left = APos.X;
                    vRect.Top = APos.Y;
                    vRect.Width = vCharWidths[viLen - 1] - ABasePos;  // 使用自定义测量的结果
                    vRect.Height = vItemHeight;
                    NewDrawItem(AItemNo, ACharOffset, 1, vRect, vParaFirst, vLineFirst, ref ALastDrawItemNo);
                    vParaFirst = false;

                    vRemainderWidth = AContentWidth - vRect.Right;  // 放入最多后的剩余量
                    FinishLine(AItemNo, ALastDrawItemNo, vRemainderWidth);

                    // 偏移到下一行顶端，准备另起一行
                    APos.X = 0;
                    APos.Y = FDrawItems[ALastDrawItemNo].Rect.Bottom;  // 不使用 vRect.Bottom 因为如果行中间有高的，会修正vRect.Bottom

                    if (viBreakOffset < viLen)
                    {
                        DoFormatTextItemToDrawItems(vText, viBreakOffset + 1, AContentWidth, vCharWidths[viBreakOffset - 1],
                            AItemNo, viLen, vItemHeight, AContentWidth, vCharWidths, ref vParaFirst, ref vLineFirst, ref APos, 
                            ref vRect, ref vRemainderWidth, ref ALastDrawItemNo);
                    }
                }
                else  // Data的宽度足够一个字符
                {
                    vRemainderWidth = APlaceWidth;
                    FinishLine(AItemNo, ALastDrawItemNo, vRemainderWidth);
                    // 偏移到下一行开始计算
                    APos.X = 0;
                    APos.Y = FDrawItems[ALastDrawItemNo].Rect.Bottom;
                    DoFormatTextItemToDrawItems(vText, ACharOffset, AContentWidth, ABasePos,
                        AItemNo, viLen, vItemHeight, AContentWidth, vCharWidths, ref vParaFirst, ref vLineFirst, ref APos,
                        ref vRect, ref vRemainderWidth, ref ALastDrawItemNo);
                }
            }
            else  // 当前行剩余宽度能放下当前Text的一部分
            {
                if (vFirstCharWidth > AContentWidth)  // Data的宽度不足一个字符
                {
                    viPlaceOffset = viBreakOffset;
                }
                else
                {
                    viPlaceOffset = viBreakOffset - 1;  // 第viBreakOffset个字符放不下，前一个能放下
                }

                FindLineBreak(vText, ACharOffset, ref viPlaceOffset);  // 判断从viPlaceOffset后打断是否合适

                if (viPlaceOffset < ACharOffset)  // 找不到截断位置，就在原位置截断(如整行文本都是逗号)
                {
                    if (vFirstCharWidth > AContentWidth)  // Data的宽度不足一个字符
                    {
                        viPlaceOffset = viBreakOffset;
                    }
                    else
                    {
                        viPlaceOffset = viBreakOffset - 1;
                    }
                }

                vRect.Left = APos.X;
                vRect.Top = APos.Y;
                vRect.Width = vCharWidths[viPlaceOffset - 1] - ABasePos;  // 使用自定义测量的结果
                vRect.Height = vItemHeight;

                NewDrawItem(AItemNo, ACharOffset, viPlaceOffset - ACharOffset + 1, vRect, vParaFirst, vLineFirst, ref ALastDrawItemNo);
                vParaFirst = false;

                vRemainderWidth = AContentWidth - vRect.Right;  // 放入最多后的剩余量
                FinishLine(AItemNo, ALastDrawItemNo, vRemainderWidth);

                // 偏移到下一行顶端，准备另起一行
                APos.X = 0;
                APos.Y = FDrawItems[ALastDrawItemNo].Rect.Bottom;  // 不使用 vRect.Bottom 因为如果行中间有高的，会修正vRect.Bottom

                if (viPlaceOffset < viLen)
                {
                    DoFormatTextItemToDrawItems(vText, viPlaceOffset + 1, AContentWidth, vCharWidths[viPlaceOffset - 1],
                        AItemNo, viLen, vItemHeight, AContentWidth, vCharWidths, ref vParaFirst, ref vLineFirst, ref APos,
                        ref vRect, ref vRemainderWidth, ref ALastDrawItemNo);
                }
            }
        }
        #region FinishLine
        /// <summary> 重整行 </summary>
        /// <param name="AEndDItemNo">行最后一个DItem</param>
        /// <param name="ARemWidth">行剩余宽度</param>
        private void FinishLine(int AItemNo, int ALineEndDItemNo, int ARemWidth)
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
            vLineBegDItemNo = ALineEndDItemNo;
            for (int i = ALineEndDItemNo; i >= 0; i--)  // 得到行起始DItemNo
            {
                if (FDrawItems[i].LineFirst)
                {
                    vLineBegDItemNo = i;
                    break;
                }
            }
            //Assert((vLineBegDItemNo >= 0), '断言失败：行起始DItemNo小于0！');
            // 找行DrawItem中最高的
            vMaxHiDrawItem = ALineEndDItemNo;  // 默认最后一个最高
            vMaxBottom = FDrawItems[ALineEndDItemNo].Rect.Bottom;  // 先默认行最后一个DItem的Rect底位置最大
            for (int i = ALineEndDItemNo - 1; i >= vLineBegDItemNo; i--)
            {
                if (FDrawItems[i].Rect.Bottom > vMaxBottom)
                {
                    vMaxBottom = FDrawItems[i].Rect.Bottom;  // 记下最大的Rect底位置
                    vMaxHiDrawItem = i;
                }
            }

            // 根据最高的处理行间距，并影响到同行DrawItem
            for (int i = ALineEndDItemNo; i >= vLineBegDItemNo; i--)
            {
                FDrawItems[i].Rect.Height = vMaxBottom - FDrawItems[i].Rect.Top;
            }

            // 处理对齐方式，放在这里，是因为方便计算行起始和结束DItem，避免绘制时的运算
            HCParaStyle vParaStyle = FStyle.ParaStyles[FItems[AItemNo].ParaNo];
            switch (vParaStyle.AlignHorz)  // 段内容水平对齐方式
            {
                case ParaAlignHorz.pahLeft:  // 默认
                    break;

                case ParaAlignHorz.pahRight:
                    {
                        for (int i = ALineEndDItemNo; i >= vLineBegDItemNo; i--)
                        {
                            FDrawItems[i].Rect.Offset(ARemWidth, 0);
                        }
                    }
                    break;

                case ParaAlignHorz.pahCenter:
                    {
                        viSplitW = ARemWidth / 2;
                        for (int i = ALineEndDItemNo; i >= vLineBegDItemNo; i--)
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
                            if (IsParaLastDrawItem(ALineEndDItemNo))  // 两端对齐，段最后一行不处理
                            {
                                return;
                            }
                        }
                        else  // 分散对齐，空行或只有一个字符时居中
                        {
                            if (vLineBegDItemNo == ALineEndDItemNo)  // 行只有一个DrawItem
                            {
                                if (FItems[FDrawItems[vLineBegDItemNo].ItemNo].Length < 2)  // 此DrawItem对应的内容长度不足2个按居中处理
                                {
                                    viSplitW = ARemWidth / 2;
                                    FDrawItems[vLineBegDItemNo].Rect.Offset(viSplitW, 0);
                                    return;
                                }
                            }
                        }

                        vCountWillSplit = 0;
                        vLineSpaceCount = 0;
                        vExtraW = 0;
                        vModWidth = 0;
                        viSplitW = ARemWidth;
                        ushort[] vDrawItemSplitCounts = new ushort[ALineEndDItemNo - vLineBegDItemNo + 1];  // 当前行各DItem分几份

                        for (int i = vLineBegDItemNo; i <= ALineEndDItemNo; i++)  // 计算空余分成几份
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
                                if ((i == ALineEndDItemNo) && (vDItemSpaceCount > 0))  // 行尾的DItem，少分一个
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
                            viSplitW = ARemWidth / vLineSpaceCount;  // 每一份的大小
                            vDItemSpaceCount = ARemWidth % vLineSpaceCount;  // 余数，借用变量
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

                        for (int i = vLineBegDItemNo + 1; i <= ALineEndDItemNo; i++)  // 以第一个为基准，其余各DrawItem增加的空间
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
        private void NewDrawItem(int AItemNo, int AOffs, int ACharLen, RECT ARect, bool AParaFirst, bool ALineFirst,
            ref int vLastDrawItemNo)
        {
            HCCustomDrawItem vDrawItem = new HCCustomDrawItem();
            vDrawItem.ItemNo = AItemNo;
            vDrawItem.CharOffs = AOffs;
            vDrawItem.CharLen = ACharLen;
            vDrawItem.ParaFirst = AParaFirst;
            vDrawItem.LineFirst = ALineFirst;
            vDrawItem.Rect = ARect;
            vLastDrawItemNo++;
            FDrawItems.Insert(vLastDrawItemNo, vDrawItem);
            if (AOffs == 1)
            {
                FItems[AItemNo].FirstDItemNo = vLastDrawItemNo;
            }    
        }
        #endregion

        #region
        private void DoFormatRectItemToDrawItem(HCCustomRectItem vRectItem, int AItemNo, int AContentWidth, int AOffset,
            bool vParaFirst, ref POINT APos, ref RECT vRect, ref bool vLineFirst, ref int ALastDrawItemNo, ref int vRemainderWidth)
        {
            vRectItem.FormatToDrawItem(this, AItemNo);
            int vWidth = AContentWidth - APos.X;
            if ((vRectItem.Width > vWidth) && (!vLineFirst))  // 当前行剩余宽度放不下且不是行首
            {
                // 偏移到下一行
                FinishLine(AItemNo, ALastDrawItemNo, vWidth);
                APos.X = 0;
                APos.Y = FDrawItems[ALastDrawItemNo].Rect.Bottom;
                vLineFirst = true;  // 作为行首
            }

            // 当前行空余宽度能放下或放不下但已经是行首了
            vRect.Left = APos.X;
            vRect.Top = APos.Y;
            vRect.Width = vRectItem.Width;
            vRect.Height = vRectItem.Height + HC.LineSpaceMin;

            NewDrawItem(AItemNo, AOffset, 1, vRect, vParaFirst, vLineFirst, ref ALastDrawItemNo);

            vRemainderWidth = AContentWidth - vRect.Right;  // 放入后的剩余量
        }
        #endregion

        /// <summary>
        /// 转换指定Item指定Offs格式化为DItem
        /// </summary>
        /// <param name="AItemNo">指定的Item</param>
        /// <param name="AOffset">指定的格式化起始位置</param>
        /// <param name="AContentWidth">当前Data格式化宽度</param>
        /// <param name="APageContenBottom">当前页格式化底部位置</param>
        /// <param name="APos">起始位置</param>
        /// <param name="ALastDNo">起始DItemNo前一个值</param>
        /// <param name="vPageBoundary">数据页底部边界</param>
        protected void _FormatItemToDrawItems(int AItemNo, int AOffset, int AContentWidth,
          ref POINT APos, ref int ALastDrawItemNo)
        {
            if (!FItems[AItemNo].Visible) return;

            RECT vRect = new RECT();
            int vItemHeight;
            bool vParaFirst, vLineFirst;
            int vRemainderWidth = 0;
            int vLastDrawItemNo = ALastDrawItemNo;
            HCCustomRectItem vRectItem = null;
            HCCustomItem vItem = FItems[AItemNo];

            if ((AOffset == 1) && (vItem.ParaFirst))  // 第一次处理段第一个Item
            {
                vParaFirst = true;
                vLineFirst = true;
            }
            else  // 非段第1个
            {
                vParaFirst = false;
                vLineFirst = (APos.X == 0);
            }

            if (vItem.StyleNo < HCStyle.Null)  // 是RectItem
            {
                vRectItem = vItem as HCCustomRectItem;
                DoFormatRectItemToDrawItem(vRectItem, AItemNo, AContentWidth, AOffset, vParaFirst, 
                    ref APos, ref vRect, ref vLineFirst, ref ALastDrawItemNo, ref vRemainderWidth);
            }
            else  // 文本
            {
                FStyle.TextStyles[vItem.StyleNo].ApplyStyle(FStyle.DefCanvas);
                vItemHeight = HCStyle.GetFontHeight(FStyle.DefCanvas);  // + vParaStyle.LineSpace;  // 行高

                TEXTMETRIC vTextMetric = new TEXTMETRIC();
                FStyle.DefCanvas.GetTextMetrics(ref vTextMetric);
                

                switch (FStyle.ParaStyles[vItem.ParaNo].LineSpaceMode)
                {
                    case ParaLineSpaceMode.pls100: 
                        vItemHeight = vItemHeight + vTextMetric.tmExternalLeading; // Round(vTextMetric.tmHeight * 0.2);
                        break;

                    case ParaLineSpaceMode.pls115: 
                        vItemHeight = vItemHeight + vTextMetric.tmExternalLeading + (int)Math.Round((vTextMetric.tmHeight + vTextMetric.tmExternalLeading) * 0.15);
                        break;

                    case ParaLineSpaceMode.pls150: 
                        vItemHeight = vItemHeight + vTextMetric.tmExternalLeading + (int)Math.Round((vTextMetric.tmHeight + vTextMetric.tmExternalLeading) * 0.5);
                        break;

                    case ParaLineSpaceMode.pls200: 
                        vItemHeight = vItemHeight + vTextMetric.tmExternalLeading + vTextMetric.tmHeight + vTextMetric.tmExternalLeading;
                        break;

                    case ParaLineSpaceMode.plsFix: 
                        vItemHeight = vItemHeight + HC.LineSpaceMin;
                        break;
                }

                vRemainderWidth = AContentWidth - APos.X;
                string vText = vItem.Text;

                if (vText == "")  // 空item(肯定是空行)
                {
                    //Assert(vItem.ParaFirst, HCS_EXCEPTION_NULLTEXT);
                    vRect.Left = APos.X;
                    vRect.Top = APos.Y;
                    vRect.Width = 0;
                    vRect.Height = vItemHeight;  //DefaultCaretHeight;
                    vParaFirst = true;
                    vLineFirst = true;
                    vLastDrawItemNo = ALastDrawItemNo;
                    NewDrawItem(AItemNo, AOffset, 0, vRect, vParaFirst, vLineFirst, ref vLastDrawItemNo);
                    ALastDrawItemNo = vLastDrawItemNo;
                    vParaFirst = false;
                }
                else  // 非空Item
                {
                    int viLen = vText.Length;

                    if (viLen > 65535)
                    {
                        throw new Exception(HC.HCS_EXCEPTION_STRINGLENGTHLIMIT);
                    }

                    int[] vCharWidths = new int[viLen];
 
                    SIZE vSize = new SIZE();
                    FStyle.DefCanvas.GetTextExtentExPoint(vText, viLen, vCharWidths, ref vSize);  // 超过65535数组元素取不到值

                    DoFormatTextItemToDrawItems(vText, AOffset, AContentWidth - APos.X, 0, AItemNo, 
                        viLen, vItemHeight, AContentWidth, vCharWidths, ref vParaFirst, ref vLineFirst, ref APos, 
                        ref vRect, ref vRemainderWidth, ref ALastDrawItemNo);
                }
            }

            // 计算下一个的位置
            if (AItemNo == FItems.Count - 1)
            {
                FinishLine(AItemNo, ALastDrawItemNo, vRemainderWidth);
            }
            else  // 不是最后一个，则为下一个Item准备位置
            {
                if (FItems[AItemNo + 1].ParaFirst)   // 下一个是段起始
                {
                    FinishLine(AItemNo, ALastDrawItemNo, vRemainderWidth);
                    // 偏移到下一行顶端，准备另起一行
                    APos.X = 0;
                    APos.Y = FDrawItems[ALastDrawItemNo].Rect.Bottom;  // 不使用 vRect.Bottom 因为如果行中间有高的，会修正其bottom
                }
                else  // 下一个不是段起始
                {
                    APos.X = vRect.Right;  // 下一个的起始坐标
                }
            }
        }

        /// <summary> 根据指定Item获取其所在段的起始和结束ItemNo </summary>
        /// <param name="AFirstItemNo1">指定</param>
        /// <param name="AFirstItemNo">起始</param>
        /// <param name="ALastItemNo">结束</param>
        protected void GetParaItemRang(int AItemNo, ref int AFirstItemNo, ref int ALastItemNo)
        {
            AFirstItemNo = AItemNo;

            while (AFirstItemNo > 0)
            {
                if (FItems[AFirstItemNo].ParaFirst)
                {
                    break;
                }
                else
                {
                    AFirstItemNo--;
                }
            }

            ALastItemNo = AItemNo + 1;
            while (ALastItemNo < FItems.Count)
            {
                if (FItems[ALastItemNo].ParaFirst)
                {
                    break;
                }
                else
                {
                    ALastItemNo++;
                }
            }

            ALastItemNo--;
        }

        protected int GetParaFirstItemNo(int AItemNo)
        {
            int Result = AItemNo;
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

        protected int GetParaLastItemNo(int AItemNo)
        {
            // 目前需要外部自己约束AItemNo < FItems.Count
            int Result = AItemNo + 1;
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
        protected int GetLineFirstItemNo(int AItemNo, int AOffset)
        {
            int Result = AItemNo;
            int vFirstDItemNo = GetDrawItemNoByOffset(AItemNo, AOffset);

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
        protected int GetLineLastItemNo(int AItemNo, int AOffset)
        {
            int Result = AItemNo;
            int vLastDItemNo = GetDrawItemNoByOffset(AItemNo, AOffset) + 1;  // 下一个开始，否则行第一个获取最后一个时还是行第一个
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
        protected virtual void GetLineDrawItemRang(ref int AFirstDItemNo, ref int ALastDItemNo)
        {
            while (AFirstDItemNo > 0)
            {
                if (FDrawItems[AFirstDItemNo].LineFirst)
                {
                    break;
                }
                else
                {
                    AFirstDItemNo--;
                }
            }

            ALastDItemNo = AFirstDItemNo + 1;
            while (ALastDItemNo < FDrawItems.Count)
            {
                if (FDrawItems[ALastDItemNo].LineFirst)
                {
                    break;
                }
                else
                {
                    ALastDItemNo++;
                }
            }

            ALastDItemNo--;
        }

        /// <summary> 获取指定DrawItem对应的Text </summary>
        /// <param name="ADrawItemNo"></param>
        /// <returns></returns>
        protected string GetDrawItemText(int ADrawItemNo)
        {
            string vText = "";
            HCCustomDrawItem vDrawItem = FDrawItems[ADrawItemNo];
            vText = FItems[vDrawItem.ItemNo].Text;
            if (vText != "")
            {
                vText = vText.Substring(vDrawItem.CharOffs - 1, vDrawItem.CharLen);
            }
            return vText;
        }

        protected void SetCaretDrawItemNo(int Value)
        {
            int vItemNo;

            if (FCaretDrawItemNo != Value)
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

                FCaretDrawItemNo = Value;

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

        protected virtual void DoDrawItemPaintBefor(HCCustomData AData, int ADrawItemNo, RECT ADrawRect,
            int ADataDrawLeft, int ADataDrawBottom, int ADataScreenTop, int ADataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        { }

        protected virtual void DoDrawItemPaintAfter(HCCustomData AData, int ADrawItemNo, RECT ADrawRect,
            int ADataDrawLeft, int ADataDrawBottom, int ADataScreenTop, int ADataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        { }

        public HCCustomData(HCStyle AStyle)
        {
            FStyle = AStyle;
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

        public virtual POINT GetScreenCoord(int X, int Y)
        {
            return this.GetRootData().GetScreenCoord(X, Y);
        }

        public virtual HCCustomItem CreateDefaultTextItem()
        {
            HCCustomItem vItem = new HCTextItem("");
            if (FStyle.CurStyleNo < HCStyle.Null)
            {
                vItem.StyleNo = 0;
            }
            else
            {
                vItem.StyleNo = FStyle.CurStyleNo;
            }

            vItem.ParaNo = FStyle.CurParaNo;

            return vItem;
        }

        public virtual HCCustomItem CreateDefaultDomainItem()
        {
            HCCustomItem Result = new HCDomainItem(this);
            Result.ParaNo = FStyle.CurParaNo;
            return Result;
        }

        private void GetRectItemInnerCaretInfo(HCCustomRectItem ARectItem, int AItemNo, int ADrawItemNo, HCCustomDrawItem ADrawItem, ref HCCaretInfo ACaretInfo)
        {
            ARectItem.GetCaretInfo(ref ACaretInfo);

            RECT vDrawRect = ADrawItem.Rect;

            int vLineSpaceHalf = GetLineSpace(ADrawItemNo) / 2;
            vDrawRect.Inflate(0, -vLineSpaceHalf);

            switch (FStyle.ParaStyles[FItems[AItemNo].ParaNo].AlignVert)  // 垂直对齐方式
            {
                case ParaAlignVert.pavCenter: 
                    ACaretInfo.Y = ACaretInfo.Y + vLineSpaceHalf + (vDrawRect.Height - ARectItem.Height) / 2;
                    break;

                case ParaAlignVert.pavTop: 
                    ACaretInfo.Y = ACaretInfo.Y + vLineSpaceHalf;
                    break;
                
                default:
                    ACaretInfo.Y = ACaretInfo.Y + vLineSpaceHalf + vDrawRect.Height - ARectItem.Height;
                    break;
            }
        }

        public virtual void GetCaretInfo(int AItemNo, int AOffset, ref HCCaretInfo ACaretInfo)
        {
            int vDrawItemNo, vStyleItemNo;
       
            /* 注意：为处理RectItem往外迭代，这里位置处理为叠加，而不是直接赋值 }*/
            if (FCaretDrawItemNo < 0)
            {
                if (FItems[AItemNo].StyleNo < HCStyle.Null)
                    vDrawItemNo = FItems[AItemNo].FirstDItemNo;
                else
                    vDrawItemNo = GetDrawItemNoByOffset(AItemNo, AOffset);  // AOffset处对应的DrawItemNo
            }
            else
                vDrawItemNo = FCaretDrawItemNo;

            HCCustomDrawItem vDrawItem = FDrawItems[vDrawItemNo];
            ACaretInfo.Height = vDrawItem.Height();  // 光标高度

            if (FStyle.UpdateInfo.ReStyle)
            {
                vStyleItemNo = AItemNo;
                if (AOffset == 0)
                {
                    if ((!FItems[AItemNo].ParaFirst)
                    && (AItemNo > 0)
                    && (Items[AItemNo - 1].StyleNo > HCStyle.Null))
                        vStyleItemNo = AItemNo - 1;
                }

                if ((Items[vStyleItemNo] is HCTextRectItem) && (FSelectInfo.StartItemOffset == HC.OffsetInner))
                    FStyle.CurStyleNo = (Items[vStyleItemNo] as HCTextRectItem).TextStyleNo;
                else
                    FStyle.CurStyleNo = Items[vStyleItemNo].StyleNo;

                FStyle.CurParaNo = Items[vStyleItemNo].ParaNo;
            }

            if (FItems[AItemNo].StyleNo < HCStyle.Null)
            {
                HCCustomRectItem vRectItem = FItems[AItemNo] as HCCustomRectItem;

                if (AOffset == HC.OffsetBefor)
                {
                    if (vRectItem.CanPageBreak)
                        GetRectItemInnerCaretInfo(vRectItem, AItemNo, vDrawItemNo, vDrawItem, ref ACaretInfo);

                    ACaretInfo.X = ACaretInfo.X + vDrawItem.Rect.Left;
                }
                else
                if (AOffset == HC.OffsetInner)
                {
                    GetRectItemInnerCaretInfo(vRectItem, AItemNo, vDrawItemNo, vDrawItem, ref ACaretInfo);
                    ACaretInfo.X = ACaretInfo.X + vDrawItem.Rect.Left;
                }
                else  // 在其右侧
                {
                    if (vRectItem.CanPageBreak)
                        GetRectItemInnerCaretInfo(vRectItem, AItemNo, vDrawItemNo, vDrawItem, ref ACaretInfo);

                    ACaretInfo.X = ACaretInfo.X + vDrawItem.Rect.Right;
                }
            }
            else  // TextItem
                ACaretInfo.X = ACaretInfo.X + vDrawItem.Rect.Left + GetDrawItemOffsetWidth(vDrawItemNo, AOffset - vDrawItem.CharOffs + 1);

            ACaretInfo.Y = ACaretInfo.Y + vDrawItem.Rect.Top;
        }

        /// <summary> 获取DItem中指定偏移处的内容绘制宽度 </summary>
        /// <param name="ADrawItemNo"></param>
        /// <param name="ADrawOffs">相对与DItem的CharOffs的Offs</param>
        /// <returns></returns>
        public int GetDrawItemOffsetWidth(int ADrawItemNo, int ADrawOffs)
        {
            int Result = 0;
            if (ADrawOffs == 0)
                return Result;

            HCCustomDrawItem vDItem = FDrawItems[ADrawItemNo];
            int vStyleNo = FItems[vDItem.ItemNo].StyleNo;
            if (vStyleNo < HCStyle.Null)
            {
                if (ADrawOffs > HC.OffsetBefor)
                    Result = FDrawItems[ADrawItemNo].Width();
            }
            else
            {
                FStyle.TextStyles[vStyleNo].ApplyStyle(FStyle.DefCanvas);

                ParaAlignHorz vAlignHorz = FStyle.ParaStyles[GetDrawItemParaStyle(ADrawItemNo)].AlignHorz;
                switch (vAlignHorz)
                {
                    case ParaAlignHorz.pahLeft:
                    case ParaAlignHorz.pahRight:
                    case ParaAlignHorz.pahCenter:
                        Result = FStyle.DefCanvas.TextWidth(FItems[vDItem.ItemNo].Text.Substring(vDItem.CharOffs - 1, ADrawOffs));
                        break;

                    case ParaAlignHorz.pahJustify:
                    case ParaAlignHorz.pahScatter:  // 20170220001 两端、分散对齐相关处理
                        if (vAlignHorz == ParaAlignHorz.pahJustify)
                        {
                            if (IsParaLastDrawItem(ADrawItemNo))
                            {
                                Result = FStyle.DefCanvas.TextWidth(FItems[vDItem.ItemNo].Text.Substring(vDItem.CharOffs - 1, ADrawOffs));
                                return Result;
                                //break;
                            }
                        }

                        string vText = GetDrawItemText(ADrawItemNo);
                        int viSplitW = vDItem.Width() - FStyle.DefCanvas.TextWidth(vText);  // 当前DItem的Rect中用于分散的空间
                        int vMod = 0;

                        // 计算当前Ditem内容分成几份，每一份在内容中的起始位置
                        List<int> vSplitList = new List<int>();
                        int vSplitCount = GetJustifyCount(vText, vSplitList);
                        bool vLineLast = IsLineLastDrawItem(ADrawItemNo);
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
                            int vCharWidth = FStyle.DefCanvas.TextWidth(vS);
                            if (vMod > 0)
                            {
                                vCharWidth++;  // 多分的余数
                                vSplitCount = 1;
                                vMod--;
                            }
                            else
                                vSplitCount = 0;

                            int vDOffset = vSplitList[i] + vS.Length - 1;
                            if (vDOffset <= ADrawOffs)
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

                                if (vDOffset == ADrawOffs)
                                    break;
                            }
                            else  // 当前字符结束位置在AOffs后，找具体位置
                            {
                                // 准备处理  a b c d e fgh ijklm n opq的形式(多个字符为一个分隔串)
                                for (int j = 1; j <= vS.Length; j++)  // 找在当前分隔的这串字符串中哪一个位置
                                {
                                    vCharWidth = FStyle.DefCanvas.TextWidth(vS[j].ToString());
                                    vDOffset = vSplitList[i] - 1 + j;

                                    if (vDOffset == vDItem.CharLen)
                                    {
                                        if (!vLineLast)
                                            vCharWidth = vCharWidth + viSplitW + vSplitCount;  // 当前DItem最后一个字符享受分隔间距和多分的余数
                                            //else 行最后一个DItem的最后一个字符不享受分隔间距和多分的余数，因为串格式化时最后一个分隔字符串右侧就不分间距
                                    }
                                    Result = Result + vCharWidth;

                                    if (vDOffset == ADrawOffs)
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
        /// <param name="AItemNo">指定的Item</param>
        /// <returns>最后面位置</returns>
        public int GetItemAfterOffset(int AItemNo)
        {
            if (FItems[AItemNo].StyleNo < HCStyle.Null)
                return HC.OffsetAfter;
            else
                return FItems[AItemNo].Length;
        }

        /// <summary>
        /// 根据给定的位置获取在此范围内的起始和结束DItem
        /// </summary>
        /// <param name="ATop"></param>
        /// <param name="ABottom"></param>
        /// <param name="AFristDItemNo"></param>
        /// <param name="ALastDItemNo"></param>
        public void GetDataDrawItemRang(int ATop, int ABottom, ref int AFirstDItemNo, ref int ALastDItemNo)
        {
            AFirstDItemNo = -1;
            ALastDItemNo = -1;
            // 获取第一个可显示的DrawItem
            for (int i = 0; i <= FDrawItems.Count - 1; i++)
            {
                if ((FDrawItems[i].LineFirst)
                  && (FDrawItems[i].Rect.Bottom > ATop)  // 底部超过区域上边
                  && (FDrawItems[i].Rect.Top < ABottom))  // 顶部没超过区域下边
                {
                    AFirstDItemNo = i;
                    break;
                }
            }

            if (AFirstDItemNo < 0)
                return;

            // 获取最后一个可显示的DrawItem
            for (int i = AFirstDItemNo; i <= FDrawItems.Count - 1; i++)
            {
                if ((FDrawItems[i].LineFirst) && (FDrawItems[i].Rect.Top >= ABottom))
                {
                    ALastDItemNo = i - 1;
                    break;
                }
            }

            if (ALastDItemNo < 0)
                ALastDItemNo = FDrawItems.Count - 1;
        }

        /// <summary>
        /// 返回指定坐标下的Item和Offset
        /// </summary>
        /// <param name="X">水平坐标值X</param>
        /// <param name="Y">垂直坐标值Y</param>
        /// <param name="AItemNo">坐标处的Item</param>
        /// <param name="AOffset">坐标在Item中的位置</param>
        /// <param name="ARestrain">True并不是在AItemNo范围内(在行最右侧或最后一行底部，通过约束坐标找到的)</param>
        public virtual void GetItemAt(int X, int Y, ref int AItemNo, ref int AOffset, ref int ADrawItemNo,
            ref bool ARestrain)
        {
            AItemNo = -1;
            AOffset = -1;
            ADrawItemNo = -1;
            ARestrain = true;  // 默认为约束找到(不在Item上面)

            if (IsEmptyData())
            {
                AItemNo = 0;
                AOffset = 0;
                ADrawItemNo = 0;
                return;
            }

            /* 获取对应位置最接近的起始DrawItem }*/
            int vStartDItemNo, vEndDItemNo = -1, vi;
            RECT vDrawRect;

            if (Y < 0)
                vStartDItemNo = 0;
            else  // 判断在哪一行
            {
                vDrawRect = FDrawItems[FDrawItems.Count - 1].Rect;
                if (Y > vDrawRect.Bottom)
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
                        if (Y > FDrawItems[vi].Rect.Bottom)
                        {
                            vStartDItemNo = vi + 1;  // 中间位置下一个
                            continue;
                        }
                        else
                        if (Y < FDrawItems[vi].Rect.Top)
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
                        if (Y > FDrawItems[vEndDItemNo].Rect.Bottom)
                            vStartDItemNo = vEndDItemNo;
                        else
                        if (Y >= FDrawItems[vEndDItemNo].Rect.Top)
                            vStartDItemNo = vEndDItemNo;

                        //else 不处理即第一个
                        break;
                        }
                    }
                }

                if (Y < FDrawItems[vStartDItemNo].Rect.Top)
                    vStartDItemNo--;
            }

            // 判断是指定行中哪一个Item
            GetLineDrawItemRang(ref vStartDItemNo, ref vEndDItemNo);  // 行起始和结束DrawItem

            if (X <= FDrawItems[vStartDItemNo].Rect.Left)
            {
                ADrawItemNo = vStartDItemNo;
                AItemNo = FDrawItems[vStartDItemNo].ItemNo;
                if (FItems[AItemNo].StyleNo < HCStyle.Null)
                    AOffset = HC.OffsetBefor;  // GetDrawItemOffset(vStartDItemNo, X)
                else
                    AOffset = FDrawItems[vStartDItemNo].CharOffs - 1;  // DrawItem起始
            }
            else
            if (X >= FDrawItems[vEndDItemNo].Rect.Right)
            {
                ADrawItemNo = vEndDItemNo;
                AItemNo = FDrawItems[vEndDItemNo].ItemNo;
                if (FItems[AItemNo].StyleNo < HCStyle.Null)
                    AOffset = HC.OffsetAfter;  // GetDrawItemOffset(vEndDItemNo, X)
                else
                    AOffset = FDrawItems[vEndDItemNo].CharOffs + FDrawItems[vEndDItemNo].CharLen - 1;  // DrawItem最后
            }
            else
            {
                for (int i = vStartDItemNo; i <= vEndDItemNo; i++)  // 行中间
                {
                    vDrawRect = FDrawItems[i].Rect;
                    if ((X >= vDrawRect.Left) && (X < vDrawRect.Right))
                    {
                        ARestrain = ((Y < vDrawRect.Top) || (Y > vDrawRect.Bottom));

                        ADrawItemNo = i;
                        AItemNo = FDrawItems[i].ItemNo;
                        if (FItems[AItemNo].StyleNo < HCStyle.Null)
                        {
                            if (ARestrain)
                            {
                                if (X < vDrawRect.Left + vDrawRect.Width / 2)
                                    AOffset = HC.OffsetBefor;
                                else
                                    AOffset = HC.OffsetAfter;
                            }
                            else
                                AOffset = GetDrawItemOffset(i, X);
                        }
                        else  // TextItem
                            AOffset = FDrawItems[i].CharOffs + GetDrawItemOffset(i, X) - 1;

                        break;
                    }
                }
            }
        }

        // Item和DItem互查 
        /// <summary>
        /// 获取Item对应的最后一个DItem
        /// </summary>
        /// <param name="AItemNo"></param>
        /// <returns></returns>
        public int GetItemLastDrawItemNo(int AItemNo)
        {
            int Result = -1;
            if (FItems[AItemNo].FirstDItemNo < 0)                
                return Result;  // 还没有格式化过

            Result = FItems[AItemNo].FirstDItemNo + 1;
            while (Result < FDrawItems.Count)
            {
                if (FDrawItems[Result].ParaFirst || (FDrawItems[Result].ItemNo != AItemNo))
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
        /// <param name="AItemNo"></param>
        /// <param name="AOffset"></param>
        /// <returns></returns>
        public bool OffsetInSelect(int AItemNo, int AOffset)
        {
            bool Result = false;

            if ((AItemNo < 0) || (AOffset < 0))
                return Result;

            if (FItems[AItemNo].StyleNo < HCStyle.Null)
            {
                if ((AOffset == HC.OffsetInner) && FItems[AItemNo].IsSelectComplate)
                    Result = true;

                return Result;
            }

            if (SelectExists())
            {
                if ((AItemNo > FSelectInfo.StartItemNo) && (AItemNo < FSelectInfo.EndItemNo))
                    Result = true;
                else

                if (AItemNo == FSelectInfo.StartItemNo)
                {
                    if (AItemNo == FSelectInfo.EndItemNo)
                        Result = (AOffset >= FSelectInfo.StartItemOffset) && (AOffset <= FSelectInfo.EndItemOffset);
                    else
                        Result = (AOffset >= FSelectInfo.StartItemOffset);
                }
                else
                if (AItemNo == FSelectInfo.EndItemNo)
                    Result = (AOffset <= FSelectInfo.EndItemOffset);
            }

            return Result;
       }

        /// <summary> 坐标是否在AItem的选中区域中 </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="AItemNo">X、Y处的Item</param>
        /// <param name="AOffset">X、Y处的Item偏移(供在RectItem上时计算)</param>
        /// <param name="ARestrain">AItemNo, AOffset是X、Y位置约束后的(此参数为方便单元格Data处理)</param>
        public virtual bool CoordInSelect(int X, int Y, int AItemNo, int AOffset, bool ARestrain)
        {
            bool Result = false;

            if ((AItemNo < 0) || (AOffset < 0))
                return Result;

            if (ARestrain)
                return Result;

            // 判断坐标是否在AItemNo对应的AOffset上
            int vDrawItemNo = GetDrawItemNoByOffset(AItemNo, AOffset);

            RECT vDrawRect = DrawItems[vDrawItemNo].Rect;
            Result = HC.PtInRect(vDrawRect, X, Y);

            if (Result)
            {
                if (FItems[AItemNo].StyleNo < HCStyle.Null)
                {
                    int vX = X - vDrawRect.Left;
                    int vY = Y - vDrawRect.Top - GetLineSpace(vDrawItemNo) / 2;

                    Result = (FItems[AItemNo] as HCCustomRectItem).CoordInSelect(vX, vY);
                }
                else
                    Result = OffsetInSelect(AItemNo, AOffset);  // 对应的AOffset在选中内容中
            }

            return Result;
        }

        /// <summary>
        /// 获取Data中的坐标X、Y处的Item和Offset，并返回X、Y相对DrawItem的坐标
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="AItemNo"></param>
        /// <param name="AOffset"></param>
        /// <param name="AX"></param>
        /// <param name="AY"></param>
        public void CoordToItemOffset(int X, int Y, int AItemNo, int AOffset, ref int AX, ref int AY)
        {
            AX = X;
            AY = Y;
            if (AItemNo < 0)
                return;

            int vDrawItemNo = GetDrawItemNoByOffset(AItemNo, AOffset);

            RECT vDrawRect = FDrawItems[vDrawItemNo].Rect;

            vDrawRect.Inflate(0, -GetLineSpace(vDrawItemNo) / 2);

            AX = AX - vDrawRect.Left;
            AY = AY - vDrawRect.Top;
            if (FItems[AItemNo].StyleNo < HCStyle.Null)
            {
                switch (FStyle.ParaStyles[FItems[AItemNo].ParaNo].AlignVert)  // 垂直对齐方式
                {
                    case ParaAlignVert.pavCenter: 
                        AY = AY - (vDrawRect.Height - (FItems[AItemNo] as HCCustomRectItem).Height) / 2;
                        break;
                    case ParaAlignVert.pavTop: 
                        break;

                    default:
                        AY = AY - (vDrawRect.Height - (FItems[AItemNo] as HCCustomRectItem).Height);
                        break;
                }
            }
        }

        /// <summary>
        /// 返回Item中指定Offset处的DrawItem序号
        /// </summary>
        /// <param name="AItemNo">指定Item</param>
        /// <param name="AOffset">Item中指定Offset</param>
        /// <returns>Offset处的DrawItem序号</returns>
        public int GetDrawItemNoByOffset(int AItemNo, int AOffset)
        {
            int Result = -1;

            if (FItems[AItemNo].StyleNo < HCStyle.Null)
                Result = FItems[AItemNo].FirstDItemNo;
            else  // TextItem
            {
                for (int i = FItems[AItemNo].FirstDItemNo; i <= FDrawItems.Count - 1; i++)
                {
                    HCCustomDrawItem vDrawItem = FDrawItems[i];
                    if (vDrawItem.ItemNo != AItemNo)
                        break;

                    if (AOffset - vDrawItem.CharOffs < vDrawItem.CharLen)
                    {
                        Result = i;
                        break;
                    }
                }
            }

            return Result;
        }

        public bool IsLineLastDrawItem(int ADrawItemNo)
        {
            // 不能在格式化进行中使用，因为DrawItems.Count可能只是当前格式化到的Item
            return ((ADrawItemNo == FDrawItems.Count - 1) || (FDrawItems[ADrawItemNo + 1].LineFirst));
        }
        
        public bool IsParaLastDrawItem(int ADrawItemNo)
        {
            bool Result = false;
            int vItemNo = FDrawItems[ADrawItemNo].ItemNo;

            if (vItemNo < FItems.Count - 1)
            {
                if (FItems[vItemNo + 1].ParaFirst)
                    Result = (FDrawItems[ADrawItemNo].CharOffsetEnd() == FItems[vItemNo].Length);  // 是Item最后一个DrawItem
            }
            else  // 是最后一个Item
                Result = (FDrawItems[ADrawItemNo].CharOffsetEnd() == FItems[vItemNo].Length);  // 是Item最后一个DrawItem

            return Result;
        }

        public bool IsParaLastItem(int AItemNo)
        {
            return ((AItemNo == FItems.Count - 1) || FItems[AItemNo + 1].ParaFirst);
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
        public int GetItemStyle(int AItemNo)
        {
            return FItems[AItemNo].StyleNo;
        }

        /// <summary> 返回DDrawItem对应的Item的文本样式 </summary>
        public int GetDrawItemStyle(int ADrawItemNo)
        {
            return GetItemStyle(FDrawItems[ADrawItemNo].ItemNo);
        }

        /// <summary> 返回Item对应的段落样式 </summary>
        public int GetItemParaStyle(int AItemNo)
        {
            return FItems[AItemNo].ParaNo;
        }

        /// <summary> 返回DDrawItem对应的Item的段落样式 </summary>
        public int GetDrawItemParaStyle(int ADrawItemNo)
        {
            return GetItemParaStyle(FDrawItems[ADrawItemNo].ItemNo);
        }

        /// <summary> 得到指定横坐标X处，是DItem内容的第几个字符 </summary>
        /// <param name="ADrawItemNo">指定的DItem</param>
        /// <param name="X">在Data中的横坐标</param>
        /// <returns>第几个字符</returns>
        public int GetDrawItemOffset(int ADrawItemNo, int X)
        {
            int Result = 0;

            HCCustomDrawItem vDrawItem = FDrawItems[ADrawItemNo];
            HCCustomItem vItem = FItems[vDrawItem.ItemNo];

            if (vItem.StyleNo < HCStyle.Null)
                Result = (vItem as HCCustomRectItem).GetOffsetAt(X - vDrawItem.Rect.Left);
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
                        Result = HC.GetCharOffsetByX(FStyle.DefCanvas, vText, X - vX);
                        break;

                    case ParaAlignHorz.pahJustify:
                    case ParaAlignHorz.pahScatter:  // 20170220001 两端、分散对齐相关处理
                        if (vParaStyle.AlignHorz == ParaAlignHorz.pahJustify)
                        {
                            if (IsParaLastDrawItem(ADrawItemNo))
                            {
                                Result = HC.GetCharOffsetByX(FStyle.DefCanvas, vText, X - vX);
                                return Result;
                            }
                        }
                        int vMod = 0;
                        int viSplitW = vDrawItem.Width() - FStyle.DefCanvas.TextWidth(vText);  // 当前DItem的Rect中用于分散的空间
                        // 计算当前Ditem内容分成几份，每一份在内容中的起始位置
                        List<int> vSplitList = new List<int>();
                        int vSplitCount = GetJustifyCount(vText, vSplitList);
                        bool vLineLast = IsLineLastDrawItem(ADrawItemNo);

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

                            if (vX + vCharWidth > X)
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
                                    if (vX > X)
                                    {
                                        if (vX - vCharWidth / 2 > X)
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

                if ((FSelectInfo.EndItemNo >= 0) && (FDrawItems[Result].CharOffsetEnd() == FSelectInfo.StartItemOffset))
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
        /// <param name="AAlign">对方方式</param>
        public virtual void ApplyParaAlignHorz(ParaAlignHorz AAlign)
        {
            ParaAlignHorzMatch vMatchStyle = new ParaAlignHorzMatch();
            vMatchStyle.Align = AAlign;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaAlignVert(ParaAlignVert AAlign)
        {
            ParaAlignVertMatch vMatchStyle = new ParaAlignVertMatch();
            vMatchStyle.Align = AAlign;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaBackColor(Color AColor)
        {
            ParaBackColorMatch vMatchStyle = new ParaBackColorMatch();
            vMatchStyle.BackColor = AColor;
            ApplySelectParaStyle(vMatchStyle);
        }

        public virtual void ApplyParaLineSpace(ParaLineSpaceMode ASpaceMode)
        {
            ParaLineSpaceMatch vMatchStyle = new ParaLineSpaceMatch();
            vMatchStyle.SpaceMode = ASpaceMode;
            ApplySelectParaStyle(vMatchStyle);
        }

        // 选中内容应用样式
        public virtual int ApplySelectTextStyle(HCStyleMatch AMatchStyle)
        {
            return -1;
        }

        public virtual int ApplySelectParaStyle(HCParaMatch AMatchStyle)
        {
            return -1;
        }

        /// <summary> 删除选中 </summary>
        public virtual bool DeleteSelected()
        {
            return false;
        }

        /// <summary> 为选中文本使用指定的文本样式 </summary>
        /// <param name="AFontStyle">文本样式</param>
        public virtual void ApplyTextStyle(HCFontStyle AFontStyle)
        {
            TextStyleMatch vMatchStyle = new TextStyleMatch();
            vMatchStyle.FontStyle = AFontStyle;
            ApplySelectTextStyle(vMatchStyle);
        }

        public virtual void ApplyTextFontName(string AFontName)
        {
            FontNameStyleMatch vMatchStyle = new FontNameStyleMatch();
            vMatchStyle.FontName = AFontName;
            ApplySelectTextStyle(vMatchStyle);
        }

        public virtual void ApplyTextFontSize(Single AFontSize)
        {
            FontSizeStyleMatch vMatchStyle = new FontSizeStyleMatch();
            vMatchStyle.FontSize = AFontSize;
            ApplySelectTextStyle(vMatchStyle);
        }

        public virtual void ApplyTextColor(Color AColor)
        {
            ColorStyleMatch vMatchStyle = new ColorStyleMatch();
            vMatchStyle.Color = AColor;
            ApplySelectTextStyle(vMatchStyle);
        }

        public virtual void ApplyTextBackColor(Color AColor)
        {
            BackColorStyleMatch vMatchStyle = new BackColorStyleMatch();
            vMatchStyle.Color = AColor;
            ApplySelectTextStyle(vMatchStyle);
        }

#region 当前显示范围内要绘制的DrawItem是否全选
    private bool DrawItemSelectAll(int AFristDItemNo, int ALastDItemNo)
    {
        int vSelStartDItemNo = GetSelectStartDrawItemNo();
        int vSelEndDItemNo = GetSelectEndDrawItemNo();

        return  // 当前页是否全选中了
            (
                (vSelStartDItemNo < AFristDItemNo)
                ||
                (
                    (vSelStartDItemNo == AFristDItemNo)
                    && (SelectInfo.StartItemOffset == FDrawItems[vSelStartDItemNo].CharOffs)
                )
            )
            &&
            (
                (vSelEndDItemNo > ALastDItemNo)
                ||
                (
                    (vSelEndDItemNo == ALastDItemNo)
                    && (SelectInfo.EndItemOffset == FDrawItems[vSelEndDItemNo].CharOffs + FDrawItems[vSelEndDItemNo].CharLen)
                )
            );
    }
#endregion

#region DrawTextJsutify 20170220001 分散对齐相关处理
        private void DrawTextJsutify(HCCanvas ACanvas, RECT ARect, string AText, bool ALineLast, int vTextDrawTop)
        {         
            int vMod = 0;
            int vX = ARect.Left;
            int viSplitW = (ARect.Right - ARect.Left) - FStyle.DefCanvas.TextWidth(AText);
            // 计算当前Ditem内容分成几份，每一份在内容中的起始位置
            List<int> vSplitList = new List<int>();
            int vSplitCount = GetJustifyCount(AText, vSplitList);
            if (ALineLast && (vSplitCount > 0))  // 行最后DItem，少分一个
                vSplitCount--;
              if (vSplitCount > 0)  // 有分到间距
              {
                  vMod = viSplitW % vSplitCount;
                  viSplitW = viSplitW / vSplitCount;
              }

              for (int i = 0; i <= vSplitList.Count - 2; i++)  // vSplitList最后一个是字符串长度所以多减1
              {
                  int vLen = vSplitList[i + 1] - vSplitList[i];
                  string vS = AText.Substring(vSplitList[i] - 1, vLen);

                  GDI.ExtTextOut(ACanvas.Handle, vX, vTextDrawTop, GDI.ETO_OPAQUE, IntPtr.Zero, vS, vLen, IntPtr.Zero);
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
        /// <param name="ADataDrawLeft">绘制目标区域Left</param>
        /// <param name="ADataDrawTop">绘制目标区域的Top</param>
        /// <param name="ADataDrawBottom">绘制目标区域的Bottom</param>
        /// <param name="ADataScreenTop">屏幕区域Top</param>
        /// <param name="ADataScreenBottom">屏幕区域Bottom</param>
        /// <param name="AVOffset">指定从哪个位置开始的数据绘制到目标区域的起始位置</param>
        /// <param name="ACanvas">画布</param>
        public virtual void PaintData(int ADataDrawLeft, int ADataDrawTop, int ADataDrawBottom,
            int ADataScreenTop, int ADataScreenBottom, int AVOffset, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (FItems.Count == 0)
                return;

            int vVOffset = ADataDrawTop - AVOffset;  // 将数据起始位置映射到绘制位置
            int vFristDItemNo = -1, vLastDItemNo = -1;
            GetDataDrawItemRang(Math.Max(ADataDrawTop, ADataScreenTop) - vVOffset,  // 可显示出来的DItem范围
                Math.Min(ADataDrawBottom, ADataScreenBottom) - vVOffset, ref vFristDItemNo, ref vLastDItemNo);

            if ((vFristDItemNo < 0) || (vLastDItemNo < 0))
                return;

            // 选中信息
            int vSelStartDOffs;
            int vSelStartDNo = GetSelectStartDrawItemNo();  // 选中起始DItem
            if (vSelStartDNo < 0)
                vSelStartDOffs = -1;
            else
                vSelStartDOffs = FSelectInfo.StartItemOffset - FDrawItems[vSelStartDNo].CharOffs + 1;

            int vSelEndDOffs;
            int vSelEndDNo = GetSelectEndDrawItemNo();      // 选中结束DrawItem
            if (vSelEndDNo < 0)
                vSelEndDOffs = -1;
            else
                vSelEndDOffs = FSelectInfo.EndItemOffset - FDrawItems[vSelEndDNo].CharOffs + 1;

            bool vDrawsSelectAll = DrawItemSelectAll(vFristDItemNo, vLastDItemNo);

            int vPrioStyleNo = -1;
            int vPrioParaNo = -1;
            int vTextHeight = 0;

            int vDCState = ACanvas.Save();
            try
            {
                int vLineSpace = -1;
                if (!FDrawItems[vFristDItemNo].LineFirst)
                    vLineSpace = GetLineSpace(vFristDItemNo);

                HCCustomDrawItem vDItem;
                RECT vDrawRect;
                ParaAlignHorz vAlignHorz = ParaAlignHorz.pahLeft;
                HCCustomItem vItem;

                for (int i = vFristDItemNo; i <= vLastDItemNo; i++)  // 遍历要绘制的数据
                {
                    vDItem = FDrawItems[i];
                    vItem = FItems[vDItem.ItemNo];
                    HCCustomRectItem vRectItem;
                    vDrawRect = vDItem.Rect;

                    vDrawRect.Offset(ADataDrawLeft, vVOffset);  // 偏移到指定的画布绘制位置(SectionData时为页数据在格式化中可显示起始位置)

                    if (FDrawItems[i].LineFirst)
                        vLineSpace = GetLineSpace(i);

                    // 绘制内容前
                    DrawItemPaintBefor(this, i, vDrawRect, ADataDrawLeft, ADataDrawBottom, ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);

                    if (vPrioParaNo != vItem.ParaNo)
                    {
                        vPrioParaNo = vItem.ParaNo;
                        vAlignHorz = FStyle.ParaStyles[vItem.ParaNo].AlignHorz;  // 段内容水平对齐方式
                    }

                    if (vItem.StyleNo < HCStyle.Null)
                    {
                        vRectItem = vItem as HCCustomRectItem;

                        vPrioStyleNo = vRectItem.StyleNo;

                        if (vRectItem.IsSelectComplate)
                        {
                            ACanvas.Brush.Color = FStyle.SelColor;
                            ACanvas.FillRect(vDrawRect);
                        }

                        // 除去行间距净Rect，即内容的显示区域
                        vDrawRect.Inflate(0, -vLineSpace / 2);

                        if (vRectItem.JustifySplit())
                        {
                            if (((vAlignHorz == ParaAlignHorz.pahJustify)
                                     && (!IsLineLastDrawItem(i))
                                  )
                                  ||
                                  (vAlignHorz == ParaAlignHorz.pahScatter)
                                )  // 分散对齐
                                vDrawRect.Inflate(-(vDrawRect.Width - vRectItem.Width) / 2, 0);
                            else
                                vDrawRect.Width = vRectItem.Width;
                        }

                        switch (FStyle.ParaStyles[vItem.ParaNo].AlignVert)  // 垂直对齐方式
                        {
                            case ParaAlignVert.pavCenter:
                                vDrawRect.Inflate(0, -(vDrawRect.Height - vRectItem.Height) / 2);
                                break;

                            case ParaAlignVert.pavTop:
                                break;

                            default:
                                vDrawRect.Top = vDrawRect.Bottom - vRectItem.Height;
                                break;
                        }

                        vItem.PaintTo(FStyle, vDrawRect, ADataDrawTop, ADataDrawBottom, ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);
                    }
                    else  // 文本Item
                    {
                        if (vItem.StyleNo != vPrioStyleNo)
                        {
                            vPrioStyleNo = vItem.StyleNo;
                            FStyle.TextStyles[vPrioStyleNo].ApplyStyle(ACanvas, APaintInfo.ScaleY / APaintInfo.Zoom);
                            FStyle.TextStyles[vPrioStyleNo].ApplyStyle(FStyle.DefCanvas);//, APaintInfo.ScaleY / APaintInfo.Zoom);

                            vTextHeight = HCStyle.GetFontHeight(FStyle.DefCanvas);
                            if (FStyle.TextStyles[vPrioStyleNo].FontStyles.Contains((byte)HCFontStyle.tsSuperscript)
                                || FStyle.TextStyles[vPrioStyleNo].FontStyles.Contains((byte)HCFontStyle.tsSubscript))
                            {
                                vTextHeight = vTextHeight + vTextHeight;
                            }
                        }

                        // 绘制文字、段、选中情况下的背景
                        if (!APaintInfo.Print)
                        {
                            if (vDrawsSelectAll)
                            {
                                ACanvas.Brush.Color = FStyle.SelColor;
                                ACanvas.FillRect(new RECT(vDrawRect.Left, vDrawRect.Top, vDrawRect.Left + vDItem.Width(), Math.Min(vDrawRect.Bottom, ADataScreenBottom)));
                            }
                            else  // 处理一部分选中
                                if (vSelEndDNo >= 0)
                                {
                                    ACanvas.Brush.Color = FStyle.SelColor;
                                    if ((vSelStartDNo == vSelEndDNo) && (i == vSelStartDNo))
                                    {
                                        ACanvas.FillRect(new RECT(vDrawRect.Left + GetDrawItemOffsetWidth(i, vSelStartDOffs),
                                            vDrawRect.Top, vDrawRect.Left + GetDrawItemOffsetWidth(i, vSelEndDOffs),
                                            Math.Min(vDrawRect.Bottom, ADataScreenBottom)));
                                    }
                                    else
                                        if (i == vSelStartDNo)
                                        {
                                            ACanvas.FillRect(new RECT(vDrawRect.Left + GetDrawItemOffsetWidth(i, vSelStartDOffs),
                                            vDrawRect.Top, vDrawRect.Right, Math.Min(vDrawRect.Bottom, ADataScreenBottom)));
                                        }
                                        else
                                            if (i == vSelEndDNo)
                                            {
                                                ACanvas.FillRect(new RECT(vDrawRect.Left,
                                                vDrawRect.Top, vDrawRect.Left + GetDrawItemOffsetWidth(i, vSelEndDOffs),
                                                Math.Min(vDrawRect.Bottom, ADataScreenBottom)));
                                            }
                                            else
                                                if ((i > vSelStartDNo) && (i < vSelEndDNo))
                                                    ACanvas.FillRect(vDrawRect);
                                }
                        }

                        // 除去行间距净Rect，即内容的显示区域
                        vDrawRect.Inflate(0, -(vDrawRect.Height - vTextHeight) / 2);//FStyle.ParaStyles[vItem.ParaNo].LineSpaceHalf);
                        int vTextDrawTop;
                        switch (FStyle.ParaStyles[vItem.ParaNo].AlignVert)  // 垂直对齐方式
                        {
                            case ParaAlignVert.pavCenter:
                                vTextDrawTop = vDrawRect.Top + (vDrawRect.Bottom - vDrawRect.Top - vTextHeight) / 2;
                                break;

                            case ParaAlignVert.pavTop:
                                vTextDrawTop = vDrawRect.Top;
                                break;

                            default:
                                vTextDrawTop = vDrawRect.Bottom - vTextHeight;
                                break;
                        }

                        if (FStyle.TextStyles[vPrioStyleNo].FontStyles.Contains((byte)HCFontStyle.tsSubscript))
                            vTextDrawTop = vTextDrawTop + vTextHeight / 2;

                        // 文字背景
                        if (FStyle.TextStyles[vPrioStyleNo].BackColor.A != 0)
                        {
                            ACanvas.Brush.Color = FStyle.TextStyles[vPrioStyleNo].BackColor;
                            ACanvas.FillRect(new RECT(vDrawRect.Left, vDrawRect.Top, vDrawRect.Left + vDItem.Width(), vDrawRect.Bottom));
                        }

                        vItem.PaintTo(FStyle, vDrawRect, ADataDrawTop, ADataDrawBottom,
                            ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);  // 触发Item绘制事件

                        // 绘制文本
                        ACanvas.Brush.Style = HCBrushStyle.bsClear;

                        string vText = "";
                        if (vDItem.CharLen > 0)
                          vText = vItem.Text.Substring(vDItem.CharOffs - 1, vDItem.CharLen);  // 为减少判断，没有直接使用GetDrawItemText(i)

                        if (vText != "")
                        {
                            switch (vAlignHorz)  // 水平对齐方式
                            {
                                case ParaAlignHorz.pahLeft:
                                case ParaAlignHorz.pahRight:
                                case ParaAlignHorz.pahCenter:  // 一般对齐
                                    int vLen = vText.Length;
                                    GDI.ExtTextOut(ACanvas.Handle, vDrawRect.Left, vTextDrawTop,
                                        GDI.ETO_OPAQUE, IntPtr.Zero, vText, vLen, IntPtr.Zero);
                                    break;

                                case ParaAlignHorz.pahJustify:
                                case ParaAlignHorz.pahScatter:  // 两端、分散对齐
                                    DrawTextJsutify(ACanvas, vDrawRect, vText, IsLineLastDrawItem(i), vTextDrawTop);
                                    break;
                            }
                        }
                        else  // 空行
                        {
                            if (!vItem.ParaFirst)
                                throw new Exception(HC.HCS_EXCEPTION_NULLTEXT);
                        }
                    }

                    DrawItemPaintAfter(this, i, vDrawRect, ADataDrawLeft, ADataDrawBottom,
                        ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);  // 绘制内容后
                }
            }
            finally
            {
                ACanvas.Restore(vDCState);
            }
        }

        /// <summary> 根据行中某DrawItem获取当前行间距 </summary>
        /// <param name="ADrawNo">行中指定的DrawItem</param>
        /// <returns>行间距</returns>
        public int GetLineSpace(int ADrawNo)
        {
            int Result = 0;
            int vFirst = -1;
            for (int i = ADrawNo; i >= 0; i--)
            {
                if (FDrawItems[i].LineFirst)
                {
                    vFirst = i;
                    break;
                }
            }

            int vLast = FDrawItems.Count - 1;

            for (int i = ADrawNo + 1; i <= FDrawItems.Count - 1; i++)
            {
                if (FDrawItems[i].LineFirst)
                {
                    vLast = i - 1;
                    break;
                }
            }

            int vMaxHi = 0;
            int vMaxDrawNo;

            using (HCCanvas vCanvas = HCStyle.CreateStyleCanvas())
            {
                vMaxDrawNo = vFirst;
                for (int i = vFirst; i <= vLast; i++)
                {
                    int vHi;
                    if (GetDrawItemStyle(i) < HCStyle.Null)
                        vHi = (FItems[FDrawItems[i].ItemNo] as HCCustomRectItem).Height;
                    else
                    {
                        FStyle.TextStyles[FItems[FDrawItems[i].ItemNo].StyleNo].ApplyStyle(vCanvas);  // APaintInfo.ScaleY / APaintInfo.Zoom);
                        vHi = HCStyle.GetFontHeight(vCanvas);
                    }

                    if (vHi > vMaxHi)
                    {
                        vMaxHi = vHi;  // 记下最大的高度
                        vMaxDrawNo = i;
                    }
                }
            }

            Result = GetDrawItemLineSpace(vMaxDrawNo);

            return Result;
        }

        /// <summary> 获取指定DrawItem的行间距 </summary>
        /// <param name="ADrawNo">指定的DrawItem</param>
        /// <returns>DrawItem的行间距</returns>
        public int GetDrawItemLineSpace(int ADrawNo)
        {
            int Result = HC.LineSpaceMin;

            if (GetDrawItemStyle(ADrawNo) >= HCStyle.Null)
            {
                HCCanvas vCanvas = HCStyle.CreateStyleCanvas();

                try
                {
                    FStyle.TextStyles[GetDrawItemStyle(ADrawNo)].ApplyStyle(vCanvas);
                    TEXTMETRIC vTextMetric = new TEXTMETRIC();
                    Win32.GDI.GetTextMetrics(vCanvas.Handle, ref vTextMetric);  // 得到字体信息

                    switch (FStyle.ParaStyles[GetDrawItemParaStyle(ADrawNo)].LineSpaceMode)
                    {
                        case ParaLineSpaceMode.pls100:
                            Result = vTextMetric.tmExternalLeading; // Round(vTextMetric.tmHeight * 0.2);
                            break;

                        case ParaLineSpaceMode.pls115:
                            Result = vTextMetric.tmExternalLeading + (int)Math.Round((vTextMetric.tmHeight + vTextMetric.tmExternalLeading) * 0.15);
                            break;

                        case ParaLineSpaceMode.pls150:
                            Result = vTextMetric.tmExternalLeading + (int)Math.Round((vTextMetric.tmHeight + vTextMetric.tmExternalLeading) * 0.5);
                            break;

                        case ParaLineSpaceMode.pls200:
                            Result = vTextMetric.tmExternalLeading + vTextMetric.tmHeight + vTextMetric.tmExternalLeading;
                            break;

                        case ParaLineSpaceMode.plsFix:
                            Result = HC.LineSpaceMin;
                            break;
                    }
                }
                finally
                {
                    HCStyle.DestroyStyleCanvas(vCanvas);
                }
            }

            return Result;
        }

        /// <summary> 是否有选中 </summary>
        public bool SelectExists(bool AIfRectItem = true)
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
                    if (AIfRectItem && (FItems[FSelectInfo.StartItemNo].StyleNo < HCStyle.Null))
                    {
                        Result = (FItems[FSelectInfo.StartItemNo] as HCCustomRectItem).SelectExists();
                    }
                }
            }

            return Result;
        }

        public void MarkStyleUsed(bool AMark)
        {
            for (int i = 0; i <= FItems.Count - 1; i++)
            {
                HCCustomItem vItem = FItems[i];
                if (AMark)
                {
                    FStyle.ParaStyles[vItem.ParaNo].CheckSaveUsed = true;
                    if (vItem.StyleNo < HCStyle.Null)
                        (vItem as HCCustomRectItem).MarkStyleUsed(AMark);
                    else
                        FStyle.TextStyles[vItem.StyleNo].CheckSaveUsed = true;
                }
                else  // 重新赋值
                {
                    vItem.ParaNo = FStyle.ParaStyles[vItem.ParaNo].TempNo;
                    if (vItem.StyleNo < HCStyle.Null)
                        (vItem as HCCustomRectItem).MarkStyleUsed(AMark);
                    else
                        vItem.StyleNo = FStyle.TextStyles[vItem.StyleNo].TempNo;
                }
            }
        }

        public virtual void SaveToStream(Stream AStream)
        {
            SaveToStream(AStream, 0, 0, FItems.Count - 1, Items[FItems.Count - 1].Length);
        }

        public virtual void SaveToStream(Stream AStream, int AStartItemNo, int AStartOffset,
            int AEndItemNo, int AEndOffset)
        {
            Int64 vBegPos = AStream.Position;
            byte[] vBuffer = System.BitConverter.GetBytes(vBegPos);
            AStream.Write(vBuffer, 0, vBuffer.Length);  // 数据大小占位，便于越过

            int vi = AEndItemNo - AStartItemNo + 1;
            vBuffer = System.BitConverter.GetBytes(vi);
            AStream.Write(vBuffer, 0, vBuffer.Length);  // 数量

            if (vi > 0)
            {
                if (AStartItemNo != AEndItemNo)
                {
                    FItems[AStartItemNo].SaveToStream(AStream, AStartOffset, FItems[AStartItemNo].Length);
                    for (int i = AStartItemNo + 1; i <= AEndItemNo - 1; i++)
                        FItems[i].SaveToStream(AStream);

                    FItems[AEndItemNo].SaveToStream(AStream, 0, AEndOffset);

                }
                else
                    FItems[AStartItemNo].SaveToStream(AStream, AStartOffset, AEndOffset);

            }
            //
            Int64 vEndPos = AStream.Position;

            AStream.Position = vBegPos;

            vBegPos = vEndPos - vBegPos - Marshal.SizeOf(vBegPos);
            vBuffer = System.BitConverter.GetBytes(vBegPos);
            AStream.Write(vBuffer, 0, vBuffer.Length);  // 当前页数据大小

            AStream.Position = vEndPos;
        }

        public string SaveToText()
        {
            return SaveToText(0, 0, FItems.Count - 1, FItems[FItems.Count - 1].Length);
        }

        public string SaveToText(int AStartItemNo, int AStartOffset, int AEndItemNo, int AEndOffset)
        {
            string Result = "";

            int vi = AEndItemNo - AStartItemNo + 1;

            if (vi > 0)
            {
                if (AStartItemNo != AEndItemNo)
                {
                    if (FItems[AStartItemNo].StyleNo > HCStyle.Null)
                        Result = (FItems[AStartItemNo] as HCTextItem).GetTextPart(AStartOffset + 1, FItems[AStartItemNo].Length - AStartOffset);
                    else
                        Result = (FItems[AStartItemNo] as HCCustomRectItem).SaveSelectToText();

                    for (int i = AStartItemNo + 1; i <= AEndItemNo - 1; i++)
                        Result = Result + FItems[i].Text;

                    if (FItems[AEndItemNo].StyleNo > HCStyle.Null)
                        Result = Result + (FItems[AEndItemNo] as HCTextItem).GetTextPart(1, AEndOffset);
                    else
                        Result = (FItems[AEndItemNo] as HCCustomRectItem).SaveSelectToText();
                }
                else  // 选中在同一Item
                {
                    if (FItems[AStartItemNo].StyleNo > HCStyle.Null)
                        Result = (FItems[AStartItemNo] as HCTextItem).GetTextPart(AStartOffset + 1, AEndOffset - AStartOffset);
                }
            }

            return Result;
        }
        
        /// <summary> 保存选中内容到流 </summary>
        public virtual void SaveSelectToStream(Stream AStream)
        {
            if (SelectExists())
            {
                if ((FSelectInfo.EndItemNo < 0)
                  && (FItems[FSelectInfo.StartItemNo].StyleNo < HCStyle.Null))
                {
                    if ((FItems[FSelectInfo.StartItemNo] as HCCustomRectItem).IsSelectComplateTheory())
                    {
                        this.SaveToStream(AStream, FSelectInfo.StartItemNo, HC.OffsetBefor, FSelectInfo.StartItemNo, HC.OffsetAfter);
                    }
                    else
                        (FItems[FSelectInfo.StartItemNo] as HCCustomRectItem).SaveSelectToStream(AStream);
                }
                else
                {
                    this.SaveToStream(AStream, FSelectInfo.StartItemNo, FSelectInfo.StartItemOffset, FSelectInfo.EndItemNo, FSelectInfo.EndItemOffset);
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

        public virtual bool InsertStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            return false;
        }

        public virtual void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            Clear();
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
