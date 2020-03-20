using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using HC.Win32;
using System.Reflection;
using System.Runtime.InteropServices;

namespace HC.View
{
    public delegate void PopupPaintEventHandler(HCCanvas aCanvas, RECT aClientRect);

    public class HCPopupForm : HCObject
    {
        private bool FOpened;
        private IntPtr FPopupWindow;
        private int FWidth, FHeight;  // 鼠标滚轮蓄能器

        private PopupPaintEventHandler FOnPaint;
        private EventHandler FOnPopupClose;
        private MouseEventHandler FOnMouseDown;
        private MouseEventHandler FOnMouseMove;
        private MouseEventHandler FOnMouseUp;
        private MouseEventHandler FOnMouseWheel;
        private MouseEventHandler FOnMouseWheelDown;
        private MouseEventHandler FOnMouseWheelUp;
        private WNDPROC WndProc;

        // 窗口过程   
        private int WindowProcedure(IntPtr hwnd, int msg, int wParam, int lParam)
        {
            switch (msg)
            {
                //case User.WM_DESTROY:
                //    User.PostQuitMessage(0);
                //    break;

                case User.WM_PAINT:
                    DoPopupFormPaint();
                    return 1;

                case User.WM_ERASEBKGND:
                    return 1;

                case User.WM_MOUSEACTIVATE:
                    return User.MA_NOACTIVATE;

                case User.WM_NCACTIVATE:
                    FOpened = false;
                    return 1;

                case User.WM_SYSCOMMAND:
                    ClosePopup(true);
                    break;

                case User.WM_LBUTTONDOWN:
                    WMLButtonDown(wParam, lParam);
                    break;

                case User.WM_LBUTTONUP:
                    WMLButtonUp(wParam, lParam);
                    break;

                case User.WM_MOUSEMOVE:
                    if (hwnd != FPopupWindow)
                        return 0;

                    WMMouseMove(wParam, lParam);
                    break;

                case User.WM_MOUSEWHEEL:
                    WMMouseWheel(wParam, lParam);
                    break;

                default:
                    return User.DefWindowProc(hwnd, msg, wParam, lParam);  
            }

            return 0;
        }


        private void RegFormClass()
        {
            IntPtr hInstance = Marshal.GetHINSTANCE(this.GetType().Module); //(IntPtr)Kernel.GetModuleHandle(null);
            WNDCLASSEX vWndCls = WNDCLASSEX.Build();
            if (!User.GetClassInfoEx(hInstance, "HCPopupForm", ref vWndCls))
            {
                vWndCls = WNDCLASSEX.Build();  //vWndCls.cbSize = 48;
                vWndCls.lpszClassName = "HCPopupForm";
                vWndCls.style = User.CS_VREDRAW | User.CS_HREDRAW | User.CS_DBLCLKS | User.CS_DROPSHADOW;
                vWndCls.hInstance = hInstance;
                vWndCls.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(WndProc);
                vWndCls.cbClsExtra = 0;
                vWndCls.cbWndExtra = 8;
                vWndCls.hIcon = IntPtr.Zero;
                vWndCls.hIconSm = IntPtr.Zero;
                vWndCls.hCursor = IntPtr.Zero;
                vWndCls.hbrBackground = (IntPtr)GDI.GetStockObject(0);
                vWndCls.lpszMenuName = null;

                if (User.RegisterClassEx(ref vWndCls) == 0)
                {
                    throw new Exception("异常：注册HCPopupForm错误");
                }
            }
        }

        private void CreateFormHandle()
        {
            if (User.IsWindow(FPopupWindow) == 0)
            {
                WndProc -= WindowProcedure;
                WndProc += WindowProcedure;
                RegFormClass();
                IntPtr hInstance = Marshal.GetHINSTANCE(this.GetType().Module);// (IntPtr)Kernel.GetModuleHandle(null);// Marshal.GetHINSTANCE(null);

                FPopupWindow = (IntPtr)User.CreateWindowEx(User.WS_EX_TOPMOST | User.WS_EX_TOOLWINDOW,
                    "HCPopupForm",
                    "",
                    User.WS_POPUP,
                    0, 0, FWidth, FHeight, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);
            }
        }

