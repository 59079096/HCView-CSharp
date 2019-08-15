/*******************************************************}
{                                                       }
{         基于HCView的电子病历程序  作者：荆通          }
{                                                       }
{ 此代码仅做学习交流使用，不可用于商业目的，由此引发的  }
{ 后果请使用者承担，加入QQ群 649023932 来获取更多的技术 }
{ 交流。                                                }
{                                                       }
{*******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.View;
using HC.Win32;
using System.Windows.Forms;
using System.Drawing;

namespace EMRView
{
    public enum ToothArea : byte
    {
        ctaNone, ctaLeftTop, ctaLeftBottom, ctaRightTop, ctaRightBottom
    }

    public class EmrToothItem : HCTextRectItem
    {
        string FLeftTopText, FLeftBottomText, FRightTopText, FRightBottomText;
        RECT FLeftTopRect, FLeftBottomRect, FRightTopRect, FRightBottomRect;
        byte FPadding;
        ToothArea FActiveArea, FMouseMoveArea;
        int FCaretOffset;
        bool FMouseLBDowning, FOutSelectInto;

        private ToothArea GetToothArea(int x, int y)
        {
            POINT vPt = new POINT(x, y);
            if (HC.View.HC.PtInRect(FLeftTopRect, vPt))
                return ToothArea.ctaLeftTop;
            else
                if (HC.View.HC.PtInRect(FLeftBottomRect, vPt))
                    return ToothArea.ctaLeftBottom;
                else
                    if (HC.View.HC.PtInRect(FRightTopRect, vPt))
                        return ToothArea.ctaRightTop;
                    else
                        if (HC.View.HC.PtInRect(FRightBottomRect, vPt))
                            return ToothArea.ctaRightBottom;
                        else
                            return ToothArea.ctaNone;

        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (this.Active && (!aPaintInfo.Print))
            {
                aCanvas.Brush.Color = HC.View.HC.clBtnFace;
                aCanvas.FillRect(aDrawRect);
            }

            aStyle.TextStyles[TextStyleNo].ApplyStyle(aCanvas, aPaintInfo.ScaleY / aPaintInfo.Zoom);
            aCanvas.TextOut(aDrawRect.Left + FLeftTopRect.Left, aDrawRect.Top + FLeftTopRect.Top, FLeftTopText);
            aCanvas.TextOut(aDrawRect.Left + FLeftBottomRect.Left, aDrawRect.Top + FLeftBottomRect.Top, FLeftBottomText);
            aCanvas.TextOut(aDrawRect.Left + FRightTopRect.Left, aDrawRect.Top + FRightTopRect.Top, FRightTopText);
            aCanvas.TextOut(aDrawRect.Left + FRightBottomRect.Left, aDrawRect.Top + FRightBottomRect.Top, FRightBottomText);
            
            aCanvas.Pen.Color = Color.Black;
            aCanvas.MoveTo(aDrawRect.Left + FPadding, aDrawRect.Top + FLeftTopRect.Bottom + FPadding);
            aCanvas.LineTo(aDrawRect.Right - FPadding, aDrawRect.Top + FLeftTopRect.Bottom + FPadding);
            aCanvas.MoveTo(aDrawRect.Left + FLeftTopRect.Right + FPadding, aDrawRect.Top + FPadding);
            aCanvas.LineTo(aDrawRect.Left + FLeftTopRect.Right + FPadding, aDrawRect.Bottom - FPadding);

            if (!aPaintInfo.Print)
            {
                RECT vFocusRect = new RECT();

                if (FActiveArea != ToothArea.ctaNone)
                {
                    switch(FActiveArea)
                    {
                        case ToothArea.ctaLeftTop: 
                            vFocusRect = FLeftTopRect;
                            break;

                        case ToothArea.ctaLeftBottom: 
                            vFocusRect = FLeftBottomRect;
                            break;

                        case ToothArea.ctaRightTop: 
                            vFocusRect = FRightTopRect;
                            break;

                        case ToothArea.ctaRightBottom: 
                            vFocusRect = FRightBottomRect;
                            break;
                    }

                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Pen.Color = Color.Gray;
                    aCanvas.Rectangle(vFocusRect);
                }
    
                if ((FMouseMoveArea != ToothArea.ctaNone) && (FMouseMoveArea != FActiveArea))
                {
                    switch(FMouseMoveArea)
                    {
                        case ToothArea.ctaLeftTop: 
                            vFocusRect = FLeftTopRect;
                            break;

                        case ToothArea.ctaLeftBottom: 
                            vFocusRect = FLeftBottomRect;
                            break;

                        case ToothArea.ctaRightTop: 
                            vFocusRect = FRightTopRect;
                            break;

                        case ToothArea.ctaRightBottom: 
                            vFocusRect = FRightBottomRect;
                            break;
                    }
                
                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Pen.Color = Color.LightGray;
                    aCanvas.Rectangle(vFocusRect);
                }
            }
        }

        protected override void SetActive(bool value)
        {
            base.SetActive(value);
            if (!value)
                FActiveArea = ToothArea.ctaNone;
        }

        public EmrToothItem(HCCustomData aOwnerData, string aLeftTopText, string aRightTopText,
            string aLeftBottomText, string aRightBottomText)
            : base(aOwnerData)
        {
            this.StyleNo = EMR.EMRSTYLE_TOOTH;
            FPadding = 5;
            FActiveArea = ToothArea.ctaNone;
            FCaretOffset = -1;
            FLeftTopText = aLeftTopText;
            FLeftBottomText = aLeftBottomText;
            FRightTopText = aRightTopText;
            FRightBottomText = aRightBottomText;
        }

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            HCStyle vStyle = aRichData.Style;
            vStyle.ApplyTempStyle(TextStyleNo);
            int vH = vStyle.TextStyles[TextStyleNo].FontHeight;
            int vLeftTopW = Math.Max(vStyle.TempCanvas.TextWidth(FLeftTopText), FPadding);
            int vLeftBottomW = Math.Max(vStyle.TempCanvas.TextWidth(FLeftBottomText), FPadding);
            int vRightTopW = Math.Max(vStyle.TempCanvas.TextWidth(FRightTopText), FPadding);
            int vRightBottomW = Math.Max(vStyle.TempCanvas.TextWidth(FRightBottomText), FPadding);

            // 计算尺寸
            int vW = 4 * FPadding;
            if (vLeftTopW > vLeftBottomW)
                vW = vW + vLeftTopW;
            else
                vW = vW + vLeftBottomW;

            if (vRightTopW > vRightBottomW)
                vW = vW + vRightTopW;
            else
                vW = vW + vRightBottomW;

            Width = vW;
            Height = vH * 2 + 4 * FPadding;

            // 计算各字符串位置
            if (vLeftTopW > vLeftBottomW)
            {
                FLeftTopRect = HC.View.HC.Bounds(FPadding, FPadding, vLeftTopW, vH);
                FLeftBottomRect = HC.View.HC.Bounds(FPadding, Height - FPadding - vH, vLeftTopW, vH);
            }
            else
            {
                FLeftTopRect = HC.View.HC.Bounds(FPadding, FPadding, vLeftBottomW, vH);
                FLeftBottomRect = HC.View.HC.Bounds(FPadding, Height - FPadding - vH, vLeftBottomW, vH);
            }

            if (vRightTopW > vRightBottomW)
            {
                FRightTopRect = HC.View.HC.Bounds(FLeftTopRect.Right + FPadding + FPadding, FPadding, vRightTopW, vH);
                FRightBottomRect = HC.View.HC.Bounds(FLeftTopRect.Right + FPadding + FPadding, Height - FPadding - vH, vRightTopW, vH);
            }
            else
            {
                FRightTopRect = HC.View.HC.Bounds(FLeftTopRect.Right + FPadding + FPadding, FPadding, vRightBottomW, vH);
                FRightBottomRect = HC.View.HC.Bounds(FLeftTopRect.Right + FPadding + FPadding, Height - FPadding - vH, vRightBottomW, vH);
            }

        }

        public override int GetOffsetAt(int x)
        {
            if (FOutSelectInto)
                return base.GetOffsetAt(x);
            else
            {
                if (x <= 0)
                    return HC.View.HC.OffsetBefor;
                else
                    if (x >= Width)
                        return HC.View.HC.OffsetAfter;
                    else
                        return HC.View.HC.OffsetInner;
            }
        }

        public override void MouseLeave()
        {
            base.MouseLeave();
            FMouseMoveArea = ToothArea.ctaNone;
        }

        public override void MouseDown(System.Windows.Forms.MouseEventArgs e)
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
            int vX = -1;
            switch (FActiveArea)
            {
                case ToothArea.ctaLeftTop:
                    vS = FLeftTopText;
                    vX = e.X - FLeftTopRect.Left;
                    break;

                case ToothArea.ctaLeftBottom:
                    vS = FLeftBottomText;
                    vX = e.X - FLeftBottomRect.Left;
                    break;

                case ToothArea.ctaRightTop:
                    vS = FRightTopText;
                    vX = e.X - FRightTopRect.Left;
                    break;

                case ToothArea.ctaRightBottom:
                    vS = FRightBottomText;
                    vX = e.X - FRightBottomRect.Left;
                    break;
            }
    
            int vOffset = 0;
            if (FActiveArea != ToothArea.ctaNone)
            {
                OwnerData.Style.ApplyTempStyle(TextStyleNo);
                vOffset = HC.View.HC.GetNorAlignCharOffsetAt(OwnerData.Style.TempCanvas, vS, vX);
            }
            else
                vOffset = -1;

            if (vOffset != FCaretOffset)
            {
                FCaretOffset = vOffset;
                OwnerData.Style.UpdateInfoReCaret();
            }
        }

        public override void MouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            if ((!FMouseLBDowning) && (e.Button == MouseButtons.Left))
                FOutSelectInto = true;

            if (!FOutSelectInto)
            {
                ToothArea vArea = GetToothArea(e.X, e.Y);
                if (vArea != FMouseMoveArea)
                {
                    FMouseMoveArea = vArea;
                    OwnerData.Style.UpdateInfoRePaint();
                }
            }
            else
                FMouseMoveArea = ToothArea.ctaNone;

            base.MouseMove(e);
        }

        public override void MouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            FMouseLBDowning = false;
            FOutSelectInto = false;
            base.MouseUp(e);
        }

        public override bool WantKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            return true;
        }

        #region KeyDown子方法
        private void BackDeleteChar(ref string s)
        {
            if (FCaretOffset > 0)
            {
                s.Remove(FCaretOffset - 1, 1);
                FCaretOffset--;
            }
        }

        private void BackspaceKeyDown()
        {
            switch (FActiveArea)
            {
                case ToothArea.ctaLeftTop:
                    BackDeleteChar(ref FLeftTopText);
                    break;

                case ToothArea.ctaLeftBottom:
                    BackDeleteChar(ref FLeftBottomText);
                    break;

                case ToothArea.ctaRightTop:
                    BackDeleteChar(ref FRightTopText);
                    break;

                case ToothArea.ctaRightBottom:
                    BackDeleteChar(ref FRightTopText);
                    break;
            }

            this.SizeChanged = true;
        }

        private void LeftKeyDown()
        {
            if (FCaretOffset > 0)
                FCaretOffset--;
        }

        private void RightKeyDown()
        {
            string vS = "";

            switch (FActiveArea)
            {
                case ToothArea.ctaLeftTop:
                    vS = FLeftTopText;
                    break;

                case ToothArea.ctaLeftBottom:
                    vS = FLeftBottomText;
                    break;

                case ToothArea.ctaRightTop:
                    vS = FRightTopText;
                    break;

                case ToothArea.ctaRightBottom:
                    vS = FRightTopText;
                    break;
            }

            if (FCaretOffset < vS.Length)
                FCaretOffset++;
        }

        private void DeleteChar(ref string s)
        {
            if (FCaretOffset < s.Length)
                s.Remove(FCaretOffset + 1 - 1, 1);
        }

        private void DeleteKeyDown()
        {
            switch (FActiveArea)
            {
                case ToothArea.ctaLeftTop:
                    DeleteChar(ref FLeftTopText);
                    break;

                case ToothArea.ctaLeftBottom:
                    DeleteChar(ref FLeftBottomText);
                    break;

                case ToothArea.ctaRightTop:
                    DeleteChar(ref FRightTopText);
                    break;

                case ToothArea.ctaRightBottom:
                    DeleteChar(ref FRightTopText);
                    break;
            }

            this.SizeChanged = true;
        }

        private void HomeKeyDown()
        {
            FCaretOffset = 0;
        }

        private void  EndKeyDown()
        {
            string vS = "";

            switch (FActiveArea)
            {
                case ToothArea.ctaLeftTop:
                    vS = FLeftTopText;
                    break;

                case ToothArea.ctaLeftBottom:
                    vS = FLeftBottomText;
                    break;

                case ToothArea.ctaRightTop:
                    vS = FRightTopText;
                    break;

                case ToothArea.ctaRightBottom:
                    vS = FRightTopText;
                    break;
            }

            FCaretOffset = vS.Length;
        }
        #endregion

        public override void KeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyValue)
            {
                case User.VK_BACK:
                    BackspaceKeyDown();  // 回删
                    break;

                case User.VK_LEFT:
                    LeftKeyDown();  // 左方向键
                    break;

                case User.VK_RIGHT:
                    RightKeyDown();  // 右方向键
                    break;

                case User.VK_DELETE:
                    DeleteKeyDown();  // 删除键
                    break;

                case User.VK_HOME:
                    HomeKeyDown();  // Home键
                    break;

                case User.VK_END:
                    EndKeyDown();  // End键
                    break;
            }
        }

        public override void KeyPress(ref char key)
        {
            if (FActiveArea != ToothArea.ctaNone)
                InsertText(key.ToString());
            else
                key = (char)0;
        }

        public override bool InsertText(string aText)
        {
            if (FActiveArea != ToothArea.ctaNone)
            {
                switch (FActiveArea)
                {
                    case ToothArea.ctaLeftTop:
                        FLeftTopText = FLeftTopText.Insert(FCaretOffset + 1 - 1, aText);
                        break;

                    case ToothArea.ctaLeftBottom:
                        FLeftBottomText = FLeftBottomText.Insert(FCaretOffset + 1 - 1, aText);
                        break;

                    case ToothArea.ctaRightTop:
                        FRightTopText = FRightTopText.Insert(FCaretOffset + 1 - 1, aText);
                        break;

                    case ToothArea.ctaRightBottom:
                        FRightBottomText = FRightBottomText.Insert(FCaretOffset + 1 - 1, aText);
                        break;
                }

                FCaretOffset += aText.Length;

                this.SizeChanged = true;
                return true;
            }
            else
                return false;
        }

        public override void GetCaretInfo(ref HCCaretInfo aCaretInfo)
        {
            if (FActiveArea != ToothArea.ctaNone)
            {
                OwnerData.Style.ApplyTempStyle(TextStyleNo);
                switch (FActiveArea)
                {
                    case ToothArea.ctaLeftTop:
                        aCaretInfo.Height = FLeftTopRect.Bottom - FLeftTopRect.Top;
                        aCaretInfo.X = FLeftTopRect.Left + OwnerData.Style.TempCanvas.TextWidth(FLeftTopText.Substring(1 - 1, FCaretOffset));
                        aCaretInfo.Y = FLeftTopRect.Top;
                        break;

                    case ToothArea.ctaLeftBottom:
                        aCaretInfo.Height = FLeftBottomRect.Bottom - FLeftBottomRect.Top;
                        aCaretInfo.X = FLeftBottomRect.Left + OwnerData.Style.TempCanvas.TextWidth(FLeftBottomText.Substring(1 - 1, FCaretOffset));
                        aCaretInfo.Y = FLeftBottomRect.Top;
                        break;

                    case ToothArea.ctaRightTop:
                        aCaretInfo.Height = FRightTopRect.Bottom - FRightTopRect.Top;
                        aCaretInfo.X = FRightTopRect.Left + OwnerData.Style.TempCanvas.TextWidth(FRightTopText.Substring(1 - 1, FCaretOffset));
                        aCaretInfo.Y = FRightTopRect.Top;
                        break;

                    case ToothArea.ctaRightBottom:
                        aCaretInfo.Height = FRightBottomRect.Bottom - FRightBottomRect.Top;
                        aCaretInfo.X = FRightBottomRect.Left + OwnerData.Style.TempCanvas.TextWidth(FRightBottomText.Substring(1 - 1, FCaretOffset));
                        aCaretInfo.Y = FRightBottomRect.Top;
                        break;
                }
            }
            else
                aCaretInfo.Visible = false;
        }

        public override void SaveToStream(System.IO.Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            HC.View.HC.HCSaveTextToStream(aStream, FLeftTopText);
            HC.View.HC.HCSaveTextToStream(aStream, FLeftTopText);
            HC.View.HC.HCSaveTextToStream(aStream, FLeftBottomText);
            HC.View.HC.HCSaveTextToStream(aStream, FRightTopText);
            HC.View.HC.HCSaveTextToStream(aStream, FRightBottomText);
        }

        public override void LoadFromStream(System.IO.Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            HC.View.HC.HCLoadTextFromStream(aStream, ref FLeftTopText, aFileVersion);
            HC.View.HC.HCLoadTextFromStream(aStream, ref FLeftTopText, aFileVersion);
            HC.View.HC.HCLoadTextFromStream(aStream, ref FLeftBottomText, aFileVersion);
            HC.View.HC.HCLoadTextFromStream(aStream, ref FRightTopText, aFileVersion);
            HC.View.HC.HCLoadTextFromStream(aStream, ref FRightBottomText, aFileVersion);
        }

        public override void ToXml(System.Xml.XmlElement aNode)
        {
            base.ToXml(aNode);
            ToXmlEmr(aNode);
        }

        public override void ParseXml(System.Xml.XmlElement aNode)
        {
            base.ParseXml(aNode);
            ParseXmlEmr(aNode);
        }

        public void ToXmlEmr(System.Xml.XmlElement aNode)
        {
            aNode.SetAttribute("DeCode", EMR.EMRSTYLE_TOOTH.ToString());
            aNode.SetAttribute("lefttop", FLeftTopText);
            aNode.SetAttribute("righttop", FRightTopText);
            aNode.SetAttribute("leftbottom", FLeftBottomText);
            aNode.SetAttribute("rightbottom", FRightBottomText);
        }

        public void ParseXmlEmr(System.Xml.XmlElement aNode)
        {
            if (aNode.Attributes["DeCode"].Value == EMR.EMRSTYLE_TOOTH.ToString())
            {
                FLeftTopText = aNode.Attributes["lefttop"].Value;
                FRightTopText = aNode.Attributes["righttop"].Value;
                FLeftBottomText = aNode.Attributes["leftbottom"].Value;
                FRightBottomText = aNode.Attributes["rightbottom"].Value;
            }
        }








    }
}
