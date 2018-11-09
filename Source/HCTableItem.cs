/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                     表格实现单元                      }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.View;
using System.Drawing;
using HC.Win32;
using System.Windows.Forms;
using System.IO;

namespace HC.View
{
    public class SelectCellRang : HCObject
    {
        private int  
            FStartRow,  // 选中起始行
            FStartCol,  // 选中起始列
            FEndRow,    // 选中结束行
            FEndCol;     // 选中结束列

        public SelectCellRang()
        {
            Initialize();
        }

        /// <summary> 初始化字段和变量 </summary>
        public void Initialize()
        {
            FStartRow = -1;
            FStartCol = -1;
            FEndRow = -1;
            FEndCol = -1;
        }

        /// <summary> 在同一单元中编辑 </summary>
        public bool EditCell()
        {
            return ((FStartRow >= 0) && (FEndRow < 0));
        }

        /// <summary> 选中在同一行 </summary>
        public bool SameRow()
        {
            return ((FStartRow >= 0) && (FStartRow == FEndRow));
        }

        /// <summary> 选中在同一列 </summary>
        public bool SameCol()
        {
            return ((FStartCol >= 0) && (FStartCol == FEndCol));
        }

        /// <summary> 选中1-n个单元格 </summary>
        public bool SelectExists()
        {
            return ((FStartRow >= 0) && (FEndRow >= 0));
        }

        public int StartRow
        {
            get { return FStartRow; }
            set { FStartRow = value; }
        }

        public int StartCol
        {
            get { return FStartCol; }
            set { FStartCol = value; }
        }

        public int EndRow
        {
            get { return FEndRow; }
            set { FEndRow = value; }
        }

        public int EndCol
        {
            get { return FEndCol; }
            set { FEndCol = value; }
        }

    }
    
    public enum TableSite : byte
    {
        tsOutside,  // 表格外面
        tsCell,  // 单元格中
        tsBorderLeft,  // 只有第一列使用此元素
        tsBorderTop,    // 只有第一行使用此元素
        tsBorderRight,  // 第X列右边
        tsBorderBottom  // 第X行下边
    }

    public struct ResizeInfo  // 缩放信息
    {
        public TableSite TableSite;
        public int DestX, DestY;
    }

    public class PageBreak  // 分页信息
    {
        public int
            /// <summary> 在此页结尾分页 </summary>
            PageIndex,
            Row,  // 分页行
            BreakSeat,  // 分页时，此行各列分页位置最大的
            BreakBottom;  // 页底部位置
    }

    /// <summary> 行跨页信息 </summary>
    class ColCross : Object
    {
        public int Col;  // 单元格所在的列
        public int DrawItemNo;  // 跨页的DrawItem
        public int VOffset;  // 跨页偏移

        public ColCross()
        {
            Col = -1;
            DrawItemNo = -1;
            VOffset = 0;
        }
    }

    public delegate void RowAddEventHandler(HCTableRow ARow);

    public class HCTableRows : HCList<HCTableRow>
    {
        private RowAddEventHandler FOnRowAdd;

        private void OnInsertRow(object sender, NListEventArgs<HCTableRow> e)
        {
            if (FOnRowAdd != null)
                FOnRowAdd(e.Item);
        }

        public HCTableRows()
        {
            this.OnInsert += new EventHandler<NListEventArgs<HCTableRow>>(OnInsertRow);
        }

        public RowAddEventHandler OnRowAdd
        {
            get { return FOnRowAdd; }
            set { FOnRowAdd = value; }
        }
    }

    public struct OutsideInfo  // 表格外面信息
    {
        public int Row;  // 外面位置处对应的行
        public bool Leftside;  // True：左边 False：右边
    }

    public class HCTableItem : HCResizeRectItem
    {
        private byte
            FBorderWidth,  // 边框宽度(要求不大于最小行高，否则分页计算会有问题)
            FCellHPadding,  // 单元格内容水平偏移
            FCellVPadding;   // 单元格内容垂直偏移(不能大于最低的DrawItem高度，否则会影响跨页)

        private OutsideInfo FOutsideInfo;  // 点击在表格左右边时对应的行信息

        private int
            FMouseDownX, FMouseDownY, FMouseDownRow, FMouseDownCol,
            FMouseMoveRow, FMouseMoveCol;

        private ResizeInfo FResizeInfo;

        private bool FBorderVisible, FMouseLBDowning, FSelecting, FDraging, FOutSelectInto, FEnableUndo;

        private SelectCellRang FSelectCellRang;

        private Color FBorderColor;  // 边框颜色

        private HCTableRows FRows;  // 行

        private List<int> FColWidths;  // 记录各列宽度(除边框、含FCellHPadding * 2)，方便有合并的单元格获取自己水平开始处的位置

        private List<PageBreak> FPageBreaks;  // 记录各行分页时的信息

        private void InitializeMouseInfo()
        {
            FMouseDownRow = -1;
            FMouseDownCol = -1;
            FMouseMoveRow = -1;
            FMouseMoveCol = -1;
            FMouseLBDowning = false;
        }

        private HCCustomData DoCellDataGetRootData()
        {
            return OwnerData.GetRootData();
        }

        private bool DoCellDataGetEnableUndo()
        {
            return (OwnerData.Style.EnableUndo && FEnableUndo);
        }

        /// <summary> 表格行有添加时 </summary>
        private void DoRowAdd(HCTableRow ARow)
        {
            HCTableCellData vCellData = null;

            for (int i = 0; i <= ARow.ColCount - 1; i++)
            {
                vCellData = ARow[i].CellData;
                if (vCellData != null)
                {
                    vCellData.OnInsertItem = (OwnerData as HCCustomRichData).OnInsertItem;
                    vCellData.OnItemResized = (OwnerData as HCCustomRichData).OnItemResized;
                    vCellData.OnDrawItemPaintAfter = (OwnerData as HCCustomRichData).OnDrawItemPaintAfter;
                    vCellData.OnDrawItemPaintBefor = (OwnerData as HCCustomRichData).OnDrawItemPaintBefor;
                    vCellData.OnDrawItemPaintAfter = (OwnerData as HCRichData).OnDrawItemPaintAfter;
                    vCellData.OnCreateItemByStyle = (OwnerData as HCRichData).OnCreateItemByStyle;
                    vCellData.OnCreateItem = (OwnerData as HCCustomRichData).OnCreateItem;
                    vCellData.OnGetUndoList = this.GetSelfUndoList;
                    vCellData.OnGetRootData = DoCellDataGetRootData;
                    vCellData.OnGetEnableUndo = DoCellDataGetEnableUndo;
                }
            }
        }

        private bool CellChangeByAction(int ARow, int ACol, HCProcedure AProcedure)
        {
            int vOldHeight = this[ARow, ACol].CellData.Height;
            AProcedure();
            return ((vOldHeight != this[ARow, ACol].CellData.Height) || ChangeNearPageBreak());
        }

        /// <summary> 获取当前表格格式化高度 </summary>
        /// <returns></returns>
        private int GetFormatHeight()
        {
            int Result = FBorderWidth;
            for (int i = 0; i <= RowCount - 1; i++)
                Result = Result + FRows[i].Height + FBorderWidth;

            return Result;
        }

        private void FormatRowFrom(int ARow)
        {
            int vDestRow = -1, vDestCol = -1, vExtraHeight = 0, vH = 0, vDestRow2 = -1, vDestCol2 = -1;
            // 为兼容分页时重新格式化，需要增加FmtOffset的值
            for (int vR = ARow; vR <= RowCount - 1; vR++)  // 计算有行合并情况下各行的高
            {
                for (int vC = 0; vC <= FRows[vR].ColCount - 1; vC++)
                {
                    if (this[vR, vC].CellData == null)
                    {
                        if (this[vR, vC].ColSpan < 0)
                            continue;

                        GetMergeDest(vR, vC, ref vDestRow, ref vDestCol);  // 获取到合并目标单元格所在行号
                        
                        if (vDestRow + this[vDestRow, vC].RowSpan == vR)
                        {
                            vExtraHeight = FCellVPadding + this[vDestRow, vC].CellData.Height + FCellVPadding;  // 目标单元格除上下边框后的高度
                            this[vDestRow, vC].Height = vExtraHeight;  // 目标单元格除上下边框后的高度
                            
                            for (int i = vDestRow; i <= vR - 1; i++)  // 从目标到此，经过各行后“消减”掉多
                                vExtraHeight = vExtraHeight - FRows[i].FmtOffset - FRows[i].Height - FBorderWidth;
                            
                            if (vExtraHeight > FRows[vR].FmtOffset + FRows[vR].Height)
                            {
                                vH = vExtraHeight - FRows[vR].FmtOffset - FRows[vR].Height;  // 增量
                                FRows[vR].Height = vExtraHeight - FRows[vR].FmtOffset;  // 当前行高赋值新值(内部各单元格高度会处理)
                                for (int i = 0; i <= FRows[vR].ColCount - 1; i++)  // 当前行中源列要影响目标单元
                                {
                                    if (this[vR, i].CellData == null)
                                    {
                                        GetMergeDest(vR, i, ref vDestRow2, ref vDestCol2);  // 获取目标单元格
                                        if ((vDestRow2 != vDestRow) && (vDestCol2 != vDestCol))
                                            this[vDestRow2, i].Height = this[vDestRow2, i].Height + vH;
                                    }
                                }
                            }
                            else  // 消减后剩余的没有当前行高，高度增加到当前行底部，处理非合并的单元格内容，大于合并结束到此行但数据底部没有此行高的情况
                            {
                                this[vDestRow, vC].Height =  // 2017-1-15_1.bmp中[1, 1]输入c时[1, 0]和[1, 2]的情况
                                this[vDestRow, vC].Height + FRows[vR].FmtOffset + FRows[vR].Height - vExtraHeight;
                            }
                        }
                    }
                }
            }
        }

        /// <summary> 返回指定单元格相对表格的起始位置坐标(如果被合并返回合并到单元格的坐标) </summary>
        /// <param name="ARow"></param>
        /// <param name="ACol"></param>
        /// <returns></returns>
        private POINT GetCellPostion(int ARow, int ACol)
        {
            POINT Result = new POINT(FBorderWidth, FBorderWidth);

            for (int i = 0; i <= ARow - 1; i++)
                Result.Y = Result.Y + FRows[i].FmtOffset + FRows[i].Height + FBorderWidth;
    
            Result.Y = Result.Y + FRows[ARow].FmtOffset;
    
            for (int i = 0; i <= ACol - 1; i++)
                Result.X = Result.X + FColWidths[i] + FBorderWidth;

            return Result;
        }

        private bool ActiveDataResizing()
        {
            bool Result = false;
            if (FSelectCellRang.EditCell())
                Result = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData.SelectedResizing();

            return Result;
        }

        /// <summary> 取消选中范围内除ARow, ACol之外单元格的选中状态(-1表示全部取消) </summary>
        private void DisSelectSelectedCell(int ARow = -1, int ACol = -1)
        {
            if (FSelectCellRang.StartRow >= 0)
            {
                HCTableCellData vCellData = null;
                // 先清起始，确保当前单元格可执行DisSelect 与201805172309相似
                if ((FSelectCellRang.StartRow == ARow) && (FSelectCellRang.StartCol == ACol))
                {

                }
                else
                {
                    vCellData = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData;
                    if (vCellData != null)
                    {
                        vCellData.DisSelect();
                        vCellData.InitializeField();
                    }
                }
                
                for (int vRow = FSelectCellRang.StartRow; vRow <= FSelectCellRang.EndRow; vRow++)
                {
                    for (int vCol = FSelectCellRang.StartCol; vCol <= FSelectCellRang.EndCol; vCol++)
                    {
                        if ((vRow == ARow) && (vCol == ACol))
                        {

                        }
                        else
                        {
                            vCellData = FRows[vRow][vCol].CellData;
                            if (vCellData != null)
                            {
                                vCellData.DisSelect();
                                vCellData.InitializeField();
                            }
                        }
                    }
                }
            }
        }

        private void SetBorderWidth(byte Value)
        {
            if (FBorderWidth != Value)
            {
                if (Value > FCellVPadding * 2)
                    FBorderWidth = (byte)(FCellVPadding * 2 - 1);
                else
                    FBorderWidth = Value;
            }
        }

        private void SetCellVPadding(byte Value)
        {
            if (FCellVPadding != Value)
            {
                FCellVPadding = Value;
                if (FBorderWidth > FCellVPadding * 2)
                    FBorderWidth = (byte)(FCellVPadding * 2 - 1);
            }
        }

        public override bool CanDrag()
        {
            bool Result = base.CanDrag();
            if (Result)
            {
                if (FSelectCellRang.EditCell())
                    Result = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData.SelectedCanDrag();
                else
                Result = this.IsSelectComplate || this.IsSelectPart;
            }

            return Result;
        }

        protected override bool GetSelectComplate()
        {
            return (FSelectCellRang.StartRow == 0)
                && (FSelectCellRang.StartCol == 0)
                && (FSelectCellRang.EndRow == FRows.Count - 1)
                && (FSelectCellRang.EndCol == FColWidths.Count - 1);
        }

        public override void SelectComplate()
        {
            base.SelectComplate();
            
            FSelectCellRang.StartRow = 0;
            FSelectCellRang.StartCol = 0;
            FSelectCellRang.EndRow = this.RowCount - 1;
            FSelectCellRang.EndCol = FColWidths.Count - 1;
            
            for (int vRow = FSelectCellRang.StartRow; vRow <= FSelectCellRang.EndRow; vRow++)
            {
                for (int vCol = FSelectCellRang.StartCol; vCol <= FSelectCellRang.EndCol; vCol++)
                {
                    if (FRows[vRow][vCol].CellData != null)
                        FRows[vRow][vCol].CellData.SelectAll();
                }
            }
        }

        protected override bool GetResizing()
        {
            return (base.GetResizing()) || ActiveDataResizing();
        }

        protected override void SetResizing(bool Value)
        {
            base.SetResizing(Value);
        }

#region CheckRowBorderShouLian 找当前行各列分页时的收敛位置
        void CheckRowBorderShouLian(int ARow, ref int vShouLian, int ADataDrawBottom, RECT ADrawRect)
        {
            if (vShouLian == 0)
            {
                int vRowDataDrawTop = ADrawRect.Top + FBorderWidth - 1;  // 因为边框在ADrawRect.Top也占1像素，所以要减掉
                for (int i = 0; i <= ARow - 1; i++)
                    vRowDataDrawTop = vRowDataDrawTop + FRows[i].FmtOffset + FRows[i].Height + FBorderWidth;
                
                if (FRows[ARow].FmtOffset > 0)
                {
                    vShouLian = vRowDataDrawTop - FBorderWidth + 1;  // 上一行底部边框位置
                    return;
                }

                vRowDataDrawTop = vRowDataDrawTop + FCellVPadding;  // 分页行Data绘制起始位置
                
                int vBreakBottom = 0;
                int vDestCellDataDrawTop = 0, vDestRow2 = -1, vDestCol2 = -1;
                HCTableCellData vCellData = null;

                for (int vC = 0; vC <= FRows[ARow].ColCount - 1; vC++)  // 遍历同行各列，获取截断位置(因为各行在CheckFormatPage已经算好分页位置，所以此处只要一个单元格跨页位置同时适用当前行所有单元格跨页位置
                {
                    vDestCellDataDrawTop = vRowDataDrawTop;//vCellDataDrawTop;
                    GetMergeDest(ARow, vC, ref vDestRow2, ref vDestCol2);  // 获取到目标单元格所在行号
                    if (vC != vDestCol2 + FRows[vDestRow2][vDestCol2].ColSpan)
                        continue;
                    
                    vCellData = FRows[vDestRow2][vDestCol2].CellData;
                    while (vDestRow2 < ARow)  // 目标单元格的CellDataDrawTop
                    {
                        vDestCellDataDrawTop = vDestCellDataDrawTop - FBorderWidth - FRows[vDestRow2].Height;
                        vDestRow2++;
                    }

                    RECT vRect = new RECT();
                    for (int i = 0; i <= vCellData.DrawItems.Count - 1; i++)
                    {
                        if (vCellData.DrawItems[i].LineFirst)
                        {
                            vRect = vCellData.DrawItems[i].Rect;
                            //if DrawiInLastLine(i) then  // 单元格内最后一行内容补充FCellVPadding
                            vRect.Bottom = vRect.Bottom + FCellVPadding; // 每一行可能是要截断的，截断时下面要能放下FCellVPadding
                            if (vDestCellDataDrawTop + vRect.Bottom > ADataDrawBottom)
                            {
                                if (i > 0)
                                {
                                    if (ADataDrawBottom - vDestCellDataDrawTop - vCellData.DrawItems[i - 1].Rect.Bottom > FCellVPadding)
                                        vShouLian = Math.Max(vShouLian, vDestCellDataDrawTop + vCellData.DrawItems[i - 1].Rect.Bottom + FCellVPadding);
                                    else
                                        vShouLian = Math.Max(vShouLian, vDestCellDataDrawTop + vCellData.DrawItems[i - 1].Rect.Bottom);  // 上一个最下面做为截断位置
                                }
                                else  // 第一行就在当前页放不下
                                    vShouLian = Math.Max(vShouLian, vDestCellDataDrawTop - FCellVPadding - FBorderWidth + 1);
                                
                                break;
                            }
                            else  // 没有超过当前页
                                vBreakBottom = Math.Max(vBreakBottom, vDestCellDataDrawTop + vRect.Bottom);  // 记录为可放下的最后一个下面(有的单元格在当前页能全部显示，并不跨页)
                        }
                    }
                }
                
                vShouLian = Math.Max(vShouLian, vBreakBottom);
            }
        }
#endregion

#region
        void DoDrawPageBreakMark(bool APageEnd, HCCanvas ACanvas, int vBorderRight, int vBorderBottom,
            int ADataDrawTop)
        {
            ACanvas.Pen.BeginUpdate();
            try
            {
                ACanvas.Pen.Color = Color.Gray;
                ACanvas.Pen.Style = HCPenStyle.psDot;
                ACanvas.Pen.Width = 1;
            }
            finally
            {
                ACanvas.Pen.EndUpdate();
            }

            if (APageEnd)
            {
                ACanvas.MoveTo(vBorderRight + 5, vBorderBottom - 1);  // vBorderBottom
                ACanvas.LineTo(vBorderRight + 20, vBorderBottom - 1);
                ACanvas.Pen.Style = HCPenStyle.psSolid;
                ACanvas.MoveTo(vBorderRight + 19, vBorderBottom - 3);
                ACanvas.LineTo(vBorderRight + 19, vBorderBottom - 10);
                ACanvas.LineTo(vBorderRight + 5, vBorderBottom - 10);
                ACanvas.LineTo(vBorderRight + 5, vBorderBottom - 2);
            }
            else  // 分页符(页起始位置)
            {
                ACanvas.MoveTo(vBorderRight + 5, ADataDrawTop + 1);  // vBorderTop
                ACanvas.LineTo(vBorderRight + 20, ADataDrawTop + 1);
                ACanvas.Pen.Style = HCPenStyle.psSolid;
                ACanvas.MoveTo(vBorderRight + 19, ADataDrawTop + 3);
                ACanvas.LineTo(vBorderRight + 19, ADataDrawTop + 10);
                ACanvas.LineTo(vBorderRight + 5, ADataDrawTop + 10);
                ACanvas.LineTo(vBorderRight + 5, ADataDrawTop + 2);
            }
            
            ACanvas.Pen.Color = Color.Black;
        }
#endregion


