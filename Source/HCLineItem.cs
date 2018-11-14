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

namespace HC.View
{
    public class HCLineItem : HCCustomRectItem
    {
        private byte FLineHeight;
        private HCPenStyle FLineStyle;

        public override int GetOffsetAt(int X)
        {
            if (X < Width / 2)
                return HC.OffsetBefor;
            else
                return HC.OffsetAfter;
        }

        public override void FormatToDrawItem(HCCustomData ARichData, int AItemNo)
        {
            Width = (ARichData as HCCustomRichData).Width;
            Height = FLineHeight;
        }

        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, int ADataDrawTop, int ADataDrawBottom, int ADataScreenTop, int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            ACanvas.Pen.BeginUpdate();
            try
            {
                ACanvas.Pen.Width = FLineHeight;
                ACanvas.Pen.Style = FLineStyle;
                ACanvas.Pen.Color = Color.Black;
            }
            finally
            {
                ACanvas.Pen.EndUpdate();
            }

            int vTop = (ADrawRect.Top + ADrawRect.Bottom) / 2;
            ACanvas.MoveTo(ADrawRect.Left, vTop);
            ACanvas.LineTo(ADrawRect.Right, vTop);
        }

        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            AStream.WriteByte(FLineHeight);
            AStream.WriteByte((byte)FLineStyle);
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);
            FLineHeight = (byte)AStream.ReadByte();
            FLineStyle = (HCPenStyle)AStream.ReadByte();
        }

        public HCLineItem(HCCustomData AOwnerData, int AWidth, int ALineHeight) : base(AOwnerData)
        {
            FLineHeight = (byte)ALineHeight;
            Width = AWidth;
            Height = ALineHeight;
            FLineStyle = HCPenStyle.psSolid;
            StyleNo = HCStyle.Line;
        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            FLineHeight = (Source as HCLineItem).LineHeight;
            FLineStyle = (Source as HCLineItem).FLineStyle;
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
