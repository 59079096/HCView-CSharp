﻿/*******************************************************}
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
using System.Xml;

namespace HC.View
{
    /// <summary> 段水平对齐方式：左、居中、右、两端、分散) </summary>
    public enum ParaAlignHorz : byte 
    {
        pahLeft = 0, pahCenter = 1, pahRight = 2, pahJustify = 3, pahScatter = 4
    }

    /// <summary> 段垂直对齐方式：上、居中、下) </summary>
    public enum ParaAlignVert : byte 
    {
        pavTop = 0, pavCenter = 1, pavBottom = 2
    }

    /// <summary> 首行缩进方式 </summary>
    public enum ParaFirstLineIndent : byte
    {
        pfiNone, pfiIndented, pfiHanging
    }

    public enum ParaLineSpaceMode : byte
    {
        pls100 = 0, pls115 = 1, pls150 = 2, pls200 = 3, plsMin = 4, plsFix = 5, plsMult = 6
    }

    public class HCParaStyle : HCObject
    {
        private ParaLineSpaceMode FLineSpaceMode;
        private Single FLineSpace;

        // 单位像素
        private int FFirstIndentPix,// 首行缩进
                    FLeftIndentPix,  // 左缩进
                    FRightIndentPix;  // 右缩进

        // 单位毫米
        private Single FFirstIndent,// 首行缩进
                       FLeftIndent,  // 左缩进
                       FRightIndent;  // 右缩进

        private bool FBreakRough;  // 是否粗暴截断
        private Color FBackColor;
        private ParaAlignHorz FAlignHorz;
        private ParaAlignVert FAlignVert;

        protected void SetFirstIndent(Single value)
        {
            if (FFirstIndent != value)
            {
                FFirstIndent = value;
                FFirstIndentPix = HCUnitConversion.MillimeterToPixX(FFirstIndent);
            }
        }

        protected void SetLeftIndent(Single value)
        {
            if (FLeftIndent != value)
            {
                FLeftIndent = value;
                FLeftIndentPix = HCUnitConversion.MillimeterToPixX(FLeftIndent);
            }
        }

        protected void SetRightIndent(Single value)
        {
            if (FRightIndent != value)
            {
                FRightIndent = value;
                FRightIndentPix = HCUnitConversion.MillimeterToPixX(FRightIndent);
            }
        }

        public bool CheckSaveUsed;
        public int TempNo;

        public HCParaStyle()
        {
            FirstIndent = 0;
            LeftIndent = 0;
            RightIndent = 0;
            FLineSpaceMode = ParaLineSpaceMode.pls150;
            FLineSpace = 1;
            FBreakRough = false;
            FBackColor = HC.HCTransparentColor;
            FAlignHorz = ParaAlignHorz.pahJustify;
            FAlignVert = ParaAlignVert.pavCenter;
        }

        ~HCParaStyle()
        {

        }

        public bool EqualsEx(HCParaStyle aSource)
        {
            return (FLineSpaceMode == aSource.LineSpaceMode)
                && (FLineSpace == aSource.LineSpace)
                && (FFirstIndent == aSource.FirstIndent)
                && (FLeftIndent == aSource.LeftIndent)
                && (FRightIndent == aSource.RightIndent)
                && (FBackColor == aSource.BackColor)
                && (FBreakRough == aSource.BreakRough)
                && (FAlignHorz == aSource.AlignHorz)
                && (FAlignVert == aSource.AlignVert);
        }

        public void AssignEx(HCParaStyle aSource)
        {
            FLineSpaceMode = aSource.LineSpaceMode;
            FLineSpace = aSource.LineSpace;
            FirstIndent = aSource.FirstIndent;
            LeftIndent = aSource.LeftIndent;
            RightIndent = aSource.RightIndent;
            FBackColor = aSource.BackColor;
            FBreakRough = aSource.BreakRough;
            FAlignHorz = aSource.AlignHorz;
            FAlignVert = aSource.AlignVert;
        }

        public void SaveToStream(Stream aStream)
        {
            byte vByte = (byte)FLineSpaceMode;
            aStream.WriteByte(vByte);

            byte[] vBuffer = BitConverter.GetBytes(FLineSpace);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FFirstIndent);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FLeftIndent);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FRightIndent);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            HC.HCSaveColorToStream(aStream, FBackColor);  // save BackColor

            vByte = 0;
            if (FBreakRough)
                vByte = (byte)(vByte | (1 << 7));

            aStream.WriteByte(vByte);

            vByte = (byte)FAlignHorz;
            aStream.WriteByte(vByte);

            vByte = (byte)FAlignVert;
            aStream.WriteByte(vByte);
        }

        public void LoadFromStream(Stream aStream, ushort aFileVersion)
        {
            byte[] vBuffer;
            if (aFileVersion < 15)
            {
                int vLineSpace = 0;
                vBuffer = BitConverter.GetBytes(vLineSpace);
                aStream.Read(vBuffer, 0, vBuffer.Length);
            }

            byte vByte = 0;
            if (aFileVersion > 16)
            {
                vByte = (byte)aStream.ReadByte();
                FLineSpaceMode = (ParaLineSpaceMode)vByte;
            }

            if (aFileVersion > 30)
            {
                vBuffer = BitConverter.GetBytes(FLineSpace);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FLineSpace = BitConverter.ToSingle(vBuffer, 0);
            }
            else
            if (FLineSpaceMode == ParaLineSpaceMode.plsFix)  // 旧版本统一按12pt处理
                FLineSpace = 12;

            if (aFileVersion < 22)
            {
                vBuffer = BitConverter.GetBytes(FFirstIndentPix);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FFirstIndentPix = BitConverter.ToInt32(vBuffer, 0);
                //
                vBuffer = BitConverter.GetBytes(FLeftIndentPix);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FLeftIndentPix = BitConverter.ToInt32(vBuffer, 0);
            }
            else
            {
                vBuffer = BitConverter.GetBytes(FFirstIndent);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FFirstIndent = BitConverter.ToSingle(vBuffer, 0);
                FFirstIndentPix = HCUnitConversion.MillimeterToPixX(FFirstIndent);
                //
                vBuffer = BitConverter.GetBytes(FLeftIndent);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FLeftIndent = BitConverter.ToSingle(vBuffer, 0);
                FLeftIndentPix = HCUnitConversion.MillimeterToPixX(FLeftIndent);
                //
                vBuffer = BitConverter.GetBytes(FRightIndent);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FRightIndent = BitConverter.ToSingle(vBuffer, 0);
                FRightIndentPix = HCUnitConversion.MillimeterToPixX(FRightIndent);
            }

            HC.HCLoadColorFromStream(aStream, ref FBackColor);

            if (aFileVersion > 31)
            {
                vByte = (byte)aStream.ReadByte();
                FBreakRough = HC.IsOdd(vByte >> 7);
            }
            
            vByte = (byte)aStream.ReadByte();
            FAlignHorz = (ParaAlignHorz)vByte;

            if (aFileVersion > 17)
            {
                vByte = (byte)aStream.ReadByte();
                FAlignVert = (ParaAlignVert)vByte;
            }
        }

        public string ToCSS()
        {
            string Result = " text-align: ";
            switch (FAlignHorz)
            {
                case ParaAlignHorz.pahLeft:
                    Result = Result + "left";
                    break;

                case ParaAlignHorz.pahRight:
                    Result = Result + "right";
                    break;

                case ParaAlignHorz.pahCenter:
                    Result = Result + "center";
                    break;

                case ParaAlignHorz.pahJustify:
                case ParaAlignHorz.pahScatter:
                    Result = Result + "justify";
                    break;
            }

            switch (FLineSpaceMode)
            {
                case ParaLineSpaceMode.pls100:
                    Result = Result + "; line-height: 100%";
                    break;

                case ParaLineSpaceMode.pls115:
                    Result = Result + "; line-height: 115%";
                    break;

                case ParaLineSpaceMode.pls150:
                    Result = Result + "; line-height: 150%";
                    break;

                case ParaLineSpaceMode.pls200:
                    Result = Result + "; line-height: 200%";
                    break;

                case ParaLineSpaceMode.plsFix:
                    Result = Result + "; line-height: 10px";
                    break;
            }

            return Result;
        }

        private string GetLineSpaceModeXML_()
        {
            switch (FLineSpaceMode)
            {
                case ParaLineSpaceMode.pls100:
                    return "100";

                case ParaLineSpaceMode.pls115:
                    return "115";

                case ParaLineSpaceMode.pls150:
                    return "150";

                case ParaLineSpaceMode.pls200:
                    return "200";

                case ParaLineSpaceMode.plsMin:
                    return "min";

                case ParaLineSpaceMode.plsMult:
                    return "mult";

                case ParaLineSpaceMode.plsFix:
                    return "fix";

                default:
                    return "100";
            }
        }

        private string GetHorzXML_()
        {
            switch (FAlignHorz)
            {
                case ParaAlignHorz.pahLeft:
                    return "left";

                case ParaAlignHorz.pahRight:
                    return "right";

                case ParaAlignHorz.pahCenter:
                    return "center";

                case ParaAlignHorz.pahJustify:
                    return "justify";

                default:
                    return "scatter";
            }
        }

        private string GetVertXML_()
        {
            switch (FAlignVert)
            {
                case ParaAlignVert.pavTop:
                    return "top";

                case ParaAlignVert.pavCenter:
                    return "center";

                default:
                    return "bottom";
            }
        }

        public void ToXml(XmlElement aNode)
        {
            aNode.SetAttribute("firstindent", FFirstIndent.ToString());
            aNode.SetAttribute("leftindent", FLeftIndent.ToString());
            aNode.SetAttribute("rightindent", FRightIndent.ToString());
            aNode.SetAttribute("bkcolor", HC.HCColorToRGBString(FBackColor));
            if (FBreakRough)
                aNode.SetAttribute("breakrough", "1");

            aNode.SetAttribute("spacemode", GetLineSpaceModeXML_());
            aNode.SetAttribute("horz", GetHorzXML_());
            aNode.SetAttribute("vert", GetVertXML_());
        }

        public void ParseXml(XmlElement aNode)
        {
            FirstIndent = float.Parse(aNode.Attributes["firstindent"].Value);
            LeftIndent = float.Parse(aNode.Attributes["leftindent"].Value);
            RightIndent = float.Parse(aNode.Attributes["rightindent"].Value);
            FBackColor = HC.HCRGBStringToColor(aNode.Attributes["bkcolor"].Value);
            FBreakRough = aNode.GetAttribute("breakrough") == "1";
            //GetXMLLineSpaceMode_;
            if (aNode.Attributes["spacemode"].Value == "100")
                FLineSpaceMode = ParaLineSpaceMode.pls100;
            else
            if (aNode.Attributes["spacemode"].Value == "115")
                FLineSpaceMode = ParaLineSpaceMode.pls115;
            else
            if (aNode.Attributes["spacemode"].Value == "150")
                FLineSpaceMode = ParaLineSpaceMode.pls150;
            else
            if (aNode.Attributes["spacemode"].Value == "200")
                FLineSpaceMode = ParaLineSpaceMode.pls200;
            else
            if (aNode.Attributes["spacemode"].Value == "min")
                FLineSpaceMode = ParaLineSpaceMode.plsMin;
            else
            if (aNode.Attributes["spacemode"].Value == "mult")
                FLineSpaceMode = ParaLineSpaceMode.plsMult;
            else
            if (aNode.Attributes["spacemode"].Value == "fix")
                FLineSpaceMode = ParaLineSpaceMode.plsFix;

            //GetXMLHorz_;
            if (aNode.Attributes["horz"].Value == "left")
                FAlignHorz = ParaAlignHorz.pahLeft;
            else
            if (aNode.Attributes["horz"].Value == "right")
                FAlignHorz = ParaAlignHorz.pahRight;
            else
            if (aNode.Attributes["horz"].Value == "center")
                FAlignHorz = ParaAlignHorz.pahCenter;
            else
            if (aNode.Attributes["horz"].Value == "justify")
                FAlignHorz = ParaAlignHorz.pahJustify;
            else
            if (aNode.Attributes["horz"].Value == "scatter")
                FAlignHorz = ParaAlignHorz.pahScatter;

            //GetXMLVert_;
            if (aNode.Attributes["vert"].Value == "top")
                FAlignVert = ParaAlignVert.pavTop;
            else
            if (aNode.Attributes["vert"].Value == "center")
                FAlignVert = ParaAlignVert.pavCenter;
            else
            if (aNode.Attributes["vert"].Value == "bottom")
                FAlignVert = ParaAlignVert.pavBottom;
        }
        
        public ParaLineSpaceMode LineSpaceMode
        {
            get { return FLineSpaceMode; }
            set { FLineSpaceMode = value; }
        }

        public Single LineSpace
        {
            get { return FLineSpace; }
            set { FLineSpace = value; }
        }

        public Single FirstIndent 
        {
            get { return FFirstIndent; }
            set { SetFirstIndent(value); }
        }

        public Single LeftIndent
        { 
            get { return FLeftIndent; }
            set { SetLeftIndent(value); }
        }

        public Single RightIndent
        {
            get { return FRightIndent; }
            set { SetRightIndent(value); }
        }

        public int FirstIndentPix
        {
            get { return FFirstIndentPix; }
        }

        public int LeftIndentPix
        {
            get { return FLeftIndentPix; }
        }

        public int RightIndentPix
        {
            get { return FRightIndentPix; }
        }

        public Color BackColor
        {
            get { return FBackColor; }
            set { FBackColor = value; }
        }

        public bool BreakRough
        {
            get { return FBreakRough; }
            set { FBreakRough = value; }
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
