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
using System.Xml;

namespace HC.View
{
    public class HCSupSubScriptItem : HCTextRectItem  // 分数(上、下文本，分数线)
    {
        private string FSupText, FSubText;
        private RECT FSupRect, FSubRect;
        private short FCaretOffset, FPadding;
        private bool FMouseLBDowning, FOutSelectInto;
        private ExpressArea FActiveArea, FMouseMoveArea;

        // 类内部方法
        private void ApplySupSubStyle(HCTextStyle aTextStyle, HCCanvas aCanvas, Single aScale = 1)
        {
            if (aTextStyle.BackColor == HC.HCTransparentColor)
                aCanvas.Brush.Style = HCBrushStyle.bsClear;
            else
                aCanvas.Brush.Color = aTextStyle.BackColor;
            
            aCanvas.Font.BeginUpdate();
            try
            {
                aCanvas.Font.Color = aTextStyle.Color;
                aCanvas.Font.Family = aTextStyle.Family;
                aCanvas.Font.Size = (int)Math.Round(aTextStyle.Size * 2 / 3f);
                if (aTextStyle.FontStyles.Contains((byte)HCFontStyle.tsBold))
                    aCanvas.Font.FontStyles.InClude((byte)HCFontStyle.tsBold);
                else
                    aCanvas.Font.FontStyles.ExClude((byte)HCFontStyle.tsBold);
            
                if (aTextStyle.FontStyles.Contains((byte)HCFontStyle.tsItalic))
                    aCanvas.Font.FontStyles.InClude((byte)HCFontStyle.tsItalic);
                else
                    aCanvas.Font.FontStyles.ExClude((byte)HCFontStyle.tsItalic);
            
                if (aTextStyle.FontStyles.Contains((byte)HCFontStyle.tsUnderline))
                    aCanvas.Font.FontStyles.InClude((byte)HCFontStyle.tsUnderline);
                else
                    aCanvas.Font.FontStyles.ExClude((byte)HCFontStyle.tsUnderline);
            
                if (aTextStyle.FontStyles.Contains((byte)HCFontStyle.tsStrikeOut))
                    aCanvas.Font.FontStyles.InClude((byte)HCFontStyle.tsStrikeOut);
                else
                    aCanvas.Font.FontStyles.ExClude((byte)HCFontStyle.tsStrikeOut);
            }
            finally
            {
                aCanvas.Font.EndUpdate();
            }
        }

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            HCStyle vStyle = aRichData.Style;
            ApplySupSubStyle(vStyle.TextStyles[TextStyleNo], vStyle.TempCanvas);
            int vH = vStyle.TempCanvas.TextHeight("H");
            int vTopW = Math.Max(vStyle.TempCanvas.TextWidth(FSupText), FPadding);
            int vBottomW = Math.Max(vStyle.TempCanvas.TextWidth(FSubText), FPadding);
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

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (this.Active && (!aPaintInfo.Print))
            {
                aCanvas.Brush.Color = HC.clBtnFace;
                aCanvas.FillRect(aDrawRect);
            }
                      
            if (!aPaintInfo.Print)
            {
                RECT vFocusRect = new RECT();
                if (FActiveArea != ExpressArea.ceaNone)
                {
                    if (FActiveArea == ExpressArea.ceaTop)
                        vFocusRect = FSupRect;
                    else
                    if (FActiveArea == ExpressArea.ceaBottom)
                        vFocusRect = FSubRect;
                
                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Pen.Color = Color.Blue;
                    aCanvas.Rectangle(vFocusRect);
                }
            
                if ((FMouseMoveArea != ExpressArea.ceaNone) && (FMouseMoveArea != FActiveArea))
                {
                    if (FMouseMoveArea == ExpressArea.ceaTop)
                        vFocusRect = FSupRect;
                    else
                    if (FMouseMoveArea == ExpressArea.ceaBottom)
                        vFocusRect = FSubRect;
                
                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Pen.Color = HC.clMedGray;
                    aCanvas.Rectangle(vFocusRect);
                }
            }

            ApplySupSubStyle(aStyle.TextStyles[TextStyleNo], aCanvas, aPaintInfo.ScaleY / aPaintInfo.Zoom);
            aCanvas.TextOut(aDrawRect.Left + FSupRect.Left, aDrawRect.Top + FSupRect.Top, FSupText);
            aCanvas.TextOut(aDrawRect.Left + FSubRect.Left, aDrawRect.Top + FSubRect.Top, FSubText);
        }

