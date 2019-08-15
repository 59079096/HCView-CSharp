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

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            HCStyle vStyle = aRichData.Style;
            vStyle.ApplyTempStyle(TextStyleNo);
            int vH = vStyle.TextStyles[TextStyleNo].FontHeight;
            int vTopW = Math.Max(vStyle.TempCanvas.TextWidth(FTopText), FPadding);
            int vBottomW = Math.Max(vStyle.TempCanvas.TextWidth(FBottomText), FPadding);
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

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (this.Active && (!aPaintInfo.Print))
            {
                aCanvas.Brush.Color = HC.clBtnFace;
                aCanvas.FillRect(aDrawRect);
            }

            if (!FLineHide)  // 分数线
            {
                aCanvas.Pen.Color = Color.Black;
                aCanvas.MoveTo(aDrawRect.Left + FPadding, aDrawRect.Top + FTopRect.Bottom + FPadding);
                aCanvas.LineTo(aDrawRect.Left + Width - FPadding, aDrawRect.Top + FTopRect.Bottom + FPadding);
            }

            if (!aPaintInfo.Print)
            {
                RECT vFocusRect = new RECT();

                if (FActiveArea != ExpressArea.ceaNone)
                {
                    if (FActiveArea == ExpressArea.ceaTop)
                        vFocusRect = FTopRect;
                    else
                        if (FActiveArea == ExpressArea.ceaBottom)
                            vFocusRect = FBottomRect;

                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Pen.Color = Color.Blue;
                    aCanvas.Rectangle(vFocusRect);
                }

                if ((FMouseMoveArea != ExpressArea.ceaNone) && (FMouseMoveArea != FActiveArea))
                {
                    if (FMouseMoveArea == ExpressArea.ceaTop)
                        vFocusRect = FTopRect;
                    else
                        if (FMouseMoveArea == ExpressArea.ceaBottom)
                            vFocusRect = FBottomRect;

                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Pen.Color = HC.clMedGray;
                    aCanvas.Rectangle(vFocusRect);
                }
            }

            aStyle.TextStyles[TextStyleNo].ApplyStyle(aCanvas, aPaintInfo.ScaleY / aPaintInfo.Zoom);
            aCanvas.TextOut(aDrawRect.Left + FTopRect.Left, aDrawRect.Top + FTopRect.Top, FTopText);
            aCanvas.TextOut(aDrawRect.Left + FBottomRect.Left, aDrawRect.Top + FBottomRect.Top, FBottomText);
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
                OwnerData.Style.ApplyTempStyle(TextStyleNo);
                vOffset = HC.GetNorAlignCharOffsetAt(OwnerData.Style.TempCanvas, vS, vX);
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
                    FTopText = FTopText.Insert(FCaretOffset, aText);
                else
                    if (FActiveArea == ExpressArea.ceaBottom)
                        FBottomText = FBottomText.Insert(FCaretOffset, aText);

                FCaretOffset += (short)aText.Length;

                this.SizeChanged = true;
                return true;
            }
            else
                return false;
        }

        public override void GetCaretInfo(ref HCCaretInfo aCaretInfo)
        {
            if ((FActiveArea != ExpressArea.ceaNone) && (FCaretOffset >= 0))
            {
                OwnerData.Style.ApplyTempStyle(TextStyleNo);
                if (FActiveArea == ExpressArea.ceaTop)
                {
                    if (FCaretOffset < 0)
                        FCaretOffset = 0;

                    aCaretInfo.Height = FTopRect.Bottom - FTopRect.Top;
                    aCaretInfo.X = FTopRect.Left + OwnerData.Style.TempCanvas.TextWidth(FTopText.Substring(0, FCaretOffset));
                    aCaretInfo.Y = FTopRect.Top;
                }
                else
                    if (FActiveArea == ExpressArea.ceaBottom)
                    {
                        if (FCaretOffset < 0)
                            FCaretOffset = 0;

                        aCaretInfo.Height = FBottomRect.Bottom - FBottomRect.Top;
                        aCaretInfo.X = FBottomRect.Left + OwnerData.Style.TempCanvas.TextWidth(FBottomText.Substring(0, FCaretOffset));
                        aCaretInfo.Y = FBottomRect.Top;
                    }
            }
            else
                aCaretInfo.Visible = false;
        }

        protected virtual ExpressArea GetExpressArea(int x, int y)
        {
            POINT vPt = new POINT(x, y);
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

        public HCFractionItem(HCCustomData aOwnerData, string aTopText, string aBottomText)
            : base(aOwnerData)
        {
            this.StyleNo = HCStyle.Fraction;
            FPadding = 5;
            FActiveArea = ExpressArea.ceaNone;
            FCaretOffset = -1;
            FLineHide = false;
            FTopText = aTopText;
            FBottomText = aBottomText;
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FTopText = (source as HCFractionItem).TopText;
            FBottomText = (source as HCFractionItem).BottomText;
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            HC.HCSaveTextToStream(aStream, FTopText);
            HC.HCSaveTextToStream(aStream, FBottomText);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            HC.HCLoadTextFromStream(aStream, ref FTopText, aFileVersion);
            HC.HCLoadTextFromStream(aStream, ref FBottomText, aFileVersion);
        }

        public override void ToXml(System.Xml.XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("toptext", FTopText);
            aNode.SetAttribute("bottomtext", FBottomText);
        }

        public override void ParseXml(System.Xml.XmlElement aNode)
        {
            base.ParseXml(aNode);
            FTopText = aNode.Attributes["toptext"].Value;
            FBottomText = aNode.Attributes["bottomtext"].Value;
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
