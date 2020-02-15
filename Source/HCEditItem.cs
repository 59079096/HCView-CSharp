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
        private bool FMouseIn, FReadOnly, FPrintOnlyText;
        private short FCaretOffset;

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            if (this.AutoSize)
            {
                SIZE vSize = new SIZE();
                aRichData.Style.ApplyTempStyle(TextStyleNo);
                if (FText != "")
                    vSize = aRichData.Style.TempCanvas.TextExtent(FText);
                else
                    vSize = aRichData.Style.TempCanvas.TextExtent("H");
                
                Width = FMargin + vSize.cx + FMargin;  // 间距
                Height = FMargin + vSize.cy + FMargin;
            }
            
            if (Width < FMinWidth)
                Width = FMinWidth;
            if (Height < FMinHeight)
                Height = FMinHeight;
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop,
                aDataScreenBottom, aCanvas, aPaintInfo);
            
            if (this.IsSelectComplate && (!aPaintInfo.Print))
            {
                aCanvas.Brush.Color = aStyle.SelColor;
                aCanvas.FillRect(aDrawRect);
            }

            aStyle.TextStyles[TextStyleNo].ApplyStyle(aCanvas, aPaintInfo.ScaleY / aPaintInfo.Zoom);

            if (!this.AutoSize)
                aCanvas.TextRect(aDrawRect, aDrawRect.Left + FMargin, aDrawRect.Top + FMargin, FText);
            else
                aCanvas.TextOut(aDrawRect.Left + FMargin, aDrawRect.Top + FMargin, FText);

            if (aPaintInfo.Print && FPrintOnlyText)
                return;

            if (FMouseIn)
                aCanvas.Pen.Color = Color.Blue;
            else  // 鼠标不在其中或打印
                aCanvas.Pen.Color = Color.Black;

            aCanvas.Pen.Width = FBorderWidth;
            aCanvas.Pen.Style = HCPenStyle.psSolid;

            if (FBorderSides.Contains((byte)BorderSide.cbsLeft))
            {
                aCanvas.MoveTo(aDrawRect.Left, aDrawRect.Top);
                aCanvas.LineTo(aDrawRect.Left, aDrawRect.Bottom);
            }

            if (FBorderSides.Contains((byte)BorderSide.cbsTop))
            {
                aCanvas.MoveTo(aDrawRect.Left, aDrawRect.Top);
                aCanvas.LineTo(aDrawRect.Right, aDrawRect.Top);
            }

            if (FBorderSides.Contains((byte)BorderSide.cbsRight))
            {
                aCanvas.MoveTo(aDrawRect.Right - 1, aDrawRect.Top);
                aCanvas.LineTo(aDrawRect.Right - 1, aDrawRect.Bottom);
            }

            if (FBorderSides.Contains((byte)BorderSide.cbsBottom))
            {
                aCanvas.MoveTo(aDrawRect.Left, aDrawRect.Bottom - 1);
                aCanvas.LineTo(aDrawRect.Right, aDrawRect.Bottom - 1);
            }
        }

        public override int GetOffsetAt(int x)
        {
            if (x <= FMargin)
                return HC.OffsetBefor;
            else
                if (x >= Width - FMargin)
                    return HC.OffsetAfter;
                else
                    return HC.OffsetInner;
        }

        protected override void SetActive(bool value)
        {
            base.SetActive(value);
            if (!value)
                FCaretOffset = -1;
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

        public override bool MouseDown(MouseEventArgs e)
        {
            bool vResult = base.MouseDown(e);
            OwnerData.Style.ApplyTempStyle(TextStyleNo);
            int vX = e.X - FMargin;// - (Width - FMargin - OwnerData.Style.DefCanvas.TextWidth(FText) - FMargin) div 2;
            short vOffset = (short)HC.GetNorAlignCharOffsetAt(OwnerData.Style.TempCanvas, FText, vX);
            if (vOffset != FCaretOffset)
            {
                FCaretOffset = vOffset;
                OwnerData.Style.UpdateInfoReCaret();
            }

            return vResult;
        }
        
        /// <summary> 正在其上时内部是否处理指定的Key和Shif </summary>
        public override bool WantKeyDown(KeyEventArgs e)
        {
            bool vResult = false;

            if (e.KeyCode == Keys.Left)
            {
                if (FCaretOffset == 0)
                {

                }
                else
                if (FCaretOffset < 0)
                {
                    FCaretOffset = (short)FText.Length;
                    vResult = true;
                }
                else
                    vResult = true;
            }
            else
            if (e.KeyCode == Keys.Right)
            {
                if (FCaretOffset == FText.Length)
                {

                }
                else
                if (FCaretOffset < 0)
                {
                    FCaretOffset = 0;
                    vResult = true;
                }
                else
                    vResult = true;
            }
            else
                vResult = true;

            return vResult;
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

                        this.SizeChanged = true;
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

        public override void KeyPress(ref Char key)
        {
            if (!FReadOnly)
            {
                FCaretOffset++;
                FText = FText.Insert(FCaretOffset - 1, key.ToString());

                this.SizeChanged = true;
            }
            else
                base.KeyPress(ref key);
        }

        public override bool InsertText(string aText)
        {
            FText = FText.Insert(FCaretOffset, aText);
            FCaretOffset += (short)aText.Length;
            this.SizeChanged = true;
            return true;
        }

        public override bool InsertStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            if (OwnerData.Style.States.Contain(HCState.hosPasting))
                return InsertText(Clipboard.GetText());
            else
                return false;
        }

        public override void GetCaretInfo(ref HCCaretInfo aCaretInfo)
        {
            if (FCaretOffset < 0)
            {
                aCaretInfo.Visible = false;
                return;
            }

            string vS = FText.Substring(0, FCaretOffset);
            OwnerData.Style.ApplyTempStyle(TextStyleNo);
            
            if (vS != "")
            {
                SIZE vSize = OwnerData.Style.TempCanvas.TextExtent(vS);
                aCaretInfo.Height = vSize.cy;
                aCaretInfo.X = FMargin + vSize.cx;// + (Width - FMargin - OwnerData.Style.DefCanvas.TextWidth(FText) - FMargin) div 2;
            }
            else
            {
                aCaretInfo.Height = OwnerData.Style.TextStyles[TextStyleNo].FontHeight;
                aCaretInfo.X = FMargin;// + (Width - FMargin - OwnerData.Style.DefCanvas.TextWidth(FText) - FMargin) div 2;
            }
            
            aCaretInfo.Y = FMargin;

            if ((!this.AutoSize) && (aCaretInfo.X > Width))
                aCaretInfo.Visible = false;
        }

        protected override string GetText()
        {
            return FText;
        }

        protected override void SetText(string value)
        {
            if ((!FReadOnly) && (FText != value))
            {
                FText = value;
                if (FCaretOffset > FText.Length)
                    FCaretOffset = 0;

                if (this.AutoSize)
                    (OwnerData as HCFormatData).ItemRequestFormat(this);
                else
                    OwnerData.Style.UpdateInfoRePaint();
            }
        }

        public HCEditItem(HCCustomData aOwnerData, string aText)
            : base(aOwnerData)
        {
            this.StyleNo = HCStyle.Edit;
            FText = aText;
            FMouseIn = false;
            FMargin = 4;
            FCaretOffset = -1;
            Width = 50;
            FPrintOnlyText = false;
            FBorderWidth = 1;
            FBorderSides = new HCBorderSides();
            FBorderSides.InClude((byte)BorderSide.cbsLeft);
            FBorderSides.InClude((byte)BorderSide.cbsTop);
            FBorderSides.InClude((byte)BorderSide.cbsRight);
            FBorderSides.InClude((byte)BorderSide.cbsBottom);
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FText = (source as HCEditItem).Text;
            FReadOnly = (source as HCEditItem).ReadOnly;
            FPrintOnlyText = (source as HCEditItem).PrintOnlyText;
            FBorderSides.Value = (source as HCEditItem).BorderSides.Value;
            FBorderWidth = (source as HCEditItem).BorderWidth;
        }

        public override void Clear()
        {
            this.Text = "";
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            HC.HCSaveTextToStream(aStream, FText);  // 存Text

            byte vByte = 0;
            if (FReadOnly)
                vByte = (byte)(vByte | (1 << 7));

            if (FPrintOnlyText)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            aStream.WriteByte(FBorderSides.Value);
            aStream.WriteByte(FBorderWidth);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            HC.HCLoadTextFromStream(aStream, ref FText, aFileVersion);

            if (aFileVersion > 33)
            {
                byte vByte = (byte)aStream.ReadByte();
                FReadOnly = HC.IsOdd(vByte >> 7);
                FPrintOnlyText = HC.IsOdd(vByte >> 6);
            }
            else
            {
                byte[] vBuffer = BitConverter.GetBytes(FReadOnly);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FReadOnly = BitConverter.ToBoolean(vBuffer, 0);
                FPrintOnlyText = false;
            }


            if (aFileVersion > 15)
            {
                FBorderSides.Value = (byte)aStream.ReadByte();
                FBorderWidth = (byte)aStream.ReadByte();
            }
        }

        public override void ToXml(System.Xml.XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FReadOnly)
                aNode.SetAttribute("readonly", "1");

            if (FPrintOnlyText)
                aNode.SetAttribute("printonlytext", "1");

            aNode.SetAttribute("border", HC.GetBorderSidePro(FBorderSides));
            aNode.SetAttribute("borderwidth", FBorderWidth.ToString());
            aNode.InnerText = FText;
        }

        public override void ParseXml(System.Xml.XmlElement aNode)
        {
            base.ParseXml(aNode);
            FReadOnly = bool.Parse(aNode.Attributes["readonly"].Value);
            FPrintOnlyText = bool.Parse(aNode.Attributes["printonlytext"].Value);
            HC.SetBorderSideByPro(aNode.Attributes["border"].Value, FBorderSides);
            FBorderWidth = byte.Parse(aNode.Attributes["borderwidth"].Value);
            FText = aNode.InnerText;
        }

        public bool ReadOnly
        {
            get { return FReadOnly; }
            set { FReadOnly = value; }
        }

        public bool PrintOnlyText
        {
            get { return FPrintOnlyText; }
            set { FPrintOnlyText = value; }
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
