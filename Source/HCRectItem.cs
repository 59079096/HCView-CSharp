/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{            文档RectItem对象基类实现单元               }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Drawing;
using HC.Win32;

namespace HC.View
{
    public class HCCustomRectItem : HCCustomItem
    {
        private int FWidth, FHeight;
        bool FTextWrapping;  // 文本环绕
        HCCustomData FOwnerData;
        // 标识内部高度是否发生了变化，用于此Item内部格式化时给其所属的Data标识需要重新格式化此Item
        // 如表格的一个单元格内容变化在没有引起表格整体变化时，不需要重新格式化表格，也不需要重新计算页数
        // 由拥有此Item的Data使用完后应该立即赋值为False，可参考TableItem.KeyPress的使用
        bool FSizeChanged,
            FCanPageBreak;  // 在当前页显示不下时是否可以分页截断显示
        GetUndoListEventHandler FOnGetMainUndoList;

        protected virtual int GetWidth()
        {
            return FWidth;
        }

        protected virtual void SetWidth(int Value)
        {
            FWidth = Value;
        }

        protected virtual int GetHeight()
        {
            return FHeight;
        }

        protected virtual void SetHeight(int Value)
        {
            FHeight = Value;
        }

        // 撤销重做相关方法
        protected virtual void DoNewUndo(HCUndo Sender) { }

        protected virtual void DoUndoDestroy(HCUndo Sender)
        {
            if (Sender.Data != null)
                Sender.Data.Dispose();
        }

        protected virtual void DoUndo(HCUndo Sender)
        {

        }

        protected virtual void DoRedo(HCUndo Sende)
        {

        }

        protected void Undo_StartRecord()
        {
            if (FOwnerData.Style.EnableUndo)
                GetSelfUndoList().NewUndo();
        }

        protected HCUndoList GetSelfUndoList()
        {
            HCUndoList Result = null;
            HCUndoList vMainUndoList = FOnGetMainUndoList();
            int vActionCount = vMainUndoList[vMainUndoList.Count - 1].Actions.Count;
            HCCustomUndoAction vLastAction = vMainUndoList[vMainUndoList.Count - 1].Actions[vActionCount - 1];
            if (vLastAction is HCItemSelfUndoAction)
            {
                HCItemSelfUndoAction vItemAction = vLastAction as HCItemSelfUndoAction;
                if (vItemAction.Object == null)
                {
                    vItemAction.Object = new HCUndoList();
                    (vItemAction.Object as HCUndoList).OnNewUndo = DoNewUndo;
                    (vItemAction.Object as HCUndoList).OnUndo = DoUndo;
                    (vItemAction.Object as HCUndoList).OnRedo = DoRedo;
                }
            
                Result = vItemAction.Object as HCUndoList;
            }

            return Result;
        }

        public HCCustomRectItem() : base()
        {

        }

        /// <summary> 适用于工作期间创建 </summary>
        public HCCustomRectItem(HCCustomData AOwnerData) : this()
        {
            FOwnerData = AOwnerData;
            this.ParaNo = AOwnerData.Style.CurParaNo;
            FWidth = 100;
            FHeight = 50;
            FTextWrapping = false;
            FSizeChanged = false;
            FCanPageBreak = false;
        }

        /// <summary> 适用于加载时创建 </summary>
        public HCCustomRectItem(HCCustomData AOwnerData, int AWidth, int AHeight) : this(AOwnerData)
        {
            Width = AWidth;
            Height = AHeight;
        }

        // 抽象方法，供继承
        public virtual int ApplySelectTextStyle(HCStyle AStyle, HCStyleMatch AMatchStyl)
        {
            return HCStyle.Null;
        }

        public virtual void ApplySelectParaStyle(HCStyle AStyle, HCParaMatch AMatchStyle) { }

        // 当前RectItem格式化时所属的Data(为松耦合请传入TCustomRichData类型)
        public virtual void FormatToDrawItem(HCCustomData ARichData, int AItemNo) { }

        /// <summary> 清除并返回为处理分页比净高增加的高度(为重新格式化时后面计算偏移用) </summary>
        public virtual int ClearFormatExtraHeight()
        {
            return 0;
        }

        public virtual bool DeleteSelected()
        {
            return false;
        }

        public virtual void MarkStyleUsed(bool AMark) { }

        public virtual void SaveSelectToStream(Stream AStream) { }

