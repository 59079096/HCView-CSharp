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
using System.Windows.Forms;

namespace HC.View
{
    public class HCCustomFloatItem : HCResizeRectItem  // 可浮动Item
    {
        private int FLeft, FTop, FPageIndex;
        private RECT FDrawRect;
        private POINT FMousePt;

        public HCCustomFloatItem() : base()
        {

        }

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

        public override bool MouseDown(MouseEventArgs e)
        {
            bool vResult = base.MouseDown(e);
            if (!this.Resizing)
                FMousePt = new POINT(e.X, e.Y);

            return vResult;
        }

        public override bool MouseMove(MouseEventArgs e)
        {
            bool vResult = base.MouseMove(e);
            if ((!this.Resizing) && (e.Button == MouseButtons.Left))
            {
                FLeft += e.X - FMousePt.X;
                FTop += e.Y - FMousePt.Y;
            }

            return vResult;
        }

        public override bool MouseUp(MouseEventArgs e)
        {
            bool vResult = false;

            if (this.Resizing)
            {
                this.Resizing = false;

                if ((this.ResizeWidth < 1) || (this.ResizeHeight < 1))
                    return vResult;

                Width = this.ResizeWidth;
                Height = this.ResizeHeight;
                vResult = true;
            }

            return vResult;
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                aCanvas, aPaintInfo);

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

            vBuffer = BitConverter.GetBytes(FPageIndex);
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

            if (aFileVersion > 28)
            {
                vBuffer = BitConverter.GetBytes(FPageIndex);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FPageIndex = BitConverter.ToInt32(vBuffer, 0);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            aNode.SetAttribute("sno", StyleNo.ToString());
            aNode.SetAttribute("left", FLeft.ToString());
            aNode.SetAttribute("top", FTop.ToString());
            aNode.SetAttribute("width", Width.ToString());
            aNode.SetAttribute("height", Height.ToString());
            aNode.SetAttribute("pageindex", FPageIndex.ToString());
        }

        public override void ParseXml(XmlElement aNode)
        {
            StyleNo = int.Parse(aNode.Attributes["sno"].Value);
            FLeft = int.Parse(aNode.Attributes["left"].Value);
            FTop = int.Parse(aNode.Attributes["top"].Value);
            Width = int.Parse(aNode.Attributes["width"].Value);
            Height = int.Parse(aNode.Attributes["height"].Value);
            FPageIndex = int.Parse(aNode.Attributes["pageindex"].Value);
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

    public delegate void FloatItemNotifyEventHandler(HCCustomFloatItem aItem);

    public class HCFloatItems : HCList<HCCustomFloatItem>
    {
        private FloatItemNotifyEventHandler FOnInsertItem, FOnRemoveItem;

        private void HCItems_OnInsert(object sender, NListEventArgs<HCCustomFloatItem> e)
        {
            if (FOnInsertItem != null)
                FOnInsertItem(e.Item);
        }

        private void HCItems_OnRemove(object sender, NListEventArgs<HCCustomFloatItem> e)
        {
            if (FOnRemoveItem != null)
                FOnRemoveItem(e.Item);
        }

        public HCFloatItems()
        {
            this.OnInsert += new EventHandler<NListEventArgs<HCCustomFloatItem>>(HCItems_OnInsert);
            this.OnDelete += new EventHandler<NListEventArgs<HCCustomFloatItem>>(HCItems_OnRemove);
        }

        public FloatItemNotifyEventHandler OnInsertItem
        {
            get { return FOnInsertItem; }
            set { FOnInsertItem = value; }
        }

        public FloatItemNotifyEventHandler OnRemoveItem
        {
            get { return FOnRemoveItem; }
            set { FOnRemoveItem = value; }
        }
    }
}
