/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{             文档撤销、恢复相关类型单元                }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace HC.View
{
    // THCUndo.Data部分，一般由Item自己使用 
    public class HCMirrorUndoData : HCObject
    {
        private MemoryStream FStream;
        public HCMirrorUndoData()
        {
            FStream = new MemoryStream();
        }

        ~HCMirrorUndoData()
        {
            if (FStream != null)
            {
                FStream.Close();
                FStream.Dispose();
            }
        }

        public MemoryStream Stream
        {
            get { return FStream; }
            set { FStream = value; }
        }
    }

    public class HCBaseKeysUndoData : HCObject  // 两个整形值基类
    {
        public int A, B;
    }

    public class HCCellUndoData : HCBaseKeysUndoData  // 单元格内部HCData自己处理
    {
        public int Row
        {
            get { return A; }
            set { A = value; }
        }

        public int Col
        {
            get { return B; }
            set { B = value; }
        }
    }

    public class HCMulCellUndoData : HCCellUndoData { }

    public class HCColSizeUndoData : HCBaseKeysUndoData  // 改变列宽
    {
        private int FCol;
        public int Col
        {
            get { return FCol; }
            set { FCol = value; }
        }

        public int OldWidth
        {
            get { return A; }
            set { A = value; }
        }

        public int NewWidth
        {
            get { return B; }
            set { B = value; }
        }
    }

    public class HCRowSizeUndoData : HCBaseKeysUndoData  // 改变行高
    {
        private int FRow;
        public int Row
        {
            get { return FRow; }
            set { FRow = value; }
        }

        public int OldHeight
        {
            get { return A; }
            set { A = value; }
        }

        public int NewHeight
        {
            get { return B; }
            set { B = value; }
        }
    }

    public class HCSizeUndoData : HCBaseKeysUndoData  // Item尺寸改变(用于RectItem)
    {
        private int FNewWidth, FNewHeight;
        public int OldWidth
        {
            get { return A; }
            set { A = value; }
        }

        public int OldHeight
        {
            get { return B; }
            set { B = value; }
        }

        public int NewWidth
        {
            get { return FNewWidth; }
            set { FNewWidth = value; }
        }

        public int NewHeight
        {
            get { return FNewHeight; }
            set { FNewHeight = value; }
        }
    }

    public enum UndoActionTag : byte
    {
        /// <summary> 向前删除文本 </summary>
        uatDeleteBackText,
        /// <summary> 向后删除文本 </summary>
        uatDeleteText,
        /// <summary> 插入文本 </summary>
        uatInsertText,
        uatSetItemText,    // 直接赋值Item的Text
        uatDeleteItem, 
        uatInsertItem, 
        uatItemProperty, 
        uatItemSelf,
        uatItemMirror
    }

    public class HCCustomUndoAction : Object
    {
        private UndoActionTag FTag;
        private int FItemNo;  // 事件发生时的ItemNo
        private int FOffset;  // 事件发生时的Offset
        private bool FParaFirst;

        public HCCustomUndoAction()
        {
            FItemNo = -1;
            FOffset = -1;
        }

        public int ItemNo
        {
            get { return FItemNo; }
            set { FItemNo = value; }
        }

        public int Offset
        {
            get { return FOffset; }
            set { FOffset = value; }
        }

        public bool ParaFirst
        {
            get { return FParaFirst; }
            set { FParaFirst = value; }
        }

        public UndoActionTag Tag
        {
            get { return FTag; }
            set { FTag = value; }
        }
    }

    public class HCTextUndoAction : HCCustomUndoAction
    {
        private string FText;

        public string Text
        {
            get { return FText; }
            set { FText = value; }
        }
    }

    public class HCSetItemTextUndoAction : HCTextUndoAction
    {
        private string FNewText;

        public string NewText
        {
            get { return FNewText; }
            set { FNewText = value; }
        }
    }

    public enum ItemProperty : byte
    {
        uipStyleNo, uipParaNo, uipParaFirst, uipPageBreak
    }

    public class HCItemPropertyUndoAction : HCCustomUndoAction
    {
        private ItemProperty FItemProperty;
        public HCItemPropertyUndoAction() : base()
        {
            this.Tag = UndoActionTag.uatItemProperty;
        }

        public ItemProperty ItemProperty
        {
            get { return FItemProperty; }
            set { FItemProperty = value; }
        }
    }

    public class HCItemStyleUndoAction : HCItemPropertyUndoAction
    {
        private int FOldStyleNo, FNewStyleNo;
        public HCItemStyleUndoAction() : base()
        {
            ItemProperty = ItemProperty.uipStyleNo;
        }

        public int OldStyleNo
        {
            get { return FOldStyleNo; }
            set { FOldStyleNo = value; }
        }

        public int NewStyleNo
        {
            get { return FNewStyleNo; }
            set { FNewStyleNo = value; }
        }
    }

    public class HCItemParaUndoAction : HCItemPropertyUndoAction
    {
        private int FOldParaNo, FNewParaNo;
        public HCItemParaUndoAction() : base()
        {
            ItemProperty = ItemProperty.uipParaNo;
        }

        public int OldParaNo
        {
            get { return FOldParaNo; }
            set { FOldParaNo = value; }
        }

        public int NewParaNo
        {
            get { return FNewParaNo; }
            set { FNewParaNo = value; }
        }
    }

    public class HCItemParaFirstUndoAction : HCItemPropertyUndoAction
    {
        private bool FOldParaFirst, FNewParaFirst;
        public HCItemParaFirstUndoAction() : base()
        {
            ItemProperty = ItemProperty.uipParaFirst;
        }

        public bool OldParaFirst
        {
            get { return FOldParaFirst; }
            set { FOldParaFirst = value; }
        }

        public bool NewParaFirst
        {
            get { return FNewParaFirst; }
            set { FNewParaFirst = value; }
        }
    }

    public class HCItemPageBreakUndoAction : HCItemPropertyUndoAction
    {
        private bool FOldPageBreak, FNewPageBreak;
        public HCItemPageBreakUndoAction()
            : base()
        {
            ItemProperty = ItemProperty.uipPageBreak;
        }

        public bool OldPageBreak
        {
            get { return FOldPageBreak; }
            set { FOldPageBreak = value; }
        }

        public bool NewPageBreak
        {
            get { return FNewPageBreak; }
            set { FNewPageBreak = value; }
        }
    }

    public class HCItemUndoAction : HCCustomUndoAction
    {
        private MemoryStream FItemStream;
        public HCItemUndoAction() : base()
        {
            FItemStream = new MemoryStream();
        }

        ~HCItemUndoAction()
        {
            FItemStream.Close();
            FItemStream.Dispose();
        }

        public MemoryStream ItemStream
        {
            get { return FItemStream; }
            set { FItemStream = value; }
        }
    }

    public class HCItemSelfUndoAction : HCCustomUndoAction
    {
        private object FObject;
        public HCItemSelfUndoAction() : base()
        {
            this.Tag = UndoActionTag.uatItemSelf;
            FObject = null;
        }

        ~HCItemSelfUndoAction()
        {
            //if (FObject != null)
            //    FObject.Dispose();
        }

        public object Object
        {
            get { return FObject; }
            set { FObject = value; }
        }
    }

    public class HCUndoActions : List<HCCustomUndoAction>
    {
        public HCCustomUndoAction First
        {
            get { return this[0]; }
        }

        public HCCustomUndoAction Last
        {
            get { return this[this.Count - 1]; }
        }
    }

    //Undo部分
    public class HCCustomUndo
    {
        private HCUndoActions FActions;
        private bool FIsUndo;  // 撤销状态

        public HCCustomUndo()
        {
            FIsUndo = true;
            FActions = new HCUndoActions();
        }

        ~HCCustomUndo()
        {
            FActions.Clear();
        }

        public HCUndoActions Actions
        {
            get { return FActions; }
            set { FActions = value; }
        }

        public bool IsUndo
        {
            get { return FIsUndo; }
            set { FIsUndo = value; }
        }
    }

    public delegate HCUndoList GetUndoListEventHandler();

    public class HCUndo : HCCustomUndo
    {
        private HCObject FData;  // 存放各类撤销对象

        public HCUndo() : base()
        {
            FData = null;
        }

        public HCCustomUndoAction ActionAppend(UndoActionTag aTag, int aItemNo, int aOffset, bool aParaFirst)
        {
            HCCustomUndoAction Result = null;
            switch (aTag)
            {
                case UndoActionTag.uatDeleteBackText:
                case UndoActionTag.uatDeleteText:
                case UndoActionTag.uatInsertText:
                    Result = new HCTextUndoAction();
                    break;

                case UndoActionTag.uatSetItemText:
                    Result = new HCSetItemTextUndoAction();
                    break;

                case UndoActionTag.uatDeleteItem:
                case UndoActionTag.uatInsertItem:
                case UndoActionTag.uatItemMirror:
                    Result = new HCItemUndoAction();
                    break;

                //case UndoActionTag.uatItemProperty:
                //    Result = new HCItemParaFirstUndoAction();
                //    break;

                case UndoActionTag.uatItemSelf:
                    Result = new HCItemSelfUndoAction();
                    break;

                default:
                    Result = new HCCustomUndoAction();
                    break;
            }

            Result.Tag = aTag;
            Result.ItemNo = aItemNo;
            Result.Offset = aOffset;
            Result.ParaFirst = aParaFirst;

            this.Actions.Add(Result);

            return Result;
        }

        public HCObject Data
        {
            get { return FData; }
            set { FData = value; }
        }
    }

    public class HCDataUndo : HCUndo
    {
        private int FCaretDrawItemNo;

        public HCDataUndo()
            : base()
        {

        }

        public int CaretDrawItemNo
        {
            get { return FCaretDrawItemNo; }
            set { FCaretDrawItemNo = value; }
        }
    }

    public class HCEditUndo : HCDataUndo
    {
        private int FHScrollPos, FVScrollPos;

        public HCEditUndo() : base()
        {
            FHScrollPos = 0;
            FVScrollPos = 0;
        }

        public int HScrollPos
        {
            get { return FHScrollPos; }
            set { FHScrollPos = value; }
        }

        public int VScrollPos
        {
            get { return FVScrollPos; }
            set { FVScrollPos = value; }
        }
    }

    public class HCSectionUndo : HCEditUndo
    {
        private int FSectionIndex;

        public HCSectionUndo() : base()
        {
            FSectionIndex = -1;
        }

        public int SectionIndex
        {
            get { return FSectionIndex; }
            set { FSectionIndex = value; }
        }
    }

    public class HCUndoGroupBegin : HCDataUndo
    {
        private int FItemNo, FOffset;
        public int ItemNo
        {
            get { return FItemNo; }
            set { FItemNo = value; }
        }

        public int Offset
        {
            get { return FOffset; }
            set { FOffset = value; }
        }
    }

    public class HCUndoEditGroupBegin : HCUndoGroupBegin
    {
        private int FHScrollPos, FVScrollPos;
        public HCUndoEditGroupBegin() : base()
        {
            FHScrollPos = 0;
            FVScrollPos = 0;
        }

        public int HScrollPos
        {
            get { return FHScrollPos; }
            set { FHScrollPos = value; }
        }

        public int VScrollPos
        {
            get { return FVScrollPos; }
            set { FVScrollPos = value; }
        }
    }

    public class HCSectionUndoGroupBegin : HCUndoEditGroupBegin
    {
        private int FSectionIndex;
        public HCSectionUndoGroupBegin() : base()
        {
            FSectionIndex = -1;
        }

        public int SectionIndex
        {
            get { return FSectionIndex; }
            set { FSectionIndex = value; }
        }
    }

    public class HCUndoGroupEnd : HCDataUndo
    {
        private int FItemNo, FOffset;
        public int ItemNo
        {
            get { return FItemNo; }
            set { FItemNo = value; }
        }

        public int Offset
        {
            get { return FOffset; }
            set { FOffset = value; }
        }
    }

    public class HCUndoEditGroupEnd : HCUndoGroupEnd
    {
        private int FHScrollPos, FVScrollPos;
        public HCUndoEditGroupEnd() : base()
        {
            FHScrollPos = 0;
            FVScrollPos = 0;
        }
        public int HScrollPos
        {
            get { return FHScrollPos; }
            set { FHScrollPos = value; }
        }
        public int VScrollPos
        {
            get { return FVScrollPos; }
            set { FVScrollPos = value; }
        }
    }

    public class HCSectionUndoGroupEnd : HCUndoEditGroupEnd
    {
        private int FSectionIndex;
        public HCSectionUndoGroupEnd() : base()
        {
            FSectionIndex = -1;
        }
        public int SectionIndex
        {
            get { return FSectionIndex; }
            set { FSectionIndex = value; }
        }
    }

    // UndoList部分

    public delegate HCUndo UndoNewEventHandler();
    public delegate void UndoEventHandler(HCUndo Sender);
    public delegate HCUndoGroupBegin UndoGroupBeginEventHandler(int AItemNo, int AOffset);
    public delegate HCUndoGroupEnd UndoGroupEndEventHandler(int AItemNo, int AOffset);

    public class HCUndoList : HCList<HCUndo>
    {
        private int FSeek;
        private bool FEnable;  // 是否可以执行撤销恢复
        private Stack FEnableStateStack;
        private bool FGroupWorking;  // 组操作锁
        private uint FMaxUndoCount;  // 撤销恢复链的最大长度

        // 当前组撤销恢复时的组起始和组结束
        private int FGroupBeginIndex, FGroupEndIndex;

        private UndoNewEventHandler FOnUndoNew;
        private UndoGroupBeginEventHandler FOnUndoGroupStart;
        private UndoGroupEndEventHandler FOnUndoGroupEnd;
        private UndoEventHandler FOnUndo, FOnRedo, FOnUndoDestroy;

        private void DoNewUndo(HCUndo aUndo)
        {
            if (FSeek < this.Count - 1)
            {
                if (FSeek > 0)
                {
                    if (this[FSeek].IsUndo)
                        FSeek++;

                    this.RemoveRange(FSeek, this.Count - FSeek);
                }
                else
                    this.Clear();
            }

            if (this.Count > FMaxUndoCount)  // 超出列表最大允许的数量
            {
                int vOver = 0, vIndex = -1;

                if (this[0] is HCUndoGroupBegin)
                {
                    for (int i = 1; i <= this.Count - 1; i++)
                    {
                        if (this[i] is HCUndoGroupEnd)
                        {
                            if (vOver == 0)
                            {
                                vIndex = i;
                                break;
                            }
                            else
                                vOver--;
                        }
                        else
                        {
                            if (this[i] is HCUndoGroupBegin)
                                vOver++;
                        }
                    }

                    this.RemoveRange(0, vIndex + 1);
                }
                else
                {
                    this.RemoveAt(0);
                }
            }

            this.Add(aUndo);
            FSeek = this.Count - 1;
        }

        // List相关的Notify事件
        private ItemNotifyEventHandler FOnItemDelete;

        private void _OnDeleteItem(object sender, NListEventArgs<HCUndo> e)
        {
            if (FOnUndoDestroy != null)
                FOnUndoDestroy(e.Item);
        }

        private void _OnClear(object sender, EventArgs e)
        {
            FSeek = -1;
            FGroupBeginIndex = -1;
            FGroupEndIndex = -1;
        }

        public HCUndoList()
        {
            FEnableStateStack = new Stack();
            FSeek = -1;
            FMaxUndoCount = 99;
            this.OnDelete += new EventHandler<NListEventArgs<HCUndo>>(_OnDeleteItem);
            this.OnClear += new EventHandler<EventArgs>(_OnClear);
            FEnable = true;
            FGroupBeginIndex = -1;
            FGroupEndIndex = -1;
        }

        ~HCUndoList()
        {

        }

        public void UndoGroupBegin(int aItemNo, int aOffset)
        {
            HCUndoGroupBegin vUndoGroupBegin = null;
            if (FOnUndoGroupStart != null)
                vUndoGroupBegin = FOnUndoGroupStart(aItemNo, aOffset);
            else
                vUndoGroupBegin = new HCUndoGroupBegin();

            vUndoGroupBegin.ItemNo = aItemNo;
            vUndoGroupBegin.Offset = aOffset;

            DoNewUndo(vUndoGroupBegin);
        }

        public void UndoGroupEnd(int aItemNo, int aOffset)
        {
            HCUndoGroupEnd vUndoGroupEnd = null;
            if (FOnUndoGroupEnd != null)
                vUndoGroupEnd = FOnUndoGroupEnd(aItemNo, aOffset);
            else
                vUndoGroupEnd = new HCUndoGroupEnd();

            vUndoGroupEnd.ItemNo = aItemNo;
            vUndoGroupEnd.Offset = aOffset;

            DoNewUndo(vUndoGroupEnd);
        }

        public HCUndo UndoNew()
        {
            HCUndo Result = null;
            if (FOnUndoNew != null)
                Result = FOnUndoNew();
            else
                Result = new HCUndo();

            DoNewUndo(Result);
            return Result;
        }

        // Undo 子方法
        private void DoSeekUndoEx(ref int ASeek)
        {
            if (FOnUndo != null)
                FOnUndo(this[ASeek]);

            this[ASeek].IsUndo = false;
            ASeek--;
        }

        public void Undo()
        {
            if (FSeek >= 0)
            {
                int vOver = 0, vBeginIndex = -1;

                if (this[FSeek] is HCUndoGroupEnd)
                {
                    vOver = 0;
                    vBeginIndex = 0;
                    for (int i = FSeek - 1; i >= 0; i--)
                    {
                        if (this[i] is HCUndoGroupBegin)
                        {
                            if (vOver == 0)
                            {
                                vBeginIndex = i;
                                break;
                            }
                            else
                                vOver--;
                        }
                        else
                        {
                            if (this[i] is HCUndoGroupEnd)
                                vOver++;
                        }
                    }

                    FGroupBeginIndex = vBeginIndex;
                    FGroupEndIndex = FSeek;
                    try
                    {
                        FGroupWorking = true;
                        while (FSeek >= vBeginIndex)
                        {
                            if (FSeek == vBeginIndex)
                                FGroupWorking = false;

                            DoSeekUndoEx(ref FSeek);
                        }
                    }
                    finally
                    {
                        FGroupWorking = false;
                        FGroupBeginIndex = -1;
                        FGroupEndIndex = -1;
                    }
                }
                else
                    DoSeekUndoEx(ref FSeek);
            }
        }

        // Redo子方法
        private void DoSeekRedoEx(ref int ASeek)
        {
            ASeek++;
            if (FOnRedo != null)
                FOnRedo(this[ASeek]);

            this[ASeek].IsUndo = true;
        }

        public void Redo()
        {
            if (FSeek < this.Count - 1)
            {
                int vOver = -1, vEndIndex = -1;
                if (this[FSeek + 1] is HCUndoGroupBegin)
                {
                    vOver = 0;
                    vEndIndex = this.Count - 1;

                    // 找结束
                    for (int i = FSeek + 2; i <= this.Count - 1; i++)
                    {
                        if (this[i] is HCUndoGroupEnd)
                        {
                            if (vOver == 0)
                            {
                                vEndIndex = i;
                                break;
                            }
                            else
                                vOver--;
                        }
                        else
                        {
                            if (this[i] is HCUndoGroupBegin)
                                vOver++;
                        }
                    }

                    FGroupBeginIndex = FSeek + 1;
                    FGroupEndIndex = vEndIndex;
                    try
                    {
                        FGroupWorking = true;
                        while (FSeek < vEndIndex)
                        {
                            if (FSeek == vEndIndex - 1)
                                FGroupWorking = false;

                            DoSeekRedoEx(ref FSeek);
                        }
                    }
                    finally
                    {
                        FGroupWorking = false;
                        FGroupBeginIndex = -1;
                        FGroupEndIndex = -1;
                    }
                }
                else
                    DoSeekRedoEx(ref FSeek);
            }
        }

        public void SaveState()
        {
            FEnableStateStack.Push(FEnable);
        }

        public void RestoreState()
        {
            if (FEnableStateStack.Count > 0)
                FEnable = (bool)(FEnableStateStack.Pop());
        }

        public bool Enable
        {
            get { return FEnable; }
            set { FEnable = value; }
        }

        public uint MaxUndoCount
        {
            get { return FMaxUndoCount; }
            set { FMaxUndoCount = value; }
        }

        public int Seek
        {
            get { return FSeek; }
        }

        public bool GroupWorking
        {
            get { return FGroupWorking; }
        }

        public int CurGroupBeginIndex
        {
            get { return FGroupBeginIndex; }
        }

        public int CurGroupEndIndex
        {
            get { return FGroupEndIndex; }
        }

        public UndoNewEventHandler OnUndoNew
        {
            get { return FOnUndoNew; }
            set { FOnUndoNew = value; }
        }

        public UndoGroupBeginEventHandler OnUndoGroupStart
        {
            get { return FOnUndoGroupStart; }
            set { FOnUndoGroupStart = value; }
        }

        public UndoGroupEndEventHandler OnUndoGroupEnd
        {
            get { return FOnUndoGroupEnd; }
            set { FOnUndoGroupEnd = value; }
        }

        public UndoEventHandler OnUndoDestroy
        {
            get { return FOnUndoDestroy; }
            set { FOnUndoDestroy = value; }
        }

        public UndoEventHandler OnUndo
        {
            get { return FOnUndo; }
            set { FOnUndo = value; }
        }

        public UndoEventHandler OnRedo
        {
            get { return FOnRedo; }
            set { FOnRedo = value; }
        }
    }
}
