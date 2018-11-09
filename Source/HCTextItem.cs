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

namespace HC.View
{
    class HCTextItem : HCCustomItem
    {
        private string FText;

        public static Type HCDefaultTextItemClass = typeof(HCTextItem);

        protected override string GetText()
        {
            return FText;
        }

        protected override void SetText(string Value)
        {
            FText = Value;
        }

        public override int GetLength()
        {
            return FText.Length;
        }

        public HCTextItem()
            : base()
        {

        }

        public HCTextItem(string AText) : this()
        {
            FText = AText;
        }

        //public virtual HCTextItem CreateByText(string AText)
        //{
        //    HCTextItem vItem = new HCTextItem();
        //    vItem.FText = AText;
        //    return vItem;
        //}

        /// <summaryy 可接受输入 </summary>
        public virtual bool CanAccept(int AOffset)
        {
            return true;
        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            FText = (Source as HCTextItem).Text;
        }

        public override HCCustomItem BreakByOffset(int AOffset)
        {
            HCCustomItem Result = null;
            if ((AOffset >= Length) || (AOffset <= 0))
            {

            }
            else
            {
                Result = base.BreakByOffset(AOffset);
                Result.Text = this.GetTextPart(AOffset + 1, Length - AOffset);
                FText = FText.Substring(0, AOffset);  // 当前Item减去光标后的字符串
            }

            return Result;
        }

        // 保存和读取
        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            string vS = GetTextPart(AStart + 1, AEnd - AStart);
            
            byte[] vBuffer = System.Text.Encoding.Default.GetBytes(vS);
            uint vDSize = (uint)vBuffer.Length;

            if (vDSize > HC.HC_TEXTMAXSIZE)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            byte[] vBytes = System.BitConverter.GetBytes(vDSize);
            AStream.Write(vBytes, 0, vBytes.Length);
           
            if (vDSize > 0)
                AStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);

            uint vDSize = 0;

            if (AFileVersion < 11)
            {
                byte[] vBuffer = new byte[2];
                AStream.Read(vBuffer, 0, 2);
                vDSize = System.BitConverter.ToUInt32(vBuffer, 0);
            }
            else
            {
                byte[] vBuffer = BitConverter.GetBytes(vDSize);
                AStream.Read(vBuffer, 0, vBuffer.Length);
                vDSize = System.BitConverter.ToUInt32(vBuffer, 0);
            }

            if (vDSize > 0)
            {
                byte[] vBuffer = new byte[vDSize];
                AStream.Read(vBuffer, 0, vBuffer.Length);
                FText = System.Text.Encoding.Default.GetString(vBuffer);
            }
        }

        /// <summaryy 复制一部分文本 </summary>
        /// <param name="AStartOffs">复制的起始位置(大于0)</param>
        /// <param name="ALength">众起始位置起复制的长度</param>
        /// <returns>文本内容</returns>
        public string GetTextPart(int AStartOffs, int ALength)
        {
            return FText.Substring(AStartOffs - 1, ALength);
        }
    }
}
