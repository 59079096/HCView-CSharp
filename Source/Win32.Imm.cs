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

    public struct CANDIDATELIST
    {
        public uint dwSize;  // 结构的大小（以字节为单位），偏移数组和所有候选字符串
        public uint dwStyle;  // 候选样式
        public uint dwCount;  // 候选字符串数量
        public uint dwSelection;  // 所选候选字符串的索引
        public uint dwPageStart;  // 候选窗口中第一个候选字符串的索引。当用户按PAGE UP和PAGE DOWN键时，这种变化是不同的
        public uint dwPageSize;  // 在候选窗口中显示在一页中的候选字符串数
        public uint[] dwOffset;  // 偏移到第一个候选字符串的开头，相对于此结构的开始。后续字符串的偏移立即跟随该成员，形成一个32位偏移量的数组
    }  // 备注:候选字符串紧跟在dwOffset数组中的最后一个偏移量

    public class Imm
    {
        public const int GCS_COMPSTR = 0x0008;
        public const int GCS_RESULTSTR = 0x0800;
        public const int IME_CMODE_SOFTKBD = 0x80;
        public const int VK_PROCESSKEY = 0xE5;
        [DllImport("imm32.dll", EntryPoint = "ImmGetContext")] public static extern IntPtr ImmGetContext(IntPtr hwnd);

        [DllImport("imm32.dll", EntryPoint = "ImmGetVirtualKey")] public static extern uint ImmGetVirtualKey(IntPtr hwnd);

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
        [DllImport("imm32.dll", EntryPoint = "ImeGetCandidateListCount")] public static extern uint ImeGetCandidateListCount(IntPtr hIMC, IntPtr lpdwListCount);
        [DllImport("imm32.dll", EntryPoint = "ImmGetCandidateList")] public static extern uint ImmGetCandidateList(IntPtr hIMC, uint deIndex, CANDIDATELIST lpCandList, uint dwBufLen);
        [DllImport("imm32.dll")] public static extern bool ImmGetOpenStatus(IntPtr himc);
        [DllImport("imm32.dll")] public static extern bool ImmSetOpenStatus(IntPtr hIMC, bool fOpen);
        [DllImport("imm32.dll")] public static extern void ImmLockIMC(IntPtr hIMC);
    }
}
