/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                 文档Tab对象实现单元                   }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;

namespace HC.View
{
    public class HCTabItem : HCTextRectItem
    {
        protected override void DoPaint(HCStyle aStyle, Win32.RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        public HCTabItem(HCCustomData aOwnerData) : base(aOwnerData)
        {
            StyleNo = HCStyle.Tab;
            aOwnerData.Style.ApplyTempStyle(TextStyleNo);
            SIZE vSize = aOwnerData.Style.TempCanvas.TextExtent("汉字");
            Width = vSize.cx;
            Height = vSize.cy;
        }

        public HCTabItem(HCCustomData aOwnerData, int aWidth, int aHeight) : base(aOwnerData, aWidth, aHeight)
        {
            StyleNo = HCStyle.Tab;
            aOwnerData.Style.ApplyTempStyle(TextStyleNo);
            SIZE vSize = aOwnerData.Style.TempCanvas.TextExtent("汉字");
            if (aWidth > 0)
                Width = aWidth;
            else
                Width = vSize.cx;

            if (aHeight > 0)
                Height = aHeight;
            else
                Height = vSize.cy;
        }

        public override bool JustifySplit()
        {
            return false;
        }

        public override int GetOffsetAt(int x)
        {
            if (x < Width / 2)
                return HC.OffsetBefor;
            else
                return HC.OffsetAfter;
        }
    }
}
