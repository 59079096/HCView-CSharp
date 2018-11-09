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

namespace HC.View
{
    public class HCFloatItem : HCResizeRectItem  // 可浮动Item
    {
        private int FLeft, FTop, FPageIndex;
        private RECT FDrawRect;
        public const byte PointSize = 5;

        public HCFloatItem(HCCustomData AOwnerData)
            : base(AOwnerData)
        {

        }

        public virtual bool PtInClient(POINT APoint)
        {
            return HC.PtInRect(HC.Bounds(0, 0, Width, Height), APoint);
        }

        public bool PtInClient(int X, int Y)
        {
            return PtInClient(new POINT(X, Y));
        }

        public override void Assign(HCCustomItem Source)
        {
            FLeft = (Source as HCFloatItem).Left;
            FTop = (Source as HCFloatItem).Top;
            Width = (Source as HCFloatItem).Width;
            Height = (Source as HCFloatItem).Height;
        }

        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, int ADataDrawTop, int ADataDrawBottom, 
            int ADataScreenTop, int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (this.Active)
                ACanvas.DrawFocuseRect(FDrawRect);
        }

        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            byte[] vBuffer = BitConverter.GetBytes(this.StyleNo);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FLeft);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FTop);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(Width);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(Height);
            AStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            // StyleNo加载时先读取并根据值创建
            byte[] vBuffer = BitConverter.GetBytes(FLeft);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FLeft = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FTop);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FTop = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FLeft);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FLeft = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FLeft);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            Width = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FLeft);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            Height = BitConverter.ToInt32(vBuffer, 0);
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
