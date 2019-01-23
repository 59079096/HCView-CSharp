/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{         文档PageBreakItem(分页）对象实现单元          }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HC.View
{
    class HCPageBreakItem : HCCustomRectItem
    {
        public override bool JustifySplit()
        {
            return false;
        }

        public HCPageBreakItem(HCCustomData aOwnerData)
            : base(aOwnerData)
        {
            StyleNo = HCStyle.PageBreak;

            if (aOwnerData.Style.CurStyleNo > HCStyle.Null)
                aOwnerData.Style.TextStyles[aOwnerData.Style.CurStyleNo].ApplyStyle(aOwnerData.Style.DefCanvas);
            else
                aOwnerData.Style.TextStyles[0].ApplyStyle(aOwnerData.Style.DefCanvas);

            Height = aOwnerData.Style.DefCanvas.TextHeight("H");
        }

        public HCPageBreakItem(HCCustomData aOwnerData, int aWidth, int aHeight) : base(aOwnerData, aWidth, aHeight)
        {
            StyleNo = HCStyle.PageBreak;

            if (aOwnerData.Style.CurStyleNo > HCStyle.Null)
                aOwnerData.Style.TextStyles[aOwnerData.Style.CurStyleNo].ApplyStyle(aOwnerData.Style.DefCanvas);
            else
                aOwnerData.Style.TextStyles[0].ApplyStyle(aOwnerData.Style.DefCanvas);

            Height = aOwnerData.Style.DefCanvas.TextHeight("H");
        }
    }
}
