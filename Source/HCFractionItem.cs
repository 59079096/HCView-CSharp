/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{       文档FractionItem(上下分数类)对象实现单元        }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using HC.Win32;
using System.Drawing;

namespace HC.View
{
    public class HCFractionItem : HCTextRectItem  // 分数(上、下文本，分数线)
    {
        private string FTopText, FBottomText;
        private RECT FTopRect, FBottomRect;
        private byte FPadding;
        private bool FLineHide;
        protected short FCaretOffset;
        protected bool FMouseLBDowning, FOutSelectInto;
        protected ExpressArea FActiveArea, FMouseMoveArea;

        public override void FormatToDrawItem(HCCustomData ARichData, int AItemNo)
        {
            HCStyle vStyle = ARichData.Style;
            vStyle.TextStyles[TextStyleNo].ApplyStyle(vStyle.DefCanvas);
            int vH = vStyle.DefCanvas.TextHeight("H");
            int vTopW = Math.Max(vStyle.DefCanvas.TextWidth(FTopText), FPadding);
            int vBottomW = Math.Max(vStyle.DefCanvas.TextWidth(FBottomText), FPadding);
            // 计算尺寸
            if (vTopW > vBottomW)
                Width = vTopW + 4 * FPadding;
            else
                Width = vBottomW + 4 * FPadding;
    
            Height = vH * 2 + 4 * FPadding;
    
            // 计算各字符串位置
    
            FTopRect = HC.Bounds(FPadding + (Width - FPadding - FPadding - vTopW) / 2,
                FPadding, vTopW, vH);
            FBottomRect = HC.Bounds(FPadding + (Width - FPadding - FPadding - vBottomW) / 2,
                Height - FPadding - vH, vBottomW, vH);
        }

        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, int ADataDrawTop, int ADataDrawBottom, 
            int ADataScreenTop, int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (this.Active && (!APaintInfo.Print))
            {
                ACanvas.Brush.Color = HC.clBtnFace;
                ACanvas.FillRect(ADrawRect);
            }

            AStyle.TextStyles[TextStyleNo].ApplyStyle(ACanvas, APaintInfo.ScaleY / APaintInfo.Zoom);
            ACanvas.TextOut(ADrawRect.Left + FTopRect.Left, ADrawRect.Top + FTopRect.Top, FTopText);
            ACanvas.TextOut(ADrawRect.Left + FBottomRect.Left, ADrawRect.Top + FBottomRect.Top, FBottomText);
            
            if (!FLineHide)
            {
                ACanvas.Pen.Color = Color.Black;
                ACanvas.MoveTo(ADrawRect.Left + FPadding, ADrawRect.Top + FTopRect.Bottom + FPadding);
                ACanvas.LineTo(ADrawRect.Left + Width - FPadding, ADrawRect.Top + FTopRect.Bottom + FPadding);
            }

