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
    public class HCFloatBarCodeItem : HCCustomFloatItem
    {
        private HCCode128 FCode128;
        private bool FAutoSize;
        
        private void DoCodeWidthChanged(object sender, EventArgs e)
        {
            this.Width = FCode128.Width;
        }

        private byte GetPenWidth()
        {
            return FCode128.Zoom;
        }

        private void SetPenWidth(byte value)
        {
            FCode128.Zoom = value;
        }

        private bool GetShowText()
        {
            return FCode128.TextVisible;
        }

        private void SetShowText(bool value)
        {
            FCode128.TextVisible = value;
        }

        protected override string GetText()
        {
            return FCode128.Text;
        }

        protected override void SetText(string value)
        {
            FCode128.Text = value;
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

        public HCFloatBarCodeItem(HCCustomData aOwnerData) : base(aOwnerData)
        {
            StyleNo = HCStyle.FloatBarCode;
            FAutoSize = true;
            FCode128 = new HCCode128("123456");
            FCode128.OnWidthChanged = DoCodeWidthChanged;
            Width = FCode128.Width;
            Height = 100;
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FCode128.Text = (source as HCFloatBarCodeItem).Text;
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            FCode128.PaintTo(aCanvas, aDrawRect);
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                aCanvas, aPaintInfo);
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);
            HC.HCSaveTextToStream(aStream, FCode128.Text);
            byte[] vBuffer = BitConverter.GetBytes(FAutoSize);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FCode128.TextVisible);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            aStream.WriteByte(FCode128.Zoom);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            string vText = "";
            HC.HCLoadTextFromStream(aStream, ref vText, aFileVersion);
            FCode128.Text = vText;
            if (aFileVersion > 34)
            {
                byte[] vBuffer = BitConverter.GetBytes(FAutoSize);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FAutoSize = BitConverter.ToBoolean(vBuffer, 0);

                vBuffer = BitConverter.GetBytes(FCode128.TextVisible);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FCode128.TextVisible = BitConverter.ToBoolean(vBuffer, 0);

                FCode128.Zoom = (Byte)aStream.ReadByte();
            }
        }

        public override void ToXml(System.Xml.XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.InnerText = FCode128.Text;

            if (FAutoSize)
                aNode.SetAttribute("autosize", "1");
            else
                aNode.SetAttribute("autosize", "0");

            if (FCode128.TextVisible)
                aNode.SetAttribute("showtext", "1");
            else
                aNode.SetAttribute("showtext", "0");

            aNode.SetAttribute("penwidth", FCode128.Zoom.ToString());
        }

        public override void ParseXml(System.Xml.XmlElement aNode)
        {
            base.ParseXml(aNode);
            FCode128.Text = aNode.InnerText;

            if (aNode.HasAttribute("autosize"))
                FAutoSize = aNode.Attributes["autosize"].Value == "1";
            else
                FAutoSize = true;

            if (aNode.HasAttribute("showtext"))
                FCode128.TextVisible = aNode.Attributes["showtext"].Value == "1";
            else
                FCode128.TextVisible = true;

            if (aNode.HasAttribute("penwidth"))
                FCode128.Zoom = Byte.Parse(aNode.Attributes["penwidth"].Value);
            else
                FCode128.Zoom = 1;
        }

        public Byte PenWidth
        {
            get { return GetPenWidth(); }
            set { SetPenWidth(value); }
        }

        public bool AutoSize
        {
            get { return FAutoSize; }
            set { FAutoSize = value; }
        }

        public bool ShowText
        {
            get { return GetShowText(); }
            set { SetShowText(value); }
        }
    }
}
