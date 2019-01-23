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

        protected override void SetText(string value)
        {
            if (FText != value)
            {
                FText = value;
            }
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom,
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            // 绘制二维码
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                aCanvas, aPaintInfo);
        }

        public HCQRCodeItem(HCCustomData aOwnerData, string aText) : base(aOwnerData)
        {
            StyleNo = HCStyle.QRCode;
            FText = aText;
            Width = 100;
            Height = 100;
        }

        ~HCQRCodeItem()
        {

        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            int vLen = System.Text.Encoding.Default.GetByteCount(FText);
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            ushort vSize = (ushort)vLen;

            byte[] vBuffer = BitConverter.GetBytes(vSize);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            if (vSize > 0)
            {
                vBuffer = System.Text.Encoding.Default.GetBytes(FText);
                aStream.Write(vBuffer, 0, vBuffer.Length);
            }
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            ushort vSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vSize = BitConverter.ToUInt16(vBuffer, 0);

            if (vSize > 0)
            {
                vBuffer = new byte[vSize];
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FText = System.Text.Encoding.Default.GetString(vBuffer);
            }
        }

        /// <summary> 约束到指定大小范围内 </summary>
        public override void RestrainSize(int aWidth, int  aHeight)
        {
            if (Width > aWidth)
            {
                Single vBL = (float)Width / aWidth;
                Width = aWidth;
                Height = (int)Math.Round(Height / vBL);
            }

            if (Height > aHeight)
            {
                Single vBL = (float)Height / aHeight;
                Height = aHeight;
                Width = (int)Math.Round(Width / vBL);
            }
        }
    }
}