            if (!APaintInfo.Print)
            {
                RECT vFocusRect = new RECT();

                if (FActiveArea != ExpressArea.ceaNone)
                {
                    if (FActiveArea == ExpressArea.ceaTop)
                        vFocusRect = FTopRect;
                    else
                        if (FActiveArea == ExpressArea.ceaBottom)
                            vFocusRect = FBottomRect;

                    vFocusRect.Offset(ADrawRect.Left, ADrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    ACanvas.Pen.Color = Color.Blue;
                    ACanvas.Rectangle(vFocusRect);
                }

                if ((FMouseMoveArea != ExpressArea.ceaNone) && (FMouseMoveArea != FActiveArea))
                {
                    if (FMouseMoveArea == ExpressArea.ceaTop)
                        vFocusRect = FTopRect;
                    else
                        if (FMouseMoveArea == ExpressArea.ceaBottom)
                            vFocusRect = FBottomRect;

                    vFocusRect.Offset(ADrawRect.Left, ADrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    ACanvas.Pen.Color = HC.clMedGray;
                    ACanvas.Rectangle(vFocusRect);
                }
            }
        }

        public override int GetOffsetAt(int X)
        {
            if (FOutSelectInto)
                return base.GetOffsetAt(X);
            else
            {
                if (X <= 0)
                    return HC.OffsetBefor;
                else
                    if (X >= Width)
                        return HC.OffsetAfter;
                    else
                        return HC.OffsetInner;
            }
        }

        protected override void SetActive(bool Value)
        {
            base.SetActive(Value);
            if (!Value)
                FActiveArea = ExpressArea.ceaNone;
        }

        public override void MouseLeave()
        {
            base.MouseLeave();
            FMouseMoveArea = ExpressArea.ceaNone;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
          
            FMouseLBDowning = (e.Button == MouseButtons.Left);
            FOutSelectInto = false;
            
            if (FMouseMoveArea != FActiveArea)
            {
                FActiveArea = FMouseMoveArea;
                OwnerData.Style.UpdateInfoReCaret();
            }

            string vS = "";
            int vX = 0;

            if (FActiveArea == ExpressArea.ceaTop)
            {
                vS = FTopText;
                vX = e.X - FTopRect.Left;
            }
            else 
            if (FActiveArea == ExpressArea.ceaBottom)
            {
                vS = FBottomText;
                vX = e.X - FBottomRect.Left;
            }
            
            int vOffset = -1;
            if (FActiveArea != ExpressArea.ceaNone)
            {
                OwnerData.Style.TextStyles[TextStyleNo].ApplyStyle(OwnerData.Style.DefCanvas);
                vOffset = HC.GetCharOffsetByX(OwnerData.Style.DefCanvas, vS, vX);
            }

            if (vOffset != FCaretOffset)
            {
                FCaretOffset = (short)vOffset;
                OwnerData.Style.UpdateInfoReCaret();
            }
        }

        public override void MouseMove(MouseEventArgs e)
        {
            if ((!FMouseLBDowning) && (e.Button == MouseButtons.Left))
                FOutSelectInto = true;
            
            if (!FOutSelectInto)
            {
                ExpressArea vArea = GetExpressArea(e.X, e.Y);
                if (vArea != FMouseMoveArea)
                {
                    FMouseMoveArea = vArea;
                    OwnerData.Style.UpdateInfoRePaint();
                }
            }
            else
                FMouseMoveArea = ExpressArea.ceaNone;

            base.MouseMove(e);
        }

        public override void MouseUp(MouseEventArgs e)
        {
            FMouseLBDowning = false;
            FOutSelectInto = false;
            base.MouseUp(e);
        }

        /// <summary> 正在其上时内部是否处理指定的Key和Shif </summary>
        public override bool WantKeyDown(KeyEventArgs e)
        {
            return true;
        }

        public override void KeyDown(KeyEventArgs e)
        {
            switch (e.KeyValue)
            {
                case User.VK_BACK:
                    if (FActiveArea == ExpressArea.ceaTop)
                    {
                        if (FCaretOffset > 0)
                        {
                            FTopText = FTopText.Remove(FCaretOffset - 1, 1);
                            FCaretOffset--;
                        }
                    }
                    else
                    if (FActiveArea == ExpressArea.ceaBottom)
                    {
                        if (FCaretOffset > 0)
                        {
                            FBottomText = FBottomText.Remove(FCaretOffset - 1, 1);
                            FCaretOffset--;
                        }
                    }

                    this.SizeChanged = true;
                    break;

                case User.VK_LEFT:
                    if (FCaretOffset > 0)
                        FCaretOffset--;
                    break;

                case User.VK_RIGHT:
                    string vS = "";
                    if (FActiveArea == ExpressArea.ceaTop)
                        vS = FTopText;
                    else
                        if (FActiveArea == ExpressArea.ceaBottom)
                            vS = FBottomText;

                    if (FCaretOffset < vS.Length)
                        FCaretOffset++;
                    break;

                case User.VK_DELETE:
                    if (FActiveArea == ExpressArea.ceaTop)
                    {
                        if (FCaretOffset < FTopText.Length)
                            FTopText = FTopText.Remove(FCaretOffset, 1);
                    }
                    else
                    if (FActiveArea == ExpressArea.ceaBottom)
                    {
                        if (FCaretOffset < FBottomText.Length)
                            FBottomText = FBottomText.Remove(FCaretOffset, 1);
                    }
                    this.SizeChanged = true;

                    break;

                case User.VK_HOME:
                    FCaretOffset = 0;
                    break;

                case User.VK_END:
                    if (FActiveArea == ExpressArea.ceaTop)
                        FCaretOffset = (short)FTopText.Length;
                    else
                        if (FActiveArea == ExpressArea.ceaBottom)
                            FCaretOffset = (short)FBottomText.Length;

                    break;
            }
        }

        public override void KeyPress(ref Char Key)
        {
            if (FActiveArea != ExpressArea.ceaNone)
                InsertText(Key.ToString());
            else
                Key = (Char)0;
        }

        public override bool InsertText(string AText)
        {
            if (FActiveArea != ExpressArea.ceaNone)
            {
                if (FActiveArea == ExpressArea.ceaTop)
                    FTopText = FTopText.Insert(FCaretOffset, AText);
                else
                    if (FActiveArea == ExpressArea.ceaBottom)
                        FBottomText = FBottomText.Insert(FCaretOffset, AText);

                FCaretOffset += (short)AText.Length;

                this.SizeChanged = true;
                return true;
            }
            else
                return false;
        }

        public override void GetCaretInfo(ref HCCaretInfo ACaretInfo)
        {
            if ((FActiveArea != ExpressArea.ceaNone) && (FCaretOffset >= 0))
            {
                OwnerData.Style.TextStyles[TextStyleNo].ApplyStyle(OwnerData.Style.DefCanvas);
                if (FActiveArea == ExpressArea.ceaTop)
                {
                    if (FCaretOffset < 0)
                        FCaretOffset = 0;

                    ACaretInfo.Height = FTopRect.Bottom - FTopRect.Top;
                    ACaretInfo.X = FTopRect.Left + OwnerData.Style.DefCanvas.TextWidth(FTopText.Substring(0, FCaretOffset));
                    ACaretInfo.Y = FTopRect.Top;
                }
                else
                    if (FActiveArea == ExpressArea.ceaBottom)
                    {
                        if (FCaretOffset < 0)
                            FCaretOffset = 0;

                        ACaretInfo.Height = FBottomRect.Bottom - FBottomRect.Top;
                        ACaretInfo.X = FBottomRect.Left + OwnerData.Style.DefCanvas.TextWidth(FBottomText.Substring(0, FCaretOffset));
                        ACaretInfo.Y = FBottomRect.Top;
                    }
            }
            else
                ACaretInfo.Visible = false;
        }

        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            int vLen = System.Text.Encoding.Default.GetByteCount(FTopText);
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            ushort vSize = (ushort)vLen;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            if (vSize > 0)
            {
                vBuffer = System.Text.Encoding.Default.GetBytes(FTopText);
                AStream.Write(vBuffer, 0, vBuffer.Length);
            }

            vLen = System.Text.Encoding.Default.GetByteCount(FBottomText);
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            vSize = (ushort)vLen;
            vBuffer = BitConverter.GetBytes(vSize);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            if (vSize > 0)
            {
                vBuffer = System.Text.Encoding.Default.GetBytes(FBottomText);
                AStream.Write(vBuffer, 0, vBuffer.Length);
            }
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);
            HC.HCLoadTextFromStream(AStream, ref FTopText);
            HC.HCLoadTextFromStream(AStream, ref FBottomText);
        }

