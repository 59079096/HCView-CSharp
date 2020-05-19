using HC.View;
using HC.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace EMRView
{
    public class HCEmrViewIH :
        #if VIEWTOOL
        HCViewTool
        #else
        HCView
        #endif
    {
        private frmInputHelper FInputHelper;
        private const string CARETSTOPCHAR = "，,。;；：:";

#if GLOBALSHORTKEY
        private const int WH_KEYBOARD_LL = 13;  // //低级键盘钩子的索引值
        private const uint LLKHF_ALTDOWN = 0x20;
        private IntPtr HHKLowLevelKybd = IntPtr.Zero;
        private FNHookProc HookProc;


        private int KeyboardProc(int nCode, int wParam, IntPtr lParam)
        {
            int Result = 0;
            bool vEatKeystroke = false;

            if (((IntPtr)User.GetFocus() == this.Handle) && (nCode == User.HC_ACTION))
            {
                if ((wParam == User.WM_SYSKEYDOWN) || (wParam == User.WM_SYSKEYUP))
                {
                    tagKBDLLHOOKSTRUCT vPKB = (tagKBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(tagKBDLLHOOKSTRUCT));
                    if (vPKB.flags == LLKHF_ALTDOWN)
                    {
                        if (vPKB.vkCode == User.VK_SPACE)
                        {
                            FInputHelper.ShowEx();
                            vEatKeystroke = true;
                        }
                    }
                }
            }

            if (vEatKeystroke)
                Result = 1;
            else
            if (nCode != 0)
                Result = User.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);

            return Result;
        }

        private void SetIHKeyHook()
        {
            if (HHKLowLevelKybd == IntPtr.Zero)
            {
                HookProc -= KeyboardProc;
                HookProc += KeyboardProc;
                IntPtr lpfn = Marshal.GetFunctionPointerForDelegate(HookProc);
                IntPtr hInstance = Marshal.GetHINSTANCE(this.GetType().Module);  //  (IntPtr)Kernel.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
                HHKLowLevelKybd = (IntPtr)User.SetWindowsHookEx(WH_KEYBOARD_LL, lpfn, hInstance, 0);
            }
        }

        private void UnSetIHKeyHook()
        {
            if (HHKLowLevelKybd != IntPtr.Zero)
            {
                User.UnhookWindowsHookEx(HHKLowLevelKybd);
                HHKLowLevelKybd = IntPtr.Zero;
            }
        }
#endif

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if ((!this.ReadOnly) && FInputHelper.EnableEx && (e.Control) && (e.KeyCode == Keys.H))
                FInputHelper.ShowEx();
            else
            if ((!this.ReadOnly) && FInputHelper.EnableEx && (!e.Control) && (!e.Shift) && (e.KeyCode == Keys.Escape))
                FInputHelper.CloseEx();
            else
                base.OnKeyDown(e);
        }

        protected override void WndProc(ref Message Message)
        {
            if (FInputHelper.EnableEx)
            {
                switch (Message.WParam.ToInt32())
                {
                    case 0x000B:
                    case 0x0004:
                        FInputHelper.CompWndMove(this.Handle, Caret.X, Caret.Y + Caret.Height);
                        break;
                }
            }

            base.WndProc(ref Message);

            if (Message.Msg == User.WM_KILLFOCUS)
            {
                if ((FInputHelper != null) && FInputHelper.EnableEx)
                    FInputHelper.CloseEx();
            }
        }

        protected virtual bool DoProcessIMECandi(string aCandi)
        {
            return true;
        }

        private string GetCompositionStr(int aType)
        {
            string Result = "";
            if (this.hImc != IntPtr.Zero)
            {
                int vSize = Imm.ImmGetCompositionString(this.hImc, aType, null, 0);
                if (vSize > 0)
                {
                    byte[] vBuffer = new byte[vSize];
                    Imm.ImmGetCompositionString(this.hImc, aType, vBuffer, vSize);
                    Result = System.Text.Encoding.Default.GetString(vBuffer);

                }
            }

            return Result;
        }

        protected override void UpdateImeComposition(int aLParam)
        {
            if (FInputHelper.EnableEx && ((aLParam & Imm.GCS_COMPSTR) != 0))
            {
                string vS = GetCompositionStr(Imm.GCS_COMPSTR);
                FInputHelper.SetCompositionString(vS);

                if (FInputHelper.ChangeSize)
                {
                    COMPOSITIONFORM vCF = new COMPOSITIONFORM();
                    if (Imm.ImmGetCompositionWindow(this.hImc, ref vCF))
                    {
                        if (FInputHelper.ResetImeCompRect(ref vCF.ptCurrentPos))
                            Imm.ImmSetCompositionWindow(this.hImc, ref vCF);
                    }
                }
            }

            if ((aLParam & Imm.GCS_RESULTSTR) != 0)
            {
                string vS = GetCompositionStr(Imm.GCS_RESULTSTR);
                if (vS != "")
                {
                    if (DoProcessIMECandi(vS))
                        InsertText(vS);
                }
            }
        }

        protected override void UpdateImePosition()
        {
            COMPOSITIONFORM vCF = new COMPOSITIONFORM();
            vCF.ptCurrentPos.X = Caret.X;
            vCF.ptCurrentPos.Y = Caret.Y + Caret.Height + 4;

            if (FInputHelper.EnableEx)
                FInputHelper.ResetImeCompRect(ref vCF.ptCurrentPos);

            vCF.dwStyle = 0x0020;
            Rectangle vr = this.ClientRectangle;
            vCF.rcArea = new RECT(vr.Left, vr.Top, vr.Right, vr.Bottom);
            Imm.ImmSetCompositionWindow(this.hImc, ref vCF);

            if (FInputHelper.EnableEx)
                FInputHelper.CompWndMove(this.Handle, Caret.X, Caret.Y + Caret.Height);
        }

