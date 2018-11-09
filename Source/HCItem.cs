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

    public enum ItemOption : byte 
    { 
        ioParaFirst, ioSelectPart, ioSelectComplate 
    }

    public class PaintInfo : HCObject
    {
        private bool FPrint;
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
        }

        ~PaintInfo()
        {
           
        }

        public ScaleInfo ScaleCanvas(HCCanvas ACanvas)
        {
            ScaleInfo Result = new ScaleInfo();
            Result.MapMode = GDI.GetMapMode(ACanvas.Handle);  // 返回映射方式，零则失败
            GDI.SetMapMode(ACanvas.Handle, GDI.MM_ANISOTROPIC);  // 逻辑单位转换成具有任意比例轴的任意单位，用SetWindowsEx和SetViewportExtEx函数指定单位、方向和需要的比例
            GDI.SetWindowOrgEx(ACanvas.Handle, 0, 0, ref Result.WindowOrg);  // 用指定的坐标设置设备环境的窗口原点
            GDI.SetWindowExtEx(ACanvas.Handle, FWindowWidth, FWindowHeight, ref Result.WindowExt);  // 为设备环境设置窗口的水平的和垂直的范围

            GDI.SetViewportOrgEx(ACanvas.Handle, 0, 0, ref Result.ViewportOrg);  // 哪个设备点映射到窗口原点(0,0)
            // 用指定的值来设置指定设备环境坐标的X轴、Y轴范围
            GDI.SetViewportExtEx(ACanvas.Handle, (int)Math.Round(FWindowWidth * FScaleX),
                (int)Math.Round(FWindowHeight * FScaleY), ref Result.ViewportExt);

            return Result;
        }

        public void RestoreCanvasScale(HCCanvas ACanvas, ScaleInfo AOldInfo)
        {
            POINT pt = new POINT();
            SIZE size = new SIZE();
            GDI.SetViewportOrgEx(ACanvas.Handle, AOldInfo.ViewportOrg.X, AOldInfo.ViewportOrg.Y, ref pt);
            GDI.SetViewportExtEx(ACanvas.Handle, AOldInfo.ViewportExt.cx, AOldInfo.ViewportExt.cy, ref size);
            GDI.SetWindowOrgEx(ACanvas.Handle, AOldInfo.WindowOrg.X, AOldInfo.WindowOrg.Y, ref pt);
            GDI.SetWindowExtEx(ACanvas.Handle, AOldInfo.WindowExt.cx, AOldInfo.WindowExt.cy, ref size);
            GDI.SetMapMode(ACanvas.Handle, AOldInfo.MapMode);
        }

        public int GetScaleX(int AValue)
        {
            return (int)Math.Round(AValue * FScaleX);
        }

        public int GetScaleY(int AValue)
        {
            return (int)Math.Round(AValue * FScaleY);
        }

        public void DrawNoScaleLine(HCCanvas ACanvas, Point[] APoints)
        {
            SIZE size = new SIZE();
            GDI.SetViewportExtEx(ACanvas.Handle, FWindowWidth, FWindowHeight, ref size);
            try
            {
                ACanvas.DrawLines(APoints);
            }
            finally
            {
                GDI.SetViewportExtEx(ACanvas.Handle, (int)Math.Round(FWindowWidth * FScaleX),
                    (int)Math.Round(FWindowHeight * FScaleY), ref size);
            }
        }

        public bool Print
        {
            get { return FPrint; }
            set { FPrint = value; }
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
        private HashSet<ItemOption> FOptions;

        protected bool GetParaFirst()
        {
            return FOptions.Contains(ItemOption.ioParaFirst);
        }

        protected void SetParaFirst(bool Value)
        {
            if (Value)
                FOptions.Add(ItemOption.ioParaFirst);
            else
                FOptions.Remove(ItemOption.ioParaFirst);
        }

        protected virtual bool GetSelectComplate()
        {
            return FOptions.Contains(ItemOption.ioSelectComplate);
        }

        protected bool GetSelectPart()
        {
            return FOptions.Contains(ItemOption.ioSelectPart);
        }

        protected virtual string GetText()
        {
            return "";
        }

        protected virtual void SetText(string Value) { }

        protected virtual void SetActive(bool Value)
        {
            FActive = Value;
        }

        public virtual int GetLength()
        {
            return 0;
        }

        protected virtual void DoPaint(HCStyle AStyle, RECT ADrawRect,
            int ADataDrawTop, int ADataDrawBottom, int ADataScreenTop, int ADataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo) { }

        public HCCustomItem()
        {
            FStyleNo = HCStyle.Null;
            FParaNo = HCStyle.Null;
            FOptions = new HashSet<ItemOption>();
            FFirstDItemNo = -1;
            FVisible = true;
            FActive = false;
        }

        public virtual void Assign(HCCustomItem Source)
        {
            this.FStyleNo = Source.StyleNo;
            this.FParaNo = Source.ParaNo;
            this.FOptions = Source.Options;
        }

        /// <summary>
        /// 绘制Item的事件
        /// </summary>
        /// <param name="ACanvas"></param>
        /// <param name="ADrawRect">当前DrawItem的区域</param>
        /// <param name="ADataDrawBottom">Item所在的Data本次绘制底部位置</param>
        /// <param name="ADataScreenTop"></param>
        /// <param name="ADataScreenBottom"></param>
        public void PaintTo(HCStyle AStyle, RECT ADrawRect,
            int APageDataDrawTop, int APageDataDrawBottom, int APageDataScreenTop, int APageDataScreenBottom,
            HCCanvas ACanvas, PaintInfo APaintInfo)  // 不可继承
        {
            int vDCState = ACanvas.Save();
            try
            {
                DoPaint(AStyle, ADrawRect, APageDataDrawTop, APageDataDrawBottom,
                    APageDataScreenTop, APageDataScreenBottom, ACanvas, APaintInfo);
            }
            finally
            {
                ACanvas.Restore(vDCState);
            }
        }

        public virtual void PaintTop(HCCanvas ACanvas) { }

        /// <summary>
        /// 将2个Item合并为同一个
        /// </summary>
        /// <param name="AItemA">ItemA</param>
        /// <param name="AItemB">ItemB</param>
        /// <returns>True合并成功，否则返回False</returns>
        public virtual bool CanConcatItems(HCCustomItem AItem)
        {
            return ((this.GetType() == AItem.GetType()) && (this.FStyleNo == AItem.StyleNo));
        }

        public virtual void DisSelect()
        {
            FOptions.Remove(ItemOption.ioSelectPart);
            FOptions.Remove(ItemOption.ioSelectComplate);
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
            FOptions.Remove(ItemOption.ioSelectPart);
            FOptions.Add(ItemOption.ioSelectComplate);
        }

        public void SelectPart()
        {
            FOptions.Remove(ItemOption.ioSelectComplate);
            FOptions.Add(ItemOption.ioSelectPart);
        }

        /// <summary> 从指定位置将当前item分成前后两部分 </summary>
        /// <param name="AOffset">分裂位置</param>
        /// <returns>后半部分对应的Item</returns>
        public virtual HCCustomItem BreakByOffset(int AOffset)
        {
            HCCustomItem Result = Activator.CreateInstance(this.GetType()) as HCCustomItem;
            Result.Assign(this);
            Result.ParaFirst = false;  // 打断后，后面的肯定不是断首

            return Result;
        }

        public void SaveToStream(Stream AStream)
        {
            SaveToStream(AStream, 0, this.Length);
        }

        public virtual void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            byte[] buffer = System.BitConverter.GetBytes(FStyleNo);
            AStream.Write(buffer, 0, buffer.Length);

            buffer = System.BitConverter.GetBytes(FParaNo);
            AStream.Write(buffer, 0, buffer.Length);

            bool vParFirst = ParaFirst;
            buffer = System.BitConverter.GetBytes(vParFirst);
            AStream.Write(buffer, 0, buffer.Length);
        }

        public virtual void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            byte[] vBuffer = BitConverter.GetBytes(FParaNo);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FParaNo = System.BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(ParaFirst);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            ParaFirst = BitConverter.ToBoolean(vBuffer, 0);
        }

        // 撤销重做相关方法
        public void Undo(object AObject)
        {
            if (AObject is HCUndoList)
                (AObject as HCUndoList).Undo();
        }

        public void Redo(object AObject)
        {
            if (AObject is HCUndoList)
                (AObject as HCUndoList).Redo();
        }

        public HashSet<ItemOption> Options
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

    public delegate void ItemNotifyEventHandler(HCCustomItem AItem);

    public class HCItems : HCList<HCCustomItem>
    {
        private ItemNotifyEventHandler FOnItemInsert;

        private void HCItems_OnInsert(object sender, NListEventArgs<HCCustomItem> e)
        {
            if (FOnItemInsert != null)
                FOnItemInsert(e.Item);
        }
        
        public HCItems()
        {
            this.OnInsert += new EventHandler<NListEventArgs<HCCustomItem>>(HCItems_OnInsert);
        }

        public ItemNotifyEventHandler OnItemInsert
        {
            get { return FOnItemInsert; }
            set { FOnItemInsert = value; }
        }
    }
}
