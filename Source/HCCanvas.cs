/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-11-4             }
{                                                       }
{                自定义画布操作实现单元                 }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using HC.Win32;

namespace HC.View
{
    public static class ColorHelper
    {
        public static uint ToRGB_UInt(this Color color)
        {
            return (uint)(color.B << 16) + (uint)(color.G << 8) + color.R;  // (uint)(color.A << 24) + 
        }

        //public static int ColorToRGB(this Color color)
        //{
        //    int viColor = (color.A << 24) + (color.B << 16) + (color.G << 8) + color.R;
        //    if (viColor < 0)
        //        return (viColor & 0x000000FF);
        //    else
        //        return viColor;
        //}
    }

    public enum HCPenStyle : byte
    {
        psSolid, psDash, psDot, psDashDot, psDashDotDot, psClear, psInsideFrame, psUserStyle, psAlternate
    }

    public enum HCPenMode : byte
    {
        pmBlack, pmWhite, pmNop, pmNot, pmCopy, pmNotCopy, pmMergePenNot, pmMaskPenNot, pmMergeNotPen, pmMaskNotPen, pmMerge,
        pmNotMerge, pmMask, pmNotMask, pmXor, pmNotXor
    }

    public class HCPen : HCObject
    {
        private Color FColor = Color.White;
        private HCPenStyle FStyle = HCPenStyle.psSolid;
        private HCPenMode FMode = HCPenMode.pmCopy;
        private int FWidth = 1;
        private IntPtr FHandle = IntPtr.Zero;
        private LOGPEN FLogPen;
        private int FUpdateCount = 0;

        private EventHandler FOnChanged = null;

        private void ReleaseDC()
        {
            if (FHandle != IntPtr.Zero)
            {
                GDI.DeleteObject(FHandle);
                FHandle = IntPtr.Zero;
            }
        }

        private void ReCreateHandle()
        {
            ReleaseDC();

            switch (FStyle)
            {
                case HCPenStyle.psSolid:
                    FLogPen.lopnStyle = GDI.PS_SOLID;
                    break;

                case HCPenStyle.psDash:
                    FLogPen.lopnStyle = GDI.PS_DASH;
                    break;

                case HCPenStyle.psDot:
                    FLogPen.lopnStyle = GDI.PS_DOT;
                    break;

                case HCPenStyle.psDashDot:
                    FLogPen.lopnStyle = GDI.PS_DASHDOT;
                    break;

                case HCPenStyle.psDashDotDot:
                    FLogPen.lopnStyle = GDI.PS_DASHDOTDOT;
                    break;

                case HCPenStyle.psClear:
                    FLogPen.lopnStyle = GDI.PS_NULL;
                    break;

                case HCPenStyle.psInsideFrame:
                    FLogPen.lopnStyle = GDI.PS_INSIDEFRAME;
                    break;

                case HCPenStyle.psUserStyle:
                    FLogPen.lopnStyle = GDI.PS_USERSTYLE;
                    break;

                case HCPenStyle.psAlternate:
                    FLogPen.lopnStyle = GDI.PS_ALTERNATE;
                    break;
            }

            FLogPen.lopnWidth.X = FWidth;
            FLogPen.lopnColor = FColor.ToRGB_UInt();
            
            if (FHandle == IntPtr.Zero)
                FHandle = (IntPtr)GDI.CreatePenIndirect(ref FLogPen);
        }

        protected void DoChange()
        {
            if (FUpdateCount == 0)
            {
                ReCreateHandle();
                if (FOnChanged != null)
                    FOnChanged(this, null);
            }
        }

        protected void SetColor(Color value)
        {
            if (FColor != value)
            {
                FColor = value;
                DoChange();
            }
        }

        protected void SetStyle(HCPenStyle value)
        {
            if (FStyle != value)
            {
                FStyle = value;
                DoChange();
            }
        }

        protected void SetMode(HCPenMode value)
        {
            if (FMode != value)
            {
                FMode = value;
                DoChange();
            }
        }

        protected void SetWidth(int value)
        {
            if (FWidth != value)
            {
                FWidth = value;
                DoChange();
            }
        }

        public HCPen()
        {
            FLogPen = new LOGPEN();
            ReCreateHandle();
        }

