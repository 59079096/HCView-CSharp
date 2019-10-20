using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HC.Win32;
using System.Drawing;
using System.IO;

namespace HC.View
{
    public class HCEdit : UserControl
    {
        private HCStyle FStyle;
        private HCViewData FData;
        private IntPtr FDC = IntPtr.Zero;
        private IntPtr FMemDC = IntPtr.Zero;
        private IntPtr FhImc = IntPtr.Zero;
        private HCUndoList FUndoList;
        private HCCaret FCaret;
        private DataFormats.Format FHCExtFormat;
        private HCScrollBar FHScrollBar;
        private HCScrollBar FVScrollBar;
        private uint FUpdateCount;
        private bool FChanged;
        private EventHandler FOnChange;
        private EventHandler FOnCaretChange = null;
        private StyleItemEventHandler FOnCreateStyleItem = null;
        private DataItemEventHandler FOnInsertItem = null, FOnRemoveItem = null;

        private int GetViewWidth()
        {
            if (FVScrollBar.Visible)
                return Width - FVScrollBar.Width;
            else
                return Width;
        }

        private int GetViewHeight()
        {
            if (FHScrollBar.Visible)
                return Height - FHScrollBar.Height;
            else
                return Height;
        }

        private int GetCurStyleNo()
        {
            return FData.GetTopLevelData().CurParaNo;
        }

        private int GetCurParaNo()
        {
            return FData.GetTopLevelData().CurStyleNo;
        }

        private void ReBuildCaret(bool aScrollBar = false)
        {
            if (FCaret == null)
                return;

            if ((!this.Focused) || (!FStyle.UpdateInfo.Draging && FData.SelectExists()))
            {
                FCaret.Hide();
                return;
            }

            HCCaretInfo vCaretInfo = new HCCaretInfo();
            vCaretInfo.X = 0;
            vCaretInfo.Y = 0;
            vCaretInfo.Height = 0;
            vCaretInfo.Visible = true;
        
            FData.GetCaretInfo(FData.SelectInfo.StartItemNo, FData.SelectInfo.StartItemOffset, ref  vCaretInfo);

            if (!vCaretInfo.Visible)
            {
                FCaret.Hide();
                return;
            }

            FCaret.X = vCaretInfo.X - FHScrollBar.Position + this.Padding.Left;
            FCaret.Y = vCaretInfo.Y - FVScrollBar.Position + this.Padding.Top;
            FCaret.Height = vCaretInfo.Height;

            int vViewHeight = GetViewHeight();
            if (aScrollBar)
            {
                if ((FCaret.X < 0) || (FCaret.X > GetViewWidth()))
                {
                    FCaret.Hide();
                    return;
                }

                if ((FCaret.Y + FCaret.Height < 0) || (FCaret.Y > vViewHeight))
                {
                    FCaret.Hide();
                    return;
                }
            }
            else  // 非滚动条(方向键、点击等)引起的光标位置变化
            {
                if (FCaret.Height < vViewHeight)
                {
                    if (FCaret.Y < 0)
                        FVScrollBar.Position = FVScrollBar.Position + FCaret.Y - this.Padding.Top;
                    else
                        if (FCaret.Y + FCaret.Height + this.Padding.Top > vViewHeight)
                            FVScrollBar.Position = FVScrollBar.Position + FCaret.Y + FCaret.Height + this.Padding.Top - vViewHeight;
                }
            }


            if (FCaret.Y + FCaret.Height > vViewHeight)
                FCaret.Height = vViewHeight - FCaret.Y;

            FCaret.Show();
            DoCaretChange();
        }

        private void CheckUpdateInfo(bool aScrollBar = false)
        {
            if ((FCaret != null) && FStyle.UpdateInfo.ReCaret)
            {
                FStyle.UpdateInfo.ReCaret = false;
                ReBuildCaret(aScrollBar);
                UpdateImmPosition();
            }
            if (FStyle.UpdateInfo.RePaint)
            {
                FStyle.UpdateInfo.RePaint = false;
                UpdateView();
            }
        }

        private void DoVScrollChange(Object Sender, ScrollCode ScrollCode, int ScrollPos)
        {
            FStyle.UpdateInfoRePaint();
            FStyle.UpdateInfoReCaret(false);
            CheckUpdateInfo(true);
        }

