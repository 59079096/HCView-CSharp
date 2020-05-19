/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                文档节对象高级管理单元                 }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Drawing;
using System.IO;
using HC.Win32;
using System.Xml;

namespace HC.View
{
    public delegate POINT GetScreenCoordEventHandler(int x, int y);

    public class HCSectionData : HCViewData
    {
        private EventHandler FOnReadOnlySwitch;
        private GetScreenCoordEventHandler FOnGetScreenCoord;

        private HCFloatItems FFloatItems;  // THCItems支持Add时控制暂时不用
        int FFloatItemIndex, FMouseDownIndex, FMouseMoveIndex;

        private int GetFloatItemAt(int x, int y)
        {
            int Result = -1;
            HCCustomFloatItem vFloatItem = null;

            for (int i = 0; i <= FFloatItems.Count - 1; i++)
            {
                vFloatItem = FFloatItems[i];

                if (vFloatItem.PointInClient(x - vFloatItem.Left, y - vFloatItem.Top))
                {
                    Result = i;
                    break;
                }
            }

            return Result;
        }

        private void DoInsertFloatItem(HCCustomFloatItem aItem)
        {
            DoInsertItem(aItem);
        }

        protected override void SetReadOnly(bool value)
        {
            if (this.ReadOnly != value)
            {
                base.SetReadOnly(value);

                if (FOnReadOnlySwitch != null)
                    FOnReadOnlySwitch(this, null);
            }
        }

        protected override void DoLoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.DoLoadFromStream(aStream, aStyle, aFileVersion);

            if (aFileVersion > 12)
            {
                int vFloatCount = 0;
                byte[] vBuffer = BitConverter.GetBytes(vFloatCount);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vFloatCount = BitConverter.ToInt32(vBuffer, 0);
                HCCustomFloatItem vFloatItem = null;

                while (vFloatCount > 0)
                {
                    int vStyleNo = HCStyle.Null;
                    vBuffer = BitConverter.GetBytes(vStyleNo);
                    aStream.Read(vBuffer, 0, vBuffer.Length);
                    vStyleNo = BitConverter.ToInt32(vBuffer, 0);

                    if ((aFileVersion < 28) && (vStyleNo == (byte)HCShapeStyle.hssLine))
                        vFloatItem = new HCFloatLineItem(this);
                    else
                        vFloatItem = CreateItemByStyle(vStyleNo) as HCCustomFloatItem;

                    vFloatItem.LoadFromStream(aStream, aStyle, aFileVersion);
                    FFloatItems.Add(vFloatItem);

                    vFloatCount--;
                }
            }
        }

        protected void UndoAction_FloatItemMirror(int aItemNo)
        {

        }

        public HCSectionData(HCStyle aStyle) : base(aStyle)
        {
            FFloatItems = new HCFloatItems();
            FFloatItems.OnInsertItem += DoInsertFloatItem;
            FFloatItemIndex = -1;
            FMouseDownIndex = -1;
            FMouseMoveIndex = -1;
        }

        ~HCSectionData()
        {

        }

        public override void Dispose()
        {
            base.Dispose();
            //FFloatItems.Free;
        }

        public bool MouseDownFloatItem(MouseEventArgs e)
        {
            bool vResult = false;

            FMouseDownIndex = GetFloatItemAt(e.X, e.Y);

            int vOldIndex = FFloatItemIndex;
            if (FFloatItemIndex != FMouseDownIndex)
            {
                if (FFloatItemIndex >= 0)
                    FFloatItems[FFloatItemIndex].Active = false;
            
                FFloatItemIndex = FMouseDownIndex;
            
                Style.UpdateInfoRePaint();
                Style.UpdateInfoReCaret();
            }

            if (FFloatItemIndex >= 0)
            {
                if (this.ReadOnly)
                    return true;

                MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks,
                    e.X - FFloatItems[FFloatItemIndex].Left, e.Y - FFloatItems[FFloatItemIndex].Top,
                    e.Delta);
                vResult = FFloatItems[FFloatItemIndex].MouseDown(vMouseArgs);
            }

            if ((FMouseDownIndex < 0) && (vOldIndex < 0))
                vResult = false;

