using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using HC.Win32;

namespace HC.View
{
    public class HCFloatBarCodeItem : HCCustomFloatItem
    {
        private string FText;

        protected override string GetText()
        {
            return FText;
        }

        protected override void SetText(string value)
        {
            if (FText != value)
            {
                FText = value;
            }
        }

        public HCFloatBarCodeItem(HCCustomData aOwnerData) : base(aOwnerData)
        {
            StyleNo = HCStyle.FloatBarCode;
            Width = 80;
            Height = 60;
            SetText("0000");
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FText = (source as HCFloatBarCodeItem).Text;
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            using (Image vBitmap = SharpZXingBarCode.Create(FText, 3, Width, Height))
            {
                if (vBitmap != null)
                    aCanvas.StretchDraw(aDrawRect, vBitmap);
            }
            // 绘制一维码
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                aCanvas, aPaintInfo);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            HC.HCSaveTextToStream(aStream, FText);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            HC.HCLoadTextFromStream(aStream, ref FText, aFileVersion);
        }

        public override void ToXml(System.Xml.XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.InnerText = FText;
        }

        public override void ParseXml(System.Xml.XmlElement aNode)
        {
            base.ParseXml(aNode);
            FText = aNode.InnerText;
        }
    }
}