        private void DoMapChanged()
        {
            if (FUpdateCount == 0)
            {
                CalcScrollRang();
                CheckUpdateInfo();
            }
        }

        private void DoCaretChange()
        {
            if (FOnCaretChange != null)
                FOnCaretChange(this, null);
        }

        private void DoDataCheckUpdateInfo()
        {
            if (FUpdateCount == 0)
                CheckUpdateInfo();
        }

        private void DoChange()
        {
            FChanged = true;
            DoMapChanged();
            if (FOnChange != null)
                FOnChange(this, null);
        }

        private void CalcScrollRang()
        {
            FHScrollBar.Max = this.Padding.Left + this.Padding.Right;
            FVScrollBar.Max = FData.Height + this.Padding.Top + this.Padding.Bottom;
        }

        private void UpdateImmPosition()
        {
            // 全局 FhImc
            LOGFONT vLogFont = new LOGFONT();
            Imm.ImmGetCompositionFont(FhImc, ref vLogFont);
            vLogFont.lfHeight = 22;
            Imm.ImmSetCompositionFont(FhImc, ref vLogFont);
            // 告诉输入法当前光标位置信息
            COMPOSITIONFORM vCF = new COMPOSITIONFORM();
            vCF.ptCurrentPos = new POINT(FCaret.X, FCaret.Y + 5);  // 输入法弹出窗体位置
            vCF.dwStyle = 1;

            Rectangle vr = this.ClientRectangle;
            vCF.rcArea = new RECT(vr.Left, vr.Top, vr.Right, vr.Bottom);
            Imm.ImmSetCompositionWindow(FhImc, ref vCF);
        }

        private void _DeleteUnUsedStyle()
        {
            for (int i = 0; i <= FStyle.TextStyles.Count - 1; i++)
            {
                FStyle.TextStyles[i].CheckSaveUsed = false;
                FStyle.TextStyles[i].TempNo = HCStyle.Null;
            }
            for (int i = 0; i <= FStyle.ParaStyles.Count - 1; i++)
            {
                FStyle.ParaStyles[i].CheckSaveUsed = false;
                FStyle.ParaStyles[i].TempNo = HCStyle.Null;
            }

            FData.MarkStyleUsed(true);
            
            int vUnCount = 0;
            for (int i = 0; i <= FStyle.TextStyles.Count - 1; i++)
            {
                if (FStyle.TextStyles[i].CheckSaveUsed)
                    FStyle.TextStyles[i].TempNo = i - vUnCount;
                else
                    vUnCount++;
            }

            vUnCount = 0;
            for (int i = 0; i <= FStyle.ParaStyles.Count - 1; i++)
            {
                if (FStyle.ParaStyles[i].CheckSaveUsed)
                    FStyle.ParaStyles[i].TempNo = i - vUnCount;
                else
                    vUnCount++;
            }

            FData.MarkStyleUsed(false);

            for (int i = FStyle.TextStyles.Count - 1; i >= 0; i--)
            {
                if (!FStyle.TextStyles[i].CheckSaveUsed)
                    FStyle.TextStyles.RemoveAt(i);
            }

            for (int i = FStyle.ParaStyles.Count - 1; i >= 0; i--)
            {
                if (!FStyle.ParaStyles[i].CheckSaveUsed)
                    FStyle.ParaStyles.RemoveAt(i);
            }
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();
            if (!DesignMode)
                FCaret = new HCCaret(this.Handle);

            if (FDC == IntPtr.Zero)
            {
                FDC = User.GetDC(this.Handle);
                //FMemDC = (IntPtr)GDI.CreateCompatibleDC(FDC);
            }

            FhImc = Imm.ImmGetContext(this.Handle);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            GDI.DeleteDC(FMemDC);
            User.ReleaseDC(this.Handle, FDC);

            Imm.ImmReleaseContext(this.Handle, FhImc);
            base.OnHandleDestroyed(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            GDI.BitBlt(FDC, 0, 0, GetViewWidth(), GetViewHeight(), FMemDC, 0, 0, GDI.SRCCOPY);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            FData.Width = GetViewWidth() - this.Padding.Left - this.Padding.Right;
            FData.ReFormat();
            FStyle.UpdateInfoRePaint();
            if (FCaret != null)
                FStyle.UpdateInfoReCaret(false);

            DoMapChanged();
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);

            FVScrollBar.Left = Width - FVScrollBar.Width;
            FVScrollBar.Height = Height - FHScrollBar.Height;
            FVScrollBar.PageSize = FVScrollBar.Height;
            //
            FHScrollBar.Top = Height - FHScrollBar.Height;
            FHScrollBar.Width = Width - FVScrollBar.Width;
            FHScrollBar.PageSize = FHScrollBar.Width;

            this.Refresh();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            MouseEventArgs vArgs = new MouseEventArgs(e.Button, e.Clicks, 
                e.X - this.Padding.Left - FHScrollBar.Position,
                e.Y - this.Padding.Top + FVScrollBar.Position, e.Delta);
            FData.MouseDown(vArgs);

            CheckUpdateInfo();
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            HC.GCursor = Cursors.IBeam;

            MouseEventArgs vArgs = new MouseEventArgs(e.Button, e.Clicks,
                e.X - this.Padding.Left - FHScrollBar.Position,
                e.Y - this.Padding.Top + FVScrollBar.Position, e.Delta);
            FData.MouseMove(vArgs);

            //if (ShowHint)
            //    ProcessHint();

            if (FStyle.UpdateInfo.Draging)
                Cursor.Current = HC.GCursor;
            else
                this.Cursor = HC.GCursor;

            CheckUpdateInfo();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Right)
                return;

