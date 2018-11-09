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

namespace HC.View
{
    public delegate POINT GetScreenCoordEventHandler(int X, int Y);

    public class HCSectionData : HCRichData
    {
        private EventHandler FOnReadOnlySwitch;
        private GetScreenCoordEventHandler FOnGetScreenCoord;
        private List<HCFloatItem> FFloatItems;  // THCItems支持Add时控制暂时不用
        int FFloatItemIndex, FMouseDownIndex, FMouseMoveIndex, FMouseX, FMouseY;

        private HCFloatItem CreateFloatItemByStyle(int AStyleNo)
        {
            HCFloatItem Result = null;
            if (AStyleNo == HCFloatStyle.Line)
            {
                Result = new HCFloatLineItem(this);
            }
            else
                throw new Exception("未找到类型 " + AStyleNo.ToString() + " 对应的创建FloatItem代码！");

            return Result;
        }

        private int GetFloatItemAt(int X, int Y)
        {
            int Result = -1;
            HCFloatItem vFloatItem = null;

            for (int i = 0; i <= FFloatItems.Count - 1; i++)
            {
                vFloatItem = FFloatItems[i];

                if (vFloatItem.PtInClient(X - vFloatItem.Left, Y - vFloatItem.Top))
                {
                    Result = i;
                    break;
                }
            }

            return Result;
        }

        private HCFloatItem GetActiveFloatItem()
        {
            if (FFloatItemIndex < 0)
                return null;
            else
                return FFloatItems[FFloatItemIndex];
        }

        public override POINT GetScreenCoord(int X, int Y)
        {
            if (FOnGetScreenCoord != null)
                return FOnGetScreenCoord(X, Y);
            else
                return new POINT();
        }

        protected override void SetReadOnly(bool Value)
        {
            if (this.ReadOnly != Value)
            {
                base.SetReadOnly(Value);

                if (FOnReadOnlySwitch != null)
                    FOnReadOnlySwitch(this, null);
            }
        }

        public HCSectionData(HCStyle AStyle) : base(AStyle)
        {
            FFloatItems = new List<HCFloatItem>();
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
            bool Result = true;
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
                MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks,
                    e.X - FFloatItems[FFloatItemIndex].Left, e.Y - FFloatItems[FFloatItemIndex].Top,
                    e.Delta);
                FFloatItems[FFloatItemIndex].MouseDown(vMouseArgs);
            }

            if ((FMouseDownIndex < 0) && (vOldIndex < 0))
                Result = false;
            else
            {
                FMouseX = e.X;
                FMouseY = e.Y;
            }

