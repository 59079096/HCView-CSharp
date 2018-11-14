/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                  HCView代码公共单元                   }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using HC.Win32;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace HC.View
{
    public static class HC
    {
        public const int
            /// <summary> 光标在RectItem前面 </summary>
          OffsetBefor = 0,

          /// <summary> 光标在RectItem区域内 </summary>
          OffsetInner = 1,

          /// <summary> 光标在RectItem后面 </summary>
          OffsetAfter = 2,

          MinRowHeight = 20,
          MinColWidth = 20;

        public static Color clActiveBorder = Color.FromArgb(180, 180, 180);
        public static Color clBtnFace = Color.FromArgb(0xF0, 0xF0, 0xF0);
        public static Color clMedGray = Color.FromArgb(0xA0, 0xA0, 0xA4);
        public static Color clMenu = Color.FromArgb(0xF0, 0xF0, 0xF0);
        public static Color clWindow = Color.FromArgb(0xFF, 0xFF, 0xFF);
        public static Color clHighlight = Color.FromArgb(0x33, 0x99, 0xFF);
        public static Color clInfoBk = Color.FromArgb(0xFF, 0xFF, 0xE1);

        public const uint HC_TEXTMAXSIZE = 4294967295;

        public static System.Windows.Forms.Cursor GCursor;

        public const int
              LineSpaceMin = 8,  // 行间距最小值
              PagePadding = 20,  // 节页面显示时之间的间距
              PMSLineHeight = 24,  // 书写范围线的长度
              AnnotationWidth = 200;  // 批注显示区域宽度

        public const string 
            HC_EXCEPTION = "HC异常：",
            HCS_EXCEPTION_NULLTEXT = HC_EXCEPTION + "文本Item的内容出现为空的情况！",
            HCS_EXCEPTION_TEXTOVER = HC_EXCEPTION + "TextItem的内容超出允许的最大字节数4294967295！",
            HCS_EXCEPTION_MEMORYLESS = HC_EXCEPTION + "复制时没有申请到足够的内存",
            //HCS_EXCEPTION_UNACCEPTDATATYPE = HC_EXCEPTION + '不可接受的数据类型！';
            HCS_EXCEPTION_STRINGLENGTHLIMIT = HC_EXCEPTION + "此版本不支持连续不换行样式字符串超过65535",
            HCS_EXCEPTION_VOIDSOURCECELL = HC_EXCEPTION + "源单元格无法再获取源单元格！",

            // 不能在行首的字符
            DontLineFirstChar = @"`-=[]\;'',./~!@#$%^&*()_+{}|:""<>?·－＝【】＼；‘，。、～！＠＃￥％……＆×（）—＋｛｝｜：“《》？°",
            DontLineLastChar = @"/\＼",

            HC_EXT = ".hcf",

            // 1.3 支持浮动对象保存和读取(未处理向下兼容)
            // 1.4 支持表格单元格边框显示属性的保存和读取
            // 1.5 重构行间距的计算方式
            // 1.6 EditItem增加边框属性
            // 1.7 增加了重构后的行间距的存储
            // 1.8 增加了段垂直对齐样式的存储
            // 1.9 重构了颜色的存储方式以便于兼容其他语言生成的文件
            // 2.0 ImageItem存图像时增加图像数据大小的存储以兼容不同语言图像数据的存储方式
            HC_FileVersion = "2.0";

        public const ushort
            HC_FileVersionInt = 20;

        public const byte
            HC_FILELAN = 2;  // 1字节表示使用的编程语言 1:delphi, 2:C#, 3:VC++

        public static bool IsKeyPressWant(KeyPressEventArgs AKey)
        {
            return (AKey.KeyChar >= 32) && (AKey.KeyChar <= 126);
        }

        public static bool IsKeyDownWant(int AKey)
        {
            return ((AKey == User.VK_BACK)
                || (AKey == User.VK_DELETE)
                || (AKey == User.VK_LEFT)
                || (AKey == User.VK_RIGHT)
                || (AKey == User.VK_UP)
                || (AKey == User.VK_DOWN)
                || (AKey == User.VK_RETURN)
                || (AKey == User.VK_HOME)
                || (AKey == User.VK_END)
                || (AKey == User.VK_TAB));
        }

        public static int PosCharHC(Char AChar, string AStr)
        {
            int Result = 0;
            for (int i = 1; i <= AStr.Length; i++)
            {
                if (AChar == AStr[i - 1])
                {
                    Result = i;
                    return Result;
                }
            }

            return Result;
        }

        public static int GetCharOffsetByX(HCCanvas ACanvas, string AText, int X)
        {
            int Result = -1;
            if (X < 0)
                Result = 0;
            else
            if (X > ACanvas.TextWidth(AText))
                Result = AText.Length;
            else
            {
                int vX = 0, vCharWidth = 0;

                for (int i = 1; i <= AText.Length; i++)
                {
                    vCharWidth = ACanvas.TextWidth(AText[i - 1]);
                    vX = vX + vCharWidth;
                    if (vX > X)
                    {
                        if (vX - vCharWidth / 2 > X)
                            Result = i - 1;  // 计为前一个后面
                        else
                            Result = i;

                        break;
                    }
                }
            }

            return Result;
        }

        public static Single GetFontSize(string AFontSize)
        {
            if (AFontSize == "初号")
                return 42;
            else
            if (AFontSize == "小初")
                return 36;
            else
            if (AFontSize == "一号")
                return 26;
            else
            if (AFontSize == "小一")
                return 24;
            else
            if (AFontSize == "二号")
                return 22;
            else
            if (AFontSize == "小二")
                return 18;
            else
            if (AFontSize == "三号")
                return 16;
            else
            if (AFontSize == "小三")
                return 15;
            else
            if (AFontSize == "四号")
                return 14;
            else
            if (AFontSize == "小四")
                return 12;
            else
            if (AFontSize == "五号")
                return 10.5f;
            else
            if (AFontSize == "小五")
                return 9;
            else
            if (AFontSize == "六号")
                return 7.5f;
            else
            if (AFontSize == "小六")
                return 6.5f;
            else
            if (AFontSize == "七号")
                return 5.5f;
            else
            if (AFontSize == "八号")
                return 5;
            else
            {
                float Result = 0;
                if (!float.TryParse(AFontSize, out Result))
                    throw new Exception(HC_EXCEPTION + "计算字号大小出错，无法识别的值：" + AFontSize);
                else
                    return Result;
            }
        }

        public static string GetFontSizeStr(Single AFontSize)
        {
            string Result = "";

            if (AFontSize == 42)
                Result = "初号";
            else
            if (AFontSize == 36)
                Result = "小初";
            else
            if (AFontSize == 26)
                Result = "一号";
            else
            if (AFontSize == 24)
                Result = "小一";
            else
            if (AFontSize == 22)
                Result = "二号";
            else
            if (AFontSize == 18)
                Result = "小二";
            else
            if (AFontSize == 16)
                Result = "三号";
            else
            if (AFontSize == 15)
                Result = "小三";
            else
            if (AFontSize == 14)
                Result = "四号";
            else
            if (AFontSize == 12)
                Result = "小四";
            else
            if (AFontSize == 10.5)
                Result = "五号";
            else
            if (AFontSize == 9)
                Result = "小五";
            else
            if (AFontSize == 7.5)
                Result = "六号";
            else
            if (AFontSize == 6.5)
                Result = "小六";
            else
            if (AFontSize == 5.5)
                Result = "七号";
            else
            if (AFontSize == 5)
                Result = "八号";
            else
                Result = string.Format("#.#", AFontSize);

            return Result;
        }

        public static string GetPaperSizeStr(int APaperSize)
        {
            string Result = "";
            switch (APaperSize)
            {
                case GDI.DMPAPER_A3: 
                    Result = "A3";
                    break;

                case GDI.DMPAPER_A4: 
                    Result = "A4";
                    break;

                case GDI.DMPAPER_A5: 
                    Result = "A5";
                    break;

                case GDI.DMPAPER_B5: 
                    Result = "B5";
                    break;

                default:
                    Result = "自定义";
                    break;
            }

            return Result;
        }

        public static ushort GetVersionAsInteger(string AVersion)
        {
            int vN;
            string vsVer = "";
            for (int i = 1; i <= AVersion.Length; i++)
            {
                if (int.TryParse(AVersion.Substring(i - 1, 1), out vN))
                {
                    vsVer += AVersion.Substring(i - 1, 1);
                }
            }

            return ushort.Parse(vsVer);
        }

        public static void HCLoadTextFromStream(Stream AStream, ref string S)
        {
            ushort vSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            vSize = BitConverter.ToUInt16(vBuffer, 0);

            if (vSize > 0)
            {
                vBuffer = new byte[vSize];
                AStream.Read(vBuffer, 0, vSize);
                S = System.Text.Encoding.Default.GetString(vBuffer);
            }
            else
                S = "";
        }

        /// <summary> 保存文件格式、版本 </summary>
        public static void _SaveFileFormatAndVersion(Stream AStream)
        {
            byte[] vBuffer = System.Text.Encoding.Unicode.GetBytes(HC_EXT);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.Text.Encoding.Unicode.GetBytes(HC_FileVersion);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            AStream.WriteByte(HC_FILELAN); // 使用的编程语言
        }

        /// <summary> 读取文件格式、版本 </summary>
        public static void _LoadFileFormatAndVersion(Stream AStream, ref string AFileFormat, ref ushort AVersion, ref byte ALan)
        {
            byte[] vBuffer = new byte[System.Text.Encoding.Unicode.GetByteCount(HC_EXT)];
            AStream.Read(vBuffer, 0, vBuffer.Length);
            AFileFormat = System.Text.Encoding.Unicode.GetString(vBuffer, 0, vBuffer.Length);

            vBuffer = new byte[System.Text.Encoding.Unicode.GetByteCount(HC_FileVersion)];
            AStream.Read(vBuffer, 0, vBuffer.Length);
            string vFileVersion = System.Text.Encoding.Unicode.GetString(vBuffer, 0, vBuffer.Length);
            AVersion = HC.GetVersionAsInteger(vFileVersion);

            if (AVersion > 19)
                ALan = (byte)AStream.ReadByte();
        }

        public static void SaveColorToStream(System.IO.Stream AStream, Color color)
        {
            AStream.WriteByte(color.A);
            AStream.WriteByte(color.R);
            AStream.WriteByte(color.G);
            AStream.WriteByte(color.B);
        }

        public static void LoadColorFromStream(System.IO.Stream AStream, ref Color color)
        {
            byte A = (byte)AStream.ReadByte();
            byte R = (byte)AStream.ReadByte();
            byte G = (byte)AStream.ReadByte();
            byte B = (byte)AStream.ReadByte();
            color = Color.FromArgb(A, R, G, B);
        }
        
        public static BreakPosition MatchBreak(CharType APrevType, CharType APosType)
        {
            switch (APosType)
            {
                case CharType.jctHZ:
                    {
                        if ((APrevType == CharType.jctZM) || (APrevType == CharType.jctSZ) || (APrevType == CharType.jctHZ))  // 当前位置是汉字，前一个是字母、数字、汉字
                        {
                            return BreakPosition.jbpPrev;
                        }
                    }
                    break;

                case CharType.jctZM:
                    {
                        if ((APrevType != CharType.jctZM) && (APrevType != CharType.jctSZ)) // 当前是字母，前一个不是数字、字母
                        {
                            return BreakPosition.jbpPrev;
                        }
                    }
                    break;

                case CharType.jctSZ:
                    {
                        if ((APrevType != CharType.jctZM) && (APrevType != CharType.jctSZ))  // 当前是数字，前一个不是字母、数字
                        {
                            return BreakPosition.jbpPrev;
                        }
                    }
                    break;

                case CharType.jctFH:
                    {
                        if (APrevType != CharType.jctFH)  // 当前是符号，前一个不是符号
                        {
                            return BreakPosition.jbpPrev;
                        }
                    }
                    break;
            }

            return BreakPosition.jbpNone;
        }

        public static CharType GetCharType(ushort AChar)
        {
            if ((AChar >= 0x4E00) && (AChar <= 0x9FA5))
            {
                return CharType.jctHZ;  // 汉字
            }

            if (   ((AChar >= 0x21) && (AChar <= 0x2F))  // !"#$%&'()*+,-./
                || ((AChar >= 0x3A) && (AChar <= 0x40))  // :;<=>?@
                || ((AChar >= 0x5B) && (AChar <= 0x60))  // [\]^_`
                || ((AChar >= 0x7B) && (AChar <= 0x7E))  // {|}~                
                )
            {
                return CharType.jctFH;
            }

            //0xFF01..0xFF0F,  // ！“＃￥％＆‘（）×＋，－。、

            if ((AChar >= 0x30) && (AChar <= 0x39))
            {
                return CharType.jctSZ;  // 0..9
            }

            if (   ((AChar >= 0x41) && (AChar <= 0x5A))  // A..Z
                || ((AChar >= 0x61) && (AChar <= 0x7A))  // a..z               
                )
            {
                return CharType.jctZM;
            }
               
            return CharType.jctBreak;
        }

        public static bool PtInRect(RECT ARect, POINT APt)
        {
            return PtInRect(ARect, APt.X, APt.Y);
        }

        public static bool PtInRect(RECT ARect, int X, int Y)
        {
            return ((X >= ARect.Left) && (X < ARect.Right) && (Y >= ARect.Top) && (Y < ARect.Bottom));
        }

        public static RECT Bounds(int ALeft, int ATop, int AWidth, int AHeight)
        {
            return new RECT(ALeft, ATop, ALeft + AWidth, ATop + AHeight);
        }

        public static bool IsOdd(int n)
        {
            return (n % 2 == 1) ? true : false;
        }

        public static void OffsetRect(ref RECT ARect, int x, int y)
        {
            ARect.Offset(x, y);
        }

        public static void InflateRect(ref RECT ARect, int x, int y)
        {
            ARect.Inflate(x, y);
        }
    }

    public delegate void HCProcedure();
    public delegate bool HCFunction();

    public enum PageOrientation : byte
    {
        cpoPortrait, cpoLandscape  // 纸张方向：纵像、横向
    }

    public enum ExpressArea : byte
    {
        ceaNone, ceaLeft, ceaTop, ceaRight, ceaBottom  // 公式的区域，仅适用于上下左右格式的
    }

    public enum BorderSide : byte
    {
        cbsLeft = 1, cbsTop = 2, cbsRight = 4, cbsBottom = 8, cbsLTRB = 16, cbsRTLB = 32
    }

    public enum HCViewModel : byte
    {
        [Description("页面视图")]
        vmPage,  // 页面视图，显示页眉、页脚
        [Description("Web视图")]
        vmWeb  // Web视图，不显示页眉、页脚
    }

    public enum SectionArea : byte  // 当前激活的是文档哪一部分
    {
        saHeader, saPage, saFooter
    }

    public enum BreakPosition : byte  // 截断位置
    {
        jbpNone,  // 不截断
        jbpPrev  // 在前一个后面截断
    }

    public enum CharType : byte 
    {
        jctBreak,  //  截断点
        jctHZ,  // 汉字
        jctZM,  // 半角字母
        //jctCNZM,  // 全角字母
        jctSZ,  // 半角数字
        //jctCNSZ,  // 全角数字
        jctFH  // 半角符号
        //jctCNFH   // 全角符号
    }

    public enum PaperSize : byte
    {
        psCustom, ps4A0, ps2A0, psA0, psA1, psA2,
        psA3, psA4, psA5, psA6, psA7, psA8,
        psA9, psA10, psB0, psB1, psB2, psB3,
        psB4, psB5, psB6, psB7, psB8, psB9,
        psB10, psC0, psC1, psC2, psC3, psC4,
        psC5, psC6, psC7, psC8, psC9, psC10,
        psLetter, psLegal, psLedger, psTabloid,
        psStatement, psQuarto, psFoolscap, psFolio,
        psExecutive, psMonarch, psGovernmentLetter,
        psPost, psCrown, psLargePost, psDemy,
        psMedium, psRoyal, psElephant, psDoubleDemy,
        psQuadDemy, psIndexCard3_5, psIndexCard4_6,
        psIndexCard5_8, psInternationalBusinessCard,
        psUSBusinessCard, psEmperor, psAntiquarian,
        psGrandEagle, psDoubleElephant, psAtlas,
        psColombier, psImperial, psDoubleLargePost,
        psPrincess, psCartridge, psSheet, psHalfPost,
        psDoublePost, psSuperRoyal, psCopyDraught,
        psPinchedPost, psSmallFoolscap, psBrief, psPott,
        psPA0, psPA1, psPA2, psPA3, psPA4, psPA5,
        psPA6, psPA7, psPA8, psPA9, psPA10, psF4,
        psA0a, psJISB0, psJISB1, psJISB2, psJISB3,
        psJISB4, psJISB5, psJISB6, psJISB7, psJISB8,
        psJISB9, psJISB10, psJISB11, psJISB12,
        psANSI_A, psANSI_B, psANSI_C, psANSI_D,
        psANSI_E, psArch_A, psArch_B, psArch_C,
        psArch_D, psArch_E, psArch_E1,
        ps16K, ps32K
    }

    public struct HCCaretInfo
    {
        public int X, Y, Height, PageIndex;
        public bool Visible;
    }

    public enum MarkType : byte
    {
        cmtBeg, cmtEnd
    }

    public class HCObject : IDisposable
    {
        public virtual void Dispose()
        {
            //GC.SuppressFinalize(this);
        }
    }

    public class HCCaret
    {
        private int FHeight;

        private IntPtr FOwnHandle;

        protected void SetHeight(int Value)
        {
            if (FHeight != Value)
            {
                FHeight = Value;
                ReCreate();
            }
        }

        public int X, Y;

        //Visible: Boolean;
        public HCCaret(IntPtr AHandle)
        {
            FOwnHandle = AHandle;
            User.CreateCaret(FOwnHandle, IntPtr.Zero, 2, 20);
        }

        ~HCCaret()
        {
            User.DestroyCaret();
            FOwnHandle = IntPtr.Zero;
        }

        public void ReCreate()
        {
            User.DestroyCaret();
            User.CreateCaret(FOwnHandle, IntPtr.Zero, 2, FHeight);
        }

        public void Show(int AX, int  AY)
        {
            ReCreate();
            User.SetCaretPos(AX, AY);
            User.ShowCaret(FOwnHandle);
        }

        public void Show()
        {
            this.Show(X, Y);
        }

        public void Hide()
        {
            User.HideCaret(FOwnHandle);
        }

        public int Height
        {
            get { return FHeight; }
            set { SetHeight(value); }
        }
    }

    public class HCSet : HCObject
    {
        private byte FValue = 0;

        public override void Dispose()
        {
            base.Dispose();
        }

        public bool Contains(byte value)
        {
            return ((FValue & (byte)value) == (byte)value);
        }

        public void InClude(byte value)
        {
            FValue = (byte)(FValue | (byte)value);
        }

        public void ExClude(byte value)
        {
            FValue = (byte)(FValue & ~(byte)value);
        }

        public byte Value 
        {
            get { return FValue; }
            set { FValue = value; }
        }
    }

    public class HCList<T> : List<T> where T : new()
    {
        /// <summary>
        /// 删除事件
        /// </summary>
        public event EventHandler<NListEventArgs<T>> OnDelete = null;
        /// <summary>
        /// 添加事件
        /// </summary>
        public event EventHandler<NListEventArgs<T>> OnInsert = null;

        public new void Add(T item)
        {
            base.Add(item);
            if (OnInsert != null)
            {
                OnInsert.Invoke(this, new NListEventArgs<T>(item, this.Count));
            }
        }

        public new void Insert(int index, T item)
        {
            base.Insert(index, item);
            if (OnInsert != null)
            {
                OnInsert.Invoke(this, new NListEventArgs<T>(item, index));
            }
        }

        public new void Remove(T item)
        {
            Int32 index = base.IndexOf(item);
            base.Remove(item);
            if (OnDelete != null)
            {
                OnDelete.Invoke(this, new NListEventArgs<T>(item, index));
            }
        }
        public new void RemoveAt(Int32 index)
        {
            T item = base[index];
            base.RemoveAt(index);
            if (OnDelete != null)
            {
                OnDelete.Invoke(this, new NListEventArgs<T>(item, index));
            }
        }

        public new void RemoveRange(int index, int count)
        {
            for (int i = index; i < index + count; i++)
            {
                this.RemoveAt(i);
            }
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            Int32 Index = base.Count;
            base.AddRange(collection);
            foreach (var item in collection)
            {
                if (OnInsert != null)
                {
                    OnInsert.Invoke(this, new NListEventArgs<T>(item, Index));
                }
                Index++;
            }
        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            base.InsertRange(index, collection);
            foreach (var item in collection)
            {
                if (OnInsert != null)
                {
                    OnInsert.Invoke(this, new NListEventArgs<T>(item, index));
                }
                index++;
            }
        }
    }

    public class NListEventArgs<T> : EventArgs
    {
        public NListEventArgs(T item, Int32 index)
        {
            Item = item;
            Index = index;
        }
        public T Item { get; set; }
        public Int32 Index { get; set; }
    }

    public class HCInhList<T> : List<T> where T : new()
    {
        /// <summary>
        /// 删除事件
        /// </summary>
        public event EventHandler<NListInhEventArgs<T>> OnDelete = null;
        /// <summary>
        /// 添加事件
        /// </summary>
        public event EventHandler<NListInhEventArgs<T>> OnInsert = null;

        public new void Add(T item)
        {
            if (OnInsert != null)
            {
                NListInhEventArgs<T> vArgs = new NListInhEventArgs<T>(item, this.Count);
                OnInsert.Invoke(this, vArgs);
                if (vArgs.Inherited)
                    base.Add(item);
            }
            else
                base.Add(item);
        }

        public new void Insert(int index, T item)
        {
            if (OnInsert != null)
            {
                NListInhEventArgs<T> vArgs = new NListInhEventArgs<T>(item, index);
                OnInsert.Invoke(this, vArgs);
                if (vArgs.Inherited)
                    base.Insert(index, item);
            }
            else
                base.Insert(index, item);
        }

        public new void Remove(T item)
        {
            Int32 index = base.IndexOf(item);

            if (OnDelete != null)
            {
                NListInhEventArgs<T> vArgs = new NListInhEventArgs<T>(item, index);
                OnDelete.Invoke(this, vArgs);
                if (vArgs.Inherited)
                    base.Remove(item);
            }
            else
                base.Remove(item);
        }

        public new void RemoveAt(Int32 index)
        {
            T item = base[index];

            if (OnDelete != null)
            {
                NListInhEventArgs<T> vArgs = new NListInhEventArgs<T>(item, index);
                OnDelete.Invoke(this, vArgs);
                if (vArgs.Inherited)
                    base.RemoveAt(index);
            }
            else
                base.RemoveAt(index);
        }

        public new void RemoveRange(int index, int count)
        {
            if (count > 0)
            {
                int vEndIndex = index + count - 1;
                if (vEndIndex > this.Count - 1)
                    vEndIndex = this.Count - 1;

                for (int i = vEndIndex; i >= index; i--)
                {
                    this.RemoveAt(i);
                }
            }
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            Int32 index = base.Count;
            if (OnInsert != null)
            {
                foreach (var item in collection)
                {
                    NListInhEventArgs<T> vArgs = new NListInhEventArgs<T>(item, index);
                    OnInsert.Invoke(this, vArgs);
                    if (vArgs.Inherited)
                        base.Insert(index, item);

                    index++;
                }
            }
            else
                base.AddRange(collection);

        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            if (OnInsert != null)
            {
                foreach (var item in collection)
                {
                    NListInhEventArgs<T> vArgs = new NListInhEventArgs<T>(item, index);
                    OnInsert.Invoke(this, vArgs);
                    if (vArgs.Inherited)
                        base.Insert(index, item);

                    index++;
                }
            }
            else
                base.InsertRange(index, collection);
        }
    }

    public class NListInhEventArgs<T> : EventArgs
    {
        private bool FInherited = true;

        public NListInhEventArgs(T item, Int32 index)
        {
            Item = item;
            Index = index;
        }
        public T Item { get; set; }
        public Int32 Index { get; set; }
        public bool Inherited 
        {
            get { return FInherited; }
            set { FInherited = value; }
        }
    }
}