            MouseEventArgs vArgs = new MouseEventArgs(e.Button, e.Clicks,
                e.X - this.Padding.Left - FHScrollBar.Position,
                e.Y - this.Padding.Top + FVScrollBar.Position, e.Delta);
            FData.MouseUp(vArgs);

            if (FStyle.UpdateInfo.Draging)
                HC.GCursor = Cursors.Default;

            this.Cursor = HC.GCursor;

            CheckUpdateInfo();

            FStyle.UpdateInfo.Selecting = false;
            FStyle.UpdateInfo.Draging = false;

            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)  // 按下ctrl
                FHScrollBar.Position -= e.Delta;
            else
                FVScrollBar.Position -= e.Delta;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if ((e.Control) && (e.KeyCode == Keys.C))
                this.Copy();
            else
            if ((e.Control) && (e.KeyCode == Keys.X))
                this.Cut();
            else
            if ((e.Control) && (e.KeyCode == Keys.V))
                this.Paste();
            else
            if ((e.Control) && (e.KeyCode == Keys.A))
                this.SelectAll();
            else
            if ((e.Control) && (e.KeyCode == Keys.Z))
                this.Undo();
            else
            if ((e.Control) && (e.KeyCode == Keys.Y))
                this.Redo();
            else
            {
                FData.KeyDown(e);

                if (HC.IsKeyDownEdit(e.KeyValue))
                    DoChange();
                else
                if (HC.IsDirectionKey(e.KeyValue))
                  DoDataCheckUpdateInfo();
            }

            CheckUpdateInfo();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            FData.KeyUp(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (HC.IsKeyPressWant(e))
            {
                Char vKey = e.KeyChar;
                FData.KeyPress(ref vKey);
                DoChange();
                CheckUpdateInfo();
            }
        }

        protected virtual HCCustomItem DoDataCreateStyleItem(HCCustomData aData, int aStyleNo)
        {
            if (FOnCreateStyleItem != null)
                return FOnCreateStyleItem(aData, aStyleNo);
            else
                return null;
        }

        protected virtual void DoDataInsertItem(HCCustomData aData, HCCustomItem aItem)
        {
            if (FOnInsertItem != null)
                FOnInsertItem(aData, aItem);
        }

        protected virtual void DoDataRemoveItem(HCCustomData aData, HCCustomItem aItem)
        {
            if (FOnRemoveItem != null)
                FOnRemoveItem(aData, aItem);
        }