        public override void Dispose()
        {
            base.Dispose();
            ReleaseDC();
        }

        public void BeginUpdate()
        {
            FUpdateCount++;
        }

        public void EndUpdate()
        {
            FUpdateCount--;
            DoChange();
        }

        public Color Color
        {
            get { return FColor; }
            set { SetColor(value); }
        }

        public HCPenStyle Style
        {
            get { return FStyle; }
            set { SetStyle(value); }
        }

        public HCPenMode Mode
        {
            get { return FMode; }
            set { SetMode(value); }
        }

        public int Width
        {
            get { return FWidth; }
            set { SetWidth(value); }
        }

        public IntPtr Handle
        {
            get { return FHandle; }
        }

        public EventHandler OnChanged
        {
            get { return FOnChanged; }
            set { FOnChanged = value; }
        }
    }

    public enum HCBrushStyle : byte
    {
        bsSolid, bsClear, bsHorizontal, bsVertical, bsFDiagonal, bsBDiagonal, bsCross, bsDiagCross
    }

    public class HCBrush : HCObject
    {
        private Color FColor = Color.White;
        private EventHandler FOnChanged = null;
        private IntPtr FHandle = IntPtr.Zero;
        private HCBrushStyle FStyle = HCBrushStyle.bsSolid;
        private LOGBRUSH FLogBrush;
        private int FUpdateCount = 0;

        private void ReleaseDC()
        {
            if (FHandle != IntPtr.Zero)
            {
                GDI.DeleteObject(FHandle);
                FHandle = IntPtr.Zero;
            }
        }

        private void ReCreateHandle()
        {
            ReleaseDC();

            FLogBrush.lbHatch = 0;
            FLogBrush.lbColor = FColor.ToRGB_UInt();

            switch (FStyle)
            {
                case HCBrushStyle.bsSolid:
                    FLogBrush.lbStyle = GDI.BS_SOLID;
                    break;

                case HCBrushStyle.bsClear:
                    FLogBrush.lbStyle = GDI.BS_HOLLOW;
                    break;

                default:
                    FLogBrush.lbStyle = GDI.BS_HATCHED;
                    FLogBrush.lbHatch = (byte)FStyle - (byte)HCBrushStyle.bsHorizontal;
                    break;
            }
            
            if (FHandle == IntPtr.Zero)
                FHandle = (IntPtr)GDI.CreateBrushIndirect(ref FLogBrush);
        }

        protected void DoChange()
        {
            if (FUpdateCount == 0)
            {
                ReCreateHandle();
                if (FOnChanged != null)
                    FOnChanged(this, null);
            }
        }

        protected void SetColor(Color value)
        {
            if ((FColor != value) || (FStyle == HCBrushStyle.bsClear))
            {
                FColor = value;
                if (FStyle == HCBrushStyle.bsClear)
                    FStyle = HCBrushStyle.bsSolid;
                DoChange();
            }
        }

        protected void SetStyle(HCBrushStyle value)
        {
            if (FStyle != value)
            {
                FStyle = value;
                DoChange();
            }
        }

        public HCBrush()
        {
            FLogBrush = new LOGBRUSH();
            ReCreateHandle();
        }

        public override void Dispose()
        {
            base.Dispose();
            ReleaseDC();
        }

        public void BeginUpdate()
        {
            FUpdateCount++;
        }

        public void EndUpdate()
        {
            FUpdateCount--;
            DoChange();
        }

        public Color Color
        {
            get { return FColor; }
            set { SetColor(value); }
        }

        public HCBrushStyle Style
        {
            get { return FStyle; }
            set { SetStyle(value); }
        }

        public IntPtr Handle
        {
            get { return FHandle; }
        }

        public EventHandler OnChanged
        {
            get { return FOnChanged; }
            set { FOnChanged = value; }
        }
    }

    public class HCFontStyles : HCSet
    {
        
    }

    public class HCFont : HCObject
    {
        private Color FColor = Color.Black;
        private Single FSize = 10.5F;  // point size
        private string FFamily = "宋体";
        private IntPtr FHandle = IntPtr.Zero;
        private EventHandler FOnChanged;
        private HCFontStyles FFontStyles;
        private LOGFONT FLogFont;
        private int FPixelsPerInch;
        private int FUpdateCount = 0;

