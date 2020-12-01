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
            if (FChecked != value)
            {
                FChecked = value;
                if (FOnSetChecked != null)
                    FOnSetChecked(this, null);
            }
        }

        public string Text = "";
        public string TextValue = "";
        public RECT Rect = new RECT();

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
        private bool FMultSelect, FMouseIn, FItemHit;
        private Byte FColumns, FBatchCount;
        private bool FColumnAlign;
        private HCList<HCRadioButton> FItems;
        private HCRadioStyle FRadioStyle = HCRadioStyle.Radio;
        public static byte RadioButtonWidth = 16;

        private void ReLayout()
        {
            if (FBatchCount > 0)
                return;

            //if (FItems == null)
            //    return;

            OwnerData.Style.ApplyTempStyle(TextStyleNo);
            int vLeft = FPaddingLeft;
            int vTop = FPaddingTop;
            SIZE vSize = new SIZE();
            if (FColumns == 0)
            {
                for (int i = 0; i < FItems.Count; i++)
                {
                    if (FItems[i].Text != "")
                        vSize = OwnerData.Style.TempCanvas.TextExtent(FItems[i].Text);
                    else
                        vSize = OwnerData.Style.TempCanvas.TextExtent("H");

                    if (this.AutoSize && (vLeft + vSize.cx + RadioButtonWidth > Width))
                    {
                        vLeft = FPaddingLeft;
                        vTop += vSize.cy + FPaddingBottom;
                    }

                    FItems[i].Rect.ReSetBounds(vLeft, vTop, RadioButtonWidth + vSize.cx, vSize.cy);
                    vLeft += RadioButtonWidth + vSize.cx + FPaddingRight;
                }

                if (this.AutoSize)
                    Width = vLeft;

                Height = vTop + vSize.cy + FPaddingBottom;
            }
            else
            {
                int vWMax = 0;
                vSize.cy = 0;
                int vCol = 1, vColumnAct = FColumns;
                if (FColumns > FItems.Count)
                    vColumnAct = FItems.Count;

                for (int i = 0; i < FItems.Count; i++)
                {
                    if (FItems[i].Text != "")
                        vSize = OwnerData.Style.TempCanvas.TextExtent(FItems[i].Text);
                    else
                        vSize = OwnerData.Style.TempCanvas.TextExtent("H");

                    FItems[i].Rect.ReSetBounds(vLeft, vTop, RadioButtonWidth + vSize.cx, vSize.cy);
                    vLeft += RadioButtonWidth + vSize.cx + FPaddingRight;

                    if (vCol == vColumnAct)
                    {
                        if (vLeft > vWMax)
                            vWMax = vLeft;

                        if (i < FItems.Count - 1)
                        {
                            vCol = 1;
                            vLeft = FPaddingLeft;
                            vTop += vSize.cy + FPaddingBottom;
                        }
                    }
                    else
                        vCol++;
                }

                Height = vTop + vSize.cy + FPaddingBottom;

                if (FColumnAlign)
                {
                    for (int i = 0; i < vColumnAct - 1; i++)
                    {
                        vCol = i;
                        vWMax = FItems[vCol].Rect.Right;
                        while (vCol + vColumnAct < FItems.Count)
                        {
                            vCol += vColumnAct;
                            if (vWMax < FItems[vCol].Rect.Right)
                                vWMax = FItems[vCol].Rect.Right;
                        }

                        vWMax += FPaddingRight;
                        vCol = i + 1;
                        FItems[vCol].Rect.Offset(vWMax - FItems[vCol].Rect.Left, 0);
                        while (vCol + vColumnAct < FItems.Count)
                        {
                            vCol += vColumnAct;
                            FItems[vCol].Rect.Offset(vWMax - FItems[vCol].Rect.Left, 0);
                        }
                    }

                    if (AutoSize)
                    {
                        vCol = vColumnAct - 1;
                        vWMax = FItems[vCol].Rect.Right;
                        while (vCol + vColumnAct < FItems.Count)
                        {
                            vCol += vColumnAct;
                            if (vWMax < FItems[vCol].Rect.Right)
                                vWMax = FItems[vCol].Rect.Right;
                        }

                        Width = vWMax + FPaddingRight;
                    }
                }
                else
                if (AutoSize)
                    Width = vWMax;
            }
        }

        private int GetItemAt(int x, int y)
        {
            int Result = -1;

            for (int i = 0; i <= FItems.Count - 1; i++)
            {
                if (FItemHit)
                {
                    if (HC.PtInRect(FItems[i].Rect, x, y))
                    {
                        Result = i;
                        break;
                    }
                }
                else
                {
                    if (HC.PtInRect(HC.Bounds(FItems[i].Rect.Left, FItems[i].Rect.Top,
                        RadioButtonWidth, RadioButtonWidth), x, y))
                    {
                        Result = i;
                        break;
                    }
                }
            }

            return Result;
        }

        private void SetColumns(Byte value)
        {
            if (FColumns != value)
            {
                FColumns = value;
                ReLayout();
            }
        }

        private void SetColumnAlig(bool value)
        {
            if (FColumnAlign != value)
            {
                FColumnAlign = value;
                ReLayout();
            }
        }

        protected void DoItemNotify(object sender, NListEventArgs<HCRadioButton> e)
        {
            e.Item.OnSetChecked = DoItemSetChecked;
            this.ReLayout();
        }

        private void OnItemDelete(object sender, NListEventArgs<HCRadioButton> e)
        {
            this.ReLayout();
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

        protected void DoPaintItems(HCCanvas canvas, RECT drawRect, PaintInfo paintInfo)
        {
            POINT vPoint = new POINT();
            RECT vItemRect;
            for (int i = 0; i <= FItems.Count - 1; i++)
            {
                vPoint.X = FItems[i].Rect.Left;
                vPoint.Y = FItems[i].Rect.Top;
                vPoint.Offset(drawRect.Left, drawRect.Top);
                vItemRect = HC.Bounds(vPoint.X, vPoint.Y, RadioButtonWidth, RadioButtonWidth);

                if (paintInfo.Print)
                {
                    if (FItems[i].Checked)
                    {
                        if (FRadioStyle == HCRadioStyle.Radio)
                            HC.HCDrawFrameControl(canvas, vItemRect, HCControlState.hcsChecked, HCControlStyle.hcyRadio);
                        else
                            HC.HCDrawFrameControl(canvas, vItemRect, HCControlState.hcsChecked, HCControlStyle.hcyCheck);
                    }
                    else
                    {
                        if (FRadioStyle == HCRadioStyle.Radio)
                            HC.HCDrawFrameControl(canvas, vItemRect, HCControlState.hcsCustom, HCControlStyle.hcyRadio);
                        else
                            HC.HCDrawFrameControl(canvas, vItemRect, HCControlState.hcsCustom, HCControlStyle.hcyCheck);
                    }

                    canvas.Brush.Style = HCBrushStyle.bsClear;
                }
                else
                {
                    if (FItems[i].Checked)
                    {
                        if (FRadioStyle == HCRadioStyle.Radio)
                            User.DrawFrameControl(canvas.Handle, ref vItemRect, Kernel.DFC_BUTTON, Kernel.DFCS_CHECKED | Kernel.DFCS_BUTTONRADIO);
                        else
                            User.DrawFrameControl(canvas.Handle, ref vItemRect, Kernel.DFC_BUTTON, Kernel.DFCS_CHECKED | Kernel.DFCS_BUTTONCHECK);
                    }
                    else
                    {
                        if (FRadioStyle == HCRadioStyle.Radio)
                            User.DrawFrameControl(canvas.Handle, ref vItemRect, Kernel.DFC_BUTTON, Kernel.DFCS_BUTTONRADIO);
                        else
                            User.DrawFrameControl(canvas.Handle, ref vItemRect, Kernel.DFC_BUTTON, Kernel.DFCS_BUTTONCHECK);
                    }
                }

                canvas.TextOut(vPoint.X + RadioButtonWidth, vPoint.Y, FItems[i].Text);
            }
        }

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
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
            
            if (aPaintInfo.Print)
            {

            }
            else
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

            aStyle.TextStyles[TextStyleNo].ApplyStyle(aCanvas, aPaintInfo.ScaleY / aPaintInfo.Zoom);
            if (!AutoSize)
            {
                RECT vClipBoxRect = new RECT();
                GDI.GetClipBox(aCanvas.Handle, ref vClipBoxRect);

                IntPtr vPaintRegion = GDI.CreateRectRgn(
                    aPaintInfo.GetScaleX(aDrawRect.Left),
                    aPaintInfo.GetScaleY(aDrawRect.Top),
                    aPaintInfo.GetScaleX(aDrawRect.Right),
                    aPaintInfo.GetScaleY(aDrawRect.Bottom));
                try
                {
                    GDI.SelectClipRgn(aCanvas.Handle, vPaintRegion);
                    DoPaintItems(aCanvas, aDrawRect, aPaintInfo);
                }
                finally
                {
                    GDI.DeleteObject(vPaintRegion);
                }

                //vPaintRegion = GDI.CreateRectRgnIndirect(ref vRect);
                vPaintRegion = GDI.CreateRectRgn(
                    aPaintInfo.GetScaleX(vClipBoxRect.Left),
                    aPaintInfo.GetScaleY(vClipBoxRect.Top),
                    aPaintInfo.GetScaleX(vClipBoxRect.Right),
                    aPaintInfo.GetScaleY(vClipBoxRect.Bottom));
                try
                {
                    GDI.SelectClipRgn(aCanvas.Handle, vPaintRegion);
                }
                finally
                {
                    GDI.DeleteObject(vPaintRegion);
                }
            }
            else
                DoPaintItems(aCanvas, aDrawRect, aPaintInfo);
        }

        public override bool MouseDown(MouseEventArgs e)
        {
            bool vResult = base.MouseDown(e);
            if (OwnerData.CanEdit() && (e.Button == MouseButtons.Left))
            {
                int vIndex = GetItemAt(e.X, e.Y);
                if (vIndex >= 0)
                {
                    FItems[vIndex].Checked = !FItems[vIndex].Checked;
                    this.DoChange();
                }
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

        public override int GetOffsetAt(int x)
        {
            if (x <= FPaddingLeft)
                return HC.OffsetBefor;
            else
            if (x >= Width - FPaddingRight)
                return HC.OffsetAfter;
            else
                return HC.OffsetInner;
        }

        protected override string GetText()
        {
            string vResult = base.GetText();
            for (int i = 0; i < FItems.Count; i++)
            {
                if (FItems[i].Checked)
                {
                    if (vResult != "")
                        vResult = vResult + "，" + FItems[i].Text;
                    else
                        vResult = vResult + FItems[i].Text;
                }
            }

            return vResult;
        }

        public HCRadioGroup(HCCustomData aOwnerData)
            : base(aOwnerData)
        {
            this.StyleNo = HCStyle.RadioGroup;
            Width = 100;
            FBatchCount = 0;
            FColumns = 0;
            FColumnAlign = true;
            FItemHit = false;
            FItems = new HCList<HCRadioButton>();
            FItems.OnInsert += new EventHandler<NListEventArgs<HCRadioButton>>(DoItemNotify);
            FItems.OnDelete += OnItemDelete;
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
                AddItem(vSource.Items[i].Text, vSource.Items[i].TextValue, vSource.Items[i].Checked);
        }

        public void BeginAdd()
        {
            FBatchCount++;
        }

        public void EndAdd()
        {
            if (FBatchCount > 0)
                FBatchCount--;

            if (FBatchCount == 0)
                ReLayout();
        }

        public void AddItem(string aText, string aTextValue = "", bool AChecked = false)
        {
            HCRadioButton vRadioButton = new HCRadioButton();
            vRadioButton.Checked = AChecked;
            vRadioButton.Text = aText;
            vRadioButton.TextValue = aTextValue;
            FItems.Add(vRadioButton);
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            Byte vByte = FColumns;
            aStream.WriteByte(vByte);

            if (FMultSelect)
                vByte = (byte)(vByte | (1 << 7));

            if (FItemHit)
                vByte = (byte)(vByte | (1 << 6));

            if (FColumnAlign)
                vByte = (byte)(vByte | (1 << 5));

            aStream.WriteByte(vByte);

            string vTexts = "", vTextValues = "";
            if (FItems.Count > 0)
            {
                vTexts = FItems[0].Text;
                vTextValues = FItems[0].TextValue;
                for (int i = 1; i < FItems.Count; i++)
                {
                    vTexts = vTexts + HC.sLineBreak + FItems[i].Text;
                    vTextValues = vTextValues + HC.sLineBreak + FItems[i].TextValue;
                }
            }

            HC.HCSaveTextToStream(aStream, vTexts);
            HC.HCSaveTextToStream(aStream, vTextValues);

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
            BeginAdd();
            try
            {
                byte[] vBuffer;

                if (aFileVersion > 39)
                {
                    FColumns = (byte)aStream.ReadByte();
                    Byte vByte = (byte)aStream.ReadByte();
                    FMultSelect = HC.IsOdd(vByte >> 7);
                    FItemHit = HC.IsOdd(vByte >> 6);
                    FColumnAlign = HC.IsOdd(vByte >> 5);
                }

                FItems.Clear();
                string vS = "";
                HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                if (vS != "")
                {
                    string[] vStrings = vS.Split(new string[] { HC.sLineBreak }, StringSplitOptions.None);
                    for (int i = 0; i < vStrings.Length; i++)
                        AddItem(vStrings[i]);

                    if (aFileVersion > 35)
                    {
                        HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                        if (vS != "")
                        {
                            vStrings = vS.Split(new string[] { HC.sLineBreak }, StringSplitOptions.None);
                            for (int i = 0; i < vStrings.Length; i++)
                                FItems[i].TextValue = vStrings[i];
                        }
                    }

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
            finally
            {
                EndAdd();
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);

            aNode.SetAttribute("col", FColumns.ToString());
            if (FMultSelect)
                aNode.SetAttribute("multsel", "1");

            if (FItemHit)
                aNode.SetAttribute("itemhit", "1");

            if (FColumnAlign)
                aNode.SetAttribute("colalign", "1");

            string vText = "", vTextValue = "";
            if (FItems.Count > 0)
            {
                vText = FItems[0].Text;
                vTextValue = FItems[0].TextValue;
                for (int i = 1; i < FItems.Count; i++)
                {
                    vText = vText + HC.sLineBreak + FItems[i].Text;
                    vTextValue = vTextValue + HC.sLineBreak + FItems[i].TextValue;
                }
            }

            aNode.SetAttribute("item", vText);
            aNode.SetAttribute("itemvalue", vTextValue);

            if (FItems.Count > 0)
            {
                if (FItems[0].Checked)
                    vText = "1";
                else
                    vText = "0";

                for (int i = 1; i < FItems.Count; i++)
                {
                    if (FItems[i].Checked)
                        vText += HC.sLineBreak + "1";
                    else
                        vText += HC.sLineBreak + "0";
                }
            }
            else
                vText = "";

            aNode.SetAttribute("check", vText);
            aNode.SetAttribute("radiostyle", ((byte)FRadioStyle).ToString());
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            BeginAdd();
            try
            {
                if (aNode.HasAttribute("col"))
                    FColumns = byte.Parse(aNode.GetAttribute("col"));

                if (aNode.HasAttribute("multsel"))
                    FMultSelect = bool.Parse(aNode.GetAttribute("multsel"));

                if (aNode.HasAttribute("itemhit"))
                    FItemHit = bool.Parse(aNode.GetAttribute("itemhit"));

                if (aNode.HasAttribute("colalign"))
                    FColumnAlign = bool.Parse(aNode.GetAttribute("colalign"));

                FItems.Clear();
                string vText = aNode.Attributes["item"].Value;
                string[] vStrings = vText.Split(new string[] { HC.sLineBreak }, StringSplitOptions.None);
                for (int i = 0; i < vStrings.Length; i++)
                    AddItem(vStrings[i]);

                if (aNode.HasAttribute("itemvalue"))
                {
                    vText = aNode.Attributes["itemvalue"].Value;
                    vStrings = vText.Split(new string[] { HC.sLineBreak }, StringSplitOptions.None);
                    for (int i = 0; i < vStrings.Length; i++)
                        FItems[i].TextValue = vStrings[i];
                }

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
            finally
            {
                EndAdd();
            }
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

        public bool ItemHit
        {
            get { return FItemHit; }
            set { FItemHit = value; }
        }

        public byte Columns
        {
            get { return FColumns; }
            set { SetColumns(value); }
        }

        public bool ColumnAlign
        {
            get { return FColumnAlign; }
            set { SetColumnAlig(value); }
        }

        public List<HCRadioButton> Items
        {
            get { return FItems; }
        }
    }
}
