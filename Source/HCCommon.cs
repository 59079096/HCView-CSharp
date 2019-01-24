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
        //public static char[] HCBoolText = { '0', '1' };

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
            HCS_EXCEPTION_TIMERRESOURCEOUTOF = HC_EXCEPTION + "安装计时器的资源不足！",

            // 不能在行首的字符
            DontLineFirstChar = @"`-=[]\;'',./~!@#$%^&*()_+{}|:""<>?·－＝【】＼；‘，。、～！＠＃￥％……＆×（）—＋｛｝｜：“《》？°",
            DontLineLastChar = @"/\＼",
            sLineBreak = "\r\n",
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
            HC_PROGRAMLANGUAGE = 2;  // 1字节表示使用的编程语言 1:delphi, 2:C#, 3:VC++, 4:HTML5

        public static bool IsKeyPressWant(KeyPressEventArgs aKey)
        {
            return (aKey.KeyChar >= 32) && (aKey.KeyChar <= 126);
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

        public static int GetCharOffsetAt(HCCanvas aCanvas, string aText, int x)
        {
            int Result = -1;
            if (x < 0)
                Result = 0;
            else
            if (x > aCanvas.TextWidth(aText))
                Result = aText.Length;
            else
            {
                int vX = 0, vCharWidth = 0;

                for (int i = 1; i <= aText.Length; i++)
                {
                    vCharWidth = aCanvas.TextWidth(aText[i - 1]);
                    vX = vX + vCharWidth;
                    if (vX > x)
                    {
                        if (vX - vCharWidth / 2 > x)
                            Result = i - 1;  // 计为前一个后面
                        else
                            Result = i;

                        break;
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
                Result = string.Format("#.#", aFontSize);

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
            int vLen = System.Text.Encoding.Default.GetByteCount(s);
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            ushort vSize = (ushort)vLen;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            aStream.Write(vBuffer, 0, vBuffer.Length);
            if (vSize > 0)
            {
                vBuffer = System.Text.Encoding.Default.GetBytes(s);
                aStream.Write(vBuffer, 0, vBuffer.Length);
            }
        }

        public static void HCLoadTextFromStream(Stream aStream, ref string s)
        {
            ushort vSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vSize = BitConverter.ToUInt16(vBuffer, 0);

            if (vSize > 0)
            {
                vBuffer = new byte[vSize];
                aStream.Read(vBuffer, 0, vSize);
                s = System.Text.Encoding.Default.GetString(vBuffer);
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
            byte A = (byte)aStream.ReadByte();
            byte R = (byte)aStream.ReadByte();
            byte G = (byte)aStream.ReadByte();
            byte B = (byte)aStream.ReadByte();
            color = Color.FromArgb(A, R, G, B);
        }

        public static Color GetXmlRGBColor(string aColorStr)
        {
            string[] vsRGB = aColorStr.Split(new string[] { "," }, StringSplitOptions.None);
            return Color.FromArgb(byte.Parse(vsRGB[0]), byte.Parse(vsRGB[1]), byte.Parse(vsRGB[2]));
        }

        public static string GetColorXmlRGB(Color aColor)
        {
            return aColor.R.ToString() + "," + aColor.G.ToString() + "," + aColor.B.ToString();
        }

        public static void SetBorderSideByPro(string aValue, HCBorderSides aBorderSides)
        {
            aBorderSides.Value = 0;
            string[] vStrings = aValue.Split(new string[] { "," }, StringSplitOptions.None);

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

        public static string GraphicToBase64(Image aGraphic)
        {
            MemoryStream vStream = new MemoryStream();
            aGraphic.Save(vStream, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] vArr = new byte[vStream.Length];
            vStream.Position = 0;
            vStream.Read(vArr, 0, (int)vStream.Length);
            vStream.Close();
            vStream.Dispose();
            return Convert.ToBase64String(vArr);
        }

        public static void Base64ToGraphic(string aBase64, Image aGraphic)
        {
            byte[] vArr = Convert.FromBase64String(aBase64);
            MemoryStream vStream = new MemoryStream(vArr);
            aGraphic = Image.FromStream(vStream);
        }
        
        public static BreakPosition MatchBreak(CharType aPrevType, CharType aPosType, string aText, int aIndex)
        {
            switch (aPosType)
            {
                case CharType.jctHZ:
                    {
                        if ((aPrevType == CharType.jctZM) || (aPrevType == CharType.jctSZ) || (aPrevType == CharType.jctHZ))  // 当前位置是汉字，前一个是字母、数字、汉字
                        {
                            return BreakPosition.jbpPrev;
                        }
                    }
                    break;

                case CharType.jctZM:
                    {
                        if ((aPrevType != CharType.jctZM) && (aPrevType != CharType.jctSZ)) // 当前是字母，前一个不是数字、字母
                        {
                            return BreakPosition.jbpPrev;
                        }
                    }
                    break;

                case CharType.jctSZ:
                    {
                        switch (aPrevType)
                        {
                            case CharType.jctZM:
                            case CharType.jctSZ:
                                break;

                            case CharType.jctFH:
                                {
                                    if (aText.Substring(aIndex - 1) == "￠")
                                    {

                                    }
                                    else
                                    {
                                        string vChar = aText.Substring(aIndex - 1);
                                        if ((vChar != ".") && (vChar != ":") && (vChar != "-") && (vChar != "^") && (vChar != "*") && (vChar != "/"))
                                            return BreakPosition.jbpPrev;
                                    }
                                }
                                break;

                            default:
                                return BreakPosition.jbpPrev;
                        }
                    }
                    break;

                case CharType.jctFH:
                    {
                        switch (aPrevType)
                        {
                            case CharType.jctFH:
                                break;

                            case CharType.jctSZ:
                                {
                                    string vChar = aText.Substring(aIndex - 1);
                                    if ((vChar != ".") && (vChar != ":") && (vChar != "-") && (vChar != "^") && (vChar != "*") && (vChar != "/"))
                                        return BreakPosition.jbpPrev;
                                }
                                break;

                            case CharType.jctZM:
                                {
                                    if (aText.Substring(aIndex - 1) != ":")
                                        return BreakPosition.jbpPrev;
                                }
                                break;

                            default:
                                return BreakPosition.jbpPrev;
                        }
                    }
                    break;
            }

            return BreakPosition.jbpNone;
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

            if ((aChar >= 0xF00) && (aChar <= 0xFFF))
                return CharType.jctHZ;  // 汉字，藏语

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
            return (n % 2 == 1) ? true : false;
        }

        public static void OffsetRect(ref RECT aRect, int x, int y)
        {
            aRect.Offset(x, y);
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
        cpoPortrait = 1,  // 纸张方向：纵像
        cpoLandscape = 1 << 1  //2  、横向
    }

    public enum ExpressArea : byte
    {
        ceaNone = 1, 
        ceaLeft = 1 << 1, 
        ceaTop = 1 << 2, 
        ceaRight = 1 << 3, 
        ceaBottom = 1 << 4  // 公式的区域，仅适用于上下左右格式的
    }

    public enum BorderSide : byte
    {
        cbsLeft = 1, cbsTop = 2, cbsRight = 4, cbsBottom = 8, cbsLTRB = 16, cbsRTLB = 32
    }

    public class HCBorderSides : HCSet
    {

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
        saHeader = 1, 
        saPage = 1 << 1, 
        saFooter = 1 << 2
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
        cmtBeg = 1, 
        cmtEnd = 1 << 1
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

        protected void SetHeight(int value)
        {
            if (FHeight != value)
            {
                FHeight = value;
                ReCreate();
            }
        }

        public int X, Y;

        //Visible: Boolean;
        public HCCaret(IntPtr aHandle)
        {
            FOwnHandle = aHandle;
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

        public void Show(int aX, int  aY)
        {
            ReCreate();
            User.SetCaretPos(aX, aY);
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