        private void ReleaseDC()
        {
            if (FHandle != IntPtr.Zero)
            {
                GDI.DeleteObject(FHandle);
                FHandle = IntPtr.Zero;
            }
        }

        private void ReCreateHandle()
        {
            ReleaseDC();

            FLogFont.lfHeight = -(int)Math.Round(FSize * FPixelsPerInch / 72);  // -Kernel.MulDiv(FSize, FPixelsPerInch, 72);
            FLogFont.lfWidth = 0;
            FLogFont.lfEscapement = 0;
            FLogFont.lfOrientation = 0;
            if (FFontStyles.Contains((byte)HCFontStyle.tsBold))
                FLogFont.lfWeight = GDI.FW_BOLD;
            else
                FLogFont.lfWeight = GDI.FW_NORMAL;

            if (FFontStyles.Contains((byte)HCFontStyle.tsItalic))
                FLogFont.lfItalic = 1;
            else
                FLogFont.lfItalic = 0;

            if (FFontStyles.Contains((byte)HCFontStyle.tsUnderline))
                FLogFont.lfUnderline = 1;
            else
                FLogFont.lfUnderline = 0;

            if (FFontStyles.Contains((byte)HCFontStyle.tsStrikeOut))
                FLogFont.lfStrikeOut = 1;
            else
                FLogFont.lfStrikeOut = 0;

            FLogFont.lfCharSet = GDI.DEFAULT_CHARSET;
            FLogFont.lfFaceName = FFamily;
            FLogFont.lfQuality = 0;
            FLogFont.lfOutPrecision = GDI.OUT_DEFAULT_PRECIS;
            FLogFont.lfClipPrecision = GDI.CLIP_DEFAULT_PRECIS;
            FLogFont.lfPitchAndFamily = GDI.DEFAULT_PITCH;

            if (FHandle == IntPtr.Zero)
                FHandle = (IntPtr)GDI.CreateFontIndirect(ref FLogFont);
        }

        protected void DoChange()
        {
            if (FUpdateCount == 0)
            {
                ReCreateHandle();
                if (FOnChanged != null)
                    FOnChanged(this, null);
            }
        }

        protected void SetColor(Color value)
        {
            if (FColor != value)
            {
                FColor = value;
                DoChange();
            }
        }

        protected void SetSize(Single value)
        {
            if (FSize != value)
            {
                FSize = value;
                DoChange();
            }
        }

        protected void SetFamily(string value)
        {
            if (FFamily != value)
            {
                FFamily = value;
                DoChange();
            }
        }

        public HCFont()
        {
            FFontStyles = new HCFontStyles();
            FPixelsPerInch = HCCanvas.ScreenLogPixels;
            FLogFont = new LOGFONT();
            ReCreateHandle();
        }

        public override void Dispose()
        {
            base.Dispose();
            ReleaseDC();
            FFontStyles.Dispose();
        }

        public void BeginUpdate()
        {
            FUpdateCount++;
        }

        public void EndUpdate()
        {
            FUpdateCount--;
            DoChange();
        }

        public Color Color
        {
            get { return FColor; }
            set { SetColor(value); }
        }

        public Single Size
        {
            get { return FSize; }
            set { SetSize(value); }
        }

        public string Family
        {
            get { return FFamily; }
            set { SetFamily(value); }
        }

        public HCFontStyles FontStyles
        {
            get { return FFontStyles; }
            set { FFontStyles = value; }
        }

        public IntPtr Handle
        {
            get { return FHandle; }
        }

        public EventHandler OnChanged
        {
            get { return FOnChanged; }
            set { FOnChanged = value; }
        }
    }

    public class HCCanvas : HCObject
    {
        private Graphics FGraphics = null;
        private HCPen FPen;
        private HCBrush FBrush;
        private HCFont FFont;
        private IntPtr FHandle = IntPtr.Zero;
        private int FTextFlags = 0;  // User.DT_LEFT | User.DT_SINGLELINE | User.DT_VCENTER;
        public static int ScreenLogPixels;

        private void ReleaseHCDC()
        {
            if (FHandle != IntPtr.Zero)
            {
                if (FGraphics != null)
                    FGraphics.ReleaseHdc(FHandle);
                else
                    GDI.DeleteDC(FHandle);

                FHandle = IntPtr.Zero;
            }
        }