        protected override void WndProc(ref Message Message)
        {
            switch (Message.Msg)
            {
                case User.WM_GETDLGCODE:
                    Message.Result = (IntPtr)(User.DLGC_WANTTAB | User.DLGC_WANTARROWS);
                    return;

                case User.WM_ERASEBKGND:
                    Message.Result = (IntPtr)1;
                    return;

                case User.WM_SETFOCUS:
                case User.WM_NCPAINT:
                    base.WndProc(ref Message);
                    FStyle.UpdateInfoReCaret(false);
                    FStyle.UpdateInfoRePaint();
                    CheckUpdateInfo();
                    return;

                case User.WM_KILLFOCUS:
                    base.WndProc(ref Message);
                    if (Message.WParam != Handle)
                        FCaret.Hide();
                    return;

                case User.WM_IME_SETCONTEXT:
                    if (Message.WParam.ToInt32() == 1)
                    {
                        Imm.ImmAssociateContext(this.Handle, FhImc);
                    }
                    break;

                case User.WM_IME_COMPOSITION:
                    if ((Message.LParam.ToInt32() & Imm.GCS_RESULTSTR) != 0)
                    {
                        if (FhImc != IntPtr.Zero)
                        {
                            int vSize = Imm.ImmGetCompositionString(FhImc, Imm.GCS_RESULTSTR, null, 0);
                            if (vSize > 0)
                            {
                                byte[] vBuffer = new byte[vSize];
                                Imm.ImmGetCompositionString(FhImc, Imm.GCS_RESULTSTR, vBuffer, vSize);
                                string vS = System.Text.Encoding.Default.GetString(vBuffer);
                                if (vS != "")
                                {
                                    FData.InsertText(vS);
                                    FStyle.UpdateInfoRePaint();
                                    FStyle.UpdateInfoReCaret();
                                    CheckUpdateInfo();

                                    return;
                                }
                            }

                            Message.Result = IntPtr.Zero;
                        }
                    }
                    break;

                case User.WM_LBUTTONDOWN:
                case User.WM_LBUTTONDBLCLK:
                    if ((!DesignMode) && (!this.Focused))
                    {
                        User.SetFocus(Handle);
                        if (!Focused)
                            return;
                    }
                    break;
            }

            base.WndProc(ref Message);
        }

        protected void Cut()
        {
            Copy();
            FData.DeleteSelected();
            CheckUpdateInfo();
        }

        protected void Copy()
        {
            if (FData.SelectExists())
            {
                MemoryStream vStream = new MemoryStream();
                try
                {
                    HC._SaveFileFormatAndVersion(vStream);  // 保存文件格式和版本
                    _DeleteUnUsedStyle();

                    FStyle.SaveToStream(vStream);
                    FData.GetTopLevelData().SaveSelectToStream(vStream);

                    byte[] vBuffer = new byte[0];
                    IntPtr vMemExt = (IntPtr)Kernel.GlobalAlloc(Kernel.GMEM_MOVEABLE | Kernel.GMEM_DDESHARE, (int)vStream.Length);
                    try
                    {
                        if (vMemExt == IntPtr.Zero)
                            throw new Exception(HC.HCS_EXCEPTION_MEMORYLESS);
                        IntPtr vPtr = (IntPtr)Kernel.GlobalLock(vMemExt);
                        try
                        {
                            vStream.Position = 0;
                            vBuffer = vStream.ToArray();
                            System.Runtime.InteropServices.Marshal.Copy(vBuffer, 0, vPtr, vBuffer.Length);
                        }
                        finally
                        {
                            Kernel.GlobalUnlock(vMemExt);
                        }
                    }
                    catch
                    {
                        Kernel.GlobalFree(vMemExt);
                        return;
                    }

                    vBuffer = System.Text.Encoding.Unicode.GetBytes(FData.GetTopLevelData().SaveSelectToText());
                    IntPtr vMem = (IntPtr)Kernel.GlobalAlloc(Kernel.GMEM_MOVEABLE | Kernel.GMEM_DDESHARE, vBuffer.Length + 1);
                    try
                    {
                        if (vMem == IntPtr.Zero)
                            throw new Exception(HC.HCS_EXCEPTION_MEMORYLESS);

                        IntPtr vPtr = (IntPtr)Kernel.GlobalLock(vMem);
                        try
                        {
                            System.Runtime.InteropServices.Marshal.Copy(vBuffer, 0, vPtr, vBuffer.Length);
                        }
                        finally
                        {
                            Kernel.GlobalUnlock(vMem);
                        }
                    }
                    catch
                    {
                        Kernel.GlobalFree(vMem);
                        return;
                    }

                    User.OpenClipboard(IntPtr.Zero);
                    try
                    {
                        User.EmptyClipboard();
                        User.SetClipboardData(FHCExtFormat.Id, vMemExt);
                        User.SetClipboardData(User.CF_TEXT, vMem);  // 文本格式
                    }
                    finally
                    {
                        User.CloseClipboard();
                    }
                }
                finally
                {
                    vStream.Close();
                    vStream.Dispose();
                }
            }
        }