        /// <summary> 在指定的位置绘制表格 </summary>
        /// <param name="AStyle"></param>
        /// <param name="ADrawRect">绘制时的Rect(相对ADataScreenTop)</param>
        /// <param name="ADataDrawTop">Table所属的Data绘制起始位置(相对ADataScreenTop，可为负数)</param>
        /// <param name="ADataDrawBottom">Table所属的Data绘制起始位置(相对ADataScreenTop，可超过ADataScreenBottom)</param>
        /// <param name="ADataScreenTop">当前页屏显起始位置(相对于点0, 0，>=0)</param>
        /// <param name="ADataScreenBottom">当前页屏幕底部位置(相对于点0, 0，<=窗口高度)</param>
        /// <param name="ACanvas"></param>
        /// <param name="APaintInfo"></param>
        protected override void DoPaint(HCStyle AStyle, RECT ADrawRect, 
            int ADataDrawTop, int ADataDrawBottom, int ADataScreenTop, int ADataScreenBottom, 
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            ACanvas.Pen.Width = FBorderWidth;
            // 单元格
            int vFirstDrawRow = -1, vCellDataDrawBottom = 0, vCellDrawLeft = 0, vShouLian = 0,
                vDestCellDataDrawTop = 0, vDestRow = 0, vDestCol = 0, vDestRow2 = 0, vBorderBottom = 0;
            bool vDrawCellData = false;

            int vCellDataDrawTop = ADrawRect.Top + FBorderWidth - 1;  // 第1行数据绘制起始位置，因为边框在ADrawRect.Top也占1像素，所以要减掉
            for (int vR = 0; vR <= FRows.Count - 1; vR++)
            {
                // 不在当前屏幕范围内的不绘制(1)
                vCellDataDrawTop = vCellDataDrawTop + FRows[vR].FmtOffset + FCellVPadding;
                if (vCellDataDrawTop > ADataScreenBottom)
                    break;
                
                vCellDataDrawBottom = vCellDataDrawTop + FRows[vR].Height - FCellVPadding - FCellVPadding;
                if (vCellDataDrawBottom < ADataScreenTop)
                {
                    vCellDataDrawTop = vCellDataDrawBottom + FCellVPadding + FBorderWidth;  // 准备判断下一行是否是可显示第一行
                    continue;
                }
                if (vFirstDrawRow < 0)
                    vFirstDrawRow = vR;
                
                vCellDrawLeft = ADrawRect.Left + FBorderWidth - 1;
                // 循环绘制行中各单元格数据和边框
                vShouLian = 0;
                for (int vC = 0; vC <= FRows[vR].ColCount - 1; vC++)
                {
                    if (FRows[vR][vC].ColSpan < 0)
                    {
                        vCellDrawLeft = vCellDrawLeft + FColWidths[vC] + FBorderWidth;
                        continue;  // 普通单元格或合并目标单元格才有数据，否则由目标单元格处理
                    }
                    
                    vDrawCellData = true;  // 处理目标行有跨页，且目标行后面有多行合并到此行时，只在跨页后绘制一次目标行的数据
                    if (FRows[vR][vC].RowSpan < 0)
                    {
                        if (vR != vFirstDrawRow)
                            vDrawCellData = false;  // 目标单元格已经这页绘制了数据，不用重复绘制了，否则跨行后的第一次要绘制
                    }

                    //vFristDItemNo := -1;
                    //vLastDItemNo := -1;
                    vDestCellDataDrawTop = vCellDataDrawTop;
                    GetMergeDest(vR, vC, ref vDestRow, ref vDestCol);  // 获取到目标单元格所在行号
                    vDestRow2 = vDestRow;
                    while (vDestRow2 < vR)  // 得到目标单元格CellDataDrawTop的值
                    {
                        vDestCellDataDrawTop = vDestCellDataDrawTop - FRows[vDestRow2].Height - FRows[vDestRow2].FmtOffset - FBorderWidth;
                        vDestRow2++;
                    }
                    if (vDestRow != vR)
                        vDestCellDataDrawTop -= FRows[vR].FmtOffset;
                    
#region 绘制单元格数据
                    if (vDrawCellData)
                    {
                        int vCellScreenBottom = Math.Min(ADataScreenBottom,  // 数据内容屏显最下端
                            vCellDataDrawTop
                            + Math.Max(FRows[vR].Height, FRows[vDestRow][vDestCol].Height) - FCellVPadding  // 行高和有合并的单元格高中最大的
                            );

                        //Assert(vCellScreenBottom - vMergeCellDataDrawTop >= FRows[vR].Height, "计划使用Continue但待确认会符合情况的");
                        HCTableCellData vCellData = FRows[vDestRow][vDestCol].CellData;  // 目标CellData，20170208003 如果移到if vDrawData外面则20170208002不需要了
                        int vCellScreenTop = Math.Max(ADataScreenTop, vCellDataDrawTop - FCellVPadding);  // 屏显最上端
                        if (vCellScreenTop - vDestCellDataDrawTop < vCellData.Height)
                        {
                            // 背景色
                            if ((this.IsSelectComplate || vCellData.CellSelectedAll) && (!APaintInfo.Print))
                                ACanvas.Brush.Color = OwnerData.Style.SelColor;
                            else
                                ACanvas.Brush.Color = FRows[vDestRow][vDestCol].BackgroundColor;
                            
                            ACanvas.FillRect(new RECT(vCellDrawLeft - FBorderWidth + 1, vCellScreenTop,  // + FRows[vR].Height,
                                vCellDrawLeft + FRows[vR][vC].Width + FBorderWidth, vCellScreenBottom));
                            
                            // 获取可显示区域的起始、结束DrawItem
                            //vCellData.GetDataDrawItemRang(Math.Max(vCellScreenTop - vDestCellDataDrawTop, 0),
                            //  vCellScreenBottom - vDestCellDataDrawTop, vFristDItemNo, vLastDItemNo);
                            //if vFristDItemNo >= 0 then
                            if (vCellScreenBottom - vCellScreenTop > FCellVPadding)
                            {
                                FRows[vDestRow][vDestCol].PaintData(
                                    vCellDrawLeft + FCellHPadding + 1, vDestCellDataDrawTop + 1,
                                    ADataDrawBottom, ADataScreenTop, ADataScreenBottom,
                                    0, ACanvas, APaintInfo);
                            }
                        }
                    }
#endregion

#region 绘制各单元格边框线                    
                    if (FBorderVisible || (!APaintInfo.Print))
                    {
                        bool vDrawBorder = true;
                        // 目标单元格的上边框绘制位置 vDestCellDataDrawTop本身占掉了1像素
                        // FBorderWidth + FCellVPadding = vDestCellDataDrawTop，vDestCellDataDrawTop和FCellVapdding重叠了1像素
                        int vBorderTop = vDestCellDataDrawTop - FCellVPadding + 1 - FBorderWidth;
                        vBorderBottom = vBorderTop  // 计算边框最下端
                            + Math.Max(FRows[vR].Height, this[vDestRow, vDestCol].Height)  // 由于可能是合并目标单元格，所以用单元格高和行高最高的
                            + FBorderWidth;
                        
                        // 目标单元格底部边框超过页底部，计算收敛位置
                        int vSrcRowBorderTop = 0;
                        if (vBorderBottom > ADataScreenBottom)
                        {
                            if (this[vR, vC].RowSpan > 0)
                            {
                                vSrcRowBorderTop = vBorderTop;
                                vDestRow2 = vR;  // 借用变量
                                while (vDestRow2 <= FRows.Count - 1)  // 找显示底部边框的源
                                {
                                    vSrcRowBorderTop = vSrcRowBorderTop + FRows[vDestRow2].FmtOffset + FRows[vDestRow2].Height + FBorderWidth;
                                    if (vSrcRowBorderTop > ADataScreenBottom)
                                    {
                                        if (vSrcRowBorderTop > ADataDrawBottom)
                                        {
                                            CheckRowBorderShouLian(vDestRow2, ref vShouLian, ADataDrawBottom, ADrawRect);  // 从当前行找收敛
                                            vBorderBottom = vShouLian;  //为什么是2 Min(vBorderBottom, vShouLian);  // ADataDrawBottom
                                        }
                                            
                                        break;
                                    }
                                       
                                    vDestRow2++;
                                }
                            }
                            else
                            if (this[vR, vC].RowSpan < 0)
                            {
                                if (vR != vFirstDrawRow)
                                    vDrawBorder = false;
                                else  // 跨页后第一次绘制
                                {
                                    /* 移动到当前行起始位置 }*/
                                    vSrcRowBorderTop = vBorderTop;  // 借用变量，vBorderTop值是目标单元格的上边框
                                    for (int i = vDestRow; i <= vR - 1; i++)
                                        vSrcRowBorderTop = vSrcRowBorderTop + FRows[i].Height + FBorderWidth;

                                    // 我是跨页后目标单元格正在此页源的第一个，我要负责目标在此页的边框
                                    vDestRow2 = vR;  // 借用变量
                                    while (vDestRow2 <= FRows.Count - 1)  // 找显示底部边框的源
                                    {
                                        vSrcRowBorderTop = vSrcRowBorderTop + FRows[vDestRow2].Height + FBorderWidth;
                                        if (vSrcRowBorderTop > ADataScreenBottom)
                                        {
                                            if (vSrcRowBorderTop > ADataDrawBottom)
                                            {
                                                CheckRowBorderShouLian(vDestRow2, ref vShouLian, ADataDrawBottom, ADrawRect);  // 从当前行找收敛
                                                vBorderBottom = vShouLian;  //为什么是2 Min(vBorderBottom, vShouLian);  // ADataDrawBottom
                                            }
                                                
                                            break;
                                        }
                                            
                                        vDestRow2++;
                                    }
                                }
                            }
                            else  // 普通单元格(不是合并目标也不是合并源)跨页，计算收敛
                            {
                                CheckRowBorderShouLian(vR, ref vShouLian, ADataDrawBottom, ADrawRect);
                                vBorderBottom = vShouLian;
                            }
                        }

                        if (vDrawBorder)
                        {
                            ACanvas.Pen.BeginUpdate();
                            try
                            {
                                ACanvas.Pen.Width = FBorderWidth;
                                if (FBorderVisible)
                                {
                                    ACanvas.Pen.Color = Color.Black;
                                    ACanvas.Pen.Style = HCPenStyle.psSolid;
                                }
                                else
                                    if (!APaintInfo.Print)
                                    {
                                        ACanvas.Pen.Color = HC.ColorActiveBorder;
                                        ACanvas.Pen.Style = HCPenStyle.psDot;
                                    }
                            }
                            finally
                            {
                                ACanvas.Pen.EndUpdate();
                            }
                            
                            int vBorderLeft = vCellDrawLeft - FBorderWidth + 1;
                            int vBorderRight = vBorderLeft + FColWidths[vC] + FBorderWidth;
                            
                            int vDestCol2 = FRows[vDestRow][vDestCol].ColSpan;  // 借用变量
                            while (vDestCol2 > 0)
                            {
                                vBorderRight = vBorderRight +  FBorderWidth + FColWidths[vDestCol + vDestCol2];
                                vDestCol2--;
                            }

                            if (vBorderTop < ADataScreenTop)
                                vBorderTop = ADataScreenTop;
                            
                            if ((vBorderTop > 0) && (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsTop)))
                            {
                                ACanvas.MoveTo(vBorderLeft, vBorderTop);   // 左上
                                ACanvas.LineTo(vBorderRight, vBorderTop);  // 右上
                            }

                            if (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsRight))
                            {
                                ACanvas.MoveTo(vBorderRight, vBorderTop);  // 右上
                                ACanvas.LineTo(vBorderRight, vBorderBottom);  // 右下
                            }

                            if ((vBorderBottom <= ADataScreenBottom) && (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsBottom)))
                            {
                                ACanvas.MoveTo(vBorderLeft, vBorderBottom);  // 左下
                                ACanvas.LineTo(vBorderRight + 1, vBorderBottom);  // 右下
                            }

                            if (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsLeft))
                            {
                                ACanvas.MoveTo(vBorderLeft, vBorderTop);
                                ACanvas.LineTo(vBorderLeft, vBorderBottom);
                            }

                            if (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsLTRB))
                            {
                                ACanvas.MoveTo(vBorderLeft, vBorderTop);
                                ACanvas.LineTo(vBorderRight, vBorderBottom);
                            }

                            if (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsRTLB))
                            {
                                ACanvas.MoveTo(vBorderRight, vBorderTop);
                                ACanvas.LineTo(vBorderLeft, vBorderBottom);
                            }

                            // "最后一列"负责绘制分页标识
                            vDestCol2 = vC + FRows[vR][vC].ColSpan;
                            if ((!APaintInfo.Print) && (vDestCol2 == FColWidths.Count - 1))
                            {
                                if (vCellDataDrawTop + FRows[vR].Height - FCellVPadding > ADataDrawBottom)
                                    DoDrawPageBreakMark(true, ACanvas, vBorderRight, vBorderBottom, ADataDrawTop);
                                else
                                if ((vR < this.RowCount - 1)
                                    && (vBorderBottom + FRows[vR + 1].FmtOffset + FRows[vR + 1].Height > ADataDrawBottom))
                                {
                                    if (FRows[vR + 1].FmtOffset > 0)
                                        DoDrawPageBreakMark(true, ACanvas, vBorderRight, vBorderBottom, ADataDrawTop);
                                    else
                                    if (vBorderBottom == ADataDrawBottom)
                                        DoDrawPageBreakMark(true, ACanvas, vBorderRight, vBorderBottom, ADataDrawTop); //* 此时下一行不在本页显示，但FmtOffset并不大于0，
                                }                                                                                      //* 如果这里不处理，循环下一行时底部大于当前页直接跳出循环失去绘制机会
                                    
                                if ((vFirstDrawRow != 0)  // 起始行不是第一行)
                                    && (vR == vFirstDrawRow)  // 起始行绘制
                                    && (ADrawRect.Top < ADataDrawTop))  // 第一行在上一页
                                    DoDrawPageBreakMark(false, ACanvas, vBorderRight, vBorderBottom, ADataDrawTop);
                            }
                        }
                    }
#endregion

                    vCellDrawLeft = vCellDrawLeft + FColWidths[vC] + FBorderWidth;  // 同行下一列的起始Left位置
                }
                    
                vCellDataDrawTop = vCellDataDrawBottom + FCellVPadding+ FBorderWidth;  // 下一行的Top位置
            }
            
#region 绘制拖动线
            if (Resizing && (FResizeInfo.TableSite == TableSite.tsBorderRight))
            {
                ACanvas.Pen.BeginUpdate();
                try
                {
                    ACanvas.Pen.Color = this.FBorderColor;
                    ACanvas.Pen.Style = HCPenStyle.psDot;
                    ACanvas.Pen.Width = 1;
                }
                finally
                {
                    ACanvas.Pen.EndUpdate();
                }

                ACanvas.MoveTo(ADrawRect.Left + FResizeInfo.DestX, Math.Max(ADataDrawTop, ADrawRect.Top));
                ACanvas.LineTo(ADrawRect.Left + FResizeInfo.DestX, (int)Math.Min(ADataDrawBottom,
                    Math.Min(ADrawRect.Bottom, vBorderBottom)));
            }
            else
            if (Resizing && (FResizeInfo.TableSite == TableSite.tsBorderBottom))
            {
                ACanvas.Pen.BeginUpdate();
                try
                {
                    ACanvas.Pen.Color = this.FBorderColor;
                    ACanvas.Pen.Style = HCPenStyle.psDot;
                    ACanvas.Pen.Width = 1;
                }
                finally
                {
                    ACanvas.Pen.EndUpdate();
                }

                ACanvas.MoveTo(ADrawRect.Left, ADrawRect.Top + FResizeInfo.DestY);
                ACanvas.LineTo(ADrawRect.Right, ADrawRect.Top + FResizeInfo.DestY);
            }
