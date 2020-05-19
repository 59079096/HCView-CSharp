/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{               文档对象对应的绘制对象                  }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using HC.Win32;

namespace HC.View
{
    public enum DrawOption : byte
    {
        doLineFirst, doLineLast, doParaFirst
    }

    public class HCCustomDrawItem
    {
        private HashSet<DrawOption> FOptions;
        protected bool GetLineFirst()
        {
            return FOptions.Contains(DrawOption.doLineFirst);
        }

        protected void SetLineFirst(bool value)
        {
            if (value)
                FOptions.Add(DrawOption.doLineFirst);
            else
                FOptions.Remove(DrawOption.doLineFirst);
        }

        protected bool GetParaFirst()
        {
            return FOptions.Contains(DrawOption.doParaFirst);
        }

        protected void SetParaFirst(bool value)
        {
            if (value)
                FOptions.Add(DrawOption.doParaFirst);
            else
                FOptions.Remove(DrawOption.doParaFirst);
        }

        public int
            ItemNo,    // 对应的Item
            CharOffs,  // 从1开始
            CharLen;   // 从CharOffs开始的字符长度

        public RECT Rect;  // 在文档中的格式化区域

        public HCCustomDrawItem()
        {
            FOptions = new HashSet<DrawOption>();
        }

        public int CharOffsetStart()
        {
            return CharOffs - 1;
        }

        public int CharOffsetEnd()
        {
            return CharOffs + CharLen - 1;
        }

        public int Width
        {
            get { return Rect.Width; }
        }

        public int Height
        {
            get {return Rect.Height;}
        }

        public bool LineFirst
        {
            get { return GetLineFirst(); }
            set { SetLineFirst(value); }
        }

        public bool ParaFirst
        {
            get { return GetParaFirst(); }
            set { SetParaFirst(value); }
        }
    }

    public class HCDrawItems : HCInhList<HCCustomDrawItem>
    {
        private int FDeleteStartDrawItemNo, FDeleteCount;

        private void HCDrawItems_OnInsert(object sender, NListInhEventArgs<HCCustomDrawItem> e)
        {
            if (FDeleteCount == 0)
            {

            }
            else
            {
                FDeleteStartDrawItemNo++;
                FDeleteCount--;
                this[e.Index] = e.Item;

                e.Inherited = false;
            }
        }

        public HCDrawItems()
        {
            this.OnInsert += new EventHandler<NListInhEventArgs<HCCustomDrawItem>>(HCDrawItems_OnInsert);
        }

        public new void Clear()
        {
            base.Clear();
            ClearFormatMark();
        }
        /// <summary> 在格式化前标记要删除的起始和结束DrawItemNo </summary>
        public void MarkFormatDelete(int aStartDrawItemNo, int aEndDrawItemNo)
        {
            FDeleteStartDrawItemNo = aStartDrawItemNo;
            FDeleteCount = aEndDrawItemNo - aStartDrawItemNo + 1;
        }

        /// <summary> 删除格式化前标记的起始和结束DrawItemNo </summary>
        public void DeleteFormatMark()
        {
            this.RemoveRange(FDeleteStartDrawItemNo, FDeleteCount);
            FDeleteStartDrawItemNo = -1;
            FDeleteCount = 0;
        }

        /// <summary> 初始化格式化参数 </summary>
        public void ClearFormatMark()
        {
            FDeleteStartDrawItemNo = -1;
            FDeleteCount = 0;
        }
    }
}
