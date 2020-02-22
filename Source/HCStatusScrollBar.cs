/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2020-2-20             }
{                                                       }
{                文档高级滚动条实现单元                 }
{                                                       }
{*******************************************************/

using HC.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace HC.View
{
    public class HCStatus : Object
    {
        private int FWidth;
        private string FText;
        private EventHandler FOnChange;

        private void SetWidth(int value)
        {
            if (FWidth != value)
            {
                FWidth = value;
                this.DoChange();
            }
        }

        private void SetText(string value)
        {
            if (FText != value)
            {
                FText = value;
                this.DoChange();
            }
        }

        private void DoChange()
        {
            if (FOnChange != null)
                FOnChange(this, null);
        }

        public HCStatus()
        {
            FWidth = 100;
            FText = "";
        }

        public int Width
        {
            get { return FWidth; }
            set { SetWidth(value); }
        }

        public string Text
        {
            get { return FText; }
            set { SetText(value); }
        }

        public EventHandler OnChange
        {
            get { return FOnChange; }
            set { FOnChange = value; }
        }
    }

    public class HCStatusScrollBar : HCScrollBar
    {
        private List<HCStatus> FStatuses;

        private void DoStatusChange(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        public HCStatusScrollBar() : base()
        {
            FStatuses = new List<HCStatus>();
        }

        public override void PaintToEx(HCCanvas ACanvas)
        {
            base.PaintToEx(ACanvas);

            if (this.Orientation == Orientation.oriHorizontal)
            {
                if (FStatuses.Count > 0)
                {
                    ACanvas.Brush.Color = Color.FromArgb(0x52, 0x59, 0x6B);
                    ACanvas.FillRect(new RECT(2, 2, FLeftBtnRect.Left, this.Height - 2));
                    ACanvas.Font.BeginUpdate();
                    try
                    {
                        ACanvas.Font.Size = 8;
                        ACanvas.Font.Color = Color.FromArgb(0xD0, 0xD1, 0xD5);
                        ACanvas.Font.Family = "Arial";
                        ACanvas.Font.FontStyles.Value = 0;
                    }
                    finally
                    {
                        ACanvas.Font.EndUpdate();
                    }

                    int vLeft = 4;
                    string vText = "";
                    RECT vRect = new RECT(0, 2, 0, Height - 2);
                    for (int i = 0; i < FStatuses.Count; i++)
                    {
                        vText = FStatuses[i].Text;
                        vRect.Left = vLeft;
                        vRect.Right = vLeft + FStatuses[i].Width;
                        ACanvas.TextRect(ref vRect, vText, User.DT_LEFT | User.DT_SINGLELINE | User.DT_VCENTER);
                        vLeft += FStatuses[i].Width + 2;
                    }
                }
            }
        }

        public void AddStatus(int width)
        {
            HCStatus vStatus = new HCStatus();
            vStatus.OnChange = DoStatusChange;
            vStatus.Width = width;
            FStatuses.Add(vStatus);

            int vWidth = 0;
            for (int i = 0; i < FStatuses.Count; i++)
                vWidth += FStatuses[i].Width;

            FLeftBlank = vWidth;
            this.Invalidate();
        }

        public List<HCStatus> Statuses
        {
            get { return FStatuses; }
        }
    }
}
