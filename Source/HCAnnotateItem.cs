/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2021-9-9              }
{                                                       }
{               文档批注对象基类实现单元                }
{                                                       }
{*******************************************************/

using HC.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace HC.View
{
    public class HCAnnotateContent
    {
        public string Title, Text;

        public void SaveToStream(Stream stream)
        {
            HC.HCSaveTextToStream(stream, Title);
            HC.HCSaveTextToStream(stream, Text);
        }

        public void LoadFromStream(Stream stream, ushort fileVersion)
        {
            HC.HCLoadTextFromStream(stream, ref Title, fileVersion);
            HC.HCLoadTextFromStream(stream, ref Text, fileVersion);
        }

        public void ToXml(XmlElement aNode)
        {
            aNode.SetAttribute("title", Title);
            aNode.InnerText = Text;
        }

        public void ParseXml(XmlElement aNode)
        {
            Title = aNode.Attributes["title"].Value;
            Text = aNode.InnerText;
        }
    }

    public class HCAnnotateItem : HCCustomRectItem
    {
        private RECT FDrawRect;
        private HCAnnotateContent FContent;
        private List<HCAnnotateContent> FReplys;

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom,
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                aCanvas, aPaintInfo);

            if (!aPaintInfo.Print)
            {
                FDrawRect.ReSet(aDrawRect);
                aPaintInfo.TopItems.Add(this);
            }
        }

        public uint ID;
        public MarkType MarkType;

        public HCAnnotateItem(HCCustomData aOwnerData)
            : base(aOwnerData)
        {
            this.StyleNo = HCStyle.Annotate;
            FDrawRect = new RECT();
            FContent = new HCAnnotateContent();
            FReplys = new List<HCAnnotateContent>();
            ID = 0;
            Width = 0;
            Height = 0;
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FContent.Text = (source as HCAnnotateItem).Content.Text;
            FContent.Title = (source as HCAnnotateItem).Content.Title;
            ID = (source as HCAnnotateItem).ID;
        }

        public bool IsBeginMark()
        {
            return MarkType == MarkType.cmtBeg;
        }

        public bool IsEndMark()
        {
            return MarkType == MarkType.cmtEnd;
        }

        public override int GetOffsetAt(int x)
        {
            if (x >= 0 && x <= Width)
            {
                if (MarkType == View.MarkType.cmtBeg)
                    return HC.OffsetAfter;
                else
                    return HC.OffsetBefor;
            }
            else
                return base.GetOffsetAt(x);
        }

        public override bool JustifySplit()
        {
            return false;
        }

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            this.Width = 0;
            this.Height = aRichData.Style.TextStyles[0].FontHeight - aRichData.Style.LineSpaceMin;

            if (this.MarkType == MarkType.cmtBeg)
            {
                if (aItemNo < aRichData.Items.Count - 1)
                {
                    HCCustomItem vItem = aRichData.Items[aItemNo + 1];
                    if (vItem.StyleNo == this.StyleNo
                        && (vItem as HCAnnotateItem).MarkType == MarkType.cmtEnd)
                    {
                        this.Width = 10;
                    }
                    else
                    if (vItem.ParaFirst)
                        this.Width = 10;
                }
                else
                    this.Width = 10;
            }
            else
            {
                HCCustomItem vItem = aRichData.Items[aItemNo - 1];
                if (vItem.StyleNo == this.StyleNo && (vItem as HCAnnotateItem).MarkType == MarkType.cmtBeg)
                    this.Width = 10;
                else
                if (this.ParaFirst)
                    this.Width = 10;
            }
        }

        public override void PaintTop(HCCanvas aCanvas)
        {
            base.PaintTop(aCanvas);

            aCanvas.Pen.Width = 1;
            if (MarkType == View.MarkType.cmtBeg)
            {
                aCanvas.Pen.BeginUpdate();
                try
                {
                    aCanvas.Pen.Width = 1;
                    aCanvas.Pen.Style = HCPenStyle.psSolid;
                    aCanvas.Pen.Color = Color.FromArgb(255, 0, 0);
                }
                finally
                {
                    aCanvas.Pen.EndUpdate();
                }

                //aCanvas.MoveTo(FDrawRect.Left + 2, FDrawRect.Top - 1);
                aCanvas.MoveTo(FDrawRect.Left, FDrawRect.Top - 1);
                aCanvas.LineTo(FDrawRect.Left, FDrawRect.Bottom + 1);
                //aCanvas.LineTo(FDrawRect.Left + 2, FDrawRect.Bottom + 1);
            }
            else
            {
                aCanvas.Pen.BeginUpdate();
                try
                {
                    aCanvas.Pen.Width = 1;
                    aCanvas.Pen.Style = HCPenStyle.psSolid;
                    aCanvas.Pen.Color = Color.FromArgb(255, 0, 0);
                }
                finally
                {
                    aCanvas.Pen.EndUpdate();
                }

                //aCanvas.MoveTo(FDrawRect.Right - 2, FDrawRect.Top - 1);
                aCanvas.MoveTo(FDrawRect.Right, FDrawRect.Top - 1);
                aCanvas.LineTo(FDrawRect.Right, FDrawRect.Bottom + 1);
                //aCanvas.LineTo(FDrawRect.Right - 2, FDrawRect.Bottom + 1);
            }
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);
            byte[] vBuffer = BitConverter.GetBytes(ID);
            aStream.Write(vBuffer, 0, vBuffer.Length);
            aStream.WriteByte((byte)MarkType);
            if (MarkType == MarkType.cmtEnd)
            {
                FContent.SaveToStream(aStream);
                int vRepCount = FReplys.Count;
                vBuffer = BitConverter.GetBytes(vRepCount);
                aStream.Write(vBuffer, 0, vBuffer.Length);
                for (int i = 0; i < vRepCount; i++)
                    FReplys[i].SaveToStream(aStream);
            }
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            byte[] vBuffer = BitConverter.GetBytes(ID);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            ID = BitConverter.ToUInt32(vBuffer, 0);
            MarkType = (MarkType)aStream.ReadByte();
            if (MarkType == MarkType.cmtEnd)
            {
                FContent.LoadFromStream(aStream, aFileVersion);
                int vRepCount = 0;
                vBuffer = BitConverter.GetBytes(vRepCount);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vRepCount = BitConverter.ToInt32(vBuffer, 0);
                if (vRepCount > 0)
                {
                    HCAnnotateContent vRepContent = null;
                    for (int i = 0; i < vRepCount; i++)
                    {
                        vRepContent = new HCAnnotateContent();
                        vRepContent.LoadFromStream(aStream, aFileVersion);
                        FReplys.Add(vRepContent);
                    }
                }
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("id", ID.ToString());
            aNode.SetAttribute("mark", ((byte)MarkType).ToString());
            if (MarkType == MarkType.cmtEnd)
            {
                XmlElement vNode = aNode.OwnerDocument.CreateElement("content");
                FContent.ToXml(vNode);
                aNode.AppendChild(vNode);

                if (FReplys.Count > 0)
                {
                    vNode = aNode.OwnerDocument.CreateElement("replys");
                    XmlElement vNodeRP = null;
                    for (int i = 0; i < FReplys.Count; i++)
                    {
                        vNodeRP = aNode.OwnerDocument.CreateElement("rp");
                        FReplys[i].ToXml(vNodeRP);
                        vNode.AppendChild(vNodeRP);
                    }

                    aNode.AppendChild(vNode);
                }
            }
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            ID = uint.Parse(aNode.Attributes["id"].Value);
            MarkType = (MarkType)byte.Parse(aNode.Attributes["mark"].Value);
            if (MarkType == MarkType.cmtEnd)
            {
                XmlElement vNode = null;
                HCAnnotateContent vRepContent = null;
                for (int i = 0; i < aNode.ChildNodes.Count; i++)
                {
                    if (aNode.ChildNodes[i].Name == "content")
                    {
                        vNode = aNode.ChildNodes[i] as XmlElement;
                        FContent.ParseXml(vNode);
                    }
                    else
                    if (aNode.ChildNodes[i].Name == "rp")
                    {
                        vNode = aNode.ChildNodes[i] as XmlElement;
                        vRepContent = new HCAnnotateContent();
                        vRepContent.ParseXml(vNode);
                        FReplys.Add(vRepContent);
                    }
                }
            }
        }

        public HCAnnotateContent Content
        {
            get { return FContent; }
        }

        public List<HCAnnotateContent> Replys
        {
            get { return FReplys; }
        }
    }
}
