/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-8-16             }
{                                                       }
{            文档FloatItem(浮动)对象实现单元            }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;
using System.IO;
using System.Xml;

namespace HC.View
{
    public class HCCustomFloatItem : HCResizeRectItem  // 可浮动Item
    {
        private int FLeft, FTop, FPageIndex;
        private RECT FDrawRect;

        public HCCustomFloatItem(HCCustomData aOwnerData)
            : base(aOwnerData)
        {

        }

        public virtual bool PointInClient(POINT aPoint)
        {
            return HC.PtInRect(HC.Bounds(0, 0, Width, Height), aPoint);
        }

        public bool PointInClient(int x, int y)
        {
            return PointInClient(new POINT(x, y));
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FLeft = (source as HCCustomFloatItem).Left;
            FTop = (source as HCCustomFloatItem).Top;
            Width = (source as HCCustomFloatItem).Width;
            Height = (source as HCCustomFloatItem).Height;
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (this.Active)
                aCanvas.DrawFocuseRect(FDrawRect);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            byte[] vBuffer = BitConverter.GetBytes(this.StyleNo);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FLeft);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FTop);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(Width);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(Height);
            aStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            // StyleNo加载时先读取并根据值创建
            byte[] vBuffer = BitConverter.GetBytes(FLeft);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FLeft = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FTop);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FTop = BitConverter.ToInt32(vBuffer, 0);


            vBuffer = BitConverter.GetBytes(Width);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            Width = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(Height);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            Height = BitConverter.ToInt32(vBuffer, 0);
        }

        public override void ToXml(XmlElement aNode)
        {
            aNode.SetAttribute("sno", StyleNo.ToString());
            aNode.SetAttribute("left", FLeft.ToString());
            aNode.SetAttribute("top", FTop.ToString());
            aNode.SetAttribute("width", Width.ToString());
            aNode.SetAttribute("height", Height.ToString());
        }

        public override void ParseXml(XmlElement aNode)
        {
            StyleNo = int.Parse(aNode.Attributes["sno"].Value);
            FLeft = int.Parse(aNode.Attributes["left"].Value);
            FTop = int.Parse(aNode.Attributes["top"].Value);
            Width = int.Parse(aNode.Attributes["width"].Value);
            Height = int.Parse(aNode.Attributes["height"].Value);
        }

        public RECT DrawRect
        {
            get { return FDrawRect; }
            set { FDrawRect = value; }
        }

        public int Left
        {
            get { return FLeft; }
            set { FLeft = value; }
        }

        public int Top
        {
            get { return FTop; }
            set { FTop = value; }
        }

        public int PageIndex
        {
            get { return FPageIndex; }
            set { FPageIndex = value; }
        }
    }
}