            return Result;
        }

        public bool MouseMoveFloatItem(MouseEventArgs e)
        {
            bool Result = true;
            if (((Control.ModifierKeys & Keys.Shift) == Keys.Shift) && (FMouseDownIndex >= 0))
            {
                HCFloatItem vFloatItem = FFloatItems[FMouseDownIndex];
                MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks, 
                    e.X - vFloatItem.Left, e.Y - vFloatItem.Top, e.Delta);
                vFloatItem.MouseMove(vMouseArgs);
                
                if (!vFloatItem.Resizing)
                {
                    vFloatItem.Left = vFloatItem.Left + e.X - FMouseX;
                    vFloatItem.Top = vFloatItem.Top + e.Y - FMouseY;
                    
                    FMouseX = e.X;
                    FMouseY = e.Y;
                }
            
                Style.UpdateInfoRePaint();
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
                    HCFloatItem vFloatItem = FFloatItems[vItemIndex];
                    MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks,
                    e.X - vFloatItem.Left, e.Y - vFloatItem.Top, e.Delta);
                    vFloatItem.MouseMove(vMouseArgs);
                }
                else
                    Result = false;
            }

            return Result;
        }

        public bool MouseUpFloatItem(MouseEventArgs e)
        {
            bool Result = true;

            if (FMouseDownIndex >= 0)
            {
                HCFloatItem vFloatItem = FFloatItems[FMouseDownIndex];
                MouseEventArgs vMouseArgs = new MouseEventArgs(e.Button, e.Clicks,
                    e.X - vFloatItem.Left, e.Y - vFloatItem.Top, e.Delta);
                vFloatItem.MouseUp(vMouseArgs);
            }
            else
                Result = false;

            return Result;
        }

        public bool KeyDownFloatItem(KeyEventArgs e)
        {
            bool Result = true;

            if (FFloatItemIndex >= 0)
            {
                int Key = e.KeyValue;
                switch (Key)
                {
                    case User.VK_BACK:
                    case User.VK_DELETE:
                        FFloatItems.RemoveAt(FFloatItemIndex);
                        FFloatItemIndex = -1;
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

        public override void GetCaretInfo(int AItemNo, int AOffset, ref HCCaretInfo ACaretInfo)
        {
            if (FFloatItemIndex >= 0)
            {
                ACaretInfo.Visible = false;
                return;
            }

            base.GetCaretInfo(AItemNo, AOffset, ref ACaretInfo);
        }

        /// <summary> 插入浮动Item </summary>
        public bool InsertFloatItem(HCFloatItem AFloatItem)
        {
            int vStartNo = this.SelectInfo.StartItemNo;
            int vStartOffset = this.SelectInfo.StartItemOffset;
            
            // 取选中起始处的DrawItem
            int vDrawNo = this.GetDrawItemNoByOffset(vStartNo, vStartOffset);
            
            AFloatItem.Left = this.DrawItems[vDrawNo].Rect.Left
                + this.GetDrawItemOffsetWidth(vDrawNo, this.SelectInfo.StartItemOffset - this.DrawItems[vDrawNo].CharOffs + 1);
            AFloatItem.Top = this.DrawItems[vDrawNo].Rect.Top;
            
            this.FloatItems.Add(AFloatItem);
            FFloatItemIndex = this.FloatItems.Count - 1;
            AFloatItem.Active = true;
            
            if (!this.DisSelect())
                Style.UpdateInfoRePaint();

            return true;
        }

        public override void SaveToStream(Stream AStream, int AStartItemNo, int AStartOffset,
            int AEndItemNo, int AEndOffset)
        {
            base.SaveToStream(AStream, AStartItemNo, AStartOffset, AEndItemNo, AEndOffset);

            byte[] vBuffer = BitConverter.GetBytes(FFloatItems.Count);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            for (int i = 0; i <= FFloatItems.Count - 1; i++)
                FFloatItems[i].SaveToStream(AStream, 0, HC.OffsetAfter);
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);
            if (AFileVersion > 12)
            {
                int vFloatCount = 0;
                byte[] vBuffer = BitConverter.GetBytes(vFloatCount);
                AStream.Read(vBuffer, 0, vBuffer.Length);
                vFloatCount = BitConverter.ToInt32(vBuffer, 0);
                HCFloatItem vFloatItem = null;
                
                while (vFloatCount > 0)
                {
                    int vStyleNo = HCStyle.Null;
                    vBuffer = BitConverter.GetBytes(vStyleNo);
                    AStream.Read(vBuffer, 0, vBuffer.Length);
                    vFloatItem = CreateFloatItemByStyle(vStyleNo);
                    vFloatItem.LoadFromStream(AStream, AStyle, AFileVersion);
                    FFloatItems.Add(vFloatItem);

                    vFloatCount--;
                }
            }
        }

        public virtual void PaintFloatItems(int APageIndex, int ADataDrawLeft, int ADataDrawTop,
            int AVOffset, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            HCFloatItem vFloatItem = null;

            for (int i = 0; i <= FFloatItems.Count - 1; i++)
            {
                vFloatItem = FFloatItems[i];
                vFloatItem.DrawRect = HC.Bounds(vFloatItem.Left, vFloatItem.Top, vFloatItem.Width, vFloatItem.Height);
                vFloatItem.DrawRect.Offset(ADataDrawLeft, ADataDrawTop - AVOffset);  // 将数据起始位置映射到绘制位置
                vFloatItem.PaintTo(this.Style, vFloatItem.DrawRect, ADataDrawTop, 0,
                    0, 0, ACanvas, APaintInfo);
            }
        }

        public int FloatItemIndex
        {
            get { return FFloatItemIndex; }
        }

        public HCFloatItem ActiveFloatItem
        {
            get { return GetActiveFloatItem(); }
        }

        public List<HCFloatItem> FloatItems
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
        private int FReFormatStartItemNo;

        private int GetPageDataFmtTop(int APageIndex)
        {
            return 0;
        }

        protected override void ReFormatData_(int AStartItemNo, int ALastItemNo = -1, int AExtraItemCount = 0)
        {
            FReFormatStartItemNo = AStartItemNo;
            base.ReFormatData_(AStartItemNo, ALastItemNo, AExtraItemCount);
        }

        protected override void DoDrawItemPaintBefor(HCCustomData AData, int ADrawItemNo, 
            RECT ADrawRect, int ADataDrawLeft, int ADataDrawBottom, int ADataScreenTop,
            int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoDrawItemPaintBefor(AData, ADrawItemNo, ADrawRect, ADataDrawLeft,
            ADataDrawBottom, ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);
            if (!APaintInfo.Print)
            {
                if (FShowLineActiveMark)
                {
                    if (ADrawItemNo == GetSelectStartDrawItemNo())
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

                        int vTop = ADrawRect.Top + DrawItems[ADrawItemNo].Height() / 2;
                        ACanvas.MoveTo(ADataDrawLeft - 10, vTop);
                        ACanvas.LineTo(ADataDrawLeft - 11, vTop);
                        ACanvas.MoveTo(ADataDrawLeft - 11, vTop - 1);
                        ACanvas.LineTo(ADataDrawLeft - 11, vTop + 2);
                        ACanvas.MoveTo(ADataDrawLeft - 12, vTop - 2);
                        ACanvas.LineTo(ADataDrawLeft - 12, vTop + 3);
                        ACanvas.MoveTo(ADataDrawLeft - 13, vTop - 3);
                        ACanvas.LineTo(ADataDrawLeft - 13, vTop + 4);
                        ACanvas.MoveTo(ADataDrawLeft - 14, vTop - 4);
                        ACanvas.LineTo(ADataDrawLeft - 14, vTop + 5);
                        ACanvas.MoveTo(ADataDrawLeft - 15, vTop - 2);
                        ACanvas.LineTo(ADataDrawLeft - 15, vTop + 3);
                        ACanvas.MoveTo(ADataDrawLeft - 16, vTop - 2);
                        ACanvas.LineTo(ADataDrawLeft - 16, vTop + 3);
                    }
                }
                
                if (FShowUnderLine)
                {
                    if (DrawItems[ADrawItemNo].LineFirst)
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
                            
                        ACanvas.MoveTo(ADataDrawLeft, ADrawRect.Bottom);
                        ACanvas.LineTo(ADataDrawLeft + this.Width, ADrawRect.Bottom);
                    }
                }

                if (FShowLineNo)
                {
                    if (DrawItems[ADrawItemNo].LineFirst)
                    {
                        int vLineNo = 0;
                        for (int i = 0; i <= ADrawItemNo; i++)
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
                                }
                                finally
                                {
                                    vFont.EndUpdate();
                                }

                                ACanvas.Brush.Color = Color.FromArgb(180, 180, 180);
                                GDI.SelectObject(ACanvas.Handle, vFont.Handle);
                                int vTop = ADrawRect.Top + (ADrawRect.Bottom - ADrawRect.Top - 16) / 2;
                                ACanvas.TextOut(ADataDrawLeft - 50, vTop, vLineNo.ToString());
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

        protected override void DoDrawItemPaintAfter(HCCustomData AData, int ADrawItemNo, RECT ADrawRect,
            int ADataDrawLeft, int ADataDrawBottom, int ADataScreenTop, int ADataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoDrawItemPaintAfter(AData, ADrawItemNo, ADrawRect, ADataDrawLeft, ADataDrawBottom, 
                ADataScreenTop, ADataScreenBottom, ACanvas, APaintInfo);
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

        public override void SaveToStream(Stream AStream)
        {
            byte[] vBuffer = BitConverter.GetBytes(FShowUnderLine);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            base.SaveToStream(AStream);
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            byte[] vBuffer = BitConverter.GetBytes(FShowUnderLine);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FShowUnderLine = BitConverter.ToBoolean(vBuffer, 0);
            base.LoadFromStream(AStream, AStyle, AFileVersion);
        }

        public override bool InsertStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            return base.InsertStream(AStream, AStyle, AFileVersion);
        }

        public HCPageData(HCStyle AStyle) : base(AStyle)
        {
            FShowLineActiveMark = false;
            FShowUnderLine = false;
            FShowLineNo = false;
        }

        public override void PaintFloatItems(int APageIndex, int ADataDrawLeft,
            int ADataDrawTop, int AVOffset, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            HCFloatItem vFloatItem = null;

            for (int i = 0; i <= this.FloatItems.Count - 1; i++)
            {
                vFloatItem = this.FloatItems[i];

                if (vFloatItem.PageIndex == APageIndex)
                {
                    vFloatItem.DrawRect = HC.Bounds(vFloatItem.Left, vFloatItem.Top, vFloatItem.Width, vFloatItem.Height);
                    vFloatItem.DrawRect.Offset(ADataDrawLeft, ADataDrawTop - AVOffset);  // 将数据起始位置映射到绘制位置
                    vFloatItem.PaintTo(this.Style, vFloatItem.DrawRect, ADataDrawTop, 0,
                        0, 0, ACanvas, APaintInfo);
                }
            }
        }

        /// <summary> 从当前位置后分页 </summary>
        public bool InsertPageBreak()
        {
            HCPageBreakItem vPageBreak = new HCPageBreakItem(this);
            vPageBreak.ParaFirst = true;
            // 第一个Item分到下一页后，前一页没有任何Item，对编辑有诸多不利，所以在前一页补充一个空Item
            if ((SelectInfo.StartItemNo == 0) && (SelectInfo.StartItemOffset == 0))
            {
                KeyEventArgs vKeyArgs = new KeyEventArgs(Keys.Return);
                KeyDown(vKeyArgs);
            }

            return this.InsertItem(vPageBreak);
        }

        /// <summary> 插入批注 </summary>
        public bool InsertAnnotate(string AText)
        {
            return false;
        }

        //
        // 保存
        public string GetTextStr()
        {
            string Result = "";
            for (int i = 0; i <= Items.Count - 1; i++)
                Result = Result + Items[i].Text;

            return Result;
        }

        public void SaveToText(string AFileName, Encoding AEncoding)
        {
            FileStream vStream = new FileStream(AFileName, FileMode.Create, FileAccess.Write);
            try
            {
                SaveToTextStream(vStream, AEncoding);
            }
            finally
            {
                vStream.Dispose();
            }
        }

        public void SaveToTextStream(Stream AStream, Encoding AEncoding)
        {
            byte[] vBuffer = AEncoding.GetBytes(GetTextStr());
            byte[] vPreamble = AEncoding.GetPreamble();
            if (vPreamble.Length > 0)
                AStream.Write(vPreamble, 0, vPreamble.Length);
            AStream.Write(vBuffer, 0, vBuffer.Length);
        }

        // 读取
        public void LoadFromText(string AFileName, Encoding AEncoding)
        {
            FileStream vStream = new FileStream(AFileName, FileMode.Open, FileAccess.Read);
            try
            {
                string vFileFormat = Path.GetExtension(AFileName);
                vFileFormat = vFileFormat.ToLower();
                if (vFileFormat == ".txt")
                    LoadFromTextStream(vStream, AEncoding);
            }
            finally
            {
                vStream.Dispose();
            }
        }

        public void LoadFromTextStream(Stream AStream, Encoding AEncoding)
        {
            Clear();
            long vSize = AStream.Length - AStream.Position;
            byte[] vBuffer = new byte[vSize];
            AStream.Read(vBuffer, 0, (int)vSize);
            string vS = AEncoding.GetString(vBuffer);
            if (vS != "")
                InsertText(vS);
        }

        //
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

        public int ReFormatStartItemNo
        {
            get { return FReFormatStartItemNo; }
        }
    }
}