        protected void Paste()
        {
            IDataObject vIData = Clipboard.GetDataObject();
            if (vIData.GetDataPresent(HC.HC_EXT))
            {
                MemoryStream vStream = (MemoryStream)vIData.GetData(HC.HC_EXT);
                try
                {
                    string vFileFormat = "";
                    ushort vFileVersion = 0;
                    byte vLan = 0;

                    vStream.Position = 0;
                    HC._LoadFileFormatAndVersion(vStream, ref vFileFormat, ref vFileVersion, ref vLan);  // 文件格式和版本
                    HCStyle vStyle = new HCStyle();
                    try
                    {
                        vStyle.LoadFromStream(vStream, vFileVersion);
                        this.BeginUpdate();
                        try
                        {
                            FData.InsertStream(vStream, vStyle, vFileVersion);
                        }
                        finally
                        {
                            this.EndUpdate();
                        }
                    }
                    finally
                    {
                        vStyle.Dispose();
                    }
                }
                finally
                {
                    vStream.Close();
                    vStream.Dispose();
                }
            }
            else
                if (vIData.GetDataPresent(DataFormats.Text))
                    FData.InsertText(Clipboard.GetText());
                else
                    if (vIData.GetDataPresent(DataFormats.Bitmap))
                    {
                        Image vImage = (Image)vIData.GetData(typeof(Bitmap));

                        HCRichData vTopData = FData.GetTopLevelData() as HCRichData;
                        HCImageItem vImageItem = new HCImageItem(vTopData);

                        vImageItem.Image = new Bitmap(vImage);

                        vImageItem.Width = vImageItem.Image.Width;
                        vImageItem.Height = vImageItem.Image.Height;


                        vImageItem.RestrainSize(vTopData.Width, vImageItem.Height);

                        FData.InsertItem(vImageItem);
                    }
        }

        protected bool DataChangeByAction(HCFunction aFun)
        {
            bool Result = aFun();
            DoChange();
            return Result;
        }

        protected HCUndoList DoGetUndoList()
        {
            return FUndoList;
        }

        protected HCUndo DoUndoNew()
        {
            HCUndo Result = new HCEditUndo();
            (Result as HCEditUndo).HScrollPos = FHScrollBar.Position;
            (Result as HCEditUndo).VScrollPos = FVScrollBar.Position;
            Result.Data = FData;

            return Result;
        }

        protected HCUndoGroupBegin DoUndoGroupBegin(int aItemNo, int aOffset)
        {
            HCUndoEditGroupBegin Result = new HCUndoEditGroupBegin();
            (Result as HCUndoEditGroupBegin).HScrollPos = FHScrollBar.Position;
            (Result as HCUndoEditGroupBegin).VScrollPos = FVScrollBar.Position;
            Result.Data = FData;
            Result.CaretDrawItemNo = FData.CaretDrawItemNo;

            return Result;
        }

        protected HCUndoGroupEnd DoUndoGroupEnd(int aItemNo, int aOffset)
        {
            HCUndoEditGroupEnd Result = new HCUndoEditGroupEnd();
            (Result as HCUndoEditGroupEnd).HScrollPos = FHScrollBar.Position;
            (Result as HCUndoEditGroupEnd).VScrollPos = FVScrollBar.Position;
            Result.Data = FData;
            Result.CaretDrawItemNo = FData.CaretDrawItemNo;

            return Result;
        }

        protected void DoUndo(HCUndo sender)
        {
            if (sender is HCEditUndo)
            {
                FHScrollBar.Position = (sender as HCEditUndo).HScrollPos;
                FVScrollBar.Position = (sender as HCEditUndo).VScrollPos;
            }
            else
            if (sender is HCUndoEditGroupBegin)
            {
                FHScrollBar.Position = (sender as HCUndoEditGroupBegin).HScrollPos;
                FVScrollBar.Position = (sender as HCUndoEditGroupBegin).VScrollPos;
            }

            HCUndoList vUndoList = DoGetUndoList();
            if (!vUndoList.GroupWorking)
            {
                HCFunction vEvent = delegate()
                {
                    FData.Undo(sender);
                    return true;
                };

                DataChangeByAction(vEvent);
            }
            else
                FData.Undo(sender);
        }