            return vResult;
        }

        public bool MouseMoveFloatItem(MouseEventArgs e)
        {
            bool vResult = false;

            if (e.Button == MouseButtons.Left)
            {
                if (this.ReadOnly)
                    return vResult;

                if (FMouseDownIndex >= 0)
                {
                    HCCustomFloatItem vFloatItem = FFloatItems[FMouseDownIndex];
                    MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks,
                        e.X - vFloatItem.Left, e.Y - vFloatItem.Top, e.Delta);
                    vResult = vFloatItem.MouseMove(vMouseArgs);

                    if (vResult)
                        Style.UpdateInfoRePaint();
                }
            }
            else  // 普通鼠标移动
            {
                int vItemIndex = GetFloatItemAt(e.X, e.Y);
                if (FMouseMoveIndex != vItemIndex)
                {
                    if (FMouseMoveIndex >= 0)
                        FFloatItems[FMouseMoveIndex].MouseLeave();
            
                    FMouseMoveIndex = vItemIndex;
                    if (FMouseMoveIndex >= 0)
                        FFloatItems[FMouseMoveIndex].MouseEnter();
                }
                
                if (vItemIndex >= 0)
                {
                    HCCustomFloatItem vFloatItem = FFloatItems[vItemIndex];
                    MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks,
                        e.X - vFloatItem.Left, e.Y - vFloatItem.Top, e.Delta);
                    vResult = vFloatItem.MouseMove(vMouseArgs);
                }
            }

            return vResult;
        }

        public bool MouseUpFloatItem(MouseEventArgs e)
        {
            bool vResult = false;

            if (FMouseDownIndex >= 0)
            {
                if (this.ReadOnly)
                    return true;

                Undo_New();
                UndoAction_FloatItemMirror(FMouseDownIndex);

                HCCustomFloatItem vFloatItem = FFloatItems[FMouseDownIndex];
                MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks,
                    e.X - vFloatItem.Left, e.Y - vFloatItem.Top, e.Delta);
                vResult = vFloatItem.MouseUp(vMouseArgs);
                if (vResult)
                    Style.UpdateInfoRePaint();
            }

            return vResult;
        }

        public bool KeyDownFloatItem(KeyEventArgs e)
        {
            if (this.ReadOnly)
                return true;

            bool Result = true;

            if ((FFloatItemIndex >= 0) && !FFloatItems[FFloatItemIndex].Lock)
            {
                int Key = e.KeyValue;
                switch (Key)
                {
                    case User.VK_BACK:
                    case User.VK_DELETE:
                        FFloatItems.RemoveAt(FFloatItemIndex);
                        FFloatItemIndex = -1;
                        FMouseMoveIndex = -1;
                        break;

                    case User.VK_LEFT:
                        FFloatItems[FFloatItemIndex].Left -= 1;
                        break;

                    case User.VK_RIGHT:
                        FFloatItems[FFloatItemIndex].Left += 1;
                        break;

                    case User.VK_UP:
                        FFloatItems[FFloatItemIndex].Top -= 1;
                        break;

                    case User.VK_DOWN:
                        FFloatItems[FFloatItemIndex].Top += 1;
                        break;

                    default:
                        Result = false;
                        break;
                }
            }
            else
                Result = false;

            if (Result)
                Style.UpdateInfoRePaint();

            return Result;
        }

        public override void Clear()
        {
            FFloatItemIndex = -1;
            FMouseDownIndex = -1;
            FMouseMoveIndex = -1;
            FFloatItems.Clear();

            base.Clear();
        }

        public override void GetCaretInfo(int aItemNo, int aOffset, ref HCCaretInfo aCaretInfo)
        {
            if (FFloatItemIndex >= 0)
            {
                aCaretInfo.Visible = false;
                return;
            }

            base.GetCaretInfo(aItemNo, aOffset, ref aCaretInfo);
        }

        public override POINT GetScreenCoord(int x, int y)
        {
            if (FOnGetScreenCoord != null)
                return FOnGetScreenCoord(x, y);
            else
                return new POINT();
        }

        public void TraverseFloatItem(HCItemTraverse aTraverse)
        {
            if (aTraverse != null)
            {
                for (int i = 0; i < FFloatItems.Count; i++)
                {
                    if (aTraverse.Stop)
                        return;

                    aTraverse.Process(this, i, aTraverse.Tag, aTraverse.DomainStack, ref aTraverse.Stop);
                }
            }
        }

        public override int GetActiveItemNo()
        {
            if (FFloatItemIndex < 0)
                return base.GetActiveItemNo();
            else
                return -1;
        }

        public override HCCustomItem GetActiveItem()
        {
            if (FFloatItemIndex < 0)
                return base.GetActiveItem();
            else
                return null;
        }

        public HCCustomFloatItem GetActiveFloatItem()
        {
            if (FFloatItemIndex < 0)
                return null;
            else
                return FFloatItems[FFloatItemIndex];
        }

        /// <summary> 插入浮动Item </summary>
        public bool InsertFloatItem(HCCustomFloatItem aFloatItem)
        {
            int vStartNo = this.SelectInfo.StartItemNo;
            int vStartOffset = this.SelectInfo.StartItemOffset;
            
            // 取选中起始处的DrawItem
            int vDrawNo = this.GetDrawItemNoByOffset(vStartNo, vStartOffset);
            
            aFloatItem.Left = this.DrawItems[vDrawNo].Rect.Left
                + this.GetDrawItemOffsetWidth(vDrawNo, this.SelectInfo.StartItemOffset - this.DrawItems[vDrawNo].CharOffs + 1);
            aFloatItem.Top = this.DrawItems[vDrawNo].Rect.Top;
            
            this.FloatItems.Add(aFloatItem);
            FFloatItemIndex = this.FloatItems.Count - 1;
            aFloatItem.Active = true;
            
            if (!this.DisSelect())
                Style.UpdateInfoRePaint();

            return true;
        }

        public override void SaveToStream(Stream aStream, int aStartItemNo, int aStartOffset,
            int aEndItemNo, int aEndOffset)
        {
            base.SaveToStream(aStream, aStartItemNo, aStartOffset, aEndItemNo, aEndOffset);

            byte[] vBuffer = BitConverter.GetBytes(FFloatItems.Count);
            aStream.Write(vBuffer, 0, vBuffer.Length);
            for (int i = 0; i <= FFloatItems.Count - 1; i++)
                FFloatItems[i].SaveToStream(aStream, 0, HC.OffsetAfter);
        }

        public override void ToXml(XmlElement aNode)
        {
            XmlElement vNode = aNode.OwnerDocument.CreateElement("items");
            base.ToXml(vNode);
            aNode.AppendChild(vNode);

            vNode = aNode.OwnerDocument.CreateElement("floatitems");
            vNode.SetAttribute("count", FFloatItems.Count.ToString());
            for (int i = 0; i < FFloatItems.Count; i++)
            {
                XmlElement vFloatItemNode = aNode.OwnerDocument.CreateElement("floatitem");
                FFloatItems[i].ToXml(vFloatItemNode);
                vNode.AppendChild(vFloatItemNode);
            }

            aNode.AppendChild(vNode);
        }

        public override void ParseXml(XmlElement aNode)
        {
            XmlElement vItemsNode = aNode.SelectSingleNode("items") as XmlElement;
            base.ParseXml(vItemsNode);

            XmlElement vNode = null;
            vItemsNode = aNode.SelectSingleNode("floatitems") as XmlElement;
            for (int i = 0; i <= vItemsNode.ChildNodes.Count - 1; i++)
            {
                vNode = vItemsNode.ChildNodes[i] as XmlElement;
                HCCustomFloatItem vFloatItem = CreateItemByStyle(int.Parse(vNode.Attributes["sno"].Value)) as HCCustomFloatItem;
                vFloatItem.ParseXml(vNode);
                FFloatItems.Add(vFloatItem);
            }
        }

        public virtual void PaintFloatItems(int aPageIndex, int aDataDrawLeft, int aDataDrawTop,
            int aVOffset, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            HCCustomFloatItem vFloatItem = null;
            RECT vRect;
            for (int i = 0; i <= FFloatItems.Count - 1; i++)
            {
                vFloatItem = FFloatItems[i];
                // 代替下面不生效的代码
                vRect = HC.Bounds(vFloatItem.Left, vFloatItem.Top, vFloatItem.Width, vFloatItem.Height);
                vRect.Offset(aDataDrawLeft, aDataDrawTop - aVOffset);  // 将数据起始位置映射到绘制位置
                vFloatItem.DrawRect = vRect;
                // 下面的操作vFloatItemDraw.DrawRect.Offset并不生效
                //vFloatItem.DrawRect = HC.Bounds(vFloatItem.Left, vFloatItem.Top, vFloatItem.Width, vFloatItem.Height);
                //vFloatItem.DrawRect.Offset(aDataDrawLeft, aDataDrawTop - aVOffset);  // 将数据起始位置映射到绘制位置
                vFloatItem.PaintTo(this.Style, vFloatItem.DrawRect, aDataDrawTop, 0,
                    0, 0, aCanvas, aPaintInfo);
            }
        }

        public int FloatItemIndex
        {
            get { return FFloatItemIndex; }
        }

        public HCCustomFloatItem ActiveFloatItem
        {
            get { return GetActiveFloatItem(); }
        }

        public HCFloatItems FloatItems
        {
            get { return FFloatItems; }
        }

        public EventHandler OnReadOnlySwitch
        {
            get { return FOnReadOnlySwitch; }
            set { FOnReadOnlySwitch = value; }
        }

        public GetScreenCoordEventHandler OnGetScreenCoord
        {
            get { return FOnGetScreenCoord; }
            set { FOnGetScreenCoord = value; }
        }
    }

    public class HCHeaderData : HCSectionData
    {
        public HCHeaderData(HCStyle AStyle) : base(AStyle)
        {

        }
    }

    public class HCFooterData : HCSectionData
    {
        public HCFooterData(HCStyle AStyle) : base(AStyle)
        {

        }
    }

    public class HCPageData : HCSectionData  // 此类中主要处理表格单元格Data不需要而正文需要的属性或事件
    {
        private bool FShowLineActiveMark;  // 当前激活的行前显示标识
        private bool FShowUnderLine;  // 下划线
        private bool FShowLineNo;  // 行号

        protected override void DoDrawItemPaintBefor(HCCustomData aData, int aItemNo, int aDrawItemNo, 
            RECT aDrawRect, int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop,
            int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoDrawItemPaintBefor(aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight,
                aDataDrawBottom, aDataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);

            if (!APaintInfo.Print)
            {
                if (FShowLineActiveMark)
                {
                    if (aDrawItemNo == GetSelectStartDrawItemNo())
                    {
                        ACanvas.Pen.BeginUpdate();
                        try
                        {
                            ACanvas.Pen.Color = Color.Blue;
                            ACanvas.Pen.Style = HCPenStyle.psSolid;
                        }
                        finally
                        {
                            ACanvas.Pen.EndUpdate();
                        }

                        int vTop = aDrawRect.Top + DrawItems[aDrawItemNo].Height / 2;
                        ACanvas.MoveTo(aDataDrawLeft - 10, vTop);
                        ACanvas.LineTo(aDataDrawLeft - 11, vTop);
                        ACanvas.MoveTo(aDataDrawLeft - 11, vTop - 1);
                        ACanvas.LineTo(aDataDrawLeft - 11, vTop + 2);
                        ACanvas.MoveTo(aDataDrawLeft - 12, vTop - 2);
                        ACanvas.LineTo(aDataDrawLeft - 12, vTop + 3);
                        ACanvas.MoveTo(aDataDrawLeft - 13, vTop - 3);
                        ACanvas.LineTo(aDataDrawLeft - 13, vTop + 4);
                        ACanvas.MoveTo(aDataDrawLeft - 14, vTop - 4);
                        ACanvas.LineTo(aDataDrawLeft - 14, vTop + 5);
                        ACanvas.MoveTo(aDataDrawLeft - 15, vTop - 2);
                        ACanvas.LineTo(aDataDrawLeft - 15, vTop + 3);
                        ACanvas.MoveTo(aDataDrawLeft - 16, vTop - 2);
                        ACanvas.LineTo(aDataDrawLeft - 16, vTop + 3);
                    }
                }
                
                if (FShowUnderLine)
                {
                    if (DrawItems[aDrawItemNo].LineFirst)
                    {
                        ACanvas.Pen.BeginUpdate();
                        try
                        {
                            ACanvas.Pen.Color = Color.Black;
                            ACanvas.Pen.Style = HCPenStyle.psSolid;
                        }
                        finally
                        {
                            ACanvas.Pen.EndUpdate();
                        }
                            
                        ACanvas.MoveTo(aDataDrawLeft, aDrawRect.Bottom);
                        ACanvas.LineTo(aDataDrawLeft + this.Width, aDrawRect.Bottom);
                    }
                }

                if (FShowLineNo)
                {
                    if (DrawItems[aDrawItemNo].LineFirst)
                    {
                        int vLineNo = 0;
                        for (int i = 0; i <= aDrawItemNo; i++)
                        {
                            if (DrawItems[i].LineFirst)
                                vLineNo++;
                        }
            
                        // 20181103
                        IntPtr vOldFont = ACanvas.Font.Handle;
                        try
                        {
                            using (HCFont vFont = new HCFont())
                            {
                                vFont.BeginUpdate();
                                try
                                {
                                    vFont.Size = 10;
                                    vFont.Family = "Courier New";
                                    vFont.Color = Color.FromArgb(180, 180, 180);
                                }
                                finally
                                {
                                    vFont.EndUpdate();
                                }

                                ACanvas.Brush.Style = HCBrushStyle.bsClear;
                                GDI.SelectObject(ACanvas.Handle, vFont.Handle);
                                int vTop = aDrawRect.Top + (aDrawRect.Bottom - aDrawRect.Top - 16) / 2;
                                ACanvas.TextOut(aDataDrawLeft - 50, vTop, vLineNo.ToString());
                            }
                        }
                        finally
                        {
                            GDI.SelectObject(ACanvas.Font.Handle, vOldFont);
                        }
                    }
                }
            }
        }

        protected override void DoDrawItemPaintAfter(HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoDrawItemPaintAfter(aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight,
                aDataDrawBottom, aDataScreenTop, aDataScreenBottom, ACanvas, APaintInfo);
        }

        protected override void DoLoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            byte[] vBuffer = BitConverter.GetBytes(FShowUnderLine);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FShowUnderLine = BitConverter.ToBoolean(vBuffer, 0);

            base.DoLoadFromStream(aStream, aStyle, aFileVersion);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            if (FShowLineActiveMark)
            {
                int vMouseDownItemNo = this.MouseDownItemNo;
                int vMouseDownItemOffset = this.MouseDownItemOffset;
                base.MouseDown(e);
                if ((vMouseDownItemNo != this.MouseDownItemNo)
                    || (vMouseDownItemOffset != this.MouseDownItemOffset))
                    Style.UpdateInfoRePaint();
            }
            else
                base.MouseDown(e);
        }

        public override void SaveToStream(Stream aStream)
        {
            byte[] vBuffer = BitConverter.GetBytes(FShowUnderLine);
            aStream.Write(vBuffer, 0, vBuffer.Length);
            
            base.SaveToStream(aStream);
        }

        public override bool InsertStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            return base.InsertStream(aStream, aStyle, aFileVersion);
        }

        public HCPageData(HCStyle aStyle) : base(aStyle)
        {
            FShowLineActiveMark = false;
            FShowUnderLine = false;
            FShowLineNo = false;
        }

        public override void PaintFloatItems(int aPageIndex, int aDataDrawLeft,
            int aDataDrawTop, int aVOffset, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            HCCustomFloatItem vFloatItem = null;

            for (int i = 0; i <= this.FloatItems.Count - 1; i++)
            {
                vFloatItem = this.FloatItems[i];

                if (vFloatItem.PageIndex == aPageIndex)
                {
                    vFloatItem.DrawRect = HC.Bounds(vFloatItem.Left + aDataDrawLeft,  // 将数据起始位置映射到绘制位置
                        vFloatItem.Top + aDataDrawTop - aVOffset, vFloatItem.Width, vFloatItem.Height);

                    vFloatItem.PaintTo(this.Style, vFloatItem.DrawRect, aDataDrawTop, 0,
                        0, 0, aCanvas, aPaintInfo);
                }
            }
        }

        /// <summary> 从当前位置后分页 </summary>
        public bool InsertPageBreak()
        {
            if (this.SelectExists())
                return false;

            if ((Items[SelectInfo.StartItemNo].StyleNo < HCStyle.Null)
                && (SelectInfo.StartItemOffset == HC.OffsetInner))
                return false;

            KeyEventArgs e = new KeyEventArgs(Keys.Return);
            this.KeyDown(e, true);

            return true;
        }

        public bool ShowLineActiveMark
        {
            get { return FShowLineActiveMark; }
            set { FShowLineActiveMark = value; }
        }

        public bool ShowLineNo
        {
            get { return FShowLineNo; }
            set { FShowLineNo = value; }
        }

        public bool ShowUnderLine
        {
            get { return FShowUnderLine; }
            set { FShowUnderLine = value; }
        }
    }
}
