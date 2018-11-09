/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-9-12             }
{                                                       }
{      文档CDateTimePicker(日期时间)对象实现单元        }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HC.View
{
    class HCDateTimePicker : HCEditItem
    {
        public HCDateTimePicker(HCCustomData AOwnerData, DateTime ADateTime)
            : base(AOwnerData, string.Format("YYYY-MM-DD HH:mm:SS", ADateTime))
        {

        }
    }
}