        public virtual string SaveSelectToText()
        {
            return "";
        }

        public virtual HCCustomItem GetActiveItem()
        {
            return this;
        }

        public virtual HCCustomDrawItem GetActiveDrawItem()
        {
            return null;
        }

        public virtual POINT GetActiveDrawItemCoord()
        {
            return new POINT(0, 0);
        }

        /// <summary> 获取指定X位置对应的Offset </summary>
        public virtual int GetOffsetAt(int X)
        {
            if (X <= 0)
                return HC.OffsetBefor;
            else
                if (X >= Width)
                    return HC.OffsetAfter;
                else
                    return HC.OffsetInner;
        }

        /// <summary> 获取坐标X、Y是否在选中区域中 </summary>
        public virtual bool CoordInSelect(int X, int Y)
        {
            return false;
        }

        /// <summary> 正在其上时内部是否处理指定的Key和Shif </summary>
        public virtual bool WantKeyDown(KeyEventArgs e)
        {
            return false;
        }

        /// <summary> 分散对齐时是否分间距 </summary>
        public virtual bool JustifySplit()
        {
            return true;
        }

        /// <summary> 更新光标位置 </summary>
        public virtual void GetCaretInfo(ref HCCaretInfo ACaretInfo) { }

        /// <summary> 获取在指定高度内的结束位置处最下端(暂时没用到注释了) </summary>
        /// <param name="AHeight">指定的高度范围</param>
        /// <param name="ADItemMostBottom">最底端DItem的底部位置</param>
        //procedure GetPageFmtBottomInfo(const AHeight: Integer; var ADItemMostBottom: Integer); virtual;

        public virtual void CheckFormatPageBreakBefor()
        {

        }

        /// <summary> 计算格式化后的分页位置 </summary>
        /// <param name="ADrawItemRectTop">对应的DrawItem的Rect.Top往下行间距一半</param>
        /// <param name="ADrawItemRectBottom">对应的DrawItem的Rect.Bottom往上行间距一半</param>
        /// <param name="APageDataFmtTop">页数据Top</param>
        /// <param name="APageDataFmtBottom">页数据Bottom</param>
        /// <param name="AStartSeat">开始计算分页位置</param>
        /// <param name="ABreakSeat">需要分页位置</param>
        /// <param name="AFmtOffset">为了避开分页位置整体向下偏移的高度</param>
        /// <param name="AFmtHeightInc">为了避开分页位置高度增加值</param>
        public virtual void CheckFormatPageBreak(int APageIndex, int ADrawItemRectTop,
            int ADrawItemRectBottom, int APageDataFmtTop, int APageDataFmtBottom, int AStartSeat,
            ref int ABreakSeat, ref int AFmtOffset, ref int AFmtHeightInc)
        {
            ABreakSeat = -1;
            AFmtOffset = 0;
            AFmtHeightInc = 0;

            if (FCanPageBreak)
            {
                ABreakSeat = Height - AStartSeat - (APageDataFmtBottom - ADrawItemRectTop);
                if (ADrawItemRectBottom > APageDataFmtBottom)
                    AFmtHeightInc = APageDataFmtBottom - ADrawItemRectBottom;
            }
            else
            {
                ABreakSeat = 0;
                if (ADrawItemRectBottom > APageDataFmtBottom)
                    AFmtOffset = APageDataFmtBottom - ADrawItemRectTop;
            }
        }

        public virtual bool ChangeNearPageBreak()
        {
            return false;
        }

        public virtual bool InsertItem(HCCustomItem AItem)
        {
            return false;
        }

        public virtual bool InsertText(string AText)
        {
            return false;
        }

        public virtual bool InsertGraphic(Image AGraphic, bool ANewPara)
        {
            return false;
        }

        public virtual bool InsertStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            return false;
        }

        public virtual void KeyDown(KeyEventArgs e)
        {
            e.Handled = true;
        }

        public virtual void KeyPress(ref Char Key)
        {
            Key = (char)0;
        }

        /// <summary> “理论”上全选中(适用于仅RectItem点击选中情况下的选中判断) </summary>
        public virtual bool IsSelectComplateTheory()
        {
            return IsSelectComplate || Active;
        }

        public virtual bool SelectExists()
        {
            return false;
        }

