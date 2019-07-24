using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using HC.Win32;
using System.Windows.Forms;

namespace EMRView
{
    public class HCInputHelper : Object
    {
        private IntPtr FHandle;
        private int FLeft, FTop, FWidth, FHeight;
        private Color FBorderColor;
        private string FCompStr;
        private bool FEnable, FResize;

        private bool GetVisible()
        {
            return User.IsWindowVisible(FHandle) != 0;
        }

        private void Paint(IntPtr aDC, RECT aRect)
        {

        }

        private void WndProc(ref Message Message)
        {

        }

        public HCInputHelper()
        {

        }

        ~HCInputHelper()
        {

        }

        public void Show()
        {

        }

        public void Show(int aLeft, int aTop)
        {

        }

        public void Show(POINT aPoint)
        {

        }

        public void Close()
        {

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

        public void SetCompositionString(string s)
        {

        }

        public void CompWndMove(IntPtr aHandle, int aCaretX, int aCaretY)
        {

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
        }

        public bool Visible
        {
            get { return GetVisible(); }
        }
    }
}
