/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2020-3-17             }
{                                                       }
{                 Code128条码实现单元                   }
{                                                       }
{*******************************************************/

using HC.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace HC.View
{
    public enum HCCode128Encoding { encA, encB, encC, encAorB, encNone };
    public enum HCCodeLineType { White, Black, BlackHalf, BlackTrack, BlackAscend, BlackDescend };

    class HCCode128
    {
        private int FWidth, FHeight, FModul;
        private byte[] FModules;
        private byte FZoom;
        private string FText;
        private bool FTextVisible;
        private string FCode;
        private EventHandler FOnWidthChanged;

        private void CalcWidth()
        {
            int vW = 0;
            if (FCode != "")
                vW = GetBarWidth(FCode) * FZoom;
            else
                vW = 60;

            if (FWidth != vW)
            {
                FWidth = vW;
                if (FOnWidthChanged != null)
                    FOnWidthChanged(this, null);
            }
        }

        private void OneBarProps(Char code, ref int width, ref HCCodeLineType lineType)
        {
            switch (code)
            {
                case '0':
                    width = this.FModules[0];
                    lineType = HCCodeLineType.White;
                    break;

                case '1':
                    width = this.FModules[1];
                    lineType = HCCodeLineType.White;
                    break;

                case '2':
                    width = this.FModules[2];
                    lineType = HCCodeLineType.White;
                    break;

                case '3':
                    width = this.FModules[3];
                    lineType = HCCodeLineType.White;
                    break;

                case '5':
                    width = this.FModules[0];
                    lineType = HCCodeLineType.Black;
                    break;

                case '6':
                    width = this.FModules[1];
                    lineType = HCCodeLineType.Black;
                    break;

                case '7':
                    width = this.FModules[2];
                    lineType = HCCodeLineType.Black;
                    break;

                case '8':
                    width = this.FModules[3];
                    lineType = HCCodeLineType.Black;
                    break;

                case 'A':
                    width = this.FModules[0];
                    lineType = HCCodeLineType.BlackHalf;
                    break;

                case 'B':
                    width = this.FModules[1];
                    lineType = HCCodeLineType.BlackHalf;
                    break;

                case 'C':
                    width = this.FModules[2];
                    lineType = HCCodeLineType.BlackHalf;
                    break;

                case 'D':
                    width = this.FModules[3];
                    lineType = HCCodeLineType.BlackHalf;
                    break;

                case 'F':
                    width = this.FModules[0];
                    lineType = HCCodeLineType.BlackTrack;
                    break;

                case 'G':
                    width = this.FModules[0];
                    lineType = HCCodeLineType.BlackAscend;
                    break;

                case 'H':
                    width = this.FModules[0];
                    lineType = HCCodeLineType.BlackDescend;
                    break;
                default:
                    throw new Exception("HCCode128计算宽度出错！");
            }
        }

        private void SetText(string Value)
        {
            if (this.FText != Value)
            {
                for (int i = 0; i < Value.Length; i++)
                {
                    if ((ushort)Value[i] > 128)
                        return;
                }

                this.FText = Value;
                try
                {
                    this.FCode = this.GetCode(this.FText);
                }
                catch
                {
                    this.FCode = "";
                }

                this.CalcWidth();
            }
        }

        private void SetZoom(byte Value)
        {
            if (this.FZoom != Value)
            {
                this.FZoom = Value;
                this.CalcWidth();
            }
        }

        private string GetCode(string text)
        {
            text = text.Replace("&FNC1;", "&1;");
            text = this.Encode(text);
            HCCode128Encoding vEncoding = HCCode128Encoding.encNone;
            int vIndex = 0, vChecksum, vCodewordPos, vIdx;
            string nextChar = this.GetNextChar(text, ref vIndex, ref vEncoding);
            string startCode = "";

            if (nextChar == "&A;")
            {
                vEncoding = HCCode128Encoding.encA;
                vChecksum = 103;
                startCode = Table128[103, 3];
            }
            else if (nextChar == "&B;")
            {
                vEncoding = HCCode128Encoding.encB;
                vChecksum = 104;
                startCode = Table128[104 ,3];
            }
            else if (nextChar == "&C;")
            {
                vEncoding = HCCode128Encoding.encC;
                vChecksum = 105;
                startCode = Table128[105, 3];
            }
            else
                throw new Exception("无效的条码内容！");

            string vResult = startCode;
            vCodewordPos = 1;

            int vLen = text.Length;
            while (vIndex < vLen)
            {
                nextChar = this.GetNextChar(text, ref vIndex, ref vEncoding);

                if (nextChar == "&A;")
                {
                    vEncoding = HCCode128Encoding.encA;
                    vIdx = 101;
                }
                else
                if (nextChar == "&B;")
                {
                    vEncoding = HCCode128Encoding.encB;
                    vIdx = 100;
                }
                else 
                if (nextChar == "&C;")
                {
                    vEncoding = HCCode128Encoding.encC;
                    vIdx = 99;
                }
                else
                if (nextChar == "&S;")
                {
                    if (vEncoding == HCCode128Encoding.encA)
                        vEncoding = HCCode128Encoding.encB;
                    else
                        vEncoding = HCCode128Encoding.encA;

                    vIdx = 98;
                }
                else if (nextChar == "&1;")
                    vIdx = 102;
                else if (nextChar == "&2;")
                    vIdx = 97;
                else if (nextChar == "&3;")
                    vIdx = 96;
                else if (nextChar == "&4;")
                {
                    if (vEncoding == HCCode128Encoding.encA)
                        vIdx = 101;
                    else
                        vIdx = 100;
                }
                else
                {
                    if (vEncoding == HCCode128Encoding.encA)
                        vIdx = this.FindCodeA(nextChar[0].ToString());
                    else if (vEncoding == HCCode128Encoding.encB)
                        vIdx = this.FindCodeB(nextChar[0].ToString());
                    else
                        vIdx = this.FindCodeC(nextChar);
                }

                if (vIdx < 0)
                    throw new Exception("无效的条码内容！");

                vResult = vResult + Table128[vIdx, 3];
                vChecksum += vIdx * vCodewordPos;
                vCodewordPos++;

                if (nextChar == "&S;")
                {
                    if (vEncoding == HCCode128Encoding.encA)
                        vEncoding = HCCode128Encoding.encB;
                    else
                        vEncoding = HCCode128Encoding.encA;
                }
            }

            vChecksum = vChecksum % 103;
            vResult = vResult + Table128[vChecksum, 3];
            vResult = vResult + "2331112";
            vResult = this.Convert(vResult);
            return vResult;
        }

        private int GetBarWidth(string ACode)
        {
            int vResult = 0;
            FModules[0] = (byte)FModul;
            FModules[1] = (byte)(FModul * 2);  // 2为宽条宽度
            FModules[2] = (byte)(FModules[1] * 3 / 2);
            FModules[3] = (byte)(FModules[1] * 2);

            int vW = 0;
            HCCodeLineType vLineType = HCCodeLineType.White;
            for (int i = 0; i < FCode.Length; i++)
            {
                OneBarProps(FCode[i], ref vW, ref vLineType);
                vResult += vW;
            }

            return vResult;
        }

        private string GetNextChar(string code, ref int index, ref HCCode128Encoding encoding)
        {
            int vLen = code.Length - 1;
            if (index > vLen)
                return "";

            string vResult = "", vC = "";

            if ((code[index] == '&') && (index + 2 <= vLen) && (code[index + 2] == ';'))
            {
                vC = code[index + 1].ToString().ToUpper();
                if ((vC == "A") || (vC == "B") || (vC == "C")
                    || (vC == "S") || (vC == "1") || (vC == "2")
                    || (vC == "3") || (vC == "4"))
                {
                    index += 3;
                    vResult = "&" + vC + ";";
                    return vResult;
                }
            }

            if ((encoding == HCCode128Encoding.encC) && (index + 1 <= vLen))
            {
                vResult = code.Substring(index, 2);
                index += 2;
                return vResult;
            }

            vResult = code.Substring(index, 1);
            index++;
            return vResult;
        }

        private string StripControlCodes(string code, bool stripFNCodes)
        {
            string vResult = "";
            int vIndex = 0, vLen = code.Length;
            HCCode128Encoding vEncoding = HCCode128Encoding.encNone;
            string vNextChar = "";

            while (vIndex < vLen)
            {
                vNextChar = this.GetNextChar(code, ref vIndex, ref vEncoding);
                if ((vNextChar != "&A;") && (vNextChar != "&B;") && (vNextChar != "&C;") && (vNextChar != "&S;"))
                {
                    if ((!stripFNCodes) || ((vNextChar != "&1;") && (vNextChar != "&2;") && (vNextChar != "&3;") && (vNextChar != "&4;")))
                        vResult = vResult + vNextChar;
                }
            }

            return vResult;
        }

        private bool IsDigit(char c)
        {
            return (c >= '0') && (c <= '9');
        }

        private bool IsFourOrMoreDigits(string code, int index, ref int numDigits)
        {
            int vLen = code.Length;
            numDigits = 0;
            if (IsDigit(code[index]) && (index + 4 < code.Length))
            {
                while ((index + numDigits < vLen) && IsDigit(code[index + numDigits]))
                    numDigits++;
            }

            return numDigits >= 4;
        }

        private int FindCodeA(string c)
        {
            for (int i = 0, vLen = Table128.GetLength(0); i < vLen; i++)
            {
                if (c == Table128[i, 0])
                    return i;
            }

            return -1;
        }

        private int FindCodeB(string c)
        {
            for (int i = 0, vLen = Table128.GetLength(0); i < vLen; i++)
            {
                if (c == Table128[i, 1])
                    return i;
            }

            return -1;
        }

        private int FindCodeC(string code)
        {
            for (int i = 0, vLen = Table128.GetLength(0); i < vLen; i++)
            {
                if (code == Table128[i, 2])
                    return i;
            }

            return -1;
        }

        private string GetNextPortion(string code, ref int aindex, ref HCCode128Encoding aencoding)
        {
            int vLen = code.Length;
            if (aindex > vLen - 1)
                return "";

            int vIndexa, vIndexb, numDigits, numChars;
            HCCode128Encoding firstCharEncoding, nextCharEncoding;
            string prefix, vC = "", vResult = "";

            if ((code[aindex] == '&') && (aindex + 2 < vLen) && (code[aindex + 2] == ';'))
            {
                vC = code[aindex + 1].ToString().ToUpper();
                if ((vC == "A") || (vC == "B") || (vC == "C") || (vC == "S") || (vC == "1") || (vC == "2") || (vC == "3") || (vC == "4"))
                {
                    vC = code.Substring(aindex, 3);
                    aindex += 3;
                }
                else
                    vC = "";
            }

            vIndexa = this.FindCodeA(code[aindex].ToString());
            vIndexb = this.FindCodeB(code[aindex].ToString());
            firstCharEncoding = HCCode128Encoding.encA;
            if ((vIndexa == -1) && (vIndexb != -1))
                firstCharEncoding = HCCode128Encoding.encB;
            else 
            if ((vIndexa != -1) && (vIndexb != -1))
                firstCharEncoding = HCCode128Encoding.encAorB;

            numDigits = 0;
            if (this.IsFourOrMoreDigits(code, aindex, ref numDigits))
                firstCharEncoding = HCCode128Encoding.encC;

            if (firstCharEncoding == HCCode128Encoding.encC)
            {
                numDigits = (numDigits / 2) * 2;
                vResult = code.Substring(aindex, numDigits);
                aindex += numDigits;
                if (aencoding != HCCode128Encoding.encC)
                    vResult = "&C;" + vC + vResult;
                else
                    vResult = vC + vResult;

                aencoding = HCCode128Encoding.encC;

                return vResult;
            }

            numChars = 1;
            while (aindex + numChars < vLen)
            {
                vIndexa = this.FindCodeA(code[aindex + numChars].ToString());
                vIndexb = this.FindCodeB(code[aindex + numChars].ToString());
                nextCharEncoding = HCCode128Encoding.encA;
                if ((vIndexa == -1) && (vIndexb != -1))
                    nextCharEncoding = HCCode128Encoding.encB;
                else 
                if ((vIndexa != -1) && (vIndexb != -1))
                    nextCharEncoding = HCCode128Encoding.encAorB;

                if (this.IsFourOrMoreDigits(code, aindex + numChars, ref numDigits))
                    nextCharEncoding = HCCode128Encoding.encC;

                if ((nextCharEncoding != HCCode128Encoding.encC) && (nextCharEncoding != firstCharEncoding))
                {
                    if (firstCharEncoding == HCCode128Encoding.encAorB)
                        firstCharEncoding = nextCharEncoding;
                    else
                    if (nextCharEncoding == HCCode128Encoding.encAorB)
                        nextCharEncoding = firstCharEncoding;
                }

                if (firstCharEncoding != nextCharEncoding)
                    break;

                numChars++;
            }

            if (firstCharEncoding == HCCode128Encoding.encAorB)
                firstCharEncoding = HCCode128Encoding.encB;

            if (firstCharEncoding == HCCode128Encoding.encA)
                prefix = "&A;";
            else
                prefix = "&B;";

            if ((aencoding != firstCharEncoding) && (numChars == 1)
                && ((aencoding == HCCode128Encoding.encA) || (aencoding == HCCode128Encoding.encB))
                && ((firstCharEncoding == HCCode128Encoding.encA) || (firstCharEncoding == HCCode128Encoding.encB)))
                prefix = "&S;";
            else
                aencoding = firstCharEncoding;

            vResult = prefix + vC + code.Substring(aindex, numChars);
            aindex += numChars;

            return vResult;            
        }

        private string Encode(string code)
        {
            code = this.StripControlCodes(code, false);
            string vResult = "";
            int vIndex = 0;
            HCCode128Encoding vEncoding = HCCode128Encoding.encNone;
            int vLen = code.Length;

            while (vIndex < vLen)
                vResult = vResult + this.GetNextPortion(code, ref vIndex, ref vEncoding);

            return vResult;
        }

        private string Convert(string s)
        {
            byte[] vSBytes = Encoding.ASCII.GetBytes(s);
            byte v;
            for (int i = 0, vLen = vSBytes.Length; i < vLen; i++)
            {
                v = (byte)(vSBytes[i] - 1);
                if (!HC.IsOdd(i))
                    v += 5;

                //vResult += Encoding.ASCII.GetString(v);
                vSBytes[i] = v;
            }

            return Encoding.ASCII.GetString(vSBytes);
        }

        public HCCode128(string text)
        {
            FTextVisible = true;
            FModules = new byte[4];
            FModul = 1;
            FZoom = 1;
            FHeight = 100;
            SetText(text);
        }

        public void PaintTo(HCCanvas canvas, RECT rect)
        {
            int vX = 0, vHeight = rect.Height, vW = 0;
            HCCodeLineType vLineType = HCCodeLineType.White;
            if (this.FTextVisible)
                vHeight -= 12;

            RECT vRect = new RECT();
            for (int i = 0, vLen = this.FCode.Length; i < vLen; i++)
            {
                vLineType = HCCodeLineType.White;
                this.OneBarProps(this.FCode[i], ref vW, ref vLineType);
                if (vLineType != HCCodeLineType.White)
                    canvas.Brush.Color = Color.Black;
                else
                    canvas.Brush.Color = Color.White;

                vRect.Left = vX;
                vRect.Top = 0;
                vRect.Right = vX + vW * this.FZoom;
                vRect.Bottom = vHeight;
                vX = vRect.Right;
                vRect.Offset(rect.Left, rect.Top);
                canvas.FillRect(vRect);
                
            }

            if (this.FCode == "")
            {
                canvas.Pen.BeginUpdate();
                try
                {
                    canvas.Pen.Width = 1;
                    canvas.Pen.Color = Color.Black;
                }
                finally
                {
                    canvas.Pen.EndUpdate();
                }

                canvas.Rectangle(rect);
            }

            if (this.FTextVisible)
            {
                canvas.Font.BeginUpdate();
                try
                {
                    canvas.Font.Size = 8;
                    canvas.Font.FontStyles.Value = 0;
                    canvas.Font.Family = "Arial";
                    canvas.Font.Color = Color.Black;
                }
                finally
                {
                    canvas.Font.EndUpdate();
                }

                canvas.Brush.Style = HCBrushStyle.bsClear;
                if (this.FCode != "")
                {
                    canvas.TextOut(rect.Left + (rect.Width - canvas.TextWidth(this.FText)) / 2,
                        rect.Top + vHeight, this.FText);
                }
                else
                {
                    SIZE vSize = canvas.TextExtent("无效条码" + this.FText);
                    canvas.TextOut(rect.Left + (rect.Width - vSize.cx) / 2,
                        rect.Top + (rect.Height - vSize.cy) / 2, "无效条码" + this.FText);
                }
            }
        }
    
        public string Text
        {
            get { return FText; }
            set { SetText(value); }
        }

        public bool TextVisible
        {
            get { return FTextVisible; }
            set { FTextVisible = value; }
        }

        public int Width
        {
            get { return FWidth; }
        }

        public int Height
        {
            get { return FHeight; }
            set { FHeight = value; }
        }

        public byte Zoom
        {
            get { return FZoom; }
            set { SetZoom(value); }
        }

        public EventHandler OnWidthChanged
        {
            get { return FOnWidthChanged; }
            set { FOnWidthChanged = value; }
        }

        private readonly string[,] Table128 = new string[106, 4] {
            { " ", " ", "00", "212222" },
            { "!", "!", "01", "222122" },  
            { "\"", "\"", "02", "222221" },
            { "#", "#", "03", "121223" },
            { "$", "$", "04", "121322" },
            { "%", "%", "05", "131222" },
            { "&", "&", "06", "122213" },
            { "'", "'", "07", "122312" },
            { "(", "(", "08", "132212" },
            { ")", ")", "09", "221213" },
            { "*", "*", "10", "221312" },
            { "+", "+", "11", "231212" },
            { ",", ",", "12", "112232" },
            { "-", "-", "13", "122132" },
            { ".", ".", "14", "122231" },
            { "/", "/", "15", "113222" },
            { "0", "0", "16", "123122" },
            { "1", "1", "17", "123221" },
            { "2", "2", "18", "223211" },
            { "3", "3", "19", "221132" },
            { "4", "4", "20", "221231" },
            { "5", "5", "21", "213212" },
            { "6", "6", "22", "223112" },
            { "7", "7", "23", "312131" },
            { "8", "8", "24", "311222" },
            { "9", "9", "25", "321122" },
            { ":", ":", "26", "321221" },
            { ";", ";", "27", "312212" },
            { "<", "<", "28", "322112" },
            { "=", "=", "29", "322211" },
            { ">", ">", "30", "212123" },
            { "?", "?", "31", "212321" },
            { "@", "@", "32", "232121" },
            { "A", "A", "33", "111323" },
            { "B", "B", "34", "131123" },
            { "C", "C", "35", "131321" },
            { "D", "D", "36", "112313" },
            { "E", "E", "37", "132113" },
            { "F", "F", "38", "132311" },
            { "G", "G", "39", "211313" },
            { "H", "H", "40", "231113" },
            { "I", "I", "41", "231311" },
            { "J", "J", "42", "112133" },
            { "K", "K", "43", "112331" },
            { "L", "L", "44", "132131" },
            { "M", "M", "45", "113123" },
            { "N", "N", "46", "113321" },
            { "O", "O", "47", "133121" },
            { "P", "P", "48", "313121" },
            { "Q", "Q", "49", "211331" },
            { "R", "R", "50", "231131" },
            { "S", "S", "51", "213113" },
            { "T", "T", "52", "213311" },
            { "U", "U", "53", "213131" },
            { "V", "V", "54", "311123" },
            { "W", "W", "55", "311321" },
            { "X", "X", "56", "331121" },
            { "Y", "Y", "57", "312113" },
            { "Z", "Z", "58", "312311" },
            { "{ ", "{ ", "59", "332111" },
            { "\\", "\\", "60", "314111" },
            { " }", " }", "61", "221411" },
            { "^", "^", "62", "431111" },
            { "_", "_", "63", "111224" },
            { ((char)0).ToString(), "`", "64", "111422" },
            { ((char)1).ToString(), "a", "65", "121124" },
            { ((char)2).ToString(), "b", "66", "121421" },
            { ((char)3).ToString(), "c", "67", "141122" },
            { ((char)4).ToString(), "d", "68", "141221" },
            { ((char)5).ToString(), "e", "69", "112214" },
            { ((char)6).ToString(), "f", "70", "112412" },
            { ((char)7).ToString(), "g", "71", "122114" },
            { ((char)8).ToString(), "h", "72", "122411" },
            { ((char)9).ToString(), "i", "73", "142112" },
            { ((char)10).ToString(), "j", "74", "142211" },
            { ((char)11).ToString(), "k", "75", "241211" },
            { ((char)12).ToString(), "l", "76", "221114" },
            { ((char)13).ToString(), "m", "77", "413111" },
            { ((char)14).ToString(), "n", "78", "241112" },
            { ((char)15).ToString(), "o", "79", "134111" },
            { ((char)16).ToString(), "p", "80", "111242" },
            { ((char)17).ToString(), "q", "81", "121142" },
            { ((char)18).ToString(), "r", "82", "121241" },
            { ((char)19).ToString(), "s", "83", "114212" },
            { ((char)20).ToString(), "t", "84", "124112" },
            { ((char)21).ToString(), "u", "85", "124211" },
            { ((char)22).ToString(), "v", "86", "411212" },
            { ((char)23).ToString(), "w", "87", "421112" },
            { ((char)24).ToString(), "x", "88", "421211" },
            { ((char)25).ToString(), "y", "89", "212141" },
            { ((char)26).ToString(), "z", "90", "214121" },
            { ((char)27).ToString(), "{", "91", "412121" },
            { ((char)28).ToString(), "|", "92", "111143" },
            { ((char)29).ToString(), "}", "93", "111341" },
            { ((char)30).ToString(), "~", "94", "131141" },
            { ((char)31).ToString(), " ", "95", "114113" },
            { " ", " ", "96", "114311" },
            { " ", " ", "97", "411113" },
            { " ", " ", "98", "411311" },
            { " ", " ", "99", "113141" },
            { " ", " ", "  ", "114131" },
            { " ", " ", "  ", "311141" },
            { " ", " ", "  ", "411131" },
            { " ", " ", "  ", "211412" },
            { " ", " ", "  ", "211214" },
            { " ", " ", "  ", "211232" }
        };
    }
}
