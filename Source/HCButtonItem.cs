using HC.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace HC.View
{
    public class HCButtonItem : HCControlItem
    {
        private string FText;
        private bool FDown;

        protected override string GetText()
        {
            return FText;
        }

        protected override void SetText(string value)
        {
            FText = value;
        }

        public override void MouseLeave()
        {
            FDown = false;
            base.MouseLeave();
        }

        public override bool MouseMove(MouseEventArgs e)
        {
            HC.GCursor = Cursors.Arrow;
            return base.MouseMove(e);
        }

        public override bool MouseDown(MouseEventArgs e)
        {
            if (this.Enabled && HC.PtInRect(this.ClientRect(), e.X, e.Y))
                FDown = e.Button == MouseButtons.Left;

            return base.MouseDown(e);
        }

        public override bool MouseUp(MouseEventArgs e)
        {
            FDown = false;
            return base.MouseUp(e);
        }

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            if (this.AutoSize)
            {
                aRichData.Style.ApplyTempStyle(TextStyleNo);
                SIZE vSize = aRichData.Style.TempCanvas.TextExtent(FText);
                Width = FPaddingLeft + FPaddingRight + vSize.cx;
                Height = FPaddingTop + FPaddingBottom + vSize.cy;
            }

            if (Width < FMinWidth)
                Width = FMinWidth;

            if (Height < FMinHeight)
                Height = FMinHeight;
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom,
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                aCanvas, aPaintInfo);

            if (this.IsSelectComplate)
                aCanvas.Brush.Color = aStyle.SelColor;
            else
            if (FDown)
                aCanvas.Brush.Color = HC.clHighlight;
            else
            if (FMouseIn)
                aCanvas.Brush.Color = HC.clBtnFace;
            else
                aCanvas.Brush.Color = HC.clMedGray;

            aCanvas.FillRect(aDrawRect);

            aCanvas.Brush.Style = HCBrushStyle.bsClear;
            aStyle.TextStyles[TextStyleNo].ApplyStyle(aCanvas, aPaintInfo.ScaleY / aPaintInfo.Zoom);
            SIZE vSize = aCanvas.TextExtent(FText);
            aCanvas.TextOut(aDrawRect.Left + (aDrawRect.Width - vSize.cx) / 2, aDrawRect.Top + (aDrawRect.Height - vSize.cy) / 2, FText);
        }

        public HCButtonItem(HCCustomData aOwnerData, string aText) : base(aOwnerData)
        {
            this.StyleNo = HCStyle.Button;
            FText = aText;
            FDown = false;
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FText = (source as HCCheckBoxItem).Text;
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);
            HC.HCSaveTextToStream(aStream, FText);  // 存Text
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            HC.HCLoadTextFromStream(aStream, ref FText, aFileVersion);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.InnerText = FText;
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FText = aNode.InnerText;
        }
    }
}
