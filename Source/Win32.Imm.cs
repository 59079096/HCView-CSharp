using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using HC.Win32;

namespace HC.View
{
    public struct COMPOSITIONFORM
    {
        public uint dwStyle;
        public POINT ptCurrentPos;
        public RECT rcArea;
    }

    public class Imm
    {
        public const int GCS_COMPSTR = 0x0008;
        public const int GCS_RESULTSTR = 0x0800;
        public const int IME_CMODE_SOFTKBD = 0x80;
        [DllImport("imm32.dll", EntryPoint = "ImmGetContext")] public static extern IntPtr ImmGetContext(IntPtr hwnd);

        [DllImport("Imm32.dll", EntryPoint = "ImmAssociateContext")] public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC); 
        [DllImport("imm32.dll", EntryPoint = "ImmGetCompositionFont")]
        public static extern bool ImmGetCompositionFont(IntPtr himc, ref LOGFONT lplogfont);
        [DllImport("imm32.dll", EntryPoint = "ImmSetCompositionFont")]
        public static extern bool ImmSetCompositionFont(IntPtr himc, ref LOGFONT lplogfont);
        [DllImport("imm32.dll", EntryPoint = "ImmGetCompositionWindow")]
        public static extern bool ImmGetCompositionWindow(IntPtr himc, ref COMPOSITIONFORM lpCompForm);
        [DllImport("imm32.dll", EntryPoint = "ImmSetCompositionWindow")]
        public static extern bool ImmSetCompositionWindow(IntPtr himc, ref COMPOSITIONFORM lpCompForm);
        [DllImport("imm32.dll", EntryPoint = "ImmGetConversionStatus")] public static extern bool ImmGetConversionStatus(IntPtr himc, ref int lpdw, ref int lpdw2);
        [DllImport("imm32.dll", EntryPoint = "ImmSetConversionStatus")] public static extern bool ImmSetConversionStatus(IntPtr himc, int dw1, int dw2);
        [DllImport("imm32.dll", EntryPoint = "ImmReleaseContext")] public static extern int ImmReleaseContext(IntPtr hwnd, IntPtr himc);

        [DllImport("imm32.dll", EntryPoint = "ImmGetCompositionString")] public static extern int ImmGetCompositionString(IntPtr hIMC, int dwIndex, byte[] lpBuf, int dwBufLen);
        [DllImport("imm32.dll")] public static extern bool ImmGetOpenStatus(IntPtr himc);
        [DllImport("imm32.dll")] public static extern bool ImmSetOpenStatus(IntPtr hIMC, bool fOpen);
        [DllImport("imm32.dll")] public static extern void ImmLockIMC(IntPtr hIMC);
    }
}
