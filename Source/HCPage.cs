/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                 文档页对象实现单元                    }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace HC.View
{
    class HCPageSize : HCObject
    {
        private Single FPixelsPerMMX, FPixelsPerMMY;  // 1毫米像素数
        private PaperKind FPaperKind;  // 纸张大小如A4、B5等
        private Single FPaperWidth, FPaperHeight;  // 纸张宽、高（单位mm）
        private int FPageWidthPix, FPageHeightPix;  // 页面大小
        private Single FPaperMarginTop, FPaperMarginLeft, FPaperMarginRight, FPaperMarginBottom;  // 纸张边距（单位mm）
        private int FPageMarginTopPix, FPageMarginLeftPix, FPageMarginRightPix, FPageMarginBottomPix;  // 页边距

        protected void SetPaperKind(PaperKind Value)
        {
            if (FPaperKind != Value)
            {
                FPaperKind = Value;

                switch (FPaperKind )
                {
                    case PaperKind.A4:
                        PaperWidth = 210;
                        PaperHeight = 297;
                        break;
                    default:
                        PaperWidth = 210;
                        PaperHeight = 297;
                        break;
                }
            }
        }

        protected void SetPaperWidth(Single Value)
        {
            FPaperWidth = Value;
            FPageWidthPix = (int)Math.Round(FPaperWidth * FPixelsPerMMX);
        }

        protected void SetPaperHeight(Single Value)
        {
            FPaperHeight = Value;
            FPageHeightPix = (int)Math.Round(FPaperHeight * FPixelsPerMMY);
        }

        protected void SetPaperMarginTop(Single Value)
        {
            FPaperMarginTop = Value;
            FPageMarginTopPix = (int)Math.Round(FPaperMarginTop * FPixelsPerMMY);
        }

        protected void SetPaperMarginLeft(Single Value)
        {
            FPaperMarginLeft = Value;
            FPageMarginLeftPix = (int)Math.Round(FPaperMarginLeft * FPixelsPerMMX);
        }

        protected void SetPaperMarginRight(Single Value)
        {
            FPaperMarginRight = Value;
            FPageMarginRightPix = (int)Math.Round(FPaperMarginRight * FPixelsPerMMX);
        }

        protected void SetPaperMarginBottom(Single Value)
        {
            FPaperMarginBottom = Value;
            FPageMarginBottomPix = (int)Math.Round(FPaperMarginBottom * FPixelsPerMMY);
        }

        public HCPageSize(Single APixelsPerMMX, Single APixelsPerMMY)  // 屏幕1英寸dpi数
        {  
            FPixelsPerMMX = APixelsPerMMX;
            FPixelsPerMMY = APixelsPerMMY;
            PaperMarginLeft = 25;
            PaperMarginTop = 25;
            PaperMarginRight = 20;
            PaperMarginBottom = 20;
            PaperKind = PaperKind.A4;  // 默认A4 210 297
        }

        public void SaveToStream(Stream AStream)
        {
            Int64 vBegPos, vEndPos;
            vBegPos = AStream.Position;

            byte[] vBuffer = System.BitConverter.GetBytes(vBegPos);
            AStream.Write(vBuffer, 0, vBuffer.Length);  // 数据大小占位

            vBuffer = System.BitConverter.GetBytes((int)FPaperKind);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FPaperWidth);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FPaperHeight);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FPaperMarginLeft);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FPaperMarginTop);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FPaperMarginRight);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FPaperMarginBottom);
            AStream.Write(vBuffer, 0, vBuffer.Length);
  
            vEndPos = AStream.Position;
            AStream.Position = vBegPos;
            vBegPos = vEndPos - vBegPos - Marshal.SizeOf(vBegPos);

            vBuffer = System.BitConverter.GetBytes(vBegPos);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            AStream.Position = vEndPos;
        }

        public void LoadToStream(Stream AStream, ushort AFileVersion)
        {
            Int64 vDataSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vDataSize);

            AStream.Read(vBuffer, 0, vBuffer.Length);
            vDataSize = System.BitConverter.ToInt64(vBuffer, 0);

            vBuffer = System.BitConverter.GetBytes((int)FPaperKind);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            PaperKind = (PaperKind)BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FPaperWidth);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            PaperWidth = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FPaperHeight);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            PaperHeight = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FPaperMarginLeft);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            PaperMarginLeft = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FPaperMarginTop);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            PaperMarginTop = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FPaperMarginRight);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            PaperMarginRight = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FPaperMarginBottom);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            PaperMarginBottom = System.BitConverter.ToSingle(vBuffer, 0);
        }

        public PaperKind PaperKind
        {
            get { return FPaperKind; }
            set { SetPaperKind(value); }
        }

        public Single PaperWidth
        {
            get { return FPaperWidth; }
            set { SetPaperWidth(value); }
        }

        public Single PaperHeight
        {
            get { return FPaperHeight; }
            set { SetPaperHeight(value); }
        }

        public Single PaperMarginTop
        {
            get { return FPaperMarginTop; }
            set { SetPaperMarginTop(value); }
        }

        public Single PaperMarginLeft
        {
            get { return FPaperMarginLeft; }
            set { SetPaperMarginLeft(value); }
        }
        
        public Single PaperMarginRight
        {
            get { return FPaperMarginRight; }
            set { SetPaperMarginRight(value); }
        }

        public Single PaperMarginBottom
        {
            get { return FPaperMarginBottom; }
            set { SetPaperMarginBottom(value); }
        }

        public int PageWidthPix  // 页宽(含页左右边距)
        {
            get { return FPageWidthPix; }
            set { FPageWidthPix = value; }
        }

        public int PageHeightPix  // 页高(含页眉、页脚)
        {
            get { return FPageHeightPix; }
            set { FPageHeightPix = value; }
        }

        public int PageMarginTopPix 
        {
            get { return FPageMarginTopPix; }
            set { FPageMarginTopPix = value; }
        }

        public int PageMarginLeftPix  
        {
            get { return FPageMarginLeftPix; }
            set { FPageMarginLeftPix = value; }
        }

        public int PageMarginRightPix 
        {
            get { return FPageMarginRightPix; }
            set { FPageMarginRightPix = value; }
        }

        public int PageMarginBottomPix 
        {
            get { return FPageMarginBottomPix; }
            set { FPageMarginBottomPix = value; }
        }
    }

    public class HCPage
    {
        public int StartDrawItemNo, EndDrawItemNo;    // 起始，结束item

        public HCPage()
        {
            StartDrawItemNo = -1;    // 起始item
            EndDrawItemNo = -1;      // 结束item
        }

        public void Assign(HCPage Source)
        {
            StartDrawItemNo = Source.StartDrawItemNo;  // 起始item
            EndDrawItemNo = Source.EndDrawItemNo;  // 结束item
        }
    }

    class HCPages : List<HCPage>
    {
        public void ClearEx()
        {
            this.RemoveRange(1, this.Count - 1);
        }
    }
}
