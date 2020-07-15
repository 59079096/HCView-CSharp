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

using HC.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;

namespace HC.View
{
    public class HCImageItem : HCResizeRectItem
    {
        private Bitmap FImage;
        private HCShapeManager FShapeManager;
        private bool FEmpty = true;

        private void DoImageChange(Object sender)
        {
            //if (FImage.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            //    FImage = FImage.Clone(new Rectangle(0, 0, FImage.Width, FImage.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            FEmpty = false;
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

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop,
            int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (aPaintInfo.Print)
            {
                if (FEmpty)
                    return;

                aCanvas.StretchPrintDrawBitmap(aDrawRect, FImage);
            }
            else
                aCanvas.StretchDraw(aDrawRect, FImage);

            if (FEmpty)
            {
                aCanvas.Pen.Color = Color.Black;
                aCanvas.Pen.Width = 1;
                aCanvas.Rectangle(aDrawRect);
            }

            FShapeManager.PaintTo(aCanvas, aDrawRect, aPaintInfo);

            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenBottom, aDataScreenBottom,
                aCanvas, aPaintInfo);
        }

        public HCImageItem(HCCustomData aOwnerData)
            : base(aOwnerData)
        {
            //FImage = new Bitmap(1, 1);
            StyleNo = HCStyle.Image;
            FShapeManager = new HCShapeManager();
            Clear();
        }

        public HCImageItem(HCCustomData aOwnerData, int aWidth, int aHeight)
            : base(aOwnerData, aWidth, aHeight)
        {
            StyleNo = HCStyle.Image;
            FShapeManager = new HCShapeManager();
            Clear();
        }

        ~HCImageItem()
        {
            FImage.Dispose();
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FImage = new Bitmap((source as HCImageItem).Image);
        }

        /// <summary>
        /// 会产生graphics异常的PixelFormat
        /// </summary>
        private static PixelFormat[] indexedPixelFormats = { PixelFormat.Undefined, PixelFormat.DontCare,
            PixelFormat.Format16bppArgb1555, PixelFormat.Format1bppIndexed, PixelFormat.Format4bppIndexed,
            PixelFormat.Format8bppIndexed
        };

        /// <summary> 判断图片的PixelFormat 是否在 引发异常的 PixelFormat 之中 </summary>
        /// <param name="imgPixelFormat">原图片的PixelFormat</param>
        /// <returns></returns>
        private static bool IsPixelFormatIndexed(PixelFormat imgPixelFormat)
        {
            foreach (PixelFormat pf in indexedPixelFormats)
            {
                if (pf.Equals(imgPixelFormat))
                    return true;
            }

            return false;
        }

        public override void PaintTop(HCCanvas aCanvas)
        {
            if (this.Resizing)
            {
                Bitmap vBitmap = FImage;

                //如果原图片是索引像素格式之列的，则需要转换
                if (IsPixelFormatIndexed(FImage.PixelFormat))
                    vBitmap = new Bitmap(FImage.Width, FImage.Height, PixelFormat.Format32bppArgb);

                using (Graphics vGraphicSrc = Graphics.FromImage(vBitmap))
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
                        IntPtr vHbitmap = FImage.GetHbitmap();// (IntPtr)GDI.CreateCompatibleBitmap(vImageHDC, FImage.Width, FImage.Height);
                        GDI.SelectObject(vMemDC, vHbitmap);

                        GDI.AlphaBlend(
                            aCanvas.Handle,
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
                        GDI.DeleteObject(vHbitmap);
                    }
                    finally
                    {
                        vGraphicSrc.ReleaseHdc(vImageHDC);
                    }
                }

                if (!vBitmap.Equals(FImage))
                    vBitmap.Dispose();
            }

