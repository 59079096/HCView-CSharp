/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-12-3             }
{                                                       }
{            支持批注功能的文档对象管理单元             }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace HC.View
{
    public class HCDataAnnotate : SelectInfo
    {
        private int FID;
        private string FTitle, FText;

        public HCDataAnnotate()
        {

        }

        ~HCDataAnnotate()
        {

        }

        public void LoadFromStream(Stream aStream, ushort aFileVersion)
        {
            byte[] vBuffer = BitConverter.GetBytes(FID);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FID = BitConverter.ToInt32(vBuffer, 0);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            FID = BitConverter.ToInt32(vBuffer, 0);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            FID = BitConverter.ToInt32(vBuffer, 0);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            FID = BitConverter.ToInt32(vBuffer, 0);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            FID = BitConverter.ToInt32(vBuffer, 0);

            HC.HCLoadTextFromStream(aStream, ref FTitle, aFileVersion);
            HC.HCLoadTextFromStream(aStream, ref FText, aFileVersion);
        }
    }
    public class HCAnnotateInfo : HCDomainInfo
    {
        public HCAnnotateInfo() : base()
        {

        }

        ~HCAnnotateInfo()
        {

        }
    }

    public delegate void DataDrawItemAnnotateEventHandler(HCCustomData aData, int aDrawItemNo,
        RECT aDrawRect, HCAnnotateItem aAnnotateItem);

    public delegate void DataAnnotateEventHandler(HCCustomData aData, HCAnnotateItem aAnnotateItem);

    public class HCAnnotateData : HCRichData
    {
        private uint FNextID, FAnnotateCount;
        private Stack<uint> FIDStrack;
        private HCAnnotateInfo FHotAnnotate, FActiveAnnotate;  // 当前高亮批注、当前激活的批注
        private DataDrawItemAnnotateEventHandler FOnDrawItemAnnotate;
        private DataAnnotateEventHandler FOnInsertAnnotate, FOnRemoveAnnotate;

        protected override void DoLoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.DoLoadFromStream(aStream, aStyle, aFileVersion);

            if (aFileVersion > 22 && aFileVersion < 55)
            {
                ushort vAnnCount = 0;
                byte[] vBuffer = BitConverter.GetBytes(vAnnCount);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vAnnCount = BitConverter.ToUInt16(vBuffer, 0);
                if (vAnnCount > 0)
                {
                    for (int i = 0; i < vAnnCount; i++)
                    {
                        HCDataAnnotate vAnn = new HCDataAnnotate();
                        vAnn.LoadFromStream(aStream, aFileVersion);
                        //FDataAnnotates.Add(vAnn);
                    }
                }
            }

            if (aFileVersion > 54)
            {
                byte[] vBuffer = BitConverter.GetBytes(FNextID);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FNextID = BitConverter.ToUInt32(vBuffer, 0);
            }
            else
                FNextID = 0;
        }

        protected override void DoInsertItem(HCCustomItem aItem)
        {
            if (aItem.StyleNo == HCStyle.Annotate && (aItem as HCAnnotateItem).MarkType == MarkType.cmtBeg)
            {
                FAnnotateCount++;
                if (FOnInsertAnnotate != null)
                    FOnInsertAnnotate(this, aItem as HCAnnotateItem);
            }

            base.DoInsertItem(aItem);
        }

        protected override void DoRemoveItem(HCCustomItem aItem)
        {
            if (aItem.StyleNo == HCStyle.Annotate && (aItem as HCAnnotateItem).MarkType == MarkType.cmtBeg)
            {
                FAnnotateCount--;
                if (FOnRemoveAnnotate != null)
                    FOnRemoveAnnotate(this, aItem as HCAnnotateItem);
            }

            base.DoRemoveItem(aItem);
        }

        protected override bool DoAcceptAction(int aItemNo, int aOffset, HCAction aAction)
        {
            bool vResult = true;
            if (Style.States.Contain(HCState.hosLoading)
                || Style.States.Contain(HCState.hosUndoing)
                || Style.States.Contain(HCState.hosRedoing)
                )
                return vResult;

            if (aAction == HCAction.actDeleteItem)
            {
                if (Items[aItemNo].StyleNo == HCStyle.Annotate)  // 是批注
                    vResult = false;
            }

            if (vResult)
                vResult = base.DoAcceptAction(aItemNo, aOffset, aAction);

            return vResult;
        }

        protected override void DoDrawItemPaintContent(HCCustomData aData, int aItemNo, int aDrawItemNo,
            RECT aDrawRect, RECT aClearRect, string aDrawText,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if ((!aPaintInfo.Print) && (FOnDrawItemAnnotate != null))  // 当前DrawItem是某批注中的一部分
            {
                if (aData.Items[aItemNo].StyleNo == HCStyle.Annotate && ((aData.Items[aItemNo] as HCAnnotateItem).MarkType == MarkType.cmtEnd))
                    FOnDrawItemAnnotate(aData, aDrawItemNo, aDrawRect, aData.Items[aItemNo] as HCAnnotateItem);
            }

            base.DoDrawItemPaintContent(aData, aItemNo, aDrawItemNo, aDrawRect, aClearRect, aDrawText,
                aDataDrawLeft, aDataDrawRight, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        public HCAnnotateData(HCStyle aStyle) : base(aStyle)
        {
            FNextID = 0;
            FIDStrack = new Stack<uint>();
            FAnnotateCount = 0;
            FHotAnnotate = new HCAnnotateInfo();
            FHotAnnotate.Data = this;
            FActiveAnnotate = new HCAnnotateInfo();
            FActiveAnnotate.Data = this;
        }

        ~HCAnnotateData()
        {

        }

        public override void GetCaretInfo(int aItemNo, int aOffset, ref HCCaretInfo aCaretInfo)
        {
            if (FAnnotateCount > 0 && this.SelectInfo.StartItemNo >= 0)
            {
                HCCustomData vTopData = GetTopLevelData();
                if (vTopData == this)
                    GetAnnotateFrom(SelectInfo.StartItemNo, SelectInfo.StartItemOffset, FActiveAnnotate);
            }

            base.GetCaretInfo(aItemNo, aOffset, ref aCaretInfo);
        }

        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);
            if (FAnnotateCount > 0)
                this.GetAnnotateFrom(this.MouseMoveItemNo, this.MouseMoveItemOffset, FHotAnnotate);
            else
                FHotAnnotate.Clear();

            HCAnnotateData vTopData = this.GetTopLevelDataAt(e.X, e.Y) as HCAnnotateData;
            if (vTopData == this || vTopData.HotAnnotate.BeginNo < 0)
            {
                if (FHotAnnotate.BeginNo >= 0)
                    Style.UpdateInfoRePaint();
            }
        }

        public override void InitializeField()
        {
            base.InitializeField();
            if (FHotAnnotate != null)
                FHotAnnotate.Clear();

            if (FActiveAnnotate != null)
                FActiveAnnotate.Clear();
        }

        public override void SaveToStream(Stream aStream)
        {
            base.SaveToStream(aStream);
            byte[] vBuffer = BitConverter.GetBytes(FNextID);
            aStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public int GetAnnotateBeginBefor(int itemNo, uint AID)
        {
            for (int i = itemNo; i >= 0; i--)
            {
                if (Items[i].StyleNo == HCStyle.Annotate)
                {
                    if ((Items[i] as HCAnnotateItem).MarkType == MarkType.cmtBeg
                        && (Items[i] as HCAnnotateItem).ID == AID)
                        return i;
                }
            }

            return -1;
        }

        public int GetAnnotateEndAfter(int itemNo, uint AID)
        {
            for (int i = itemNo; i < Items.Count; i++)
            {
                if (Items[i].StyleNo == HCStyle.Annotate)
                {
                    if ((Items[i] as HCAnnotateItem).MarkType == MarkType.cmtEnd
                        && (Items[i] as HCAnnotateItem).ID == AID)
                        return i;
                }
            }

            return -1;
        }

        public void GetAnnotateFrom(int itemNo, int offset, HCAnnotateInfo annotateInfo)
        {
            annotateInfo.Clear();

            if (itemNo < 0 || offset < 0)
                return;

            HCAnnotateItem vAnnotateItem = null;
            uint vID = 0;
            if (Items[itemNo].StyleNo == HCStyle.Annotate)  // 起始位置就是批注
            {
                vAnnotateItem = Items[itemNo] as HCAnnotateItem;
                if (vAnnotateItem.MarkType == MarkType.cmtBeg)  // 起始位置是起始标记
                {
                    if (offset == HC.OffsetAfter)  // 光标在起始标记后面
                    {
                        annotateInfo.Data = this;
                        annotateInfo.BeginNo = itemNo;  // 当前即为起始标识
                        vID = vAnnotateItem.ID;
                        annotateInfo.EndNo = GetAnnotateEndAfter(itemNo + 1, vID);  // 这里+1是批注不会出现不配对的前提下
                        return;
                    }
                }
                else  // 起始位置是结束标记
                {
                    if (offset == HC.OffsetBefor)  // 光标在结束标记前面
                    {
                        annotateInfo.Data = this;
                        annotateInfo.EndNo = itemNo;
                        vID = vAnnotateItem.ID;
                        annotateInfo.BeginNo = GetAnnotateBeginBefor(itemNo - 1, vID);  // 这里-1是批注不会出现不配对的前提下
                        return;
                    }
                }
            }

            FIDStrack.Clear();
            for (int i = itemNo; i >= 0; i--)
            {
                if (Items[i].StyleNo == HCStyle.Annotate)
                {
                    vAnnotateItem = Items[i] as HCAnnotateItem;
                    if (vAnnotateItem.MarkType == MarkType.cmtEnd)  // 是结束
                        FIDStrack.Push(vAnnotateItem.ID);
                    else  // 是起始
                    {
                        if (FIDStrack.Count > 0)  // 有栈
                        {
                            vID = FIDStrack.Peek();
                            if (vAnnotateItem.ID == vID)  // 消除配对
                                FIDStrack.Pop();
                            else
                            {
                                annotateInfo.Data = this;
                                annotateInfo.BeginNo = i;
                                vID = vAnnotateItem.ID;
                                break;
                            }
                        }
                        else  // 没有栈
                        {
                            annotateInfo.Data = this;
                            annotateInfo.BeginNo = i;
                            vID = vAnnotateItem.ID;
                            break;
                        }
                    }
                }
            }

            if (annotateInfo.BeginNo >= 0)
                annotateInfo.EndNo = GetAnnotateEndAfter(itemNo + 1, vID);
        }

        private int InsertAnnotateByOffset(HCAnnotateItem annotateItem, int itemNo, int offset)
        {
            int vResult = 0;

            if (offset == HC.OffsetBefor)  // 在最开始
            {
                annotateItem.ParaFirst = Items[itemNo].ParaFirst;
                annotateItem.PageBreak = Items[itemNo].PageBreak;
                if (Items[itemNo].StyleNo > HCStyle.Null && Items[itemNo].Text == "")  // 空item
                {
                    UndoAction_DeleteItem(itemNo, 0);
                    Items.Delete(itemNo);
                    vResult--;
                }
                else
                {
                    if (Items[itemNo].ParaFirst)
                    {
                        UndoAction_ItemParaFirst(itemNo, 0, false);
                        Items[itemNo].ParaFirst = false;
                    }

                    if (Items[itemNo].PageBreak)
                    {
                        UndoAction_ItemPageBreak(itemNo, 0, false);
                        Items[itemNo].PageBreak = false;
                    }
                }

                Items.Insert(itemNo, annotateItem);
                UndoAction_InsertItem(itemNo, HC.OffsetBefor);
                vResult++;
            }
            else
            if (offset == GetItemOffsetAfter(itemNo))  // 在最后面
            {
                Items.Insert(itemNo + 1, annotateItem);
                UndoAction_InsertItem(itemNo + 1, HC.OffsetBefor);
                vResult++;
            }
            else  // 在item中间
            if (IsRectItem(itemNo))  // 在RectItem中间按最后面算
            {
                Items.Insert(itemNo + 1, annotateItem);
                UndoAction_InsertItem(itemNo + 1, HC.OffsetBefor);
                vResult++;
            }
            else  // 在结束TextItem中间打断
            {
                string vS = (Items[itemNo] as HCTextItem).SubString(offset + 1, Items[itemNo].Length - offset);
                UndoAction_DeleteText(itemNo, offset + 1, vS);
                // 原位置后半部分
                HCCustomItem vAfterItem = Items[itemNo].BreakByOffset(offset);
                // 插入原TextItem后半部分增加Text后的
                Style.States.Include(HCState.hosInsertBreakItem);  // 最后一个是原文在插入前就存在的内容，为插入事件提供更精细化的处理
                try
                {
                    Items.Insert(itemNo + 1, vAfterItem);
                }
                finally
                {
                    Style.States.Exclude(HCState.hosInsertBreakItem);
                }

                UndoAction_InsertItem(itemNo + 1, 0);
                vResult++;

                Items.Insert(itemNo + 1, annotateItem);
                UndoAction_InsertItem(itemNo + 1, HC.OffsetBefor);
                vResult++;
            }

            return vResult;
        }

        public bool InsertAnnotate(string aTitle, string aText)
        {
            bool vResult = false;
            if (Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null && SelectInfo.StartItemOffset == HC.OffsetInner)
            {
                Undo_New();

                HCCustomRectItem vRectItem = Items[SelectInfo.StartItemNo] as HCCustomRectItem;
                if (vRectItem.MangerUndo)
                    UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                else
                    UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                vResult = (Items[SelectInfo.StartItemNo] as HCCustomRectItem).InsertAnnotate(aTitle, aText);

                int vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
                if (vRectItem.IsFormatDirty)
                {
                    GetFormatRange(ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);
                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo);
                }
                else
                    this.FormatInit();

                return vResult;
            }
            else
            {

                if (!CanEdit())
                    return false;

                this.Style.States.Include(HCState.hosBatchInsert);  // 批量插入防止插入1个起始触发取配对时，结束还没插入呢
                try
                {
                    int vIncCount = 0, vFormatFirstDrawItemNo = -1, vFormatLastItemNo = -1;
                    Undo_New();

                    if (this.SelectExists())
                    {
                        vFormatFirstDrawItemNo = GetFormatFirstDrawItem(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                        vFormatLastItemNo = GetParaLastItemNo(SelectInfo.EndItemNo);
                    }
                    else
                        GetFormatRange(SelectInfo.StartItemNo, 1, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);

                    FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);
                    FNextID++;

                    // 插入尾
                    HCAnnotateItem vAnnotateItem = new HCAnnotateItem(this);
                    vAnnotateItem.MarkType = MarkType.cmtEnd;
                    vAnnotateItem.ID = FNextID;
                    vAnnotateItem.Content.Title = aTitle;
                    vAnnotateItem.Content.Text = aText;
                    if (SelectInfo.EndItemNo >= 0) // 有选中结束item
                        vIncCount = InsertAnnotateByOffset(vAnnotateItem, SelectInfo.EndItemNo, SelectInfo.EndItemOffset);
                    else
                        vIncCount = InsertAnnotateByOffset(vAnnotateItem, SelectInfo.StartItemNo, SelectInfo.StartItemOffset);

                    // 插入头
                    vAnnotateItem = new HCAnnotateItem(this);
                    vAnnotateItem.MarkType = MarkType.cmtBeg;
                    vAnnotateItem.ID = FNextID;
                    vIncCount = vIncCount + InsertAnnotateByOffset(vAnnotateItem, SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
                    ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo + vIncCount, vIncCount);
                }
                finally
                {
                    this.Style.States.Exclude(HCState.hosBatchInsert);
                }

                Style.UpdateInfoRePaint();
                Style.UpdateInfoReCaret();
                // 因上面批量屏蔽了取当前位置域的功能，所以都插完后，取一下当前位置域情况
                ReSetSelectAndCaret(SelectInfo.StartItemNo);
                return true;
            }
        }

        public bool DeleteAnnotate(int beginNo, int endNo, bool keepPara = true)
        {
            if (!CanEdit())
                return false;

            if (endNo < beginNo)
                return false;

            this.InitializeField();

            int vFormatFirstDrawItemNo = -1, vFormatDrawItemNo2 = -1, vFormatLastItemNo = -1;
            GetFormatRange(beginNo, 0, ref vFormatFirstDrawItemNo, ref vFormatLastItemNo);  // 头
            if ((!keepPara) && (endNo < Items.Count - 1) && (Items[endNo + 1].ParaFirst))
                GetFormatRange(endNo + 1, GetItemOffsetAfter(endNo + 1), ref vFormatDrawItemNo2, ref vFormatLastItemNo);
            else
                GetFormatRange(endNo, GetItemOffsetAfter(endNo), ref vFormatDrawItemNo2, ref vFormatLastItemNo);  // 尾

            if (Items[beginNo].ParaFirst && vFormatFirstDrawItemNo > 0)  // 段首删除时要从上一段最后
            {
                vFormatFirstDrawItemNo--;
                vFormatFirstDrawItemNo = GetFormatFirstDrawItem(vFormatFirstDrawItemNo);  // 从行首开始
            }

            FormatPrepare(vFormatFirstDrawItemNo, vFormatLastItemNo);

            bool vStartParaFirst = Items[beginNo].ParaFirst;
            int vDelCount = 2;

            Undo_New();

            UndoAction_DeleteItem(endNo, 0);
            Items.Delete(endNo);
            UndoAction_DeleteItem(beginNo, 0);
            Items.Delete(beginNo);

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
                if (beginNo < Items.Count - 1 && !Items[beginNo].ParaFirst)  // 下一个不是段首(同段还有内容，置首)
                {
                    UndoAction_ItemParaFirst(beginNo, 0, true);
                    Items[beginNo].ParaFirst = true;
                }
                else  // 段删除完了
                if (keepPara)  // 保持段
                {
                    HCCustomItem vItem = CreateDefaultTextItem();
                    this.CurStyleNo = vItem.StyleNo;
                    vItem.ParaFirst = true;
                    Items.Insert(beginNo, vItem);
                    vDelCount--;
                    UndoAction_InsertItem(beginNo, 0);
                }
            }

            ReFormatData(vFormatFirstDrawItemNo, vFormatLastItemNo - vDelCount, -vDelCount);

            Style.UpdateInfoRePaint();
            Style.UpdateInfoReCaret();

            if (vStartParaFirst && keepPara)
                ReSetSelectAndCaret(beginNo, 0);
            else
            if (beginNo > 0)  // 不是从第1个开始删除
                ReSetSelectAndCaret(beginNo - 1);
            else  // 从第一个开始删除
                ReSetSelectAndCaret(0, 0);  // 光标置到现在的最前面，为其后插入内容做准备

            return true;
        }

        public bool DeleteActiveAnnotate()
        {
            if (SelectExists())
                return false;

            bool vResult = false;

            if (FActiveAnnotate.BeginNo >= 0)
                vResult = DeleteAnnotate(FActiveAnnotate.BeginNo, FActiveAnnotate.EndNo);
            else
            if (Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null
                && SelectInfo.StartItemOffset == HC.OffsetInner)
            {
                Undo_New();

                HCCustomRectItem vRectItem = Items[SelectInfo.StartItemNo] as HCCustomRectItem;
                if (vRectItem.MangerUndo)
                    UndoAction_ItemSelf(SelectInfo.StartItemNo, HC.OffsetInner);
                else
                    UndoAction_ItemMirror(SelectInfo.StartItemNo, HC.OffsetInner);

                vResult = (Items[SelectInfo.StartItemNo] as HCCustomRectItem).DeleteActiveAnnotate();
                if (vRectItem.IsFormatDirty)
                {
                    int vFirstDrawItemNo = -1, vLastItemNo = -1;
                    GetFormatRange(ref vFirstDrawItemNo, ref vLastItemNo);
                    FormatPrepare(vFirstDrawItemNo, vLastItemNo);
                    ReFormatData(vFirstDrawItemNo, vLastItemNo);
                }
                else
                    this.FormatInit();
            }

            return vResult;
        }

        public HCAnnotateInfo HotAnnotate
        {
            get { return FHotAnnotate; }
        }

        public HCAnnotateInfo ActiveAnnotate
        {
            get { return FActiveAnnotate; }
        }

        public DataDrawItemAnnotateEventHandler OnDrawItemAnnotate
        {
            get { return FOnDrawItemAnnotate;}
            set { FOnDrawItemAnnotate = value; }
        }

        public DataAnnotateEventHandler OnInsertAnnotate
        {
            get { return FOnInsertAnnotate; }
            set { FOnInsertAnnotate = value; }
        }

        public DataAnnotateEventHandler OnRemoveAnnotate
        {
            get { return FOnRemoveAnnotate; }
            set { FOnRemoveAnnotate = value; }
        }
    }
}
