/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{         文档BarCodeItem(一维码)对象实现单元           }
{                                                       }
{*******************************************************/

using HC.Win32;
using System;
using System.IO;

namespace HC.View
{
    public class HCBarCodeItem : HCResizeRectItem
    {
        private HCCode128 FCode128;

        protected void DoCodeWidthChanged(object sender, EventArgs e)
        {
            this.Width = FCode128.Width;
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom,
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            FCode128.PaintTo(aCanvas, aDrawRect);
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                aCanvas, aPaintInfo);
        }

        protected override void SetWidth(int value)
        {
            base.SetWidth(FCode128.Width);
        }

        protected override void SetHeight(int value)
        {
            base.SetHeight(value);
            FCode128.Height = this.Height;
        }

        protected override string GetText()
        {
            return FCode128.Text;
        }

        protected override void SetText(string value)
        {
            FCode128.Text = value;
        }

        public HCBarCodeItem(HCCustomData aOwnerData, string aText) : base(aOwnerData)
        {
            StyleNo = HCStyle.BarCode;
            FCode128 = new HCCode128(aText);
            FCode128.OnWidthChanged = DoCodeWidthChanged;
            Width = FCode128.Width;
            Height = 100;
        }

        ~HCBarCodeItem()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FCode128.Text = (source as HCBarCodeItem).Text;
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);
            HC.HCSaveTextToStream(aStream, FCode128.Text);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            string vText = "";
            HC.HCLoadTextFromStream(aStream, ref vText, aFileVersion);
            FCode128.Text = vText;
        }

        public override void ToXml(System.Xml.XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.InnerText = FCode128.Text;
        }

        public override void ParseXml(System.Xml.XmlElement aNode)
        {
            base.ParseXml(aNode);
            FCode128.Text = aNode.InnerText;
        }

        /// <summary> 约束到指定大小范围内 </summary>
        public override void RestrainSize(int aWidth, int aHeight)
        {
            if (Height > aHeight)
                Height = aHeight;
        }
    }
}
