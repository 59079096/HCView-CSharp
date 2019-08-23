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
using System.Xml;

namespace HC.View
{
    public enum HCLineObj : byte
    {
        cloNone, cloLine, cloLeftOrTop, cloRightOrBottom
    }

    public class HCFloatLineItem : HCCustomFloatItem  // 可浮动LineItem
    {
        private POINT FLeftTop;
        private HCShapeLine FShapeLine;

        protected override void SetActive(bool value)
        {
            base.SetActive(value);
            FShapeLine.Active = this.Active;
        }

        public HCFloatLineItem(HCCustomData aOwnerData) : base(aOwnerData)
        {
            this.StyleNo = (byte)HCShapeStyle.hssLine;
            Width = 100;
            Height = 70;
            FShapeLine = new HCShapeLine(new POINT(0, 0), new POINT(Width, Height));
        }

        public override bool PointInClient(POINT aPoint)
        {
            return FShapeLine.PointInClient(aPoint);
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FShapeLine.Assign((source as HCFloatLineItem).FShapeLine);
        }

        public override bool MouseDown(MouseEventArgs e)
        {
            bool vResult = FShapeLine.MouseDown(e);
            Active = FShapeLine.ActiveObj != HCShapeLineObj.sloNone;
            if (Active)
            {
                this.Resizing = (e.Button == MouseButtons.Left) 
                        && ((FShapeLine.ActiveObj == HCShapeLineObj.sloStart) || (FShapeLine.ActiveObj == HCShapeLineObj.sloEnd));

                if (this.Resizing)
                {
                    this.FResizeX = e.X;
                    this.FResizeY = e.Y;

                    // 缩放前的Rect的LeftTop
                    if (FShapeLine.StartPt.X < FShapeLine.EndPt.X)
                        FLeftTop.X = FShapeLine.StartPt.X;
                    else
                        FLeftTop.X = FShapeLine.EndPt.X;

                    if (FShapeLine.StartPt.Y < FShapeLine.EndPt.Y)
                        FLeftTop.Y = FShapeLine.StartPt.Y;
                    else
                        FLeftTop.Y = FShapeLine.EndPt.Y;
                }
            }

            return vResult;
        }

        public override bool MouseMove(MouseEventArgs e)
        {
            bool vResult = FShapeLine.MouseMove(e);
            if (Active)
            {
                if (this.Resizing)
                {
                    this.FResizeX = e.X;
                    this.FResizeY = e.Y;
                }
            }
            
            if (vResult)
                HC.GCursor = FShapeLine.Cursor;

            return vResult;
        }

        public override bool MouseUp(MouseEventArgs e)
        {
            if (this.Resizing)
            {
                this.Resizing = false;
                POINT vNewLeftTop = new POINT();
                
                // 缩放后的Rect的LeftTop
                if (FShapeLine.StartPt.X < FShapeLine.EndPt.X)
                    vNewLeftTop.X = FShapeLine.StartPt.X;
                else
                    vNewLeftTop.X = FShapeLine.EndPt.X;

                if (FShapeLine.StartPt.Y < FShapeLine.EndPt.Y)
                    vNewLeftTop.Y = FShapeLine.StartPt.Y;
                else
                    vNewLeftTop.Y = FShapeLine.EndPt.Y;
            
                vNewLeftTop.X = vNewLeftTop.X - FLeftTop.X;
                vNewLeftTop.Y = vNewLeftTop.Y - FLeftTop.Y;
                
                this.Left = this.Left + vNewLeftTop.X;
                this.Top = this.Top + vNewLeftTop.Y;
                // 线的点坐标以新LeftTop为原点
                FShapeLine.StartPt.Offset(-vNewLeftTop.X, -vNewLeftTop.Y);
                FShapeLine.EndPt.Offset(-vNewLeftTop.X, -vNewLeftTop.Y);

                this.Width = Math.Abs(FShapeLine.EndPt.X - FShapeLine.StartPt.X);
                this.Height = Math.Abs(FShapeLine.EndPt.Y - FShapeLine.StartPt.Y);
            }

            return FShapeLine.MouseUp(e);
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, 
            int aDataDrawTop, int  aDataDrawBottom, int  aDataScreenTop, 
            int  aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            FShapeLine.PaintTo(aCanvas, aDrawRect, aPaintInfo);
        }

        public override void SaveToStream(Stream aStream, int aStart, int  aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            FShapeLine.SaveToStream(aStream);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            if (aFileVersion > 26)
                FShapeLine.LoadFromStream(aStream);
            else
            {
                FShapeLine.Width = 1;
                FShapeLine.Color = Color.Black;
                int vX = 0, vY = 0;
                byte[] vBuffer = BitConverter.GetBytes(vX);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vX = BitConverter.ToInt32(vBuffer, 0);

                aStream.Read(vBuffer, 0, vBuffer.Length);
                vY = BitConverter.ToInt32(vBuffer, 0);

                FShapeLine.StartPt = new POINT(vX, vY);
                
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vX = BitConverter.ToInt32(vBuffer, 0);

                aStream.Read(vBuffer, 0, vBuffer.Length);
                vY = BitConverter.ToInt32(vBuffer, 0);

                FShapeLine.EndPt = new POINT(vX, vY);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            FShapeLine.ToXml(aNode);
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FShapeLine.ParseXml(aNode);
        }
    }
}