        private void DestroyForm()
        {
            if (User.IsWindow(FPopupWindow) > 0)
            {
                User.DestroyWindow(FPopupWindow);
                FPopupWindow = IntPtr.Zero;

                IntPtr hInstance = Marshal.GetHINSTANCE(this.GetType().Module);
                User.UnregisterClass("HCPopupForm", hInstance);
            }
        }

        private void DoPopupFormPaint()
        {
            if (FOnPaint != null)
            {
                PAINTSTRUCT vPaintStruct = new PAINTSTRUCT();
                IntPtr vDC = (IntPtr)User.BeginPaint(FPopupWindow, ref vPaintStruct);
                try
                {
                    HCCanvas vCanvas = new HCCanvas(vDC);
                    try
                    {
                        RECT vRect = new RECT();
                        User.GetClientRect(FPopupWindow, ref vRect);
                        FOnPaint(vCanvas, vRect);
                    }
                    finally
                    {
                        vCanvas.Dispose();
                    }
                }
                finally
                {
                    User.EndPaint(FPopupWindow, ref vPaintStruct);
                }
            }
        }

        private POINT CalcCursorPos()
        {
            POINT Result = new POINT();
            User.GetCursorPos(out Result);
            User.ScreenToClient(FPopupWindow, ref Result);
            return Result;
        }

        private void DoMouseDown(int x, int y)
        {
            if (FOnMouseDown != null)
            {
                if ((Width > 32768) || (Height > 32768))
                {
                    POINT vPt = CalcCursorPos();
                    MouseEventArgs e = new MouseEventArgs(MouseButtons.Left, 0, vPt.X, vPt.Y, 0);
                    FOnMouseDown(this, e);
                }
                else
                {
                    MouseEventArgs e = new MouseEventArgs(MouseButtons.Left, 0, x, y, 0);
                    FOnMouseDown(this, e);
                }
            }
        }

        private void DoMouseUp(int x, int y)
        {
            if (FOnMouseUp != null)
            {
                MouseEventArgs e = new MouseEventArgs(MouseButtons.Left, 1, x, y, 0);
                FOnMouseUp(this, e);
            }
        }

        private bool DoMouseWheel(MouseEventArgs e)
        {
            bool Result = false;
            if (FOnMouseWheel != null)
            {
                FOnMouseWheel(this, e);
                Result = true;
            }

            return Result;
        }

        private int WMMouseWheel(int wParam, int lParam)
        {
            int Result = 0;
            MouseEventArgs e = new MouseEventArgs(MouseButtons.Middle, 0,
                LoWord(lParam), HiWord(lParam), HiWord(wParam));

            if (DoMouseWheel(e))
                Result = 1;

            return Result;
        }

        private int LoWord(int a)
        {
            return a & 0xFFFF;
        }

        private int HiWord(int a)
        {
            return a >> 16;
        }

        private void WMLButtonDown(int wParam, int lParam)
        {
            DoMouseDown(LoWord(lParam), HiWord(lParam));
        }

        private void WMMouseMove(int wParam, int lParam)
        {
            if (FOnMouseMove != null)
            {
                if ((Width > 32768) || (Height > 32768))
                {
                    POINT vPt = CalcCursorPos();
                    MouseEventArgs e = new MouseEventArgs(MouseButtons.Left, 0, vPt.X, vPt.Y, 0);
                    FOnMouseMove(this, e);
                }
                else
                {
                    MouseEventArgs e = new MouseEventArgs(MouseButtons.Left, 0, LoWord(lParam), HiWord(lParam), 0);
                    FOnMouseMove(this, e);
                }
            }
        }