        private void SelectObject()
        {
            GDI.SelectObject(FHandle, FPen.Handle);
            GDI.SelectObject(FHandle, FBrush.Handle);
            GDI.SelectObject(FHandle, FFont.Handle);
        }

        protected void SetGraphic(Graphics g)
        {
            ReleaseHCDC();

            FGraphics = g;
            FHandle = FGraphics.GetHdc();
            SelectObject();
        }

        protected void DoPenChanged(object sender, EventArgs e)
        {
            GDI.SelectObject(FHandle, Pen.Handle);

            switch (FPen.Mode)
            {
                case HCPenMode.pmBlack:
                    GDI.SetROP2(FHandle, GDI.R2_BLACK);
                    break;

                case HCPenMode.pmWhite:
                    GDI.SetROP2(FHandle, GDI.R2_WHITE);
                    break;

                case HCPenMode.pmNop:
                    GDI.SetROP2(FHandle, GDI.R2_NOP);
                    break;

                case HCPenMode.pmNot:
                    GDI.SetROP2(FHandle, GDI.R2_NOT);
                    break;

                case HCPenMode.pmCopy:
                    GDI.SetROP2(FHandle, GDI.R2_COPYPEN);
                    break;

                case HCPenMode.pmNotCopy:
                    GDI.SetROP2(FHandle, GDI.R2_NOTCOPYPEN);
                    break;

                case HCPenMode.pmMergePenNot:
                    GDI.SetROP2(FHandle, GDI.R2_MERGEPENNOT);
                    break;

                case HCPenMode.pmMaskPenNot:
                    GDI.SetROP2(FHandle, GDI.R2_MASKPENNOT);
                    break;

                case HCPenMode.pmMergeNotPen:
                    GDI.SetROP2(FHandle, GDI.R2_MERGENOTPEN);
                    break;

                case HCPenMode.pmMaskNotPen:
                    GDI.SetROP2(FHandle, GDI.R2_MASKNOTPEN);
                    break;

                case HCPenMode.pmMerge:
                    GDI.SetROP2(FHandle, GDI.R2_MERGEPEN);
                    break;

                case HCPenMode.pmNotMerge:
                    GDI.SetROP2(FHandle, GDI.R2_NOTMERGEPEN);
                    break;

                case HCPenMode.pmMask:
                    GDI.SetROP2(FHandle, GDI.R2_MASKPEN);
                    break;

                case HCPenMode.pmNotMask:
                    GDI.SetROP2(FHandle, GDI.R2_NOTMASKPEN);
                    break;

                case HCPenMode.pmXor:
                    GDI.SetROP2(FHandle, GDI.R2_XORPEN);
                    break;

                case HCPenMode.pmNotXor:
                    GDI.SetROP2(FHandle, GDI.R2_NOTXORPEN);
                    break;
            }
        }

        protected void DoBrushChanged(object sender, EventArgs e)
        {
            GDI.UnrealizeObject(FBrush.Handle);
            GDI.SelectObject(FHandle, FBrush.Handle);
            if (FBrush.Style == HCBrushStyle.bsSolid)
            {
                GDI.SetBkColor(FHandle, FBrush.Color.ToRGB_UInt());
                GDI.SetBkMode(FHandle, GDI.OPAQUE);
            }
            else
            {
                GDI.SetBkColor(FHandle, ~FBrush.Color.ToRGB_UInt());
                GDI.SetBkMode(FHandle, GDI.TRANSPARENT);
            }
        }

        protected void DoFontChanged(object sender, EventArgs e)
        {
            GDI.SelectObject(FHandle, FFont.Handle);
            GDI.SetTextColor(FHandle, (int)Font.Color.ToRGB_UInt());
        }

        public HCCanvas()
        {
            IntPtr vDC = User.GetDC(IntPtr.Zero);
            try
            {
                ScreenLogPixels = GDI.GetDeviceCaps(vDC, GDI.LOGPIXELSY);
            }
            finally
            {
                User.ReleaseDC(IntPtr.Zero, vDC);
            }

            FPen = new HCPen();
            FPen.OnChanged = DoPenChanged;

            FBrush = new HCBrush();
            FBrush.OnChanged = DoBrushChanged;

            FFont = new HCFont();
            FFont.OnChanged = DoFontChanged;
        }

