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
    public enum HCRadioStyle : byte
    {
        Radio,
        CheckBox
    }

    public class HCRadioButton : HCObject
    {
        private bool FChecked = false;
        private EventHandler FOnSetChecked;

        private void SetChecked(bool value)
        {

        }

        public string Text = "";
        public POINT Position = new POINT();

        public bool Checked
        {
            get { return FChecked; }
            set { SetChecked(value); }
        }

        public EventHandler OnSetChecked
        {
            get { return FOnSetChecked; }
            set { FOnSetChecked = value; }
        }
    }

    public class HCRadioGroup : HCControlItem
    {
        private bool FMultSelect, FMouseIn;
        private HCList<HCRadioButton> FItems;
        private HCRadioStyle FRadioStyle = HCRadioStyle.Radio;
        public static byte RadioButtonWidth = 16;

        private int GetItemAt(int x, int  y)
        {
            int Result = -1;
            this.OwnerData.Style.ApplyTempStyle(TextStyleNo);
            
            //SIZE vSize = new SIZE();
            for (int i = 0; i <= FItems.Count - 1; i++)
            {
                //vSize = this.OwnerData.Style.TempCanvas.TextExtent(FItems[i].Text);
                //if (HC.PtInRect(HC.Bounds(FItems[i].Position.X, FItems[i].Position.Y,
                //    RadioButtonWidth + vSize.cx, vSize.cy), x, y))

                if (HC.PtInRect(HC.Bounds(FItems[i].Position.X, FItems[i].Position.Y,
                    RadioButtonWidth, RadioButtonWidth), x, y))
                {
                    Result = i;
                    break;
                }
            }

            return Result;
        }

        protected void DoItemNotify(object sender, NListEventArgs<HCRadioButton> e)
        {
            e.Item.OnSetChecked = DoItemSetChecked;
        }

        protected void DoItemSetChecked(object sender, EventArgs e)
        {
            if ((!FMultSelect) && (sender as HCRadioButton).Checked)
            {
                int vIndex = FItems.IndexOf(sender as HCRadioButton);
                for (int i = 0; i < FItems.Count; i++)
                {
                    if (i != vIndex)
                        FItems[i].Checked = false;
                }
            }
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
                
                if (!this.AutoSize && vLeft + vSize.cx + RadioButtonWidth > Width)
                {
                    vLeft = FMargin;
                    vTop = vTop + vSize.cy + FMargin;
                }

                FItems[i].Position.X = vLeft;
                FItems[i].Position.Y = vTop;

                vLeft = vLeft + RadioButtonWidth + vSize.cx + FMargin;
            }

            if (this.AutoSize)
                Width = vLeft;

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
                    User.DrawFrameControl(aCanvas.Handle, ref vItemRect, Kernel.DFC_BUTTON, Kernel.DFCS_CHECKED | (FRadioStyle == HCRadioStyle.Radio ? Kernel.DFCS_BUTTONRADIO : Kernel.DFCS_BUTTONCHECK));
                else
                    User.DrawFrameControl(aCanvas.Handle, ref vItemRect, Kernel.DFC_BUTTON, (FRadioStyle == HCRadioStyle.Radio ? Kernel.DFCS_BUTTONRADIO : Kernel.DFCS_BUTTONCHECK));
                
                aCanvas.TextOut(vPoint.X + RadioButtonWidth, vPoint.Y, FItems[i].Text);
            }
        }

        public override bool MouseDown(MouseEventArgs e)
        {
            bool vResult = base.MouseDown(e);
            if (OwnerData.CanEdit() && (e.Button == MouseButtons.Left))
            {
                int vIndex = GetItemAt(e.X, e.Y);
                if (vIndex >= 0)
                    FItems[vIndex].Checked = !FItems[vIndex].Checked;
            }

            return vResult;
        }
    
        public override bool MouseMove(MouseEventArgs e)
        {
            HC.GCursor = Cursors.Default;
            return base.MouseMove(e);
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
            FItems = new HCList<HCRadioButton>();
            FItems.OnInsert += new EventHandler<NListEventArgs<HCRadioButton>>(DoItemNotify);
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
                for (int i = 1; i < FItems.Count; i++)
                    vS = vS + HC.sLineBreak + FItems[i].Text;
            }

            HC.HCSaveTextToStream(aStream, vS);
            byte[] vBuffer;
            for (int i = 0; i < FItems.Count; i++)
            {
                vBuffer = BitConverter.GetBytes(FItems[i].Checked);
                aStream.Write(vBuffer, 0, vBuffer.Length);
            }

            aStream.WriteByte((byte)this.FRadioStyle);
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            FItems.Clear();

            string vS = "";
            byte[] vBuffer;
            HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            if (vS != "")
            {
                string[] vStrings = vS.Split(new string[] { HC.sLineBreak }, StringSplitOptions.None);

                for (int i = 0; i < vStrings.Length; i++)
                    AddItem(vStrings[i]);

                vBuffer = BitConverter.GetBytes(false);
                for (int i = 0; i < FItems.Count; i++)
                {
                    aStream.Read(vBuffer, 0, vBuffer.Length);
                    FItems[i].Checked = BitConverter.ToBoolean(vBuffer, 0);
                }
            }

            if (aFileVersion > 33)
                this.FRadioStyle = (HCRadioStyle)aStream.ReadByte();
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
            aNode.SetAttribute("radiostyle", ((byte)FRadioStyle).ToString());
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

            FRadioStyle = (HCRadioStyle)(byte.Parse(aNode.Attributes["radiostyle"].Value));
        }
    
        public bool MultSelect
        {
            get { return FMultSelect; }
            set { FMultSelect = value; }
        }

        public HCRadioStyle RadioStyle
        {
            get { return FRadioStyle; }
            set { FRadioStyle = value; }
        }

        public List<HCRadioButton> Items
        {
            get { return FItems; }
        }
    }
}
