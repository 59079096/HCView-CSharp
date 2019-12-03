using HC.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HC.View
{
    public class HCToolBarControl
    {
        private int FWidth, FHeight;
        EventHandler FOnResize;

        private void DoResize()
        {
            if (FOnResize != null)
                FOnResize(this, null);
        }

        protected void SetWdith(int value)
        {
            if (FWidth != value)
            {
                FWidth = value;
                DoResize();
            }
        }

        protected void SetHeight(int value)
        {
            if (FHeight != value)
            {
                FHeight = value;
                DoResize();
            }
        }

        public string Text;
        public int Tag;

        public HCToolBarControl()
        {
            FWidth = 20;
            FHeight = 20;
        }

        public virtual void PaintTo(int aLeft, int aTop, HCCanvas aCanvas)
        {
            aCanvas.Pen.Color = Color.Blue;
            aCanvas.Rectangle(HC.Bounds(aLeft, aTop, FWidth, FHeight));
        }

        public int Width
        {
            get { return FWidth; }
            set { SetWdith(value); }
        }

        public int Height
        {
            get { return FHeight; }
            set { SetHeight(value); }
        }
    }

    public class HCCustomToolButton : HCToolBarControl
    {

    }

    public class HCToolButton : HCCustomToolButton
    {

    }

    public class HCToolControls : HCList<HCToolBarControl>
    {
        private EventHandler FOnCountChange;

        private void DoOnInsert(object sender, NListEventArgs<HCToolBarControl> e)
        {
            if (FOnCountChange != null)
                FOnCountChange(e.Item, null);
        }

        private void DoDelete(object sender, NListEventArgs<HCToolBarControl> e)
        {
            if (FOnCountChange != null)
                FOnCountChange(e.Item, null);
        }

        public HCToolControls()
        {
            this.OnInsert += new EventHandler<NListEventArgs<HCToolBarControl>>(DoOnInsert);
            this.OnDelete += new EventHandler<NListEventArgs<HCToolBarControl>>(DoDelete);
        }

        public EventHandler OnCountChange
        {
            get { return FOnCountChange; }
            set { FOnCountChange = value; }
        }
    }

    public delegate void UpdateViewEventHandler(RECT aRect, HCCanvas aCanvase);

    public delegate void ToolBarControlPaintEventHandle(HCToolBarControl control, int left, int top, HCCanvas canvas);
    public delegate void ToolBarControlClickEventHandle(HCToolBarControl control);

    public class HCToolBar
    {
        private bool FVisible;
        private byte FPadding;
        int FLeft, FTop, FHotIndex, FActiveIndex;
        HCToolControls FControls;
        Bitmap FGraphic;
        HCCanvas FGraphicCanvas;
        UpdateViewEventHandler FOnUpdateView;
        ToolBarControlPaintEventHandle FOnControlPaint;
        ToolBarControlClickEventHandle FOnControlClick;

        private void DoControlCountChange(object sender, EventArgs e)
        {
            SetBounds();
        }

        private int GetWidth()
        {
            return FGraphic.Width;
        }

        private int GetHeight()
        {
            return FGraphic.Height;
        }

        private int GetControlAt(int x, int y)
        {
            POINT vPt = new POINT(x, y);
            int vLeft = FPadding;
            for (int i = 0; i < FControls.Count; i++)
            {
                if (HC.PtInRect(HC.Bounds(vLeft, 0, FControls[i].Width, FControls[i].Height), vPt))
                    return i;

                vLeft += FControls[i].Width + FPadding;
            }

            return -1;
        }

        protected virtual void SetVisible(bool value)
        {
            if (FVisible != value)
            {
                FVisible = value;
                UpdateView();
            }
        }

        protected void SetActiveIndex(int value)
        {
            if (FActiveIndex != value)
            {
                FActiveIndex = value;
                UpdateView();
            }
        }

        public HCToolBar()
        {
            FVisible = false;
            FPadding = 5;
            FHotIndex = -1;
            FActiveIndex = -1;
            FGraphic = new Bitmap(10, 25);
            FGraphicCanvas = new HCCanvas();
            FGraphicCanvas.Graphics = Graphics.FromImage(FGraphic);
            FControls = new HCToolControls();
            FControls.OnCountChange = DoControlCountChange;
        }

        ~HCToolBar()
        {
            FGraphic.Dispose();
        }

        public void MouseEnter()
        {

        }
        public void MouseLeave()
        {
            if (FHotIndex >= 0)
            {
                FHotIndex = -1;
                UpdateView();
            }
        }

        public void MouseDown(MouseEventArgs e)
        {
            ActiveIndex = GetControlAt(e.X, e.Y);
        }

        public void MouseMove(MouseEventArgs e)
        {
            int vIndex = GetControlAt(e.X, e.Y);
            if (FHotIndex != vIndex)
            {
                FHotIndex = vIndex;
                UpdateView();
            }
        }

        public void MouseUp(MouseEventArgs e)
        {
            if ((FHotIndex >= 0) && (FOnControlClick != null))
                FOnControlClick(FControls[FHotIndex]);
        }

        public int AddControl(HCToolBarControl aControl)
        {
            FControls.Add(aControl);
            return FControls.Count - 1;
        }

        public HCToolButton AddButton()
        {
            HCToolButton Result = new HCToolButton();
            Result.Width = FGraphic.Height;
            Result.Height = FGraphic.Height;
            FControls.Add(Result);

            return Result;
        }

        public RECT Bound()
        {
            return HC.Bounds(FLeft, FTop, FGraphic.Width, FGraphic.Height);
        }

        public HCToolBarControl ActiveControl()
        {
            if (FActiveIndex < 0)
                return null;
            else
                return FControls[FActiveIndex];
        }

        public void SetBounds()
        {
            int vWidth = FPadding;
            for (int i = 0; i < FControls.Count; i++)
                vWidth += FControls[i].Width + FPadding;

            if (FGraphic.Width != vWidth)
            {
                FGraphic = new Bitmap(vWidth, FGraphic.Height);
                FGraphicCanvas.Graphics = Graphics.FromImage(FGraphic);
                UpdateView();
            }
        }

        public void UpdateView()
        {
            UpdateView(HC.Bounds(0, 0, FGraphic.Width, FGraphic.Height));
        }

        public void UpdateView(RECT aRect)
        {
            FGraphicCanvas.Brush.Color = HC.clBtnFace;
            FGraphicCanvas.FillRect(HC.Bounds(0, 0, FGraphic.Width, FGraphic.Height));
            FGraphicCanvas.Pen.Color = Color.FromArgb(240, 240, 240);
            FGraphicCanvas.MoveTo(0, 0);
            FGraphicCanvas.LineTo(0, FGraphic.Height - 2);
            FGraphicCanvas.LineTo(FGraphic.Width - 2, FGraphic.Height - 2);
            FGraphicCanvas.LineTo(FGraphic.Width - 2, 0);
            FGraphicCanvas.LineTo(0, 0);

            FGraphicCanvas.Pen.Color = Color.FromArgb(0x66, 0x66, 0x66);
            FGraphicCanvas.MoveTo(1, FGraphic.Height - 1);
            FGraphicCanvas.LineTo(FGraphic.Width - 1, FGraphic.Height - 1);
            FGraphicCanvas.LineTo(FGraphic.Width - 1, 1);

            int vLeft = FPadding;
            for (int i = 0; i < FControls.Count; i++)
            {
                if (FControls[i] is HCCustomToolButton)
                {
                    if (i == FActiveIndex)
                    {
                        FGraphicCanvas.Brush.Color = Color.FromArgb(51, 153, 255);
                        FGraphicCanvas.FillRect(HC.Bounds(vLeft, 1, FControls[i].Width, FGraphic.Height - 3));
                    }
                    else
                    if (i == FHotIndex)
                    {
                        FGraphicCanvas.Brush.Color = Color.FromArgb(0, 102, 204);
                        FGraphicCanvas.FillRect(HC.Bounds(vLeft, 1, FControls[i].Width, FGraphic.Height - 3));
                    }

                    if (FOnControlPaint != null)
                        FOnControlPaint(FControls[i], vLeft, 0, FGraphicCanvas);
                    else
                        FControls[i].PaintTo(vLeft, 0, FGraphicCanvas);
                }

                vLeft = vLeft + FControls[i].Width + FPadding;
            }

            if (FOnUpdateView != null)
                FOnUpdateView(aRect, FGraphicCanvas);
        }

        public void PaintTo(HCCanvas aCanvas, int aLeft, int aTop)
        {
            FLeft = aLeft;
            FTop = aTop;
            GDI.BitBlt(aCanvas.Handle, FLeft, FTop, FGraphic.Width, FGraphic.Height,
                FGraphicCanvas.Handle, 0, 0, GDI.SRCCOPY);
        }

        public HCToolControls Controls
        {
            get { return FControls; }
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

        public int HotIndex
        {
            get { return FHotIndex; }
        }

        public int ActiveIndex
        {
            get { return FActiveIndex; }
            set { SetActiveIndex(value); }
        }

        public int Width
        {
            get { return GetWidth(); }
        }

        public int Height
        {
            get { return GetHeight(); }
        }

        public bool Visible
        {
            get { return FVisible; }
            set { SetVisible(value); }
        }

        public UpdateViewEventHandler OnUpdateView
        {
            get { return FOnUpdateView; }
            set { FOnUpdateView = value; }
        }

        public ToolBarControlPaintEventHandle OnControlPaint
        {
            get { return FOnControlPaint; }
            set { FOnControlPaint = value; }
        }

        public ToolBarControlClickEventHandle OnControlClick
        {
            get { return FOnControlClick; }
            set { FOnControlClick = value; }
        }
    }

    public class HCTableToolBar : HCToolBar
    {
        protected override void SetVisible(bool value)
        {
            if (Visible != value)
            {
                base.SetVisible(value);
                if (value)
                    ActiveIndex = -1;
            }
        }

        public HCTableToolBar() : base()
        {
            HCToolButton vButton = this.AddButton();
            vButton.Tag = 9;
        }
    }

    public class HCImageToolBar : HCToolBar
    {
        protected override void SetVisible(bool value)
        {
            if (Visible != value)
            {
                base.SetVisible(value);
                if (value)
                    ActiveIndex = 0;
            }
        }

        public HCImageToolBar() : base()
        {
            // 鼠标箭头
            HCToolButton vButton = this.AddButton();
            vButton.Tag = 0;
            // 直线
            vButton = this.AddButton();
            vButton.Tag = 1;
            // 矩形
            vButton = this.AddButton();
            vButton.Tag = 2;
            // 椭圆
            vButton = this.AddButton();
            vButton.Tag = 3;
            // 多边形
            vButton = this.AddButton();
            vButton.Tag = 4;
        }
    }
}
