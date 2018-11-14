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

        private int GetItemAt(int X, int  Y)
        {
            int Result = -1;
            this.OwnerData.Style.TextStyles[TextStyleNo].ApplyStyle(this.OwnerData.Style.DefCanvas);
            
            SIZE vSize = new SIZE();
            for (int i = 0; i <= FItems.Count - 1; i++)
            {
                vSize = this.OwnerData.Style.DefCanvas.TextExtent(FItems[i].Text);
                if (HC.PtInRect(HC.Bounds(FItems[i].Position.X, FItems[i].Position.Y,
                    RadioButtonWidth + vSize.cx, vSize.cy), X, Y))
                
                {
                    Result = i;
                    break;
                }
            }

            return Result;
        }

        public override void FormatToDrawItem(HCCustomData ARichData, int AItemNo)
        {
            Height = FMinHeight;
            ARichData.Style.TextStyles[TextStyleNo].ApplyStyle(ARichData.Style.DefCanvas);
            int vLeft = FMargin;
            int vTop = FMargin;
            SIZE vSize = new SIZE();

            for (int i = 0; i <= FItems.Count - 1; i++)
            {
                if (FItems[i].Text != "")
                    vSize = ARichData.Style.DefCanvas.TextExtent(FItems[i].Text);
                else
                    vSize = ARichData.Style.DefCanvas.TextExtent("I");
                
                if (vLeft + vSize.cx + RadioButtonWidth > Width)
                {
                    vLeft = FMargin;
                    vTop = vTop + vSize.cy + FMargin;
                }

                FItems[i].Position.X = vLeft;
                FItems[i].Position.Y = vTop;
                
                vLeft = vLeft + RadioButtonWidth + vSize.cx;
            }
            
            Height = vTop + vSize.cy + FMargin;
            
            if (Width < FMinWidth)
                Width = FMinWidth;
            if (Height < FMinHeight)
                Height = FMinHeight;
        }
    
        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, int ADataDrawTop, int ADataDrawBottom, 
            int ADataScreenTop, int ADataScreenBottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            base.DoPaint(AStyle, ADrawRect, ADataDrawTop, ADataDrawBottom, ADataScreenTop,
                ADataScreenBottom, ACanvas, APaintInfo);
            
            if (FMouseIn)
            {
                ACanvas.Brush.Color = HC.clBtnFace;
                ACanvas.FillRect(ADrawRect);
            }

            AStyle.TextStyles[TextStyleNo].ApplyStyle(ACanvas, APaintInfo.ScaleY / APaintInfo.Zoom);
            
            POINT vPoint = new POINT();
            RECT vItemRect = new RECT();
            for (int i = 0; i <= FItems.Count - 1; i++)
            {
                vPoint.X = FItems[i].Position.X;
                vPoint.Y = FItems[i].Position.Y;
                vPoint.Offset(ADrawRect.Left, ADrawRect.Top);
                vItemRect = HC.Bounds(vPoint.X, vPoint.Y, RadioButtonWidth, RadioButtonWidth);
                if (FItems[i].Checked)
                    User.DrawFrameControl(ACanvas.Handle, ref vItemRect, Kernel.DFC_BUTTON, Kernel.DFCS_CHECKED | Kernel.DFCS_BUTTONRADIO);
                else
                    User.DrawFrameControl(ACanvas.Handle, ref vItemRect, Kernel.DFC_BUTTON, Kernel.DFCS_BUTTONRADIO);
                
                ACanvas.TextOut(vPoint.X + RadioButtonWidth, vPoint.Y, FItems[i].Text);
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

        public override void GetCaretInfo(ref HCCaretInfo ACaretInfo)
        {
            if (this.Active)
                ACaretInfo.Visible = false;
        }

        public override int GetOffsetAt(int X)
        {
            if (X <= FMargin)
                return HC.OffsetBefor;
            else
                if (X >= Width - FMargin)
                    return HC.OffsetAfter;
                else
                    return HC.OffsetInner;
        }

        public override void SaveToStream(Stream AStream, int AStart, int  AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);
            string vS = "";
            for (int i = 0; i < FItems.Count; i++)
                vS = vS + FItems[i].Text + "\r\n";

            int vLen = System.Text.Encoding.Default.GetByteCount(vS);
            if (vLen > ushort.MaxValue)
                throw new Exception(HC.HCS_EXCEPTION_TEXTOVER);

            ushort vSize = (ushort)vLen;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            if (vSize > 0)
                vBuffer = System.Text.Encoding.Default.GetBytes(vS);

            for (int i = 0; i < FItems.Count; i++)
            {
                vBuffer = BitConverter.GetBytes(FItems[i].Checked);
                AStream.Write(vBuffer, 0, vBuffer.Length);
            }
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            base.LoadFromStream(AStream, AStyle, AFileVersion);

            ushort vSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vSize);
            AStream.Read(vBuffer, 0, vBuffer.Length);

            if (vSize > 0)
            {
                vBuffer = new byte[vSize];
                AStream.Read(vBuffer, 0, vBuffer.Length);
                string vTexts = System.Text.Encoding.Default.GetString(vBuffer);
                string[] vStrings = vTexts.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                for (int i = 0; i < vStrings.Length; i++)
                    AddItem(vStrings[i]);

                vBuffer = BitConverter.GetBytes(false);
                for (int i = 0; i < FItems.Count; i++)
                {
                    AStream.Read(vBuffer, 0, vBuffer.Length);
                    FItems[i].Checked = BitConverter.ToBoolean(vBuffer, 0);
                }
            }
        }
        
        public HCRadioGroup(HCCustomData AOwnerData) : base(AOwnerData)
        {
            this.StyleNo = HCStyle.RadioGroup;
            Width = 100;
            FItems = new List<HCRadioButton>();
        }

        ~HCRadioGroup()
        {
            //FItems
        }

        public override void Assign(HCCustomItem Source)
        {
            base.Assign(Source);
            HCRadioGroup vSource = Source as HCRadioGroup;

            FItems.Clear();
            for (int i = 0; i < vSource.Items.Count; i++)
                AddItem(vSource.Items[i].Text, vSource.Items[i].Checked);
        }

        public void AddItem(string AText, bool AChecked = false)
        {
            HCRadioButton vRadioButton = new HCRadioButton();
            vRadioButton.Checked = AChecked;
            vRadioButton.Text = AText;
            FItems.Add(vRadioButton);
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
