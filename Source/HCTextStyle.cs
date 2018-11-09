/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                 文本文字样式实现单元                  }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using HC.Win32;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace HC.View
{
    [Serializable]
    public enum HCFontStyle : byte
    {
        tsBold = 1, tsItalic = 2, tsUnderline = 4, tsStrikeOut = 8, tsSuperscript = 16, tsSubscript = 32
    }

    public class HCTextStyle : HCObject
    {
        public const Single DefaultFontSize = 10.5F;  // 五号
        public const string DefaultFontFamily = "宋体";
        public const Single MaxFontSize = 512F;

        private Single FSize;
        private string FFamily;
        private HCFontStyles FFontStyles;
        private Color FColor;  // 字体颜色
        private Color FBackColor;

        protected void SetFamily(string Value)
        {
            if (FFamily != Value)
                FFamily = Value;
        }

        protected void SetSize(Single Value)
        {
            if (FSize != Value)
                FSize = Value;
        }

        protected void SetFontStyles(HCFontStyles Value)
        {
            if (FFontStyles != Value)
                FFontStyles = Value;
        }

        public bool CheckSaveUsed;
        public int TempNo;

        public HCTextStyle()
        {
            FSize = DefaultFontSize;
            FFamily = DefaultFontFamily;
            FFontStyles = new HCFontStyles();
            FColor = Color.Black;
            FBackColor = Color.Transparent;
        }

        ~HCTextStyle()
        {

        }
        
        public bool IsSizeStored()
        {
            return (FSize == DefaultFontSize);
        }

        public bool IsFamilyStored()
        {
            return (FFamily != DefaultFontFamily);
        }

        public void ApplyStyle(HCCanvas ACanvas, Single AScale = 1)
        {
            ACanvas.Brush.Color = FBackColor;

            ACanvas.Font.BeginUpdate();
            try
            {
                ACanvas.Font.Color = FColor;
                ACanvas.Font.Family = FFamily;
                if ((FFontStyles.Contains((byte)HCFontStyle.tsSuperscript)) || (FFontStyles.Contains((byte)HCFontStyle.tsSubscript)))
                    ACanvas.Font.Size = FSize / 2;
                else
                    ACanvas.Font.Size = FSize;

                ACanvas.Font.FontStyles = FFontStyles;
            }
            finally
            {
                ACanvas.Font.EndUpdate();
            }
        }

        public bool EqualsEx(HCTextStyle ASource)
        {
            return (this.FSize == ASource.Size)
                && (this.FFontStyles.Value == ASource.FontStyles.Value)
                && (this.FFamily == ASource.Family)
                && (this.FColor == ASource.Color)
                && (this.FBackColor == ASource.BackColor);
        }

        public void AssignEx(HCTextStyle ASource)
        {
            this.FSize = ASource.Size;
            this.FFontStyles = ASource.FontStyles;
            this.FFamily = ASource.Family;
            this.FColor = ASource.Color;
            this.FBackColor = ASource.BackColor;
        }

        public void SaveToStream(Stream AStream)
        {
            byte[] vBuffer = BitConverter.GetBytes(FSize);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            byte[] vBuffer2 = System.Text.Encoding.Default.GetBytes(FFamily);
            ushort vSize = (ushort)vBuffer2.Length;

            vBuffer = BitConverter.GetBytes(vSize);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            if (vSize > 0)
                AStream.Write(vBuffer2, 0, vSize);

            AStream.WriteByte(FFontStyles.Value);  // save FFontStyles

            HC.SaveColorToStream(AStream, FColor);  // save FColor
            HC.SaveColorToStream(AStream, FBackColor);  // save FBackColor
        }

        public void LoadFromStream(Stream AStream, ushort AFileVersion)
        {
            int vOldSize = 10;
            ushort vSize = 10;
            if (AFileVersion < 12)
            {
                byte[] vBuffer1 = BitConverter.GetBytes(vOldSize);
                AStream.Read(vBuffer1, 0, vBuffer1.Length);
                vOldSize = BitConverter.ToInt32(vBuffer1, 0);
                FSize = (ushort)vOldSize;
            }
            else
            {
                byte[] vBuffer1 = BitConverter.GetBytes(FSize);
                AStream.Read(vBuffer1, 0, vBuffer1.Length);
                FSize = BitConverter.ToSingle(vBuffer1, 0);  // 字号
            }

            // 字体
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            vSize = BitConverter.ToUInt16(vBuffer, 0);
            if (vSize > 0)
            {
                vBuffer = new byte[vSize];
                AStream.Read(vBuffer, 0, vBuffer.Length);
                FFamily = System.Text.Encoding.Default.GetString(vBuffer);
            }

            FFontStyles.Value = (byte)AStream.ReadByte();  // load FFontStyles

            HC.LoadColorFromStream(AStream, ref FColor);  // load FColor
            HC.LoadColorFromStream(AStream, ref FBackColor);  // load FBackColor
        }

        public string Family
        {
            get { return FFamily; }
            set { SetFamily(value); }
        }

        public Single Size
        {
            get { return FSize; }
            set { SetSize(value); }
        }

        public HCFontStyles FontStyles
        {
            get { return FFontStyles; }
            set { SetFontStyles(value); }
        }

        public Color Color
        {
            get { return FColor; }
            set { FColor = value; }
        }

        public Color BackColor
        {
            get { return FBackColor; }
            set { FBackColor = value; }
        }
    }
}
