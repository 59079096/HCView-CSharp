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
        private short FCaretOffset, FSelEnd = -1, FSelMove = -1;
        private int FLeftOffset = 0;
        private SIZE FTextSize;

        private void CalcTextSize()
        {
            OwnerData.Style.ApplyTempStyle(TextStyleNo);
            if (FText != "")
                FTextSize = OwnerData.Style.TempCanvas.TextExtent(FText);
            else
                FTextSize = OwnerData.Style.TempCanvas.TextExtent("H");
        }
        private void ScrollAdjust(int offset)
        {
            if (this.AutoSize)
            {
                FLeftOffset = 0;
                return;
            }

            if (FTextSize.cx + FPaddingLeft <= Width - FPaddingRight)
            {
                FLeftOffset = 0;
                return;
            }

            if (FTextSize.cx + FPaddingLeft - FLeftOffset < Width - FPaddingRight)
            {
                FLeftOffset = FLeftOffset - (Width - FPaddingLeft - FTextSize.cx + FLeftOffset - FPaddingRight);
                return;
            }

            OwnerData.Style.ApplyTempStyle(TextStyleNo);
            string vText = FText.Substring(0, offset);
            int vRight = OwnerData.Style.TempCanvas.TextWidth(vText) + FPaddingLeft - FLeftOffset;
            if (vRight > Width - FPaddingRight)
                FLeftOffset = FLeftOffset + vRight - Width + FPaddingRight;
            else
            if (vRight < 0)
                FLeftOffset = FLeftOffset + vRight;
        }

        private int GetCharDrawLeft(int offset)
        {
            int vResult = 0;
            if (offset > 0)
            {
                if (offset == FText.Length)
                    vResult = Width;
                else
                {
                    OwnerData.Style.ApplyTempStyle(TextStyleNo);
                    vResult = FPaddingLeft + OwnerData.Style.TempCanvas.TextWidth(FText.Substring(0, offset)) - FLeftOffset;
                }
            }

            return vResult;
        }

        private bool OffsetInSelect(int offset)
        {
            return (offset >= FCaretOffset) && (offset <= FSelEnd);
        }

        private void DeleteSelectText()
        {
            FText = FText.Remove(FCaretOffset, FSelEnd - FCaretOffset);
            FSelEnd = -1;
            FSelMove = FCaretOffset;
            CalcTextSize();
        }

        private void DisSelectText()
        {
            FSelMove = FCaretOffset;
            if (SelectTextExists())
                FSelEnd = -1;
        }

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            CalcTextSize();

            if (this.AutoSize)
            {
                Width = FPaddingLeft + FTextSize.cx + FPaddingRight;  // 间距
                Height = FPaddingTop + FTextSize.cy + FPaddingBottom;
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

            if (!aPaintInfo.Print)
            {
                if (this.IsSelectComplate)
                {
                    aCanvas.Brush.Color = aStyle.SelColor;
                    aCanvas.FillRect(aDrawRect);
                }
                else
                if (SelectTextExists())
                {
                    aCanvas.Brush.Color = aStyle.SelColor;
                    int vLeft = GetCharDrawLeft(FCaretOffset);
                    int vRight = GetCharDrawLeft(FSelEnd);
                    vLeft = Math.Max(0, Math.Min(vLeft, Width));
                    vRight = Math.Max(0, Math.Min(vRight, Width));
                    aCanvas.FillRect(new RECT(aDrawRect.Left + vLeft, aDrawRect.Top, aDrawRect.Left + vRight, aDrawRect.Bottom));
                }
            }

            aStyle.TextStyles[TextStyleNo].ApplyStyle(aCanvas, aPaintInfo.ScaleY / aPaintInfo.Zoom);

            if (!this.AutoSize)
                aCanvas.TextRect(aDrawRect, aDrawRect.Left + FPaddingLeft - FLeftOffset, aDrawRect.Top + FPaddingTop, FText);
            else
                aCanvas.TextOut(aDrawRect.Left + FPaddingLeft, aDrawRect.Top + FPaddingTop, FText);

            if (aPaintInfo.Print && FPrintOnlyText)
                return;

            if (FBorderSides.Value > 0)
            {
                if (FMouseIn || Active)
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
        }

        public override int GetOffsetAt(int x)
        {
            if (x <= FPaddingLeft)
                return HC.OffsetBefor;
            else
                if (x >= Width - FPaddingRight)
                    return HC.OffsetAfter;
                else
                    return HC.OffsetInner;
        }

        protected override void SetActive(bool value)
        {
            base.SetActive(value);
            if (!value)
            {
                DisSelectText();
                FLeftOffset = 0;
                FCaretOffset = -1;
            }
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
            if (!this.Active)
                return vResult;

            OwnerData.Style.ApplyTempStyle(TextStyleNo);
            short vOffset = (short)HC.GetNorAlignCharOffsetAt(OwnerData.Style.TempCanvas, FText, e.X - FPaddingLeft + FLeftOffset);
            if (e.Button == MouseButtons.Left)
                DisSelectText();
            else
            {
                if (!OffsetInSelect(vOffset))
                    DisSelectText();
                else
                    return vResult;
            }

            if (vOffset != FCaretOffset)
            {
                FCaretOffset = vOffset;
                FSelMove = vOffset;
                ScrollAdjust(vOffset);
                OwnerData.Style.UpdateInfoReCaret();
            }

            return vResult;
        }

        public override bool MouseMove(MouseEventArgs e)
        {
            bool vResult = base.MouseMove(e);
            if (e.Button == MouseButtons.Left)
            {
                if (e.X < 0)
                    FLeftOffset = (short)Math.Max(0, FLeftOffset - OwnerData.Style.TextStyles[TextStyleNo].TextMetric_tmAveCharWidth);
                else
                if (e.X > Width - FPaddingRight)
                    FLeftOffset = (short)Math.Max(0, Math.Min(FTextSize.cx - Width + FPaddingRight, FLeftOffset + OwnerData.Style.TextStyles[TextStyleNo].TextMetric_tmAveCharWidth));

                FSelEnd = (short)HC.GetNorAlignCharOffsetAt(OwnerData.Style.TempCanvas, FText, e.X - FPaddingLeft + FLeftOffset);
                FSelMove = FSelEnd;
                if (!SelectTextExists() && (FSelEnd >= 0))  // 回到同一位置
                {
                    FSelEnd = -1;
                    FSelMove = FCaretOffset;
                }

                ScrollAdjust(FSelMove);
            }

            return vResult;
        }

        public override bool MouseUp(MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (FSelEnd >= 0) && (FSelEnd < FCaretOffset))
            {
                short vSel = FCaretOffset;
                FCaretOffset = FSelEnd;
                FSelEnd = vSel;
            }

            if (OwnerData.Style.UpdateInfo.Draging)
                this.DisSelect();

            return base.MouseUp(e);
        }

        /// <summary> 正在其上时内部是否处理指定的Key和Shif </summary>
        public override bool WantKeyDown(KeyEventArgs e)
        {
            bool vResult = false;

            if (e.KeyCode == Keys.Left)
            {
                if (FCaretOffset == 0)
                    FCaretOffset = -1;
                else
                if (FCaretOffset < 0)
                {
                    FCaretOffset = (short)FText.Length;
                    ScrollAdjust(FCaretOffset);
                    OwnerData.Style.UpdateInfoRePaint();
                    vResult = true;
                }
                else
                    vResult = true;
            }
            else
            if (e.KeyCode == Keys.Right)
            {
                if (FCaretOffset == FText.Length)
                    FCaretOffset = -1;
                else
                if (FCaretOffset < 0)
                {
                    FCaretOffset = 0;
                    ScrollAdjust(FCaretOffset);
                    OwnerData.Style.UpdateInfoRePaint();
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
                        if (SelectTextExists())
                            DeleteSelectText();
                        else
                        if (FCaretOffset > 0)
                        {
                            FText = FText.Remove(FCaretOffset - 1, 1);
                            FCaretOffset--;
                            CalcTextSize();
                        }

                        ScrollAdjust(FCaretOffset);
                        this.SizeChanged = true;
                        break;

                    case User.VK_LEFT:
                        DisSelectText();
                        if (FCaretOffset > 0)
                            FCaretOffset--;

                        ScrollAdjust(FCaretOffset);
                        OwnerData.Style.UpdateInfoRePaint();
                        break;

                    case User.VK_RIGHT:
                        DisSelectText();
                        if (FCaretOffset < FText.Length)
                            FCaretOffset++;

                        ScrollAdjust(FCaretOffset);
                        OwnerData.Style.UpdateInfoRePaint();
                        break;

                    case User.VK_DELETE:
                        if (SelectTextExists())
                            DeleteSelectText();
                        else
                        if (FCaretOffset < FText.Length)
                        {
                            FText = FText.Remove(FCaretOffset, 1);
                            CalcTextSize();
                        }

                        ScrollAdjust(FCaretOffset);
                        this.SizeChanged = true;
                        break;

                    case User.VK_HOME:
                        FCaretOffset = 0;
                        ScrollAdjust(FCaretOffset);
                        break;

                    case User.VK_END:
                        FCaretOffset = (short)FText.Length;
                        ScrollAdjust(FCaretOffset);
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
                if (SelectTextExists())
                    DeleteSelectText();

                FText = FText.Insert(FCaretOffset, key.ToString());
                FCaretOffset++;
                ScrollAdjust(FCaretOffset);
                this.SizeChanged = true;
            }
            else
                base.KeyPress(ref key);
        }

        public override bool InsertText(string aText)
        {
            FText = FText.Insert(FCaretOffset, aText);
            FCaretOffset += (short)aText.Length;
            CalcTextSize();
            ScrollAdjust(FCaretOffset);
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

            if (SelectTextExists())
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
                aCaretInfo.X = FPaddingLeft - FLeftOffset + vSize.cx;// + (Width - FMargin - OwnerData.Style.DefCanvas.TextWidth(FText) - FMargin) div 2;
            }
            else
            {
                aCaretInfo.Height = OwnerData.Style.TextStyles[TextStyleNo].FontHeight;
                aCaretInfo.X = FPaddingLeft;// + (Width - FMargin - OwnerData.Style.DefCanvas.TextWidth(FText) - FMargin) div 2;
            }
            
            aCaretInfo.Y = FPaddingLeft;

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
            FPaddingLeft = 4;
            FPaddingRight = 4;
            FPaddingTop = 4;
            FPaddingBottom = 4;
            FCaretOffset = -1;
            Width = 50;
            FPrintOnlyText = true;
            FBorderWidth = 1;
            FBorderSides = new HCBorderSides();
            FBorderSides.InClude((byte)BorderSide.cbsLeft);
            FBorderSides.InClude((byte)BorderSide.cbsTop);
            FBorderSides.InClude((byte)BorderSide.cbsRight);
            FBorderSides.InClude((byte)BorderSide.cbsBottom);
        }

        public bool SelectTextExists()
        {
            return (FSelEnd >= 0) && (FSelEnd != FCaretOffset);
        }

        public override bool CoordInSelect(int x, int y)
        {
            return SelectExists() && HC.PtInRect(HC.Bounds(0, 0, Width, Height), x, y);
        }

        public override bool SelectExists()
        {
            return base.SelectExists() || SelectTextExists();
        }

        public override bool IsSelectComplateTheory()
        {
            return IsSelectComplate;
        }

        public override bool DeleteSelected()
        {
            bool vResult = base.DeleteSelected();
            if (SelectTextExists())
            {
                DeleteSelectText();
                vResult = true;
            }

            return vResult;
        }

        public override void DisSelect()
        {
            DisSelectText();
            base.DisSelect();
        }

        public override string SaveSelectToText()
        {
            if (SelectTextExists())
                return FText.Substring(FCaretOffset + 1 - 1, FSelEnd - FCaretOffset);
            else
                return base.SaveSelectToText();
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
