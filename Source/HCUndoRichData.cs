/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{             文档撤销、恢复相关操作单元                }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HC.View
{
    public class HCUndoRichData : HCCustomRichData
    {
        private int FFormatFirstItemNo, FFormatLastItemNo, FUndoGroupCount, FItemCount;

        private void DoUndoRedo(HCCustomUndo AUndo)
        {

        }

        public HCUndoRichData(HCStyle AStyle) : base(AStyle)
        {

        }

        public override void Undo(HCCustomUndo AUndo)
        {

        }

        public override void Redo(HCCustomUndo ARedo)
        {

        }
    }
}