#endregion

        }

        // 继承THCCustomItem抽象方法
        public override void MouseDown(MouseEventArgs e)
        {
            int vMouseDownRow = -1, vMouseDownCol = -1;
            HCTableCell vCell = null;
            POINT vCellPt = new POINT();

            FMouseLBDowning = (e.Button == MouseButtons.Left);
            FOutSelectInto = false;
            FSelecting = false;  // 准备划选
            FDraging = false;  // 准备拖拽
            FOutsideInfo.Row = -1;
            
            FResizeInfo = GetCellAt(e.X, e.Y, ref vMouseDownRow, ref vMouseDownCol);
            Resizing = (e.Button == MouseButtons.Left) 
              && (  (FResizeInfo.TableSite == TableSite.tsBorderRight) 
                    ||(FResizeInfo.TableSite == TableSite.tsBorderBottom)
                  );

            if (Resizing)
            {
                FMouseDownRow = vMouseDownRow;
                FMouseDownCol = vMouseDownCol;
                FMouseDownX = e.X;
                FMouseDownY = e.Y;
                OwnerData.Style.UpdateInfoRePaint();
                return;
            }

            if (FResizeInfo.TableSite == TableSite.tsCell)
            {
                if (CoordInSelect(e.X, e.Y))
                {
                    if (FMouseLBDowning)
                        FDraging = true;
                    
                    FMouseDownRow = vMouseDownRow;  // 记录拖拽起始单元格
                    FMouseDownCol = vMouseDownCol;
                    
                    vCellPt = GetCellPostion(FMouseDownRow, FMouseDownCol);

                    MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                        e.X - vCellPt.X - FCellHPadding, e.Y - vCellPt.Y - FCellVPadding,
                        e.Delta);
                    FRows[FMouseDownRow][FMouseDownCol].CellData.MouseDown(vEventArgs);
                }
                else  // 不在选中区域中
                {
                    // 如果先执行DisSelect会清除Mouse信息，导致当前编辑单元格不能响应取消激活事件
                    if ((vMouseDownRow != FMouseDownRow) || (vMouseDownCol != FMouseDownCol))
                    {
                        vCell = GetEditCell();
                        if (vCell != null)
                            vCell.Active = false;
                        OwnerData.Style.UpdateInfoReCaret();
                    }

                    DisSelect();  // 清除原选中
                    
                    FMouseDownRow = vMouseDownRow;
                    FMouseDownCol = vMouseDownCol;
                    
                    FSelectCellRang.StartRow = FMouseDownRow;
                    FSelectCellRang.StartCol = FMouseDownCol;
                    
                    vCellPt = GetCellPostion(FMouseDownRow, FMouseDownCol);
                    
                    MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                        e.X - vCellPt.X - FCellHPadding, e.Y - vCellPt.Y - FCellVPadding, e.Delta);
                    FRows[FMouseDownRow][FMouseDownCol].CellData.MouseDown(vEventArgs);
                }
            }
            else  // 不在单元格内
            {
                DisSelect();  // 取消原来选中
                this.InitializeMouseInfo();
                if (FResizeInfo.TableSite == TableSite.tsOutside)
                {
                    FOutsideInfo.Row = vMouseDownRow;  // 左右边时对应的行
                    FOutsideInfo.Leftside = (e.X < 0);  // 左边
                }
            }
        }

        #region
        private void AdjustSelectRang(int vMoveRow, int vMoveCol)
        {
            // 先清除起始单元格之外的，以便下面重新处理选中单元格的全选
            if (FSelectCellRang.StartRow >= 0)
            {
                for (int i = FSelectCellRang.StartRow; i <= FSelectCellRang.EndRow; i++)
                {
                    for (int j = FSelectCellRang.StartCol; j <= FSelectCellRang.EndCol; j++)
                    {
                        if ((i == FMouseDownRow) && (j == FMouseDownCol))
                        {

                        }
                        else
                        {
                            if (this[i, j].CellData != null)
                                this[i, j].CellData.DisSelect();
                        }
                    }
                }
            }

            int vRow = -1, vCol = -1;
            if (FMouseDownRow < 0)
            {
                if (vMoveRow == 0)
                {
                    FMouseDownRow = 0;
                    FMouseDownCol = 0;
                    
                    FSelectCellRang.StartRow = FMouseDownRow;
                    FSelectCellRang.StartCol = FMouseDownCol;
                    FSelectCellRang.EndRow = vMoveRow;
                    FSelectCellRang.EndCol = vMoveCol;
                }
                else  // 从下面选入
                {
                    GetDestCell(this.RowCount - 1, this.FColWidths.Count - 1, ref vRow, ref vCol);
                    FMouseDownRow = vRow;
                    FMouseDownCol = vCol;
                    
                    FSelectCellRang.StartRow = vMoveRow;
                    FSelectCellRang.StartCol = vMoveCol;
                    FSelectCellRang.EndRow = FMouseDownRow;
                    FSelectCellRang.EndCol = FMouseDownCol;
                }

                FOutSelectInto = true;
            }
            else
            if (FMouseMoveRow > FMouseDownRow)
            {
                FSelectCellRang.StartRow = FMouseDownRow;
                FSelectCellRang.EndRow = FMouseMoveRow;
                
                if (FMouseMoveCol < FMouseDownCol)
                {
                    FSelectCellRang.StartCol = FMouseMoveCol;
                    FSelectCellRang.EndCol = FMouseDownCol;
                }
                else
                {
                    FSelectCellRang.StartCol = FMouseDownCol;
                    FSelectCellRang.EndCol = FMouseMoveCol;
            }
            }
            else
            if (FMouseMoveRow < FMouseDownRow)
            {
                FSelectCellRang.StartRow = FMouseMoveRow;
                FSelectCellRang.EndRow = FMouseDownRow;
                
                if (FMouseMoveCol < FMouseDownCol)
                {
                    FSelectCellRang.StartCol = FMouseMoveCol;
                    FSelectCellRang.EndCol = FMouseDownCol;
                }
                else  // 移动列在按下前后面
                {
                    FSelectCellRang.StartCol = FMouseDownCol;
                    FSelectCellRang.EndCol = FMouseMoveCol;
                }
            }
            else  // FMouseMoveRow = FMouseDownRow 移动行 = 按下行
            {
                FSelectCellRang.StartRow = FMouseDownRow;
                FSelectCellRang.EndRow = FMouseMoveRow;
                
                if (FMouseMoveCol > FMouseDownCol)
                {
                    FSelectCellRang.StartCol = FMouseDownCol;
                    FSelectCellRang.EndCol = FMouseMoveCol;
                }
                else
                if (FMouseMoveCol < FMouseDownCol)
                {
                    FSelectCellRang.StartCol = FMouseMoveCol;
                    FSelectCellRang.EndCol = FMouseDownCol;
                }
                else  // 移动列 = 按下列
                {
                    FSelectCellRang.StartCol = FMouseDownCol;
                    FSelectCellRang.EndCol = FMouseMoveCol;
                }
            }
            if ((FSelectCellRang.StartRow == FSelectCellRang.EndRow)
                && (FSelectCellRang.StartCol == FSelectCellRang.EndCol))
            {
                FSelectCellRang.EndRow = -1;
                FSelectCellRang.EndCol = -1;
            }
            else
            {
                if (FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol].IsMergeSource())
                {
                    GetDestCell(FSelectCellRang.StartRow, FSelectCellRang.StartCol, ref vRow, ref vCol);
                    FSelectCellRang.StartRow = vRow;
                    FSelectCellRang.StartCol = vCol;
                }

                if (FRows[FSelectCellRang.EndRow][FSelectCellRang.EndCol].IsMergeDest())
                {
                    GetSourceCell(FSelectCellRang.EndRow, FSelectCellRang.EndCol, ref vRow, ref vCol);  // 获取目标方法如果传递的是目标得到的是源
                    FSelectCellRang.EndRow = vRow;
                    FSelectCellRang.EndCol = vCol;
                }
                if ((FSelectCellRang.StartRow == FSelectCellRang.EndRow)
                    && (FSelectCellRang.StartCol == FSelectCellRang.EndCol))
                {
                    FSelectCellRang.EndRow = -1;
                    FSelectCellRang.EndCol = -1;
                }
            }
        }

        private void MatchCellSelectState()
        {
            if (!FSelectCellRang.EditCell())
            {
                for (int vRow = FSelectCellRang.StartRow; vRow <= FSelectCellRang.EndRow; vRow++)
                {
                    for (int vCol = FSelectCellRang.StartCol; vCol <= FSelectCellRang.EndCol; vCol++)
                    {
                        /*if (vRow = vMoveRow) and (vCol = vMoveCol) then else 什么情况下需要跳过?}*/
                        if (this[vRow, vCol].CellData != null)
                            this[vRow, vCol].CellData.SelectAll();
                    }
                }
            }
        }

        #endregion

        public override void MouseMove(MouseEventArgs e)
        {
            POINT vCellPt = new POINT();

            if (ActiveDataResizing())
            {
                vCellPt = GetCellPostion(FSelectCellRang.StartRow, FSelectCellRang.StartCol);
                MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                    e.X - vCellPt.X - FCellHPadding, e.Y - vCellPt.Y - FCellVPadding, e.Delta);
                this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData.MouseMove(vEventArgs);
                
                return;
            }

            if (Resizing)
            {
                FResizeInfo.DestX = e.X;
                FResizeInfo.DestY = e.Y;
                OwnerData.Style.UpdateInfoRePaint();
                
                return;
            }

            int vMoveRow = -1, vMoveCol = -1;
            ResizeInfo vResizeInfo = GetCellAt(e.X, e.Y, ref vMoveRow, ref vMoveCol);
            
            if (vResizeInfo.TableSite == TableSite.tsCell)
            {
                if (FMouseLBDowning)
                {
                    if (FDraging || OwnerData.Style.UpdateInfo.Draging)
                    {
                        FMouseMoveRow = vMoveRow;
                        FMouseMoveCol = vMoveCol;
                        vCellPt = GetCellPostion(FMouseMoveRow, FMouseMoveCol);

                        MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                            e.X - vCellPt.X - FCellHPadding, e.Y - vCellPt.Y - FCellVPadding, e.Delta);
                        this[FMouseMoveRow, FMouseMoveCol].CellData.MouseMove(vEventArgs);
                        
                        return;
                    }

                    if (!FSelecting)
                        FSelecting = true;
                    
                    if ((vMoveRow != FMouseMoveRow) || (vMoveCol != FMouseMoveCol))
                    {
                        FMouseMoveRow = vMoveRow;
                        FMouseMoveCol = vMoveCol;
                        AdjustSelectRang(vMoveRow, vMoveCol);  // 计算选中起始结束范围(会纠正从后、下往前选的情况)
                        MatchCellSelectState();  // 处理选中范围内各单元格的选中状态
                    }

                    MouseEventArgs vEventArgs2 = new MouseEventArgs(e.Button, e.Clicks,
                            e.X - vCellPt.X - FCellHPadding, e.Y - vCellPt.Y - FCellVPadding, e.Delta);
                    this[FMouseMoveRow, FMouseMoveCol].CellData.MouseMove(vEventArgs2);
                }
                else  // 鼠标移动，没有按键按下
                {
                    if ((vMoveRow != FMouseMoveRow) || (vMoveCol != FMouseMoveCol))
                    {
                        if ((FMouseMoveRow >= 0) && (FMouseMoveCol >= 0))
                        {
                            if (FRows[FMouseMoveRow][FMouseMoveCol].CellData != null)
                                FRows[FMouseMoveRow][FMouseMoveCol].CellData.MouseLeave();  // .MouseMove(Shift, -1, -1);  // 旧单元格移出
                        }
                        
                        FMouseMoveRow = vMoveRow;
                        FMouseMoveCol = vMoveCol;
                    }
                    
                    if ((FMouseMoveRow < 0) || (FMouseMoveCol < 0))
                        return;
                    
                    vCellPt = GetCellPostion(FMouseMoveRow, FMouseMoveCol);

                    MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                        e.X - vCellPt.X - FCellHPadding, e.Y - vCellPt.Y - FCellVPadding, e.Delta);
                    FRows[FMouseMoveRow][FMouseMoveCol].CellData.MouseMove(vEventArgs);
                }
            }
            else  // 鼠标不在单元格中
            {
                if ((FMouseMoveRow >= 0) && (FMouseMoveCol >= 0))
                {
                    if (FRows[FMouseMoveRow][FMouseMoveCol].CellData != null)
                        FRows[FMouseMoveRow][FMouseMoveCol].CellData.MouseLeave();  // .MouseMove(Shift, -1, -1);  // 旧单元格移出
                }
                
                FMouseMoveRow = -1;
                FMouseMoveCol = -1;

                if (vResizeInfo.TableSite == TableSite.tsBorderRight)
                    HC.GCursor = Cursors.VSplit;
                else
                    if (vResizeInfo.TableSite == TableSite.tsBorderBottom)
                        HC.GCursor = Cursors.HSplit;
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {
            POINT vPt = new POINT();

            FMouseLBDowning = false;
            
            if (ActiveDataResizing())
            {
                vPt = GetCellPostion(FSelectCellRang.StartRow, FSelectCellRang.StartCol);
                MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks, 
                    e.X - vPt.X - FCellHPadding, e.Y - vPt.Y - FCellVPadding, e.Delta);
                this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData.MouseUp(vEventArgs);
                
                return;
            }
            
            int vUpRow = -1, vUpCol = -1;
            ResizeInfo vResizeInfo;

            if (Resizing)
            {
                if (FResizeInfo.TableSite == TableSite.tsBorderRight)
                {
                    vPt.X = e.X - FMouseDownX;  // 不使用FResizeInfo.DestX(会造成按下处弹出也有偏移)
                    if (vPt.X != 0)
                    {
                        // AReDest为False用于处理拖动改变列宽时，如拖动处列是合并源，其他行此列并无合并操作
                        // 这时弹起，如果取拖动列目标列变宽，则其他行拖动处的列并没变宽
                        vResizeInfo = GetCellAt(FMouseDownX, FMouseDownY, ref vUpRow, ref vUpCol, false);
                        
                        if ((vResizeInfo.TableSite != TableSite.tsOutside) && (vPt.X != 0))
                        {
                            if (vPt.X > 0)
                            {
                                if (vUpCol < FColWidths.Count - 1)
                                {
                                    if (FColWidths[vUpCol + 1] - vPt.X < HC.MinColWidth)
                                        vPt.X = FColWidths[vUpCol + 1] - HC.MinColWidth;
                                   
                                    if (vPt.X != 0)
                                    {
                                        Undo_ColResize(vUpCol, FColWidths[vUpCol], FColWidths[vUpCol] + vPt.X);
                                        
                                        FColWidths[vUpCol] = FColWidths[vUpCol] + vPt.X;  // 当前列变化
                                        if (vUpCol < FColWidths.Count - 1)
                                            FColWidths[vUpCol + 1] = FColWidths[vUpCol + 1] - vPt.X;
                                    }
                                }
                                else  // 最右侧列拖宽
                                {
                                    FColWidths[vUpCol] = FColWidths[vUpCol] + vPt.X;  // 当前列变化
                                    Undo_ColResize(vUpCol, FColWidths[vUpCol], FColWidths[vUpCol] + vPt.X);
                                }
                            }
                            else  // 拖窄了
                            {
                                if (FColWidths[vUpCol] + vPt.X < HC.MinColWidth)
                                    vPt.X = HC.MinColWidth - FColWidths[vUpCol];
                                
                                if (vPt.X != 0)
                                {
                                    Undo_ColResize(vUpCol, FColWidths[vUpCol], FColWidths[vUpCol] + vPt.X);
                                    
                                    FColWidths[vUpCol] = FColWidths[vUpCol] + vPt.X;  // 当前列变化
                                    if (vUpCol < FColWidths.Count - 1)
                                        FColWidths[vUpCol + 1] = FColWidths[vUpCol + 1] - vPt.X;
                                }
                            }
                        }
                    }
                }
                else
                if (FResizeInfo.TableSite == TableSite.tsBorderBottom)
                {
                    vPt.Y = e.Y - FMouseDownY;  // // 不使用FResizeInfo.DestY(会造成按下处弹出也有偏移)
                    if (vPt.Y != 0)
                    {
                        FRows[FMouseDownRow].Height = FRows[FMouseDownRow].Height + vPt.Y;
                        FRows[FMouseDownRow].AutoHeight = false;
                    }
                }

                Resizing = false;
                HC.GCursor = Cursors.Default;
                OwnerData.Style.UpdateInfoRePaint();
                OwnerData.Style.UpdateInfoReCaret();
                
                return;
            }

            if (FSelecting || OwnerData.Style.UpdateInfo.Selecting)
            {
                FSelecting = false;
                
                // 先在按下单元格弹起，以便单元格中嵌套的表格有机会响应弹起(取消按下、划选状态，划选完成)
                if ((FMouseDownRow >= 0) && (!FOutSelectInto))
                {
                    vPt = GetCellPostion(FMouseDownRow, FMouseDownCol);

                    MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                        e.X - vPt.X - FCellHPadding, e.Y - vPt.Y - FCellVPadding, e.Delta);
                    this[FMouseDownRow, FMouseDownCol].CellData.MouseUp(vEventArgs);
                }

                vResizeInfo = GetCellAt(e.X, e.Y, ref vUpRow, ref vUpCol);
                if (vResizeInfo.TableSite == TableSite.tsCell)
                {
                    if ((vUpRow != FMouseDownRow) || (vUpCol != FMouseDownCol))
                    {
                        vPt = GetCellPostion(vUpRow, vUpCol);

                        MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                            e.X - vPt.X - FCellHPadding, e.Y - vPt.Y - FCellVPadding, e.Delta);
                        this[vUpRow, vUpCol].CellData.MouseUp(vEventArgs);
                    }
                }
            }
            else
            if (FDraging || OwnerData.Style.UpdateInfo.Draging)
            {
                FDraging = false;
                vResizeInfo = GetCellAt(e.X, e.Y, ref vUpRow, ref vUpCol);
                if (vResizeInfo.TableSite == TableSite.tsCell)
                {
                    DisSelect();
                    FMouseMoveRow = vUpRow;  // 拖拽时的单元格定位使用的是MouseMove相关数据
                    FMouseMoveCol = vUpCol;
           
                    // 不管是否在在选中单元格中弹起，拖拽弹起都需要编辑到选中单元格，
                    FSelectCellRang.StartRow = vUpRow;
                    FSelectCellRang.StartCol = vUpCol;
                    vPt = GetCellPostion(vUpRow, vUpCol);

                    MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                        e.X - vPt.X - FCellHPadding, e.Y - vPt.Y - FCellVPadding, e.Delta);
                    this[vUpRow, vUpCol].CellData.MouseUp(vEventArgs);
                }
            }
            else  // 非划选，非拖拽
            if (FMouseDownRow >= 0)
            {
                vPt = GetCellPostion(FMouseDownRow, FMouseDownCol);

                MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                    e.X - vPt.X - FCellHPadding, e.Y - vPt.Y - FCellVPadding, e.Delta);
                this[FMouseDownRow, FMouseDownCol].CellData.MouseUp(vEventArgs);
            }
        }

        public override void MouseLeave()
        {
            base.MouseLeave();
            if ((FMouseMoveRow < 0) || (FMouseMoveCol < 0))
                return;
    
            if (FRows[FMouseMoveRow][FMouseMoveCol].CellData != null)
                FRows[FMouseMoveRow][FMouseMoveCol].CellData.MouseLeave();  // .MouseMove([], -1, -1);  // 处理鼠标移上高亮在迅速移出表格后不能恢复的问题
    
            if (!SelectExists())
                this.InitializeMouseInfo();
        }

        public override void KillFocus()
        {

        }

        // 继承TCustomRectItem抽象方法
        public override int ApplySelectTextStyle(HCStyle AStyle, HCStyleMatch AMatchStyle)
        {
            int Result = -1;
            if (FSelectCellRang.EditCell())
                Result = GetEditCell().CellData.ApplySelectTextStyle(AMatchStyle);
            else
                if (FSelectCellRang.StartRow >= 0)
                {
                    for (int vR = FSelectCellRang.StartRow; vR <= FSelectCellRang.EndRow; vR++)
                    {
                        /* TODO -jingtong : 当单元格SelectComplate时，处理全部应用样式 }*/
                        for (int vC = FSelectCellRang.StartCol; vC <= FSelectCellRang.EndCol; vC++)
                        {
                            if (this[vR, vC].CellData != null)
                                Result = this[vR, vC].CellData.ApplySelectTextStyle(AMatchStyle);
                        }
                    }
                }

            return Result;
        }

        public override void ApplySelectParaStyle(HCStyle AStyle, HCParaMatch AMatchStyle)
        {
            if (FSelectCellRang.StartRow >= 0)
            {
                if (FSelectCellRang.EndRow >= 0)
                {
                    for (int vR = FSelectCellRang.StartRow; vR <= FSelectCellRang.EndRow; vR++)
                    {
                        /* TODO -jingtong : 当单元格SelectComplate时，处理全部应用样式 }*/
                        for (int vC = FSelectCellRang.StartCol; vC <= FSelectCellRang.EndCol; vC++)
                        {
                            if (this[vR, vC].CellData != null)
                                this[vR, vC].CellData.ApplySelectParaStyle(AMatchStyle);
                        }
                    }
                }
                else  // 在同一单元格
                    GetEditCell().CellData.ApplySelectParaStyle(AMatchStyle);
            }
            else
                this.ParaNo = AMatchStyle.GetMatchParaNo(OwnerData.Style, this.ParaNo);
        }

        #region
        private void UpdateCellSize(int ARow)
        {
            int vNorHeightMax = 0;  // 行中未发生合并的最高单元格
            // 得到行中未发生合并内容最高的单元格高度
            for (int vC = 0; vC <= FRows[ARow].ColCount - 1; vC++)
            {
                if ((FRows[ARow][vC].CellData != null)  // 不是被合并的单元格)
                    && (FRows[ARow][vC].RowSpan == 0))  // 不是行合并的行单元格
                    vNorHeightMax = Math.Max(vNorHeightMax, FRows[ARow][vC].Height);
            }

            for (int vC = 0; vC <= FRows[ARow].ColCount - 1; vC++)
                FRows[ARow][vC].Height = vNorHeightMax;

            if (FRows[ARow].AutoHeight)
                FRows[ARow].Height = vNorHeightMax;
            else  // 拖动改变了高度
            {
                if (vNorHeightMax > FRows[ARow].Height)
                {
                    FRows[ARow].AutoHeight = true;
                    FRows[ARow].Height = vNorHeightMax;
                }
                //FRows[ARow].Height := Max(FRows[ARowID].Height, vNorHeightMax);  // 记录行高，其实可以用行中第一个没有合并的列高度
            }
        }

        private void ConvertRow(int ARow)
        {
            int vWidth = 0;
            HCTableRow vRow = FRows[ARow];
            vRow.FmtOffset = 0;  // 恢复上次格式化可能的偏移
            // 格式化各单元格中的Data
            for (int vC = 0; vC <= vRow.ColCount - 1; vC++)
            {
                if (vRow[vC].CellData != null)
                {
                    vWidth = FColWidths[vC];
                    for (int i = 1; i <= vRow[vC].ColSpan; i++)
                        vWidth = vWidth + FBorderWidth + FColWidths[vC + i];
                    
                    vRow[vC].Width = vWidth;
                    vRow[vC].CellData.Width = vWidth - FCellHPadding - FCellHPadding;
                    vRow[vC].CellData.ReFormat(0);
                    vRow[vC].Height = FCellHPadding + vRow[vC].CellData.Height + FCellHPadding;
                }
            }
        }

        #endregion

        public override void FormatToDrawItem(HCCustomData ARichData, int AItemNo)
        {
            for (int vR = 0; vR <= RowCount - 1; vR++)  // 格式化各
            {
                ConvertRow(vR);  // 格式化行，并计算行高度
                UpdateCellSize(vR);  // 以行中所有无行合并操作列中最大高度更新其他列
            }

            FormatRowFrom(0);
            
            Height = GetFormatHeight();  // 计算整体高度
            
            // 计算整体宽度
            int i = FBorderWidth;
            for (int vC = 0; vC <= FColWidths.Count - 1; vC++)
                i = i + FColWidths[vC] + FBorderWidth;
            Width = i;
        }

        /// <summary> 正在其上时内部是否处理指定的Key和Shif </summary>
        public override bool WantKeyDown(KeyEventArgs e)
        {
            return true;
        }

        /// <summary> 清除并返回为处理分页行和行中间有向下偏移后，比净高增加的高度(为重新格式化时后面计算偏移用) </summary>
        public override int ClearFormatExtraHeight()
        {
            int Result = 0;
            for (int i = 0; i <= FRows.Count - 1; i++)
                Result = Result + FRows[i].ClearFormatExtraHeight();
            this.Height = this.Height - Result;

            return Result;
        }

        public override bool DeleteSelected()
        {
            bool Result = base.DeleteSelected();
            
            if (FSelectCellRang.StartRow >= 0)
            {
                if (FSelectCellRang.EndRow >= 0)
                {
                    Result = true;
                    for (int vR = FSelectCellRang.StartRow; vR <= FSelectCellRang.EndRow; vR++)
                    {
                        for (int vC = FSelectCellRang.StartCol; vC <= FSelectCellRang.EndCol; vC++)
                        {
                            if (this[vR, vC].CellData != null)
                                this[vR, vC].CellData.DeleteSelected();
                    }
                }
            }
            else  // 在同一单元格
                Result = GetEditCell().CellData.DeleteSelected();
            }

            return Result;
        }

        public override void DisSelect()
        {
            base.DisSelect();
            DisSelectSelectedCell();
            FSelectCellRang.Initialize();
            this.InitializeMouseInfo();
            FSelecting = false;
            FDraging = false;
            FOutSelectInto = false;
        }

        public override void MarkStyleUsed(bool AMark)
        {
            base.MarkStyleUsed(AMark);
            for (int vR = 0; vR <= FRows.Count - 1; vR++)
            {
                for (int vC = 0; vC <= FRows[vR].ColCount - 1; vC++)
                {
                    if (FRows[vR][vC].CellData != null)
                        FRows[vR][vC].CellData.MarkStyleUsed(AMark);
                }
            }
        }

        public override void GetCaretInfo(ref HCCaretInfo ACaretInfo)
        {
            int vRow = -1, vCol = -1;

            if (OwnerData.Style.UpdateInfo.Draging)
            {
                vRow = FMouseMoveRow;
                vCol = FMouseMoveCol;
            }
            else  // 非拖拽
            {
                vRow = FSelectCellRang.StartRow;
                vCol = FSelectCellRang.StartCol;
            }
            
            HCTableCell vCaretCell = null;
            int vTop = -1, vBottom = -1;
            if (vRow < 0)
            {
                if (FOutsideInfo.Row >= 0)
                {
                    if (FOutsideInfo.Leftside)
                        ACaretInfo.X = ACaretInfo.X - 2;  // 为使光标更明显，向左偏移2
                    
                    vTop = 0;
                    for (int i = FPageBreaks.Count - 1; i >= 0; i--)  // 找光标顶部位
                    {
                        if (FPageBreaks[i].Row <= FOutsideInfo.Row)
                        {
                            if (FPageBreaks[i].PageIndex == ACaretInfo.PageIndex - 1)
                            {
                                vTop = FPageBreaks[i].BreakBottom;  // 分页底部位置
                                break;
                            }
                        }
                    }

                    vBottom = this.Height;
                    for (int i = 0; i <= FPageBreaks.Count - 1; i++)  // 找光标底部位
                    {
                        if (FPageBreaks[i].Row >= FOutsideInfo.Row)
                        {
                            if (FPageBreaks[i].PageIndex == ACaretInfo.PageIndex)
                            {
                                vBottom = FPageBreaks[i].BreakSeat;  // 分页顶部位置
                                break;
                            }
                        }
                    }

                    ACaretInfo.Y = ACaretInfo.Y + vTop;
                    ACaretInfo.Height = vBottom - vTop;
                }
                else
                    ACaretInfo.Visible = false;
                
                return;
            }
            else
                vCaretCell = this[vRow, vCol];

            if (OwnerData.Style.UpdateInfo.Draging)
            {
                if ((vCaretCell.CellData.MouseMoveItemNo < 0)
                    || (vCaretCell.CellData.MouseMoveItemOffset < 0))
                {
                    ACaretInfo.Visible = false;
                    return;
                }

                vCaretCell.GetCaretInfo(vCaretCell.CellData.MouseMoveItemNo,
                    vCaretCell.CellData.MouseMoveItemOffset, ref ACaretInfo);
            }
            else  // 非拖拽
            {
                if ((vCaretCell.CellData.SelectInfo.StartItemNo < 0)
                    || (vCaretCell.CellData.SelectInfo.StartItemOffset < 0))
                {
                    ACaretInfo.Visible = false;
                    return;
                }

                vCaretCell.GetCaretInfo(vCaretCell.CellData.SelectInfo.StartItemNo,
                vCaretCell.CellData.SelectInfo.StartItemOffset, ref ACaretInfo);
            }

            POINT vPos = GetCellPostion(vRow, vCol);
            ACaretInfo.X = vPos.X + ACaretInfo.X + FCellHPadding;
            ACaretInfo.Y = vPos.Y + ACaretInfo.Y + FCellVPadding;
        }

        protected override void SetActive(bool Value)
        {
            if (this.Active != Value)
            {
                HCTableCell vCell = GetEditCell();
                if ((vCell != null) && (vCell.CellData != null))
                    vCell.CellData.Active = Value;
                
                if (!Value)
                    this.InitializeMouseInfo();
                
                base.SetActive(Value);
            }
        }

        /// <summary> 获取表格在指定高度内的结束位置处行中最下端(暂时没用到注释了) </summary>
        /// <param name="AHeight">指定的高度范围</param>
        /// <param name="ADItemMostBottom">最后一行最底端DItem的底部位置</param>
        //procedure GetPageFmtBottomInfo(const AHeight: Integer; var ADItemMostBottom: Integer); override;
        public override void DblClick(int X, int Y)
        {
            if (FSelectCellRang.EditCell())
            {
                POINT vPt = GetCellPostion(FSelectCellRang.StartRow, FSelectCellRang.StartCol);
                this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData.DblClick(
                    X - vPt.X - FCellHPadding, Y - vPt.Y - FCellVPadding);
            }
            else
                base.DblClick(X, Y);
        }

        public override bool CoordInSelect(int X, int Y)
        {
            bool Result = base.CoordInSelect(X, Y);  // 有选中且在RectItem区域中(粗略估算)
            if (Result)
            {
                int vRow = -1, vCol = -1;
                ResizeInfo vResizeInfo = GetCellAt(X, Y, ref vRow, ref vCol);  // 坐标处信息
                Result = vResizeInfo.TableSite == TableSite.tsCell;  // 坐标处在单元格中不在边框上
                if (Result)
                {
                    if (FSelectCellRang.StartRow >= 0)
                    {
                        if (FSelectCellRang.EndRow >= 0)
                        {
                            Result = (vRow >= FSelectCellRang.StartRow)
                                && (vRow <= FSelectCellRang.EndRow)
                                && (vCol >= FSelectCellRang.StartCol)
                                && (vCol <= FSelectCellRang.EndCol);
                        }
                        else  // 无选择结束行，判断是否在当前单元格的选中中
                        {
                            HCTableCellData vCellData = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData;
                            if (vCellData.SelectExists())
                            {
                                POINT vCellPt = GetCellPostion(FSelectCellRang.StartRow, FSelectCellRang.StartCol);
                                int vX = X - vCellPt.X - FCellHPadding;
                                int vY = Y - vCellPt.Y - FCellVPadding;

                                int vItemNo = -1, vOffset = -1, vDrawItemNo = -1; 
                                bool vRestrain = false;
                                vCellData.GetItemAt(vX, vY, ref vItemNo, ref vOffset, ref vDrawItemNo, ref vRestrain);
                                Result = vCellData.CoordInSelect(vX, vY, vItemNo, vOffset, vRestrain);
                            }
                        }
                    }
                }
            }

            return Result;
        }

        public override HCCustomData GetTopLevelDataAt(int X, int Y)
        {
            HCCustomData Result = null;
            int vRow = -1, vCol = -1;
            ResizeInfo vResizeInfo = GetCellAt(X, Y, ref vRow, ref vCol);
            if ((vRow < 0) || (vCol < 0))
                return Result;
            
            POINT vCellPt = GetCellPostion(vRow, vCol);
            Result = (this[vRow, vCol].CellData as HCCustomRichData).GetTopLevelDataAt(
                X - vCellPt.X - FCellHPadding, Y - vCellPt.Y - FCellVPadding);

            return Result;
        }

        public override HCCustomData GetActiveData()
        {
            HCCustomData Result = null;
            HCTableCell vCell = GetEditCell();
            if (vCell != null)
                Result = vCell.CellData.GetTopLevelData();

            return Result;
        }

        public override HCCustomItem GetActiveItem()
        {
            HCCustomItem Result = this;
            HCTableCell vCell = GetEditCell();
            if (vCell != null)
                Result = vCell.CellData.GetTopLevelItem();

            return Result;
        }

        public override HCCustomDrawItem GetActiveDrawItem()
        {
            HCCustomDrawItem Result = null;
            HCTableCellData vCellData = GetActiveData() as HCTableCellData;
            if (vCellData != null)
                Result = vCellData.GetTopLevelDrawItem();

            return Result;
        }

        public override POINT GetActiveDrawItemCoord()
        {
            POINT Result = new POINT(0, 0);
            HCTableCell vCell = GetEditCell();
            if (vCell != null)
            {
                Result = vCell.CellData.GetActiveDrawItemCoord();
                POINT vPt = GetCellPostion(FSelectCellRang.StartRow, FSelectCellRang.StartCol);
                Result.X = Result.X + vPt.X + FCellHPadding;
                Result.Y = Result.Y + vPt.Y + FCellVPadding;
            }

            return Result;
        }

        public override string GetHint()
        {
            string Result = base.GetHint();
            if ((FMouseMoveRow < 0) || (FMouseMoveCol < 0))
                return Result;
    
            HCTableCell vCell = this[FMouseMoveRow, FMouseMoveCol];
            if ((vCell != null) && (vCell.CellData != null))
                Result = vCell.CellData.GetHint();

            return Result;
        }

        public override bool InsertText(string AText)
        {
            bool vResult = false;

            if (FSelectCellRang.EditCell())
            {
                HCProcedure vEvent = delegate()
                {
                    HCTableCell vEditCell = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol];
                    vResult = vEditCell.CellData.InsertText(AText);
                };

                this.SizeChanged = CellChangeByAction(FSelectCellRang.StartRow,
                    FSelectCellRang.StartCol, vEvent);

                return vResult;
            }
            else
                return base.InsertText(AText);
        }

        public override bool InsertItem(HCCustomItem AItem)
        {
            bool Result = false;
            HCTableCell vCell = GetEditCell();
            if (vCell != null)
                Result = vCell.CellData.InsertItem(AItem);

            return Result;
        }

        public override bool InsertStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            bool Result = false;
            HCTableCell vCell = GetEditCell();
            if (vCell != null)
                Result = vCell.CellData.InsertStream(AStream, AStyle, AFileVersion);

            return Result;
        }

        #region
        private bool IsDirectionKey(int AKey)
        {
            return ((AKey == User.VK_LEFT)
                || (AKey == User.VK_UP)
                || (AKey == User.VK_RIGHT)
                || (AKey == User.VK_DOWN));
        }

        private bool DoCrossCellKey(int AKey)
        {
            bool Result = false;
            int vRow = -1, vCol = -1;
            HCTableCell vEditCell = null;

            if (AKey == User.VK_LEFT)
            {
                if (vEditCell.CellData.SelectFirstItemOffsetBefor())
                {
                    // 找左侧单元格
                    for (int i = FSelectCellRang.StartCol; i >= 0; i--)
                    {
                        if (this[FSelectCellRang.StartRow, i].ColSpan == 0)
                        {
                            vCol = i;
                            break;
                        }
                    }
                    
                    if (vCol >= 0)
                    {
                        FSelectCellRang.StartCol = vCol;
                        HCTableCellData vData = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData;
                        vData.SelectInfo.StartItemNo = 0;
                        vData.SelectInfo.StartItemOffset = 0;
                        vData.CaretDrawItemNo = vData.DrawItems.Count - 1;
                    
                        Result = true;
                    }
                }
            }
            else
            if (AKey == User.VK_RIGHT)
            {
                if (vEditCell.CellData.SelectLastItemOffsetAfter())
                {
                    // 找右侧单元格
                    for (int i = FSelectCellRang.StartCol; i <= FColWidths.Count - 1; i++)
                    {
                        if (this[FSelectCellRang.StartRow, i].ColSpan == 0)
                        {
                            vCol = i;
                            break;
                        }
                    }
                    
                    if (vCol >= 0)
                    {
                        FSelectCellRang.StartCol = vCol;
                        HCTableCellData vData = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData;
                        vData.SelectInfo.StartItemNo = 0;
                        vData.SelectInfo.StartItemOffset = 0;
                        vData.CaretDrawItemNo = 0;
                    
                        Result = true;
                    }
                }
            }
            else
            if (AKey == User.VK_UP)
            {
                if ((vEditCell.CellData.SelectFirstLine()) && (FSelectCellRang.StartRow > 0))
                {
                    GetDestCell(FSelectCellRang.StartRow - 1, FSelectCellRang.StartCol, ref vRow, ref vCol);
                    if ((vRow >= 0) && (vCol >= 0))
                    {
                        FSelectCellRang.StartRow = vRow;
                        FSelectCellRang.StartCol = vCol;
                        HCTableCellData vData = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData;
                        vData.SelectInfo.StartItemNo = vData.Items.Count - 1;
                        vData.SelectInfo.StartItemOffset = vData.GetItemAfterOffset(vData.SelectInfo.StartItemNo);
                        vData.CaretDrawItemNo = vData.DrawItems.Count - 1;
                    
                        Result = true;
                    }
                }
            }
            else
            if (AKey == User.VK_DOWN)
            {
                if ((vEditCell.CellData.SelectLastLine()) && (FSelectCellRang.StartRow < this.RowCount - 1))
                {
                    GetDestCell(FSelectCellRang.StartRow + 1, FSelectCellRang.StartCol, ref vRow, ref vCol);
                    if ((vRow >= 0) && (vCol >= 0))
                    {
                        FSelectCellRang.StartRow = vRow;
                        FSelectCellRang.StartCol = vCol;
                        HCTableCellData vData = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData;
                        vData.SelectInfo.StartItemNo = 0;
                        vData.SelectInfo.StartItemOffset = 0;
                        vData.CaretDrawItemNo = 0;
                    
                        Result = true;
                    }
                }
            }

            return Result;
        }
        #endregion
        public override void KeyDown(KeyEventArgs e)
        {
            this.SizeChanged = false;
            HCTableCell vEditCell = GetEditCell();
            if (vEditCell != null)
            {
                HCProcedure vEvent = delegate()
                {
                    vEditCell.CellData.KeyDown(e);
                };

                this.SizeChanged = CellChangeByAction(FSelectCellRang.StartRow,
                    FSelectCellRang.StartCol, vEvent);

                if ((e.Handled) && IsDirectionKey(e.KeyValue))
                {
                    if (DoCrossCellKey(e.KeyValue))
                    {
                        OwnerData.Style.UpdateInfoReCaret();
                    }
                }
            }
            else
                e.Handled = true; ;
        }

        public override void KeyPress(ref Char Key)
        {
            this.SizeChanged = false;
            HCTableCell vEditCell = GetEditCell();
            if (vEditCell != null)
            {
                Char vOldKey = Key;

                HCProcedure vEvent = delegate()
                {
                    vEditCell.CellData.KeyPress(ref vOldKey);
                };

                this.SizeChanged = CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);
               
                Key = vOldKey;
            }
        }

        public override bool IsSelectComplateTheory()
        {
            return IsSelectComplate;
        }

        public override bool SelectExists()
        {
            bool Result = false;
            if (this.IsSelectComplate)
                Result = true;
            else
            if (FSelectCellRang.StartRow >= 0)
            {
                if (FSelectCellRang.EndRow >= 0)
                    Result = true;
                else  // 无选择结束行，判断当前单元格是否有选中
                    Result = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData.SelectExists();
            }

            return Result;
        }

        public override void TraverseItem(HCItemTraverse ATraverse)
        {
            for (int vR = 0; vR <= FRows.Count - 1; vR++)
            {
                if (ATraverse.Stop)
                    break;

                for (int vC = 0; vC <= FColWidths.Count - 1; vC++)
                {
                    if (ATraverse.Stop)
                        break;

                    if (this[vR, vC].CellData != null)
                        this[vR, vC].CellData.TraverseItem(ATraverse);
                }
            }
        }

        // 撤销重做相关方法
        protected override void DoNewUndo(HCUndo Sender)
        {
            if (FSelectCellRang.EditCell())
            {
                HCUndoCell vUndoCell = new HCUndoCell();
                vUndoCell.Row = FSelectCellRang.StartRow;
                vUndoCell.Col = FSelectCellRang.StartCol;
                Sender.Data = vUndoCell;
            }
        }

        protected override void DoUndoDestroy(HCUndo Sender)
        {
            if (Sender.Data is HCUndoCell)
                (Sender.Data as HCUndoCell).Dispose();

            base.DoUndoDestroy(Sender);
        }

        protected override void DoUndo(HCUndo Sender)
        {
            if (Sender.Data is HCUndoCell)
            {
                HCUndoCell vCell = Sender.Data as HCUndoCell;
                this[vCell.Row, vCell.Col].CellData.Undo(Sender);
            }
            else
            if (Sender.Data is HCUndoColSize)
            {
                HCUndoColSize vColSize = Sender.Data as HCUndoColSize;
                if (vColSize.Col < FColWidths.Count - 1)
                {
                    FColWidths[vColSize.Col + 1] = FColWidths[vColSize.Col + 1] +
                        FColWidths[vColSize.Col] - vColSize.OldWidth;
                }
                FColWidths[vColSize.Col] = vColSize.OldWidth;
            }
            else
            if (Sender.Data is HCUndoMirror)
            {
                MemoryStream vStream = new MemoryStream();
                try
                {
                    this.SaveToStream(vStream);  // 记录撤销前状态
                    // 恢复原样
                    HCUndoMirror vMirror = Sender.Data as HCUndoMirror;
                    vMirror.Stream.Position = 0;

                    int vStyleNo = HCStyle.Null;
                    byte[] vBuffer = BitConverter.GetBytes(vStyleNo);
                    vMirror.Stream.Read(vBuffer, 0, vBuffer.Length);
                    vStyleNo = BitConverter.ToInt32(vBuffer, 0);
                    FEnableUndo = false;
                    try
                    {
                        this.LoadFromStream(vMirror.Stream, OwnerData.Style, HC.HC_FileVersionInt);
                    }
                    finally
                    {
                        FEnableUndo = true;
                    }
            
                    vMirror.Stream.SetLength(0);
                    vStream.CopyTo(vMirror.Stream);  // 保存撤销前状态
                }
                finally
                {
                    vStream.Close();
                    vStream.Dispose();
                }
            }
            else
                base.DoUndo(Sender);
        }

        protected override void DoRedo(HCUndo Sender)
        {
            if (Sender.Data is HCUndoCell)
            {
                HCUndoCell vRedoCell = Sender.Data as HCUndoCell;
                this[vRedoCell.Row, vRedoCell.Col].CellData.Redo(Sender);
            }
            else
            if (Sender.Data is HCUndoColSize)
            {
                HCUndoColSize vColSize = Sender.Data as HCUndoColSize;
                if (vColSize.Col < FColWidths.Count - 1)
                {
                    FColWidths[vColSize.Col + 1] = FColWidths[vColSize.Col + 1] +
                    FColWidths[vColSize.Col] - vColSize.NewWidth;
            }
                FColWidths[vColSize.Col] = vColSize.NewWidth;
            }
            else
            if (Sender.Data is HCUndoMirror)
            {
                MemoryStream vStream = new MemoryStream();
                try
                {
                    this.SaveToStream(vStream);  // 记录恢复前状态
                    HCUndoMirror vMirror = Sender.Data as HCUndoMirror;
                    vMirror.Stream.Position = 0;

                    int vStyleNo = HCStyle.Null;
                    byte[] vBuffer = BitConverter.GetBytes(vStyleNo);
                    vMirror.Stream.Read(vBuffer, 0, vBuffer.Length);
                    vStyleNo = BitConverter.ToInt32(vBuffer, 0);
                    FEnableUndo = false;
                
                    try
                    {
                        this.LoadFromStream(vMirror.Stream, OwnerData.Style, HC.HC_FileVersionInt);
                    }
                    finally
                    {
                        FEnableUndo = true;
                    }

                    vMirror.Stream.SetLength(0);
                    vStream.CopyTo(vMirror.Stream);  // 保存恢复前状态
                }
                finally
                {
                    vStream.Close();
                    vStream.Dispose();
                }
            }
            else
                base.DoRedo(Sender);
        }

        protected void Undo_ColResize(int ACol, int AOldWidth, int ANewWidth)
        {
            if (OwnerData.Style.EnableUndo)
            {
                Undo_StartRecord();
                HCUndoList vUndoList = GetSelfUndoList();
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCUndoColSize vUndoColSize = new HCUndoColSize();
                    vUndoColSize.Col = ACol;
                    vUndoColSize.OldWidth = AOldWidth;
                    vUndoColSize.NewWidth = ANewWidth;
                    vUndo.Data = vUndoColSize;
                }
            }
        }

        protected void Undo_MergeCells()
        {
            if (OwnerData.Style.EnableUndo)
            {
                Undo_StartRecord();
                HCUndoList vUndoList = GetSelfUndoList();
                HCUndo vUndo = vUndoList[vUndoList.Count - 1];
                if (vUndo != null)
                {
                    HCUndoMirror vMirror = new HCUndoMirror();
                    this.SaveToStream(vMirror.Stream);
                    vUndo.Data = vMirror;
                }
            }
        }

        protected int GetRowCount()
        {
            return FRows.Count;
        }

        protected int GetColCount()
        {
            return FColWidths.Count;
        }

        /// <summary> 获取指定行列范围实际对应的行列范围
        /// </summary>
        /// <param name="AStartRow"></param>
        /// <param name="AStartCol"></param>
        /// <param name="AEndRow"></param>
        /// <param name="AEndCol"></param>
        protected void AdjustCellRange(int AStartRow, int AStartCol, ref int AEndRow, ref int AEndCol)
        {
            int vLastRow = AEndRow;
            int vLastCol = AEndCol;
            HCTableCell vCell = null;
            int vDestRow = -1, vDestCol = -1;
            for (int vR = AStartRow; vR <= AEndRow; vR++)
            {
                for (int vC = AStartCol; vC <= AEndCol; vC++)
                {
                    vCell = FRows[vR][vC];
                    if ((vCell.RowSpan > 0) || (vCell.ColSpan > 0))
                    {
                        GetDestCell(vR, vC, ref vDestRow, ref vDestCol);
                        vCell = FRows[vDestRow][vDestCol];
                        vDestRow = vDestRow + vCell.RowSpan;
                        vDestCol = vDestCol + vCell.ColSpan;
                        if (vLastRow < vDestRow)
                            vLastRow = vDestRow;
                        if (vLastCol < vDestCol)
                            vLastCol = vDestCol;
                    }
                }
            }

            AEndRow = vLastRow;
            AEndCol = vLastCol;
        }

        #region
        private void DeleteEmptyRows(int ASRow, int AERow)
        {
            bool vEmptyRow = false;
            for (int vR = AERow; vR >= ASRow; vR--)  // 遍历
            {
                vEmptyRow = true;
                for (int vC = 0; vC <= FRows[vR].ColCount - 1; vC++)  // 当前行各
                {
                    if (FRows[vR][vC].CellData != null)
                    {
                        vEmptyRow = false;  // 不是空行
                        break;
                    }
                }
                if (vEmptyRow)
                {
                    for (int vR1 = 0; vR1 <= vR - 1; vR1++)
                    {
                        for (int vC = 0; vC <= FRows[vR1].ColCount - 1; vC++)
                        {
                            if (this[vR1, vC].RowSpan > 0)
                                this[vR1, vC].RowSpan = this[vR1, vC].RowSpan - 1;
                        }
                    }
                    
                    for (int vR1 = vR; vR1 <= FRows.Count - 1; vR1++)
                    {
                        for (int vC = 0; vC <= FRows[vR1].ColCount - 1; vC++)
                        {
                            if (this[vR1, vC].RowSpan < 0)
                                this[vR1, vC].RowSpan = this[vR1, vC].RowSpan + 1;
                        }
                    }
                    
                    FRows.RemoveAt(vR);  // 删除当前空行
                }
            }
        }

        private void DeleteEmptyCols(int ASCol, int AECol)
        {
            bool vEmptyCol = false;

            for (int vC = AECol; vC >= ASCol; vC--)  // 循环各
            {
                vEmptyCol = true;
                for (int vR = 0; vR <= RowCount - 1; vR++)  // 循环各
                {
                    if (FRows[vR][vC].CellData != null)
                    {
                        vEmptyCol = false;
                        break;
                    }
                }

                if (vEmptyCol)
                {
                    HCTableCell vTableCell = null;
                    for (int vR = RowCount; vR >= 0; vR--)  // 循环各行，删除对应
                    {
                        for (int vC2 = 0; vC2 <= vC - 1; vC2++)
                        {
                            vTableCell = FRows[vR][vC2];
                            if (vC2 + vTableCell.ColSpan >= vC)
                                vTableCell.ColSpan = vTableCell.ColSpan - 1;
                        }

                        for (int vC2 = vC; vC2 <= FRows[vR].ColCount - 1; vC2++)
                        {
                            vTableCell = FRows[vR][vC2];
                            if (vC2 + vTableCell.ColSpan < vC)
                                vTableCell.ColSpan = vTableCell.ColSpan + 1;
                        }
                            
                        FRows[vR].RemoveAt(vC);  // 删除列
                    }
                    
                    FColWidths[vC - 1] = FColWidths[vC -1] + FBorderWidth + FColWidths[vC];
                    FColWidths.RemoveAt(vC);
                }
            }
        }
        #endregion

        protected bool MergeCells(int AStartRow, int AStartCol, int AEndRow, int AEndCol)
        {
            bool Result = false;
            int vEndRow = AEndRow;
            int vEndCol = AEndCol;
            
            AdjustCellRange(AStartRow, AStartCol, ref vEndRow, ref vEndCol);
            Result = CellsCanMerge(AStartRow, AStartCol, vEndRow, vEndCol);
            if (!Result)
                return Result;
            
            // 经过上面的校验和判断后，起始行、列和结束行、列组成一个矩形区域
            if (AStartRow == vEndRow)
            {
                for (int vC = AStartCol; vC <= vEndCol; vC++)  // 合并
                {
                    if (FRows[AStartRow][vC].CellData != null)
                    {
                        this[AStartRow, AStartCol].CellData.AddData(this[AStartRow, vC].CellData);
                        this[AStartRow, vC].CellData.Dispose();
                        this[AStartRow, vC].CellData = null;
                    }
                    
                    this[AStartRow, vC].ColSpan = AStartCol - vC;
                }

                this[AStartRow, AStartCol].ColSpan = vEndCol - AStartCol;  // 合并源增加
                
                DeleteEmptyCols(AStartCol + 1, vEndCol);
                Result = true;
            }
            else
            if (AStartCol == vEndCol)
            {
                for (int vR = AStartRow; vR <= vEndRow; vR++)  // 合并各
                {
                    if (FRows[vR][AStartCol].CellData != null)
                    {
                        FRows[AStartRow][AStartCol].CellData.AddData(FRows[vR][AStartCol].CellData);
                        FRows[vR][AStartCol].CellData.Dispose();
                        FRows[vR][AStartCol].CellData = null;
                    }

                    this[vR, AStartCol].RowSpan = AStartRow - vR;
                }
                
                FRows[AStartRow][AStartCol].RowSpan = vEndRow - AStartRow;
                
                DeleteEmptyRows(AStartRow + 1, vEndRow);
                Result = true;
            }
            else  // 不同行，不同列
            {
                for (int vC = AStartCol; vC <= vEndCol; vC++)  // 起始行各列合
                {
                    if (FRows[AStartRow][vC].CellData != null)
                    {
                        FRows[AStartRow][AStartCol].CellData.AddData(FRows[AStartRow][vC].CellData);
                        FRows[AStartRow][vC].CellData.Dispose();
                        FRows[AStartRow][vC].CellData = null;
                    }
                    
                    FRows[AStartRow][vC].RowSpan = 0;
                    FRows[AStartRow][vC].ColSpan = AStartCol - vC;
                }

                for (int vR = AStartRow; vR <= vEndRow; vR++)  // 剩余行各列合
                {
                    for (int vC = AStartCol; vC <= vEndCol; vC++)
                    {
                        if (FRows[vR][vC].CellData != null)
                        {
                            FRows[AStartRow][AStartCol].CellData.AddData(FRows[vR][vC].CellData);
                            FRows[vR][vC].CellData.Dispose();
                            FRows[vR][vC].CellData = null;
                        }

                        FRows[vR][vC].ColSpan = AStartCol - vC;
                        FRows[vR][vC].RowSpan = AStartRow - vR;
                    }
                }

                FRows[AStartRow][AStartCol].RowSpan = vEndRow - AStartRow;
                FRows[AStartRow][AStartCol].ColSpan = vEndCol - AStartCol;
                
                DeleteEmptyRows(AStartRow + 1, vEndRow);
                // 删除空列
                DeleteEmptyCols(AStartCol + 1, vEndCol);
                
                Result = true;
            }

            return Result;
        }

        protected HCTableCell GetCells(int ARow, int ACol)
        {
            return FRows[ARow][ACol];
        }

        protected bool InsertCol(int ACol, int ACount)
        {
            /* TODO : 根据各行当前列平均减少一定的宽度给要插入的列 }*/
            int viDestRow = -1, viDestCol = -1;
            int vWidth = HC.MinColWidth - FBorderWidth;
            for (int i = 0; i <= ACount - 1; i++)
            {
                for (int vRow = 0; vRow <= RowCount - 1; vRow++)
                {
                    HCTableCell vCell = new HCTableCell(OwnerData.Style);
                    vCell.Width = vWidth;
                    
                    if ((ACol < FColWidths.Count) && (FRows[vRow][ACol].ColSpan < 0))
                    {
                        GetDestCell(vRow, ACol, ref viDestRow, ref viDestCol);  // 目标行列
                        
                        // 新插入的列在当前列后面，也做为被合并的列
                        vCell.CellData.Dispose();
                        vCell.CellData = null;
                        vCell.RowSpan = FRows[vRow][ACol].RowSpan;
                        vCell.ColSpan = FRows[vRow][ACol].ColSpan;
                        
                        for (int j = ACol; j <= viDestCol + this[viDestRow, viDestCol].ColSpan; j++)  // 后续列离目标远
                            FRows[vRow][j].ColSpan = FRows[vRow][j].ColSpan - 1;  // 离目标列远1
                        
                        if (vRow == viDestRow + FRows[viDestRow][viDestCol].RowSpan)
                            FRows[viDestRow][viDestCol].ColSpan = FRows[viDestRow][viDestCol].ColSpan + 1;
                    }
                    FRows[vRow].Insert(ACol, vCell);
                }
                
                FColWidths.Insert(ACol, vWidth);  // 右侧插入列
            }
            
            this.InitializeMouseInfo();
            FSelectCellRang.Initialize();
            
            return true;
        }

        protected bool InsertRow(int ARow, int ACount)
        {
            int viDestRow = -1, viDestCol = -1;

            for (int i = 0; i <= ACount - 1; i++)
            {
                HCTableRow vTableRow = new HCTableRow(OwnerData.Style, FColWidths.Count);
                for (int vCol = 0; vCol <= FColWidths.Count - 1; vCol++)
                {
                    vTableRow[vCol].Width = FColWidths[vCol];
                    if ((ARow < FRows.Count) && (FRows[ARow][vCol].RowSpan < 0))
                    {
                        GetDestCell(ARow, vCol, ref viDestRow, ref viDestCol);
                        
                        vTableRow[vCol].CellData.Dispose();
                        vTableRow[vCol].CellData = null;
                        vTableRow[vCol].RowSpan = FRows[ARow][vCol].RowSpan;
                        vTableRow[vCol].ColSpan = FRows[ARow][vCol].ColSpan;
                        
                        for (int j = ARow; j <= viDestRow + this[viDestRow, viDestCol].RowSpan; j++)  // 目标的行跨度 - 已经跨
                            FRows[j][vCol].RowSpan = FRows[j][vCol].RowSpan - 1;  // 离目标行远1
                        
                        if (vCol == viDestCol + this[viDestRow, viDestCol].ColSpan)
                            FRows[viDestRow][viDestCol].RowSpan = FRows[viDestRow][viDestCol].RowSpan + 1;  // 目标行包含的合并源增加1
                    }
                }

                FRows.Insert(ARow, vTableRow);
            }
            this.InitializeMouseInfo();
            FSelectCellRang.Initialize();
            
            return true;
        }

        protected bool DeleteCol(int ACol)
        {
            if (!ColCanDelete(ACol))
                return false;

            int viDestRow = -1, viDestCol = -1;
            for (int vRow = 0; vRow <= RowCount - 1; vRow++)
            {
                if (FRows[vRow][ACol].ColSpan < 0)
                {
                    GetDestCell(vRow, ACol, ref viDestRow, ref viDestCol);  // 目标行、列
                    for (int i = ACol; i <= viDestCol + FRows[viDestRow][viDestCol].ColSpan; i++)  // 当前列右面的合并源列离目标近
                        FRows[vRow][i].ColSpan = FRows[vRow][i].ColSpan + 1;
                    
                    if (vRow == viDestRow + FRows[viDestRow][viDestCol].RowSpan)
                        FRows[viDestRow][viDestCol].ColSpan = FRows[viDestRow][viDestCol].ColSpan - 1;
                }
                else
                if (FRows[vRow][ACol].ColSpan > 0)
                {
                }

                FRows[vRow].RemoveAt(ACol);
            }
            FColWidths.RemoveAt(ACol);

            this.InitializeMouseInfo();
            FSelectCellRang.Initialize();
            
            return true;
        }

        protected bool DeleteRow(int ARow)
        {
            if (!RowCanDelete(ARow))
                return false;

            int viDestRow = -1, viDestCol = -1;
            for (int vCol = 0; vCol <= FColWidths.Count - 1; vCol++)
            {
                if (FRows[ARow][vCol].RowSpan < 0)
                {
                    GetDestCell(ARow, vCol, ref viDestRow, ref viDestCol);  // 目标行、列
                    for (int i = ARow; i <= viDestRow + FRows[viDestRow][viDestCol].RowSpan; i++)  // 当前行下面的合并源行离目标近
                    FRows[i][vCol].RowSpan = FRows[i][vCol].RowSpan + 1;
                    if (vCol == viDestCol + FRows[viDestRow][viDestCol].ColSpan)
                    FRows[viDestRow][viDestCol].RowSpan = FRows[viDestRow][viDestCol].RowSpan - 1;
                }
                else
                if (FRows[ARow][vCol].ColSpan > 0)
                {
                }
            }

            FRows.RemoveAt(ARow);
            this.InitializeMouseInfo();
            FSelectCellRang.Initialize();
            
            return true;
        }

        public HCTableItem(HCCustomData AOwnerData, int ARowCount, int AColCount, int AWidth) 
            : base(AOwnerData)
        {
            if (ARowCount == 0)
                throw new Exception("异常：不能创建行数为0的表格！");
            if (AColCount == 0)
                throw new Exception("异常：不能创建列数为0的表格！");
            
            GripSize = 2;
            FCellHPadding = 2;
            FCellVPadding = 2;
            FDraging = false;
            FEnableUndo = true;
            FBorderWidth = 1;
            FBorderColor = Color.Black;
            FBorderVisible = true;
            
            StyleNo = HCStyle.Table;
            ParaNo = OwnerData.Style.CurParaNo;
            CanPageBreak = true;
            FPageBreaks = new List<PageBreak>();
            
            //FWidth := FRows[0].ColCount * (MinColWidth + FBorderWidth) + FBorderWidth;
            Height = ARowCount * (HC.MinRowHeight + FBorderWidth) + FBorderWidth;
            FRows = new HCTableRows();
            FRows.OnRowAdd = DoRowAdd;  // 添加行时触发的事件
            FSelectCellRang = new View.SelectCellRang();
            this.InitializeMouseInfo();
            //
            int vDataWidth = AWidth - (AColCount + 1) * FBorderWidth;
            for (int i = 0; i <= ARowCount - 1; i++)
            {
                HCTableRow vRow = new HCTableRow(OwnerData.Style, AColCount);
                vRow.SetRowWidth(vDataWidth);
                FRows.Add(vRow);
            }
            FColWidths = new List<int>();
            for (int i = 0; i <= AColCount - 1; i++)
                FColWidths.Add(FRows[0][i].Width);
        }

        ~HCTableItem()
        {
            
        }

        public override void Dispose()
        {
            base.Dispose();
            FSelectCellRang.Dispose();
            //FPageBreaks.Free;
            FRows.Clear();
            //FRows.Free;
            //FColWidths.Free;
        }

        public override void Assign(HCCustomItem Source)
        {
            // 必需保证行、列数量一致
            base.Assign(Source);
            HCTableItem vSrcTable = Source as HCTableItem;
            
            FBorderVisible = vSrcTable.BorderVisible;
            FBorderWidth = vSrcTable.BorderWidth;
            
            for (int vC = 0; vC <= this.ColCount - 1; vC++)
                FColWidths[vC] = vSrcTable.FColWidths[vC];
            
            for (int vR = 0; vR <= this.RowCount - 1; vR++)
            {
                FRows[vR].AutoHeight = vSrcTable.Rows[vR].AutoHeight;
                FRows[vR].Height = vSrcTable.Rows[vR].Height;
            
                for (int vC = 0; vC <= this.ColCount - 1; vC++)
                {
                    this[vR, vC].Width = FColWidths[vC];
                    this[vR, vC].RowSpan = vSrcTable[vR, vC].RowSpan;
                    this[vR, vC].ColSpan = vSrcTable[vR, vC].ColSpan;
                    this[vR, vC].BackgroundColor = vSrcTable[vR, vC].BackgroundColor;
                    this[vR, vC].AlignVert = vSrcTable[vR, vC].AlignVert;
                    this[vR, vC].BorderSides = vSrcTable[vR, vC].BorderSides;
                    
                    if (vSrcTable[vR, vC].CellData != null)
                        this[vR, vC].CellData.AddData(vSrcTable[vR, vC].CellData);
                    else
                    {
                        this[vR, vC].CellData.Dispose();
                        this[vR, vC].CellData = null;
                    }
                }
            }
        }

        /// <summary> 当前位置开始查找指定的内容 </summary>
        /// <param name="AKeyword">要查找的关键字</param>
        /// <param name="AForward">True：向前，False：向后</param>
        /// <param name="AMatchCase">True：区分大小写，False：不区分大小写</param>
        /// <returns>True：找到</returns>
        public override bool Search(string AKeyword, bool AForward, bool AMatchCase)
        {
            bool Result = false;
            int vRow = -1, vCol = -1;
            HCTableCellData vCellData = null;
            if (AForward)
            {
                if (FSelectCellRang.StartRow < 0)
                {
                    FSelectCellRang.StartRow = FRows.Count - 1;
                    FSelectCellRang.StartCol = FColWidths.Count - 1;
                    
                    vRow = FSelectCellRang.StartRow;
                    vCol = FSelectCellRang.StartCol;
                    
                    // 从最后开始
                    if (this[vRow, vCol].CellData != null)
                    {
                        vCellData = this[vRow, vCol].CellData;
                        vCellData.SelectInfo.StartItemNo = vCellData.Items.Count - 1;
                        vCellData.SelectInfo.StartItemOffset = vCellData.GetItemAfterOffset(vCellData.Items.Count - 1);
                    }
                }

                vRow = FSelectCellRang.StartRow;
                vCol = FSelectCellRang.StartCol;
                if ((vRow >= 0) && (vCol >= 0))
                {
                    if (this[vRow, vCol].CellData != null)
                        Result = this[vRow, vCol].CellData.Search(AKeyword, AForward, AMatchCase);
                    
                    if (!Result)
                    {
                        for (int j = vCol; j >= 0; j--)  // 在同行后面的单元格
                        {
                            if ((this[vRow, j].ColSpan < 0) || (this[vRow, j].RowSpan < 0))
                                continue;
                            else
                            {
                                vCellData = this[vRow, j].CellData;
                                vCellData.SelectInfo.StartItemNo = vCellData.Items.Count - 1;
                                vCellData.SelectInfo.StartItemOffset = vCellData.GetItemAfterOffset(vCellData.Items.Count - 1);

                                Result = this[vRow, j].CellData.Search(AKeyword, AForward, AMatchCase);
                            }

                            if (Result)
                            {
                                FSelectCellRang.StartCol = j;
                                break;
                            }
                        }
                    }
                    if (!Result)
                    {
                        for (int i = FSelectCellRang.StartRow; i >= 0; i--)
                        {
                            for (int j = FColWidths.Count; j >= 0; j--)
                            {
                                if ((this[i, j].ColSpan < 0) || (this[i, j].RowSpan < 0))
                                    continue;
                                else
                                {
                                    vCellData = this[i, j].CellData;
                                    vCellData.SelectInfo.StartItemNo = vCellData.Items.Count - 1;
                                    vCellData.SelectInfo.StartItemOffset = vCellData.GetItemAfterOffset(vCellData.Items.Count - 1);
                                
                                    Result = this[i, j].CellData.Search(AKeyword, AForward, AMatchCase);
                                }

                                if (Result)
                                {
                                    FSelectCellRang.StartCol = j;
                                    break;
                                }
                            }

                            if (Result)
                            {
                                FSelectCellRang.StartRow = i;
                                break;
                            }
                        }
                    }
                }
            }
            else  // 向后查找
            {
                if (FSelectCellRang.StartRow < 0)
                {
                    FSelectCellRang.StartRow = 0;
                    FSelectCellRang.StartCol = 0;
                    // 从头开始
                    this[0, 0].CellData.SelectInfo.StartItemNo = 0;
                    this[0, 0].CellData.SelectInfo.StartItemOffset = 0;
                }

                vRow = FSelectCellRang.StartRow;
                vCol = FSelectCellRang.StartCol;
                
                if ((vRow >= 0) && (vCol >= 0))
                {
                    Result = this[vRow, vCol].CellData.Search(AKeyword, AForward, AMatchCase);
                    if (!Result)
                    {
                        for (int j = vCol; j <= FColWidths.Count - 1; j++)  // 在同行后面的单元格
                        {
                            if ((this[vRow, j].ColSpan < 0) || (this[vRow, j].RowSpan < 0))
                                continue;
                            else
                            {
                                this[vRow, j].CellData.SelectInfo.StartItemNo = 0;
                                this[vRow, j].CellData.SelectInfo.StartItemOffset = 0;
                                Result = this[vRow, j].CellData.Search(AKeyword, AForward, AMatchCase);
                        }
                            if (Result)
                            {
                                FSelectCellRang.StartCol = j;
                                break;
                            }
                        }
                    }

                    if (!Result)
                    {
                        for (int i = FSelectCellRang.StartRow; i <= FRows.Count - 1; i++)
                        {
                            for (int j = 0; j <= FColWidths.Count - 1; j++)
                            {
                                if ((this[i, j].ColSpan < 0) || (this[i, j].RowSpan < 0))
                                    continue;
                                else
                                {
                                    this[i, j].CellData.SelectInfo.StartItemNo = 0;
                                    this[i, j].CellData.SelectInfo.StartItemOffset = 0;
                                    Result = this[i, j].CellData.Search(AKeyword, AForward, AMatchCase);
                                }

                                if (Result)
                                {
                                    FSelectCellRang.StartCol = j;
                                    break;
                                }
                            }

                            if (Result)
                            {
                                FSelectCellRang.StartRow = i;
                                break;
                            }
                        }
                    }
                }
            }

            if (!Result)
                FSelectCellRang.Initialize();

            return Result;
        }

        public override void CheckFormatPageBreakBefor()
        {
            FPageBreaks.Clear();
        }

        #region
        private void AddPageBreak(int ARow, int  ABreakSeat, int APageIndex, 
            int APageDataFmtBottom, int ADrawItemRectTop)
        {
            PageBreak vPageBreak = new PageBreak();
            vPageBreak.PageIndex = APageIndex;  // 分页时当前页序号
            vPageBreak.Row = ARow;  // 分页行
            vPageBreak.BreakSeat = ABreakSeat;  // 分页时，此行各列分页位置最大的
            vPageBreak.BreakBottom = APageDataFmtBottom - ADrawItemRectTop;  // 页底部位置
            FPageBreaks.Add(vPageBreak);
        }
        #endregion

        /// <summary> 表格分页 </summary>
        /// <param name="ADrawItemRectTop">表格对应的DrawItem的Rect.Top</param>
        /// <param name="ADrawItemRectTop">表格对应的DrawItem的Rect.Bottom</param>
        /// <param name="APageDataFmtTop">当前页的数据顶部位置</param>
        /// <param name="APageDataFmtBottom">当前页的数据底部位置</param>
        /// <param name="ACheckRow">当前页从哪行开始排版</param>
        /// <param name="ABreakRow">当前页最后分页于哪行</param>
        /// <param name="AFmtOffset">表格对应的DrawItem向下整体偏移的量</param>
        /// <param name="ACellMaxInc">返回当前页各列为了避开分页位置额外偏移的最大高度(参数原名AFmtHeightInc为便于分析重命名)</param>
        public override void CheckFormatPageBreak(int APageIndex, int ADrawItemRectTop,
            int ADrawItemRectBottom, int APageDataFmtTop, int APageDataFmtBottom, int AStartRow,
            ref int ABreakRow, ref int AFmtOffset, ref int ACellMaxInc)
        {
            ABreakRow = -1;
            AFmtOffset = 0;
            ACellMaxInc = 0;  // vCellInc的最大值
            
            /* 得到起始行的Fmt起始位置 }*/
            int vBreakRowFmtTop = ADrawItemRectTop + FBorderWidth - 1;  // 第1行排版位置(上边框线结束位置)，因为边框在ADrawItemRectTop也占1像素，所以要减掉
            for (int i = 0; i <= AStartRow - 1; i++)
                vBreakRowFmtTop = vBreakRowFmtTop + FRows[i].FmtOffset + FRows[i].Height + FBorderWidth;  // 第i行结束位置(含下边框结束位置)
            
            /* 从起始行开始检测当前页是否能放完表格 }*/
            int vR = AStartRow, vBreakRowBottom = 0;
            while (vR < RowCount)  // 遍历每一行
            {
                vBreakRowBottom = vBreakRowFmtTop + FRows[vR].FmtOffset + FRows[vR].Height + FBorderWidth;  // 第i行结束位置(含下边框结束位置)
                if (vBreakRowBottom > APageDataFmtBottom)
                {
                    ABreakRow = vR;  // 第i行需要处理分页
                    break;
                }
                vBreakRowFmtTop = vBreakRowBottom;  // 第i+1行起始位置(上边框结束位置)
                vR++;
            }

            if (ABreakRow < 0)
                return;

            if ((!this.CanPageBreak) && (ABreakRow == 0))
            {
                AFmtOffset = APageDataFmtBottom - ADrawItemRectTop;
                return;
            }

            bool vFirstLinePlace = true;
            int vPageBreakBottom = APageDataFmtBottom;
            
            int vDestRow = -1, vDestCol = -1, vDestCellDataFmtTop = 0;
            HCTableCellData vCellData = null;
            HCCustomDrawItem vDrawItem = null;

            // 先判断是不是有单元格里第一行内容就放不下，需要整体下移
            for (int vC = 0; vC <= FRows[ABreakRow].ColCount - 1; vC++)  // 遍历所有单元格中DrawItem，找从哪个开始向下偏移及偏移
            {
                if (FRows[ABreakRow][vC].ColSpan < 0)
                    continue;
                
                GetMergeDest(ABreakRow, vC, ref vDestRow, ref vDestCol);
                vCellData = FRows[vDestRow][vDestCol].CellData;
                
                // 计算目标单元格数据起始位置
                vDestCellDataFmtTop = vBreakRowFmtTop;  // 先分页行起始位置(上边框结束位置)
                while (vDestRow < ABreakRow)  // 恢复到目标单元格
                {
                    vDestCellDataFmtTop = vDestCellDataFmtTop - FBorderWidth - FRows[vDestRow].Height;
                    vDestRow++;
                }
                vDestCellDataFmtTop = vDestCellDataFmtTop + FCellVPadding;
                //
                // 判断合并目标内容在当前分页行的分页位置
                for (int i = 0; i <= vCellData.DrawItems.Count - 1; i++)
                {
                    vDrawItem = vCellData.DrawItems[i];
                    if (!vDrawItem.LineFirst)
                        continue;

                    if (vDestCellDataFmtTop + vDrawItem.Rect.Bottom + FCellVPadding + FBorderWidth > APageDataFmtBottom)
                    {                                               // |如果FBorderWidth比行高大就不合适
                        if (i == 0)
                        {
                            vFirstLinePlace = false;
                            vPageBreakBottom = vBreakRowFmtTop;
                            break;
                        }
                    }
                }
            
                if (!vFirstLinePlace)
                    break;
            }
      
            // 根据上面计算出来的截断位置(可能是PageData底部也可能是整体下移行底部)
            // 处理内容的偏移，循环原理和上面找是否有整体下移行一样
            int vCellInc = 0;  // 行各内容为避开分页额外增加的格式化高度
            int vRowBreakSeat = 0, vLastDFromRowBottom = 0, vH = 0;

            List<ColCross> vColCrosses = new List<ColCross>();

            for (int vC = 0; vC <= FRows[ABreakRow].ColCount - 1; vC++)  // 遍历所有单元格中DrawItem，找从哪个开始向下偏移及偏移
            {
                if (FRows[ABreakRow][vC].ColSpan < 0)
                    continue;

                GetMergeDest(ABreakRow, vC, ref vDestRow, ref vDestCol);
                vCellData = FRows[vDestRow][vDestCol].CellData;
                vLastDFromRowBottom =  // 原最后一个DrawItem底部距离行底部的空白距离(不含底部的FCellVPadding)
                    FRows[vDestRow][vDestCol].Height - (FCellVPadding + vCellData.Height + FCellVPadding);
                    
                // 计算目标单元格数据起始位置
                vDestCellDataFmtTop = vBreakRowFmtTop;  // 先分页行起始位置(上边框结束位置)
                while (vDestRow < ABreakRow)  // 恢复到目标单元格
                {
                    vDestCellDataFmtTop = vDestCellDataFmtTop - FBorderWidth - FRows[vDestRow].Height;
                    vDestRow++;
                }
                vDestCellDataFmtTop = vDestCellDataFmtTop + FCellVPadding;
                //
                ColCross vColCross = new ColCross();
                vColCross.Col = vC;
                
                // 判断合并目标内容在当前分页行的分页位置
                for (int i = 0; i <= vCellData.DrawItems.Count - 1; i++)
                {
                    vDrawItem = vCellData.DrawItems[i];
                    if (! vDrawItem.LineFirst)
                        continue;
                        
                    if (vDestCellDataFmtTop + vDrawItem.Rect.Bottom + FCellVPadding + FBorderWidth > vPageBreakBottom)
                    {                                    // |如果FBorderWidth比行高大就不合适
                        // 计算分页的DrawItem向下偏移多少可在下一页全显示该DrawItem
                        vH = APageDataFmtBottom - (vDestCellDataFmtTop + vDrawItem.Rect.Top) // 页Data底部 - 当前DrawItem在页的相对位置
                            + FBorderWidth + FCellVPadding - 1;  // 预留出顶部边框和FCellVPadding，因为边框在APageDataFmtBottom也占1像素，所以要减掉
                       
                        // 单元格实际增加的高度 = DrawItem分页向下偏移的距离 - 原最后一个DrawItem底部距离行底部的空白距离(不含底部的FCellVPadding)
                        if (vH > vLastDFromRowBottom)
                            vCellInc = vH - vLastDFromRowBottom;
                        else  // 偏移量让底部空白抵消了
                            vCellInc = 0;
                        
                        vColCross.DrawItemNo = i;  // 从第j个DrawItem处开始分页
                        vColCross.VOffset = vH;  // DrawItem分页偏移，注意，DrawItem向下偏移和单元格增加的高并不一定相等，如原底部有空白时，单元格增加高度<Draw向下偏移
                        
                        if (i > 0)
                        {
                            if (vDestCellDataFmtTop + vCellData.DrawItems[i - 1].Rect.Bottom + FCellVPadding + FBorderWidth > vRowBreakSeat)
                                vRowBreakSeat = vDestCellDataFmtTop + vCellData.DrawItems[i - 1].Rect.Bottom + FCellVPadding + FBorderWidth;
                        }
                        else  // 第一个DrawItem就放不下
                        {
                            if (vDestCellDataFmtTop > vRowBreakSeat)
                                vRowBreakSeat = vDestCellDataFmtTop - FCellVPadding;
                        }
                
                        break;
                    }
                }
            
                if (ACellMaxInc < vCellInc)
                    ACellMaxInc = vCellInc;  // 记录各列中分页向下偏移的最大增量
            
                vColCrosses.Add(vColCross);
            }
            
            vRowBreakSeat = vRowBreakSeat - ADrawItemRectTop + 1;  // 起始为1，截断为2，截断位置是2所以要增加1
           
            if (!vFirstLinePlace)
            {
                if (ABreakRow == 0)
                {
                    AFmtOffset = APageDataFmtBottom - ADrawItemRectTop;
                    ACellMaxInc = 0;  // 整体向下偏移时，就代表了第一行的向下偏移，或者说第一行的FmtOffset永远是0，因为整体向下偏移的依据是判断第一行
                    return;
                }

                // 偏移量增加到此行高度
                for (int i = 0; i <= vColCrosses.Count - 1; i++)  // vColCrosses里只有源或普通单元
                {
                    if ((vColCrosses[i].VOffset > 0) && (vColCrosses[i].DrawItemNo == 0))
                    {
                        FRows[ABreakRow].FmtOffset = vColCrosses[i].VOffset;  // 表格行向下偏移后整行起始在下一页显示，同行多个单元都放不下第一个时会重复赋相同值
                        vColCrosses[i].VOffset = 0;
                    }
                }
            }
            else
                FRows[ABreakRow].Height = FRows[ABreakRow].Height + ACellMaxInc;
            
            AddPageBreak(ABreakRow, vRowBreakSeat, APageIndex, APageDataFmtBottom, ADrawItemRectTop);
            
            for (int vC = 0; vC <= vColCrosses.Count - 1; vC++)  // 遍历所有内容向下有偏移的单元格，将偏移扩散到分页后面的DrawIte
            {
                if (vColCrosses[vC].DrawItemNo < 0)
                    continue;

                GetMergeDest(ABreakRow, vColCrosses[vC].Col, ref vDestRow, ref vDestCol);
                vCellData = FRows[vDestRow][vDestCol].CellData;
                for (int i = vColCrosses[vC].DrawItemNo; i <= vCellData.DrawItems.Count - 1; i++)
                    HC.OffsetRect(ref vCellData.DrawItems[i].Rect, 0, vColCrosses[vC].VOffset);
            }
            
            // 当前行分页的单元格，有的可能是合并源，目标对应的源在此行下面，所以为了使
            // 各个单元格分页增加的偏移量能够传递到对应的结束单元格，从分页行重新格式化
            FormatRowFrom(ABreakRow);
        }

        // 变动是否在分页处
        public override bool ChangeNearPageBreak()
        {
            if (FSelectCellRang.StartRow < 0)
                return false;

            bool Result = false;
            int vSrcRow = -1, vSrcCol = -1;

            GetSourceCell(FSelectCellRang.StartRow, FSelectCellRang.StartCol, ref vSrcRow, ref vSrcCol);
            for (int i = 0; i <= FPageBreaks.Count - 1; i++)
            {
                if ( ((FPageBreaks[i].Row >= FSelectCellRang.StartRow) && (FPageBreaks[i].Row <= vSrcRow))  // 编辑行是分页行或其中某一源行)
                    || (FPageBreaks[i].Row == vSrcRow + 1))  // 编辑行下一行是分页行
                {
                    Result = true;
                    break;
                }
            }

            return Result;
        }

        // 保存和读取
        public override void SaveToStream(Stream AStream, int AStart, int AEnd)
        {
            base.SaveToStream(AStream, AStart, AEnd);

            byte[] vBuffer = BitConverter.GetBytes(FBorderVisible);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FRows.Count);  // 行数
            AStream.Write(vBuffer, 0, vBuffer.Length);
            
            vBuffer = BitConverter.GetBytes(FColWidths.Count);
            AStream.Write(vBuffer, 0, vBuffer.Length);  // 列数
            
            for (int i = 0; i <= FColWidths.Count - 1; i++)  // 各列标准宽
            {
                vBuffer = BitConverter.GetBytes(FColWidths[i]);
                AStream.Write(vBuffer, 0, vBuffer.Length);
            }

            for (int vR = 0; vR <= FRows.Count - 1; vR++)  // 各行数
            {
                vBuffer = BitConverter.GetBytes(FRows[vR].AutoHeight);
                AStream.Write(vBuffer, 0, vBuffer.Length);

                if (!FRows[vR].AutoHeight)
                {
                    vBuffer = BitConverter.GetBytes(FRows[vR].Height);
                    AStream.Write(vBuffer, 0, vBuffer.Length);
                }
                
                for (int vC = 0; vC <= FRows[vR].ColCount - 1; vC++)  // 各列数
                    FRows[vR][vC].SaveToStream(AStream);
            }
        }

        public override void SaveSelectToStream(Stream AStream)
        {
            if (this.IsSelectComplate)
                throw new Exception("保存选中内容出错，表格不应该由内部处理全选中的保存！");
            else
            {
                HCCustomData vCellData = GetActiveData();
                if (vCellData != null)
                    vCellData.SaveSelectToStream(AStream);
            }
        }

        public override void LoadFromStream(Stream AStream, HCStyle AStyle, ushort AFileVersion)
        {
            FRows.Clear();
            base.LoadFromStream(AStream, AStyle, AFileVersion);
            
            byte[] vBuffer = BitConverter.GetBytes(FBorderVisible);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            FBorderVisible = BitConverter.ToBoolean(vBuffer, 0);
            
            int vRowCount = 0;
            vBuffer = BitConverter.GetBytes(vRowCount);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            vRowCount = BitConverter.ToInt32(vBuffer, 0);  // 行数

            int vColCount = 0;
            vBuffer = BitConverter.GetBytes(vColCount);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            vColCount = BitConverter.ToInt32(vBuffer, 0);  // 列数

            /* 创建行、列 }*/
            for (int i = 0; i <= vRowCount - 1; i++)
            {
                HCTableRow vRow = new HCTableRow(OwnerData.Style, vColCount);  // 注意行创建时是table拥有者的Style，加载时是传入的AStyle
                FRows.Add(vRow);
            }

            /* 加载各列标准宽度 }*/
            FColWidths.Clear();

            int vWidth = HC.MinColWidth;
            vBuffer = BitConverter.GetBytes(vWidth);
            for (int i = 0; i <= vColCount - 1; i++)
            {
                AStream.Read(vBuffer, 0, vBuffer.Length);
                vWidth = BitConverter.ToInt32(vBuffer, 0);

                FColWidths.Add(vWidth);
            }

            /* 加载各列数据 }*/
            bool vAutoHeight = false;
            byte[] vBytes = BitConverter.GetBytes(vWidth);

            vBuffer = BitConverter.GetBytes(vAutoHeight);
            for (int vR = 0; vR <= FRows.Count - 1; vR++)
            {
                AStream.Read(vBuffer, 0, vBuffer.Length);
                vAutoHeight = BitConverter.ToBoolean(vBuffer, 0);
                FRows[vR].AutoHeight = vAutoHeight;
                if (!FRows[vR].AutoHeight)
                {
                    AStream.Read(vBytes, 0, vBytes.Length);
                    vWidth = BitConverter.ToInt32(vBytes, 0);
                    FRows[vR].Height = vWidth;
                }
                for (int vC = 0; vC <= FRows[vR].ColCount - 1; vC++)
                {
                    FRows[vR][vC].CellData.Width = FColWidths[vC] - 2 * FCellHPadding;
                    FRows[vR][vC].LoadFromStream(AStream, AStyle, AFileVersion);
                }
            }
        }

        #region
        private bool CheckRowBorderRang(int Y, int ABottom)
        {
            return ((Y >= ABottom - GripSize) && (Y <= ABottom + GripSize));
        }

        private bool CheckColBorderRang(int X, int ALeft)
        {
            return ((X >= ALeft - GripSize) && (X <= ALeft + GripSize));
        }
        #endregion

        /// <summary> 获取指定位置处的行、列(如果是被合并单元格则返回目标单元格行、列) </summary>
        /// <param name="X">横坐标</param>
        /// <param name="Y">纵坐标</param>
        /// <param name="ARow">坐标处的行</param>
        /// <param name="ACol">坐标处的列</param>
        ///  <param name="AReDest">如果坐标是合并源，返回目标</param>
        /// <returns></returns>
        public ResizeInfo GetCellAt(int X, int Y, ref int ARow, ref int ACol, bool AReDest = true)
        {
            ResizeInfo Result = new ResizeInfo();
            Result.TableSite = TableSite.tsOutside;
            Result.DestX = -1;
            Result.DestY = -1;
            
            ARow = -1;
            ACol = -1;
            if ((Y < 0) || (Y > Height))
                return Result;

            int vTop = 0, vBottom =0;

            if ((X < 0) || (X > Width))
            {
                vTop = FBorderWidth;
                for (int i = 0; i <= RowCount - 1; i++)
                {
                    vTop = vTop + FRows[i].FmtOffset;  // 以实际内容Top为顶位置，避免行有跨页时，在上一页底部点击选中的是下一页第一行
                    vBottom = vTop + FRows[i].Height + FBorderWidth;
                    if ((vTop < Y) && (vBottom > Y))
                    {
                        ARow = i;
                        break;
                    }
                    vTop = vBottom;
                }
                
                return Result;
            }
            
            /* 获取是否在行或列的边框上 }*/
            // 判断是否在最上边框
            vTop = FBorderWidth;
            if (CheckRowBorderRang(Y, vTop))
            {
                Result.TableSite = TableSite.tsBorderTop;
                return Result;
            }
            // 判断是否在最左边框
            if (CheckColBorderRang(X, vTop))
            {
                Result.TableSite = TableSite.tsBorderLeft;
                return Result;
            }
            
            // 判断是在行边框上还是行中
            for (int i = 0; i <= RowCount - 1; i++)
            {
                vTop = vTop + FRows[i].FmtOffset;  // 以实际内容Top为顶位置，避免行有跨页时，在上一页底部点击选中的是下一页第一行
                vBottom = vTop + FRows[i].Height + FBorderWidth;
                if (CheckRowBorderRang(Y, vBottom))
                {
                    ARow = i;
                    Result.TableSite = TableSite.tsBorderBottom;
                    Result.DestY = vBottom;
                    break;  // 为处理跨单元格划选时，划到下边框时ACol<0造成中间选中的也被忽略掉的问题，不能像下面列找不到时Exit
                }
                if ((vTop < Y) && (vBottom > Y))
                {
                    ARow = i;
                    break;
                }
                
                vTop = vBottom;
            }

            if (ARow < 0)
                return Result;

            // 判断是在列边框上还是列中
            int vLeft = FBorderWidth, vRight = 0, vDestRow = -1, vDestCol = -1;

            for (int i = 0; i <= FColWidths.Count - 1; i++)
            {
                vRight = vLeft + FColWidths[i] + FBorderWidth;
                GetDestCell(ARow, i, ref vDestRow, ref vDestCol);
                if (CheckColBorderRang(X, vRight))
                {
                    ACol = i;
                    if (vDestCol + this[vDestRow, vDestCol].ColSpan != i)
                        Result.TableSite = TableSite.tsCell;
                    else
                        Result.TableSite = TableSite.tsBorderRight;
                    
                    Result.DestX = vRight;
                    
                    break;
                }

                if ((vLeft < X) && (vRight > X))
                {
                    ACol = i;
                    if ((Result.TableSite == TableSite.tsBorderBottom)
                        && (vDestRow + this[vDestRow, vDestCol].RowSpan != ARow))
                        Result.TableSite = TableSite.tsCell;
                    
                    break;
                }
                
                vLeft = vRight;
            }
            
            if (ACol >= 0)
            {
                if (Result.TableSite == TableSite.tsOutside)
                    Result.TableSite = TableSite.tsCell;
                
                if (AReDest && (this[ARow, ACol].CellData == null))
                    GetDestCell(ARow, ACol, ref ARow, ref ACol);
            }

            return Result;
        }

        public void GetDestCell(int ARow, int ACol, ref int ADestRow, ref int ADestCol)
        {
            if (this[ARow, ACol].CellData != null)
            {
                ADestRow = ARow;
                ADestCol = ACol;
            }
            else
            {
                ADestRow = ARow + this[ARow, ACol].RowSpan;
                ADestCol = ACol + this[ARow, ACol].ColSpan;
            }
        }

        public void GetSourceCell(int ARow, int ACol, ref int ASrcRow, ref int ASrcCol)
        {
             if (this[ARow, ACol].CellData != null)
            {
                ASrcRow = ARow + FRows[ARow][ACol].RowSpan;
                ASrcCol = ACol + FRows[ARow][ACol].ColSpan;
            }
            else  // 源单元格不能获取源单元格
                throw new Exception(HC.HCS_EXCEPTION_VOIDSOURCECELL);
        }

        public void SelectAll()
        {
            SelectComplate();
        }

        /// <summary> 判断指定范围内的单元格是否可以合并(为了给界面合并菜单控制可用状态放到public域中) </summary>
        /// <param name="AStartRow"></param>
        /// <param name="AStartCol"></param>
        /// <param name="AEndRow"></param>
        /// <param name="AEndCol"></param>
        /// <returns></returns>
        public bool CellsCanMerge(int AStartRow, int AStartCol, int AEndRow, int AEndCol)
        {
            bool Result = false;
            for (int vR = AStartRow; vR <= AEndRow; vR++)
            {
                for (int vC = AStartCol; vC <= AEndCol; vC++)
                {
                    if (FRows[vR][vC].CellData != null)
                    {
                        if (!FRows[vR][vC].CellData.CellSelectedAll)
                            return Result;
                    }
                }
            }
            
            Result = true;

            return Result;
        }

        /// <summary> 指定行是否能删除 </summary>
        public bool RowCanDelete(int ARow)
        {
            bool Result = false;
            for (int vCol = 0; vCol <= FColWidths.Count - 1; vCol++)
            {
                if (FRows[ARow][vCol].RowSpan > 0)
                    return Result;
            }
            Result = true;

            return Result;
        }

        public bool CurRowCanDelete()
        {
            return (FSelectCellRang.EndRow < 0)
                && (FSelectCellRang.StartRow >= 0)
                && RowCanDelete(FSelectCellRang.StartRow);
        }

        /// <summary> 指定列是否能删除 </summary>
        public bool ColCanDelete(int ACol)
        {
            bool Result = false;
            for (int vRow = 0; vRow <= RowCount - 1; vRow++)
            {
                if (FRows[vRow][ACol].ColSpan > 0)
                    return Result;
            }
            Result = true;

            return Result;
        }

        public bool CurColCanDelete()
        {
            return (FSelectCellRang.EndCol < 0)
                && (FSelectCellRang.StartCol >= 0)
                && ColCanDelete(FSelectCellRang.StartCol);
        }

        /// <summary> 获取指定单元格合并后的单元格 </summary>
        public void GetMergeDest(int ARow, int ACol, ref int ADestRow, ref int ADestCol)
        {
            ADestRow = ARow;
            ADestCol = ACol;

            if (this[ARow, ACol].RowSpan < 0)
                ADestRow = ADestRow + this[ARow, ACol].RowSpan;
            
            if (this[ARow, ACol].ColSpan < 0)
                ADestCol = ADestCol + this[ARow, ACol].ColSpan;
        }

        /// <summary> 获取指定单元格合并后单元格的Data </summary>
        //function GetMergeDestCellData(const ARow, ACol: Integer): THCTableCellData;
        public bool MergeSelectCells()
        {
            bool Result = false;

            if ((FSelectCellRang.StartRow >= 0) && (FSelectCellRang.EndRow >= 0))
            {
                Undo_MergeCells();

                Result = MergeCells(FSelectCellRang.StartRow, FSelectCellRang.StartCol,
                    FSelectCellRang.EndRow, FSelectCellRang.EndCol);
                if (Result)
                {
                    /* 防止合并后有空行或空列被删除后，DisSelect访问越界，所以合并后直接赋值结束信息 }*/
                    //this.InitializeMouseInfo;  // 合并后不保留选中单元格
                    FSelectCellRang.EndRow = -1;
                    FSelectCellRang.EndCol = -1;
                    this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData.InitializeField();
                    DisSelect();
                }
            }
            else
                if (FSelectCellRang.EditCell())
                    Result = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData.MergeTableSelectCells();
                else
                    Result = false;

            return Result;
        }

        public bool SelectedCellCanMerge()
        {
            bool Result = false;
            if (FSelectCellRang.SelectExists())
            {
                int vEndRow = FSelectCellRang.EndRow;
                int vEndCol = FSelectCellRang.EndCol;
                AdjustCellRange(FSelectCellRang.StartRow, FSelectCellRang.StartCol, ref vEndRow, ref vEndCol);
                Result = CellsCanMerge(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEndRow, vEndCol);
            }

            return Result;
        }

        public HCTableCell GetEditCell()
        {
            if (FSelectCellRang.EditCell())
                return this[FSelectCellRang.StartRow, FSelectCellRang.StartCol];
            else
                return null;
        }

        public void GetEditCell(ref int ARow, int ACol)
        {
            ARow = -1;
            ACol = -1;
            if (FSelectCellRang.EditCell())
            {
                ARow = FSelectCellRang.StartRow;
                ACol = FSelectCellRang.StartCol;
            }
        }

        public bool InsertRowAfter(int ACount)
        {
            HCTableCell vCell = GetEditCell();
            if (vCell == null)
                return false;

            vCell.CellData.InitializeField();
            if (vCell.RowSpan > 0)
                return InsertRow(FSelectCellRang.StartRow + vCell.RowSpan + 1, ACount);
            else
                return InsertRow(FSelectCellRang.StartRow + 1, ACount);
        }

        public bool InsertRowBefor(int ACount)
        {
            HCTableCell vCell = GetEditCell();
            if (vCell == null)
                return false;
            
            vCell.CellData.InitializeField();
            return InsertRow(FSelectCellRang.StartRow, ACount);
        }

        public bool InsertColAfter(int ACount)
        {
            HCTableCell vCell = GetEditCell();
            if (vCell == null)
                return false;

            vCell.CellData.InitializeField();
            if (vCell.ColSpan > 0)
                return InsertCol(FSelectCellRang.StartCol + vCell.ColSpan + 1, ACount);
            else
                return InsertCol(FSelectCellRang.StartCol + 1, ACount);
        }

        public bool InsertColBefor(int ACount)
        {
            HCTableCell vCell = GetEditCell();
            if (vCell == null)
                return false;

            vCell.CellData.InitializeField();
            return InsertCol(FSelectCellRang.StartCol, ACount);
        }

        public bool DeleteCurCol()
        {
            HCTableCell vCell = GetEditCell();
            if (vCell == null)
                return false;

            vCell.CellData.InitializeField();
            if (FColWidths.Count > 1)
                return DeleteCol(FSelectCellRang.StartCol);
            else
                return false;
        }

        public bool DeleteCurRow()
        {
            HCTableCell vCell = GetEditCell();
            if (vCell == null)
                return false;

            vCell.CellData.InitializeField();
            if (FRows.Count > 1)
                return DeleteRow(FSelectCellRang.StartRow);
            else
                return false;
        }

        public bool SplitCurRow()
        {
            // 借用 vTopCell 变量
            HCTableCell vTopCell = GetEditCell();
            if (vTopCell == null)
                return false;

            vTopCell.CellData.InitializeField();
            
            int vCurRow = FSelectCellRang.StartRow;
            int vCurCol = FSelectCellRang.StartCol;
            int vSrcRow = -1, vSrcCol = -1, vDestRow = -1, vDestCol = -1;
            
            // 拆分时，光标所单元格RowSpan>=0，ColSpan>=0
            if (this[vCurRow, vCurCol].RowSpan > 0)
            {
                GetSourceCell(vCurRow, vCurCol, ref vSrcRow, ref vSrcCol);  // 得到范围
                
                this[vCurRow, vCurCol].RowSpan = 0;  // 目标不再向下合并单元格了
                for (int i = vCurRow; i <= vSrcRow; i++)  // 从目标行下一行开始，重新设置合并目
                {
                    for (int vC = vCurCol; vC <= vSrcCol; vC++)  // 遍历拆分前光标所在的行各
                        this[i, vC].RowSpan = this[i, vC].RowSpan + 1;
                }
                
                // 原合并目标单元格正下面的单元格作为拆分后，下面合并源的新目标
                this[vCurRow + 1, vCurCol].CellData = new HCTableCellData(OwnerData.Style);
                this[vCurRow + 1, vCurCol].RowSpan = vSrcRow - (vCurRow + 1);
                this[vCurRow + 1, vCurCol].ColSpan = vSrcCol - vCurCol;
            }
            else  // Cells[vCurRow, vCurCol].RowSpan = 0 拆分时光标所在单元格是普通单元格
            if (InsertRow(vCurRow + 1, 1))
            {
                int vC = 0;
                while (vC < this.ColCount)
                {
                    vTopCell = this[vCurRow, vC];
                    
                    if (vC == vCurCol)
                    {
                        if (vTopCell.ColSpan > 0)
                        {
                            vSrcCol = vCurCol + vTopCell.ColSpan;
                            while (vC <= vSrcCol)
                            {
                                this[vCurRow + 1, vC].ColSpan = this[vCurRow, vC].ColSpan;
                                if (this[vCurRow + 1, vC].ColSpan < 0)
                                {
                                    this[vCurRow + 1, vC].CellData.Dispose();
                                    this[vCurRow + 1, vC].CellData = null;
                                }
                                
                                vC++;
                            }
                        }
                        else  // vLeftCell.ColSpan < 0 的在ColSpan > 0 里处理了，vLeftCell.ColSpan = 0 不需要处理
                            vC++;
                    }
                    else  // vC <> vCurCol
                    {
                        if (vTopCell.ColSpan == 0)
                        {
                            if (vTopCell.RowSpan == 0)
                            {
                                this[vCurRow + 1, vC].CellData.Dispose();
                                this[vCurRow + 1, vC].CellData = null;
                                this[vCurRow + 1, vC].RowSpan = -1;
                                vTopCell.RowSpan = 1;
                                vC++;
                            }
                            else
                            if (vTopCell.RowSpan < 0)
                            {
                                vDestRow = vCurRow + vTopCell.RowSpan;  // 目标行
                                vSrcRow = vDestRow + this[vDestRow, vC].RowSpan;
                                if (vCurRow == vSrcRow)
                                {
                                    this[vCurRow + 1, vC].CellData.Dispose();
                                    this[vCurRow + 1, vC].CellData = null;
                                    this[vCurRow + 1, vC].RowSpan = vTopCell.RowSpan - 1;
                                    this[vDestRow, vC].RowSpan = this[vDestRow, vC].RowSpan + 1;
                                }
                                
                                vC++;
                            }
                            else  // vTopCell.RowSpan > 0 上面是同行合并目标，由上面插入行处理了插入行的合并
                                vC++;
                        }
                        else
                        if (vTopCell.ColSpan > 0)
                        {
                            if (vTopCell.RowSpan == 0)
                            {
                                vTopCell.RowSpan = 1;
                                vDestCol = vC;
                                vSrcCol = vC + vTopCell.ColSpan;
                                
                                while (vC <= vSrcCol)
                                {
                                    this[vCurRow + 1, vC].CellData.Dispose();
                                    this[vCurRow + 1, vC].CellData = null;
                                    this[vCurRow + 1, vC].ColSpan = vDestCol - vC;
                                    this[vCurRow + 1, vC].RowSpan = -1;
                                    vC++;
                                }
                            }
                            else  // 合并目标不可能 vTopCell.RowSpan < 0，vTopCell.RowSpan > 0由上面插入行处理了合并
                                vC++;
                        }
                        else  // vLeftCell.ColSpan < 0 的情况，由目标单元格在vLeftCell.ColSpan > 0中处理了
                            vC++;
                    }
                }
            }
            
            return true;
        }

        public bool SplitCurCol()
        {
            // 借用 vLeftCell 变量
            HCTableCell vLeftCell = GetEditCell();
            if (vLeftCell == null)
                return false;

            vLeftCell.CellData.InitializeField();
            int vCurRow = FSelectCellRang.StartRow;
            int vCurCol = FSelectCellRang.StartCol;
            int vSrcRow = -1, vSrcCol = -1, vDestRow = -1, vDestCol = -1;
            
            // 拆分时，光标所单元格RowSpan>=0，ColSpan>=0
            if (this[vCurRow, vCurCol].ColSpan > 0)
            {
                GetSourceCell(vCurRow, vCurCol, ref vSrcRow, ref vSrcCol);  // 得到范围
                
                this[vCurRow, vCurCol].ColSpan = 0;  // 合并目标不再向右合并单元格了
                for (int i = vCurCol; i <= vSrcCol; i++)  // 目标列同行右侧的重新设置合并目
                {
                    for (int vR = vCurRow; vR <= vSrcRow; vR++)  // 遍历拆分前光标所在的行各
                        this[vR, i].ColSpan = this[vR, i].ColSpan + 1;
                }

                // 原合并目标单元格右侧的单元格作为拆分后，右侧合并源的新目标
                this[vCurRow, vCurCol + 1].CellData = new HCTableCellData(OwnerData.Style);
                this[vCurRow, vCurCol + 1].RowSpan = vSrcRow - vCurRow;
                this[vCurRow, vCurCol + 1].ColSpan = vSrcCol - (vCurCol + 1);
            }
            else  // Cells[vCurRow, vCurCol].ColSpan = 0 拆分时光标所在单元格是普通单元格
            if (InsertCol(vCurCol + 1, 1))
            {
                int vR = 0;
                while (vR < this.RowCount)
                {
                    vLeftCell = this[vR, vCurCol];
                    
                    if (vR == vCurRow)
                    {
                        if (vLeftCell.RowSpan > 0)
                        {
                            vSrcRow = vCurRow + vLeftCell.RowSpan;
                            while (vR <= vSrcRow)
                            {
                                this[vR, vCurCol + 1].RowSpan = this[vR, vCurCol].RowSpan;
                                if (this[vR, vCurCol + 1].RowSpan < 0)
                                {
                                    this[vR, vCurCol + 1].CellData.Dispose();
                                    this[vR, vCurCol + 1].CellData = null;
                                }
                                
                                vR++;
                            }
                        }
                        else  // vLeftCell.RowSpan < 0 的在RowSpan > 0 里处理了，vLeftCell.RowSpan = 0 不需要处理
                            vR++;
                    }
                    else  // vR <> vCurRow
                    {
                        if (vLeftCell.RowSpan == 0)
                        {
                            if (vLeftCell.ColSpan == 0)
                            {
                                this[vR, vCurCol + 1].CellData.Dispose();
                                this[vR, vCurCol + 1].CellData = null;
                                this[vR, vCurCol + 1].ColSpan = -1;
                                vLeftCell.ColSpan = 1;
                                vR++;
                            }
                            else
                            if (vLeftCell.ColSpan < 0)
                            {
                                vDestCol = vCurCol + vLeftCell.ColSpan;  // 目标列
                                vSrcCol = vDestCol + this[vR, vDestCol].ColSpan;
                                if (vCurCol == vSrcCol)
                                {
                                    this[vR, vCurCol + 1].CellData.Dispose();
                                    this[vR, vCurCol + 1].CellData = null;
                                    this[vR, vCurCol + 1].ColSpan = vLeftCell.ColSpan - 1;
                                    this[vR, vDestCol].ColSpan = this[vR, vDestCol].ColSpan + 1;
                                }
                                
                                vR++;
                            }
                            else  // vLeftCell.ColSpan > 0 左侧是同行合并目标，由右侧插入列处理了插入列的合并
                                vR++;
                        }
                        else
                        if (vLeftCell.RowSpan > 0)
                        {
                            if (vLeftCell.ColSpan == 0)
                            {
                                vLeftCell.ColSpan = 1;
                                vDestRow = vR;
                                vSrcRow = vR + vLeftCell.RowSpan;
                                
                                while (vR <= vSrcRow)
                                {
                                    this[vR, vCurCol + 1].CellData.Dispose();
                                    this[vR, vCurCol + 1].CellData = null;
                                    this[vR, vCurCol + 1].RowSpan = vDestRow - vR;
                                    this[vR, vCurCol + 1].ColSpan = -1;
                                    vR++;
                                }
                            }
                            else  // 合并目标不可能 vLeftCell.ColSpan < 0，vLeftCell.ColSpan > 0由右侧插入列处理了合并
                                vR++;
                        }
                        else  // vLeftCell.RowSpan < 0 的情况，由目标单元格在vLeftCell.RowSpan > 0中处理了
                            vR++;
                    }
                }
            }
            
            return true;
        }

        public HCTableCell this[int ARow, int ACol]
        {
            get { return GetCells(ARow, ACol); }
        }

        public HCTableRows Rows
        {
            get { return FRows; }
        }

        public int RowCount
        {
            get { return GetRowCount(); }
        }

        public int ColCount
        {
            get { return GetColCount(); }
        }

        public SelectCellRang SelectCellRang
        {
            get { return FSelectCellRang; }
        }

        public bool BorderVisible
        {
            get { return FBorderVisible; }
            set { FBorderVisible = value; }
        }

        public byte BorderWidth
        {
            get { return FBorderWidth; }
            set { SetBorderWidth(value); }
        }

        public byte CellHPadding
        {
            get { return FCellHPadding; }
            set { FCellHPadding = value; }
        }

        public byte CellVPadding
        {
            get { return FCellVPadding; }
            set { SetCellVPadding(value); }
        }
    }
}
