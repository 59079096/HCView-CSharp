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

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            HCStyle vStyle = aRichData.Style;
            vStyle.ApplyTempStyle(TextStyleNo);
            int vH = vStyle.TempCanvas.TextHeight("H");
            int vLeftW = Math.Max(vStyle.TempCanvas.TextWidth(FLeftText), Padding);
            int vTopW = Math.Max(vStyle.TempCanvas.TextWidth(TopText), Padding);
            int vRightW = Math.Max(vStyle.TempCanvas.TextWidth(FRightText), Padding);
            int vBottomW = Math.Max(vStyle.TempCanvas.TextWidth(BottomText), Padding);
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

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (this.Active && (!aPaintInfo.Print))
            {
                aCanvas.Brush.Color = HC.clBtnFace;
                aCanvas.FillRect(aDrawRect);
            }
            
            aCanvas.Pen.Color = Color.Black;
            aCanvas.MoveTo(aDrawRect.Left + FLeftRect.Right + Padding, aDrawRect.Top + TopRect.Bottom + Padding);
            aCanvas.LineTo(aDrawRect.Left + FRightRect.Left - Padding, aDrawRect.Top + TopRect.Bottom + Padding);
            
            if (!aPaintInfo.Print)
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

                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Pen.Color = Color.Blue;
                    aCanvas.Rectangle(vFocusRect);
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
                
                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Pen.Color = HC.clMedGray;
                    aCanvas.Rectangle(vFocusRect);
                }
            }

            aStyle.TextStyles[TextStyleNo].ApplyStyle(aCanvas, aPaintInfo.ScaleY / aPaintInfo.Zoom);
            aCanvas.TextOut(aDrawRect.Left + FLeftRect.Left, aDrawRect.Top + FLeftRect.Top, FLeftText);
            aCanvas.TextOut(aDrawRect.Left + TopRect.Left, aDrawRect.Top + TopRect.Top, TopText);
            aCanvas.TextOut(aDrawRect.Left + FRightRect.Left, aDrawRect.Top + FRightRect.Top, FRightText);
            aCanvas.TextOut(aDrawRect.Left + BottomRect.Left, aDrawRect.Top + BottomRect.Top, BottomText);
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
                OwnerData.Style.ApplyTempStyle(TextStyleNo);
                vOffset = HC.GetNorAlignCharOffsetAt(OwnerData.Style.TempCanvas, vS, vX);
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

        protected override ExpressArea GetExpressArea(int x, int y)
        {
            ExpressArea Result = base.GetExpressArea(x, y);
            if (Result == ExpressArea.ceaNone)
            {
                POINT vPt = new POINT(x, y);
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

        public override bool InsertText(string aText)
        {
            if (FActiveArea != ExpressArea.ceaNone)
            {
                switch (FActiveArea)
                {
                    case ExpressArea.ceaLeft:
                        FLeftText = FLeftText.Insert(FCaretOffset, aText);
                        FCaretOffset += (short)aText.Length;
                        this.SizeChanged = true;
                        return true;

                    case ExpressArea.ceaRight:
                        FRightText = FRightText.Insert(FCaretOffset, aText);
                        FCaretOffset += (short)aText.Length;
                        this.SizeChanged = true;
                        return true;

                    default:
                        return base.InsertText(aText);
                }
            }
            else
                return false;
        }

        public override void GetCaretInfo(ref HCCaretInfo aCaretInfo)
        {
            if (FActiveArea != ExpressArea.ceaNone)
            {
                OwnerData.Style.ApplyTempStyle(TextStyleNo);
                
                switch (FActiveArea)
                {
                    case ExpressArea.ceaLeft:
                        if (FCaretOffset < 0)
                            FCaretOffset = 0;

                        aCaretInfo.Height = FLeftRect.Bottom - FLeftRect.Top;
                        aCaretInfo.X = FLeftRect.Left + OwnerData.Style.TempCanvas.TextWidth(FLeftText.Substring(0, FCaretOffset));
                        aCaretInfo.Y = FLeftRect.Top;
                        break;
                
                    case ExpressArea.ceaTop:
                        if (FCaretOffset < 0)
                            FCaretOffset = 0;

                        aCaretInfo.Height = TopRect.Bottom - TopRect.Top;
                        aCaretInfo.X = TopRect.Left + OwnerData.Style.TempCanvas.TextWidth(TopText.Substring(0, FCaretOffset));
                        aCaretInfo.Y = TopRect.Top;
                        break;
                
                    case ExpressArea.ceaRight:
                        if (FCaretOffset < 0)
                            FCaretOffset = 0;

                        aCaretInfo.Height = FRightRect.Bottom - FRightRect.Top;
                        aCaretInfo.X = FRightRect.Left + OwnerData.Style.TempCanvas.TextWidth(FRightText.Substring(0, FCaretOffset));
                        aCaretInfo.Y = FRightRect.Top;
                        break;

                    case ExpressArea.ceaBottom:
                        if (FCaretOffset < 0)
                            FCaretOffset = 0;

                        aCaretInfo.Height = BottomRect.Bottom - BottomRect.Top;
                        aCaretInfo.X = BottomRect.Left + OwnerData.Style.TempCanvas.TextWidth(BottomText.Substring(0, FCaretOffset));
                        aCaretInfo.Y = BottomRect.Top;
                        break;
                }
            }
            else
                aCaretInfo.Visible = false;
        }

        public HCExpressItem(HCCustomData aOwnerData, string aLeftText, string aTopText, string aRightText, string aBottomText)
            : base(aOwnerData, aTopText, aBottomText)
        {
            this.StyleNo = HCStyle.Express;
            FLeftText = aLeftText;
            FRightText = aRightText;
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FLeftText = (source as HCExpressItem).LeftText;
            FRightText = (source as HCExpressItem).RightText;
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            HC.HCSaveTextToStream(aStream, FLeftText);
            HC.HCSaveTextToStream(aStream, FRightText);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            HC.HCLoadTextFromStream(aStream, ref FLeftText, aFileVersion);
            HC.HCLoadTextFromStream(aStream, ref FRightText, aFileVersion);
        }

        public override void ToXml(System.Xml.XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("lefttext", FLeftText);
            aNode.SetAttribute("righttext", FRightText);
        }

        public override void ParseXml(System.Xml.XmlElement aNode)
        {
            base.ParseXml(aNode);
            FLeftText = aNode.Attributes["lefttext"].Value;
            FRightText = aNode.Attributes["righttext"].Value;
        }

        public RECT LeftRect
        {
            get { return FLeftRect; }
            set { FLeftRect = value; }
        }

        public RECT RightRect
        {
            get { return FRightRect; }
            set { FRightRect = value; }
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
