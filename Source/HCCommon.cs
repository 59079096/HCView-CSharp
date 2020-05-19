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
        public static Color AnnotateBKColor = Color.FromArgb(0xFF, 0xD5, 0xD5);
        public static Color AnnotateBKActiveColor = Color.FromArgb(0xA8, 0xA8, 0xFF);
        public static Color HyperTextColor = Color.FromArgb(0x05, 0x63, 0xC1);
        public static Color HCTransparentColor = Color.Transparent;  // 透明色
        //public static char[] HCBoolText = { '0', '1' };

        public const uint HC_TEXTMAXSIZE = 4294967295;

        public static System.Windows.Forms.Cursor GCursor;

        public const byte
            HC_PROGRAMLANGUAGE = 2,  // 1字节表示使用的编程语言 1:delphi, 2:C#, 3:VC++, 4:HTML5
            TabCharWidth = 28;  // 默认Tab宽度(五号) 14 * 2个

        public const int
              DefaultColWidth = 50,
              PMSLineHeight = 24,  // 书写范围线的长度
              AnnotationWidth = 200,  // 批注显示区域宽度
              DMPAPER_HC_16K = -1000;

        public const string 
            HC_EXCEPTION = "HC异常：",
            HCS_EXCEPTION_NULLTEXT = HC_EXCEPTION + "文本Item的内容出现为空的情况！",
            HCS_EXCEPTION_TEXTOVER = HC_EXCEPTION + "TextItem的内容超出允许的最大字节数4294967295！",
            HCS_EXCEPTION_MEMORYLESS = HC_EXCEPTION + "复制时没有申请到足够的内存",
            //HCS_EXCEPTION_UNACCEPTDATATYPE = HC_EXCEPTION + '不可接受的数据类型！';
            //HCS_EXCEPTION_STRINGLENGTHLIMIT = HC_EXCEPTION + "此版本不支持连续不换行样式字符串超过65535",
            HCS_EXCEPTION_VOIDSOURCECELL = HC_EXCEPTION + "源单元格无法再获取源单元格！",
            HCS_EXCEPTION_TIMERRESOURCEOUTOF = HC_EXCEPTION + "安装计时器的资源不足！",

            #if UNPLACEHOLDERCHAR
            UnPlaceholderChar = "\u0F74\u0F7A\u0F7C\u0F72"
                + "\u0FB8\u0F7E\u0F83\u0F37\u0F35\u0F7F\u0FB7\u0FBA\u0F95"
                + "\u0F96\u0F7B\u0FB2\u0F9F\u0FB1\u0FAD\u0F80\u0F7D\u0FA5"
                + "\u0FA9\u0FAA\u0FAB\u0FB0\u0FB6\u0FA1\u0FA6\u0F94\u0FA8"
                + "\u0F84\u0F92\u0F92\u0FAE\u0FAF\u0FB4\u0F90\u0F91\u0FA4"
                + "\u0FA3\u0FA0\u0F97\u0F99\u0FBC\u0FBB\u0F19\u0F71\u0F3E"
                + "\u0F3F\u0F87\u0F86\u0F76\u0F77\u0F78\u0F79\u0F73\u0F9A"
                + "\u0F75\u0F73\u0F9C\u0FC6\u0FB5\u0FB9\u0F82\u0F9E\u0F9B",
            #endif
            // 不能在行首的字符
            DontLineFirstChar = @"`-=[]\;,./~!@#$%^&*()_+{}|:""<>?·－＝【】＼；’，。、～！＠＃￥％……＆×（）——＋｛｝｜：”《》？°"
            #if UNPLACEHOLDERCHAR
                + UnPlaceholderChar
            #endif
                ,
            DontLineLastChar = @"/\＼“‘",
            /// <summary> 可以挤压宽度的字符 </summary>
            LineSqueezeChar = "，。；、？“”",
            sLineBreak = "\r\n",
            HC_EXT = ".hcf",
            HC_EXT_DOCX = ".docx",

            // 1.3 支持浮动对象保存和读取(未处理向下兼容)
            // 1.4 支持表格单元格边框显示属性的保存和读取
            // 1.5 重构行间距的计算方式
            // 1.6 EditItem增加边框属性
            // 1.7 增加了重构后的行间距的存储
            // 1.8 增加了段垂直对齐样式的存储
            // 1.9 重构了颜色的存储方式以便于兼容其他语言生成的文件
            // 2.0 ImageItem存图像时增加图像数据大小的存储以兼容不同语言图像数据的存储方式
            // 2.1 GifImage保存读取改用兼容其他语言的方式
            // 2.2 增加段缩进的存储
            // 2.3 增加批注的保存和读取
            // 2.4 兼容EmrView保存保护元素属性
            // 2.5 使用unicode字符集保存文档以便支持藏文等
            // 2.6 文件保存时直接使用TItemOptions集合变量的值，不再单独判断成员存储
            // 2.7 浮动直线改为ShapeLine
            // 2.8 浮动Item都使用HCStyle的样式定义(负数)，这样便于统一按Item处理遍历等操作
            // 2.9 浮动Item保存PageIndex，原因见 20190906001
            // 3.0 表格增加边框宽度的存储
            // 3.1 增加行间距 最小值、固定值、多倍的存储
            // 3.2 表格边框改用磅为单位、段样式增加BreakRough处理截断、兼容EmrView使用TDeImageItem类处理ImageItem
            // 3.3 兼容32版本图片保存时没有按DeImageItem保存，读取时不正确的问题
            // 3.4 RadioGroun控件保存选项样式、保存文件所用的排版算法版本
            // 3.5 数据元增加DeleteProtect控制是否能删除掉整个数据元，表格存储CellPadding，FloatBarCode存储单线条宽度
            // 3.6 Combobox和RadioGrou的选项改为键值对的形式
            // 3.7 兼容Combobox无下拉选项时保存选项后打不开的问题
            // 3.8 浮动Item增加Lock属性用于锁定Item不可移动和修改
            // 3.9 域Item保存时存Level

            HC_FileVersion = "3.9";

        public const ushort
            HC_FileVersionInt = 39;

        private static DataFormats.Format hcExtFormat = null;
        public static DataFormats.Format HCExtFormat
        {
            get { return hcExtFormat; }
            set { hcExtFormat = value; }
        }

        public static ushort SwapBytes(ushort aValue)
        {
            return (ushort)((aValue >> 8) | ((ushort)(aValue << 8)));
        }

        public static bool IsKeyPressWant(KeyPressEventArgs aKey)
        {
            #if UNPLACEHOLDERCHAR
            return ((aKey.KeyChar >= 32) && (aKey.KeyChar <= 126))
                || ((aKey.KeyChar >= 3840) && (aKey.KeyChar <= 4095))  // 藏文
                || ((aKey.KeyChar >= 6144) && (aKey.KeyChar <= 6319));  // 蒙古文
            #else
            return (aKey.KeyChar >= 32) && (aKey.KeyChar <= 126);
            #endif
        }

        public static bool IsKeyDownWant(int aKey)
        {
            return ((aKey == User.VK_BACK)
                || (aKey == User.VK_DELETE)
                || (aKey == User.VK_LEFT)
                || (aKey == User.VK_RIGHT)
                || (aKey == User.VK_UP)
                || (aKey == User.VK_DOWN)
                || (aKey == User.VK_RETURN)
                || (aKey == User.VK_HOME)
                || (aKey == User.VK_END)
                || (aKey == User.VK_TAB));
        }

        public static bool IsKeyDownEdit(int aKey)
        {
            return ((aKey == User.VK_BACK)
                || (aKey == User.VK_DELETE)
                || (aKey == User.VK_RETURN)
                || (aKey == User.VK_TAB));
        }

        public static bool IsDirectionKey(int aKey)
        {
            return ((aKey == User.VK_LEFT)
                || (aKey == User.VK_UP)
                || (aKey == User.VK_RIGHT)
                || (aKey == User.VK_DOWN));
        }

        public static IntPtr CreateExtPen(HCPen aPen)
        {
            LOGBRUSH vPenParams = new LOGBRUSH();

            switch (aPen.Style)
            {
                case HCPenStyle.psSolid:
                case HCPenStyle.psInsideFrame:
                    vPenParams.lbStyle = GDI.PS_SOLID;
                    break;

                case HCPenStyle.psDash:
                    vPenParams.lbStyle = GDI.PS_DASH;
                    break;

                case HCPenStyle.psDot:
                    vPenParams.lbStyle = GDI.PS_DOT;
                    break;

                case HCPenStyle.psDashDot:
                    vPenParams.lbStyle = GDI.PS_DASHDOT;
                    break;

                case HCPenStyle.psDashDotDot:
                    vPenParams.lbStyle = GDI.PS_DASHDOTDOT;
                    break;

                case HCPenStyle.psClear:
                    vPenParams.lbStyle = GDI.PS_NULL;
                    break;

                default:
                    vPenParams.lbStyle = GDI.PS_SOLID;
                    break;
            }

            vPenParams.lbColor = aPen.Color.ToRGB_UInt();
            vPenParams.lbHatch = 0;

            //if (aPen.Width != 1)
                return (IntPtr)(GDI.ExtCreatePen(GDI.PS_GEOMETRIC | GDI.PS_ENDCAP_SQUARE | GDI.PS_JOIN_MITER, aPen.Width, ref vPenParams, 0, IntPtr.Zero));
            //else
            //    return (IntPtr)(GDI.ExtCreatePen(GDI.PS_COSMETIC | GDI.PS_ENDCAP_SQUARE, aPen.Width, ref vPenParams, 0, IntPtr.Zero));
        }

        public static int PosCharHC(Char aChar, string aStr)
        {
            int Result = 0;
            for (int i = 1; i <= aStr.Length; i++)
            {
                if (aChar == aStr[i - 1])
                {
                    Result = i;
                    return Result;
                }
            }

            return Result;
        }

        #if UNPLACEHOLDERCHAR
        public static bool IsUnPlaceHolderChar(char aChar)
        {
            return HC.UnPlaceholderChar.IndexOf(aChar) >= 0;
        }

        public static int GetTextActualOffset(string aText, int aOffset, bool aAfter = false)
        {
            int Result = aOffset;

            int vLen = aText.Length;
            if (aAfter)
            {
                while (Result < vLen)
                {
                    if (HC.UnPlaceholderChar.IndexOf(aText[Result + 1 - 1]) >= 0)
                        Result++;
                    else
                        break;
                }
            }
            else
            {
                while (Result > 1)
                {
                    if (HC.UnPlaceholderChar.IndexOf(aText[Result - 1]) >= 0)
                        Result--;
                    else
                        break;
                }
            }

            return Result;
        }

        public static int GetCharHalfFarfrom(string aText, int aOffset, int[] aCharWArr)
        {
            int Result = 0;

            int vEndOffs = GetTextActualOffset(aText, aOffset, true);
            int vBeginOffs = GetTextActualOffset(aText, aOffset) - 1;

            if (vBeginOffs > 0)
            {
                if (vEndOffs == vBeginOffs)
                {
                    if (vBeginOffs > 1)
                        Result = aCharWArr[vBeginOffs - 2] + ((aCharWArr[vEndOffs - 1] - aCharWArr[vBeginOffs - 2]) / 2);
                    else
                        Result = aCharWArr[vBeginOffs - 1] / 2;
                }
                else
                    Result = aCharWArr[vBeginOffs - 1] + ((aCharWArr[vEndOffs - 1] - aCharWArr[vBeginOffs - 1]) / 2);
            }
            else
                Result = aCharWArr[vEndOffs - 1] / 2;

            return Result;
        }
        #else
        public static int GetCharHalfFarfrom(int aOffset, int[] aCharWArr)
        {
            int Result = 0;

            if (aOffset > 1)
                Result = aCharWArr[aOffset - 2] + ((aCharWArr[aOffset - 1] - aCharWArr[aOffset - 2]) / 2);
            else
            if (aOffset == 1)
                Result = aCharWArr[aOffset - 1] / 2;

            return Result;
        }
        #endif

        public static int GetNorAlignCharOffsetAt(HCCanvas aCanvas, string aText, int x)
        {
            int Result = -1;

            if (x < 0)
                Result = 0;
            else
            {
                int vLen = aText.Length;
                int[] vCharWArr = new int[vLen];
                SIZE vSize = new SIZE(0, 0);
                aCanvas.GetTextExtentExPoint(aText, vLen, vCharWArr, ref vSize);

                if (x > vSize.cx)
                    Result = vLen;
                else
                {
                    int i = 1;
                    while (i <= vLen)
                    {
                        #if UNPLACEHOLDERCHAR
                        i = HC.GetTextActualOffset(aText, i, true);
                        #endif

                        if (x == vCharWArr[i - 1])
                        {
                            Result = i;
                            break;
                        }
                        else
                        if (x > vCharWArr[i - 1])
                            i++;
                        else
                        {
                            if (x > HC.GetCharHalfFarfrom(
                                #if UNPLACEHOLDERCHAR
                                aText,
                                #endif
                                i, vCharWArr))
                            {
                                Result = i;
                            }
                            else
                            {
                                #if UNPLACEHOLDERCHAR
                                Result = HC.GetTextActualOffset(aText, i) - 1;
                                #else
                                Result = i - 1;
                                #endif
                            }

                            break;
                        }
                    }
                }
            }

            return Result;
        }

        public static Single GetFontSize(string aFontSize)
        {
            if (aFontSize == "初号")
                return 42;
            else
            if (aFontSize == "小初")
                return 36;
            else
            if (aFontSize == "一号")
                return 26;
            else
            if (aFontSize == "小一")
                return 24;
            else
            if (aFontSize == "二号")
                return 22;
            else
            if (aFontSize == "小二")
                return 18;
            else
            if (aFontSize == "三号")
                return 16;
            else
            if (aFontSize == "小三")
                return 15;
            else
            if (aFontSize == "四号")
                return 14;
            else
            if (aFontSize == "小四")
                return 12;
            else
            if (aFontSize == "五号")
                return 10.5f;
            else
            if (aFontSize == "小五")
                return 9;
            else
            if (aFontSize == "六号")
                return 7.5f;
            else
            if (aFontSize == "小六")
                return 6.5f;
            else
            if (aFontSize == "七号")
                return 5.5f;
            else
            if (aFontSize == "八号")
                return 5;
            else
            {
                float Result = 0;
                if (!float.TryParse(aFontSize, out Result))
                    throw new Exception(HC_EXCEPTION + "计算字号大小出错，无法识别的值：" + aFontSize);
                else
                    return Result;
            }
        }

        public static string GetFontSizeStr(Single aFontSize)
        {
            string Result = "";

            if (aFontSize == 42)
                Result = "初号";
            else
            if (aFontSize == 36)
                Result = "小初";
            else
            if (aFontSize == 26)
                Result = "一号";
            else
            if (aFontSize == 24)
                Result = "小一";
            else
            if (aFontSize == 22)
                Result = "二号";
            else
            if (aFontSize == 18)
                Result = "小二";
            else
            if (aFontSize == 16)
                Result = "三号";
            else
            if (aFontSize == 15)
                Result = "小三";
            else
            if (aFontSize == 14)
                Result = "四号";
            else
            if (aFontSize == 12)
                Result = "小四";
            else
            if (aFontSize == 10.5)
                Result = "五号";
            else
            if (aFontSize == 9)
                Result = "小五";
            else
            if (aFontSize == 7.5)
                Result = "六号";
            else
            if (aFontSize == 6.5)
                Result = "小六";
            else
            if (aFontSize == 5.5)
                Result = "七号";
            else
            if (aFontSize == 5)
                Result = "八号";
            else
                Result = string.Format("{0:0.#}", aFontSize);

            return Result;
        }

        public static string GetPaperSizeStr(int aPaperSize)
        {
            string Result = "";
            switch (aPaperSize)
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

        public static ushort GetVersionAsInteger(string aVersion)
        {
            int vN;
            string vsVer = "";
            for (int i = 1; i <= aVersion.Length; i++)
            {
                if (int.TryParse(aVersion.Substring(i - 1, 1), out vN))
                {
                    vsVer += aVersion.Substring(i - 1, 1);
                }
            }

            return ushort.Parse(vsVer);
        }

        public static void HCSaveTextToStream(Stream aStream, string s)
        {
            #if UNPLACEHOLDERCHAR
            int vLen = System.Text.Encoding.Unicode.GetByteCount(s);
            #else
            int vLen = System.Text.Encoding.Default.GetByteCount(s);
            #endif
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            ushort vSize = (ushort)vLen;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            aStream.Write(vBuffer, 0, vBuffer.Length);
            if (vSize > 0)
            {
                vBuffer = System.Text.Encoding.Unicode.GetBytes(s);
                aStream.Write(vBuffer, 0, vBuffer.Length);
            }
        }

        public static void HCLoadTextFromStream(Stream aStream, ref string s, ushort aFileVersion)
        {
            ushort vSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vSize = BitConverter.ToUInt16(vBuffer, 0);

            if (vSize > 0)
            {
                vBuffer = new byte[vSize];
                aStream.Read(vBuffer, 0, vSize);
                #if UNPLACEHOLDERCHAR
                if (aFileVersion > 24)
                    s = System.Text.Encoding.Unicode.GetString(vBuffer);
                else
                    s = System.Text.Encoding.Default.GetString(vBuffer);
                #else
                s = System.Text.Encoding.Default.GetString(vBuffer);
                #endif
            }
            else
                s = "";
        }

        /// <summary> 保存文件格式、版本 </summary>
        public static void _SaveFileFormatAndVersion(Stream aStream)
        {
            byte[] vBuffer = System.Text.Encoding.Unicode.GetBytes(HC_EXT);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.Text.Encoding.Unicode.GetBytes(HC_FileVersion);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            aStream.WriteByte(HC.HC_PROGRAMLANGUAGE); // 使用的编程语言
        }

        /// <summary> 读取文件格式、版本 </summary>
        public static void _LoadFileFormatAndVersion(Stream aStream, ref string aFileFormat, ref ushort aVersion, ref byte aLang)
        {
            byte[] vBuffer = new byte[System.Text.Encoding.Unicode.GetByteCount(HC_EXT)];
            aStream.Read(vBuffer, 0, vBuffer.Length);
            aFileFormat = System.Text.Encoding.Unicode.GetString(vBuffer, 0, vBuffer.Length);

            vBuffer = new byte[System.Text.Encoding.Unicode.GetByteCount(HC_FileVersion)];
            aStream.Read(vBuffer, 0, vBuffer.Length);
            string vFileVersion = System.Text.Encoding.Unicode.GetString(vBuffer, 0, vBuffer.Length);
            aVersion = HC.GetVersionAsInteger(vFileVersion);

            if (aVersion > 19)
                aLang = (byte)aStream.ReadByte();
        }

        public static void HCSaveColorToStream(System.IO.Stream aStream, Color color)
        {
            aStream.WriteByte(color.A);
            aStream.WriteByte(color.R);
            aStream.WriteByte(color.G);
            aStream.WriteByte(color.B);
        }

        public static void HCLoadColorFromStream(System.IO.Stream aStream, ref Color color)
        {
            // to do:不透明时要将argb转换为rgb
            byte A = (byte)aStream.ReadByte();
            byte R = (byte)aStream.ReadByte();
            byte G = (byte)aStream.ReadByte();
            byte B = (byte)aStream.ReadByte();
            color = Color.FromArgb(A, R, G, B);
        }

        public static Color GetXmlRGBColor(string aColorStr)
        {
            string[] vsRGB = aColorStr.Split(new string[] { "," }, StringSplitOptions.None);
            if (vsRGB.Length > 3)
            {
                if (vsRGB[0] == "0")
                    return HCTransparentColor;
                else
                    return Color.FromArgb(byte.Parse(vsRGB[1]), byte.Parse(vsRGB[2]), byte.Parse(vsRGB[3]));
            }
            else
                return Color.FromArgb(byte.Parse(vsRGB[0]), byte.Parse(vsRGB[1]), byte.Parse(vsRGB[2]));
        }

        public static string GetColorXmlRGB(Color aColor)
        {
            if (aColor == HCTransparentColor)
                return "0,255,255,255";
            else
                return string.Format("255,{0},{1},{2}", aColor.R, aColor.G, aColor.B);
        }

        public static string GetXmlRN(string aText)
        {
            return aText.Replace(((Char)10).ToString(), "\r\n");
        }

        public static void SetBorderSideByPro(string aValue, HCBorderSides aBorderSides)
        {
            aBorderSides.Value = 0;
            string[] vStrings = aValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < vStrings.Length; i++)
            {
                if (vStrings[i] == "left")
                    aBorderSides.InClude((byte)BorderSide.cbsLeft);
                else
                if (vStrings[i] == "top")
                    aBorderSides.InClude((byte)BorderSide.cbsTop);
                else
                if (vStrings[i] == "right")
                    aBorderSides.InClude((byte)BorderSide.cbsRight);
                else
                if (vStrings[i] == "bottom")
                    aBorderSides.InClude((byte)BorderSide.cbsBottom);
                else
                if (vStrings[i] == "ltrb")
                    aBorderSides.InClude((byte)BorderSide.cbsLTRB);
                else
                if (vStrings[i] == "rtlb")
                    aBorderSides.InClude((byte)BorderSide.cbsRTLB);
            }
        }

        public static string GetBorderSidePro(HCBorderSides aBorderSides)
        {
            string Result = "";
            if (aBorderSides.Contains((byte)BorderSide.cbsLeft))
                Result = "left";

            if (aBorderSides.Contains((byte)BorderSide.cbsTop))
            {
                if (Result != "")
                    Result = Result + ",top";
                else
                    Result = "top";
            }

            if (aBorderSides.Contains((byte)BorderSide.cbsRight))
            {
                if (Result != "")
                    Result = Result + ",right";
                else
                    Result = "right";
            }

            if (aBorderSides.Contains((byte)BorderSide.cbsBottom))
            {
                if (Result != "")
                    Result = Result + ",bottom";
                else
                    Result = "bottom";
            }

            if (aBorderSides.Contains((byte)BorderSide.cbsLTRB))
            {
                if (Result != "")
                    Result = Result + ",ltrb";
                else
                    Result = "ltrb";
            }

            if (aBorderSides.Contains((byte)BorderSide.cbsRTLB))
            {
                if (Result != "")
                    Result = Result + ",rtlb";
                else
                    Result = "rtlb";
            }

            return Result;
        }

        public static string GraphicToBase64(Image aGraphic, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream vStream = new MemoryStream())
            {
                using (Bitmap bitmap = new Bitmap(aGraphic))  // 解决GDI+ 中发生一般性错误，因为该文件仍保留锁定对于对象的生存期
                {
                    bitmap.Save(vStream, format);  //  System.Drawing.Imaging.ImageFormat.Bmp
                }

                byte[] vArr = new byte[vStream.Length];
                vStream.Position = 0;
                vStream.Read(vArr, 0, (int)vStream.Length);
                vStream.Close();
                vStream.Dispose();
                return Convert.ToBase64String(vArr);
            }
        }

        public static Image Base64ToGraphic(string aBase64)
        {
            byte[] vArr = Convert.FromBase64String(aBase64);
            using (MemoryStream vStream = new MemoryStream(vArr))
            {
                return Image.FromStream(vStream);
            }
        }

        public static string HCDeleteBreak(string s)
        {
            return s.Replace(sLineBreak, "");
        }

        public static CharType GetUnicodeCharType(uint aChar)
        {
            if ((aChar >= 0x2E80) && (aChar <= 0x2EF3)  // 部首扩展 115
                || (aChar >= 0x2F00) && (aChar <= 0x2FD5)  // 熙部首 214
                || (aChar >= 0x2FF0) && (aChar <= 0x2FFB)  // 汉字结构 12
                || (aChar == 0x3007)  // 〇 1
                || (aChar >= 0x3105) && (aChar <= 0x312F)  // 汉字注音 43
                || (aChar >= 0x31A0) && (aChar <= 0x31BA)  // 注音扩展 22
                || (aChar >= 0x31C0) && (aChar <= 0x31E3)  // 汉字笔划 36
                || (aChar >= 0x3400) && (aChar <= 0x4DB5)  // 扩展A 6582个
                || (aChar >= 0x4E00) && (aChar <= 0x9FA5)  // 基本汉字 20902个
                || (aChar >= 0x9FA6) && (aChar <= 0x9FEF)  // 基本汉字补充 74个
                || (aChar >= 0xE400) && (aChar <= 0xE5E8)  // 部件扩展 452
                || (aChar >= 0xE600) && (aChar <= 0xE6CF)  // PUA增补 207
                || (aChar >= 0xE815) && (aChar <= 0xE86F)  // PUA(GBK)部件 81
                || (aChar >= 0xF900) && (aChar <= 0xFAD9)  // 兼容汉字 477
                || (aChar >= 0x20000) && (aChar <= 0x2A6D6)  // 扩展B 42711个
                || (aChar >= 0x2A700) && (aChar <= 0x2B734)  // 扩展C 4149
                || (aChar >= 0x2B740) && (aChar <= 0x2B81D)  // 扩展D 222
                || (aChar >= 0x2B820) && (aChar <= 0x2CEA1)  // 扩展E 5762
                || (aChar >= 0x2CEB0) && (aChar <= 0x2EBE0)  // 扩展F 7473
                || (aChar >= 0x2F800) && (aChar <= 0x2FA1D)  // 兼容扩展 542
                )
                return CharType.jctHZ;  // 汉字

            if ((aChar >= 0x0F00) && (aChar <= 0x0FFF))
                return CharType.jctHZ;  // 汉字，藏文

            if ((aChar >= 0x1800) && (aChar <= 0x18AF))
                return CharType.jctHZ;  // 汉字，蒙古字符

            if (   ((aChar >= 0x21) && (aChar <= 0x2F))  // !"#$%&'()*+,-./
                || ((aChar >= 0x3A) && (aChar <= 0x40))  // :;<=>?@
                || ((aChar >= 0x5B) && (aChar <= 0x60))  // [\]^_`
                || ((aChar >= 0x7B) && (aChar <= 0x7E))  // {|}~      
                || (aChar == 0xFFE0)  // ￠
                )
            {
                return CharType.jctFH;
            }

            //0xFF01..0xFF0F,  // ！“＃￥％＆‘（）×＋，－。、

            if ((aChar >= 0x30) && (aChar <= 0x39))
            {
                return CharType.jctSZ;  // 0..9
            }

            if (   ((aChar >= 0x41) && (aChar <= 0x5A))  // A..Z
                || ((aChar >= 0x61) && (aChar <= 0x7A))  // a..z               
                )
            {
                return CharType.jctZM;
            }
               
            return CharType.jctBreak;
        }

        public static bool PtInRect(RECT aRect, POINT aPt)
        {
            return PtInRect(aRect, aPt.X, aPt.Y);
        }

        public static bool PtInRect(RECT aRect, int x, int y)
        {
            return ((x >= aRect.Left) && (x < aRect.Right) && (y >= aRect.Top) && (y < aRect.Bottom));
        }

        public static RECT Bounds(int aLeft, int aTop, int aWidth, int aHeight)
        {
            return new RECT(aLeft, aTop, aLeft + aWidth, aTop + aHeight);
        }

        public static bool IsOdd(int n)
        {
            //return (n % 2 == 1) ? true : false;
            return (n & 1) == 1 ? true : false;
        }

        public static void OffsetRect(ref RECT aRect, int x, int y)
        {
            aRect.Offset(x, y);
        }

        public static void InflateRect(ref RECT ARect, int x, int y)
        {
            ARect.Inflate(x, y);
        }

        public static void HCDrawArrow(HCCanvas canvas, Color color, int left, int top, byte type)
        {
            switch (type)
            {
                case 0:  // 上
                    canvas.Pen.Color = color;
                    canvas.MoveTo(left, top);
                    canvas.LineTo(left - 1, top);
                    canvas.MoveTo(left - 1, top + 1);
                    canvas.LineTo(left + 2, top + 1);
                    canvas.MoveTo(left - 2, top + 2);
                    canvas.LineTo(left + 3, top + 2);
                    canvas.MoveTo(left - 3, top + 3);
                    canvas.LineTo(left + 4, top + 3);
                    canvas.MoveTo(left - 4, top + 4);
                    canvas.LineTo(left + 5, top + 4);
                    break;

                case 1:  // 下
                    canvas.Pen.Color = color;
                    canvas.MoveTo(left, top);
                    canvas.LineTo(left - 1, top);
                    canvas.MoveTo(left - 1, top - 1);
                    canvas.LineTo(left + 2, top - 1);
                    canvas.MoveTo(left - 2, top - 2);
                    canvas.LineTo(left + 3, top - 2);
                    canvas.MoveTo(left - 3, top - 3);
                    canvas.LineTo(left + 4, top - 3);
                    canvas.MoveTo(left - 4, top - 4);
                    canvas.LineTo(left + 5, top - 4);
                    break;

                case 2:  // 左
                    break;

                case 3:  // 右
                    break;
            }
        }
    }

    public delegate void HCProcedure();
    public delegate bool HCFunction();

    public enum PaperOrientation : byte  // 纸张方向
    {
        cpoPortrait = 0,  // 纵向
        cpoLandscape = 1  // 横向
    }

    public enum ExpressArea : byte
    {
        ceaNone = 0, 
        ceaLeft = 1, 
        ceaTop = 2, 
        ceaRight = 3, 
        ceaBottom = 4  // 公式的区域，仅适用于上下左右格式的
    }

    public enum BorderSide : byte
    {
        cbsLeft = 1, cbsTop = 1 << 1, cbsRight = 1 << 2, cbsBottom = 1 << 3, cbsLTRB = 1 << 4, cbsRTLB = 1 << 5
    }

    public class HCBorderSides : HCSet { }

    public enum SectionArea : byte  // 当前激活的是文档哪一部分
    {
        saHeader = 1, 
        saPage = 1 << 1, 
        saFooter = 1 << 2
    }

    public enum BreakPosition : byte  // 截断位置
    {
        jbpNone,  // 不截断
        jbpPrev  // 在前一个后面截断
    }

    public enum HCContentAlign : byte   // 表格单元格对齐方式
    {
        tcaTopLeft, tcaTopCenter, tcaTopRight, tcaCenterLeft, tcaCenterCenter, 
        tcaCenterRight, tcaBottomLeft, tcaBottomCenter, tcaBottomRight
    }

    public enum HCState : byte
    {
        hosLoading,  // 文档加载
        hosCopying,  // 复制
        hosPasting,  // 粘贴
        hosUndoing,
        hosRedoing,
        hosBatchInsert,  // 调用InsertItem批量插入多个Item时(如数据组批量插入2个)防止别的操作引起位置变化导致后面插入位置不正确
        hosDestroying  // 编辑器在销毁中
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

    public enum HCAction : byte
    {
        actBackDeleteText,  // 向前删除文本
        actDeleteText,  // 向后删除文本
        actInsertText,  // 插入文本
        actReturnItem,  // 在Item上回车
        actSetItemText, // 直接赋值Item的Text
        actDeleteItem,  // 删除Item
        actInsertItem,  // 插入Item
        actItemProperty,  // Item属性变化
        actItemSelf,  // Item自己管理
        actItemMirror,  // Item镜像
        actConcatText  // 粘接文本(两头)
    }

    public struct HCCaretInfo
    {
        public int X, Y, Height, PageIndex;
        public bool Visible;
    }

    public enum MarkType : byte
    {
        cmtBeg = 0, 
        cmtEnd = 1
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
        private bool FReCreate, FDisFocus, FVScroll, FHScroll;
        private int FHeight;
        private IntPtr FOwnHandle;
        private int FX, FY;
        private byte FWidth;

        protected void SetX(int value)
        {
            if (FX != value)
            {
                FX = value;
                Show();
            }
        }

        protected void SetY(int value)
        {
            if (FY != value)
            {
                FY = value;
                Show();
            }
        }

        protected void SetHeight(int value)
        {
            if (FHeight != value)
            {
                FHeight = value;
                FReCreate = true;
            }
        }

        protected void SetWidth(byte value)
        {
            if (FWidth != value)
            {
                FWidth = value;
                FReCreate = true;
            }
        }

        //Visible: Boolean;
        public HCCaret(IntPtr aHandle)
        {
            FOwnHandle = aHandle;
            FWidth = 2;
            User.CreateCaret(FOwnHandle, IntPtr.Zero, FWidth, 20);
            FReCreate = false;
            FDisFocus = false;
        }

        ~HCCaret()
        {
            User.DestroyCaret();
            FOwnHandle = IntPtr.Zero;
        }

        public void ReCreate()
        {
            User.DestroyCaret();
            User.CreateCaret(FOwnHandle, IntPtr.Zero, FWidth, FHeight);
        }

        public void Show(int aX, int  aY)
        {
            FDisFocus = false;

            if (FReCreate)
                ReCreate();

            User.SetCaretPos(aX, aY);
            User.ShowCaret(FOwnHandle);
        }

        public void Show()
        {
            this.Show(FX, FY);
        }

        public void Hide(bool aDisFocus = false)
        {
            FDisFocus = aDisFocus;
            User.HideCaret(FOwnHandle);
        }

        public int Height
        {
            get { return FHeight; }
            set { SetHeight(value); }
        }

        public Byte Width
        {
            get { return FWidth; }
            set { SetWidth(value); }
        }

        public int X
        {
            get { return FX; }
            set { SetX(value); }
        }

        public int Y
        {
            get { return FY; }
            set { SetY(value); }
        }

        public bool DisFocus
        {
            get { return FDisFocus; }
        }

        public bool VScroll
        {
            get { return FVScroll; }
            set { FVScroll = value; }
        }

        public bool HScroll
        {
            get { return FHScroll; }
            set { FHScroll = value; }
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
            return ((FValue & value) == value);
        }

        public void InClude(byte value)
        {
            FValue = (byte)(FValue | value);
        }

        public void ExClude(byte value)
        {
            FValue = (byte)(FValue & ~value);
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

        public event EventHandler<EventArgs> OnClear = null;

        public new void Add(T item)
        {
            base.Add(item);
            if (OnInsert != null)
            {
                OnInsert.Invoke(this, new NListEventArgs<T>(item, this.Count));
            }
        }

        public new void Clear()
        {
            for (int i = this.Count - 1; i >= 0; i--)
            {
                if (OnDelete != null)
                    OnDelete.Invoke(this, new NListEventArgs<T>(base[i], i));
            }

            base.Clear();
            if (OnClear != null)
                OnClear.Invoke(this, new EventArgs());
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
            for (int i = index + count - 1; i >= index; i--)
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

        public void Delete(int index)
        {
            RemoveAt(index);
        }

        private T GetFirst()
        {
            return this[0];
        }

        private void SetFirst(T item)
        {
            this[0] = item;
        }

        private T GetLast()
        {
            return this[this.Count - 1];
        }

        private void SetLast(T item)
        {
            this[this.Count - 1] = item;
        }

        public T First
        {
            get { return GetFirst(); }
            set { SetFirst(value); }
        }

        public T Last
        {
            get { return GetLast(); }
            set { SetLast(value); }
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
