using HC.View.Properties;
using HC.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HC.View
{
    public class HCViewTool : HCView
    {
        private HCCustomData FTopData;
        private HCCustomItem FActiveItem;
        private RECT FActiveItemRect;
        private int FToolOffset;
        private HCToolBar FHotToolBar, FCaptureToolBar;
        private ContextMenuStrip FTableToolMenu;
        private HCTableToolBar FTableToolBar;
        private HCImageToolBar FImageToolBar;
        private POINT FMouseViewPt;
        private bool FUseTableTool, FUseImageTool;
        private HCToolState FState;
        private EventHandler FOnTableToolPropertyClick;

        private void DoTableToolPropertyClick(object sender, EventArgs e)
        {
            if (FOnTableToolPropertyClick != null)
                FOnTableToolPropertyClick(sender, e);
        }

        private void DoImageShapeStructOver(object sender, EventArgs e)
        {
            // 构建完成后，图片工具栏恢复到指针按钮
            (FActiveItem as HCImageItem).ShapeManager.OperStyle = HCShapeStyle.hssNone;
            FImageToolBar.ActiveIndex = 0;
        }

        private void SetUseTableTool(bool value)
        {
            if (FUseTableTool != value)
            {
                FUseTableTool = value;
                if (!value)
                    FTableToolBar.Visible = false;
            }
        }

        private void SetUseImageTool(bool value)
        {
            if (FUseImageTool != value)
            {
                FUseImageTool = value;
                if (!value)
                    FImageToolBar.Visible = false;
            }
        }

        private void SetActiveToolItem(HCCustomItem aItem)
        {
            // MouseDown里会触发重绘，此时ToolBar并未确定显示，处理ToolBar的Visible属性
            // 会重新触发重绘，重绘是通过DoImageToolBarUpdateView(Rect)，需要先计算区域参数
            // 然后触发UpdateView，所以需要提前计算ToolBar的坐标vPt位置
            if (FActiveItem != aItem)
            {
                if (FUseTableTool && (FActiveItem is HCTableItem))
                    FTableToolBar.Visible = false;
                else
                if (FUseImageTool && (FActiveItem is HCImageItem))
                    FImageToolBar.Visible = false;

                if ((aItem != null) && (aItem.Active))
                {
                    POINT vPt = this.GetTopLevelRectDrawItemViewCoord();

                    if (FUseTableTool && (aItem is HCTableItem) && (FTableToolBar.Controls.Count > 0))
                    {
                        FActiveItem = aItem;
                        FTableToolBar.Left = vPt.X - FTableToolBar.Width + FToolOffset;
                        FTableToolBar.Top = vPt.Y;// - FTableToolBar.Height + FToolOffset;
                        FTableToolBar.Visible = true;
                    }
                    else
                    if (FUseImageTool && (aItem is HCImageItem) && (FImageToolBar.Controls.Count > 0))
                    {
                        FActiveItem = aItem;
                        (FActiveItem as HCImageItem).ShapeManager.OnStructOver = DoImageShapeStructOver;
                        FImageToolBar.Left = vPt.X;
                        FImageToolBar.Top = vPt.Y - FImageToolBar.Height + FToolOffset;
                        FImageToolBar.Visible = true;
                    }
                    else
                        FActiveItem = null;
                }
                else
                    FActiveItem = null;
            }
            else
            if ((FActiveItem != null) && (!FActiveItem.Active))
            {
                if (FUseTableTool && (FActiveItem is HCTableItem))
                    FTableToolBar.Visible = false;
                else
                if (FUseImageTool && (FActiveItem is HCImageItem))
                    FImageToolBar.Visible = false;

                FActiveItem = null;
            }
        }

        private void CancelActiveToolItem()
        {
            if (FActiveItem is HCImageItem)
            {
                (FActiveItem as HCImageItem).ShapeManager.DisActive();
                FImageToolBar.Visible = false;
            }
            else
            if (FActiveItem is HCTableItem)  // 是表格
                FTableToolBar.Visible = false;

            FActiveItem = null;
        }

        private bool PtInTableToolBar(int x, int y)
        {
            POINT vPt = new POINT(x, y);
            return HC.PtInRect(FTableToolBar.Bound(), vPt);
        }

        private bool PtInImageToolBar(int x, int y)
        {
            POINT vPt = new POINT(x, y);
            return HC.PtInRect(FImageToolBar.Bound(), vPt);
        }

        private void DoTableToolBarUpdateView(RECT aRect, HCCanvas aCanvas)
        {
            if (this.IsHandleCreated && (FTableToolBar != null))
            {
                if (FState == HCToolState.hcsRemoveItem)
                    return;

                RECT vRect = aRect;
                vRect.Offset(FTableToolBar.Left, FTableToolBar.Top);
                UpdateView(vRect);
            }
        }

        private void DoTableToolBarControlPaint(HCToolBarControl control, int left, int top, HCCanvas canvas)
        {
            string resName = "tool" + control.Tag.ToString();
            Icon icon = (Icon)Resources.ResourceManager.GetObject(resName);
            if (icon != null)
            {
                Bitmap vBmp = ((System.Drawing.Icon)icon).ToBitmap();
                canvas.Draw(left + 4, top + 4, vBmp);
            }
        }

        private void DoImageToolBarUpdateView(RECT aRect, HCCanvas aCanvas)
        {
            if (this.IsHandleCreated && (FImageToolBar != null))
            {
                if (FState == HCToolState.hcsRemoveItem)
                    return;

                RECT vRect = aRect;
                vRect.Offset(FImageToolBar.Left, FImageToolBar.Top);
                UpdateView(vRect);
            }
        }

        private void DoImageToolBarControlPaint(HCToolBarControl control, int left, int top, HCCanvas canvas)
        {
            string resName = "tool" + control.Tag.ToString();
            Icon icon = (Icon)Resources.ResourceManager.GetObject(resName);
            Bitmap vBmp = ((System.Drawing.Icon)icon).ToBitmap();
            canvas.Draw(left + 4, top + 4, vBmp);
        }

        private bool TableMouseDown(MouseEventArgs e)
        {
            if (PtInTableToolBar(e.X, e.Y))
            {
                MouseEventArgs vEvent = new MouseEventArgs(e.Button, e.Clicks, e.X - FTableToolBar.Left, e.Y - FTableToolBar.Top, e.Delta);
                FTableToolBar.MouseDown(vEvent);
                return true;
            }

            return false;
        }

        private bool TableMouseMove(MouseEventArgs e)
        {
            if (PtInTableToolBar(e.X, e.Y))
            {
                if (FHotToolBar != FTableToolBar)
                {
                    if (FHotToolBar != null)
                        FHotToolBar.MouseLeave();

                    FHotToolBar = FTableToolBar;
                    FHotToolBar.MouseEnter();
                    this.Cursor = Cursors.Default;
                }
            }
            else
            if (FHotToolBar == FTableToolBar)
            {
                FTableToolBar.MouseLeave();
                FHotToolBar = null;
            }

            return false;
        }

        private bool TableMouseUp(MouseEventArgs e)
        {
            if (PtInTableToolBar(e.X, e.Y))
            {
                if (FTableToolBar.ActiveIndex == 0)
                {
                    FTableToolBar.ActiveIndex = -1;
                    Point vPt = new Point(FTableToolBar.Left, FTableToolBar.Top + FTableToolBar.Height);
                    vPt = this.PointToScreen(vPt);
                    FTableToolMenu.Show(vPt.X, vPt.Y);
                }
                else
                {
                    MouseEventArgs vEvent = new MouseEventArgs(e.Button, e.Clicks, e.X - FTableToolBar.Left, e.Y - FTableToolBar.Top, e.Delta);
                    FTableToolBar.MouseUp(vEvent);
                    FTableToolBar.ActiveIndex = -1;
                }
                return true;
            }
            else
                FTableToolBar.ActiveIndex = -1;

            return false;
        }

        private bool TableKeyDown(KeyEventArgs e)
        {
            return false;
        }

        //
        private bool ImageMouseDown(MouseEventArgs e)
        {
            if (PtInImageToolBar(e.X, e.Y))
            {
                MouseEventArgs vEvent = new MouseEventArgs(e.Button, e.Clicks, e.X - FImageToolBar.Left, e.Y - FImageToolBar.Top, e.Delta);
                FImageToolBar.MouseDown(vEvent);
                if (FImageToolBar.ActiveIndex >= 0)
                    (FActiveItem as HCImageItem).ShapeManager.OperStyle = (HCShapeStyle)FImageToolBar.ActiveControl().Tag;

                return true;
            }
            else
            {
                HCImageItem vImageItem = FActiveItem as HCImageItem;
                if (HC.PtInRect(HC.Bounds(FActiveItemRect.Left, FActiveItemRect.Top, vImageItem.Width, vImageItem.Height),
                        new POINT(FMouseViewPt.X, FMouseViewPt.Y)))
                {
                    MouseEventArgs vEvent = new MouseEventArgs(e.Button, e.Clicks, FMouseViewPt.X - FActiveItemRect.Left, FMouseViewPt.Y - FActiveItemRect.Top, e.Delta);
                    if (vImageItem.ShapeManager.MouseDown(vEvent))
                    {
                        this.UpdateView(new RECT(FActiveItemRect.Left, FActiveItemRect.Top - FImageToolBar.Height, FActiveItemRect.Right, FActiveItemRect.Bottom));

                        return true;
                    }
                }
            }

            return false;
        }

        private bool ImageMouseMove(MouseEventArgs e)
        {
            if (PtInImageToolBar(e.X, e.Y))
            {
                if (FHotToolBar != FImageToolBar)
                {
                    if (FHotToolBar != null)
                        FHotToolBar.MouseLeave();

                    FHotToolBar = FImageToolBar;
                    FHotToolBar.MouseEnter();
                    this.Cursor = Cursors.Default;
                }
            }
            else
            if (FHotToolBar == FImageToolBar)
            {
                FImageToolBar.MouseLeave();
                FHotToolBar = null;
            }
            else
            //if FImageToolBar.ActiveIndex > 0 then  // 有效的样式、第一个是指针
            {
                HCImageItem vImageItem = FActiveItem as HCImageItem;
                if (HC.PtInRect(HC.Bounds(FActiveItemRect.Left, FActiveItemRect.Top, vImageItem.Width, vImageItem.Height),
                    new POINT(FMouseViewPt.X, FMouseViewPt.Y)))
                {
                    MouseEventArgs vEvent = new MouseEventArgs(e.Button, e.Clicks, FMouseViewPt.X - FActiveItemRect.Left, FMouseViewPt.Y - FActiveItemRect.Top, e.Delta);
                    if (vImageItem.ShapeManager.MouseMove(vEvent))
                    {
                        this.UpdateView(new RECT(FActiveItemRect.Left, FActiveItemRect.Top - FImageToolBar.Height, FActiveItemRect.Right, FActiveItemRect.Bottom));

                        if (vImageItem.ShapeManager.HotIndex >= 0)
                            Cursor = vImageItem.ShapeManager[vImageItem.ShapeManager.HotIndex].Cursor;
                        
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ImageMouseUp(MouseEventArgs e)
        {
            if (PtInImageToolBar(e.X, e.Y))
            {
                MouseEventArgs vEvent = new MouseEventArgs(e.Button, e.Clicks, e.X - FImageToolBar.Left, e.Y - FImageToolBar.Top, e.Delta);
                FImageToolBar.MouseUp(vEvent);
                return true;
            }
            else
            //if FImageToolBar.ActiveIndex > 0 then  // 第一个是指针
            {
                HCImageItem vImageItem = FActiveItem as HCImageItem;
                if (HC.PtInRect(HC.Bounds(FActiveItemRect.Left, FActiveItemRect.Top, vImageItem.Width, vImageItem.Height),
                    new POINT(FMouseViewPt.X, FMouseViewPt.Y)))
                {
                    MouseEventArgs vEvent = new MouseEventArgs(e.Button, e.Clicks, FMouseViewPt.X - FActiveItemRect.Left, FMouseViewPt.Y - FActiveItemRect.Top, e.Delta);
                    if (vImageItem.ShapeManager.MouseUp(vEvent))
                        return true;
                }
                else
                    DoImageShapeStructOver(null, e);
            }

            return false;
        }

        private bool ImageKeyDown(KeyEventArgs e)
        {
            HCImageItem vImageItem = FActiveItem as HCImageItem;
            if (vImageItem.ShapeManager.KeyDown(e))
            {
                this.UpdateView(new RECT(FActiveItemRect.Left, FActiveItemRect.Top - FImageToolBar.Height, FActiveItemRect.Right, FActiveItemRect.Bottom));
                return true;
            }

            return false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            FCaptureToolBar = null;

            if (!this.ReadOnly)
            {
                if (FUseTableTool && FTableToolBar.Visible && TableMouseDown(e))
                {
                    FCaptureToolBar = FTableToolBar;
                    return;
                }

                if (FUseImageTool && FImageToolBar.Visible && ImageMouseDown(e))
                {
                    FCaptureToolBar = FImageToolBar;
                    return;
                }
            }

            base.OnMouseDown(e);

            FTopData = this.ActiveSectionTopLevelData();
            HCCustomItem vTopItem = FTopData.GetActiveItem();
            SetActiveToolItem(vTopItem);

            HCCustomData vData = null;
            while (FActiveItem == null)
            {
                vData = FTopData.GetRootData();
                if (vData == FTopData)
                    break;

                FTopData = vData;
                vTopItem = FTopData.GetActiveItem();
                SetActiveToolItem(vTopItem);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!this.ReadOnly)
            {
                FMouseViewPt.X = ZoomOut(e.X);
                FMouseViewPt.Y = ZoomOut(e.Y);

                if (FCaptureToolBar == FTableToolBar)
                {
                    TableMouseMove(e);
                    return;
                }
                else
                if (FCaptureToolBar == FImageToolBar)
                {
                    ImageMouseMove(e);
                    return;
                }

                if (FTableToolBar.Visible && TableMouseMove(e))
                    return;

                if (FImageToolBar.Visible && ImageMouseMove(e))
                    return;
            }

            if (FHotToolBar != null)
            {
                MouseEventArgs vEvent = new MouseEventArgs(e.Button, e.Clicks, e.X - FHotToolBar.Left, e.Y - FHotToolBar.Top, e.Delta);
                FHotToolBar.MouseMove(vEvent);
            }
            else
                base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!this.ReadOnly)
            {
                if (FCaptureToolBar == FTableToolBar)
                {
                    FCaptureToolBar = null;
                    TableMouseUp(e);
                    return;
                }
                else
                if (FCaptureToolBar == FImageToolBar)
                {
                    FCaptureToolBar = null;
                    ImageMouseUp(e);
                    return;
                }

                if (FTableToolBar.Visible && TableMouseUp(e))
                    return;

                if (FImageToolBar.Visible && ImageMouseUp(e))
                    return;
            }

            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!this.ReadOnly)
            {
                if (FTableToolBar.Visible && TableKeyDown(e))
                    return;

                if (FImageToolBar.Visible && ImageKeyDown(e))
                    return;
            }

            this.BeginUpdate();
            try
            {
                base.OnKeyDown(e);  // 删除Item时会触发ToolBar隐藏，重绘时环境还没有准备好
            }
            finally
            {
                this.EndUpdate();
            }
        }

        private void DoContextPopup(object sender, CancelEventArgs e)
        {
            if (FImageToolBar.Visible && (FImageToolBar.ActiveIndex > 0))
                e.Cancel = true;
            
            if (FTableToolBar.Visible && (FTableToolBar.ActiveIndex >= 0))
                e.Cancel = true;
        }

        protected override void OnContextMenuStripChanged(EventArgs e)
        {
            base.OnContextMenuStripChanged(e);
            this.ContextMenuStrip.Opening += DoContextPopup;
        }

        protected override void DoKillFocus()
        {
            base.DoKillFocus();
            if ((FTableToolBar != null) && FTableToolBar.Visible)
                FTableToolBar.UpdateView();
            else
            if ((FImageToolBar != null) && FImageToolBar.Visible)
                FImageToolBar.UpdateView();
        }

        protected override void DoCaretChange()
        {
            base.DoCaretChange();

            if (!(FActiveItem is HCTableItem))  // 不是表格
                FTableToolBar.Visible = false;

            if (FActiveItem is HCImageItem)
                (FActiveItem as HCImageItem).ShapeManager.DisActive();
            else
                FImageToolBar.Visible = false;
        }

        protected override void DoSectionRemoveItem(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            base.DoSectionRemoveItem(sender, aData, aItem);
            if (aItem == FActiveItem)
            {
                FState = HCToolState.hcsRemoveItem;
                try
                {
                    CancelActiveToolItem();
                }
                finally
                {
                    FState = HCToolState.hcsNone;
                }
            }
        }

        protected override void DoSectionDrawItemPaintAfter(object sender, HCCustomData aData, int aItemNo, int aDrawItemNo, 
            RECT aDrawRect, RECT aClearRect, int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, 
            int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoSectionDrawItemPaintAfter(sender, aData, aItemNo, aDrawItemNo, aDrawRect, aClearRect, aDataDrawLeft, aDataDrawRight, 
                aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);

            if ((aData == FTopData) && (aData.Items[aItemNo] == FActiveItem))
                FActiveItemRect = aDrawRect;
        }

        protected override void DoPaintViewBefor(HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaintViewBefor(aCanvas, aPaintInfo);
            FActiveItemRect.Top = -1000;
        }

        protected override void DoPaintViewAfter(HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaintViewAfter(aCanvas, aPaintInfo);

            if ((FActiveItem != null) && (!this.ReadOnly) && this.Focused)
            {
                if (FTableToolBar.Visible)
                {
                    if (aPaintInfo.ScaleX != 1)
                    {
                        SIZE vPt = new SIZE();

                        GDI.SetViewportExtEx(aCanvas.Handle, aPaintInfo.WindowWidth, aPaintInfo.WindowHeight, ref vPt);
                        try
                        {
                            FTableToolBar.PaintTo(aCanvas, aPaintInfo.GetScaleX(FActiveItemRect.Left - FTableToolBar.Width + FToolOffset),
                                aPaintInfo.GetScaleY(FActiveItemRect.Top));// + FToolOffset - FTableToolBar.Height);
                        }
                        finally
                        {
                            GDI.SetViewportExtEx(aCanvas.Handle, aPaintInfo.GetScaleX(aPaintInfo.WindowWidth),
                                aPaintInfo.GetScaleY(aPaintInfo.WindowHeight), ref vPt);
                        }
                    }
                    else
                    {
                        FTableToolBar.PaintTo(aCanvas, FActiveItemRect.Left - FTableToolBar.Width + FToolOffset,
                            FActiveItemRect.Top);// - Style.LineSpaceMin / 2 + FToolOffset - FTableToolBar.Height);
                    }
                }
                else
                if (FImageToolBar.Visible)
                {
                    if (aPaintInfo.ScaleX != 1)
                    {
                        SIZE vPt = new SIZE();

                        GDI.SetViewportExtEx(aCanvas.Handle, aPaintInfo.WindowWidth, aPaintInfo.WindowHeight, ref vPt);
                        try
                        {
                            FImageToolBar.PaintTo(aCanvas, aPaintInfo.GetScaleX(FActiveItemRect.Left),
                                aPaintInfo.GetScaleY(FActiveItemRect.Top) + FToolOffset - FImageToolBar.Height);
                        }
                        finally
                        { 
                            GDI.SetViewportExtEx(aCanvas.Handle, aPaintInfo.GetScaleX(aPaintInfo.WindowWidth),
                                aPaintInfo.GetScaleY(aPaintInfo.WindowHeight), ref vPt);
                        }
                    }
                    else
                        FImageToolBar.PaintTo(aCanvas, FActiveItemRect.Left, FActiveItemRect.Top + FToolOffset - FImageToolBar.Height);
                }
            }
        }

        public HCViewTool() : base()
        {
            FToolOffset = -4;
            FActiveItem = null;
            FHotToolBar = null;
            FUseTableTool = true;
            FUseImageTool = true;

            FTableToolMenu = new ContextMenuStrip();
            ToolStripMenuItem vMenuItem = new ToolStripMenuItem("重设行列");
            FTableToolMenu.Items.Add(vMenuItem);
            vMenuItem.DropDownItems.Add("2 x 2").Click += delegate (object sender, EventArgs e)
            {
                this.ActiveTableResetRowCol(2, 2);
            };


            vMenuItem = new ToolStripMenuItem("表格属性");
            vMenuItem.Click += DoTableToolPropertyClick;
            FTableToolMenu.Items.Add(vMenuItem);

            FTableToolBar = new HCTableToolBar();
            FTableToolBar.OnUpdateView = DoTableToolBarUpdateView;
            FTableToolBar.OnControlPaint = DoTableToolBarControlPaint;

            FImageToolBar = new HCImageToolBar();
            FImageToolBar.OnUpdateView = DoImageToolBarUpdateView;
            FImageToolBar.OnControlPaint = DoImageToolBarControlPaint;
        }

        public bool UseTableTool
        {
            get { return FUseTableTool; }
            set { SetUseTableTool(value); }
        }

        public bool UseImageTool
        {
            get { return FUseImageTool; }
            set { SetUseImageTool(value); }
        }

        public EventHandler OnTableToolPropertyClick
        {
            get { return FOnTableToolPropertyClick; }
            set { FOnTableToolPropertyClick = value; }
        }
    }

    public enum HCToolState : byte
    {
        hcsNone = 0,
        hcsRemoveItem = 1
    }
}