        private void WMLButtonUp(int wParam, int lParam)
        {
            DoMouseUp(LoWord(lParam), HiWord(lParam));
        }

        protected void SetWidth(int Value)
        {
            if (FWidth != Value)
            {
                FWidth = Value;
                User.SetWindowPos(FPopupWindow, IntPtr.Zero, 0, 0, FWidth, FHeight, User.SWP_NOZORDER);
            }
        }

        protected void SetHeight(int Value)
        {
            if (FHeight != Value)
            {
                FHeight = Value;
                User.SetWindowPos(FPopupWindow, IntPtr.Zero, 0, 0, FWidth, FHeight, User.SWP_NOZORDER);
            }
        }

        public HCPopupForm()
        {
            FPopupWindow = IntPtr.Zero;
            FOpened = false;
            FWidth = 50;
            FHeight = 100;

            //WndProc -= WindowProcedure;
            //WndProc += WindowProcedure;
            //RegFormClass();
        }

        ~HCPopupForm()
        {

        }

        public override void Dispose()
        {
            base.Dispose();
            DestroyForm();
        }

        private bool IsFPopupWindow(IntPtr Wnd)
        {
            while ((Wnd != IntPtr.Zero) && (Wnd != FPopupWindow))
                Wnd = (IntPtr)User.GetParent(Wnd);

            return (Wnd == FPopupWindow);
        }

        private void MessageLoop()
        {
            try
            {
                MSG vmsg = new MSG();

                while (true)
                {
                    if (!FOpened)
                        return;

                    if (User.PeekMessage(ref vmsg, IntPtr.Zero, 0, 0, User.PM_NOREMOVE) > 0)
                    {
                        if ((vmsg.message == User.WM_NCLBUTTONDOWN)
                            || (vmsg.message == User.WM_NCLBUTTONDBLCLK)
                            || (vmsg.message == User.WM_LBUTTONDOWN)
                            || (vmsg.message == User.WM_LBUTTONDBLCLK)
                            || (vmsg.message == User.WM_NCRBUTTONDOWN)
                            || (vmsg.message == User.WM_NCRBUTTONDBLCLK)
                            || (vmsg.message == User.WM_RBUTTONDOWN)
                            || (vmsg.message == User.WM_RBUTTONDBLCLK)
                            || (vmsg.message == User.WM_NCMBUTTONDOWN)
                            || (vmsg.message == User.WM_NCMBUTTONDBLCLK)
                            || (vmsg.message == User.WM_MBUTTONDOWN)
                            || (vmsg.message == User.WM_MBUTTONDBLCLK))
                        {
                            if (!IsFPopupWindow(vmsg.hwnd))
                            {
                                User.PeekMessage(ref vmsg, IntPtr.Zero, 0, 0, User.PM_REMOVE);
                                break;
                            }
                        }
                        else
                        if (vmsg.message == User.WM_MOUSEWHEEL)
                        {
                            User.PeekMessage(ref vmsg, IntPtr.Zero, 0, 0, User.PM_REMOVE);
                            User.SendMessage(FPopupWindow, vmsg.message, vmsg.wParam, vmsg.lParam);
                            continue;
                        }
                        else
                        if ((vmsg.message == User.WM_KILLFOCUS) || (vmsg.message == User.WM_ACTIVATEAPP))
                        {
                            return;
                        }
                        else
                        if (vmsg.message == User.WM_ACTIVATEAPP)
                        {
                            break;
                        }

                        //Application.HandleMessage;
                        if (User.PeekMessage(ref vmsg, IntPtr.Zero, 0, 0, User.PM_REMOVE) > 0)
                        {
                            if (vmsg.message != User.WM_QUIT)
                            {
                                User.TranslateMessage(ref vmsg);
                                User.DispatchMessage(ref vmsg);
                            }
                        }
                    }
                    else
                        Application.RaiseIdle(null);
                }
            }
            finally
            {
                if (FOpened)
                    ClosePopup(true);
            }
        }

