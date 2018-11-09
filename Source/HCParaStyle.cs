/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                  文本段样式实现单元                   }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace HC.View
{
    /// <summary> 段水平对齐方式：左、右、居中、两端、分散) </summary>
    public enum ParaAlignHorz : byte 
    {
        pahLeft, pahRight, pahCenter, pahJustify, pahScatter
    }

    /// <summary> 段垂直对齐方式：下、居中、上) </summary>
    public enum ParaAlignVert : byte 
    {
        pavBottom, pavCenter, pavTop
    }

    public enum ParaLineSpaceMode : byte
    {
        pls100, pls115, pls150, pls200, plsFix
    }

    public class HCParaStyle : HCObject
    {
        private ParaLineSpaceMode FLineSpaceMode;
        private int FFristIndent,// 首行缩进
                    FLeftIndent;  // 左缩进

        private Color FBackColor;
        private ParaAlignHorz FAlignHorz;
        private ParaAlignVert FAlignVert;

        public bool CheckSaveUsed;
        public int TempNo;

        public HCParaStyle()
        {
            FFristIndent = 0;
            FLeftIndent = 0;
            FBackColor = Color.Silver;
            FAlignHorz = ParaAlignHorz.pahJustify;
            FAlignVert = ParaAlignVert.pavCenter;
        }

        ~HCParaStyle()
        {

        }

        public bool EqualsEx(HCParaStyle ASource)
        {
            return (this.FLineSpaceMode == ASource.LineSpaceMode) 
                && (this.FFristIndent == ASource.FristIndent)
                && (this.LeftIndent == ASource.LeftIndent)
                && (this.FBackColor == ASource.BackColor)
                && (this.FAlignHorz == ASource.AlignHorz)
                && (this.FAlignVert == ASource.AlignVert);
        }

        public void AssignEx(HCParaStyle ASource)
        {
            this.FLineSpaceMode = ASource.LineSpaceMode;
            this.FFristIndent = ASource.FristIndent;
            this.FLeftIndent = ASource.LeftIndent;
            this.FBackColor = ASource.BackColor;
            this.FAlignHorz = ASource.AlignHorz;
            this.FAlignVert = ASource.AlignVert;
        }

        public void SaveToStream(Stream AStream)
        {
            byte vByte = (byte)FLineSpaceMode;
            AStream.WriteByte(vByte);

            byte[] vBuffer = BitConverter.GetBytes(FFristIndent);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FLeftIndent);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            HC.SaveColorToStream(AStream, FBackColor);  // save BackColor

            vByte = (byte)FAlignHorz;
            AStream.WriteByte(vByte);

            vByte = (byte)FAlignVert;
            AStream.WriteByte(vByte);
        }

        public void LoadFromStream(Stream AStream, ushort AFileVersion)
        {
            byte[] vBuffer;
            if (AFileVersion < 15)
            {
                int vLineSpace = 0;
                vBuffer = BitConverter.GetBytes(vLineSpace);
                AStream.Read(vBuffer, 0, vBuffer.Length);
            }

            byte vByte = 0;
            if (AFileVersion > 16)
            {
                vByte = (byte)AStream.ReadByte();
                FLineSpaceMode = (ParaLineSpaceMode)vByte;
            }

            vBuffer = BitConverter.GetBytes(FFristIndent);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FFristIndent = BitConverter.ToInt32(vBuffer, 0);
            //
            vBuffer = BitConverter.GetBytes(FLeftIndent);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FLeftIndent = BitConverter.ToInt32(vBuffer, 0);
            //
            HC.LoadColorFromStream(AStream, ref FBackColor);
            //
            vByte = (byte)AStream.ReadByte();
            FAlignHorz = (ParaAlignHorz)vByte;

            if (AFileVersion > 17)
            {
                vByte = (byte)AStream.ReadByte();
                FAlignVert = (ParaAlignVert)vByte;
            }
        }
        
        public ParaLineSpaceMode LineSpaceMode
        {
            get { return FLineSpaceMode; }
            set { FLineSpaceMode = value; }
        }

        public int FristIndent 
        {
            get { return FFristIndent; }
            set { FFristIndent = value; }
        }

        public int LeftIndent
        { 
            get { return FLeftIndent; }
            set { FLeftIndent = value; }
        }

        public Color BackColor
        {
            get { return FBackColor; }
            set { FBackColor = value; }
        }

        public ParaAlignHorz AlignHorz
        {
            get { return FAlignHorz; }
            set { FAlignHorz = value; }
        }

        public ParaAlignVert AlignVert
        {
            get { return FAlignVert; }
            set { FAlignVert = value; }
        }
    }
}
