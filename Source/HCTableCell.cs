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
using System.Xml;
using System.Windows.Forms;

namespace HC.View
{
    public enum TableSite : byte
    {
        tsOutside,  // 表格外面
        tsCell,  // 单元格中
        tsBorderLeft,  // 只有第一列使用此元素
        tsBorderTop,    // 只有第一行使用此元素
        tsBorderRight,  // 第X列右边
        tsBorderBottom  // 第X行下边
    }

    public struct ResizeInfo  // 缩放信息
    {
        public TableSite TableSite;
        public int DestX, DestY;
    }

    public struct OutsideInfo  // 表格外面信息
    {
        public int Row;  // 外面位置处对应的行
        public bool Leftside;  // True：左边 False：右边
    }

    public class SelectCellRang : HCObject
    {
        private int
            FStartRow,  // 选中起始行
            FStartCol,  // 选中起始列
            FEndRow,    // 选中结束行
            FEndCol;     // 选中结束列

        public SelectCellRang()
        {
            Initialize();
        }

        /// <summary> 初始化字段和变量 </summary>
        public void Initialize()
        {
            FStartRow = -1;
            FStartCol = -1;
            InitializeEnd();
        }

        public void InitializeEnd()
        {
            FEndRow = -1;
            FEndCol = -1;
        }

        public void SetStart(int aRow, int aCol)
        {
            FStartRow = aRow;
            FStartCol = aCol;
        }

        public void SetEnd(int aRow, int aCol)
        {
            FEndRow = aRow;
            FEndCol = aCol;
        }

        /// <summary> 在同一单元中编辑 </summary>
        public bool EditCell()
        {
            return ((FStartRow >= 0) && (FEndRow < 0));
        }

        /// <summary> 选中在同一行 </summary>
        public bool SameRow()
        {
            return ((FStartRow >= 0) && (FStartRow == FEndRow));
        }

        /// <summary> 选中在同一列 </summary>
        public bool SameCol()
        {
            return ((FStartCol >= 0) && (FStartCol == FEndCol));
        }

        /// <summary> 选中1-n个单元格 </summary>
        public bool SelectExists()
        {
            return ((FStartRow >= 0) && (FEndRow >= 0));
        }

        public int StartRow
        {
            get { return FStartRow; }
            set { FStartRow = value; }
        }

        public int StartCol
        {
            get { return FStartCol; }
            set { FStartCol = value; }
        }

        public int EndRow
        {
            get { return FEndRow; }
            set { FEndRow = value; }
        }

        public int EndCol
        {
            get { return FEndCol; }
            set { FEndCol = value; }
        }
    }

    /// <summary> 垂直对齐方式：上、居中、下) </summary>
    public enum HCAlignVert : byte
    {
        cavTop = 0, cavCenter = 1, cavBottom = 2
    }

    public class HCTableCell : HCObject
    {
        private HCTableCellData FCellData;

        private int FWidth;  // 被合并后记录原始宽(否则当行第一列被合并后，第二列无法确认水平起始位置)
        private int FHeight ;  // 被合并后记录原始高、记录拖动改变后高
        private int FRowSpan;  // 单元格跨几行，用于合并目标单元格记录合并了几行，合并源记录合并到单元格的行号，0没有行合并
        private int FColSpan ;  // 单元格跨几列，用于合并目标单元格记录合并了几列，合并源记录合并到单元格的列号，0没有列合并

        private Color FBackgroundColor;
        private HCAlignVert FAlignVert;
        private HCBorderSides FBorderSides;

        private int GetCellDataTop(byte aCellVPadding)
        {
            switch (FAlignVert)
            {
                case HCAlignVert.cavTop:
                    return aCellVPadding;

                case HCAlignVert.cavCenter: 
                    return (FHeight - aCellVPadding - FCellData.Height - aCellVPadding) / 2;

                default: 
                    return FHeight - aCellVPadding - FCellData.Height;
            }
        }

        protected bool GetActive()
        {
            if (FCellData != null)
                return FCellData.Active;
            else
                return false;
        }

        protected void SetActive(bool value)
        {
            if (FCellData != null)
                FCellData.Active = value;
        }

        protected void SetHeight(int value)
        {
             if (FHeight != value)
            {
                FHeight = value;
                if (FCellData != null)
                    FCellData.CellHeight = value;
            }
        }

        public HCTableCell()
        {

        }

        public HCTableCell(HCStyle AStyle) : this()
        {
            FCellData = new HCTableCellData(AStyle);
            FAlignVert = View.HCAlignVert.cavTop;
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

        public void MouseDown(MouseEventArgs e, byte aCellHPadding, byte aCellVPadding)
        {
            if (FCellData != null)
            {
                MouseEventArgs vEvent = new MouseEventArgs(e.Button, e.Clicks, e.X - aCellHPadding,
                    e.Y - GetCellDataTop(aCellVPadding), e.Delta);
                FCellData.MouseDown(vEvent);
            }
        }

        public void MouseMove(MouseEventArgs e, byte aCellHPadding, byte aCellVPadding)
        {
            if (FCellData != null)
            {
                MouseEventArgs vEvent = new MouseEventArgs(e.Button, e.Clicks, e.X - aCellHPadding,
                    e.Y - GetCellDataTop(aCellVPadding), e.Delta);
                FCellData.MouseMove(vEvent);
            }
        }

        public void MouseUp(MouseEventArgs e, byte aCellHPadding, byte aCellVPadding)
        {
            if (FCellData != null)
            {
                MouseEventArgs vEvent = new MouseEventArgs(e.Button, e.Clicks, e.X - aCellHPadding,
                    e.Y - GetCellDataTop(aCellVPadding), e.Delta);
                FCellData.MouseUp(vEvent);
            }
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
            if (FCellData != null)
                return FCellData.ClearFormatExtraHeight();
            else
                return 0;
        }

        public virtual void SaveToStream(Stream aStream)
        {
            /* 因为可能是合并后的单元格，所以单独存宽、高 }*/
            byte[] vBuffer = BitConverter.GetBytes(FWidth);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FHeight);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FRowSpan);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FColSpan);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            byte vByte = (byte)FAlignVert;
            aStream.WriteByte(vByte);  // 垂直对齐方式

