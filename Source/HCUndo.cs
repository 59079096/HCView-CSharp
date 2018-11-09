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

namespace HC.View
{
    public delegate HCUndoList GetUndoListEventHandler();
    public delegate void UndoEventHandler(HCUndo Sender);

    public class HCUndoMirror : HCObject
    {
        private MemoryStream FStream;

        public HCUndoMirror()
        {

        }

        ~HCUndoMirror()
        {

        }

        public MemoryStream Stream
        {
            get { return FStream; }
            set { FStream = value; }
        }
    }

    public class HCUndoBaseData : HCObject  // 两个整形值基类
    {
        public int A, B;
    }

    public class HCUndoCell : HCUndoBaseData  // 单元格内部HCData自己处理
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

    public class HCUndoColSize : HCUndoBaseData  // 改变列宽
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

    public class HCUndoSize : HCUndoBaseData  // Item尺寸改变(用于RectItem)
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
        uatDeleteText, uatInsertText, uatDeleteItem, uatInsertItem, uatItemProperty, uatItemSelf
    }

    public class HCCustomUndoAction : Object
    {
        private UndoActionTag FTag;

        private int FItemNo;  // 事件发生时的ItemNo

        private int FOffset;  // 事件发生时的Offset

        public HCCustomUndoAction()
        {

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

    public enum ItemProperty : byte
    {
        uipStyleNo, uipParaNo, uipParaFirst
    }

    public class HCItemPropertyUndoAction : HCCustomUndoAction
    {
        private ItemProperty FItemProperty;

        public HCItemPropertyUndoAction()
        {

        }

        public ItemProperty ItemProperty
        {
            get { return FItemProperty; }
            set { FItemProperty = value; }
        }
    }

    public class HCItemParaFirstUndoAction : HCItemPropertyUndoAction
    {
        private bool FOldParaFirst, FNewParaFirst;

        public HCItemParaFirstUndoAction()
        {

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

    public class HCItemUndoAction : HCCustomUndoAction
    {
        private MemoryStream FItemStream;

        public HCItemUndoAction()
        {

        }

        ~HCItemUndoAction()
        {

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

        public HCItemSelfUndoAction()
        {

        }

        ~HCItemSelfUndoAction()
        {

        }

        public object Object
        {
            get { return FObject; }
            set { FObject = value; }
        }
    }

    public class HCUndoActions : List<HCCustomUndoAction>
    {

    }

    public class HCCustomUndo
    {
        private HCUndoActions FActions;

        private bool FIsUndo;

        public HCCustomUndo()
        {

        }

        ~HCCustomUndo()
        {

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

    public class HCUndo : HCCustomUndo
    {
        private int FSectionIndex;

        private HCObject FData;  // 存放差异数据

        public HCUndo()
        {

        }

        public HCCustomUndoAction ActionAppend(UndoActionTag ATag, int AItemNo, int AOffset)
        {
            HCCustomUndoAction Result = null;

            switch (ATag)
            {
                case UndoActionTag.uatDeleteText:
                case UndoActionTag.uatInsertText:
                    Result = new HCTextUndoAction();
                    break;

                case UndoActionTag.uatDeleteItem:
                case UndoActionTag.uatInsertItem:
                    Result = new HCItemUndoAction();
                    break;

                case UndoActionTag.uatItemProperty:
                    Result = new HCItemParaFirstUndoAction();
                    break;

                case UndoActionTag.uatItemSelf:
                    Result = new HCItemSelfUndoAction();
                    break;

                default:
                    Result = new HCCustomUndoAction();
                    break;
            }

            Result.Tag = ATag;
            Result.ItemNo = AItemNo;
            Result.Offset = AOffset;

            this.Actions.Add(Result);

            return Result;
        }

        public int SectionIndex
        {
            get { return FSectionIndex; }
            set { FSectionIndex = value; }
        }

        public HCObject Data
        {
            get { return FData; }
            set { FData = value; }
        }
    }

    public class HCUndoList : HCList<HCUndo>
    {
        private int FSeek;
        private uint FMaxUndoCount;
        private UndoEventHandler FOnUndo, FOnRedo, FOnNewUndo, FOnUndoDestroy;

        private  void DoNewUndo(HCUndo AUndo)
        {

        }

        private ItemNotifyEventHandler FOnItemDelete;

        private void _OnDeleteItem(object sender, NListEventArgs<HCUndo> e)
        {
            if (FOnUndoDestroy != null)
                FOnUndoDestroy(e.Item);
        }

        public HCUndoList()
        {
            FSeek = -1;
            FMaxUndoCount = 99;
            this.OnDelete += new EventHandler<NListEventArgs<HCUndo>>(_OnDeleteItem);
        }

        ~HCUndoList()
        {

        }

        public  void BeginUndoGroup(int AItemNo, int  AOffset)
        {

        }

        public  void EndUndoGroup(int AItemNo, int  AOffset)
        {

        }

        public  void NewUndo()
        {

        }

        public  void Undo()
        {

        }

        public  void Redo()
        {

        }

        public uint MaxUndoCount
        {
            get { return FMaxUndoCount; }
            set { FMaxUndoCount = value; }
        }

        public UndoEventHandler OnNewUndo
        {
            get { return FOnNewUndo; }
            set { FOnNewUndo = value; }
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
