/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                  文档滚动条实现单元                   }
{                                                       }
{*******************************************************/

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using HC.Win32;

namespace HC.View
{
    public enum Orientation : byte
    {
        oriHorizontal, oriVertical
    }

    public enum ScrollCode : byte 
    {
        scLineUp, scLineDown, scPageUp, scPageDown, scPosition,
        scTrack, scTop, scBottom, scEndScroll
    }

    enum BarControl : byte 
    {
        cbcBar, cbcLeftBtn, cbcThum, cbcRightBtn
    }

    public delegate void ScrollEventHandler(object Sender, ScrollCode ScrollCode, int ScrollPos);

    public class HCScrollBar : Control
    {
        public const int ButtonSize = 20;
        private static Color LineColor = HC.clMedGray;
        private const int IconWidth = 16;
        private Color Color = Color.FromArgb(0xAA, 0xAB, 0xB3);
        private static Color ThumBackColor = Color.FromArgb(0xD0, 0xD1, 0xD5);

        private int FMin, FMax, FRange, FPosition, FBtnStep, FPageSize;
        private Single FPercent;
        private Orientation FOrientation;
        private ScrollEventHandler FOnScroll;
        private BarControl FMouseDownControl;
        private EventHandler FOnVisibleChanged;
        protected POINT FMouseDownPt;
        protected RECT FThumRect, FLeftBtnRect, FRightBtnRect;
        protected int FLeftBlank, FRightBlank;

        /// <summary>
        /// 得到鼠标上去要实现改变的区域
        /// </summary>
        private void ReCalcButtonRect()
        {
            if (FOrientation == View.Orientation.oriHorizontal)
            {
                FLeftBtnRect = HC.Bounds(FLeftBlank, 0, ButtonSize, Height);
                FRightBtnRect = HC.Bounds(Width - FRightBlank - ButtonSize, 0, ButtonSize, Height);
            }
            else
            {
                FLeftBtnRect = HC.Bounds(0, FLeftBlank, Width, ButtonSize);
                FRightBtnRect = HC.Bounds(0, Height - FRightBlank - ButtonSize, Width, ButtonSize);
            }
        }

        /// <summary>
        /// 计算滑块区域
        /// </summary>
        private void ReCalcThumRect()
        {
            Single vPer = 0F;
            int vThumHeight = 0;

            if (FOrientation == View.Orientation.oriHorizontal)
            {
                FThumRect.Top = 0;
                FThumRect.Bottom = Height;
                if (FPageSize < FRange)
                {
                    vPer = FPageSize / (float)FRange;  // 计算滑块比例
                    // 计算滑块的高度
                    vThumHeight = (int)Math.Round((Width - FLeftBlank - FRightBlank - 2 * ButtonSize) * vPer);
                    if (vThumHeight < ButtonSize)
                        vThumHeight = ButtonSize;
                    
                    FPercent = (Width - FLeftBlank - FRightBlank - 2 * ButtonSize - vThumHeight) / (float)(FRange - FPageSize);  // 界面可滚动范围和实际代表范围的比率
                    if (FPercent < 0)
                        return;
                    
                    FThumRect.Left = FLeftBlank + ButtonSize + (int)Math.Round(FPosition * FPercent);
                    FThumRect.Right = FThumRect.Left + vThumHeight;
                }
                else  // 滚动轨道大于等于范围
                {
                    FThumRect.Left = FLeftBlank + ButtonSize;
                    FThumRect.Right = Width - FRightBlank - ButtonSize;
                }
            }
            else
            {
                FThumRect.Left = 0;
                FThumRect.Right = Width;
                if (FPageSize < FRange)
                {
                    vPer = FPageSize / (float)FRange;  // 计算滑块比例
                    // 计算滑块的高度
                    vThumHeight = (int)Math.Round((Height - FLeftBlank - FRightBlank - 2 * ButtonSize) * vPer);
                    if (vThumHeight < ButtonSize)
                        vThumHeight = ButtonSize;
                    FPercent = (Height - FLeftBlank - FRightBlank - 2 * ButtonSize - vThumHeight) / (float)(FRange - FPageSize);  // 界面可滚动范围和实际代表范围的比率
                    if (FPercent < 0)
                        return;

                    FThumRect.Top = FLeftBlank + ButtonSize + (int)Math.Round(FPosition * FPercent);
                    FThumRect.Bottom = FThumRect.Top + vThumHeight;
                    //Scroll(scTrack, FPosition);  //鼠标移动改变滑块的垂直位置
                }
                else  // 滚动轨道大于等于范围
                {
                    FThumRect.Top = FLeftBlank + ButtonSize;
                    FThumRect.Bottom = Height - FRightBlank - ButtonSize;
                }
            }

            if (FPercent == 0)
                FPercent = 1;
        }

        /// <summary>
        /// 设置滚动条类型（垂直滚动条、水平滚动条）
        /// </summary>
        /// <param name="Value">滚动条类型</param>
        private void SetOrientation(Orientation Value)
        {
            if (FOrientation != Value)
            {
                FOrientation = Value;
                if (Value == View.Orientation.oriHorizontal)
                    Height = 20;  // 赋值水平滚动条的高度为 20
                else
                if (Value == View.Orientation.oriVertical)
                    Width = 20;

                ReCalcButtonRect();
                ReCalcThumRect();
                UpdateRangRect();  // 重绘
            }
        }

        /// <summary>
        /// 设置滚动条的最小值
        /// </summary>
        /// <param name="Value">最小值</param>
        private void SetMin(int Value)
        {
            if (FMin != Value)
            {
                if (Value > FMax)
                    FMin = FMax;
                else
                    FMin = Value;

                if (FPosition < FMin)
                    FPosition = FMin;
                
                FRange = FMax - FMin;
                ReCalcThumRect();  // 滑块区域
                UpdateRangRect();  // 重绘
            }
        }

        /// <summary>
        /// 设置滚动条的最大值
        /// </summary>
        /// <param name="Value">最大值</param>
        private void SetMax(int Value)
        {
            if (FMax != Value)
            {
                if (Value < FMin)
                    FMax = FMin;
                else
                    FMax = Value;
                
                if (FPosition + FPageSize > FMax)
                    FPosition = Math.Max(FMax - FPageSize, FMin);
                
                FRange = FMax - FMin;
                ReCalcThumRect();  // 滑块区域
                UpdateRangRect();  // 重绘
            }
        }

        /// <summary>
        /// 设置滚动条的初始位置
        /// </summary>
        /// <param name="Value">初始位置</param>
        private void SetPosition(int Value)
        {
            int vPos = 0;

            if (Value < FMin)
                vPos = FMin;
            else
                if (Value + FPageSize > FMax)
                    vPos = Math.Max(FMax - FPageSize, FMin);
                else
                    vPos = Value;
            
            if (FPosition != vPos)
            {
                FPosition = vPos;
                ReCalcThumRect();  // 滑块区域
                //Repaint;
                UpdateRangRect();  // 重绘

                if (FOnScroll != null)
                    FOnScroll(this, ScrollCode.scPosition, FPosition);
            }
        }

        /// <summary>
        /// 设置滚动条表示的页面大小（相对Max - Min）
        /// </summary>
        /// <param name="Value">页面大小</param>
        private void SetPageSize(int Value )
        {
            if (FPageSize != Value)
            {
                FPageSize = Value;
                //ReCalcButtonRect;
                ReCalcThumRect();  // 重新计算相对比率（相对Max - Min）
                UpdateRangRect();  // 重绘
            }
        }

        /// <summary>
        /// 点击滚动条按钮页面移动范围
        /// </summary>
        /// <param name="Value">移动范围</param>
        private void SetBtnStep(int Value)
        {
            if (FBtnStep != Value)
                FBtnStep = Value;
        }

        private void UpdateRangRect()
        {
            if (IsHandleCreated)
            {
                //this.Invalidate(this.ClientRectangle);
                //this.Update();

                RECT vRect = new RECT(0, 0, Width, Height);
                User.InvalidateRect(this.Handle, ref vRect, 0);
                //User.UpdateWindow(Handle);
            }
        }

        private bool PtInLeftBlankArea(int x, int y)
        {
            if (FLeftBlank != 0)
            {
                if (FOrientation == Orientation.oriHorizontal)
                    return HC.PtInRect(HC.Bounds(0, 0, FLeftBlank, Height), new POINT(x, y));
                else
                    return HC.PtInRect(HC.Bounds(0, 0, Width, FLeftBlank), new POINT(x, y));
            }

            return false;
        }

        private bool PtInRightBlankArea(int x, int y)
        {
            if (FRightBlank != 0)
            {
                if (FOrientation == Orientation.oriHorizontal)
                    return HC.PtInRect(HC.Bounds(Width - FRightBlank, 0, FRightBlank, Height), new POINT(x, y));
                else
                    return HC.PtInRect(HC.Bounds(0, Height - FRightBlank, Width, FRightBlank), new POINT(x, y));
            }

            return false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (FOrientation == View.Orientation.oriVertical)
                this.FPageSize = Height;
            else
                this.FPageSize = Width;

            if (FPosition + FPageSize > FMax)
                FPosition = Math.Max(FMax - FPageSize, FMin);

            ReCalcThumRect();  // 重新计算滑块区域
            ReCalcButtonRect();  // 重新计算按钮区域
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
        }

        protected void ScrollStep(ScrollCode ScrollCode)
        {
            int vPos = 0;
            switch (ScrollCode)
            {
                case View.ScrollCode.scLineUp:
                    vPos = FPosition - FBtnStep;
                    if (vPos < FMin)
                        vPos = FMin;

                    if (FPosition != vPos)
                        Position = vPos;
                    break;

                case View.ScrollCode.scLineDown:
                    vPos = FPosition + FBtnStep;
                    if (vPos > FRange - FPageSize)
                        vPos = FRange - FPageSize;

                    if (FPosition != vPos)
                        Position = vPos;
                    break;

                case View.ScrollCode.scPageUp:
                    vPos = FPosition - FPageSize;
                    if (vPos < FMin)
                        vPos = FMin;

                    if (FPosition != vPos)
                        Position = vPos;
                    break;

                case View.ScrollCode.scPageDown:
                    vPos = FPosition + FPageSize;
                    if (vPos > FRange - FPageSize)
                        vPos = FRange - FPageSize;

                    if (FPosition != vPos)
                        Position = vPos;
                    break;

                default:
                    break;
            }
        }

        public HCScrollBar() : base()
        {
            SetStyle(ControlStyles.Selectable, false);
            FMin = 0;
            FMax = 100;
            FRange = 100;
            FPageSize = 0;
            FBtnStep = 5;
            FLeftBlank = 0;
            FRightBlank = 0;
            //
            Width = 20;
            Height = 20;
            
            FOrientation = Orientation.oriHorizontal;
            this.Cursor = Cursors.Default;
            this.DoubleBuffered = true;
        }

        ~HCScrollBar()
        {

        }

        protected override void OnGotFocus(EventArgs e)
        {
            //base.OnGotFocus(e);
            (this.Parent as UserControl).Focus();
        }

        public void DoMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            FMouseDownPt.X = e.X;
            FMouseDownPt.Y = e.Y;
            if (HC.PtInRect(FLeftBtnRect, FMouseDownPt))
            {
                FMouseDownControl = BarControl.cbcLeftBtn;  // 鼠标所在区域类型
                ScrollStep(ScrollCode.scLineUp);  // 数据向上（左）滚动
            }
            else
            if (HC.PtInRect(FThumRect, FMouseDownPt))
            {
                FMouseDownControl = BarControl.cbcThum;
            }
            else
            if (HC.PtInRect(FRightBtnRect, FMouseDownPt))
            {
                FMouseDownControl = BarControl.cbcRightBtn;
                ScrollStep(ScrollCode.scLineDown);  // 数据向下（右）滚动
            }
            else  // 鼠标在滚动条的其他区域
            if (PtInLeftBlankArea(e.X, e.Y))  // 左空白区域
            {

            }
            else
            if (PtInRightBlankArea(e.X, e.Y))  // 右空白区域
            {

            }
            else
            {
                FMouseDownControl = BarControl.cbcBar;  // 滚动条其他区域类型
                if ((FThumRect.Top > e.Y) || (FThumRect.Left > e.X))
                    ScrollStep(ScrollCode.scPageUp); // 数据向上（左）翻页
                else
                if ((FThumRect.Bottom < e.Y) || (FThumRect.Right < e.X))
                    ScrollStep(ScrollCode.scPageDown);  // 数据向下（右）翻页
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            DoMouseDown(e);
        }

        public void DoMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int vOffs = 0;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (FOrientation == View.Orientation.oriHorizontal)
                {
                    if (FMouseDownControl == BarControl.cbcThum)
                    {
                        vOffs = e.X - FMouseDownPt.X;
                        Position = FPosition + (int)Math.Round(vOffs / FPercent);
                        FMouseDownPt.X = e.X;  // 对水平坐标赋值
                    }
                }
                else  // 垂直
                {
                    if (FMouseDownControl == BarControl.cbcThum)
                    {
                        vOffs = e.Y - FMouseDownPt.Y;  // 拖块在最下面时，往下快速拖动，还是会触发滚动事件，造成闪烁，如何解决？word是限制拖动块附近的范围
                        Position = FPosition + (int)Math.Round(vOffs / FPercent);
                        FMouseDownPt.Y = e.Y;  // 对垂直坐标赋当前Y值
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            DoMouseMove(e);
        }

        public void DoMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            DoMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ///////base.OnPaint(e);
            using (HCCanvas vCanvas = new HCCanvas())
            {
                vCanvas.Graphics = e.Graphics;
                PaintToEx(vCanvas);
            }
        }

        protected virtual void DoDrawThumBefor(HCCanvas ACanvas, RECT AThumRect) { }

        protected Single Percent
        {
            get { return FPercent; }
            set { FPercent = value; }
        }

        public virtual void PaintToEx(HCCanvas ACanvas)
        {
            RECT vRect = new RECT();
            ACanvas.Brush.Color = this.Color;
            ACanvas.FillRect(HC.Bounds(0, 0, Width, Height));

            if (FOrientation == Orientation.oriHorizontal)  // 水平滚动条
            {
                // 左按钮
                ACanvas.Pen.Color = Color.White;
                vRect.Left = FLeftBtnRect.Left + ((FLeftBtnRect.Right - FLeftBtnRect.Left) - 4) / 2 + 4;
                vRect.Top = FLeftBtnRect.Top + ((FLeftBtnRect.Bottom - FLeftBtnRect.Top) - 7) / 2;
                ACanvas.DrawLine(vRect.Left, vRect.Top, vRect.Left, vRect.Top + 7);
                ACanvas.DrawLine(vRect.Left - 1, vRect.Top + 1, vRect.Left - 1, vRect.Top + 6);
                ACanvas.DrawLine(vRect.Left - 2, vRect.Top + 2, vRect.Left - 2, vRect.Top + 5);
                ACanvas.DrawLine(vRect.Left - 3, vRect.Top + 3, vRect.Left - 3, vRect.Top + 4);
                // 右按钮
                vRect.Left = FRightBtnRect.Left + ((FRightBtnRect.Right - FRightBtnRect.Left) - 4) / 2;
                vRect.Top = FRightBtnRect.Top + ((FRightBtnRect.Bottom - FRightBtnRect.Top) - 7) / 2;
                ACanvas.DrawLine(vRect.Left, vRect.Top, vRect.Left, vRect.Top + 7);
                ACanvas.DrawLine(vRect.Left + 1, vRect.Top + 1, vRect.Left + 1, vRect.Top + 6);
                ACanvas.DrawLine(vRect.Left + 2, vRect.Top + 2, vRect.Left + 2, vRect.Top + 5);
                ACanvas.DrawLine(vRect.Left + 3, vRect.Top + 3, vRect.Left + 3, vRect.Top + 4);
                // 水平滑块
                vRect = FThumRect;
                HC.InflateRect(ref vRect, 0, -1);
                DoDrawThumBefor(ACanvas, vRect);
                ACanvas.Brush.Color = ThumBackColor;
                ACanvas.Pen.Color = LineColor;
                ACanvas.Rectangle(vRect);
                // 滑块上的修饰
                vRect.Left = vRect.Left + (vRect.Right - vRect.Left) / 2;
                ACanvas.DrawLine(vRect.Left, 5, vRect.Left, Height - 5);
                ACanvas.DrawLine(vRect.Left + 3, 5, vRect.Left + 3, Height - 5);
                ACanvas.DrawLine(vRect.Left - 3, 5, vRect.Left - 3, Height - 5);
            }
            else  // 垂直滚动条
            {
                // 上按钮
                ACanvas.Pen.Color = Color.White;
                vRect.Left = FLeftBtnRect.Left + ((FLeftBtnRect.Right - FLeftBtnRect.Left) - 7) / 2;
                vRect.Top = FLeftBtnRect.Top + ((FLeftBtnRect.Bottom - FLeftBtnRect.Top) - 4) / 2 + 4;

                ACanvas.DrawLine(6, 12, 13, 12);
                ACanvas.DrawLine(vRect.Left, vRect.Top, vRect.Left + 7, vRect.Top);
                ACanvas.DrawLine(vRect.Left + 1, vRect.Top - 1, vRect.Left + 6, vRect.Top - 1);
                ACanvas.DrawLine(vRect.Left + 2, vRect.Top - 2, vRect.Left + 5, vRect.Top - 2);
                ACanvas.DrawLine(vRect.Left + 3, vRect.Top - 3, vRect.Left + 4, vRect.Top - 3);
                // 下按钮
                vRect.Left = FRightBtnRect.Left + ((FRightBtnRect.Right - FRightBtnRect.Left) - 7) / 2;
                vRect.Top = FRightBtnRect.Top + ((FRightBtnRect.Bottom - FRightBtnRect.Top) - 4) / 2;
                ACanvas.DrawLine(vRect.Left, vRect.Top, vRect.Left + 7, vRect.Top);
                ACanvas.DrawLine(vRect.Left + 1, vRect.Top + 1, vRect.Left + 6, vRect.Top + 1);
                ACanvas.DrawLine(vRect.Left + 2, vRect.Top + 2, vRect.Left + 5, vRect.Top + 2);
                ACanvas.DrawLine(vRect.Left + 3, vRect.Top + 3, vRect.Left + 4, vRect.Top + 3);
                // 滑块
                vRect = FThumRect;
                HC.InflateRect(ref vRect, -1, 0);
                DoDrawThumBefor(ACanvas, vRect);
                ACanvas.Brush.Color = ThumBackColor;
                ACanvas.Pen.Color = LineColor;
                ACanvas.Rectangle(vRect);
                // 滑块上的修饰
                vRect.Top = vRect.Top + (vRect.Bottom - vRect.Top) / 2;
                ACanvas.DrawLine(5, vRect.Top, Width - 5, vRect.Top);
                ACanvas.DrawLine(5, vRect.Top - 3, Width - 5, vRect.Top - 3);
                ACanvas.DrawLine(5, vRect.Top + 3, Width - 5, vRect.Top + 3);
            }
        }

        public int Max
        {
            get { return FMax; }
            set { SetMax(value); }
        }

        public int Min
        {
            get { return FMin; }
            set { SetMin(value); }
        }

        public int Rang
        {
            get { return FRange; }
        }

        public int PageSize
        {
            get { return FPageSize; }
            set { SetPageSize(value); }
        }

        public int BtnStep
        {
            get { return FBtnStep; }
            set { SetBtnStep(value); }
        }

        public int Position
        {
            get { return FPosition; }
            set { SetPosition(value); }
        }

        public Orientation Orientation
        {
            get { return FOrientation; }
            set { SetOrientation(value); }
        }

        public ScrollEventHandler OnScroll
        {
            get { return FOnScroll; }
            set { FOnScroll = value; }
        }

        public new EventHandler OnVisibleChanged
        {
            get { return FOnVisibleChanged; }
            set { FOnVisibleChanged = value; }
        }
    }
}