        protected void DoRedo(HCUndo sender)
        {
            if (sender is HCEditUndo)
            {
                FHScrollBar.Position = (sender as HCEditUndo).HScrollPos;
                FVScrollBar.Position = (sender as HCEditUndo).VScrollPos;
            }
            else
                if (sender is HCUndoEditGroupBegin)
                {
                    FHScrollBar.Position = (sender as HCUndoEditGroupBegin).HScrollPos;
                    FVScrollBar.Position = (sender as HCUndoEditGroupBegin).VScrollPos;
                }

            HCUndoList vUndoList = DoGetUndoList();
            if (!vUndoList.GroupWorking)
            {
                HCFunction vEvent = delegate()
                {
                    FData.Redo(sender);
                    return true;
                };

                DataChangeByAction(vEvent);
            }
            else
                FData.Redo(sender);
        }

        public HCEdit()
        {
            HCUnitConversion.Initialization();
            //this.DoubleBuffered = true;
            //Create();  // 便于子类在构造函数前执行
            FHCExtFormat = DataFormats.GetFormat(HC.HC_EXT);
            SetStyle(ControlStyles.Selectable, true);  // 可接收焦点

            FStyle = new HCStyle(true, true);

            FUndoList = new HCUndoList();
            FUndoList.OnUndo = DoUndo;
            FUndoList.OnRedo = DoRedo;
            FUndoList.OnUndoNew = DoUndoNew;
            FUndoList.OnUndoGroupStart = DoUndoGroupBegin;
            FUndoList.OnUndoGroupEnd = DoUndoGroupEnd;

            FData = new HCViewData(FStyle);
            FData.Width = 200;
            FData.OnGetUndoList = DoGetUndoList;
            FData.OnCreateItemByStyle = DoDataCreateStyleItem;
            FData.OnInsertItem = DoDataInsertItem;
            FData.OnRemoveItem = DoDataRemoveItem;

            // 垂直滚动条，范围在Resize中设置
            FVScrollBar = new HCScrollBar();
            FVScrollBar.Orientation = Orientation.oriVertical;
            FVScrollBar.OnScroll = DoVScrollChange;
            // 水平滚动条，范围在Resize中设置
            FHScrollBar = new HCScrollBar();
            FHScrollBar.Orientation = Orientation.oriHorizontal;
            FHScrollBar.OnScroll = DoVScrollChange;

            this.Controls.Add(FHScrollBar);
            this.Controls.Add(FVScrollBar);

            FChanged = false;
            this.ResumeLayout();
        }

        /// <summary> 修改当前光标所在段水平对齐方式 </summary>
        public void ApplyParaAlignHorz(ParaAlignHorz aAlign)
        {
            FData.ApplyParaAlignHorz(aAlign);
            CheckUpdateInfo();
        }

        /// <summary> 修改当前光标所在段垂直对齐方式 </summary>
        public void ApplyParaAlignVert(ParaAlignVert aAlign)
        {
            FData.ApplyParaAlignVert(aAlign);
            CheckUpdateInfo();
        }

        /// <summary> 修改当前光标所在段背景色 </summary>
        public void ApplyParaBackColor(Color aColor)
        {
            FData.ApplyParaBackColor(aColor);
            CheckUpdateInfo();
        }

        /// <summary> 修改当前光标所在段行间距 </summary>
        public void ApplyParaLineSpace(ParaLineSpaceMode aSpaceMode, Single aSpace = 1)
        {
            FData.ApplyParaLineSpace(aSpaceMode, aSpace);
            CheckUpdateInfo();
        }

        /// <summary> 修改当前选中文本的样式 </summary>
        public void ApplyTextStyle(HCFontStyle aFontStyle)
        {
            FData.ApplyTextStyle(aFontStyle);
            CheckUpdateInfo();
        }

