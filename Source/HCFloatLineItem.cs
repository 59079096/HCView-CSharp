/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-8-17             }
{                                                       }
{         文档FloatLineItem(直线)对象实现单元           }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace HC.View
{
    public enum HCLineObj : byte
    {
        cloNone, cloLine, cloLeftOrTop, cloRightOrBottom
    }

    public class HCFloatLineItem : HCFloatItem  // 可浮动LineItem
    {

        private POINT FStartPt, FEndPt, FLeftTop;

        private HCLineObj FMouseDownObj;

        private HCLineObj GetLineObjAt(int X, int  Y)
        {
            HCLineObj Result = HCLineObj.cloNone;
            if (HC.PtInRect(new RECT(FStartPt.X - PointSize, FStartPt.Y - PointSize, FStartPt.X + PointSize, FStartPt.Y + PointSize), new POINT(X, Y)))
                Result = HCLineObj.cloLeftOrTop;
            else
            if (HC.PtInRect(new RECT(FEndPt.X - PointSize, FEndPt.Y - PointSize, FEndPt.X + PointSize, FEndPt.Y + PointSize), new POINT(X, Y)))
                Result = HCLineObj.cloRightOrBottom;
            else
            {
                POINT[] vPointArr = new POINT[4];
                vPointArr[0] = new POINT(FStartPt.X - PointSize, FStartPt.Y);
                vPointArr[1] = new POINT(FStartPt.X + PointSize, FStartPt.Y);
                vPointArr[2] = new POINT(FEndPt.X + PointSize, FEndPt.Y);
                vPointArr[3] = new POINT(FEndPt.X - PointSize, FEndPt.Y);
                IntPtr vRgn = (IntPtr)GDI.CreatePolygonRgn(ref vPointArr[0], 4, GDI.WINDING);
                try
                {
                    if (GDI.PtInRegion(vRgn, X, Y) > 0)
                        Result = HCLineObj.cloLine;
                }
                finally
                {
                    GDI.DeleteObject(vRgn);
                }
            }

            return Result;
        }

        public HCFloatLineItem(HCCustomData AOwnerData) : base(AOwnerData)
        {
            this.StyleNo = HCFloatStyle.Line;
            FMouseDownObj = HCLineObj.cloNone;
            Width = 100;
            Height = 70;
            FStartPt = new POINT(0, 0);
            FEndPt = new POINT(Width, Height);
        }

        public override bool PtInClient(POINT APoint)
        {
            return (GetLineObjAt(APoint.X, APoint.Y) != HCLineObj.cloNone);
        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            FStartPt.X = (Source as HCFloatLineItem).FStartPt.X;
            FStartPt.Y = (Source as HCFloatLineItem).FStartPt.Y;
            FEndPt.X = (Source as HCFloatLineItem).FEndPt.X;
            FEndPt.Y = (Source as HCFloatLineItem).FEndPt.Y;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            if (Active)
            {
                FMouseDownObj = GetLineObjAt(e.X, e.Y);
                this.Resizing = (e.Button == MouseButtons.Left) 
                        && ((FMouseDownObj == HCLineObj.cloLeftOrTop) || (FMouseDownObj == HCLineObj.cloRightOrBottom));
                if (this.Resizing)
                {
                    this.FResizeX = e.X;
                    this.FResizeY = e.Y;
                    // 缩放前的Rect的LeftTop
                    if (FStartPt.X < FEndPt.X)
                        FLeftTop.X = FStartPt.X;
                    else
                        FLeftTop.X = FEndPt.X;

                    if (FStartPt.Y < FEndPt.Y)
                        FLeftTop.Y = FStartPt.Y;
                    else
                        FLeftTop.Y = FEndPt.Y;
                }
            }
            else
            {
                FMouseDownObj = HCLineObj.cloNone;
                Active = PtInClient(e.X, e.Y);
            }
        }

        public override void MouseMove(MouseEventArgs e)
        {
            if (Active)
            {
                if (this.Resizing)
                {
                    if (FMouseDownObj == HCLineObj.cloLeftOrTop)
                        FStartPt.Offset(e.X - this.FResizeX, e.Y - this.FResizeY);
                    else
                        FEndPt.Offset(e.X - this.FResizeX, e.Y - this.FResizeY);
                    
                    this.FResizeX = e.X;
                    this.FResizeY = e.Y;
            
                    HC.GCursor = Cursors.Cross;
                }
                else
                {
                    HCLineObj vLineObj = GetLineObjAt(e.X, e.Y);
                    if ((vLineObj == HCLineObj.cloLeftOrTop) || (vLineObj == HCLineObj.cloRightOrBottom))
                        HC.GCursor = Cursors.Cross;
                    else
                    if (vLineObj != HCLineObj.cloNone)
                        HC.GCursor = Cursors.SizeAll;
                }
            }
            else
                HC.GCursor = Cursors.Default;
        }

        public override void MouseUp(MouseEventArgs e)
        {
            if (this.Resizing)
            {
                this.Resizing = false;
                POINT vNewLeftTop = new POINT();
                
                // 缩放后的Rect的LeftTop
                if (FStartPt.X < FEndPt.X)
                    vNewLeftTop.X = FStartPt.X;
                else
                    vNewLeftTop.X = FEndPt.X;

                if (FStartPt.Y < FEndPt.Y)
                    vNewLeftTop.Y = FStartPt.Y;
                else
                    vNewLeftTop.Y = FEndPt.Y;
            
                vNewLeftTop.X = vNewLeftTop.X - FLeftTop.X;
                vNewLeftTop.Y = vNewLeftTop.Y - FLeftTop.Y;
            
                FStartPt.Offset(-vNewLeftTop.X, -vNewLeftTop.Y);
                FEndPt.Offset(-vNewLeftTop.X, -vNewLeftTop.Y);
            
                this.Left = this.Left + vNewLeftTop.X;
                this.Top = this.Top + vNewLeftTop.Y;
            
                this.Width = Math.Abs(FEndPt.X - FStartPt.X);
                this.Height = Math.Abs(FEndPt.Y - FStartPt.Y);
            }
        }

        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, 
            int ADataDrawTop, int  ADataDrawBottom, int  ADataScreenTop, 
            int  ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            ACanvas.Pen.BeginUpdate();
            try
            {
                ACanvas.Pen.Color = Color.Black;
                ACanvas.Pen.Style = HCPenStyle.psSolid;
            }
            finally
            {
                ACanvas.Pen.EndUpdate();
            }

            ACanvas.MoveTo(FStartPt.X + this.DrawRect.Left, FStartPt.Y + this.DrawRect.Top);
            ACanvas.LineTo(FEndPt.X + this.DrawRect.Left, FEndPt.Y + this.DrawRect.Top);

            if ((this.Active) && (!APaintInfo.Print))  // 激活
            {
                ACanvas.Rectangle(FStartPt.X + this.DrawRect.Left - PointSize, FStartPt.Y + this.DrawRect.Top - PointSize,
                    FStartPt.X + this.DrawRect.Left + PointSize, FStartPt.Y + this.DrawRect.Top + PointSize);
                ACanvas.Rectangle(FEndPt.X + this.DrawRect.Left - PointSize, FEndPt.Y + this.DrawRect.Top - PointSize,
                    FEndPt.X + this.DrawRect.Left + PointSize, FEndPt.Y + this.DrawRect.Top + PointSize);
            }
        }

        public override void SaveToStream(Stream AStream, int AStart, int  AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            byte[] vBuffer = BitConverter.GetBytes(FStartPt.X);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FStartPt.Y);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FEndPt.X);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FEndPt.X);
            AStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);
            byte[] vBuffer = BitConverter.GetBytes(FStartPt.X);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FStartPt.X = BitConverter.ToInt32(vBuffer, 0);

            AStream.Read(vBuffer, 0, vBuffer.Length);
            FStartPt.Y = BitConverter.ToInt32(vBuffer, 0);

            AStream.Read(vBuffer, 0, vBuffer.Length);
            FEndPt.X = BitConverter.ToInt32(vBuffer, 0);

            AStream.Read(vBuffer, 0, vBuffer.Length);
            FEndPt.X = BitConverter.ToInt32(vBuffer, 0);
        }
    }
}
