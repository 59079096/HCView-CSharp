/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{             文档撤销、恢复相关操作单元                }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HC.View
{
    public class HCUndoData : HCCustomData  // 支持撤销恢复功能的Data
    {
        private int FFormatFirstItemNo, FFormatLastItemNo, FUndoGroupCount, FItemAddCount;

        private void UndoRedoDeleteBackText(HCCustomUndoAction aAction, bool aIsUndo, ref int aCaretItemNo, ref int aCaretOffset)
        {
            HCTextUndoAction vAction = aAction as HCTextUndoAction;
            aCaretItemNo = vAction.ItemNo;
            int vLen = vAction.Text.Length;
            string vText = Items[vAction.ItemNo].Text;
            if (aIsUndo)
            {
                vText = vText.Insert(vAction.Offset - 1, vAction.Text);
                aCaretOffset = vAction.Offset + vLen - 1;
            }
            else
            {
                vText = vText.Remove(vAction.Offset - 1, vLen);
                aCaretOffset = vAction.Offset - 1;
            }

            Items[vAction.ItemNo].Text = vText;
        }

        private void UndoRedoDeleteText(HCCustomUndoAction aAction, bool aIsUndo, ref int aCaretItemNo, ref int aCaretOffset)
        {
            HCTextUndoAction vAction = aAction as HCTextUndoAction;
            aCaretItemNo = vAction.ItemNo;
            int vLen = vAction.Text.Length;
            string vText = Items[vAction.ItemNo].Text;
            if (aIsUndo)
                vText = vText.Insert(vAction.Offset + 1, vAction.Text);
            else
                vText = vText.Remove(vAction.Offset + 1, vLen);

            aCaretOffset = vAction.Offset - 1;
            Items[vAction.ItemNo].Text = vText;
        }

        private void UndoRedoInsertText(HCCustomUndoAction aAction, bool aIsUndo, ref int aCaretItemNo, ref int aCaretOffset)
        {
            HCTextUndoAction vAction = aAction as HCTextUndoAction;
            aCaretItemNo = vAction.ItemNo;
            string vText = Items[vAction.ItemNo].Text;;
            int vLen = vAction.Text.Length;

            if (aIsUndo)
            {
                vText = vText.Remove(vAction.Offset - 1, vLen);
                aCaretOffset = vAction.Offset - 1;
            }
            else
            {
                vText = vText.Insert(vAction.Offset - 1, vAction.Text);
                aCaretOffset = vAction.Offset + vLen - 1;
            }

            Items[vAction.ItemNo].Text = vText;
        }

        private void UndoRedoDeleteItem(HCCustomUndoAction aAction, bool aIsUndo, ref int aCaretItemNo, ref int aCaretOffset)
        {
            HCItemUndoAction vAction = aAction as HCItemUndoAction;
            aCaretItemNo = vAction.ItemNo;

            if (aIsUndo)  // 撤销
            {
                HCCustomItem vItem = null;
                LoadItemFromStreamAlone(vAction.ItemStream, ref vItem);
                Items.Insert(vAction.ItemNo, vItem);
                FItemAddCount++;

                aCaretOffset = vAction.Offset;
            }
            else  // 重做
            {
                Items.RemoveAt(vAction.ItemNo);
                FItemAddCount--;

                if (aCaretItemNo > 0)
                {
                    aCaretItemNo--;

                    if (Items[aCaretItemNo].StyleNo > HCStyle.Null)
                        aCaretOffset = Items[aCaretItemNo].Length;
                    else
                        aCaretOffset = HC.OffsetAfter;
                }
                else
                    aCaretOffset = 0;
            }
        }

        private void UndoRedoInsertItem(HCCustomUndo aUndo, HCCustomUndoAction aAction, bool aIsUndo, ref int aCaretItemNo, ref int aCaretOffset, ref int aCaretDrawItemNo)
        {
            HCItemUndoAction vAction = aAction as HCItemUndoAction;
            aCaretItemNo = vAction.ItemNo;
            if (aIsUndo)
            {
                Items.RemoveAt(vAction.ItemNo);
                FItemAddCount--;
                if (aCaretItemNo > 0)
                {
                    aCaretItemNo--;
                    if (Items[aCaretItemNo].StyleNo > HCStyle.Null)
                        aCaretOffset = Items[aCaretItemNo].Length;
                    else
                        aCaretOffset = HC.OffsetAfter;
                }
                else
                    aCaretOffset = 0;
            }
            else  // 重做
            {
                HCCustomItem vItem = null;
                LoadItemFromStreamAlone(vAction.ItemStream, ref vItem);
                Items.Insert(vAction.ItemNo, vItem);
                FItemAddCount++;

                aCaretItemNo = vAction.ItemNo;
                if (Items[aCaretItemNo].StyleNo > HCStyle.Null)
                    aCaretOffset = Items[aCaretItemNo].Length;
                else
                    aCaretOffset = HC.OffsetAfter;

                aCaretDrawItemNo = (aUndo as HCDataUndo).CaretDrawItemNo + 1;
            }
        }

        private void UndoRedoItemProperty(HCCustomUndoAction aAction, bool aIsUndo, ref int aCaretItemNo, ref int aCaretOffset)
        {
            HCItemPropertyUndoAction vAction = aAction as HCItemPropertyUndoAction;
            aCaretItemNo = vAction.ItemNo;
            aCaretOffset = vAction.Offset;
            HCCustomItem vItem = Items[vAction.ItemNo];
            switch (vAction.ItemProperty)
            {
                case ItemProperty.uipStyleNo:
                    {
                        if (aIsUndo)
                            vItem.StyleNo = (vAction as HCItemStyleUndoAction).OldStyleNo;
                        else
                            vItem.StyleNo = (vAction as HCItemStyleUndoAction).NewStyleNo;
                    }
                    break;
                
                case ItemProperty.uipParaNo:
                    {
                        if (aIsUndo)
                            vItem.ParaNo = (vAction as HCItemParaUndoAction).OldParaNo;
                        else
                            vItem.ParaNo = (vAction as HCItemParaUndoAction).NewParaNo;
                    }
                    break;
              
                case ItemProperty.uipParaFirst:
                    {
                        if (aIsUndo)
                            vItem.ParaFirst = (vAction as HCItemParaFirstUndoAction).OldParaFirst;
                        else
                            vItem.ParaFirst = (vAction as HCItemParaFirstUndoAction).NewParaFirst;
                    }
                    break;
            }
        }

        private void UndoRedoItemSelf(HCCustomUndoAction aAction, bool aIsUndo, ref int aCaretItemNo, ref int aCaretOffset)
        {
            HCItemSelfUndoAction vAction = aAction as HCItemSelfUndoAction;
            aCaretItemNo = vAction.ItemNo;
            aCaretOffset = vAction.Offset;
            if (aIsUndo)
                Items[aCaretItemNo].Undo(vAction);
            else
                Items[aCaretItemNo].Redo(vAction);
        }

        private void UndoRedoItemMirror(HCCustomUndoAction aAction, bool aIsUndo, ref int aCaretItemNo, ref int aCaretOffset)
        {
            HCItemUndoAction vAction = aAction as HCItemUndoAction;
            aCaretItemNo = vAction.ItemNo;
            aCaretOffset = vAction.Offset;
            HCCustomItem vItem = Items[aCaretItemNo];
            if (aIsUndo)
                LoadItemFromStreamAlone(vAction.ItemStream, ref vItem);
            else
                LoadItemFromStreamAlone(vAction.ItemStream, ref vItem);
        }


        private void DoUndoRedoAction(HCCustomUndo aUndo, HCCustomUndoAction aAction, bool aIsUndo, ref int aCaretItemNo, ref int aCaretOffset, ref int aCaretDrawItemNo)
        {
            switch (aAction.Tag)
            {
                case UndoActionTag.uatDeleteBackText: 
                    UndoRedoDeleteBackText(aAction, aIsUndo, ref aCaretItemNo, ref aCaretOffset);
                    break;

                case UndoActionTag.uatDeleteText: 
                    UndoRedoDeleteText(aAction, aIsUndo, ref aCaretItemNo, ref aCaretOffset);
                    break;

                case UndoActionTag.uatInsertText:
                    UndoRedoInsertText(aAction, aIsUndo, ref aCaretItemNo, ref aCaretOffset);
                    break;

                case UndoActionTag.uatDeleteItem: 
                    UndoRedoDeleteItem(aAction, aIsUndo, ref aCaretItemNo, ref aCaretOffset);
                    break;

                case UndoActionTag.uatInsertItem: 
                    UndoRedoInsertItem(aUndo, aAction, aIsUndo, ref aCaretItemNo, ref aCaretOffset, ref aCaretDrawItemNo);
                    break;

                case UndoActionTag.uatItemProperty: 
                    UndoRedoItemProperty(aAction, aIsUndo, ref aCaretItemNo, ref aCaretOffset);
                    break;

                case UndoActionTag.uatItemSelf:
                    UndoRedoItemSelf(aAction, aIsUndo, ref aCaretItemNo, ref aCaretOffset);
                    break;

                case UndoActionTag.uatItemMirror: 
                    UndoRedoItemMirror(aAction, aIsUndo, ref aCaretItemNo, ref aCaretOffset);
                    break;
            }
        }

        private int GetActionAffect(HCCustomUndoAction aAction, bool aIsUndo)
        {
            int Result = aAction.ItemNo;
            switch (aAction.Tag)
            {
                case UndoActionTag.uatDeleteItem:
                    {
                        if (aIsUndo && (Result > 0))
                            Result--;
                    }
                    break;

                case UndoActionTag.uatInsertItem:
                    {
                        if ((!aIsUndo) && (Result > Items.Count - 1))
                            Result--;
                    }
                    break;

                default:
                    {
                        if (Result > Items.Count - 1)
                            Result = Items.Count - 1;
                    }
                    break;
            }

            return Result;
        }

        private void DoUndoRedo(HCCustomUndo aUndo)
        {
            if (aUndo is HCUndoGroupEnd)
            {
                if (aUndo.IsUndo)
                {
                    if (FUndoGroupCount == 0)
                    {
                        HCUndoList vUndoList = GetUndoList();
                        FFormatFirstItemNo = (vUndoList[vUndoList.CurGroupBeginIndex] as HCUndoGroupBegin).ItemNo;
                        FFormatLastItemNo = (vUndoList[vUndoList.CurGroupEndIndex] as HCUndoGroupEnd).ItemNo;

                        if (FFormatFirstItemNo != FFormatLastItemNo)
                        {
                            FFormatFirstItemNo = GetParaFirstItemNo(FFormatFirstItemNo);
                            FFormatLastItemNo = GetParaLastItemNo(FFormatLastItemNo);
                        }
                        else
                            GetReformatItemRange(ref FFormatFirstItemNo, ref FFormatLastItemNo, FFormatFirstItemNo, 0);

                        _FormatItemPrepare(FFormatFirstItemNo, FFormatLastItemNo);

                        SelectInfo.Initialize();
                        this.InitializeField();
                        FItemAddCount = 0;
                    }

                    FUndoGroupCount++;
                }
                else  // 组恢复结束
                {
                    FUndoGroupCount--;

                    if (FUndoGroupCount == 0)
                    {
                        _ReFormatData(FFormatFirstItemNo, FFormatLastItemNo + FItemAddCount, FItemAddCount);

                        SelectInfo.StartItemNo = (aUndo as HCUndoGroupEnd).ItemNo;
                        SelectInfo.StartItemOffset = (aUndo as HCUndoGroupEnd).Offset;
                        CaretDrawItemNo = (aUndo as HCUndoGroupEnd).CaretDrawItemNo;

                        Style.UpdateInfoReCaret();
                        Style.UpdateInfoRePaint();
                    }
                }

                return;
            }
            else
            if (aUndo is HCUndoGroupBegin)
            {
                if (aUndo.IsUndo)
                {
                    FUndoGroupCount--;

                    if (FUndoGroupCount == 0)
                    {
                        _ReFormatData(FFormatFirstItemNo, FFormatLastItemNo + FItemAddCount, FItemAddCount);

                        SelectInfo.StartItemNo = (aUndo as HCUndoGroupBegin).ItemNo;
                        SelectInfo.StartItemOffset = (aUndo as HCUndoGroupBegin).Offset;
                        CaretDrawItemNo = (aUndo as HCUndoGroupBegin).CaretDrawItemNo;

                        Style.UpdateInfoReCaret();
                        Style.UpdateInfoRePaint();
                    }
                }
                else
                {
                    if (FUndoGroupCount == 0)
                    {
                        FFormatFirstItemNo = (aUndo as HCUndoGroupBegin).ItemNo;
                        FFormatLastItemNo = FFormatFirstItemNo;
                        _FormatItemPrepare(FFormatFirstItemNo, FFormatLastItemNo);

                        SelectInfo.Initialize();
                        this.InitializeField();
                        FItemAddCount = 0;
                    }

                    FUndoGroupCount++;
                }

                return;
            }

            if (aUndo.Actions.Count == 0)
                return;

            int vCaretDrawItemNo = -1, vCaretItemNo = -1, vCaretOffset = -1; ;

            if (FUndoGroupCount == 0)
            {
                SelectInfo.Initialize();
                this.InitializeField();
                FItemAddCount = 0;
                vCaretDrawItemNo = (aUndo as HCDataUndo).CaretDrawItemNo;

                if (aUndo.Actions[0].ItemNo > aUndo.Actions[aUndo.Actions.Count - 1].ItemNo)
                {
                    FFormatFirstItemNo = GetParaFirstItemNo(GetActionAffect(aUndo.Actions[aUndo.Actions.Count - 1], aUndo.IsUndo));
                    FFormatLastItemNo = GetParaLastItemNo(GetActionAffect(aUndo.Actions[0], aUndo.IsUndo));
                }
                else
                {
                    FFormatFirstItemNo = GetParaFirstItemNo(GetActionAffect(aUndo.Actions[0], aUndo.IsUndo));
                    FFormatLastItemNo = GetParaLastItemNo(GetActionAffect(aUndo.Actions[aUndo.Actions.Count - 1], aUndo.IsUndo));
                }

                _FormatItemPrepare(FFormatFirstItemNo, FFormatLastItemNo);
            }

            if (aUndo.IsUndo)
            {
                for (int i = aUndo.Actions.Count - 1; i >= 0; i--)
                    DoUndoRedoAction(aUndo, aUndo.Actions[i], true, ref vCaretItemNo, ref vCaretOffset, ref vCaretDrawItemNo);
            }
            else
            {
                for (int i = 0; i <= aUndo.Actions.Count - 1; i++)
                    DoUndoRedoAction(aUndo, aUndo.Actions[i], false, ref vCaretItemNo, ref vCaretOffset, ref vCaretDrawItemNo);
            }

            if (FUndoGroupCount == 0)
            {
                _ReFormatData(FFormatFirstItemNo, FFormatLastItemNo + FItemAddCount, FItemAddCount);

                if (vCaretDrawItemNo < 0)
                    vCaretDrawItemNo = GetDrawItemNoByOffset(vCaretItemNo, vCaretOffset);
                else
                    if (vCaretDrawItemNo > this.DrawItems.Count - 1)
                        vCaretDrawItemNo = this.DrawItems.Count - 1;

                CaretDrawItemNo = vCaretDrawItemNo;

                Style.UpdateInfoReCaret();
                Style.UpdateInfoRePaint();
            }

            SelectInfo.StartItemNo = vCaretItemNo;
            SelectInfo.StartItemOffset = vCaretOffset;
        }

        protected void SaveItemToStreamAlone(HCCustomItem aItem, Stream aStream)
        {
            HC._SaveFileFormatAndVersion(aStream);
            aItem.SaveToStream(aStream);
            if (aItem.StyleNo > HCStyle.Null)
                Style.TextStyles[aItem.StyleNo].SaveToStream(aStream);

            Style.ParaStyles[aItem.ParaNo].SaveToStream(aStream);
        }

        protected void LoadItemFromStreamAlone(Stream aStream, ref HCCustomItem aItem)
        {
            string vFileExt = "";
            ushort vFileVersion = 0;
            byte vLan = 0;

            aStream.Position = 0;
            HC._LoadFileFormatAndVersion(aStream, ref vFileExt, ref vFileVersion, ref vLan);  // 文件格式和版本
            if ((vFileExt != HC.HC_EXT) && (vFileExt != "cff."))
                throw new Exception("加载失败，不是" + HC.HC_EXT + "文件！");

            int vStyleNo = HCStyle.Null;
            byte[] vBuffer = BitConverter.GetBytes(vStyleNo);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vStyleNo = BitConverter.ToInt32(vBuffer, 0);

            if (aItem == null)
                aItem = CreateItemByStyle(vStyleNo);

            aItem.LoadFromStream(aStream, null, vFileVersion);

            if (vStyleNo > HCStyle.Null)
            {
                HCTextStyle vTextStyle = new HCTextStyle();
                try
                {
                    vTextStyle.LoadFromStream(aStream, vFileVersion);
                    vStyleNo = Style.GetStyleNo(vTextStyle, true);
                    aItem.StyleNo = vStyleNo;
                }
                finally
                {
                    vTextStyle.Dispose();
                }
            }

            int vParaNo = -1;
            HCParaStyle vParaStyle = new HCParaStyle();
            try
            {
                vParaStyle.LoadFromStream(aStream, vFileVersion);
                vParaNo = Style.GetParaNo(vParaStyle, true);
            }
            finally
            {
                vParaStyle.Dispose();
            }

            aItem.ParaNo = vParaNo;
        }

        protected void Undo_New()
        {
            HCUndoList vUndoList = GetUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                HCUndo vUndo = vUndoList.UndoNew();
                if (vUndo is HCDataUndo)
                    (vUndo as HCDataUndo).CaretDrawItemNo = CaretDrawItemNo;
            }
        }

        protected void Undo_GroupBegin(int aItemNo, int aOffset)
        {
            HCUndoList vUndoList = GetUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
                vUndoList.UndoGroupBegin(aItemNo, aOffset);
        }

        protected void Undo_GroupEnd(int aItemNo, int aOffset)
        {
            HCUndoList vUndoList = GetUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
                vUndoList.UndoGroupEnd(aItemNo, aOffset);
        }

        protected void UndoAction_DeleteBackText(int aItemNo, int aOffset, string aText)
        {
            HCUndoList vUndoList = GetUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCTextUndoAction vTextAction = vUndo.ActionAppend(UndoActionTag.uatDeleteBackText, aItemNo, aOffset) as HCTextUndoAction;
                    vTextAction.Text = aText;
                }
            }
        }

        protected void UndoAction_DeleteText(int aItemNo, int aOffset, string aText)
        {
            HCUndoList vUndoList = GetUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCTextUndoAction vTextAction = vUndo.ActionAppend(UndoActionTag.uatDeleteText, aItemNo, aOffset) as HCTextUndoAction;
                    vTextAction.Text = aText;
                }
            }
        }

        protected void UndoAction_InsertText(int aItemNo, int aOffset, string aText)
        {
            HCUndoList vUndoList = GetUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCTextUndoAction vTextAction = vUndo.ActionAppend(UndoActionTag.uatInsertText, aItemNo, aOffset) as HCTextUndoAction;
                    vTextAction.Text = aText;
                }
            }
        }

        protected void UndoAction_DeleteItem(int aItemNo, int aOffset)
        {
            HCUndoList vUndoList = GetUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCItemUndoAction vItemAction = vUndo.ActionAppend(UndoActionTag.uatDeleteItem, aItemNo, aOffset) as HCItemUndoAction;
                    SaveItemToStreamAlone(Items[aItemNo], vItemAction.ItemStream);
                }
            }
        }

        /// <summary> 插入Item到指定位置 </summary>
        /// <param name="AItemNo">操作发生时的ItemNo</param>
        /// <param name="AOffset">操作发生时的Offset</param>
        protected void UndoAction_InsertItem(int aItemNo, int aOffset)
        {
            HCUndoList vUndoList = GetUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCItemUndoAction vItemAction = vUndo.ActionAppend(UndoActionTag.uatInsertItem, aItemNo, aOffset) as HCItemUndoAction;
                    SaveItemToStreamAlone(Items[aItemNo], vItemAction.ItemStream);
                }
            }
        }

        protected void UndoAction_ItemStyle(int aItemNo, int aOffset, int aNewStyleNo)
        {
            HCUndoList vUndoList = GetUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCItemStyleUndoAction vItemAction = new HCItemStyleUndoAction();
                    vItemAction.ItemNo = aItemNo;
                    vItemAction.Offset = aOffset;
                    vItemAction.OldStyleNo = Items[aItemNo].StyleNo;
                    vItemAction.NewStyleNo = aNewStyleNo;
                    vUndo.Actions.Add(vItemAction);
                }
            }
        }

        protected void UndoAction_ItemParaFirst(int aItemNo, int aOffset, bool aNewParaFirst)
        {
            HCUndoList vUndoList = GetUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCItemParaFirstUndoAction vItemAction = new HCItemParaFirstUndoAction();
                    vItemAction.ItemNo = aItemNo;
                    vItemAction.Offset = aOffset;
                    vItemAction.OldParaFirst = Items[aItemNo].ParaFirst;
                    vItemAction.NewParaFirst = aNewParaFirst;
                    vUndo.Actions.Add(vItemAction);
                }
            }
        }

        protected void UndoAction_ItemSelf(int aItemNo, int aOffset)
        {
            HCUndoList vUndoList = GetUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                    vUndo.ActionAppend(UndoActionTag.uatItemSelf, aItemNo, aOffset);
            }
        }

        protected void UndoAction_ItemMirror(int aItemNo, int aOffset)
        {
            HCUndoList vUndoList = GetUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCItemUndoAction vItemAction = vUndo.ActionAppend(UndoActionTag.uatItemMirror, aItemNo, aOffset) as HCItemUndoAction;
                    SaveItemToStreamAlone(Items[aItemNo], vItemAction.ItemStream);
                }
            }
        }

        public HCUndoData(HCStyle aStyle) : base(aStyle)
        {
            FUndoGroupCount = 0;
            FItemAddCount = 0;
        }

        public override void Clear()
        {
            if (Items.Count > 0)
            {
                HCUndoList vUndoList = GetUndoList();
                if ((vUndoList != null) && vUndoList.Enable)
                {
                    Undo_New();
                    for (int i = Items.Count - 1; i >= 0; i--)
                        UndoAction_DeleteItem(i, 0);
                }
            }

            base.Clear();
        }

        public virtual void Undo(HCCustomUndo aUndo)
        {
            DoUndoRedo(aUndo);
        }

        public virtual void Redo(HCCustomUndo aRedo)
        {
            DoUndoRedo(aRedo);
        }
    }
}
