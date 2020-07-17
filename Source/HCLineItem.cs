/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{           文档LineItem(直线)对象实现单元              }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HC.Win32;
using System.Drawing;
using System.Xml;

namespace HC.View
{
    public class HCLineItem : HCCustomRectItem
    {
        private byte FLineHeight;
        private HCPenStyle FLineStyle;

        public override int GetOffsetAt(int x)
        {
            if (x < Width / 2)
                return HC.OffsetBefor;
            else
                return HC.OffsetAfter;
        }

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            //Width = (aRichData as HCRichData).Width;
            //Height = FLineHeight;
        }

        private void PaintLine(HCCanvas canvas, RECT drawRect)
        {
            int vTop = (drawRect.Top + drawRect.Bottom) / 2;
            canvas.MoveTo(drawRect.Left, vTop);
            canvas.LineTo(drawRect.Right, vTop);
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            aCanvas.Pen.BeginUpdate();
            try
            {
                aCanvas.Pen.Width = FLineHeight;
                aCanvas.Pen.Style = FLineStyle;
                aCanvas.Pen.Color = Color.Black;
            }
            finally
            {
                aCanvas.Pen.EndUpdate();
            }

            if (this.Height > 1)
            {
                IntPtr vExtPen = HC.CreateExtPen(aCanvas.Pen, GDI.PS_ENDCAP_FLAT);
                IntPtr vOldPen = (IntPtr)GDI.SelectObject(aCanvas.Handle, vExtPen);
                try
                {
                    PaintLine(aCanvas, aDrawRect);
                }
                finally
                {
                    GDI.SelectObject(aCanvas.Handle, vOldPen);
                    GDI.DeleteObject(vExtPen);
                }
            }
            else
                PaintLine(aCanvas, aDrawRect);
        }

        public HCLineItem(HCCustomData aOwnerData, int aWidth, int aHeight)
            : base(aOwnerData)
        {
            FLineHeight = 1;
            Width = aWidth;
            Height = aHeight;
            FLineStyle = HCPenStyle.psSolid;
            StyleNo = HCStyle.Line;
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FLineHeight = (source as HCLineItem).LineHeight;
            FLineStyle = (source as HCLineItem).FLineStyle;
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            aStream.WriteByte(FLineHeight);
            aStream.WriteByte((byte)FLineStyle);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            FLineHeight = (byte)aStream.ReadByte();
            FLineStyle = (HCPenStyle)aStream.ReadByte();
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("height", FLineHeight.ToString());
            aNode.SetAttribute("style", ((byte)FLineStyle).ToString());
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FLineHeight = byte.Parse(aNode.Attributes["height"].Value);
            FLineStyle = (HCPenStyle)byte.Parse(aNode.Attributes["style"].Value);
        }

        public HCPenStyle LineStyle
        {
            get { return FLineStyle; }
            set { FLineStyle = value; }
        }

        public byte LineHeight
        {
            get { return FLineHeight; }
            set { FLineHeight = value; }
        }
    }
}
