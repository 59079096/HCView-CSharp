/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                  文档形状对象管理单元                 }
{                                                       }
{*******************************************************/

using HC.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace HC.View
{
    public enum HCShapeStyle : byte
    {
        /// <summary> 无形状 </summary>
        hssNone = 0,
        /// <summary> 直线 </summary>
        hssLine = 1,
        /// <summary> 矩形 </summary>
        hssRectangle = 2,
        /// <summary> 椭圆 </summary>
        hssEllipse = 3,
        /// <summary> 多边形 </summary>
        hssPolygon = 4
    }

    public enum HCStructState : byte
    {
        /// <summary> 构建停止 </summary>
        hstcStop = 0,
        /// <summary> 构建开始 </summary>
        hstcStart = 1,
        /// <summary> 构建中 </summary>
        hstcStructing = 2
    }
    public class HCShape
    {
        private byte FVersion;
        private HCShapeStyle FStyle;
        private bool FActive;
        private Color FColor;
        public static byte PointSize = 5;
        private HCStructState FStructState;
        private EventHandler FOnStructOver;

        protected virtual void PaintAnchor(HCCanvas aCanvas, RECT aRect) { }

        protected virtual void SetActive(bool value)
        {
            if (FActive != value)
            {
                FActive = value;
                if (!value)
                    FStructState = HCStructState.hstcStop;
            }
        }

        protected virtual void SetColor(Color value)
        {
            if (FColor != value)
                FColor = value;
        }

        protected byte Version
        {
            get { return FVersion; }
            set { FVersion = value; }
        }

        protected HCStructState StructState
        {
            get { return FStructState; }
            set { FStructState = value; }
        }

        public Cursor Cursor;

        public HCShape()
        {
            FStyle = HCShapeStyle.hssNone;
            FStructState = HCStructState.hstcStop;
            FVersion = 0;
            FColor = Color.Black;
            Cursor = Cursors.Default;
        }

        public virtual void Assign(HCShape source)
        {
            FStyle = source.Style;
            FVersion = source.Version;
            FColor = source.Color;
        }

        public virtual bool MouseDown(MouseEventArgs e)
        {
            Active = true;
            return FActive;
        }

        public virtual bool MouseMove(MouseEventArgs e)
        {
            return FActive;
        }

        public virtual bool MouseUp(MouseEventArgs e)
        {
            return FActive;
        }

        public virtual bool KeyDown(KeyEventArgs e)
        {
            return false;
        }

        public virtual bool KeyPress(KeyEventArgs e)
        {
            return false;
        }

        public virtual bool KeyUp(KeyEventArgs e)
        {
            return false;
        }

        public virtual void PaintTo(HCCanvas aCanvas, RECT aRect, PaintInfo aPaintInfo) { }

        public virtual bool PointInClient(int x, int y)
        {
            return HC.PtInRect(ClientRect(), x, y);
        }

        public virtual RECT ClientRect()
        {
            return HC.Bounds(0, 0, 0, 0);
        }

        public virtual void SaveToStream(Stream aStream)
        {
            if (FStyle == HCShapeStyle.hssNone)
                throw new Exception("HCShape保存失败，无效的样式值！");

            aStream.WriteByte((byte)FStyle);
            aStream.WriteByte(FVersion);
            HC.HCSaveColorToStream(aStream, FColor);
        }

        public virtual void LoadFromStream(Stream aStream)
        {
            FStyle = (HCShapeStyle)aStream.ReadByte();
            FVersion = (byte)aStream.ReadByte();
            HC.HCLoadColorFromStream(aStream, ref FColor);
        }

        public virtual void ToXml(XmlElement aNode)
        {
            aNode.SetAttribute("style", ((byte)FStyle).ToString());
            aNode.SetAttribute("ver", FVersion.ToString());
            aNode.SetAttribute("color", HC.HCColorToRGBString(FColor));
        }

        public virtual void ParseXml(XmlElement aNode)
        {
            FStyle = (HCShapeStyle)byte.Parse(aNode.Attributes["style"].Value);
            FVersion = byte.Parse(aNode.Attributes["ver"].Value);
            FColor = HC.HCRGBStringToColor(aNode.Attributes["color"].Value);
        }

        public virtual void StructStart()
        {
            FStructState = HCStructState.hstcStart;
        }

        public virtual void StructOver()
        {
            FStructState = HCStructState.hstcStop;
            if (FOnStructOver != null)
                FOnStructOver(this, null);
        }

        public HCShapeStyle Style
        {
            get { return FStyle; }
            set { FStyle = value; }
        }

        public bool Active
        {
            get { return FActive; }
            set { SetActive(value); }
        }

        public Color Color
        {
            get { return FColor; }
            set { SetColor(value); }
        }

        public EventHandler OnStructOver
        {
            get { return FOnStructOver; }
            set { FOnStructOver = value; }
        }
    }

    public enum HCShapeLineObj : byte
    {
        sloNone = 0, sloLine = 1, sloStart = 2, sloEnd = 3
    }

    public class HCShapeLine : HCShape
    {
        private POINT FStartPt, FEndPt, FMousePt;
        private HCShapeLineObj FActiveOjb;
        private byte FWidth;
        private HCPenStyle FLineStyle;

        private void SetWidth(byte value)
        {
            if (FWidth != value)
                FWidth = value;
        }

        private void SetLineStyle(HCPenStyle value)
        {
            if (FLineStyle != value)
                FLineStyle = value;
        }

        protected override void PaintAnchor(HCCanvas aCanvas, RECT aRect)
        {
            aCanvas.Pen.BeginUpdate();
            try
            {
                aCanvas.Pen.Color = Color.Black;
                aCanvas.Pen.Width = 1;
                aCanvas.Pen.Style = HCPenStyle.psSolid;
                aCanvas.Brush.Color = Color.White;
            }
            finally
            {
                aCanvas.Pen.EndUpdate();
            }

            aCanvas.Rectangle(FStartPt.X + aRect.Left - PointSize, FStartPt.Y + aRect.Top - PointSize,
                FStartPt.X + aRect.Left + PointSize, FStartPt.Y + aRect.Top + PointSize);

            aCanvas.Rectangle(FEndPt.X + aRect.Left - PointSize, FEndPt.Y + aRect.Top - PointSize,
                FEndPt.X + aRect.Left + PointSize, FEndPt.Y + aRect.Top + PointSize);
        }

        protected override void SetActive(bool value)
        {
            base.SetActive(value);
            if (!this.Active)
                FActiveOjb = HCShapeLineObj.sloNone;
        }

        protected virtual HCShapeLineObj GetObjAt(int x, int y)
        {
            HCShapeLineObj vResult = HCShapeLineObj.sloNone;

            if (HC.PtInRect(new RECT(FStartPt.X - PointSize, FStartPt.Y - PointSize,
                   FStartPt.X + PointSize, FStartPt.Y + PointSize), new POINT(x, y)))
                vResult = HCShapeLineObj.sloStart;
            else
            if (HC.PtInRect(new RECT(FEndPt.X - PointSize, FEndPt.Y - PointSize,
                   FEndPt.X + PointSize, FEndPt.Y + PointSize), new POINT(x, y)))
                vResult = HCShapeLineObj.sloEnd;
            else
            {
                POINT[] vPointArr = new POINT[4];
                vPointArr[0] = new POINT(FStartPt.X - PointSize, FStartPt.Y);
                vPointArr[1] = new POINT(FStartPt.X + PointSize, FStartPt.Y);
                vPointArr[2] = new POINT(FEndPt.X + PointSize, FEndPt.Y);
                vPointArr[3] = new POINT(FEndPt.X - PointSize, FEndPt.Y);

                IntPtr vRgn = GDI.CreatePolygonRgn(vPointArr, 4, GDI.WINDING);
                try
                {
                    if (GDI.PtInRegion(vRgn, x, y))
                        vResult = HCShapeLineObj.sloLine;
                }
                finally
                {
                    GDI.DeleteObject(vRgn);
                }
            }

            return vResult;
        }

        public HCShapeLine() : base()
        {
            Style = HCShapeStyle.hssLine;
            FStartPt = new POINT(0, 0);
            FEndPt = new POINT(0, 0);
            FWidth = 1;
            FActiveOjb = HCShapeLineObj.sloNone;
            FLineStyle = HCPenStyle.psSolid;
        }

        public HCShapeLine(POINT aStartPt, POINT aEndPt) : this()
        {
            FStartPt = aStartPt;
            FEndPt = aEndPt;
        }

        public override void Assign(HCShape source)
        {
            base.Assign(source);
            FStartPt.X = (source as HCShapeLine).FStartPt.X;
            FStartPt.Y = (source as HCShapeLine).FStartPt.Y;
            FEndPt.X = (source as HCShapeLine).FEndPt.X;
            FEndPt.Y = (source as HCShapeLine).FEndPt.Y;
        }

        public override bool MouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return false;

            bool vResult = false;

            if (StructState != HCStructState.hstcStop)  // 正在构建
            {
                if (StructState == HCStructState.hstcStart)  // 开妈构建
                {
                    FStartPt = new POINT(e.X, e.Y);
                    FEndPt = new POINT(e.X, e.Y);
                    StructState = HCStructState.hstcStructing;
                }
                else  // 构建进行中按下，完成构建
                    StructOver();

                vResult = true;
            }
            else
            {
                HCShapeLineObj vLineObje = GetObjAt(e.X, e.Y);
                if (FActiveOjb != vLineObje)
                {
                    FActiveOjb = vLineObje;
                    Active = FActiveOjb != HCShapeLineObj.sloNone;
                    vResult = Active;
                }
                else
                    vResult = vLineObje != HCShapeLineObj.sloNone;

                if ((vResult) && (FActiveOjb == HCShapeLineObj.sloLine))
                {
                    FMousePt.X = e.X;
                    FMousePt.Y = e.Y;
                }
            }

            return vResult;
        }

        public override bool MouseMove(MouseEventArgs e)
        {
            if (StructState == HCStructState.hstcStructing)
            {
                FEndPt = new POINT(e.X, e.Y);
                return true;
            }

            bool vResult = false;
            if ((e.Button == MouseButtons.Left) && (Control.ModifierKeys == Keys.None) && (FActiveOjb != HCShapeLineObj.sloNone))
            {
                vResult = true;

                switch (FActiveOjb)
                {
                    case HCShapeLineObj.sloLine:
                        FStartPt.X = FStartPt.X + e.X - FMousePt.X;
                        FStartPt.Y = FStartPt.Y + e.Y - FMousePt.Y;
                        FEndPt.X = FEndPt.X + e.X - FMousePt.X;
                        FEndPt.Y = FEndPt.Y + e.Y - FMousePt.Y;
                        FMousePt.X = e.X;
                        FMousePt.Y = e.Y;
                        break;

                    case HCShapeLineObj.sloStart:
                        FStartPt.X = e.X;
                        FStartPt.Y = e.Y;
                        break;

                    case HCShapeLineObj.sloEnd:
                        FEndPt.X = e.X;
                        FEndPt.Y = e.Y;
                        break;
                }
            }
            else
            {
                HCShapeLineObj vLineOjb = GetObjAt(e.X, e.Y);
                if (Active && ((vLineOjb == HCShapeLineObj.sloStart) || (vLineOjb == HCShapeLineObj.sloEnd)))
                    this.Cursor = Cursors.Cross;
                else
                if (vLineOjb != HCShapeLineObj.sloNone)
                    this.Cursor = Cursors.SizeAll;

                vResult = vLineOjb != HCShapeLineObj.sloNone;
            }

            return vResult;
        }

        public override bool MouseUp(MouseEventArgs e)
        {
            return false;
        }

        public override void PaintTo(HCCanvas aCanvas, RECT aRect, PaintInfo aPaintInfo)
        {
            aCanvas.Pen.BeginUpdate();
            try
            {
                aCanvas.Pen.Color = Color;
                aCanvas.Pen.Width = FWidth;
                aCanvas.Pen.Style = FLineStyle;
            }
            finally
            {
                aCanvas.Pen.EndUpdate();
            }

            aCanvas.MoveTo(FStartPt.X + aRect.Left, FStartPt.Y + aRect.Top);
            aCanvas.LineTo(FEndPt.X + aRect.Left, FEndPt.Y + aRect.Top);

            if ((!aPaintInfo.Print) && (this.Active))
                PaintAnchor(aCanvas, aRect);
        }

        public override bool PointInClient(int x, int y)
        {
            return GetObjAt(x, y) != HCShapeLineObj.sloNone;
        }

        public override RECT ClientRect()
        {
            RECT vResult = new RECT();

            if (FStartPt.X < FEndPt.X)
            {
                vResult.Left = FStartPt.X;
                vResult.Right = FEndPt.X;
            }
            else
            {
                vResult.Left = FEndPt.X;
                vResult.Right = FStartPt.X;
            }

            if (FStartPt.Y < FEndPt.Y)
            {
                vResult.Top = FStartPt.Y;
                vResult.Bottom = FEndPt.Y;
            }
            else
            {
                vResult.Top = FEndPt.Y;
                vResult.Bottom = FStartPt.Y;
            }

            return vResult;
        }

        public override void SaveToStream(Stream aStream)
        {
            base.SaveToStream(aStream);

            aStream.WriteByte(FWidth);
            aStream.WriteByte((byte)FLineStyle);

            byte[] vBuffer = BitConverter.GetBytes(FStartPt.X);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FStartPt.Y);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FEndPt.X);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FEndPt.Y);
            aStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(Stream aStream)
        {
            base.LoadFromStream(aStream);

            FWidth = (byte)aStream.ReadByte();
            FLineStyle = (HCPenStyle)aStream.ReadByte();

            byte[] vBuffer = BitConverter.GetBytes(FStartPt.X);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FStartPt.X = BitConverter.ToInt32(vBuffer, 0);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            FStartPt.Y = BitConverter.ToInt32(vBuffer, 0);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            FEndPt.X = BitConverter.ToInt32(vBuffer, 0);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            FEndPt.Y = BitConverter.ToInt32(vBuffer, 0);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("width", FWidth.ToString());
            aNode.SetAttribute("ls", ((byte)FLineStyle).ToString());
            aNode.SetAttribute("sx", FStartPt.X.ToString());
            aNode.SetAttribute("sy", FStartPt.Y.ToString());
            aNode.SetAttribute("ex", FEndPt.X.ToString());
            aNode.SetAttribute("ey", FEndPt.Y.ToString());
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FWidth = byte.Parse(aNode.Attributes["width"].Value);
            FLineStyle = (HCPenStyle)byte.Parse(aNode.Attributes["ls"].Value);
            FStartPt.X = int.Parse(aNode.Attributes["sx"].Value);
            FStartPt.Y = int.Parse(aNode.Attributes["sy"].Value);
            FEndPt.X = int.Parse(aNode.Attributes["ex"].Value);
            FEndPt.Y = int.Parse(aNode.Attributes["ey"].Value);
        }

        public POINT StartPt
        {
            get { return FStartPt; }
            set { FStartPt = value; }
        }

        public POINT EndPt
        {
            get { return FEndPt; }
            set { FEndPt = value; }
        }

        public byte Width
        {
            get { return FWidth; }
            set { SetWidth(value); }
        }

        public HCPenStyle LineStyle
        {
            get { return FLineStyle; }
            set { SetLineStyle(value); }
        }

        public HCShapeLineObj ActiveObj
        {
            get { return FActiveOjb; }
        }
    }

    public class HCShapeRectangle : HCShapeLine
    {
        private Color FBackColor;

        protected override HCShapeLineObj GetObjAt(int x, int y)
        {
            HCShapeLineObj vResult = HCShapeLineObj.sloNone;

            if (HC.PtInRect(new RECT(StartPt.X - PointSize, StartPt.Y - PointSize,
                   StartPt.X + PointSize, StartPt.Y + PointSize), new POINT(x, y)))
                vResult = HCShapeLineObj.sloStart;
            else
            if (HC.PtInRect(new RECT(EndPt.X - PointSize, EndPt.Y - PointSize,
                   EndPt.X + PointSize, EndPt.Y + PointSize), new POINT(x, y)))
                vResult = HCShapeLineObj.sloEnd;
            else
            {
                RECT vRect = ClientRect();
                vRect.Inflate(PointSize, PointSize);
                if (HC.PtInRect(vRect, x, y))  // 在边框点宽度外
                {
                    vRect.Inflate(-PointSize - PointSize, -PointSize - PointSize);
                    if (!HC.PtInRect(vRect, x, y))
                        vResult = HCShapeLineObj.sloLine;
                }
            }

            return vResult;
        }

        public HCShapeRectangle() : base()
        {
            Style = HCShapeStyle.hssRectangle;
            FBackColor = HC.HCTransparentColor;
        }

        public override void PaintTo(HCCanvas aCanvas, RECT aRect, PaintInfo aPaintInfo)
        {
            aCanvas.Pen.BeginUpdate();
            try
            {
                aCanvas.Pen.Color = Color;
                aCanvas.Pen.Width = Width;
                aCanvas.Pen.Style = LineStyle;
            }
            finally
            {
                aCanvas.Pen.EndUpdate();
            }

            if (FBackColor == HC.HCTransparentColor)
                aCanvas.Brush.Style = HCBrushStyle.bsClear;

            aCanvas.Rectangle(StartPt.X + aRect.Left, StartPt.Y + aRect.Top,
                EndPt.X + aRect.Left, EndPt.Y + aRect.Top);

            if (!aPaintInfo.Print && this.Active)
                PaintAnchor(aCanvas, aRect);
        }

        public Color BackColor
        {
            get { return FBackColor; }
            set { FBackColor = value; }
        }
    }

    public class HCShapeEllipse : HCShapeRectangle
    {
        protected override HCShapeLineObj GetObjAt(int x, int y)
        {
            HCShapeLineObj vResult = HCShapeLineObj.sloNone;

            if (HC.PtInRect(new RECT(StartPt.X - PointSize, StartPt.Y - PointSize,
                   StartPt.X + PointSize, StartPt.Y + PointSize), new POINT(x, y)))
                vResult = HCShapeLineObj.sloStart;
            else
            if (HC.PtInRect(new RECT(EndPt.X - PointSize, EndPt.Y - PointSize,
                   EndPt.X + PointSize, EndPt.Y + PointSize), new POINT(x, y)))
                vResult = HCShapeLineObj.sloEnd;
            else
            {
                RECT vRect = ClientRect();
                vRect.Inflate(PointSize, PointSize);
                IntPtr vRgn1 = GDI.CreateEllipticRgnIndirect(ref vRect);
                try
                {
                    if (GDI.PtInRegion(vRgn1, x, y))  // 在外围
                    {
                        vRect.Inflate(-PointSize - PointSize, -PointSize - PointSize);
                        IntPtr vRgn2 = GDI.CreateEllipticRgnIndirect(ref vRect);
                        try
                        {
                            if (!GDI.PtInRegion(vRgn2, x, y))  // 不在内围
                                vResult = HCShapeLineObj.sloLine;
                        }
                        finally
                        {
                            GDI.DeleteObject(vRgn2);
                        }
                    }
                }
                finally
                {
                    GDI.DeleteObject(vRgn1);
                }
            }

            return vResult;
        }

        public HCShapeEllipse() : base()
        {
            Style = HCShapeStyle.hssEllipse;
        }

        public override void PaintTo(HCCanvas aCanvas, RECT aRect, PaintInfo aPaintInfo)
        {
            aCanvas.Pen.BeginUpdate();
            try
            {
                aCanvas.Pen.Color = Color;
                aCanvas.Pen.Width = Width;
                aCanvas.Pen.Style = LineStyle;
            }
            finally
            {
                aCanvas.Pen.EndUpdate();
            }

            if (BackColor == HC.HCTransparentColor)
                aCanvas.Brush.Style = HCBrushStyle.bsClear;

            aCanvas.Ellipse(StartPt.X + aRect.Left, StartPt.Y + aRect.Top,
                EndPt.X + aRect.Left, EndPt.Y + aRect.Top);

            if (!aPaintInfo.Print && this.Active)
                PaintAnchor(aCanvas, aRect);
        }
    }

    public class HCPoint
    {
        public int X, Y;

        public HCPoint(int ax, int ay)
        {
            Init(ax, ay);
        }

        public void Init(int ax, int ay)
        {
            X = ax;
            Y = ay;
        }

        public void Offset(int ax, int ay)
        {
            X += ax;
            Y += ay;
        }
    }

    public class HCShapePolygon : HCShape
    {
        POINT FMousePt;
        List<HCPoint> FPoints;
        byte FWidth;
        HCPenStyle FLineStyle;
        int FActivePointIndex, FActiveLineIndex;

        private void OffsetPoints(int x, int y)
        {
            for (int i = 0; i < FPoints.Count; i++)
                FPoints[i].Offset(x, y);
        }

        private void SetWidth(byte value)
        {
            if (FWidth != value)
                FWidth = value;
        }

        private void SetLineStyle(HCPenStyle value)
        {
            if (FLineStyle != value)
                FLineStyle = value;
        }

        protected override void PaintAnchor(HCCanvas aCanvas, RECT aRect)
        {
            aCanvas.Pen.BeginUpdate();
            try
            {
                aCanvas.Pen.Color = Color.Black;
                aCanvas.Pen.Width = 1;
                aCanvas.Pen.Style = HCPenStyle.psSolid;
                aCanvas.Brush.Color = Color.White;
            }
            finally
            {
                aCanvas.Pen.EndUpdate();
            }

            for (int i = 0; i < FPoints.Count; i++)
            {
                aCanvas.Rectangle(FPoints[i].X + aRect.Left - PointSize, FPoints[i].Y + aRect.Top - PointSize,
                    FPoints[i].X + aRect.Left + PointSize, FPoints[i].Y + aRect.Top + PointSize);
            }

            if (FActivePointIndex >= 0)
            {
                aCanvas.Pen.Color = Color.Red;
                if (StructState == HCStructState.hstcStructing)
                    aCanvas.Pen.Style = HCPenStyle.psDot;

                aCanvas.Rectangle(
                    FPoints[FActivePointIndex].X + aRect.Left - PointSize,
                    FPoints[FActivePointIndex].Y + aRect.Top - PointSize,
                    FPoints[FActivePointIndex].X + aRect.Left + PointSize,
                    FPoints[FActivePointIndex].Y + aRect.Top + PointSize);
            }
        }

        protected override void SetActive(bool value)
        {
            base.SetActive(value);
            if (!this.Active)
            {
                FActivePointIndex = -1;
                FActiveLineIndex = -1;
            }
        }

        protected int GetPointAt(int x, int y)
        {
            HCPoint vPoint = null;

            for (int i = 0; i < FPoints.Count; i++)
            {
                vPoint = FPoints[i];
                if (HC.PtInRect(new RECT(vPoint.X - PointSize, vPoint.Y - PointSize,
                     vPoint.X + PointSize, vPoint.Y + PointSize), new POINT(x, y)))
                {
                    return i;
                }
            }

            return -1;
        }

        protected int GetLineAt(int x, int y)
        {
            POINT[] vPointArr = new POINT[4];
            IntPtr vRgn = IntPtr.Zero;

            for (int i = 0; i < FPoints.Count; i++)
            {
                vPointArr[0] = new POINT(FPoints[i].X - PointSize, FPoints[i].Y);
                vPointArr[1] = new POINT(FPoints[i].X + PointSize, FPoints[i].Y);

                if (i == FPoints.Count - 1)
                {
                    vPointArr[2] = new POINT(FPoints[0].X + PointSize, FPoints[0].Y);
                    vPointArr[3] = new POINT(FPoints[0].X - PointSize, FPoints[0].Y);
                }
                else
                {
                    vPointArr[2] = new POINT(FPoints[i + 1].X + PointSize, FPoints[i + 1].Y);
                    vPointArr[3] = new POINT(FPoints[i + 1].X - PointSize, FPoints[i + 1].Y);
                }

                vRgn = GDI.CreatePolygonRgn(vPointArr, 4, GDI.WINDING);
                try
                {
                    if (GDI.PtInRegion(vRgn, x, y))
                        return i;
                }
                finally
                {
                    GDI.DeleteObject(vRgn);
                }
            }

            return -1;
        }

        protected List<HCPoint> Points
        {
            get { return FPoints; }
        }

        public HCShapePolygon() : base()
        {
            Style = HCShapeStyle.hssPolygon;
            FWidth = 1;
            FLineStyle = HCPenStyle.psSolid;
            FPoints = new List<HCPoint>();
            FActivePointIndex = -1;
            FActiveLineIndex = -1;
        }

        public override void Assign(HCShape source)
        {
            base.Assign(source);

            FPoints.Clear();
            for (int i = 0; i < (source as HCShapePolygon).Points.Count; i++)
            {
                HCPoint vPoint = new HCPoint(
                    (source as HCShapePolygon).Points[i].X,
                    (source as HCShapePolygon).Points[i].Y);

                FPoints.Add(vPoint);
            }
        }

        public override bool MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (StructState == HCStructState.hstcStructing)
                    StructOver();

                return false;
            }

            if (e.Button != MouseButtons.Left)
                return false;

            bool vResult = false;
            if (StructState != HCStructState.hstcStop)  // 没有处于构建状态
            {
                if (StructState == HCStructState.hstcStart)  // 开始构建
                {
                    HCPoint vPoint = new HCPoint(e.X, e.Y);
                    FPoints.Add(vPoint);

                    vPoint = new HCPoint(e.X, e.Y);
                    FPoints.Add(vPoint);
                    FActivePointIndex = FPoints.Count - 1;
                    StructState = HCStructState.hstcStructing;
                }
                else
                if (StructState == HCStructState.hstcStructing)
                {
                    HCPoint vPoint = new HCPoint(e.X, e.Y);
                    FPoints.Add(vPoint);
                    FActivePointIndex = FPoints.Count - 1;
                }
                else
                    StructOver();

                vResult = true;
            }
            else
            {
                int vIndex = GetPointAt(e.X, e.Y);
                if (FActivePointIndex != vIndex)
                {
                    FActivePointIndex = vIndex;
                    Active = FActivePointIndex >= 0;
                    vResult = Active;
                }
                else
                    vResult = vIndex >= 0;

                if (!vResult)  // 是否在线段上
                {
                    vIndex = GetLineAt(e.X, e.Y);
                    if (FActiveLineIndex != vIndex)
                    {
                        FActiveLineIndex = vIndex;
                        Active = FActiveLineIndex >= 0;
                        vResult = Active;
                    }
                    else
                        vResult = vIndex >= 0;
                }

                if (vResult)
                {
                    FMousePt.X = e.X;
                    FMousePt.Y = e.Y;
                }
            }

            return vResult;
        }

        public override bool MouseMove(MouseEventArgs e)
        {
            if (StructState == HCStructState.hstcStructing)
            {
                FPoints[FActivePointIndex].Init(e.X, e.Y);
                return true;
            }

            if (e.Button == MouseButtons.Left && Control.ModifierKeys == Keys.None)
            {
                if (FActivePointIndex >= 0)
                {
                    FPoints[FActivePointIndex].Init(e.X, e.Y);
                    return true;
                }
                else
                if (FActiveLineIndex >= 0)  // 整体移动
                {
                    OffsetPoints(e.X - FMousePt.X, e.Y - FMousePt.Y);

                    FMousePt.X = e.X;
                    FMousePt.Y = e.Y;

                    return true;
                }
            }
            else
            {
                int vIndex = GetPointAt(e.X, e.Y);
                if (vIndex >= 0)
                {
                    this.Cursor = Cursors.Cross;
                    return true;
                }
                else
                {
                    vIndex = GetLineAt(e.X, e.Y);
                    if (vIndex >= 0)
                    {
                        this.Cursor = Cursors.SizeAll;
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool MouseUp(MouseEventArgs e)
        {
            return false;
        }

        public override bool KeyDown(KeyEventArgs e)
        {
            if ((e.KeyValue == User.VK_BACK) || (e.KeyValue == User.VK_DELETE))
            {
                if ((StructState == HCStructState.hstcStop) && (FActivePointIndex >= 0))
                {
                    if (FPoints.Count > 2)
                    {
                        FPoints.RemoveAt(FActivePointIndex);
                        FActivePointIndex = -1;
                        return true;
                    }
                }
            }

            return false;
        }

        public override void PaintTo(HCCanvas aCanvas, RECT aRect, PaintInfo aPaintInfo)
        {
            aCanvas.Pen.BeginUpdate();
            try
            {
                aCanvas.Pen.Color = Color;
                aCanvas.Pen.Width = FWidth;
                aCanvas.Pen.Style = FLineStyle;
            }
            finally
            {
                aCanvas.Pen.EndUpdate();
            }

            aCanvas.MoveTo(FPoints[0].X + aRect.Left, FPoints[0].Y + aRect.Top);
            for (int i = 1; i < FPoints.Count; i++)
                aCanvas.LineTo(FPoints[i].X + aRect.Left, FPoints[i].Y + aRect.Top);

            if (FPoints.Count > 1)  // 首尾相连
                aCanvas.LineTo(FPoints[0].X + aRect.Left, FPoints[0].Y + aRect.Top);

            if ((!aPaintInfo.Print) && this.Active)
                PaintAnchor(aCanvas, aRect);
        }

        public override bool PointInClient(int x, int y)
        {
            int vIndex = GetPointAt(x, y);
            if (vIndex >= 0)
                return true;
            else
            {
                vIndex = GetLineAt(x, y);
                if (vIndex >= 0)
                    return true;
            }

            return false;
        }

        public override void StructOver()
        {
            FActivePointIndex = -1;
            FActiveLineIndex = -1;
            if (FPoints.Count > 2)
                FPoints.RemoveAt(FPoints.Count - 1);

            base.StructOver();
        }

        public override void SaveToStream(Stream aStream)
        {
            base.SaveToStream(aStream);

            aStream.WriteByte(FWidth);
            aStream.WriteByte((byte)FLineStyle);

            byte[] vBuffer = BitConverter.GetBytes(FPoints.Count);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            for (int i = 0; i < FPoints.Count; i++)
            {
                vBuffer = BitConverter.GetBytes(FPoints[i].X);
                aStream.Write(vBuffer, 0, vBuffer.Length);
                vBuffer = BitConverter.GetBytes(FPoints[i].Y);
                aStream.Write(vBuffer, 0, vBuffer.Length);
            }
        }

        public override void LoadFromStream(Stream aStream)
        {
            FPoints.Clear();

            base.LoadFromStream(aStream);

            FWidth = (byte)aStream.ReadByte();
            FLineStyle = (HCPenStyle)aStream.ReadByte();

            int vCount = 0;
            byte[] vBuffer = BitConverter.GetBytes(vCount);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vCount = BitConverter.ToInt32(vBuffer, 0);

            int vX = 0, vY = 0;
            HCPoint vPoint = null;
            for (int i = 0; i < vCount; i++)
            {
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vX = BitConverter.ToInt32(vBuffer, 0);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vY = BitConverter.ToInt32(vBuffer, 0);
                vPoint = new HCPoint(vX, vY);
                FPoints.Add(vPoint);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);

            aNode.SetAttribute("width", FWidth.ToString());
            aNode.SetAttribute("ls", ((byte)FLineStyle).ToString());

            XmlElement vNode = null;

            for (int i = 0; i < FPoints.Count; i++)
            {
                vNode = aNode.OwnerDocument.CreateElement("pt");
                vNode.SetAttribute("x", FPoints[i].X.ToString());
                vNode.SetAttribute("y", FPoints[i].Y.ToString());
                aNode.AppendChild(vNode);
            }
        }

        public override void ParseXml(XmlElement aNode)
        {
            FPoints.Clear();

            base.ParseXml(aNode);

            FWidth = byte.Parse(aNode.Attributes["width"].Value);
            FLineStyle = (HCPenStyle)byte.Parse(aNode.Attributes["ls"].Value);

            HCPoint vPoint = null;
            for (int i = 0; i < aNode.ChildNodes.Count; i++)
            {
                vPoint = new HCPoint(int.Parse(aNode.ChildNodes[i].Attributes["x"].Value),
                    int.Parse(aNode.ChildNodes[i].Attributes["y"].Value));

                FPoints.Add(vPoint);
            }
        }
    }

    public class HCShapeManager : HCList<HCShape>
    {
        private int FActiveIndex, FHotIndex;
        private HCShapeStyle FOperStyle;
        private EventHandler FOnStructOver;

        private int NewShape(HCShapeStyle aStyle)
        {
            HCShape vShpae = null;

            switch (aStyle)
            {
                case HCShapeStyle.hssNone:
                    break;

                case HCShapeStyle.hssLine:
                    vShpae = new HCShapeLine();
                    break;

                case HCShapeStyle.hssRectangle:
                    vShpae = new HCShapeRectangle();
                    break;

                case HCShapeStyle.hssEllipse:
                    vShpae = new HCShapeEllipse();
                    break;

                case HCShapeStyle.hssPolygon:
                    vShpae = new HCShapePolygon();
                    break;
            }

            if (vShpae != null)
            {
                vShpae.OnStructOver = DoShapeStructOver;
                this.Add(vShpae);
                return this.Count - 1;
            }

            return -1;
        }

        private void DoShapeStructOver(object sender, EventArgs e)
        {
            ActiveIndex = -1;
            if (FOnStructOver != null)
                FOnStructOver(sender, e);
        }

        private void SetOperStyle(HCShapeStyle value)
        {
            if (FOperStyle != value)
            {
                ActiveIndex = -1;
                FOperStyle = value;
            }
        }

        private void SetActiveIndex(int value)
        {
            if (FActiveIndex != value)
            {
                if (FActiveIndex >= 0)
                    this[FActiveIndex].Active = false;

                FActiveIndex = value;
                if (FActiveIndex >= 0)
                    this[FActiveIndex].Active = true;
            }
        }

        public HCShapeManager() : base()
        {
            FActiveIndex = -1;
            FHotIndex = -1;
            FOperStyle = HCShapeStyle.hssNone;
        }

        public bool MouseDown(MouseEventArgs e)
        {
            bool vResult = false;

            if (FOperStyle != HCShapeStyle.hssNone)
            {
                if (FActiveIndex < 0)
                {
                    ActiveIndex = NewShape(FOperStyle);
                    this[FActiveIndex].StructStart();
                }

                if (FActiveIndex >= 0)
                    vResult = this[FActiveIndex].MouseDown(e);
            }
            else  // 不在绘制
            {
                int vIndex = -1;
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].PointInClient(e.X, e.Y))
                    {
                        if (this[i].MouseDown(e))
                        {
                            vIndex = i;
                            vResult = true;
                            break;
                        }
                    }
                }

                if (vIndex != FActiveIndex)
                {
                    ActiveIndex = vIndex;
                    vResult = true;
                }
            }

            return vResult;
        }

        public bool MouseMove(MouseEventArgs e)
        {
            if (FActiveIndex >= 0)
            {
                if (this[FActiveIndex].MouseMove(e))
                {
                    FHotIndex = FActiveIndex;
                    return true;
                }
            }

            FHotIndex = -1;
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].PointInClient(e.X, e.Y))
                {
                    if (this[i].MouseMove(e))
                    {
                        FHotIndex = i;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool MouseUp(MouseEventArgs e)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].MouseUp(e))
                    return true;
            }

            return false;
        }

        public bool KeyDown(KeyEventArgs e)
        {
            if (FActiveIndex >= 0)
            {
                if (this[FActiveIndex].KeyDown(e))
                    return true;
                else
                if ((e.KeyValue == User.VK_BACK) || (e.KeyValue == User.VK_DELETE))
                {
                    this.RemoveAt(FActiveIndex);
                    FActiveIndex = -1;
                    return true;
                }
            }

            return false;
        }

        public void DisActive()
        {
            FOperStyle = HCShapeStyle.hssNone;
            if (FActiveIndex >= 0)
                this[FActiveIndex].Active = false;
        }

        public void PaintTo(HCCanvas aCanvas, RECT aRect, PaintInfo aPaintInfo)
        {
            for (int i = 0; i < this.Count; i++)
                this[i].PaintTo(aCanvas, aRect, aPaintInfo);
        }

        public void SaveToStream(Stream aStream)
        {
            byte[] vBuffer = BitConverter.GetBytes(this.Count);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            for (int i = 0; i < this.Count; i++)
                this[i].SaveToStream(aStream);
        }

        public void LoadFromStream(Stream aStream)
        {
            this.Clear();

            int vCount = 0;
            byte[] vBuffer = BitConverter.GetBytes(vCount);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vCount = BitConverter.ToInt32(vBuffer, 0);

            HCShape vShape = null;
            HCShapeStyle vStyle = HCShapeStyle.hssNone;
            for (int i = 0; i < vCount; i++)
            {
                vStyle = (HCShapeStyle)aStream.ReadByte();

                switch (vStyle)
                {
                    case HCShapeStyle.hssNone:
                        throw new Exception("HCShape读取失败，无效的样式值！");

                    case HCShapeStyle.hssLine:
                        vShape = new HCShapeLine();
                        break;

                    case HCShapeStyle.hssRectangle:
                        vShape = new HCShapeRectangle();
                        break;

                    case HCShapeStyle.hssEllipse:
                        vShape = new HCShapeEllipse();
                        break;

                    case HCShapeStyle.hssPolygon:
                        vShape = new HCShapePolygon();
                        break;
                }

                aStream.Position = aStream.Position - sizeof(byte);
                vShape.LoadFromStream(aStream);
                this.Add(vShape);
            }
        }

        public void ToXml(XmlElement aNode)
        {
            XmlElement vShapeNode = null;
            for (int i = 0; i < this.Count; i++)
            {
                vShapeNode = aNode.OwnerDocument.CreateElement("shape");
                this[i].ToXml(vShapeNode);
                aNode.AppendChild(vShapeNode);
            }
        }

        public void ParseXml(XmlElement aNode)
        {
            this.Clear();

            HCShape vShape = null;
            HCShapeStyle vStyle = HCShapeStyle.hssNone;
            XmlElement vShapeNode = null;
            for (int i = 0; i < aNode.ChildNodes.Count; i++)
            {
                vShapeNode = aNode.ChildNodes[i] as XmlElement;
                vStyle = (HCShapeStyle)byte.Parse(vShapeNode.Attributes["style"].Value);

                switch (vStyle)
                {
                    case HCShapeStyle.hssNone:
                        throw new Exception("HCShape读取失败，无效的样式值！");

                    case HCShapeStyle.hssLine:
                        vShape = new HCShapeLine();
                        break;

                    case HCShapeStyle.hssRectangle:
                        vShape = new HCShapeRectangle();
                        break;

                    case HCShapeStyle.hssEllipse:
                        vShape = new HCShapeEllipse();
                        break;

                    case HCShapeStyle.hssPolygon:
                        vShape = new HCShapePolygon();
                        break;
                }

                vShape.ParseXml(vShapeNode);
                this.Add(vShape);
            }
        }

        public HCShapeStyle OperStyle
        {
            get { return FOperStyle; }
            set { SetOperStyle(value); }
        }

        public int ActiveIndex
        {
            get { return FActiveIndex; }
            set { SetActiveIndex(value); }
        }

        public int HotIndex
        {
            get { return FHotIndex; }
        }

        public EventHandler OnStructOver
        {
            get { return FOnStructOver; }
            set { FOnStructOver = value; }
        }
    }
}