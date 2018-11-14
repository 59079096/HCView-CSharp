/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-7-9              }
{                                                       }
{          文档EditItem(文本框)对象实现单元             }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace HC.View
{
    public class HCEditItem : HCControlItem
    {
        private string FText;
        private byte FBorderWidth;
        private HCBorderSides FBorderSides;
        private bool FMouseIn, FReadOnly;
        private short FCaretOffset;

        public override void FormatToDrawItem(HCCustomData ARichData, int AItemNo)
        {
            if (this.AutoSize)
            {
                SIZE vSize = new SIZE();
                ARichData.Style.TextStyles[TextStyleNo].ApplyStyle(ARichData.Style.DefCanvas);
                if (FText != "")
                    vSize = ARichData.Style.DefCanvas.TextExtent(FText);
                else
                    vSize = ARichData.Style.DefCanvas.TextExtent("I");
                
                Width = FMargin + vSize.cx + FMargin;  // 间距
                Height = FMargin + vSize.cy + FMargin;
            }
            
            if (Width < FMinWidth)
                Width = FMinWidth;
            if (Height < FMinHeight)
                Height = FMinHeight;
        }

        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, int ADataDrawTop, int ADataDrawBottom, 
            int ADataScreenTop, int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoPaint(AStyle, ADrawRect, ADataDrawTop, ADataDrawBottom, ADataScreenTop,
                ADataScreenBottom, ACanvas, APaintInfo);
            
            if (this.IsSelectComplate && (!APaintInfo.Print))
            {
                ACanvas.Brush.Color = AStyle.SelColor;
                ACanvas.FillRect(ADrawRect);
            }

            AStyle.TextStyles[TextStyleNo].ApplyStyle(ACanvas, APaintInfo.ScaleY / APaintInfo.Zoom);
            if (!this.AutoSize)
                ACanvas.TextRect(ADrawRect, ADrawRect.Left + FMargin, ADrawRect.Top + FMargin, FText);
            else
                ACanvas.TextOut(ADrawRect.Left + FMargin, ADrawRect.Top + FMargin, FText);
            
            if (FMouseIn && (!APaintInfo.Print))
                ACanvas.Pen.Color = Color.Blue;
            else  // 鼠标不在其中或打印
                ACanvas.Pen.Color = Color.Black;

            ACanvas.Pen.Width = FBorderWidth;
            if (FBorderSides.Contains((byte)BorderSide.cbsLeft))
            {
                ACanvas.MoveTo(ADrawRect.Left, ADrawRect.Top);
                ACanvas.LineTo(ADrawRect.Left, ADrawRect.Bottom);
            }

            if (FBorderSides.Contains((byte)BorderSide.cbsTop))
            {
                ACanvas.MoveTo(ADrawRect.Left, ADrawRect.Top);
                ACanvas.LineTo(ADrawRect.Right, ADrawRect.Top);
            }

            if (FBorderSides.Contains((byte)BorderSide.cbsRight))
            {
                ACanvas.MoveTo(ADrawRect.Right, ADrawRect.Top);
                ACanvas.LineTo(ADrawRect.Right, ADrawRect.Bottom);
            }

            if (FBorderSides.Contains((byte)BorderSide.cbsBottom))
            {
                ACanvas.MoveTo(ADrawRect.Left, ADrawRect.Bottom);
                ACanvas.LineTo(ADrawRect.Right, ADrawRect.Bottom);
            }
        }

        public override int GetOffsetAt(int X)
        {
            if (X <= FMargin)
                return HC.OffsetBefor;
            else
                if (X >= Width - FMargin)
                    return HC.OffsetAfter;
                else
                    return HC.OffsetInner;
        }

        public override void MouseEnter()
        {
            base.MouseEnter();
            FMouseIn = true;
        }

        public override void MouseLeave()
        {
            base.MouseLeave();
            FMouseIn = false;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            OwnerData.Style.TextStyles[TextStyleNo].ApplyStyle(OwnerData.Style.DefCanvas);
            int vX = e.X - FMargin;// - (Width - FMargin - OwnerData.Style.DefCanvas.TextWidth(FText) - FMargin) div 2;
            short vOffset = (short)HC.GetCharOffsetByX(OwnerData.Style.DefCanvas, FText, vX);
            if (vOffset != FCaretOffset)
            {
                FCaretOffset = vOffset;
                OwnerData.Style.UpdateInfoReCaret();
            }
        }

        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);
        }

        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);
        }
        
        /// <summary> 正在其上时内部是否处理指定的Key和Shif </summary>
        public override bool WantKeyDown(KeyEventArgs e)
        {
            return true;
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (!FReadOnly)
            {
                switch (e.KeyValue)
                {
                    case User.VK_BACK:
                        if (FCaretOffset > 0)
                        {
                            FText = FText.Remove(FCaretOffset - 1, 1);
                            FCaretOffset--;
                        }
                        this.SizeChanged = true;
                        break;

                    case User.VK_LEFT:
                        if (FCaretOffset > 0)
                            FCaretOffset--;
                        break;

                    case User.VK_RIGHT:
                        if (FCaretOffset < FText.Length)
                            FCaretOffset++;
                        break;

                    case User.VK_DELETE:
                        if (FCaretOffset < FText.Length)
                            FText = FText.Remove(FCaretOffset, 1);
                        break;

                    case User.VK_HOME:
                        FCaretOffset = 0;
                        break;

                    case User.VK_END:
                        FCaretOffset = (short)FText.Length;
                        break;

                    default:
                        base.KeyDown(e);
                        break;
                }
            }
            else
                base.KeyDown(e);
        }

        public override void KeyPress(ref Char Key)
        {
            if (!FReadOnly)
            {
                FCaretOffset++;
                FText = FText.Insert(FCaretOffset - 1, Key.ToString());

                this.SizeChanged = true;
            }
            else
                base.KeyPress(ref Key);
        }

        public override bool InsertText(string AText)
        {
            FText = FText.Insert(FCaretOffset, AText);
            FCaretOffset += (short)AText.Length;
            this.SizeChanged = true;
            return true;
        }

        public override void GetCaretInfo(ref HCCaretInfo ACaretInfo)
        {
            if (FCaretOffset < 0)
            {
                ACaretInfo.Visible = false;
                return;
            }

            string vS = FText.Substring(0, FCaretOffset);
            OwnerData.Style.TextStyles[TextStyleNo].ApplyStyle(OwnerData.Style.DefCanvas);
            if (vS != "")
            {
                SIZE vSize = OwnerData.Style.DefCanvas.TextExtent(vS);
                ACaretInfo.Height = vSize.cy;
                ACaretInfo.X = FMargin + vSize.cx;// + (Width - FMargin - OwnerData.Style.DefCanvas.TextWidth(FText) - FMargin) div 2;
            }
            else
            {
                ACaretInfo.Height = OwnerData.Style.DefCanvas.TextHeight("H");
                ACaretInfo.X = FMargin;// + (Width - FMargin - OwnerData.Style.DefCanvas.TextWidth(FText) - FMargin) div 2;
            }
            
            ACaretInfo.Y = FMargin;
            if ((!this.AutoSize) && (ACaretInfo.X > Width))
                ACaretInfo.Visible = false;
        }

        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            int vLen = System.Text.Encoding.Default.GetByteCount(FText);
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            ushort vSize = (ushort)vLen;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            if (vSize > 0)
            {
                vBuffer = System.Text.Encoding.Default.GetBytes(FText);
                AStream.Write(vBuffer, 0, vBuffer.Length);
            }

            vBuffer = BitConverter.GetBytes(FReadOnly);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            AStream.WriteByte(FBorderSides.Value);
            AStream.WriteByte(FBorderWidth);
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);

            ushort vSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            if (vSize > 0)
            {
                vBuffer = new byte[vSize];
                AStream.Write(vBuffer, 0, vBuffer.Length);
                FText = System.Text.Encoding.Default.GetString(vBuffer);
            }

            vBuffer = BitConverter.GetBytes(FReadOnly);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FReadOnly = BitConverter.ToBoolean(vBuffer, 0);

            FBorderSides.Value = (byte)AStream.ReadByte();
            FBorderWidth = (byte)AStream.ReadByte();
        }

        protected virtual void SetText(string Value)
        {
            if ((!FReadOnly) && (FText != Value))
            {
                FText = Value;
                if (FCaretOffset > FText.Length)
                    FCaretOffset = 0;

                OwnerData.Style.UpdateInfoRePaint();
            }
        }

        public HCEditItem(HCCustomData AOwnerData, string AText) : base(AOwnerData)
        {
            this.StyleNo = HCStyle.Edit;
            FText = AText;
            FMouseIn = false;
            FMargin = 4;
            FCaretOffset = -1;
            Width = 50;
            FBorderWidth = 1;
            FBorderSides = new HCBorderSides();
            FBorderSides.InClude((byte)BorderSide.cbsLeft);
            FBorderSides.InClude((byte)BorderSide.cbsTop);
            FBorderSides.InClude((byte)BorderSide.cbsRight);
            FBorderSides.InClude((byte)BorderSide.cbsBottom);
        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            FText = (Source as HCEditItem).Text;
            FReadOnly = (Source as HCEditItem).ReadOnly;
            FBorderSides.Value = (Source as HCEditItem).BorderSides.Value;
            FBorderWidth = (Source as HCEditItem).BorderWidth;
        }

        public string Text
        {
            get { return FText; }
            set { SetText(value); }
        }

        public bool ReadOnly
        {
            get { return FReadOnly; }
            set { FReadOnly = value; }
        }

        public HCBorderSides BorderSides
        {
            get { return FBorderSides; }
            set { FBorderSides = value; }
        }

        public byte BorderWidth
        {
            get { return FBorderWidth; }
            set { FBorderWidth = value; }
        }
    }
}
