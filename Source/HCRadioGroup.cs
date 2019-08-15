/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-9-15             }
{                                                       }
{             文档RadioGroup对象实现单元                }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace HC.View
{
    public class HCRadioButton : HCObject
    {
        public bool Checked = false;
        public string Text = "";
        public POINT Position = new POINT();
    }

    public class HCRadioGroup : HCControlItem
    {
        private bool FMultSelect, FMouseIn;
        private List<HCRadioButton> FItems;
        public static byte RadioButtonWidth = 16;

        private int GetItemAt(int x, int  y)
        {
            int Result = -1;
            this.OwnerData.Style.ApplyTempStyle(TextStyleNo);
            
            SIZE vSize = new SIZE();
            for (int i = 0; i <= FItems.Count - 1; i++)
            {
                vSize = this.OwnerData.Style.TempCanvas.TextExtent(FItems[i].Text);
                if (HC.PtInRect(HC.Bounds(FItems[i].Position.X, FItems[i].Position.Y,
                    RadioButtonWidth + vSize.cx, vSize.cy), x, y))
                
                {
                    Result = i;
                    break;
                }
            }

            return Result;
        }

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            Height = FMinHeight;

            aRichData.Style.ApplyTempStyle(TextStyleNo);

            int vLeft = FMargin;
            int vTop = FMargin;
            SIZE vSize = new SIZE();

            for (int i = 0; i <= FItems.Count - 1; i++)
            {
                if (FItems[i].Text != "")
                    vSize = aRichData.Style.TempCanvas.TextExtent(FItems[i].Text);
                else
                    vSize = aRichData.Style.TempCanvas.TextExtent("H");
                
                if (vLeft + vSize.cx + RadioButtonWidth > Width)
                {
                    vLeft = FMargin;
                    vTop = vTop + vSize.cy + FMargin;
                }

                FItems[i].Position.X = vLeft;
                FItems[i].Position.Y = vTop;

                vLeft = vLeft + RadioButtonWidth + vSize.cx + FMargin;
            }
            
            Height = vTop + vSize.cy + FMargin;
            
            if (Width < FMinWidth)
                Width = FMinWidth;
            if (Height < FMinHeight)
                Height = FMinHeight;
        }
    
        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop,
                aDataScreenBottom, aCanvas, aPaintInfo);
            
            if (FMouseIn)
            {
                aCanvas.Brush.Color = HC.clBtnFace;
                aCanvas.FillRect(aDrawRect);
            }

            aStyle.TextStyles[TextStyleNo].ApplyStyle(aCanvas, aPaintInfo.ScaleY / aPaintInfo.Zoom);
            
            POINT vPoint = new POINT();
            RECT vItemRect = new RECT();
            for (int i = 0; i <= FItems.Count - 1; i++)
            {
                vPoint.X = FItems[i].Position.X;
                vPoint.Y = FItems[i].Position.Y;
                vPoint.Offset(aDrawRect.Left, aDrawRect.Top);
                vItemRect = HC.Bounds(vPoint.X, vPoint.Y, RadioButtonWidth, RadioButtonWidth);
                if (FItems[i].Checked)
                    User.DrawFrameControl(aCanvas.Handle, ref vItemRect, Kernel.DFC_BUTTON, Kernel.DFCS_CHECKED | Kernel.DFCS_BUTTONRADIO);
                else
                    User.DrawFrameControl(aCanvas.Handle, ref vItemRect, Kernel.DFC_BUTTON, Kernel.DFCS_BUTTONRADIO);
                
                aCanvas.TextOut(vPoint.X + RadioButtonWidth, vPoint.Y, FItems[i].Text);
            }
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                int vIndex = GetItemAt(e.X, e.Y);
                if (vIndex >= 0)
                {
                    FItems[vIndex].Checked = !FItems[vIndex].Checked;
                    if (!FMultSelect)
                    {
                        for (int i = 0; i <= FItems.Count - 1; i++)
                        {
                            if (i != vIndex)
                                FItems[i].Checked = false;
                        }
                    }
                }
            }
        }
    
        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);
            HC.GCursor = Cursors.Default;
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

        public override void GetCaretInfo(ref HCCaretInfo aCaretInfo)
        {
            if (this.Active)
                aCaretInfo.Visible = false;
        }

        public override int GetOffsetAt(int x)
        {
            if (x <= FMargin)
                return HC.OffsetBefor;
            else
                if (x >= Width - FMargin)
                    return HC.OffsetAfter;
                else
                    return HC.OffsetInner;
        }

        public HCRadioGroup(HCCustomData aOwnerData)
            : base(aOwnerData)
        {
            this.StyleNo = HCStyle.RadioGroup;
            Width = 100;
            FItems = new List<HCRadioButton>();
        }

        ~HCRadioGroup()
        {
            //FItems
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            HCRadioGroup vSource = source as HCRadioGroup;

            FItems.Clear();
            for (int i = 0; i < vSource.Items.Count; i++)
                AddItem(vSource.Items[i].Text, vSource.Items[i].Checked);
        }

        public void AddItem(string aText, bool AChecked = false)
        {
            HCRadioButton vRadioButton = new HCRadioButton();
            vRadioButton.Checked = AChecked;
            vRadioButton.Text = aText;
            FItems.Add(vRadioButton);
        }

        public override void SaveToStream(Stream aStream, int aStart, int  aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            string vS = "";
            if (FItems.Count > 0)
            {
                vS = FItems[0].Text;
                if (vS == "")
                    vS = "未命名";

                for (int i = 1; i < FItems.Count; i++)
                {
                    if (FItems[i].Text != "")
                        vS = vS + HC.sLineBreak + FItems[i].Text;
                    else
                        vS = vS + HC.sLineBreak + "未命名";
                }
            }

            HC.HCSaveTextToStream(aStream, vS);

            for (int i = 0; i < FItems.Count; i++)
            {
                byte[] vBuffer = BitConverter.GetBytes(FItems[i].Checked);
                aStream.Write(vBuffer, 0, vBuffer.Length);
            }
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            FItems.Clear();

            string vS = "";
            HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            if (vS != "")
            {
                string[] vStrings = vS.Split(new string[] { HC.sLineBreak }, StringSplitOptions.None);

                for (int i = 0; i < vStrings.Length; i++)
                    AddItem(vStrings[i]);

                byte[]  vBuffer = BitConverter.GetBytes(false);
                for (int i = 0; i < FItems.Count; i++)
                {
                    aStream.Read(vBuffer, 0, vBuffer.Length);
                    FItems[i].Checked = BitConverter.ToBoolean(vBuffer, 0);
                }
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            string vS = "";
            if (FItems.Count > 0)
            {
                for (int i = 0; i < FItems.Count; i++)
                    vS += FItems[i] + HC.sLineBreak;
            }

            aNode.SetAttribute("item", vS);

            vS = "";
            if (FItems.Count > 0)
            {
                for (int i = 0; i < FItems.Count; i++)
                {
                    if (FItems[i].Checked)
                        vS += "1" + HC.sLineBreak;
                    else
                        vS += "0" + HC.sLineBreak;
                }
            }
            aNode.SetAttribute("check", vS);
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FItems.Clear();
            string vText = aNode.Attributes["item"].Value;
            string[] vStrings = vText.Split(new string[] { HC.sLineBreak }, StringSplitOptions.None);

            for (int i = 0; i < vStrings.Length; i++)
                AddItem(vStrings[i]);

            vText = aNode.Attributes["check"].Value;
            vStrings = vText.Split(new string[] { HC.sLineBreak }, StringSplitOptions.None);

            for (int i = 0; i < vStrings.Length; i++)
            {
                if (vStrings[i] == "1")
                    FItems[i].Checked = true;
                else
                    FItems[i].Checked = false;
            }
        }
    
        public bool MultSelect
        {
            get { return FMultSelect; }
            set { FMultSelect = value; }
        }

        public List<HCRadioButton> Items
        {
            get { return FItems; }
        }
    }
}
