/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{          文档QRCodeItem(二维码)对象实现单元           }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;
using System.IO;

namespace HC.View
{
    public class HCQRCodeItem : HCResizeRectItem
    {
        private string FText;

        protected override string GetText()
        {
            return FText;
        }

        protected override void SetText(string Value)
        {
            if (FText != Value)
            {
                FText = Value;
            }
        }

        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, int ADataDrawTop, int ADataDrawBottom,
            int ADataScreenTop, int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            // 绘制二维码
            base.DoPaint(AStyle, ADrawRect, ADataDrawTop, ADataDrawBottom, ADataScreenTop, ADataScreenBottom,
                ACanvas, APaintInfo);
        }

        public HCQRCodeItem(HCCustomData AOwnerData, string AText) : base(AOwnerData)
        {
            StyleNo = HCStyle.QRCode;
            FText = AText;
            Width = 100;
            Height = 100;
        }

        ~HCQRCodeItem()
        {

        }

        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            int vLen = System.Text.Encoding.Default.GetByteCount(FText);
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            ushort vSize = (ushort)vLen;

            byte[] vBuffer = BitConverter.GetBytes(vSize);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            if (vSize > 0)
            {
                vBuffer = System.Text.Encoding.Default.GetBytes(FText);
                AStream.Write(vBuffer, 0, vBuffer.Length);
            }
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);

            ushort vSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            vSize = BitConverter.ToUInt16(vBuffer, 0);

            if (vSize > 0)
            {
                vBuffer = new byte[vSize];
                AStream.Read(vBuffer, 0, vBuffer.Length);
                FText = System.Text.Encoding.Default.GetString(vBuffer);
            }
        }

        /// <summary> 约束到指定大小范围内 </summary>
        public override void RestrainSize(int AWidth, int  AHeight)
        {
            if (Width > AWidth)
            {
                Single vBL = (float)Width / AWidth;
                Width = AWidth;
                Height = (int)Math.Round(Height / vBL);
            }

            if (Height > AHeight)
            {
                Single vBL = (float)Height / AHeight;
                Height = AHeight;
                Width = (int)Math.Round(Width / vBL);
            }
        }
    }
}
