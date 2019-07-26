using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using HC.Win32;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using HC.View;

namespace EMRView
{
    public class HCInputHelper : Object
    {
        private IntPtr FHandle;
        private int FLeft, FTop, FWidth, FHeight;
        private Color FBorderColor;
        private string FCompStr, FBeforText, FAfterText;
        private bool FEnable, FResize, FActive;
        private const string ImeExtFormClassName = "THCInputHelper";
        private WNDPROC WndProc;

        private bool GetVisible()
        {
            return User.IsWindowVisible(FHandle) != 0;
        }

        private void SetActive(bool value)
        {
            if (FActive != value)
            {
                FActive = value;
                UpdateView();
            }
        }

        private void Paint(HCCanvas aCanvas, ref RECT aRect)
        {
            aCanvas.Font.Family = "宋体";
            aCanvas.Font.Size = 10;
            aCanvas.Pen.Color = FBorderColor;
            if (!FActive)
                aCanvas.Brush.Color = Color.FromArgb(0xFE, 0xFE, 0xFF);
            else
              aCanvas.Brush.Color = HC.View.HC.clInfoBk;

            aCanvas.Rectangle(aRect);

            aRect.Inflate(-5, -5);

            string vText = "你好，我可以给你提供相关知识^_^";
            if (FCompStr != "")
                vText = "[" + FBeforText +"]" + "[" + FAfterText + "]";// "1.第1项 2.第2项 3.第3项 4.第4项 5.第5项";
            else
                vText = "我现在还没有和知识结合呢^_^";
            
            aCanvas.TextRect(ref aRect, vText, User.DT_VCENTER | User.DT_SINGLELINE);
        }

        private void UpdateView()
        {
            RECT vRect = ClientRect();
            User.InvalidateRect(FHandle, ref vRect, 0);
        }

        private int WindowProcedure(IntPtr hwnd, int msg, int wParam, int lParam)
        {
            switch (msg)
            {
                //case User.WM_DESTROY:
                //    User.PostQuitMessage(0);
                //    break;

                case User.WM_PAINT:
                    PAINTSTRUCT vPaintStruct = new PAINTSTRUCT();
                    IntPtr vDC = (IntPtr)User.BeginPaint(hwnd, ref vPaintStruct);
                    try
                    {
                        HCCanvas vCanvas = new HCCanvas(vDC);
                        try
                        {
                            RECT vRect = new RECT();
                            User.GetClientRect(hwnd, ref vRect);
                            Paint(vCanvas, ref vRect);
                        }
                        finally
                        {
                            vCanvas.Dispose();
                        }
                    }
                    finally
                    {
                        User.EndPaint(hwnd, ref vPaintStruct);
                    }
                    return 1;

                case User.WM_ERASEBKGND:
                    return 1;

                case User.WM_MOUSEACTIVATE:
                    return 3;

                default:
                    return User.DefWindowProc(hwnd, msg, wParam, lParam);
            }
        }

        public HCInputHelper()
        {
            WndProc -= WindowProcedure;
            WndProc += WindowProcedure;

            IntPtr hInstance = Marshal.GetHINSTANCE(this.GetType().Module); //(IntPtr)Kernel.GetModuleHandle(null);
            WNDCLASSEX vWndCls;
            if (!User.GetClassInfoEx(hInstance, ImeExtFormClassName, out vWndCls))
            {
                vWndCls = WNDCLASSEX.Build();  //vWndCls.cbSize = 48;
                vWndCls.lpszClassName = ImeExtFormClassName;
                vWndCls.style = User.CS_VREDRAW | User.CS_HREDRAW | User.CS_DROPSHADOW;
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
                    throw new Exception("异常：注册输入法提示窗口错误");
                }
            }

            FLeft = 20;
            FTop = 20;
            FWidth = 400;
            FHeight = 24;
            FResize = false;
            FEnable = true;
            FActive = false;
            FBorderColor = Color.FromArgb(0xB5, 0xC5, 0xD2);
            FCompStr = "";

            if (User.IsWindow(FHandle) == 0)
            {
                FHandle = (IntPtr)User.CreateWindowEx(User.WS_EX_TOPMOST | User.WS_EX_TOOLWINDOW,
                    ImeExtFormClassName, ImeExtFormClassName, User.WS_POPUP | User.WS_DISABLED,
                    0, 0, FWidth, FHeight, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);
            }

            if (User.IsWindow(FHandle) == 0)
                throw new Exception("HCInputHelper创建失败：");
        }

        ~HCInputHelper()
        {
            if (User.IsWindow(FHandle) > 0)
            {
                User.DestroyWindow(FHandle);
                FHandle = IntPtr.Zero;

                IntPtr hInstance = Marshal.GetHINSTANCE(this.GetType().Module);
                User.UnregisterClass(ImeExtFormClassName, hInstance);
            }
        }

        public void Show()
        {
            FActive = false;

            if (!Visible)
                User.ShowWindow(FHandle, User.SW_SHOWNOACTIVATE);

            User.MoveWindow(FHandle, FLeft, FTop, FWidth, FHeight, 0);
        }

        public void Show(int aLeft, int aTop)
        {
            FLeft = aLeft;
            FTop = aTop;
            Show();
        }

        public void Show(POINT aPoint)
        {
            Show(aPoint.X, aPoint.Y);
        }

        public void Close()
        {
            User.ShowWindow(FHandle, User.SW_HIDE);
        }

        public bool ResetImeCompRect(ref POINT aImePosition)
        {
            bool vResult = false;

            if (true)
            {
                aImePosition.Y += FHeight + 2;
                vResult = true;
            }

            return vResult;
        }

        public RECT ClientRect()
        {
            return new RECT(0, 0, FWidth, FHeight);
        }

        public string GetCandiText(uint aIndex)
        {
            return "第" + (aIndex + 1).ToString() + "项";
        }

        public void SetCompositionString(string s)
        {
            FResize = false;

            if (FCompStr != s)
            {
                FCompStr = s;
                // TODO : 新的输入，重新匹配知识词条 

                if (FCompStr != "")  // 有知识
                {
                    if (!Visible)
                        Show();
                    else
                        UpdateView();
                }
                else  // 无知识
                    Close();
            }
        }

        public void SetCaretString(string aBeforText, string aAfterText)
        {
            FBeforText = aBeforText;
            FAfterText = aAfterText;
            UpdateView();
        }

        public void CompWndMove(IntPtr aHandle, int aCaretX, int aCaretY)
        {
            POINT vPt = new POINT(aCaretX, aCaretY);
            User.ClientToScreen(aHandle, ref vPt);
            FLeft = vPt.X + 2;
            FTop = vPt.Y + 4;
            if (FCompStr != "")
                Show();
        }

        public int Height
        {
            get { return FHeight; }
        }

        public bool Resize
        {
            get { return FResize; }
        }

        public bool Enable
        {
            get { return FEnable; }
            set { FEnable = value; }
        }

        public bool Visible
        {
            get { return GetVisible(); }
        }

        public bool Active
        {
            get { return FActive; }
            set { SetActive(value); }
        }
    }
}