        /// <summary> 修改当前选中文本的字体 </summary>
        public void ApplyTextFontName(string aFontName)
        {
            FData.ApplyTextFontName(aFontName);
            CheckUpdateInfo();
        }

        /// <summary> 修改当前选中文本的字号 </summary>
        public void ApplyTextFontSize(Single aFontSize)
        {
            FData.ApplyTextFontSize(aFontSize);
            CheckUpdateInfo();
        }

        /// <summary> 修改当前选中文本的颜色 </summary>
        public void ApplyTextColor(Color aColor)
        {
            FData.ApplyTextColor(aColor);
            CheckUpdateInfo();
        }

        /// <summary> 修改当前选中文本的背景颜色 </summary>
        public void ApplyTextBackColor(Color aColor)
        {
            FData.ApplyTextBackColor(aColor);
            CheckUpdateInfo();
        }

        public bool InsertItem(HCCustomItem aItem)
        {
            HCFunction vEvent = delegate()
            {
                return FData.InsertItem(aItem);
            };

            return DataChangeByAction(vEvent);
        }

        public bool InsertItem(int aIndex, HCCustomItem aItem)
        {
            HCFunction vEvent = delegate()
            {
                return FData.InsertItem(aIndex, aItem);
            };

            return DataChangeByAction(vEvent);
        }

        public bool InsertDomain(HCDomainItem aMouldDomain)
        {
            HCFunction vEvent = delegate()
            {
                return FData.InsertDomain(aMouldDomain);
            };

            return DataChangeByAction(vEvent);
        }

        public bool InsertTable(int aRowCount, int aColCount)
        {
            HCFunction vEvent = delegate()
            {
                HCRichData vTopData = FData.GetTopLevelData() as HCRichData;
                return vTopData.InsertTable(aRowCount, aColCount);
            };

            return DataChangeByAction(vEvent);
        }

        public HCCustomData TopLevelData()
        {
            return FData.GetTopLevelData();
        }

        /// <summary> 设置当前TextItem的文本内容 </summary>
        public void SetActiveItemText(string aText)
        {
            FData.SetActiveItemText(aText);
            CheckUpdateInfo();
        }

        public void SelectAll()
        {
            FData.SelectAll();

            FStyle.UpdateInfoRePaint();
            CheckUpdateInfo();
        }

        public void SaveToFile(string aFileName)
        {
            FileStream vStream = new FileStream(aFileName, FileMode.Create, FileAccess.Write);
            try
            {
                SaveToStream(vStream);
            }
            finally
            {
                vStream.Close();
                vStream.Dispose();
            }
        }

        public void LoadFromFile(string aFileName)
        {
            FileStream vStream = new FileStream(aFileName, FileMode.Open, FileAccess.Read);
            try
            {
                LoadFromStream(vStream);
            }
            finally
            {
                vStream.Dispose();
            }
        }

        public void SaveToStream(Stream aStream)
        {
            HC._SaveFileFormatAndVersion(aStream);  // 文件格式和版本
            _DeleteUnUsedStyle();
            FStyle.SaveToStream(aStream);
            FData.SaveToStream(aStream);
        }

        public void LoadFromStream(Stream aStream)
        {
            this.BeginUpdate();
            try
            {
                // 清除撤销恢复数据
                FUndoList.Clear();
                FUndoList.SaveState();
                try
                {
                    FUndoList.Enable = false;

                    FData.Clear();
                    FStyle.Initialize();

                    aStream.Position = 0;
                    string vFileExt = "";
                    ushort viVersion = 0;
                    byte vLang = 0;
                    HC._LoadFileFormatAndVersion(aStream, ref vFileExt, ref viVersion, ref vLang);
                    if (vFileExt != HC.HC_EXT)
                        throw new Exception("加载失败，不是" + HC.HC_EXT + "文件！");

                    FStyle.LoadFromStream(aStream, viVersion);
                    FData.LoadFromStream(aStream, FStyle, viVersion);
                    DoMapChanged();
                }
                finally
                {
                    FUndoList.RestoreState();
                }
            }
            finally
            {
                this.EndUpdate();
            }
        }

        public void Clear()
        {
            FData.Clear();
        }

