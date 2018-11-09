/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                  表格单元格实现单元                   }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace HC.View
{
    /// <summary> 垂直对齐方式：上、居中、下) </summary>
    public enum AlignVert : byte
    {
        cavTop, cavCenter, cavBottom
    }

    public class HCBorderSides : HCSet
    {

    }

    public class HCTableCell : HCObject
    {
        private HCTableCellData FCellData;

        private int FWidth;  // 被合并后记录原始宽(否则当行第一列被合并后，第二列无法确认水平起始位置)

        private int FHeight ;  // 被合并后记录原始高、记录拖动改变后高

        private int FRowSpan;  // 单元格跨几行，用于合并目标单元格记录合并了几行，合并源记录合并到单元格的行号，0没有行合并

        private int FColSpan ;  // 单元格跨几列，用于合并目标单元格记录合并了几列，合并源记录合并到单元格的列号，0没有列合并

        private Color FBackgroundColor;

        private AlignVert FAlignVert;

        private HCBorderSides FBorderSides;

        protected bool GetActive()
        {
            if (FCellData != null)
                return FCellData.Active;
            else
                return false;
        }

        protected void SetActive(bool Value)
        {
            if (FCellData != null)
                FCellData.Active = Value;
        }

        protected void SetHeight(int Value)
        {
             if (FHeight != Value)
            {
                FHeight = Value;
                if (FCellData != null)
                    FCellData.CellHeight = Value;
            }
        }

        public HCTableCell()
        {

        }

        public HCTableCell(HCStyle AStyle) : this()
        {
            FCellData = new HCTableCellData(AStyle);
            FAlignVert = View.AlignVert.cavTop;
            FBorderSides = new HCBorderSides();
            FBorderSides.InClude((byte)BorderSide.cbsLeft);
            FBorderSides.InClude((byte)BorderSide.cbsTop);
            FBorderSides.InClude((byte)BorderSide.cbsRight);
            FBorderSides.InClude((byte)BorderSide.cbsBottom);
            FBackgroundColor = AStyle.BackgroudColor;
            FRowSpan = 0;
            FColSpan = 0;
        }

        ~HCTableCell()
        {
            
        }

        public override void Dispose()
        {
            base.Dispose();
            FCellData.Dispose();
        }

        public bool IsMergeSource()
        {
            return (FCellData == null);
        }

        public bool IsMergeDest()
        {
            return ((FRowSpan > 0) || (FColSpan > 0));
        }

        /// <summary> 清除并返回为处理分页比净高增加的高度(为重新格式化时后面计算偏移用) </summary>
        public int ClearFormatExtraHeight()
        {
            int Result = 0;
            if (FCellData != null)
                Result = FCellData.ClearFormatExtraHeight();

            return Result;
        }

        public virtual void SaveToStream(Stream AStream)
        {
            /* 因为可能是合并后的单元格，所以单独存宽、高 }*/
            byte[] vBuffer = BitConverter.GetBytes(FWidth);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FHeight);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FRowSpan);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FColSpan);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            byte vByte = (byte)FAlignVert;
            AStream.WriteByte(vByte);  // 垂直对齐方式

            HC.SaveColorToStream(AStream, FBackgroundColor); // 背景色

            AStream.WriteByte(FBorderSides.Value);

            /* 存数据 }*/
            bool vNullData = (FCellData == null);
            vBuffer = BitConverter.GetBytes(vNullData);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            if (!vNullData)
                FCellData.SaveToStream(AStream);
        }

        public void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            byte[] vBuffer = BitConverter.GetBytes(FWidth);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FWidth = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FHeight);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FHeight = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FRowSpan);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FRowSpan = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FColSpan);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FColSpan = BitConverter.ToInt32(vBuffer, 0);

            if (AFileVersion > 11)
            {
                byte vByte = 0;
                vByte = (byte)AStream.ReadByte();
                FAlignVert = (View.AlignVert)vByte;  // 垂直对齐方式

                HC.LoadColorFromStream(AStream, ref FBackgroundColor);  // 背景色
            }
            if (AFileVersion > 13)
            {
                FBorderSides.Value = (byte)AStream.ReadByte(); // load FBorderSides              
            }

            bool vNullData = false;
            vBuffer = BitConverter.GetBytes(vNullData);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            vNullData = BitConverter.ToBoolean(vBuffer, 0);
            if (!vNullData)
            {
                FCellData.LoadFromStream(AStream, AStyle, AFileVersion);
                FCellData.CellHeight = FHeight;
            }
            else
            {
                FCellData.Dispose();
                FCellData = null;
            }
        }

        public void GetCaretInfo(int AItemNo, int  AOffset, ref HCCaretInfo ACaretInfo)
        {
            if (FCellData != null)
            {
                FCellData.GetCaretInfo(AItemNo, AOffset, ref ACaretInfo);
                if (ACaretInfo.Visible)
                {
                    if (FAlignVert == AlignVert.cavBottom)
                        ACaretInfo.Y = ACaretInfo.Y + FHeight - FCellData.Height;
                    else
                    if (FAlignVert == AlignVert.cavCenter)
                        ACaretInfo.Y = ACaretInfo.Y + (FHeight - FCellData.Height) / 2;
                }
            }
            else
                ACaretInfo.Visible = false;
        }

        /// <summary> 绘制数据 </summary>
        /// <param name="ADataDrawLeft">绘制目标区域Left</param>
        /// <param name="ADataDrawTop">绘制目标区域的Top</param>
        /// <param name="ADataDrawBottom">绘制目标区域的Bottom</param>
        /// <param name="ADataScreenTop">屏幕区域Top</param>
        /// <param name="ADataScreenBottom">屏幕区域Bottom</param>
        /// <param name="AVOffset">指定从哪个位置开始的数据绘制到目标区域的起始位置</param>
        /// <param name="ACanvas">画布</param>
        public  void PaintData(int ADataDrawLeft, int ADataDrawTop, int ADataDrawBottom, 
            int ADataScreenTop, int ADataScreenBottom, int AVOffset, 
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (FCellData != null)
            {
                int vTop = 0;
                switch (FAlignVert)
                {
                    case View.AlignVert.cavTop: 
                        vTop = ADataDrawTop;
                        break;

                    case View.AlignVert.cavBottom: 
                        vTop = ADataDrawTop + FHeight - FCellData.Height;
                        break;

                    case View.AlignVert.cavCenter: 
                        vTop = ADataDrawTop + (FHeight - FCellData.Height) / 2;
                        break;
                }
            
                FCellData.PaintData(ADataDrawLeft, vTop, ADataDrawBottom, ADataScreenTop,
                    ADataScreenBottom, AVOffset, ACanvas, APaintInfo);
            }
        }

        public HCTableCellData CellData
        {
            get { return FCellData; }
            set { FCellData = value; }
        }

        /// <summary> 单元格宽度(含CellHPadding)，数据的宽度在TableItem中处理 </summary>
        public int Width
        {
            get { return FWidth; }
            set { FWidth = value; }
        }

        /// <summary> 单元格高度(含CellVPadding * 2 主要用于合并目标单元格，如果发生合并，则>=数据高度) </summary>
        public int Height
        {
            get { return FHeight; }
            set { SetHeight(value); }
        }

        public int RowSpan
        {
            get { return FRowSpan; }
            set { FRowSpan = value; }
        }

        public int ColSpan
        {
            get { return FColSpan; }
            set { FColSpan = value; }
        }

        public Color BackgroundColor
        {
            get { return FBackgroundColor; }
            set { FBackgroundColor = value; }
        }

        // 用于表格切换编辑的单元格
        public bool Active
        {
            get { return GetActive(); }
            set { SetActive(value); }
        }

        public AlignVert AlignVert
        {
            get { return FAlignVert; }
            set { FAlignVert = value; }
        }

        public HCBorderSides BorderSides
        {
            get { return FBorderSides; }
            set { FBorderSides = value; }
        }

    }
}
