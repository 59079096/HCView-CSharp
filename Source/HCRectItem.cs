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
using System.Xml;

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

        protected bool FMangerUndo;  // 是否自己管理撤销恢复

        protected virtual int GetWidth()
        {
            return FWidth;
        }

        protected virtual void SetWidth(int value)
        {
            FWidth = value;
        }

        protected virtual int GetHeight()
        {
            return FHeight;
        }

        protected virtual void SetHeight(int value)
        {
            FHeight = value;
        }

        protected virtual void DoSizeChanged()
        {
            FormatDirty();
        }

        protected void SetSizeChanged(bool value)
        {
            if (FSizeChanged != value)
            {
                FSizeChanged = value;
                DoSizeChanged();
            }
        }

        protected void SelfUndoListInitializate(HCUndoList aUndoList)
        {
            aUndoList.OnUndoNew = DoSelfUndoNew;
            aUndoList.OnUndo = DoSelfUndo;
            aUndoList.OnRedo = DoSelfRedo;
            aUndoList.OnUndoDestroy = DoSelfUndoDestroy;
        }

        protected void SelfUndo_New()
        {
            HCUndoList vUndoList = GetSelfUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
                vUndoList.UndoNew();
        }

        protected HCUndoList GetSelfUndoList()
        {
            if (FOnGetMainUndoList == null)
                return null;

            HCUndoList Result = FOnGetMainUndoList();

            if ((Result != null) && Result.Enable
                && Result.Count > 0
                && Result.Last.Actions.Count > 0
                && (Result.Last.Actions.Last is HCItemSelfUndoAction))
            {
                HCItemSelfUndoAction vItemAction = Result.Last.Actions.Last as HCItemSelfUndoAction;
                if (vItemAction.Object == null)
                {
                    vItemAction.Object = new HCUndoList();
                    SelfUndoListInitializate(vItemAction.Object as HCUndoList);
                }

                Result = vItemAction.Object as HCUndoList;
            }

            return Result;
        }

        protected virtual void DoSelfUndoDestroy(HCUndo aUndo)
        {
            if (aUndo.Data != null)
            {
                aUndo.Data.Dispose();
                aUndo.Data = null;
            }
        }

        protected virtual HCUndo DoSelfUndoNew()
        {
            return new HCUndo();
        }

        protected virtual void DoSelfUndo(HCUndo aUndo) { }

        protected virtual void DoSelfRedo(HCUndo aRedo) { }

        public HCCustomRectItem()
            : base()
        {

        }

        /// <summary> 适用于工作期间创建 </summary>
        public HCCustomRectItem(HCCustomData aOwnerData)
            : this()
        {
            FOwnerData = aOwnerData;
            this.ParaNo = aOwnerData.CurParaNo;
            FOnGetMainUndoList = (aOwnerData as HCCustomData).OnGetUndoList;
            FWidth = 100;
            FHeight = 50;
            FTextWrapping = false;
            FSizeChanged = false;
            FCanPageBreak = false;
            FMangerUndo = false;
        }

        /// <summary> 适用于加载时创建 </summary>
        public HCCustomRectItem(HCCustomData aOwnerData, int aWidth, int aHeight)
            : this(aOwnerData)
        {
            Width = aWidth;
            Height = aHeight;
        }

        public virtual void ApplySelectParaStyle(HCStyle aStyle, HCParaMatch aMatchStyle) { }

        public virtual void ApplySelectTextStyle(HCStyle aStyle, HCStyleMatch aMatchStyl) { }

        public virtual void ApplyContentAlign(HCContentAlign aAlign) { }

        // 当前RectItem格式化时所属的Data(为松耦合请传入TCustomRichData类型)
        public virtual void FormatToDrawItem(HCCustomData aRichData, int aItemNo) { }

        /// <summary> 清除并返回为处理分页比净高增加的高度(为重新格式化时后面计算偏移用) </summary>
        public virtual int ClearFormatExtraHeight()
        {
            return 0;
        }

        /// <summary> ActiveItem重新适应其环境(供外部直接修改Item属性后重新和其前后Item连接组合) </summary>
        public virtual void ReFormatActiveItem() { }

        public virtual void ActiveItemReAdaptEnvironment() { }

        public virtual bool DeleteSelected()
        {
            return false;
        }

        /// <summary> 删除当前域 </summary>
        public virtual bool DeleteActiveDomain() 
        {
            return false;
        }

        /// <summary> 删除当前Data指定范围内的Item </summary>
        public virtual void DeleteActiveDataItems(int aStartNo, int aEndNo, bool aKeepPara) { }

        /// <summary> 直接设置当前TextItem的Text值 </summary>
        public virtual void SetActiveItemText(string aText) { }

        public virtual void MarkStyleUsed(bool aMark) { }

        public virtual void SaveSelectToStream(Stream AStream) { }

        public virtual string SaveSelectToText()
        {
            return "";
        }

        public virtual HCCustomItem GetActiveItem()
        {
            return this;
        }

        public virtual HCCustomItem GetTopLevelItem()
        {
            return this;
        }

        public virtual HCCustomDrawItem GetTopLevelDrawItem()
        {
            return null;
        }

        public virtual POINT GetTopLevelDrawItemCoord()
        {
            return new POINT(-1, -1);
        }

        public virtual HCCustomDrawItem GetTopLevelRectDrawItem()
        {
            return null;
        }

        public virtual POINT GetTopLevelRectDrawItemCoord()
        {
            return new POINT(-1, -1);
        }

        /// <summary> 获取指定X位置对应的Offset </summary>
        public virtual int GetOffsetAt(int x)
        {
            if (x <= 0)
                return HC.OffsetBefor;
            else
            if (x >= Width)
                return HC.OffsetAfter;
            else
                return HC.OffsetInner;
        }

        /// <summary> 获取坐标X、Y是否在选中区域中 </summary>
        public virtual bool CoordInSelect(int x, int y)
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
        public virtual void GetCaretInfo(ref HCCaretInfo aCaretInfo)
        {
            aCaretInfo.Visible = false;
        }

        public virtual void CheckFormatPageBreakBefor() { }

        /// <summary> 计算格式化后的分页位置 </summary>
        /// <param name="aDrawItemRectTop">对应的DrawItem的Rect.Top往下行间距一半</param>
        /// <param name="aDrawItemRectBottom">对应的DrawItem的Rect.Bottom往上行间距一半</param>
        /// <param name="aPageDataFmtTop">页数据Top</param>
        /// <param name="aPageDataFmtBottom">页数据Bottom</param>
        /// <param name="aStartSeat">开始计算分页位置</param>
        /// <param name="aBreakSeat">需要分页位置</param>
        /// <param name="aFmtOffset">为了避开分页位置整体向下偏移的高度</param>
        /// <param name="aFmtHeightInc">为了避开分页位置高度增加值</param>
        public virtual void CheckFormatPageBreak(int aPageIndex, int aDrawItemRectTop,
            int aDrawItemRectBottom, int aPageDataFmtTop, int aPageDataFmtBottom, int aStartSeat,
            ref int aBreakSeat, ref int aFmtOffset, ref int aFmtHeightInc)
        {
            aBreakSeat = -1;
            aFmtOffset = 0;
            aFmtHeightInc = 0;

            if (FCanPageBreak)
            {
                aBreakSeat = Height - aStartSeat - (aPageDataFmtBottom - aDrawItemRectTop);
                if (aDrawItemRectBottom > aPageDataFmtBottom)
                    aFmtHeightInc = aPageDataFmtBottom - aDrawItemRectBottom;
            }
            else
            {
                aBreakSeat = 0;
                if (aDrawItemRectBottom > aPageDataFmtBottom)
                    aFmtOffset = aPageDataFmtBottom - aDrawItemRectTop;
            }
        }

        public virtual bool InsertItem(HCCustomItem aItem)
        {
            return false;
        }

        public virtual bool InsertText(string aText)
        {
            return false;
        }

        public virtual bool InsertGraphic(Image aGraphic, bool aNewPara)
        {
            return false;
        }

        public virtual bool InsertStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            return false;
        }

        public virtual void KeyDown(KeyEventArgs e)
        {
            e.Handled = true;
        }

        public virtual void KeyPress(ref Char key)
        {
            key = (char)0;
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
        /// <param name="aKeyword">要查找的关键字</param>
        /// <param name="aForward">True：向前，False：向后</param>
        /// <param name="aMatchCase">True：区分大小写，False：不区分大小写</param>
        /// <returns>True：找到</returns>
        public virtual bool Search(string aKeyword, bool aForward, bool aMatchCase)
        {
            return false;
        }

        public virtual void Clear() { }

        public virtual void SilenceChange() { }

        /// <summary> 当前RectItem是否有需要处理的Data(为松耦合请返回TCustomRichData类型) </summary>
        public virtual HCCustomData GetActiveData()
        {
            return null;
        }

        /// <summary> 返回指定位置处的顶层Data(为松耦合请返回TCustomRichData类型) </summary>
        public virtual HCCustomData GetTopLevelDataAt(int x, int y)
        {
            return null;
        }

        public virtual HCCustomData GetTopLevelData()
        {
            return null;
        }

        public virtual void FormatDirty() { }

        public virtual void TraverseItem(HCItemTraverse ATraverse) { }

        public virtual bool SaveToBitmap(ref Bitmap aBitmap) 
        {
            if ((FWidth == 0) || (FHeight == 0))
                return false;

            aBitmap = new Bitmap(FWidth, FHeight);
            PaintInfo vPaintInfo = new PaintInfo();
            vPaintInfo.Print = true;
            vPaintInfo.WindowWidth = aBitmap.Width;
            vPaintInfo.WindowHeight = aBitmap.Height;
            vPaintInfo.ScaleX = 1;
            vPaintInfo.ScaleY = 1;
            vPaintInfo.Zoom = 1;
            
            using (HCCanvas vCanvas = new HCCanvas())
            {
                vCanvas.Graphics = Graphics.FromImage(aBitmap);
                vCanvas.Brush.Color = Color.White;
                vCanvas.FillRect(new RECT(0, 0, aBitmap.Width, aBitmap.Height));
                this.DoPaint(OwnerData.Style, new RECT(0, 0, aBitmap.Width, aBitmap.Height),
                    0, aBitmap.Height, 0, aBitmap.Height, vCanvas, vPaintInfo);
                    
                vCanvas.Dispose();
            }

            return true;
        }
        //
        public override bool MouseDown(MouseEventArgs e)
        {
            this.Active = HC.PtInRect(new RECT(0, 0, FWidth, FHeight), e.X, e.Y);
            return this.Active;
        }

        public override HCCustomItem BreakByOffset(int aOffset)
        {
            return null;
        }
  
        public override bool CanConcatItems(HCCustomItem aItem)
        {
            return false;
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FWidth = (source as HCCustomRectItem).Width;
            FHeight = (source as HCCustomRectItem).Height;
        }

        // 撤销重做相关方法
        public override void Undo(HCCustomUndoAction aUndoAction)
        {
            if (aUndoAction is HCItemSelfUndoAction)
            {
                HCUndoList vUndoList = (aUndoAction as HCItemSelfUndoAction).Object as HCUndoList;
                if (vUndoList != null)
                    vUndoList.Undo();
                else
                    base.Undo(aUndoAction);
            }
            else
                base.Undo(aUndoAction);
        }

        public override void Redo(HCCustomUndoAction aRedoAction)
        {
            if (aRedoAction is HCItemSelfUndoAction)
            {
                HCUndoList vUndoList = (aRedoAction as HCItemSelfUndoAction).Object as HCUndoList;
                if (vUndoList != null)
                {
                    if (vUndoList.Seek < 0)
                        SelfUndoListInitializate(vUndoList);

                    vUndoList.Redo();
                }
                else
                    base.Redo(aRedoAction);
            }
            else
                base.Redo(aRedoAction);
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);
            byte[] vBuffer = BitConverter.GetBytes(FWidth);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FHeight);
            aStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte[] vBuffer = BitConverter.GetBytes(FWidth);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FWidth = BitConverter.ToInt32(vBuffer, 0);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            FHeight = BitConverter.ToInt32(vBuffer, 0);
            FormatDirty();
        }

        public override string ToHtml(string aPath) 
        {
            Bitmap vBitmap = null;
            if (!this.SaveToBitmap(ref vBitmap))
                return "";

            string Result = "";
            if (aPath != "")
            {
                if (!Directory.Exists(aPath + "images"))
                    Directory.CreateDirectory(aPath + "images");

                string vFileName = OwnerData.Style.GetHtmlFileTempName() + ".bmp";
                vBitmap.Save(aPath + "images\\" + vFileName);

                Result = "<img width=\"" + FWidth.ToString() + "\" height=\"" + FHeight.ToString()
                    + "\" src=\"images/" + vFileName + "\" alt=\"" + this.GetType().Name + "\" />";
            }
            else  // 保存为Base64
            {
                Result = "<img width=\"" + FWidth.ToString() + "\" height=\"" + FHeight.ToString()
                    + "\" src=\"data:img/jpg;base64," + HC.GraphicToBase64(vBitmap, vBitmap.RawFormat) + "\" alt=\"" + this.GetType().Name + "\" />";
            }

            return Result;
        }

        public override void ToXml(XmlElement aNode) 
        {
            base.ToXml(aNode);
            aNode.SetAttribute("width", FWidth.ToString());
            aNode.SetAttribute("height", FHeight.ToString());
        }

        public override void ParseXml(XmlElement aNode) 
        {
            base.ParseXml(aNode);
            FWidth = int.Parse(aNode.Attributes["width"].Value);
            FHeight = int.Parse(aNode.Attributes["height"].Value);
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
            set { SetSizeChanged(value); }
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

        public bool MangerUndo
        {
            get { return FMangerUndo; }
        }
    }

    public class HCDomainItem : HCCustomRectItem  // 域
    {
        private byte FLevel;
        private RECT FDrawRect;
        private MarkType FMarkType;

        public static Type HCDefaultDomainItemClass = typeof(HCDomainItem);

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                aCanvas, aPaintInfo);

            if (!aPaintInfo.Print)
            {
                FDrawRect.ReSet(aDrawRect);
                aPaintInfo.TopItems.Add(this);
            }
        }

        public HCDomainItem(HCCustomData aOwnerData)
            : base(aOwnerData)
        {
            this.StyleNo = HCStyle.Domain;
            FDrawRect = new RECT();
            FLevel = 0;
            Width = 0;
            Height = aOwnerData.Style.TextStyles[0].FontHeight;
        }

        public static bool IsBeginMark(HCCustomItem aItem)
        {
            return (aItem is HCDomainItem) && ((aItem as HCDomainItem).MarkType == MarkType.cmtBeg);
        }

        public static bool IsEndMark(HCCustomItem aItem)
        {
            return (aItem is HCDomainItem) && ((aItem as HCDomainItem).MarkType == MarkType.cmtEnd);
        }

        public override int GetOffsetAt(int x)
        {
            if ((x >= 0) && (x <= Width))
            {
                if (FMarkType == View.MarkType.cmtBeg)
                    return HC.OffsetAfter;
                else
                    return HC.OffsetBefor;
            }
            else
                return base.GetOffsetAt(x);
        }

        public override bool JustifySplit()
        {
            return false;
        }

        // 当前RectItem格式化时所属的Data(为松耦合请传入TCustomRichData类型)
        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            this.Width = 0;
            this.Height = aRichData.Style.TextStyles[0].FontHeight;  // 默认大小
            if (this.MarkType == MarkType.cmtBeg)
            {
                if (aItemNo < aRichData.Items.Count - 1)
                {
                    HCCustomItem vItem = aRichData.Items[aItemNo + 1];
                    if ((vItem.StyleNo == this.StyleNo)  // 下一个是组标识
                        && ((vItem as HCDomainItem).MarkType == MarkType.cmtEnd)) // 下一个是结束标识
                        this.Width = 10;  // 增加宽度以便输入时光标可方便点击
                    else
                    if (vItem.ParaFirst)  // 下一个是段首，我是段尾
                        this.Width = 10;  // 增加宽度以便输入时光标可方便点击
                    else
                    if (vItem.StyleNo > HCStyle.Null)  // 后面是文本，跟随后面的高度
                        this.Height = aRichData.Style.TextStyles[vItem.StyleNo].FontHeight;
                }
                else
                    this.Width = 10;
            }
            else  // 域结束标识
            {
                HCCustomItem vItem = aRichData.Items[aItemNo - 1];
                if ((vItem.StyleNo == this.StyleNo) && ((vItem as HCDomainItem).MarkType == MarkType.cmtBeg))
                    this.Width = 10;
                else
                if (this.ParaFirst)  // 结束标识是段首，增加宽度
                    this.Width = 10;
                else
                if (vItem.StyleNo > HCStyle.Null)  // 前面是文本，距离前面的高度
                    this.Height = aRichData.Style.TextStyles[vItem.StyleNo].FontHeight;
            }
        }

        public override void PaintTop(HCCanvas aCanvas)
        {
            base.PaintTop(aCanvas);

            aCanvas.Pen.Width = 1;
            if (FMarkType == View.MarkType.cmtBeg)
            {
                aCanvas.Pen.BeginUpdate();
                try
                {
                    aCanvas.Pen.Width = 1;
                    aCanvas.Pen.Style = HCPenStyle.psSolid;
                    aCanvas.Pen.Color = Color.FromArgb(0, 0, 255);
                }
                finally
                {
                    aCanvas.Pen.EndUpdate();
                }
                aCanvas.MoveTo(FDrawRect.Left + 2, FDrawRect.Top - 1);
                aCanvas.LineTo(FDrawRect.Left, FDrawRect.Top - 1);
                aCanvas.LineTo(FDrawRect.Left, FDrawRect.Bottom + 1);
                aCanvas.LineTo(FDrawRect.Left + 2, FDrawRect.Bottom + 1);
            }
            else
            {
                aCanvas.Pen.BeginUpdate();
                try
                {
                    aCanvas.Pen.Width = 1;
                    aCanvas.Pen.Style = HCPenStyle.psSolid;
                    aCanvas.Pen.Color = Color.FromArgb(0, 0, 255);
                }
                finally
                {
                    aCanvas.Pen.EndUpdate();
                }

                aCanvas.MoveTo(FDrawRect.Right - 2, FDrawRect.Top - 1);
                aCanvas.LineTo(FDrawRect.Right, FDrawRect.Top - 1);
                aCanvas.LineTo(FDrawRect.Right, FDrawRect.Bottom + 1);
                aCanvas.LineTo(FDrawRect.Right - 2, FDrawRect.Bottom + 1);
            }
        }

        public override bool SaveToBitmap(ref Bitmap aBitmap)
        {
            return false;
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);
            aStream.WriteByte((byte)FMarkType);
            aStream.WriteByte(FLevel);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            FMarkType = (MarkType)aStream.ReadByte();
            if (aFileVersion > 38)
                FLevel = (byte)aStream.ReadByte();
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("mark", ((byte)FMarkType).ToString());
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FMarkType = (MarkType)byte.Parse(aNode.Attributes["mark"].Value);
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

        protected virtual void SetTextStyleNo(int value)
        {
            if (FTextStyleNo != value)
                FTextStyleNo = value;
        }

        public HCTextRectItem(HCCustomData aOwnerData) : base(aOwnerData)
        {
            if (aOwnerData.CurStyleNo > HCStyle.Null)
                FTextStyleNo = aOwnerData.CurStyleNo;
            else
                FTextStyleNo = 0;
        }

        public HCTextRectItem(HCCustomData aOwnerData, int aWidth, int aHeight) : base(aOwnerData, aWidth, aHeight)
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FTextStyleNo = (source as HCTextRectItem).TextStyleNo;
        }

        public override int GetOffsetAt(int x)
        {
            if (x < Width / 2)
                return HC.OffsetBefor;
            else
                return HC.OffsetAfter;
        }

        public override bool JustifySplit()
        {
            return false;
        }

        public override void ApplySelectTextStyle(HCStyle aStyle, HCStyleMatch aMatchStyle)
        {
            FTextStyleNo = aMatchStyle.GetMatchStyleNo(aStyle, FTextStyleNo);
        }

        public override void MarkStyleUsed(bool aMark)
        {
            if (aMark)
                OwnerData.Style.TextStyles[FTextStyleNo].CheckSaveUsed = true;
            else
                FTextStyleNo = OwnerData.Style.TextStyles[FTextStyleNo].TempNo;
        }

        public override bool SelectExists()
        {
            return GetSelectComplate();
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);
            byte[] vBuffer = BitConverter.GetBytes(FTextStyleNo);
            aStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            byte[] vBuffer = BitConverter.GetBytes(FTextStyleNo);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FTextStyleNo = BitConverter.ToInt32(vBuffer, 0);

            if ((aStyle != null) && (FTextStyleNo > aStyle.TextStyles.Count - 1))  // 兼容历史错误(删除多余样式时没有)
                FTextStyleNo = 0;
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("textsno", FTextStyleNo.ToString());
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FTextStyleNo = int.Parse(aNode.Attributes["textsno"].Value);
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
        private EventHandler FOnClick;
        protected byte FPaddingLeft, FPaddingTop, FPaddingRight, FPaddingBottom;
        protected int FMinWidth, FMinHeight;
        
        protected virtual void DoClick()
        {
            if (FOnClick != null && OwnerData.CanEdit())
                FOnClick(this, null);
        }

        public override bool MouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && HC.PtInRect(this.ClientRect(), e.X, e.Y))
                this.DoClick();

            return base.MouseUp(e);
        }

        public HCControlItem(HCCustomData aOwnerData) : base(aOwnerData)
        {
            FAutoSize = true;
            FPaddingLeft = 5;
            FPaddingRight = 5;
            FPaddingTop = 5;
            FPaddingBottom = 5;
            FMinWidth = 20;
            FMinHeight = 10;
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FAutoSize = (source as HCControlItem).AutoSize;
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int  aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);
            byte[] vBuffer = BitConverter.GetBytes(FAutoSize);
            aStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            byte[] vBuffer = BitConverter.GetBytes(FAutoSize);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FAutoSize = BitConverter.ToBoolean(vBuffer, 0);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("autosize", FAutoSize.ToString());
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FAutoSize = bool.Parse(aNode.Attributes["autosize"].Value);
        }

        public virtual RECT ClientRect()
        {
            return new RECT(0, 0, this.Width, this.Height);
        }

        public bool AutoSize
        {
            get { return FAutoSize; }
            set { FAutoSize = value; }
        }

        public EventHandler OnClick
        {
            get { return FOnClick; }
            set { FOnClick = value; }
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

        private GripType GetGripType(int x, int  y)
        {
            POINT vPt = new POINT(x, y);

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

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, 
            int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                aCanvas, aPaintInfo);

            if ((!aPaintInfo.Print) && Active)
            {
                if (Resizing)
                {
                    switch (FResizeGrip)
                    {
                        case GripType.gtLeftTop:
                            FResizeRect = HC.Bounds(aDrawRect.Left + Width - FResizeWidth,
                                aDrawRect.Top + Height - FResizeHeight, FResizeWidth, FResizeHeight);
                            break;

                        case GripType.gtRightTop:
                            FResizeRect = HC.Bounds(aDrawRect.Left,
                                aDrawRect.Top + Height - FResizeHeight, FResizeWidth, FResizeHeight);
                            break;

                        case GripType.gtLeftBottom:
                            FResizeRect = HC.Bounds(aDrawRect.Left + Width - FResizeWidth,
                                aDrawRect.Top, FResizeWidth, FResizeHeight);
                            break;

                        case GripType.gtRightBottom:
                            FResizeRect = HC.Bounds(aDrawRect.Left, aDrawRect.Top, FResizeWidth, FResizeHeight);
                            break;
                    }
                
                    aPaintInfo.TopItems.Add(this);
                }

                if (AllowResize)  // 绘制缩放拖动提示锚点
                {
                    aCanvas.Brush.Color = Color.Gray;
                    aCanvas.FillRect(HC.Bounds(aDrawRect.Left, aDrawRect.Top, GripSize, GripSize));
                    aCanvas.FillRect(HC.Bounds(aDrawRect.Right - GripSize, aDrawRect.Top, GripSize, GripSize));
                    aCanvas.FillRect(HC.Bounds(aDrawRect.Left, aDrawRect.Bottom - GripSize, GripSize, GripSize));
                    aCanvas.FillRect(HC.Bounds(aDrawRect.Right - GripSize, aDrawRect.Bottom - GripSize, GripSize, GripSize));
                }
            }
        }

        public override bool MouseDown(MouseEventArgs e)
        {
            FResizeGrip = GripType.gtNone;
            bool vResult = base.MouseDown(e);
            if (Active && AllowResize)
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

            return vResult;
        }

        // 撤销恢复相关方法
        protected void SelfUndo_Resize(int aNewWidth, int aNewHeight)
        {
            HCUndoList vUndoList = GetSelfUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                SelfUndo_New();
                HCUndo vUndo = vUndoList.Last;
                if (vUndo != null)
                {
                    HCSizeUndoData vSizeUndoData = new HCSizeUndoData();
                    vSizeUndoData.OldWidth = this.Width;
                    vSizeUndoData.OldHeight = this.Height;
                    vSizeUndoData.NewWidth = aNewWidth;
                    vSizeUndoData.NewHeight = aNewHeight;

                    vUndo.Data = vSizeUndoData;
                }
            }
        }

        protected override void DoSelfUndoDestroy(HCUndo aUndo)
        {
            if ((aUndo.Data != null) && (aUndo.Data is HCSizeUndoData))
            {
                (aUndo.Data as HCSizeUndoData).Dispose();
                aUndo.Data = null;
            }

            base.DoSelfUndoDestroy(aUndo);
        }

        protected override void DoSelfUndo(HCUndo aUndo)
        {
            if (aUndo.Data is HCSizeUndoData)
            {
                HCSizeUndoData vSizeAction = aUndo.Data as HCSizeUndoData;
                this.Width = vSizeAction.OldWidth;
                this.Height = vSizeAction.OldHeight;
            }
            else
                base.DoSelfUndo(aUndo);
        }

        protected override void DoSelfRedo(HCUndo aRedo)
        {
            if (aRedo.Data is HCSizeUndoData)
            {
                HCSizeUndoData vSizeAction = aRedo.Data as HCSizeUndoData;
                this.Width = vSizeAction.NewWidth;
                this.Height = vSizeAction.NewHeight;
            }
            else
                base.DoSelfRedo(aRedo);
        }

        protected virtual bool GetResizing()
        {
            return FResizing;
        }

        protected virtual void SetResizing(bool value)
        {
            if (FResizing != value)
                FResizing = value;
        }

        protected bool GetAllowResize()
        {
            return FCanResize && OwnerData.CanEdit();
        }

        protected GripType ResizeGrip
        {
            get { return FResizeGrip; }
        }

        protected RECT ResizeRect
        {
            get { return FResizeRect; }
        }

        public HCResizeRectItem() : base()
        {

        }

        public HCResizeRectItem(HCCustomData aOwnerData) : base(aOwnerData)
        {
            FCanResize = true;
            FGripSize = 8;
        }

        public HCResizeRectItem(HCCustomData aOwnerData, int aWidth, int aHeight) : base(aOwnerData, aWidth, aHeight)
        {
            FCanResize = true;
            FGripSize = 8;
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FCanResize = (source as HCResizeRectItem).CanResize;
        }

        /// <summary> 获取坐标X、Y是否在选中区域中 </summary>
        public override bool CoordInSelect(int x, int  y)
        {
            return SelectExists() && HC.PtInRect(HC.Bounds(0, 0, Width, Height), x, y) && (GetGripType(x, y) == GripType.gtNone);
        }

        public override void PaintTop(HCCanvas aCanvas)
        {
            base.PaintTop(aCanvas);
            if (FResizing)
            {
                aCanvas.Brush.Style = HCBrushStyle.bsClear;
                aCanvas.Rectangle(FResizeRect);
                aCanvas.Brush.Color = Color.White;
                aCanvas.Font.BeginUpdate();
                try
                {
                    aCanvas.Font.Color = Color.Black;
                    aCanvas.Font.FontStyles.Value = 0;
                }
                finally
                {
                    aCanvas.Font.EndUpdate();
                }

                aCanvas.TextOut(FResizeRect.Left + 2, FResizeRect.Top + 2,
                    FResizeWidth.ToString() + " x " + FResizeHeight.ToString());
            }
        }

        // 继承THCCustomItem抽象方法
        public override bool MouseMove(MouseEventArgs e)
        {
            bool vResult = base.MouseMove(e);
            HC.GCursor = Cursors.Default;
            if (Active && AllowResize)
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

            return vResult;
        }

        public override bool MouseUp(MouseEventArgs e)
        {
            bool vResult = base.MouseUp(e);
            if (FResizing)
            {
                if ((FResizeWidth < 0) || (FResizeHeight < 0))
                {
                    FResizing = false;
                    return vResult;
                }

                SelfUndo_Resize(FResizeWidth, FResizeHeight);
                Width = FResizeWidth;
                Height = FResizeHeight;
                FResizing = false;
            }

            return vResult;
        }

        public override bool CanDrag()
        {
            return !FResizing;
        }

        public override bool SelectExists()
        {
            return IsSelectComplateTheory();
        }

        public override bool IsSelectComplateTheory()
        {
            if (!AllowResize)
                return false;
            else
                return base.IsSelectComplateTheory();
        }

        public override int GetOffsetAt(int x)
        {
            if (AllowResize)
                return base.GetOffsetAt(x);
            {
                if (x < Width / 2)
                    return HC.OffsetBefor;
                else
                    return HC.OffsetAfter;
            }
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);
            byte[] vBuffer = BitConverter.GetBytes(FCanResize);
            aStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            if (aFileVersion > 44)
            {
                byte[] vBuffer = BitConverter.GetBytes(FCanResize);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FCanResize = BitConverter.ToBoolean(vBuffer, 0);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("canresize", FCanResize.ToString());
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            if (aNode.HasAttribute("canresize"))
                FCanResize = bool.Parse(aNode.Attributes["canresize"].Value);
        }

        /// <summary> 约束到指定大小范围内 </summary>
        public virtual void RestrainSize(int aWidth, int aHeight) { }

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

        public bool AllowResize
        {
            get { return GetAllowResize(); }
        }
    }

    public class HCAnimateRectItem : HCCustomRectItem  // 动画RectItem
    {
        public override int GetOffsetAt(int x)
        {
            if (x < Width / 2)
                return HC.OffsetBefor;
            else
                return HC.OffsetAfter;
        }

        public HCAnimateRectItem(HCCustomData aOwnerData)
            : base(aOwnerData) { }

        public HCAnimateRectItem(HCCustomData aOwnerData, int aWidth, int aHeight)
            : base(aOwnerData, aWidth, aHeight) { }
    }

    public class HCDataItem : HCResizeRectItem
    {
        public HCDataItem() : base()
        {

        }

        public HCDataItem(HCCustomData aOwnerData) : base(aOwnerData)
        {

        }

        public HCDataItem(HCCustomData aOwnerData, int aWidth, int aHeight) : base(aOwnerData, aWidth, aHeight)
        {

        }

        public override void SilenceChange()
        {
            this.FormatDirty();
            this.OwnerData.SilenceChange();
        }
    }
}