        /// <summary> 撤销 </summary>
        public void Undo()
        {
            if (FUndoList.Enable)
            {
                try
                {
                    FUndoList.Enable = false;

                    BeginUpdate();
                    try
                    {
                        FUndoList.Undo();
                    }
                    finally
                    {
                        EndUpdate();
                    }
                }
                finally
                {
                    FUndoList.Enable = true;
                }
            }
        }

        /// <summary> 重做 </summary>
        public void Redo()
        {
            if (FUndoList.Enable)
            {
                try
                {
                    FUndoList.Enable = false;

                    BeginUpdate();
                    try
                    {
                        FUndoList.Redo();
                    }
                    finally
                    {
                        EndUpdate();
                    }
                }
                finally
                {
                    FUndoList.Enable = true;
                }
            }
        }

        public void UndoGroupBegin()
        {
            if (FUndoList.Enable)
                FUndoList.UndoGroupBegin(FData.SelectInfo.StartItemNo, FData.SelectInfo.StartItemOffset);
        }

        public void UndoGroupEnd()
        {
            if (FUndoList.Enable)
                FUndoList.UndoGroupEnd(FData.SelectInfo.StartItemNo, FData.SelectInfo.StartItemOffset);
        }

        public void UpdateView()
        {
            if ((FUpdateCount == 0) && IsHandleCreated)
            {
                if (FMemDC != IntPtr.Zero)
                    GDI.DeleteDC(FMemDC);
                FMemDC = (IntPtr)GDI.CreateCompatibleDC(FDC);

                int vViewWidth = GetViewWidth();
                int vViewHeight = GetViewHeight();
                IntPtr vBitmap = (IntPtr)GDI.CreateCompatibleBitmap(FDC, vViewWidth, vViewHeight);
                GDI.SelectObject(FMemDC, vBitmap);
                try
                {
                    using (HCCanvas vDataBmpCanvas = new HCCanvas(FMemDC))
                    {
                        // 控件背景
                        vDataBmpCanvas.Brush.Color = Color.White;// $00E7BE9F;
                        vDataBmpCanvas.FillRect(new RECT(0, 0, vViewWidth, vViewHeight));

                        PaintInfo vPaintInfo = new PaintInfo();
                        try
                        {
                            FData.PaintData(this.Padding.Left - FHScrollBar.Position,  // 当前页数据要绘制到的Left
                              this.Padding.Top,     // 当前页数据要绘制到的Top
                              this.Width - FHScrollBar.Position - this.Padding.Right,
                              this.Padding.Top + FData.Height,  // 当前页数据要绘制的Bottom
                              this.Padding.Top,     // 界面呈现当前页数据的Top位置
                              this.Height - FHScrollBar.Height,  // 界面呈现当前页数据Bottom位置
                              FVScrollBar.Position,  // 指定从哪个位置开始的数据绘制到页数据起始位置
                              vDataBmpCanvas,
                              vPaintInfo);

                            for (int i = 0; i <= vPaintInfo.TopItems.Count - 1; i++)  // 绘制顶层Ite
                                vPaintInfo.TopItems[i].PaintTop(vDataBmpCanvas);
                        }
                        finally
                        {
                            vPaintInfo.Dispose();
                        }

                        GDI.BitBlt(FDC, 0, 0, vViewWidth, vViewHeight, FMemDC, 0, 0, GDI.SRCCOPY);
                    }
                }
                finally
                {
                    GDI.DeleteObject(vBitmap);
                }

                RECT vRect = new RECT(0, 0, vViewWidth, vViewHeight);
                User.InvalidateRect(this.Handle, ref vRect, 0);  // 只更新变动区域，防止闪烁，解决BitBlt光标滞留问题
                User.UpdateWindow(this.Handle);
            }
        }

        public void BeginUpdate()
        {
            FUpdateCount++;
        }

        public void EndUpdate()
        {
            FUpdateCount--;
            DoMapChanged();
        }

        public int CurStyleNo
        {
            get { return GetCurStyleNo(); }
        }

        public int CurParaNo
        {
            get { return GetCurParaNo(); }
        }

        public HCViewData Data
        {
            get { return FData; }
        }

        public HCStyle Style
        {
            get { return FStyle; }
        }

        public bool Changed
        {
            get { return FChanged; }
            set { FChanged = value; }
        }

        public EventHandler OnChange
        {
            get { return FOnChange; }
            set { FOnChange = value; }
        }
    }
}
