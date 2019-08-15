/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                    表格行实现单元                     }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace HC.View
{
    public class HCTableRow : HCList<HCTableCell>
    {
        private int FHeight,  // 行高，不带上下边框 = 行中单元格最高的(单元格高包括单元格为分页而在某行额外偏移的高度)
            FFmtOffset;  // 格式化时的偏移，主要是处理当前行整体下移到下一页时，简化上一页底部的鼠标点击、移动时的计算;

        private bool FAutoHeight;  // True根据内容自动匹配合适的高度 False用户拖动后的自定义高度

        public HCTableRow(HCStyle aStyle, int aColCount) : this()
        {
            HCTableCell vCell = null;
            for (int i = 0; i <= aColCount - 1; i++)
            {
                vCell = new HCTableCell(aStyle);
                this.Add(vCell);
            }
            FAutoHeight = true;
        }

        public HCTableRow()
        {

        }

         ~HCTableRow()
        {

        }

        public void SetRowWidth(int aWidth)
        {
            int vWidth = aWidth / this.Count;
            for (int i = 0; i <= this.Count - 2; i++)
            {
                this[i].Width = vWidth;
            }

            this[this.Count - 1].Width = aWidth - (this.Count - 1) * vWidth;
        }

        public void SetHeight(int value)
        {
            if (FHeight != value)
            {
                int vMaxDataHeight = 0;
                for (int i = 0; i <= this.Count - 1; i++)  // 找行中最高的单元
                {
                    if ((this[i].CellData != null) && (this[i].RowSpan == 0))
                    {
                        if (this[i].CellData.Height > vMaxDataHeight)
                        vMaxDataHeight = this[i].CellData.Height;
                    }
                }

                if (vMaxDataHeight < value)
                    FHeight = value;
                else
                    FHeight = vMaxDataHeight;
                
                for (int i = 0; i <= this.Count - 1; i++)
                    this[i].Height = FHeight;
            }
        }

        public void ToXml(XmlElement aNode)
        {
            aNode.SetAttribute("autoheight", FAutoHeight.ToString());
            aNode.SetAttribute("height", FHeight.ToString());
            for (int i = 0; i <= this.Count - 1; i++)
            {
                XmlElement vNode = aNode.OwnerDocument.CreateElement("cell");
                this[i].ToXml(vNode);
                aNode.AppendChild(vNode);
            }
        }

        public void ParseXml(XmlElement aNode)
        {
            FAutoHeight = bool.Parse(aNode.Attributes["autoheight"].Value);
            FHeight = int.Parse(aNode.Attributes["height"].Value);
            for (int i = 0; i <= aNode.ChildNodes.Count - 1; i++)
                this[i].ParseXml(aNode.ChildNodes[i] as XmlElement);
        }

        //property Capacity: Integer read FCapacity write SetCapacity;
        public int ColCount
        {
            get { return this.Count; }
        }

        /// <summary> 当前行中所有没有发生合并单元格的高度(含CellVPadding * 2因为会受有合并列的影响，所以>=数据高度) </summary>
        public int Height
        {
            get { return FHeight; }
            set { SetHeight(value); }
        }

        public bool AutoHeight
        {
            get { return FAutoHeight; }
            set { FAutoHeight = value; }
        }

        /// <summary>因跨页向下整体偏移的量</summary>
        public int FmtOffset
        {
            get { return FFmtOffset; }
            set { FFmtOffset = value; }
        }
    }
}
