/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{          文档QRCodeItem(二维码)对象实现单元           }
{                                                       }
{*******************************************************/

using System;
using HC.Win32;
using System.IO;
using System.Drawing;

namespace HC.View
{
    public class HCQRCodeItem : HCResizeRectItem
    {
        private string FText;

        protected override string GetText()
        {
            return FText;
        }

        protected override void SetText(string value)
        {
            if (FText != value)
                FText = value;
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom,
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {

            using (Image vImage = SharpZXingQRCode.Create(FText, Width, Height))
            {
                if (vImage != null)
                {
                    if (aPaintInfo.Print)
                        aCanvas.StretchPrintDrawImage(aDrawRect, vImage);
                    else
                        aCanvas.StretchDraw(aDrawRect, vImage);
                }
            }
            // 绘制二维码
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                aCanvas, aPaintInfo);
        }

        public HCQRCodeItem(HCCustomData aOwnerData, string aText) : base(aOwnerData)
        {
            StyleNo = HCStyle.QRCode;
            FText = aText;
            Width = 100;
            Height = 100;
        }

        ~HCQRCodeItem()
        {

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

        /// <summary> 约束到指定大小范围内 </summary>
        public override void RestrainSize(int aWidth, int  aHeight)
        {
            if (Width > aWidth)
            {
                Single vBL = (float)Width / aWidth;
                Width = aWidth;
                Height = (int)Math.Round(Height / vBL);
            }

            if (Height > aHeight)
            {
                Single vBL = (float)Height / aHeight;
                Height = aHeight;
                Width = (int)Math.Round(Width / vBL);
            }
        }
    }
}
