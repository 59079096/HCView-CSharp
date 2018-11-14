/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-9-29             }
{                                                       }
{       文档SupSubScript(同时上下标)对象实现单元        }
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
    public class HCSupSubScriptItem : HCTextRectItem  // 分数(上、下文本，分数线)
    {
        private string FSupText, FSubText;
        private RECT FSupRect, FSubRect;
        private short FCaretOffset, FPadding;
        private bool FMouseLBDowning, FOutSelectInto;
        private ExpressArea FActiveArea, FMouseMoveArea;

        private void ApplySupSubStyle(HCTextStyle ATextStyle, HCCanvas ACanvas, Single AScale = 1)
        {
            if (ATextStyle.BackColor == Color.Transparent)
                ACanvas.Brush.Style = HCBrushStyle.bsClear;
            else
                ACanvas.Brush.Color = ATextStyle.BackColor;
            
            ACanvas.Font.BeginUpdate();
            try
            {
                ACanvas.Font.Color = ATextStyle.Color;
                ACanvas.Font.Family = ATextStyle.Family;
                ACanvas.Font.Size = (int)Math.Round(ATextStyle.Size * 2 / 3f);
                if (ATextStyle.FontStyles.Contains((byte)HCFontStyle.tsBold))
                    ACanvas.Font.FontStyles.InClude((byte)HCFontStyle.tsBold);
                else
                    ACanvas.Font.FontStyles.ExClude((byte)HCFontStyle.tsBold);
            
                if (ATextStyle.FontStyles.Contains((byte)HCFontStyle.tsItalic))
                    ACanvas.Font.FontStyles.InClude((byte)HCFontStyle.tsItalic);
                else
                    ACanvas.Font.FontStyles.ExClude((byte)HCFontStyle.tsItalic);
            
                if (ATextStyle.FontStyles.Contains((byte)HCFontStyle.tsUnderline))
                    ACanvas.Font.FontStyles.InClude((byte)HCFontStyle.tsUnderline);
                else
                    ACanvas.Font.FontStyles.ExClude((byte)HCFontStyle.tsUnderline);
            
                if (ATextStyle.FontStyles.Contains((byte)HCFontStyle.tsStrikeOut))
                    ACanvas.Font.FontStyles.InClude((byte)HCFontStyle.tsStrikeOut);
                else
                    ACanvas.Font.FontStyles.ExClude((byte)HCFontStyle.tsStrikeOut);
            }
            finally
            {
                ACanvas.Font.EndUpdate();
            }
        }

        public override void FormatToDrawItem(HCCustomData ARichData, int AItemNo)
        {
            HCStyle vStyle = ARichData.Style;
            ApplySupSubStyle(vStyle.TextStyles[TextStyleNo], vStyle.DefCanvas);
            int vH = vStyle.DefCanvas.TextHeight("H");
            int vTopW = Math.Max(vStyle.DefCanvas.TextWidth(FSupText), FPadding);
            int vBottomW = Math.Max(vStyle.DefCanvas.TextWidth(FSubText), FPadding);
            // 计算尺寸
            if (vTopW > vBottomW)
                Width = vTopW + 4 * FPadding;
            else
                Width = vBottomW + 4 * FPadding;
    
            Height = vH * 2 + 4 * FPadding;
    
            // 计算各字符串位置
            FSupRect = HC.Bounds(FPadding, FPadding, vTopW, vH);
            FSubRect = HC.Bounds(FPadding, Height - FPadding - vH, vBottomW, vH);
        }

        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, int ADataDrawTop, int ADataDrawBottom, 
            int ADataScreenTop, int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (this.Active && (!APaintInfo.Print))
            {
                ACanvas.Brush.Color = HC.clBtnFace;
                ACanvas.FillRect(ADrawRect);
            }
            
            ApplySupSubStyle(AStyle.TextStyles[TextStyleNo], ACanvas, APaintInfo.ScaleY / APaintInfo.Zoom);
            ACanvas.TextOut(ADrawRect.Left + FSupRect.Left, ADrawRect.Top + FSupRect.Top, FSupText);
            ACanvas.TextOut(ADrawRect.Left + FSubRect.Left, ADrawRect.Top + FSubRect.Top, FSubText);
            
            if (!APaintInfo.Print)
            {
                RECT vFocusRect = new RECT();
                if (FActiveArea != ExpressArea.ceaNone)
                {
                    if (FActiveArea == ExpressArea.ceaTop)
                        vFocusRect = FSupRect;
                    else
                    if (FActiveArea == ExpressArea.ceaBottom)
                        vFocusRect = FSubRect;
                
                    vFocusRect.Offset(ADrawRect.Left, ADrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    ACanvas.Pen.Color = Color.Blue;
                    ACanvas.Rectangle(vFocusRect);
                }
            
                if ((FMouseMoveArea != ExpressArea.ceaNone) && (FMouseMoveArea != FActiveArea))
                {
                    if (FMouseMoveArea == ExpressArea.ceaTop)
                        vFocusRect = FSupRect;
                    else
                    if (FMouseMoveArea == ExpressArea.ceaBottom)
                        vFocusRect = FSubRect;
                
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
                vS = FSupText;
                vX = e.X - FSupRect.Left;
            }
            else
            if (FActiveArea == ExpressArea.ceaBottom)
            {
                vS = FSubText;
                vX = e.X - FSubRect.Left;
            }
            
            int vOffset = 0;
            if (FActiveArea != ExpressArea.ceaNone)
            {
                ApplySupSubStyle(OwnerData.Style.TextStyles[TextStyleNo], OwnerData.Style.DefCanvas);
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
                            FSupText = FSupText.Remove(FCaretOffset - 1, 1);
                            FCaretOffset--;
                        }
                    }
                    else
                    if (FActiveArea == ExpressArea.ceaBottom)
                    {
                        if (FCaretOffset > 0)
                        {
                            FSubText = FSubText.Remove(FCaretOffset - 1, 1);
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
                        vS = FSupText;
                    else
                        if (FActiveArea == ExpressArea.ceaBottom)
                            vS = FSubText;

                    if (FCaretOffset < vS.Length)
                        FCaretOffset--;
                    break;

                case User.VK_DELETE:
                    if (FActiveArea == ExpressArea.ceaTop)
                    {
                        if (FCaretOffset < FSupText.Length)
                            FSupText = FSupText.Remove(FCaretOffset, 1);
                    }
                    else
                    if (FActiveArea == ExpressArea.ceaBottom)
                    {
                        if (FCaretOffset < FSubText.Length)
                            FSubText = FSubText.Remove(FCaretOffset, 1);
                    }

                    this.SizeChanged = true;
                    break;

                case User.VK_HOME:
                    FCaretOffset = 0;
                    break;

                case User.VK_END:
                    if (FActiveArea == ExpressArea.ceaTop)
                        FCaretOffset = (short)FSupText.Length;
                    else
                        if (FActiveArea == ExpressArea.ceaBottom)
                            FCaretOffset = (short)FSubText.Length;
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
                    FSupText = FSupText.Insert(FCaretOffset, AText);
                else
                    if (FActiveArea == ExpressArea.ceaBottom)
                        FSubText = FSubText.Insert(FCaretOffset, AText);

                FCaretOffset += (short)AText.Length;
                this.SizeChanged = true;
                return true;
            }
            else
                return false;
        }

        public override void GetCaretInfo(ref HCCaretInfo ACaretInfo)
        {
            if (FActiveArea != ExpressArea.ceaNone)
            {
                ApplySupSubStyle(OwnerData.Style.TextStyles[TextStyleNo], OwnerData.Style.DefCanvas);
                if (FActiveArea == ExpressArea.ceaTop)
                {
                    if (FCaretOffset < 0)
                        FCaretOffset = 0;

                    ACaretInfo.Height = FSupRect.Bottom - FSupRect.Top;
                    ACaretInfo.X = FSupRect.Left + OwnerData.Style.DefCanvas.TextWidth(FSupText.Substring(0, FCaretOffset));
                    ACaretInfo.Y = FSupRect.Top;
                }
                else
                if (FActiveArea == ExpressArea.ceaBottom)
                {
                    if (FCaretOffset < 0)
                        FCaretOffset = 0;

                    ACaretInfo.Height = FSubRect.Bottom - FSubRect.Top;
                    ACaretInfo.X = FSubRect.Left + OwnerData.Style.DefCanvas.TextWidth(FSubText.Substring(0, FCaretOffset));
                    ACaretInfo.Y = FSubRect.Top;
                }
            }
            else
                ACaretInfo.Visible = false;
        }

        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            int vLen = System.Text.Encoding.Default.GetByteCount(FSupText);
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            ushort vSize = (ushort)vLen;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            if (vSize > 0)
            {
                vBuffer = System.Text.Encoding.Default.GetBytes(FSupText);
                AStream.Write(vBuffer, 0, vBuffer.Length);
            }

            vLen = System.Text.Encoding.Default.GetByteCount(FSubText);
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            vSize = (ushort)vLen;
            vBuffer = BitConverter.GetBytes(vSize);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            if (vSize > 0)
            {
                vBuffer = System.Text.Encoding.Default.GetBytes(FSubText);
                AStream.Write(vBuffer, 0, vBuffer.Length);
            }
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);
            HC.HCLoadTextFromStream(AStream, ref FSupText);
            HC.HCLoadTextFromStream(AStream, ref FSubText);
        }

        protected virtual ExpressArea GetExpressArea(int X, int Y)
        {
            ExpressArea Result = ExpressArea.ceaNone;
            POINT vPt = new POINT(X, Y);
            if (HC.PtInRect(FSupRect, vPt))
                Result = ExpressArea.ceaTop;
            else
            if (HC.PtInRect(FSubRect, vPt))
                Result = ExpressArea.ceaBottom;

            return Result;
        }

        protected RECT SupRect
        {
            get { return FSupRect; }
            set { FSupRect = value; }
        }

        protected RECT SubRect
        {
            get { return FSubRect; }
            set { FSubRect = value; }
        }

        public HCSupSubScriptItem(HCCustomData AOwnerData, string ASupText, string ASubText) : base(AOwnerData)
        {
            this.StyleNo = HCStyle.SupSubScript;
            FPadding = 1;
            FActiveArea = ExpressArea.ceaNone;
            FCaretOffset = -1;

            FSupText = ASupText;
            FSubText = ASubText;
        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            FSupText = (Source as HCSupSubScriptItem).SupText;
            FSubText = (Source as HCSupSubScriptItem).SubText;
        }

        public string SupText
        {
            get { return FSupText; }
            set { FSupText = value; }
        }

        public string SubText
        {
            get { return FSubText; }
            set { FSubText = value; }
        }
    }
}