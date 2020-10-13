/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                文档高级滚动条实现单元                 }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;
using System.Drawing;
using System.Windows.Forms;

namespace HC.View
{
    class AreaMark  // 区域标记
    {
        private int FTag, FPosition, FHeight;

        public int Position
        {
            get { return FPosition; }
            set { FPosition = value; }
        }

        public int Height
        {
            get { return FHeight; }
            set { FHeight = value; }
        }

        public int Tag
        {
            get { return FTag; }
            set { FTag = value; }
        }
    }

    public class HCRichScrollBar : HCScrollBar
    {
        private List<AreaMark> FAreaMarks;
        private EventHandler FOnPageUpClick, FOnPageDownClick;

        private int GetAreaMarkByTag(int aTag)
        {
            int Result = -1;
            for (int i = 0; i < FAreaMarks.Count; i++)
            {
                if (FAreaMarks[i].Tag == aTag)
                {
                    Result = i;
                    break;
                }
            }

            return Result;
        }

        private RECT GetAreaMarkRect(int aIndex)
        {
            RECT Result = new RECT();
            if (this.Orientation == Orientation.oriVertical)
            {
                int vTop = FLeftBlank + HCScrollBar.ButtonSize + (int)Math.Round(FAreaMarks[aIndex].Position * Percent);
                int vHeight = (int)Math.Round(FAreaMarks[aIndex].Height * Percent);
                if (vHeight < 2)
                    vHeight = 2;

                Result = HC.Bounds(0, vTop, Width, vHeight);
            }

            return Result;
        }

        protected override void DoDrawThumBefor(HCCanvas aCanvas, RECT aThumRect)
        {
            if (this.Orientation == View.Orientation.oriVertical)
            {
                if (FAreaMarks != null)  // 有标记区域
                {
                    aCanvas.Brush.Color = Color.FromArgb(0x52, 0x59, 0x6b);

                    RECT vRect;

                    for (int i = 0; i <= FAreaMarks.Count - 1; i++)
                    {
                        vRect = GetAreaMarkRect(i);
                        if ((vRect.Bottom > FLeftBlank + HCScrollBar.ButtonSize) && (vRect.Top < this.Height - FRightBlank - HCScrollBar.ButtonSize))
                            aCanvas.FillRect(vRect);
                    }
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (HC.PtInRect(FThumRect, FMouseDownPt))
                return;

            if (this.Orientation == Orientation.oriVertical)
            {
                if (FAreaMarks != null)
                {
                    RECT vRect;
                    for (int i = 0; i < FAreaMarks.Count; i++)
                    {
                        vRect = GetAreaMarkRect(i);
                        if (HC.PtInRect(vRect, FMouseDownPt))
                        {
                            this.Position = FAreaMarks[i].Position - vRect.Top;
                            break;
                        }
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (this.Orientation == Orientation.oriVertical)
            {
                POINT vPt = new POINT(e.X, e.Y);
                if (HC.PtInRect(HC.Bounds(2, Height - FRightBlank + 2, 16, 16), vPt))
                {
                    if (FOnPageUpClick != null)
                        FOnPageUpClick(this, null);
                }
                else
                if (HC.PtInRect(HC.Bounds(2, Height - FRightBlank + 2 + 16 + 2, 16, 16), vPt))
                {
                    if (FOnPageDownClick != null)
                        FOnPageDownClick(this, null);
                }
            }
        }

        public override void PaintToEx(HCCanvas aCanvas)
        {
            base.PaintToEx(aCanvas);

            if (this.Orientation == Orientation.oriVertical)  // 垂直滚动条
            {
                if (FRightBlank > 0)
                {
                    aCanvas.Brush.Color = Color.FromArgb(0x52, 0x59, 0x6B);
                    aCanvas.FillRect(new RECT(2, Height - FRightBlank + 2, Width - 2, Height - 2));

                    // 上按钮
                    aCanvas.Pen.Color = Color.FromArgb(0xAA, 0xAB, 0xB3);
                    int vX = (Width - 5) / 2;
                    int vY = Height - FRightBlank + 2 + ButtonSize - 9;
                    aCanvas.MoveTo(vX, vY);
                    aCanvas.LineTo(vX + 5, vY);
                    aCanvas.MoveTo(vX + 1, vY - 1);
                    aCanvas.LineTo(vX + 4, vY - 1);
                    aCanvas.MoveTo(vX + 2, vY - 2);
                    aCanvas.LineTo(vX + 3, vY - 2);

                    vY = vY - 3;
                    aCanvas.MoveTo(vX, vY);
                    aCanvas.LineTo(vX + 5, vY);
                    aCanvas.MoveTo(vX + 1, vY - 1);
                    aCanvas.LineTo(vX + 4, vY - 1);
                    aCanvas.MoveTo(vX + 2, vY - 2);
                    aCanvas.LineTo(vX + 3, vY - 2);

                    // 下按钮
                    vY = Height - FRightBlank + 2 + ButtonSize + 2 + 3;
                    aCanvas.MoveTo(vX, vY);
                    aCanvas.LineTo(vX + 5, vY);
                    aCanvas.MoveTo(vX + 1, vY + 1);
                    aCanvas.LineTo(vX + 4, vY + 1);
                    aCanvas.MoveTo(vX + 2, vY + 2);
                    aCanvas.LineTo(vX + 3, vY + 2);

                    vY = vY + 3;
                    aCanvas.MoveTo(vX, vY);
                    aCanvas.LineTo(vX + 5, vY);
                    aCanvas.MoveTo(vX + 1, vY + 1);
                    aCanvas.LineTo(vX + 4, vY + 1);
                    aCanvas.MoveTo(vX + 2, vY + 2);
                    aCanvas.LineTo(vX + 3, vY + 2);
                }
            }
        }

        public HCRichScrollBar()
        {
            FRightBlank = 40;
        }

        ~HCRichScrollBar()
        {

        }

        //
        public void SetAreaPos(int aTag, int aPosition, int aHeight)
        {
            if (FAreaMarks == null)
                FAreaMarks = new List<AreaMark>();

            int vIndex = GetAreaMarkByTag(aTag);
            if (vIndex < 0)
            {
                AreaMark vAreaMark = new AreaMark();
                vAreaMark.Tag = aTag;
                vAreaMark.Position = aPosition;
                vAreaMark.Height = aHeight;

                FAreaMarks.Add(vAreaMark);
                RECT vRect = GetAreaMarkRect(FAreaMarks.Count - 1);
                User.InvalidateRect(this.Handle, ref vRect, 0);
            }
            else
            if ((FAreaMarks[vIndex].Position != aPosition) || (FAreaMarks[vIndex].Height != aHeight))
            {
                RECT vRect = GetAreaMarkRect(vIndex);
                FAreaMarks[vIndex].Position = aPosition;
                FAreaMarks[vIndex].Height = aHeight;
                User.InvalidateRect(this.Handle, ref vRect, 0);

                vRect = GetAreaMarkRect(vIndex);
                User.InvalidateRect(this.Handle, ref vRect, 0);
            }
        }

        public EventHandler OnPageUpClick
        {
            get { return FOnPageUpClick; }
            set { FOnPageUpClick = value; }
        }

        public EventHandler OnPageDownClick
        {
            get { return FOnPageDownClick; }
            set { FOnPageDownClick = value; }
        }
    }
}