        public void Popup(int x, int y)
        {
            CreateFormHandle();  // 创建下拉弹出窗体
            RECT vBound = new RECT();
            User.GetWindowRect(FPopupWindow, ref vBound);

            int vW = vBound.Width;
            int vH = vBound.Height;

            IntPtr vMonitor = User.MonitorFromPoint(new POINT(x, y), User.MonitorOptions.MONITOR_DEFAULTTONEAREST);

            if (vMonitor != IntPtr.Zero)
            {
                MONITORINFO vMonInfo = new MONITORINFO();
                vMonInfo.cbSize = 40;
                User.GetMonitorInfo(vMonitor, ref vMonInfo);

                if (x + vW > vMonInfo.rcWork.Right)
                    x = vMonInfo.rcWork.Right - vW;
                if (y + vH > vMonInfo.rcWork.Bottom)
                    y = y - vH - 20;

                if (x < vMonInfo.rcWork.Left)
                    x = vMonInfo.rcWork.Left;
                if (y < vMonInfo.rcWork.Top)
                    y = vMonInfo.rcWork.Top;
            }
            else
            {
                int vScreenWidth = User.GetSystemMetrics(User.SM_CXSCREEN);
                int vScreenHeight = User.GetSystemMetrics(User.SM_CYSCREEN);

                if (x + vW > vScreenWidth)
                    x = vScreenWidth - vW;

                if (y + vH > vScreenHeight)
                    y = vBound.Top - vH;

                if (x < 0)
                    x = 0;
                if (y < 0)
                    y = 0;
            }

            User.SetWindowPos(FPopupWindow, IntPtr.Zero, x, y, vW, vH, User.SWP_NOACTIVATE | User.SWP_SHOWWINDOW);
            //User.BringWindowToTop(FPopupWindow);
            FOpened = true;
            MessageLoop();
        }

        public void ClosePopup(bool aCancel)
        {
            if ((!aCancel) && (FOnPopupClose != null))
                FOnPopupClose(this, null);

            DestroyForm();
            FOpened = false;
        }

        public void UpdatePopup()
        {
            if (User.IsWindowVisible(FPopupWindow) > 0)
            {
                RECT vRect = new RECT();
                User.GetClientRect(FPopupWindow, ref vRect);
                User.InvalidateRect(FPopupWindow, ref vRect, 0);
            }
        }

        public EventHandler OnPopupClose
        {
            get { return FOnPopupClose; }
            set { FOnPopupClose = value; }
        }

        public bool Open
        {
            get { return FOpened; }
        }

        public int Width
        {
            get { return FWidth; }
            set { SetWidth(value); }
        }

        public int Height
        {
            get { return FHeight; }
            set { SetHeight(value); }
        }

        public PopupPaintEventHandler OnPaint
        {
            get { return FOnPaint; }
            set { FOnPaint = value; }
        }

        public MouseEventHandler OnMouseDown
        {
            get { return FOnMouseDown; }
            set { FOnMouseDown = value; }
        }

        public MouseEventHandler OnMouseMove
        {
            get { return FOnMouseMove; }
            set { FOnMouseMove = value; }
        }

        public MouseEventHandler OnMouseUp
        {
            get { return FOnMouseUp; }
            set { FOnMouseUp = value; }
        }

        public MouseEventHandler OnMouseWheel
        {
            get { return FOnMouseWheel; }
            set { FOnMouseWheel = value; }
        }

        public MouseEventHandler OnMouseWheelDown
        {
            get { return FOnMouseWheelDown; }
            set { FOnMouseWheelDown = value; }
        }

        public MouseEventHandler OnMouseWheelUp
        {
            get { return FOnMouseWheelUp; }
            set { FOnMouseWheelUp = value; }
        }
    }
}