        public override int GetOffsetAt(int x)
        {
            if (FOutSelectInto)
                return base.GetOffsetAt(x);
            else
            {
                if (x <= 0)
                    return HC.OffsetBefor;
                else
                    if (x >= Width)
                        return HC.OffsetAfter;
                    else
                        return HC.OffsetInner;
            }
        }

        protected override void SetActive(bool value)
        {
            base.SetActive(value);
            if (!value)
                FActiveArea = ExpressArea.ceaNone;
        }

        public override void MouseLeave()
        {
            base.MouseLeave();
            FMouseMoveArea = ExpressArea.ceaNone;
        }

        public override bool MouseDown(MouseEventArgs e)
        {
            bool vResult = base.MouseDown(e);
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
                ApplySupSubStyle(OwnerData.Style.TextStyles[TextStyleNo], OwnerData.Style.TempCanvas);
                vOffset = HC.GetNorAlignCharOffsetAt(OwnerData.Style.TempCanvas, vS, vX);
            }
            else
                vOffset = -1;
            
            if (vOffset != FCaretOffset)
            {
                FCaretOffset = (short)vOffset;
                OwnerData.Style.UpdateInfoReCaret();
            }

            return vResult;
        }

        public override bool MouseMove(MouseEventArgs e)
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
            
            return base.MouseMove(e);
        }

        public override bool MouseUp(MouseEventArgs e)
        {
            FMouseLBDowning = false;
            FOutSelectInto = false;
            return base.MouseUp(e);
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

        public override void KeyPress(ref Char key)
        {
            if (FActiveArea != ExpressArea.ceaNone)
                InsertText(key.ToString());
            else
                key = (Char)0;
        }

        public override bool InsertText(string aText)
        {
            if (FActiveArea != ExpressArea.ceaNone)
            {
                if (FActiveArea == ExpressArea.ceaTop)
                    FSupText = FSupText.Insert(FCaretOffset, aText);
                else
                    if (FActiveArea == ExpressArea.ceaBottom)
                        FSubText = FSubText.Insert(FCaretOffset, aText);

                FCaretOffset += (short)aText.Length;
                this.SizeChanged = true;
                return true;
            }
            else
                return false;
        }

        public override void GetCaretInfo(ref HCCaretInfo aCaretInfo)
        {
            if (FActiveArea != ExpressArea.ceaNone)
            {
                ApplySupSubStyle(OwnerData.Style.TextStyles[TextStyleNo], OwnerData.Style.TempCanvas);
                if (FActiveArea == ExpressArea.ceaTop)
                {
                    aCaretInfo.Height = FSupRect.Bottom - FSupRect.Top;
                    aCaretInfo.X = FSupRect.Left + OwnerData.Style.TempCanvas.TextWidth(FSupText.Substring(0, FCaretOffset));
                    aCaretInfo.Y = FSupRect.Top;
                }
                else
                    if (FActiveArea == ExpressArea.ceaBottom)
                    {
                        aCaretInfo.Height = FSubRect.Bottom - FSubRect.Top;
                        aCaretInfo.X = FSubRect.Left + OwnerData.Style.TempCanvas.TextWidth(FSubText.Substring(0, FCaretOffset));
                        aCaretInfo.Y = FSubRect.Top;
                    }
            }
            else
                aCaretInfo.Visible = false;
        }

        protected virtual ExpressArea GetExpressArea(int x, int y)
        {
            ExpressArea Result = ExpressArea.ceaNone;
            POINT vPt = new POINT(x, y);
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

        public HCSupSubScriptItem(HCCustomData aOwnerData, string aSupText, string aSubText) : base(aOwnerData)
        {
            this.StyleNo = HCStyle.SupSubScript;
            FPadding = 1;
            FActiveArea = ExpressArea.ceaNone;
            FCaretOffset = -1;

            FSupText = aSupText;
            FSubText = aSubText;
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FSupText = (source as HCSupSubScriptItem).SupText;
            FSubText = (source as HCSupSubScriptItem).SubText;
        }

        /// <summary> 正在其上时内部是否处理指定的Key和Shif </summary>
        public override bool WantKeyDown(KeyEventArgs e)
        {
            return true;
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            HC.HCSaveTextToStream(aStream, FSupText);
            HC.HCSaveTextToStream(aStream, FSubText);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            HC.HCLoadTextFromStream(aStream, ref FSupText, aFileVersion);
            HC.HCLoadTextFromStream(aStream, ref FSubText, aFileVersion);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("sup", FSupText);
            aNode.SetAttribute("sub", FSubText);
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FSupText = aNode.Attributes["sup"].Value;
            FSubText = aNode.Attributes["sub"].Value;
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