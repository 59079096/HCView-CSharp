/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{           来获取更多的技术交流 2018-5-18              }
{                                                       }
{         文档GifItem(动画图像)对象实现单元             }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;
using System.Drawing;
using System.Xml;
using System.IO;

namespace HC.View
{
    public class HCGifItem : HCAnimateRectItem
    {
        private RECT FDrawRect;
        private Image FGifImage;
        private bool FAnimate = false;
        private EventHandler evtHandler = null;

        private void DoImageAnimate(object sender, EventArgs e)
        {
            //if (FAnimate)
            //    OwnerData.Style.InvalidateRect(FDrawRect);
        }

        //开始动画方法
        private void BeginAnimate()
        {
            if (FGifImage != null)
            {
                //当gif动画每隔一定时间后，都会变换一帧，那么就会触发一事件，该方法就是将当前image每变换一帧时，都会调用当前这个委托所关联的方法。
                ImageAnimator.Animate(FGifImage, evtHandler);
            }
        }

        //获得当前gif动画的下一步需要渲染的帧，当下一步任何对当前gif动画的操作都是对该帧进行操作)
        private void UpdateImage()
        {
            ImageAnimator.UpdateFrames(FGifImage);
        }

        //关闭显示动画，该方法可以在winform关闭时，或者某个按钮的触发事件中进行调用，以停止渲染当前gif动画。

        private void StopAnimate()
        {
            ImageAnimator.StopAnimate(FGifImage, evtHandler);
        }

        protected override int GetWidth()
        {
            int Result = base.GetWidth();
            if (Result == 0)
                Result = FGifImage.Width;

            return Result;
        }

        protected override int GetHeight()
        {
            int Result = base.GetHeight();
            if (Result == 0)
                Result = FGifImage.Height;

            return Result;
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            FDrawRect = aDrawRect;
            //if (FAnimate)
            //    UpdateImage();  // 获得当前gif动画下一步要渲染的帧

            //aCanvas.Draw(aDrawRect.Left, aDrawRect.Top, FGifImage);
            aCanvas.StretchDraw(aDrawRect, FGifImage);
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        public HCGifItem(HCCustomData aOwnerData) : base(aOwnerData)
        {
            FGifImage = null;
            StyleNo = HCStyle.Gif;
            evtHandler = new EventHandler(DoImageAnimate);
        }

        public HCGifItem(HCCustomData aOwnerData, int aWidth, int aHeight)
            : base(aOwnerData, aWidth, aHeight)
        {
            FGifImage = null;
            StyleNo = HCStyle.Gif;
            evtHandler = new EventHandler(DoImageAnimate);
        }

        ~HCGifItem()
        {
            FGifImage.Dispose();
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FGifImage = (source as HCGifItem).Image.Clone() as Image;
        }

        public void LoadFromFile(string aFileName)
        {
            FGifImage = Image.FromFile(aFileName);
            this.Width = FGifImage.Width;
            this.Height = FGifImage.Height;
            FAnimate = true;
            BeginAnimate();  // 调用开始动画方法
        }

        public override void SaveToStream(System.IO.Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            // 图像不能直接写流，会导致流前面部分数据错误
            using (MemoryStream vImgStream = new MemoryStream())
            {
                FGifImage.Save(vImgStream, System.Drawing.Imaging.ImageFormat.Gif);

                // write gif data size
                uint vSize = (uint)vImgStream.Length;
                byte[] vBuffer = BitConverter.GetBytes(vSize);
                aStream.Write(vBuffer, 0, vBuffer.Length);

                vBuffer = new byte[vImgStream.Length];
                vImgStream.Seek(0, SeekOrigin.Begin);
                vImgStream.Read(vBuffer, 0, vBuffer.Length);

                aStream.Write(vBuffer, 0, vBuffer.Length);
            }
        }

        public override void LoadFromStream(System.IO.Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            uint vSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vSize = BitConverter.ToUInt32(vBuffer, 0);

            vBuffer = new byte[vSize];
            aStream.Read(vBuffer, 0, vBuffer.Length);

            using (MemoryStream vImgStream = new MemoryStream(vBuffer))
            {
                FGifImage = Image.FromStream(vImgStream);
            }

            this.Width = FGifImage.Width;
            this.Height = FGifImage.Height;

            FAnimate = true;
            BeginAnimate();  // 调用开始动画方法
        }

        public override string ToHtml(string aPath)
        {
            if (aPath != "")  // 保存到指定的文件夹中
            {
                if (!Directory.Exists(aPath + "images"))
                    Directory.CreateDirectory(aPath + "images");

                string vFileName = OwnerData.Style.GetHtmlFileTempName() + ".gif";
                FGifImage.Save(aPath + "images\"" + vFileName);
                return "<img width=\"" + Width.ToString() + "\" height=\"" + Height.ToString()
                    + "\" src=\"images/" + vFileName + " alt=\"HCGifItem\" />";
            }
            else  // 保存为Base64
                return "<img width=\"" + Width.ToString() + "\" height=\"" + Height.ToString()
                    + "\" src=\"data:img/jpg;base64," + HC.GraphicToBase64(FGifImage, System.Drawing.Imaging.ImageFormat.Gif) + "\" alt=\"HCGifItem\" />";
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.InnerText = HC.GraphicToBase64(FGifImage, System.Drawing.Imaging.ImageFormat.Gif);
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FGifImage = HC.Base64ToGraphic(aNode.InnerText);
            this.Width = FGifImage.Width;
            this.Height = FGifImage.Height;
            FAnimate = true;
        }

        public Image Image
        {
            get { return FGifImage; }
        }

        public bool Animate
        {
            get { return FAnimate; }
            set { FAnimate = value; }
        }
    }
}
