﻿/*******************************************************}
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
    public delegate bool TextItemActionEventHandler(HCTextItem aTextItem);
    public delegate bool RectItemActionEventHandler(HCCustomRectItem aRectItem);
    public delegate void ItemMouseEventHandler(HCCustomData aData, int aItemNo, int aOffset, MouseEventArgs e);
    public delegate void DrawItemMouseEventHandle(HCCustomData data, int itemNo, int offset, int drawItemNo, MouseEventArgs e);

    public class HCRichData : HCUndoData
    {
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
    
        private bool FReadOnly, FSelecting, FDraging;
        private bool FCheckEmptyItem;
        private DataItemNoEventHandler FOnItemResized;
        private ItemMouseEventHandler FOnItemMouseDown, FOnItemMouseUp;
        private DrawItemMouseEventHandle FOnDrawItemMouseMove;
        private EventHandler FOnCreateItem;  // 新建了Item(目前主要是为了打字和用中文输入法输入英文时痕迹的处理)
        private DataActionEventHandler FOnAcceptAction;

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
                        if (((AMouseDownItemNo > FSelectSeekNo) && (AMouseDownItemNo < SelectInfo.EndItemNo))
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
                        if (((AMouseDownItemNo > SelectInfo.StartItemNo) && (AMouseDownItemNo < FSelectSeekNo))
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

        /// <summary> 初始化为只有一个空Item的Data</summary>
        private void SetEmptyData()
        {
            if (this.Items.Count == 0)
            {
                HCCustomItem vItem = CreateDefaultTextItem();
                vItem.ParaFirst = true;
                Items.Add(vItem);

                ReFormat();
            }
        }

        /// <summary> Data只有空行Item时插入Item(用于替换当前空行Item的情况) </summary>
        private bool EmptyDataInsertItem(HCCustomItem aItem)
        {
            if ((aItem.StyleNo > HCStyle.Null) && (aItem.Text == ""))
                return false;

            aItem.ParaNo = Items[0].ParaNo;
            UndoAction_DeleteItem(0, 0);
            Items.Clear();
            DrawItems.Clear();
            aItem.ParaFirst = true;
            Items.Add(aItem);
            UndoAction_InsertItem(0, 0);
            SelectInfo.StartItemOffset = GetItemOffsetAfter(0);

            ReFormat();
            return true;
        }

        /// <summary> 是否完美选中（判断文本和Rect混合选择时，Rect全选中。无选中是返回true）</summary>
        private bool SelectPerfect()
        {
            if (SelectInfo.EndItemNo >= 0)  // 混选时某RectItem只有一部分选中时不处理,仅取消选中
            {
                for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)
                {
                    if ((this.Items[i].StyleNo < HCStyle.Null) && this.Items[i].IsSelectPart)
                        return false;
                }
            }

            return true;
        }

        /// <summary> 为避免表格插入行、列大量重复代码 </summary>
        private bool RectItemAction(RectItemActionEventHandler aAction)
        {
            bool Result = false;
            int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
            int vCurItemNo = GetActiveItemNo();
            if (Items[vCurItemNo] is HCCustomRectItem)
            {
                GetFormatRange(vCurItemNo, 1, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                Undo_New();
                if ((Items[vCurItemNo] as HCCustomRectItem).MangerUndo)
                    UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                else
                    UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                Result = aAction(Items[vCurItemNo] as HCCustomRectItem);
                if (Result)
                {
                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0);
                    Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();
                }

                InitializeMouseField();
            }

            return Result;
        }

        private bool TextItemAction(TextItemActionEventHandler aAction)
        {
            bool vResult = false;
            int vCurItemNo = this.GetActiveItemNo();
            int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
            if (this.Items[vCurItemNo] is HCTextItem)
            {
                GetFormatRange(vCurItemNo, 1, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                Undo_New();
                UndoAction_ItemMirror(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);

                //UndoAction_ItemSelf(vCurItemNo, OffsetInner);
                vResult = aAction(Items[vCurItemNo] as HCTextItem);
                if (vResult)
                {
                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0);
                    //DisSelect;
                    Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();
                }

                InitializeMouseField();
            }

            return vResult;
        }

        private void InitializeMouseField()
        {
            FMouseLBDowning = false;
            FMouseDownItemNo = -1;
            FMouseDownItemOffset = -1;
            FMouseMoveItemNo = -1;
            FMouseMoveItemOffset = -1;
            FMouseMoveDrawItemNo = -1;
            FMouseMoveRestrain = false;
            FSelecting = false;
            FDraging = false;
        }

        /// <summary> 划完完成后最后操作位置是否在选中范围起始 </summary>
        private bool IsSelectSeekStart()
        {
            return (FSelectSeekNo == SelectInfo.StartItemNo) && (FSelectSeekOffset == SelectInfo.StartItemOffset);
        }

        #region AdjustSelectRange
        /// <summary> 给定起始结束位置，判断正确的选中位置并修正输出 </summary>
        /// <param name="ADrawItemNo">光标处的DrawItem(暂时无意义)</param>
        /// <param name="AStartItemNo"></param>
        /// <param name="AStartItemOffset"></param>
        /// <param name="AEndItemNo"></param>
        /// <param name="AEndItemNoOffset"></param>
        protected void AdjustSelectRange(ref int aStartItemNo, ref int aStartItemOffset, ref int aEndItemNo, ref int aEndItemNoOffset)
        {
            bool vLeftToRight = false;
            // 记录原来选中范围
            int vOldStartItemNo = SelectInfo.StartItemNo;
            int vOldEndItemNo = SelectInfo.EndItemNo;

            if (aStartItemNo < aEndItemNo)
            {
                vLeftToRight = true;

                if ((Items[aStartItemNo].Length > 0)
                    && (aStartItemOffset == GetItemOffsetAfter(aStartItemNo)))  // 起始在Item最后面，改为下一个Item开始
                {
                    if ((aStartItemNo < Items.Count - 1) && (!Items[aStartItemNo + 1].ParaFirst))
                    {
                        aStartItemNo = aStartItemNo + 1;
                        aStartItemOffset = 0;
                    }
                }

                if ((aStartItemNo != aEndItemNo) && (aEndItemNo >= 0)
                    && (Items[aEndItemNo].Length > 0) && (aEndItemNoOffset == 0)
                    && (!Items[aEndItemNo].ParaFirst))
                {
                    Items[aEndItemNo].DisSelect();  // 从前往后选，鼠标移动到前一次前面，原鼠标处被移出选中范围

                    aEndItemNo = aEndItemNo - 1;
                    aEndItemNoOffset = GetItemOffsetAfter(aEndItemNo);
                }
            }
            else
            if (aEndItemNo < aStartItemNo)
            {
                vLeftToRight = false;

                if ((aStartItemNo > 0) && (aStartItemOffset == 0)
                    && (!Items[aStartItemNo].ParaFirst))
                {
                    aStartItemNo = aStartItemNo - 1;
                    aStartItemOffset = GetItemOffsetAfter(aStartItemNo);
                }

                if ((aStartItemNo != aEndItemNo) && (aEndItemNoOffset == GetItemOffsetAfter(aEndItemNo)))
                {
                    Items[aEndItemNo].DisSelect();  // 从后往前选，鼠标移动到前一个后面，原鼠标处被移出选中范围

                    if ((aEndItemNo < Items.Count - 1) && (!Items[aEndItemNo + 1].ParaFirst))
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
                if (aEndItemNoOffset < aStartItemOffset)  // 选中结束位置小于起始位置
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

                    if (SelectInfo.StartItemNo >= 0)
                    {
                        if ((aStartItemOffset == 0) || (aStartItemOffset == GetItemOffsetAfter(SelectInfo.StartItemNo)))  // 回到两头
                            Items[SelectInfo.StartItemNo].DisSelect();
                    }

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

        protected override void DoLoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            FReadOnly = false;
            if (!CanEdit())
                return;

            base.DoLoadFromStream(aStream, aStyle, aFileVersion);

            this.BeginFormat();
            try
            {
                FCheckEmptyItem = false;
                InsertStream(aStream, aStyle, aFileVersion);
                if (aFileVersion > 48)
                {

                }
                else
                if (aFileVersion > 47)
                {
                    byte[] vBuffer = BitConverter.GetBytes(false);
                    aStream.Read(vBuffer, 0, vBuffer.Length);
                    FReadOnly = BitConverter.ToBoolean(vBuffer, 0);
                    if (FReadOnly)
                        FReadOnly = false;
                }

                if (FCheckEmptyItem)
                    this.DeleteEmptyItem();

                ReSetSelectAndCaret(0, 0);
            }
            finally
            {
                this.EndFormat();
            }
        }

        protected override void ReSetSelectAndCaret(int aItemNo, int aOffset, bool aNextWhenMid = false)
        {
            base.ReSetSelectAndCaret(aItemNo, aOffset, aNextWhenMid);
            FSelectSeekNo = SelectInfo.StartItemNo;
            FSelectSeekOffset = SelectInfo.StartItemOffset;
        }

        protected virtual bool DoAcceptAction(int aItemNo, int aOffset, HCAction aAction)
        {
            bool vResult = this.Items[aItemNo].AcceptAction(aOffset, SelectInfo.StartRestrain, aAction);    
            if (vResult && (FOnAcceptAction != null))
                vResult = FOnAcceptAction(this, aItemNo, aOffset, aAction);

            return vResult;
        }

        protected virtual void DoDrawItemMouseMove(HCCustomData data, int itemNo, int offset, int drawItemNo, MouseEventArgs e)
        {
            if (FOnDrawItemMouseMove != null)
                FOnDrawItemMouseMove(data, itemNo, offset, drawItemNo, e);
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

        protected virtual void DoItemResized(int aItemNo)
        {
            this.Change();
            if (FOnItemResized != null)
                FOnItemResized(this, aItemNo);
        }

        protected virtual int GetHeight()
        {
            return CalcContentHeight();
        }

        protected virtual void SetReadOnly(bool value)
        {
            FReadOnly = value;
        }

        protected int MouseMoveDrawItemNo
        {
            get { return FMouseMoveDrawItemNo; }
        }

        public HCRichData(HCStyle aStyle)
            : base(aStyle)
        {
            FReadOnly = false;
            InitializeField();
            SetEmptyData();
        }

        public override HCCustomItem CreateItemByStyle(int aStyleNo)
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

                    case HCStyle.Button:
                        Result = new HCButtonItem(this, "");
                        break;

                    case HCStyle.RadioGroup:
                        Result = new HCRadioGroup(this);
                        break;

                    case HCStyle.SupSubScript:
                        Result = new HCSupSubScriptItem(this, "", "");
                        break;

                    case HCStyle.FloatLine:
                        Result = new HCFloatLineItem(this);
                        break;

                    case HCStyle.FloatBarCode:
                        Result = new HCFloatBarCodeItem(this);
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

        public override bool CanEdit()
        {
            if (!FReadOnly)
                return base.CanEdit();
            else
                return false;
        }

        public override void Clear()
        {
            InitializeField();

            base.Clear();
            SetEmptyData();
        }

        // 选中内容应用样式

        #region ApplySelectTextStyle子方法 ApplySameItem选中在同一个Item
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
                CurStyleNo = vStyleNo;
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

                        Style.States.Include(HCState.hosInsertBreakItem);
                        try
                        {
                            Items.Insert(aItemNo + 1, vAfterItem);
                        }
                        finally
                        {
                            Style.States.Exclude(HCState.hosInsertBreakItem);
                        }

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

        #region ApplySelectTextStyle子方法 ApplyRangeStartItem选中在不同Item中，处理选中起始Item
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

                        Style.States.Include(HCState.hosInsertBreakItem);
                        try
                        {
                            Items.Insert(aItemNo + 1, vAfterItem);
                        }
                        finally
                        {
                            Style.States.Exclude(HCState.hosInsertBreakItem);
                        }

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

        #region ApplySelectTextStyle子方法 ApplyRangeEndItem选中在不同Item中，处理选中结束Item
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

        #region ApplySelectTextStyle子方法 ApplyNorItem选中在不同Item，处理中间Item
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

        public override void ApplySelectTextStyle(HCStyleMatch aMatchStyle)
        {
            Undo_New();

            this.InitializeField();
            int vExtraCount = 0, vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;

            if (!SelectExists())
            {
                if (CurStyleNo > HCStyle.Null)
                {
                    aMatchStyle.Append = !aMatchStyle.StyleHasMatch(Style, CurStyleNo);  // 根据当前判断是添加样式还是减掉样式
                    CurStyleNo = aMatchStyle.GetMatchStyleNo(Style, CurStyleNo);

                    Style.UpdateInfoRePaint();
                    if (Items[SelectInfo.StartItemNo].Length == 0)
                    {
                        GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                        FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                        UndoAction_ItemStyle(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, CurStyleNo);
                        Items[SelectInfo.StartItemNo].StyleNo = CurStyleNo;

                        ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                        Style.UpdateInfoReCaret();
                    }
                    else  // 不是空行
                    {
                        if (Items[SelectInfo.StartItemNo] is HCTextRectItem)
                        {
                            GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                            (Items[SelectInfo.StartItemNo] as HCTextRectItem).TextStyleNo = CurStyleNo;
                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                        }

                        Style.UpdateInfoReCaret(false);
                    }
                }

                ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                return;
            }

            if (SelectInfo.EndItemNo < 0)  // 没有连续选中内容
            {
                if (Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null)
                {
                    if ((Items[SelectInfo.StartItemNo] as HCCustomRectItem).MangerUndo)
                        UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                    else
                        UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                    (Items[SelectInfo.StartItemNo] as HCCustomRectItem).ApplySelectTextStyle(Style, aMatchStyle);
                    if ((Items[SelectInfo.StartItemNo] as HCCustomRectItem).IsFormatDirty)
                    {
                        // 如果改变会引起RectItem宽度变化，则需要格式化到最后一个Item
                        GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                        FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                        ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                        (Items[SelectInfo.StartItemNo] as HCCustomRectItem).IsFormatDirty = false;
                    }
                    else
                        this.FormatInit();
                }
            }
            else  // 有连续选中内容
            {
                GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                if (SelectInfo.StartItemNo != SelectInfo.EndItemNo)
                    vFormatLastItemNo = GetParaLastItemNo(SelectInfo.EndItemNo);

                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)
                {
                    if (Items[i].StyleNo > HCStyle.Null)
                    {
                        aMatchStyle.Append = !aMatchStyle.StyleHasMatch(Style, Items[i].StyleNo);  // 根据第一个判断是添加样式还是减掉样式
                        break;
                    }
                    else
                    {
                        if (Items[i] is HCTextRectItem)
                        {
                            aMatchStyle.Append = !aMatchStyle.StyleHasMatch(Style, (Items[i] as HCTextRectItem).TextStyleNo);  // 根据第一个判断是添加样式还是减掉样式
                            break;
                        }
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

                    // 样式变化后，从后往前处理选中范围内变化后的合并 
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
                            Items.Delete(SelectInfo.StartItemNo);
                            vExtraCount--;

                            SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                            SelectInfo.StartItemOffset = vLen;
                            SelectInfo.EndItemNo = SelectInfo.EndItemNo - 1;
                            if (SelectInfo.StartItemNo == SelectInfo.EndItemNo)
                                SelectInfo.EndItemOffset = SelectInfo.EndItemOffset + vLen;
                        }
                    }
                }

                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + vExtraCount, vExtraCount);
            }

            MatchItemSelectState();
            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();
        }

        #region ApplySelectParaStyle子方法DoApplyParagraphStyle
        private void DoApplyParagraphStyle(int aItemNo, HCParaMatch aMatchStyle)
        {
            int vFirstNo = -1, vLastNo = -1;
            GetParaItemRang(aItemNo, ref vFirstNo, ref vLastNo);
            int vParaNo = aMatchStyle.GetMatchParaNo(this.Style, GetItemParaStyle(aItemNo));

            if (GetItemParaStyle(vFirstNo) != vParaNo)
            {
                UndoAction_ItemParaNo(vFirstNo, 0, vParaNo);
                for (int i = vFirstNo; i <= vLastNo; i++)
                    Items[i].ParaNo = vParaNo;
            }
        }
        #endregion

        #region ApplySelectParaStyle子方法ApplyParaSelectedRangStyle
        private void ApplyParagraphSelecteStyle(HCParaMatch aMatchStyle)
        {
            int vFirstNo = -1, vLastNo = -1;
            GetParaItemRang(SelectInfo.StartItemNo, ref vFirstNo, ref vLastNo);
            DoApplyParagraphStyle(SelectInfo.StartItemNo, aMatchStyle);

            int i = vLastNo + 1;
            while (i <= SelectInfo.EndItemNo)
            {
                if (Items[i].ParaFirst)
                    DoApplyParagraphStyle(i, aMatchStyle);

                i++;
            }
        }
        #endregion

        public override void ApplySelectParaStyle(HCParaMatch aMatchStyle)
        {
            if (SelectInfo.StartItemNo < 0)
                return;

            //GetReformatItemRange(vFormatFirstItemNo, vFormatLastItemNo);
            int vFormatFirstDrawItemNo = -1;
            int vFormatLastItemNo = -1;

            Undo_New();
            if (SelectInfo.EndItemNo >= 0)
            {
                vFormatFirstDrawItemNo = Items[GetParaFirstItemNo(SelectInfo.StartItemNo)].FirstDItemNo;
                vFormatLastItemNo = GetParaLastItemNo(SelectInfo.EndItemNo);
                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                ApplyParagraphSelecteStyle(aMatchStyle);
                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
            }
            else  // 没有选中内容
            {
                if ((GetItemStyle(SelectInfo.StartItemNo) < HCStyle.Null)
                    && (SelectInfo.StartItemOffset == HC.OffsetInner))
                {
                    if ((Items[SelectInfo.StartItemNo] as HCCustomRectItem).MangerUndo)
                        UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                    else
                        UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                    vFormatFirstDrawItemNo = GetFormatFirstDrawItem(Items[SelectInfo.StartItemNo].FirstDItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, SelectInfo.StartItemNo);
                    (Items[SelectInfo.StartItemNo] as HCCustomRectItem).ApplySelectParaStyle(this.Style, aMatchStyle);
                    ReFormatData(vFormatFirstDrawItemNo, SelectInfo.StartItemNo);
                }
                else
                {
                    vFormatFirstDrawItemNo = Items[GetParaFirstItemNo(SelectInfo.StartItemNo)].FirstDItemNo;
                    vFormatLastItemNo = GetParaLastItemNo(SelectInfo.StartItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                    DoApplyParagraphStyle(SelectInfo.StartItemNo, aMatchStyle);  // 应用样式
                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                }
            }

            if (FSelectSeekNo >= 0)
                FCurParaNo = Items[FSelectSeekNo].ParaNo;
            else
                FCurParaNo = Items[SelectInfo.StartItemNo].ParaNo;

            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();
        }

        public override void ApplyTableCellAlign(HCContentAlign aAlign)
        {
            if (!CanEdit()) 
                return;

            RectItemActionEventHandler vEvent = delegate(HCCustomRectItem aRectItem)
            {
                (aRectItem as HCTableItem).ApplyContentAlign(aAlign);
                return true;
            };

            RectItemAction(vEvent);
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

        #region DeleteSelected子方法DeleteItemSelectComplate删除全选中的单个Item
        private bool DeleteItemSelectComplate(ref int vDelCount, int vParaFirstItemNo, int vParaLastItemNo,
            int vFormatFirstItemNo, int vFormatLastItemNo)
        {
            if (!DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actDeleteItem))
                return false;
            
            UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
            Items.RemoveAt(SelectInfo.StartItemNo);

            vDelCount++;

            if ((SelectInfo.StartItemNo > vFormatFirstItemNo) && (SelectInfo.StartItemNo < vFormatLastItemNo))
            {
                int vLen = Items[SelectInfo.StartItemNo - 1].Length;
                if (MergeItemText(Items[SelectInfo.StartItemNo - 1], Items[SelectInfo.StartItemNo]))
                {
                    UndoAction_InsertText(SelectInfo.StartItemNo - 1, vLen + 1, Items[SelectInfo.StartItemNo].Text);
                    UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                    Items.RemoveAt(SelectInfo.StartItemNo);
                    vDelCount++;

                    SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                    SelectInfo.StartItemOffset = vLen;
                }
                else  // 删除位置前后不能合并，光标置为前一个后面
                {
                    SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                    SelectInfo.StartItemOffset = GetItemOffsetAfter(SelectInfo.StartItemNo);
                }
            }
            else
            {
                if (SelectInfo.StartItemNo == vParaFirstItemNo)  // 段第一个ItemNo
                {
                    if (vParaFirstItemNo == vParaLastItemNo)  // 段就一个Item全删除了
                    {
                        HCCustomItem vNewItem = CreateDefaultTextItem();
                        this.CurStyleNo = vNewItem.StyleNo; // 防止RectItem删除插入文本当前样式不正确
                        vNewItem.ParaFirst = true;
                        Items.Insert(SelectInfo.StartItemNo, vNewItem);
                        UndoAction_InsertItem(SelectInfo.StartItemNo, 0);

                        SelectInfo.StartItemOffset = 0;
                        vDelCount--;
                    }
                    else
                    {
                        SelectInfo.StartItemOffset = 0;

                        UndoAction_ItemParaFirst(SelectInfo.StartItemNo, 0, true);
                        Items[SelectInfo.StartItemNo].ParaFirst = true;
                    }
                }
                else
                {
                    if (SelectInfo.StartItemNo == vParaLastItemNo)  // 段最后一个ItemNo
                    {
                        SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                        SelectInfo.StartItemOffset = GetItemOffsetAfter(SelectInfo.StartItemNo);
                    }
                    else  // 全选中的Item是起始格式化或结束格式化或在段内
                    {     // 这里的代码会触发吗？
                        if (SelectInfo.StartItemNo > 0)
                        {
                            int vLen = Items[SelectInfo.StartItemNo - 1].Length;
                            if (MergeItemText(Items[SelectInfo.StartItemNo - 1], Items[SelectInfo.StartItemNo]))
                            {
                                UndoAction_InsertText(SelectInfo.StartItemNo - 1, vLen + 1, Items[SelectInfo.StartItemNo].Text);
                                UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                Items.RemoveAt(SelectInfo.StartItemNo);
                                vDelCount++;
                                SelectInfo.StartItemOffset = vLen;
                            }

                            SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                            SelectInfo.StartItemOffset = GetItemOffsetAfter(SelectInfo.StartItemNo);  // 删除选中再插入时如果是RectItem用原有Offset不行
                        }
                    }
                }
            }

            return true;
        }
        #endregion

        /// <summary> 是否成功删除了选中内容（无选中返回true） </summary>
        public override bool DeleteSelected()
        {
            if (!CanEdit())
                return false;

            if (!SelectPerfect())
            {
                this.DisSelect();
                return false;
            }

            if (!SelectExists())
                return true;

            if (!DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actDeleteSelected))
                return false;

            bool Result = false;
            bool vSelectSeekStart = IsSelectSeekStart();

            int vDelCount = 0;
            this.InitializeField();  // 删除后原鼠标处可能已经没有了

            int vFormatFirstDrawItemNo = -1, vFormatFirstItemNo = -1, vFormatLastItemNo = -1,
                vParaFirstItemNo = -1, vParaLastItemNo = -1, vUnDeleteSeekItemNo = -1;

            if ((SelectInfo.EndItemNo < 0)
                && (Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null))
            {
                // 如果变动会引起RectItem的宽度变化，则需要格式化到段最后一个Item
                GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                if (this.FormatCount > 0)
                    vFormatFirstItemNo = SelectInfo.StartItemNo;
                else
                    vFormatFirstItemNo = DrawItems[vFormatFirstDrawItemNo].ItemNo;

                Undo_New();

                if ((Items[SelectInfo.StartItemNo] as HCCustomRectItem).IsSelectComplateTheory())
                {
                    GetParaItemRang(SelectInfo.StartItemNo, ref vParaFirstItemNo, ref vParaLastItemNo);
                    Result = DeleteItemSelectComplate(ref vDelCount, vParaFirstItemNo, vParaLastItemNo,
                        vFormatFirstItemNo, vFormatLastItemNo);
                }
                else
                {
                    if ((Items[SelectInfo.StartItemNo] as HCCustomRectItem).MangerUndo)
                        UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                    else
                        UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                    Result = (Items[SelectInfo.StartItemNo] as HCCustomRectItem).DeleteSelected();
                }

                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
            }
            else  // 选中不是发生在RectItem内部
            {
                HCCustomItem vEndItem = Items[SelectInfo.EndItemNo];  // 选中结束Item
                if (SelectInfo.EndItemNo == SelectInfo.StartItemNo)
                {
                    Undo_New();

                    GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                    if (this.FormatCount > 0)
                        vFormatFirstItemNo = SelectInfo.StartItemNo;
                    else
                        vFormatFirstItemNo = DrawItems[vFormatFirstDrawItemNo].ItemNo;

                    if (vEndItem.IsSelectComplate)
                    {
                        GetParaItemRang(SelectInfo.StartItemNo, ref vParaFirstItemNo, ref vParaLastItemNo);
                        Result = DeleteItemSelectComplate(ref vDelCount, vParaFirstItemNo, vParaLastItemNo,
                            vFormatFirstItemNo, vFormatLastItemNo);
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
                        if (DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actBackDeleteText))
                        {
                            string vText = vEndItem.Text;
                            UndoAction_DeleteBackText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1,
                                vText.Substring(SelectInfo.StartItemOffset + 1 - 1, SelectInfo.EndItemOffset - SelectInfo.StartItemOffset));

                            vEndItem.Text = vText.Remove(SelectInfo.StartItemOffset + 1 - 1, SelectInfo.EndItemOffset - SelectInfo.StartItemOffset);
                        }
                    }

                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
                }
                else  // 选中发生在不同Item，起始(可能是段首)全选中结尾没全选，起始没全选结尾全选，起始结尾都没全选
                {
                    vUnDeleteSeekItemNo = SelectInfo.EndItemNo;

                    vFormatFirstItemNo = GetParaFirstItemNo(SelectInfo.StartItemNo);  // 取段第一个为起始
                    vFormatFirstDrawItemNo = Items[vFormatFirstItemNo].FirstDItemNo;
                    vFormatLastItemNo = GetParaLastItemNo(SelectInfo.EndItemNo);  // 取段最后一个为结束，如果变更注意下面
                    
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                    bool vSelStartParaFirst = Items[SelectInfo.StartItemNo].ParaFirst;
                    bool vSelStartComplate = Items[SelectInfo.StartItemNo].IsSelectComplate;  // 起始是否全选
                    bool vSelEndComplate = Items[SelectInfo.EndItemNo].IsSelectComplate;  // 结尾是否全选

                    Undo_New();

                    bool vStartDel = false, vEndDel = false;
                    // 先处理选中结束Item
                    if (vEndItem.StyleNo < HCStyle.Null)
                    {
                        if (vSelEndComplate)
                        {
                            if (DoAcceptAction(SelectInfo.EndItemNo, SelectInfo.EndItemOffset, HCAction.actDeleteItem))
                            {
                                UndoAction_DeleteItem(SelectInfo.EndItemNo, HC.OffsetAfter);
                                Items.Delete(SelectInfo.EndItemNo);
                                vEndDel = true;
                                vDelCount++;
                                vUnDeleteSeekItemNo--;
                            }
                            else
                                vUnDeleteSeekItemNo = SelectInfo.EndItemNo;
                        }
                        else
                        {
                            if (SelectInfo.EndItemOffset == HC.OffsetInner)  // 在其上
                                (vEndItem as HCCustomRectItem).DeleteSelected();
                            else
                            if (Items[SelectInfo.EndItemNo].ParaFirst)
                            {
                                UndoAction_ItemParaFirst(SelectInfo.EndItemNo, 0, false);
                                Items[SelectInfo.EndItemNo].ParaFirst = false;
                            }
                        }
                    }
                    else  // TextItem
                    {
                        if (vSelEndComplate)
                        {
                            if (DoAcceptAction(SelectInfo.EndItemNo, SelectInfo.EndItemOffset, HCAction.actDeleteItem))
                            {
                                UndoAction_DeleteItem(SelectInfo.EndItemNo, vEndItem.Length);
                                Items.RemoveAt(SelectInfo.EndItemNo);
                                vEndDel = true;
                                vDelCount++;
                                vUnDeleteSeekItemNo--;
                            }
                            else
                                vUnDeleteSeekItemNo = SelectInfo.EndItemNo;
                        }
                        else  // 文本且不在选中结束Item最后
                        if (DoAcceptAction(SelectInfo.EndItemNo, SelectInfo.EndItemOffset, HCAction.actBackDeleteText))
                        {
                            if (SelectInfo.EndItemOffset == 0 && Items[SelectInfo.EndItemNo].ParaFirst)
                            {
                                UndoAction_ItemParaFirst(SelectInfo.EndItemNo, 0, false);
                                Items[SelectInfo.EndItemNo].ParaFirst = false;
                            }
                            else
                            {
                                UndoAction_DeleteBackText(SelectInfo.EndItemNo, 1, vEndItem.Text.Substring(1 - 1, SelectInfo.EndItemOffset));
                                // 结束Item留下的内容
                                string vText = (vEndItem as HCTextItem).SubString(SelectInfo.EndItemOffset + 1,
                                    vEndItem.Length - SelectInfo.EndItemOffset);
                                vEndItem.Text = vText;
                            }
                        }
                        else
                            vUnDeleteSeekItemNo = SelectInfo.EndItemNo;
                    }

                    // 删除选中起始Item下一个到结束Item上一个
                    for (int i = SelectInfo.EndItemNo - 1; i >= SelectInfo.StartItemNo + 1; i--)
                    {
                        if (DoAcceptAction(i, 0, HCAction.actDeleteItem))
                        {
                            UndoAction_DeleteItem(i, 0);
                            Items.Delete(i);

                            vDelCount++;
                            vUnDeleteSeekItemNo--;
                        }
                        else
                            vUnDeleteSeekItemNo = i;
                    }

                    HCCustomItem vStartItem = Items[SelectInfo.StartItemNo];  // 选中起始Item
                    if (vStartItem.StyleNo < HCStyle.Null)
                    {
                        if (SelectInfo.StartItemOffset == HC.OffsetBefor)  // 在其前
                        {
                            if (DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actDeleteItem))  // 允许删除
                            {
                                UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                Items.Delete(SelectInfo.StartItemNo);
                                vStartDel = true;
                                vDelCount++;
                                vUnDeleteSeekItemNo--;
                            }
                            else
                                vUnDeleteSeekItemNo = SelectInfo.StartItemNo;
                        }
                        else
                        {
                            if (SelectInfo.StartItemOffset == HC.OffsetInner)
                                (vStartItem as HCCustomRectItem).DeleteSelected();
                        }
                    }
                    else  // 选中起始是TextItem
                    {
                        if (vSelStartComplate)
                        {
                            if (DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actDeleteItem))
                            {
                                UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                Items.RemoveAt(SelectInfo.StartItemNo);
                                vStartDel = true;
                                vDelCount++;
                                vUnDeleteSeekItemNo--;
                            }
                            else
                                vUnDeleteSeekItemNo = SelectInfo.StartItemNo;
                        }
                        else
                        if (SelectInfo.StartItemOffset < vStartItem.Length)
                        {
                            if (DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actBackDeleteText))
                            {
                                UndoAction_DeleteBackText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1,
                                    vStartItem.Text.Substring(SelectInfo.StartItemOffset + 1 - 1, vStartItem.Length - SelectInfo.StartItemOffset));
                                string vText = (vStartItem as HCTextItem).SubString(1, SelectInfo.StartItemOffset);
                                vStartItem.Text = vText;  // 起始留下的内容
                            }
                            else
                                vUnDeleteSeekItemNo = SelectInfo.StartItemNo;
                        }
                    }

                    if (SelectInfo.EndItemNo - SelectInfo.StartItemNo + 1 == vDelCount)  // 选中的Item都删除完
                    {
                        if (SelectInfo.StartItemNo == vFormatFirstItemNo)
                        {
                            if (SelectInfo.EndItemNo == vFormatLastItemNo)
                            {
                                HCCustomItem vNewItem = CreateDefaultTextItem();
                                this.CurStyleNo = vNewItem.StyleNo; // 防止RectItem删除插入文本当前样式不正确
                                vNewItem.ParaFirst = true;
                                Items.Insert(SelectInfo.StartItemNo, vNewItem);
                                UndoAction_InsertItem(SelectInfo.StartItemNo, vNewItem.Length);

                                vDelCount--;
                            }
                            else  // 选中结束不在段最后
                                Items[SelectInfo.EndItemNo - vDelCount + 1].ParaFirst = true;  // 选中结束位置后面的成为段首
                        }
                        else
                        {
                            if (SelectInfo.EndItemNo == vFormatLastItemNo)  // 结束在段最后
                            {
                                SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                                SelectInfo.StartItemOffset = GetItemOffsetAfter(SelectInfo.StartItemNo);
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
                    }
                    else  // 选中范围内的Item没有删除完
                    {
                        if (vStartDel)  // 起始删除完了
                        {
                            if (Items[vUnDeleteSeekItemNo].ParaFirst != vSelStartParaFirst)
                            {
                                UndoAction_ItemParaFirst(vUnDeleteSeekItemNo, 0, vSelStartParaFirst);
                                Items[vUnDeleteSeekItemNo].ParaFirst = vSelStartParaFirst;
                            }
                        }
                        else
                        {
                            if ((!vEndDel) && (SelectInfo.StartItemNo + 1 == SelectInfo.EndItemNo - vDelCount))  // 起始和结束都没有删除完且中间没有不可删除的
                            {
                                if (MergeItemText(Items[SelectInfo.StartItemNo], Items[SelectInfo.EndItemNo - vDelCount]))  // 起始和结束挨在一起了
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
                    }

                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
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
            return true;
        }

        /// <summary> 初始化相关字段和变量 </summary>
        public override void InitializeField()
        {
            FSelectSeekNo = -1;
            FSelectSeekOffset = -1;
            InitializeMouseField();
            base.InitializeField();
        }

        public override bool InsertStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            Int64 vDataSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vDataSize);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vDataSize = BitConverter.ToInt64(vBuffer, 0);
            if (vDataSize == 0)
                return false;

            if (!CanEdit())
                return false;

            if (!DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actInsertItem)) 
                return false;

            if (!SelectPerfect())
                return false;

            bool Result = false;
            HCCustomItem vAfterItem = null;
            bool vInsertBefor = false, vInsertEmptyLine = false;
            int vInsPos = 0, vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
            int vCaretParaNo = CurParaNo;

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
                        if (SelectInfo.StartItemOffset == HC.OffsetInner)  // 其上
                        {
                            GetFormatRange(SelectInfo.StartItemNo, HC.OffsetInner, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            Undo_New();
                            if ((Items[vInsPos] as HCCustomRectItem).MangerUndo)
                                UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                            else
                                UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                            aStream.Position -= vBuffer.Length;
                            Result = (Items[vInsPos] as HCCustomRectItem).InsertStream(aStream, aStyle, aFileVersion);
                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            return Result;
                        }
                        else
                        {
                            if (SelectInfo.StartItemOffset == HC.OffsetBefor)  // 其前
                                vInsertBefor = true;
                            else  // 其后
                                vInsPos = vInsPos + 1;
                        }
                    }
                    else  // TextItem
                    {
                        // 先判断光标是否在最后，防止空Item时SelectInfo.StartItemOffset = 0按其前处理
                        if (SelectInfo.StartItemOffset == Items[vInsPos].Length)
                        {
                            vInsertEmptyLine = IsEmptyLine(vInsPos);
                            vInsPos = vInsPos + 1;
                        }
                        else
                        {
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
                }

                int vItemCount = 0;
                vBuffer = BitConverter.GetBytes(vItemCount);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vItemCount = BitConverter.ToInt32(vBuffer, 0);

                if (vItemCount == 0)
                    return Result;

                // 因为插入的第一个可能和插入位置前一个合并，插入位置可能是行首，所以要从插入位置
                // 行上一个开始格式化，为简单处理，直接使用段首，可优化为上一行首
                //GetParaItemRang(SelectInfo.StartItemNo, vFormatFirstItemNo, vFormatLastItemNo);

                GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);

                // 计算格式化起始、结束ItemNo
                if (Items.Count > 0)
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                else
                {
                    vFormatFirstDrawItemNo = 0;
                    vFormatLastItemNo = -1;
                }

                int vItemCountAct = 0;  // 实际插入的数量
                int vIgnoreCount = 0;  // 忽略掉的数据
                Undo_New();

                int vStyleNo = HCStyle.Null;
                HCCustomItem vItem = null;
                for (int i = 0; i <= vItemCount - 1; i++)
                {
                    aStream.Read(vBuffer, 0, vBuffer.Length);  // besure vBuffer.Length = 4
                    vStyleNo = BitConverter.ToInt32(vBuffer, 0);
                    vItem = CreateItemByStyle(vStyleNo);
                    vItem.LoadFromStream(aStream, aStyle, aFileVersion);
                    if (aStyle != null)  // 有样式表
                    {
                        if (vItem.StyleNo > HCStyle.Null)
                            vItem.StyleNo = Style.GetStyleNo(aStyle.TextStyles[vItem.StyleNo], true);

                        if (Style.States.Contain(HCState.hosPasting))
                            vItem.ParaNo = vCaretParaNo;
                        else
                            vItem.ParaNo = Style.GetParaNo(aStyle.ParaStyles[vItem.ParaNo], true);
                    }
                    else  // 无样式表
                    {
                        if (vItem.StyleNo > HCStyle.Null)
                            vItem.StyleNo = CurStyleNo;

                        vItem.ParaNo = vCaretParaNo;
                    }

                    if (vItem.StyleNo > HCStyle.Null && vItem.Text == "" && !vItem.ParaFirst)
                    {
                        vIgnoreCount++;
                        vItem.Dispose();
                        continue;
                    }

                    if (i == 0)  // 插入的第一个Item
                    {
                        if (vInsertBefor)  // 第一个在某Item最前面插入(粘贴)
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
                    else  // 插入非第一个Item
                    if (!this.Loading && !vItem.ParaFirst && MergeItemText(Items[vInsPos + i - 1 - vIgnoreCount], vItem))  // 和插入位置前一个能合并，有些不允许复制的粘贴时会造成前后可合并
                    {
                        vIgnoreCount++;
                        vItem.Dispose();
                        continue;
                    }

                    if (!FCheckEmptyItem && i > 0 && !vItem.ParaFirst)
                    {
                        if (vItem.StyleNo > HCStyle.Null && vItem.Length == 0)
                            FCheckEmptyItem = true;

                        if (Items[vInsPos + i - vIgnoreCount - 1].StyleNo > HCStyle.Null
                            && Items[vInsPos + i - vIgnoreCount - 1].Length == 0)
                        {
                            FCheckEmptyItem = true;
                        }
                    }

                    Items.Insert(vInsPos + i - vIgnoreCount, vItem);
                    UndoAction_InsertItem(vInsPos + i - vIgnoreCount, 0);
                    vItemCountAct++;
                }

                vItemCount = CheckInsertItemCount(vInsPos, vInsPos + vItemCountAct - 1);  // 检查插入的Item是否合格并删除不合格

                int vInsetLastNo = vInsPos + vItemCount - 1;  // 光标在最后一个Item
                int vCaretOffse = GetItemOffsetAfter(vInsetLastNo);  // 最后一个Item后面

                if (vAfterItem != null)
                {
                    if (MergeItemText(Items[vInsetLastNo], vAfterItem))
                    {
                        UndoAction_InsertText(vInsetLastNo, Items[vInsetLastNo].Length - vAfterItem.Length + 1, vAfterItem.Text);
                        vAfterItem.Dispose();
                    }
                    else  // 插入最后一个和后半部分不能合并
                    {
                        Style.States.Include(HCState.hosInsertBreakItem);
                        try
                        {
                            Items.Insert(vInsetLastNo + 1, vAfterItem);
                        }
                        finally
                        {
                            Style.States.Exclude(HCState.hosInsertBreakItem);
                        }

                        UndoAction_InsertItem(vInsetLastNo + 1, 0);

                        vItemCount++;
                    }
                }

                if (vInsPos > 0)
                {
                    if (vInsertEmptyLine)  // 插入位置前面是空行Item
                    {
                        //if (Items.Count > 1) // 有必要时可以这样判断前面把不合格的处理掉后造成实际有效插入0个又把第一个空行删除掉造成Data没有Item的错误
                        //{
                        UndoAction_ItemParaFirst(vInsPos, 0, Items[vInsPos - 1].ParaFirst);
                        Items[vInsPos].ParaFirst = Items[vInsPos - 1].ParaFirst;

                        if (Items[vInsPos - 1].PageBreak)
                        {
                            UndoAction_ItemPageBreak(vInsPos, 0, true);
                            Items[vInsPos].PageBreak = true;
                        }

                        UndoAction_DeleteItem(vInsPos - 1, 0);
                        Items.RemoveAt(vInsPos - 1);  // 删除空行

                        vItemCount--;
                        vInsetLastNo--;
                        //}
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
                            Items.Delete(vInsPos);

                            if (vItemCount == 1)
                                vCaretOffse = vOffsetStart + vCaretOffse;

                            vItemCount--;
                            vInsetLastNo--;
                        }
                        else
                        if (Items[vInsPos].StyleNo > HCStyle.Null && Items[vInsPos].Length == 0)
                        {
                            UndoAction_DeleteItem(vInsPos, 0);
                            Items.Delete(vInsPos);

                            vItemCount--;
                            vInsetLastNo--;
                        }
                    }

                    if ((vInsetLastNo < Items.Count - 1)  // 插入最后Item和后面的能合并)
                        && (!Items[vInsetLastNo + 1].ParaFirst)
                        && MergeItemText(Items[vInsetLastNo], Items[vInsetLastNo + 1]))
                    {
                        UndoAction_InsertText(vInsetLastNo, Items[vInsetLastNo].Length - Items[vInsetLastNo + 1].Length + 1, 
                            Items[vInsetLastNo + 1].Text);
                        UndoAction_DeleteItem(vInsetLastNo + 1, 0);

                        Items.Delete(vInsetLastNo + 1);
                        vItemCount--;
                    }
                }
                else  // 在最开始第0个位置处插入
                //if (vInsetLastNo < Items.Count - 1) then
                {
                    if (MergeItemText(Items[vInsetLastNo], Items[vInsetLastNo + 1]))
                    {
                        vItem = Items[vInsetLastNo + 1];
                        UndoAction_InsertText(vInsetLastNo, Items[vInsetLastNo].Length - vItem.Length + 1, vItem.Text);
                        UndoAction_DeleteItem(vInsPos + vItemCount, 0);

                        Items.Delete(vInsPos + vItemCount);
                        vItemCount--;
                    }
                }

                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + vItemCount, vItemCount);

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

        public override void ParseXml(XmlElement aNode)
        {
            if (!CanEdit())
                return;

            base.ParseXml(aNode);

            ReFormat();

            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();
            Style.UpdateInfoReScroll();
        }

        /// <summary> 在光标处插入Item </summary>
        /// <param name="aItem"></param>
        /// <returns></returns>
        public virtual bool InsertItem(HCCustomItem aItem)
        {
            if (!CanEdit())
                return false;

            if (!DeleteSelected())
                return false;

            bool Result = false;

            if (aItem.ParaNo > this.Style.ParaStyles.Count - 1)
                aItem.ParaNo = CurParaNo;

            if (!aItem.ParaFirst)  // 不是段首
            {
                if (IsEmptyData())  // 空数据
                {
                    Undo_New();
                    Result = EmptyDataInsertItem(aItem);
                    CurParaNo = aItem.ParaNo;
                    return Result;
                }
                else  // 随其后
                    aItem.ParaNo = CurParaNo;
            }

            int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
            int vCurItemNo = GetActiveItemNo();
            if (Items[vCurItemNo].StyleNo < HCStyle.Null)
            {
                if (SelectInfo.StartItemOffset == HC.OffsetInner)  // 正在其上
                {
                    GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                    Undo_New();
                    UndoAction_ItemSelf(vCurItemNo, HC.OffsetInner);
                    Result = (Items[vCurItemNo] as HCCustomRectItem).InsertItem(aItem);
                    if (Result)
                        ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0);
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
                {
                    if (SelectInfo.StartItemOffset == 0)  // 在最前面插入
                        Result = InsertItem(SelectInfo.StartItemNo, aItem);
                    else  // 在Item中间
                    {
                        GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                        FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                        string vText = Items[vCurItemNo].Text;
                        string vsBefor = vText.Substring(1 - 1, SelectInfo.StartItemOffset);  // 前半部分文本
                        string vsAfter = vText.Substring(SelectInfo.StartItemOffset + 1 - 1, Items[vCurItemNo].Length - SelectInfo.StartItemOffset);  // 后半部分文本

                        Undo_New();
                        if (Items[vCurItemNo].CanConcatItems(aItem))  // 能合并
                        {
                            if (aItem.ParaFirst)  // 新段
                            {
                                UndoAction_DeleteBackText(vCurItemNo, SelectInfo.StartItemOffset + 1, vsAfter);
                                Items[vCurItemNo].Text = vsBefor;
                                aItem.Text = aItem.Text + vsAfter;

                                vCurItemNo = vCurItemNo + 1;
                                Items.Insert(vCurItemNo, aItem);
                                UndoAction_InsertItem(vCurItemNo, 0);

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + 1, 1);
                                ReSetSelectAndCaret(vCurItemNo);
                            }
                            else  // 同一段中插入
                            {
                                UndoAction_InsertText(vCurItemNo, SelectInfo.StartItemOffset + 1, aItem.Text);
                                vsBefor = vsBefor + aItem.Text;
                                Items[vCurItemNo].Text = vsBefor + vsAfter;

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0);
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
                            Style.States.Include(HCState.hosInsertBreakItem);
                            try
                            {
                                Items.Insert(vCurItemNo, vAfterItem);
                            }
                            finally
                            {
                                Style.States.Exclude(HCState.hosInsertBreakItem);
                            }

                            UndoAction_InsertItem(vCurItemNo, 0);

                            // 插入新Item
                            Items.Insert(vCurItemNo, aItem);
                            UndoAction_InsertItem(vCurItemNo, 0);

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + 2, 2);
                            ReSetSelectAndCaret(vCurItemNo);
                        }

                        Result = true;
                    }
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

            if (aItem.ParaNo > this.Style.ParaStyles.Count - 1)
                aItem.ParaNo = CurParaNo;

            if (!aItem.ParaFirst)  // 不是段首
            {
                if (IsEmptyData())  // 空数据
                {
                    Undo_New();
                    bool vRe = EmptyDataInsertItem(aItem);
                    CurParaNo = aItem.ParaNo;
                    return vRe;
                }
                else  // 随其后
                    aItem.ParaNo = CurParaNo;
            }

            int vIncCount = 0, vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
            Undo_New();
            if (aItem.StyleNo < HCStyle.Null)  // 插入RectItem
            {
                int vInsPos = aIndex;
                if (aIndex < Items.Count)
                {
                    if (aOffsetBefor)  // 在原位置Item前面插入
                    {
                        GetFormatRange(aIndex, 1, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                        FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                        if (IsEmptyLine(aIndex))
                        {
                            aItem.ParaFirst = true;
                            UndoAction_DeleteItem(aIndex, 0);
                            Items.Delete(aIndex);
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

                                if (Items[aIndex].PageBreak)
                                {
                                    UndoAction_ItemPageBreak(aIndex, 0, false);
                                    Items[aIndex].PageBreak = false;
                                    aItem.PageBreak = true;
                                }
                            }
                        }
                    }
                    else  // 在某Item后面插入
                    {
                        if ((aIndex > 0)
                            && (Items[aIndex - 1].StyleNo > HCStyle.Null)
                            && (Items[aIndex - 1].Text == ""))
                        {
                            GetFormatRange(aIndex - 1, 1, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            aItem.ParaFirst = true;
                            UndoAction_DeleteItem(aIndex - 1, 0);
                            Items.RemoveAt(aIndex - 1);
                            vIncCount--;
                            vInsPos--;
                        }
                        else  // 插入位置前一个不是空行
                        {
                            GetFormatRange(aIndex - 1, GetItemLastDrawItemNo(aIndex - 1), ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                        }
                    }
                }
                else  // 在末尾添加一个新Item
                {
                    GetFormatRange(aIndex - 1, GetItemLastDrawItemNo(aIndex - 1), ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
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

                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + vIncCount, vIncCount);
                ReSetSelectAndCaret(vInsPos);
            }
            else  // 插入文本Item
            {
                bool vMerged = false;
                if (!aItem.ParaFirst)
                {
                    // 在2个Item中间插入一个Item，需要同时判断和前后能否合并
                    if (aOffsetBefor)
                    {
                        if ((aIndex < Items.Count) && (Items[aIndex].CanConcatItems(aItem)))
                        {
                            GetFormatRange(aIndex, 1, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            UndoAction_InsertText(aIndex, 1, aItem.Text);  // 201806261644
                            Items[aIndex].Text = aItem.Text + Items[aIndex].Text;

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0);
                            ReSetSelectAndCaret(aIndex);

                            vMerged = true;
                        }
                        else
                        if ((!Items[aIndex].ParaFirst) && (aIndex > 0) && Items[aIndex - 1].CanConcatItems(aItem))
                        {
                            GetFormatRange(aIndex - 1, GetItemOffsetAfter(aIndex - 1), ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            UndoAction_InsertText(aIndex - 1, Items[aIndex - 1].Length + 1, aItem.Text);  // 201806261650
                            Items[aIndex - 1].Text = Items[aIndex - 1].Text + aItem.Text;

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0);
                            ReSetSelectAndCaret(aIndex - 1);

                            vMerged = true;
                        }
                    }
                    else  // 在Item后面插入，未指定另起一段，在Item后面插入AIndex肯定是大于0
                    {
                        if (IsEmptyLine(aIndex - 1))  // 在空行后插入不换行
                        {
                            GetFormatRange(aIndex - 1, 1, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            aItem.ParaFirst = true;
                            Items.Insert(aIndex, aItem);
                            UndoAction_InsertItem(aIndex, 0);

                            UndoAction_DeleteItem(aIndex - 1, 0);
                            Items.Delete(aIndex - 1);

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0);
                            ReSetSelectAndCaret(aIndex - 1);

                            vMerged = true;
                        }
                        else
                        if (Items[aIndex - 1].CanConcatItems(aItem))
                        {
                            // 能合并，重新获取前一个的格式化信息
                            GetFormatRange(aIndex - 1, GetItemOffsetAfter(aIndex - 1), ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            UndoAction_InsertText(aIndex - 1, Items[aIndex - 1].Length + 1, aItem.Text);  // 201806261650
                            Items[aIndex - 1].Text = Items[aIndex - 1].Text + aItem.Text;

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0);
                            ReSetSelectAndCaret(aIndex - 1);
                            vMerged = true;
                        }
                        else
                        if ((aIndex < Items.Count) && (!Items[aIndex].ParaFirst) && (Items[aIndex].CanConcatItems(aItem)))
                        {
                            GetFormatRange(aIndex, 1, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            UndoAction_InsertText(aIndex, 1, aItem.Text);  // 201806261644
                            Items[aIndex].Text = aItem.Text + Items[aIndex].Text;

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0);
                            ReSetSelectAndCaret(aIndex, aItem.Length);

                            vMerged = true;
                        }
                    }
                }

                if (!vMerged)
                {
                    if (aOffsetBefor)
                    {
                        GetFormatRange(aIndex, 1, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
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
                    else
                        GetFormatRange(aIndex - 1, GetItemOffsetAfter(aIndex - 1), ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);

                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                    Items.Insert(aIndex, aItem);
                    UndoAction_InsertItem(aIndex, 0);
                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + 1, 1);

                    ReSetSelectAndCaret(aIndex);
                }
            }

            return true;
        }

        public void SetActiveItemText(string aText)
        {
            if (!CanEdit())
                return;

            if (aText == "")
                return;

            this.InitializeField();
            HCCustomItem vActiveItem = GetActiveItem();
            if (vActiveItem == null)
                return;

            int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
            if ((vActiveItem.StyleNo < HCStyle.Null)  // 当前位置是 RectItem)
                && (SelectInfo.StartItemOffset == HC.OffsetInner))  // 在其上输入内容
            {
                Undo_New();

                HCCustomRectItem vRectItem = vActiveItem as HCCustomRectItem;
                if (vRectItem.MangerUndo)
                    UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                else
                    UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                vRectItem.SetActiveItemText(aText);
                if (vRectItem.IsFormatDirty)
                {
                    GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);

                    vRectItem.IsFormatDirty = false;
                }
                else
                    this.FormatInit();
            }
            else
            {
                if (aText.IndexOf("\r\n") >= 0)
                {
                    Undo_GroupBegin(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                    try
                    {
                        DeleteItems(SelectInfo.StartItemNo, SelectInfo.StartItemNo, true);
                        InsertText(aText);
                    }
                    finally
                    {
                        Undo_GroupEnd(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                    }

                    return;
                }

                if (!DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actSetItemText))
                    return;

                Undo_New();
                UndoAction_SetItemText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, aText);
                Items[SelectInfo.StartItemNo].Text = aText;

                GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);

                SelectInfo.StartItemOffset = aText.Length;
                SelectInfo.EndItemNo = -1;
                SelectInfo.EndItemOffset = -1;
                Items[SelectInfo.StartItemNo].DisSelect();

                ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            }

            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();
            //Style.UpdateInfoReScroll;
        }

        public virtual void KillFocus()
        {
            int vItemNo = GetActiveItemNo();
            if (vItemNo > 0)
                Items[vItemNo].KillFocus();
        }

        #region MouseDown子方法DoItemMouseDown
        private void DoItemMouseDown(int aItemNo, int aOffset, MouseEventArgs e)
        {
            if (aItemNo < 0)
                return;

            int vX = -1, vY = -1;
            CoordToItemOffset(e.X, e.Y, aItemNo, aOffset, ref vX, ref vY);

            MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
            Items[aItemNo].MouseDown(vMouseArgs);

            if (FOnItemMouseDown != null)
                FOnItemMouseDown(this, aItemNo, aOffset, vMouseArgs);
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
            FSelectSeekNo = vMouseDownItemNo;
            FSelectSeekOffset = vMouseDownItemOffset;

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
                    Style.UpdateInfo.DragingSelected = true;
                }

                if (Items[vMouseDownItemNo].StyleNo < HCStyle.Null)
                    DoItemMouseDown(vMouseDownItemNo, vMouseDownItemOffset, e);
            }
            else  // 没点在选中区域中
            {
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
                    if (!SelectInfo.StartRestrain && vRestrain)
                        Items[FMouseDownItemNo].Active = false;

                    SelectInfo.StartItemNo = FMouseDownItemNo;
                    SelectInfo.StartItemOffset = FMouseDownItemOffset;
                    SelectInfo.StartRestrain = vRestrain;
                    CaretDrawItemNo = vDrawItemNo;
                }

                //if not vRestrain then  // 没收敛，因跨页Item点击在前后位置时需要处理光标数据所以不能限制
                DoItemMouseDown(FMouseDownItemNo, FMouseDownItemOffset, e);
            }
        }

        #region MouseMove子方法
        private void DoItemMouseMove(int aItemNo, int aOffset, MouseEventArgs e, bool aDrawItemMouseMove = false)
        {
            if (aItemNo < 0)
                return;

            int vX = -1, vY = -1;
            CoordToItemOffset(e.X, e.Y, aItemNo, aOffset, ref vX, ref vY);
            MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
            if (aDrawItemMouseMove)
                DoDrawItemMouseMove(this, aItemNo, aOffset, FMouseMoveDrawItemNo, vMouseArgs);
            
            Items[aItemNo].MouseMove(vMouseArgs);
        }
        #endregion

        public virtual void MouseMove(MouseEventArgs e)
        {
            if (SelectedResizing())
            {
                FMouseMoveItemNo = FMouseDownItemNo;
                FMouseMoveItemOffset = FMouseDownItemOffset;
                FMouseMoveDrawItemNo = GetDrawItemNoByOffset(FMouseMoveItemNo, FMouseMoveItemOffset);
                FMouseMoveRestrain = false;
                DoItemMouseMove(FMouseMoveItemNo, FMouseMoveItemOffset, e);
                Style.UpdateInfoRePaint();

                return;
            }

            int vMouseMoveItemNo = -1, vMouseMoveItemOffset = -1;
            bool vRestrain = false;
            GetItemAt(e.X, e.Y, ref vMouseMoveItemNo, ref vMouseMoveItemOffset, ref FMouseMoveDrawItemNo, ref vRestrain);

            if (FDraging || Style.UpdateInfo.DragingSelected)  // 拖拽
            {
                HC.GCursor = Cursors.Arrow;  // crDrag

                FMouseMoveItemNo = vMouseMoveItemNo;
                FMouseMoveItemOffset = vMouseMoveItemOffset;
                SelectInfo.StartItemNo = vMouseMoveItemNo;
                SelectInfo.StartItemOffset = vMouseMoveItemOffset;
                FMouseMoveRestrain = vRestrain;
                CaretDrawItemNo = FMouseMoveDrawItemNo;

                Style.UpdateInfoReCaret();

                if ((!vRestrain) && (Items[FMouseMoveItemNo].StyleNo < HCStyle.Null))
                    DoItemMouseMove(FMouseMoveItemNo, FMouseMoveItemOffset, e);
            }
            else
            {
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

                        DoItemMouseMove(FMouseMoveItemNo, FMouseMoveItemOffset, e);
                        Style.UpdateInfoRePaint();
                        Style.UpdateInfoReCaret();
                        return;
                    }
                    else
                    {
                        FMouseMoveItemNo = vMouseMoveItemNo;
                        FMouseMoveItemOffset = vMouseMoveItemOffset;
                        FMouseMoveRestrain = vRestrain;
                    }

                    AdjustSelectRange(ref FMouseDownItemNo, ref FMouseDownItemOffset,
                      ref FMouseMoveItemNo, ref FMouseMoveItemOffset);  // 确定SelectRang

                    FSelectSeekNo = FMouseMoveItemNo;
                    FSelectSeekOffset = FMouseMoveItemOffset;
                    FMouseMoveDrawItemNo = GetDrawItemNoByOffset(FMouseMoveItemNo, FMouseMoveItemOffset);

                    if (SelectExists())
                        MatchItemSelectState();  // 设置选中范围内的Item选中状态
                    else
                        CaretDrawItemNo = FMouseMoveDrawItemNo;  // 按下上一下DrawItem最后，划选到下一个开始时，没有选中内容，要更换CaretDrawIemNo
                    
                    if ((Items[FMouseMoveItemNo].StyleNo < HCStyle.Null) && (FMouseDownItemOffset == HC.OffsetInner))
                        DoItemMouseMove(FMouseMoveItemNo, FMouseMoveItemOffset, e);

                    Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();
                }
                else  // 非拖拽，非划选
                {
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
                                {
                                    if (FMouseMoveRestrain && (!vRestrain))
                                    {
                                        if (vMouseMoveItemNo >= 0)
                                            DoItemMouseEnter(vMouseMoveItemNo);
                                    }
                                }

                                Style.UpdateInfoRePaint();
                            }
                        }

                        FMouseMoveItemNo = vMouseMoveItemNo;
                        FMouseMoveItemOffset = vMouseMoveItemOffset;
                        FMouseMoveRestrain = vRestrain;

                        if (!vRestrain)
                        {
                            DoItemMouseMove(FMouseMoveItemNo, FMouseMoveItemOffset, e, true);
                            if ((Control.ModifierKeys & Keys.Control) == Keys.Control
                                && (Items[FMouseMoveItemNo].HyperLink != ""))
                                HC.GCursor = Cursors.Hand;
                        }
                    }
                }
            }
        }

        #region MouseUp子方法DoItemMouseUp
        private void DoItemMouseUp(int aItemNo, int aOffset, bool vRestrain, MouseEventArgs e)
        {
            if (aItemNo < 0)
                return;

            int vX = -1, vY = -1;
            CoordToItemOffset(e.X, e.Y, aItemNo, aOffset, ref vX, ref vY);

            MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, vX, vY, e.Delta);
            Items[aItemNo].MouseUp(vMouseArgs);

            if ((!vRestrain) && (FOnItemMouseUp != null))
                FOnItemMouseUp(this, aItemNo, aOffset, vMouseArgs);
        }
        #endregion

        #region MouseUp子方法DoNormalMouseUp
        private void DoNormalMouseUp(int vUpItemNo, int vUpItemOffset, int vDrawItemNo, bool vRestrain, MouseEventArgs e)
        {
            if (FMouseMoveItemNo < 0)
            {
                SelectInfo.StartItemNo = vUpItemNo;
                SelectInfo.StartItemOffset = vUpItemOffset;
                SelectInfo.StartRestrain = vRestrain;
                CaretDrawItemNo = vDrawItemNo;
            }
            else
            {
                SelectInfo.StartItemNo = FMouseMoveItemNo;
                SelectInfo.StartItemOffset = FMouseMoveItemOffset;
                SelectInfo.StartRestrain = vRestrain;
                CaretDrawItemNo = FMouseMoveDrawItemNo;
            }

            Style.UpdateInfoRePaint();

            if (!FMouseDownReCaret)
                Style.UpdateInfoReCaret();

            //if (Items[vUpItemNo].StyleNo < HCStyle.Null)
            DoItemMouseUp(vUpItemNo, vUpItemOffset, vRestrain, e);  // 弹起，因为可能是移出Item后弹起，所以这里不受vRestrain约束
        }
        #endregion

        public virtual void MouseUp(MouseEventArgs e)
        {
            FMouseLBDowning = false;

            if (FMouseLBDouble)
                return;

            if ((e.Button == MouseButtons.Left) && ((Control.ModifierKeys & Keys.Shift) == Keys.Shift))
                return;

            bool vRestrain = false;
            if (SelectedResizing())
            {
                Undo_New();
                UndoAction_ItemSelf(FMouseDownItemNo, FMouseDownItemOffset);

                DoItemMouseUp(FMouseDownItemNo, FMouseDownItemOffset, vRestrain, e);
                DoItemResized(FMouseDownItemNo);  // 缩放完成事件(可控制缩放不要超过页面)

                int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
                GetFormatRange(FMouseDownItemNo, FMouseDownItemOffset, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                Style.UpdateInfoRePaint();

                return;
            }

            int vUpItemNo = -1, vUpItemOffset = -1, vDrawItemNo = -1;
            GetItemAt(e.X, e.Y, ref vUpItemNo, ref vUpItemOffset, ref vDrawItemNo, ref vRestrain);

            if (FSelecting || Style.UpdateInfo.Selecting)
            {
                FSelecting = false;
                // 选中范围内的RectItem取消划选状态(此时表格的FSelecting为True)
                if (SelectInfo.StartItemNo >= 0)
                {
                    for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)
                    {
                        if ((i != vUpItemNo) && (Items[i].StyleNo < HCStyle.Null))
                            DoItemMouseUp(i, 0, vRestrain, e);
                    }
                }

                if (SelectInfo.EndItemNo < 0)  // 在RectItem里划选或TextItem划选回起始位置
                {
                    if ((FMouseDownItemNo >= 0) && (Items[FMouseDownItemNo].StyleNo < HCStyle.Null))  // 弹起时在RectItem
                        DoItemMouseUp(FMouseDownItemNo, HC.OffsetInner, vRestrain, e);
                    else
                        DoItemMouseUp(vUpItemNo, vUpItemOffset, vRestrain, e);
                }
                else
                if (Items[vUpItemNo].StyleNo < HCStyle.Null)
                    DoItemMouseUp(vUpItemNo, vUpItemOffset, vRestrain, e);
            }
            else
            {
                if (FDraging || Style.UpdateInfo.DragingSelected)
                {
                    FDraging = false;
                    //bool vMouseUpInSelect = CoordInSelect(e.X, e.Y, vUpItemNo, vUpItemOffset, vRestrain);

                    // 清除弹起位置之外的Item选中状态，弹起处自己处理，弹起处不在选中范围内时
                    // 保证弹起处取消(从ItemA上选中拖拽到另一个ItemB时，ItemA选中状态需要取消)
                    // 与201805172309相似
                    if (SelectInfo.StartItemNo >= 0)
                    {
                        for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)  // 遍历弹起位置之外的其他Item
                            Items[i].DisSelect();
                    }

                    // 为拖拽光标准备
                    FMouseMoveItemNo = vUpItemNo;
                    FMouseMoveItemOffset = vUpItemOffset;
                    FMouseMoveDrawItemNo = vDrawItemNo;
                    // 为下一次点击时清除上一次点击选中做准备
                    FMouseDownItemNo = vUpItemNo;
                    FMouseDownItemOffset = vUpItemOffset;

                    DoNormalMouseUp(vUpItemNo, vUpItemOffset, vDrawItemNo, vRestrain, e);  // 弹起处自己处理Item选中状态，并以弹起处为当前编辑位置

                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
                }
                else  // 非拖拽、非划选
                {
                    if (SelectExists(false))
                        DisSelect();

                    DoNormalMouseUp(vUpItemNo, vUpItemOffset, vDrawItemNo, vRestrain, e);
                }
            }
        }

        public virtual void MouseLeave()
        {
            if (FMouseMoveItemNo >= 0)
            {
                DoItemMouseLeave(FMouseMoveItemNo);
                FMouseMoveItemNo = -1;
                FMouseMoveItemOffset = -1;
                FMouseMoveDrawItemNo = -1;
                Style.UpdateInfoRePaint();
            }
        }

        // Key返回0表示此键按下Data没有做任何事情
        public virtual void KeyPress(ref Char key)
        {
            if (!CanEdit())
                return;

            //DeleteSelected();

            HCCustomItem vCarteItem = GetActiveItem();
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
                if (vRectItem.IsFormatDirty)
                {
                    int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
                    GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                    if (key != 0)
                        ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);

                    vRectItem.IsFormatDirty = false;
                    Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();
                    Style.UpdateInfoReScroll();
                }
                else
                    this.FormatInit();
            }
            else
                InsertText(key.ToString());
        }

        #region KeyDown子方法CheckSelectEndEff 判断选择结束是否和起始在同一位置，是则取消选中
        private void CheckSelectEndEff()
        {
            int vStartNo = SelectInfo.StartItemNo,
                vStartOffs = SelectInfo.StartItemOffset,
                vEndNo = SelectInfo.EndItemNo,
                vEndOffs = SelectInfo.EndItemOffset;

            AdjustSelectRange(ref vStartNo, ref vStartOffs, ref vEndNo, ref vEndOffs);
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
            if ((SelectInfo.StartItemOffset == 0) && (Items[SelectInfo.StartItemNo].ParaFirst))
            {
                HCParaStyle vParaStyle = Style.ParaStyles[vCurItem.ParaNo];
                ApplyParaFirstIndent(vParaStyle.FirstIndent + HCUnitConversion.PixXToMillimeter(HC.TabCharWidth));
            }
            else
            {
                if (vCurItem.StyleNo < HCStyle.Null)
                {
                    if (SelectInfo.StartItemOffset == HC.OffsetInner)
                    {
                        if ((vCurItem as HCCustomRectItem).WantKeyDown(e))  // 处理此键
                        {
                            int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
                            GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                            (vCurItem as HCCustomRectItem).KeyDown(e);
                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                        }
                    }
                    else
                    {
                        HCTabItem vTabItem = new HCTabItem(this);
                        this.InsertItem(vTabItem);
                    }
                }
                else  // TextItem
                {
                    HCTabItem vTabItem = new HCTabItem(this);
                    this.InsertItem(vTabItem);
                }
            }
        }

        // LeftKeyDown子方法
        private void SelectPrio(ref int aItemNo, ref int aOffset)
        {
            if (aOffset > 0)
            {
                if (Items[aItemNo].StyleNo < HCStyle.Null)
                {
                    if (Items[aItemNo] is HCDomainItem)
                        aOffset = (Items[aItemNo] as HCDomainItem).GetOffsetAt(0);
                    else
                        aOffset = HC.OffsetBefor;
                }
                else
                    aOffset = aOffset - 1;
            }
            else
            if (aItemNo > 0)
            {
                //Items[aItemNo].DisSelect();
                aItemNo = aItemNo - 1;
                if (Items[aItemNo].StyleNo < HCStyle.Null)
                {
                    if (Items[aItemNo] is HCDomainItem)
                        aOffset = (Items[aItemNo] as HCDomainItem).GetOffsetAt(0);
                    else
                        aOffset = HC.OffsetBefor;
                }
                else
                {
                    if (Items[aItemNo].Length > 0)
                        aOffset = Items[aItemNo].Length - 1;  // 倒数第1个前面
                    else
                        aOffset = 0;
                }
            }

            #if UNPLACEHOLDERCHAR
            if ((Items[aItemNo].StyleNo > HCStyle.Null)
                && (Items[aItemNo].Length > 0)
                && HC.IsUnPlaceHolderChar(Items[aItemNo].Text[aOffset + 1 - 1]))
                aOffset = GetItemActualOffset(aItemNo, aOffset) - 1;
            #endif
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
            if (e.Shift)  // Shift+方向键选择
            {
                if (SelectInfo.EndItemNo >= 0)
                {
                    if (IsSelectSeekStart())  // 游标在选中起始
                    {
                        SelectStartItemPrio();
                        CheckSelectEndEff();
                        SetSelectSeekStart();
                    }
                    else  // 游标在选中结束
                    {
                        SelectEndItemPrio();
                        CheckSelectEndEff();
                        SetSelectSeekEnd();
                    }
                }
                else  // 没有选中
                {
                    if ((SelectInfo.StartItemNo > 0) && (SelectInfo.StartItemOffset == 0))
                    {
                        SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                        SelectInfo.StartItemOffset = GetItemOffsetAfter(SelectInfo.StartItemNo);
                    }

                    SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                    SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;

                    SelectStartItemPrio();
                    CheckSelectEndEff();
                    SetSelectSeekStart();
                }

                MatchItemSelectState();
                Style.UpdateInfoRePaint();

                if (!SelectExists(false))
                    CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            }
            else  // 没有按下Shift
            {
                if (vSelectExist)
                {
                    for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)
                        Items[i].DisSelect();

                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
                }
                else  // 无选中内容
                {
                    if (SelectInfo.StartItemOffset != 0)  // 不在Item最开始
                    {
                        SelectInfo.StartItemOffset = SelectInfo.StartItemOffset - 1;
                        #if UNPLACEHOLDERCHAR
                        Char vChar = Items[SelectInfo.StartItemNo].Text[SelectInfo.StartItemOffset + 1 - 1];
                        if (HC.TibetanVowel.IndexOf(vChar) >= 0)
                            ;
                        else
                        if (HC.IsUnPlaceHolderChar(vChar))
                            SelectInfo.StartItemOffset = GetItemActualOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset) - 1;                   
                        #endif
                    }
                    else  // 在Item最开始左方向键
                    {
                        if (SelectInfo.StartItemNo > 0)
                        {
                            SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;  // 上一个
                            SelectInfo.StartItemOffset = GetItemOffsetAfter(SelectInfo.StartItemNo);

                            if (!DrawItems[Items[SelectInfo.StartItemNo + 1].FirstDItemNo].LineFirst)
                            {
                                Style.UpdateInfoRePaint();
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
                        if ((vNewCaretDrawItemNo == CaretDrawItemNo - 1)  // 移动到前一个了
                            && (DrawItems[vNewCaretDrawItemNo].ItemNo == DrawItems[CaretDrawItemNo].ItemNo)  // 是同一个Item
                            && (DrawItems[CaretDrawItemNo].LineFirst)  // 原是行首
                            && (SelectInfo.StartItemOffset == DrawItems[CaretDrawItemNo].CharOffs - 1)) // 光标位置也是原DrawItem的最前面
                        {
                            // 不更换
                        }
                        else
                            CaretDrawItemNo = vNewCaretDrawItemNo;
                    }
                    else
                    if (SelectInfo.StartRestrain)
                    {
                        SelectInfo.StartRestrain = false;
                        Items[DrawItems[vNewCaretDrawItemNo].ItemNo].Active = true;
                    }

                    Style.UpdateInfoRePaint();
                }
            }
        }

        // RightKeyDown子方法
        private void SelectNext(ref int AItemNo, ref int AOffset)
        {
            if (AOffset == GetItemOffsetAfter(AItemNo))
            {
                if (AItemNo < Items.Count - 1)
                {
                    AItemNo++;

                    if (Items[AItemNo].StyleNo < HCStyle.Null)
                    {
                        if (Items[AItemNo] is HCDomainItem)
                            AOffset = (Items[AItemNo] as HCDomainItem).GetOffsetAt(0);
                        else
                            AOffset = HC.OffsetAfter;
                    }
                    else
                    if (Items[AItemNo].Length > 0)
                        AOffset = 1;
                    else
                        AOffset = 0;
                }
            }
            else  // 不在最后
            {
                if (Items[AItemNo].StyleNo < HCStyle.Null)
                {
                    if (Items[AItemNo] is HCDomainItem)
                        AOffset = (Items[AItemNo] as HCDomainItem).GetOffsetAt(0);
                    else
                        AOffset = HC.OffsetAfter;
                }
                else
                    AOffset = AOffset + 1;
            }

            #if UNPLACEHOLDERCHAR
            AOffset = GetItemActualOffset(AItemNo, AOffset, true);
            #endif
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
                if (SelectInfo.EndItemNo >= 0)  // 有选中内容
                {
                    if (IsSelectSeekStart())
                    {
                        SelectStartItemNext();
                        CheckSelectEndEff();
                        SetSelectSeekStart();
                    }
                    else  // 游标在选中结束
                    {
                        SelectEndItemNext();
                        CheckSelectEndEff();
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
                        {
                            if (SelectInfo.StartItemOffset == Items[SelectInfo.StartItemNo].Length)
                            {
                                SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                                SelectInfo.StartItemOffset = 0;
                            }
                        }
                    }

                    SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                    SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;

                    SelectEndItemNext();
                    CheckSelectEndEff();
                    SetSelectSeekEnd();
                }

                MatchItemSelectState();
                Style.UpdateInfoRePaint();

                if (!SelectExists(false))
                    CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            }
            else  // 没有按下Shift
            {
                if (vSelectExist)
                {
                    for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)
                        Items[i].DisSelect();

                    SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                    SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
                }
                else  // 无选中内容
                {
                    if (SelectInfo.StartItemOffset < vCurItem.Length)
                        #if UNPLACEHOLDERCHAR
                        SelectInfo.StartItemOffset = GetItemActualOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1, true);
                        #else
                        SelectInfo.StartItemOffset = SelectInfo.StartItemOffset + 1;
                        #endif
                    else  // 在Item最右边
                    {
                        if (SelectInfo.StartItemNo < Items.Count - 1)
                        {
                            SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;  // 选中下一个Item
                            SelectInfo.StartItemOffset = 0;  // 下一个最前面
                            if (!DrawItems[Items[SelectInfo.StartItemNo].FirstDItemNo].LineFirst)
                            {
                                Style.UpdateInfoRePaint();
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

                    Style.UpdateInfoRePaint();
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
                    if (IsSelectSeekStart())  // 游标在选中起始
                    {
                        SelectInfo.StartItemNo = DrawItems[vFirstDItemNo].ItemNo;
                        if (Items[SelectInfo.StartItemNo] is HCDomainItem)
                            SelectInfo.StartItemOffset = (Items[SelectInfo.StartItemNo] as HCDomainItem).GetOffsetAt(0);
                        else
                            SelectInfo.StartItemOffset = DrawItems[vFirstDItemNo].CharOffs - 1;

                        CheckSelectEndEff();
                        SetSelectSeekStart();
                    }
                    else  // 游标在选中结束
                    {
                        if (DrawItems[vFirstDItemNo].ItemNo > SelectInfo.StartItemNo)
                        {
                            SelectInfo.EndItemNo = DrawItems[vFirstDItemNo].ItemNo;
                            if (Items[SelectInfo.EndItemNo] is HCDomainItem)
                                SelectInfo.EndItemOffset = (Items[SelectInfo.EndItemNo] as HCDomainItem).GetOffsetAt(0);
                            else
                                SelectInfo.EndItemOffset = DrawItems[vFirstDItemNo].CharOffs - 1;

                            CheckSelectEndEff();
                            SetSelectSeekEnd();
                        }
                        else
                        {
                            if (DrawItems[vFirstDItemNo].ItemNo == SelectInfo.StartItemNo)
                            {
                                if (DrawItems[vFirstDItemNo].CharOffs - 1 > SelectInfo.StartItemOffset)
                                {
                                    SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                                    if (Items[SelectInfo.EndItemNo] is HCDomainItem)
                                        SelectInfo.EndItemOffset = (Items[SelectInfo.EndItemNo] as HCDomainItem).GetOffsetAt(0);
                                    else
                                        SelectInfo.EndItemOffset = DrawItems[vFirstDItemNo].CharOffs - 1;

                                    CheckSelectEndEff();
                                    SetSelectSeekEnd();
                                }
                            }
                            else
                            {
                                SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                                SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;
                                SelectInfo.StartItemNo = DrawItems[vFirstDItemNo].ItemNo;
                                if (Items[SelectInfo.StartItemNo] is HCDomainItem)
                                    SelectInfo.StartItemOffset = (Items[SelectInfo.StartItemNo] as HCDomainItem).GetOffsetAt(0);
                                else
                                    SelectInfo.StartItemOffset = DrawItems[vFirstDItemNo].CharOffs - 1;

                                CheckSelectEndEff();
                                SetSelectSeekStart();
                            }
                        }
                    }
                }
                else   // 没有选中
                {
                    SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                    SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;
                    SelectInfo.StartItemNo = DrawItems[vFirstDItemNo].ItemNo;
                    if (Items[SelectInfo.StartItemNo] is HCDomainItem)
                        SelectInfo.StartItemOffset = (Items[SelectInfo.StartItemNo] as HCDomainItem).GetOffsetAt(0);
                    else
                        SelectInfo.StartItemOffset = DrawItems[vFirstDItemNo].CharOffs - 1;

                    CheckSelectEndEff();
                    SetSelectSeekStart();
                }

                MatchItemSelectState();
                Style.UpdateInfoRePaint();

                if (!SelectExists(false))
                    CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            }
            else
            {
                if (vSelectExist)
                {
                    for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)
                        Items[i].DisSelect();

                    SelectInfo.EndItemNo = -1;
                    SelectInfo.EndItemOffset = -1;
                }
                else  // 无选中内容
                {
                    int vFirstDItemNo = GetSelectStartDrawItemNo();
                    int vLastDItemNo = -1;
                    GetLineDrawItemRang(ref vFirstDItemNo, ref vLastDItemNo);
                    SelectInfo.StartItemNo = DrawItems[vFirstDItemNo].ItemNo;
                    if (Items[SelectInfo.StartItemNo] is HCDomainItem)
                        SelectInfo.StartItemOffset = (Items[SelectInfo.StartItemNo] as HCDomainItem).GetOffsetAt(0);
                    else
                        SelectInfo.StartItemOffset = DrawItems[vFirstDItemNo].CharOffs - 1;
                }

                if (!e.Handled)
                    CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);

                Style.UpdateInfoRePaint();
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
                    if (IsSelectSeekStart())  // 游标在选中起始
                    {
                        if (DrawItems[vLastDItemNo].ItemNo > SelectInfo.EndItemNo)
                        {
                            SelectInfo.StartItemNo = DrawItems[vLastDItemNo].ItemNo;
                            if (Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null)
                            {
                                if (Items[SelectInfo.StartItemNo] is HCDomainItem)
                                    SelectInfo.StartItemOffset = (Items[SelectInfo.StartItemNo] as HCDomainItem).GetOffsetAt(0);
                                else
                                    SelectInfo.StartItemOffset = HC.OffsetAfter;
                            }
                            else
                                SelectInfo.StartItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();

                            CheckSelectEndEff();
                            SetSelectSeekStart();
                        }
                        else
                        if (DrawItems[vLastDItemNo].ItemNo == SelectInfo.EndItemNo)
                        {
                            SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                            if (DrawItems[vLastDItemNo].CharOffsetEnd() < SelectInfo.EndItemOffset)
                            {
                                if (Items[SelectInfo.StartItemNo] is HCDomainItem)
                                    SelectInfo.StartItemOffset = (Items[SelectInfo.StartItemNo] as HCDomainItem).GetOffsetAt(0);
                                else
                                    SelectInfo.StartItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();

                                CheckSelectEndEff();
                                SetSelectSeekStart();
                            }
                            else
                            {
                                SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                                if (Items[SelectInfo.EndItemNo] is HCDomainItem)
                                    SelectInfo.EndItemOffset = (Items[SelectInfo.EndItemNo] as HCDomainItem).GetOffsetAt(0);
                                else
                                    SelectInfo.EndItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();

                                CheckSelectEndEff();
                                SetSelectSeekEnd();
                            }
                        }
                        else
                        {
                            SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                            SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                            SelectInfo.EndItemNo = DrawItems[vLastDItemNo].ItemNo;
                            if (Items[SelectInfo.EndItemNo] is HCDomainItem)
                                SelectInfo.EndItemOffset = (Items[SelectInfo.EndItemNo] as HCDomainItem).GetOffsetAt(0);
                            else
                                SelectInfo.EndItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();

                            CheckSelectEndEff();
                            SetSelectSeekEnd();
                        }
                    }
                    else  // 游标在选中结束
                    {
                        SelectInfo.EndItemNo = DrawItems[vLastDItemNo].ItemNo;
                        if (Items[SelectInfo.EndItemNo] is HCDomainItem)
                            SelectInfo.EndItemOffset = (Items[SelectInfo.EndItemNo] as HCDomainItem).GetOffsetAt(0);
                        else
                            SelectInfo.EndItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();

                        CheckSelectEndEff();
                        SetSelectSeekEnd();
                    }
                }
                else   // 没有选中
                {
                    SelectInfo.EndItemNo = DrawItems[vLastDItemNo].ItemNo;
                    if (Items[SelectInfo.EndItemNo] is HCDomainItem)
                        SelectInfo.EndItemOffset = (Items[SelectInfo.EndItemNo] as HCDomainItem).GetOffsetAt(0);
                    else
                        SelectInfo.EndItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();

                    CheckSelectEndEff();

                    if (SelectInfo.EndItemNo < 0)
                        SetSelectSeekStart();
                    else
                        SetSelectSeekEnd();
                }

                MatchItemSelectState();
                Style.UpdateInfoRePaint();
                
                if (!SelectExists(false))
                    CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            }
            else
            {
                if (vSelectExist)
                {
                    for (int i = SelectInfo.StartItemNo; i <= SelectInfo.EndItemNo; i++)
                        Items[i].DisSelect();

                    SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                    if (Items[SelectInfo.StartItemNo] is HCDomainItem)
                        SelectInfo.StartItemOffset = (Items[SelectInfo.StartItemNo] as HCDomainItem).GetOffsetAt(0);
                    else
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
                    if (Items[SelectInfo.StartItemNo] is HCDomainItem)
                        SelectInfo.StartItemOffset = (Items[SelectInfo.StartItemNo] as HCDomainItem).GetOffsetAt(0);
                    else
                        SelectInfo.StartItemOffset = DrawItems[vLastDItemNo].CharOffsetEnd();
                }

                if (!e.Handled)
                    CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);

                Style.UpdateInfoRePaint();
            }
        }

        // UpKeyDown子方法
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
                        if (Items[DrawItems[i].ItemNo] is HCDomainItem)
                            ADrawItemOffset = (Items[DrawItems[i].ItemNo] as HCDomainItem).GetOffsetAt(0);
                        else
                            ADrawItemOffset = DrawItems[i].CharOffs + GetDrawItemOffsetAt(i, vX) - 1;

                        return Result;  // 有合适，则退出
                    }
                }

                // 没合适则选择到最后
                ADrawItemNo = vLastDItemNo;
                if (Items[DrawItems[vLastDItemNo].ItemNo] is HCDomainItem)
                    ADrawItemOffset = (Items[DrawItems[vLastDItemNo].ItemNo] as HCDomainItem).GetOffsetAt(0);
                else
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
                    if (IsSelectSeekStart())  // 游标在选中起始
                    {
                        vDrawItemNo = GetSelectStartDrawItemNo();
                        vDrawItemOffset = SelectInfo.StartItemOffset - DrawItems[vDrawItemNo].CharOffs + 1;
                        if (GetUpDrawItemNo(ref vDrawItemNo, ref vDrawItemOffset))
                        {
                            SelectInfo.StartItemNo = DrawItems[vDrawItemNo].ItemNo;
                            SelectInfo.StartItemOffset = vDrawItemOffset;
                            CheckSelectEndEff();
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
                                CheckSelectEndEff();
                                SetSelectSeekEnd();
                            }
                            else
                            if (DrawItems[vDrawItemNo].ItemNo == SelectInfo.StartItemNo)
                            {
                                SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                                if (vDrawItemOffset > SelectInfo.StartItemOffset)
                                {
                                    SelectInfo.EndItemOffset = vDrawItemOffset;
                                    CheckSelectEndEff();
                                    SetSelectSeekEnd();
                                }
                                else  // 移动到起始Offset前面
                                {
                                    SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;
                                    SelectInfo.StartItemOffset = vDrawItemOffset;
                                    CheckSelectEndEff();
                                    SetSelectSeekStart();
                                }
                            }
                            else  // 移动到起始Item前面了
                            {
                                SelectInfo.EndItemNo = SelectInfo.StartItemNo;
                                SelectInfo.EndItemOffset = SelectInfo.StartItemOffset;
                                SelectInfo.StartItemNo = DrawItems[vDrawItemNo].ItemNo;
                                SelectInfo.StartItemOffset = vDrawItemOffset;
                                CheckSelectEndEff();
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
                        CheckSelectEndEff();
                        SetSelectSeekStart();
                    }
                }

                MatchItemSelectState();
                Style.UpdateInfoRePaint();

                if (!SelectExists(false))
                    CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
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
                        Style.UpdateInfoRePaint();
                    }
                    else
                        e.Handled = true;
                }
            }
        }

        // DownKeyDown子方法
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
                        if (Items[DrawItems[i].ItemNo] is HCDomainItem)
                            ADrawItemOffset = (Items[DrawItems[i].ItemNo] as HCDomainItem).GetOffsetAt(0);
                        else
                            ADrawItemOffset = DrawItems[i].CharOffs + GetDrawItemOffsetAt(i, vX) - 1;

                        return Result;  // 有合适，则退出
                    }
                }

                // 没合适则选择到最后
                ADrawItemNo = vLastDItemNo;
                if (Items[DrawItems[vLastDItemNo].ItemNo] is HCDomainItem)
                    ADrawItemOffset = (Items[DrawItems[vLastDItemNo].ItemNo] as HCDomainItem).GetOffsetAt(0);
                else
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
                                CheckSelectEndEff();
                                SetSelectSeekStart();
                            }
                            else
                            if (DrawItems[vDrawItemNo].ItemNo == SelectInfo.EndItemNo)
                            {
                                SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                                if (vDrawItemOffset < SelectInfo.EndItemOffset)
                                {
                                    SelectInfo.StartItemOffset = vDrawItemOffset;
                                    CheckSelectEndEff();
                                    SetSelectSeekStart();
                                }
                                else  // 位置在结束Offset后面
                                {
                                    SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                                    SelectInfo.EndItemOffset = vDrawItemOffset;
                                    CheckSelectEndEff();
                                    SetSelectSeekEnd();
                                }
                            }
                            else  // 移动到结束Item后面，交换
                            {
                                SelectInfo.StartItemNo = SelectInfo.EndItemNo;
                                SelectInfo.StartItemOffset = SelectInfo.EndItemOffset;
                                SelectInfo.EndItemNo = DrawItems[vDrawItemNo].ItemNo;
                                SelectInfo.EndItemOffset = vDrawItemOffset;
                                CheckSelectEndEff();
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
                            CheckSelectEndEff();
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
                        CheckSelectEndEff();
                        SetSelectSeekEnd();
                    }
                }

                MatchItemSelectState();
                Style.UpdateInfoRePaint();

                if (!SelectExists(false))
                    CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
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
                        Style.UpdateInfoRePaint();
                    }
                    else  // 当前行是最后一行
                        e.Handled = true;
                }
            }
        }

        private void RectItemKeyDown(bool vSelectExist, ref HCCustomItem vCurItem, KeyEventArgs e, bool aPageBreak)
        {
            int Key = e.KeyValue;
            int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
            HCCustomRectItem vRectItem = vCurItem as HCCustomRectItem;

            if (SelectInfo.StartItemOffset == HC.OffsetInner)  // 在其上
            {
                if (vRectItem.WantKeyDown(e))
                {
                    Undo_New();
                    if (vRectItem.MangerUndo)
                        UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                    else
                        UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                    vRectItem.KeyDown(e);
                    if (vRectItem.IsFormatDirty)
                    {
                        GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                        FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                        ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                        vRectItem.IsFormatDirty = false;
                    }
                    else
                        this.FormatInit();
                }
                else  // 内部不响应此键
                {
                    switch (Key)
                    {
                        case User.VK_BACK:
                            SelectInfo.StartItemOffset = HC.OffsetAfter;
                            RectItemKeyDown(vSelectExist, ref vCurItem, e, aPageBreak);
                            break;

                        case User.VK_DELETE:
                            SelectInfo.StartItemOffset = HC.OffsetBefor;
                            RectItemKeyDown(vSelectExist, ref vCurItem, e, aPageBreak);
                            break;

                        case User.VK_LEFT:  // 移出去
                            SelectInfo.StartItemOffset = HC.OffsetBefor;
                            vRectItem.Active = false;
                            Style.UpdateInfoRePaint();
                            break;

                        case User.VK_RIGHT:  // 移出去
                            SelectInfo.StartItemOffset = HC.OffsetAfter;
                            vRectItem.Active = false;
                            Style.UpdateInfoRePaint();
                            break;
                    }
                }
            }
            else
            if (SelectInfo.StartItemOffset == HC.OffsetBefor)  // 在RectItem前
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
                            if (vRectItem is HCDomainItem)
                                SelectInfo.StartItemOffset = vRectItem.GetOffsetAt(vRectItem.Width + 1);
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
                        if (!DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actReturnItem))
                            return;
                        
                        GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                        FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                        if (vCurItem.ParaFirst)  // RectItem在段首，插入空行
                        {
                            vCurItem = CreateDefaultTextItem();
                            vCurItem.ParaFirst = true;
                            Items.Insert(SelectInfo.StartItemNo, vCurItem);

                            Undo_New();
                            UndoAction_InsertItem(SelectInfo.StartItemNo, 0);

                            if (aPageBreak)
                            {
                                UndoAction_ItemPageBreak(SelectInfo.StartItemNo + 1, 0, true);  // 我变成下一个了
                                Items[SelectInfo.StartItemNo + 1].PageBreak = true;
                            }

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + 1, 1);

                            SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                            ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                        }
                        else  // RectItem不在行首
                        {
                            Undo_New();

                            UndoAction_ItemParaFirst(SelectInfo.StartItemNo, 0, true);
                            vCurItem.ParaFirst = true;

                            if (aPageBreak)
                            {
                                UndoAction_ItemPageBreak(SelectInfo.StartItemNo, 0, true);
                                vCurItem.PageBreak = true;
                            }

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                        }

                        break;

                    case User.VK_BACK:  // 在RectItem前
                        if (vCurItem.ParaFirst && DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actBackDeleteText))
                        {
                            if (SelectInfo.StartItemNo > 0)  // 第一个前回删不处理，停止格式化
                            {
                                if (vCurItem.ParaFirst && (SelectInfo.StartItemNo > 0))
                                {
                                    vFormatFirstDrawItemNo = GetFormatFirstDrawItem(SelectInfo.StartItemNo - 1,
                                        GetItemOffsetAfter(SelectInfo.StartItemNo - 1));

                                    vFormatLastItemNo = GetParaLastItemNo(SelectInfo.StartItemNo);
                                }
                                else
                                    GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                                        
                                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                Undo_New();

                                if (IsEmptyLine(SelectInfo.StartItemNo - 1))  // 上一行是空行
                                {
                                    UndoAction_DeleteItem(SelectInfo.StartItemNo - 1, 0);
                                    Items.Delete(SelectInfo.StartItemNo - 1);

                                    SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;

                                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);
                                    ReSetSelectAndCaret(SelectInfo.StartItemNo, 0);
                                }
                                else
                                {
                                    UndoAction_ItemParaFirst(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, false);
                                    vCurItem.ParaFirst = false;

                                    if (vCurItem.PageBreak)
                                    {
                                        UndoAction_ItemPageBreak(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, false);
                                        vCurItem.PageBreak = false;
                                    }

                                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                                }
                            }
                        }
                        else  // 不是段首
                        {
                            // 选到上一个最后
                            SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                            SelectInfo.StartItemOffset = GetItemOffsetAfter(SelectInfo.StartItemNo);

                            KeyDown(e);  // 执行前一个的删除
                        }
                        break;

                    case User.VK_DELETE:  // 在RectItem前
                        if (!DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actDeleteItem))
                        {
                            SelectInfo.StartItemOffset = HC.OffsetAfter;
                            return;
                        }

                        GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                        FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                        if (vCurItem.ParaFirst)  // 是段首
                        {
                            if (SelectInfo.StartItemNo != vFormatLastItemNo)
                            {
                                Undo_New();
                                UndoAction_ItemParaFirst(SelectInfo.StartItemNo + 1, 0, true);
                                Items[SelectInfo.StartItemNo + 1].ParaFirst = true;

                                UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                Items.RemoveAt(SelectInfo.StartItemNo);

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);
                            }
                            else  // 段删除空了
                            {
                                Undo_New();
                                UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                Items.Delete(SelectInfo.StartItemNo);

                                vCurItem = CreateDefaultTextItem();
                                vCurItem.ParaFirst = true;
                                Items.Insert(SelectInfo.StartItemNo, vCurItem);
                                UndoAction_InsertItem(SelectInfo.StartItemNo, 0);

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                            }
                        }
                        else  // 不是段首
                        {
                            if (SelectInfo.StartItemNo < vFormatLastItemNo)
                            {
                                int vLen = GetItemOffsetAfter(SelectInfo.StartItemNo - 1);

                                Undo_New();
                                UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                // 如果RectItem前面(同一行)有高度小于此RectItme的Item(如Tab)，
                                // 其格式化时以RectItem为高，重新格式化时如果从RectItem所在位置起始格式化，
                                // 行高度仍会以Tab为行高，也就是RectItem高度，所以需要从行开始格式化
                                Items.Delete(SelectInfo.StartItemNo);
                                if (MergeItemText(Items[SelectInfo.StartItemNo - 1], Items[SelectInfo.StartItemNo]))
                                {
                                    UndoAction_InsertText(SelectInfo.StartItemNo - 1,
                                        Items[SelectInfo.StartItemNo - 1].Length + 1, Items[SelectInfo.StartItemNo].Text);

                                    Items.RemoveAt(SelectInfo.StartItemNo);
                                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 2, -2);
                                }
                                else
                                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);

                                SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                                SelectInfo.StartItemOffset = vLen;
                            }
                            else  // 段尾(段不只一个Item)
                            {
                                Undo_New();
                                UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                Items.Delete(SelectInfo.StartItemNo);

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);

                                SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                                SelectInfo.StartItemOffset = GetItemOffsetAfter(SelectInfo.StartItemNo);
                            }
                        }

                        ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                        break;

                    case User.VK_TAB:
                        TABKeyDown(vCurItem, e);
                        break;
                }
            }
            else
            if (SelectInfo.StartItemOffset == HC.OffsetAfter)  // 在其后
            {
                switch (Key)
                {
                    case User.VK_BACK:
                        if (!DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actDeleteItem))
                        {
                            SelectInfo.StartItemOffset = HC.OffsetBefor;
                            return;
                        }

                        GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                        FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                        if (vCurItem.ParaFirst)
                        {
                            if ((SelectInfo.StartItemNo >= 0)
                                && (SelectInfo.StartItemNo < Items.Count - 1)
                                && (!Items[SelectInfo.StartItemNo + 1].ParaFirst))
                            {
                                Undo_New();
                                UndoAction_DeleteItem(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                                Items.Delete(SelectInfo.StartItemNo);

                                UndoAction_ItemParaFirst(SelectInfo.StartItemNo, 0, true);
                                Items[SelectInfo.StartItemNo].ParaFirst = true;
                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);

                                ReSetSelectAndCaret(SelectInfo.StartItemNo, 0);
                            }
                            else  // 空段了
                            {
                                Undo_New();
                                UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                Items.Delete(SelectInfo.StartItemNo);

                                HCCustomItem vItem = CreateDefaultTextItem();
                                vItem.ParaFirst = true;
                                Items.Insert(SelectInfo.StartItemNo, vItem);
                                UndoAction_InsertItem(SelectInfo.StartItemNo, 0);

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                                SelectInfo.StartItemOffset = 0;
                            }
                        }
                        else  // 不是段首
                        {
                            SelectInfo.StartItemOffset = HC.OffsetBefor;
                            Keys vKeys = Keys.Delete;
                            if (e.Shift)
                                vKeys |= Keys.Shift;

                            if (e.Alt)
                                vKeys |= Keys.Alt;
                            KeyEventArgs vArgs = new KeyEventArgs(vKeys);
                            RectItemKeyDown(vSelectExist, ref vCurItem, vArgs, aPageBreak);
                        }
                        break;

                    case User.VK_DELETE:
                        if (SelectInfo.StartItemNo < Items.Count - 1)  // 不是最后一个
                        {
                            if (Items[SelectInfo.StartItemNo + 1].ParaFirst)  // 下一个是段首（当前是在段最后面delete删除）
                            {
                                vFormatFirstDrawItemNo = GetFormatFirstDrawItem(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                                vFormatLastItemNo = GetParaLastItemNo(SelectInfo.StartItemNo + 1);
                                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                Undo_New();
                                if (IsEmptyLine(SelectInfo.StartItemNo + 1))
                                {
                                    UndoAction_DeleteItem(SelectInfo.StartItemNo + 1, 0);
                                    Items.Delete(SelectInfo.StartItemNo + 1);

                                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);
                                }
                                else
                                {
                                    UndoAction_ItemParaFirst(SelectInfo.StartItemNo + 1, 0, false);
                                    Items[SelectInfo.StartItemNo + 1].ParaFirst = false;

                                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                                    ReSetSelectAndCaret(SelectInfo.StartItemNo + 1, 0);
                                }
                            }
                            else
                            {
                                SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                                SelectInfo.StartItemOffset = 0;
                                KeyDown(e);
                            }
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
                            if (vRectItem is HCDomainItem)
                                SelectInfo.StartItemOffset = vRectItem.GetOffsetAt(-1);
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
                        if (!DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actReturnItem))
                            return;

                        GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                        FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                        Undo_New();
                        if ((SelectInfo.StartItemNo < Items.Count - 1)  // 不是最后一个)
                                && (!Items[SelectInfo.StartItemNo + 1].ParaFirst))  // 下一个不是段首
                        {
                            UndoAction_ItemParaFirst(SelectInfo.StartItemNo + 1, 0, true);
                            Items[SelectInfo.StartItemNo + 1].ParaFirst = true;

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                            SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                            SelectInfo.StartItemOffset = 0;
                            CaretDrawItemNo = Items[SelectInfo.StartItemNo].FirstDItemNo;
                        }
                        else
                        {
                            vCurItem = CreateDefaultTextItem();
                            vCurItem.ParaFirst = true;
                            vCurItem.PageBreak = aPageBreak;

                            Items.Insert(SelectInfo.StartItemNo + 1, vCurItem);
                            UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + 1, 1);
                            ReSetSelectAndCaret(SelectInfo.StartItemNo + 1, vCurItem.Length);
                        }
                        break;

                    case User.VK_TAB:
                        TABKeyDown(vCurItem, e);
                        break;
                }
            }
        }

        private void EnterKeyDown(ref HCCustomItem vCurItem, KeyEventArgs e, bool aPageBreak)
        {
            if (!DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actReturnItem))
                return;

            int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
            GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
            // 判断光标位置内容如何换行
            if (SelectInfo.StartItemOffset == 0)  // 光标在Item最前面
            {
                if (!vCurItem.ParaFirst)  // 原来不是段首(光标在Item最前面)
                {
                    Undo_New();

                    UndoAction_ItemParaFirst(SelectInfo.StartItemNo, 0, true);
                    vCurItem.ParaFirst = true;

                    if (aPageBreak)
                    {
                        UndoAction_ItemPageBreak(SelectInfo.StartItemNo, 0, true);
                        vCurItem.PageBreak = true;
                    }

                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0, aPageBreak);
                }
                else  // 原来就是段首(光标在Item最前面)
                {
                    if (aPageBreak)
                    {
                        Undo_New();
                        if (this.Items.Count == 1)
                        {
                            // 补充一个空行
                            HCCustomItem vItem = CreateDefaultTextItem();
                            vItem.StyleNo = vCurItem.StyleNo;
                            vItem.ParaNo = vCurItem.ParaNo;
                            vItem.ParaFirst = true;

                            UndoAction_ItemPageBreak(SelectInfo.StartItemNo, 0, true);
                            vCurItem.PageBreak = true;

                            Items.Insert(SelectInfo.StartItemNo, vItem);
                            UndoAction_InsertItem(SelectInfo.StartItemNo, 0);

                            SelectInfo.StartItemNo++;
                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + 1, 1);
                        }
                        else  // 不是文档唯一Item
                        {
                            UndoAction_ItemPageBreak(SelectInfo.StartItemNo, 0, true);
                            vCurItem.PageBreak = true;

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0, true);
                        }
                    }
                    else
                    {
                        HCCustomItem vItem = CreateDefaultTextItem();
                        vItem.ParaNo = vCurItem.ParaNo;
                        vItem.StyleNo = vCurItem.StyleNo;
                        vItem.ParaFirst = true;

                        Items.Insert(SelectInfo.StartItemNo, vItem);  // 插入到当前

                        Undo_New();
                        UndoAction_InsertItem(SelectInfo.StartItemNo, 0);

                        SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                        ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + 1, 1);
                    }
                }
            }
            else
            if (SelectInfo.StartItemOffset == vCurItem.Length)  // 光标在Item最后面
            {
                HCCustomItem vItem = null;
                if (SelectInfo.StartItemNo < Items.Count - 1)
                {
                    vItem = Items[SelectInfo.StartItemNo + 1];  // 下一个Item
                    if (!vItem.ParaFirst)  // 下一个不是段起始
                    {
                        Undo_New();

                        UndoAction_ItemParaFirst(SelectInfo.StartItemNo + 1, 0, true);
                        vItem.ParaFirst = true;

                        if (aPageBreak)
                        {
                            UndoAction_ItemPageBreak(SelectInfo.StartItemNo + 1, 0, true);
                            vItem.PageBreak = true;
                        }

                        ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0, aPageBreak);
                    }
                    else  // 下一个是段起始
                    {
                        vItem = CreateDefaultTextItem();
                        vItem.ParaNo = vCurItem.ParaNo;
                        vItem.StyleNo = vCurItem.StyleNo;
                        vItem.ParaFirst = true;

                        if (aPageBreak)
                            vItem.PageBreak = true;

                        Items.Insert(SelectInfo.StartItemNo + 1, vItem);

                        Undo_New();
                        UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);

                        ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + 1, 1, aPageBreak);
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

                    if (aPageBreak)
                        vItem.PageBreak = true;

                    Items.Insert(SelectInfo.StartItemNo + 1, vItem);

                    Undo_New();
                    UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);

                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + 1, 1, aPageBreak);
                    SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                    SelectInfo.StartItemOffset = 0;
                }
            }
            else  // 光标在Item中间
            {
                HCCustomItem vItem = vCurItem.BreakByOffset(SelectInfo.StartItemOffset);  // 截断当前Item

                Undo_New();
                UndoAction_DeleteText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1, vItem.Text);

                vItem.ParaFirst = true;

                if (aPageBreak)
                    vItem.PageBreak = true;

                Items.Insert(SelectInfo.StartItemNo + 1, vItem);
                UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);

                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + 1, 1, aPageBreak);

                SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                SelectInfo.StartItemOffset = 0;
            }

            if (!e.Handled)
                CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
        }

        private void DeleteKeyDown(HCCustomItem vCurItem, KeyEventArgs e)
        {
            if (!DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actDeleteText))  // 不可删除
            {
                SelectInfo.StartItemOffset = Items[SelectInfo.StartItemNo].Length;  // SelectInfo.StartItemOffset + 1
                ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                return;
            }

            int vDelCount = 0, vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
            int vCurItemNo = SelectInfo.StartItemNo;
            GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);

            if (SelectInfo.StartItemOffset == vCurItem.Length)
            {
                if (vCurItemNo != Items.Count - 1)
                {
                    if (Items[vCurItemNo + 1].ParaFirst)
                    {
                        vFormatLastItemNo = GetParaLastItemNo(vCurItemNo + 1);  // 获取下一段最后一个
                        if (vCurItem.Length == 0)  // 当前是空行
                        {
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            Undo_New();
                            UndoAction_DeleteItem(vCurItemNo, 0);
                            Items.Delete(vCurItemNo);

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);
                        }
                        else  // 当前不是空行
                        {
                            if (Items[vCurItemNo + 1].StyleNo < HCStyle.Null)
                            {
                                vFormatLastItemNo = GetParaLastItemNo(vCurItemNo + 1);  // 获取下一段最后一个
                                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                Undo_New();

                                UndoAction_ItemParaFirst(vCurItemNo + 1, 0, false);
                                Items[vCurItemNo + 1].ParaFirst = false;

                                if (Items[vCurItemNo + 1].PageBreak)
                                {
                                    UndoAction_ItemPageBreak(vCurItemNo + 1, 0, false);
                                    Items[vCurItemNo + 1].PageBreak = false;
                                }

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                SelectInfo.StartItemNo = vCurItemNo + 1;
                                SelectInfo.StartItemOffset = HC.OffsetBefor;
                            }
                            else  // 下一个段首是TextItem(当前在上一段段尾)
                            {
                                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                if (Items[vCurItemNo + 1].Length == 0)  // 下一段的段首是是空行
                                {
                                    Undo_New();
                                    UndoAction_DeleteItem(vCurItemNo + 1, 0);
                                    Items.Delete(vCurItemNo + 1);
                                    vDelCount++;
                                }
                                else  // 下一段的段首不是空行
                                {
                                    if (vCurItem.CanConcatItems(Items[vCurItemNo + 1]))
                                    {
                                        Undo_New();

                                        UndoAction_InsertText(vCurItemNo, vCurItem.Length + 1, Items[vCurItemNo + 1].Text);
                                        vCurItem.Text = vCurItem.Text + Items[vCurItemNo + 1].Text;

                                        UndoAction_DeleteItem(vCurItemNo + 1, 0);
                                        Items.Delete(vCurItemNo + 1);

                                        vDelCount++;
                                    }
                                    else// 下一段段首不是空行也不能合并
                                        Items[vCurItemNo + 1].ParaFirst = false;

                                    // 修正下一段合并上来的Item段样式，对齐样式
                                    int vParaNo = Items[vCurItemNo].ParaNo;
                                    for (int i = vCurItemNo + 1; i <= vFormatLastItemNo - vDelCount; i++)
                                        Items[i].ParaNo = vParaNo;
                                }

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
                            }
                        }
                    }
                    else  // 下一个不能合并也不是段首，移动到下一个开头再调用DeleteKeyDown
                    {
                        SelectInfo.StartItemNo = vCurItemNo + 1;
                        SelectInfo.StartItemOffset = 0;
                        CaretDrawItemNo = Items[SelectInfo.StartItemNo].FirstDItemNo;

                        KeyDown(e);
                        return;
                    }
                }
            }
            else  // 光标不在Item最右边
            {
                string vText = Items[vCurItemNo].Text;
                #if UNPLACEHOLDERCHAR
                int vCharCount = HC.GetTextActualOffset(vText, SelectInfo.StartItemOffset + 1, true) - SelectInfo.StartItemOffset;
                string vsDelete = vText.Substring(SelectInfo.StartItemOffset + 1 - 1, vCharCount);
                vText = vText.Remove(SelectInfo.StartItemOffset + 1 - 1, vCharCount);
                #else
                string vsDelete = vText.Substring(SelectInfo.StartItemOffset + 1 - 1, 1);
                vCurItem.Text = vText.Remove(SelectInfo.StartItemOffset + 1 - 1, 1);
                #endif
                DoItemAction(vCurItemNo, SelectInfo.StartItemOffset + 1, HCAction.actDeleteText);

                if (vText == "")  // 删除后没有内容了
                {
                    if (!DrawItems[Items[vCurItemNo].FirstDItemNo].LineFirst)  // 该Item不是行首
                    {
                        if (vCurItemNo < Items.Count - 1)
                        {
                            int vLen = -1;
                            if (MergeItemText(Items[vCurItemNo - 1], Items[vCurItemNo + 1]))  // 下一个可合并到上一个
                            {
                                vLen = Items[vCurItemNo + 1].Length;

                                Undo_New();
                                UndoAction_InsertText(vCurItemNo - 1, Items[vCurItemNo - 1].Length - vLen + 1, Items[vCurItemNo + 1].Text);

                                GetFormatRange(vCurItemNo - 1, vLen, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                                        
                                UndoAction_DeleteItem(vCurItemNo, 0);
                                Items.Delete(vCurItemNo);  // 删除当前

                                UndoAction_DeleteItem(vCurItemNo, 0);
                                Items.Delete(vCurItemNo);  // 删除下一个

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 2, -2);
                            }
                            else  // 下一个合并不到上一个
                            {
                                vLen = 0;
                                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                Undo_New();
                                UndoAction_DeleteItem(vCurItemNo, 0);
                                Items.Delete(vCurItemNo);

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);
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
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            Undo_New();
                            UndoAction_DeleteItem(vCurItemNo, 0);
                            Items.Delete(vCurItemNo);

                            SelectInfo.StartItemNo = vCurItemNo - 1;
                            SelectInfo.StartItemOffset = GetItemOffsetAfter(SelectInfo.StartItemNo);
                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);
                        }
                    }
                    else  // 行首Item被删空了
                    {
                        if (vCurItemNo != vFormatLastItemNo)
                        {
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                            SelectInfo.StartItemOffset = 0;

                            Undo_New();
                            UndoAction_ItemParaFirst(vCurItemNo + 1, 0, Items[vCurItemNo].ParaFirst);
                            Items[vCurItemNo + 1].ParaFirst = Items[vCurItemNo].ParaFirst;

                            Undo_New();
                            UndoAction_DeleteItem(vCurItemNo, 0);
                            Items.RemoveAt(vCurItemNo);

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);
                        }
                        else  // 当前段删除空了
                        {
                            // 防止不支持键盘输入修改内容的数据元是段第一个且用键盘删空内容后
                            // 再用键盘输入时数据元保留空内容的问题(因数据元不支持手动修改内容)
                            // 所以使用先删除为空的Item再插入空Item 20190802001
                            FormatPrepare(vFormatFirstDrawItemNo);

                            bool vPageBreak = vCurItem.PageBreak;

                            Undo_New();
                            UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                            Items.Delete(SelectInfo.StartItemNo);

                            HCCustomItem vItem = CreateDefaultTextItem();
                            vItem.ParaFirst = true;
                            vItem.PageBreak = vPageBreak;

                            Items.Insert(SelectInfo.StartItemNo, vItem);
                            UndoAction_InsertItem(SelectInfo.StartItemNo, 0);
                            /*vCurItem.Text = vText;

                            Undo_New();
                            UndoAction_DeleteText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1, vsDelete);

                            FormatPrepare(vFormatFirstDrawItemNo);*/
                            SelectInfo.StartItemOffset = 0;
                            ReFormatData(vFormatFirstDrawItemNo);
                        }
                    }
                }
                else  // 删除后还有内容
                {
                    vCurItem.Text = vText;

                    Undo_New();
                    UndoAction_DeleteText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1, vsDelete);

                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                }
            }

            if (!e.Handled)
                CaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
        }

        private void BackspaceKeyDown(bool vSelectExist, ref HCCustomItem vCurItem, int vParaFirstItemNo, int vParaLastItemNo, KeyEventArgs e)
        {
            if (!DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actBackDeleteText))  // 不允许删除
            {
                //LeftKeyDown  // 往前走
                SelectInfo.StartItemOffset = 0;
                ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                return;
            }

            int vCurItemNo = -1, vLen = -1, vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1, vParaNo = -1;
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
                if (vCurItem.PageBreak)
                {
                    GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                    Undo_New();
                    UndoAction_ItemPageBreak(SelectInfo.StartItemNo, 0, false);
                    vCurItem.PageBreak = false;

                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo, 0, true);
                }
                else
                if (SelectInfo.StartItemNo != 0)  // 不是第1个Item最前面删除
                {
                    if (vCurItem.ParaFirst)  // 是段起始Item
                    {
                        vLen = Items[SelectInfo.StartItemNo - 1].Length;
                        if (vCurItem.CanConcatItems(Items[SelectInfo.StartItemNo - 1]))
                        {
                            vFormatFirstDrawItemNo = GetFormatFirstDrawItem(SelectInfo.StartItemNo - 1, vLen);
                            vFormatLastItemNo = GetParaLastItemNo(SelectInfo.StartItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            Undo_New();
                            UndoAction_InsertText(SelectInfo.StartItemNo - 1, Items[SelectInfo.StartItemNo - 1].Length + 1,
                                Items[SelectInfo.StartItemNo].Text);

                            Items[SelectInfo.StartItemNo - 1].Text = Items[SelectInfo.StartItemNo - 1].Text
                                + Items[SelectInfo.StartItemNo].Text;

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

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);

                            ReSetSelectAndCaret(SelectInfo.StartItemNo - 1, vLen);
                        }
                        else  // 段起始且不能和上一个合并
                        {
                            if (IsEmptyLine(SelectInfo.StartItemNo - 1))
                            {
                                vFormatFirstDrawItemNo = GetFormatFirstDrawItem(SelectInfo.StartItemNo - 1, vLen);
                                vFormatLastItemNo = GetParaLastItemNo(SelectInfo.StartItemNo);
                                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                Undo_New();
                                UndoAction_DeleteItem(SelectInfo.StartItemNo - 1, 0);
                                Items.Delete(SelectInfo.StartItemNo - 1);

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);

                                ReSetSelectAndCaret(SelectInfo.StartItemNo - 1, 0);
                            }
                            else
                            {
                                if (vCurItem.Length == 0)  // 已经没有内容了
                                {
                                    vFormatFirstDrawItemNo = GetFormatFirstDrawItem(SelectInfo.StartItemNo - 1, vLen);
                                    FormatPrepare(vFormatFirstDrawItemNo, SelectInfo.StartItemNo);

                                    Undo_New();
                                    UndoAction_DeleteItem(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                                    Items.Delete(SelectInfo.StartItemNo);

                                    ReFormatData(vFormatFirstDrawItemNo, SelectInfo.StartItemNo - 1, -1);

                                    ReSetSelectAndCaret(SelectInfo.StartItemNo - 1);
                                }
                                else  // 段前删除且不能和上一段最后合并
                                {
                                    vFormatFirstDrawItemNo = GetFormatFirstDrawItem(SelectInfo.StartItemNo - 1, GetItemOffsetAfter(SelectInfo.StartItemNo - 1));
                                    vFormatLastItemNo = GetParaLastItemNo(SelectInfo.StartItemNo);
                                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

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

                                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                    ReSetSelectAndCaret(SelectInfo.StartItemNo, 0);
                                }
                            }
                        }
                    }
                    else  // 在Item开始往前删，但Item不是段起始
                    {
                        // 从前一个最后开始重新处理
                        SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                        SelectInfo.StartItemOffset = GetItemOffsetAfter(SelectInfo.StartItemNo);
                        CaretDrawItemNo = Items[SelectInfo.StartItemNo].FirstDItemNo;
                        KeyDown(e);
                        return;
                    }
                }
            }
            else  // 光标不在Item最开始  文本TextItem
            {
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

                            GetFormatRange(vCurItemNo - 1, vLen, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                            Items.Delete(vCurItemNo);  // 删除当前

                            UndoAction_DeleteItem(vCurItemNo, 0);
                            Items.Delete(vCurItemNo);  // 删除下一个

                            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 2, -2);

                            ReSetSelectAndCaret(SelectInfo.StartItemNo - 1, vLen);  // 上一个原光标位置
                        }
                        else  // 当前不是行首，删除后没有内容了，且不能合并上一个和下一个
                        {
                            if (SelectInfo.StartItemNo == vParaLastItemNo)
                            {
                                GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                Undo_New();
                                UndoAction_DeleteItem(vCurItemNo, SelectInfo.StartItemOffset);
                                Items.Delete(vCurItemNo);

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);

                                ReSetSelectAndCaret(vCurItemNo - 1);
                            }
                            else  // 不是段最后一个
                            {
                                GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                Undo_New();
                                UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                Items.Delete(vCurItemNo);

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);

                                ReSetSelectAndCaret(vCurItemNo - 1);
                            }
                        }
                    }
                    else  // Item是行第一个、行首Item删除空了，
                    {
                        if (Items[vCurItemNo].ParaFirst)
                        {
                            GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                            if (vCurItemNo < vFormatLastItemNo)
                            {
                                Undo_New();

                                vParaFirst = true;  // Items[vCurItemNo].ParaFirst;  // 记录行首Item的段属性

                                UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                Items.Delete(vCurItemNo);

                                if (vParaFirst)
                                {
                                    UndoAction_ItemParaFirst(vCurItemNo, 0, vParaFirst);
                                    Items[vCurItemNo].ParaFirst = vParaFirst;  // 其后继承段首属性
                                }

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);
                                ReSetSelectAndCaret(vCurItemNo, 0);  // 下一个最前面
                            }
                            else  // 是段首删除空，同段后面没有内容了，变成空行
                            {
                                // 防止不支持键盘输入修改内容的数据元是段第一个且用键盘Backspace删空内容后
                                // 再用键盘输入时数据元保留空内容的问题(因数据元不支持手动修改内容)
                                // 所以使用先删除为空的Item再插入空Item 20190802001
                                bool vPageBreak = vCurItem.PageBreak;

                                Undo_New();
                                UndoAction_DeleteItem(SelectInfo.StartItemNo, 0);
                                Items.Delete(SelectInfo.StartItemNo);

                                HCCustomItem vItem = CreateDefaultTextItem();
                                vItem.ParaFirst = true;
                                vItem.PageBreak = vPageBreak;

                                Items.Insert(SelectInfo.StartItemNo, vItem);
                                UndoAction_InsertItem(SelectInfo.StartItemNo, 0);
                                SelectInfo.StartItemOffset = 0;
                                
                                /*
                                UndoAction_DeleteBackText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, vCurItem.Text);  // Copy(vText, SelectInfo.StartItemOffset, 1));

                                //System.Delete(vText, SelectInfo.StartItemOffset, 1);
                                vCurItem.Text = "";  // vText;
                                SelectInfo.StartItemOffset = SelectInfo.StartItemOffset - 1; */

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);  // 保留空行
                            }
                        }
                        else  // 不是段首Item，仅是行首Item删除空了
                        {
                            Undo_New();

                            if (vCurItemNo < GetParaLastItemNo(vCurItemNo))
                            {
                                vLen = Items[vCurItemNo - 1].Length;
                                if (MergeItemText(Items[vCurItemNo - 1], Items[vCurItemNo + 1]))
                                {
                                    UndoAction_InsertText(vCurItemNo - 1,
                                        Items[vCurItemNo - 1].Length - Items[vCurItemNo + 1].Length + 1, Items[vCurItemNo + 1].Text);

                                    GetFormatRange(vCurItemNo - 1, GetItemOffsetAfter(vCurItemNo - 1), ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);  // 取前一个格式化起始位置
                                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                    UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);  // 删除空的Item
                                    Items.Delete(vCurItemNo);

                                    UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);  // 被合并的Item
                                    Items.Delete(vCurItemNo);

                                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 2, -2);
                                    ReSetSelectAndCaret(vCurItemNo - 1, vLen);
                                }
                                else  // 前后不能合并
                                {
                                    GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                    UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                    Items.Delete(vCurItemNo);

                                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);
                                    ReSetSelectAndCaret(vCurItemNo - 1);
                                }
                            }
                            else  // 同段后面没有内容了
                            {
                                GetFormatRange(ref vFormatFirstDrawItemNo, ref  vFormatLastItemNo);
                                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                                UndoAction_DeleteItem(vCurItemNo, Items[vCurItemNo].Length);
                                Items.Delete(vCurItemNo);

                                ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - 1, -1);
                                ReSetSelectAndCaret(vCurItemNo - 1);
                            }
                        }
                    }
                }
                else  // 删除后还有内容
                {
                    GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                    DoItemAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actBackDeleteText);
                    string vText = vCurItem.Text;  // 和上面 201806242257 处一样

                    Undo_New();
                    UndoAction_DeleteBackText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, vText.Substring(SelectInfo.StartItemOffset - 1, 1));

                    vCurItem.Text = vText.Remove(SelectInfo.StartItemOffset - 1, 1); ;

                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);

                    SelectInfo.StartItemOffset = SelectInfo.StartItemOffset - 1;
                    ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                }
            }
        }
        #endregion

        // Key返回0表示此键按下Data没有做任何事情
        public virtual void KeyDown(KeyEventArgs e, bool aPageBreak = false)
        {
            if (HC.IsKeyDownEdit(e.KeyValue) && (!CanEdit()))
                return;

            int Key = e.KeyValue;

            if ((Key == User.VK_BACK)
                || (Key == User.VK_DELETE)
                || (Key == User.VK_RETURN)
                || (Key == User.VK_TAB))
                this.InitializeMouseField();  // 如果Item删除完了，原MouseMove处ItemNo可能不存在了，再MouseMove时清除旧的出错

            HCCustomItem vCurItem = GetActiveItem();
            if (vCurItem == null)
                return;

            bool vSelectExist = SelectExists();
            if (vSelectExist && ((Key == User.VK_BACK)
                                    || (Key == User.VK_DELETE)
                                    || (Key == User.VK_RETURN)
                                    || (Key == User.VK_TAB))
                )
            {
                if (!DeleteSelected())
                    return;

                if ((Key == User.VK_BACK) || (Key == User.VK_DELETE))
                    return;
            }

            int vParaFirstItemNo = -1, vParaLastItemNo = -1;
            GetParaItemRang(SelectInfo.StartItemNo, ref vParaFirstItemNo, ref vParaLastItemNo);

            if (vCurItem.StyleNo < HCStyle.Null)
                RectItemKeyDown(vSelectExist, ref vCurItem, e, aPageBreak);
            else
            {
                switch (Key)
                {
                    case User.VK_BACK:
                        BackspaceKeyDown(vSelectExist, ref vCurItem, vParaFirstItemNo, vParaLastItemNo, e);  // 回删
                        break;

                    case User.VK_RETURN:
                        EnterKeyDown(ref vCurItem, e, aPageBreak);  // 回车
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

        public virtual bool DoInsertTextBefor(int aItemNo, int aOffset, string aText)
        {
            return aText != "";
        }

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

        public void DeleteItems(int aStartNo, int aEndNo, bool aKeepPara)
        {
            if (!CanEdit()) return;

            if (aEndNo < aStartNo) return;

            this.InitializeField();

            int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
            GetFormatRange(aStartNo, 0, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);

            int vFormatDrawItemNo2 = 0;
            if ((!aKeepPara) && (aEndNo < Items.Count - 1) && (Items[aEndNo + 1].ParaFirst))
                GetFormatRange(aEndNo + 1, GetItemOffsetAfter(aEndNo + 1), ref vFormatDrawItemNo2, ref vFormatLastItemNo);
            else
                GetFormatRange(aEndNo, GetItemOffsetAfter(aEndNo), ref vFormatDrawItemNo2, ref vFormatLastItemNo);

            if (Items[aStartNo].ParaFirst && (vFormatFirstDrawItemNo > 0))
            {
                vFormatFirstDrawItemNo--;
                vFormatFirstDrawItemNo = GetFormatFirstDrawItem(vFormatFirstDrawItemNo);
            }

            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

            bool vStartParaFirst = Items[aStartNo].ParaFirst;
            int vDelCount = aEndNo - aStartNo + 1;
            Undo_New();
            for (int i = aEndNo; i >= aStartNo; i--)
            {
                UndoAction_DeleteItem(i, 0);
                Items.Delete(i);
            }

            if (Items.Count == 0)  // 删除没有了，不用SetEmptyData，因为其无Undo
            {
                HCCustomItem vItem = CreateDefaultTextItem();
                this.CurStyleNo = vItem.StyleNo;
                vItem.ParaFirst = true;
                Items.Add(vItem);
                vDelCount--;
                UndoAction_InsertItem(0, 0);
            }
            else
            if (vStartParaFirst)  // 段首删除了
            {
                if ((aStartNo < Items.Count - 1) && (!Items[aStartNo].ParaFirst))  // 下一个不是段首
                {
                    UndoAction_ItemParaFirst(aStartNo, 0, true);
                    Items[aStartNo].ParaFirst = true;
                }
                else  // 段删除完了
                if (aKeepPara)  // 保持段
                {
                    HCCustomItem vItem = CreateDefaultTextItem();
                    this.CurStyleNo = vItem.StyleNo;
                    vItem.ParaFirst = true;
                    Items.Insert(aStartNo, vItem);
                    vDelCount--;
                    UndoAction_InsertItem(aStartNo, 0);
                }
            }

            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - vDelCount, -vDelCount);
            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();

            if (vStartParaFirst && aKeepPara)
                ReSetSelectAndCaret(aStartNo, 0);
            else
            if (aStartNo > 0)
                ReSetSelectAndCaret(aStartNo - 1);
            else  // 从第一个开始删除
                ReSetSelectAndCaret(0, 0);
        }

        public void DeleteActiveDataItems(int aStartNo, int aEndNo, bool aKeepPara)
        {
            if ((aStartNo < 0) || (aStartNo > Items.Count - 1))
                return;

            HCCustomItem vActiveItem = this.GetActiveItem();

            if ((vActiveItem.StyleNo < HCStyle.Null)  // 当前位置是 RectItem)
                && (SelectInfo.StartItemOffset == HC.OffsetInner))  // 在其上输入内容
            {
                Undo_New();

                HCCustomRectItem vRectItem = vActiveItem as HCCustomRectItem;
                if (vRectItem.MangerUndo)
                    UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                else
                    UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                vRectItem.DeleteActiveDataItems(aStartNo, aEndNo, aKeepPara);
                if (vRectItem.IsFormatDirty)
                {
                    int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
                    GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                    vRectItem.IsFormatDirty = false;
                }
                else
                    this.FormatInit();
            }
            else
                DeleteItems(aStartNo, aEndNo, aKeepPara);
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
                if (!aSrcData.IsEmptyLine(i))
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

        #region DoTextItemInsert 在文本Item前后或中间插入文
        private bool DoTextItemInsert(string AText, bool ANewPara, ref int vAddCount)
        {
            bool Result = false;
            HCTextItem vTextItem = Items[SelectInfo.StartItemNo] as HCTextItem;

            if (vTextItem.StyleNo == this.CurStyleNo)
            {
                int vOffset = 0;

                if (SelectInfo.StartItemOffset == 0 && vTextItem.Length > 0)  // 在TextItem最前面插入
                {
                    vOffset = AText.Length;

                    if (ANewPara || AText == "")  // 另起一段
                    {
                        if ((!ANewPara) && (!Items[SelectInfo.StartItemNo].ParaFirst))
                        {
                            UndoAction_ItemParaFirst(SelectInfo.StartItemNo, 0, true);
                            vTextItem.ParaFirst = true;
                        }
                        else
                        {
                            HCCustomItem vNewItem = CreateDefaultTextItem();
                            vNewItem.Text = AText;
                            vNewItem.ParaFirst = true;

                            Items.Insert(SelectInfo.StartItemNo, vNewItem);
                            UndoAction_InsertItem(SelectInfo.StartItemNo, 0);
                            vAddCount++;
                        }
                    }
                    else  // 同段
                    if (vTextItem.AcceptAction(0, SelectInfo.StartRestrain, HCAction.actConcatText))  // TextItem最前面可接受连接字符
                    {
                        UndoAction_InsertText(SelectInfo.StartItemNo, 1, AText);
                        vTextItem.Text = AText + vTextItem.Text;
                    }
                    else  // TextItem最前面不接收字符
                    {
                        if (vTextItem.ParaFirst)  // 段最前面不接收，新插入的作为段首
                        {
                            HCCustomItem vNewItem = CreateDefaultTextItem();
                            vNewItem.Text = AText;
                            vNewItem.ParaFirst = true;

                            UndoAction_ItemParaFirst(SelectInfo.StartItemNo, 0, false);
                            vTextItem.ParaFirst = false;

                            Items.Insert(SelectInfo.StartItemNo, vNewItem);
                            UndoAction_InsertItem(SelectInfo.StartItemNo, 0);
                            vAddCount++;
                        }
                        else  // TextItem最前面不接收字符，TextItem不是段最前面
                        if (Items[SelectInfo.StartItemNo - 1].StyleNo > HCStyle.Null)  // 前一个是文本
                        {
                            vTextItem = Items[SelectInfo.StartItemNo - 1] as HCTextItem;
                            if (vTextItem.AcceptAction(vTextItem.Length, true, HCAction.actConcatText))  // 前一个在最后可接受连接字符
                            //and MergeItemText(Items[SelectInfo.StartItemNo - 1], vNewItem)
                            {
                                UndoAction_InsertText(SelectInfo.StartItemNo - 1, vTextItem.Length + 1, AText);
                                vTextItem.Text = vTextItem.Text + AText;
                                SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                                vOffset = vTextItem.Length;
                            }
                            else
                            {
                                HCCustomItem vNewItem = CreateDefaultTextItem();
                                vNewItem.Text = AText;

                                Items.Insert(SelectInfo.StartItemNo, vNewItem);
                                UndoAction_InsertItem(SelectInfo.StartItemNo, 0);
                                vAddCount++;
                            }
                        }
                        else  // 前一个不是文本
                        {
                            HCCustomItem vNewItem = CreateDefaultTextItem();
                            vNewItem.Text = AText;

                            Items.Insert(SelectInfo.StartItemNo, vNewItem);
                            UndoAction_InsertItem(SelectInfo.StartItemNo, 0);
                            vAddCount++;
                        }
                    }
                }
                else
                if (SelectInfo.StartItemOffset == vTextItem.Length)  // 在TextItem最后插入
                {

                    if (ANewPara || (AText == ""))  // 另起一段
                    {
                        if ((!ANewPara)
                            && (SelectInfo.StartItemNo < Items.Count - 1)
                            && (!Items[SelectInfo.StartItemNo + 1].ParaFirst))
                        {
                            UndoAction_ItemParaFirst(SelectInfo.StartItemNo + 1, 0, true);
                            Items[SelectInfo.StartItemNo + 1].ParaFirst = true;

                            SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                            vOffset = 0;
                        }
                        else
                        {
                            HCCustomItem vNewItem = CreateDefaultTextItem();
                            vNewItem.Text = AText;
                            vNewItem.ParaFirst = true;

                            Items.Insert(SelectInfo.StartItemNo + 1, vNewItem);
                            UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);
                            vAddCount++;

                            SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                            vOffset = vNewItem.Length;
                        }
                    }
                    else  // 最后面插入文本不另起一段
                    if (vTextItem.AcceptAction(SelectInfo.StartItemOffset, SelectInfo.StartRestrain, HCAction.actConcatText))  // 最后面能接收文本
                    {
                        UndoAction_InsertText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1, AText);
                        vTextItem.Text = vTextItem.Text + AText;
                        vOffset = vTextItem.Length;
                    }
                    else  // 最后面不能接收输入
                    if (!IsParaLastItem(SelectInfo.StartItemNo))  // 同段后面还有内容
                    {
                        if (Items[SelectInfo.StartItemNo + 1].StyleNo > HCStyle.Null)  // 同段后面是文本
                        {
                            vTextItem = Items[SelectInfo.StartItemNo + 1] as HCTextItem;
                            if (vTextItem.AcceptAction(0, true, HCAction.actConcatText))  // 后一个在最前可接受连接字符
                            //and MergeItemText(Items[SelectInfo.StartItemNo - 1], vNewItem)
                            {
                                UndoAction_InsertText(SelectInfo.StartItemNo + 1, 1, AText);
                                vTextItem.Text = AText + vTextItem.Text;
                                SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                                vOffset = AText.Length;
                            }
                            else
                            {
                                HCCustomItem vNewItem = CreateDefaultTextItem();
                                vNewItem.Text = AText;

                                Items.Insert(SelectInfo.StartItemNo + 1, vNewItem);
                                UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);
                                vAddCount++;
                                SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                                vOffset = AText.Length;
                            }
                        }
                        else  // 同段后面不是文本
                        {
                            HCCustomItem vNewItem = CreateDefaultTextItem();
                            vNewItem.Text = AText;

                            Items.Insert(SelectInfo.StartItemNo + 1, vNewItem);
                            UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);
                            vAddCount++;
                            SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                            vOffset = AText.Length;
                        }
                    }
                    else  // 段最后一个后面不接收输入
                    {
                        HCCustomItem vNewItem = CreateDefaultTextItem();
                        vNewItem.Text = AText;

                        Items.Insert(SelectInfo.StartItemNo + 1, vNewItem);
                        UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);
                        vAddCount++;
                        SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                        vOffset = AText.Length;
                    }
                }
                else  // 在Item中间
                {
                    if (ANewPara || (AText == ""))
                    {
                        // 原TextItem打断
                        string vS = vTextItem.SubString(SelectInfo.StartItemOffset + 1 - 1, vTextItem.Length - SelectInfo.StartItemOffset);
                        UndoAction_DeleteText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1, vS);
                        // 原位置后半部分
                        HCCustomItem vAfterItem = vTextItem.BreakByOffset(SelectInfo.StartItemOffset);
                        vAfterItem.Text = AText + vAfterItem.Text;
                        vAfterItem.ParaFirst = true;
                        // 插入原TextItem后半部分增加Text后的
                        Style.States.Include(HCState.hosInsertBreakItem);
                        try
                        {
                            Items.Insert(SelectInfo.StartItemNo + 1, vAfterItem);
                        }
                        finally
                        {
                            Style.States.Exclude(HCState.hosInsertBreakItem);
                        }

                        UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);
                        vAddCount++;

                        SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                        vOffset = AText.Length;
                    }
                    else
                    {
                        vOffset = SelectInfo.StartItemOffset + AText.Length;
                        string vS = vTextItem.Text;
                        vS = vS.Insert(SelectInfo.StartItemOffset, AText);
                        UndoAction_InsertText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1, AText);
                        vTextItem.Text = vS;
                    }
                }

                SelectInfo.StartItemOffset = vOffset;

                Result = true;
            }
            else  // 插入位置TextItem样式和当前样式不同，在TextItem头、中、尾没选中，但应用了新样式，以新样式处理
            {
                HCCustomItem vNewItem = CreateDefaultTextItem();
                vNewItem.Text = AText;
                vNewItem.ParaFirst = ANewPara || (AText == "");
   
                if (SelectInfo.StartItemOffset == 0)
                {
                    if ((!vNewItem.ParaFirst) && vTextItem.ParaFirst)
                    {
                        vNewItem.ParaFirst = true;
                        if (vTextItem.Length == 0)
                        {
                            UndoAction_DeleteItem(this.SelectInfo.StartItemNo, 0);
                            this.Items.RemoveAt(this.SelectInfo.StartItemNo);
                            vAddCount--;
                        }
                        else
                        {
                            UndoAction_ItemParaFirst(this.SelectInfo.StartItemNo, 0, false);
                            vTextItem.ParaFirst = false;
                        }
                    }

                    Items.Insert(SelectInfo.StartItemNo, vNewItem);
                    UndoAction_InsertItem(SelectInfo.StartItemNo, 0);
                    vAddCount++;

                    SelectInfo.StartItemOffset = vNewItem.Length;
                }
                else
                if (SelectInfo.StartItemOffset == vTextItem.Length)
                {
                    Items.Insert(SelectInfo.StartItemNo + 1, vNewItem);
                    UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);
                    vAddCount++;

                    SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                    SelectInfo.StartItemOffset = vNewItem.Length;
                }
                else  // 在TextItem中间插入
                {
                    // 原TextItem打断
                    string vS = vTextItem.SubString(SelectInfo.StartItemOffset + 1, vTextItem.Length - SelectInfo.StartItemOffset);
                    UndoAction_DeleteText(SelectInfo.StartItemNo, SelectInfo.StartItemOffset + 1, vS);
                    // 原位置后半部分
                    HCCustomItem vAfterItem = vTextItem.BreakByOffset(SelectInfo.StartItemOffset);
                    vAfterItem.ParaFirst = false;
                    // 先插入新的
                    Items.Insert(SelectInfo.StartItemNo + 1, vNewItem);
                    UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);
                    vAddCount++;
                    SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                    SelectInfo.StartItemOffset = vNewItem.Length;
                    // 再插入原TextItem后半部分
                    Style.States.Include(HCState.hosInsertBreakItem);
                    try
                    {
                        Items.Insert(SelectInfo.StartItemNo + 1, vAfterItem);
                    }
                    finally
                    {
                        Style.States.Exclude(HCState.hosInsertBreakItem);
                    }

                    UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);
                    vAddCount++;
                }
            }

            CurStyleNo = Items[SelectInfo.StartItemNo].StyleNo;  // 防止连续插入不同样式的
            return Result;
        }
        #endregion

        private void DoInsertTextEx(string aText, bool ANewPara, ref int vAddCount)
        {
            HCCustomItem vItem = Items[SelectInfo.StartItemNo];

            if (vItem.StyleNo < HCStyle.Null)
            {
                if (SelectInfo.StartItemOffset == HC.OffsetAfter)  // 在其后输入内容
                {
                    if ((SelectInfo.StartItemNo < Items.Count - 1)
                        && (Items[SelectInfo.StartItemNo + 1].StyleNo > HCStyle.Null)
                        && (!Items[SelectInfo.StartItemNo + 1].ParaFirst))
                    {
                        SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                        SelectInfo.StartItemOffset = 0;
                        CurStyleNo = Items[SelectInfo.StartItemNo].StyleNo;
                        DoInsertTextEx(aText, ANewPara, ref vAddCount);  // 在下一个TextItem前面插入
                    }
                    else  // 最后或下一个还是RectItem或当前是段尾
                    {
                        if ((aText == "")
                            && (SelectInfo.StartItemNo < Items.Count - 1)
                            && (!Items[SelectInfo.StartItemNo + 1].ParaFirst))
                        {
                            UndoAction_ItemParaFirst(SelectInfo.StartItemNo + 1, 0, true);
                            Items[SelectInfo.StartItemNo + 1].ParaFirst = true;

                            SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                            SelectInfo.StartItemOffset = 0;
                        }
                        else
                        {
                            vItem = CreateDefaultTextItem();
                            vItem.Text = aText;
                            vItem.ParaFirst = ANewPara || (aText == "");

                            Items.Insert(SelectInfo.StartItemNo + 1, vItem);  // 在两个RectItem中间插入
                            UndoAction_InsertItem(SelectInfo.StartItemNo + 1, 0);
                            vAddCount++;

                            SelectInfo.StartItemNo = SelectInfo.StartItemNo + 1;
                            SelectInfo.StartItemOffset = vItem.Length;
                            CurStyleNo = vItem.StyleNo;
                        }
                    }
                }
                else  // 在RectItem前输入内容
                {
                    if ((SelectInfo.StartItemNo > 0)
                        && (Items[SelectInfo.StartItemNo - 1].StyleNo > HCStyle.Null)
                        && (!vItem.ParaFirst))
                    {
                        SelectInfo.StartItemNo = SelectInfo.StartItemNo - 1;
                        SelectInfo.StartItemOffset = Items[SelectInfo.StartItemNo].Length;
                        CurStyleNo = Items[SelectInfo.StartItemNo].StyleNo;
                        DoInsertTextEx(aText, ANewPara, ref vAddCount);  // 在前一个后面插入
                    }
                    else  // 最前或前一个还是RectItem
                    {
                        if (aText == "" && (!vItem.ParaFirst))
                        {
                            UndoAction_ItemParaFirst(SelectInfo.StartItemNo, 0, true);
                            vItem.ParaFirst = true;
                        }
                        else
                        {
                            vItem = CreateDefaultTextItem();
                            vItem.Text = aText;

                            if (ANewPara || (aText == ""))
                                vItem.ParaFirst = true;

                            if (Items[SelectInfo.StartItemNo].ParaFirst)  // 原位置是段首
                            {
                                UndoAction_ItemParaFirst(SelectInfo.StartItemNo, 0, false);
                                Items[SelectInfo.StartItemNo].ParaFirst = false;
                                vItem.ParaFirst = true;
                            }

                            Items.Insert(SelectInfo.StartItemNo, vItem);  // 在两个RectItem中间插入
                            UndoAction_InsertItem(SelectInfo.StartItemNo, 0);
                            vAddCount++;

                            SelectInfo.StartItemOffset = vItem.Length;
                            CurStyleNo = vItem.StyleNo;
                        }
                    }
                }
            }
            else  // 当前位置是TextItem
                DoTextItemInsert(aText, ANewPara, ref vAddCount);
        }

        /// <summary> 在光标处插入字符串(可带回车换行符) </summary>
        public bool InsertText(string aText)
        {
            if (!CanEdit())
                return false;

            if (!DeleteSelected())
                return false;

            if (!DoAcceptAction(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, HCAction.actInsertText))
                return false;

            if (!DoInsertTextBefor(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, aText))
                return false;

            bool Result = false;

            Undo_GroupBegin(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            try
            {
                Undo_New();

                int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
                if ((Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null)  // 当前位置是 RectItem)
                    && (SelectInfo.StartItemOffset == HC.OffsetInner))
                {
                    HCCustomRectItem vRectItem = Items[SelectInfo.StartItemNo] as HCCustomRectItem;
                    if (vRectItem.MangerUndo)
                        UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                    else
                        UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                    Result = vRectItem.InsertText(aText);
                    if (vRectItem.IsFormatDirty)
                    {
                        GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                        FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                        ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                        vRectItem.IsFormatDirty = false;
                    }
                    else
                        this.FormatInit();
                }
                else  // 当前位置是TextItem
                {
                    bool vNewPara = false;
                    int vAddCount = 0;
                    CurStyleNo = MatchTextStyleNoAt(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);  // 防止静默移动选中位置没有更新当前样式
                    GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                    string[] vStrings = aText.Split(new string[] { HC.sLineBreak }, StringSplitOptions.None);
                    string vS;
                    for (int i = 0; i < vStrings.Length; i++)
                    {
                        vS = vStrings[i];
                        DoInsertTextEx(vS, vNewPara, ref vAddCount);
                        vNewPara = true;
                    }

                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + vAddCount, vAddCount);
                    Result = true;
                }

                ReSetSelectAndCaret(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
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

            HCRichData vTopData = GetTopLevelData() as HCRichData;
            HCTableItem vItem = new HCTableItem(vTopData, aRowCount, aColCount, vTopData.Width);
            Result = InsertItem(vItem);
            InitializeMouseField();

            return Result;
        }

        public bool InsertImage(Bitmap aImage)
        {
            if (!CanEdit())
                return false;

            bool Result = false;
            HCRichData vTopData = this.GetTopLevelData() as HCRichData;
            HCImageItem vImageItem = new HCImageItem(vTopData);
            vImageItem.Image = aImage;
            vImageItem.Width = aImage.Width;
            vImageItem.Height = aImage.Height;
            vImageItem.RestrainSize(vTopData.Width, vImageItem.Height);
            Result = InsertItem(vImageItem);
            InitializeMouseField();

            return Result;
        }

        public bool InsertGifImage(string AFile)
        {
            if (!CanEdit())
                return false;

            bool Result = false;
            HCRichData vTopData = this.GetTopLevelData() as HCRichData;
            HCGifItem vGifItem = new HCGifItem(vTopData);
            vGifItem.LoadFromFile(AFile);
            Result = InsertItem(vGifItem);
            InitializeMouseField();

            return Result;
        }

        /// <summary> 在光标处插入直线 </summary>
        public bool InsertLine(int aLineHeight)
        {
            if (!CanEdit())
                return false;

            bool Result = false;

            HCLineItem vItem = new HCLineItem(this, this.Width, aLineHeight);

            Result = InsertItem(vItem);
            InitializeMouseField();

            return Result;
        }

        public bool SetActiveImage(Stream aImageStream)
        {
            if (!CanEdit())
                return false;

            RectItemActionEventHandler vEvent = delegate (HCCustomRectItem aRectItem)
            {
                (aRectItem as HCImageItem).Image = new Bitmap(aImageStream);
                return true;
            };

            return RectItemAction(vEvent);
        }

        public bool ActiveTableResetRowCol(int rowCount, int colCount)
        {
            if (!CanEdit())
                return false;

            RectItemActionEventHandler vEvent = delegate (HCCustomRectItem ARectItem)
            {
                return (ARectItem as HCTableItem).ResetRowCol(this.Width, rowCount, colCount);
            };

            return RectItemAction(vEvent);
        }

        public bool TableInsertRowAfter(byte aRowCount)
        {
            if (!CanEdit())
                return false;

            RectItemActionEventHandler vEvent = delegate(HCCustomRectItem ARectItem)
            {
                return (ARectItem as HCTableItem).InsertRowAfter(aRowCount);
            };

            return RectItemAction(vEvent);
        }

        public bool TableInsertRowBefor(byte aRowCount)
        {
            if (!CanEdit())
                return false;

            RectItemActionEventHandler vEvent = delegate(HCCustomRectItem ARectItem)
            {
                return (ARectItem as HCTableItem).InsertRowBefor(aRowCount);
            };

            return RectItemAction(vEvent);
        }

        public bool ActiveTableDeleteCurRow()
        {
            if (!CanEdit())
                return false;

            RectItemActionEventHandler vEvent = delegate (HCCustomRectItem ARectItem)
            {
                return (ARectItem as HCTableItem).DeleteCurRow();
            };

            return RectItemAction(vEvent);
        }

        public bool ActiveTableSplitCurRow()
        {
            if (!CanEdit())
                return false;

            RectItemActionEventHandler vEvent = delegate(HCCustomRectItem ARectItem)
            {
                return (ARectItem as HCTableItem).SplitCurRow();
            };

            return RectItemAction(vEvent);
        }

        public bool ActiveTableSplitCurCol()
        {
            if (!CanEdit())
                return false;

            RectItemActionEventHandler vEvent = delegate(HCCustomRectItem ARectItem)
            {
                return (ARectItem as HCTableItem).SplitCurCol();
            };

            return RectItemAction(vEvent);
        }

        public bool TableInsertColAfter(byte aColCount)
        {
            if (!CanEdit())
                return false;

            RectItemActionEventHandler vEvent = delegate(HCCustomRectItem ARectItem)
            {
                return (ARectItem as HCTableItem).InsertColAfter(aColCount);
            };

            return RectItemAction(vEvent);
        }

        public bool TableInsertColBefor(byte aColCount)
        {
            if (!CanEdit())
                return false;

            RectItemActionEventHandler vEvent = delegate(HCCustomRectItem ARectItem)
            {
                return (ARectItem as HCTableItem).InsertColBefor(aColCount);
            };

            return RectItemAction(vEvent);
        }

        public bool ActiveTableDeleteCurCol()
        {
            if (!CanEdit())
                return false;

            RectItemActionEventHandler vEvent = delegate (HCCustomRectItem ARectItem)
            {
                return (ARectItem as HCTableItem).DeleteCurCol();
            };

            return RectItemAction(vEvent);
        }

        public bool MergeTableSelectCells()
        {
            if (!CanEdit())
                return false;

            RectItemActionEventHandler vEvent = delegate(HCCustomRectItem ARectItem)
            {
                return (ARectItem as HCTableItem).MergeSelectCells();
            };

            return RectItemAction(vEvent);
        }

        public bool TableApplyContentAlign(HCContentAlign aAlign)
        {
            if (!CanEdit())
                return false;

            RectItemActionEventHandler vEvent = delegate (HCCustomRectItem ARectItem)
            {
                (ARectItem as HCTableItem).ApplyContentAlign(aAlign);
                return true;
            };

            return RectItemAction(vEvent);
        }

        public void ActiveItemReAdaptEnvironment()
        {
            if (!CanEdit())
                return;

            this.InitializeField();
            HCCustomItem vActiveItem = GetActiveItem();
            if (vActiveItem == null)
                return;

            int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
            if ((vActiveItem.StyleNo < HCStyle.Null)  // 当前位置是 RectItem
                && (SelectInfo.StartItemOffset == HC.OffsetInner))  // 在其上输入内容
            {
                Undo_New();

                HCCustomRectItem vRectItem = vActiveItem as HCCustomRectItem;
                if (vRectItem.MangerUndo)
                    UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                else
                    UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                vRectItem.ActiveItemReAdaptEnvironment();
                if (vRectItem.IsFormatDirty)
                {
                    GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);

                    vRectItem.IsFormatDirty = false;
                }
                else
                    this.FormatInit();
            }
            else
            {
                int vExtraCount = 0;
                GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

                Undo_New();

                int vItemNo = SelectInfo.StartItemNo;
                if (MergeItemToNext(vItemNo))
                {
                    UndoAction_InsertText(vItemNo, Items[vItemNo].Length - Items[vItemNo + 1].Length + 1, Items[vItemNo + 1].Text);
                    UndoAction_DeleteItem(vItemNo + 1, 0);
                    Items.Delete(vItemNo + 1);
                    vExtraCount--;
                }
                if (vItemNo > 0)  // 向前合并
                {
                    int vLen = Items[vItemNo - 1].Length;
                    if (MergeItemToPrio(vItemNo))
                    {
                        UndoAction_InsertText(vItemNo - 1, Items[vItemNo - 1].Length - Items[vItemNo].Length + 1, Items[vItemNo].Text);
                        UndoAction_DeleteItem(vItemNo, 0);
                        Items.Delete(vItemNo);
                        vExtraCount--;

                        ReSetSelectAndCaret(SelectInfo.StartItemNo - 1, vLen + SelectInfo.StartItemOffset);
                    }
                }

                if (vExtraCount != 0)
                {
                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + vExtraCount, vExtraCount);
                    Style.UpdateInfoRePaint();
                    Style.UpdateInfoReCaret();
                }
            }
        }

        /// <summary> 取消激活(用于页眉、页脚、正文切换时原激活的取消) </summary>
        public void DisActive()
        {
            this.InitializeField();

            if (Items.Count > 0)
            {
                HCCustomItem vItem = GetActiveItem();
                if (vItem != null)
                    vItem.Active = false;
            }
        }

        public string GetHint()
        {
            if ((!FMouseMoveRestrain) && (FMouseMoveItemNo >= 0))
                return Items[FMouseMoveItemNo].GetHint();
            else
                return "";
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

        public int HotDrawItemNo
        {
            get { return FMouseMoveDrawItemNo; }
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

        public DataItemNoEventHandler OnItemResized
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

        public DrawItemMouseEventHandle OnDrawItemMouseMove
        {
            get { return FOnDrawItemMouseMove; }
            set { FOnDrawItemMouseMove = value; }
        }

        public EventHandler OnCreateItem
        {
            get { return FOnCreateItem; }
            set { FOnCreateItem = value; }
        }

        public DataActionEventHandler OnAcceptAction
        {
            get { return FOnAcceptAction; }
            set { FOnAcceptAction = value; }
        }
    }
}