        public HCCanvas(IntPtr dc) : this()
        {
            FHandle = dc;
            SelectObject();
        }

        ~HCCanvas()
        {

        }

        public override void Dispose()
        {
            base.Dispose();
            ReleaseHCDC();

            FPen.Dispose();
            FBrush.Dispose();
            FFont.Dispose();
            GC.Collect();
        }

        public int Save()
        {
            return GDI.SaveDC(FHandle);
        }

        public int Restore(int ASavedDC)
        {
            return GDI.RestoreDC(FHandle, ASavedDC);
        }

        public int TextWidth(char c)
        {
            return TextWidth(c.ToString());
        }

        public int TextWidth(string AText)
        {
            SIZE vSize = new SIZE(0, 0);
            GDI.GetTextExtentPoint32(FHandle, AText, AText.Length, ref vSize);
            return vSize.cx;
        }

        public int TextHeight(string AText)
        {
            SIZE vSize = new SIZE(0, 0);
            GDI.GetTextExtentPoint32(FHandle, AText, AText.Length, ref vSize);
            return vSize.cy;
        }

        public void DrawLine(int x1, int y1, int x2, int y2)
        {
            GDI.MoveToEx(FHandle, x1, y1, IntPtr.Zero);
            GDI.LineTo(FHandle, x2, y2);
        }

        public void DrawLines(Point[] APoints)
        {
            GDI.MoveToEx(FHandle, APoints[0].X, APoints[0].Y, IntPtr.Zero);

            for (int i = 1; i < APoints.Length; i++)
            {
                GDI.LineTo(FHandle, APoints[i].X, APoints[i].Y);
            }
        }

        public void GetTextMetrics(ref TEXTMETRIC ATextMetric)
        {
            GDI.GetTextMetrics(FHandle, ref ATextMetric);
        }

        public void GetTextExtentExPoint(string AText, int ALen, int[] alpDx, ref SIZE ASize)  // 超过65535数组元素取不到值
        {
            GDI.GetTextExtentExPoint(FHandle, AText, ALen, 0, IntPtr.Zero, alpDx, ref ASize);
        }

        public void FillRect(RECT ARect)
        {
            User.FillRect(FHandle, ref ARect, FBrush.Handle);
        }

        public void RoundRect(RECT ARect, int x, int y)
        {
            GDI.RoundRect(FHandle, ARect.Left, ARect.Top, ARect.Right, ARect.Bottom, x, y);
        }

        public void DrawFocuseRect(RECT ARect)
        {
            User.DrawFocusRect(FHandle, ref ARect);
        }

        public void MoveTo(int x, int y)
        {
            GDI.MoveToEx(FHandle, x, y, IntPtr.Zero);
        }

        public void LineTo(int x, int y)
        {
            GDI.LineTo(FHandle, x, y);
        }

        public void TextOut(int x, int y, string Text)
        {
            GDI.ExtTextOut(FHandle, x, y, FTextFlags, IntPtr.Zero, Text, Text.Length, IntPtr.Zero);
            MoveTo(x + TextWidth(Text), y);
        }

        public void Rectangle(RECT ARect)
        {
            Rectangle(ARect.Left, ARect.Top, ARect.Right, ARect.Bottom);
        }

        public void Rectangle(int ALeft, int ATop, int ARight, int ABottom)
        {
            GDI.Rectangle(FHandle, ALeft, ATop, ARight, ABottom);
        }

        public void StretchDraw(RECT ARect, Bitmap ABitmap)
        {
            Graphics vGraphics = Graphics.FromHdc(FHandle);
            vGraphics.DrawImage(ABitmap, new Rectangle(ARect.Left, ARect.Top, ARect.Width, ARect.Height));
        }

        public HCPen Pen
        {
            get { return FPen; }
        }

        public HCBrush Brush
        {
            get { return FBrush; }
        }

        public HCFont Font
        {
            get { return FFont; }
        }

        public IntPtr Handle
        {
            get { return FHandle; }
        }

        public Graphics Graphics
        {
            get { return FGraphics; }
            set { SetGraphic(value); }
        }
    }
}
