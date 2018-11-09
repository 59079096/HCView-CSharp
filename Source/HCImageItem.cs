/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{          文档ImageItem(图像)对象实现单元              }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using HC.Win32;

namespace HC.View
{
    public class HCImageItem : HCResizeRectItem
    {
        private Bitmap FImage;

        private void DoImageChange(Object Sender)
        {
            if (FImage.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                FImage = FImage.Clone(new Rectangle(0, 0, FImage.Width, FImage.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        }

        protected override int GetWidth()
        {
            int Result = base.GetWidth();
            if (Result == 0)
                Result = FImage.Width;

            return Result;
        }

        protected override int GetHeight()
        {
            int Result = base.GetHeight();
            if (Result == 0)
                Result = FImage.Height;

            return Result;
        }

        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);

            // 图像不能直接写流，会导致流前面部分数据错误
            using (MemoryStream vImgStream = new MemoryStream())
            {
                FImage.Save(vImgStream, System.Drawing.Imaging.ImageFormat.Bmp);

                // write bitmap data size
                uint vSize = (uint)vImgStream.Length;
                byte[] vBuffer = BitConverter.GetBytes(vSize);
                AStream.Write(vBuffer, 0, vBuffer.Length);

                vBuffer = new byte[vImgStream.Length];
                vImgStream.Seek(0, SeekOrigin.Begin);
                vImgStream.Read(vBuffer, 0, vBuffer.Length);

                AStream.Write(vBuffer, 0, vBuffer.Length);
            }
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);

            // read bitmap data size
            uint vSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            vSize = BitConverter.ToUInt32(vBuffer, 0);

            vBuffer = new byte[vSize];
            AStream.Read(vBuffer, 0, vBuffer.Length);

            using (MemoryStream vImgStream = new MemoryStream(vBuffer))
            {  
                FImage = new Bitmap(vImgStream);
            }

            DoImageChange(this);
        }

        //
        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, int ADataDrawTop, 
            int ADataDrawBottom, int ADataScreenTop, int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            ACanvas.StretchDraw(ADrawRect, FImage);

            base.DoPaint(AStyle, ADrawRect, ADataDrawTop, ADataDrawBottom, ADataScreenBottom, ADataScreenBottom,
                ACanvas, APaintInfo);
        }

        public override void PaintTop(HCCanvas ACanvas)
        {
            using (Graphics vGraphicSrc = Graphics.FromImage(FImage))
            {
                BLENDFUNCTION vBlendFunction = new BLENDFUNCTION();
                vBlendFunction.BlendOp = GDI.AC_SRC_OVER;
                vBlendFunction.BlendFlags = 0;
                vBlendFunction.AlphaFormat = GDI.AC_SRC_OVER;  // 通常为 0，如果源位图为32位真彩色，可为 AC_SRC_ALPHA
                vBlendFunction.SourceConstantAlpha = 128; // 透明度


                IntPtr vImageHDC = vGraphicSrc.GetHdc();
                try
                {
                    IntPtr vMemDC = (IntPtr)GDI.CreateCompatibleDC(vImageHDC);
                    IntPtr vBitmap = FImage.GetHbitmap();// (IntPtr)GDI.CreateCompatibleBitmap(vImageHDC, FImage.Width, FImage.Height);
                    GDI.SelectObject(vMemDC, vBitmap);

                    GDI.AlphaBlend(
                        ACanvas.Handle,
                        ResizeRect.Left,
                        ResizeRect.Top,
                        ResizeWidth,
                        ResizeHeight,
                        vMemDC,
                        0,
                        0,
                        FImage.Width,
                        FImage.Height,
                        vBlendFunction);

                    GDI.DeleteDC(vMemDC);
                    GDI.DeleteObject(vBitmap);
                }
                finally
                {
                    vGraphicSrc.ReleaseHdc(vImageHDC);
                }
            }

            base.PaintTop(ACanvas);
        }

        public HCImageItem(HCCustomData AOwnerData) : base(AOwnerData)
        {
            FImage = new Bitmap(1, 1);
            StyleNo = HCStyle.Image;
        }

        public HCImageItem(HCCustomData AOwnerData, int AWidth, int AHeight) : base(AOwnerData, AWidth, AHeight)
        {
            StyleNo = HCStyle.Image;
        }

        ~HCImageItem()
        {
            FImage.Dispose();
        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            FImage = new Bitmap((Source as HCImageItem).Image);
        }

        /// <summary> 约束到指定大小范围内 </summary>
        public override void RestrainSize(int AWidth, int AHeight)
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

        public void LoadFromBmpFile(string AFileName)
        {
            FImage = new Bitmap(AFileName);
            DoImageChange(this);

            this.Width = FImage.Width;
            this.Height = FImage.Height;
        }

        /// <summary> 恢复到原始尺寸 </summary>
        public void RecoverOrigianlSize()
        {
            Width = FImage.Width;
            Height = FImage.Height;
        }

        public Bitmap Image
        {
            get { return FImage; }
            set { FImage = value; }
        }
    }
}
