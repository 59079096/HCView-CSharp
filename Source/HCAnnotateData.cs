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
using System.IO;

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

        public void SaveToStream(Stream aStream)
        {
            byte[] vBuffer = BitConverter.GetBytes(this.StartItemNo);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(this.StartItemOffset);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(this.EndItemNo);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(this.EndItemOffset);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FID);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            HC.HCSaveTextToStream(aStream, FTitle);
            HC.HCSaveTextToStream(aStream, FText);
        }

        public void LoadFromStream(Stream aStream, ushort aFileVersion)
        {
            byte[] vBuffer = BitConverter.GetBytes(this.StartItemNo);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            this.StartItemNo = BitConverter.ToInt32(vBuffer, 0);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            this.StartItemOffset = BitConverter.ToInt32(vBuffer, 0);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            this.EndItemNo = BitConverter.ToInt32(vBuffer, 0);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            this.EndItemOffset = BitConverter.ToInt32(vBuffer, 0);

            aStream.Read(vBuffer, 0, vBuffer.Length);
            FID = BitConverter.ToInt32(vBuffer, 0);

            HC.HCLoadTextFromStream(aStream, ref FTitle, aFileVersion);
            HC.HCLoadTextFromStream(aStream, ref FText, aFileVersion);
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

        public void DeleteByID(int aID)
        {
            for (int i = 0; i <= this.Count - 1; i++)
            {
                if (this[i].ID == aID)
                {
                    this.RemoveAt(i);
                    break;
                }
            }
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

    public class HCAnnotateData : HCRichData
    {
        private HCDataAnnotates FDataAnnotates;
        private HCDataAnnotate FHotAnnotate, FActiveAnnotate;  // 当前高亮批注、当前激活的批注
        private HCDrawItemAnnotates FDrawItemAnnotates;
        private DataDrawItemAnnotateEventHandler FOnDrawItemAnnotate;
        private DataAnnotateEventHandler FOnInsertAnnotate, FOnRemoveAnnotate;

        /// <summary> 获取指定的DrawItem所属的批注以及在各批注中的区域 </summary>
        /// <param name="ADrawItemNo"></param>
        /// <param name="ACanvas">应用了DrawItem样式的Canvas</param>
        /// <returns></returns>
        private bool DrawItemOfAnnotate(int aDrawItemNo, HCCanvas aCanvas, RECT aDrawRect)
        {
            if (FDataAnnotates.Count == 0)
                return false;

            int vItemNo = this.DrawItems[aDrawItemNo].ItemNo;
            if (vItemNo < FDataAnnotates.First.StartItemNo)
                return false;
            if (vItemNo > FDataAnnotates.Last.EndItemNo)
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
                    else  // 仅是批注头
                    {
                        FDrawItemAnnotates.NewDrawAnnotate(
                            new RECT(aDrawRect.Left + GetDrawItemOffsetWidth(aDrawItemNo, vDataAnnotate.StartItemOffset - this.DrawItems[aDrawItemNo].CharOffs + 1, aCanvas),
                                aDrawRect.Top, aDrawRect.Right, aDrawRect.Bottom),
                        HCAnnotateMark.amFirst, vDataAnnotate);
                    }

                    Result = true;
                }
                else
                    if (aDrawItemNo == vDataAnnotate.EndDrawItemNo)  // 当前DrawItem是批注结束
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
                Style.ApplyTempStyle(vStyleNo);

            if (DrawItemOfAnnotate(aDrawItemNo, Style.TempCanvas, DrawItems[aDrawItemNo].Rect))
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

        #region  DoItemAction子方法

        private void _AnnotateRemove(int aItemNo, int aOffset)
        {

        }

        private void _AnnotateInsertChar(int aItemNo, int aOffset)
        {
            HCDataAnnotate vDataAnn;

            for (int i = FDataAnnotates.Count - 1; i >= 0; i--)
            {
                if (FDataAnnotates[i].StartItemNo > aItemNo)
                    continue;

                if (FDataAnnotates[i].EndItemNo < aItemNo)
                    break;

                vDataAnn = FDataAnnotates[i];
                if (vDataAnn.StartItemNo == aItemNo)
                {
                    if (vDataAnn.EndItemNo == aItemNo)
                    {
                        if (aOffset <= vDataAnn.StartItemOffset)
                        {
                            vDataAnn.StartItemOffset = vDataAnn.StartItemOffset + 1;
                            vDataAnn.EndItemOffset = vDataAnn.EndItemOffset + 1;
                        }
                        else
                        {
                            if (aOffset < vDataAnn.StartItemOffset)
                                vDataAnn.StartItemOffset = vDataAnn.StartItemOffset + 1;
                        }

                        if (vDataAnn.StartItemOffset == vDataAnn.EndItemOffset)
                            FDataAnnotates.Delete(i);
                    }
                    else  // 批注起始和结束不是同一个Item
                    {
                        if (aOffset <= vDataAnn.StartItemOffset)
                            vDataAnn.StartItemOffset = vDataAnn.StartItemOffset + 1;
                    }
                }
                else
                {
                    if (vDataAnn.EndItemNo == aItemNo)  // 是批注结束Item
                    {
                        if (aOffset <= vDataAnn.EndItemOffset)
                            vDataAnn.EndItemOffset = vDataAnn.EndItemOffset + 1;
                    }
                }
            }
        }

        private void _AnnotateBackChar(int aItemNo, int aOffset)
        {
            HCDataAnnotate vDataAnn;

            for (int i = FDataAnnotates.Count - 1; i >= 0; i--)
            {
                if (FDataAnnotates[i].StartItemNo > aItemNo)
                    continue;
                if (FDataAnnotates[i].EndItemNo < aItemNo)
                    break;

                vDataAnn = FDataAnnotates[i];
                if (vDataAnn.StartItemNo == aItemNo)
                {
                    if (vDataAnn.EndItemNo == aItemNo)
                    {
                        if ((aOffset > vDataAnn.StartItemOffset) && (aOffset <= vDataAnn.EndItemOffset))
                        {
                            vDataAnn.EndItemOffset = vDataAnn.EndItemOffset - 1;
                        }
                        else  // 在批注所在的Item批注位置前面删除
                        {
                            vDataAnn.StartItemOffset = vDataAnn.StartItemOffset - 1;
                            vDataAnn.EndItemOffset = vDataAnn.EndItemOffset - 1;
                        }
                    }
                    else
                    {
                        if (aOffset >= vDataAnn.StartItemOffset)
                            vDataAnn.StartItemOffset = vDataAnn.StartItemOffset - 1;
                    }
                }
                else
                {
                    if (vDataAnn.EndItemNo == aItemNo)
                    {
                        if (aOffset <= vDataAnn.EndItemOffset)
                            vDataAnn.EndItemOffset = vDataAnn.EndItemOffset - 1;
                    }
                }
            }
        }

        private void _AnnotateDeleteChar(int aItemNo, int aOffset)
        {
            HCDataAnnotate vDataAnn;

            for (int i = FDataAnnotates.Count - 1; i >= 0; i--)
            {
                if (FDataAnnotates[i].StartItemNo > aItemNo)
                    continue;
                if (FDataAnnotates[i].EndItemNo < aItemNo)
                    break;

                vDataAnn = FDataAnnotates[i];
                if (vDataAnn.StartItemNo == aItemNo)
                {
                    if (vDataAnn.EndItemNo == aItemNo)
                    {
                        if (aOffset <= vDataAnn.StartItemOffset)
                        {
                            vDataAnn.StartItemOffset = vDataAnn.StartItemOffset - 1;
                            vDataAnn.EndItemOffset = vDataAnn.EndItemOffset - 1;
                        }
                        else
                        {
                            if (aOffset <= vDataAnn.EndItemOffset)
                                vDataAnn.EndItemOffset = vDataAnn.EndItemOffset - 1;
                        }

                        if (vDataAnn.StartItemOffset == vDataAnn.EndItemOffset)
                            FDataAnnotates.Delete(i);
                    }
                    else
                    {
                        if (aOffset <= vDataAnn.StartItemOffset)
                            vDataAnn.StartItemOffset = vDataAnn.StartItemOffset - 1;
                    }
                }
                else
                {
                    if (vDataAnn.EndItemNo == aItemNo)
                    {
                        if (aOffset <= vDataAnn.EndItemOffset)
                            vDataAnn.EndItemOffset = vDataAnn.EndItemOffset - 1;
                    }
                }
            }
        }

        #endregion

        protected override void DoLoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.DoLoadFromStream(aStream, aStyle, aFileVersion);

            if (aFileVersion > 22)
            {
                ushort vAnnCount = 0;
                byte[] vBuffer = BitConverter.GetBytes(vAnnCount);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                if (vAnnCount > 0)
                {
                    for (int i = 0; i <= vAnnCount - 1; i++)
                    {
                        HCDataAnnotate vAnn = new HCDataAnnotate();
                        vAnn.LoadFromStream(aStream, aFileVersion);
                        FDataAnnotates.Add(vAnn);
                    }
                }
            }
        }

        protected override void DoItemAction(int aItemNo, int aOffset, HCItemAction aAction)
        {
            switch (aAction)
            {
                case HCItemAction.hiaRemove:
                    _AnnotateRemove(aItemNo, aOffset);
                    break;

                case HCItemAction.hiaInsertChar:
                    _AnnotateInsertChar(aItemNo, aOffset);
                    break;

                case HCItemAction.hiaBackDeleteChar:
                    _AnnotateBackChar(aItemNo, aOffset);
                    break;

                case HCItemAction.hiaDeleteChar:
                    _AnnotateDeleteChar(aItemNo, aOffset);
                    break;
            }
        }

        protected override void DoDrawItemPaintContent(HCCustomData aData, int aDrawItemNo,
            RECT aDrawRect, RECT aClearRect, string aDrawText,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
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
                aDataDrawLeft, aDataDrawRight, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
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
        }

        ~HCAnnotateData()
        {
            FDataAnnotates.Clear();
            FDrawItemAnnotates.Clear();
        }

        public override void GetCaretInfo(int aItemNo, int aOffset, ref HCCaretInfo aCaretInfo)
        {
            base.GetCaretInfo(aItemNo, aOffset, ref aCaretInfo);

            int vCaretDrawItemNo = -1;
            if (this.CaretDrawItemNo < 0)
            {
                if (Style.UpdateInfo.Draging)
                    vCaretDrawItemNo = GetDrawItemNoByOffset(this.MouseMoveItemNo, this.MouseMoveItemOffset);
                else
                    vCaretDrawItemNo = GetDrawItemNoByOffset(SelectInfo.StartItemNo, SelectInfo.StartItemOffset);
            }
            else
                vCaretDrawItemNo = CaretDrawItemNo;

            HCDataAnnotate vDataAnnotate = null;
            if (Style.UpdateInfo.Draging)
            {
                vDataAnnotate = GetDrawItemFirstDataAnnotateAt(vCaretDrawItemNo,
                    GetDrawItemOffsetWidth(vCaretDrawItemNo,
                      this.MouseMoveItemOffset - DrawItems[vCaretDrawItemNo].CharOffs + 1),
                    DrawItems[vCaretDrawItemNo].Rect.Top + 1);
            }
            else
            {
                vDataAnnotate = GetDrawItemFirstDataAnnotateAt(vCaretDrawItemNo,
                    GetDrawItemOffsetWidth(vCaretDrawItemNo,
                      SelectInfo.StartItemOffset - DrawItems[vCaretDrawItemNo].CharOffs + 1),
                    DrawItems[vCaretDrawItemNo].Rect.Top + 1);
            }

            if (FActiveAnnotate != vDataAnnotate)
            {
                FActiveAnnotate = vDataAnnotate;
                Style.UpdateInfoRePaint();
            }
        }

        public override void PaintData(int aDataDrawLeft, int aDataDrawTop, int aDataDrawRight, int aDataDrawBottom,
            int aDataScreenTop, int aDataScreenBottom, int aVOffset, int aFirstDItemNo, int aLastDItemNo,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            CheckAnnotateRange(aFirstDItemNo, aLastDItemNo);  // 指定DrawItem范围内的批注获取各自的DrawItem范围
            base.PaintData(aDataDrawLeft, aDataDrawTop, aDataDrawRight, aDataDrawBottom, aDataScreenTop,
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

        public override void SaveToStream(Stream aStream, int aStartItemNo, int aStartOffset, int aEndItemNo, int aEndOffset)
        {
 	        base.SaveToStream(aStream, aStartItemNo, aStartOffset, aEndItemNo, aEndOffset);
            ushort vAnnCount = (ushort)FDataAnnotates.Count;
            byte[] vBuffer = BitConverter.GetBytes(vAnnCount);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            for (int i = 0; i <= vAnnCount - 1; i++)
                FDataAnnotates[i].SaveToStream(aStream);
        }

        public bool InsertAnnotate(string aTitle, string aText)
        {
            if (!CanEdit())
                return false;

            if (!this.SelectExists())
                return false;

            HCCustomData vTopData = GetTopLevelData();
            if ((vTopData is HCAnnotateData) && (vTopData != this))
                (vTopData as HCAnnotateData).InsertAnnotate(aTitle, aText);
            else
                FDataAnnotates.NewDataAnnotate(SelectInfo, aTitle, aText);

            return true;
        }

        public HCDataAnnotates DataAnnotates
        {
            get { return FDataAnnotates; }
        }

        public HCDataAnnotate HotAnnotate
        {
            get { return FHotAnnotate; }
        }

        public HCDataAnnotate ActiveAnnotate
        {
            get { return FActiveAnnotate; }
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
    }
}
