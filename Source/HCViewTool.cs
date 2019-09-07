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
        private HCToolBar FHotToolBar;
        private HCTableToolBar FTableToolBar;
        private HCImageToolBar FImageToolBar;
        private POINT FMouseViewPt;

        private void DoImageShapeStructOver(object sender, EventArgs e)
        {
            // 构建完成后，图片工具栏恢复到指针按钮
            (FActiveItem as HCImageItem).ShapeManager.OperStyle = HCShapeStyle.hssNone;
            FImageToolBar.ActiveIndex = 0;
        }

        private void SetActiveItem(HCCustomItem value)
        {
            // MouseDown里会触发重绘，此时ToolBar并未确定显示，处理ToolBar的Visible属性
            // 会重新触发重绘，重绘是通过DoImageToolBarUpdateView(Rect)，需要先计算区域参数
            // 然后触发UpdateView，所以需要提前计算ToolBar的坐标vPt位置
            if (FActiveItem != value)
            {
                if (FActiveItem is HCTableItem)
                    FTableToolBar.Visible = false;
                else
                if (FActiveItem is HCImageItem)
                    FImageToolBar.Visible = false;

                if ((value != null) && (value.Active))
                {
                    POINT vPt;

                    if (value is HCTableItem)
                    {
                        FActiveItem = value;
                        vPt = this.GetActiveDrawItemViewCoord();
                        FTableToolBar.Left = vPt.X;
                        FTableToolBar.Top = vPt.Y - FTableToolBar.Height + FToolOffset;
                        // FTableToolBar.Visible = true; 暂时没有不显示
                    }
                    else
                    if (value is HCImageItem)
                    {
                        FActiveItem = value;
                        (FActiveItem as HCImageItem).ShapeManager.OnStructOver = DoImageShapeStructOver;
                        vPt = this.GetActiveDrawItemViewCoord();
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
                if (FActiveItem is HCTableItem)
                    FTableToolBar.Visible = false;
                else
                if (FActiveItem is HCImageItem)
                    FImageToolBar.Visible = false;

                FActiveItem = null;
            }
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
                RECT vRect = aRect;
                vRect.Offset(FTableToolBar.Left, FTableToolBar.Top);
                UpdateView(vRect);
            }
        }

        private void DoImageToolBarUpdateView(RECT aRect, HCCanvas aCanvas)
        {
            if (this.IsHandleCreated && (FImageToolBar != null))
            {
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
            else
            //if FTableToolBar.ActiveIndex >= 0 then  // 鼠标不在表格编辑工具条上，但是点击了某个编辑按钮
            {
                HCTableItem vTableItem = FActiveItem as HCTableItem;
                if (HC.PtInRect(HC.Bounds(FActiveItemRect.Left, FActiveItemRect.Top, vTableItem.Width, vTableItem.Height),
                        new POINT(FMouseViewPt.X, FMouseViewPt.Y)))
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
                }
            }
            else
            if (FHotToolBar == FTableToolBar)
            {
                FTableToolBar.MouseLeave();
                FHotToolBar = null;
            }
            else
            //if FTableToolBar.ActiveIndex > 0 then  // 第一个是指针
            {
                HCTableItem vTableItem = FActiveItem as HCTableItem;
                if (HC.PtInRect(HC.Bounds(FActiveItemRect.Left, FActiveItemRect.Top, vTableItem.Width, vTableItem.Height),
                        new POINT(FMouseViewPt.X, FMouseViewPt.Y)))
                    return true;
            }

            return false;
        }

        private bool TableMouseUp(MouseEventArgs e)
        {
            if (PtInTableToolBar(e.X, e.Y))
            {
                MouseEventArgs vEvent = new MouseEventArgs(e.Button, e.Clicks, e.X - FTableToolBar.Left, e.Y - FTableToolBar.Top, e.Delta);
                FTableToolBar.MouseUp(vEvent);
                return true;
            }
            else
            //if FTableToolBar.ActiveIndex > 0 then  // 第一个是指针
            {
                HCTableItem vTableItem = FActiveItem as HCTableItem;
                if (HC.PtInRect(HC.Bounds(FActiveItemRect.Left, FActiveItemRect.Top, vTableItem.Width, vTableItem.Height),
                        new POINT(FMouseViewPt.X, FMouseViewPt.Y)))
                    return true;
            }

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
            if (!this.ReadOnly)
            {
                if (FTableToolBar.Visible && TableMouseDown(e))
                    return;

                if (FImageToolBar.Visible && ImageMouseDown(e))
                    return;
            }

            base.OnMouseDown(e);

            FTopData = this.ActiveSectionTopLevelData();
            HCCustomItem vTopItem = this.GetTopLevelItem();
            SetActiveItem(vTopItem);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!this.ReadOnly)
            {
                FMouseViewPt.X = ZoomOut(e.X);
                FMouseViewPt.Y = ZoomOut(e.Y);

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

            base.OnKeyDown(e);
        }

        private void DoContextPopup(object sender, CancelEventArgs e)
        {
            if (FTableToolBar.Visible || FImageToolBar.Visible)
                e.Cancel = true;
        }

        protected override void OnContextMenuStripChanged(EventArgs e)
        {
            base.OnContextMenuStripChanged(e);
            this.ContextMenuStrip.Opening += DoContextPopup;
        }

        protected override void DoCaretChange()
        {
            base.DoCaretChange();

            FTableToolBar.Visible = false;
            FImageToolBar.Visible = false;
            if (FActiveItem is HCTableItem)
            {
                //(FActiveItem as THCTableItem).ShapeManager.DisActive
            }
            else
            if (FActiveItem is HCImageItem)
                (FActiveItem as HCImageItem).ShapeManager.DisActive();
        }

        protected override void DoSectionRemoveItem(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            base.DoSectionRemoveItem(sender, aData, aItem);
            if (aItem == FActiveItem)
                FActiveItem = null;
        }

        protected override void DoSectionDrawItemPaintAfter(object sender, HCCustomData aData, int aItemNo, int aDrawItemNo, 
            RECT aDrawRect, int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, 
            int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoSectionDrawItemPaintAfter(sender, aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight, 
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

            if ((FActiveItem != null) && (!this.ReadOnly))
            {
                if (FTableToolBar.Visible)
                {
                    if (aPaintInfo.ScaleX != 1)
                    {
                        SIZE vPt = new SIZE();

                        GDI.SetViewportExtEx(aCanvas.Handle, aPaintInfo.WindowWidth, aPaintInfo.WindowHeight, ref vPt);
                        try
                        {
                            FTableToolBar.PaintTo(aCanvas, aPaintInfo.GetScaleX(FActiveItemRect.Left),
                                aPaintInfo.GetScaleY(FActiveItemRect.Top) + FToolOffset - FTableToolBar.Height);
                        }
                        finally
                        { 
                            GDI.SetViewportExtEx(aCanvas.Handle, aPaintInfo.GetScaleX(aPaintInfo.WindowWidth),
                                aPaintInfo.GetScaleY(aPaintInfo.WindowHeight), ref vPt);
                        }
                    }
                    else
                        FTableToolBar.PaintTo(aCanvas, FActiveItemRect.Left, FActiveItemRect.Top + FToolOffset - FTableToolBar.Height);
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

            FTableToolBar = new HCTableToolBar();
            FTableToolBar.OnUpdateView = DoTableToolBarUpdateView;

            FImageToolBar = new HCImageToolBar();
            FImageToolBar.OnUpdateView = DoImageToolBarUpdateView;
            FImageToolBar.OnControlPaint = DoImageToolBarControlPaint;
        }

    }
}
