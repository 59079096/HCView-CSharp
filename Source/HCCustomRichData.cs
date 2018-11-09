/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{             文档内各类对象基本管理单元                }
{                                                       }
{*******************************************************/

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using HC.Win32;

namespace HC.View
{
    public delegate bool InsertProcEventHandler(HCCustomItem AItem);

    public delegate void DrawItemPaintEventHandler(HCCustomData AData, int ADrawItemNo, RECT ADrawRect,
        int ADataDrawLeft, int ADataDrawBottom, int ADataScreenTop, int ADataScreenBottom,
        HCCanvas ACanvas, PaintInfo APaintInfo);

    public delegate void ItemMouseEventHandler(HCCustomData AData, int AItemNo, MouseEventArgs e);

    public delegate void DataItemEventHandler(HCCustomData AData, int AItemNo);

    public class HCCustomRichData : HCCustomData
    {
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
            FSelectSeekNo,
            FSelectSeekOffset;  // 选中操作时的游标

        private bool FReadOnly, FSelecting, FDraging;

        private DataItemEventHandler FOnItemResized;

        private ItemNotifyEventHandler FOnInsertItem;

        private ItemMouseEventHandler FOnItemMouseDown, FOnItemMouseUp;

        private DrawItemPaintEventHandler FOnDrawItemPaintBefor, FOnDrawItemPaintAfter;

        private EventHandler FOnCreateItem;  // 新建了Item(目前主要是为了打字和用中文输入法输入英文时痕迹的处理)

        private void FormatData(int AStartItemNo, int ALastItemNo)
        {
            int vPrioDrawItemNo = 0;
            POINT vPos = new POINT();

            _FormatReadyParam(AStartItemNo, ref vPrioDrawItemNo, ref vPos);

            for (int i = AStartItemNo; i <= ALastItemNo; i++)
            {
                _FormatItemToDrawItems(i, 1, FWidth, ref vPos, ref vPrioDrawItemNo);
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
        private bool EmptyDataInsertItem(HCCustomItem AItem)
        {
            if ((AItem.StyleNo > HCStyle.Null) && (AItem.Text == ""))
                return false;

            Undo_DeleteItem(0, 0);
            Items.Clear();
            DrawItems.Clear();
            AItem.ParaFirst = true;
            Items.Add(AItem);
            Undo_InsertItem(0, 0);
            FormatData(0, 0);
            ReSetSelectAndCaret(0);
            return true;
        }

        /// <summary> 为避免表格插入行、列大量重复代码 </summary>
        private bool TableInsertRC(InsertProcEventHandler AProc)
        {
            bool Result = false;
            int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
            int vCurItemNo = GetCurItemNo();
            if (Items[vCurItemNo] is HCTableItem)
            {
                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, vCurItemNo, 0);
                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                Result = AProc(Items[vCurItemNo]);
                if (Result)
                {
                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo, 0);
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

        protected void Undo_StartGroup(int AItemNo, int  AOffset)
        {
            if (EnableUndo())
                GetUndoList().BeginUndoGroup(AItemNo, AOffset);
        }

        protected void Undo_EndGroup(int AItemNo, int  AOffset)
        {
            if (EnableUndo())
                GetUndoList().EndUndoGroup(AItemNo, AOffset);
        }

        protected void Undo_StartRecord()
        {
            if (EnableUndo())
                GetUndoList().NewUndo();
        }

        /// <summary> 删除Text </summary>
        /// <param name="AItemNo">操作发生时的ItemNo</param>
        /// <param name="AOffset">删除的起始位置</param>
        /// <param name="AText"></param>
        protected void Undo_DeleteText(int AItemNo, int  AOffset, string AText)
        {
            if (EnableUndo())
            {
                HCUndoList vUndoList = GetUndoList();
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCTextUndoAction vTextAction = vUndo.ActionAppend(UndoActionTag.uatDeleteText, AItemNo, AOffset) as HCTextUndoAction;
                    vTextAction.Text = AText;
                }
            }
        }

        protected void Undo_InsertText(int AItemNo, int  AOffset, string AText)
        {
            if (EnableUndo())
            {
                HCUndoList vUndoList = GetUndoList();
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCTextUndoAction vTextAction = vUndo.ActionAppend(UndoActionTag.uatInsertText, AItemNo, AOffset) as HCTextUndoAction;
                    vTextAction.Text = AText;
                }
            }
        }

        /// <summary> 删除指定的Item </summary>
        /// <param name="AItemNo">操作发生时的ItemNo</param>
        /// <param name="AOffset">操作发生时的Offset</param>
        protected void Undo_DeleteItem(int AItemNo, int  AOffset)
        {
            if (EnableUndo())
            {
                HCUndoList vUndoList = GetUndoList();
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCItemUndoAction vItemAction = vUndo.ActionAppend(UndoActionTag.uatDeleteItem, AItemNo, AOffset) as HCItemUndoAction;
                    SaveItemToStreamAlone(Items[AItemNo], (Stream)vItemAction.ItemStream);
                }
            }
        }

        /// <summary> 插入Item到指定位置 </summary>
        /// <param name="AItemNo">操作发生时的ItemNo</param>
        /// <param name="AOffset">操作发生时的Offset</param>
        protected void Undo_InsertItem(int AItemNo, int  AOffset)
        {
            if (EnableUndo())
            {
                HCUndoList vUndoList = GetUndoList();
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCItemUndoAction vItemAction = vUndo.ActionAppend(UndoActionTag.uatInsertItem, AItemNo, AOffset) as HCItemUndoAction;
                    SaveItemToStreamAlone(Items[AItemNo], (Stream)vItemAction.ItemStream);
                }
            }
        }

        protected void Undo_ItemParaFirst(int AItemNo, int  AOffset, bool ANewParaFirst)
        {
            if (EnableUndo())
            {
                HCUndoList vUndoList = GetUndoList();
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCItemParaFirstUndoAction vItemAction = new HCItemParaFirstUndoAction();
                    vItemAction.ItemNo = AItemNo;
                    vItemAction.Offset = AOffset;
                    vItemAction.OldParaFirst = Items[AItemNo].ParaFirst;
                    vItemAction.NewParaFirst = ANewParaFirst;
                    vUndo.Actions.Add(vItemAction);
                }
            }
        }

        protected void Undo_ItemSelf(int AItemNo, int  AOffset)
        {
            if (EnableUndo())
            {
                HCUndoList vUndoList = GetUndoList();
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    vUndo.ActionAppend(UndoActionTag.uatItemSelf, AItemNo, AOffset);
                }
            }
        }

        protected virtual HCCustomItem CreateItemByStyle(int AStyleNo)
        {
            HCCustomItem Result = null;
            if (AStyleNo < HCStyle.Null)
            {
                switch (AStyleNo)
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
                    
                    default:
                        throw new Exception("未找到类型 " + AStyleNo.ToString() + " 对应的创建Item代码！");
                        break;
                }
            }
            else
            {
                Result = CreateDefaultTextItem();
                Result.StyleNo = AStyleNo;
            }

            return Result;
        }

        // <summary> 设置光标位置到指定的Item最后面 </summary>
        protected void ReSetSelectAndCaret(int AItemNo)
        {
            ReSetSelectAndCaret(AItemNo, GetItemAfterOffset(AItemNo));
        }

        /// <summary> 设置光标位置到指定的Item指定位置 </summary>
        /// <param name="AItemNo">指定ItemNo</param>
        /// <param name="AOffset">指定位置</param>
        /// <param name="ANextWhenMid">如果此位置前后的DrawItem正好分行，True后一个DrawItem前面，False前一个后面</param>
        protected void ReSetSelectAndCaret(int AItemNo, int AOffset, bool ANextWhenMid = false)
        {
            SelectInfo.StartItemNo = AItemNo;
            SelectInfo.StartItemOffset = AOffset;

            int vDrawItemNo = GetDrawItemNoByOffset(AItemNo, AOffset);
            if (    (ANextWhenMid)
                    && (vDrawItemNo < DrawItems.Count - 1)
                    && (DrawItems[vDrawItemNo + 1].ItemNo == AItemNo)
                    && (DrawItems[vDrawItemNo + 1].CharOffs == AOffset + 1))
               vDrawItemNo++;

          CaretDrawItemNo = vDrawItemNo;
        }

        /// <summary> 当前Item对应的格式化起始Item和结束Item(段最后一个Item) </summary>
        /// <param name="AFirstItemNo">起始ItemNo</param>
        /// <param name="ALastItemNo">结束ItemNo</param>
        protected void GetReformatItemRange(ref int AFirstItemNo, ref int ALastItemNo)
        {
            GetReformatItemRange(ref AFirstItemNo, ref ALastItemNo, SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
        }

        /// <summary> 指定Item对应的格式化起始Item和结束Item(段最后一个Item) </summary>
        /// <param name="AFirstItemNo">起始ItemNo</param>
        /// <param name="ALastItemNo">结束ItemNo</param>
        protected void GetReformatItemRange(ref int AFirstItemNo, ref int ALastItemNo, int AItemNo, int  AItemOffset)
        {
            if ((AItemNo > 0)
                && DrawItems[Items[AItemNo].FirstDItemNo].LineFirst
                && (AItemOffset == 0))
            {
                if (!Items[AItemNo].ParaFirst)
                    AFirstItemNo = GetLineFirstItemNo(AItemNo - 1, Items[AItemNo - 1].Length);
                else  // 是段首
                    AFirstItemNo = AItemNo;
            }
            else
                AFirstItemNo = GetLineFirstItemNo(AItemNo, 0);  // 取行第一个DrawItem对应的ItemNo
                
            ALastItemNo = GetParaLastItemNo(AItemNo);
        }

        /// <summary>
        /// 合并2个文本Item
        /// </summary>
        /// <param name="ADestItem">合并后的Item</param>
        /// <param name="ASrcItem">源Item</param>
        /// <returns>True:合并成功，False不能合并</returns>
        protected virtual bool MergeItemText(HCCustomItem ADestItem, HCCustomItem  ASrcItem)
        {
            bool Result = ADestItem.CanConcatItems(ASrcItem);
            if (Result)
                ADestItem.Text += ASrcItem.Text;

            return Result;
        }

        protected override void DoDrawItemPaintBefor(HCCustomData AData, int ADrawItemNo, 
            RECT ADrawRect, int ADataDrawLeft, int ADataDrawBottom, int ADataScreenTop, 
            int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoDrawItemPaintBefor(AData, ADrawItemNo, ADrawRect, ADataDrawLeft,
                ADataDrawBottom, ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);

            if (FOnDrawItemPaintBefor != null)
                FOnDrawItemPaintBefor(AData, ADrawItemNo, ADrawRect, ADataDrawLeft, ADataDrawBottom, ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);
        }

        protected override void DoDrawItemPaintAfter(HCCustomData AData, int ADrawItemNo, 
            RECT ADrawRect, int ADataDrawLeft, int ADataDrawBottom, int ADataScreenTop,
            int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoDrawItemPaintAfter(AData, ADrawItemNo, ADrawRect, ADataDrawLeft,
                ADataDrawBottom, ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);

            if (FOnDrawItemPaintAfter != null)
                FOnDrawItemPaintAfter(AData, ADrawItemNo, ADrawRect, ADataDrawLeft, ADataDrawBottom, ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);
        }

        protected virtual bool CanDeleteItem(int AItemNo)
        {
            return CanEdit();
        }

        /// <summary> 用于从流加载完Items后，检查不合格的Item并删除 </summary>
        protected virtual int CheckInsertItemCount(int AStartNo, int  AEndNo)
        {
            return AEndNo - AStartNo + 1;
        }

        protected virtual void DoItemInsert(HCCustomItem AItem)
        {
            if (FOnInsertItem != null)
                FOnInsertItem(AItem);
        }

        protected virtual void DoItemMouseLeave(int AItemNo)
        {
            Items[AItemNo].MouseLeave();
        }

        protected virtual void DoItemMouseEnter(int AItemNo)
        {
            Items[AItemNo].MouseEnter();
        }

        protected void DoItemResized(int AItemNo)
        {
            if (FOnItemResized != null)
                FOnItemResized(this, AItemNo);
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
                    Undo_StartRecord();
                    Undo_InsertText(vCarteItemNo, SelectInfo.StartItemOffset + 1, AText);
                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

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

                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
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
        
        protected bool DoInsertText(string AText)
        {
            bool Result = false;
            if (AText != "")
            {
                int vCarteItemNo = GetCurItemNo();
                HCCustomItem vCarteItem = Items[vCarteItemNo];
                int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                if (vCarteItem.StyleNo < HCStyle.Null)
                {
                    if (SelectInfo.StartItemOffset == HC.OffsetInner)
                    {
                        Undo_StartRecord();
                        Undo_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);

                        HCCustomRectItem vRectItem = vCarteItem as HCCustomRectItem;
                        Result = vRectItem.InsertText(AText);
                        if (vRectItem.SizeChanged)
                        {
                            vRectItem.SizeChanged = false;
                            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                            FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                        }
                    }
                    else
                    if (SelectInfo.StartItemOffset == HC.OffsetAfter)
                    {
                        if ((vCarteItemNo < Items.Count - 1)
                            && (Items[vCarteItemNo + 1].StyleNo > HCStyle.Null)
                            && (!Items[vCarteItemNo + 1].ParaFirst))
                        // 下一个是TextItem且不是段首，则合并到下一个开始
                        {
                            vCarteItemNo++;
                            SelectInfo.StartItemNo = vCarteItemNo;
                            SelectInfo.StartItemOffset = 0;
                            this.Style.CurStyleNo = Items[vCarteItemNo].StyleNo;
                            Result = DoTextItemInsert(vCarteItemNo, AText, ref vFormatFirstItemNo, ref vFormatLastItemNo);  // 在下一个TextItem前面插入
                        }
                        else  // 最后或下一个还是RectItem或当前是段尾
                        {
                            HCCustomItem vNewItem = CreateDefaultTextItem();
                            vNewItem.Text = AText;
                            SelectInfo.StartItemNo = vCarteItemNo + 1;
                            Result = InsertItem(SelectInfo.StartItemNo, vNewItem, false);  // 在两个RectItem中间插入
                        }
                    }
                    else
                    {
                        if ((vCarteItemNo > 0)
                            && (Items[vCarteItemNo - 1].StyleNo > HCStyle.Null)
                            && (!Items[vCarteItemNo].ParaFirst))
                        // 前一个是TextItem，当前不是段首，合并到前一个尾
                        {
                            vCarteItemNo--;
                            SelectInfo.StartItemNo = vCarteItemNo;
                            SelectInfo.StartItemOffset = Items[vCarteItemNo].Length;
                            this.Style.CurStyleNo = Items[vCarteItemNo].StyleNo;
                            Result = DoTextItemInsert(vCarteItemNo, AText, ref vFormatFirstItemNo, ref vFormatLastItemNo);  // 在前一个后面插入
                        }
                        else  // 最前或前一个还是RectItem
                        {
                            HCCustomItem vNewItem = CreateDefaultTextItem();
                            vNewItem.Text = AText;
                            Result = InsertItem(SelectInfo.StartItemNo, vNewItem, true);  // 在两个RectItem中间插入
                        }
                    }
                }
                else
                    Result = DoTextItemInsert(vCarteItemNo, AText, ref vFormatFirstItemNo, ref vFormatLastItemNo);
            }
            else
                Result = InsertBreak();

            return Result;
        }

        protected virtual int GetWidth()
        {
            return FWidth;
        }

        protected void SetWidth(int Value)
        {
            if (FWidth != Value)
                FWidth = Value;
        }

        protected virtual int GetHeight()
        {
            return CalcContentHeight();
        }

        protected virtual void SetReadOnly(bool Value)
        {
            FReadOnly = Value;
        }

        /// <summary> 准备格式化参数 </summary>
        /// <param name="AStartItemNo">开始格式化的Item</param>
        /// <param name="APrioDItemNo">上一个Item的最后一个DrawItemNo</param>
        /// <param name="APos">开始格式化位置</param>
        protected virtual void _FormatReadyParam(int AStartItemNo, ref int APrioDrawItemNo, ref POINT APos)
        {
            if (AStartItemNo > 0)
            {
                APrioDrawItemNo = GetItemLastDrawItemNo(AStartItemNo - 1);  // 上一个最后的DItem
                if (Items[AStartItemNo].ParaFirst)
                {
                    APos.X = 0;
                    APos.Y = DrawItems[APrioDrawItemNo].Rect.Bottom;
                }
                else
                {
                    APos.X = DrawItems[APrioDrawItemNo].Rect.Right;
                    APos.Y = DrawItems[APrioDrawItemNo].Rect.Top;
                }
            }
            else  // 是第一个
            {
                APrioDrawItemNo = -1;
                APos.X = 0;
                APos.Y = 0;
            }
        }

        // Format仅负责格式化Item，ReFormat负责格式化后对后面Item和DrawItem的关联处理
        protected virtual void ReFormatData_(int AStartItemNo, int ALastItemNo = -1, int AExtraItemCount = 0)
        {
            int vDrawItemCount = DrawItems.Count;
            if (ALastItemNo < 0)
                FormatData(AStartItemNo, AStartItemNo);
            else
                FormatData(AStartItemNo, ALastItemNo);  // 格式化指定范围内的Item
            DrawItems.DeleteFormatMark();
            vDrawItemCount = DrawItems.Count - vDrawItemCount;

            // 计算格式化后段的底部位置变化
            int vLastDrawItemNo = -1;
            if (ALastItemNo < 0)
                vLastDrawItemNo = GetItemLastDrawItemNo(AStartItemNo);
            else
                vLastDrawItemNo = GetItemLastDrawItemNo(ALastItemNo);
            int vFormatIncHight = DrawItems[vLastDrawItemNo].Rect.Bottom - DrawItems.FormatBeforBottom;  // 段格式化后，高度的增量

            // 某段格式化后，处理对其后面Item对应DrawItem的影响
            // 由图2017-6-8_1变为图2017-6-8_2的过程中，第3段位置没变，也没有新的Item数量变化，
            // 但是DrawItem的数量有变化
            // 第3段Item对应的FirstDItemNo需要修改，所以此处增加DrawItemCount数量的变化
            // 目前格式化时ALastItemNo为段的最后一个，所以vLastDrawItemNo为段最后一个DrawItem
            if ((vFormatIncHight != 0) || (AExtraItemCount != 0) || (vDrawItemCount != 0))
            {
                if (DrawItems.Count > vLastDrawItemNo)
                {
                    int vLastItemNo = -1;
                    for (int i = vLastDrawItemNo + 1; i < DrawItems.Count; i++)  // 从格式化变动段的下一段开始
                    {
                        // 处理格式化后面各DrawItem对应的ItemNo偏移
                        DrawItems[i].ItemNo = DrawItems[i].ItemNo + AExtraItemCount;
                        if (vLastItemNo != DrawItems[i].ItemNo)
                        {
                            vLastItemNo = DrawItems[i].ItemNo;
                            Items[vLastItemNo].FirstDItemNo = i;
                        }

                        if (vFormatIncHight != 0)
                        {
                            // 将原格式化因分页等原因引起的整体下移或增加的高度恢复回来
                            // 如果不考虑上面处理ItemNo的偏移，可将TTableCellData.ClearFormatExtraHeight方法写到基类，这里直接调用
                            int vFmtTopOffset = -1;
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

        protected virtual bool EnableUndo()
        {
            return Style.EnableUndo;
        }

        // Item单独保存和读取事件
        protected void SaveItemToStreamAlone(HCCustomItem AItem, Stream AStream)
        {
            HC._SaveFileFormatAndVersion(AStream);
            AItem.SaveToStream(AStream);
            if (AItem.StyleNo > HCStyle.Null)
                Style.TextStyles[AItem.StyleNo].SaveToStream(AStream);

            Style.ParaStyles[AItem.ParaNo].SaveToStream(AStream);
        }

        protected HCCustomItem LoadItemFromStreamAlone(Stream AStream)
        {
            string vFileExt = "";
            ushort vFileVersion = 0;
            byte vLan = 0;

            AStream.Position = 0;
            HC._LoadFileFormatAndVersion(AStream, ref vFileExt, ref vFileVersion, ref vLan);  // 文件格式和版本
            if ((vFileExt != HC.HC_EXT) && (vFileExt != "cff."))
                throw new Exception("加载失败，不是" + HC.HC_EXT + "文件！");

            int vStyleNo = HCStyle.Null;
            byte[] vBuffer = BitConverter.GetBytes(vStyleNo);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            vStyleNo = BitConverter.ToInt32(vBuffer, 0);

            HCCustomItem Result = CreateItemByStyle(vStyleNo);
            Result.LoadFromStream(AStream, Style, vFileVersion);

            if (vStyleNo > HCStyle.Null)
            {
                HCTextStyle vTextStyle = new HCTextStyle();
                try
                {
                    vTextStyle.LoadFromStream(AStream, vFileVersion);
                    vStyleNo = Style.GetStyleNo(vTextStyle, true);
                    Result.StyleNo = vStyleNo;
                }
                finally
                {
                    //vTextStyle.;
                }
            }

            int vParaNo = -1;
            HCParaStyle vParaStyle = new HCParaStyle();
            try
            {
                vParaStyle.LoadFromStream(AStream, vFileVersion);
                vParaNo = Style.GetParaNo(vParaStyle, true);
            }
            finally
            {
                vParaStyle.Dispose();
            }

            Result.ParaNo = vParaNo;

            return Result;
        }

        protected int CalcContentHeight()
        {
            if (DrawItems.Count > 0)
                return DrawItems[DrawItems.Count - 1].Rect.Bottom - DrawItems[0].Rect.Top;
            else
                return 0;
        }

        public HCCustomRichData(HCStyle AStyle) : base(AStyle)
        {
            Items.OnItemInsert = DoItemInsert;
            FReadOnly = false;
            InitializeField();
            SetEmptyData();
        }

        public override void Clear()
        {
            if (Items.Count > 0)
            {
                Undo_StartRecord();
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    Undo_DeleteItem(i, 0);
                }
            }

            InitializeField();

            base.Clear();
            SetEmptyData();
        }

        // 选中内容应用样式

#region MergeItemToPrio 当前Item成功合并到同段前一个Item
        private bool MergeItemToPrio(int AItemNo)
        {
            return (AItemNo > 0) && (!Items[AItemNo].ParaFirst) && MergeItemText(Items[AItemNo - 1], Items[AItemNo]);
        }
#endregion

#region MergeItemToNext 同段后一个Item成功合并到当前Item
        private bool MergeItemToNext(int AItemNo)
        {
            return (AItemNo < Items.Count - 1) && (!Items[AItemNo + 1].ParaFirst) && MergeItemText(Items[AItemNo], Items[AItemNo + 1]);
        }
#endregion

#region ApplySameItem选中在同一个Item
        private void ApplySameItem(int AItemNo, ref int vExtraCount, HCStyleMatch AMatchStyle)
        {
            HCCustomItem vItem = Items[AItemNo];
            if (vItem.StyleNo < HCStyle.Null)
            {
                (vItem as HCCustomRectItem).ApplySelectTextStyle(Style, AMatchStyle);
            }
            else  // 文本
            {
                int vStyleNo = AMatchStyle.GetMatchStyleNo(Style, vItem.StyleNo);
                if (vItem.IsSelectComplate)
                {
                    vItem.StyleNo = vStyleNo;  // 直接修改样式编号
                    if (MergeItemToNext(AItemNo))
                    {
                        Items.RemoveAt(AItemNo + 1);
                        vExtraCount--;
                    }
                    if (AItemNo > 0)
                    {
                        int vLen = Items[AItemNo - 1].Length;
                        if (MergeItemToPrio(AItemNo))
                        {
                            Items.RemoveAt(AItemNo);
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
                    HCCustomItem vAfterItem = Items[AItemNo].BreakByOffset(SelectInfo.EndItemOffset);  // 后半部分对应的Item
                    if (vAfterItem != null)
                    {
                        Items.Insert(AItemNo + 1, vAfterItem);
                        vExtraCount++;
                    }
                    
                    if (vsBefor != "")
                    {
                        vItem.Text = vsBefor;  // 保留前半部分文本
                            
                        // 创建选中文本对应的Item
                        HCCustomItem vSelItem = CreateDefaultTextItem();
                        vSelItem.ParaNo = vItem.ParaNo;
                        vSelItem.StyleNo = vStyleNo;
                        vSelItem.Text = vSelText;

                        if (vAfterItem != null)
                        {
                            Items.Insert(AItemNo + 1, vSelItem);
                            vExtraCount++;
                        }
                        else  // 没有后半部分，说明选中需要和后面判断合并
                        {
                            if ((AItemNo < Items.Count - 1)
                                && (!Items[AItemNo + 1].ParaFirst)
                                && MergeItemText(vSelItem, Items[AItemNo + 1]))
                            {
                                Items[AItemNo + 1].Text = vSelText + Items[AItemNo + 1].Text;
                                SelectInfo.StartItemNo = AItemNo + 1;
                                SelectInfo.StartItemOffset = 0;
                                SelectInfo.EndItemNo = AItemNo + 1;
                                SelectInfo.EndItemOffset = vSelText.Length;
                                
                                return;
                            }

                            Items.Insert(AItemNo + 1, vSelItem);
                            vExtraCount++;
                        }

                        SelectInfo.StartItemNo = AItemNo + 1;
                        SelectInfo.StartItemOffset = 0;
                        SelectInfo.EndItemNo = AItemNo + 1;
                        SelectInfo.EndItemOffset = vSelText.Length;
                    }
                    else  // 选择起始位置是Item最开始
                    {
                        //vItem.Text := vSelText;  // BreakByOffset已经保留选中部分文本
                        vItem.StyleNo = vStyleNo;
                        
                        if (MergeItemToPrio(AItemNo))
                        {
                            Items.RemoveAt(AItemNo);
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
        private void ApplyRangeStartItem(int AItemNo, ref int vExtraCount, HCStyleMatch AMatchStyle)
        {
            HCCustomItem vItem = Items[AItemNo];
            if (vItem.StyleNo < HCStyle.Null)
                (vItem as HCCustomRectItem).ApplySelectTextStyle(Style, AMatchStyle);
            else  // 文本
            {
                int vStyleNo = AMatchStyle.GetMatchStyleNo(Style, vItem.StyleNo);
                if (vItem.StyleNo != vStyleNo)
                {
                    if (vItem.IsSelectComplate)
                        vItem.StyleNo = vStyleNo;
                    else  // Item部分选中
                    {
                        HCCustomItem vAfterItem = Items[AItemNo].BreakByOffset(SelectInfo.StartItemOffset);  // 后半部分对应的Item
                        vAfterItem.StyleNo = vStyleNo;
                        Items.Insert(AItemNo + 1, vAfterItem);
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
        private void ApplyRangeEndItem(int AItemNo, ref int vExtraCount, HCStyleMatch AMatchStyle)
        {
            HCCustomItem vItem = Items[AItemNo];
            if (vItem.StyleNo < HCStyle.Null)
                (vItem as HCCustomRectItem).ApplySelectTextStyle(Style, AMatchStyle);
            else  // 文本
            {
                int vStyleNo = AMatchStyle.GetMatchStyleNo(Style, vItem.StyleNo);
                if (vItem.StyleNo != vStyleNo)
                {
                    if (vItem.IsSelectComplate)
                        vItem.StyleNo = vStyleNo;
                    else  // Item部分选中了
                    {
                        string vText = vItem.Text;
                        string vSelText = vText.Substring(1 - 1, SelectInfo.EndItemOffset); // 选中的文本
                        vItem.Text = vText.Remove(1 - 1, SelectInfo.EndItemOffset); ;

                        HCCustomItem vBeforItem = CreateDefaultTextItem();
                        vBeforItem.ParaNo = vItem.ParaNo;
                        vBeforItem.StyleNo = vStyleNo;
                        vBeforItem.Text = vSelText;  // 创建前半部分文本对应的Item
                        vBeforItem.ParaFirst = vItem.ParaFirst;
                        vItem.ParaFirst = false;

                        Items.Insert(AItemNo, vBeforItem);
                        vExtraCount++;
                    }
                }
            }
        }
#endregion

#region ApplyNorItem选中在不同Item，处理中间Item
        private void ApplyRangeNorItem(int AItemNo, HCStyleMatch AMatchStyle)
        {
            HCCustomItem vItem = Items[AItemNo];
            if (vItem.StyleNo < HCStyle.Null)  // 非文本
                (vItem as HCCustomRectItem).ApplySelectTextStyle(Style, AMatchStyle);
            else  // 文本
                vItem.StyleNo = AMatchStyle.GetMatchStyleNo(Style, vItem.StyleNo);
        }
#endregion

        public override int ApplySelectTextStyle(HCStyleMatch AMatchStyle)
        {
            this.InitializeField();
            int vExtraCount = 0, vFormatFirstItemNo = -1, vFormatLastItemNo = -1;

            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
            if (!SelectExists())
            {
                if (Style.CurStyleNo > HCStyle.Null)
                {
                    AMatchStyle.Append = !AMatchStyle.StyleHasMatch(Style, Style.CurStyleNo);  // 根据当前判断是添加样式还是减掉样式
                    Style.CurStyleNo = AMatchStyle.GetMatchStyleNo(Style, Style.CurStyleNo);

                    Style.UpdateInfoRePaint();
                    if (Items[SelectInfo.StartItemNo].Length == 0)
                    {
                        FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                        Items[SelectInfo.StartItemNo].StyleNo = Style.CurStyleNo;
                        ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                        Style.UpdateInfoReCaret();
                    }
                    else  // 不是空行
                    {
                        if (Items[SelectInfo.StartItemNo] is HCTextRectItem)
                        {
                            FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                            (Items[SelectInfo.StartItemNo] as HCTextRectItem).TextStyleNo = Style.CurStyleNo;
                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                        }

                        Style.UpdateInfoReCaret(false);
                    }
                }

                return HCStyle.Null;
            }

            if (SelectInfo.EndItemNo < 0)
            {
                if (Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null)
                {
                    // 如果改变会引起RectItem宽度变化，则需要格式化到最后一个Item
                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                    (Items[SelectInfo.StartItemNo] as HCCustomRectItem).ApplySelectTextStyle(Style, AMatchStyle);
                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                }
            }
            else  // 有连续选中内容
            {
                vFormatLastItemNo = GetParaLastItemNo(SelectInfo.EndItemNo);
                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)
                {
                    if (Items[i].StyleNo > HCStyle.Null)
                    {
                        AMatchStyle.Append = !AMatchStyle.StyleHasMatch(Style, Items[i].StyleNo);  // 根据第一个判断是添加样式还是减掉样式
                        break;
                    }
                    else
                    if (Items[i] is HCTextRectItem)
                    {
                        AMatchStyle.Append = !AMatchStyle.StyleHasMatch(Style, (Items[i] as HCTextRectItem).TextStyleNo);  // 根据第一个判断是添加样式还是减掉样式
                        break;
                    }
                }

                if (SelectInfo.StartItemNo == SelectInfo.EndItemNo)
                    ApplySameItem(SelectInfo.StartItemNo, ref vExtraCount, AMatchStyle);
                else  // 选中发生在不同的Item，采用先处理选中范围内样式改变，再处理合并，再处理选中内容全、部分选中状态
                {
                    ApplyRangeEndItem(SelectInfo.EndItemNo, ref vExtraCount, AMatchStyle);
                    for (int i = SelectInfo.EndItemNo - 1; i >= SelectInfo.StartItemNo + 1; i--)
                        ApplyRangeNorItem(i, AMatchStyle);  // 处理每一个Item的样式
                    ApplyRangeStartItem(SelectInfo.StartItemNo, ref vExtraCount, AMatchStyle);

                    /* 样式变化后，从后往前处理选中范围内变化后的合并 }*/
                    if (SelectInfo.EndItemNo < vFormatLastItemNo + vExtraCount)
                    {
                        if (MergeItemToNext(SelectInfo.EndItemNo))
                        {
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

                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo + vExtraCount, vExtraCount);
            }

            MatchItemSelectState();
            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();

            return 0;
        }

#region DoApplyParaStyle
        private void DoApplyParaStyle(int AItemNo, HCParaMatch AMatchStyle)
        {
            if (GetItemStyle(AItemNo) < HCStyle.Null)
                (Items[AItemNo] as HCCustomRectItem).ApplySelectParaStyle(this.Style, AMatchStyle);
            else
            {
                int vFirstNo = -1, vLastNo = -1;
                GetParaItemRang(AItemNo, ref vFirstNo, ref vLastNo);
                int vParaNo = AMatchStyle.GetMatchParaNo(this.Style, GetItemParaStyle(AItemNo));
                if (GetItemParaStyle(vFirstNo) != vParaNo)
                {
                    for (int i = vFirstNo; i <= vLastNo; i++)
                        Items[i].ParaNo = vParaNo;
                }
            }
        }
#endregion

#region ApplyParaSelectedRangStyle
        private void ApplyParaSelectedRangStyle(HCParaMatch AMatchStyle)
        {
            int vFirstNo = -1, vLastNo = -1;
            GetParaItemRang(SelectInfo.StartItemNo, ref vFirstNo, ref vLastNo);
            DoApplyParaStyle(SelectInfo.StartItemNo, AMatchStyle);

            int i = vLastNo + 1;
            while (i <= SelectInfo.EndItemNo)
            {
                if (Items[i].ParaFirst)
                    DoApplyParaStyle(i, AMatchStyle);

                i++;
            }
        }
#endregion

        public override int ApplySelectParaStyle(HCParaMatch AMatchStyle)
        {
            if (SelectInfo.StartItemNo < 0)
                return HCStyle.Null;

            //GetReformatItemRange(vFormatFirstItemNo, vFormatLastItemNo);
            int vFormatLastItemNo = -1;
            int vFormatFirstItemNo = GetParaFirstItemNo(SelectInfo.StartItemNo);

            if (SelectInfo.EndItemNo >= 0)
            {
                vFormatLastItemNo = GetParaLastItemNo(SelectInfo.EndItemNo);
                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                ApplyParaSelectedRangStyle(AMatchStyle);
                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
            }
            else  // 没有选中内容
            {
                vFormatLastItemNo = GetParaLastItemNo(SelectInfo.StartItemNo);
                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                DoApplyParaStyle(SelectInfo.StartItemNo, AMatchStyle);  // 应用样式
                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
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
            Undo_DeleteItem(SelectInfo.StartItemNo, 0);
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
                        Undo_InsertItem(SelectInfo.StartItemNo, 0);
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
                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                    if ((Items[SelectInfo.StartItemNo] as HCCustomRectItem).IsSelectComplateTheory())
                    {
                        Undo_StartRecord();
                        GetParaItemRang(SelectInfo.StartItemNo, ref vParaFirstItemNo, ref vParaLastItemNo);
                        Result = DeleteItemSelectComplate(ref vDelCount, vParaFirstItemNo, vParaLastItemNo);
                    }
                    else
                        Result = (Items[SelectInfo.StartItemNo] as HCCustomRectItem).DeleteSelected();

                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
                }
                else  // 选中不是发生在RectItem内部
                {
                    HCCustomItem vEndItem = Items[SelectInfo.EndItemNo];  // 选中结束Item
                    if (SelectInfo.EndItemNo == SelectInfo.StartItemNo)
                    {
                        Undo_StartRecord();
                        GetParaItemRang(SelectInfo.StartItemNo, ref vParaFirstItemNo, ref vParaLastItemNo);
                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                        FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                        if (vEndItem.IsSelectComplate)
                            Result = DeleteItemSelectComplate(ref vDelCount, vParaFirstItemNo, vParaLastItemNo);
                        else  // Item部分选中
                        {
                            if (vEndItem.StyleNo < HCStyle.Null)
                                (vEndItem as HCCustomRectItem).DeleteSelected();
                            else  // 同一个TextItem
                            {
                                string vText = vEndItem.Text;
                                Undo_DeleteText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1,
                                    vText.Substring(SelectInfo.StartItemOffset + 1 - 1, SelectInfo.EndItemOffset - SelectInfo.StartItemOffset));

                                vEndItem.Text = vText.Remove(SelectInfo.StartItemOffset + 1 - 1, SelectInfo.EndItemOffset - SelectInfo.StartItemOffset);
                            }
                        }
            
                        ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
                    }
                    else  // 选中发生在不同Item，起始(可能是段首)全选中结尾没全选，起始没全选结尾全选，起始结尾都没全选
                    {
                        vFormatFirstItemNo = GetParaFirstItemNo(SelectInfo.StartItemNo);  // 取段第一个为起始
                        vFormatLastItemNo = GetParaLastItemNo(SelectInfo.EndItemNo);  // 取段最后一个为结束，如果变更注意下面
                        
                        FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                        
                        bool vSelStartParaFirst = Items[SelectInfo.StartItemNo].ParaFirst;
                        bool vSelStartComplate = Items[SelectInfo.StartItemNo].IsSelectComplate;  // 起始是否全选
                        bool vSelEndComplate = Items[SelectInfo.EndItemNo].IsSelectComplate;  // 结尾是否全选

                        Undo_StartRecord();

                        // 先处理选中结束Item
                        if (vEndItem.StyleNo < HCStyle.Null)
                        {
                            if (vSelEndComplate)
                            {
                                if (CanDeleteItem(SelectInfo.EndItemNo))
                                {
                                    Undo_DeleteItem(SelectInfo.EndItemNo, HC.OffsetAfter);
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
                                    Undo_DeleteItem(SelectInfo.EndItemNo, vEndItem.Length);
                                    Items.RemoveAt(SelectInfo.EndItemNo);
                                    vDelCount++;
                                }
                            }
                            else  // 文本且不在选中结束Item最后
                            {
                                Undo_DeleteText(SelectInfo.EndItemNo, 1, vEndItem.Text.Substring(1 - 1, SelectInfo.EndItemOffset));
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
                                Undo_DeleteItem(i, 0);
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
                                    Undo_DeleteItem(SelectInfo.StartItemNo, 0);
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
                                    Undo_DeleteItem(SelectInfo.StartItemNo, 0);
                                    Items.RemoveAt(SelectInfo.StartItemNo);
                                    vDelCount++;
                                }
                            }
                            else
                            //if SelectInfo.StartItemOffset < vStartItem.Length then  // 在中间(不用判断了吧？)
                            {
                                Undo_DeleteText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1,
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
                                    Undo_InsertItem(SelectInfo.StartItemNo, vNewItem.Length);

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
                                    Undo_InsertText(SelectInfo.StartItemNo - 1,
                                    Items[SelectInfo.StartItemNo - 1].Length - Items[SelectInfo.EndItemNo - vDelCount + 1].Length + 1,
                                    Items[SelectInfo.EndItemNo - vDelCount + 1].Text);
                                    SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                                    SelectInfo.StartItemOffset = vLen;
                                    Undo_DeleteItem(SelectInfo.EndItemNo - vDelCount + 1, 0);
                                    Items.RemoveAt(SelectInfo.EndItemNo - vDelCount + 1);
                                    vDelCount++;
                                }
                                else  // 起始前面和结束后面不能合并，如果选中起始和结束不在同一段
                                {
                                    if (Items[SelectInfo.EndItemNo - vDelCount + 1].ParaFirst)
                                    {
                                        Undo_ItemParaFirst(SelectInfo.EndItemNo - vDelCount + 1, 0, false);
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
                                    Undo_ItemParaFirst(SelectInfo.EndItemNo - vDelCount, 0, vSelStartParaFirst);
                                    Items[SelectInfo.EndItemNo - vDelCount].ParaFirst = vSelStartParaFirst;
                                }
                            }
                            else
                            if (!vSelEndComplate)
                            {
                                if (MergeItemText(Items[SelectInfo.StartItemNo], Items[SelectInfo.EndItemNo - vDelCount]))
                                {
                                    Undo_InsertText(SelectInfo.StartItemNo,
                                    Items[SelectInfo.StartItemNo].Length - Items[SelectInfo.EndItemNo - vDelCount].Length + 1,
                                    Items[SelectInfo.EndItemNo - vDelCount].Text);
                                    Undo_DeleteItem(SelectInfo.EndItemNo - vDelCount, 0);
                                    Items.RemoveAt(SelectInfo.EndItemNo - vDelCount);
                                    vDelCount++;
                                }
                                else  // 选中起始、结束位置的Item不能合并
                                {
                                    if (SelectInfo.EndItemNo != vFormatLastItemNo)
                                    {
                                        if (Items[SelectInfo.EndItemNo - vDelCount].ParaFirst)
                                        {
                                            Undo_ItemParaFirst(SelectInfo.EndItemNo - vDelCount, 0, false);
                                            Items[SelectInfo.EndItemNo - vDelCount].ParaFirst = false;  // 合并不成功就挨着
                                        }
                                    }
                                }
                            }
                        }
                        
                        ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
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
        /// <param name="AItem"></param>
        /// <returns></returns>
        public virtual bool InsertItem(HCCustomItem AItem)
        {
            bool Result = false;
            if (!CanEdit())
                return Result;

            DeleteSelected();

            AItem.ParaNo = Style.CurParaNo;

            if (IsEmptyData())
            {
                Undo_StartRecord();
                Result = EmptyDataInsertItem(AItem);
                return Result;
            }
            int vCurItemNo = GetCurItemNo();

            int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
            if (Items[vCurItemNo].StyleNo < HCStyle.Null)
            {
                if (SelectInfo.StartItemOffset == HC.OffsetInner)
                {
                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                    Undo_StartRecord();
                    Undo_ItemSelf(vCurItemNo, HC.OffsetInner);
                    Result = (Items[vCurItemNo] as HCCustomRectItem).InsertItem(AItem);
                    if (Result)
                        ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo, 0);
                }
                else  // 其前or其后
                {
                    if (SelectInfo.StartItemOffset == HC.OffsetBefor)
                        Result = InsertItem(SelectInfo.StartItemNo, AItem);
                    else  // 其后
                        Result = InsertItem(SelectInfo.StartItemNo + 1, AItem, false);
                    }
                }
                else  // 当前位置是TextItem
                {
                    // 先判断是否在后面，这样对于空行插入时从后面插入，否则会造成空行向后积压
                    if ((SelectInfo.StartItemOffset == Items[vCurItemNo].Length))
                        Result = InsertItem(SelectInfo.StartItemNo + 1, AItem, false);
                    else
                    if (SelectInfo.StartItemOffset == 0)
                        Result = InsertItem(SelectInfo.StartItemNo, AItem);
                else  // 在Item中间
                {
                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                    string vText = Items[vCurItemNo].Text;
                    string vsBefor = vText.Substring(1 - 1, SelectInfo.StartItemOffset);  // 前半部分文本
                    string vsAfter = vText.Substring(SelectInfo.StartItemOffset + 1 - 1, Items[vCurItemNo].Length - SelectInfo.StartItemOffset);  // 后半部分文本
                        
                    Undo_StartRecord();
                    if (Items[vCurItemNo].CanConcatItems(AItem))
                    {
                        if (AItem.ParaFirst)
                        {
                            Undo_DeleteText(vCurItemNo, SelectInfo.StartItemOffset + 1, vsAfter);
                            Items[vCurItemNo].Text = vsBefor;
                            AItem.Text = AItem.Text + vsAfter;
                            vCurItemNo = vCurItemNo + 1;
                            Items.Insert(vCurItemNo, AItem);
                            Undo_InsertItem(vCurItemNo, 0);
                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo, 1);
                            ReSetSelectAndCaret(vCurItemNo);
                        }
                        else  // 同一段中插入
                        {
                            Undo_InsertText(vCurItemNo, SelectInfo.StartItemOffset + 1, AItem.Text);
                            vsBefor = vsBefor + AItem.Text;
                            Items[vCurItemNo].Text = vsBefor + vsAfter;
                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo, 0);
                            SelectInfo.StartItemNo = vCurItemNo;
                            SelectInfo.StartItemOffset = vsBefor.Length;
                        //CaretDrawItemNo := GetItemLastDrawItemNo(vCurItemNo);
                        }
                    }
                    else  // 不能合并
                    {
                        Undo_DeleteText(vCurItemNo, SelectInfo.StartItemOffset + 1, vsAfter);
                        HCCustomItem vAfterItem = Items[vCurItemNo].BreakByOffset(SelectInfo.StartItemOffset);  // 后半部分对应的Item
                            
                        // 插入后半部分对应的Item
                        vCurItemNo = vCurItemNo + 1;
                        Items.Insert(vCurItemNo, vAfterItem);
                        Undo_InsertItem(vCurItemNo, 0);
                            
                        // 插入新Item
                        Items.Insert(vCurItemNo, AItem);
                        Undo_InsertItem(vCurItemNo, 0);

                        ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo + 2, 2);
                        ReSetSelectAndCaret(vCurItemNo);
                    }
            
                    Result = true;
                }
            }

            return Result;
        }

        /// <summary> 在指定的位置插入Item </summary>
        /// <param name="AIndex">插入位置</param>
        /// <param name="AItem">插入的Item</param>
        /// <param name="AOffsetBefor">插入时在原位置Item前面(True)或后面(False)</param>
        /// <returns></returns>
        public virtual bool InsertItem(int AIndex, HCCustomItem AItem, bool AOffsetBefor = true)
        {
            if (!CanEdit())
                return false;

            AItem.ParaNo = Style.CurParaNo;

            if (IsEmptyData())
            {
                Undo_StartRecord();
                return EmptyDataInsertItem(AItem);
            }

            int vIncCount = 0, vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
            Undo_StartRecord();
            if (AItem.StyleNo < HCStyle.Null)
            {
                int vInsPos = AIndex;
                if (AIndex < Items.Count)
                {
                    if (AOffsetBefor)
                    {
                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, AIndex, 0);
                        FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
 
                        if ((Items[AIndex].StyleNo > HCStyle.Null) && (Items[AIndex].Text == ""))
                        {
                            AItem.ParaFirst = true;
                            Undo_DeleteItem(AIndex, 0);
                            Items.RemoveAt(AIndex);
                            vIncCount--;
                            }
                            else  // 插入位置不是空行
                            if (!AItem.ParaFirst)
                            {
                                AItem.ParaFirst = Items[AIndex].ParaFirst;
                                if (Items[AIndex].ParaFirst)
                                {
                                    Undo_ItemParaFirst(AIndex, 0, false);
                                    Items[AIndex].ParaFirst = false;
                                }
                            }
                        }
                        else  // 在某Item后面插入
                        {
                            if ((AIndex > 0)
                                && (Items[AIndex - 1].StyleNo > HCStyle.Null)
                                && (Items[AIndex - 1].Text == ""))
                            {
                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, AIndex - 1, 0);
                                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                
                                AItem.ParaFirst = true;
                                Undo_DeleteItem(AIndex - 1, 0);
                                Items.RemoveAt(AIndex - 1);
                                vIncCount--;
                                vInsPos--;
                            }
                            else  // 插入位置前一个不是空行
                            {
                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, AIndex, 0);
                                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                            }
                        }
                    }
                    else  // 在末尾添加一个新Item
                    {
                        vFormatFirstItemNo = AIndex - 1;
                        vFormatLastItemNo = AIndex - 1;
                        FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                        if ((!AItem.ParaFirst)  // 插入不是另起一段)
                            && (Items[AIndex - 1].StyleNo > HCStyle.Null)  // 前面是TextItem
                            && (Items[AIndex - 1].Text == "")) // 空行
                        {
                            AItem.ParaFirst = true;
                            Undo_DeleteItem(AIndex - 1, 0);
                            Items.RemoveAt(AIndex - 1);
                            vIncCount--;
                            vInsPos--;
                        }
                    }

                    Items.Insert(vInsPos, AItem);
                    Undo_InsertItem(vInsPos, HC.OffsetAfter);
                    vIncCount++;

                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo + vIncCount, vIncCount);
                    ReSetSelectAndCaret(vInsPos);
                }
                else  // 插入文本Item
                {
                    bool vMerged = false;
                    if (AIndex > 0)
                    {
                        if (AIndex < Items.Count)
                        {
                            if (!Items[AIndex].ParaFirst)
                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, AIndex - 1, 0);
                            else
                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, AIndex, 0);
                        }
                        else  // 在最后面追加
                            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, AIndex - 1, 0);
                    }
                    else
                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, 0, 0);

                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                    
                    if (!AItem.ParaFirst)
                    {
                        // 在2个Item中间插入一个Item，需要同时判断和前后能否合并
                        if (AOffsetBefor)
                        {
                            if ((AIndex < Items.Count) && (Items[AIndex].CanConcatItems(AItem)))
                            {
                                Undo_InsertText(AIndex, 1, AItem.Text);  // 201806261644
                                Items[AIndex].Text = AItem.Text + Items[AIndex].Text;

                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo, 0);
                                ReSetSelectAndCaret(AIndex);

                                vMerged = true;
                            }
                            else
                            if ((!Items[AIndex].ParaFirst) && (AIndex > 0) && Items[AIndex - 1].CanConcatItems(AItem))
                            {
                                Undo_InsertText(AIndex - 1, Items[AIndex - 1].Length + 1, AItem.Text);  // 201806261650
                                Items[AIndex - 1].Text = Items[AIndex - 1].Text + AItem.Text;

                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo, 0);
                                ReSetSelectAndCaret(AIndex - 1);
                                vMerged = true;
                            }
                        }
                        else  // 在Item后面插入
                        {
                            if ((AIndex > 0) && Items[AIndex - 1].CanConcatItems(AItem))
                            {
                                Undo_InsertText(AIndex - 1, Items[AIndex - 1].Length + 1, AItem.Text);  // 201806261650
                                Items[AIndex - 1].Text = Items[AIndex - 1].Text + AItem.Text;

                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo, 0);
                                ReSetSelectAndCaret(AIndex - 1);

                                vMerged = true;
                            }
                            else
                        if ((AIndex < Items.Count) && (!Items[AIndex].ParaFirst) && (Items[AIndex].CanConcatItems(AItem)))
                        {
                            Undo_InsertText(AIndex, 1, AItem.Text);  // 201806261644
                            Items[AIndex].Text = AItem.Text + Items[AIndex].Text;

                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo, 0);
                            ReSetSelectAndCaret(AIndex, AItem.Length);
                            
                            vMerged = true;
                        }
                    }
                }

                if (!vMerged)
                {
                    if (AOffsetBefor && (!AItem.ParaFirst))
                    {
                        AItem.ParaFirst = Items[AIndex].ParaFirst;
                        if (Items[AIndex].ParaFirst)
                        {
                            Undo_ItemParaFirst(AIndex, 0, false);
                            Items[AIndex].ParaFirst = false;
                        }
                    }

                    Items.Insert(AIndex, AItem);
                    Undo_InsertItem(AIndex, 0);
                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);
                    
                    ReSetSelectAndCaret(AIndex);
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
        private void DoItemMouseDown(int AItemNo, int AOffset, MouseEventArgs e)
        {
            if (AItemNo < 0)
                return;

            int vX = -1, vY = -1;
            CoordToItemOffset(e.X, e.Y, AItemNo, AOffset, ref vX, ref vY);

            MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
            Items[AItemNo].MouseDown(vMouseArgs);

            if (FOnItemMouseDown != null)
                FOnItemMouseDown(this, AItemNo, vMouseArgs);
        }
#endregion

        public virtual void MouseDown(MouseEventArgs e)
        {
            FSelecting = false;  // 准备划选
            FDraging = false;  // 准备拖拽
            FMouseLBDouble = false;
            FMouseDownReCaret = false;
            FSelectSeekOffset = -1;
            FMouseLBDowning = (e.Button == MouseButtons.Left);
            FMouseDownX = e.X;
            FMouseDownY = e.Y;

            int vMouseDownItemNo = -1, vMouseDownItemOffset = -1, vDrawItemNo = -1;
            bool vRestrain = false;
            GetItemAt(e.X, e.Y, ref vMouseDownItemNo, ref vMouseDownItemOffset, ref vDrawItemNo, ref vRestrain);
            
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
                  || (CaretDrawItemNo != vDrawItemNo))
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
        private void AdjustSelectRange(int vMoveDrawItemNo)
        {
            bool vLeftToRight = false;
            // 记录原来选中范围
            int vOldStartItemNo = SelectInfo.StartItemNo;
            int vOldEndItemNo = SelectInfo.EndItemNo;

            if (FMouseDownItemNo < FMouseMoveItemNo)
            {
                vLeftToRight = true;

                if (FMouseDownItemOffset == GetItemAfterOffset(FMouseDownItemNo))
                {
                    if (FMouseDownItemNo < Items.Count - 1)
                    {
                        FMouseDownItemNo = FMouseDownItemNo + 1;
                        FMouseDownItemOffset = 0;
                    }
                }

                if ((FMouseDownItemNo != FMouseMoveItemNo) && (FMouseMoveItemNo >= 0) && (FMouseMoveItemOffset == 0))
                {
                    Items[FMouseMoveItemNo].DisSelect();  // 从前往后选，鼠标移动到前一次前面，原鼠标处被移出选中范围
                    
                    FMouseMoveItemNo = FMouseMoveItemNo - 1;
                    FMouseMoveItemOffset = GetItemAfterOffset(FMouseMoveItemNo);
                }
            }
            else
            if (FMouseMoveItemNo < FMouseDownItemNo)
            {
                vLeftToRight = false;

                if ((FMouseDownItemNo > 0) && (FMouseDownItemOffset == 0))
                {
                    FMouseDownItemNo = FMouseDownItemNo - 1;
                    FMouseDownItemOffset = GetItemAfterOffset(FMouseDownItemNo);
                }

                if ((FMouseDownItemNo != FMouseMoveItemNo) && (FMouseMoveItemOffset == GetItemAfterOffset(FMouseMoveItemNo)))
                {
                    Items[FMouseMoveItemNo].DisSelect();  // 从后往前选，鼠标移动到前一个后面，原鼠标处被移出选中范围
                    
                    if (FMouseMoveItemNo < Items.Count - 1)
                    {
                        FMouseMoveItemNo = FMouseMoveItemNo + 1;
                        FMouseMoveItemOffset = 0;
                    }
                }
            }

            if (FMouseDownItemNo == FMouseMoveItemNo)
            {
                if (FMouseMoveItemOffset > FMouseDownItemOffset)
                {
                    if (Items[FMouseDownItemNo].StyleNo < HCStyle.Null)
                    {
                        SelectInfo.StartItemNo = FMouseDownItemNo;
                        SelectInfo.StartItemOffset = FMouseDownItemOffset;
                        if ((FMouseDownItemOffset == HC.OffsetBefor) && (FMouseMoveItemOffset == HC.OffsetAfter))
                        {
                            SelectInfo.EndItemNo = FMouseMoveItemNo;
                            SelectInfo.EndItemOffset = FMouseMoveItemOffset;
                        }
                        else  // 没有全选中
                        {
                            SelectInfo.EndItemNo = -1;
                            SelectInfo.EndItemOffset = -1;
                            CaretDrawItemNo = vMoveDrawItemNo;
                        }
                    }
                    else  // TextItem
                    {
                        SelectInfo.StartItemNo = FMouseDownItemNo;
                        SelectInfo.StartItemOffset = FMouseDownItemOffset;
                        SelectInfo.EndItemNo = FMouseDownItemNo;
                        SelectInfo.EndItemOffset = FMouseMoveItemOffset;
                    }
                }
                else
                if (FMouseMoveItemOffset < FMouseDownItemOffset)
                {
                    if (Items[FMouseDownItemNo].StyleNo < HCStyle.Null)
                    {
                        if (FMouseMoveItemOffset == HC.OffsetBefor)
                        {
                            SelectInfo.StartItemNo = FMouseDownItemNo;
                            SelectInfo.StartItemOffset = FMouseMoveItemOffset;
                            SelectInfo.EndItemNo = FMouseDownItemNo;
                            SelectInfo.EndItemOffset = FMouseDownItemOffset;
                        }
                        else  // 从后往前选到OffsetInner了
                        {
                            SelectInfo.StartItemNo = FMouseDownItemNo;
                            SelectInfo.StartItemOffset = FMouseDownItemOffset;
                            SelectInfo.EndItemNo = -1;
                            SelectInfo.EndItemOffset = -1;

                            CaretDrawItemNo = vMoveDrawItemNo;
                        }
                    }
                    else  // TextItem
                    {
                        SelectInfo.StartItemNo = FMouseMoveItemNo;
                        SelectInfo.StartItemOffset = FMouseMoveItemOffset;
                        SelectInfo.EndItemNo = FMouseMoveItemNo;
                        SelectInfo.EndItemOffset = FMouseDownItemOffset;
                    }
                }
                else  // 结束位置和起始位置相同(同一个Item)
                {
                    if (SelectInfo.EndItemNo >= 0)
                        Items[SelectInfo.EndItemNo].DisSelect();

                    SelectInfo.StartItemNo = FMouseDownItemNo;
                    SelectInfo.StartItemOffset = FMouseDownItemOffset;
                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
            
                    CaretDrawItemNo = vMoveDrawItemNo;
                }
            }
            else  // 选择操作不在同一个Item
            {
                if (vLeftToRight)
                {
                    SelectInfo.StartItemNo = FMouseDownItemNo;
                    SelectInfo.StartItemOffset = FMouseDownItemOffset;
                    SelectInfo.EndItemNo = FMouseMoveItemNo;
                    SelectInfo.EndItemOffset = FMouseMoveItemOffset;
                }
                else
                {
                    SelectInfo.StartItemNo = FMouseMoveItemNo;
                    SelectInfo.StartItemOffset = FMouseMoveItemOffset;
                    SelectInfo.EndItemNo = FMouseDownItemNo;
                    SelectInfo.EndItemOffset = FMouseDownItemOffset;
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
                for (int i = vOldEndItemNo; i>= SelectInfo.StartItemNo + 1; i--)  // 当前后面的取消选中
                    Items[i].DisSelect();
            }
            else  // 有选中结束
            {
                for (int i = vOldEndItemNo; i>= SelectInfo.EndItemNo + 1; i--)  // 原结束倒序到现结束下一个的取消选中
                    Items[i].DisSelect();
            }
        }
#endregion

#region
        private void DoItemMouseMove(int AItemNo, int AOffset, MouseEventArgs e)
        {
            if (AItemNo < 0)
                return;

            int vX = -1, vY = -1;
            CoordToItemOffset(e.X, e.Y, AItemNo, AOffset, ref vX, ref vY);

            MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
            Items[AItemNo].MouseMove(vMouseArgs);
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
            if (FSelecting)
            {
                FMouseMoveItemNo = vMouseMoveItemNo;
                FMouseMoveItemOffset = vMouseMoveItemOffset;
                FMouseMoveRestrain = vRestrain;
                FSelectSeekNo = vMouseMoveItemNo;
                FSelectSeekOffset = vMouseMoveItemOffset;

                AdjustSelectRange(vMoveDrawItemNo);  // 确定SelectRang
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
                    DoItemMouseMove(FMouseMoveItemNo, FMouseMoveItemOffset, e);
            }
        }

#region DoItemMouseUp
        private void DoItemMouseUp(int AItemNo, int AOffset, MouseEventArgs e)
        {
            if (AItemNo < 0)
                return;

            int vX = -1, vY = -1;
            CoordToItemOffset(e.X, e.Y, AItemNo, AOffset, ref vX, ref vY);

            MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
            Items[AItemNo].MouseUp(vMouseArgs);

        if (FOnItemMouseUp != null)
            FOnItemMouseUp(this, AItemNo, vMouseArgs);
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

            if (Items[vUpItemNo].StyleNo < HCStyle.Null)
                DoItemMouseUp(vUpItemNo, vUpItemOffset, e);  // 弹起，因为可能是移出Item后弹起，所以这里不受vRestrain约束
        }
#endregion

        public virtual void MouseUp(MouseEventArgs e)
        {
            if (!FMouseLBDowning)
                return;

            FMouseLBDowning = false;
            if (FMouseLBDouble)
                return;

            if (SelectedResizing())
            {
                Undo_StartRecord();
                Undo_ItemSelf(FMouseDownItemNo, FMouseDownItemOffset);

                DoItemMouseUp(FMouseDownItemNo, FMouseDownItemOffset, e);
                DoItemResized(FMouseDownItemNo);  // 缩放完成事件(可控制缩放不要超过页面)

                int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, FMouseDownItemNo, FMouseDownItemOffset);
                
                if ((vFormatFirstItemNo > 0) && (!Items[vFormatFirstItemNo].ParaFirst))
                {
                    vFormatFirstItemNo--;
                    vFormatFirstItemNo = GetLineFirstItemNo(vFormatFirstItemNo, 0);
                }

                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
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
                for (int i = SelectInfo.StartItemNo; i<= SelectInfo.EndItemNo; i++)
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
        public virtual void KeyPress(ref Char Key)
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
                Undo_StartRecord();
                Undo_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                HCCustomRectItem vRectItem = vCarteItem as HCCustomRectItem;
                vRectItem.KeyPress(ref Key);
                if (vRectItem.SizeChanged)
                {
                    vRectItem.SizeChanged = false;

                    int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                    if (Key != 0)
                        ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);

                    Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();
                    Style.UpdateInfoReScroll();
                    }
                }
            else
                InsertText(Key.ToString());
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
                        FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                        (vCurItem as HCCustomRectItem).KeyDown(e);
                        ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                    }

                    return;
                }
            }

            this.InsertItem(vTabItem);
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
                                return;;
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

        private  void SelectStartItemNext()
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
                        ADrawItemOffset = DrawItems[i].CharOffs + GetDrawItemOffset(i, vX) - 1;
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
                /* 获取当前光标X位置 }*/
                int vX = DrawItems[ADrawItemNo].Rect.Left + GetDrawItemOffsetWidth(ADrawItemNo, ADrawItemOffset);
           
                /* 获取下一行在X位置对应的DItem和Offset }*/
                vFirstDItemNo = vLastDItemNo + 1;
                GetLineDrawItemRang(ref vFirstDItemNo, ref vLastDItemNo);  // 下一行起始和结束DItem
            
                for (int i = vFirstDItemNo; i <= vLastDItemNo; i++)
                {
                    if (DrawItems[i].Rect.Right > vX)
                    {
                        ADrawItemNo = i;
                        ADrawItemOffset = DrawItems[i].CharOffs + GetDrawItemOffset(i, vX) - 1;
                        return Result;  // 有合适，则退出
                    }
                }
                    
                /* 没合适则选择到最后 }*/
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
                    vRectItem.KeyDown(e);
                    if (vRectItem.SizeChanged)
                    {
                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                        FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                        vRectItem.SizeChanged = false;
                        ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
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
            {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            {
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
                        FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                        if (vCurItem.ParaFirst)
                        {
                            vCurItem = CreateDefaultTextItem();
                            vCurItem.ParaFirst = true;
                            Items.Insert(SelectInfo.StartItemNo, vCurItem);

                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);
                            
                            SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                            ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                        }
                        else  // RectItem不在行首
                        {
                            vCurItem.ParaFirst = true;
                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                        }
                        
                        break;

                    case User.VK_BACK:  // 在RectItem前
                        if (vCurItem.ParaFirst)
                        {
                            if (SelectInfo.StartItemNo > 0)
                            {
                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                Undo_StartRecord();
                                Undo_ItemParaFirst(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, false);
                                vCurItem.ParaFirst = false;
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
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
                        FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                        
                        if (vCurItem.ParaFirst)
                        {
                            if (SelectInfo.StartItemNo != vFormatLastItemNo)
                            {
                                Undo_StartRecord();

                                Undo_ItemParaFirst(SelectInfo.StartItemNo + 1, 0, true);
                                Items[SelectInfo.StartItemNo + 1].ParaFirst = true;

                                Undo_DeleteItem(SelectInfo.StartItemNo, 0);
                                Items.RemoveAt(SelectInfo.StartItemNo);

                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                            }
                            else  // 段删除空了
                            {
                                Undo_StartRecord();
                                Undo_DeleteItem(SelectInfo.StartItemNo, 0);
                                Items.RemoveAt(SelectInfo.StartItemNo);

                                vCurItem = CreateDefaultTextItem();
                                vCurItem.ParaFirst = true;
                                Items.Insert(SelectInfo.StartItemNo, vCurItem);
                                Undo_InsertItem(SelectInfo.StartItemNo, 0);
                                
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                            }
                        }
                        else  // 不是段首
                        {
                            if (SelectInfo.StartItemNo < vFormatLastItemNo)
                            {
                                int vLen = GetItemAfterOffset(SelectInfo.StartItemNo - 1);
                                Undo_StartRecord();
                                Undo_DeleteItem(SelectInfo.StartItemNo, 0);
                                // 如果RectItem前面(同一行)有高度小于此RectItme的Item(如Tab)，
                                // 其格式化时以RectItem为高，重新格式化时如果从RectItem所在位置起始格式化，
                                // 行高度仍会以Tab为行高，也就是RectItem高度，所以需要从行开始格式化
                                Items.RemoveAt(SelectInfo.StartItemNo);
                                if (MergeItemText(Items[SelectInfo.StartItemNo - 1], Items[SelectInfo.StartItemNo]))
                                {
                                    Undo_InsertText(SelectInfo.StartItemNo - 1,
                                        Items[SelectInfo.StartItemNo - 1].Length + 1, Items[SelectInfo.StartItemNo].Text);
                                    
                                    Items.RemoveAt(SelectInfo.StartItemNo);
                                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 2, -2);
                                }
                                else
                                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);

                                SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                                SelectInfo.StartItemOffset = vLen;
                            }
                            else  // 段尾(段不只一个Item)
                            {
                                Undo_StartRecord();
                                Undo_DeleteItem(SelectInfo.StartItemNo, 0);
                                Items.RemoveAt(SelectInfo.StartItemNo);

                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                                
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
            }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      }
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
                        FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                        if (vCurItem.ParaFirst)
                        {
                            if ((SelectInfo.StartItemNo >= 0)
                                        && (SelectInfo.StartItemNo < Items.Count - 1)
                                        && (!Items[SelectInfo.StartItemNo + 1].ParaFirst))
                            {
                                Undo_StartRecord();
                                Undo_DeleteItem(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                                Items.RemoveAt(SelectInfo.StartItemNo);

                                Undo_ItemParaFirst(SelectInfo.StartItemNo, 0, true);
                                Items[SelectInfo.StartItemNo].ParaFirst = true;
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);

                                ReSetSelectAndCaret(SelectInfo.StartItemNo, 0);
                            }
                            else  // 空段了
                            {
                                Undo_StartRecord();
                                Undo_DeleteItem(SelectInfo.StartItemNo, 0);
                                Items.RemoveAt(SelectInfo.StartItemNo);

                                HCCustomItem vItem = CreateDefaultTextItem();
                                vItem.ParaFirst = true;
                                Items.Insert(SelectInfo.StartItemNo, vItem);
                                Undo_InsertItem(SelectInfo.StartItemNo, 0);
                                
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
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
                                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);

                                Undo_StartRecord();
                                Undo_ItemParaFirst(SelectInfo.StartItemNo, 0, false);
                            
                                Items[SelectInfo.StartItemNo].ParaFirst = false;
                            
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
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
                        FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
            
                        if ((SelectInfo.StartItemNo < Items.Count - 1)  // 不是最后一个)
                              && (!Items[SelectInfo.StartItemNo + 1].ParaFirst))  // 下一个不是段首
                        {
                            Items[SelectInfo.StartItemNo + 1].ParaFirst = true;
                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                            SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                            SelectInfo.StartItemOffset = 0;
                            CaretDrawItemNo = Items[SelectInfo.StartItemNo].FirstDItemNo;
                        }
                        else
                        {
                            vCurItem = CreateDefaultTextItem();
                            vCurItem.ParaFirst = true;
                            Items.Insert(SelectInfo.StartItemNo + 1, vCurItem);
                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);
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
            FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
            // 判断光标位置内容如何换行
            if (SelectInfo.StartItemOffset == 0)
            {
                if (!vCurItem.ParaFirst)
                {
                    vCurItem.ParaFirst = true;
                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                }
                else  // 原来就是段首
                {
                    HCCustomItem vItem = CreateDefaultTextItem();
                    vItem.ParaNo = vCurItem.ParaNo;
                    vItem.StyleNo = vCurItem.StyleNo;
                    vItem.ParaFirst = true;
                    Items.Insert(SelectInfo.StartItemNo, vItem);  // 原位置的向下移动
                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);
                    SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                }
            }
            else
            if (SelectInfo.StartItemOffset == vCurItem.Length)
            {
                HCCustomItem vItem = null;
                if (SelectInfo.StartItemNo < Items.Count - 1)
                {
                    vItem = Items[SelectInfo.StartItemNo + 1];  // 下一个Item
                    if (!vItem.ParaFirst)
                    {
                        vItem.ParaFirst = true;
                        ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                    }
                    else  // 下一个是段起始
                    {
                        vItem = CreateDefaultTextItem();
                        vItem.ParaNo = vCurItem.ParaNo;
                        vItem.StyleNo = vCurItem.StyleNo;
                        vItem.ParaFirst = true;
                        Items.Insert(SelectInfo.StartItemNo + 1, vItem);
                        ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);
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
                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);
                    SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                    SelectInfo.StartItemOffset = 0;
                }
            }
            else  // 光标在Item中间
            {
                HCCustomItem vItem = vCurItem.BreakByOffset(SelectInfo.StartItemOffset);  // 截断当前Item
                vItem.ParaFirst = true;

                Items.Insert(SelectInfo.StartItemNo + 1, vItem);
                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo + 1, 1);

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
                            FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                            Items.RemoveAt(vCurItemNo);
                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
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
                                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                
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
            
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
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
                else  // 可删除
                {
                    string vText = Items[vCurItemNo].Text;

                    vCurItem.Text = vText.Remove(SelectInfo.StartItemOffset + 1 - 1, 1);
                    if (vText == "")
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
                                    FormatItemPrepare(vCurItemNo - 1, vFormatLastItemNo);
                                    Items.RemoveAt(vCurItemNo);  // 删除当前
                                    Items.RemoveAt(vCurItemNo);  // 删除下一个
                                    ReFormatData_(vCurItemNo - 1, vFormatLastItemNo - 2, -2);
                                }
                                else  // 下一个合并不到上一个
                                {
                                    vLen = 0;
                                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                    Items.RemoveAt(vCurItemNo);
                                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
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
                                FormatItemPrepare(vCurItemNo);
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
                                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                SelectInfo.StartItemOffset = 0;
                                Items[vCurItemNo + 1].ParaFirst = Items[vCurItemNo].ParaFirst;
                                Items.RemoveAt(vCurItemNo);
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                            }
                            else  // 当前段删除空了
                            {
                                FormatItemPrepare(vCurItemNo);
                                SelectInfo.StartItemOffset = 0;
                                ReFormatData_(vCurItemNo);
                            }
                        }
                    }
                    else  // 删除后还有内容
                    {
                        FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                        ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                    }
                }
            }

            if (!e.Handled)
                CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
        }

        private void BackspaceKeyDown(ref HCCustomItem vCurItem, int vParaFirstItemNo, int vParaLastItemNo, KeyEventArgs e)
        {
            int vCurItemNo = -1, vLen = -1, vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
            int vParaNo = -1, vDelCount = 0;
            bool vParaFirst = false;
            if (SelectInfo.StartItemOffset == 0)
            {
                if ((vCurItem.Text == "") && (Style.ParaStyles[vCurItem.ParaNo].AlignHorz != ParaAlignHorz.pahJustify))
                    ApplyParaAlignHorz(ParaAlignHorz.pahJustify);  // 居中等对齐的空Item，删除时切换到分散对齐
                else
                if (SelectInfo.StartItemNo != 0)
                {
                    vCurItemNo = SelectInfo.StartItemNo;
                    if (vCurItem.ParaFirst)
                    {
                        vLen = Items[SelectInfo.StartItemNo - 1].Length;
                        if (vCurItem.CanConcatItems(Items[SelectInfo.StartItemNo - 1]))
                        {
                            Undo_StartRecord();
                            Undo_InsertText(SelectInfo.StartItemNo - 1, Items[SelectInfo.StartItemNo - 1].Length + 1,
                                Items[SelectInfo.StartItemNo].Text);

                            Items[SelectInfo.StartItemNo - 1].Text = Items[SelectInfo.StartItemNo - 1].Text
                                + Items[SelectInfo.StartItemNo].Text;

                            vFormatFirstItemNo = GetLineFirstItemNo(SelectInfo.StartItemNo - 1, vLen);
                            vFormatLastItemNo = GetParaLastItemNo(SelectInfo.StartItemNo);
                            FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                            
                            Undo_DeleteItem(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                            Items.RemoveAt(SelectInfo.StartItemNo);
                                            
                            // 修正下一段合并上来的Item的段样式，对齐样式
                            vParaNo = Items[SelectInfo.StartItemNo - 1].ParaNo;
                            if (vParaNo != vCurItem.ParaNo)
                            {
                                for (int i = SelectInfo.StartItemNo; i <= vFormatLastItemNo - 1; i++)
                                {
                                    //Undo_ItemParaNo(i, 0, vParaNo);
                                    Items[i].ParaNo = vParaNo;
                                }
                            }
            
                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
            
                            ReSetSelectAndCaret(SelectInfo.StartItemNo - 1, vLen);
                        }
                        else  // 段起始且不能和上一个合并
                        {
                            if (vCurItem.Length == 0)
                            {
                                FormatItemPrepare(SelectInfo.StartItemNo - 1, SelectInfo.StartItemNo);
                                
                                Undo_StartRecord();
                                Undo_DeleteItem(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                                Items.RemoveAt(SelectInfo.StartItemNo);
                                
                                ReFormatData_(SelectInfo.StartItemNo - 1, SelectInfo.StartItemNo - 1, -1);
                                
                                ReSetSelectAndCaret(SelectInfo.StartItemNo - 1);
                            }
                            else  // 段前删除且不能和上一段最后合并
                            {
                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                
                                Undo_StartRecord();
                                Undo_ItemParaFirst(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, false);
                                
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
            
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
            
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
                                Undo_StartRecord();
                                vParaFirst = Items[vCurItemNo].ParaFirst;  // 记录前面的RectItem段首属性
                                
                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                
                                // 删除前面的RectItem
                                Undo_DeleteItem(vCurItemNo, HC.OffsetAfter);
                                Items.RemoveAt(vCurItemNo);
                                
                                if (vParaFirst)
                                {
                                    Undo_ItemParaFirst(vCurItemNo, 0, vParaFirst);
                                    vCurItem.ParaFirst = vParaFirst;  // 赋值前面RectItem的段起始属性
                                    vLen = 0;
                                }
                                else  // 前面删除的RectItem不是段首
                                {
                                    vDelCount = 1;
                                    vCurItemNo = vCurItemNo - 1;  // 上一个
                                    vLen = Items[vCurItemNo].Length;  // 上一个最后面

                                    if (MergeItemText(Items[vCurItemNo], vCurItem))
                                    {
                                        Undo_InsertText(vCurItemNo, vLen + 1, vCurItem.Text);
                                        Undo_DeleteItem(vCurItemNo + 1, 0);
                                        Items.RemoveAt(vCurItemNo + 1); // 删除当前的
                                        vDelCount = 2;
                                    }
                                }
            
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
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
                            BackspaceKeyDown(ref vCurItem, vParaFirstItemNo, vParaLastItemNo, e);  // 重新处理
                            
                            return;
                        }
                    }
                }
            }
            else  // 光标不在Item最开始  文本TextItem
            {
                if (vCurItem.Length == 1)
                {
                    vCurItemNo = SelectInfo.StartItemNo;  // 记录原位置
                    if (!DrawItems[Items[vCurItemNo].FirstDItemNo].LineFirst)
                    {
                        vLen = Items[vCurItemNo - 1].Length;
                        if ((vCurItemNo > 0) && (vCurItemNo < vParaLastItemNo)  // 不是段最后一个)
                            && MergeItemText(Items[vCurItemNo - 1], Items[vCurItemNo + 1]))
                        {
                            Undo_StartRecord();
                            Undo_InsertText(vCurItemNo - 1, Items[vCurItemNo - 1].Length - Items[vCurItemNo + 1].Length + 1,
                                Items[vCurItemNo + 1].Text);

                            GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, vCurItemNo - 1, vLen);
                            FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                            
                            Undo_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                            Items.RemoveAt(vCurItemNo);  // 删除当前
                            
                            Undo_DeleteItem(vCurItemNo, 0);
                            Items.RemoveAt(vCurItemNo);  // 删除下一个
                            
                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 2, -2);
                            
                            ReSetSelectAndCaret(SelectInfo.StartItemNo - 1, vLen);  // 上一个原光标位置
                        }
                        else  // 当前不是行首，删除后没有内容了，且不能合并上一个和下一个
                        {
                            if (SelectInfo.StartItemNo == vParaLastItemNo)
                            {
                                vFormatFirstItemNo = GetLineFirstItemNo(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                                vFormatLastItemNo = vParaLastItemNo;
                                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                
                                Undo_StartRecord();
                                Undo_DeleteItem(vCurItemNo, SelectInfo.StartItemOffset);
                                Items.RemoveAt(vCurItemNo);
                                
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                                
                                ReSetSelectAndCaret(vCurItemNo - 1);
                            }
                            else  // 不是段最后一个
                            {
                                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                
                                Undo_StartRecord();
                                Undo_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                Items.RemoveAt(vCurItemNo);
                                
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                                
                                ReSetSelectAndCaret(vCurItemNo - 1);
                            }
                        }
                    }
                    else  // Item是行第一个、行首Item删除空了，
                    {
                        GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                        if (Items[vCurItemNo].ParaFirst)
                        {
                            FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                            if (vCurItemNo < vFormatLastItemNo)
                            {
                                Undo_StartRecord();
                                vParaFirst = true;  // Items[vCurItemNo].ParaFirst;  // 记录行首Item的段属性
                                
                                Undo_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                Items.RemoveAt(vCurItemNo);
                                
                                if (vParaFirst)
                                {
                                    Undo_ItemParaFirst(vCurItemNo, 0, vParaFirst);
                                    Items[vCurItemNo].ParaFirst = vParaFirst;  // 其后继承段首属性
                                }

                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                                ReSetSelectAndCaret(vCurItemNo, 0);  // 下一个最前面
                            }
                            else  // 同段后面没有内容了，保持空行
                            {
                                Undo_StartRecord();
                                Undo_DeleteText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, vCurItem.Text);  // Copy(vText, SelectInfo.StartItemOffset, 1));
                                
                                //System.Delete(vText, SelectInfo.StartItemOffset, 1);
                                vCurItem.Text = "";  // vText;
                                SelectInfo.StartItemOffset = SelectInfo.StartItemOffset - 1;
                                
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);  // 保留空行
                            }
                        }
                        else  // 不是段首Item，仅是行首Item删除空了
                        {
                            Undo_StartRecord();

                            if (vCurItemNo < vFormatLastItemNo)
                            {
                                vLen = Items[vCurItemNo - 1].Length;
                                if (MergeItemText(Items[vCurItemNo - 1], Items[vCurItemNo + 1]))
                                {
                                    Undo_InsertText(vCurItemNo - 1,
                                        Items[vCurItemNo - 1].Length - Items[vCurItemNo + 1].Length + 1, Items[vCurItemNo + 1].Text);
                                    
                                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo, vCurItemNo - 1, Items[vCurItemNo - 1].Length);  // 取前一个格式化起始位置
                                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                    
                                    Undo_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);  // 删除空的Item
                                    Items.RemoveAt(vCurItemNo);
                                    
                                    Undo_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);  // 被合并的Item
                                    Items.RemoveAt(vCurItemNo);
                                    
                                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 2, -2);
                                    ReSetSelectAndCaret(vCurItemNo - 1, vLen);
                                }
                                else  // 前后不能合并
                                {
                                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                    
                                    Undo_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                    Items.RemoveAt(vCurItemNo);
                                    
                                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
                                    ReSetSelectAndCaret(vCurItemNo - 1);
                                }
                            }
                            else  // 同段后面没有内容了
                            {
                                if (vFormatFirstItemNo == vCurItemNo)
                                    vFormatFirstItemNo--;

                                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                                
                                Undo_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                Items.RemoveAt(vCurItemNo);
                                
                                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo - 1, -1);
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
                    
                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                    
                    string vText = vCurItem.Text;  // 和上面 201806242257 处一样
                    
                    Undo_StartRecord();
                    Undo_DeleteText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, vText.Substring(SelectInfo.StartItemOffset - 1, 1));

                    vCurItem.Text = vText.Remove(SelectInfo.StartItemOffset - 1, 1); ;
                    
                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                    
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
            if (vSelectExist && ( (Key == User.VK_BACK)
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
                        BackspaceKeyDown(ref vCurItem, vParaFirstItemNo, vParaLastItemNo, e);  // 回删
                        break;

                    case User.VK_RETURN: 
                        EnterKeyDown(ref vCurItem, e);      // 回车
                        break;

                    case User.VK_LEFT:   
                        LeftKeyDown(vSelectExist, e);       // 左方向键
                        break;

                    case User.VK_RIGHT:  
                        RightKeyDown(vSelectExist, vCurItem, e);      // 右方向键
                        break;

                    case User.VK_DELETE: 
                        DeleteKeyDown(vCurItem, e);     // 删除键
                        break;

                    case User.VK_HOME:   
                        HomeKeyDown(vSelectExist, e);       // Home键
                        break;

                    case User.VK_END:    
                        EndKeyDown(vSelectExist, e);        // End键
                        break;

                    case User.VK_UP:     
                        UpKeyDown(vSelectExist, e);         // 上方向键
                        break;

                    case User.VK_DOWN:
                        DownKeyDown(vSelectExist, e);       // 下方向键
                        break;

                    case User.VK_TAB: 
                        TABKeyDown(vCurItem, e);        // TAB键
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

        public virtual void Undo(HCCustomUndo AUndo) { }

        public virtual void Redo(HCCustomUndo ARedo) { }

        /// <summary> 初始化相关字段和变量 </summary>
        public override void InitializeField()
        {
            InitializeMouseField();
            base.InitializeField();
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            if (!CanEdit())
                return;

            base.LoadFromStream(AStream, AStyle, AFileVersion);
            InsertStream(AStream, AStyle, AFileVersion);
            // 加载完成后，初始化(有一部分在LoadFromStream中初始化了)
            ReSetSelectAndCaret(0, 0);
        }

        public override bool InsertStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            if (!CanEdit())
                return false;

            bool Result = false;
            HCCustomItem vAfterItem = null;
            bool vInsertBefor = false;
            int vInsPos = 0, vFormatFirstItemNo = -1, vFormatLastItemNo = -1;

            Undo_StartGroup(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
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
                            FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                            Undo_StartRecord();
                            Undo_ItemSelf(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                            Result = (Items[vInsPos] as HCCustomRectItem).InsertStream(AStream, AStyle, AFileVersion);
                            ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);

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
                            else  // 其中
                            {
                                Undo_StartRecord();
                                Undo_DeleteText(vInsPos, 1, Items[vInsPos].Text.Substring(1 - 1, SelectInfo.StartItemOffset));
                                vAfterItem = Items[vInsPos].BreakByOffset(SelectInfo.StartItemOffset);  // 后半部分对应的Item
                                vInsPos = vInsPos + 1;
                            }
                    }
                }

                Int64 vDataSize = 0;
                byte[] vBuffer = BitConverter.GetBytes(vDataSize);
                AStream.Read(vBuffer, 0, vBuffer.Length);
                vDataSize = BitConverter.ToInt64(vBuffer, 0);
                if (vDataSize == 0)
                    return Result;

                int vItemCount = 0;
                vBuffer = BitConverter.GetBytes(vItemCount);
                AStream.Read(vBuffer, 0, vBuffer.Length);
                vItemCount = BitConverter.ToInt32(vBuffer, 0);
                
                if (vItemCount == 0)
                    return Result;

                // 因为插入的第一个可能和插入位置前一个合并，插入位置可能是行首，所以要从插入位置
                // 行上一个开始格式化，为简单处理，直接使用段首，可优化为上一行首
                //GetParaItemRang(SelectInfo.StartItemNo, vFormatFirstItemNo, vFormatLastItemNo);

                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);

                // 计算格式化起始、结束ItemNo
                if (Items.Count > 0)
                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                else
                {
                    vFormatFirstItemNo = 0;
                    vFormatLastItemNo = -1;
                }

                Undo_StartRecord();

                int vStyleNo = HCStyle.Null;
                HCCustomItem vItem = null;
                for (int i = 0; i <= vItemCount - 1; i++)
                {
                    AStream.Read(vBuffer, 0, vBuffer.Length);  // besure vBuffer.Length = 4
                    vStyleNo = BitConverter.ToInt32(vBuffer, 0);
                    vItem = CreateItemByStyle(vStyleNo);
                    if (vStyleNo < HCStyle.Null)
                        Undo_ItemSelf(i, 0);
                    vItem.LoadFromStream(AStream, AStyle, AFileVersion);
                    if (AStyle != null)
                    {
                        if (vItem.StyleNo > HCStyle.Null)
                            vItem.StyleNo = Style.GetStyleNo(AStyle.TextStyles[vItem.StyleNo], true);

                        vItem.ParaNo = Style.GetParaNo(AStyle.ParaStyles[vItem.ParaNo], true);
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
                                Undo_ItemParaFirst(vInsPos, 0, false);
                                Items[vInsPos].ParaFirst = false;
                            }
                        }
                        else
                            vItem.ParaFirst = false;
                    }

                    Items.Insert(vInsPos + i, vItem);
                    Undo_InsertItem(vInsPos + i, 0);
                }

                vItemCount = CheckInsertItemCount(vInsPos, vInsPos + vItemCount - 1);  // 检查插入的Item是否合格并删除不合格

                int vInsetLastNo = vInsPos + vItemCount - 1;  // 光标在最后一个Item
                int vCaretOffse = GetItemAfterOffset(vInsetLastNo);  // 最后一个Item后面

                if (vAfterItem != null)
                {
                    if (MergeItemText(Items[vInsetLastNo], vAfterItem))
                    {
                        Undo_InsertText(vInsetLastNo, Items[vInsetLastNo].Length - vAfterItem.Length + 1, vAfterItem.Text);
                        vAfterItem.Dispose();
                    }
                    else  // 插入最后一个和后半部分不能合并
                    {
                        Items.Insert(vInsetLastNo + 1, vAfterItem);
                        Undo_InsertItem(vInsetLastNo + 1, 0);
                        vItemCount++;
                    }
                }

                if (vInsPos > 0)
                {
                    if (Items[vInsPos - 1].Length == 0)
                    {
                        Undo_ItemParaFirst(vInsPos, 0, Items[vInsPos - 1].ParaFirst);
                        Items[vInsPos].ParaFirst = Items[vInsPos - 1].ParaFirst;

                        Undo_DeleteItem(vInsPos - 1, 0);
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
                            Undo_InsertText(vInsPos - 1, Items[vInsPos - 1].Length + 1, Items[vInsPos].Text);
                            Undo_DeleteItem(vInsPos, 0);

                            if (vItemCount == 1)
                                vCaretOffse = vOffsetStart + vCaretOffse;

                            Items.RemoveAt(vInsPos);
                            vItemCount--;
                            vInsetLastNo--;
                        }
                    }

                    if ((vInsetLastNo < Items.Count - 1)  // 插入最后Item和后面的能合并)
                        && (!Items[vInsetLastNo + 1].ParaFirst)
                        && MergeItemText(Items[vInsetLastNo], Items[vInsetLastNo + 1]))
                    {
                        Undo_DeleteItem(vInsetLastNo + 1, 0);

                        Items.RemoveAt(vInsetLastNo + 1);
                        vItemCount--;
                    }
                }
                else  // 在最开始第0个位置处插入
                //if (vInsetLastNo < Items.Count - 1) then
                {
                    if (MergeItemText(Items[vInsetLastNo], Items[vInsetLastNo + 1]))
                    {
                        Undo_DeleteItem(vInsPos + vItemCount, 0);
                        Items.RemoveAt(vInsPos + vItemCount);
                        vItemCount--;
                    }
                }

                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo + vItemCount, vItemCount);

                ReSetSelectAndCaret(vInsetLastNo, vCaretOffse);  // 选中插入内容最后Item位置
            }
            finally
            {
                Undo_EndGroup(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            }

            InitializeMouseField();  // 201807311101
            
            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();
            Style.UpdateInfoReScroll();

            return Result;
        }

        //
        public void DblClick(int X, int  Y)
        {
            FMouseLBDouble = true;
            int vItemNo = -1, vItemOffset = -1, vDrawItemNo = -1;
            bool vRestrain = false;

            GetItemAt(X, Y, ref vItemNo, ref vItemOffset, ref vDrawItemNo, ref vRestrain);
            if (vItemNo < 0)
                return;

            if (Items[vItemNo].StyleNo < HCStyle.Null)
            {
                int vX = -1, vY = -1;
                CoordToItemOffset(X, Y, vItemNo, vItemOffset, ref vX, ref vY);
                Items[vItemNo].DblClick(vX, vY);
            }
            else  // TextItem双击时根据光标处内容，选中范围
            if (Items[vItemNo].Length > 0)
            {
                string vText = GetDrawItemText(vDrawItemNo);  // DrawItem对应的文本
                vItemOffset = vItemOffset - DrawItems[vDrawItemNo].CharOffs + 1;  // 映射到DrawItem上
                
                CharType vPosType;
                if (vItemOffset > 0)
                    vPosType = HC.GetCharType((ushort)vText[vItemOffset - 1]);
                else
                    vPosType = HC.GetCharType((ushort)vText[1 - 1]);

                int vStartOffset = 0;
                for (int i = vItemOffset - 1; i >= 1; i--)  // 往前找Char类型不一样的位置
                {
                    if (HC.GetCharType((ushort)vText[i - 1]) != vPosType)
                    {
                        vStartOffset = i;
                        break;
                    }
                }
            
                int vEndOffset = vText.Length;
                for (int i = vItemOffset + 1; i <= vText.Length; i++)  // 往后找Char类型不一样的位置
                {
                    if (HC.GetCharType((ushort)vText[i - 1]) != vPosType)
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

        public bool CanEdit()
        {
            bool Result = !FReadOnly;
            if (!Result)
                User.MessageBeep(0);

            return Result;
        }

        public void DeleteItems(int AStartNo, int AEndNo = -1)
        {
            InitializeField();
            DisSelect();  // 防止删除后原选中ItemNo不存在

            int vEndNo;
            if (AEndNo < 0)
                vEndNo = AStartNo;
            else
                vEndNo = AEndNo;

            int vDelCount = vEndNo - AStartNo + 1;
            //FormatItemPrepare(vStartNo, vEndNo);
            Items.RemoveRange(AStartNo, vDelCount);
            //ReFormatData_(vStartNo, vEndNo - vDelCount, -vDelCount);
            if (Items.Count == 0)
            {
                HCCustomItem vItem = CreateDefaultTextItem();
                vItem.ParaFirst = true;
                Items.Add(vItem);  // 不使用InsertText，为避免其触发ReFormat时因为没有格式化过，获取不到对应的DrawItem
            }
            else  // 删除完了还有
            if ((AStartNo > 0) && (!Items[AStartNo].ParaFirst))
            {
                if (Items[AStartNo - 1].CanConcatItems(Items[AStartNo]))
                {
                    //vItem := Items[AStartNo - 1];
                    Items[AStartNo - 1].Text = Items[AStartNo - 1].Text + Items[AStartNo].Text;
                    Items.RemoveAt(AStartNo);
                }
            }
        }

        /// <summary> 添加Data到当前 </summary>
        /// <param name="ASrcData">源Data</param>
        public void AddData(HCCustomData ASrcData)
        {
            this.InitializeField();

            int vAddStartNo = 0;
            if (Items[Items.Count - 1].CanConcatItems(ASrcData.Items[0]))
            {
                Items[Items.Count - 1].Text = Items[Items.Count - 1].Text + ASrcData.Items[0].Text;
                vAddStartNo = 1;
            }
            else
                vAddStartNo = 0;

            for (int i = vAddStartNo; i <= ASrcData.Items.Count - 1; i++)
            {
                if ((ASrcData.Items[i].StyleNo < HCStyle.Null)
                  || ((ASrcData.Items[i].StyleNo > HCStyle.Null) && (ASrcData.Items[i].Text != "")))
                {
                    HCCustomItem vItem = CreateItemByStyle(ASrcData.Items[i].StyleNo);
                    vItem.Assign(ASrcData.Items[i]);
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
        public bool InsertText(string AText)
        {
            if (!CanEdit())
                return false;

            bool Result = false;
            DeleteSelected();

            bool vParaFirst = false;

            string[] vStrings = AText.Split(new string[] { "\r\n" }, StringSplitOptions.None);
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

            InitializeMouseField();  // 201807311101
            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();
            Style.UpdateInfoReScroll();

            return Result;
        }

        /// <summary> 在光标处插入指定行列的表格 </summary>
        public bool InsertTable(int ARowCount, int  AColCount)
        {
            if (!CanEdit())
                return false;

            bool Result = false;

            HCCustomRichData vTopData = GetTopLevelData();
            HCTableItem vItem = new HCTableItem(vTopData, ARowCount, AColCount, vTopData.Width);
            Result = InsertItem(vItem);
            InitializeMouseField();

            return Result;
        }

        /// <summary> 在光标处插入直线 </summary>
        public bool InsertLine(int ALineHeight)
        {
            if (!CanEdit())
                return false;

            bool Result = false;

            HCCustomRichData vTopData = GetTopLevelData();
            HCLineItem vItem = new HCLineItem(vTopData, vTopData.Width, 1);
            vItem.LineHeight = (byte)ALineHeight;

            Result = InsertItem(vItem);
            InitializeMouseField();

            return Result;
        }

        public bool TableInsertRowAfter(byte ARowCount)
        {
            if (!CanEdit())
                return false;

            InsertProcEventHandler vEvent = delegate(HCCustomItem AItem)
            {
                return (AItem as HCTableItem).InsertRowAfter(ARowCount);
            };

            return TableInsertRC(vEvent);
        }

        public bool TableInsertRowBefor(byte ARowCount)
        {
            if (!CanEdit())
                return false;

            InsertProcEventHandler vEvent = delegate(HCCustomItem AItem)
            {
                return (AItem as HCTableItem).InsertRowBefor(ARowCount);
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

        public bool TableInsertColAfter(byte AColCount)
        {
            if (!CanEdit())
                return false;

            InsertProcEventHandler vEvent = delegate(HCCustomItem AItem)
            {
                return (AItem as HCTableItem).InsertColAfter(AColCount);
            };

            return TableInsertRC(vEvent);
        }

        public bool TableInsertColBefor(byte AColCount)
        {
            if (!CanEdit())
                return false;

            InsertProcEventHandler vEvent = delegate(HCCustomItem AItem)
            {
                return (AItem as HCTableItem).InsertColBefor(AColCount);
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
                Undo_StartRecord();
                Undo_ItemSelf(vItemNo, HC.OffsetInner);
                Result = (Items[vItemNo] as HCTableItem).MergeSelectCells();
                if (Result)  // 合并成功
                {
                    int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                    GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                    FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                    ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                    DisSelect();  // 合并后清空选中，会导致当前ItemNo没有了
                    InitializeMouseField();  // 201807311101
                    Style.UpdateInfoRePaint();
                }
            }

            return Result;
        }

        // Format仅负责格式化Item，ReFormat仅负责格式化后对后面Item和DrawItem的关联处理
        // 目前仅单元格用了，需要放到CellData中吗？
        public void ReFormat(int AStartItemNo)
        {
            if (AStartItemNo > 0)
            {
                FormatItemPrepare(AStartItemNo, Items.Count - 1);
                FormatData(AStartItemNo, Items.Count - 1);
                DrawItems.DeleteFormatMark();
            }
            else  // 从0开始，适用于处理外部调用提供的方法(非内部操作)引起的Item变化且没处理Item对应的DrawItem的情况
            {
                DrawItems.Clear();
                FormatData(0, Items.Count - 1);
            }
        }

        /// <summary> 重新格式化当前Item(用于仅修改当前Item属性或内容) </summary>
        public void ReFormatActiveItem()
        {
            if (SelectInfo.StartItemNo >= 0)
            {
                int vFormatFirstItemNo = -1, vFormatLastItemNo = -1;
                GetReformatItemRange(ref vFormatFirstItemNo, ref vFormatLastItemNo);
                FormatItemPrepare(vFormatFirstItemNo, vFormatLastItemNo);
                ReFormatData_(vFormatFirstItemNo, vFormatLastItemNo);
                Style.UpdateInfoRePaint();
                Style.UpdateInfoReCaret();
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
            string Result = "";
            if ((!FMouseMoveRestrain) && (FMouseMoveItemNo >= 0))
                Result = Items[FMouseMoveItemNo].GetHint();
            else
                Result = "";

            return Result;
        }

        /// <summary> 返回当前光标处的顶层Data </summary>
        public HCCustomRichData GetTopLevelData()
        {
            HCCustomRichData Result = null;
            if ((SelectInfo.StartItemNo >= 0) && (SelectInfo.EndItemNo < 0))
            {
                if ((Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null)
                  && (SelectInfo.StartItemOffset == HC.OffsetInner))
                    Result = (Items[SelectInfo.StartItemNo] as HCCustomRectItem).GetActiveData() as HCCustomRichData;
            }

            if (Result == null)
                Result = this;

            return Result;
        }

        /// <summary> 返回指定位置处的顶层Data </summary>
        public HCCustomRichData GetTopLevelDataAt(int X, int  Y)
        {
            HCCustomRichData Result = null;
            int vItemNo = -1, vOffset = -1, vDrawItemNo = -1;
            bool vRestrain = false;
            GetItemAt(X, Y, ref vItemNo, ref vOffset, ref vDrawItemNo, ref vRestrain);
            if ((!vRestrain) && (vItemNo >= 0))
            {
                if (Items[vItemNo].StyleNo < HCStyle.Null)
                {
                    int vX = -1, vY = -1;
                    CoordToItemOffset(X, Y, vItemNo, vOffset, ref vX, ref vY);
                    Result = (Items[vItemNo] as HCCustomRectItem).GetTopLevelDataAt(vX, vY) as HCCustomRichData;
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

        public ItemNotifyEventHandler OnInsertItem
        {
            get { return FOnInsertItem; }
            set { FOnInsertItem = value; }
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
