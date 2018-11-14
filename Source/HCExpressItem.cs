/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{        文档ExpressItem(分数类公式)对象实现单元        }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HC.Win32;
using System.Windows.Forms;
using System.Drawing;

namespace HC.View
{
    public class HCExpressItem : HCFractionItem  // 公式(上、下、左、右文本，带分数线)
    {
        private string FLeftText, FRightText;
        private RECT FLeftRect, FRightRect;

        public override void FormatToDrawItem(HCCustomData ARichData, int AItemNo)
        {
            HCStyle vStyle = ARichData.Style;
            vStyle.TextStyles[TextStyleNo].ApplyStyle(vStyle.DefCanvas);
            int vH = vStyle.DefCanvas.TextHeight("H");
            int vLeftW = Math.Max(vStyle.DefCanvas.TextWidth(FLeftText), Padding);
            int vTopW = Math.Max(vStyle.DefCanvas.TextWidth(TopText), Padding);
            int vRightW = Math.Max(vStyle.DefCanvas.TextWidth(FRightText), Padding);
            int vBottomW = Math.Max(vStyle.DefCanvas.TextWidth(BottomText), Padding);
            // 计算尺寸
            if (vTopW > vBottomW)
                Width = vLeftW + vTopW + vRightW + 6 * Padding;
            else
                Width = vLeftW + vBottomW + vRightW + 6 * Padding;
            
            Height = vH * 2 + 4 * Padding;
            
            // 计算各字符串位置
            FLeftRect = HC.Bounds(Padding, (Height - vH) / 2, vLeftW, vH);
            FRightRect = HC.Bounds(Width - Padding - vRightW, (Height - vH) / 2, vRightW, vH);
            TopRect = HC.Bounds(FLeftRect.Right + Padding + (FRightRect.Left - Padding - (FLeftRect.Right + Padding) - vTopW) / 2,
                Padding, vTopW, vH);
            BottomRect = HC.Bounds(FLeftRect.Right + Padding + (FRightRect.Left - Padding - (FLeftRect.Right + Padding) - vBottomW) / 2,
                Height - Padding - vH, vBottomW, vH);
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
            ACanvas.TextOut(ADrawRect.Left + FLeftRect.Left, ADrawRect.Top + FLeftRect.Top, FLeftText);
            ACanvas.TextOut(ADrawRect.Left + TopRect.Left, ADrawRect.Top + TopRect.Top, TopText);
            ACanvas.TextOut(ADrawRect.Left + FRightRect.Left, ADrawRect.Top + FRightRect.Top, FRightText);
            ACanvas.TextOut(ADrawRect.Left + BottomRect.Left, ADrawRect.Top + BottomRect.Top, BottomText);
            
            ACanvas.Pen.Color = Color.Black;
            ACanvas.MoveTo(ADrawRect.Left + FLeftRect.Right + Padding, ADrawRect.Top + TopRect.Bottom + Padding);
            ACanvas.LineTo(ADrawRect.Left + FRightRect.Left - Padding, ADrawRect.Top + TopRect.Bottom + Padding);
            
            if (!APaintInfo.Print)
            {
                RECT vFocusRect = new RECT();

                if (FActiveArea != ExpressArea.ceaNone)
                {
                    switch (FActiveArea)
                    {
                        case ExpressArea.ceaLeft: 
                            vFocusRect = FLeftRect;
                            break;

                        case ExpressArea.ceaTop:
                            vFocusRect = TopRect;
                            break;

                        case ExpressArea.ceaRight: 
                            vFocusRect = FRightRect;
                            break;

                        case ExpressArea.ceaBottom: 
                            vFocusRect = BottomRect;
                            break;
                    }

                    vFocusRect.Offset(ADrawRect.Left, ADrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    ACanvas.Pen.Color = Color.Blue;
                    ACanvas.Rectangle(vFocusRect);
                }

                if ((FMouseMoveArea != ExpressArea.ceaNone) && (FMouseMoveArea != FActiveArea))
                {
                    switch (FMouseMoveArea)
                    {
                        case ExpressArea.ceaLeft: 
                            vFocusRect = FLeftRect;
                            break;

                        case ExpressArea.ceaTop: 
                            vFocusRect = TopRect;
                            break;

                        case ExpressArea.ceaRight: 
                            vFocusRect = FRightRect;
                            break;

                        case ExpressArea.ceaBottom: 
                            vFocusRect = BottomRect;
                            break;
                    }
                
                    vFocusRect.Offset(ADrawRect.Left, ADrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    ACanvas.Pen.Color = HC.clMedGray;
                    ACanvas.Rectangle(vFocusRect);
                }
            }
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
            switch (FActiveArea)
            {   //ceaNone: ;
                case ExpressArea.ceaLeft:
                    vS = FLeftText;
                    vX = e.X - FLeftRect.Left;
                    break;

                case ExpressArea.ceaTop:
                    vS = TopText;
                    vX = e.X - TopRect.Left;
                    break;

                case ExpressArea.ceaRight:
                    vS = FRightText;
                    vX = e.X - FRightRect.Left;
                    break;

                case ExpressArea.ceaBottom:
                    vS = BottomText;
                    vX = e.X - BottomRect.Left;
                    break;
            }

            int vOffset = 0;
            if (FActiveArea != ExpressArea.ceaNone)
            {
                OwnerData.Style.TextStyles[TextStyleNo].ApplyStyle(OwnerData.Style.DefCanvas);
                vOffset = HC.GetCharOffsetByX(OwnerData.Style.DefCanvas, vS, vX);
            }
            else
                vOffset = -1;
            
            if (vOffset != FCaretOffset)
            {
                FCaretOffset = (short)vOffset;
                OwnerData.Style.UpdateInfoReCaret();
            }
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if ((FActiveArea == ExpressArea.ceaLeft) || (FActiveArea == ExpressArea.ceaRight))
            {
                switch (e.KeyValue)
                {
                    case User.VK_BACK:
                        if (FActiveArea == ExpressArea.ceaLeft)
                        {
                            if (FCaretOffset > 0)
                            {
                                FLeftText = FLeftText.Remove(FCaretOffset - 1, 1);
                                FCaretOffset--;
                            }
                        }
                        else
                        {
                            if (FCaretOffset > 0)
                            {
                                FRightText = FRightText.Remove(FCaretOffset - 1, 1);
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
                        string vS = FRightText;
                        if (FActiveArea == ExpressArea.ceaLeft)
                            vS = FLeftText;

                        if (FCaretOffset < vS.Length)
                            FCaretOffset++;
                        break;

                    case User.VK_DELETE:
                        if (FActiveArea == ExpressArea.ceaLeft)
                        {
                            if (FCaretOffset < FLeftText.Length)
                                FLeftText = FLeftText.Remove(FCaretOffset, 1);
                        }
                        else
                        {
                            if (FCaretOffset < FRightText.Length)
                                FRightText = FRightText.Remove(FCaretOffset, 1);
                        }

                        this.SizeChanged = true;
                        break;

                    case User.VK_HOME:
                        FCaretOffset = 0;
                        break;

                    case User.VK_END:
                        if (FActiveArea == ExpressArea.ceaLeft)
                            FCaretOffset = (short)FLeftText.Length;
                        else
                            FCaretOffset = (short)FRightText.Length;

                        break;
                }
            }
            else
                base.KeyDown(e);
        }

        protected override ExpressArea GetExpressArea(int X, int Y)
        {
            ExpressArea Result = base.GetExpressArea(X, Y);
            if (Result == ExpressArea.ceaNone)
            {
                POINT vPt = new POINT(X, Y);
                if (HC.PtInRect(FLeftRect, vPt))
                    return ExpressArea.ceaLeft;
                else
                    if (HC.PtInRect(FRightRect, vPt))
                        return ExpressArea.ceaRight;
                    else
                        return Result;
            }
            else
                return Result;
        }

        public override bool InsertText(string AText)
        {
            if (FActiveArea != ExpressArea.ceaNone)
            {
                switch (FActiveArea)
                {
                    case ExpressArea.ceaLeft:
                        FLeftText = FLeftText.Insert(FCaretOffset, AText);
                        FCaretOffset += (short)AText.Length;
                        this.SizeChanged = true;
                        return true;

                    case ExpressArea.ceaRight:
                        FRightText = FRightText.Insert(FCaretOffset, AText);
                        FCaretOffset += (short)AText.Length;
                        this.SizeChanged = true;
                        return true;

                    default:
                        return base.InsertText(AText);
                }
            }
            else
                return false;
        }

        public override void GetCaretInfo(ref HCCaretInfo ACaretInfo)
        {
            if (FActiveArea != ExpressArea.ceaNone)
            {
                OwnerData.Style.TextStyles[TextStyleNo].ApplyStyle(OwnerData.Style.DefCanvas);
                
                switch (FActiveArea)
                {
                    case ExpressArea.ceaLeft:
                        if (FCaretOffset < 0)
                            FCaretOffset = 0;

                        ACaretInfo.Height = FLeftRect.Bottom - FLeftRect.Top;
                        ACaretInfo.X = FLeftRect.Left + OwnerData.Style.DefCanvas.TextWidth(FLeftText.Substring(0, FCaretOffset));
                        ACaretInfo.Y = FLeftRect.Top;
                        break;
                
                    case ExpressArea.ceaTop:
                        if (FCaretOffset < 0)
                            FCaretOffset = 0;

                        ACaretInfo.Height = TopRect.Bottom - TopRect.Top;
                        ACaretInfo.X = TopRect.Left + OwnerData.Style.DefCanvas.TextWidth(TopText.Substring(0, FCaretOffset));
                        ACaretInfo.Y = TopRect.Top;
                        break;
                
                    case ExpressArea.ceaRight:
                        if (FCaretOffset < 0)
                            FCaretOffset = 0;

                        ACaretInfo.Height = FRightRect.Bottom - FRightRect.Top;
                        ACaretInfo.X = FRightRect.Left + OwnerData.Style.DefCanvas.TextWidth(FRightText.Substring(0, FCaretOffset));
                        ACaretInfo.Y = FRightRect.Top;
                        break;

                    case ExpressArea.ceaBottom:
                        if (FCaretOffset < 0)
                            FCaretOffset = 0;

                        ACaretInfo.Height = BottomRect.Bottom - BottomRect.Top;
                        ACaretInfo.X = BottomRect.Left + OwnerData.Style.DefCanvas.TextWidth(BottomText.Substring(0, FCaretOffset));
                        ACaretInfo.Y = BottomRect.Top;
                        break;
                }
            }
            else
                ACaretInfo.Visible = false;
        }

        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            int vLen = System.Text.Encoding.Default.GetByteCount(FLeftText);
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            ushort vSize = (ushort)vLen;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            if (vSize > 0)
            {
                vBuffer = System.Text.Encoding.Default.GetBytes(FLeftText);
                AStream.Write(vBuffer, 0, vBuffer.Length);
            }

            vLen = System.Text.Encoding.Default.GetByteCount(FRightText);
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            vSize = (ushort)System.Text.Encoding.Default.GetByteCount(FRightText);
            vBuffer = BitConverter.GetBytes(vSize);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            if (vSize > 0)
            {
                vBuffer = System.Text.Encoding.Default.GetBytes(FRightText);
                AStream.Write(vBuffer, 0, vBuffer.Length);
            }
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);
            HC.HCLoadTextFromStream(AStream, ref FLeftText);
            HC.HCLoadTextFromStream(AStream, ref FRightText);
        }

        public HCExpressItem(HCCustomData AOwnerData, string ALeftText, string ATopText, string ARightText, string ABottomText)
            : base(AOwnerData, ATopText, ABottomText)
        {
            this.StyleNo = HCStyle.Express;
            FLeftText = ALeftText;
            FRightText = ARightText;
        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            FLeftText = (Source as HCExpressItem).LeftText;
            FRightText = (Source as HCExpressItem).RightText;
        }

        public RECT LeftRect
        {
            get { return FLeftRect; }
        }

        public RECT RightRect
        {
            get { return FRightRect; }
        }

        public string LeftText
        {
            get { return FLeftText; }
            set { FLeftText = value; }
        }

        public string RightText
        {
            get { return FRightText; }
            set { FRightText = value; }
        }
    }
}
