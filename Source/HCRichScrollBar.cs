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

namespace HC.View
{
    class AreaMark  // 区域标记
    {
        private int FPosition, FHeight;

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
    }

    class HCRichScrollBar : HCScrollBar
    {
        private List<AreaMark> FAreaMarks;

        protected override void DoDrawThumBefor(HCCanvas aCanvas, RECT aThumRect)
        {
            if (this.Orientation == View.Orientation.oriVertical)
            {
                if (FAreaMarks != null)
                {
                    aCanvas.Brush.Color = Color.Blue;
                    int vDrawTop = 0, vDrawHeight = 0;

                    for (int i = 0; i <= FAreaMarks.Count - 1; i++)
                    {
                        vDrawTop = ButtonSize + (int)Math.Round(FAreaMarks[i].Position * Percent);
                        vDrawHeight = (int)Math.Round(FAreaMarks[i].Height * Percent);
                        aCanvas.FillRect(HC.Bounds(aThumRect.Left, vDrawTop, aThumRect.Width, vDrawHeight));
                    }
                }
            }
        }

        public HCRichScrollBar()
        {

        }

        ~HCRichScrollBar()
        {

        }

        //
        public void AddAreaPos(int aPosition, int  aHeight)
        {
            AreaMark vAreaMark = new AreaMark();
            vAreaMark.Position = aPosition;
            vAreaMark.Height = aHeight;

            if (FAreaMarks == null)
                FAreaMarks = new List<AreaMark>();

            FAreaMarks.Add(vAreaMark);
        }
    }
}