            HC.HCSaveColorToStream(aStream, FBackgroundColor); // 背景色

            aStream.WriteByte(FBorderSides.Value);

            // 存数据
            bool vNullData = (FCellData == null);
            vBuffer = BitConverter.GetBytes(vNullData);
            aStream.Write(vBuffer, 0, vBuffer.Length);
            if (!vNullData)
                FCellData.SaveToStream(aStream);
        }

        public void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            byte[] vBuffer = BitConverter.GetBytes(FWidth);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FWidth = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FHeight);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FHeight = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FRowSpan);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FRowSpan = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FColSpan);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FColSpan = BitConverter.ToInt32(vBuffer, 0);

            if (aFileVersion > 11)
            {
                byte vByte = 0;
                vByte = (byte)aStream.ReadByte();
                FAlignVert = (View.HCAlignVert)vByte;  // 垂直对齐方式

                HC.HCLoadColorFromStream(aStream, ref FBackgroundColor);  // 背景色
            }
            if (aFileVersion > 13)
            {
                FBorderSides.Value = (byte)aStream.ReadByte(); // load FBorderSides              
            }

            bool vNullData = false;
            vBuffer = BitConverter.GetBytes(vNullData);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vNullData = BitConverter.ToBoolean(vBuffer, 0);
            if (!vNullData)
            {
                FCellData.LoadFromStream(aStream, aStyle, aFileVersion);
                FCellData.CellHeight = FHeight;
            }
            else
            {
                FCellData.Dispose();
                FCellData = null;
            }
        }

        public void ToXml(XmlElement aNode)
        {
            aNode.SetAttribute("width", FWidth.ToString());
            aNode.SetAttribute("height", FHeight.ToString());
            aNode.SetAttribute("rowspan", FRowSpan.ToString());
            aNode.SetAttribute("colspan", FColSpan.ToString());
            aNode.SetAttribute("vert", ((byte)FAlignVert).ToString());
            aNode.SetAttribute("bkcolor", HC.GetColorXmlRGB(FBackgroundColor));
            aNode.SetAttribute("border", HC.GetBorderSidePro(FBorderSides));

            if (FCellData != null)  // 存数据
            {
                XmlElement vNode = aNode.OwnerDocument.CreateElement("items");
                FCellData.ToXml(vNode);
                aNode.AppendChild(vNode);
            }
        }

        public void ParseXml(XmlElement aNode)
        {
            FWidth = int.Parse(aNode.Attributes["width"].Value);
            FHeight = int.Parse(aNode.Attributes["height"].Value);
            FRowSpan = int.Parse(aNode.Attributes["rowspan"].Value);
            FColSpan = int.Parse(aNode.Attributes["colspan"].Value);
            FAlignVert = (HCAlignVert)(byte.Parse(aNode.Attributes["vert"].Value));
            FBackgroundColor = HC.GetXmlRGBColor(aNode.Attributes["bkcolor"].Value);
            HC.SetBorderSideByPro(aNode.Attributes["border"].Value, FBorderSides);

            if ((FRowSpan < 0) || (FColSpan < 0))
            {
                FCellData.Dispose();
                FCellData = null;
            }
            else
            {
                FCellData.Width = FWidth;  // // 不准确的赋值，应该减去2个水平padding，加载时使用无大碍
                FCellData.ParseXml(aNode.SelectSingleNode("items") as XmlElement);
            }
        }

        public void GetCaretInfo(int aItemNo, int aOffset, byte aCellHPadding, byte aCellVPadding, ref HCCaretInfo aCaretInfo)
        {
            if (FCellData != null)
            {
                FCellData.GetCaretInfo(aItemNo, aOffset, ref aCaretInfo);
                if (aCaretInfo.Visible)
                {
                    aCaretInfo.X += aCellHPadding;
                    aCaretInfo.Y += GetCellDataTop(aCellVPadding);
                }
            }
            else
                aCaretInfo.Visible = false;
        }

        /// <summary> 绘制数据 </summary>
        /// <param name="aDataDrawLeft">绘制目标区域Left</param>
        /// <param name="aDataDrawTop">绘制目标区域的Top</param>
        /// <param name="aDataDrawBottom">绘制目标区域的Bottom</param>
        /// <param name="aDataScreenTop">屏幕区域Top</param>
        /// <param name="aDataScreenBottom">屏幕区域Bottom</param>
        /// <param name="aVOffset">指定从哪个位置开始的数据绘制到目标区域的起始位置</param>
        /// <param name="ACanvas">画布</param>
        public void PaintTo(int aDrawLeft, int aDrawTop, int aDrawRight, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, int aVOffset, byte aCellHPadding, byte aCellVPadding,
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (FCellData != null)
            {
                int vTop = aDrawTop + GetCellDataTop(aCellVPadding);
                FCellData.PaintData(aDrawLeft + aCellHPadding, vTop, aDrawRight - aCellHPadding, 
                    aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aVOffset, ACanvas, APaintInfo);
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

        public HCAlignVert AlignVert
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
