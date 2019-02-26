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
    public class HCPageSize : HCObject
    {
        private PaperKind FPaperKind;  // 纸张大小如A4、B5等
        private Single FPaperWidth, FPaperHeight;  // 纸张宽、高（单位mm）
        private int FPageWidthPix, FPageHeightPix;  // 页面大小
        private Single FPaperMarginTop, FPaperMarginLeft, FPaperMarginRight, FPaperMarginBottom;  // 纸张边距（单位mm）
        private int FPageMarginTopPix, FPageMarginLeftPix, FPageMarginRightPix, FPageMarginBottomPix;  // 页边距

        protected void SetPaperKind(PaperKind value)
        {
            if (FPaperKind != value)
            {
                FPaperKind = value;

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

        protected void SetPaperWidth(Single value)
        {
            FPaperWidth = value;
            FPageWidthPix = HCUnitConversion.MillimeterToPixX(FPaperWidth);
        }

        protected void SetPaperHeight(Single value)
        {
            FPaperHeight = value;
            FPageHeightPix = HCUnitConversion.MillimeterToPixY(FPaperHeight);
        }

        protected void SetPaperMarginTop(Single value)
        {
            FPaperMarginTop = value;
            FPageMarginTopPix = HCUnitConversion.MillimeterToPixY(FPaperMarginTop);
        }

        protected void SetPaperMarginLeft(Single value)
        {
            FPaperMarginLeft = value;
            FPageMarginLeftPix = HCUnitConversion.MillimeterToPixX(FPaperMarginLeft);
        }

        protected void SetPaperMarginRight(Single value)
        {
            FPaperMarginRight = value;
            FPageMarginRightPix = HCUnitConversion.MillimeterToPixX(FPaperMarginRight);
        }

        protected void SetPaperMarginBottom(Single value)
        {
            FPaperMarginBottom = value;
            FPageMarginBottomPix = HCUnitConversion.MillimeterToPixY(FPaperMarginBottom); ;
        }

        public HCPageSize()
        {  
            PaperMarginLeft = 25;
            PaperMarginTop = 25;
            PaperMarginRight = 20;
            PaperMarginBottom = 20;
            PaperKind = PaperKind.A4;  // 默认A4 210 297
        }

        public void SaveToStream(Stream aStream)
        {
            Int64 vBegPos, vEndPos;
            vBegPos = aStream.Position;

            byte[] vBuffer = System.BitConverter.GetBytes(vBegPos);
            aStream.Write(vBuffer, 0, vBuffer.Length);  // 数据大小占位

            vBuffer = System.BitConverter.GetBytes((int)FPaperKind);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FPaperWidth);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FPaperHeight);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FPaperMarginLeft);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FPaperMarginTop);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FPaperMarginRight);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FPaperMarginBottom);
            aStream.Write(vBuffer, 0, vBuffer.Length);
  
            vEndPos = aStream.Position;
            aStream.Position = vBegPos;
            vBegPos = vEndPos - vBegPos - Marshal.SizeOf(vBegPos);

            vBuffer = System.BitConverter.GetBytes(vBegPos);
            aStream.Write(vBuffer, 0, vBuffer.Length);
            aStream.Position = vEndPos;
        }

        public void LoadToStream(Stream aStream, ushort aFileVersion)
        {
            Int64 vDataSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vDataSize);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            vDataSize = System.BitConverter.ToInt64(vBuffer, 0);

            vBuffer = System.BitConverter.GetBytes((int)FPaperKind);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            PaperKind = (PaperKind)BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FPaperWidth);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            PaperWidth = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FPaperHeight);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            PaperHeight = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FPaperMarginLeft);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            PaperMarginLeft = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FPaperMarginTop);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            PaperMarginTop = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FPaperMarginRight);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            PaperMarginRight = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FPaperMarginBottom);
            aStream.Read(vBuffer, 0, vBuffer.Length);
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
        }

        public int PageHeightPix  // 页高(含页眉、页脚)
        {
            get { return FPageHeightPix; }
        }

        public int PageMarginTopPix 
        {
            get { return FPageMarginTopPix; }
        }

        public int PageMarginLeftPix  
        {
            get { return FPageMarginLeftPix; }
        }

        public int PageMarginRightPix 
        {
            get { return FPageMarginRightPix; }
        }

        public int PageMarginBottomPix 
        {
            get { return FPageMarginBottomPix; }
        }
    }

    public class HCPage
    {
        private int FStartDrawItemNo, FEndDrawItemNo;    // 起始，结束item

        public HCPage()
        {
            Clear();
        }

        public void Assign(HCPage source)
        {
            FStartDrawItemNo = source.StartDrawItemNo;  // 起始item
            FEndDrawItemNo = source.EndDrawItemNo;  // 结束item
        }

        public void Clear()
        {
            FStartDrawItemNo = 0;
            FEndDrawItemNo = 0;
        }

        public int StartDrawItemNo
        {
            get { return FStartDrawItemNo; }
            set { FStartDrawItemNo = value; }
        }

        public int EndDrawItemNo
        {
            get { return FEndDrawItemNo; }
            set { FEndDrawItemNo = value; }
        }
    }

    class HCPages : List<HCPage>
    {
        public void ClearEx()
        {
            this.RemoveRange(1, this.Count - 1);
            this[0].Clear();
        }
    }
}