            base.PaintTop(aCanvas);
        }

        public override void Clear()
        {
            FImage = new Bitmap(this.Width, this.Height);
            FEmpty = true;
        }

        /// <summary> 约束到指定大小范围内 </summary>
        public override void RestrainSize(int aWidth, int aHeight)
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

        public void LoadGraphicFile(string aFileName, bool resize = true)
        {
            FImage = new Bitmap(aFileName);
            DoImageChange(this);

            if (resize)
            {
                this.Width = FImage.Width;
                this.Height = FImage.Height;
            }

            this.FEmpty = false;
        }

        public void LoadGraphicStream(Stream stream, bool resize = true)
        {
            FImage = new Bitmap(stream);
            DoImageChange(this);

            if (resize)
            {
                this.Width = FImage.Width;
                this.Height = FImage.Height;
            }

            this.FEmpty = false;
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            // 图像不能直接写流，会导致流前面部分数据错误
            using (MemoryStream vImgStream = new MemoryStream())
            {
                using (Bitmap bitmap = new Bitmap(FImage))  // 解决GDI+ 中发生一般性错误，因为该文件仍保留锁定对于对象的生存期
                {
                    bitmap.Save(vImgStream, ImageFormat.Bmp);
                }

                // write bitmap data size
                uint vSize = (uint)vImgStream.Length;
                byte[] vBuffer = BitConverter.GetBytes(vSize);
                aStream.Write(vBuffer, 0, vBuffer.Length);

                vBuffer = new byte[vImgStream.Length];
                vImgStream.Seek(0, SeekOrigin.Begin);
                vImgStream.Read(vBuffer, 0, vBuffer.Length);

                aStream.Write(vBuffer, 0, vBuffer.Length);
            }

            FShapeManager.SaveToStream(aStream);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            // read bitmap data size
            uint vSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vSize = BitConverter.ToUInt32(vBuffer, 0);

            if (vSize > 0)
            {
                vBuffer = new byte[vSize];
                aStream.Read(vBuffer, 0, vBuffer.Length);

                using (MemoryStream vImgStream = new MemoryStream(vBuffer))
                {
                    FImage = new Bitmap(vImgStream);
                }

                DoImageChange(this);
            }
            else
            {
                FImage = new Bitmap(this.Width, this.Height);
                FEmpty = true;
            }

            if (aFileVersion > 26)
                FShapeManager.LoadFromStream(aStream);
        }

        public override string ToHtml(string aPath)
        {
            if (aPath != "")  // 保存到指定的文件夹中
            {
                if (!Directory.Exists(aPath + "images"))
                    Directory.CreateDirectory(aPath + "images");

                string vFileName = OwnerData.Style.GetHtmlFileTempName() + ".bmp";
                FImage.Save(aPath + "images\"" + vFileName);
                return "<img width=\"" + Width.ToString() + "\" height=\"" + Height.ToString()
                    + "\" src=\"images/" + vFileName + " alt=\"HCImageItem\" />";
            }
            else  // 保存为Base64
                return "<img width=\"" + Width.ToString() + "\" height=\"" + Height.ToString()
                    + "\" src=\"data:img/jpg;base64," + HC.GraphicToBase64(FImage, FImage.RawFormat) + "\" alt=\"HCImageItem\" />";
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);

            XmlElement vNode = aNode.OwnerDocument.CreateElement("img");
            if (!FEmpty)
                vNode.InnerText = HC.GraphicToBase64(FImage, FImage.RawFormat);

            aNode.AppendChild(vNode);

            vNode = aNode.OwnerDocument.CreateElement("shapes");
            FShapeManager.ToXml(vNode);
            aNode.AppendChild(vNode);
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FEmpty = true;
            if (aNode.HasChildNodes)
            {
                for (int i = 0; i < aNode.ChildNodes.Count; i++)
                {
                    if (aNode.ChildNodes[i].Name == "img")
                    {
                        if (aNode.ChildNodes[i].InnerText != "")
                        {
                            FImage = new Bitmap(HC.Base64ToGraphic(aNode.ChildNodes[i].InnerText));
                            DoImageChange(this);
                        }
                    }
                    else
                    if (aNode.ChildNodes[i].Name == "shapes")
                        FShapeManager.ParseXml(aNode.ChildNodes[i] as XmlElement);
                }
            }
            else
            if (aNode.InnerText != "")
            {
                FImage = (Bitmap)HC.Base64ToGraphic(aNode.InnerText);
                DoImageChange(this);
            }
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

        public bool Empty
        {
            get { return FEmpty; }
        }

        public HCShapeManager ShapeManager
        {
            get { return FShapeManager; }
        }
    }
}