        /// <summary> 当前位置开始查找指定的内容 </summary>
        /// <param name="AKeyword">要查找的关键字</param>
        /// <param name="AForward">True：向前，False：向后</param>
        /// <param name="AMatchCase">True：区分大小写，False：不区分大小写</param>
        /// <returns>True：找到</returns>
        public virtual bool Search(string AKeyword, bool AForward, bool AMatchCase)
        {
            return false;
        }

        /// <summary> 当前RectItem是否有需要处理的Data(为松耦合请返回TCustomRichData类型) </summary>
        public virtual HCCustomData GetActiveData()
        {
            return null;
        }

        /// <summary> 返回指定位置处的顶层Data(为松耦合请返回TCustomRichData类型) </summary>
        public virtual HCCustomData GetTopLevelDataAt(int X, int Y)
        {
            return null;
        }

        public virtual void TraverseItem(HCItemTraverse ATraverse) { }
        //
        public override void MouseDown(MouseEventArgs e)
        {
            this.Active = HC.PtInRect(new RECT(0, 0, FWidth, FHeight), e.X, e.Y);
        }

        public override HCCustomItem BreakByOffset(int AOffset)
        {
            return null;
        }
  
        public override bool CanConcatItems(HCCustomItem AItem)
        {
            return false;
        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            FWidth = (Source as HCCustomRectItem).Width;
            FHeight = (Source as HCCustomRectItem).Height;
        }

        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            byte[] vBuffer = BitConverter.GetBytes(FWidth);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FHeight);
            AStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);
            byte[] vBuffer = BitConverter.GetBytes(FWidth);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FWidth = BitConverter.ToInt32(vBuffer, 0);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FHeight = BitConverter.ToInt32(vBuffer, 0);
        }

        public override int GetLength()
        {
            return 1;
        }

        public int Width
        {
            get { return GetWidth(); }
            set { SetWidth(value); }
        }

        public int Height
        {
            get { return GetHeight(); }
            set { SetHeight(value); }
        }

        public bool TextWrapping  // 文本环绕
        {
            get { return FTextWrapping; }
            set { FTextWrapping = value; }  
        }

        public bool SizeChanged
        {
            get { return FSizeChanged; }
            set { FSizeChanged = value; }
        }

        /// <summary> 在当前页显示不下时是否可以分页截断显示 </summary>
        public bool CanPageBreak
        {
            get { return FCanPageBreak; }
            set { FCanPageBreak = value; }
        }

        public HCCustomData OwnerData
        {
            get { return FOwnerData; }
        }
    }

    public class HCDomainItem : HCCustomRectItem  // 域
    {
        private byte FLevel;
        private MarkType FMarkType;

        public static Type HCDefaultDomainItemClass = typeof(HCDomainItem);

        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, 
            int ADataDrawTop, int ADataDrawBottom, int ADataScreenTop, int ADataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoPaint(AStyle, ADrawRect, ADataDrawTop, ADataDrawBottom, ADataScreenTop, ADataScreenBottom,
                ACanvas, APaintInfo);

            if (!APaintInfo.Print)
            {
                if (FMarkType == View.MarkType.cmtBeg)
                {
                    ACanvas.Pen.BeginUpdate();
                    try
                    {
                        ACanvas.Pen.Style = HCPenStyle.psSolid;
                        ACanvas.Pen.Color = HC.ColorActiveBorder;
                    }
                    finally
                    {
                        ACanvas.Pen.EndUpdate();
                    }
                    ACanvas.MoveTo(ADrawRect.Left + 2, ADrawRect.Top - 1);
                    ACanvas.LineTo(ADrawRect.Left, ADrawRect.Top - 1);
                    ACanvas.LineTo(ADrawRect.Left, ADrawRect.Bottom + 1);
                    ACanvas.LineTo(ADrawRect.Left + 2, ADrawRect.Bottom + 1);
                }
                else
                {
                    ACanvas.Pen.BeginUpdate();
                    try
                    {
                        ACanvas.Pen.Style = HCPenStyle.psSolid;
                        ACanvas.Pen.Color = HC.ColorActiveBorder;
                    }
                    finally
                    {
                        ACanvas.Pen.EndUpdate();
                    }

                    ACanvas.MoveTo(ADrawRect.Right - 2, ADrawRect.Top - 1);
                    ACanvas.LineTo(ADrawRect.Right, ADrawRect.Top - 1);
                    ACanvas.LineTo(ADrawRect.Right, ADrawRect.Bottom + 1);
                    ACanvas.LineTo(ADrawRect.Right - 2, ADrawRect.Bottom + 1);
                }
            }
        }

        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            AStream.WriteByte((byte)FMarkType);
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);
            FMarkType = (MarkType)AStream.ReadByte();
        }

        public HCDomainItem(HCCustomData AOwnerData) : base(AOwnerData)
        {
            this.StyleNo = HCStyle.Domain;
            FLevel = 0;
            Width = 0;
            Height = 10;
        }

        public override int GetOffsetAt(int X)
        {
            if ((X >= 0) && (X <= Width))
            {
                if (FMarkType == View.MarkType.cmtBeg)
                    return HC.OffsetAfter;
                else
                    return HC.OffsetBefor;
            }
            else
                return base.GetOffsetAt(X);
        }

        public override bool JustifySplit()
        {
            return false;
        }

        // 当前RectItem格式化时所属的Data(为松耦合请传入TCustomRichData类型)
        public override void FormatToDrawItem(HCCustomData ARichData, int AItemNo)
        {
            this.Width = 0;
            this.Height = 5;  // 默认大小
            if (this.MarkType == MarkType.cmtBeg)
            {
                if (AItemNo < ARichData.Items.Count - 1)
                {
                    HCCustomItem vItem = ARichData.Items[AItemNo + 1];
                    if ((vItem.StyleNo == this.StyleNo)  // 下一个是组标识)
                        && ((vItem as HCDomainItem).MarkType == MarkType.cmtEnd)) // 下一个是结束标识
                        this.Width = 10;  // 增加宽度以便输入时光标可方便点击
                    else
                    if (vItem.StyleNo > HCStyle.Null)
                    {
                        ARichData.Style.TextStyles[vItem.StyleNo].ApplyStyle(ARichData.Style.DefCanvas);
                        this.Height = ARichData.Style.DefCanvas.TextHeight("H");
                    }
                }
                else
                    this.Width = 10;
            }
            else  // 域结束标识
            {
                HCCustomItem vItem = ARichData.Items[AItemNo - 1];
                if ((vItem.StyleNo == this.StyleNo)
                    && ((vItem as HCDomainItem).MarkType == MarkType.cmtBeg))
                    this.Width = 10;
                else
                if (vItem.StyleNo > HCStyle.Null)
                {
                    ARichData.Style.TextStyles[vItem.StyleNo].ApplyStyle(ARichData.Style.DefCanvas);
                    this.Height = ARichData.Style.DefCanvas.TextHeight("H");
                }
            }
        }

        public MarkType MarkType
        {
            get { return FMarkType; }
            set { FMarkType = value; }
        }

        public byte Level
        {
            get { return FLevel; }
            set { FLevel = value; }
        }
    }

    public class HCTextRectItem : HCCustomRectItem  // 带文本样式的RectItem
    {
        private int FTextStyleNo;

        public override void SaveToStream(Stream AStream, int AStart, int  AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            byte[] vBuffer = BitConverter.GetBytes(FTextStyleNo);
            AStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);
            byte[] vBuffer = BitConverter.GetBytes(FTextStyleNo);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FTextStyleNo = BitConverter.ToInt32(vBuffer, 0);
        }

        protected virtual void SetTextStyleNo(int Value)
        {
            if (FTextStyleNo != Value)
                FTextStyleNo = Value;
        }

        public HCTextRectItem(HCCustomData AOwnerData) : base(AOwnerData)
        {
            if (AOwnerData.Style.CurStyleNo > HCStyle.Null)
                FTextStyleNo = AOwnerData.Style.CurStyleNo;
            else
                FTextStyleNo = 0;
        }

        public HCTextRectItem(HCCustomData AOwnerData, int AWidth, int AHeight) : base(AOwnerData, AWidth, AHeight)
        {

        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            FTextStyleNo = (Source as HCTextRectItem).TextStyleNo;
        }

        public override int GetOffsetAt(int X)
        {
            if (X < Width / 2)
                return HC.OffsetBefor;
            else
                return HC.OffsetAfter;
        }

        public override bool JustifySplit()
        {
            return false;
        }

        public override int ApplySelectTextStyle(HCStyle AStyle, HCStyleMatch AMatchStyle)
        {
            FTextStyleNo = AMatchStyle.GetMatchStyleNo(AStyle, FTextStyleNo);
            return FTextStyleNo;
        }

        public override bool SelectExists()
        {
            return this.Options.Contains(ItemOption.ioSelectComplate);
        }

        public int TextStyleNo
        {
            get { return FTextStyleNo; }
            set { SetTextStyleNo(value); }
        }

    }

    public class HCControlItem : HCTextRectItem
    {
        private bool FAutoSize;
        protected byte FMargin;
        protected int FMinWidth, FMinHeight;

        public HCControlItem(HCCustomData AOwnerData) : base(AOwnerData)
        {
            FAutoSize = true;
            FMargin = 5;
            FMinWidth = 20;
            FMinHeight = 10;
        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            FAutoSize = (Source as HCControlItem).AutoSize;
        }

        public override void SaveToStream(Stream AStream, int AStart, int  AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            byte[] vBuffer = BitConverter.GetBytes(FAutoSize);
            AStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);
            byte[] vBuffer = BitConverter.GetBytes(FAutoSize);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FAutoSize = BitConverter.ToBoolean(vBuffer, 0);
        }

        public bool AutoSize
        {
            get { return FAutoSize; }
            set { FAutoSize = value; }
        }
    }

    public enum GripType : byte 
    {
        gtNone, gtLeftTop, gtRightTop, gtLeftBottom, gtRightBottom, gtLeft, gtTop, gtRight, gtBottom
    }

    public class HCResizeRectItem : HCCustomRectItem  // 可改变大小的RectItem
    {
        private ushort FGripSize;  // 拖动块大小
        private bool FResizing;  // 正在拖动改变大小
        private bool FCanResize;  // 当前是否处于可改变大小状态
        private GripType FResizeGrip;
        private RECT FResizeRect;
        private int FResizeWidth, FResizeHeight;  // 缩放后的宽、高

        private GripType GetGripType(int X, int  Y)
        {
            POINT vPt = new POINT(X, Y);

            if (HC.PtInRect(HC.Bounds(0, 0, GripSize, GripSize), vPt))
                return GripType.gtLeftTop;
            else
            if (HC.PtInRect(HC.Bounds(Width - GripSize, 0, GripSize, GripSize), vPt))
                return GripType.gtRightTop;
            else
            if (HC.PtInRect(HC.Bounds(0, Height - GripSize, GripSize, GripSize), vPt))
                return GripType.gtLeftBottom;
            else
            if (HC.PtInRect(HC.Bounds(Width - GripSize, Height - GripSize, GripSize, GripSize), vPt))
                return GripType.gtRightBottom;
            else
                return GripType.gtNone;
        }

        protected int FResizeX, FResizeY;  // 拖动缩放时起始位置

        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, 
            int ADataDrawTop, int  ADataDrawBottom, int  ADataScreenTop, int  ADataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoPaint(AStyle, ADrawRect, ADataDrawTop, ADataDrawBottom, ADataScreenTop, ADataScreenBottom,
                ACanvas, APaintInfo);

            if ((!APaintInfo.Print) && Active)
            {
                if (Resizing)
                {
                    switch (FResizeGrip)
                    {
                        case GripType.gtLeftTop:
                            FResizeRect = HC.Bounds(ADrawRect.Left + Width - FResizeWidth,
                                ADrawRect.Top + Height - FResizeHeight, FResizeWidth, FResizeHeight);
                            break;

                        case GripType.gtRightTop:
                            FResizeRect = HC.Bounds(ADrawRect.Left,
                                ADrawRect.Top + Height - FResizeHeight, FResizeWidth, FResizeHeight);
                            break;

                        case GripType.gtLeftBottom:
                            FResizeRect = HC.Bounds(ADrawRect.Left + Width - FResizeWidth,
                                ADrawRect.Top, FResizeWidth, FResizeHeight);
                            break;

                        case GripType.gtRightBottom:
                            FResizeRect = HC.Bounds(ADrawRect.Left, ADrawRect.Top, FResizeWidth, FResizeHeight);
                            break;
                    }
                
                APaintInfo.TopItems.Add(this);
            }

            // 绘制缩放拖动提示锚点
            ACanvas.Brush.Color = Color.Gray;
            ACanvas.FillRect(HC.Bounds(ADrawRect.Left, ADrawRect.Top, GripSize, GripSize));
            ACanvas.FillRect(HC.Bounds(ADrawRect.Right - GripSize, ADrawRect.Top, GripSize, GripSize));
            ACanvas.FillRect(HC.Bounds(ADrawRect.Left, ADrawRect.Bottom - GripSize, GripSize, GripSize));
            ACanvas.FillRect(HC.Bounds(ADrawRect.Right - GripSize, ADrawRect.Bottom - GripSize, GripSize, GripSize));
            }
        }

        public override void MouseDown(MouseEventArgs e)
        {
            FResizeGrip = GripType.gtNone;
            base.MouseDown(e);
            if (Active)
            {
                FResizeGrip = GetGripType(e.X, e.Y);
                FResizing = FResizeGrip != GripType.gtNone;

                if (FResizing)
                {
                    FResizeX = e.X;
                    FResizeY = e.Y;
                    FResizeWidth = Width;
                    FResizeHeight = Height;
                }
            }
        }

        // 撤销恢复相关方法
        protected  void Undo_Resize(int ANewWidth, int  ANewHeight)
        {
            if (OwnerData.Style.EnableUndo)
            {
                Undo_StartRecord();
                HCUndoList vUndoList = GetSelfUndoList();
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCUndoSize vUndoSize = new HCUndoSize();
                    vUndoSize.OldWidth = this.Width;
                    vUndoSize.OldHeight = this.Height;
                    vUndoSize.NewWidth = ANewWidth;
                    vUndoSize.NewHeight = ANewHeight;
                    vUndo.Data = vUndoSize;
                }
            }
        }

        protected override void DoUndoDestroy(HCUndo Sender)
        {
            if (Sender.Data is HCUndoSize)
                (Sender.Data as HCUndoSize).Dispose();

            base.DoUndoDestroy(Sender);
        }

        protected override void DoUndo(HCUndo Sender)
        {
            if (Sender.Data is HCUndoSize)
            {
                HCUndoSize vSizeAction = Sender.Data as HCUndoSize;
                this.Width = vSizeAction.OldWidth;
                this.Height = vSizeAction.OldHeight;
            }
            else
                base.DoUndo(Sender);
        }

        protected override void DoRedo(HCUndo Sender)
        {
            if (Sender.Data is HCUndoSize)
            {
                HCUndoSize vSizeAction = Sender.Data as HCUndoSize;
                this.Width = vSizeAction.NewWidth;
                this.Height = vSizeAction.NewHeight;
            }
            else
                base.DoRedo(Sender);
        }

        protected virtual bool GetResizing()
        {
            return FResizing;
        }

        protected virtual void SetResizing(bool Value)
        {
            if (FResizing != Value)
                FResizing = Value;
        }

        protected GripType ResizeGrip
        {
            get { return FResizeGrip; }
        }

        protected RECT ResizeRect
        {
            get { return FResizeRect; }
        }

        public HCResizeRectItem(HCCustomData AOwnerData) : base(AOwnerData)
        {
            FCanResize = true;
            FGripSize = 8;
        }

        public HCResizeRectItem(HCCustomData AOwnerData, int AWidth, int AHeight) : base(AOwnerData, AWidth, AHeight)
        {
            FCanResize = true;
            FGripSize = 8;
        }

        /// <summary> 获取坐标X、Y是否在选中区域中 </summary>
        public override bool CoordInSelect(int X, int  Y)
        {
            return SelectExists() && HC.PtInRect(HC.Bounds(0, 0, Width, Height), X, Y) && (GetGripType(X, Y) == GripType.gtNone);
        }

        public override void PaintTop(HCCanvas ACanvas)
        {
            base.PaintTop(ACanvas);
            ACanvas.Brush.Style = HCBrushStyle.bsClear;
            ACanvas.Rectangle(FResizeRect);
        }

        // 继承THCCustomItem抽象方法
        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);
                HC.GCursor = Cursors.Default;
            if (Active)
            {
                int vW = 0, vH = 0, vTempW = 0, vTempH = 0;
                Single vBL = 0;
                if (FResizing)
                {
                    vBL = (float)Width / Height;
                    vW = e.X - FResizeX;
                    vH = e.Y - FResizeY;

                    // 根据缩放位置在对角线的不同方位计算长宽
                    switch (FResizeGrip)
                    {
                        case GripType.gtLeftTop:
                            vTempW = (int)Math.Round(vH * vBL);
                            vTempH = (int)Math.Round(vW / vBL);
                            if (vTempW > vW)
                                vH = vTempH;
                            else
                                vW = vTempW;
                            
                            FResizeWidth = Width - vW;
                            FResizeHeight = Height - vH;
                            break;
                        
                        case GripType.gtRightTop:
                            vTempW = Math.Abs((int)Math.Round(vH * vBL));
                            vTempH = Math.Abs((int)Math.Round(vW / vBL));
                        
                            if (vW < 0)
                            {
                                if (vH > vTempH)
                                    vH = vTempH;
                                else
                                if (vH > 0)
                                    vW = -vTempW;
                                else
                                    vW = vTempW;
                            }
                            else
                            {
                                if (-vH < vTempH)
                                    vH = -vTempH;
                                else
                                    vW = vTempW;
                            }

                            FResizeWidth = Width + vW;
                            FResizeHeight = Height - vH;
                            break;
                    
                        case GripType.gtLeftBottom:
                            vTempW = Math.Abs((int)Math.Round(vH * vBL));
                            vTempH = Math.Abs((int)Math.Round(vW / vBL));
                            
                            if (vW < 0)
                            {
                                if (vH < vTempH)
                                    vH = vTempH;
                                else  // 对角线下面，横向以纵向为准
                                    vW = -vTempW;
                            }
                            else  // 右侧
                            {
                                if ((vW > vTempW) || (vH > vTempH))
                                {
                                    if (vH < 0)
                                        vW = vTempW;
                                    else
                                        vW = -vTempW;
                                }
                                else  // 对角线上面，纵向以横向为准
                                    vH = -vTempH;
                            }

                            FResizeWidth = Width - vW;
                            FResizeHeight = Height + vH;
                            break;
                    
                        case GripType.gtRightBottom:
                            vTempW = (int)Math.Round(vH * vBL);
                            vTempH = (int)Math.Round(vW / vBL);
                            if (vTempW > vW)
                                vW = vTempW;
                            else
                                vH = vTempH;
                        
                            FResizeWidth = Width + vW;
                            FResizeHeight = Height + vH;
                            break;
                    }
                }
                else  // 非缩放中
                {
                    switch (GetGripType(e.X, e.Y))
                    {
                        case GripType.gtLeftTop:
                        case GripType.gtRightBottom:
                            HC.GCursor = Cursors.SizeNWSE;
                            break;
                        
                        case GripType.gtRightTop:
                        case GripType.gtLeftBottom:
                            HC.GCursor = Cursors.SizeNESW;
                            break;

                        case GripType.gtLeft:
                        case GripType.gtRight:
                            HC.GCursor = Cursors.SizeWE;
                            break;

                        case GripType.gtTop:
                        case GripType.gtBottom:
                            HC.GCursor = Cursors.SizeNS;
                            break;
                    }
                }
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);
            if (FResizing)
            {
                FResizing = false;

                if ((FResizeWidth < 0) || (FResizeHeight < 0))
                    return;

                Undo_Resize(FResizeWidth, FResizeHeight);
                Width = FResizeWidth;
                Height = FResizeHeight;
            }
        }

        public override bool CanDrag()
        {
            return !FResizing;
        }

        /// <summary> 更新光标位置 </summary>
        public override void GetCaretInfo(ref HCCaretInfo ACaretInfo)
        {
            if (this.Active)
                ACaretInfo.Visible = false;
        }

        public override bool SelectExists()
        {
            return IsSelectComplateTheory();
        }

        /// <summary> 约束到指定大小范围内 </summary>
        public virtual void RestrainSize(int AWidth, int  AHeight) { }

        public ushort GripSize
        {
            get { return FGripSize; }
            set { FGripSize = value; }
        }

        public bool Resizing
        {
            get { return GetResizing(); }
            set { SetResizing(value); }
        }

        public int ResizeWidth
        {
            get { return FResizeWidth; }
        }

        public int ResizeHeight
        {
            get { return FResizeHeight; }
        }

        public bool CanResize
        {
            get { return FCanResize; }
            set { FCanResize = value; }
        }
    }

    public class HCAnimateRectItem : HCCustomRectItem  // 动画RectItem
    {
        public override int GetOffsetAt(int X)
        {
            if (X < Width / 2)
                return HC.OffsetBefor;
            else
                return HC.OffsetAfter;
        }
    }
}
