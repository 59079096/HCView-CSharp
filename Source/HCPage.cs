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
    public class HCPaper : HCObject
    {
        private PaperKind FSize;  // 纸张大小如A4、B5等
        private Single FWidth, FHeight;  // 纸张宽、高（单位mm）
        private int FWidthPix, FHeightPix;  // 页面大小
        private Single FMarginTop, FMarginLeft, FMarginRight, FMarginBottom;  // 纸张边距（单位mm）
        private int FMarginTopPix, FMarginLeftPix, FMarginRightPix, FMarginBottomPix;  // 页边距

        protected void SetSize(PaperKind value)
        {
            if (FSize != value)
                FSize = value;
        }

        protected void SetWidth(Single value)
        {
            FWidth = value;
            FWidthPix = HCUnitConversion.MillimeterToPixX(FWidth);
        }

        protected void SetHeight(Single value)
        {
            FHeight = value;
            FHeightPix = HCUnitConversion.MillimeterToPixY(FHeight);
        }

        protected void SetMarginTop(Single value)
        {
            FMarginTop = value;
            FMarginTopPix = HCUnitConversion.MillimeterToPixY(FMarginTop);
        }

        protected void SetMarginLeft(Single value)
        {
            FMarginLeft = value;
            FMarginLeftPix = HCUnitConversion.MillimeterToPixX(FMarginLeft);
        }

        protected void SetMarginRight(Single value)
        {
            FMarginRight = value;
            FMarginRightPix = HCUnitConversion.MillimeterToPixX(FMarginRight);
        }

        protected void SetMarginBottom(Single value)
        {
            FMarginBottom = value;
            FMarginBottomPix = HCUnitConversion.MillimeterToPixY(FMarginBottom);
        }

        public HCPaper()
        {  
            MarginLeft = 25;
            MarginTop = 25;
            MarginRight = 20;
            MarginBottom = 20;
            Size = PaperKind.A4;  // 默认A4 210 297
            Width = 210;
            Height = 297;
        }

        public void SaveToStream(Stream aStream)
        {
            Int64 vBegPos, vEndPos;
            vBegPos = aStream.Position;

            byte[] vBuffer = System.BitConverter.GetBytes(vBegPos);
            aStream.Write(vBuffer, 0, vBuffer.Length);  // 数据大小占位

            vBuffer = System.BitConverter.GetBytes((int)FSize);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FWidth);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FHeight);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FMarginLeft);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FMarginTop);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FMarginRight);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = System.BitConverter.GetBytes(FMarginBottom);
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

            vBuffer = System.BitConverter.GetBytes((int)FSize);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            Size = (PaperKind)BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FWidth);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            Width = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FHeight);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            Height = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FMarginLeft);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            MarginLeft = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FMarginTop);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            MarginTop = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FMarginRight);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            MarginRight = System.BitConverter.ToSingle(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FMarginBottom);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            MarginBottom = System.BitConverter.ToSingle(vBuffer, 0);
        }

        public PaperKind Size
        {
            get { return FSize; }
            set { SetSize(value); }
        }

        public Single Width
        {
            get { return FWidth; }
            set { SetWidth(value); }
        }

        public Single Height
        {
            get { return FHeight; }
            set { SetHeight(value); }
        }

        public Single MarginTop
        {
            get { return FMarginTop; }
            set { SetMarginTop(value); }
        }

        public Single MarginLeft
        {
            get { return FMarginLeft; }
            set { SetMarginLeft(value); }
        }
        
        public Single MarginRight
        {
            get { return FMarginRight; }
            set { SetMarginRight(value); }
        }

        public Single MarginBottom
        {
            get { return FMarginBottom; }
            set { SetMarginBottom(value); }
        }

        public int WidthPix  // 页宽(含页左右边距)
        {
            get { return FWidthPix; }
        }

        public int HeightPix  // 页高(含页眉、页脚)
        {
            get { return FHeightPix; }
        }

        public int MarginTopPix 
        {
            get { return FMarginTopPix; }
        }

        public int MarginLeftPix  
        {
            get { return FMarginLeftPix; }
        }

        public int MarginRightPix 
        {
            get { return FMarginRightPix; }
        }

        public int MarginBottomPix 
        {
            get { return FMarginBottomPix; }
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