#region 取光标前后字符
        private bool GetCharBefor(int aOffset, ref string aChars)
        {
            for (int i = aOffset - 1; i >= 1 - 1; i--)
            {
                if (CARETSTOPCHAR.IndexOf(aChars[i]) >= 0)
                {
                    aChars = aChars.Substring(i + 1, aOffset - i - 1);
                    return true;
                }
            }

            return false;
        }

        private void GetBeforString(HCCustomData aData, int aStartItemNo, ref string aBefor)
        {
            string vText = "";
            for (int i = aStartItemNo - 1; i >= 0; i--)
            {
                vText = aData.Items[i].Text;
                if ((vText != "") && (GetCharBefor(vText.Length, ref vText)))
                {
                    aBefor = vText + aBefor;
                    return;
                }
                else
                    aBefor = vText + aBefor;
            }
        }

        private bool GetCharAfter(int aOffset, ref string aChars)
        {
            for (int i = aOffset - 1; i < aChars.Length; i++)
            {
                if (CARETSTOPCHAR.IndexOf(aChars[i]) >= 0)
                {
                    aChars = aChars.Substring(aOffset - 1, i - aOffset + 1);
                    return true;
                }
            }

            return false;
        }

        private void GetAfterString(HCCustomData aData, int aStartItemNo, ref string aAfter)
        {
            string vText = "";
            for (int i = aStartItemNo + 1; i < aData.Items.Count; i++)
            {
                vText = aData.Items[i].Text;
                if ((vText != "") && (GetCharAfter(1, ref vText)))
                {
                    aAfter = aAfter + vText;
                    return;
                }
                else
                    aAfter = aAfter + vText;
            }
        }
#endregion

        protected override void DoCaretChange()
        {
            base.DoCaretChange();

            if (!FInputHelper.EnableEx)
                return;

            if (!this.Style.UpdateInfo.ReStyle)
                return;

            string vsBefor = "";
            string vsAfter = "";

            HCCustomData vTopData = this.ActiveSectionTopLevelData();
            int vCurItemNo = vTopData.SelectInfo.StartItemNo;
            HCCustomItem vCurItem = vTopData.GetActiveItem();
            if (vCurItem.StyleNo < HCStyle.Null)
            {
                if (vTopData.SelectInfo.StartItemOffset == HC.View.HC.OffsetBefor)
                    GetBeforString(vTopData, vCurItemNo - 1, ref vsBefor);
                else
                    GetAfterString(vTopData, vCurItemNo + 1, ref vsAfter);
            }
            else
            {
                // 取光标前
                string vText = vCurItem.Text;
                if (GetCharBefor(vTopData.SelectInfo.StartItemOffset, ref vText))
                    vsBefor = vText;
                else
                {
                    vsBefor = vText.Substring(0, vTopData.SelectInfo.StartItemOffset);
                    GetBeforString(vTopData, vCurItemNo - 1, ref vsBefor);
                }

                // 取光标后
                vText = vCurItem.Text;
                if (GetCharAfter(vTopData.SelectInfo.StartItemOffset + 1, ref vText))
                    vsAfter = vText;
                else
                {
                    vsAfter = vText.Substring(vTopData.SelectInfo.StartItemOffset + 1 - 1, vText.Length - vTopData.SelectInfo.StartItemOffset);
                    GetAfterString(vTopData, vCurItemNo + 1, ref vsAfter);
                }
            }

            FInputHelper.SetCaretString(vsBefor, vsAfter);
        }

        public HCEmrViewIH() : base()
        {
#if GLOBALSHORTKEY
            SetIHKeyHook();  // 调试时可关掉提升效率
#endif
            FInputHelper = new frmInputHelper();
        }

        ~HCEmrViewIH()
        {
#if GLOBALSHORTKEY
            UnSetIHKeyHook();  // 调试时可关掉提升效率
#endif
        }

        public override bool PreProcessMessage(ref Message msg)
        {
            if (FInputHelper.Visible && (msg.Msg == User.WM_KEYDOWN) && (msg.WParam.ToInt32() == Imm.VK_PROCESSKEY))
            {
                uint vVirtualKey = Imm.ImmGetVirtualKey(msg.HWnd);
                if (vVirtualKey - 127 == 59)  // ; 需要设置输入法;号的功能，如二三候选
                {
                    FInputHelper.ActiveEx = !FInputHelper.ActiveEx;
                    return true;
                }
                else
                if ((FInputHelper.ActiveEx) && ((vVirtualKey == 32) || ((vVirtualKey >= 49) && (vVirtualKey <= 57))))
                {
                    User.keybd_event(User.VK_ESCAPE, 0, 0, 0);
                    User.keybd_event(User.VK_ESCAPE, 0, User.KEYEVENTF_KEYUP, 0);
                    if (vVirtualKey == 32)
                        vVirtualKey = 49;

                    string vText = FInputHelper.GetCandiText(vVirtualKey - 49);
                    this.InsertText(vText);
                    return true;
                }
            }

            return base.PreProcessMessage(ref msg);
        }

        public bool InputHelpEnable
        {
            get { return FInputHelper.EnableEx; }
            set { FInputHelper.EnableEx = value; }
        }
    }
}
