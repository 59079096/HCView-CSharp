/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{        文档CheckBoxItem(勾选框)对象实现单元           }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HC.Win32;
using System.IO;
using System.Drawing;

namespace HC.View
{
    public class HCCheckBoxItem : HCControlItem
    {
        private string FText;
        private bool FChecked, FMouseIn;

        private RECT GetBoxRect()
        {
            return HC.Bounds(FMargin, (Height - CheckBoxSize) / 2, CheckBoxSize, CheckBoxSize);
        }

        protected void SetChecked(bool Value)
        {
            if (FChecked != Value)
                FChecked = Value;
        }

        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, int ADataDrawTop, int ADataDrawBottom,
            int ADataScreenTop, int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoPaint(AStyle, ADrawRect, ADataDrawTop, ADataDrawBottom, ADataScreenTop, ADataScreenBottom,
                ACanvas, APaintInfo);

            if ((FMouseIn) && (!APaintInfo.Print))
            {
                ACanvas.Brush.Color = HC.clBtnFace;
                ACanvas.FillRect(ADrawRect);
            }

            RECT vBoxRect = GetBoxRect();
            HC.OffsetRect(ref vBoxRect, ADrawRect.Left, ADrawRect.Top);

            if (this.IsSelectComplate && (!APaintInfo.Print))
            {
                ACanvas.Brush.Color = AStyle.SelColor;
                ACanvas.FillRect(ADrawRect);
            }

            AStyle.TextStyles[TextStyleNo].ApplyStyle(ACanvas, APaintInfo.ScaleY / APaintInfo.Zoom);
            ACanvas.TextOut(ADrawRect.Left + FMargin + CheckBoxSize + FMargin, ADrawRect.Top + (Height - ACanvas.TextHeight("H")) / 2, FText);

            if (FChecked)  // 勾选
                User.DrawFrameControl(ACanvas.Handle, ref vBoxRect, Kernel.DFC_MENU, Kernel.DFCS_CHECKED | Kernel.DFCS_MENUCHECK);

            if (FMouseIn && (!APaintInfo.Print))  // 鼠标在其中，且非打印
            {
                ACanvas.Pen.Color = Color.Blue;
                ACanvas.Rectangle(vBoxRect.Left, vBoxRect.Top, vBoxRect.Right, vBoxRect.Bottom);
                HC.InflateRect(ref vBoxRect, 1, 1);
                ACanvas.Pen.Color = HC.clBtnFace;
                ACanvas.Rectangle(vBoxRect.Left, vBoxRect.Top, vBoxRect.Right, vBoxRect.Bottom);
            }
            else  // 鼠标不在其中或打印
            {
                ACanvas.Pen.Color = Color.Black;
                ACanvas.Rectangle(vBoxRect.Left, vBoxRect.Top, vBoxRect.Right, vBoxRect.Bottom);
            }
        }
        //
        public override void MouseEnter()
        {
            base.MouseEnter();
            FMouseIn = true;
        }

        public override void MouseLeave()
        {
            base.MouseLeave();
            FMouseIn = false;
        }

        public override void FormatToDrawItem(HCCustomData ARichData, int AItemNo)
        {
            if (this.AutoSize)
            {
                ARichData.Style.TextStyles[TextStyleNo].ApplyStyle(ARichData.Style.DefCanvas);
                SIZE vSize = ARichData.Style.DefCanvas.TextExtent(FText);
                Width = FMargin + CheckBoxSize + FMargin + vSize.cx;
                Height = Math.Max(vSize.cy, CheckBoxSize);
            }

            if (Width < FMinWidth)
                Width = FMinWidth;

            if (Height < FMinHeight)
                Height = FMinHeight;
        }

        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);
            HC.GCursor = Cursors.Arrow;
        }

        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);
            if (HC.PtInRect(GetBoxRect(), e.X, e.Y))  // 点在了勾选框中
                Checked = !FChecked;
        }

        public byte CheckBoxSize = 14;

        public HCCheckBoxItem(HCCustomData AOwnerData, string AText, bool AChecked)
            : base(AOwnerData)
        {
            this.StyleNo = HCStyle.CheckBox;
            FChecked = AChecked;
            FText = AText;
            FMouseIn = false;
            FMargin = 2;
        }

        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);

            byte[] vBuffer = BitConverter.GetBytes(FChecked);
            AStream.Write(vBuffer, 0, vBuffer.Length);  // 存勾选状态
            // 存Text
            int vLen = System.Text.Encoding.Default.GetByteCount(FText);
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            ushort vSize = (ushort)vLen;
            vBuffer = BitConverter.GetBytes(vSize);
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

            byte[] vBuffer = BitConverter.GetBytes(FChecked);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FChecked = BitConverter.ToBoolean(vBuffer, 0);

            ushort vSize = 0;
            vBuffer = BitConverter.GetBytes(vSize);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            if (vSize > 0)
            {
                vBuffer = new byte[vSize];
                AStream.Read(vBuffer, 0, vBuffer.Length);
                FText = System.Text.Encoding.Default.GetString(vBuffer);
            }
        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            FChecked = (Source as HCCheckBoxItem).Checked;  // 勾选状态
            FText = (Source as HCCheckBoxItem).Text;
        }

        public bool Checked
        {
            get { return FChecked; }
            set { SetChecked(value); }
        }
        public string Text
        {
            get { return FText; }
            set { FText = value; }
        }
    }
}
