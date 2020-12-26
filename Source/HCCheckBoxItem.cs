﻿/*******************************************************}
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
using System.Xml;

namespace HC.View
{
    public class HCCheckBoxItem : HCControlItem
    {
        private string FText;
        private bool FChecked, FMouseIn, FItemHit;

        private RECT GetBoxRect()
        {
            return HC.Bounds(FPaddingLeft, (Height - CheckBoxSize) / 2, CheckBoxSize, CheckBoxSize);
        }

        protected void SetChecked(bool value)
        {
            if (FChecked != value)
            {
                FChecked = value;
                this.DoChange();
            }
        }

        protected override string GetText()
        {
            return FText;
        }

        protected override void SetText(string value)
        {
            FText = value;
        }

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

        public override bool MouseMove(MouseEventArgs e)
        {
            HC.GCursor = Cursors.Arrow;
            return base.MouseMove(e);            
        }

        public override bool MouseUp(MouseEventArgs e)
        {
            if (OwnerData.CanEdit() && !OwnerData.Style.UpdateInfo.Selecting)
            {
                if (FItemHit)
                {
                    OwnerData.Style.ApplyTempStyle(TextStyleNo);
                    SIZE vSize = OwnerData.Style.TempCanvas.TextExtent(FText);
                    if (HC.PtInRect(HC.Bounds(FPaddingLeft, 0, FPaddingLeft + CheckBoxSize + FPaddingLeft + vSize.cx, vSize.cy), e.X, e.Y))
                        Checked = !FChecked;
                }
                else
                if (HC.PtInRect(GetBoxRect(), e.X, e.Y))  // 点在了勾选框中
                    Checked = !FChecked;
            }

            return base.MouseUp(e);            
        }

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            if (this.AutoSize)
            {
                aRichData.Style.ApplyTempStyle(TextStyleNo);
                SIZE vSize = aRichData.Style.TempCanvas.TextExtent(FText);
                Width = FPaddingLeft + CheckBoxSize + FPaddingLeft + vSize.cx;
                Height = Math.Max(vSize.cy, CheckBoxSize);
            }

            if (Width < FMinWidth)
                Width = FMinWidth;

            if (Height < FMinHeight)
                Height = FMinHeight;
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom,
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                aCanvas, aPaintInfo);

            RECT vBoxRect = GetBoxRect();
            HC.OffsetRect(ref vBoxRect, aDrawRect.Left, aDrawRect.Top);

            if (aPaintInfo.Print)
            {
                if (FChecked)
                    HC.HCDrawFrameControl(aCanvas, vBoxRect, HCControlState.hcsChecked, HCControlStyle.hcyCheck);
                else
                    HC.HCDrawFrameControl(aCanvas, vBoxRect, HCControlState.hcsCustom, HCControlStyle.hcyCheck);
            }
            else
            {
                if (this.IsSelectComplate)
                {
                    aCanvas.Brush.Color = aStyle.SelColor;
                    aCanvas.FillRect(aDrawRect);
                }
                else
                if (FMouseIn)
                {
                    aCanvas.Brush.Color = HC.clBtnFace;
                    aCanvas.FillRect(aDrawRect);
                }

                if (FChecked)
                {
                    aCanvas.Brush.Style = HCBrushStyle.bsSolid;
                    User.DrawFrameControl(aCanvas.Handle, ref vBoxRect, Kernel.DFC_MENU, Kernel.DFCS_CHECKED | Kernel.DFCS_MENUCHECK);
                }

                HC.HCDrawFrameControl(aCanvas, vBoxRect, HCControlState.hcsCustom, HCControlStyle.hcyCheck);
            }

            aCanvas.Brush.Style = HCBrushStyle.bsClear;
            aStyle.TextStyles[TextStyleNo].ApplyStyle(aCanvas, aPaintInfo.ScaleY / aPaintInfo.Zoom);
            aCanvas.TextOut(aDrawRect.Left + FPaddingLeft + CheckBoxSize + FPaddingLeft, aDrawRect.Top + (Height - aCanvas.TextHeight("H")) / 2, FText);
        }

        public byte CheckBoxSize = 14;

        public HCCheckBoxItem(HCCustomData aOwnerData, string aText, bool aChecked)
            : base(aOwnerData)
        {
            this.StyleNo = HCStyle.CheckBox;
            FChecked = aChecked;
            FText = aText;
            FMouseIn = false;
            FItemHit = false;
            FPaddingLeft = 2;
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FChecked = (source as HCCheckBoxItem).Checked;  // 勾选状态
            FText = (source as HCCheckBoxItem).Text;
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte[] vBuffer = BitConverter.GetBytes(FChecked);
            aStream.Write(vBuffer, 0, vBuffer.Length);  // 存勾选状态
            HC.HCSaveTextToStream(aStream, FText);  // 存Text
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte[] vBuffer = BitConverter.GetBytes(FChecked);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FChecked = BitConverter.ToBoolean(vBuffer, 0);
            HC.HCLoadTextFromStream(aStream, ref FText, aFileVersion);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("check", FChecked.ToString());
            aNode.InnerText = FText;
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FChecked = bool.Parse(aNode.Attributes["check"].Value);
            FText = aNode.InnerText;
        }

        public bool Checked
        {
            get { return FChecked; }
            set { SetChecked(value); }
        }
    }
}
