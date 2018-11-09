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

        public HCPageBreakItem(HCCustomData AOwnerData)
            : base(AOwnerData)
        {
            StyleNo = HCStyle.PageBreak;

            if (AOwnerData.Style.CurStyleNo > HCStyle.Null)
                AOwnerData.Style.TextStyles[AOwnerData.Style.CurStyleNo].ApplyStyle(AOwnerData.Style.DefCanvas);
            else
                AOwnerData.Style.TextStyles[0].ApplyStyle(AOwnerData.Style.DefCanvas);

            Height = AOwnerData.Style.DefCanvas.TextHeight("H");
        }

        public HCPageBreakItem(HCCustomData AOwnerData, int AWidth, int AHeight) : base(AOwnerData, AWidth, AHeight)
        {
            StyleNo = HCStyle.PageBreak;

            if (AOwnerData.Style.CurStyleNo > HCStyle.Null)
                AOwnerData.Style.TextStyles[AOwnerData.Style.CurStyleNo].ApplyStyle(AOwnerData.Style.DefCanvas);
            else
                AOwnerData.Style.TextStyles[0].ApplyStyle(AOwnerData.Style.DefCanvas);

            Height = AOwnerData.Style.DefCanvas.TextHeight("H");
        }
    }
}