        protected virtual ExpressArea GetExpressArea(int X, int Y)
        {
            POINT vPt = new POINT(X, Y);
            if (HC.PtInRect(FTopRect, vPt))
                return ExpressArea.ceaTop;
            else
                if (HC.PtInRect(FBottomRect, vPt))
                    return ExpressArea.ceaBottom;
                else
                    return ExpressArea.ceaNone;
        }

        protected RECT TopRect
        {
            get { return FTopRect; }
            set { FTopRect = value; }
        }

        protected RECT BottomRect
        {
            get { return FBottomRect; }
            set { FBottomRect = value; }
        }

        public HCFractionItem(HCCustomData AOwnerData, string ATopText, string ABottomText) : base(AOwnerData)
        {
            this.StyleNo = HCStyle.Fraction;
            FPadding = 5;
            FActiveArea = ExpressArea.ceaNone;
            FCaretOffset = -1;
            FLineHide = false;
            FTopText = ATopText;
            FBottomText = ABottomText;
        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            FTopText = (Source as HCFractionItem).TopText;
            FBottomText = (Source as HCFractionItem).BottomText;
        }

        public byte Padding
        {
            get { return FPadding; }
        }

        public bool LineHide
        {
            get { return FLineHide; }
            set { FLineHide = value; }
        }

        public string TopText
        {
            get { return FTopText; }
            set { FTopText = value; }
        }

        public string BottomText
        {
            get { return FBottomText; }
            set { FBottomText = value; }
        }
    }
}
