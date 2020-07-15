/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                文本类的HCItem基类单元                 }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace HC.View
{
    public class HCTextItem : HCCustomItem
    {
        private string FText = "";
        private string FHyperLink = "";

        public static Type HCDefaultTextItemClass = typeof(HCTextItem);

        protected override string GetText()
        {
            return FText;
        }

        protected override void SetText(string value)
        {
            FText = HC.HCDeleteBreak(value);
        }

        protected override string GetHyperLink()
        {
            return FHyperLink;
        }

        protected override void SetHyperLink(string value)
        {
            FHyperLink = value;
        }

        public override int GetLength()
        {
            return FText.Length;
        }

        public HCTextItem() : base()
        {

        }

        public HCTextItem(string aText)
            : this()  // =CreateByText
        {
            FText = aText;
            FHyperLink = "";
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FText = (source as HCTextItem).Text;
            FHyperLink = (source as HCTextItem).HyperLink;
        }

        public override HCCustomItem BreakByOffset(int aOffset)
        {
            HCCustomItem Result = null;
            if ((aOffset >= Length) || (aOffset <= 0))
            {

            }
            else
            {
                Result = base.BreakByOffset(aOffset);
                Result.Text = this.SubString(aOffset + 1, Length - aOffset);
                FText = FText.Substring(0, aOffset);  // 当前Item减去光标后的字符串
            }

            return Result;
        }

        public override bool CanConcatItems(HCCustomItem aItem)
        {
            bool vResult = base.CanConcatItems(aItem);
            if (vResult)
                vResult = FHyperLink == aItem.HyperLink;

            return vResult;
        }

        // 保存和读取
        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            string vS = SubString(aStart + 1, aEnd - aStart);
            
            byte[] vBuffer = System.Text.Encoding.Unicode.GetBytes(vS);
            uint vDSize = (uint)vBuffer.Length;

            if (vDSize > HC.HC_TEXTMAXSIZE)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            byte[] vBytes = System.BitConverter.GetBytes(vDSize);
            aStream.Write(vBytes, 0, vBytes.Length);
           
            if (vDSize > 0)
                aStream.Write(vBuffer, 0, vBuffer.Length);

            HC.HCSaveTextToStream(aStream, FHyperLink);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            uint vDSize = 0;

            if (aFileVersion < 11)
            {
                byte[] vBuffer = new byte[2];
                aStream.Read(vBuffer, 0, 2);
                vDSize = System.BitConverter.ToUInt32(vBuffer, 0);
            }
            else
            {
                byte[] vBuffer = BitConverter.GetBytes(vDSize);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vDSize = System.BitConverter.ToUInt32(vBuffer, 0);
            }

            if (vDSize > 0)
            {
                byte[] vBuffer = new byte[vDSize];
                aStream.Read(vBuffer, 0, vBuffer.Length);
                if (aFileVersion > 24)
                    FText = System.Text.Encoding.Unicode.GetString(vBuffer);
                else
                    FText = System.Text.Encoding.Default.GetString(vBuffer);
            }

            if (aFileVersion > 34)
                HC.HCLoadTextFromStream(aStream, ref FHyperLink, aFileVersion);
            else
                FHyperLink = "";
        }

        public override string ToHtml(string aPath)
        {
            return "<a class=\"fs" + StyleNo.ToString() + "\">" + Text + "</a>";
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FHyperLink != "")
                aNode.SetAttribute("link", FHyperLink);

            aNode.InnerText = Text;
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            if (aNode.HasAttribute("link"))
                FHyperLink = aNode.GetAttribute("link");

            FText = aNode.InnerText;
        }

        /// <summaryy 复制一部分文本 </summary>
        /// <param name="AStartOffs">复制的起始位置(大于0)</param>
        /// <param name="ALength">众起始位置起复制的长度</param>
        /// <returns>文本内容</returns>
        public string SubString(int aStartOffs, int aLength)
        {
            return FText.Substring(aStartOffs - 1, aLength);
        }
    }
}
