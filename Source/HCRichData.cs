/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{             支持格式化文档对象管理单元                }
{                                                       }
{*******************************************************/

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using HC.Win32;
using System.Xml;

namespace HC.View
{
    public delegate void DataItemEventHandler(HCCustomData aData, int aItemNo);

    public class HCRichData : HCUndoData
    {
        public delegate bool InsertProcEventHandler(HCCustomItem aItem);

        public delegate void DrawItemPaintEventHandler(HCCustomData aData, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas aCanvas, PaintInfo aPaintInfo);

        public delegate void ItemMouseEventHandler(HCCustomData aData, int aItemNo, MouseEventArgs e);

        private int FWidth;

        /// <summary> 鼠标左键按下(打开文件对话框双击文件后会触发MouseMouse，MouseUp) </summary>
        private bool FMouseLBDowning,
            /// <summary> 鼠标双击(处理双击自动选中，弹起清除选中的问题) </summary>
            FMouseLBDouble,
            FMouseDownReCaret,
            FMouseMoveRestrain;  // 并不是在Item范围内MouseMove而是通过约束坐标找到的

        private int FMouseDownX, FMouseDownY,
            FMouseDownItemNo,
            FMouseDownItemOffset,
            FMouseMoveItemNo,
            FMouseMoveItemOffset,
            FMouseMoveDrawItemNo,
            FSelectSeekNo,
            FSelectSeekOffset;  // 选中操作时的游标

        /// <summary> 调用InsertItem批量插入多个Item时(如数据组批量插入2个)防止别的操作引起位置变化导致后面插入位置不正确 </summary>
        private int FBatchInsertCount;
    
        private bool FReadOnly, FSelecting, FDraging;

        private DataItemEventHandler FOnItemResized;

        private ItemMouseEventHandler FOnItemMouseDown, FOnItemMouseUp;

        private DrawItemPaintEventHandler FOnDrawItemPaintBefor, FOnDrawItemPaintAfter;

        private EventHandler FOnCreateItem;  // 新建了Item(目前主要是为了打字和用中文输入法输入英文时痕迹的处理)

        private void FormatData(int aStartItemNo, int aLastItemNo)
        {
            int vPrioDrawItemNo = 0;
            POINT vPos = new POINT();

            _FormatReadyParam(aStartItemNo, ref vPrioDrawItemNo, ref vPos);
            HCParaStyle vParaStyle = Style.ParaStyles[Items[aStartItemNo].ParaNo];

            for (int i = aStartItemNo; i <= aLastItemNo; i++)
            {
                if (Items[i].ParaFirst)
                {
                    vParaStyle = Style.ParaStyles[Items[i].ParaNo];
                    vPos.X = vParaStyle.LeftIndentPix;
                }

                _FormatItemToDrawItems(i, 1, vParaStyle.LeftIndentPix, 
                    FWidth - vParaStyle.RightIndentPix, FWidth, ref vPos, ref vPrioDrawItemNo);
            }
        }

        /// <summary> 初始化为只有一个空Item的Data</summary>
        private void SetEmptyData()
        {
            if (this.Items.Count == 0)
            {
                HCCustomItem vItem = CreateDefaultTextItem();
                vItem.ParaFirst = true;
                Items.Add(vItem);

                FormatData(0, 0);
                ReSetSelectAndCaret(0);
            }
        }

        /// <summary> Data只有空行Item时插入Item(用于替换当前空行Item的情况) </summary>
        private bool EmptyDataInsertItem(HCCustomItem aItem)
        {
            if ((aItem.StyleNo > HCStyle.Null) && (aItem.Text == ""))
                return false;

            UndoAction_DeleteItem(0, 0);
            Items.Clear();
            DrawItems.Clear();
            aItem.ParaFirst = true;
            Items.Add(aItem);
            UndoAction_InsertItem(0, 0);
            FormatData(0, 0);
            ReSetSelectAndCaret(0);
            return true;
        }

        /// <summary> 为避免表格插入行、列大量重复代码 </summary>
        private bool TableInsertRC(InsertProcEventHandler aProc)
        {
            bool Result = false;
            int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
            int vCurItemNo = GetCurItemNo();
            if (Items[vCurItemNo] is HCTableItem)
            {
                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, vCurItemNo, 0);
                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                Result = aProc(Items[vCurItemNo]);
                if (Result)
                {
                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo, 0);
                    Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();
                }

                InitializeMouseField();
            }

            return Result;
        }

        private void InitializeMouseField()
        {
            FMouseLBDowning = false;
            FMouseDownItemNo = -1;
            FMouseDownItemOffset = -1;
            FMouseMoveItemNo = -1;
            FMouseMoveItemOffset = -1;
            FMouseMoveRestrain = false;
            FSelecting = false;
            FDraging = false;
        }

        /// <summary> 划完完成后最后操作位置是否在选中范围起始 </summary>
        private bool IsSelectSeekStart()
        {
            return (FSelectSeekNo == SelectInfo.StartItemNo) && (FSelectSeekOffset == SelectInfo.StartItemOffset);
        }

        private bool SelectByMouseDownShift(ref int AMouseDownItemNo, ref int AMouseDownItemOffset)
        {
            bool Result = true;

            int vSelItemNo = -1, vSelItemOffset = -1;
            if (this.SelectExists())  // 原来就有选中
            {
                if (IsSelectSeekStart())  // 上一次划选完成后是在选中起始
                {
                    if ((AMouseDownItemNo < FSelectSeekNo)
                        || ((AMouseDownItemNo == FSelectSeekNo) && (AMouseDownItemOffset < FSelectSeekOffset)))
                    {
                        vSelItemNo = SelectInfo.EndItemNo;
                        vSelItemOffset = SelectInfo.EndItemOffset;
                        AdjustSelectRange(ref AMouseDownItemNo, ref AMouseDownItemOffset, ref vSelItemNo, ref vSelItemOffset);  // 确定SelectRang
                    }
                    else
                    if (    ((AMouseDownItemNo > FSelectSeekNo) && (AMouseDownItemNo < SelectInfo.EndItemNo))
                         || ((AMouseDownItemNo == FSelectSeekNo) && (AMouseDownItemOffset > FSelectSeekOffset))
                         || ((AMouseDownItemNo == SelectInfo.EndItemNo) && (AMouseDownItemOffset < SelectInfo.EndItemOffset)))
                    {
                        vSelItemNo = SelectInfo.EndItemNo;
                        vSelItemOffset = SelectInfo.EndItemOffset;
                
                        AdjustSelectRange(ref AMouseDownItemNo, ref AMouseDownItemOffset, ref vSelItemNo, ref vSelItemOffset);  // 确定SelectRang
                    }
                    else
                    if ((AMouseDownItemNo > SelectInfo.EndItemNo)
                        || ((AMouseDownItemNo == SelectInfo.EndItemNo) && (AMouseDownItemOffset > SelectInfo.EndItemOffset)))
                    {
                        vSelItemNo = SelectInfo.EndItemNo;
                        vSelItemOffset = SelectInfo.EndItemOffset;

                        AdjustSelectRange(ref vSelItemNo, ref vSelItemOffset, ref AMouseDownItemNo, ref AMouseDownItemOffset);  // 确定SelectRang
                    }
                    else
                        Result = false;
                }
                else  // 划选完成后是在结束
                {
                    if ((AMouseDownItemNo > FSelectSeekNo)
                        || ((AMouseDownItemNo == FSelectSeekNo) && (AMouseDownItemOffset > FSelectSeekOffset)))
                    {
                        vSelItemNo = SelectInfo.StartItemNo;
                        vSelItemOffset = SelectInfo.StartItemOffset;
                        AdjustSelectRange(ref vSelItemNo, ref vSelItemOffset, ref AMouseDownItemNo, ref AMouseDownItemOffset);  // 确定SelectRang
                    }
                    else
                    if (    ((AMouseDownItemNo > SelectInfo.StartItemNo) && (AMouseDownItemNo < FSelectSeekNo))
                         || ((AMouseDownItemNo == FSelectSeekNo) && (AMouseDownItemOffset < FSelectSeekOffset))
                         || ((AMouseDownItemNo == SelectInfo.StartItemNo) && (AMouseDownItemOffset > SelectInfo.StartItemOffset)))
                    {
                        vSelItemNo = SelectInfo.StartItemNo;
                        vSelItemOffset = SelectInfo.StartItemOffset;

                        AdjustSelectRange(ref vSelItemNo, ref vSelItemOffset, ref AMouseDownItemNo, ref AMouseDownItemOffset);  // 确定SelectRang
                    }
                    else
                    if ((AMouseDownItemNo < SelectInfo.StartItemNo)
                        || ((AMouseDownItemNo == SelectInfo.StartItemNo) && (AMouseDownItemOffset < SelectInfo.StartItemOffset)))
                    {
                        vSelItemNo = SelectInfo.StartItemNo;
                        vSelItemOffset = SelectInfo.StartItemOffset;

                        AdjustSelectRange(ref AMouseDownItemNo, ref AMouseDownItemOffset, ref vSelItemNo, ref vSelItemOffset);  // 确定SelectRang
                    }
                    else
                        Result = false;
                }
            }
            else  // 原来没有选中
            if (SelectInfo.StartItemNo >= 0)
            {
                if ((AMouseDownItemNo < SelectInfo.StartItemNo)
                    || ((AMouseDownItemNo == SelectInfo.StartItemNo) && (AMouseDownItemOffset < SelectInfo.StartItemOffset)))
                {
                    vSelItemNo = SelectInfo.StartItemNo;
                    vSelItemOffset = SelectInfo.StartItemOffset;

                    AdjustSelectRange(ref AMouseDownItemNo, ref AMouseDownItemOffset, ref vSelItemNo, ref vSelItemOffset);  // 确定SelectRang
                }
                else
                if ((AMouseDownItemNo > SelectInfo.StartItemNo)
                    || ((AMouseDownItemNo == SelectInfo.StartItemNo) && (AMouseDownItemOffset > SelectInfo.StartItemOffset)))
                {
                    vSelItemNo = SelectInfo.StartItemNo;
                    vSelItemOffset = SelectInfo.StartItemOffset;
                    AdjustSelectRange(ref vSelItemNo, ref vSelItemOffset, ref AMouseDownItemNo, ref AMouseDownItemOffset);  // 确定SelectRang
                }
                else
                    Result = false;
            }

            return Result;
        }

        protected override void DoDrawItemPaintBefor(HCCustomData aData, int aDrawItemNo,
            RECT aDrawRect, int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop,
            int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoDrawItemPaintBefor(aData, aDrawItemNo, aDrawRect, aDataDrawLeft,
                aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);

            if (FOnDrawItemPaintBefor != null)
                FOnDrawItemPaintBefor(aData, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        protected override void DoDrawItemPaintAfter(HCCustomData aData, int aDrawItemNo,
            RECT aDrawRect, int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop,
            int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoDrawItemPaintAfter(aData, aDrawItemNo, aDrawRect, aDataDrawLeft,
                aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);

            if (FOnDrawItemPaintAfter != null)
                FOnDrawItemPaintAfter(aData, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        /// <summary> 准备格式化参数 </summary>
        /// <param name="aStartItemNo">开始格式化的Item</param>
        /// <param name="APrioDItemNo">上一个Item的最后一个DrawItemNo</param>
        /// <param name="aPos">开始格式化位置</param>
        protected virtual void _FormatReadyParam(int aStartItemNo, ref int aPrioDrawItemNo, ref POINT aPos)
        {
            if (aStartItemNo > 0)
            {
                aPrioDrawItemNo = GetItemLastDrawItemNo(aStartItemNo - 1);  // 上一个最后的DItem
                if (Items[aStartItemNo].ParaFirst)
                {
                    aPos.X = 0;
                    aPos.Y = DrawItems[aPrioDrawItemNo].Rect.Bottom;
                }
                else
                {
                    aPos.X = DrawItems[aPrioDrawItemNo].Rect.Right;
                    aPos.Y = DrawItems[aPrioDrawItemNo].Rect.Top;
                }
            }
            else  // 是第一个
            {
                aPrioDrawItemNo = -1;
                aPos.X = 0;
                aPos.Y = 0;
            }
        }

        // Format仅负责格式化Item，ReFormat负责格式化后对后面Item和DrawItem的关联处理
        protected override void _ReFormatData(int aStartItemNo, int aLastItemNo = -1, int aExtraItemCount = 0)
        {
            int vDrawItemCount = DrawItems.Count;
            if (aLastItemNo < 0)
                FormatData(aStartItemNo, aStartItemNo);
            else
                FormatData(aStartItemNo, aLastItemNo);  // 格式化指定范围内的Item
            DrawItems.DeleteFormatMark();
            vDrawItemCount = DrawItems.Count - vDrawItemCount;

            // 计算格式化后段的底部位置变化
            int vLastDrawItemNo = -1;
            if (aLastItemNo < 0)
                vLastDrawItemNo = GetItemLastDrawItemNo(aStartItemNo);
            else
                vLastDrawItemNo = GetItemLastDrawItemNo(aLastItemNo);
            int vFormatIncHight = DrawItems[vLastDrawItemNo].Rect.Bottom - DrawItems.FormatBeforBottom;  // 段格式化后，高度的增量

            // 某段格式化后，处理对其后面Item对应DrawItem的影响
            // 由图2017-6-8_1变为图2017-6-8_2的过程中，第3段位置没变，也没有新的Item数量变化，
            // 但是DrawItem的数量有变化
            // 第3段Item对应的FirstDItemNo需要修改，所以此处增加DrawItemCount数量的变化
            // 目前格式化时ALastItemNo为段的最后一个，所以vLastDrawItemNo为段最后一个DrawItem
            if ((vFormatIncHight != 0) || (aExtraItemCount != 0) || (vDrawItemCount != 0))
            {
                if (DrawItems.Count > vLastDrawItemNo)
                {
                    int vLastItemNo = -1, vFmtTopOffset = -1;;
                    for (int i = vLastDrawItemNo + 1; i < DrawItems.Count; i++)  // 从格式化变动段的下一段开始
                    {
                        // 处理格式化后面各DrawItem对应的ItemNo偏移
                        DrawItems[i].ItemNo = DrawItems[i].ItemNo + aExtraItemCount;
                        if (vLastItemNo != DrawItems[i].ItemNo)
                        {
                            vLastItemNo = DrawItems[i].ItemNo;
                            Items[vLastItemNo].FirstDItemNo = i;
                        }

                        if (vFormatIncHight != 0)
                        {
                            // 将原格式化因分页等原因引起的整体下移或增加的高度恢复回来
                            // 如果不考虑上面处理ItemNo的偏移，可将TTableCellData.ClearFormatExtraHeight方法写到基类，这里直接调用
                            
                            if (DrawItems[i].LineFirst)
                                vFmtTopOffset = DrawItems[i - 1].Rect.Bottom - DrawItems[i].Rect.Top;

                            User.OffsetRect(ref DrawItems[i].Rect, 0, vFmtTopOffset);

                            if (Items[DrawItems[i].ItemNo].StyleNo < HCStyle.Null)
                            {
                                int vClearFmtHeight = (Items[DrawItems[i].ItemNo] as HCCustomRectItem).ClearFormatExtraHeight();
                                DrawItems[i].Rect.Bottom = DrawItems[i].Rect.Bottom - vClearFmtHeight;
                            }
                        }
                    }
                }
            }
        }

        protected override HCCustomItem CreateItemByStyle(int aStyleNo)
        {
            HCCustomItem Result = null;
            if (aStyleNo < HCStyle.Null)
            {
                switch (aStyleNo)
                {
                    case HCStyle.Image:
                        Result = new HCImageItem(this, 0, 0);
                        break;

                    case HCStyle.Table:
                        Result = new HCTableItem(this, 1, 1, 1);
                        break;

                    case HCStyle.Tab:
                        Result = new HCTabItem(this, 0, 0);
                        break;

                    case HCStyle.Line:
                        Result = new HCLineItem(this, 1, 1);
                        break;

                    case HCStyle.Express:
                        Result = new HCExpressItem(this, "", "", "", "");
                        break;

                    case HCStyle.Domain:
                        Result = CreateDefaultDomainItem();
                        break;

                    case HCStyle.PageBreak:
                        Result = new HCPageBreakItem(this, 0, 1);
                        break;

                    case HCStyle.CheckBox:
                        Result = new HCCheckBoxItem(this, "勾选框", false);
                        break;

                    case HCStyle.Gif:
                        Result = new HCGifItem(this, 1, 1);
                        break;

                    case HCStyle.Edit:
                        Result = new HCEditItem(this, "");
                        break;

                    case HCStyle.Combobox:
                        Result = new HCComboboxItem(this, "");
                        break;

                    case HCStyle.QRCode:
                        Result = new HCQRCodeItem(this, "");
                        break;

                    case HCStyle.BarCode:
                        Result = new HCBarCodeItem(this, "");
                        break;

                    case HCStyle.Fraction:
                        Result = new HCFractionItem(this, "", "");
                        break;

                    case HCStyle.DateTimePicker:
                        Result = new HCDateTimePicker(this, DateTime.Now);
                        break;

                    case HCStyle.RadioGroup:
                        Result = new HCRadioGroup(this);
                        break;

                    case HCStyle.SupSubScript:
                        Result = new HCSupSubScriptItem(this, "", "");
                        break;

                    default:
                        throw new Exception("未找到类型 " + aStyleNo.ToString() + " 对应的创建Item代码！");
                }
            }
            else
            {
                Result = CreateDefaultTextItem();
                Result.StyleNo = aStyleNo;
            }

            if (FOnCreateItem != null)
                FOnCreateItem(Result, null);

            return Result;
        }

        // <summary> 设置光标位置到指定的Item最后面 </summary>
        protected void ReSetSelectAndCaret(int aItemNo)
        {
            ReSetSelectAndCaret(aItemNo, GetItemAfterOffset(aItemNo));
        }

        /// <summary> 设置光标位置到指定的Item指定位置 </summary>
        /// <param name="AItemNo">指定ItemNo</param>
        /// <param name="AOffset">指定位置</param>
        /// <param name="ANextWhenMid">如果此位置前后的DrawItem正好分行，True：后一个DrawItem前面，False：前一个后面</param>
        protected void ReSetSelectAndCaret(int aItemNo, int aOffset, bool aNextWhenMid = false)
        {
            SelectInfo.StartItemNo = aItemNo;

            int vOffset = -1;
            if (Items[aItemNo].StyleNo > HCStyle.Null)
            {
                if (SelectInfo.StartItemOffset > Items[aItemNo].Length)
                    vOffset = Items[aItemNo].Length;
                else
                    vOffset = aOffset;
            }
            else
                vOffset = aOffset;

            SelectInfo.StartItemOffset = vOffset;

            int vDrawItemNo = GetDrawItemNoByOffset(aItemNo, aOffset);
            if ((aNextWhenMid)
                    && (vDrawItemNo < DrawItems.Count - 1)
                    && (DrawItems[vDrawItemNo + 1].ItemNo == aItemNo)
                    && (DrawItems[vDrawItemNo + 1].CharOffs == aOffset + 1))
                vDrawItemNo++;

            CaretDrawItemNo = vDrawItemNo;
        }

        /// <summary> 当前Item对应的格式化起始Item和结束Item(段最后一个Item) </summary>
        /// <param name="aFirstItemNo">起始ItemNo</param>
        /// <param name="aLastItemNo">结束ItemNo</param>
        protected void GetReformatItemRange(ref int aFirstItemNo, ref int aLastItemNo)
        {
            GetReformatItemRange(ref aFirstItemNo, ref aLastItemNo, SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
        }

        /// <summary> 指定Item对应的格式化起始Item和结束Item(段最后一个Item) </summary>
        /// <param name="aFirstItemNo">起始ItemNo</param>
        /// <param name="aLastItemNo">结束ItemNo</param>
        protected void GetReformatItemRange(ref int aFirstItemNo, ref int aLastItemNo, int aItemNo, int aItemOffset)
        {
            if ((aItemNo > 0)
                && DrawItems[Items[aItemNo].FirstDItemNo].LineFirst
                && (aItemOffset == 0))
            {
                if (!Items[aItemNo].ParaFirst)
                    aFirstItemNo = GetLineFirstItemNo(aItemNo - 1, Items[aItemNo - 1].Length);
                else  // 是段首
                    aFirstItemNo = aItemNo;
            }
            else
                aFirstItemNo = GetLineFirstItemNo(aItemNo, 0);  // 取行第一个DrawItem对应的ItemNo

            aLastItemNo = GetParaLastItemNo(aItemNo);
        }

        /// <summary>
        /// 合并2个文本Item
        /// </summary>
        /// <param name="aDestItem">合并后的Item</param>
        /// <param name="aSrcItem">源Item</param>
        /// <returns>True:合并成功，False不能合并</returns>
        protected virtual bool MergeItemText(HCCustomItem aDestItem, HCCustomItem aSrcItem)
        {
            bool Result = aDestItem.CanConcatItems(aSrcItem);
            if (Result)
                aDestItem.Text += aSrcItem.Text;
            return Result;
        }

        protected virtual bool CanDeleteItem(int aItemNo)
        {
            return CanEdit();
        }

        /// <summary> 用于从流加载完Items后，检查不合格的Item并删除 </summary>
        protected virtual int CheckInsertItemCount(int aStartNo, int aEndNo)
        {
            return aEndNo - aStartNo + 1;  // 默认原数返回
        }

        protected virtual void DoItemMouseLeave(int aItemNo)
        {
            Items[aItemNo].MouseLeave();
        }

        protected virtual void DoItemMouseEnter(int aItemNo)
        {
            Items[aItemNo].MouseEnter();
        }

        protected void DoItemResized(int aItemNo)
        {
            if (FOnItemResized != null)
                FOnItemResized(this, aItemNo);
        }

        #region DoTextItemInsert 在文本Item前后或中间插入文
        private bool DoTextItemInsert(int vCarteItemNo, string AText, ref int vFormatFirstItemNo, ref int vFormatLastItemNo)
        {
            bool Result = false;
            HCTextItem vTextItem = Items[vCarteItemNo] as HCTextItem;
            if (vTextItem.StyleNo == this.Style.CurStyleNo)
            {
                if (vTextItem.CanAccept(SelectInfo.StartItemOffset))
                {
                    Undo_New();
                    UndoAction_InsertText(vCarteItemNo, SelectInfo.StartItemOffset + 1, AText);
                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                    int vLen = -1;
                    if (SelectInfo.StartItemOffset == 0)
                    {
                        vTextItem.Text = AText + vTextItem.Text;
                        vLen = AText.Length;
                    }
                    else
                        if (SelectInfo.StartItemOffset == vTextItem.Length)
                        {
                            vTextItem.Text = vTextItem.Text + AText;
                            vLen = vTextItem.Length;
                        }
                        else  // 在Item中间
                        {
                            vLen = SelectInfo.StartItemOffset + AText.Length;
                            string vS = vTextItem.Text;
                            vS = vS.Insert(SelectInfo.StartItemOffset, AText);
                            vTextItem.Text = vS;
                        }

                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                    ReSetSelectAndCaret(vCarteItemNo, vLen);
                    Result = true;
                }
                else  // 此位置不可接受输入
                {
                    if ((SelectInfo.StartItemOffset == 0)
                        || (SelectInfo.StartItemOffset == vTextItem.Length))   // 在首尾不可接受时，插入到前后位置
                    {
                        HCCustomItem vNewItem = CreateDefaultTextItem();
                        vNewItem.Text = AText;

                        if (SelectInfo.StartItemOffset == 0)
                            Result = InsertItem(vCarteItemNo, vNewItem, true);
                        else
                            Result = InsertItem(vCarteItemNo + 1, vNewItem, false);
                    }
                }
            }
            else  // 插入位置TextItem样式和当前样式不同，在TextItem头、中、尾没选中，但应用了新样式，以新样式处理
            {
                HCCustomItem vNewItem = CreateDefaultTextItem();
                vNewItem.Text = AText;
                Result = InsertItem(vNewItem);
            }

            return Result;
        }
        #endregion

        protected bool DoInsertText(string aText)
        {
            bool Result = false;
            if (aText != "")
            {
                int vCarteItemNo = GetCurItemNo();
                HCCustomItem vCarteItem = Items[vCarteItemNo];
                int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                if (vCarteItem.StyleNo < HCStyle.Null)
                {
                    if (SelectInfo.StartItemOffset == HC.OffsetInner)
                    {
                        Undo_New();
                        UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);

                        HCCustomRectItem vRectItem = vCarteItem as HCCustomRectItem;
                        Result = vRectItem.InsertText(aText);
                        if (vRectItem.SizeChanged)
                        {
                            vRectItem.SizeChanged = false;
                            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                            _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                        }
                    }
                    else  // 其后或其前
                        if (SelectInfo.StartItemOffset == HC.OffsetAfter)  // 在其后输入内容
                        {
                            if ((vCarteItemNo < Items.Count - 1)
                                && (Items[vCarteItemNo + 1].StyleNo > HCStyle.Null)
                                && (!Items[vCarteItemNo + 1].ParaFirst))  // 下一个是TextItem且不是段首，则合并到下一个开始
                            {
                                vCarteItemNo++;
                                SelectInfo.StartItemNo = vCarteItemNo;
                                SelectInfo.StartItemOffset = 0;
                                this.Style.CurStyleNo = Items[vCarteItemNo].StyleNo;
                                Result = DoTextItemInsert(vCarteItemNo, aText, ref vFormatFirstItemNo, ref vFormatLastItemNo);  // 在下一个TextItem前面插入
                            }
                            else  // 最后或下一个还是RectItem或当前是段尾
                            {
                                HCCustomItem vNewItem = CreateDefaultTextItem();
                                vNewItem.Text = aText;
                                SelectInfo.StartItemNo = vCarteItemNo + 1;
                                Result = InsertItem(SelectInfo.StartItemNo, vNewItem, false);  // 在两个RectItem中间插入
                            }
                        }
                        else  // 在其前输入内容
                        {
                            if ((vCarteItemNo > 0)
                                && (Items[vCarteItemNo - 1].StyleNo > HCStyle.Null)
                                && (!Items[vCarteItemNo].ParaFirst))  // 前一个是TextItem，当前不是段首，合并到前一个尾
                            {
                                vCarteItemNo--;
                                SelectInfo.StartItemNo = vCarteItemNo;
                                SelectInfo.StartItemOffset = Items[vCarteItemNo].Length;
                                this.Style.CurStyleNo = Items[vCarteItemNo].StyleNo;
                                Result = DoTextItemInsert(vCarteItemNo, aText, ref vFormatFirstItemNo, ref vFormatLastItemNo);  // 在前一个后面插入
                            }
                            else  // 最前或前一个还是RectItem
                            {
                                HCCustomItem vNewItem = CreateDefaultTextItem();
                                vNewItem.Text = aText;
                                Result = InsertItem(SelectInfo.StartItemNo, vNewItem, true);  // 在两个RectItem中间插入
                            }
                        }
                }
                else
                    Result = DoTextItemInsert(vCarteItemNo, aText, ref vFormatFirstItemNo, ref vFormatLastItemNo);
            }
            else
                Result = InsertBreak();

            return Result;
        }

        protected virtual int GetWidth()
        {
            return FWidth;
        }

        protected void SetWidth(int value)
        {
            if (FWidth != value)
                FWidth = value;
        }

        protected virtual int GetHeight()
        {
            return CalcContentHeight();
        }

        protected virtual void SetReadOnly(bool value)
        {
            FReadOnly = value;
        }

        protected int CalcContentHeight()
        {
            if (DrawItems.Count > 0)
                return DrawItems[DrawItems.Count - 1].Rect.Bottom - DrawItems[0].Rect.Top;
            else
                return 0;
        }

        protected int MouseMoveDrawItemNo
        {
            get { return FMouseMoveDrawItemNo; }
        }

        public HCRichData(HCStyle aStyle) : base(aStyle)
        {
            FBatchInsertCount = 0;
            FReadOnly = false;
            InitializeField();
            SetEmptyData();
        }

        public override void Clear()
        {
            InitializeField();

            base.Clear();
            SetEmptyData();
        }

        // 选中内容应用样式

        #region MergeItemToPrio 当前Item成功合并到同段前一个Item
        private bool MergeItemToPrio(int aItemNo)
        {
            return (aItemNo > 0) && (!Items[aItemNo].ParaFirst) && MergeItemText(Items[aItemNo - 1], Items[aItemNo]);
        }
        #endregion

        #region MergeItemToNext 同段后一个Item成功合并到当前Item
        private bool MergeItemToNext(int aItemNo)
        {
            return (aItemNo < Items.Count - 1) && (!Items[aItemNo + 1].ParaFirst) && MergeItemText(Items[aItemNo], Items[aItemNo + 1]);
        }
        #endregion

        #region ApplySameItem选中在同一个Item
        private void ApplySameItem(int aItemNo, ref int vExtraCount, HCStyleMatch aMatchStyle)
        {
            HCCustomItem vItem = Items[aItemNo];
            if (vItem.StyleNo < HCStyle.Null)
            {
                if ((vItem as HCCustomRectItem).MangerUndo)
                    UndoAction_ItemSelf(aItemNo, HC.OffsetInner);
                else
                    UndoAction_ItemMirror(aItemNo, HC.OffsetInner);

                (vItem as HCCustomRectItem).ApplySelectTextStyle(Style, aMatchStyle);
            }
            else  // 文本
            {
                int vStyleNo = aMatchStyle.GetMatchStyleNo(Style, vItem.StyleNo);
                Style.CurStyleNo = vStyleNo;
                if (vItem.IsSelectComplate)
                {
                    UndoAction_ItemStyle(aItemNo, SelectInfo.EndItemOffset, vStyleNo);
                    vItem.StyleNo = vStyleNo;  // 直接修改样式编号

                    if (MergeItemToNext(aItemNo))
                    {
                        UndoAction_InsertText(aItemNo, Items[aItemNo].Length - Items[aItemNo + 1].Length + 1, Items[aItemNo + 1].Text);
                        UndoAction_DeleteItem(aItemNo + 1, 0);
                        Items.RemoveAt(aItemNo + 1);
                        vExtraCount--;
                    }

                    if (aItemNo > 0)
                    {
                        int vLen = Items[aItemNo - 1].Length;
                        if (MergeItemToPrio(aItemNo))
                        {
                            UndoAction_InsertText(aItemNo - 1, Items[aItemNo - 1].Length - Items[aItemNo].Length + 1, Items[aItemNo].Text);
                            UndoAction_DeleteItem(aItemNo, 0);
                            Items.RemoveAt(aItemNo);
                            vExtraCount--;

                            SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                            SelectInfo.StartItemOffset = vLen;
                            SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                            SelectInfo.EndItemOffset = vLen + SelectInfo.EndItemOffset;
                        }
                    }
                }
                else  // Item一部分被选中
                {
                    string vText = vItem.Text;
                    string vSelText = vText.Substring(SelectInfo.StartItemOffset + 1 - 1,  // 选中的文本
                        SelectInfo.EndItemOffset - SelectInfo.StartItemOffset);
                    string vsBefor = vText.Substring(1 - 1, SelectInfo.StartItemOffset);  // 前半部分文本
                    HCCustomItem vAfterItem = Items[aItemNo].BreakByOffset(SelectInfo.EndItemOffset);  // 后半部分对应的Item
                    if (vAfterItem != null)
                    {
                        UndoAction_DeleteText(aItemNo, SelectInfo.EndItemOffset + 1, vAfterItem.Text);

                        Items.Insert(aItemNo + 1, vAfterItem);
                        UndoAction_InsertItem(aItemNo + 1, 0);
                        vExtraCount++;
                    }

                    if (vsBefor != "")
                    {
                        UndoAction_DeleteText(aItemNo, SelectInfo.StartItemOffset + 1, vSelText);
                        vItem.Text = vsBefor;  // 保留前半部分文本

                        // 创建选中文本对应的Item
                        HCCustomItem vSelItem = CreateDefaultTextItem();
                        vSelItem.ParaNo = vItem.ParaNo;
                        vSelItem.StyleNo = vStyleNo;
                        vSelItem.Text = vSelText;

                        if (vAfterItem != null)
                        {
                            Items.Insert(aItemNo + 1, vSelItem);
                            UndoAction_InsertItem(aItemNo + 1, 0);
                            vExtraCount++;
                        }
                        else  // 没有后半部分，说明选中需要和后面判断合并
                        {
                            if ((aItemNo < Items.Count - 1)
                                && (!Items[aItemNo + 1].ParaFirst)
                                && MergeItemText(vSelItem, Items[aItemNo + 1]))
                            {
                                UndoAction_InsertText(aItemNo + 1, 1, vSelText);
                                Items[aItemNo + 1].Text = vSelText + Items[aItemNo + 1].Text;
                                vSelItem.Dispose();

                                SelectInfo.StartItemNo = aItemNo + 1;
                                SelectInfo.StartItemOffset = 0;
                                SelectInfo.EndItemNo = aItemNo + 1;
                                SelectInfo.EndItemOffset = vSelText.Length;

                                return;
                            }

                            Items.Insert(aItemNo + 1, vSelItem);
                            UndoAction_InsertItem(aItemNo + 1, 0);
                            vExtraCount++;
                        }

                        SelectInfo.StartItemNo = aItemNo + 1;
                        SelectInfo.StartItemOffset = 0;
                        SelectInfo.EndItemNo = aItemNo + 1;
                        SelectInfo.EndItemOffset = vSelText.Length;
                    }
                    else  // 选择起始位置是Item最开始
                    {
                        //vItem.Text := vSelText;  // BreakByOffset已经保留选中部分文本
                        UndoAction_ItemStyle(aItemNo, SelectInfo.EndItemOffset, vStyleNo);
                        vItem.StyleNo = vStyleNo;

                        if (MergeItemToPrio(aItemNo))
                        {
                            UndoAction_InsertText(aItemNo - 1, Items[aItemNo - 1].Length - Items[aItemNo].Length + 1, Items[aItemNo].Text);
                            UndoAction_DeleteItem(aItemNo, 0);
                            Items.RemoveAt(aItemNo);
                            vExtraCount--;

                            SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                            int vLen = Items[SelectInfo.StartItemNo].Length;
                            SelectInfo.StartItemOffset = vLen - vSelText.Length;
                            SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                            SelectInfo.EndItemOffset = vLen;
                        }
                    }
                }
            }
        }
        #endregion

        #region ApplyRangeStartItem选中在不同Item中，处理选中起始Item
        private void ApplyRangeStartItem(int aItemNo, ref int vExtraCount, HCStyleMatch aMatchStyle)
        {
            HCCustomItem vItem = Items[aItemNo];
            if (vItem.StyleNo < HCStyle.Null)
            {
                if ((vItem as HCCustomRectItem).MangerUndo)
                    UndoAction_ItemSelf(aItemNo, SelectInfo.StartItemOffset);
                else
                    UndoAction_ItemMirror(aItemNo, SelectInfo.StartItemOffset);

                (vItem as HCCustomRectItem).ApplySelectTextStyle(Style, aMatchStyle);
            }
            else  // 文本
            {
                int vStyleNo = aMatchStyle.GetMatchStyleNo(Style, vItem.StyleNo);
                if (vItem.StyleNo != vStyleNo)
                {
                    if (vItem.IsSelectComplate)
                    {
                        UndoAction_ItemStyle(aItemNo, 0, vStyleNo);
                        vItem.StyleNo = vStyleNo;
                    }
                    else  // Item部分选中
                    {
                        HCCustomItem vAfterItem = Items[aItemNo].BreakByOffset(SelectInfo.StartItemOffset);  // 后半部分对应的Item
                        UndoAction_DeleteText(aItemNo, SelectInfo.StartItemOffset + 1, vAfterItem.Text);
                        
                        Items.Insert(aItemNo + 1, vAfterItem);
                        UndoAction_InsertItem(aItemNo + 1, 0);

                        UndoAction_ItemStyle(aItemNo + 1, 0, vStyleNo);
                        vAfterItem.StyleNo = vStyleNo;
                        vExtraCount++;

                        SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                        SelectInfo.StartItemOffset = 0;
                        SelectInfo.EndItemNo = SelectInfo.EndItemNo + 1;
                    }
                }
            }
        }
        #endregion

        #region ApplyRangeEndItem选中在不同Item中，处理选中结束Item
        private void ApplyRangeEndItem(int aItemNo, ref int vExtraCount, HCStyleMatch aMatchStyle)
        {
            HCCustomItem vItem = Items[aItemNo];
            if (vItem.StyleNo < HCStyle.Null)
            {
                if ((vItem as HCCustomRectItem).MangerUndo)
                    UndoAction_ItemSelf(aItemNo, SelectInfo.EndItemOffset);
                else
                    UndoAction_ItemMirror(aItemNo, SelectInfo.EndItemOffset);

                (vItem as HCCustomRectItem).ApplySelectTextStyle(Style, aMatchStyle);
            }
            else  // 文本
            {
                int vStyleNo = aMatchStyle.GetMatchStyleNo(Style, vItem.StyleNo);
                if (vItem.StyleNo != vStyleNo)
                {
                    if (vItem.IsSelectComplate)
                    {
                        UndoAction_ItemStyle(aItemNo, SelectInfo.EndItemOffset, vStyleNo);
                        vItem.StyleNo = vStyleNo;
                    }
                    else  // Item部分选中了
                    {
                        string vText = vItem.Text;
                        string vSelText = vText.Substring(1 - 1, SelectInfo.EndItemOffset); // 选中的文本
                        UndoAction_DeleteBackText(aItemNo, 1, vSelText);
                        vItem.Text = vText.Remove(1 - 1, SelectInfo.EndItemOffset); ;

                        HCCustomItem vBeforItem = CreateDefaultTextItem();
                        vBeforItem.ParaNo = vItem.ParaNo;
                        vBeforItem.StyleNo = vStyleNo;
                        vBeforItem.Text = vSelText;  // 创建前半部分文本对应的Item
                        vBeforItem.ParaFirst = vItem.ParaFirst;
                        vItem.ParaFirst = false;

                        Items.Insert(aItemNo, vBeforItem);
                        UndoAction_InsertItem(aItemNo, 0);
                        vExtraCount++;
                    }
                }
            }
        }
        #endregion

        #region ApplyNorItem选中在不同Item，处理中间Item
        private void ApplyRangeNorItem(int aItemNo, HCStyleMatch aMatchStyle)
        {
            HCCustomItem vItem = Items[aItemNo];
            if (vItem.StyleNo < HCStyle.Null)  // 非文本
            {
                if ((vItem as HCCustomRectItem).MangerUndo)
                    UndoAction_ItemSelf(aItemNo, HC.OffsetInner);
                else
                    UndoAction_ItemMirror(aItemNo, HC.OffsetInner);

                (vItem as HCCustomRectItem).ApplySelectTextStyle(Style, aMatchStyle);
            }
            else  // 文本
            {
                int vNewStyleNo = aMatchStyle.GetMatchStyleNo(Style, vItem.StyleNo);
                UndoAction_ItemStyle(aItemNo, 0, vNewStyleNo);
                vItem.StyleNo = vNewStyleNo;
            }
        }
        #endregion

        public override int ApplySelectTextStyle(HCStyleMatch aMatchStyle)
        {
            Undo_New();

            this.InitializeField();
            int vExtraCount = 0, vFormatFirstItemNo = -1, vFormatLastItemNo = -1;

            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
            if (!SelectExists())
            {
                if (Style.CurStyleNo > HCStyle.Null)
                {
                    aMatchStyle.Append = !aMatchStyle.StyleHasMatch(Style, Style.CurStyleNo);  // 根据当前判断是添加样式还是减掉样式
                    Style.CurStyleNo = aMatchStyle.GetMatchStyleNo(Style, Style.CurStyleNo);

                    Style.UpdateInfoRePaint();
                    if (Items[SelectInfo.StartItemNo].Length == 0)
                    {
                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                        UndoAction_ItemStyle(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, Style.CurStyleNo);
                        Items[SelectInfo.StartItemNo].StyleNo = Style.CurStyleNo;

                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                        Style.UpdateInfoReCaret();
                    }
                    else  // 不是空行
                    {
                        if (Items[SelectInfo.StartItemNo] is HCTextRectItem)
                        {
                            _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                            (Items[SelectInfo.StartItemNo] as HCTextRectItem).TextStyleNo = Style.CurStyleNo;
                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                        }

                        Style.UpdateInfoReCaret(false);
                    }
                }

                ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                return HCStyle.Null;
            }

            if (SelectInfo.EndItemNo < 0)
            {
                if (Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null)
                {
                    // 如果改变会引起RectItem宽度变化，则需要格式化到最后一个Item
                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                    if ((Items[SelectInfo.StartItemNo] as HCCustomRectItem).MangerUndo)
                        UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                    else
                        UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                    (Items[SelectInfo.StartItemNo] as HCCustomRectItem).ApplySelectTextStyle(Style, aMatchStyle);
                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                }
            }
            else  // 有连续选中内容
            {
                vFormatLastItemNo = GetParaLastItemNo(SelectInfo.EndItemNo);
                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)
                {
                    if (Items[i].StyleNo > HCStyle.Null)
                    {
                        aMatchStyle.Append = !aMatchStyle.StyleHasMatch(Style, Items[i].StyleNo);  // 根据第一个判断是添加样式还是减掉样式
                        break;
                    }
                    else
                        if (Items[i] is HCTextRectItem)
                        {
                            aMatchStyle.Append = !aMatchStyle.StyleHasMatch(Style, (Items[i] as HCTextRectItem).TextStyleNo);  // 根据第一个判断是添加样式还是减掉样式
                            break;
                        }
                }

                if (SelectInfo.StartItemNo == SelectInfo.EndItemNo)
                    ApplySameItem(SelectInfo.StartItemNo, ref vExtraCount, aMatchStyle);
                else  // 选中发生在不同的Item，采用先处理选中范围内样式改变，再处理合并，再处理选中内容全、部分选中状态
                {
                    ApplyRangeEndItem(SelectInfo.EndItemNo, ref vExtraCount, aMatchStyle);
                    for (int i = SelectInfo.EndItemNo - 1; i >= SelectInfo.StartItemNo + 1; i--)
                        ApplyRangeNorItem(i, aMatchStyle);  // 处理每一个Item的样式
                    ApplyRangeStartItem(SelectInfo.StartItemNo, ref vExtraCount, aMatchStyle);

                    /* 样式变化后，从后往前处理选中范围内变化后的合并 }*/
                    if (SelectInfo.EndItemNo < vFormatLastItemNo + vExtraCount)
                    {
                        if (MergeItemToNext(SelectInfo.EndItemNo))
                        {
                            UndoAction_InsertText(SelectInfo.EndItemNo,
                                Items[SelectInfo.EndItemNo].Length - Items[SelectInfo.EndItemNo + 1].Length + 1,
                                Items[SelectInfo.EndItemNo + 1].Text);
                            UndoAction_DeleteItem(SelectInfo.EndItemNo + 1, 0);
                            Items.RemoveAt(SelectInfo.EndItemNo + 1);
                            vExtraCount--;
                        }
                    }

                    int vLen = -1;
                    for (int i = SelectInfo.EndItemNo; i >= SelectInfo.StartItemNo + 1; i--)
                    {
                        vLen = Items[i - 1].Length;
                        if (MergeItemToPrio(i))
                        {
                            UndoAction_InsertText(i - 1, Items[i - 1].Length - Items[i].Length + 1, Items[i].Text);
                            UndoAction_DeleteItem(i, 0);
                            Items.RemoveAt(i);
                            vExtraCount--;

                            if (i == SelectInfo.EndItemNo)
                                SelectInfo.EndItemOffset = SelectInfo.EndItemOffset + vLen;
                            SelectInfo.EndItemNo = SelectInfo.EndItemNo - 1;
                        }
                    }

                    // 起始范围
                    if ((SelectInfo.StartItemNo > 0) && (!Items[SelectInfo.StartItemNo].ParaFirst))
                    {
                        vLen = Items[SelectInfo.StartItemNo - 1].Length;
                        if (MergeItemToPrio(SelectInfo.StartItemNo))
                        {
                            UndoAction_InsertText(SelectInfo.StartItemNo - 1,
                                Items[SelectInfo.StartItemNo - 1].Length - Items[SelectInfo.StartItemNo].Length + 1,
                                Items[SelectInfo.StartItemNo].Text);
                            UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                            Items.RemoveAt(SelectInfo.StartItemNo);
                            vExtraCount--;

                            SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                            SelectInfo.StartItemOffset = vLen;
                            SelectInfo.EndItemNo = SelectInfo.EndItemNo - 1;
                            if (SelectInfo.StartItemNo == SelectInfo.EndItemNo)
                                SelectInfo.EndItemOffset = SelectInfo.EndItemOffset + vLen;
                        }
                    }
                }

                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo + vExtraCount, vExtraCount);
            }

            MatchItemSelectState();
            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();

            return 0;
        }

        #region DoApplyParaStyle
        private void DoApplyParaStyle(int aItemNo, HCParaMatch aMatchStyle)
        {
            if (GetItemStyle(aItemNo) < HCStyle.Null)
                (Items[aItemNo] as HCCustomRectItem).ApplySelectParaStyle(this.Style, aMatchStyle);
            else
            {
                int vFirstNo = -1, vLastNo = -1;
                GetParaItemRang(aItemNo, ref vFirstNo, ref vLastNo);
                int vParaNo = aMatchStyle.GetMatchParaNo(this.Style, GetItemParaStyle(aItemNo));
                if (GetItemParaStyle(vFirstNo) != vParaNo)
                {
                    for (int i = vFirstNo; i <= vLastNo; i++)
                        Items[i].ParaNo = vParaNo;
                }
            }
        }
        #endregion

        #region ApplyParaSelectedRangStyle
        private void ApplyParaSelectedRangStyle(HCParaMatch aMatchStyle)
        {
            int vFirstNo = -1, vLastNo = -1;
            GetParaItemRang(SelectInfo.StartItemNo, ref vFirstNo, ref vLastNo);
            DoApplyParaStyle(SelectInfo.StartItemNo, aMatchStyle);

            int i = vLastNo + 1;
            while (i <= SelectInfo.EndItemNo)
            {
                if (Items[i].ParaFirst)
                    DoApplyParaStyle(i, aMatchStyle);

                i++;
            }
        }
        #endregion

        public override int ApplySelectParaStyle(HCParaMatch aMatchStyle)
        {
            if (SelectInfo.StartItemNo < 0)
                return HCStyle.Null;

            //GetReformatItemRange(vFormatFirstItemNo, vFormatLastItemNo);
            int vFormatLastItemNo = -1;
            int vFormatFirstItemNo = GetParaFirstItemNo(SelectInfo.StartItemNo);

            if (SelectInfo.EndItemNo >= 0)
            {
                vFormatLastItemNo = GetParaLastItemNo(SelectInfo.EndItemNo);
                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                ApplyParaSelectedRangStyle(aMatchStyle);
                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
            }
            else  // 没有选中内容
            {
                vFormatLastItemNo = GetParaLastItemNo(SelectInfo.StartItemNo);
                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                DoApplyParaStyle(SelectInfo.StartItemNo, aMatchStyle);  // 应用样式
                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
            }
            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();

            return 0;
        }

        public override bool DisSelect()
        {
            bool Result = base.DisSelect();
            if (Result)
            {
                // 拖拽完成时清除
                FDraging = false;  // 拖拽完成
                FSelecting = false;  // 准备划选
                Style.UpdateInfoRePaint();
            }
            Style.UpdateInfoReCaret();  // 选择起始信息被重置为-1

            return Result;
        }

        /// <summary> 删除选中内容(内部已经判断了是否有选中) </summary>
        /// <returns>True:有选中且删除成功</returns>

        #region DeleteItemSelectComplate删除全选中的单个Item
        private bool DeleteItemSelectComplate(ref int vDelCount, int vParaFirstItemNo, int vParaLastItemNo)
        {
            if (CanDeleteItem(SelectInfo.StartItemNo))
            {
                UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                Items.RemoveAt(SelectInfo.StartItemNo);

                vDelCount++;

                int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                if ((SelectInfo.StartItemNo > vFormatFirstItemNo)
                    && (SelectInfo.StartItemNo < vFormatLastItemNo))
                {
                    int vLen = Items[SelectInfo.StartItemNo - 1].Length;
                    if (MergeItemText(Items[SelectInfo.StartItemNo - 1], Items[SelectInfo.StartItemNo]))
                    {
                        Items.RemoveAt(SelectInfo.StartItemNo);
                        vDelCount++;

                        SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                        SelectInfo.StartItemOffset = vLen;
                    }
                    else  // 删除位置前后不能合并，光标置为前一个后面
                    {
                        SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                        SelectInfo.StartItemOffset = GetItemAfterOffset(SelectInfo.StartItemNo);
                    }
                }
                else
                    if (SelectInfo.StartItemNo == vParaFirstItemNo)
                    {
                        if (vParaFirstItemNo == vParaLastItemNo)
                        {
                            HCCustomItem vNewItem = CreateDefaultTextItem();
                            vNewItem.ParaFirst = true;
                            Items.Insert(SelectInfo.StartItemNo, vNewItem);
                            UndoAction_InsertItem(SelectInfo.StartItemNo, 0);

                            SelectInfo.StartItemOffset = 0;
                            vDelCount--;
                        }
                        else
                        {
                            SelectInfo.StartItemOffset = 0;
                            Items[SelectInfo.StartItemNo].ParaFirst = true;
                        }
                    }
                    else
                        if (SelectInfo.StartItemNo == vParaLastItemNo)
                        {
                            SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                            SelectInfo.StartItemOffset = Items[SelectInfo.StartItemNo].Length;
                        }
                        else  // 全选中的Item是起始格式化或结束格式化或在段内
                        {
                            if (SelectInfo.StartItemNo > 0)
                            {
                                int vLen = Items[SelectInfo.StartItemNo - 1].Length;
                                if (MergeItemText(Items[SelectInfo.StartItemNo - 1], Items[SelectInfo.StartItemNo]))
                                {
                                    Items.RemoveAt(SelectInfo.StartItemNo);
                                    vDelCount++;
                                    SelectInfo.StartItemOffset = vLen;
                                }

                                SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                            }
                        }
            }

            return true;
        }
        #endregion

        public override bool DeleteSelected()
        {
            bool Result = false;
            if (!CanEdit())
                return Result;

            if (SelectExists())
            {
                bool vSelectSeekStart = IsSelectSeekStart();

                int vDelCount = 0;
                this.InitializeField();  // 删除后原鼠标处可能已经没有了

                int vFormatFirstItemNo = -1, vFormatLastItemNo = -1,
                    vParaFirstItemNo = -1, vParaLastItemNo = -1;

                if ((SelectInfo.EndItemNo < 0)
                    && (Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null))
                {
                    // 如果变动会引起RectItem的宽度变化，则需要格式化到段最后一个Item
                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                    Undo_New();

                    if ((Items[SelectInfo.StartItemNo] as HCCustomRectItem).IsSelectComplateTheory())
                    {
                        GetParaItemRang(SelectInfo.StartItemNo, ref vParaFirstItemNo, ref vParaLastItemNo);
                        Result = DeleteItemSelectComplate(ref vDelCount, vParaFirstItemNo, vParaLastItemNo);
                    }
                    else
                    {
                        if ((Items[SelectInfo.StartItemNo] as HCCustomRectItem).MangerUndo)
                            UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                        else
                            UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);
                        Result = (Items[SelectInfo.StartItemNo] as HCCustomRectItem).DeleteSelected();
                    }

                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
                }
                else  // 选中不是发生在RectItem内部
                {
                    HCCustomItem vEndItem = Items[SelectInfo.EndItemNo];  // 选中结束Item
                    if (SelectInfo.EndItemNo == SelectInfo.StartItemNo)
                    {
                        Undo_New();

                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                        if (vEndItem.IsSelectComplate)
                        {
                            GetParaItemRang(SelectInfo.StartItemNo, ref vParaFirstItemNo, ref vParaLastItemNo);
                            Result = DeleteItemSelectComplate(ref vDelCount, vParaFirstItemNo, vParaLastItemNo);
                        }
                        else  // Item部分选中
                        {
                            if (vEndItem.StyleNo < HCStyle.Null)
                            {
                                if ((Items[SelectInfo.StartItemNo] as HCCustomRectItem).MangerUndo)
                                    UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                                else
                                    UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                                (vEndItem as HCCustomRectItem).DeleteSelected();
                            }
                            else  // 同一个TextItem
                            {
                                string vText = vEndItem.Text;
                                UndoAction_DeleteBackText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1,
                                    vText.Substring(SelectInfo.StartItemOffset + 1 - 1, SelectInfo.EndItemOffset - SelectInfo.StartItemOffset));

                                vEndItem.Text = vText.Remove(SelectInfo.StartItemOffset + 1 - 1, SelectInfo.EndItemOffset - SelectInfo.StartItemOffset);
                            }
                        }

                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
                    }
                    else  // 选中发生在不同Item，起始(可能是段首)全选中结尾没全选，起始没全选结尾全选，起始结尾都没全选
                    {
                        vFormatFirstItemNo = GetParaFirstItemNo(SelectInfo.StartItemNo);  // 取段第一个为起始
                        vFormatLastItemNo = GetParaLastItemNo(SelectInfo.EndItemNo);  // 取段最后一个为结束，如果变更注意下面

                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                        bool vSelStartParaFirst = Items[SelectInfo.StartItemNo].ParaFirst;
                        bool vSelStartComplate = Items[SelectInfo.StartItemNo].IsSelectComplate;  // 起始是否全选
                        bool vSelEndComplate = Items[SelectInfo.EndItemNo].IsSelectComplate;  // 结尾是否全选

                        Undo_New();

                        // 先处理选中结束Item
                        if (vEndItem.StyleNo < HCStyle.Null)
                        {
                            if (vSelEndComplate)
                            {
                                if (CanDeleteItem(SelectInfo.EndItemNo))
                                {
                                    UndoAction_DeleteItem(SelectInfo.EndItemNo, HC.OffsetAfter);
                                    Items.RemoveAt(SelectInfo.EndItemNo);
                                    vDelCount++;
                                }
                            }
                            else
                                if (SelectInfo.EndItemOffset == HC.OffsetInner)
                                    (vEndItem as HCCustomRectItem).DeleteSelected();
                        }
                        else  // TextItem
                        {
                            if (vSelEndComplate)
                            {
                                if (CanDeleteItem(SelectInfo.EndItemNo))
                                {
                                    UndoAction_DeleteItem(SelectInfo.EndItemNo, vEndItem.Length);
                                    Items.RemoveAt(SelectInfo.EndItemNo);
                                    vDelCount++;
                                }
                            }
                            else  // 文本且不在选中结束Item最后
                            {
                                UndoAction_DeleteBackText(SelectInfo.EndItemNo, 1, vEndItem.Text.Substring(1 - 1, SelectInfo.EndItemOffset));
                                // 结束Item留下的内容
                                string vText = (vEndItem as HCTextItem).GetTextPart(SelectInfo.EndItemOffset + 1,
                                    vEndItem.Length - SelectInfo.EndItemOffset);
                                vEndItem.Text = vText;
                            }
                        }

                        // 删除选中起始Item下一个到结束Item上一个
                        for (int i = SelectInfo.EndItemNo - 1; i >= SelectInfo.StartItemNo + 1; i--)
                        {
                            if (CanDeleteItem(i))
                            {
                                UndoAction_DeleteItem(i, 0);
                                Items.RemoveAt(i);
                                vDelCount++;
                            }
                        }

                        HCCustomItem vStartItem = Items[SelectInfo.StartItemNo];  // 选中起始Item
                        if (vStartItem.StyleNo < HCStyle.Null)
                        {
                            if (SelectInfo.StartItemOffset == HC.OffsetBefor)
                            {
                                if (CanDeleteItem(SelectInfo.StartItemNo))
                                {
                                    UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                    Items.RemoveAt(SelectInfo.StartItemNo);
                                    vDelCount++;
                                }
                                if (SelectInfo.StartItemNo > vFormatFirstItemNo)
                                    SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                            }
                            else
                                if (SelectInfo.StartItemOffset == HC.OffsetInner)
                                    (vStartItem as HCCustomRectItem).DeleteSelected();
                        }
                        else  // 选中起始是TextItem
                        {
                            if (vSelStartComplate)
                            {
                                if (CanDeleteItem(SelectInfo.StartItemNo))
                                {
                                    UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                    Items.RemoveAt(SelectInfo.StartItemNo);
                                    vDelCount++;
                                }
                            }
                            else
                            //if SelectInfo.StartItemOffset < vStartItem.Length then  // 在中间(不用判断了吧？)
                            {
                                UndoAction_DeleteBackText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1,
                                    vStartItem.Text.Substring(SelectInfo.StartItemOffset + 1 - 1, vStartItem.Length - SelectInfo.StartItemOffset));
                                string vText = (vStartItem as HCTextItem).GetTextPart(1, SelectInfo.StartItemOffset);
                                vStartItem.Text = vText;  // 起始留下的内容
                            }
                        }

                        if (vSelStartComplate && vSelEndComplate)
                        {
                            if (SelectInfo.StartItemNo == vFormatFirstItemNo)
                            {
                                if (SelectInfo.EndItemNo == vFormatLastItemNo)
                                {
                                    HCCustomItem vNewItem = CreateDefaultTextItem();
                                    vNewItem.ParaFirst = true;
                                    Items.Insert(SelectInfo.StartItemNo, vNewItem);
                                    UndoAction_InsertItem(SelectInfo.StartItemNo, vNewItem.Length);

                                    vDelCount--;
                                }
                                else  // 选中结束不在段最后
                                    Items[SelectInfo.EndItemNo - vDelCount + 1].ParaFirst = true;  // 选中结束位置后面的成为段首
                            }
                            else
                                if (SelectInfo.EndItemNo == vFormatLastItemNo)
                                {
                                    SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                                    SelectInfo.StartItemOffset = GetItemAfterOffset(SelectInfo.StartItemNo);
                                }
                                else  // 选中起始在起始段中间，选中结束在结束段中间
                                {
                                    int vLen = Items[SelectInfo.StartItemNo - 1].Length;
                                    if (MergeItemText(Items[SelectInfo.StartItemNo - 1], Items[SelectInfo.EndItemNo - vDelCount + 1]))
                                    {
                                        UndoAction_InsertText(SelectInfo.StartItemNo - 1,
                                            Items[SelectInfo.StartItemNo - 1].Length - Items[SelectInfo.EndItemNo - vDelCount + 1].Length + 1,
                                            Items[SelectInfo.EndItemNo - vDelCount + 1].Text);
                                        
                                        SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                                        SelectInfo.StartItemOffset = vLen;

                                        UndoAction_DeleteItem(SelectInfo.EndItemNo - vDelCount + 1, 0);
                                        Items.RemoveAt(SelectInfo.EndItemNo - vDelCount + 1);
                                        vDelCount++;
                                    }
                                    else  // 起始前面和结束后面不能合并，如果选中起始和结束不在同一段
                                    {
                                        if (Items[SelectInfo.EndItemNo - vDelCount + 1].ParaFirst)
                                        {
                                            UndoAction_ItemParaFirst(SelectInfo.EndItemNo - vDelCount + 1, 0, false);
                                            Items[SelectInfo.EndItemNo - vDelCount + 1].ParaFirst = false;  // 合并不成功就挨着
                                        }
                                    }
                                }
                        }
                        else  // 选中范围内的Item没有删除完
                        {
                            if (vSelStartComplate)
                            {
                                if (Items[SelectInfo.EndItemNo - vDelCount].ParaFirst != vSelStartParaFirst)
                                {
                                    UndoAction_ItemParaFirst(SelectInfo.EndItemNo - vDelCount, 0, vSelStartParaFirst);
                                    Items[SelectInfo.EndItemNo - vDelCount].ParaFirst = vSelStartParaFirst;
                                }
                            }
                            else
                                if (!vSelEndComplate)
                                {
                                    if (MergeItemText(Items[SelectInfo.StartItemNo], Items[SelectInfo.EndItemNo - vDelCount]))
                                    {
                                        UndoAction_InsertText(SelectInfo.StartItemNo,
                                            Items[SelectInfo.StartItemNo].Length - Items[SelectInfo.EndItemNo - vDelCount].Length + 1,
                                            Items[SelectInfo.EndItemNo - vDelCount].Text);

                                        UndoAction_DeleteItem(SelectInfo.EndItemNo - vDelCount, 0);
                                        Items.RemoveAt(SelectInfo.EndItemNo - vDelCount);
                                        vDelCount++;
                                    }
                                    else  // 选中起始、结束位置的Item不能合并
                                    {
                                        if (SelectInfo.EndItemNo != vFormatLastItemNo)
                                        {
                                            if (Items[SelectInfo.EndItemNo - vDelCount].ParaFirst)
                                            {
                                                UndoAction_ItemParaFirst(SelectInfo.EndItemNo - vDelCount, 0, false);
                                                Items[SelectInfo.EndItemNo - vDelCount].ParaFirst = false;  // 合并不成功就挨着
                                            }
                                        }
                                    }
                                }
                        }

                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
                    }

                    for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo - vDelCount; i++)  // 不允许删除的取消选中状态
                        Items[i].DisSelect();

                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
                }

                Style.UpdateInfoRePaint();
                Style.UpdateInfoReCaret();

                base.DeleteSelected();

                ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, !vSelectSeekStart);
                Result = true;
            }

            return Result;
        }

        /// <summary> 在光标处插入Item </summary>
        /// <param name="aItem"></param>
        /// <returns></returns>
        public virtual bool InsertItem(HCCustomItem aItem)
        {
            bool Result = false;
            if (!CanEdit())
                return Result;

            DeleteSelected();

            aItem.ParaNo = Style.CurParaNo;

            if (IsEmptyData())
            {
                Undo_New();
                Result = EmptyDataInsertItem(aItem);
                return Result;
            }
            int vCurItemNo = GetCurItemNo();

            int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
            if (Items[vCurItemNo].StyleNo < HCStyle.Null)
            {
                if (SelectInfo.StartItemOffset == HC.OffsetInner)
                {
                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                    Undo_New();
                    UndoAction_ItemSelf(vCurItemNo, HC.OffsetInner);
                    Result = (Items[vCurItemNo] as HCCustomRectItem).InsertItem(aItem);
                    if (Result)
                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo, 0);
                }
                else  // 其前or其后
                {
                    if (SelectInfo.StartItemOffset == HC.OffsetBefor)
                        Result = InsertItem(SelectInfo.StartItemNo, aItem);
                    else  // 其后
                        Result = InsertItem(SelectInfo.StartItemNo + 1, aItem, false);
                }
            }
            else  // 当前位置是TextItem
            {
                // 先判断是否在后面，这样对于空行插入时从后面插入，否则会造成空行向后积压
                if ((SelectInfo.StartItemOffset == Items[vCurItemNo].Length))
                    Result = InsertItem(SelectInfo.StartItemNo + 1, aItem, false);
                else
                    if (SelectInfo.StartItemOffset == 0)
                        Result = InsertItem(SelectInfo.StartItemNo, aItem);
                    else  // 在Item中间
                    {
                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                        string vText = Items[vCurItemNo].Text;
                        string vsBefor = vText.Substring(1 - 1, SelectInfo.StartItemOffset);  // 前半部分文本
                        string vsAfter = vText.Substring(SelectInfo.StartItemOffset + 1 - 1, Items[vCurItemNo].Length - SelectInfo.StartItemOffset);  // 后半部分文本

                        Undo_New();
                        if (Items[vCurItemNo].CanConcatItems(aItem))
                        {
                            if (aItem.ParaFirst)
                            {
                                UndoAction_DeleteBackText(vCurItemNo, SelectInfo.StartItemOffset + 1, vsAfter);
                                Items[vCurItemNo].Text = vsBefor;
                                aItem.Text = aItem.Text + vsAfter;

                                vCurItemNo = vCurItemNo + 1;
                                Items.Insert(vCurItemNo, aItem);
                                UndoAction_InsertItem(vCurItemNo, 0);

                                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);
                                ReSetSelectAndCaret(vCurItemNo);
                            }
                            else  // 同一段中插入
                            {
                                UndoAction_InsertText(vCurItemNo, SelectInfo.StartItemOffset + 1, aItem.Text);
                                vsBefor = vsBefor + aItem.Text;
                                Items[vCurItemNo].Text = vsBefor + vsAfter;

                                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo, 0);
                                SelectInfo.StartItemNo = vCurItemNo;
                                SelectInfo.StartItemOffset = vsBefor.Length;
                                //CaretDrawItemNo := GetItemLastDrawItemNo(vCurItemNo);
                            }
                        }
                        else  // 不能合并
                        {
                            UndoAction_DeleteBackText(vCurItemNo, SelectInfo.StartItemOffset + 1, vsAfter);
                            HCCustomItem vAfterItem = Items[vCurItemNo].BreakByOffset(SelectInfo.StartItemOffset);  // 后半部分对应的Item

                            // 插入后半部分对应的Item
                            vCurItemNo = vCurItemNo + 1;
                            Items.Insert(vCurItemNo, vAfterItem);
                            UndoAction_InsertItem(vCurItemNo, 0);

                            // 插入新Item
                            Items.Insert(vCurItemNo, aItem);
                            UndoAction_InsertItem(vCurItemNo, 0);

                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo + 2, 2);
                            ReSetSelectAndCaret(vCurItemNo);
                        }

                        Result = true;
                    }
            }

            return Result;
        }

        /// <summary> 在指定的位置插入Item </summary>
        /// <param name="aIndex">插入位置</param>
        /// <param name="aItem">插入的Item</param>
        /// <param name="aOffsetBefor">插入时在原位置Item前面(True)或后面(False)</param>
        /// <returns></returns>
        public virtual bool InsertItem(int aIndex, HCCustomItem aItem, bool aOffsetBefor = true)
        {
            if (!CanEdit())
                return false;

            aItem.ParaNo = Style.CurParaNo;

            if (IsEmptyData())
            {
                Undo_New();
                return EmptyDataInsertItem(aItem);
            }

            int vIncCount = 0, vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
            Undo_New();
            if (aItem.StyleNo < HCStyle.Null)
            {
                int vInsPos = aIndex;
                if (aIndex < Items.Count)
                {
                    if (aOffsetBefor)
                    {
                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, aIndex, 0);
                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                        if ((Items[aIndex].StyleNo > HCStyle.Null) && (Items[aIndex].Text == ""))
                        {
                            aItem.ParaFirst = true;
                            UndoAction_DeleteItem(aIndex, 0);
                            Items.RemoveAt(aIndex);
                            vIncCount--;
                        }
                        else  // 插入位置不是空行
                            if (!aItem.ParaFirst)
                            {
                                aItem.ParaFirst = Items[aIndex].ParaFirst;
                                if (Items[aIndex].ParaFirst)
                                {
                                    UndoAction_ItemParaFirst(aIndex, 0, false);
                                    Items[aIndex].ParaFirst = false;
                                }
                            }
                    }
                    else  // 在某Item后面插入
                    {
                        if ((aIndex > 0)
                            && (Items[aIndex - 1].StyleNo > HCStyle.Null)
                            && (Items[aIndex - 1].Text == ""))
                        {
                            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, aIndex - 1, 0);
                            _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                            aItem.ParaFirst = true;
                            UndoAction_DeleteItem(aIndex - 1, 0);
                            Items.RemoveAt(aIndex - 1);
                            vIncCount--;
                            vInsPos--;
                        }
                        else  // 插入位置前一个不是空行
                        {
                            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, aIndex, 0);
                            _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                        }
                    }
                }
                else  // 在末尾添加一个新Item
                {
                    vFormatFirstItemNo = aIndex - 1;
                    vFormatLastItemNo = aIndex - 1;
                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                    if ((!aItem.ParaFirst)  // 插入不是另起一段)
                        && (Items[aIndex - 1].StyleNo > HCStyle.Null)  // 前面是TextItem
                        && (Items[aIndex - 1].Text == "")) // 空行
                    {
                        aItem.ParaFirst = true;
                        UndoAction_DeleteItem(aIndex - 1, 0);
                        Items.RemoveAt(aIndex - 1);
                        vIncCount--;
                        vInsPos--;
                    }
                }

                Items.Insert(vInsPos, aItem);
                UndoAction_InsertItem(vInsPos, HC.OffsetAfter);
                vIncCount++;

                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo + vIncCount, vIncCount);
                ReSetSelectAndCaret(vInsPos);
            }
            else  // 插入文本Item
            {
                bool vMerged = false;
                if (aIndex > 0)
                {
                    if (aIndex < Items.Count)
                    {
                        if (!Items[aIndex].ParaFirst)
                            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, aIndex - 1, 0);
                        else
                            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, aIndex, 0);
                    }
                    else  // 在最后面追加
                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, aIndex - 1, 0);
                }
                else
                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, 0, 0);

                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                if (!aItem.ParaFirst)
                {
                    // 在2个Item中间插入一个Item，需要同时判断和前后能否合并
                    if (aOffsetBefor)
                    {
                        if ((aIndex < Items.Count) && (Items[aIndex].CanConcatItems(aItem)))
                        {
                            UndoAction_InsertText(aIndex, 1, aItem.Text);  // 201806261644
                            Items[aIndex].Text = aItem.Text + Items[aIndex].Text;

                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo, 0);
                            ReSetSelectAndCaret(aIndex);

                            vMerged = true;
                        }
                        else
                            if ((!Items[aIndex].ParaFirst) && (aIndex > 0) && Items[aIndex - 1].CanConcatItems(aItem))
                            {
                                UndoAction_InsertText(aIndex - 1, Items[aIndex - 1].Length + 1, aItem.Text);  // 201806261650
                                Items[aIndex - 1].Text = Items[aIndex - 1].Text + aItem.Text;

                                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo, 0);
                                ReSetSelectAndCaret(aIndex - 1);

                                vMerged = true;
                            }
                    }
                    else  // 在Item后面插入，未指定另起一段，在Item后面插入AIndex肯定是大于0
                    {
                        if ((Items[aIndex - 1].StyleNo > HCStyle.Null) && (Items[aIndex - 1].Text == ""))  // 在空行后插入不换行，替换空行
                        {
                            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, aIndex - 1, 0);
                            _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                            aItem.ParaFirst = true;
                            Items.Insert(aIndex, aItem);
                            UndoAction_InsertItem(aIndex, 0);

                            UndoAction_DeleteItem(aIndex - 1, 0);
                            Items.RemoveAt(aIndex - 1);

                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo, 0);
                            ReSetSelectAndCaret(aIndex - 1);

                            vMerged = true;
                        }
                        else
                        if (Items[aIndex - 1].CanConcatItems(aItem))  // 先判断和前一个能否合并
                        {
                            // 能合并，重新获取前一个的格式化信息
                            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, aIndex - 1, 0);
                            _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                            UndoAction_InsertText(aIndex - 1, Items[aIndex - 1].Length + 1, aItem.Text);  // 201806261650
                            Items[aIndex - 1].Text = Items[aIndex - 1].Text + aItem.Text;

                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo, 0);
                            ReSetSelectAndCaret(aIndex - 1);

                            vMerged = true;
                        }
                        else
                            if ((aIndex < Items.Count) && (!Items[aIndex].ParaFirst) && (Items[aIndex].CanConcatItems(aItem)))
                            {
                                UndoAction_InsertText(aIndex, 1, aItem.Text);  // 201806261644
                                Items[aIndex].Text = aItem.Text + Items[aIndex].Text;

                                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo, 0);
                                ReSetSelectAndCaret(aIndex, aItem.Length);

                                vMerged = true;
                            }
                    }
                }

                if (!vMerged)
                {
                    if (aOffsetBefor && (!aItem.ParaFirst))
                    {
                        aItem.ParaFirst = Items[aIndex].ParaFirst;
                        if (Items[aIndex].ParaFirst)
                        {
                            UndoAction_ItemParaFirst(aIndex, 0, false);
                            Items[aIndex].ParaFirst = false;
                        }
                    }

                    Items.Insert(aIndex, aItem);
                    UndoAction_InsertItem(aIndex, 0);
                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);

                    ReSetSelectAndCaret(aIndex);
                }
            }

            return true;
        }

        public virtual void KillFocus()
        {
            int vItemNo = GetCurItemNo();
            if (vItemNo > 0)
                Items[vItemNo].KillFocus();
        }

        #region DoItemMouseDown
        private void DoItemMouseDown(int aItemNo, int aOffset, MouseEventArgs e)
        {
            if (aItemNo < 0)
                return;

            int vX = -1, vY = -1;
            CoordToItemOffset(e.X, e.Y, aItemNo, aOffset, ref vX, ref vY);

            MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
            Items[aItemNo].MouseDown(vMouseArgs);

            if (FOnItemMouseDown != null)
                FOnItemMouseDown(this, aItemNo, vMouseArgs);
        }
        #endregion

        public virtual void MouseDown(MouseEventArgs e)
        {
            FSelecting = false;  // 准备划选
            FDraging = false;  // 准备拖拽
            FMouseLBDouble = false;
            FMouseDownReCaret = false;
            //FSelectSeekOffset = -1;

            FMouseLBDowning = (e.Button == MouseButtons.Left);
            
            FMouseDownX = e.X;
            FMouseDownY = e.Y;

            int vMouseDownItemNo = -1, vMouseDownItemOffset = -1, vDrawItemNo = -1;
            bool vRestrain = false;
            GetItemAt(e.X, e.Y, ref vMouseDownItemNo, ref vMouseDownItemOffset, ref vDrawItemNo, ref vRestrain);

            if ((e.Button == MouseButtons.Left) && ((Control.ModifierKeys & Keys.Shift) == Keys.Shift))  // shift键重新确定选中范围
            {
                if (SelectByMouseDownShift(ref vMouseDownItemNo, ref  vMouseDownItemOffset))
                {
                    MatchItemSelectState();  // 设置选中范围内的Item选中状态
                    Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();

                    FMouseDownItemNo = vMouseDownItemNo;
                    FMouseDownItemOffset = vMouseDownItemOffset;
                    FSelectSeekNo = vMouseDownItemNo;
                    FSelectSeekOffset = vMouseDownItemOffset;

                    if ((!vRestrain) && (Items[FMouseDownItemNo].StyleNo < HCStyle.Null))  // RectItem
                        DoItemMouseDown(FMouseDownItemNo, FMouseDownItemOffset, e);

                    return;
                }
            }

            bool vMouseDownInSelect = CoordInSelect(e.X, e.Y, vMouseDownItemNo, vMouseDownItemOffset, vRestrain);

            if (vMouseDownInSelect)
            {
                if (FMouseLBDowning)
                {
                    FDraging = true;
                    Style.UpdateInfo.Draging = true;
                }

                if (Items[vMouseDownItemNo].StyleNo < HCStyle.Null)
                    DoItemMouseDown(vMouseDownItemNo, vMouseDownItemOffset, e);
            }
            else  // 没点在选中区域中
            {
                if (SelectInfo.StartItemNo >= 0)
                {
                    if (Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null)
                        (Items[SelectInfo.StartItemNo] as HCCustomRectItem).DisSelect();

                    Style.UpdateInfoRePaint();  // 旧的去焦点，新的入焦点
                }

                if ((vMouseDownItemNo != FMouseDownItemNo)
                    || (vMouseDownItemOffset != FMouseDownItemOffset)
                    || (CaretDrawItemNo != vDrawItemNo))  // 位置发生变化
                {
                    Style.UpdateInfoReCaret();
                    FMouseDownReCaret = true;

                    DisSelect();

                    // 重新赋值新位置
                    FMouseDownItemNo = vMouseDownItemNo;
                    FMouseDownItemOffset = vMouseDownItemOffset;

                    SelectInfo.StartItemNo = FMouseDownItemNo;
                    SelectInfo.StartItemOffset = FMouseDownItemOffset;
                    CaretDrawItemNo = vDrawItemNo;
                }

                //if not vRestrain then  // 没收敛，因跨页Item点击在前后位置时需要处理光标数据所以不能限制
                DoItemMouseDown(FMouseDownItemNo, FMouseDownItemOffset, e);
            }
        }

        #region AdjustSelectRange
        /// <summary> 给定起始结束位置，判断正确的选中位置并修正输出 </summary>
        /// <param name="ADrawItemNo">光标处的DrawItem(暂时无意义)</param>
        /// <param name="AStartItemNo"></param>
        /// <param name="AStartItemOffset"></param>
        /// <param name="AEndItemNo"></param>
        /// <param name="AEndItemNoOffset"></param>
        private void AdjustSelectRange(ref int aStartItemNo, ref int aStartItemOffset, ref int aEndItemNo, ref int aEndItemNoOffset)
        {
            bool vLeftToRight = false;
            // 记录原来选中范围
            int vOldStartItemNo = SelectInfo.StartItemNo;
            int vOldEndItemNo = SelectInfo.EndItemNo;

            if (aStartItemNo < aEndItemNo)
            {
                vLeftToRight = true;

                if (aStartItemOffset == GetItemAfterOffset(aStartItemNo))
                {
                    if (aStartItemNo < Items.Count - 1)
                    {
                        aStartItemNo = aStartItemNo + 1;
                        aStartItemOffset = 0;
                    }
                }

                if ((aStartItemNo != aEndItemNo) && (aEndItemNo >= 0) && (aEndItemNoOffset == 0))
                {
                    Items[aEndItemNo].DisSelect();  // 从前往后选，鼠标移动到前一次前面，原鼠标处被移出选中范围

                    aEndItemNo = aEndItemNo - 1;
                    aEndItemNoOffset = GetItemAfterOffset(aEndItemNo);
                }
            }
            else
                if (aEndItemNo < aStartItemNo)
                {
                    vLeftToRight = false;

                    if ((aStartItemNo > 0) && (aStartItemOffset == 0))
                    {
                        aStartItemNo = aStartItemNo - 1;
                        aStartItemOffset = GetItemAfterOffset(aStartItemNo);
                    }

                    if ((aStartItemNo != aEndItemNo) && (aEndItemNoOffset == GetItemAfterOffset(aEndItemNo)))
                    {
                        Items[aEndItemNo].DisSelect();  // 从后往前选，鼠标移动到前一个后面，原鼠标处被移出选中范围

                        if (aEndItemNo < Items.Count - 1)
                        {
                            aEndItemNo = aEndItemNo + 1;
                            aEndItemNoOffset = 0;
                        }
                    }
                }

            if (aStartItemNo == aEndItemNo)
            {
                if (aEndItemNoOffset > aStartItemOffset)
                {
                    if (Items[aStartItemNo].StyleNo < HCStyle.Null)
                    {
                        SelectInfo.StartItemNo = aStartItemNo;
                        SelectInfo.StartItemOffset = aStartItemOffset;
                        if ((aStartItemOffset == HC.OffsetBefor) && (aEndItemNoOffset == HC.OffsetAfter))
                        {
                            SelectInfo.EndItemNo = aEndItemNo;
                            SelectInfo.EndItemOffset = aEndItemNoOffset;
                        }
                        else  // 没有全选中
                        {
                            SelectInfo.EndItemNo = -1;
                            SelectInfo.EndItemOffset = -1;
                            //CaretDrawItemNo = vMoveDrawItemNo;
                        }
                    }
                    else  // TextItem
                    {
                        SelectInfo.StartItemNo = aStartItemNo;
                        SelectInfo.StartItemOffset = aStartItemOffset;
                        SelectInfo.EndItemNo = aStartItemNo;
                        SelectInfo.EndItemOffset = aEndItemNoOffset;
                    }
                }
                else
                    if (aEndItemNoOffset < aStartItemOffset)
                    {
                        if (Items[aStartItemNo].StyleNo < HCStyle.Null)
                        {
                            if (aEndItemNoOffset == HC.OffsetBefor)
                            {
                                SelectInfo.StartItemNo = aStartItemNo;
                                SelectInfo.StartItemOffset = aEndItemNoOffset;
                                SelectInfo.EndItemNo = aStartItemNo;
                                SelectInfo.EndItemOffset = aStartItemOffset;
                            }
                            else  // 从后往前选到OffsetInner了
                            {
                                SelectInfo.StartItemNo = aStartItemNo;
                                SelectInfo.StartItemOffset = aStartItemOffset;
                                SelectInfo.EndItemNo = -1;
                                SelectInfo.EndItemOffset = -1;
                            }
                        }
                        else  // TextItem
                        {
                            SelectInfo.StartItemNo = aEndItemNo;
                            SelectInfo.StartItemOffset = aEndItemNoOffset;
                            SelectInfo.EndItemNo = aEndItemNo;
                            SelectInfo.EndItemOffset = aStartItemOffset;
                        }
                    }
                    else  // 结束位置和起始位置相同(同一个Item)
                    {
                        if (SelectInfo.EndItemNo >= 0)
                            Items[SelectInfo.EndItemNo].DisSelect();

                        SelectInfo.StartItemNo = aStartItemNo;
                        SelectInfo.StartItemOffset = aStartItemOffset;
                        SelectInfo.EndItemNo = -1;
                        SelectInfo.EndItemOffset = -1;
                    }
            }
            else  // 选择操作不在同一个Item
            {
                if (vLeftToRight)
                {
                    SelectInfo.StartItemNo = aStartItemNo;
                    SelectInfo.StartItemOffset = aStartItemOffset;
                    SelectInfo.EndItemNo = aEndItemNo;
                    SelectInfo.EndItemOffset = aEndItemNoOffset;
                }
                else
                {
                    SelectInfo.StartItemNo = aEndItemNo;
                    SelectInfo.StartItemOffset = aEndItemNoOffset;
                    SelectInfo.EndItemNo = aStartItemNo;
                    SelectInfo.EndItemOffset = aStartItemOffset;
                }
            }

            // 新选中范围外的清除选中
            if (vOldStartItemNo >= 0)
            {
                if (vOldStartItemNo > SelectInfo.StartItemNo)
                {
                    for (int i = vOldStartItemNo; i >= SelectInfo.StartItemNo + 1; i--)
                        Items[i].DisSelect();
                }
                else
                {
                    for (int i = vOldStartItemNo; i <= SelectInfo.StartItemNo - 1; i++)
                        Items[i].DisSelect();
                }
            }

            if (SelectInfo.EndItemNo < 0)
            {
                for (int i = vOldEndItemNo; i >= SelectInfo.StartItemNo + 1; i--)  // 当前后面的取消选中
                    Items[i].DisSelect();
            }
            else  // 有选中结束
            {
                for (int i = vOldEndItemNo; i >= SelectInfo.EndItemNo + 1; i--)  // 原结束倒序到现结束下一个的取消选中
                    Items[i].DisSelect();
            }
        }
        #endregion

        #region
        private void DoItemMouseMove(int aItemNo, int aOffset, MouseEventArgs e)
        {
            if (aItemNo < 0)
                return;

            int vX = -1, vY = -1;
            CoordToItemOffset(e.X, e.Y, aItemNo, aOffset, ref vX, ref vY);

            MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
            Items[aItemNo].MouseMove(vMouseArgs);
        }
        #endregion

        public virtual void MouseMove(MouseEventArgs e)
        {
            if (SelectedResizing())
            {
                FMouseMoveItemNo = FMouseDownItemNo;
                FMouseMoveItemOffset = FMouseDownItemOffset;
                FMouseMoveRestrain = false;
                DoItemMouseMove(FMouseMoveItemNo, FMouseMoveItemOffset, e);
                Style.UpdateInfoRePaint();

                return;
            }

            //vOldMouseMoveItemOffset := FMouseMoveItemOffset;

            int vMouseMoveItemNo = -1, vMouseMoveItemOffset = -1, vMoveDrawItemNo = -1;
            bool vRestrain = false;
            GetItemAt(e.X, e.Y, ref vMouseMoveItemNo, ref vMouseMoveItemOffset, ref vMoveDrawItemNo, ref vRestrain);

            if (FDraging || Style.UpdateInfo.Draging)
            {
                HC.GCursor = Cursors.Arrow;  // crDrag
                FMouseMoveItemNo = vMouseMoveItemNo;
                FMouseMoveItemOffset = vMouseMoveItemOffset;
                FMouseMoveRestrain = vRestrain;
                CaretDrawItemNo = vMoveDrawItemNo;

                Style.UpdateInfoReCaret();

                if ((!vRestrain) && (Items[FMouseMoveItemNo].StyleNo < HCStyle.Null))
                    DoItemMouseMove(FMouseMoveItemNo, FMouseMoveItemOffset, e);
            }
            else
                if (FSelecting)  // 划选
                {
                    if ((Items[FMouseDownItemNo].StyleNo < HCStyle.Null)
                        && (FMouseDownItemOffset == HC.OffsetInner))
                    {
                        FMouseMoveItemNo = FMouseDownItemNo;
                        FMouseMoveItemOffset = FMouseDownItemOffset;

                        if (vMouseMoveItemNo == FMouseDownItemNo)  // 在按下的RectItem上移动
                            FMouseMoveRestrain = vRestrain;
                        else  // 都视为约束
                            FMouseMoveRestrain = true;
                    }
                    else
                    {
                        FMouseMoveItemNo = vMouseMoveItemNo;
                        FMouseMoveItemOffset = vMouseMoveItemOffset;
                        FMouseMoveRestrain = vRestrain;
                    }

                    AdjustSelectRange(ref FMouseDownItemNo, ref FMouseDownItemOffset,
                      ref FMouseMoveItemNo, ref FMouseMoveItemOffset);  // 确定SelectRang
                    FSelectSeekNo = vMouseMoveItemNo;
                    FSelectSeekOffset = vMouseMoveItemOffset;

                    MatchItemSelectState();  // 设置选中范围内的Item选中状态
                    Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();

                    if ((!vRestrain) && (Items[FMouseMoveItemNo].StyleNo < HCStyle.Null))
                        DoItemMouseMove(FMouseMoveItemNo, FMouseMoveItemOffset, e);
                }
                else  // 非拖拽，非划选
                    if (FMouseLBDowning && ((FMouseDownX != e.X) || (FMouseDownY != e.Y)))
                    {
                        FSelecting = true;
                        Style.UpdateInfo.Selecting = true;
                    }
                    else  // 非拖拽，非划选，非按下
                    {
                        if (vMouseMoveItemNo != FMouseMoveItemNo)
                        {
                            if (FMouseMoveItemNo >= 0)
                                DoItemMouseLeave(FMouseMoveItemNo);
                            if ((vMouseMoveItemNo >= 0) && (!vRestrain))
                                DoItemMouseEnter(vMouseMoveItemNo);

                            Style.UpdateInfoRePaint();
                        }
                        else  // 本次移动到的Item和上一次是同一个(不代表一直在一个Item上移动)
                        {
                            if (vRestrain != FMouseMoveRestrain)
                            {
                                if ((!FMouseMoveRestrain) && vRestrain)
                                {
                                    if (FMouseMoveItemNo >= 0)
                                        DoItemMouseLeave(FMouseMoveItemNo);
                                }
                                else
                                    if (FMouseMoveRestrain && (!vRestrain))
                                    {
                                        if (vMouseMoveItemNo >= 0)
                                            DoItemMouseEnter(vMouseMoveItemNo);
                                    }

                                Style.UpdateInfoRePaint();
                            }
                        }

                        FMouseMoveItemNo = vMouseMoveItemNo;
                        FMouseMoveItemOffset = vMouseMoveItemOffset;
                        FMouseMoveRestrain = vRestrain;

                        if (!vRestrain)
                        {
                            DoItemMouseMove(FMouseMoveItemNo, FMouseMoveItemOffset, e);
                            if ((Control.ModifierKeys & Keys.Control) == Keys.Control
                                && (Items[FMouseMoveItemNo].HyperLink != ""))
                                HC.GCursor = Cursors.Hand;
                        }
                    }
        }

        #region DoItemMouseUp
        private void DoItemMouseUp(int aItemNo, int aOffset, MouseEventArgs e)
        {
            if (aItemNo < 0)
                return;

            int vX = -1, vY = -1;
            CoordToItemOffset(e.X, e.Y, aItemNo, aOffset, ref vX, ref vY);

            MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
            Items[aItemNo].MouseUp(vMouseArgs);

            if (FOnItemMouseUp != null)
                FOnItemMouseUp(this, aItemNo, vMouseArgs);
        }
        #endregion

        #region DoNormalMouseUp
        private void DoNormalMouseUp(int vUpItemNo, int vUpItemOffset, int vDrawItemNo, MouseEventArgs e)
        {
            if (FMouseMoveItemNo < 0)
            {
                SelectInfo.StartItemNo = vUpItemNo;
                SelectInfo.StartItemOffset = vUpItemOffset;
            }
            else
            {
                SelectInfo.StartItemNo = FMouseMoveItemNo;
                SelectInfo.StartItemOffset = FMouseMoveItemOffset;
            }

            CaretDrawItemNo = vDrawItemNo;
            Style.UpdateInfoRePaint();

            if (!FMouseDownReCaret)
                Style.UpdateInfoReCaret();

            //if (Items[vUpItemNo].StyleNo < HCStyle.Null)
            DoItemMouseUp(vUpItemNo, vUpItemOffset, e);  // 弹起，因为可能是移出Item后弹起，所以这里不受vRestrain约束
        }
        #endregion

        public virtual void MouseUp(MouseEventArgs e)
        {
            FMouseLBDowning = false;
            
            if (FMouseLBDouble)
                return;

            if (SelectedResizing())
            {
                Undo_New();
                UndoAction_ItemSelf(FMouseDownItemNo, FMouseDownItemOffset);

                DoItemMouseUp(FMouseDownItemNo, FMouseDownItemOffset, e);
                DoItemResized(FMouseDownItemNo);  // 缩放完成事件(可控制缩放不要超过页面)

                int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, FMouseDownItemNo, FMouseDownItemOffset);

                if ((vFormatFirstItemNo > 0) && (!Items[vFormatFirstItemNo].ParaFirst))
                {
                    vFormatFirstItemNo--;
                    vFormatFirstItemNo = GetLineFirstItemNo(vFormatFirstItemNo, 0);
                }

                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                Style.UpdateInfoRePaint();

                return;
            }

            int vUpItemNo = -1, vUpItemOffset = -1, vDrawItemNo = -1;
            bool vRestrain = false;
            GetItemAt(e.X, e.Y, ref vUpItemNo, ref vUpItemOffset, ref vDrawItemNo, ref vRestrain);

            if (FSelecting || Style.UpdateInfo.Selecting)
            {
                FSelecting = false;
                // 选中范围内的RectItem取消划选状态(此时表格的FSelecting为True)
                //if SelectInfo.StartItemNo >= 0 then
                for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)
                {
                    if ((i != vUpItemNo) && (Items[i].StyleNo < HCStyle.Null))
                        DoItemMouseUp(i, 0, e);
                }

                if (Items[vUpItemNo].StyleNo < HCStyle.Null)
                    DoItemMouseUp(vUpItemNo, vUpItemOffset, e);
            }
            else
                if (FDraging || Style.UpdateInfo.Draging)
                {
                    FDraging = false;
                    bool vMouseUpInSelect = CoordInSelect(e.X, e.Y, vUpItemNo, vUpItemOffset, vRestrain);

                    // 清除弹起位置之外的Item选中状态，弹起处自己处理，弹起处不在选中范围内时
                    // 保证弹起处取消(从ItemA上选中拖拽到另一个ItemB时，ItemA选中状态需要取消)
                    // 与201805172309相似
                    if (SelectInfo.StartItemNo >= 0)
                    {
                        if (SelectInfo.StartItemNo != vUpItemNo)
                        {
                            Items[SelectInfo.StartItemNo].DisSelect();
                            //Items[SelectInfo.StartItemNo].Active := False;
                        }
                        // 选中范围内其他Item取消选中
                        for (int i = SelectInfo.StartItemNo + 1; i <= SelectInfo.EndItemNo; i++)  // 遍历弹起位置之外的其他Item
                        {
                            if (i != vUpItemNo)
                            {
                                Items[i].DisSelect();
                                //Items[i].Active := False;
                            }
                        }
                    }

                    // 为拖拽光标准备
                    FMouseMoveItemNo = vUpItemNo;
                    FMouseMoveItemOffset = vUpItemOffset;
                    // 为下一次点击时清除上一次点击选中做准备
                    FMouseDownItemNo = vUpItemNo;
                    FMouseDownItemOffset = vUpItemOffset;

                    DoNormalMouseUp(vUpItemNo, vUpItemOffset, vDrawItemNo, e);  // 弹起处自己处理Item选中状态，并以弹起处为当前编辑位置

                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
                }
                else  // 非拖拽、非划选
                {
                    if (SelectExists(false))
                        DisSelect();

                    DoNormalMouseUp(vUpItemNo, vUpItemOffset, vDrawItemNo, e);
                }
        }

        public override void ParseXml(XmlElement aNode)
        {
            if (!CanEdit())
                return;

            base.ParseXml(aNode);

            ReFormat(0);
            InitializeMouseField();
            ReSetSelectAndCaret(0, 0);

            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();
            Style.UpdateInfoReScroll();
        }

        public virtual void MouseLeave()
        {
            if (FMouseMoveItemNo >= 0)
            {
                DoItemMouseLeave(FMouseMoveItemNo);
                FMouseMoveItemNo = -1;
                FMouseMoveItemOffset = -1;
                Style.UpdateInfoRePaint();
            }
        }

        // Key返回0表示此键按下Data没有做任何事情
        public virtual void KeyPress(ref Char key)
        {
            if (!CanEdit())
                return;

            DeleteSelected();

            HCCustomItem vCarteItem = GetCurItem();
            if (vCarteItem == null)
                return;

            if ((vCarteItem.StyleNo < HCStyle.Null)  // 当前位置是 RectItem)
                && (SelectInfo.StartItemOffset == HC.OffsetInner)) // 在其上输入内容
            {
                Undo_New();

                HCCustomRectItem vRectItem = vCarteItem as HCCustomRectItem;
                if (vRectItem.MangerUndo)
                    UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                else
                    UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                vRectItem.KeyPress(ref key);
                if (vRectItem.SizeChanged)
                {
                    vRectItem.SizeChanged = false;

                    int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                    if (key != 0)
                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);

                    Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();
                    Style.UpdateInfoReScroll();
                }
            }
            else
                InsertText(key.ToString());
        }

        #region CheckSelectEndEff 判断选择结束是否和起始在同一位置，是则取消选中
        private void CheckSelectEndEff()
        {
            if ((SelectInfo.StartItemNo == SelectInfo.EndItemNo)
                && (SelectInfo.StartItemOffset == SelectInfo.EndItemOffset))
            {
                Items[SelectInfo.EndItemNo].DisSelect();
                SelectInfo.EndItemNo = -1;
                SelectInfo.EndItemOffset = -1;
            }
        }

        private void SetSelectSeekStart()
        {
            FSelectSeekNo = SelectInfo.StartItemNo;
            FSelectSeekOffset = SelectInfo.StartItemOffset;
        }

        private void SetSelectSeekEnd()
        {
            FSelectSeekNo = SelectInfo.EndItemNo;
            FSelectSeekOffset = SelectInfo.EndItemOffset;
        }

        private void TABKeyDown(HCCustomItem vCurItem, KeyEventArgs e)
        {
            HCTabItem vTabItem = new HCTabItem(this);
            if (vCurItem.StyleNo < HCStyle.Null)
            {
                if (SelectInfo.StartItemOffset == HC.OffsetInner)
                {
                    if ((vCurItem as HCCustomRectItem).WantKeyDown(e))
                    {
                        int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                        (vCurItem as HCCustomRectItem).KeyDown(e);
                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                    }
                }
            }
            else  // TextItem
            {
                if ((SelectInfo.StartItemOffset == 0) && Items[SelectInfo.StartItemNo].ParaFirst)  // 段首
                {
                    HCParaStyle vParaStyle = Style.ParaStyles[vCurItem.ParaNo];
                    ApplyParaFirstIndent(vParaStyle.FirstIndent + HCUnitConversion.PixXToMillimeter(HC.TabCharWidth));
                }
                else
                    this.InsertItem(vTabItem);
            }
        }

        private void SelectPrio(ref int AItemNo, ref int AOffset)
        {
            if (AOffset > 0)
            {
                if (Items[AItemNo].StyleNo > HCStyle.Null)
                    AOffset = AOffset - 1;
                else
                    AOffset = HC.OffsetBefor;
            }
            else
                if (AItemNo > 0)
                {
                    Items[AItemNo].DisSelect();
                    AItemNo = AItemNo - 1;
                    if (Items[AItemNo].StyleNo < HCStyle.Null)
                        AOffset = HC.OffsetBefor;
                    else
                        AOffset = Items[AItemNo].Length - 1;  // 倒数第1个前面
                }
        }

        private void SelectStartItemPrio()
        {
            int vItemNo = SelectInfo.StartItemNo;
            int vOffset = SelectInfo.StartItemOffset;
            SelectPrio(ref vItemNo, ref vOffset);
            SelectInfo.StartItemNo = vItemNo;
            SelectInfo.StartItemOffset = vOffset;
        }

        private void SelectEndItemPrio()
        {
            int vItemNo = SelectInfo.EndItemNo;
            int vOffset = SelectInfo.EndItemOffset;
            SelectPrio(ref vItemNo, ref vOffset);
            SelectInfo.EndItemNo = vItemNo;
            SelectInfo.EndItemOffset = vOffset;
        }

        private void LeftKeyDown(bool vSelectExist, KeyEventArgs e)
        {
            if (e.Shift)
            {
                if (SelectInfo.EndItemNo >= 0)
                {
                    if (IsSelectSeekStart())
                    {
                        SelectStartItemPrio();
                        SetSelectSeekStart();
                    }
                    else  // 游标在选中结束
                    {
                        SelectEndItemPrio();
                        SetSelectSeekEnd();
                    }
                }
                else  // 没有选中
                {
                    if ((SelectInfo.StartItemNo > 0) && (SelectInfo.StartItemOffset == 0))
                    {
                        SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                        SelectInfo.StartItemOffset = GetItemAfterOffset(SelectInfo.StartItemNo);
                    }

                    SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                    SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;

                    SelectStartItemPrio();
                    SetSelectSeekStart();
                }

                CheckSelectEndEff();
                MatchItemSelectState();
                Style.UpdateInfoRePaint();
            }
            else  // 没有按下Shift
            {
                if (vSelectExist)
                {
                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
                }
                else  // 无选中内容
                {
                    if (SelectInfo.StartItemOffset != 0)
                        SelectInfo.StartItemOffset = SelectInfo.StartItemOffset - 1;
                    else  // 在Item最开始左方向键
                    {
                        if (SelectInfo.StartItemNo > 0)
                        {
                            SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;  // 上一个
                            SelectInfo.StartItemOffset = GetItemAfterOffset(SelectInfo.StartItemNo);

                            if (!DrawItems[Items[SelectInfo.StartItemNo + 1].FirstDItemNo].LineFirst)
                            {
                                KeyDown(e);
                                return; ;
                            }
                        }
                        else  // 在第一个Item最左面按下左方向键
                            e.Handled = true;
                    }
                }

                if (!e.Handled)
                {
                    int vNewCaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                    if (vNewCaretDrawItemNo != CaretDrawItemNo)
                    {
                        if ((vNewCaretDrawItemNo == CaretDrawItemNo - 1)  // 移动到前一个了)
                            && (DrawItems[vNewCaretDrawItemNo].ItemNo == DrawItems[CaretDrawItemNo].ItemNo)  // 是同一个Item
                            && (DrawItems[CaretDrawItemNo].LineFirst)  // 原是行首
                            && (SelectInfo.StartItemOffset == DrawItems[CaretDrawItemNo].CharOffs - 1)) // 光标位置也是原DrawItem的最前面
                        {
                            // 不更换
                        }
                        else
                            CaretDrawItemNo = vNewCaretDrawItemNo;
                    }
                }
            }
        }

        private void SelectNext(ref int AItemNo, ref int AOffset)
        {
            if (AOffset == GetItemAfterOffset(AItemNo))
            {
                if (AItemNo < Items.Count - 1)
                {
                    AItemNo++;
                    if (Items[AItemNo].StyleNo < HCStyle.Null)
                        AOffset = HC.OffsetAfter;
                    else
                        AOffset = 1;
                }
            }
            else  // 不在最后
            {
                if (Items[AItemNo].StyleNo < HCStyle.Null)
                    AOffset = HC.OffsetAfter;
                else
                    AOffset = AOffset + 1;
            }
        }

        private void SelectStartItemNext()
        {
            int vItemNo = SelectInfo.StartItemNo;
            int vOffset = SelectInfo.StartItemOffset;
            SelectNext(ref vItemNo, ref vOffset);
            SelectInfo.StartItemNo = vItemNo;
            SelectInfo.StartItemOffset = vOffset;
        }

        private void SelectEndItemNext()
        {
            int vItemNo = SelectInfo.EndItemNo;
            int vOffset = SelectInfo.EndItemOffset;
            SelectNext(ref vItemNo, ref vOffset);
            SelectInfo.EndItemNo = vItemNo;
            SelectInfo.EndItemOffset = vOffset;
        }

        private void RightKeyDown(bool vSelectExist, HCCustomItem vCurItem, KeyEventArgs e)
        {
            if (e.Shift)
            {
                if (SelectInfo.EndItemNo >= 0)
                {
                    if (IsSelectSeekStart())
                    {
                        SelectStartItemNext();
                        SetSelectSeekStart();
                    }
                    else  // 游标在选中结束
                    {
                        SelectEndItemNext();
                        SetSelectSeekEnd();
                    }
                }
                else   // 没有选中
                {
                    if (SelectInfo.StartItemNo < Items.Count - 1)
                    {
                        if (Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null)
                        {
                            if (SelectInfo.StartItemOffset == HC.OffsetAfter)
                            {
                                SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                                SelectInfo.StartItemOffset = 0;
                            }
                        }
                        else
                            if (SelectInfo.StartItemOffset == Items[SelectInfo.StartItemNo].Length)
                            {
                                SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                                SelectInfo.StartItemOffset = 0;
                            }
                    }

                    SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                    SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;

                    SelectEndItemNext();
                    SetSelectSeekEnd();
                }

                CheckSelectEndEff();
                MatchItemSelectState();
                Style.UpdateInfoRePaint();
            }
            else  // 没有按下Shift
            {
                if (vSelectExist)
                {
                    SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                    SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
                }
                else  // 无选中内容
                {
                    if (SelectInfo.StartItemOffset < vCurItem.Length)
                        SelectInfo.StartItemOffset = SelectInfo.StartItemOffset + 1;
                    else  // 在Item最右边
                    {
                        if (SelectInfo.StartItemNo < Items.Count - 1)
                        {
                            SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;  // 选中下一个Item
                            SelectInfo.StartItemOffset = 0;  // 下一个最前面
                            if (!DrawItems[Items[SelectInfo.StartItemNo].FirstDItemNo].LineFirst)
                            {
                                KeyDown(e);
                                return;
                            }
                        }
                        else  // 在最后一个Item最右面按下右方向键
                            e.Handled = true;
                    }
                }

                if (!e.Handled)
                {
                    int vNewCaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                    if (vNewCaretDrawItemNo == CaretDrawItemNo)
                    {
                        if ((SelectInfo.StartItemOffset == DrawItems[vNewCaretDrawItemNo].CharOffsetEnd())  // 移动到DrawItem最后面了)
                            && (vNewCaretDrawItemNo < DrawItems.Count - 1)  // 不是最后一个
                            && (DrawItems[vNewCaretDrawItemNo].ItemNo == DrawItems[vNewCaretDrawItemNo + 1].ItemNo)  // 下一个DrawItem和当前是同一个Item
                            && (DrawItems[vNewCaretDrawItemNo + 1].LineFirst)  // 下一个是行首
                            && (SelectInfo.StartItemOffset == DrawItems[vNewCaretDrawItemNo + 1].CharOffs - 1)) // 光标位置也是下一个DrawItem的最前面
                            CaretDrawItemNo = vNewCaretDrawItemNo + 1;  // 更换为下一个行首
                    }
                    else
                        CaretDrawItemNo = vNewCaretDrawItemNo;
                }
            }
        }

        private void HomeKeyDown(bool vSelectExist, KeyEventArgs e)
        {
            if (e.Shift)
            {
                // 取行首DrawItem
                int vFirstDItemNo = GetDrawItemNoByOffset(FSelectSeekNo, FSelectSeekOffset);  // GetSelectStartDrawItemNo
                while (vFirstDItemNo > 0)
                {
                    if (DrawItems[vFirstDItemNo].LineFirst)
                        break;
                    else
                        vFirstDItemNo--;
                }

                if (SelectInfo.EndItemNo >= 0)
                {
                    if (IsSelectSeekStart())
                    {
                        SelectInfo.StartItemNo = DrawItems[vFirstDItemNo].ItemNo;
                        SelectInfo.StartItemOffset = DrawItems[vFirstDItemNo].CharOffs - 1;
                        SetSelectSeekStart();
                    }
                    else  // 游标在选中结束
                    {
                        if (DrawItems[vFirstDItemNo].ItemNo > SelectInfo.StartItemNo)
                        {
                            SelectInfo.EndItemNo = DrawItems[vFirstDItemNo].ItemNo;
                            SelectInfo.EndItemOffset = DrawItems[vFirstDItemNo].CharOffs - 1;
                            SetSelectSeekEnd();
                        }
                        else
                            if (DrawItems[vFirstDItemNo].ItemNo == SelectInfo.StartItemNo)
                            {
                                if (DrawItems[vFirstDItemNo].CharOffs - 1 > SelectInfo.StartItemOffset)
                                {
                                    SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                                    SelectInfo.EndItemOffset = DrawItems[vFirstDItemNo].CharOffs - 1;
                                    SetSelectSeekEnd();
                                }
                            }
                            else
                            {
                                SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                                SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;
                                SelectInfo.StartItemNo = DrawItems[vFirstDItemNo].ItemNo;
                                SelectInfo.StartItemOffset = DrawItems[vFirstDItemNo].CharOffs - 1;
                                SetSelectSeekStart();
                            }
                    }
                }
                else   // 没有选中
                {
                    SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                    SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;
                    SelectInfo.StartItemNo = DrawItems[vFirstDItemNo].ItemNo;
                    SelectInfo.StartItemOffset = DrawItems[vFirstDItemNo].CharOffs - 1;
                    SetSelectSeekStart();
                }

                CheckSelectEndEff();
                MatchItemSelectState();
                Style.UpdateInfoRePaint();
            }
            else
            {
                if (vSelectExist)
                {
                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
                }
                else  // 无选中内容
                {
                    int vFirstDItemNo = GetSelectStartDrawItemNo();
                    int vLastDItemNo = -1;
                    GetLineDrawItemRang(ref vFirstDItemNo, ref vLastDItemNo);
                    SelectInfo.StartItemNo = DrawItems[vFirstDItemNo].ItemNo;
                    SelectInfo.StartItemOffset = DrawItems[vFirstDItemNo].CharOffs - 1;
                }

                if (!e.Handled)
                    CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            }
        }

        private void EndKeyDown(bool vSelectExist, KeyEventArgs e)
        {
            if (e.Shift)
            {
                // 取行尾DrawItem
                int vLastDItemNo = GetDrawItemNoByOffset(FSelectSeekNo, FSelectSeekOffset);// GetSelectEndDrawItemNo;
                vLastDItemNo = vLastDItemNo + 1;
                while (vLastDItemNo < DrawItems.Count)
                {
                    if (DrawItems[vLastDItemNo].LineFirst)
                        break;
                    else
                        vLastDItemNo++;
                }

                vLastDItemNo--;

                if (SelectInfo.EndItemNo >= 0)
                {
                    if (IsSelectSeekStart())
                    {
                        if (DrawItems[vLastDItemNo].ItemNo > SelectInfo.EndItemNo)
                        {
                            SelectInfo.StartItemNo = DrawItems[vLastDItemNo].ItemNo;
                            SelectInfo.StartItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();
                            SetSelectSeekStart();
                        }
                        else
                            if (DrawItems[vLastDItemNo].ItemNo == SelectInfo.EndItemNo)
                            {
                                SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                                if (DrawItems[vLastDItemNo].CharOffsetEnd() < SelectInfo.EndItemOffset)
                                {
                                    SelectInfo.StartItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();
                                    SetSelectSeekStart();
                                }
                                else
                                {
                                    SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                                    SelectInfo.EndItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();
                                    SetSelectSeekEnd();
                                }
                            }
                            else
                            {
                                SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                                SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                                SelectInfo.EndItemNo = DrawItems[vLastDItemNo].ItemNo;
                                SelectInfo.EndItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();
                                SetSelectSeekEnd();
                            }
                    }
                    else  // 游标在选中结束
                    {
                        SelectInfo.EndItemNo = DrawItems[vLastDItemNo].ItemNo;
                        SelectInfo.EndItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();
                        SetSelectSeekEnd();
                    }
                }
                else   // 没有选中
                {
                    SelectInfo.EndItemNo = DrawItems[vLastDItemNo].ItemNo;
                    SelectInfo.EndItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();
                    SetSelectSeekEnd();
                }

                CheckSelectEndEff();
                MatchItemSelectState();
                Style.UpdateInfoRePaint();
            }
            else
            {
                if (vSelectExist)
                {
                    SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                    SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
                }
                else  // 无选中内容
                {
                    int vFirstDItemNo = GetSelectStartDrawItemNo();
                    int vLastDItemNo = -1;
                    GetLineDrawItemRang(ref vFirstDItemNo, ref vLastDItemNo);
                    SelectInfo.StartItemNo = DrawItems[vLastDItemNo].ItemNo;
                    SelectInfo.StartItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();
                }

                if (!e.Handled)
                    CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            }
        }

        private bool GetUpDrawItemNo(ref int ADrawItemNo, ref int ADrawItemOffset)
        {
            bool Result = false;
            int vFirstDItemNo = ADrawItemNo;
            int vLastDItemNo = -1;
            GetLineDrawItemRang(ref vFirstDItemNo, ref vLastDItemNo);  // 当前行起始结束DrawItemNo
            if (vFirstDItemNo > 0)
            {
                Result = true;
                // 获取当前光标X位置
                int vX = DrawItems[ADrawItemNo].Rect.Left + GetDrawItemOffsetWidth(ADrawItemNo, ADrawItemOffset);
                // 获取上一行在X位置对应的DItem和Offset
                vFirstDItemNo = vFirstDItemNo - 1;
                GetLineDrawItemRang(ref vFirstDItemNo, ref vLastDItemNo);  // 上一行起始和结束DItem
                for (int i = vFirstDItemNo; i <= vLastDItemNo; i++)
                {
                    if (DrawItems[i].Rect.Right > vX)
                    {
                        ADrawItemNo = i;
                        ADrawItemOffset = DrawItems[i].CharOffs + GetDrawItemOffsetAt(i, vX) - 1;
                        return Result;  // 有合适，则退出
                    }
                }

                // 没合适则选择到最后
                ADrawItemNo = vLastDItemNo;
                ADrawItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();
            }

            return Result;
        }

        private void UpKeyDown(bool vSelectExist, KeyEventArgs e)
        {
            if (e.Shift)
            {
                int vDrawItemNo = -1, vDrawItemOffset = -1;

                if (SelectInfo.EndItemNo >= 0)
                {
                    if (IsSelectSeekStart())
                    {
                        vDrawItemNo = GetSelectStartDrawItemNo();
                        vDrawItemOffset = SelectInfo.StartItemOffset - DrawItems[vDrawItemNo].CharOffs + 1;
                        if (GetUpDrawItemNo(ref vDrawItemNo, ref vDrawItemOffset))
                        {
                            SelectInfo.StartItemNo = DrawItems[vDrawItemNo].ItemNo;
                            SelectInfo.StartItemOffset = vDrawItemOffset;
                            SetSelectSeekStart();
                        }
                    }
                    else  // 游标在选中结束
                    {
                        vDrawItemNo = GetSelectEndDrawItemNo();
                        vDrawItemOffset = SelectInfo.EndItemOffset - DrawItems[vDrawItemNo].CharOffs + 1;
                        if (GetUpDrawItemNo(ref vDrawItemNo, ref vDrawItemOffset))
                        {
                            if (DrawItems[vDrawItemNo].ItemNo > SelectInfo.StartItemNo)
                            {
                                SelectInfo.EndItemNo = vDrawItemNo;
                                SelectInfo.EndItemOffset = vDrawItemOffset;
                                SetSelectSeekEnd();
                            }
                            else
                                if (DrawItems[vDrawItemNo].ItemNo == SelectInfo.StartItemNo)
                                {
                                    SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                                    if (vDrawItemOffset > SelectInfo.StartItemOffset)
                                    {
                                        SelectInfo.EndItemOffset = vDrawItemOffset;
                                        SetSelectSeekEnd();
                                    }
                                    else  // 移动到起始Offset前面
                                    {
                                        SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;
                                        SelectInfo.StartItemOffset = vDrawItemOffset;
                                        SetSelectSeekStart();
                                    }
                                }
                                else  // 移动到起始Item前面了
                                {
                                    SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                                    SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;
                                    SelectInfo.StartItemNo = DrawItems[vDrawItemNo].ItemNo;
                                    SelectInfo.StartItemOffset = vDrawItemOffset;
                                    SetSelectSeekStart();
                                }
                        }
                    }
                }
                else   // 没有选中
                {
                    vDrawItemNo = CaretDrawItemNo;
                    vDrawItemOffset = SelectInfo.StartItemOffset - DrawItems[vDrawItemNo].CharOffs + 1;
                    if (GetUpDrawItemNo(ref vDrawItemNo, ref vDrawItemOffset))
                    {
                        SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                        SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;
                        SelectInfo.StartItemNo = DrawItems[vDrawItemNo].ItemNo;
                        SelectInfo.StartItemOffset = vDrawItemOffset;
                        SetSelectSeekStart();
                    }
                }

                CheckSelectEndEff();
                MatchItemSelectState();
                Style.UpdateInfoRePaint();
            }
            else  // 无Shift按下
            {
                if (vSelectExist)
                {
                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
                }
                else  // 无选中内容
                {
                    int vDrawItemNo = CaretDrawItemNo;  // GetSelectStartDrawItemNo;
                    int vDrawItemOffset = SelectInfo.StartItemOffset - DrawItems[vDrawItemNo].CharOffs + 1;
                    if (GetUpDrawItemNo(ref vDrawItemNo, ref vDrawItemOffset))
                    {
                        SelectInfo.StartItemNo = DrawItems[vDrawItemNo].ItemNo;
                        SelectInfo.StartItemOffset = vDrawItemOffset;
                        CaretDrawItemNo = vDrawItemNo;
                    }
                    else
                        e.Handled = true;
                }
            }
        }

        private bool GetDownDrawItemNo(ref int ADrawItemNo, ref int ADrawItemOffset)
        {
            bool Result = false;
            int vFirstDItemNo = ADrawItemNo;  // GetSelectStartDrawItemNo;
            int vLastDItemNo = -1;
            GetLineDrawItemRang(ref vFirstDItemNo, ref vLastDItemNo);  // 当前行起始结束DItemNo
            if (vLastDItemNo < DrawItems.Count - 1)
            {
                Result = true;
                // 获取当前光标X位置
                int vX = DrawItems[ADrawItemNo].Rect.Left + GetDrawItemOffsetWidth(ADrawItemNo, ADrawItemOffset);

                // 获取下一行在X位置对应的DItem和Offset
                vFirstDItemNo = vLastDItemNo + 1;
                GetLineDrawItemRang(ref vFirstDItemNo, ref vLastDItemNo);  // 下一行起始和结束DItem

                for (int i = vFirstDItemNo; i <= vLastDItemNo; i++)
                {
                    if (DrawItems[i].Rect.Right > vX)
                    {
                        ADrawItemNo = i;
                        ADrawItemOffset = DrawItems[i].CharOffs + GetDrawItemOffsetAt(i, vX) - 1;
                        return Result;  // 有合适，则退出
                    }
                }

                // 没合适则选择到最后
                ADrawItemNo = vLastDItemNo;
                ADrawItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();
            }

            return Result;
        }

        private void DownKeyDown(bool vSelectExist, KeyEventArgs e)
        {
            if (e.Shift)
            {
                int vDrawItemNo = -1, vDrawItemOffset = -1;
                if (SelectInfo.EndItemNo >= 0)
                {
                    if (IsSelectSeekStart())
                    {
                        vDrawItemNo = GetSelectStartDrawItemNo();
                        vDrawItemOffset = SelectInfo.StartItemOffset - DrawItems[vDrawItemNo].CharOffs + 1;
                        if (GetDownDrawItemNo(ref vDrawItemNo, ref vDrawItemOffset))
                        {
                            if (DrawItems[vDrawItemNo].ItemNo < SelectInfo.EndItemNo)
                            {
                                SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                                SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                                SetSelectSeekStart();
                            }
                            else
                                if (DrawItems[vDrawItemNo].ItemNo == SelectInfo.EndItemNo)
                                {
                                    SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                                    if (vDrawItemOffset < SelectInfo.EndItemOffset)
                                    {
                                        SelectInfo.StartItemOffset = vDrawItemOffset;
                                        SetSelectSeekStart();
                                    }
                                    else  // 位置在结束Offset后面
                                    {
                                        SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                                        SelectInfo.EndItemOffset = vDrawItemOffset;
                                        SetSelectSeekEnd();
                                    }
                                }
                                else  // 移动到结束Item后面，交换
                                {
                                    SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                                    SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                                    SelectInfo.EndItemNo = DrawItems[vDrawItemNo].ItemNo;
                                    SelectInfo.EndItemOffset = vDrawItemOffset;
                                    SetSelectSeekEnd();
                                }
                        }
                    }
                    else  // 游标在选中结束
                    {
                        vDrawItemNo = GetSelectEndDrawItemNo();
                        vDrawItemOffset = SelectInfo.EndItemOffset - DrawItems[vDrawItemNo].CharOffs + 1;
                        if (GetDownDrawItemNo(ref vDrawItemNo, ref vDrawItemOffset))
                        {
                            SelectInfo.EndItemNo = DrawItems[vDrawItemNo].ItemNo;
                            SelectInfo.EndItemOffset = vDrawItemOffset;
                            SetSelectSeekEnd();
                        }
                    }
                }
                else   // 没有选中
                {
                    vDrawItemNo = CaretDrawItemNo;
                    vDrawItemOffset = SelectInfo.StartItemOffset - DrawItems[vDrawItemNo].CharOffs + 1;
                    if (GetDownDrawItemNo(ref vDrawItemNo, ref vDrawItemOffset))
                    {
                        SelectInfo.EndItemNo = DrawItems[vDrawItemNo].ItemNo;
                        SelectInfo.EndItemOffset = vDrawItemOffset;
                        SetSelectSeekEnd();
                    }
                }

                CheckSelectEndEff();
                MatchItemSelectState();
                Style.UpdateInfoRePaint();
            }
            else  // 无Shift按下
            {
                if (vSelectExist)
                {
                    SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                    SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
                }
                else  // 无选中内容
                {
                    int vDrawItemNo = CaretDrawItemNo;  // GetSelectStartDrawItemNo;
                    int vDrawItemOffset = SelectInfo.StartItemOffset - DrawItems[vDrawItemNo].CharOffs + 1;
                    if (GetDownDrawItemNo(ref vDrawItemNo, ref vDrawItemOffset))
                    {
                        SelectInfo.StartItemNo = DrawItems[vDrawItemNo].ItemNo;
                        SelectInfo.StartItemOffset = vDrawItemOffset;
                        CaretDrawItemNo = vDrawItemNo;
                    }
                    else  // 当前行是最后一行
                        e.Handled = true;
                }
            }
        }

        private void RectItemKeyDown(bool vSelectExist, ref HCCustomItem vCurItem, KeyEventArgs e)
        {
            int Key = e.KeyValue;
            int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
            HCCustomRectItem vRectItem = vCurItem as HCCustomRectItem;
            if (SelectInfo.StartItemOffset == HC.OffsetInner)
            {
                if (vRectItem.WantKeyDown(e))
                {
                    Undo_New();
                    if (vRectItem.MangerUndo)
                        UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                    else
                        UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                    vRectItem.KeyDown(e);
                    if (vRectItem.SizeChanged)
                    {
                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                        vRectItem.SizeChanged = false;
                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                    }
                }
                else  // 内部不响应此键
                {
                    switch (Key)
                    {
                        case User.VK_BACK:
                            SelectInfo.StartItemOffset = HC.OffsetAfter;
                            RectItemKeyDown(vSelectExist, ref vCurItem, e);
                            break;

                        case User.VK_DELETE:
                            SelectInfo.StartItemOffset = HC.OffsetBefor;
                            RectItemKeyDown(vSelectExist, ref vCurItem, e);
                            break;
                    }
                }
            }
            else
                if (SelectInfo.StartItemOffset == HC.OffsetBefor)  // 在RectItem前                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            if (SelectInfo.StartItemOffset == OffsetBefor)
                {
                    {
                        switch (Key)
                        {
                            case User.VK_LEFT:
                                LeftKeyDown(vSelectExist, e);
                                break;

                            case User.VK_RIGHT:
                                if (e.Shift)
                                    RightKeyDown(vSelectExist, vCurItem, e);
                                else
                                {
                                    if (vRectItem.WantKeyDown(e))
                                        SelectInfo.StartItemOffset = HC.OffsetInner;
                                    else
                                        SelectInfo.StartItemOffset = HC.OffsetAfter;

                                    CaretDrawItemNo = Items[SelectInfo.StartItemNo].FirstDItemNo;
                                }
                                break;

                            case User.VK_UP:
                                UpKeyDown(vSelectExist, e);
                                break;

                            case User.VK_DOWN:
                                DownKeyDown(vSelectExist, e);
                                break;

                            case User.VK_END:
                                EndKeyDown(vSelectExist, e);
                                break;

                            case User.VK_HOME:
                                HomeKeyDown(vSelectExist, e);
                                break;

                            case User.VK_RETURN:
                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                if (vCurItem.ParaFirst)  // RectItem在段首，插入空行
                                {
                                    vCurItem = CreateDefaultTextItem();
                                    vCurItem.ParaFirst = true;
                                    Items.Insert(SelectInfo.StartItemNo, vCurItem);

                                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);

                                    SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                                    ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                                }
                                else  // RectItem不在行首
                                {
                                    vCurItem.ParaFirst = true;
                                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                                }

                                break;

                            case User.VK_BACK:  // 在RectItem前
                                if (vCurItem.ParaFirst)
                                {
                                    if (SelectInfo.StartItemNo > 0)
                                    {
                                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                        Undo_New();
                                        UndoAction_ItemParaFirst(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, false);

                                        vCurItem.ParaFirst = false;
                                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                                    }
                                    else
                                        DrawItems.ClearFormatMark();  // 第一个前回删不处理，停止格式化
                                }
                                else  // 不是段首
                                {
                                    // 选到上一个最后
                                    SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                                    SelectInfo.StartItemOffset = GetItemAfterOffset(SelectInfo.StartItemNo);

                                    KeyDown(e);  // 执行前一个的删除
                                }
                                break;

                            case User.VK_DELETE:  // 在RectItem前
                                if (!CanDeleteItem(SelectInfo.StartItemNo))
                                {
                                    SelectInfo.StartItemOffset = HC.OffsetAfter;
                                    return;
                                }

                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                if (vCurItem.ParaFirst)  // 是段首
                                {
                                    if (SelectInfo.StartItemNo != vFormatLastItemNo)
                                    {
                                        Undo_New();
                                        UndoAction_ItemParaFirst(SelectInfo.StartItemNo + 1, 0, true);
                                        Items[SelectInfo.StartItemNo + 1].ParaFirst = true;

                                        UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                        Items.RemoveAt(SelectInfo.StartItemNo);

                                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                                    }
                                    else  // 段删除空了
                                    {
                                        Undo_New();
                                        UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                        Items.RemoveAt(SelectInfo.StartItemNo);

                                        vCurItem = CreateDefaultTextItem();
                                        vCurItem.ParaFirst = true;
                                        Items.Insert(SelectInfo.StartItemNo, vCurItem);
                                        UndoAction_InsertItem(SelectInfo.StartItemNo, 0);

                                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                                    }
                                }
                                else  // 不是段首
                                {
                                    if (SelectInfo.StartItemNo < vFormatLastItemNo)
                                    {
                                        int vLen = GetItemAfterOffset(SelectInfo.StartItemNo - 1);

                                        Undo_New();
                                        UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                        // 如果RectItem前面(同一行)有高度小于此RectItme的Item(如Tab)，
                                        // 其格式化时以RectItem为高，重新格式化时如果从RectItem所在位置起始格式化，
                                        // 行高度仍会以Tab为行高，也就是RectItem高度，所以需要从行开始格式化
                                        Items.RemoveAt(SelectInfo.StartItemNo);
                                        if (MergeItemText(Items[SelectInfo.StartItemNo - 1], Items[SelectInfo.StartItemNo]))
                                        {
                                            UndoAction_InsertText(SelectInfo.StartItemNo - 1,
                                                Items[SelectInfo.StartItemNo - 1].Length + 1, Items[SelectInfo.StartItemNo].Text);

                                            Items.RemoveAt(SelectInfo.StartItemNo);
                                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 2, -2);
                                        }
                                        else
                                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);

                                        SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                                        SelectInfo.StartItemOffset = vLen;
                                    }
                                    else  // 段尾(段不只一个Item)
                                    {
                                        Undo_New();
                                        UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                        Items.RemoveAt(SelectInfo.StartItemNo);

                                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);

                                        SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                                        SelectInfo.StartItemOffset = GetItemAfterOffset(SelectInfo.StartItemNo);
                                    }
                                }

                                ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);

                                break;

                            case User.VK_TAB:
                                TABKeyDown(vCurItem, e);
                                break;
                        }
                    }
                }
                else
                    if (SelectInfo.StartItemOffset == HC.OffsetAfter)  // 在其后                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        if (SelectInfo.StartItemOffset == OffsetAfter)
                    {
                        switch (Key)
                        {
                            case User.VK_BACK:
                                if (!CanDeleteItem(SelectInfo.StartItemNo))
                                {
                                    SelectInfo.StartItemOffset = HC.OffsetBefor;
                                    return;
                                }

                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                if (vCurItem.ParaFirst)
                                {
                                    if ((SelectInfo.StartItemNo >= 0)
                                                && (SelectInfo.StartItemNo < Items.Count - 1)
                                                && (!Items[SelectInfo.StartItemNo + 1].ParaFirst))
                                    {
                                        Undo_New();
                                        UndoAction_DeleteItem(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                                        Items.RemoveAt(SelectInfo.StartItemNo);

                                        UndoAction_ItemParaFirst(SelectInfo.StartItemNo, 0, true);
                                        Items[SelectInfo.StartItemNo].ParaFirst = true;
                                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);

                                        ReSetSelectAndCaret(SelectInfo.StartItemNo, 0);
                                    }
                                    else  // 空段了
                                    {
                                        Undo_New();
                                        UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                        Items.RemoveAt(SelectInfo.StartItemNo);

                                        HCCustomItem vItem = CreateDefaultTextItem();
                                        vItem.ParaFirst = true;
                                        Items.Insert(SelectInfo.StartItemNo, vItem);
                                        UndoAction_InsertItem(SelectInfo.StartItemNo, 0);

                                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                                        SelectInfo.StartItemOffset = 0;
                                    }
                                }
                                else  // 不是段首
                                {
                                    SelectInfo.StartItemOffset = HC.OffsetBefor;
                                    Key = User.VK_DELETE;  // 临时替换
                                    RectItemKeyDown(vSelectExist, ref vCurItem, e);
                                    Key = User.VK_BACK;  // 还原
                                }
                                break;

                            case User.VK_DELETE:
                                if (SelectInfo.StartItemNo < Items.Count - 1)
                                {
                                    SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                                    SelectInfo.StartItemOffset = 0;

                                    if (Items[SelectInfo.StartItemNo].ParaFirst)
                                    {
                                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                        Undo_New();
                                        UndoAction_ItemParaFirst(SelectInfo.StartItemNo, 0, false);

                                        Items[SelectInfo.StartItemNo].ParaFirst = false;

                                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                                        ReSetSelectAndCaret(SelectInfo.StartItemNo, 0);
                                    }
                                    else
                                        KeyDown(e);
                                }
                                break;

                            case User.VK_LEFT:
                                if (e.Shift)
                                    LeftKeyDown(vSelectExist, e);
                                else
                                {
                                    if (vRectItem.WantKeyDown(e))
                                        SelectInfo.StartItemOffset = HC.OffsetInner;
                                    else
                                        SelectInfo.StartItemOffset = HC.OffsetBefor;

                                    CaretDrawItemNo = Items[SelectInfo.StartItemNo].FirstDItemNo;
                                }
                                break;

                            case User.VK_RIGHT:
                                RightKeyDown(vSelectExist, vCurItem, e);
                                break;

                            case User.VK_UP:
                                UpKeyDown(vSelectExist, e);
                                break;

                            case User.VK_DOWN:
                                DownKeyDown(vSelectExist, e);
                                break;

                            case User.VK_END:
                                EndKeyDown(vSelectExist, e);
                                break;

                            case User.VK_HOME:
                                HomeKeyDown(vSelectExist, e);
                                break;

                            case User.VK_RETURN:
                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                if ((SelectInfo.StartItemNo < Items.Count - 1)  // 不是最后一个)
                                        && (!Items[SelectInfo.StartItemNo + 1].ParaFirst))  // 下一个不是段首
                                {
                                    Items[SelectInfo.StartItemNo + 1].ParaFirst = true;
                                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                                    SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                                    SelectInfo.StartItemOffset = 0;
                                    CaretDrawItemNo = Items[SelectInfo.StartItemNo].FirstDItemNo;
                                }
                                else
                                {
                                    vCurItem = CreateDefaultTextItem();
                                    vCurItem.ParaFirst = true;
                                    Items.Insert(SelectInfo.StartItemNo + 1, vCurItem);
                                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);
                                    ReSetSelectAndCaret(SelectInfo.StartItemNo + 1, vCurItem.Length);
                                }
                                break;

                            case User.VK_TAB:
                                TABKeyDown(vCurItem, e);
                                break;
                        }
                    }
        }

        private void EnterKeyDown(ref HCCustomItem vCurItem, KeyEventArgs e)
        {
            int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
            _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
            // 判断光标位置内容如何换行
            if (SelectInfo.StartItemOffset == 0)
            {
                if (!vCurItem.ParaFirst)
                {
                    vCurItem.ParaFirst = true;
                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                }
                else  // 原来就是段首
                {
                    HCCustomItem vItem = CreateDefaultTextItem();
                    vItem.ParaNo = vCurItem.ParaNo;
                    vItem.StyleNo = vCurItem.StyleNo;
                    vItem.ParaFirst = true;
                    Items.Insert(SelectInfo.StartItemNo, vItem);  // 原位置的向下移动
                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);
                    SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                }
            }
            else
                if (SelectInfo.StartItemOffset == vCurItem.Length)  // 光标在Item最后面
                {
                    HCCustomItem vItem = null;
                    if (SelectInfo.StartItemNo < Items.Count - 1)
                    {
                        vItem = Items[SelectInfo.StartItemNo + 1];  // 下一个Item
                        if (!vItem.ParaFirst)
                        {
                            vItem.ParaFirst = true;
                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                        }
                        else  // 下一个是段起始
                        {
                            vItem = CreateDefaultTextItem();
                            vItem.ParaNo = vCurItem.ParaNo;
                            vItem.StyleNo = vCurItem.StyleNo;
                            vItem.ParaFirst = true;
                            Items.Insert(SelectInfo.StartItemNo + 1, vItem);
                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);
                        }

                        SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                        SelectInfo.StartItemOffset = 0;
                    }
                    else  // 是Data最后一个Item，新建空行
                    {
                        vItem = CreateDefaultTextItem();
                        vItem.ParaNo = vCurItem.ParaNo;
                        vItem.StyleNo = vCurItem.StyleNo;
                        vItem.ParaFirst = true;
                        Items.Insert(SelectInfo.StartItemNo + 1, vItem);
                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);
                        SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                        SelectInfo.StartItemOffset = 0;
                    }
                }
                else  // 光标在Item中间
                {
                    HCCustomItem vItem = vCurItem.BreakByOffset(SelectInfo.StartItemOffset);  // 截断当前Item
                    vItem.ParaFirst = true;

                    Items.Insert(SelectInfo.StartItemNo + 1, vItem);
                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);

                    SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                    SelectInfo.StartItemOffset = 0;
                }

            if (!e.Handled)
                CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
        }

        private void DeleteKeyDown(HCCustomItem vCurItem, KeyEventArgs e)
        {
            int vDelCount = 0, vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
            int vCurItemNo = SelectInfo.StartItemNo;
            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);

            if (SelectInfo.StartItemOffset == vCurItem.Length)
            {
                if (vCurItemNo != Items.Count - 1)
                {
                    if (Items[vCurItemNo + 1].ParaFirst)
                    {
                        vFormatLastItemNo = GetParaLastItemNo(vCurItemNo + 1);  // 获取下一段最后一个
                        if (vCurItem.Length == 0)
                        {
                            _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                            Items.RemoveAt(vCurItemNo);
                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                        }
                        else  // 当前不是空行
                        {
                            if (Items[vCurItemNo + 1].StyleNo < HCStyle.Null)
                            {
                                SelectInfo.StartItemNo = vCurItemNo + 1;
                                SelectInfo.StartItemOffset = HC.OffsetBefor;

                                KeyDown(e);
                                return;
                            }
                            else  // 下一个段首是TextItem(当前在上一段段尾)
                            {
                                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                if (Items[vCurItemNo + 1].Length == 0)
                                {
                                    Items.RemoveAt(vCurItemNo + 1);
                                    vDelCount++;
                                }
                                else  // 下一段的段首不是空行
                                {
                                    if (vCurItem.CanConcatItems(Items[vCurItemNo + 1]))
                                    {
                                        vCurItem.Text = vCurItem.Text + Items[vCurItemNo + 1].Text;
                                        Items.RemoveAt(vCurItemNo + 1);
                                        vDelCount++;
                                    }
                                    else// 下一段段首不是空行也不能合并
                                        Items[vCurItemNo + 1].ParaFirst = false;

                                    // 修正下一段合并上来的Item段样式，对齐样式
                                    int vParaNo = Items[vCurItemNo].ParaNo;
                                    for (int i = vCurItemNo + 1; i <= vFormatLastItemNo - vDelCount; i++)
                                        Items[i].ParaNo = vParaNo;
                                }

                                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
                            }
                        }
                    }
                    else  // 下一个不能合并也不是段首，移动到下一个开头再调用DeleteKeyDown
                    {
                        SelectInfo.StartItemNo = vCurItemNo + 1;
                        SelectInfo.StartItemOffset = 0;
                        vCurItem = GetCurItem();

                        KeyDown(e);
                        return;
                    }
                }
            }
            else  // 光标不在Item最右边
            {
                if (!CanDeleteItem(vCurItemNo))
                    SelectInfo.StartItemOffset = SelectInfo.StartItemOffset + 1;
                else
                    if (!vCurItem.CanAccept(SelectInfo.StartItemOffset, HCItemAction.hiaDeleteChar))
                        SelectInfo.StartItemOffset = SelectInfo.StartItemOffset + 1;
                    else  // 可删除
                    {
                        string vText = Items[vCurItemNo].Text;
                        string vsDelete = vText.Substring(SelectInfo.StartItemOffset + 1 - 1, 1);
                        vCurItem.Text = vText.Remove(SelectInfo.StartItemOffset + 1 - 1, 1);
                        DoItemAction(vCurItemNo, SelectInfo.StartItemOffset + 1, HCItemAction.hiaDeleteChar);

                        if (vText == "")  // 删除后没有内容了
                        {
                            if (!DrawItems[Items[vCurItemNo].FirstDItemNo].LineFirst)
                            {
                                if (vCurItemNo < Items.Count - 1)
                                {
                                    int vLen = -1;
                                    if (MergeItemText(Items[vCurItemNo - 1], Items[vCurItemNo + 1]))
                                    {
                                        vLen = Items[vCurItemNo + 1].Length;
                                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, vCurItemNo - 1, vLen);
                                        _FormatItemPrepare(vCurItemNo - 1, vFormatLastItemNo);

                                        Undo_New();
                                        UndoAction_DeleteItem(vCurItemNo, 0);
                                        Items.RemoveAt(vCurItemNo);  // 删除当前

                                        Undo_New();
                                        UndoAction_DeleteItem(vCurItemNo, 0);
                                        Items.RemoveAt(vCurItemNo);  // 删除下一个

                                        _ReFormatData(vCurItemNo - 1, vFormatLastItemNo - 2, -2);
                                    }
                                    else  // 下一个合并不到上一个
                                    {
                                        vLen = 0;
                                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                        Undo_New();
                                        UndoAction_DeleteItem(vCurItemNo, 0);
                                        Items.RemoveAt(vCurItemNo);

                                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                                    }

                                    // 光标左移
                                    SelectInfo.StartItemNo = vCurItemNo - 1;
                                    if (GetItemStyle(SelectInfo.StartItemNo) < HCStyle.Null)
                                        SelectInfo.StartItemOffset = HC.OffsetAfter;
                                    else
                                        SelectInfo.StartItemOffset = Items[SelectInfo.StartItemNo].Length - vLen;
                                }
                                else  // 是最后一个Item删除空了
                                {
                                    // 光标左移
                                    _FormatItemPrepare(vCurItemNo);

                                    Undo_New();
                                    UndoAction_DeleteItem(vCurItemNo, 0);
                                    Items.RemoveAt(vCurItemNo);

                                    SelectInfo.StartItemNo = vCurItemNo - 1;
                                    SelectInfo.StartItemOffset = GetItemAfterOffset(SelectInfo.StartItemNo);

                                    DrawItems.DeleteFormatMark();
                                }
                            }
                            else  // 行首Item被删空了
                            {
                                if (vCurItemNo != vFormatLastItemNo)
                                {
                                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                    SelectInfo.StartItemOffset = 0;

                                    Undo_New();
                                    UndoAction_ItemParaFirst(vCurItemNo + 1, 0, Items[vCurItemNo].ParaFirst);
                                    Items[vCurItemNo + 1].ParaFirst = Items[vCurItemNo].ParaFirst;

                                    Undo_New();
                                    UndoAction_DeleteItem(vCurItemNo, 0);
                                    Items.RemoveAt(vCurItemNo);

                                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                                }
                                else  // 当前段删除空了
                                {
                                    Undo_New();
                                    UndoAction_DeleteText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1, vsDelete);

                                    _FormatItemPrepare(vCurItemNo);
                                    SelectInfo.StartItemOffset = 0;
                                    _ReFormatData(vCurItemNo);
                                }
                            }
                        }
                        else  // 删除后还有内容
                        {
                            Undo_New();
                            UndoAction_DeleteText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1, vsDelete);

                            _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                        }
                    }
            }

            if (!e.Handled)
                CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
        }

        private void BackspaceKeyDown(bool vSelectExist, ref HCCustomItem vCurItem, int vParaFirstItemNo, int vParaLastItemNo, KeyEventArgs e)
        {
            int vCurItemNo = -1, vLen = -1, vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
            int vParaNo = -1, vDelCount = 0;
            bool vParaFirst = false;
            if (SelectInfo.StartItemOffset == 0)
            {
                if ((vCurItem.Text == "") && (Style.ParaStyles[vCurItem.ParaNo].AlignHorz != ParaAlignHorz.pahJustify))
                    ApplyParaAlignHorz(ParaAlignHorz.pahJustify);  // 居中等对齐的空Item，删除时切换到分散对齐
                else
                if (vCurItem.ParaFirst && (Style.ParaStyles[vCurItem.ParaNo].FirstIndent > 0))  // 在段最前面删除
                {
                    HCParaStyle vParaStyle = Style.ParaStyles[vCurItem.ParaNo];
                    ApplyParaFirstIndent(vParaStyle.FirstIndent - HCUnitConversion.PixXToMillimeter(HC.TabCharWidth));
                }
                else
                    if (SelectInfo.StartItemNo != 0)
                    {
                        vCurItemNo = SelectInfo.StartItemNo;
                        if (vCurItem.ParaFirst)
                        {
                            vLen = Items[SelectInfo.StartItemNo - 1].Length;
                            if (vCurItem.CanConcatItems(Items[SelectInfo.StartItemNo - 1]))
                            {
                                Undo_New();
                                UndoAction_InsertText(SelectInfo.StartItemNo - 1, Items[SelectInfo.StartItemNo - 1].Length + 1,
                                    Items[SelectInfo.StartItemNo].Text);

                                Items[SelectInfo.StartItemNo - 1].Text = Items[SelectInfo.StartItemNo - 1].Text
                                    + Items[SelectInfo.StartItemNo].Text;

                                vFormatFirstItemNo = GetLineFirstItemNo(SelectInfo.StartItemNo - 1, vLen);
                                vFormatLastItemNo = GetParaLastItemNo(SelectInfo.StartItemNo);
                                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                UndoAction_DeleteItem(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                                Items.RemoveAt(SelectInfo.StartItemNo);

                                // 修正下一段合并上来的Item的段样式，对齐样式
                                vParaNo = Items[SelectInfo.StartItemNo - 1].ParaNo;
                                if (vParaNo != vCurItem.ParaNo)
                                {
                                    for (int i = SelectInfo.StartItemNo; i <= vFormatLastItemNo - 1; i++)
                                    {
                                        Items[i].ParaNo = vParaNo;
                                    }
                                }

                                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);

                                ReSetSelectAndCaret(SelectInfo.StartItemNo - 1, vLen);
                            }
                            else  // 段起始且不能和上一个合并
                            {
                                if (vCurItem.Length == 0)
                                {
                                    _FormatItemPrepare(SelectInfo.StartItemNo - 1, SelectInfo.StartItemNo);

                                    Undo_New();
                                    UndoAction_DeleteItem(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                                    Items.RemoveAt(SelectInfo.StartItemNo);

                                    _ReFormatData(SelectInfo.StartItemNo - 1, SelectInfo.StartItemNo - 1, -1);

                                    ReSetSelectAndCaret(SelectInfo.StartItemNo - 1);
                                }
                                else  // 段前删除且不能和上一段最后合并
                                {
                                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                    Undo_New();
                                    UndoAction_ItemParaFirst(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, false);

                                    vCurItem.ParaFirst = false;  // 当前段和上一段Item拼接成一段

                                    vParaNo = Items[SelectInfo.StartItemNo - 1].ParaNo;  // 上一段的ParaNo
                                    if (vParaNo != vCurItem.ParaNo)
                                    {
                                        for (int i = SelectInfo.StartItemNo; i <= vFormatLastItemNo; i++)
                                        {
                                            //Undo_ItemParaNo(i, 0, vParaNo);
                                            Items[i].ParaNo = vParaNo;
                                        }
                                    }

                                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);

                                    ReSetSelectAndCaret(SelectInfo.StartItemNo, 0);
                                }
                            }
                        }
                        else  // 在Item开始往前删，但Item不是段起始
                        {
                            if (Items[SelectInfo.StartItemNo - 1].StyleNo < HCStyle.Null)
                            {
                                vCurItemNo = SelectInfo.StartItemNo - 1;
                                if (CanDeleteItem(vCurItemNo))
                                {
                                    Undo_New();
                                    vParaFirst = Items[vCurItemNo].ParaFirst;  // 记录前面的RectItem段首属性

                                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                    // 删除前面的RectItem
                                    UndoAction_DeleteItem(vCurItemNo, HC.OffsetAfter);
                                    Items.RemoveAt(vCurItemNo);
                                    vDelCount = 1;

                                    if (vParaFirst)
                                    {
                                        UndoAction_ItemParaFirst(vCurItemNo, 0, vParaFirst);
                                        vCurItem.ParaFirst = vParaFirst;  // 赋值前面RectItem的段起始属性
                                        vLen = 0;
                                    }
                                    else  // 前面删除的RectItem不是段首
                                    {
                                        vCurItemNo = vCurItemNo - 1;  // 上一个
                                        vLen = Items[vCurItemNo].Length;  // 上一个最后面

                                        if (MergeItemText(Items[vCurItemNo], vCurItem))
                                        {
                                            UndoAction_InsertText(vCurItemNo, vLen + 1, vCurItem.Text);
                                            UndoAction_DeleteItem(vCurItemNo + 1, 0);
                                            Items.RemoveAt(vCurItemNo + 1); // 删除当前的
                                            vDelCount = 2;
                                        }
                                    }

                                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
                                }
                                else  // 不能删除，光标放最前
                                    vLen = HC.OffsetBefor;

                                ReSetSelectAndCaret(vCurItemNo, vLen);
                            }
                            else  // 前面是文本，赋值为前面的最后，再重新处理删除
                            {
                                SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                                SelectInfo.StartItemOffset = GetItemAfterOffset(SelectInfo.StartItemNo);
                                vCurItem = GetCurItem();
                                Style.UpdateInfoReStyle();
                                BackspaceKeyDown(vSelectExist, ref vCurItem, vParaFirstItemNo, vParaLastItemNo, e);  // 重新处理

                                return;
                            }
                        }
                    }
            }
            else  // 光标不在Item最开始  文本TextItem
            {
                if (!vCurItem.CanAccept(SelectInfo.StartItemOffset, HCItemAction.hiaBackDeleteChar))  // 不允许删除
                    LeftKeyDown(vSelectExist, e);  // 往前走
                else
                    if (vCurItem.Length == 1)  // 删除后没有内容了
                    {
                        vCurItemNo = SelectInfo.StartItemNo;  // 记录原位置
                        if (!DrawItems[Items[vCurItemNo].FirstDItemNo].LineFirst)
                        {
                            vLen = Items[vCurItemNo - 1].Length;
                            if ((vCurItemNo > 0) && (vCurItemNo < vParaLastItemNo)  // 不是段最后一个)
                                && MergeItemText(Items[vCurItemNo - 1], Items[vCurItemNo + 1]))
                            {
                                Undo_New();
                                UndoAction_InsertText(vCurItemNo - 1, Items[vCurItemNo - 1].Length - Items[vCurItemNo + 1].Length + 1,
                                    Items[vCurItemNo + 1].Text);

                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, vCurItemNo - 1, vLen);
                                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                Items.RemoveAt(vCurItemNo);  // 删除当前

                                UndoAction_DeleteItem(vCurItemNo, 0);
                                Items.RemoveAt(vCurItemNo);  // 删除下一个

                                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 2, -2);

                                ReSetSelectAndCaret(SelectInfo.StartItemNo - 1, vLen);  // 上一个原光标位置
                            }
                            else  // 当前不是行首，删除后没有内容了，且不能合并上一个和下一个
                            {
                                if (SelectInfo.StartItemNo == vParaLastItemNo)
                                {
                                    vFormatFirstItemNo = GetLineFirstItemNo(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                                    vFormatLastItemNo = vParaLastItemNo;
                                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                    Undo_New();
                                    UndoAction_DeleteItem(vCurItemNo, SelectInfo.StartItemOffset);
                                    Items.RemoveAt(vCurItemNo);

                                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);

                                    ReSetSelectAndCaret(vCurItemNo - 1);
                                }
                                else  // 不是段最后一个
                                {
                                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                    Undo_New();
                                    UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                    Items.RemoveAt(vCurItemNo);

                                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);

                                    ReSetSelectAndCaret(vCurItemNo - 1);
                                }
                            }
                        }
                        else  // Item是行第一个、行首Item删除空了，
                        {
                            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                            if (Items[vCurItemNo].ParaFirst)
                            {
                                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                if (vCurItemNo < vFormatLastItemNo)
                                {
                                    Undo_New();
                                    vParaFirst = true;  // Items[vCurItemNo].ParaFirst;  // 记录行首Item的段属性

                                    UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                    Items.RemoveAt(vCurItemNo);

                                    if (vParaFirst)
                                    {
                                        UndoAction_ItemParaFirst(vCurItemNo, 0, vParaFirst);
                                        Items[vCurItemNo].ParaFirst = vParaFirst;  // 其后继承段首属性
                                    }

                                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                                    ReSetSelectAndCaret(vCurItemNo, 0);  // 下一个最前面
                                }
                                else  // 同段后面没有内容了，保持空行
                                {
                                    Undo_New();
                                    UndoAction_DeleteBackText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, vCurItem.Text);  // Copy(vText, SelectInfo.StartItemOffset, 1));

                                    //System.Delete(vText, SelectInfo.StartItemOffset, 1);
                                    vCurItem.Text = "";  // vText;
                                    SelectInfo.StartItemOffset = SelectInfo.StartItemOffset - 1;

                                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);  // 保留空行
                                }
                            }
                            else  // 不是段首Item，仅是行首Item删除空了
                            {
                                Undo_New();

                                if (vCurItemNo < vFormatLastItemNo)
                                {
                                    vLen = Items[vCurItemNo - 1].Length;
                                    if (MergeItemText(Items[vCurItemNo - 1], Items[vCurItemNo + 1]))
                                    {
                                        UndoAction_InsertText(vCurItemNo - 1,
                                            Items[vCurItemNo - 1].Length - Items[vCurItemNo + 1].Length + 1, Items[vCurItemNo + 1].Text);

                                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, vCurItemNo - 1, Items[vCurItemNo - 1].Length);  // 取前一个格式化起始位置
                                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                        UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);  // 删除空的Item
                                        Items.RemoveAt(vCurItemNo);

                                        UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);  // 被合并的Item
                                        Items.RemoveAt(vCurItemNo);

                                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 2, -2);
                                        ReSetSelectAndCaret(vCurItemNo - 1, vLen);
                                    }
                                    else  // 前后不能合并
                                    {
                                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                        UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                        Items.RemoveAt(vCurItemNo);

                                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                                        ReSetSelectAndCaret(vCurItemNo - 1);
                                    }
                                }
                                else  // 同段后面没有内容了
                                {
                                    if (vFormatFirstItemNo == vCurItemNo)
                                        vFormatFirstItemNo--;

                                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                    UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                    Items.RemoveAt(vCurItemNo);

                                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                                    ReSetSelectAndCaret(vCurItemNo - 1);
                                }
                            }
                        }
                    }
                    else  // 删除后还有内容
                    {
                        if (SelectInfo.StartItemNo > vParaFirstItemNo)
                            vFormatFirstItemNo = GetLineFirstItemNo(SelectInfo.StartItemNo - 1, 0);
                        else
                            vFormatFirstItemNo = SelectInfo.StartItemNo;

                        vFormatLastItemNo = vParaLastItemNo;

                        _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                        string vText = vCurItem.Text;  // 和上面 201806242257 处一样

                        Undo_New();
                        UndoAction_DeleteBackText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, vText.Substring(SelectInfo.StartItemOffset - 1, 1));

                        vCurItem.Text = vText.Remove(SelectInfo.StartItemOffset - 1, 1); ;

                        _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);

                        SelectInfo.StartItemOffset = SelectInfo.StartItemOffset - 1;
                        ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                    }
            }
        }
        #endregion

        // Key返回0表示此键按下Data没有做任何事情
        public virtual void KeyDown(KeyEventArgs e)
        {
            if (!CanEdit())
                return;

            int Key = e.KeyValue;

            if ((Key == User.VK_BACK)
                || (Key == User.VK_DELETE)
                || (Key == User.VK_RETURN)
                || (Key == User.VK_TAB))
                this.InitializeMouseField();  // 如果Item删除完了，原MouseMove处ItemNo可能不存在了，再MouseMove时清除旧的出错

            HCCustomItem vCurItem = GetCurItem();
            if (vCurItem == null)
                return;

            bool vSelectExist = SelectExists();
            if (vSelectExist && ((Key == User.VK_BACK)
                                    || (Key == User.VK_DELETE)
                                    || (Key == User.VK_RETURN)
                                    || (Key == User.VK_TAB))
                )
            {
                if (DeleteSelected())
                {
                    if ((Key == User.VK_BACK) || (Key == User.VK_DELETE))
                        return;
                }
            }

            int vParaFirstItemNo = -1, vParaLastItemNo = -1;
            GetParaItemRang(SelectInfo.StartItemNo, ref vParaFirstItemNo, ref vParaLastItemNo);

            if (vCurItem.StyleNo < HCStyle.Null)
                RectItemKeyDown(vSelectExist, ref vCurItem, e);
            else
            {
                switch (Key)
                {
                    case User.VK_BACK:
                        BackspaceKeyDown(vSelectExist, ref vCurItem, vParaFirstItemNo, vParaLastItemNo, e);  // 回删
                        break;

                    case User.VK_RETURN:
                        EnterKeyDown(ref vCurItem, e);  // 回车
                        break;

                    case User.VK_LEFT:
                        LeftKeyDown(vSelectExist, e);  // 左方向键
                        break;

                    case User.VK_RIGHT:
                        RightKeyDown(vSelectExist, vCurItem, e);  // 右方向键
                        break;

                    case User.VK_DELETE:
                        DeleteKeyDown(vCurItem, e);  // 删除键
                        break;

                    case User.VK_HOME:
                        HomeKeyDown(vSelectExist, e);  // Home键
                        break;

                    case User.VK_END:
                        EndKeyDown(vSelectExist, e);  // End键
                        break;

                    case User.VK_UP:
                        UpKeyDown(vSelectExist, e);  // 上方向键
                        break;

                    case User.VK_DOWN:
                        DownKeyDown(vSelectExist, e);  // 下方向键
                        break;

                    case User.VK_TAB:
                        TABKeyDown(vCurItem, e);  // TAB键
                        break;
                }
            }

            switch (Key)
            {
                case User.VK_BACK:
                case User.VK_DELETE:
                case User.VK_RETURN:
                case User.VK_TAB:
                    Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();  // 删除后以新位置光标为当前样式
                    Style.UpdateInfoReScroll();
                    break;

                case User.VK_LEFT:
                case User.VK_RIGHT:
                case User.VK_UP:
                case User.VK_DOWN:
                case User.VK_HOME:
                case User.VK_END:
                    if (vSelectExist)
                        Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();
                    Style.UpdateInfoReScroll();
                    break;
            }
        }

        // Key返回0表示此键按下Data没有做任何事情
        public virtual void KeyUp(KeyEventArgs e)
        {
            if (!CanEdit())
                return;
        }

        public bool BatchInsert()
        {
            return FBatchInsertCount > 0;
        }

        public void BeginBatchInsert()
        {
            FBatchInsertCount++;
        }

        public void EndBatchInsert()
        {
            FBatchInsertCount--;
        }

        /// <summary> 初始化相关字段和变量 </summary>
        public override void InitializeField()
        {
            InitializeMouseField();
            base.InitializeField();
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            if (!CanEdit())
                return;

            base.LoadFromStream(aStream, aStyle, aFileVersion);
            InsertStream(aStream, aStyle, aFileVersion);
            // 加载完成后，初始化(有一部分在LoadFromStream中初始化了)
            ReSetSelectAndCaret(0, 0);
        }

        public override bool InsertStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            if (!CanEdit())
                return false;

            bool Result = false;
            HCCustomItem vAfterItem = null;
            bool vInsertBefor = false;
            int vInsPos = 0, vFormatFirstItemNo = -1, vFormatLastItemNo = -1;

            Undo_GroupBegin(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            try
            {
                if (Items.Count == 0)
                    vInsPos = 0;
                else  // 有数据
                {
                    DeleteSelected();
                    // 确定插入位置
                    vInsPos = SelectInfo.StartItemNo;
                    if (Items[vInsPos].StyleNo < HCStyle.Null)
                    {
                        if (SelectInfo.StartItemOffset == HC.OffsetInner)
                        {
                            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, SelectInfo.StartItemNo, HC.OffsetInner);
                            _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                            Undo_New();
                            if ((Items[vInsPos] as HCCustomRectItem).MangerUndo)
                                UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                            else
                                UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                            Result = (Items[vInsPos] as HCCustomRectItem).InsertStream(aStream, aStyle, aFileVersion);
                            _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);

                            return Result;
                        }
                        else
                            if (SelectInfo.StartItemOffset == HC.OffsetBefor)
                                vInsertBefor = true;
                            else  // 其后
                                vInsPos = vInsPos + 1;
                    }
                    else  // TextItem
                    {
                        // 先判断光标是否在最后，防止空Item时SelectInfo.StartItemOffset = 0按其前处理
                        if (SelectInfo.StartItemOffset == Items[vInsPos].Length)
                            vInsPos = vInsPos + 1;
                        else
                            if (SelectInfo.StartItemOffset == 0)
                                vInsertBefor = Items[vInsPos].Length != 0;
                            else  // TextItem中间
                            {
                                Undo_New();
                                UndoAction_DeleteBackText(vInsPos, SelectInfo.StartItemOffset + 1,
                                    Items[vInsPos].Text.Substring(SelectInfo.StartItemOffset + 1 - 1, Items[vInsPos].Length - SelectInfo.StartItemOffset));

                                vAfterItem = Items[vInsPos].BreakByOffset(SelectInfo.StartItemOffset);  // 后半部分对应的Item
                                vInsPos = vInsPos + 1;
                            }
                    }
                }

                Int64 vDataSize = 0;
                byte[] vBuffer = BitConverter.GetBytes(vDataSize);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vDataSize = BitConverter.ToInt64(vBuffer, 0);
                if (vDataSize == 0)
                    return Result;

                int vItemCount = 0;
                vBuffer = BitConverter.GetBytes(vItemCount);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vItemCount = BitConverter.ToInt32(vBuffer, 0);

                if (vItemCount == 0)
                    return Result;

                // 因为插入的第一个可能和插入位置前一个合并，插入位置可能是行首，所以要从插入位置
                // 行上一个开始格式化，为简单处理，直接使用段首，可优化为上一行首
                //GetParaItemRang(SelectInfo.StartItemNo, vFormatFirstItemNo, vFormatLastItemNo);

                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);

                // 计算格式化起始、结束ItemNo
                if (Items.Count > 0)
                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                else
                {
                    vFormatFirstItemNo = 0;
                    vFormatLastItemNo = -1;
                }

                Undo_New();

                int vStyleNo = HCStyle.Null;
                HCCustomItem vItem = null;
                for (int i = 0; i <= vItemCount - 1; i++)
                {
                    aStream.Read(vBuffer, 0, vBuffer.Length);  // besure vBuffer.Length = 4
                    vStyleNo = BitConverter.ToInt32(vBuffer, 0);
                    vItem = CreateItemByStyle(vStyleNo);
                    if (vStyleNo < HCStyle.Null)
                    {
                        if ((vItem as HCCustomRectItem).MangerUndo)
                            UndoAction_ItemSelf(i, 0);
                        else
                            UndoAction_ItemMirror(i, HC.OffsetInner);
                    }

                    vItem.LoadFromStream(aStream, aStyle, aFileVersion);
                    if (aStyle != null)
                    {
                        if (vItem.StyleNo > HCStyle.Null)
                            vItem.StyleNo = Style.GetStyleNo(aStyle.TextStyles[vItem.StyleNo], true);

                        vItem.ParaNo = Style.GetParaNo(aStyle.ParaStyles[vItem.ParaNo], true);
                    }
                    else  // 无样式表
                    {
                        if (vItem.StyleNo > HCStyle.Null)
                            vItem.StyleNo = Style.CurStyleNo;

                        vItem.ParaNo = Style.CurParaNo;
                    }
                    if (i == 0)
                    {
                        if (vInsertBefor)
                        {
                            vItem.ParaFirst = Items[vInsPos].ParaFirst;

                            if (Items[vInsPos].ParaFirst)
                            {
                                UndoAction_ItemParaFirst(vInsPos, 0, false);
                                Items[vInsPos].ParaFirst = false;
                            }
                        }
                        else
                            vItem.ParaFirst = false;
                    }

                    Items.Insert(vInsPos + i, vItem);
                    UndoAction_InsertItem(vInsPos + i, 0);
                }

                vItemCount = CheckInsertItemCount(vInsPos, vInsPos + vItemCount - 1);  // 检查插入的Item是否合格并删除不合格

                int vInsetLastNo = vInsPos + vItemCount - 1;  // 光标在最后一个Item
                int vCaretOffse = GetItemAfterOffset(vInsetLastNo);  // 最后一个Item后面

                if (vAfterItem != null)
                {
                    if (MergeItemText(Items[vInsetLastNo], vAfterItem))
                    {
                        UndoAction_InsertText(vInsetLastNo, Items[vInsetLastNo].Length - vAfterItem.Length + 1, vAfterItem.Text);
                        vAfterItem.Dispose();
                    }
                    else  // 插入最后一个和后半部分不能合并
                    {
                        Items.Insert(vInsetLastNo + 1, vAfterItem);
                        UndoAction_InsertItem(vInsetLastNo + 1, 0);

                        vItemCount++;
                    }
                }

                if (vInsPos > 0)
                {
                    if (Items[vInsPos - 1].Length == 0)
                    {
                        UndoAction_ItemParaFirst(vInsPos, 0, Items[vInsPos - 1].ParaFirst);
                        Items[vInsPos].ParaFirst = Items[vInsPos - 1].ParaFirst;

                        UndoAction_DeleteItem(vInsPos - 1, 0);
                        Items.RemoveAt(vInsPos - 1);  // 删除空行

                        vItemCount--;
                        vInsetLastNo--;
                    }
                    else  // 插入位置前面不是空行Item
                    {
                        int vOffsetStart = Items[vInsPos - 1].Length;
                        if ((!Items[vInsPos].ParaFirst)
                            && MergeItemText(Items[vInsPos - 1], Items[vInsPos]))
                        {
                            UndoAction_InsertText(vInsPos - 1, 
                                Items[vInsPos - 1].Length - Items[vInsPos].Length + 1, 
                                Items[vInsPos].Text);
                            UndoAction_DeleteItem(vInsPos, 0);

                            if (vItemCount == 1)
                                vCaretOffse = vOffsetStart + vCaretOffse;

                            Items.RemoveAt(vInsPos);

                            if (vItemCount == 1)
                                vCaretOffse = vOffsetStart + vCaretOffse;

                            vItemCount--;
                            vInsetLastNo--;
                        }
                    }

                    if ((vInsetLastNo < Items.Count - 1)  // 插入最后Item和后面的能合并)
                        && (!Items[vInsetLastNo + 1].ParaFirst)
                        && MergeItemText(Items[vInsetLastNo], Items[vInsetLastNo + 1]))
                    {
                        UndoAction_DeleteItem(vInsetLastNo + 1, 0);

                        Items.RemoveAt(vInsetLastNo + 1);
                        vItemCount--;
                    }
                }
                else  // 在最开始第0个位置处插入
                //if (vInsetLastNo < Items.Count - 1) then
                {
                    if (MergeItemText(Items[vInsetLastNo], Items[vInsetLastNo + 1]))
                    {
                        UndoAction_DeleteItem(vInsPos + vItemCount, 0);

                        Items.RemoveAt(vInsPos + vItemCount);
                        vItemCount--;
                    }
                }

                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo + vItemCount, vItemCount);

                ReSetSelectAndCaret(vInsetLastNo, vCaretOffse);  // 选中插入内容最后Item位置
            }
            finally
            {
                Undo_GroupEnd(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            }

            InitializeMouseField();  // 201807311101

            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();
            Style.UpdateInfoReScroll();

            return Result;
        }

        //
        public void DblClick(int x, int y)
        {
            FMouseLBDouble = true;
            int vItemNo = -1, vItemOffset = -1, vDrawItemNo = -1;
            bool vRestrain = false;

            GetItemAt(x, y, ref vItemNo, ref vItemOffset, ref vDrawItemNo, ref vRestrain);
            if (vItemNo < 0)
                return;

            if (Items[vItemNo].StyleNo < HCStyle.Null)
            {
                int vX = -1, vY = -1;
                CoordToItemOffset(x, y, vItemNo, vItemOffset, ref vX, ref vY);
                Items[vItemNo].DblClick(vX, vY);
            }
            else  // TextItem双击时根据光标处内容，选中范围
                if (Items[vItemNo].Length > 0)
                {
                    string vText = GetDrawItemText(vDrawItemNo);  // DrawItem对应的文本
                    vItemOffset = vItemOffset - DrawItems[vDrawItemNo].CharOffs + 1;  // 映射到DrawItem上

                    CharType vPosType;
                    if (vItemOffset > 0)
                        vPosType = HC.GetUnicodeCharType((ushort)vText[vItemOffset - 1]);
                    else
                        vPosType = HC.GetUnicodeCharType((ushort)vText[1 - 1]);

                    int vStartOffset = 0;
                    for (int i = vItemOffset - 1; i >= 1; i--)  // 往前找Char类型不一样的位置
                    {
                        if (HC.GetUnicodeCharType((ushort)vText[i - 1]) != vPosType)
                        {
                            vStartOffset = i;
                            break;
                        }
                    }

                    int vEndOffset = vText.Length;
                    for (int i = vItemOffset + 1; i <= vText.Length; i++)  // 往后找Char类型不一样的位置
                    {
                        if (HC.GetUnicodeCharType((ushort)vText[i - 1]) != vPosType)
                        {
                            vEndOffset = i - 1;
                            break;
                        }
                    }

                    this.SelectInfo.StartItemNo = vItemNo;
                    this.SelectInfo.StartItemOffset = vStartOffset + DrawItems[vDrawItemNo].CharOffs - 1;

                    if (vStartOffset != vEndOffset)
                    {
                        this.SelectInfo.EndItemNo = vItemNo;
                        this.SelectInfo.EndItemOffset = vEndOffset + DrawItems[vDrawItemNo].CharOffs - 1;

                        MatchItemSelectState();
                    }
                }

            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret(false);
        }

        public virtual bool CanEdit()
        {
            bool Result = !FReadOnly;
            if (!Result)
                User.MessageBeep(0);

            return Result;
        }

        public void DeleteItems(int aStartNo, int aEndNo = -1)
        {
            InitializeField();
            DisSelect();  // 防止删除后原选中ItemNo不存在

            int vEndNo;
            if (aEndNo < 0)
                vEndNo = aStartNo;
            else
                vEndNo = aEndNo;

            int vDelCount = vEndNo - aStartNo + 1;
            //FormatItemPrepare(vStartNo, vEndNo);
            Items.RemoveRange(aStartNo, vDelCount);
            //_ReFormatData(vStartNo, vEndNo - vDelCount, -vDelCount);
            if (Items.Count == 0)
            {
                HCCustomItem vItem = CreateDefaultTextItem();
                vItem.ParaFirst = true;
                Items.Add(vItem);  // 不使用InsertText，为避免其触发ReFormat时因为没有格式化过，获取不到对应的DrawItem
            }
            else  // 删除完了还有
            {
                if ((aStartNo > 0) && (!Items[aStartNo].ParaFirst))
                {
                    if (Items[aStartNo - 1].CanConcatItems(Items[aStartNo]))
                    {
                        Items[aStartNo - 1].Text = Items[aStartNo - 1].Text + Items[aStartNo].Text;
                        Items.RemoveAt(aStartNo);
                    }
                }
            }
        }

        /// <summary> 添加Data到当前 </summary>
        /// <param name="aSrcData">源Data</param>
        public void AddData(HCCustomData aSrcData)
        {
            this.InitializeField();

            int vAddStartNo = 0;
            if ((this.Items.Count > 0) && (Items[Items.Count - 1].CanConcatItems(aSrcData.Items[0])))
            {
                Items[Items.Count - 1].Text = Items[Items.Count - 1].Text + aSrcData.Items[0].Text;
                vAddStartNo = 1;
            }
            else
                vAddStartNo = 0;

            for (int i = vAddStartNo; i <= aSrcData.Items.Count - 1; i++)
            {
                if ((aSrcData.Items[i].StyleNo < HCStyle.Null)
                    || ((aSrcData.Items[i].StyleNo > HCStyle.Null) && (aSrcData.Items[i].Text != "")))
                {
                    HCCustomItem vItem = CreateItemByStyle(aSrcData.Items[i].StyleNo);
                    vItem.Assign(aSrcData.Items[i]);
                    //vItem.ParaFirst := False;  // 需要考虑合并
                    vItem.Active = false;
                    vItem.DisSelect();
                    this.Items.Add(vItem);
                }
            }
        }

        /// <summary> 在光标处换行 </summary>
        public bool InsertBreak()
        {
            if (!CanEdit())
                return false;

            KeyEventArgs e = new KeyEventArgs(Keys.Return);
            KeyDown(e);
            InitializeMouseField();  // 201807311101
            return true;
        }

        /// <summary> 在光标处插入字符串(可带回车换行符) </summary>
        public bool InsertText(string aText)
        {
            if (!CanEdit())
                return false;

            bool Result = false;

            Undo_GroupBegin(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            try
            {
                DeleteSelected();

                bool vParaFirst = false;

                string[] vStrings = aText.Split(new string[] { HC.sLineBreak }, StringSplitOptions.None);
                for (int i = 0; i < vStrings.Length; i++)
                {
                    string vS = vStrings[i];
                    if (vParaFirst)
                    {
                        HCCustomItem vTextItem = CreateDefaultTextItem();
                        vTextItem.ParaFirst = true;
                        vTextItem.Text = vS;
                        Result = InsertItem(vTextItem);
                    }
                    else
                        Result = DoInsertText(vS);

                    vParaFirst = true;
                }
            }
            finally
            {
                Undo_GroupEnd(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            }

            InitializeMouseField();  // 201807311101
            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();
            Style.UpdateInfoReScroll();

            return Result;
        }

        /// <summary> 在光标处插入指定行列的表格 </summary>
        public bool InsertTable(int aRowCount, int aColCount)
        {
            if (!CanEdit())
                return false;

            bool Result = false;

            HCRichData vTopData = GetTopLevelData();
            HCTableItem vItem = new HCTableItem(vTopData, aRowCount, aColCount, vTopData.Width);
            Result = InsertItem(vItem);
            InitializeMouseField();

            return Result;
        }

        /// <summary> 在光标处插入直线 </summary>
        public bool InsertLine(int aLineHeight)
        {
            if (!CanEdit())
                return false;

            bool Result = false;

            HCRichData vTopData = GetTopLevelData();
            HCLineItem vItem = new HCLineItem(vTopData, vTopData.Width, aLineHeight);

            Result = InsertItem(vItem);
            InitializeMouseField();

            return Result;
        }

        public bool TableInsertRowAfter(byte aRowCount)
        {
            if (!CanEdit())
                return false;

            InsertProcEventHandler vEvent = delegate(HCCustomItem AItem)
            {
                return (AItem as HCTableItem).InsertRowAfter(aRowCount);
            };

            return TableInsertRC(vEvent);
        }

        public bool TableInsertRowBefor(byte aRowCount)
        {
            if (!CanEdit())
                return false;

            InsertProcEventHandler vEvent = delegate(HCCustomItem AItem)
            {
                return (AItem as HCTableItem).InsertRowBefor(aRowCount);
            };

            return TableInsertRC(vEvent);
        }

        public bool ActiveTableDeleteCurRow()
        {
            if (!CanEdit())
                return false;

            InsertProcEventHandler vEvent = delegate(HCCustomItem AItem)
            {
                return (AItem as HCTableItem).DeleteCurRow();
            };

            return TableInsertRC(vEvent);
        }

        public bool ActiveTableSplitCurRow()
        {
            if (!CanEdit())
                return false;

            InsertProcEventHandler vEvent = delegate(HCCustomItem AItem)
            {
                return (AItem as HCTableItem).SplitCurRow();
            };

            return TableInsertRC(vEvent);
        }

        public bool ActiveTableSplitCurCol()
        {
            if (!CanEdit())
                return false;

            InsertProcEventHandler vEvent = delegate(HCCustomItem AItem)
            {
                return (AItem as HCTableItem).SplitCurCol();
            };

            return TableInsertRC(vEvent);
        }

        public bool TableInsertColAfter(byte aColCount)
        {
            if (!CanEdit())
                return false;

            InsertProcEventHandler vEvent = delegate(HCCustomItem AItem)
            {
                return (AItem as HCTableItem).InsertColAfter(aColCount);
            };

            return TableInsertRC(vEvent);
        }

        public bool TableInsertColBefor(byte aColCount)
        {
            if (!CanEdit())
                return false;

            InsertProcEventHandler vEvent = delegate(HCCustomItem AItem)
            {
                return (AItem as HCTableItem).InsertColBefor(aColCount);
            };

            return TableInsertRC(vEvent);
        }

        public bool ActiveTableDeleteCurCol()
        {
            if (!CanEdit())
                return false;

            InsertProcEventHandler vEvent = delegate(HCCustomItem AItem)
            {
                return (AItem as HCTableItem).DeleteCurCol();
            };

            return TableInsertRC(vEvent);
        }

        public bool MergeTableSelectCells()
        {
            if (!CanEdit())
                return false;

            bool Result = false;
            int vItemNo = GetCurItemNo();
            if (Items[vItemNo].StyleNo == HCStyle.Table)
            {
                Undo_New();
                UndoAction_ItemSelf(vItemNo, HC.OffsetInner);
                Result = (Items[vItemNo] as HCTableItem).MergeSelectCells();
                if (Result)  // 合并成功
                {
                    int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                    _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                    _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);
                    //DisSelect();  // 合并后清空选中，会导致当前ItemNo没有了
                    InitializeMouseField();  // 201807311101
                    Style.UpdateInfoRePaint();
                }
            }

            return Result;
        }

        // Format仅负责格式化Item，ReFormat仅负责格式化后对后面Item和DrawItem的关联处理
        // 目前仅单元格用了，需要放到CellData中吗？
        public void ReFormat(int aStartItemNo)
        {
            if (aStartItemNo > 0)
            {
                _FormatItemPrepare(aStartItemNo, Items.Count - 1);
                FormatData(aStartItemNo, Items.Count - 1);
                DrawItems.DeleteFormatMark();
            }
            else  // 从0开始，适用于处理外部调用提供的方法(非内部操作)引起的Item变化且没处理Item对应的DrawItem的情况
            {
                DrawItems.Clear();
                InitializeField();
                FormatData(0, Items.Count - 1);
                if (SelectInfo.StartItemNo < 0)
                    ReSetSelectAndCaret(0, 0);
                else
                    ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);  // 防止清空后格式化完成后没有选中起始访问出错
            }
        }

        /// <summary> 重新格式化当前段(用于修改了段缩进等) </summary>
        public void ReFormatActiveParagraph()
        {
            if (SelectInfo.StartItemNo >= 0)
            {
                int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                GetParaItemRang(SelectInfo.StartItemNo, ref vFormatFirstItemNo, ref vFormatLastItemNo);
                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);

                Style.UpdateInfoRePaint();
                Style.UpdateInfoReCaret();

                ReSetSelectAndCaret(SelectInfo.StartItemNo);
            }
        }

        /// <summary> 重新格式化当前Item(用于仅修改当前Item属性或内容) </summary>
        public void ReFormatActiveItem()
        {
            if (SelectInfo.StartItemNo >= 0)
            {
                int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                _FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                _ReFormatData(vFormatFirstItemNo, vFormatLastItemNo);

                Style.UpdateInfoRePaint();
                Style.UpdateInfoReCaret();

                if (SelectInfo.StartItemOffset > Items[SelectInfo.StartItemNo].Length)
                    ReSetSelectAndCaret(SelectInfo.StartItemNo);
            }
        }

        public HCCustomItem GetTopLevelItem()
        {
            HCCustomItem Result = GetCurItem();
            if ((Result != null) && (Result.StyleNo < HCStyle.Null))
                Result = (Result as HCCustomRectItem).GetActiveItem();

            return Result;
        }

        public HCCustomDrawItem GetTopLevelDrawItem()
        {
            HCCustomDrawItem Result = null;
            HCCustomItem vItem = GetCurItem();
            if (vItem.StyleNo < HCStyle.Null)
                Result = (vItem as HCCustomRectItem).GetActiveDrawItem();

            if (Result == null)
                Result = GetCurDrawItem();

            return Result;
        }

        public POINT GetActiveDrawItemCoord()
        {
            POINT Result = new POINT(0, 0);
            POINT vPt = new POINT(0, 0);

            HCCustomDrawItem vDrawItem = GetCurDrawItem();
            if (vDrawItem != null)
            {
                Result = vDrawItem.Rect.TopLeft();

                HCCustomItem vItem = GetCurItem();
                if (vItem.StyleNo < HCStyle.Null)
                    vPt = (vItem as HCCustomRectItem).GetActiveDrawItemCoord();

                Result.X = Result.X + vPt.X;
                Result.Y = Result.Y + vPt.Y;
            }

            return Result;
        }

        /// <summary> 取消激活(用于页眉、页脚、正文切换时原激活的取消) </summary>
        public void DisActive()
        {
            this.InitializeField();
            if (Items.Count > 0)
            {
                HCCustomItem vItem = GetCurItem();
                if (vItem != null)
                    vItem.Active = false;
            }
        }

        public string GetHint()
        {
            if ((!FMouseMoveRestrain) && (FMouseMoveItemNo >= 0))
                return Items[FMouseMoveItemNo].GetHint();

            return "";
        }

        /// <summary> 返回当前光标处的顶层Data </summary>
        public HCRichData GetTopLevelData()
        {
            HCRichData Result = null;
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

        /// <summary> 返回指定位置处的顶层Data </summary>
        public HCRichData GetTopLevelDataAt(int x, int y)
        {
            HCRichData Result = null;
            int vItemNo = -1, vOffset = -1, vDrawItemNo = -1;
            bool vRestrain = false;
            GetItemAt(x, y, ref vItemNo, ref vOffset, ref vDrawItemNo, ref vRestrain);
            if ((!vRestrain) && (vItemNo >= 0))
            {
                if (Items[vItemNo].StyleNo < HCStyle.Null)
                {
                    int vX = -1, vY = -1;
                    CoordToItemOffset(x, y, vItemNo, vOffset, ref vX, ref vY);
                    Result = (Items[vItemNo] as HCCustomRectItem).GetTopLevelDataAt(vX, vY) as HCRichData;
                }
            }

            if (Result == null)
                Result = this;

            return Result;
        }

        public int MouseDownItemNo
        {
            get { return FMouseDownItemNo; }
        }

        public int MouseDownItemOffset
        {
            get { return FMouseDownItemOffset; }
        }

        public int MouseMoveItemNo
        {
            get { return FMouseMoveItemNo; }
        }

        public int MouseMoveItemOffset
        {
            get { return FMouseMoveItemOffset; }
        }

        public bool MouseMoveRestrain
        {
            get { return FMouseMoveRestrain; }
        }

        public int Width
        {
            get { return GetWidth(); }
            set { SetWidth(value); }
        }

        public int Height
        {
            get { return GetHeight(); }
        }

        public bool ReadOnly
        {
            get { return FReadOnly; }
            set { SetReadOnly(value); }
        }

        public bool Selecting
        {
            get { return FSelecting; }
        }

        public DataItemEventHandler OnItemResized
        {
            get { return FOnItemResized; }
            set { FOnItemResized = value; }
        }

        public ItemMouseEventHandler OnItemMouseDown
        {
            get { return FOnItemMouseDown; }
            set { FOnItemMouseDown = value; }
        }

        public ItemMouseEventHandler OnItemMouseUp
        {
            get { return FOnItemMouseUp; }
            set { FOnItemMouseUp = value; }
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

        public EventHandler OnCreateItem
        {
            get { return FOnCreateItem; }
            set { FOnCreateItem = value; }
        }
    }
}
