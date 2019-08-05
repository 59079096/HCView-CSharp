/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                文档对象基类实现单元                   }
{                                                       }
{*******************************************************/

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.IO;
using HC.Win32;
using System.Xml;
using System.ComponentModel;

namespace HC.View
{
    public struct ScaleInfo
    {
        public int MapMode;
        public POINT WindowOrg;
        public SIZE WindowExt;
        public POINT ViewportOrg;
        public SIZE ViewportExt;
    }

    public enum HCViewModel : byte
    {
        [Description("胶卷视图，显示页眉、页脚")]
        hvmFilm,
        [Description("页面视图，不显示页眉、页脚")]
        hvmPage
    }

    public enum ItemOption : byte 
    {
        ioParaFirst = 1, ioSelectPart = 1 << 1, ioSelectComplate = 1 << 2, ioPageBreak = 1 << 3 
    }

    public enum HCItemAction : byte
    {
        hiaRemove, hiaInsertChar, hiaBackDeleteChar, hiaDeleteChar
    }

    public class PaintInfo : HCObject
    {
        private bool FPrint;
        private HCViewModel FViewModel;
        List<HCCustomItem> FTopItems;
        int FWindowWidth, FWindowHeight;
        Single
            FScaleX, FScaleY,  // 目标画布和显示器画布dpi比例(打印机dpi和显示器dpi不一致时的缩放比例)
            FZoom;  // 视图设置的放大比例

        public PaintInfo()
        {
            FTopItems = new List<HCCustomItem>();  // 只管理不负责释放
            FScaleX = 1;
            FScaleY = 1;
            FZoom = 1;
            FViewModel = HCViewModel.hvmFilm;
        }

        ~PaintInfo()
        {
           
        }

        public ScaleInfo ScaleCanvas(HCCanvas aCanvas)
        {
            ScaleInfo Result = new ScaleInfo();
            Result.MapMode = GDI.GetMapMode(aCanvas.Handle);  // 返回映射方式，零则失败
            GDI.SetMapMode(aCanvas.Handle, GDI.MM_ANISOTROPIC);  // 逻辑单位转换成具有任意比例轴的任意单位，用SetWindowsEx和SetViewportExtEx函数指定单位、方向和需要的比例
            GDI.SetWindowOrgEx(aCanvas.Handle, 0, 0, ref Result.WindowOrg);  // 用指定的坐标设置设备环境的窗口原点
            GDI.SetWindowExtEx(aCanvas.Handle, FWindowWidth, FWindowHeight, ref Result.WindowExt);  // 为设备环境设置窗口的水平的和垂直的范围

            GDI.SetViewportOrgEx(aCanvas.Handle, 0, 0, ref Result.ViewportOrg);  // 哪个设备点映射到窗口原点(0,0)
            // 用指定的值来设置指定设备环境坐标的X轴、Y轴范围
            GDI.SetViewportExtEx(aCanvas.Handle, (int)Math.Round(FWindowWidth * FScaleX),
                (int)Math.Round(FWindowHeight * FScaleY), ref Result.ViewportExt);

            return Result;
        }

        public void RestoreCanvasScale(HCCanvas aCanvas, ScaleInfo aOldInfo)
        {
            POINT pt = new POINT();
            SIZE size = new SIZE();
            GDI.SetViewportOrgEx(aCanvas.Handle, aOldInfo.ViewportOrg.X, aOldInfo.ViewportOrg.Y, ref pt);
            GDI.SetViewportExtEx(aCanvas.Handle, aOldInfo.ViewportExt.cx, aOldInfo.ViewportExt.cy, ref size);
            GDI.SetWindowOrgEx(aCanvas.Handle, aOldInfo.WindowOrg.X, aOldInfo.WindowOrg.Y, ref pt);
            GDI.SetWindowExtEx(aCanvas.Handle, aOldInfo.WindowExt.cx, aOldInfo.WindowExt.cy, ref size);
            GDI.SetMapMode(aCanvas.Handle, aOldInfo.MapMode);
        }

        public int GetScaleX(int aValue)
        {
            return (int)Math.Round(aValue * FScaleX);
        }

        public int GetScaleY(int aValue)
        {
            return (int)Math.Round(aValue * FScaleY);
        }

        public void DrawNoScaleLine(HCCanvas aCanvas, Point[] aPoints)
        {
            SIZE size = new SIZE();
            GDI.SetViewportExtEx(aCanvas.Handle, FWindowWidth, FWindowHeight, ref size);
            try
            {
                aCanvas.MoveTo(GetScaleX(aPoints[0].X), GetScaleY(aPoints[0].Y));
                for (int i = 1; i < aPoints.Length; i++)
                {
                    aCanvas.LineTo(GetScaleX(aPoints[i].X), GetScaleY(aPoints[i].Y));
                }
            }
            finally
            {
                GDI.SetViewportExtEx(aCanvas.Handle, (int)Math.Round(FWindowWidth * FScaleX),
                    (int)Math.Round(FWindowHeight * FScaleY), ref size);
            }
        }

        public bool Print
        {
            get { return FPrint; }
            set { FPrint = value; }
        }

        public HCViewModel ViewModel
        {
            get { return FViewModel; }
            set { FViewModel = value; }
        }

        /// <summary> 只管理不负责释放 </summary>
        public List<HCCustomItem> TopItems
        {
            get { return FTopItems; }
        }

        /// <summary> 用于绘制的区域高度 </summary>
        public int WindowWidth
        {
            get { return FWindowWidth; }
            set { FWindowWidth = value; }
        }

        /// <summary> 用于绘制的区域宽度 </summary>
        public int WindowHeight
        {
            get { return FWindowHeight; }
            set { FWindowHeight = value; }
        }

        /// <summary> 横向缩放 </summary>
        public Single ScaleX
        {
            get { return FScaleX; }
            set { FScaleX = value; }
        }

        /// <summary> 纵向缩放 </summary>
        public Single ScaleY
        {
            get { return FScaleY; }
            set { FScaleY = value; }
        }

        public Single Zoom
        {
            get { return FZoom; }
            set { FZoom = value; }
        }
    }

    public class HCCustomItem : HCObject
    {
        private int FParaNo, FStyleNo, FFirstDItemNo;
        private bool FActive, FVisible;
        private HCSet FOptions;

        protected bool GetParaFirst()
        {
            return FOptions.Contains((byte)ItemOption.ioParaFirst);
        }

        protected void SetParaFirst(bool Value)
        {
            if (Value)
                FOptions.InClude((byte)ItemOption.ioParaFirst);
            else
                FOptions.ExClude((byte)ItemOption.ioParaFirst);
        }

        protected bool GetPageBreak()
        {
            return FOptions.Contains((byte)ItemOption.ioPageBreak);
        }

        protected void SetPageBreak(bool Value)
        {
            if (Value)
                FOptions.InClude((byte)ItemOption.ioPageBreak);
            else
                FOptions.ExClude((byte)ItemOption.ioPageBreak);
        }

        protected virtual bool GetSelectComplate()
        {
            return FOptions.Contains((byte)ItemOption.ioSelectComplate);
        }

        protected bool GetSelectPart()
        {
            return FOptions.Contains((byte)ItemOption.ioSelectPart);
        }

        protected virtual string GetText()
        {
            return "";
        }

        protected virtual void SetText(string Value) { }

        protected virtual string GetHyperLink()
        {
            return "";
        }

        protected virtual void SetHyperLink(string value) { }

        protected virtual void SetActive(bool Value)
        {
            FActive = Value;
        }

        public virtual int GetLength()
        {
            return 0;
        }

        protected virtual void DoPaint(HCStyle aStyle, RECT aDrawRect,
            int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas aCanvas, PaintInfo aPaintInfo) { }

        public HCCustomItem()
        {
            FStyleNo = HCStyle.Null;
            FParaNo = HCStyle.Null;
            FOptions = new HCSet();
            FFirstDItemNo = -1;
            FVisible = true;
            FActive = false;
        }

        public virtual void Assign(HCCustomItem source)
        {
            this.FStyleNo = source.StyleNo;
            this.FParaNo = source.ParaNo;
            //this.FOptions = new HashSet<ItemOption>(source.Options);
            this.FOptions.Value = source.Options.Value;
        }

        /// <summary>
        /// 绘制Item的事件
        /// </summary>
        /// <param name="ACanvas"></param>
        /// <param name="aDrawRect">当前DrawItem的区域</param>
        /// <param name="ADataDrawBottom">Item所在的Data本次绘制底部位置</param>
        /// <param name="ADataScreenTop"></param>
        /// <param name="ADataScreenBottom"></param>
        public void PaintTo(HCStyle aStyle, RECT aDrawRect,
            int APageDataDrawTop, int APageDataDrawBottom, int APageDataScreenTop, int APageDataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)  // 不可继承
        {
            int vDCState = ACanvas.Save();
            try
            {
                DoPaint(aStyle, aDrawRect, APageDataDrawTop, APageDataDrawBottom,
                    APageDataScreenTop, APageDataScreenBottom, ACanvas, APaintInfo);
            }
            finally
            {
                ACanvas.Restore(vDCState);
                ACanvas.Refresh();  // 处理下一个使用Pen时修改Pen的属性值和当前属性值一样时，不会触发Canvas重新SelectPen导致Pen的绘制失效的问题
            }
        }

        public virtual void PaintTop(HCCanvas aCanvas) { }

        /// <summary>
        /// 将2个Item合并为同一个
        /// </summary>
        /// <param name="AItemA">ItemA</param>
        /// <param name="AItemB">ItemB</param>
        /// <returns>True合并成功，否则返回False</returns>
        public virtual bool CanConcatItems(HCCustomItem aItem)
        {
            return ((this.GetType() == aItem.GetType()) && (this.FStyleNo == aItem.StyleNo));
        }

        public virtual void DisSelect()
        {
            FOptions.ExClude((byte)ItemOption.ioSelectPart);
            FOptions.ExClude((byte)ItemOption.ioSelectComplate);
        }

        public virtual bool CanDrag()
        {
            return true;
        }

        public virtual void KillFocus() { }

        public virtual void DblClick(int X, int Y) { }

        public virtual void MouseDown(MouseEventArgs e)
        {
            FActive = true;
        }

        public virtual void MouseMove(MouseEventArgs e) { }

        public virtual void MouseUp(MouseEventArgs e) { }

        public virtual void MouseEnter() { }

        public virtual void MouseLeave() { }

        public virtual string GetHint()
        {
            return "";
        }

        public virtual void SelectComplate()
        {
            FOptions.ExClude((byte)ItemOption.ioSelectPart);
            FOptions.InClude((byte)ItemOption.ioSelectComplate);
        }

        public void SelectPart()
        {
            FOptions.ExClude((byte)ItemOption.ioSelectComplate);
            FOptions.InClude((byte)ItemOption.ioSelectPart);
        }

        public virtual bool CanAccept(int aOffset, HCItemAction aAction)
        {
            return true;
        }

        /// <summary> 从指定位置将当前item分成前后两部分 </summary>
        /// <param name="aOffset">分裂位置</param>
        /// <returns>后半部分对应的Item</returns>
        public virtual HCCustomItem BreakByOffset(int aOffset)
        {
            HCCustomItem Result = Activator.CreateInstance(this.GetType()) as HCCustomItem;
            Result.Assign(this);
            Result.ParaFirst = false;  // 打断后，后面的肯定不是断首

            return Result;
        }

        public void SaveToStream(Stream aStream)
        {
            SaveToStream(aStream, 0, this.Length);
        }

        public virtual void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            byte[] buffer = System.BitConverter.GetBytes(FStyleNo);
            aStream.Write(buffer, 0, buffer.Length);

            buffer = System.BitConverter.GetBytes(FParaNo);
            aStream.Write(buffer, 0, buffer.Length);

            FOptions.ExClude((byte)ItemOption.ioSelectPart);
            FOptions.ExClude((byte)ItemOption.ioSelectComplate);
            aStream.WriteByte(FOptions.Value);
        }

        public virtual void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            byte[] vBuffer = BitConverter.GetBytes(FParaNo);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FParaNo = System.BitConverter.ToInt32(vBuffer, 0);

            if (aFileVersion > 25)
                FOptions.Value = (byte)aStream.ReadByte();
            else
            {
                vBuffer = BitConverter.GetBytes(ParaFirst);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                ParaFirst = BitConverter.ToBoolean(vBuffer, 0);
            }
        }

        public virtual string ToHtml(string aPath)
        {
            return "";
        }

        public virtual void ToXml(XmlElement aNode)
        {
            aNode.Attributes["sno"].Value = FStyleNo.ToString();
            aNode.Attributes["pno"].Value = FParaNo.ToString();
            aNode.Attributes["parafirst"].Value = this.ParaFirst.ToString();
            aNode.Attributes["pagebreak"].Value = this.PageBreak.ToString();
        }

        public virtual void ParseXml(XmlElement aNode)
        {
            FStyleNo = int.Parse(aNode.Attributes["sno"].Value);
            FParaNo = int.Parse(aNode.Attributes["pno"].Value);
            this.ParaFirst = bool.Parse(aNode.Attributes["parafirst"].Value);
            this.PageBreak = bool.Parse(aNode.Attributes["pagebreak"].Value);
        }

        // 撤销重做相关方法
        public virtual void Undo(HCCustomUndoAction aUndoAction) { }

        public virtual void Redo(HCCustomUndoAction aRedoAction) { }

        public HCSet Options
        {
            get { return FOptions; }
        }

        public string Text
        {
            get { return GetText(); }
            set { SetText(value); }
        }

        public int Length
        {
            get { return GetLength(); }
        }

        public bool ParaFirst
        {
            get { return GetParaFirst(); }
            set { SetParaFirst(value); }
        }

        public bool PageBreak
        {
            get { return GetPageBreak(); }
            set { SetPageBreak(value); }
        }

        public string HyperLink
        {
            get { return GetHyperLink(); }
            set { SetHyperLink(value); }
        }

        public bool IsSelectComplate
        {
            get { return GetSelectComplate(); }
        }

        public bool IsSelectPart
        {
            get { return GetSelectPart(); }
        }

        public int StyleNo
        {
            get { return FStyleNo; }
            set { FStyleNo = value; }
        }

        public int ParaNo
        {
            get { return FParaNo; }
            set { FParaNo = value; }
        }

        public int FirstDItemNo
        {
            get { return FFirstDItemNo; }
            set { FFirstDItemNo = value; }
        }

        public bool Active
        {
            get { return FActive; }
            set { SetActive(value); }
        }

        public bool Visible
        {
            get { return FVisible; }
            set { FVisible = value; }
        }
    }

    public delegate void ItemNotifyEventHandler(HCCustomItem aItem);

    public class HCItems : HCList<HCCustomItem>
    {
        private ItemNotifyEventHandler FOnInsertItem, FOnRemoveItem;

        private void HCItems_OnInsert(object sender, NListEventArgs<HCCustomItem> e)
        {
            if (FOnInsertItem != null)
                FOnInsertItem(e.Item);
        }

        private void HCItems_OnRemove(object sender, NListEventArgs<HCCustomItem> e)
        {
            if (FOnRemoveItem != null)
                FOnRemoveItem(e.Item);
        }
        
        public HCItems()
        {
            this.OnInsert += new EventHandler<NListEventArgs<HCCustomItem>>(HCItems_OnInsert);
            this.OnDelete += new EventHandler<NListEventArgs<HCCustomItem>>(HCItems_OnRemove);
        }

        public ItemNotifyEventHandler OnInsertItem
        {
            get { return FOnInsertItem; }
            set { FOnInsertItem = value; }
        }

        public ItemNotifyEventHandler OnRemoveItem
        {
            get { return FOnRemoveItem; }
            set { FOnRemoveItem = value; }
        }
    }
}
