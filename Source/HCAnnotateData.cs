/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-12-3             }
{                                                       }
{            支持批注功能的文档对象管理单元             }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;
using System.Windows.Forms;
using System.Drawing;

namespace HC.View
{
    public class HCDataAnnotate : SelectInfo
    {
        private int FID, FStartDrawItemNo, FEndDrawItemNo;
        private string FTitle, FText;

        public HCDataAnnotate()
        {

        }

        ~HCDataAnnotate()
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            FID = -1;
        }

        public void CopyRange(SelectInfo aSrc)
        {
            this.StartItemNo = aSrc.StartItemNo;
            this.StartItemOffset = aSrc.StartItemOffset;
            this.EndItemNo = aSrc.EndItemNo;
            this.EndItemOffset = aSrc.EndItemOffset;
        }

        public int ID
        {
            get { return FID; }
            set { FID = value; }
        }

        public int StartDrawItemNo
        {
            get { return FStartDrawItemNo; }
            set { FStartDrawItemNo = value; }
        }

        public int EndDrawItemNo
        {
            get { return FEndDrawItemNo; }
            set { FEndDrawItemNo = value; }
        }

        public string Title
        {
            get { return FTitle; }
            set { FTitle = value; }
        }

        public string Text
        {
            get { return FText; }
            set { FText = value; }
        }
    }

    public class HCDataAnnotates : HCList<HCDataAnnotate>
    {
        private EventHandler FOnInsertAnnotate, FOnRemoveAnnotate;

        private void HCDataAnnotate_OnInsert(object sender, NListEventArgs<HCDataAnnotate> e)
        {
            if (FOnInsertAnnotate != null)
                FOnInsertAnnotate(e.Item, null);
        }

        private void HCDataAnnotate_OnRemove(object sender, NListEventArgs<HCDataAnnotate> e)
        {
            if (FOnRemoveAnnotate != null)
                FOnRemoveAnnotate(e.Item, null);
        }

        public HCDataAnnotates()
        {
            this.OnInsert += new EventHandler<NListEventArgs<HCDataAnnotate>>(HCDataAnnotate_OnInsert);
            this.OnDelete += new EventHandler<NListEventArgs<HCDataAnnotate>>(HCDataAnnotate_OnRemove);
        }

        public void NewDataAnnotate(SelectInfo aSelectInfo, string aTitle, string aText)
        {
            HCDataAnnotate vDataAnnotate = new HCDataAnnotate();
            vDataAnnotate.CopyRange(aSelectInfo);
            vDataAnnotate.Title = aTitle;
            vDataAnnotate.Text = aText;
            this.Add(vDataAnnotate);
            vDataAnnotate.ID = this.Count - 1;
        }

        public EventHandler OnInsertAnnotate
        {
            get { return FOnInsertAnnotate; }
            set { FOnInsertAnnotate = value; }
        }

        public EventHandler OnRemoveAnnotate
        {
            get { return FOnRemoveAnnotate; }
            set { FOnRemoveAnnotate = value; }
        }
    }

    public enum HCAnnotateMark : byte
    {
        amFirst, amNormal, amLast, amBoth
    }

    public class HCDrawItemAnnotate : Object
    {
        public RECT DrawRect;
        public HCAnnotateMark Mark;
        public HCDataAnnotate DataAnnotate;

        public bool First()
        {
            return (Mark == HCAnnotateMark.amFirst) || (Mark == HCAnnotateMark.amBoth);
        }

        public bool Last()
        {
            return (Mark == HCAnnotateMark.amLast) | (Mark == HCAnnotateMark.amBoth);
        }
    }

    public class HCDrawItemAnnotates : HCList<HCDrawItemAnnotate>
    {
        public void NewDrawAnnotate(RECT aRect, HCAnnotateMark aMark, HCDataAnnotate aDataAnnotate)
        {
            HCDrawItemAnnotate vDrawItemAnnotate = new HCDrawItemAnnotate();
            vDrawItemAnnotate.DrawRect = aRect;
            vDrawItemAnnotate.Mark = aMark;
            vDrawItemAnnotate.DataAnnotate = aDataAnnotate;
            this.Add(vDrawItemAnnotate);
        }
    }

    public delegate void DataDrawItemAnnotateEventHandler(HCCustomData aData, int aDrawItemNo,
        RECT aDrawRect, HCDataAnnotate aDataAnnotate);

    public delegate void DataAnnotateEventHandler(HCCustomData aData, HCDataAnnotate aDataAnnotate);

    public delegate void DataItemNotifyEventHandler(HCCustomData aData, HCCustomItem aItem);

    public class HCAnnotateData : HCRichData
    {
        private HCDataAnnotates FDataAnnotates;
        private HCDataAnnotate FHotAnnotate, FActiveAnnotate;  // 当前高亮批注、当前激活的批注
        private HCDrawItemAnnotates FDrawItemAnnotates;
        private DataDrawItemAnnotateEventHandler FOnDrawItemAnnotate;
        private DataAnnotateEventHandler FOnInsertAnnotate, FOnRemoveAnnotate;
        private DataItemNotifyEventHandler FOnInsertItem, FOnRemoveItem;

        private void DoInsertItem(HCCustomItem aItem)
        {
            DoDataInsertItem(this, aItem);
        }

        private void DoRemoveItem(HCCustomItem aItem)
        {
            DoDataRemoveItem(this, aItem);
        }

        /// <summary> 获取指定的DrawItem所属的批注以及在各批注中的区域 </summary>
        /// <param name="ADrawItemNo"></param>
        /// <param name="ACanvas">应用了DrawItem样式的Canvas</param>
        /// <returns></returns>
        private bool DrawItemOfAnnotate(int aDrawItemNo, HCCanvas aCanvas, RECT aDrawRect)
        {
            if (FDataAnnotates.Count == 0)
                return false;

            int vItemNo = this.DrawItems[aDrawItemNo].ItemNo;
            if (vItemNo < FDataAnnotates[0].StartItemNo)
                return false;
            if (vItemNo > FDataAnnotates[FDataAnnotates.Count - 1].EndItemNo)
                return false;

            bool Result = false;
            FDrawItemAnnotates.Clear();
            for (int i = 0; i <= FDataAnnotates.Count - 1; i++)
            {
                HCDataAnnotate vDataAnnotate = FDataAnnotates[i];
                if (vDataAnnotate.EndItemNo < vItemNo)
                    continue;

                if (vDataAnnotate.StartItemNo > vItemNo)
                    break;

                if (aDrawItemNo == vDataAnnotate.StartDrawItemNo)
                {
                    if (aDrawItemNo == vDataAnnotate.EndDrawItemNo)
                    {
                        FDrawItemAnnotates.NewDrawAnnotate(
                            new RECT(aDrawRect.Left + GetDrawItemOffsetWidth(aDrawItemNo, vDataAnnotate.StartItemOffset - this.DrawItems[aDrawItemNo].CharOffs + 1, aCanvas),
                                aDrawRect.Top,
                                aDrawRect.Left + GetDrawItemOffsetWidth(aDrawItemNo, vDataAnnotate.EndItemOffset - this.DrawItems[aDrawItemNo].CharOffs + 1, aCanvas),
                                aDrawRect.Bottom),
                            HCAnnotateMark.amBoth, vDataAnnotate);
                    }
                    else
                    {
                        FDrawItemAnnotates.NewDrawAnnotate(
                            new RECT(aDrawRect.Left + GetDrawItemOffsetWidth(aDrawItemNo, vDataAnnotate.StartItemOffset - this.DrawItems[aDrawItemNo].CharOffs + 1, aCanvas),
                                aDrawRect.Top, aDrawRect.Right, aDrawRect.Bottom),
                        HCAnnotateMark.amFirst, vDataAnnotate);
                    }

                    Result = true;
                }
                else
                    if (aDrawItemNo == vDataAnnotate.EndDrawItemNo)
                    {
                        FDrawItemAnnotates.NewDrawAnnotate(
                            new RECT(aDrawRect.Left, aDrawRect.Top,
                                aDrawRect.Left + GetDrawItemOffsetWidth(aDrawItemNo, vDataAnnotate.EndItemOffset - this.DrawItems[aDrawItemNo].CharOffs + 1, aCanvas),
                                aDrawRect.Bottom),
                        HCAnnotateMark.amLast, vDataAnnotate);

                        Result = true;
                    }
                    else
                    {
                        FDrawItemAnnotates.NewDrawAnnotate(aDrawRect, HCAnnotateMark.amNormal, vDataAnnotate);
                        Result = true;
                    }
            }

            return Result;
        }

        /// <summary> 指定DrawItem范围内的批注获取各自的DrawItem范围 </summary>
        /// <param name="AFirstDrawItemNo">起始DrawItem</param>
        /// <param name="ALastDrawItemNo">结束DrawItem</param>
        private void CheckAnnotateRange(int aFirstDrawItemNo, int aLastDrawItemNo)
        {
            if (aFirstDrawItemNo < 0)
                return;

            int vFirstNo = this.DrawItems[aFirstDrawItemNo].ItemNo;
            int vLastNo = this.DrawItems[aLastDrawItemNo].ItemNo;

            for (int i = 0; i <= FDataAnnotates.Count - 1; i++)
            {
                HCDataAnnotate vDataAnnotate = FDataAnnotates[i];

                if (vDataAnnotate.EndItemNo < vFirstNo)  // 未进入本次查找范围
                    continue;

                if (vDataAnnotate.StartItemNo > vLastNo)  // 超出本次查找的范围
                    break;

                vDataAnnotate.StartDrawItemNo =
                    this.GetDrawItemNoByOffset(vDataAnnotate.StartItemNo, vDataAnnotate.StartItemOffset);
                vDataAnnotate.EndDrawItemNo =
                    this.GetDrawItemNoByOffset(vDataAnnotate.EndItemNo, vDataAnnotate.EndItemOffset);
                if (vDataAnnotate.EndItemOffset == this.DrawItems[vDataAnnotate.EndDrawItemNo].CharOffs)  // 如果在结束的最前面，按上一个
                  vDataAnnotate.EndDrawItemNo = vDataAnnotate.EndDrawItemNo - 1;
            }
        }

        private HCDataAnnotate GetDrawItemFirstDataAnnotateAt(int aDrawItemNo, int x, int y)
        {
            HCDataAnnotate Result = null;

            int vStyleNo = GetDrawItemStyle(aDrawItemNo);
            if (vStyleNo > HCStyle.Null)
                Style.TextStyles[vStyleNo].ApplyStyle(Style.DefCanvas);

            if (DrawItemOfAnnotate(aDrawItemNo, Style.DefCanvas, DrawItems[aDrawItemNo].Rect))
            {
                POINT vPt = new POINT(x, y);
                for (int i = 0; i <= FDrawItemAnnotates.Count - 1; i++)
                {
                    if (HC.PtInRect(FDrawItemAnnotates[i].DrawRect, vPt))
                    {
                        Result = FDrawItemAnnotates[i].DataAnnotate;
                        break;
                    }
                }
            }

            return Result;
        }

        protected virtual void DoDataInsertItem(HCCustomData aData, HCCustomItem aItem)
        {
            if (FOnInsertItem != null)
                FOnInsertItem(aData, aItem);
        }

        protected virtual void DoDataRemoveItem(HCCustomData aData, HCCustomItem aItem)
        {
            if (FOnRemoveItem != null)
                FOnRemoveItem(aData, aItem);
        }

        protected override void DoItemAction(int aItemNo, int aOffset, HCItemAction aAction)
        {

        }

        protected override void DoDrawItemPaintContent(HCCustomData aData, int aDrawItemNo,
            RECT aDrawRect, RECT aClearRect, string aDrawText,
            int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if ((FOnDrawItemAnnotate != null) && DrawItemOfAnnotate(aDrawItemNo, aCanvas, aClearRect))  // 当前DrawItem是某批注中的一部分
            {
                for (int i = 0; i <= FDrawItemAnnotates.Count - 1; i++)
                {
                    HCDrawItemAnnotate vDrawAnnotate = FDrawItemAnnotates[i];

                    if (!aPaintInfo.Print)
                    {
                        bool vActive = vDrawAnnotate.DataAnnotate.Equals(FHotAnnotate)
                            || vDrawAnnotate.DataAnnotate.Equals(FActiveAnnotate);

                        if (vActive)
                            aCanvas.Brush.Color = HC.AnnotateBKActiveColor;
                        else
                            aCanvas.Brush.Color = HC.AnnotateBKColor;
                    }

                    if (vDrawAnnotate.First())  // 是批注头
                    {
                        aCanvas.Pen.Color = Color.Red;
                        aCanvas.MoveTo(vDrawAnnotate.DrawRect.Left + 2, vDrawAnnotate.DrawRect.Top - 2);
                        aCanvas.LineTo(vDrawAnnotate.DrawRect.Left, vDrawAnnotate.DrawRect.Top);
                        aCanvas.LineTo(vDrawAnnotate.DrawRect.Left, vDrawAnnotate.DrawRect.Bottom);
                        aCanvas.LineTo(vDrawAnnotate.DrawRect.Left + 2, vDrawAnnotate.DrawRect.Bottom + 2);
                    }

                    if (vDrawAnnotate.Last())  // 是批注尾
                    {
                        aCanvas.Pen.Color = Color.Red;
                        aCanvas.MoveTo(vDrawAnnotate.DrawRect.Right - 2, vDrawAnnotate.DrawRect.Top - 2);
                        aCanvas.LineTo(vDrawAnnotate.DrawRect.Right, vDrawAnnotate.DrawRect.Top);
                        aCanvas.LineTo(vDrawAnnotate.DrawRect.Right, vDrawAnnotate.DrawRect.Bottom);
                        aCanvas.LineTo(vDrawAnnotate.DrawRect.Right - 2, vDrawAnnotate.DrawRect.Bottom + 2);

                        FOnDrawItemAnnotate(aData, aDrawItemNo, vDrawAnnotate.DrawRect, vDrawAnnotate.DataAnnotate);
                    }
                }
            }

            base.DoDrawItemPaintContent(aData, aDrawItemNo, aDrawRect, aClearRect, aDrawText,
                aDataDrawLeft, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        protected void DoInsertAnnotate(object sender, EventArgs e)
        {
            Style.UpdateInfoRePaint();
            if (FOnInsertAnnotate != null)
                FOnInsertAnnotate(this, (HCDataAnnotate)sender);
        }

        protected void DoRemoveAnnotate(object sender, EventArgs e)
        {
            Style.UpdateInfoRePaint();
            if (FOnRemoveAnnotate != null)
                FOnRemoveAnnotate(this, (HCDataAnnotate)sender);
        }

        public HCAnnotateData(HCStyle aStyle) : base(aStyle)
        {
            FDataAnnotates = new HCDataAnnotates();
            FDataAnnotates.OnInsertAnnotate = DoInsertAnnotate;
            FDataAnnotates.OnRemoveAnnotate = DoRemoveAnnotate;
            FDrawItemAnnotates = new HCDrawItemAnnotates();
            FHotAnnotate = null;
            FActiveAnnotate = null;
            this.Items.OnInsertItem = DoInsertItem;
            this.Items.OnRemoveItem = DoRemoveItem;
        }

        ~HCAnnotateData()
        {
            FDataAnnotates.Clear();
            FDrawItemAnnotates.Clear();
        }

        public override void GetCaretInfo(int aItemNo, int aOffset, ref HCCaretInfo aCaretInfo)
        {
            base.GetCaretInfo(aItemNo, aOffset, ref aCaretInfo);

            HCDataAnnotate vDataAnnotate = GetDrawItemFirstDataAnnotateAt(CaretDrawItemNo,
                GetDrawItemOffsetWidth(CaretDrawItemNo,
                  SelectInfo.StartItemOffset - DrawItems[CaretDrawItemNo].CharOffs + 1),
                DrawItems[CaretDrawItemNo].Rect.Top + 1);

            if (FActiveAnnotate != vDataAnnotate)
            {
                FActiveAnnotate = vDataAnnotate;
                Style.UpdateInfoRePaint();
            }
        }

        public override void PaintData(int aDataDrawLeft, int aDataDrawTop, int aDataDrawBottom,
            int aDataScreenTop, int aDataScreenBottom, int aVOffset, int aFirstDItemNo, int aLastDItemNo,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            CheckAnnotateRange(aFirstDItemNo, aLastDItemNo);  // 指定DrawItem范围内的批注获取各自的DrawItem范围
            base.PaintData(aDataDrawLeft, aDataDrawTop, aDataDrawBottom, aDataScreenTop,
                aDataScreenBottom, aVOffset, aFirstDItemNo, aLastDItemNo, aCanvas, aPaintInfo);
            FDrawItemAnnotates.Clear();
        }

        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);

            HCDataAnnotate vDataAnnotate = GetDrawItemFirstDataAnnotateAt(MouseMoveDrawItemNo, e.X, e.Y);

            if (FHotAnnotate != vDataAnnotate)
            {
                FHotAnnotate = vDataAnnotate;
                Style.UpdateInfoRePaint();
            }
        }

        public override void InitializeField()
        {
            base.InitializeField();
            FHotAnnotate = null;
            FActiveAnnotate = null;
        }

        public override void Clear()
        {
            FDataAnnotates.Clear();
            base.Clear();
        }

        public bool InsertAnnotate(string aTitle, string aText)
        {
            if (!CanEdit())
                return false;

            if (!this.SelectExists())
                return false;

            FDataAnnotates.NewDataAnnotate(SelectInfo, aTitle, aText);
            return true;
        }

        public DataDrawItemAnnotateEventHandler OnDrawItemAnnotate
        {
            get { return FOnDrawItemAnnotate;}
            set { FOnDrawItemAnnotate = value; }
        }

        public DataAnnotateEventHandler OnInsertAnnotate
        {
            get { return FOnInsertAnnotate; }
            set { FOnInsertAnnotate = value; }
        }

        public DataAnnotateEventHandler OnRemoveAnnotate
        {
            get { return FOnRemoveAnnotate; }
            set { FOnRemoveAnnotate = value; }
        }

        public DataItemNotifyEventHandler OnInsertItem
        {
            get { return FOnInsertItem; }
            set { FOnInsertItem = value; }
        }

        public DataItemNotifyEventHandler OnRemoveItem
        {
            get { return FOnRemoveItem; }
            set { FOnRemoveItem = value; }
        }

        public HCDataAnnotate HotAnnotate
        {
            get { return FHotAnnotate; }
        }
        
        public HCDataAnnotate ActiveAnnotate
        {
            get { return FActiveAnnotate; }
        }
    }
}
