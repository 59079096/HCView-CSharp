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
using System.Xml;

namespace HC.View
{
    public class PageBreak  // 分页信息
    {
        public int
            /// <summary> 在此页结尾分页 </summary>
            PageIndex,
            Row,  // 分页行
            BreakSeat,  // 分页时，此行各列分页位置最大的
            BreakBottom;  // 页底部位置
    }

    public delegate void RowAddEventHandler(HCTableRow aRow);

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

    /// <summary> 行跨页信息 </summary>
    class ColCross : Object
    {
        public int Col;  // 单元格所在的列
        public int DrawItemNo;  // 跨页的DrawItem
        public int VDrawOffset;  // 跨页偏移

        public ColCross()
        {
            Col = -1;
            DrawItemNo = -1;
            VDrawOffset = 0;
        }
    }

    public delegate void HCCellPaintEventHandle(object sender, HCTableCell aCell, RECT aRect, HCCanvas aCanvas, PaintInfo aPaintInfo,
        ref bool aDrawDefault);

    public delegate void HCCellPaintDataEventHandle(object sender, RECT aTableRect, RECT aCellRect, int aRow, int aCol, int aCellDataDrawTop, 
        int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo);

    public class HCTableItem : HCResizeRectItem
    {
        private byte
            FBorderWidth,   // 边框宽度(要求不大于最小行高，否则分页计算会有问题)
            FCellHPadding,  // 单元格内容水平偏移
            FCellVPadding,  // 单元格内容垂直偏移(不能大于最低的DrawItem高度，否则会影响跨页)
            FFixRowCount,   // 固定行数量
            FFixColCount;   // 固定列数量

        sbyte FFixCol,  // 从哪列开始固定列
              FFixRow;  // 从哪行开始固定行

        private OutsideInfo FOutsideInfo;  // 点击在表格左右边时对应的行信息

        private int
            FMouseDownRow, FMouseDownCol,
            FMouseMoveRow, FMouseMoveCol,
            FMouseDownX, FMouseDownY,
            FFormatHeight;

        private ResizeInfo FResizeInfo;

        private bool FBorderVisible, FMouseLBDowning, FSelecting, FDraging,
            FOutSelectInto, FLastChangeFormated;  // 最后变动已经格式化完了

        private SelectCellRang FSelectCellRang;
        private Color FBorderColor;  // 边框颜色
        private HCTableRows FRows;  // 行
        private List<int> FColWidths;  // 记录各列宽度(除边框、含FCellHPadding * 2)，方便有合并的单元格获取自己水平开始处的位置
        private List<PageBreak> FPageBreaks;  // 记录各行分页时的信息
        private HCCellPaintEventHandle FOnCellPaintBK = null;
        private HCCellPaintDataEventHandle FOnCellPaintData = null;

        private void InitializeMouseInfo()
        {
            FMouseDownRow = -1;
            FMouseDownCol = -1;
            FMouseMoveRow = -1;
            FMouseMoveCol = -1;
            FMouseLBDowning = false;
        }

        private void InitializeCellData(HCTableCellData aCellData)
        {
            aCellData.OnInsertItem = (OwnerData as HCViewData).OnInsertItem;
            aCellData.OnRemoveItem = (OwnerData as HCViewData).OnRemoveItem;
            aCellData.OnItemMouseUp = (OwnerData as HCViewData).OnItemMouseUp;

            aCellData.OnCreateItemByStyle = (OwnerData as HCViewData).OnCreateItemByStyle;      
            aCellData.OnDrawItemPaintBefor = (OwnerData as HCRichData).OnDrawItemPaintBefor;
            aCellData.OnDrawItemPaintAfter = (OwnerData as HCViewData).OnDrawItemPaintAfter;

            aCellData.OnInsertAnnotate = (OwnerData as HCViewData).OnInsertAnnotate;
            aCellData.OnRemoveAnnotate = (OwnerData as HCViewData).OnRemoveAnnotate;
            aCellData.OnDrawItemAnnotate = (OwnerData as HCViewData).OnDrawItemAnnotate;

            aCellData.OnCanEdit = (OwnerData as HCViewData).OnCanEdit;
            aCellData.OnItemResized = (OwnerData as HCRichData).OnItemResized;
            aCellData.OnCurParaNoChange = (OwnerData as HCRichData).OnCurParaNoChange;

            aCellData.OnCreateItem = (OwnerData as HCRichData).OnCreateItem;
            aCellData.OnGetUndoList = this.GetSelfUndoList;
            aCellData.OnGetRootData = DoCellDataGetRootData;
        }

        private HCCustomData DoCellDataGetRootData()
        {
            return OwnerData.GetRootData();
        }

        /// <summary> 表格行有添加时 </summary>
        private void DoRowAdd(HCTableRow aRow)
        {
            HCTableCellData vCellData = null;

            for (int i = 0; i <= aRow.ColCount - 1; i++)
            {
                vCellData = aRow[i].CellData;
                if (vCellData != null)
                    InitializeCellData(vCellData);
            }
        }

        private void CellChangeByAction(int aRow, int aCol, HCProcedure aProcedure)
        {
            this.SizeChanged = false;
            aProcedure();
            if (!this.SizeChanged)
                this.SizeChanged = FRows[aRow][aCol].CellData.FormatHeightChange;

            FLastChangeFormated = !this.SizeChanged;
        }

        /// <summary> 获取当前表格格式化高度 </summary>
        /// <returns></returns>
        private int GetFormatHeight()
        {
            FFormatHeight = FBorderWidth;
            for (int i = 0; i <= RowCount - 1; i++)
                FFormatHeight = FFormatHeight + FRows[i].Height + FBorderWidth;

            return FFormatHeight;
        }

        // 获取行中最高单元格高度
        private void CalcRowCellHeight(int aRow)
        {
            int vNorHeightMax = 0;  // 行中未发生合并的最高单元格
            for (int vC = 0; vC <= FRows[aRow].ColCount - 1; vC++)  // 得到行中未发生合并Data内容最高的单元格高
            {
                if ((FRows[aRow][vC].CellData != null)  // 不是被合并的单元格)
                    && (FRows[aRow][vC].RowSpan == 0))  // 不是行合并的行单元格
                    vNorHeightMax = Math.Max(vNorHeightMax, FRows[aRow][vC].CellData.Height);
            }

            vNorHeightMax = FCellVPadding + vNorHeightMax + FCellVPadding;  // 增加上下边距

            if (FRows[aRow].AutoHeight)
                FRows[aRow].Height = vNorHeightMax;
            else  // 拖动改变了高度
            {
                if (vNorHeightMax > FRows[aRow].Height)
                {
                    FRows[aRow].AutoHeight = true;
                    FRows[aRow].Height = vNorHeightMax;
                }
            }
        }

        // 计算有合并的单元格高度影响到的行高度
        private void CalcMergeRowHeightFrom(int aRow)
        {
            int vDestRow = -1, vDestCol = -1, vExtraHeight = 0, vH = 0, vDestRow2 = -1, vDestCol2 = -1;

            // 为兼容分页时重新格式化调用此方法，所以有对FmtOffset的处理，否则不需要管FmtOffset
            for (int vR = aRow; vR <= RowCount - 1; vR++)  // 计算有行合并情况下各行的高
            {
                for (int vC = 0; vC <= FRows[vR].ColCount - 1; vC++)
                {
                    if (FRows[vR][vC].CellData == null)
                    {
                        if (FRows[vR][vC].ColSpan < 0)
                            continue;

                        GetDestCell(vR, vC, ref vDestRow, ref vDestCol);  // 获取到合并目标单元格所在行号

                        if (vDestRow + FRows[vDestRow][vC].RowSpan == vR)
                        {
                            vExtraHeight = FCellVPadding + FRows[vDestRow][vC].CellData.Height + FCellVPadding;  // 目标单元格除上下边框后的高度
                            FRows[vDestRow][vC].Height = vExtraHeight;  // 目标单元格除上下边框后的高度
                            vExtraHeight = vExtraHeight - FRows[vDestRow].Height - FBorderWidth;  // “消减”自己所在行

                            for (int i = vDestRow + 1; i <= vR - 1; i++)  // 从目标下一行到此，经过各行后“消减”掉多
                                vExtraHeight = vExtraHeight - FRows[i].FmtOffset - FRows[i].Height - FBorderWidth;
                            
                            if (vExtraHeight > FRows[vR].FmtOffset + FRows[vR].Height)
                            {
                                vH = vExtraHeight - FRows[vR].FmtOffset - FRows[vR].Height;  // 高多少
                                FRows[vR].Height = vExtraHeight - FRows[vR].FmtOffset;  // 当前行高赋值新值(内部各单元格高度会处理)

                                for (int i = 0; i <= FRows[vR].ColCount - 1; i++)  // 当前行中源列要影响目标单元
                                {
                                    if (FRows[vR][i].CellData == null)
                                    {
                                        GetDestCell(vR, i, ref vDestRow2, ref vDestCol2);  // 获取目标单元格
                                        if ((vDestRow2 != vDestRow) && (vDestCol2 != vDestCol))
                                            FRows[vDestRow2][i].Height = FRows[vDestRow2][i].Height + vH;
                                    }
                                }
                            }
                            else  // 消减后剩余的没有当前行高，高度增加到当前行底部，处理非合并的单元格内容，大于合并结束到此行但数据底部没有此行高的情况
                            {
                                FRows[vDestRow][vC].Height =  // 2017-1-15_1.bmp中[1, 1]输入c时[1, 0]和[1, 2]的情况
                                    FRows[vDestRow][vC].Height + FRows[vR].FmtOffset + FRows[vR].Height - vExtraHeight;
                            }
                        }
                    }
                }
            }
        }

        private int SrcCellDataTopDistanceToDest(int aSrcRow, int aDestRow)
        {
            int Result = FBorderWidth + FRows[aSrcRow].FmtOffset;

            int vR = aSrcRow - 1;
            while (vR > aDestRow)
            {
                Result += FRows[vR].Height + FBorderWidth + FRows[vR].FmtOffset;
                vR--;
            }

            return Result + FRows[aDestRow].Height;
        }

        /// <summary> 返回指定单元格相对表格的起始位置坐标(如果被合并返回合并到单元格的坐标) </summary>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <returns></returns>
        private POINT GetCellPostion(int aRow, int aCol)
        {
            POINT Result = new POINT(FBorderWidth, FBorderWidth);

            for (int i = 0; i <= aRow - 1; i++)
                Result.Y = Result.Y + FRows[i].FmtOffset + FRows[i].Height + FBorderWidth;
    
            Result.Y = Result.Y + FRows[aRow].FmtOffset;
            for (int i = 0; i <= aCol - 1; i++)
                Result.X = Result.X + FColWidths[i] + FBorderWidth;

            return Result;
        }

        private bool ActiveDataResizing()
        {
            if (FSelectCellRang.EditCell())
                return this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData.SelectedResizing();
            else
                return false;
        }

        /// <summary> 取消选中范围内除ARow, ACol之外单元格的选中状态(-1表示全部取消) </summary>
        private void DisSelectSelectedCell(int aRow = -1, int aCol = -1)
        {
            if (FSelectCellRang.StartRow >= 0)
            {
                HCTableCellData vCellData = null;
                // 先清起始，确保当前单元格可执行DisSelect 与201805172309相似
                if ((FSelectCellRang.StartRow == aRow) && (FSelectCellRang.StartCol == aCol))
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
                        if ((vRow == aRow) && (vCol == aCol))
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

        private void SetBorderWidth(byte value)
        {
            if (FBorderWidth != value)
            {
                if (value > FCellVPadding * 2)
                    FBorderWidth = (byte)(FCellVPadding * 2 - 1);
                else
                    FBorderWidth = value;
            }
        }

        private void SetCellVPadding(byte value)
        {
            if (FCellVPadding != value)
            {
                FCellVPadding = value;
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

        protected override void SetResizing(bool value)
        {
            base.SetResizing(value);
        }

        #region DoPaint子方法CheckRowBorderShouLian 找当前行各列分页时的收敛位置
        private void CheckRowBorderShouLian(int aRow, int aDataDrawBottom, RECT aDrawRect, ref int vShouLian, ref int vFirstDrawRow)
        {
            if (vShouLian == 0)
            {
                int vRowDataDrawTop = aDrawRect.Top + FBorderWidth - 1;  // 因为边框在ADrawRect.Top也占1像素，所以要减掉
                for (int i = 0; i <= aRow - 1; i++)
                    vRowDataDrawTop = vRowDataDrawTop + FRows[i].FmtOffset + FRows[i].Height + FBorderWidth;
                
                if ((FRows[aRow].FmtOffset > 0) && (aRow != vFirstDrawRow))
                {
                    vShouLian = vRowDataDrawTop - FBorderWidth;  // 上一行底部边框位置
                    return;
                }

                vRowDataDrawTop += + FRows[aRow].FmtOffset + FCellVPadding;  // 分页行Data绘制起始位置
                
                int vBreakBottom = 0;
                int vDestCellDataDrawTop = 0, vDestRow2 = -1, vDestCol2 = -1;
                HCTableCellData vCellData = null;

                for (int vC = 0; vC <= FRows[aRow].ColCount - 1; vC++)  // 遍历同行各列，获取截断位置(因为各行在CheckFormatPage已经算好分页位置，所以此处只要一个单元格跨页位置同时适用当前行所有单元格跨页位置
                {
                    vDestCellDataDrawTop = vRowDataDrawTop;//vCellDataDrawTop;
                    GetDestCell(aRow, vC, ref vDestRow2, ref vDestCol2);  // 获取到目标单元格所在行号

                    if (vC != vDestCol2 + FRows[vDestRow2][vDestCol2].ColSpan)
                        continue;
                    
                    vCellData = FRows[vDestRow2][vDestCol2].CellData;
                    if (vDestRow2 != aRow)  // 是源行，先取目标位置，再取到此行消减掉后的位置
                        vDestCellDataDrawTop = vDestCellDataDrawTop - SrcCellDataTopDistanceToDest(aRow, vDestRow2);

                    RECT vRect = new RECT();
                    for (int i = 0; i <= vCellData.DrawItems.Count - 1; i++)
                    {
                        if (vCellData.DrawItems[i].LineFirst)
                        {
                            vRect = vCellData.DrawItems[i].Rect;
                            //if DrawiInLastLine(i) then  // 单元格内最后一行内容补充FCellVPadding
                            vRect.Bottom = vRect.Bottom + FCellVPadding; // 每一行可能是要截断的，截断时下面要能放下FCellVPadding
                            if (vDestCellDataDrawTop + vRect.Bottom > aDataDrawBottom)
                            {
                                if (i > 0)
                                {
                                    if (aDataDrawBottom - vDestCellDataDrawTop - vCellData.DrawItems[i - 1].Rect.Bottom > FCellVPadding)
                                        vShouLian = Math.Max(vShouLian, vDestCellDataDrawTop + vCellData.DrawItems[i - 1].Rect.Bottom + FCellVPadding);
                                    else
                                        vShouLian = Math.Max(vShouLian, vDestCellDataDrawTop + vCellData.DrawItems[i - 1].Rect.Bottom);  // 上一个最下面做为截断位置
                                }
                                else  // 第一行就在当前页放不下
                                    vShouLian = Math.Max(vShouLian, vDestCellDataDrawTop - FCellVPadding - FBorderWidth);
                                
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

        // 绘制分页标识符
        void DoDrawPageBreakMark(bool aPageEnd, HCCanvas aCanvas, int vBorderRight, int vBorderBottom,
            int ADataDrawTop)
        {
            aCanvas.Pen.BeginUpdate();
            try
            {
                aCanvas.Pen.Color = Color.Gray;
                aCanvas.Pen.Style = HCPenStyle.psDot;
                aCanvas.Pen.Width = 1;
            }
            finally
            {
                aCanvas.Pen.EndUpdate();
            }

            if (aPageEnd)
            {
                aCanvas.MoveTo(vBorderRight + 5, vBorderBottom - 1);  // vBorderBottom
                aCanvas.LineTo(vBorderRight + 20, vBorderBottom - 1);

                aCanvas.Pen.Style = HCPenStyle.psSolid;
                aCanvas.MoveTo(vBorderRight + 19, vBorderBottom - 3);
                aCanvas.LineTo(vBorderRight + 19, vBorderBottom - 10);
                aCanvas.LineTo(vBorderRight + 5, vBorderBottom - 10);
                aCanvas.LineTo(vBorderRight + 5, vBorderBottom - 2);
            }
            else  // 分页符(页起始位置)
            {
                aCanvas.MoveTo(vBorderRight + 5, ADataDrawTop + 1);  // vBorderTop
                aCanvas.LineTo(vBorderRight + 20, ADataDrawTop + 1);

                aCanvas.Pen.Style = HCPenStyle.psSolid;
                aCanvas.MoveTo(vBorderRight + 19, ADataDrawTop + 3);
                aCanvas.LineTo(vBorderRight + 19, ADataDrawTop + 10);
                aCanvas.LineTo(vBorderRight + 5, ADataDrawTop + 10);
                aCanvas.LineTo(vBorderRight + 5, ADataDrawTop + 2);
            }
            
            aCanvas.Pen.Color = Color.Black;
        }
        #endregion


        /// <summary> 在指定的位置绘制表格 </summary>
        /// <param name="aStyle"></param>
        /// <param name="aDrawRect">绘制时的Rect(相对ADataScreenTop)</param>
        /// <param name="aDataDrawTop">Table所属的Data绘制起始位置(相对ADataScreenTop，可为负数)</param>
        /// <param name="aDataDrawBottom">Table所属的Data绘制起始位置(相对ADataScreenTop，可超过ADataScreenBottom)</param>
        /// <param name="aDataScreenTop">当前页屏显起始位置(相对于点0, 0，>=0)</param>
        /// <param name="aDataScreenBottom">当前页屏幕底部位置(相对于点0, 0，<=窗口高度)</param>
        /// <param name="aCanvas"></param>
        /// <param name="aPaintInfo"></param>
        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, 
            int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, 
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            // 单元格
            int vFirstDrawRow = -1, vCellDataDrawBottom = 0, vCellDrawLeft = 0, vShouLian = 0,
                vDestCellDataDrawTop = 0, vDestRow = 0, vDestCol = 0, vDestRow2 = 0, vDestCol2 = 0, vBorderBottom = 0;
            bool vDrawCellData = false;
            int vFixHeight = GetFixRowHeight();
            int vBorderOffs = FBorderWidth / 2;
            bool vFirstDrawRowIsBreak = false;
            int vCellDataDrawTop = aDrawRect.Top + FBorderWidth;  // 第1行数据绘制起始位置，因为边框在ADrawRect.Top也占1像素，所以要减掉
            for (int vR = 0; vR <= FRows.Count - 1; vR++)
            {
                // 不在当前屏幕范围内的不绘制(1)
                vCellDataDrawTop = vCellDataDrawTop + FRows[vR].FmtOffset + FCellVPadding;
                if (vCellDataDrawTop > aDataScreenBottom)
                {
                    if ((vFirstDrawRow < 0) && IsBreakRow(vR))
                        vFirstDrawRowIsBreak = (FFixRow >= 0) && (vR > FFixRow + FFixRowCount - 1);

                    break;
                }

                vCellDataDrawBottom = vCellDataDrawTop - FCellVPadding + FRows[vR].Height - FCellVPadding;

                if (vCellDataDrawBottom < aDataScreenTop)
                {
                    vCellDataDrawTop = vCellDataDrawBottom + FCellVPadding + FBorderWidth;  // 准备判断下一行是否是可显示第一行
                    continue;
                }

                if (vFirstDrawRow < 0)
                {
                    vFirstDrawRow = vR;

                    if (IsBreakRow(vR))
                        vFirstDrawRowIsBreak = (FFixRow >= 0) && (vR > FFixRow + FFixRowCount - 1);
                }

                vCellDrawLeft = aDrawRect.Left + FBorderWidth;

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
                    GetDestCell(vR, vC, ref vDestRow, ref vDestCol);  // 获取到目标单元格所在行号
                    if (vDestRow != vR)  // 得到目标单元格CellDataDrawTop的值
                        vDestCellDataDrawTop = vDestCellDataDrawTop - SrcCellDataTopDistanceToDest(vR, vDestRow);
              
                    #region 绘制单元格数据
                    if (vDrawCellData)
                    {
                        int vCellScreenBottom = Math.Min(aDataScreenBottom,  // 数据内容屏显最下端
                            vCellDataDrawTop
                            + Math.Max(FRows[vR].Height, FRows[vDestRow][vDestCol].Height) - FCellVPadding  // 行高和有合并的单元格高中最大的
                            );

                        //Assert(vCellScreenBottom - vMergeCellDataDrawTop >= FRows[vR].Height, "计划使用Continue但待确认会符合情况的");
                        HCTableCellData vCellData = FRows[vDestRow][vDestCol].CellData;  // 目标CellData，20170208003 如果移到if vDrawData外面则20170208002不需要了
                        int vCellScreenTop = Math.Max(aDataScreenTop, vCellDataDrawTop - FCellVPadding);  // 屏显最上端
                        if (vCellScreenTop - vDestCellDataDrawTop < vCellData.Height)
                        {
                            RECT vCellRect = new RECT(vCellDrawLeft, vCellScreenTop, vCellDrawLeft + FRows[vR][vC].Width, vCellScreenBottom);

                            // 背景色
                            if ((this.IsSelectComplate || vCellData.CellSelectedAll) && (!aPaintInfo.Print))
                            {
                                aCanvas.Brush.Color = OwnerData.Style.SelColor;
                                aCanvas.FillRect(vCellRect);
                            }
                            else  // 默认的绘制
                            {
                                bool vDrawDefault = true;
                                if (FOnCellPaintBK != null)
                                    FOnCellPaintBK(this, FRows[vDestRow][vDestCol], vCellRect, aCanvas, aPaintInfo, ref vDrawDefault);

                                if (vDrawDefault)
                                {
                                    if (IsFixRow(vR) || IsFixCol(vC))
                                        aCanvas.Brush.Color = HC.clBtnFace;
                                    else
                                        if (FRows[vDestRow][vDestCol].BackgroundColor != HC.HCTransparentColor)
                                            aCanvas.Brush.Color = FRows[vDestRow][vDestCol].BackgroundColor;
                                        else
                                            aCanvas.Brush.Style = HCBrushStyle.bsClear;

                                    aCanvas.FillRect(vCellRect);
                                }
                            }

                            // 获取可显示区域的起始、结束DrawItem
                            //vCellData.GetDataDrawItemRang(Math.Max(vCellScreenTop - vDestCellDataDrawTop, 0),
                            //  vCellScreenBottom - vDestCellDataDrawTop, vFristDItemNo, vLastDItemNo);
                            //if vFristDItemNo >= 0 then
                            if (vCellScreenBottom - vCellScreenTop > FCellVPadding)
                            {
                                FRows[vDestRow][vDestCol].PaintTo(
                                    vCellDrawLeft, vDestCellDataDrawTop - FCellVPadding,
                                    aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                                    0, FCellHPadding, FCellVPadding, aCanvas, aPaintInfo);

                                if (FOnCellPaintData != null)
                                {
                                    FOnCellPaintData(this, aDrawRect, vCellRect, vDestRow, vDestCol,
                                        vDestCellDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom,
                                        aCanvas, aPaintInfo);
                                }
                            }
                        }
                    }
                    #endregion

                    #region 绘制各单元格边框线                    
                    if (FBorderVisible || (!aPaintInfo.Print))
                    {
                        bool vDrawBorder = true;
                        // 目标单元格的上边框绘制位置 vDestCellDataDrawTop本身占掉了1像素
                        // FBorderWidth + FCellVPadding = vDestCellDataDrawTop，vDestCellDataDrawTop和FCellVapdding重叠了1像素
                        int vBorderTop = vDestCellDataDrawTop - FCellVPadding - FBorderWidth;
                        vBorderBottom = vBorderTop + FBorderWidth // 计算边框最下端
                            + Math.Max(FRows[vR].Height, this[vDestRow, vDestCol].Height);  // 由于可能是合并目标单元格，所以用单元格高和行高最高的
                        
                        // 目标单元格底部边框超过页底部，计算收敛位置
                        if (vBorderBottom > aDataScreenBottom)
                        {
                            int vSrcRowBorderTop = 0;

                            if (this[vR, vC].RowSpan > 0)
                            {
                                vSrcRowBorderTop = vBorderTop;
                                vDestRow2 = vR;  // 借用变量
                                while (vDestRow2 <= FRows.Count - 1)  // 找显示底部边框的源
                                {
                                    vSrcRowBorderTop = vSrcRowBorderTop + FRows[vDestRow2].FmtOffset + FRows[vDestRow2].Height + FBorderWidth;
                                    if (vSrcRowBorderTop > aDataScreenBottom)
                                    {
                                        if (vSrcRowBorderTop > aDataDrawBottom)
                                        {
                                            CheckRowBorderShouLian(vDestRow2, aDataDrawBottom, aDrawRect, ref vShouLian, ref vFirstDrawRow);  // 从当前行找收敛
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
                                    // 移动到当前行起始位置
                                    vSrcRowBorderTop = vBorderTop;  // 借用变量，vBorderTop值是目标单元格的上边框
                                    for (int i = vDestRow; i <= vR - 1; i++)
                                        vSrcRowBorderTop = vSrcRowBorderTop + FRows[i].Height + FBorderWidth;

                                    // 我是跨页后目标单元格正在此页源的第一个，我要负责目标在此页的边框
                                    vDestRow2 = vR;  // 借用变量
                                    while (vDestRow2 <= FRows.Count - 1)  // 找显示底部边框的源
                                    {
                                        vSrcRowBorderTop = vSrcRowBorderTop + FRows[vDestRow2].Height + FBorderWidth;
                                        if (vSrcRowBorderTop > aDataScreenBottom)
                                        {
                                            if (vSrcRowBorderTop > aDataDrawBottom)
                                            {
                                                CheckRowBorderShouLian(vDestRow2, aDataDrawBottom, aDrawRect, ref vShouLian, ref vFirstDrawRow);  // 从当前行找收敛
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
                                CheckRowBorderShouLian(vR, aDataDrawBottom, aDrawRect, ref vShouLian, ref vFirstDrawRow);
                                vBorderBottom = vShouLian;
                            }
                        }

                        if (vDrawBorder)
                        {
                            aCanvas.Pen.BeginUpdate();
                            try
                            {
                                aCanvas.Pen.Width = FBorderWidth;

                                if (FBorderVisible)
                                {
                                    aCanvas.Pen.Color = Color.Black;
                                    aCanvas.Pen.Style = HCPenStyle.psSolid;
                                }
                                else
                                    if (!aPaintInfo.Print)
                                    {
                                        aCanvas.Pen.Color = HC.clActiveBorder;
                                        aCanvas.Pen.Style = HCPenStyle.psDot;
                                    }
                            }
                            finally
                            {
                                aCanvas.Pen.EndUpdate();
                            }
                            
                            int vBorderLeft = vCellDrawLeft - FBorderWidth;
                            int vBorderRight = vCellDrawLeft + FColWidths[vC] + GetColSpanWidth(vDestRow, vDestCol);
                            
                            if ((vBorderTop < aDataScreenTop) && (aDataDrawTop >= 0))
                                vBorderTop = aDataScreenTop;

                            IntPtr vExtPen = HC.CreateExtPen(aCanvas.Pen);
                            IntPtr vOldPen = (IntPtr)GDI.SelectObject(aCanvas.Handle, vExtPen);
                            try
                            {
                                if ((vBorderTop > 0) && (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsTop)))
                                {
                                    aCanvas.MoveTo(vBorderLeft + vBorderOffs, vBorderTop + vBorderOffs);   // 左上
                                    aCanvas.LineTo(vBorderRight + vBorderOffs, vBorderTop + vBorderOffs);  // 右上
                                }

                                if (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsRight))
                                {
                                    aCanvas.MoveTo(vBorderRight + vBorderOffs, vBorderTop + vBorderOffs);  // 右上
                                    aCanvas.LineTo(vBorderRight + vBorderOffs, vBorderBottom + vBorderOffs);  // 右下
                                }

                                if ((vBorderBottom <= aDataScreenBottom) && (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsBottom)))
                                {
                                    aCanvas.MoveTo(vBorderLeft + vBorderOffs, vBorderBottom + vBorderOffs);  // 左下
                                    aCanvas.LineTo(vBorderRight + vBorderOffs, vBorderBottom + vBorderOffs);  // 右下
                                }

                                if (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsLeft))
                                {
                                    aCanvas.MoveTo(vBorderLeft + vBorderOffs, vBorderTop + vBorderOffs);
                                    aCanvas.LineTo(vBorderLeft + vBorderOffs, vBorderBottom + vBorderOffs);
                                }

                                if (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsLTRB))
                                {
                                    aCanvas.MoveTo(vBorderLeft + vBorderOffs, vBorderTop + vBorderOffs);
                                    aCanvas.LineTo(vBorderRight + vBorderOffs, vBorderBottom + vBorderOffs);
                                }

                                if (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsRTLB))
                                {
                                    aCanvas.MoveTo(vBorderRight + vBorderOffs, vBorderTop + vBorderOffs);
                                    aCanvas.LineTo(vBorderLeft + vBorderOffs, vBorderBottom + vBorderOffs);
                                }
                            }
                            finally
                            {
                                GDI.SelectObject(aCanvas.Handle, vOldPen);
                                GDI.DeleteObject(vExtPen);
                            }

                            // "最后一列"负责绘制分页标识
                            vDestCol2 = vC + FRows[vR][vC].ColSpan;
                            if ((!aPaintInfo.Print) && (vDestCol2 == FColWidths.Count - 1))
                            {
                                if (vCellDataDrawTop + FRows[vR].Height - FCellVPadding > aDataDrawBottom)
                                    DoDrawPageBreakMark(true, aCanvas, vBorderRight, vBorderBottom, aDataDrawTop);
                                else
                                if ((vR < this.RowCount - 1)
                                    && (vBorderBottom + FRows[vR + 1].FmtOffset + FRows[vR + 1].Height > aDataDrawBottom))
                                {
                                    if (FRows[vR + 1].FmtOffset > 0)
                                        DoDrawPageBreakMark(true, aCanvas, vBorderRight, vBorderBottom, aDataDrawTop);
                                    else
                                    if (vBorderBottom == aDataDrawBottom)
                                        DoDrawPageBreakMark(true, aCanvas, vBorderRight, vBorderBottom, aDataDrawTop); //* 此时下一行不在本页显示，但FmtOffset并不大于0，
                                }                                                                                      //* 如果这里不处理，循环下一行时底部大于当前页直接跳出循环失去绘制机会
                                    
                                if ((vFirstDrawRow != 0)  // 起始行不是第一行)
                                    && (vR == vFirstDrawRow)  // 起始行绘制
                                    && (aDrawRect.Top < aDataDrawTop))  // 第一行在上一页
                                    DoDrawPageBreakMark(false, aCanvas, vBorderRight, vBorderBottom, aDataDrawTop);
                            }
                        }
                    }
                    #endregion

                    vCellDrawLeft = vCellDrawLeft + FColWidths[vC] + FBorderWidth;  // 同行下一列的起始Left位置
                }
                    
                vCellDataDrawTop = vCellDataDrawBottom + FCellVPadding + FBorderWidth;  // 下一行的Top位置
            }

            if (vFirstDrawRowIsBreak)
                PaintFixRows(aDrawRect.Left, aDataDrawTop, aDataScreenBottom, aCanvas, aPaintInfo);

            if ((FFixCol >= 0) && (GetFixColLeft() + aDrawRect.Left < 0))
                PaintFixCols(aDrawRect.Top, 0, aDataDrawTop, aDataScreenBottom, aCanvas, aPaintInfo);
            
            #region 绘制拖动线
            if (Resizing && (FResizeInfo.TableSite == TableSite.tsBorderRight))
            {
                aCanvas.Pen.BeginUpdate();
                try
                {
                    aCanvas.Pen.Color = this.FBorderColor;
                    aCanvas.Pen.Style = HCPenStyle.psDot;
                    aCanvas.Pen.Width = 1;
                }
                finally
                {
                    aCanvas.Pen.EndUpdate();
                }

                aCanvas.MoveTo(aDrawRect.Left + FResizeInfo.DestX, Math.Max(aDataDrawTop, aDrawRect.Top));
                aCanvas.LineTo(aDrawRect.Left + FResizeInfo.DestX, (int)Math.Min(aDataDrawBottom,
                    Math.Min(aDrawRect.Bottom, vBorderBottom)));
            }
            else
            if (Resizing && (FResizeInfo.TableSite == TableSite.tsBorderBottom))
            {
                aCanvas.Pen.BeginUpdate();
                try
                {
                    aCanvas.Pen.Color = this.FBorderColor;
                    aCanvas.Pen.Style = HCPenStyle.psDot;
                    aCanvas.Pen.Width = 1;
                }
                finally
                {
                    aCanvas.Pen.EndUpdate();
                }

                aCanvas.MoveTo(aDrawRect.Left, aDrawRect.Top + FResizeInfo.DestY);
                aCanvas.LineTo(aDrawRect.Right, aDrawRect.Top + FResizeInfo.DestY);
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
              && ((FResizeInfo.TableSite == TableSite.tsBorderRight) 
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
                        e.X - vCellPt.X, e.Y - vCellPt.Y, e.Delta);
                    FRows[FMouseDownRow][FMouseDownCol].MouseDown(vEventArgs, FCellHPadding, FCellVPadding);
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
                    
                    FSelectCellRang.SetStart(FMouseDownRow, FMouseDownCol);
                    
                    vCellPt = GetCellPostion(FMouseDownRow, FMouseDownCol);
                    
                    MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                        e.X - vCellPt.X, e.Y - vCellPt.Y, e.Delta);
                    FRows[FMouseDownRow][FMouseDownCol].MouseDown(vEventArgs, FCellHPadding, FCellVPadding);
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

        #region MouseMove子方法
        private void AdjustSelectRang(int vMoveRow, int vMoveCol)
        {
            // 先清除起始单元格之外的，以便下面重新处理选中单元格的全选
            if (FSelectCellRang.StartRow >= 0)
            {
                for (int vR = FSelectCellRang.StartRow; vR <= FSelectCellRang.EndRow; vR++)
                {
                    for (int vC = FSelectCellRang.StartCol; vC <= FSelectCellRang.EndCol; vC++)
                    {
                        if ((vR == FMouseDownRow) && (vC == FMouseDownCol))
                        {

                        }
                        else
                        {
                            if (this[vR, vC].CellData != null)
                                this[vR, vC].CellData.DisSelect();
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
                    
                    FSelectCellRang.SetStart(FMouseDownRow, FMouseDownCol);
                    FSelectCellRang.SetEnd(vMoveRow, vMoveCol);
                }
                else  // 从下面选入
                {
                    GetDestCell(this.RowCount - 1, this.FColWidths.Count - 1, ref vRow, ref vCol);
                    FMouseDownRow = vRow;
                    FMouseDownCol = vCol;
                    
                    FSelectCellRang.SetStart(vMoveRow, vMoveCol);
                    FSelectCellRang.SetEnd(FMouseDownRow, FMouseDownCol);
                }

                FOutSelectInto = true;
            }
            else
            if (FMouseMoveRow > FMouseDownRow)
            {
                FSelectCellRang.StartRow = FMouseDownRow;
                FSelectCellRang.EndRow = FMouseMoveRow;

                if (FMouseMoveCol < FMouseDownCol)  // 移动列在按下列前面
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
            if (FMouseMoveRow < FMouseDownRow)  // 移动行在按下行上面
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
                && (FSelectCellRang.StartCol == FSelectCellRang.EndCol))  // 不处理合并时，选中在同一单元格
            {
                FSelectCellRang.InitializeEnd();
            }
            else
            {
                if (FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol].IsMergeSource())
                {
                    GetDestCell(FSelectCellRang.StartRow, FSelectCellRang.StartCol, ref vRow, ref vCol);
                    FSelectCellRang.SetStart(vRow, vCol);
                }

                if (FRows[FSelectCellRang.EndRow][FSelectCellRang.EndCol].IsMergeDest())
                {
                    GetSourceCell(FSelectCellRang.EndRow, FSelectCellRang.EndCol, ref vRow, ref vCol);  // 获取目标方法如果传递的是目标得到的是源
                    FSelectCellRang.SetEnd(vRow, vCol);
                }
                if ((FSelectCellRang.StartRow == FSelectCellRang.EndRow)
                    && (FSelectCellRang.StartCol == FSelectCellRang.EndCol))
                    FSelectCellRang.InitializeEnd();
            }
        }

        private void MatchCellSelectState()
        {
            if (!FSelectCellRang.EditCell())
            {
                for (int vR = FSelectCellRang.StartRow; vR <= FSelectCellRang.EndRow; vR++)
                {
                    for (int vC = FSelectCellRang.StartCol; vC <= FSelectCellRang.EndCol; vC++)
                    {
                        /*if (vRow = vMoveRow) and (vCol = vMoveCol) then else 什么情况下需要跳过?}*/
                        if (this[vR, vC].CellData != null)
                            this[vR, vC].CellData.SelectAll();
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
                    e.X - vCellPt.X, e.Y - vCellPt.Y, e.Delta);
                this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].MouseMove(vEventArgs, FCellHPadding, FCellVPadding);
                
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
                if (FMouseLBDowning || (e.Button == MouseButtons.Left))
                {
                    if (FDraging || OwnerData.Style.UpdateInfo.Draging)
                    {
                        FMouseMoveRow = vMoveRow;
                        FMouseMoveCol = vMoveCol;
                        vCellPt = GetCellPostion(FMouseMoveRow, FMouseMoveCol);

                        MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                            e.X - vCellPt.X, e.Y - vCellPt.Y, e.Delta);
                        this[FMouseMoveRow, FMouseMoveCol].MouseMove(vEventArgs, FCellHPadding, FCellVPadding);
                        
                        return;
                    }

                    if (!FSelecting)
                        FSelecting = true;

                    if ((vMoveRow != FMouseMoveRow) || (vMoveCol != FMouseMoveCol))  // 鼠标移动到新单元格
                    {
                        FMouseMoveRow = vMoveRow;
                        FMouseMoveCol = vMoveCol;

                        AdjustSelectRang(vMoveRow, vMoveCol);  // 计算选中起始结束范围(会纠正从后、下往前选的情况)
                        MatchCellSelectState();  // 处理选中范围内各单元格的选中状态
                    }

                    vCellPt = GetCellPostion(FMouseMoveRow, FMouseMoveCol);
                    MouseEventArgs vEventArgs2 = new MouseEventArgs(e.Button, e.Clicks,
                            e.X - vCellPt.X, e.Y - vCellPt.Y, e.Delta);
                    this[FMouseMoveRow, FMouseMoveCol].MouseMove(vEventArgs2, FCellHPadding, FCellVPadding);
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
                        e.X - vCellPt.X, e.Y - vCellPt.Y, e.Delta);
                    FRows[FMouseMoveRow][FMouseMoveCol].MouseMove(vEventArgs, FCellHPadding, FCellVPadding);
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
            POINT vCellPt = new POINT();

            FMouseLBDowning = false;
            
            if (ActiveDataResizing())
            {
                vCellPt = GetCellPostion(FSelectCellRang.StartRow, FSelectCellRang.StartCol);
                MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks, 
                    e.X - vCellPt.X, e.Y - vCellPt.Y, e.Delta);
                this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].MouseUp(vEventArgs, FCellHPadding, FCellVPadding);
                
                return;
            }
            
            int vUpRow = -1, vUpCol = -1;
            ResizeInfo vResizeInfo;

            if (Resizing)
            {
                if (FResizeInfo.TableSite == TableSite.tsBorderRight)
                {
                    vCellPt.X = e.X - FMouseDownX;  // 不使用FResizeInfo.DestX(会造成按下处弹出也有偏移)
                    if (vCellPt.X != 0)
                    {
                        // AReDest为False用于处理拖动改变列宽时，如拖动处列是合并源，其他行此列并无合并操作
                        // 这时弹起，如果取拖动列目标列变宽，则其他行拖动处的列并没变宽
                        vResizeInfo = GetCellAt(FMouseDownX, FMouseDownY, ref vUpRow, ref vUpCol, false);
                        
                        if ((vResizeInfo.TableSite != TableSite.tsOutside) && (vCellPt.X != 0))
                        {
                            if (vCellPt.X > 0)
                            {
                                if (vUpCol < FColWidths.Count - 1)
                                {
                                    if (FColWidths[vUpCol + 1] - vCellPt.X < HC.MinColWidth)
                                        vCellPt.X = FColWidths[vUpCol + 1] - HC.MinColWidth;
                                   
                                    if (vCellPt.X != 0)
                                    {
                                        Undo_ColResize(vUpCol, FColWidths[vUpCol], FColWidths[vUpCol] + vCellPt.X);
                                        
                                        FColWidths[vUpCol] = FColWidths[vUpCol] + vCellPt.X;  // 当前列变化
                                        //if (vUpCol < FColWidths.Count - 1)
                                        //    FColWidths[vUpCol + 1] = FColWidths[vUpCol + 1] - vCellPt.X;
                                    }
                                }
                                else  // 最右侧列拖宽
                                {
                                    FColWidths[vUpCol] = FColWidths[vUpCol] + vCellPt.X;  // 当前列变化
                                    Undo_ColResize(vUpCol, FColWidths[vUpCol], FColWidths[vUpCol] + vCellPt.X);
                                }
                            }
                            else  // 拖窄了
                            {
                                if (FColWidths[vUpCol] + vCellPt.X < HC.MinColWidth)
                                    vCellPt.X = HC.MinColWidth - FColWidths[vUpCol];
                                
                                if (vCellPt.X != 0)
                                {
                                    Undo_ColResize(vUpCol, FColWidths[vUpCol], FColWidths[vUpCol] + vCellPt.X);
                                    
                                    FColWidths[vUpCol] = FColWidths[vUpCol] + vCellPt.X;  // 当前列变化
                                    //if (vUpCol < FColWidths.Count - 1)
                                    //    FColWidths[vUpCol + 1] = FColWidths[vUpCol + 1] - vCellPt.X;
                                }
                            }
                        }
                    }
                }
                else
                if (FResizeInfo.TableSite == TableSite.tsBorderBottom)
                {
                    vCellPt.Y = e.Y - FMouseDownY;  // 不使用FResizeInfo.DestY(会造成按下处弹出也有偏移)
                    if (vCellPt.Y != 0)
                    {
                        Undo_RowResize(FMouseDownRow, FRows[FMouseDownRow].Height, FRows[FMouseDownRow].Height + vCellPt.Y);
                        FRows[FMouseDownRow].Height = FRows[FMouseDownRow].Height + vCellPt.Y;
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
                    vCellPt = GetCellPostion(FMouseDownRow, FMouseDownCol);

                    MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                        e.X - vCellPt.X, e.Y - vCellPt.Y, e.Delta);
                    this[FMouseDownRow, FMouseDownCol].MouseUp(vEventArgs, FCellHPadding, FCellVPadding);
                }

                vResizeInfo = GetCellAt(e.X, e.Y, ref vUpRow, ref vUpCol);
                if (vResizeInfo.TableSite == TableSite.tsCell)
                {
                    if ((vUpRow != FMouseDownRow) || (vUpCol != FMouseDownCol))
                    {
                        vCellPt = GetCellPostion(vUpRow, vUpCol);

                        MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                            e.X - vCellPt.X, e.Y - vCellPt.Y, e.Delta);
                        this[vUpRow, vUpCol].MouseUp(vEventArgs, FCellHPadding, FCellVPadding);
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
                    vCellPt = GetCellPostion(vUpRow, vUpCol);

                    MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                        e.X - vCellPt.X, e.Y - vCellPt.Y, e.Delta);
                    this[vUpRow, vUpCol].MouseUp(vEventArgs, FCellHPadding, FCellVPadding);
                }
            }
            else  // 非划选，非拖拽
            if (FMouseDownRow >= 0)
            {
                vCellPt = GetCellPostion(FMouseDownRow, FMouseDownCol);

                MouseEventArgs vEventArgs = new MouseEventArgs(e.Button, e.Clicks,
                    e.X - vCellPt.X, e.Y - vCellPt.Y, e.Delta);
                this[FMouseDownRow, FMouseDownCol].MouseUp(vEventArgs, FCellHPadding, FCellVPadding);
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

        /// <summary> 清除并返回为处理分页行和行中间有向下偏移后，比净高增加的高度(为重新格式化时后面计算偏移用) </summary>
        public override int ClearFormatExtraHeight()
        {
            if (this.Height == FFormatHeight)
                return 0;

            int vOldHeight = Height;
            int vRowFrom = -1;
            int Result = 0;

            for (int vR = FRows.Count - 1; vR >= 0; vR--)
            {
                if (FRows[vR].FmtOffset != 0)
                {
                    vRowFrom = vR;
                    FRows[vR].FmtOffset = 0;
                }

                for (int vC = 0; vC <= ColCount - 1; vC++)
                {
                    HCTableCell vCell = FRows[vR][vC];

                    if ((vCell.ClearFormatExtraHeight() != 0)
                         || ((vCell.CellData != null) && (vCell.Height != FCellHPadding + vCell.CellData.Height + FCellHPadding)))
                    {
                        vRowFrom = vR;
                        CalcRowCellHeight(vR);
                    }
                }
            }

            if (vRowFrom >= 0)
            {
                CalcMergeRowHeightFrom(vRowFrom);
                this.Height = GetFormatHeight();
                Result = vOldHeight - this.Height;
            }

            return Result;
        }

        public override bool DeleteSelected()
        {
            bool Result = base.DeleteSelected();

            if (FSelectCellRang.StartRow >= 0)
            {
                if (FSelectCellRang.EndRow >= 0)
                {
                    for (int vR = FSelectCellRang.StartRow; vR <= FSelectCellRang.EndRow; vR++)
                    {
                        for (int vC = FSelectCellRang.StartCol; vC <= FSelectCellRang.EndCol; vC++)
                        {
                            if (this[vR, vC].CellData != null)
                                this[vR, vC].CellData.DeleteSelected();
                        }
                    }

                    FLastChangeFormated = false;
                    Result = true;
                }
                else  // 在同一单元格
                {
                    HCProcedure vEvent = delegate ()
                    {
                        Result = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData.DeleteSelected();
                    };

                    CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);
                }
            }

            return Result;
        }

        public override void DisSelect()
        {
            base.DisSelect();

            DisSelectSelectedCell();

            this.InitializeMouseInfo();
            FSelectCellRang.Initialize();

            FSelecting = false;
            FDraging = false;
            FOutSelectInto = false;
        }

        public override void MarkStyleUsed(bool aMark)
        {
            base.MarkStyleUsed(aMark);
            for (int vR = 0; vR <= FRows.Count - 1; vR++)
            {
                for (int vC = 0; vC <= FRows[vR].ColCount - 1; vC++)
                {
                    if (FRows[vR][vC].CellData != null)
                        FRows[vR][vC].CellData.MarkStyleUsed(aMark);
                }
            }
        }

        public override void GetCaretInfo(ref HCCaretInfo aCaretInfo)
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
                        aCaretInfo.X = aCaretInfo.X - 2;  // 为使光标更明显，向左偏移2

                    vTop = 0;
                    for (int i = FPageBreaks.Count - 1; i >= 0; i--)  // 找光标顶部位
                    {
                        if (FPageBreaks[i].Row <= FOutsideInfo.Row)
                        {
                            if (FPageBreaks[i].PageIndex == aCaretInfo.PageIndex - 1)
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
                            if (FPageBreaks[i].PageIndex == aCaretInfo.PageIndex)
                            {
                                vBottom = FPageBreaks[i].BreakSeat;  // 分页顶部位置
                                break;
                            }
                        }
                    }

                    aCaretInfo.Y = aCaretInfo.Y + vTop;
                    aCaretInfo.Height = vBottom - vTop;
                }
                else
                    aCaretInfo.Visible = false;

                return;
            }
            else
                vCaretCell = this[vRow, vCol];

            if (OwnerData.Style.UpdateInfo.Draging)
            {
                if ((vCaretCell.CellData.MouseMoveItemNo < 0)
                    || (vCaretCell.CellData.MouseMoveItemOffset < 0))
                {
                    aCaretInfo.Visible = false;
                    return;
                }

                vCaretCell.GetCaretInfo(vCaretCell.CellData.MouseMoveItemNo,
                    vCaretCell.CellData.MouseMoveItemOffset, FCellHPadding, FCellVPadding, ref aCaretInfo);
            }
            else  // 非拖拽
            {
                if ((vCaretCell.CellData.SelectInfo.StartItemNo < 0)
                    || (vCaretCell.CellData.SelectInfo.StartItemOffset < 0))
                {
                    aCaretInfo.Visible = false;
                    return;
                }

                vCaretCell.GetCaretInfo(vCaretCell.CellData.SelectInfo.StartItemNo,
                    vCaretCell.CellData.SelectInfo.StartItemOffset, FCellHPadding, 
                    FCellVPadding, ref aCaretInfo);
            }

            POINT vPos = GetCellPostion(vRow, vCol);
            aCaretInfo.X = vPos.X + aCaretInfo.X;
            aCaretInfo.Y = vPos.Y + aCaretInfo.Y;
        }

        protected override void SetActive(bool value)
        {
            if (this.Active != value)
            {
                HCTableCell vCell = GetEditCell();
                if ((vCell != null) && (vCell.CellData != null))
                    vCell.CellData.Active = value;

                if (!value)
                    this.InitializeMouseInfo();

                base.SetActive(value);
            }
        }

        // 撤销重做相关方法
        protected override HCUndo DoSelfUndoNew()
        {
            if (FSelectCellRang.EditCell())  // 在同一单元格中编辑
            {
                HCUndo Result = new HCDataUndo();
                HCCellUndoData vCellUndoData = new HCCellUndoData();
                vCellUndoData.Row = FSelectCellRang.StartRow;
                vCellUndoData.Col = FSelectCellRang.StartCol;
                Result.Data = vCellUndoData;
                return Result;
            }
            else
                return base.DoSelfUndoNew();
        }

        protected override void DoSelfUndoDestroy(HCUndo aUndo)
        {
            if (aUndo.Data is HCCellUndoData)
                (aUndo.Data as HCCellUndoData).Dispose();

            base.DoSelfUndoDestroy(aUndo);
        }

        protected override void DoSelfUndo(HCUndo aUndo)
        {
            this.InitializeMouseInfo();
            FSelectCellRang.Initialize();

            if (aUndo.Data is HCCellUndoData)
            {
                HCCellUndoData vCellUndoData = aUndo.Data as HCCellUndoData;
                FSelectCellRang.StartRow = vCellUndoData.Row;
                FSelectCellRang.StartCol = vCellUndoData.Col;

                HCProcedure vEvent = delegate()
                {
                    FRows[vCellUndoData.Row][vCellUndoData.Col].CellData.Undo(aUndo);
                };

                CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);
            }
            else
            if (aUndo.Data is HCColSizeUndoData)
            {
                HCColSizeUndoData vColSizeUndoData = aUndo.Data as HCColSizeUndoData;
                if (vColSizeUndoData.Col < FColWidths.Count - 1)
                {
                    FColWidths[vColSizeUndoData.Col + 1] = FColWidths[vColSizeUndoData.Col + 1] +
                        FColWidths[vColSizeUndoData.Col] - vColSizeUndoData.OldWidth;
                }
                FColWidths[vColSizeUndoData.Col] = vColSizeUndoData.OldWidth;
                FLastChangeFormated = false;
            }
            else
            if (aUndo.Data is HCRowSizeUndoData)
            {
                HCRowSizeUndoData vRowSizeUndoData = aUndo.Data as HCRowSizeUndoData;
                FRows[vRowSizeUndoData.Row].Height = vRowSizeUndoData.OldHeight;
                FLastChangeFormated = false;
            }
            else
            if (aUndo.Data is HCMirrorUndoData)
            {
                MemoryStream vStream = new MemoryStream();
                try
                {
                    this.SaveToStream(vStream);  // 记录撤销前状态
                    // 恢复原样
                    HCMirrorUndoData vMirrorUndoData = aUndo.Data as HCMirrorUndoData;
                    vMirrorUndoData.Stream.Position = 0;

                    int vStyleNo = HCStyle.Null;
                    byte[] vBuffer = BitConverter.GetBytes(vStyleNo);
                    vMirrorUndoData.Stream.Read(vBuffer, 0, vBuffer.Length);
                    vStyleNo = BitConverter.ToInt32(vBuffer, 0);
                    this.LoadFromStream(vMirrorUndoData.Stream, OwnerData.Style, HC.HC_FileVersionInt);

                    vMirrorUndoData.Stream.SetLength(0);
                    vStream.CopyTo(vMirrorUndoData.Stream);  // 保存撤销前状态
                    FLastChangeFormated = false;
                }
                finally
                {
                    vStream.Close();
                    vStream.Dispose();
                }
            }
            else
                base.DoSelfUndo(aUndo);
        }

        protected override void DoSelfRedo(HCUndo aRedo)
        {
            this.InitializeMouseInfo();
            FSelectCellRang.Initialize();

            if (aRedo.Data is HCCellUndoData)
            {
                HCCellUndoData vRedoCellUndoData = aRedo.Data as HCCellUndoData;
                FSelectCellRang.StartRow = vRedoCellUndoData.Row;
                FSelectCellRang.StartCol = vRedoCellUndoData.Col;

                HCProcedure vEvent = delegate()
                {
                    FRows[vRedoCellUndoData.Row][vRedoCellUndoData.Col].CellData.Redo(aRedo);
                };

                CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);
            }
            else
            if (aRedo.Data is HCColSizeUndoData)
            {
                HCColSizeUndoData vColSizeUndoData = aRedo.Data as HCColSizeUndoData;
                if (vColSizeUndoData.Col < FColWidths.Count - 1)
                {
                    FColWidths[vColSizeUndoData.Col + 1] = FColWidths[vColSizeUndoData.Col + 1] +
                        FColWidths[vColSizeUndoData.Col] - vColSizeUndoData.NewWidth;
                }
                FColWidths[vColSizeUndoData.Col] = vColSizeUndoData.NewWidth;
                FLastChangeFormated = false;
            }
            else
            if (aRedo.Data is HCRowSizeUndoData)
            {
                HCRowSizeUndoData vRowSizeUndoData = aRedo.Data as HCRowSizeUndoData;
                FRows[vRowSizeUndoData.Row].Height = vRowSizeUndoData.NewHeight;
                FLastChangeFormated = false;
            }
            else
            if (aRedo.Data is HCMirrorUndoData)
            {
                MemoryStream vStream = new MemoryStream();
                try
                {
                    this.SaveToStream(vStream);  // 记录恢复前状态

                    HCMirrorUndoData vMirrorUndoData = aRedo.Data as HCMirrorUndoData;
                    vMirrorUndoData.Stream.Position = 0;

                    int vStyleNo = HCStyle.Null;
                    byte[] vBuffer = BitConverter.GetBytes(vStyleNo);
                    vMirrorUndoData.Stream.Read(vBuffer, 0, vBuffer.Length);
                    vStyleNo = BitConverter.ToInt32(vBuffer, 0);
                    this.LoadFromStream(vMirrorUndoData.Stream, OwnerData.Style, HC.HC_FileVersionInt);

                    vMirrorUndoData.Stream.SetLength(0);
                    vStream.CopyTo(vMirrorUndoData.Stream);  // 保存恢复前状态
                    FLastChangeFormated = false;
                }
                finally
                {
                    vStream.Close();
                    vStream.Dispose();
                }
            }
            else
                base.DoSelfRedo(aRedo);
        }

        protected void Undo_ColResize(int aCol, int aOldWidth, int aNewWidth)
        {
            HCUndoList vUndoList = GetSelfUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                SelfUndo_New();
                HCUndo vUndo = vUndoList.Last;
                if (vUndo != null)
                {
                    HCColSizeUndoData vColSizeUndoData = new HCColSizeUndoData();
                    vColSizeUndoData.Col = aCol;
                    vColSizeUndoData.OldWidth = aOldWidth;
                    vColSizeUndoData.NewWidth = aNewWidth;

                    vUndo.Data = vColSizeUndoData;
                }
            }
        }

        protected void Undo_RowResize(int aRow, int aOldHeight, int aNewHeight)
        {
            HCUndoList vUndoList = GetSelfUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                SelfUndo_New();
                HCUndo vUndo = vUndoList.Last;
                if (vUndoList != null)
                {
                    HCRowSizeUndoData vRowSizeUndoData = new HCRowSizeUndoData();
                    vRowSizeUndoData.Row = aRow;
                    vRowSizeUndoData.OldHeight = aOldHeight;
                    vRowSizeUndoData.NewHeight = aNewHeight;

                    vUndo.Data = vRowSizeUndoData;
                }
            }
        }

        protected void Undo_MergeCells()
        {
            HCUndoList vUndoList = GetSelfUndoList();
            if ((vUndoList != null) && vUndoList.Enable)
            {
                SelfUndo_New();
                HCUndo vUndo = vUndoList.Last;
                if (vUndo != null)
                {
                    HCMirrorUndoData vMirrorUndoData = new HCMirrorUndoData();
                    this.SaveToStream(vMirrorUndoData.Stream);

                    vUndo.Data = vMirrorUndoData;
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

        protected void CheckFixColSafe(int aCol)
        {
            if (FFixCol + FFixColCount - 1 >= aCol)
            {
                FFixCol = -1;
                FFixColCount = 0;
            }
        }

        protected void CheckFixRowSafe(int aRow)
        {
            if (FFixRow + FFixRowCount - 1 >= aRow)
            {
                FFixRow = -1;
                FFixRowCount = 0;
            }
        }

        /// <summary> 获取指定行列范围实际对应的行列范围
        /// </summary>
        /// <param name="aStartRow"></param>
        /// <param name="aStartCol"></param>
        /// <param name="aEndRow"></param>
        /// <param name="aEndCol"></param>
        protected void AdjustCellRange(int aStartRow, int aStartCol, ref int aEndRow, ref int aEndCol)
        {
            int vLastRow = aEndRow;
            int vLastCol = aEndCol;
            HCTableCell vCell = null;
            int vDestRow = -1, vDestCol = -1;
            for (int vR = aStartRow; vR <= aEndRow; vR++)
            {
                for (int vC = aStartCol; vC <= aEndCol; vC++)
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

            aEndRow = vLastRow;
            aEndCol = vLastCol;
        }

        #region
        private void DeleteEmptyRows(int aSRow, int aERow)
        {
            bool vEmptyRow = false;
            for (int vR = aERow; vR >= aSRow; vR--)  // 遍历
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
                    for (int i = 0; i <= vR - 1; i++)
                    {
                        for (int vC = 0; vC <= FRows[i].ColCount - 1; vC++)
                        {
                            if (FRows[i][vC].RowSpan > 0)
                                FRows[i][vC].RowSpan = FRows[i][vC].RowSpan - 1;
                        }
                    }

                    for (int i = vR + 1; i <= FRows.Count - 1; i++)
                    {
                        for (int vC = 0; vC <= FRows[i].ColCount - 1; vC++)
                        {
                            if (FRows[i][vC].RowSpan < 0)
                                FRows[i][vC].RowSpan = FRows[i][vC].RowSpan + 1;
                        }
                    }

                    FRows.Delete(vR);  // 删除当前空行
                }
            }
        }

        private void DeleteEmptyCols(int aSCol, int aECol)
        {
            bool vEmptyCol = false;

            for (int vC = aECol; vC >= aSCol; vC--)  // 循环各
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
                    for (int vR = RowCount - 1; vR >= 0; vR--)  // 循环各行，删除对应
                    {
                        for (int i = 0; i <= vC - 1; i++)
                        {
                            vTableCell = FRows[vR][i];
                            if (i + vTableCell.ColSpan >= vC)
                                vTableCell.ColSpan = vTableCell.ColSpan - 1;
                        }

                        for (int i = vC; i <= FRows[vR].ColCount - 1; i++)
                        {
                            vTableCell = FRows[vR][i];
                            if (i + vTableCell.ColSpan < vC)
                                vTableCell.ColSpan = vTableCell.ColSpan + 1;
                        }

                        FRows[vR].Delete(vC);  // 删除列
                    }

                    FColWidths[vC - 1] = FColWidths[vC - 1] + FBorderWidth + FColWidths[vC];
                    FColWidths.RemoveAt(vC);
                }
            }
        }
        #endregion

        protected bool MergeCells(int aStartRow, int aStartCol, int aEndRow, int aEndCol)
        {
            bool Result = false;
            int vEndRow = aEndRow;
            int vEndCol = aEndCol;

            AdjustCellRange(aStartRow, aStartCol, ref vEndRow, ref vEndCol);

            Result = CellsCanMerge(aStartRow, aStartCol, vEndRow, vEndCol);
            if (!Result)
                return Result;

            // 经过上面的校验和判断后，起始行、列和结束行、列组成一个矩形区域
            if (aStartRow == vEndRow)
            {
                for (int vC = aStartCol + 1; vC <= vEndCol; vC++)  // 合并
                {
                    if (FRows[aStartRow][vC].CellData != null)
                    {
                        FRows[aStartRow][aStartCol].CellData.AddData(this[aStartRow, vC].CellData);
                        FRows[aStartRow][vC].CellData.Dispose();
                        FRows[aStartRow][vC].CellData = null;
                    }

                    FRows[aStartRow][vC].ColSpan = aStartCol - vC;
                }

                FRows[aStartRow][aStartCol].ColSpan = vEndCol - aStartCol;  // 合并源增加

                DeleteEmptyCols(aStartCol + 1, vEndCol);
                Result = true;
            }
            else
                if (aStartCol == vEndCol)  // 同列合并
                {
                    for (int vR = aStartRow + 1; vR <= vEndRow; vR++)  // 合并各行
                    {
                        if (FRows[vR][aStartCol].CellData != null)
                        {
                            FRows[aStartRow][aStartCol].CellData.AddData(FRows[vR][aStartCol].CellData);
                            FRows[vR][aStartCol].CellData.Dispose();
                            FRows[vR][aStartCol].CellData = null;
                        }

                        FRows[vR][aStartCol].RowSpan = aStartRow - vR;
                    }

                    FRows[aStartRow][aStartCol].RowSpan = vEndRow - aStartRow;

                    DeleteEmptyRows(aStartRow + 1, vEndRow);
                    Result = true;
                }
                else  // 不同行，不同列
                {
                    for (int vC = aStartCol + 1; vC <= vEndCol; vC++)  // 起始行各列合
                    {
                        if (FRows[aStartRow][vC].CellData != null)
                        {
                            FRows[aStartRow][aStartCol].CellData.AddData(FRows[aStartRow][vC].CellData);
                            FRows[aStartRow][vC].CellData.Dispose();
                            FRows[aStartRow][vC].CellData = null;
                        }

                        FRows[aStartRow][vC].RowSpan = 0;
                        FRows[aStartRow][vC].ColSpan = aStartCol - vC;
                    }

                    for (int vR = aStartRow + 1; vR <= vEndRow; vR++)  // 剩余行各列合
                    {
                        for (int vC = aStartCol; vC <= vEndCol; vC++)
                        {
                            if (FRows[vR][vC].CellData != null)
                            {
                                FRows[aStartRow][aStartCol].CellData.AddData(FRows[vR][vC].CellData);
                                FRows[vR][vC].CellData.Dispose();
                                FRows[vR][vC].CellData = null;
                            }

                            FRows[vR][vC].ColSpan = aStartCol - vC;
                            FRows[vR][vC].RowSpan = aStartRow - vR;
                        }
                    }

                    FRows[aStartRow][aStartCol].RowSpan = vEndRow - aStartRow;
                    FRows[aStartRow][aStartCol].ColSpan = vEndCol - aStartCol;

                    DeleteEmptyRows(aStartRow + 1, vEndRow);
                    // 删除空列
                    DeleteEmptyCols(aStartCol + 1, vEndCol);

                    Result = true;
                }

            return Result;
        }

        protected HCTableCell GetCells(int aRow, int aCol)
        {
            return FRows[aRow][aCol];
        }

        protected int GetColWidth(int aIndex)
        {
            return FColWidths[aIndex];
        }

        protected void SetColWidth(int aIndex, int aWidth)
        {
            FColWidths[aIndex] = aWidth;
        }

        protected bool InsertCol(int aCol, int aCount)
        {
            // TODO : 根据各行当前列平均减少一定的宽度给要插入的列
            int viDestRow = -1, viDestCol = -1;
            int vWidth = HC.MinColWidth - FBorderWidth;
            for (int i = 0; i <= aCount - 1; i++)
            {
                for (int vRow = 0; vRow <= RowCount - 1; vRow++)
                {
                    HCTableCell vCell = new HCTableCell(OwnerData.Style);
                    vCell.Width = vWidth;
                    InitializeCellData(vCell.CellData);

                    if ((aCol < FColWidths.Count) && (FRows[vRow][aCol].ColSpan < 0))
                    {
                        GetDestCell(vRow, aCol, ref viDestRow, ref viDestCol);  // 目标行列

                        // 新插入的列在当前列后面，也做为被合并的列
                        vCell.CellData.Dispose();
                        vCell.CellData = null;
                        vCell.RowSpan = FRows[vRow][aCol].RowSpan;
                        vCell.ColSpan = FRows[vRow][aCol].ColSpan;

                        for (int j = aCol; j <= viDestCol + this[viDestRow, viDestCol].ColSpan; j++)  // 后续列离目标远
                            FRows[vRow][j].ColSpan = FRows[vRow][j].ColSpan - 1;  // 离目标列远1

                        if (vRow == viDestRow + FRows[viDestRow][viDestCol].RowSpan)
                            FRows[viDestRow][viDestCol].ColSpan = FRows[viDestRow][viDestCol].ColSpan + 1;
                    }

                    FRows[vRow].Insert(aCol, vCell);
                }

                FColWidths.Insert(aCol, vWidth);  // 右侧插入列
            }

            this.InitializeMouseInfo();
            FSelectCellRang.Initialize();
            this.SizeChanged = true;
            FLastChangeFormated = false;

            return true;
        }

        protected bool InsertRow(int aRow, int aCount)
        {
            int viDestRow = -1, viDestCol = -1;

            for (int i = 0; i <= aCount - 1; i++)
            {
                HCTableRow vTableRow = new HCTableRow(OwnerData.Style, FColWidths.Count);
                for (int vCol = 0; vCol <= FColWidths.Count - 1; vCol++)
                {
                    vTableRow[vCol].Width = FColWidths[vCol];

                    if ((aRow < FRows.Count) && (FRows[aRow][vCol].RowSpan < 0))
                    {
                        GetDestCell(aRow, vCol, ref viDestRow, ref viDestCol);

                        vTableRow[vCol].CellData.Dispose();
                        vTableRow[vCol].CellData = null;
                        vTableRow[vCol].RowSpan = FRows[aRow][vCol].RowSpan;
                        vTableRow[vCol].ColSpan = FRows[aRow][vCol].ColSpan;

                        for (int j = aRow; j <= viDestRow + FRows[viDestRow][viDestCol].RowSpan; j++)  // 目标的行跨度 - 已经跨
                            FRows[j][vCol].RowSpan = FRows[j][vCol].RowSpan - 1;  // 离目标行远1

                        if (vCol == viDestCol + FRows[viDestRow][viDestCol].ColSpan)
                            FRows[viDestRow][viDestCol].RowSpan = FRows[viDestRow][viDestCol].RowSpan + 1;  // 目标行包含的合并源增加1
                    }
                }

                FRows.Insert(aRow, vTableRow);
            }
            this.InitializeMouseInfo();
            FSelectCellRang.Initialize();
            this.SizeChanged = true;
            FLastChangeFormated = false;

            return true;
        }

        protected bool DeleteCol(int aCol)
        {
            if (!ColCanDelete(aCol))
                return false;

            int viDestRow = -1, viDestCol = -1;
            for (int vRow = 0; vRow <= RowCount - 1; vRow++)
            {
                if (FRows[vRow][aCol].ColSpan < 0)
                {
                    GetDestCell(vRow, aCol, ref viDestRow, ref viDestCol);  // 目标行、列
                    for (int i = aCol; i <= viDestCol + FRows[viDestRow][viDestCol].ColSpan; i++)  // 当前列右面的合并源列离目标近
                        FRows[vRow][i].ColSpan = FRows[vRow][i].ColSpan + 1;

                    if (vRow == viDestRow + FRows[viDestRow][viDestCol].RowSpan)
                        FRows[viDestRow][viDestCol].ColSpan = FRows[viDestRow][viDestCol].ColSpan - 1;
                }
                else
                    if (FRows[vRow][aCol].ColSpan > 0)
                    {
                    }

                FRows[vRow].Delete(aCol);
            }

            FColWidths.RemoveAt(aCol);
            CheckFixColSafe(aCol);
            this.InitializeMouseInfo();
            FSelectCellRang.Initialize();
            this.SizeChanged = true;
            FLastChangeFormated = false;
            return true;
        }

        protected bool DeleteRow(int aRow)
        {
            if (!RowCanDelete(aRow))
                return false;

            int viDestRow = -1, viDestCol = -1;
            for (int vCol = 0; vCol <= FColWidths.Count - 1; vCol++)
            {
                if (FRows[aRow][vCol].RowSpan < 0)
                {
                    GetDestCell(aRow, vCol, ref viDestRow, ref viDestCol);  // 目标行、列
                    for (int i = aRow; i <= viDestRow + FRows[viDestRow][viDestCol].RowSpan; i++)  // 当前行下面的合并源行离目标近
                        FRows[i][vCol].RowSpan = FRows[i][vCol].RowSpan + 1;

                    if (vCol == viDestCol + FRows[viDestRow][viDestCol].ColSpan)
                        FRows[viDestRow][viDestCol].RowSpan = FRows[viDestRow][viDestCol].RowSpan - 1;
                }
                else
                    if (FRows[aRow][vCol].ColSpan > 0)
                    {
                    }
            }

            FRows.Delete(aRow);
            CheckFixRowSafe(aRow);
            this.InitializeMouseInfo();
            FSelectCellRang.Initialize();
            this.SizeChanged = true;
            FLastChangeFormated = false;
            return true;
        }

        public HCTableItem(HCCustomData aOwnerData, int aRowCount, int aColCount, int aWidth)
            : base(aOwnerData)
        {
            if (aRowCount == 0)
                throw new Exception("异常：不能创建行数为0的表格！");
            if (aColCount == 0)
                throw new Exception("异常：不能创建列数为0的表格！");

            GripSize = 2;
            FFixCol = -1;
            FFixColCount = 0;
            FFixRow = -1;
            FFixRowCount = 0;
            FCellHPadding = 2;
            FCellVPadding = 2;
            FFormatHeight = 0;
            FDraging = false;
            FBorderWidth = 1;
            FBorderColor = Color.Black;
            FBorderVisible = true;

            StyleNo = HCStyle.Table;
            ParaNo = OwnerData.CurParaNo;
            CanPageBreak = true;
            FPageBreaks = new List<PageBreak>();

            //FWidth := FRows[0].ColCount * (MinColWidth + FBorderWidth) + FBorderWidth;
            Height = aRowCount * (HC.MinRowHeight + FBorderWidth) + FBorderWidth;
            FRows = new HCTableRows();
            FRows.OnRowAdd = DoRowAdd;  // 添加行时触发的事件
            FSelectCellRang = new View.SelectCellRang();
            this.InitializeMouseInfo();
            //
            int vDataWidth = aWidth - (aColCount + 1) * FBorderWidth;
            for (int i = 0; i <= aRowCount - 1; i++)
            {
                HCTableRow vRow = new HCTableRow(OwnerData.Style, aColCount);
                vRow.SetRowWidth(vDataWidth);
                FRows.Add(vRow);
            }
            FColWidths = new List<int>();
            for (int i = 0; i <= aColCount - 1; i++)
                FColWidths.Add(FRows[0][i].Width);

            FMangerUndo = true;  // 自己管理自己的撤销和恢复操作
            FLastChangeFormated = false;
        }

        ~HCTableItem()
        {

        }

        public override void Dispose()
        {
            FSelectCellRang.Dispose();
            //FPageBreaks.Free;
            FRows.Clear();
            //FRows.Free;
            //FColWidths.Free;
            base.Dispose();
        }

        public override void Assign(HCCustomItem source)
        {
            // 必需保证行、列数量一致
            base.Assign(source);
            HCTableItem vSrcTable = source as HCTableItem;

            FBorderVisible = vSrcTable.BorderVisible;
            FBorderWidth = vSrcTable.BorderWidth;
            FFixRow = vSrcTable.FixRow;
            FFixRowCount = vSrcTable.FixRowCount;
            FFixCol = vSrcTable.FixCol;
            FFixColCount = vSrcTable.FixColCount;

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

        /// <summary> 获取表格在指定高度内的结束位置处行中最下端(暂时没用到注释了) </summary>
        /// <param name="AHeight">指定的高度范围</param>
        /// <param name="ADItemMostBottom">最后一行最底端DItem的底部位置</param>
        public override void DblClick(int x, int y)
        {
            if (FSelectCellRang.EditCell())
            {
                POINT vPt = GetCellPostion(FSelectCellRang.StartRow, FSelectCellRang.StartCol);
                this[FSelectCellRang.StartRow, FSelectCellRang.StartCol].CellData.DblClick(
                    x - vPt.X - FCellHPadding, y - vPt.Y - FCellVPadding);
            }
            else
                base.DblClick(x, y);
        }

        public override bool CoordInSelect(int x, int y)
        {
            bool Result = base.CoordInSelect(x, y);  // 有选中且在RectItem区域中(粗略估算)
            if (Result)
            {
                int vRow = -1, vCol = -1;
                ResizeInfo vResizeInfo = GetCellAt(x, y, ref vRow, ref vCol);  // 坐标处信息
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
                                int vX = x - vCellPt.X - FCellHPadding;
                                int vY = y - vCellPt.Y - FCellVPadding;

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

        public override HCCustomData GetTopLevelDataAt(int x, int y)
        {
            HCCustomData Result = null;
            int vRow = -1, vCol = -1;
            ResizeInfo vResizeInfo = GetCellAt(x, y, ref vRow, ref vCol);
            if ((vRow < 0) || (vCol < 0))
                return Result;

            POINT vCellPt = GetCellPostion(vRow, vCol);
            Result = (FRows[vRow][vCol].CellData as HCRichData).GetTopLevelDataAt(
                x - vCellPt.X - FCellHPadding, y - vCellPt.Y - FCellVPadding);

            return Result;
        }

        public override HCCustomData GetTopLevelData()
        {
            HCTableCell vCell = GetEditCell();

            if (vCell != null)
                return vCell.CellData.GetTopLevelData();
            else
                return base.GetTopLevelData();
        }

        public override HCCustomData GetActiveData()
        {
            HCTableCell vCell = GetEditCell();
            if (vCell != null)
                return vCell.CellData.GetTopLevelData();
            else
                return base.GetActiveData();
        }

        public override HCCustomItem GetActiveItem()
        {
            HCTableCell vCell = GetEditCell();
            if (vCell != null)
                return vCell.CellData.GetActiveItem();
            else
                return base.GetActiveItem();
        }

        public override HCCustomItem GetTopLevelItem()
        {
            HCTableCell vCell = GetEditCell();
            if (vCell != null)
                return vCell.CellData.GetTopLevelItem();
            else
                return base.GetTopLevelItem();
        }

        public override HCCustomDrawItem GetActiveDrawItem()
        {
            HCTableCellData vCellData = GetActiveData() as HCTableCellData;
            if (vCellData != null)
                return vCellData.GetTopLevelDrawItem();
            else
                return base.GetActiveDrawItem();
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

        public override bool InsertText(string aText)
        {
            bool vResult = false;

            if (FSelectCellRang.EditCell())
            {
                HCProcedure vEvent = delegate()
                {
                    HCTableCell vEditCell = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol];
                    vResult = vEditCell.CellData.InsertText(aText);
                };

                CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);
                return vResult;
            }
            else
                return base.InsertText(aText);
        }

        public override bool InsertItem(HCCustomItem aItem)
        {
            HCTableCell vCell = GetEditCell();
            if (vCell == null)
                return false;

            bool Result = false;
            HCProcedure vEvent = delegate()
            {
                Result = vCell.CellData.InsertItem(aItem);
            };

            CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);

            return Result;
        }

        public override bool InsertStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            bool vResult = false;

            if (FSelectCellRang.EditCell())
            {
                HCProcedure vEvent = delegate ()
                {
                    HCTableCell vEditCell = this[FSelectCellRang.StartRow, FSelectCellRang.StartCol];
                    vResult = vEditCell.CellData.InsertStream(aStream, aStyle, aFileVersion);
                };

                CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);
                return vResult;
            }
            else
                return base.InsertStream(aStream, aStyle, aFileVersion);
        }

        public override void ReFormatActiveItem()
        {
            if (FSelectCellRang.EditCell())
            {
                HCProcedure vEvent = delegate()
                {
                    HCTableCell vEditCell = FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol];
                    vEditCell.CellData.ReFormatActiveItem();
                };

                CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);
            }
        }

        public override void ReAdaptActiveItem()
        {
            if (FSelectCellRang.EditCell())
            {
                HCProcedure vEvent = delegate()
                {
                    HCTableCell vEditCell = FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol];
                    vEditCell.CellData.ReAdaptActiveItem();
                };

                CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);
            }
        }

        public override bool DeleteActiveDomain()
        {
            bool Result = base.DeleteActiveDomain();

            if (FSelectCellRang.EditCell())
            {
                HCProcedure vEvent = delegate()
                {
                    HCTableCell vEditCell = FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol];
                    Result = vEditCell.CellData.DeleteActiveDomain();
                };

                CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);
            }

            return Result;
        }

        public override void DeleteActiveDataItems(int aStartNo, int aEndNo)
        {
            if (FSelectCellRang.EditCell())
            {
                HCProcedure vEvent = delegate()
                {
                    HCTableCell vEditCell = FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol];
                    vEditCell.CellData.DeleteActiveDataItems(aStartNo, aEndNo);
                };

                CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);
            }
        }

        public override void SetActiveItemText(string aText)
        {
            base.SetActiveItemText(aText);

            if (FSelectCellRang.EditCell())
            {
                HCProcedure vEvent = delegate()
                {
                    HCTableCell vEditCell = FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol];
                    vEditCell.CellData.SetActiveItemText(aText);
                };

                CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);
            }
        }

        #region KeyDown子方法

        private bool DoCrossCellKey(int aKey)
        {
            bool Result = false;
            int vRow = -1, vCol = -1;
            HCTableCell vEditCell = null;

            if (aKey == User.VK_LEFT)
            {
                if (vEditCell.CellData.SelectFirstItemOffsetBefor())
                {
                    // 找左侧单元格
                    for (int i = FSelectCellRang.StartCol; i >= 0; i--)
                    {
                        if (FRows[FSelectCellRang.StartRow][i].ColSpan >= 0)
                        {
                            if (FRows[FSelectCellRang.StartRow][i].RowSpan < 0)
                                FSelectCellRang.StartRow = FSelectCellRang.StartRow + FRows[FSelectCellRang.StartRow][i].RowSpan;

                            vCol = i;
                            break;
                        }
                    }

                    if (vCol >= 0)
                    {
                        FSelectCellRang.StartCol = vCol;
                        FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol].CellData.SelectLastItemAfterWithCaret();
                        Result = true;
                    }
                }
            }
            else
                if (aKey == User.VK_RIGHT)
                {
                    if (vEditCell.CellData.SelectLastItemOffsetAfter())
                    {
                        // 找右侧单元格
                        for (int i = FSelectCellRang.StartCol; i <= FColWidths.Count - 1; i++)
                        {
                            if (this[FSelectCellRang.StartRow, i].ColSpan >= 0)
                            {
                                if (FRows[FSelectCellRang.StartRow][i].RowSpan < 0)
                                    FSelectCellRang.StartRow = FSelectCellRang.StartRow + FRows[FSelectCellRang.StartRow][i].RowSpan;

                                vCol = i;
                                break;
                            }
                        }

                        if (vCol >= 0)
                        {
                            FSelectCellRang.StartCol = vCol;
                            FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol].CellData.SelectFirstItemBeforWithCaret();
                            Result = true;
                        }
                    }
                }
                else
                    if (aKey == User.VK_UP)
                    {
                        if ((vEditCell.CellData.SelectFirstLine()) && (FSelectCellRang.StartRow > 0))
                        {
                            GetDestCell(FSelectCellRang.StartRow - 1, FSelectCellRang.StartCol, ref vRow, ref vCol);
                            if ((vRow >= 0) && (vCol >= 0))
                            {
                                FSelectCellRang.StartRow = vRow;
                                FSelectCellRang.StartCol = vCol;
                                FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol].CellData.SelectLastItemAfterWithCaret();
                                Result = true;
                            }
                        }
                    }
                    else
                        if (aKey == User.VK_DOWN)
                        {
                            if ((vEditCell.CellData.SelectLastLine()) && (FSelectCellRang.StartRow < this.RowCount - 1))
                            {
                                GetDestCell(FSelectCellRang.StartRow + 1, FSelectCellRang.StartCol, ref vRow, ref vCol);
                                if ((vRow >= 0) && (vCol >= 0))
                                {
                                    FSelectCellRang.StartRow = vRow;
                                    FSelectCellRang.StartCol = vCol;
                                    FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol].CellData.SelectFirstItemBeforWithCaret();
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
                bool vOldKey = e.Handled;
                switch (e.KeyValue)
                {
                    case User.VK_BACK:
                    case User.VK_DELETE:
                    case User.VK_RETURN:
                    case User.VK_TAB:
                        HCProcedure vEvent = delegate()
                        {
                            vEditCell.CellData.KeyDown(e);
                        };

                        CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);
                        break;

                    case User.VK_LEFT:
                    case User.VK_RIGHT:
                    case User.VK_UP:
                    case User.VK_DOWN:
                    case User.VK_HOME:
                    case User.VK_END:
                        vEditCell.CellData.KeyDown(e);
                        if ((e.Handled) && (HC.IsDirectionKey(e.KeyValue)))
                        {
                            if (DoCrossCellKey(e.KeyValue))
                            {
                                OwnerData.Style.UpdateInfoReCaret();
                                e.Handled = vOldKey;
                            }
                        }
                        break;
                }
            }
            else
                e.Handled = true; ;
        }

        public override void KeyPress(ref Char key)
        {
            HCTableCell vEditCell = GetEditCell();
            if (vEditCell != null)
            {
                Char vOldKey = key;

                HCProcedure vEvent = delegate()
                {
                    vEditCell.CellData.KeyPress(ref vOldKey);
                };

                CellChangeByAction(FSelectCellRang.StartRow, FSelectCellRang.StartCol, vEvent);

                key = vOldKey;
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

        public override void TraverseItem(HCItemTraverse aTraverse)
        {
            for (int vR = 0; vR <= FRows.Count - 1; vR++)
            {
                if (aTraverse.Stop)
                    break;

                for (int vC = 0; vC <= FColWidths.Count - 1; vC++)
                {
                    if (aTraverse.Stop)
                        break;

                    if (this[vR, vC].CellData != null)
                        this[vR, vC].CellData.TraverseItem(aTraverse);
                }
            }
        }

        /// <summary> 当前位置开始查找指定的内容 </summary>
        /// <param name="aKeyword">要查找的关键字</param>
        /// <param name="aForward">True：向前，False：向后</param>
        /// <param name="aMatchCase">True：区分大小写，False：不区分大小写</param>
        /// <returns>True：找到</returns>
        public override bool Search(string aKeyword, bool aForward, bool aMatchCase)
        {
            bool Result = false;
            int vRow = -1, vCol = -1;
            HCTableCellData vCellData = null;
            if (aForward)
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
                        vCellData.SelectInfo.StartItemOffset = vCellData.GetItemOffsetAfter(vCellData.Items.Count - 1);
                    }
                }

                vRow = FSelectCellRang.StartRow;
                vCol = FSelectCellRang.StartCol;

                if ((vRow >= 0) && (vCol >= 0))
                {
                    if (this[vRow, vCol].CellData != null)
                        Result = this[vRow, vCol].CellData.Search(aKeyword, aForward, aMatchCase);

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
                                vCellData.SelectInfo.StartItemOffset = vCellData.GetItemOffsetAfter(vCellData.Items.Count - 1);

                                Result = this[vRow, j].CellData.Search(aKeyword, aForward, aMatchCase);
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
                                if ((FRows[i][j].ColSpan < 0) || (FRows[i][j].RowSpan < 0))
                                    continue;
                                else
                                {
                                    vCellData = FRows[i][j].CellData;
                                    vCellData.SelectInfo.StartItemNo = vCellData.Items.Count - 1;
                                    vCellData.SelectInfo.StartItemOffset = vCellData.GetItemOffsetAfter(vCellData.Items.Count - 1);

                                    Result = FRows[i][j].CellData.Search(aKeyword, aForward, aMatchCase);
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
                    FRows[0][0].CellData.SelectInfo.StartItemNo = 0;
                    FRows[0][0].CellData.SelectInfo.StartItemOffset = 0;
                }

                vRow = FSelectCellRang.StartRow;
                vCol = FSelectCellRang.StartCol;

                if ((vRow >= 0) && (vCol >= 0))
                {
                    Result = FRows[vRow][vCol].CellData.Search(aKeyword, aForward, aMatchCase);
                    if (!Result)
                    {
                        for (int j = vCol; j <= FColWidths.Count - 1; j++)  // 在同行后面的单元格
                        {
                            if ((FRows[vRow][j].ColSpan < 0) || (FRows[vRow][j].RowSpan < 0))
                                continue;
                            else
                            {
                                FRows[vRow][j].CellData.SelectInfo.StartItemNo = 0;
                                FRows[vRow][j].CellData.SelectInfo.StartItemOffset = 0;
                                Result = FRows[vRow][j].CellData.Search(aKeyword, aForward, aMatchCase);
                            }

                            if (Result)
                            {
                                FSelectCellRang.StartCol = j;
                                break;
                            }
                        }
                    }

                    if (!Result)  // 同行后面的单元格没找到
                    {
                        for (int i = FSelectCellRang.StartRow; i <= FRows.Count - 1; i++)
                        {
                            for (int j = 0; j <= FColWidths.Count - 1; j++)
                            {
                                if ((FRows[i][j].ColSpan < 0) || (FRows[i][j].RowSpan < 0))
                                    continue;
                                else
                                {
                                    FRows[i][j].CellData.SelectInfo.StartItemNo = 0;
                                    FRows[i][j].CellData.SelectInfo.StartItemOffset = 0;
                                    Result = FRows[i][j].CellData.Search(aKeyword, aForward, aMatchCase);
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

        // 继承TCustomRectItem抽象方法
        public override void ApplySelectTextStyle(HCStyle aStyle, HCStyleMatch aMatchStyle)
        {
            HCTableCellData vData = null;

            if (FSelectCellRang.EditCell())  // 在同一单元格中编辑
            {
                vData = GetEditCell().CellData;
                vData.ApplySelectTextStyle(aMatchStyle);
                this.SizeChanged = vData.FormatHeightChange | vData.FormatDrawItemCountChange;
            }
            else
            if (FSelectCellRang.StartRow >= 0)  // 有选择起始行
            {
                for (int vR = FSelectCellRang.StartRow; vR <= FSelectCellRang.EndRow; vR++)
                {
                    for (int vC = FSelectCellRang.StartCol; vC <= FSelectCellRang.EndCol; vC++)
                    {
                        vData = FRows[vR][vC].CellData;
                        if (vData != null)
                        {
                            if (this.SizeChanged)
                            {
                                vData.BeginFormat();
                                try
                                {
                                    vData.ApplySelectTextStyle(aMatchStyle);
                                }
                                finally
                                {
                                    vData.EndFormat(false);
                                }
                            }
                            else
                            {
                                vData.ApplySelectTextStyle(aMatchStyle);
                                this.SizeChanged = vData.FormatHeightChange | vData.FormatDrawItemCountChange;
                            }
                        }
                    }
                }
            }

            FLastChangeFormated = !this.SizeChanged;
        }

        public override void ApplySelectParaStyle(HCStyle aStyle, HCParaMatch aMatchStyle)
        {
            base.ApplySelectParaStyle(aStyle, aMatchStyle);

            if (FSelectCellRang.StartRow >= 0)
            {
                HCTableCellData vData = null;

                if (FSelectCellRang.EndRow >= 0)
                {
                    for (int vR = FSelectCellRang.StartRow; vR <= FSelectCellRang.EndRow; vR++)
                    {
                        for (int vC = FSelectCellRang.StartCol; vC <= FSelectCellRang.EndCol; vC++)
                        {
                            vData = FRows[vR][vC].CellData;
                            if (vData != null)
                            {
                                if (this.SizeChanged)
                                {
                                    vData.BeginFormat();
                                    try
                                    {
                                        vData.ApplySelectParaStyle(aMatchStyle);
                                    }
                                    finally
                                    {
                                        vData.EndFormat(false);
                                    }
                                }
                                else
                                {
                                    vData.ApplySelectParaStyle(aMatchStyle);
                                    this.SizeChanged = vData.FormatHeightChange | vData.FormatDrawItemCountChange;
                                }
                            }
                        }
                    }
                }
                else  // 在同一单元格
                {
                    vData = GetEditCell().CellData;
                    vData.ApplySelectParaStyle(aMatchStyle);
                    this.SizeChanged = vData.FormatHeightChange | vData.FormatDrawItemCountChange;
                }

                FLastChangeFormated = !this.SizeChanged;
            }
            else
                this.ParaNo = aMatchStyle.GetMatchParaNo(OwnerData.Style, this.ParaNo);
        }

        #region ApplyContentAlign子方法
        private void ApplyCellAlign_(HCTableCell aCell, HCContentAlign aAlign)
        {
            switch (aAlign)
            {
                case HCContentAlign.tcaTopLeft:
                case HCContentAlign.tcaTopCenter:
                case HCContentAlign.tcaTopRight:
                    aCell.AlignVert = HCAlignVert.cavTop;
                    break;

                case HCContentAlign.tcaCenterLeft:
                case HCContentAlign.tcaCenterCenter:
                case HCContentAlign.tcaCenterRight:
                    aCell.AlignVert = HCAlignVert.cavCenter;
                    break;

                default:
                    aCell.AlignVert = HCAlignVert.cavBottom;
                    break;
            }

            HCTableCellData vData = aCell.CellData;
            if (vData != null)
            {
                vData.BeginFormat();
                try
                {
                    switch(aAlign)
                    {
                        case HCContentAlign.tcaTopLeft:
                        case HCContentAlign.tcaCenterLeft:
                        case HCContentAlign.tcaBottomLeft:
                            vData.ApplyParaAlignHorz(ParaAlignHorz.pahLeft);
                            break;
                        case HCContentAlign.tcaTopCenter:
                        case HCContentAlign.tcaCenterCenter:
                        case HCContentAlign.tcaBottomCenter:
                            vData.ApplyParaAlignHorz(ParaAlignHorz.pahCenter);
                            break;

                        default:
                            vData.ApplyParaAlignHorz(ParaAlignHorz.pahRight);
                            break;
                    }
                }
                finally
                {
                    vData.EndFormat(true);
                }
            }
        }
        #endregion
        public override void ApplyContentAlign(HCContentAlign aAlign)
        {
            base.ApplyContentAlign(aAlign);

            if (FSelectCellRang.StartRow >= 0)
            {
                if (FSelectCellRang.EndRow >= 0)
                {
                    for (int vR = FSelectCellRang.StartRow; vR <= FSelectCellRang.EndRow; vR++)
                    {
                        for (int vC = FSelectCellRang.StartCol; vR <= FSelectCellRang.EndCol; vC++)
                            ApplyCellAlign_(FRows[vR][vC], aAlign);
                    }
                }
                else
                    ApplyCellAlign_(GetEditCell(), aAlign);
            }
        }

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            if (FLastChangeFormated)
            {
                ClearFormatExtraHeight();
                return;
            }

            for (int vR = 0; vR <= RowCount - 1; vR++)  // 格式化各
            {
                FormatRow(vR);  // 格式化行，并计算行高度
                CalcRowCellHeight(vR);  // 以行中所有无行合并操作列中最大高度更新其他列
            }

            FLastChangeFormated = false;

            CalcMergeRowHeightFrom(0);

            this.Height = GetFormatHeight();
            this.Width = GetFormatWidth();
        }

        /// <summary> 正在其上时内部是否处理指定的Key和Shif </summary>
        public override bool WantKeyDown(KeyEventArgs e)
        {
            return true;
        }

        #region CheckFormatPageBreak子方法
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
        /// <param name="aDrawItemRectTop">表格对应的DrawItem的Rect.Top</param>
        /// <param name="aDrawItemRectTop">表格对应的DrawItem的Rect.Bottom</param>
        /// <param name="aPageDataFmtTop">当前页的数据顶部位置</param>
        /// <param name="aPageDataFmtBottom">当前页的数据底部位置</param>
        /// <param name="ACheckRow">当前页从哪行开始排版</param>
        /// <param name="aBreakRow">当前页最后分页于哪行</param>
        /// <param name="aFmtOffset">表格对应的DrawItem向下整体偏移的量</param>
        /// <param name="aCellMaxInc">返回当前页各列为了避开分页位置额外偏移的最大高度(参数原名AFmtHeightInc为便于分析重命名)</param>
        public override void CheckFormatPageBreak(int aPageIndex, int aDrawItemRectTop,
            int aDrawItemRectBottom, int aPageDataFmtTop, int aPageDataFmtBottom, int aStartRow,
            ref int aBreakRow, ref int aFmtOffset, ref int aCellMaxInc)
        {
            aBreakRow = -1;
            aFmtOffset = 0;
            aCellMaxInc = 0;  // vCellInc的最大值
            
            // 得到起始行的Fmt起始位置
            int vBreakRowFmtTop = aDrawItemRectTop + FBorderWidth - 1;  // 第1行排版位置(上边框线结束位置)，因为边框在ADrawItemRectTop也占1像素，所以要减掉
            for (int vR1 = 0; vR1 <= aStartRow - 1; vR1++)
                vBreakRowFmtTop = vBreakRowFmtTop + FRows[vR1].FmtOffset + FRows[vR1].Height + FBorderWidth;  // 第i行结束位置(含下边框结束位置)
            
            // 从起始行开始检测当前页是否能放完表格
            int vR = aStartRow, vBreakRowBottom = 0;
            while (vR < RowCount)  // 遍历每一行
            {
                vBreakRowBottom = vBreakRowFmtTop + FRows[vR].FmtOffset + FRows[vR].Height + FBorderWidth;  // 第i行结束位置(含下边框结束位置)
                if (vBreakRowBottom > aPageDataFmtBottom)
                {
                    aBreakRow = vR;  // 第i行需要处理分页
                    break;
                }
                vBreakRowFmtTop = vBreakRowBottom;  // 第i+1行起始位置(上边框结束位置)

                vR++;
            }

            if (aBreakRow < 0)
                return;

            if ((!this.CanPageBreak) && (aBreakRow == 0))
            {
                aFmtOffset = aPageDataFmtBottom - aDrawItemRectTop;
                return;
            }

            bool vFirstLinePlace = true;
            int vPageBreakBottom = aPageDataFmtBottom;
            
            int vDestRow = -1, vDestCol = -1, vDestCellDataFmtTop = 0;
            HCTableCellData vCellData = null;
            HCCustomDrawItem vDrawItem = null;

            // 先判断是不是有单元格里第一行内容就放不下，需要整体下移
            for (int vC = 0; vC <= FRows[aBreakRow].ColCount - 1; vC++)  // 遍历所有单元格中DrawItem，找从哪个开始向下偏移及偏移
            {
                if (FRows[aBreakRow][vC].ColSpan < 0)
                    continue;

                GetDestCell(aBreakRow, vC, ref vDestRow, ref vDestCol);
                vCellData = FRows[vDestRow][vDestCol].CellData;
                
                // 计算目标单元格数据起始位置
                vDestCellDataFmtTop = vBreakRowFmtTop + FCellVPadding;  // 先分页行起始位置(上边框结束位置)

                if (aBreakRow != vDestRow)
                    vDestCellDataFmtTop = vDestCellDataFmtTop - SrcCellDataTopDistanceToDest(aBreakRow, vDestRow);
                
                // 判断合并目标内容在当前分页行的分页位置
                for (int i = 0; i <= vCellData.DrawItems.Count - 1; i++)
                {
                    vDrawItem = vCellData.DrawItems[i];
                    if (!vDrawItem.LineFirst)
                        continue;

                    if (vDestCellDataFmtTop + vDrawItem.Rect.Bottom + FCellVPadding + FBorderWidth > aPageDataFmtBottom)
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

            for (int vC = 0; vC <= FRows[aBreakRow].ColCount - 1; vC++)  // 遍历所有单元格中DrawItem，找从哪个开始向下偏移及偏移
            {
                if (FRows[aBreakRow][vC].ColSpan < 0)
                    continue;

                GetDestCell(aBreakRow, vC, ref vDestRow, ref vDestCol);
                vCellData = FRows[vDestRow][vDestCol].CellData;
                vLastDFromRowBottom =  // 原最后一个DrawItem底部距离行底部的空白距离(不含底部的FCellVPadding)
                    FRows[vDestRow][vDestCol].Height - (FCellVPadding + vCellData.Height + FCellVPadding);
                    
                // 计算目标单元格数据起始位置
                vDestCellDataFmtTop = vBreakRowFmtTop;  // 先分页行起始位置(上边框结束位置)
                if (aBreakRow != vDestRow)  // 恢复到目标单元格
                    vDestCellDataFmtTop = vDestCellDataFmtTop - SrcCellDataTopDistanceToDest(aBreakRow, vDestRow);
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
                        vH = aPageDataFmtBottom - (vDestCellDataFmtTop + vDrawItem.Rect.Top) // 页Data底部 - 当前DrawItem在页的相对位置
                            + FBorderWidth + FCellVPadding - 1;  // 预留出顶部边框和FCellVPadding，因为边框在APageDataFmtBottom也占1像素，所以要减掉
                       
                        // 单元格实际增加的高度 = DrawItem分页向下偏移的距离 - 原最后一个DrawItem底部距离行底部的空白距离(不含底部的FCellVPadding)
                        if (vH > vLastDFromRowBottom)
                            vCellInc = vH - vLastDFromRowBottom;
                        else  // 偏移量让底部空白抵消了
                            vCellInc = 0;
                        
                        vColCross.DrawItemNo = i;  // 从第j个DrawItem处开始分页
                        vColCross.VDrawOffset = vH;  // DrawItem分页偏移，注意，DrawItem向下偏移和单元格增加的高并不一定相等，如原底部有空白时，单元格增加高度<Draw向下偏移
                        
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
            
                if (aCellMaxInc < vCellInc)
                    aCellMaxInc = vCellInc;  // 记录各列中分页向下偏移的最大增量
            
                vColCrosses.Add(vColCross);
            }
            
            vRowBreakSeat = vRowBreakSeat - aDrawItemRectTop + 1;  // 起始为1，截断为2，截断位置是2所以要增加1

            int vFixHeight = 0;
            if ((FFixRow >= 0) && (aBreakRow > FFixRow + FFixRowCount - 1))
            {
                vFixHeight = GetFixRowHeight();
                aCellMaxInc = aCellMaxInc + vFixHeight;
            }

            if (!vFirstLinePlace)
            {
                if (aBreakRow == 0)
                {
                    aFmtOffset = aPageDataFmtBottom - aDrawItemRectTop;
                    aCellMaxInc = 0;  // 整体向下偏移时，就代表了第一行的向下偏移，或者说第一行的FmtOffset永远是0，因为整体向下偏移的依据是判断第一行
                    return;
                }

                // 偏移量增加到此行高度
                for (int i = 0; i <= vColCrosses.Count - 1; i++)  // vColCrosses里只有源或普通单元
                {
                    if ((vColCrosses[i].VDrawOffset > 0) && (vColCrosses[i].DrawItemNo == 0))
                    {
                        FRows[aBreakRow].FmtOffset = vColCrosses[i].VDrawOffset + vFixHeight;  // 表格行向下偏移后整行起始在下一页显示，同行多个单元都放不下第一个时会重复赋相同值
                        vColCrosses[i].VDrawOffset = 0;
                    }
                }
            }
            else
                FRows[aBreakRow].Height = FRows[aBreakRow].Height + aCellMaxInc;
            
            AddPageBreak(aBreakRow, vRowBreakSeat, aPageIndex, aPageDataFmtBottom, aDrawItemRectTop);
            
            for (int vC = 0; vC <= vColCrosses.Count - 1; vC++)  // 遍历所有内容向下有偏移的单元格，将偏移扩散到分页后面的DrawIte
            {
                if ((vColCrosses[vC].DrawItemNo < 0) || (vColCrosses[vC].VDrawOffset == 0))
                    continue;

                GetDestCell(aBreakRow, vColCrosses[vC].Col, ref vDestRow, ref vDestCol);
                vCellData = FRows[vDestRow][vDestCol].CellData;
                for (int i = vColCrosses[vC].DrawItemNo; i <= vCellData.DrawItems.Count - 1; i++)
                    HC.OffsetRect(ref vCellData.DrawItems[i].Rect, 0, vColCrosses[vC].VDrawOffset + vFixHeight);
            }
            
            // 当前行分页的单元格，有的可能是合并源，目标对应的源在此行下面，所以为了使
            // 各个单元格分页增加的偏移量能够传递到对应的结束单元格，从分页行重新格式化
            CalcMergeRowHeightFrom(aBreakRow);
        }

        // 保存和读取
        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte[] vBuffer = BitConverter.GetBytes(FBorderVisible);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FRows.Count);  // 行数
            aStream.Write(vBuffer, 0, vBuffer.Length);
            
            vBuffer = BitConverter.GetBytes(FColWidths.Count);
            aStream.Write(vBuffer, 0, vBuffer.Length);  // 列数

            aStream.WriteByte((byte)FFixCol);
            aStream.WriteByte(FFixRowCount);
            aStream.WriteByte((byte)FFixRow);
            aStream.WriteByte(FFixColCount);
            
            for (int i = 0; i <= FColWidths.Count - 1; i++)  // 各列标准宽
            {
                vBuffer = BitConverter.GetBytes(FColWidths[i]);
                aStream.Write(vBuffer, 0, vBuffer.Length);
            }

            for (int vR = 0; vR <= FRows.Count - 1; vR++)  // 各行数
            {
                vBuffer = BitConverter.GetBytes(FRows[vR].AutoHeight);
                aStream.Write(vBuffer, 0, vBuffer.Length);

                if (!FRows[vR].AutoHeight)
                {
                    vBuffer = BitConverter.GetBytes(FRows[vR].Height);
                    aStream.Write(vBuffer, 0, vBuffer.Length);
                }
                
                for (int vC = 0; vC <= FRows[vR].ColCount - 1; vC++)  // 各列数
                    FRows[vR][vC].SaveToStream(aStream);
            }
        }

        public override void SaveSelectToStream(Stream aStream)
        {
            if (this.IsSelectComplate)
                throw new Exception("保存选中内容出错，表格不应该由内部处理全选中的保存！");
            else
            {
                HCCustomData vCellData = GetActiveData();
                if (vCellData != null)
                    vCellData.SaveSelectToStream(aStream);
            }
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            FRows.Clear();
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            
            byte[] vBuffer = BitConverter.GetBytes(FBorderVisible);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FBorderVisible = BitConverter.ToBoolean(vBuffer, 0);
            
            int vRowCount = 0;
            vBuffer = BitConverter.GetBytes(vRowCount);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vRowCount = BitConverter.ToInt32(vBuffer, 0);  // 行数

            int vColCount = 0;
            vBuffer = BitConverter.GetBytes(vColCount);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vColCount = BitConverter.ToInt32(vBuffer, 0);  // 列数

            if (aFileVersion > 24)
            {
                FFixRow = (sbyte)aStream.ReadByte();
                FFixRowCount = (byte)aStream.ReadByte();
                FFixCol = (sbyte)aStream.ReadByte();
                FFixColCount = (byte)aStream.ReadByte();
            }

            // 创建行、列
            for (int i = 0; i <= vRowCount - 1; i++)
            {
                HCTableRow vRow = new HCTableRow(OwnerData.Style, vColCount);  // 注意行创建时是table拥有者的Style，加载时是传入的AStyle
                FRows.Add(vRow);
            }

            // 加载各列标准宽度
            FColWidths.Clear();

            int vWidth = HC.MinColWidth;
            vBuffer = BitConverter.GetBytes(vWidth);
            for (int i = 0; i <= vColCount - 1; i++)
            {
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vWidth = BitConverter.ToInt32(vBuffer, 0);

                FColWidths.Add(vWidth);
            }

            // 加载各列数据
            bool vAutoHeight = false;
            byte[] vBytes = BitConverter.GetBytes(vWidth);

            vBuffer = BitConverter.GetBytes(vAutoHeight);
            for (int vR = 0; vR <= FRows.Count - 1; vR++)
            {
                aStream.Read(vBuffer, 0, vBuffer.Length);
                vAutoHeight = BitConverter.ToBoolean(vBuffer, 0);
                FRows[vR].AutoHeight = vAutoHeight;
                if (!FRows[vR].AutoHeight)
                {
                    aStream.Read(vBytes, 0, vBytes.Length);
                    vWidth = BitConverter.ToInt32(vBytes, 0);
                    FRows[vR].Height = vWidth;
                }
                for (int vC = 0; vC <= FRows[vR].ColCount - 1; vC++)
                {
                    FRows[vR][vC].CellData.Width = FColWidths[vC] - 2 * FCellHPadding;
                    FRows[vR][vC].LoadFromStream(aStream, aStyle, aFileVersion);
                }
            }
        }

        public override string ToHtml(string aPath)
        {
            string Result = "<table border=\"" + FBorderWidth.ToString()
                + "\" cellpadding=\"0\"; cellspacing=\"0\"";
            for (int vR = 0; vR <= FRows.Count - 1; vR++)
            {
                Result = Result + HC.sLineBreak + "<tr>";
                for (int vC = 0; vC <= FColWidths.Count - 1; vC++)
                {
                    HCTableCell vCell = FRows[vR][vC];
                    if ((vCell.RowSpan < 0) || (vCell.ColSpan < 0))
                        continue;

                    Result = Result + HC.sLineBreak + string.Format("<td rowspan=\"{0}\"; colspan=\"{1}\"; width=\"{2}\"; height=\"{3}\">",
                        vCell.RowSpan + 1, vCell.ColSpan + 1, vCell.Width, vCell.Height);

                    if (vCell.CellData != null)
                        Result = Result + vCell.CellData.ToHtml(aPath);

                    Result = Result + HC.sLineBreak + "</td>";
                }
                Result = Result + HC.sLineBreak + "</tr>";
            }
            Result = Result + HC.sLineBreak + "</table>";

            return Result;
        }

        public override void ToXml(System.Xml.XmlElement aNode)
        {
            base.ToXml(aNode);

            string vS = FColWidths[0].ToString();
            for (int vC = 1; vC <= FColWidths.Count - 1; vC++)
                vS = vS + "," + FColWidths[vC].ToString();

            aNode.Attributes["bordervisible"].Value = FBorderVisible.ToString();
            aNode.Attributes["borderwidth"].Value = FBorderWidth.ToString();
            aNode.Attributes["row"].Value = FRows.Count.ToString();
            aNode.Attributes["col"].Value = FColWidths.Count.ToString();
            aNode.Attributes["colwidth"].Value = vS;
            aNode.Attributes["link"].Value = "";

            for (int vR = 0; vR <= FRows.Count - 1; vR++)
            {
                XmlElement vNode = aNode.OwnerDocument.CreateElement("row");
                FRows[vR].ToXml(vNode);
                aNode.AppendChild(vNode);
            }
        }

        public override void ParseXml(XmlElement aNode)
        {
            FRows.Clear();

            base.ParseXml(aNode);
            FBorderVisible = bool.Parse(aNode.Attributes["bordervisible"].Value);
            FBorderWidth = byte.Parse(aNode.Attributes["borderwidth"].Value);
            int vR = int.Parse(aNode.Attributes["row"].Value);
            int vC = int.Parse(aNode.Attributes["col"].Value);

            // 创建行、列
            for (int i = 0; i <= vR - 1; i++)
            {
                HCTableRow vRow = new HCTableRow(OwnerData.Style, vC);  // 注意行创建时是table拥有者的Style，加载时是传入的AStyle
                FRows.Add(vRow);
            }

            // 加载各列标准宽度
            FColWidths.Clear();
            string[] vStrings = aNode.Attributes["colwidth"].Value.Split(new string[] { "," }, StringSplitOptions.None);
            for (int i = 0; i <= vC - 1; i++)
                FColWidths.Add(int.Parse(vStrings[i]));

            // 加载各列数据
            for (int i = 0; i <= aNode.ChildNodes.Count - 1; i++)
                FRows[i].ParseXml(aNode.ChildNodes[i] as XmlElement);
        }

        public void ReSetRowCol(int aRowCount, int aColCount)
        {
            FFixRow = -1;
            FFixRowCount = 0;
            FFixCol = -1;
            FFixColCount = 0;

            this.InitializeMouseInfo();
            FSelectCellRang.Initialize();
            
            FColWidths.Clear();
            for (int i = 0; i <= aColCount - 1; i++)
                FColWidths.Add(HC.DefaultColWidth);

            int vWidth = GetFormatWidth();
            
            FRows.Clear();
            for (int i = 0; i <= aRowCount - 1; i++)
            {
                HCTableRow vRow = new HCTableRow(OwnerData.Style, aColCount);
                vRow.SetRowWidth(vWidth);
                FRows.Add(vRow);
            }

            FLastChangeFormated = false;
        }

        public int GetFormatWidth()
        {
            int Result = FBorderWidth;
            for (int i = 0; i <= FColWidths.Count - 1; i++)
                Result = Result + FColWidths[i] + FBorderWidth;

            return Result;
        }

        #region GetCellAt子方法
        private bool CheckRowBorderRang(int y, int aBottom)
        {
            return ((y >= aBottom - GripSize) && (y <= aBottom + GripSize));
        }

        private bool CheckColBorderRang(int X, int ALeft)
        {
            return ((X >= ALeft - GripSize) && (X <= ALeft + GripSize));
        }
        #endregion

        /// <summary> 获取指定位置处的行、列(如果是被合并单元格则返回目标单元格行、列) </summary>
        /// <param name="x">横坐标</param>
        /// <param name="y">纵坐标</param>
        /// <param name="aRow">坐标处的行</param>
        /// <param name="aCol">坐标处的列</param>
        ///  <param name="aReDest">如果坐标是合并源，返回目标</param>
        /// <returns></returns>
        public ResizeInfo GetCellAt(int x, int y, ref int aRow, ref int aCol, bool aReDest = true)
        {
            ResizeInfo Result = new ResizeInfo();
            Result.TableSite = TableSite.tsOutside;
            Result.DestX = -1;
            Result.DestY = -1;
            
            aRow = -1;
            aCol = -1;
            if ((y < 0) || (y > Height))
                return Result;

            int vTop = 0, vBottom =0;

            if ((x < 0) || (x > Width))
            {
                vTop = FBorderWidth;
                for (int i = 0; i <= RowCount - 1; i++)
                {
                    vTop = vTop + FRows[i].FmtOffset;  // 以实际内容Top为顶位置，避免行有跨页时，在上一页底部点击选中的是下一页第一行
                    vBottom = vTop + FRows[i].Height;

                    if ((vTop < y) && (vBottom > y))
                    {
                        aRow = i;
                        break;
                    }
                    
                    vTop = vBottom;
                }
                
                return Result;
            }
            
            // 获取是否在行或列的边框上
            // 判断是否在最上边框
            vTop = FBorderWidth;
            if (CheckRowBorderRang(y, vTop))
            {
                Result.TableSite = TableSite.tsBorderTop;
                return Result;
            }
            // 判断是否在最左边框
            if (CheckColBorderRang(x, vTop))
            {
                Result.TableSite = TableSite.tsBorderLeft;
                return Result;
            }
            
            // 判断是在行边框上还是行中
            for (int i = 0; i <= RowCount - 1; i++)
            {
                vTop = vTop + FRows[i].FmtOffset;  // 以实际内容Top为顶位置，避免行有跨页时，在上一页底部点击选中的是下一页第一行
                vBottom = vTop + FRows[i].Height + FBorderWidth;
                if (CheckRowBorderRang(y, vBottom))
                {
                    aRow = i;
                    Result.TableSite = TableSite.tsBorderBottom;
                    Result.DestY = vBottom;
                    break;  // 为处理跨单元格划选时，划到下边框时ACol<0造成中间选中的也被忽略掉的问题，不能像下面列找不到时Exit
                }

                if ((vTop < y) && (vBottom > y))
                {
                    aRow = i;
                    break;
                }
                
                vTop = vBottom;
            }

            if (aRow < 0)
                return Result;

            // 判断是在列边框上还是列中
            int vLeft = FBorderWidth, vRight = 0, vDestRow = -1, vDestCol = -1;

            for (int i = 0; i <= FColWidths.Count - 1; i++)
            {
                vRight = vLeft + FColWidths[i] + FBorderWidth;
                GetDestCell(aRow, i, ref vDestRow, ref vDestCol);
                if (CheckColBorderRang(x, vRight))
                {
                    aCol = i;
                    if (vDestCol + this[vDestRow, vDestCol].ColSpan != i)
                        Result.TableSite = TableSite.tsCell;
                    else
                        Result.TableSite = TableSite.tsBorderRight;
                    
                    Result.DestX = vRight;
                    
                    break;
                }

                if ((vLeft < x) && (vRight > x))
                {
                    aCol = i;
                    if ((Result.TableSite == TableSite.tsBorderBottom)
                        && (vDestRow + this[vDestRow, vDestCol].RowSpan != aRow))
                        Result.TableSite = TableSite.tsCell;
                    
                    break;
                }
                
                vLeft = vRight;
            }
            
            if (aCol >= 0)
            {
                if (Result.TableSite == TableSite.tsOutside)
                    Result.TableSite = TableSite.tsCell;
                
                if (aReDest && (this[aRow, aCol].CellData == null))
                    GetDestCell(aRow, aCol, ref aRow, ref aCol);
            }

            return Result;
        }

        public void GetDestCell(int aRow, int aCol, ref int aDestRow, ref int aDestCol)
        {
            aDestRow = aRow;
            aDestCol = aCol;


            if (FRows[aRow][aCol].RowSpan < 0)
                aDestRow = aDestRow + FRows[aRow][aCol].RowSpan;

            if (FRows[aRow][aCol].ColSpan < 0)
                aDestCol = aDestCol + FRows[aRow][aCol].ColSpan;
        }

        public void GetSourceCell(int aRow, int aCol, ref int aSrcRow, ref int aSrcCol)
        {
             if (this[aRow, aCol].CellData != null)
            {
                aSrcRow = aRow + FRows[aRow][aCol].RowSpan;
                aSrcCol = aCol + FRows[aRow][aCol].ColSpan;
            }
            else  // 源单元格不能获取源单元格
                throw new Exception(HC.HCS_EXCEPTION_VOIDSOURCECELL);
        }

        public void SelectAll()
        {
            SelectComplate();
        }

        public void PaintRow(int ARow, int ALeft, int ATop, int ABottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            int vBorderOffs = FBorderWidth / 2;
            int vCellDrawLeft = ALeft + FBorderWidth;
            int vCellDataDrawTop = ATop + FBorderWidth + FCellVPadding;
            int vCellDrawBottom = -1, vBorderTop = -1, vBorderBottom = -1, vBorderLeft = -1, vBorderRight = -1;
            HCTableCellData vCellData = null;
            bool vDrawDefault = false;
            
            for (int vC = 0; vC <= FRows[ARow].ColCount - 1; vC++)
            {
                if ((FRows[ARow][vC].ColSpan < 0) || (FRows[ARow][vC].RowSpan < 0))
                {
                    vCellDrawLeft = vCellDrawLeft + FColWidths[vC] + FBorderWidth;
                    continue;  // 普通单元格或合并目标单元格才有数据，否则由目标单元格处理
                }

                vCellDrawBottom = Math.Min(ABottom,  // 数据内容屏显最下端
                vCellDataDrawTop
                + Math.Max(FRows[ARow].Height, FRows[ARow][vC].Height) - FCellVPadding  // 行高和有合并的单元格高中最大的
                );

                RECT vCellRect = new RECT(vCellDrawLeft, ATop + FBorderWidth, vCellDrawLeft + FRows[ARow][vC].Width, vCellDrawBottom);
                vCellData = FRows[ARow][vC].CellData;
            
                if ((this.IsSelectComplate || vCellData.CellSelectedAll) && (!APaintInfo.Print))
                {
                    ACanvas.Brush.Color = OwnerData.Style.SelColor;
                    ACanvas.FillRect(vCellRect);
                }
                else  // 默认的绘制
                {
                    vDrawDefault = true;
                    if (FOnCellPaintBK != null)
                        FOnCellPaintBK(this, FRows[ARow][vC], vCellRect, ACanvas, APaintInfo, ref  vDrawDefault);

                    if (vDrawDefault)
                    {
                        if (FRows[ARow][vC].BackgroundColor != HC.HCTransparentColor)
                            ACanvas.Brush.Color = FRows[ARow][vC].BackgroundColor;
                        else
                            ACanvas.Brush.Style = HCBrushStyle.bsClear;

                        ACanvas.FillRect(vCellRect);
                    }
                }

                if (vCellDrawBottom - vCellDataDrawTop > FCellVPadding)
                {
                    FRows[ARow][vC].PaintTo(vCellDrawLeft, vCellDataDrawTop - FCellVPadding,
                        vCellDrawBottom, ATop, ABottom, 0, FCellHPadding, FCellVPadding, ACanvas, APaintInfo);
                }
           
                // 绘制各单元格边框线
                if (FBorderVisible || (!APaintInfo.Print))
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
                        ACanvas.Pen.Color = HC.clActiveBorder;
                        ACanvas.Pen.Style = HCPenStyle.psDot;
                    }

                    vBorderTop = vCellDataDrawTop - FCellVPadding - FBorderWidth;
                    vBorderBottom = vBorderTop + FBorderWidth  // 计算边框最下端
                        + Math.Max(FRows[ARow].Height, FRows[ARow][vC].Height);  // 由于可能是合并目标单元格，所以用单元格高和行高最高的
                
                    vBorderLeft = vCellDrawLeft - FBorderWidth;
                    vBorderRight = vCellDrawLeft + FColWidths[vC] + GetColSpanWidth(ARow, vC);
                
                    IntPtr vExtPen = HC.CreateExtPen(ACanvas.Pen);  // 因为默认的画笔没有线帽的控制，新增支持线帽的画笔
                    IntPtr vOldPen = (IntPtr)GDI.SelectObject(ACanvas.Handle, vExtPen);
                    try
                    {
                        if ((vBorderTop >= 0) && (FRows[ARow][vC].BorderSides.Contains((byte)BorderSide.cbsTop)))
                        {
                            ACanvas.MoveTo(vBorderLeft + vBorderOffs, vBorderTop + vBorderOffs);   // 左上
                            ACanvas.LineTo(vBorderRight + vBorderOffs, vBorderTop + vBorderOffs);  // 右上
                        }

                        if (FRows[ARow][vC].BorderSides.Contains((byte)BorderSide.cbsRight))
                        {
                            ACanvas.MoveTo(vBorderRight + vBorderOffs, vBorderTop + vBorderOffs);  // 右上
                            ACanvas.LineTo(vBorderRight + vBorderOffs, vBorderBottom + vBorderOffs);  // 右下
                        }

                        if ((vBorderBottom <= ABottom) && (FRows[ARow][vC].BorderSides.Contains((byte)BorderSide.cbsBottom)))
                        {
                            ACanvas.MoveTo(vBorderLeft + vBorderOffs, vBorderBottom + vBorderOffs);  // 左下
                            ACanvas.LineTo(vBorderRight + vBorderOffs, vBorderBottom + vBorderOffs);  // 右下
                        }

                        if (FRows[ARow][vC].BorderSides.Contains((byte)BorderSide.cbsLeft))
                        {
                            ACanvas.MoveTo(vBorderLeft + vBorderOffs, vBorderTop + vBorderOffs);
                            ACanvas.LineTo(vBorderLeft + vBorderOffs, vBorderBottom + vBorderOffs);
                        }

                        if (FRows[ARow][vC].BorderSides.Contains((byte)BorderSide.cbsLTRB))
                        {
                            ACanvas.MoveTo(vBorderLeft + vBorderOffs, vBorderTop + vBorderOffs);
                            ACanvas.LineTo(vBorderRight + vBorderOffs, vBorderBottom + vBorderOffs);
                        }

                        if (FRows[ARow][vC].BorderSides.Contains((byte)BorderSide.cbsRTLB))
                        {
                            ACanvas.MoveTo(vBorderRight + vBorderOffs, vBorderTop + vBorderOffs);
                            ACanvas.LineTo(vBorderLeft + vBorderOffs, vBorderBottom + vBorderOffs);
                        }
                    }
                    finally
                    {
                        GDI.SelectObject(ACanvas.Handle, vOldPen);
                        GDI.DeleteObject(vExtPen);
                    }
                }
        
                vCellDrawLeft = vCellDrawLeft + FColWidths[vC] + FBorderWidth;  // 同行下一列的起始Left位置
            }
        }

        public void PaintFixRows(int ALeft, int ATop, int ABottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            RECT vRect = HC.Bounds(ALeft, ATop, Width, GetFixRowHeight());
            ACanvas.Brush.Color = HC.clBtnFace;
            ACanvas.FillRect(vRect);

            int vTop = ATop, vH = -1;
            for (int vR = FFixRow; vR <= FFixRow + FFixRowCount - 1; vR++)
            {
                vH = 0;  // 行中未发生合并的最高单元格
                for (int vC = 0; vC <= FRows[vR].ColCount - 1; vC++)  // 得到行中未发生合并Data内容最高的单元格高
                {
                    if ((FRows[vR][vC].CellData != null)  // 不是被合并的单元格)
                    && (FRows[vR][vC].RowSpan >= 0))  // 不是行合并的行单元格
                    vH = Math.Max(vH, FRows[vR][vC].CellData.Height);
                }

                vH = FCellVPadding + vH + FCellVPadding;  // 增加上下边距
                vH = Math.Max(vH, FRows[vR].Height) + FBorderWidth + FBorderWidth;

                vRect = HC.Bounds(ALeft, vTop, Width, vH);
                if (vRect.Top >= ABottom)
                    break;

                PaintRow(vR, vRect.Left, vRect.Top, vRect.Bottom, ACanvas, APaintInfo);
                
                vTop = vTop + FBorderWidth + FRows[vR].Height;
            }
        }

        public void PaintFixCols(int ATableDrawTop, int ALeft, int ATop, int ABottom, HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            int vCellTop = Math.Max(ATop, ATableDrawTop) + FBorderWidth;
            for (int vR = 0; vR <= FFixRow + FFixRowCount - 1; vR++)
                vCellTop = vCellTop + FRows[vR].FmtOffset + FRows[vR].Height + FBorderWidth;

            RECT vRect = HC.Bounds(ALeft, vCellTop, GetFixColWidth(), ABottom - vCellTop);
            ACanvas.Brush.Color = HC.clBtnFace;
            ACanvas.FillRect(vRect);

            int vCellBottom = 0, vCellLeft = 0, vBorderTop = 0, vBorderBottom = 0, vBorderLeft = 0, vBorderRight = 0;

            int vBorderOffs = FBorderWidth / 2;

            vCellTop = ATableDrawTop + FBorderWidth;
        
            for (int vR = 0; vR <= FRows.Count - 1; vR++)
            {
                vCellTop = vCellTop + FRows[vR].FmtOffset;
                vCellBottom = vCellTop + FRows[vR].Height;
                if ((vCellBottom < ATop) || (vR < FFixRow + FFixRowCount))
                {
                    vCellTop = vCellBottom + FBorderWidth;
                    continue;
                }

                vCellLeft = ALeft + FBorderWidth;
                for (int vC = FFixCol; vC <= FFixCol + FFixColCount - 1; vC++)
                {
                    vRect = new RECT(vCellLeft, vCellTop, vCellLeft + FColWidths[vC], vCellBottom);
                    if (vRect.Top > ABottom)
                        break;

                    if (vRect.Bottom > ABottom)
                        vRect.Bottom = ABottom;

                    FRows[vR][vC].PaintTo(vCellLeft, vCellTop, vCellBottom, ATop, ABottom, 0,
                    FCellHPadding, FCellVPadding, ACanvas, APaintInfo);

                    if (FBorderVisible || (!APaintInfo.Print))
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
                            ACanvas.Pen.Color = HC.clActiveBorder;
                            ACanvas.Pen.Style = HCPenStyle.psDot;
                        }

                        vBorderTop = vCellTop - FBorderWidth;
                        vBorderBottom = vBorderTop + FBorderWidth  // 计算边框最下端
                            + Math.Max(FRows[vR].Height, FRows[vR][vC].Height);  // 由于可能是合并目标单元格，所以用单元格高和行高最高的
                    
                        vBorderLeft = vCellLeft - FBorderWidth;
                        vBorderRight = vCellLeft + FColWidths[vC] + GetColSpanWidth(vR, vC);
                    
                        IntPtr vExtPen = HC.CreateExtPen(ACanvas.Pen);  // 因为默认的画笔没有线帽的控制，新增支持线帽的画笔
                        IntPtr vOldPen = (IntPtr)GDI.SelectObject(ACanvas.Handle, vExtPen);
                        try
                        {
                            if ((vBorderTop >= 0) && (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsTop)))
                            {
                                ACanvas.MoveTo(vBorderLeft + vBorderOffs, vBorderTop + vBorderOffs);   // 左上
                                ACanvas.LineTo(vBorderRight + vBorderOffs, vBorderTop + vBorderOffs);  // 右上
                            }

                            if (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsRight))
                            {
                                ACanvas.MoveTo(vBorderRight + vBorderOffs, vBorderTop + vBorderOffs);  // 右上
                                ACanvas.LineTo(vBorderRight + vBorderOffs, vBorderBottom + vBorderOffs);  // 右下
                                ACanvas.MoveTo(vBorderRight + vBorderOffs + 1, vBorderTop + vBorderOffs);  // 右上
                                ACanvas.LineTo(vBorderRight + vBorderOffs + 1, vBorderBottom + vBorderOffs);  // 右下
                            }

                            if ((vBorderBottom <= ABottom) && (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsBottom)))
                            {
                                ACanvas.MoveTo(vBorderLeft + vBorderOffs, vBorderBottom + vBorderOffs);  // 左下
                                ACanvas.LineTo(vBorderRight + vBorderOffs, vBorderBottom + vBorderOffs);  // 右下
                            }

                            if (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsLeft))
                            {
                                ACanvas.MoveTo(vBorderLeft + vBorderOffs, vBorderTop + vBorderOffs);
                                ACanvas.LineTo(vBorderLeft + vBorderOffs, vBorderBottom + vBorderOffs);
                            }

                            if (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsLTRB))
                            {
                                ACanvas.MoveTo(vBorderLeft + vBorderOffs, vBorderTop + vBorderOffs);
                                ACanvas.LineTo(vBorderRight + vBorderOffs, vBorderBottom + vBorderOffs);
                            }

                            if (FRows[vR][vC].BorderSides.Contains((byte)BorderSide.cbsRTLB))
                            {
                                ACanvas.MoveTo(vBorderRight + vBorderOffs, vBorderTop + vBorderOffs);
                                ACanvas.LineTo(vBorderLeft + vBorderOffs, vBorderBottom + vBorderOffs);
                            }
                        }
                        finally
                        {
                            GDI.SelectObject(ACanvas.Handle, vOldPen);
                            GDI.DeleteObject(vExtPen);
                        }
                    }
            
                    vCellLeft = vCellLeft + FColWidths[vC] + FBorderWidth;
                }

                vCellTop = vCellBottom + FBorderWidth;
            }
        }

        public void FormatRow(int ARow)
        {
            HCTableRow vRow = FRows[ARow];
            vRow.FmtOffset = 0;  // 恢复上次格式化可能的偏移
            // 格式化各单元格中的Data
            for (int vC = 0; vC <= vRow.ColCount - 1; vC++)
            {
                if (vRow[vC].CellData != null)
                {
                    int vWidth = FColWidths[vC] + GetColSpanWidth(ARow, vC);

                    vRow[vC].Width = vWidth;
                    vRow[vC].CellData.Width = vWidth - FCellHPadding - FCellHPadding;
                    vRow[vC].CellData.ReFormat();
                }
            }
        }

        public int GetColSpanWidth(int ARow, int ACol)
        {
            int Result = 0;
            for (int i = 1; i <= FRows[ARow][ACol].ColSpan; i++)
                Result = Result + FBorderWidth + FColWidths[ACol + i];

            return Result;
        }

        /// <summary> 判断指定范围内的单元格是否可以合并(为了给界面合并菜单控制可用状态放到public域中) </summary>
        /// <param name="aStartRow"></param>
        /// <param name="aStartCol"></param>
        /// <param name="aEndRow"></param>
        /// <param name="aEndCol"></param>
        /// <returns></returns>
        public bool CellsCanMerge(int aStartRow, int aStartCol, int aEndRow, int aEndCol)
        {
            bool Result = false;
            for (int vR = aStartRow; vR <= aEndRow; vR++)
            {
                for (int vC = aStartCol; vC <= aEndCol; vC++)
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
        public bool RowCanDelete(int aRow)
        {
            bool Result = false;
            for (int vCol = 0; vCol <= FColWidths.Count - 1; vCol++)
            {
                if (FRows[aRow][vCol].RowSpan > 0)
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
        public bool ColCanDelete(int aCol)
        {
            bool Result = false;
            for (int vRow = 0; vRow <= RowCount - 1; vRow++)
            {
                if (FRows[vRow][aCol].ColSpan > 0)
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
                    FLastChangeFormated = false;
                    // 防止合并后有空行或空列被删除后，DisSelect访问越界，所以合并后直接赋值结束信息
                    int vSelRow = FSelectCellRang.StartRow;
                    int vSelCol = FSelectCellRang.StartCol;
                    FSelectCellRang.InitializeEnd();
                    DisSelect();
                    FSelectCellRang.SetStart(vSelRow, vSelCol);
                    FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol].CellData.InitializeField();
                }
            }
            else
                if (FSelectCellRang.EditCell())
                    Result = FRows[FSelectCellRang.StartRow][FSelectCellRang.StartCol].CellData.MergeTableSelectCells();
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

        public void GetEditCell(ref int aRow, int aCol)
        {
            aRow = -1;
            aCol = -1;
            if (FSelectCellRang.EditCell())
            {
                aRow = FSelectCellRang.StartRow;
                aCol = FSelectCellRang.StartCol;
            }
        }

        public bool InsertRowAfter(int aCount)
        {
            HCTableCell vCell = GetEditCell();
            if (vCell == null)
                return false;

            vCell.CellData.InitializeField();

            if (vCell.RowSpan > 0)
                return InsertRow(FSelectCellRang.StartRow + vCell.RowSpan + 1, aCount);
            else
                return InsertRow(FSelectCellRang.StartRow + 1, aCount);
        }

        public bool InsertRowBefor(int aCount)
        {
            HCTableCell vCell = GetEditCell();
            if (vCell == null)
                return false;
            
            vCell.CellData.InitializeField();

            return InsertRow(FSelectCellRang.StartRow, aCount);
        }

        public bool InsertColAfter(int aCount)
        {
            HCTableCell vCell = GetEditCell();
            if (vCell == null)
                return false;

            vCell.CellData.InitializeField();

            if (vCell.ColSpan > 0)
                return InsertCol(FSelectCellRang.StartCol + vCell.ColSpan + 1, aCount);
            else
                return InsertCol(FSelectCellRang.StartCol + 1, aCount);
        }

        public bool InsertColBefor(int aCount)
        {
            HCTableCell vCell = GetEditCell();
            if (vCell == null)
                return false;

            vCell.CellData.InitializeField();

            return InsertCol(FSelectCellRang.StartCol, aCount);
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
            if (FRows[vCurRow][vCurCol].RowSpan > 0)
            {
                GetSourceCell(vCurRow, vCurCol, ref vSrcRow, ref vSrcCol);  // 得到范围

                FRows[vCurRow][vCurCol].RowSpan = 0;  // 目标不再向下合并单元格了
                for (int i = vCurRow; i <= vSrcRow; i++)  // 从目标行下一行开始，重新设置合并目
                {
                    for (int vC = vCurCol; vC <= vSrcCol; vC++)  // 遍历拆分前光标所在的行各
                        FRows[i][vC].RowSpan = FRows[i][vC].RowSpan + 1;
                }
                
                // 原合并目标单元格正下面的单元格作为拆分后，下面合并源的新目标
                FRows[vCurRow + 1][vCurCol].CellData = new HCTableCellData(OwnerData.Style);
                FRows[vCurRow + 1][vCurCol].RowSpan = vSrcRow - (vCurRow + 1);
                FRows[vCurRow + 1][vCurCol].ColSpan = vSrcCol - vCurCol;
            }
            else  // Cells[vCurRow, vCurCol].RowSpan = 0 拆分时光标所在单元格是普通单元格
            if (InsertRow(vCurRow + 1, 1))  // 下面插入行
            {
                int vC = 0;
                while (vC < this.ColCount)
                {
                    vTopCell = FRows[vCurRow][vC];
                    
                    if (vC == vCurCol)  // 拆分时光标所在列
                    {
                        if (vTopCell.ColSpan > 0)  // 上面是列合并目标
                        {
                            vSrcCol = vCurCol + vTopCell.ColSpan;
                            while (vC <= vSrcCol)
                            {
                                FRows[vCurRow + 1][vC].ColSpan = FRows[vCurRow][vC].ColSpan;
                                if (FRows[vCurRow + 1][vC].ColSpan < 0)
                                {
                                    FRows[vCurRow + 1][vC].CellData.Dispose();
                                    FRows[vCurRow + 1][vC].CellData = null;
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
                                FRows[vCurRow + 1][vC].CellData.Dispose();
                                FRows[vCurRow + 1][vC].CellData = null;
                                FRows[vCurRow + 1][vC].RowSpan = -1;
                                vTopCell.RowSpan = 1;
                                vC++;
                            }
                            else
                            if (vTopCell.RowSpan < 0)
                            {
                                vDestRow = vCurRow + vTopCell.RowSpan;  // 目标行
                                vSrcRow = vDestRow + FRows[vDestRow][vC].RowSpan;
                                if (vCurRow == vSrcRow)
                                {
                                    FRows[vCurRow + 1][vC].CellData.Dispose();
                                    FRows[vCurRow + 1][vC].CellData = null;
                                    FRows[vCurRow + 1][vC].RowSpan = vTopCell.RowSpan - 1;
                                    FRows[vDestRow][vC].RowSpan = this[vDestRow, vC].RowSpan + 1;
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
                                    FRows[vCurRow + 1][vC].CellData.Dispose();
                                    FRows[vCurRow + 1][vC].CellData = null;
                                    FRows[vCurRow + 1][vC].ColSpan = vDestCol - vC;
                                    FRows[vCurRow + 1][vC].RowSpan = -1;
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
                        FRows[vR][i].ColSpan = FRows[vR][i].ColSpan + 1;
                }

                // 原合并目标单元格右侧的单元格作为拆分后，右侧合并源的新目标
                FRows[vCurRow][vCurCol + 1].CellData = new HCTableCellData(OwnerData.Style);
                FRows[vCurRow][vCurCol + 1].RowSpan = vSrcRow - vCurRow;
                FRows[vCurRow][vCurCol + 1].ColSpan = vSrcCol - (vCurCol + 1);
            }
            else  // Cells[vCurRow, vCurCol].ColSpan = 0 拆分时光标所在单元格是普通单元格
            if (InsertCol(vCurCol + 1, 1))
            {
                int vR = 0;
                while (vR < this.RowCount)
                {
                    vLeftCell = FRows[vR][vCurCol];
                    
                    if (vR == vCurRow)
                    {
                        if (vLeftCell.RowSpan > 0)
                        {
                            vSrcRow = vCurRow + vLeftCell.RowSpan;
                            while (vR <= vSrcRow)
                            {
                                FRows[vR][vCurCol + 1].RowSpan = FRows[vR][vCurCol].RowSpan;
                                if (FRows[vR][vCurCol + 1].RowSpan < 0)
                                {
                                    FRows[vR][vCurCol + 1].CellData.Dispose();
                                    FRows[vR][vCurCol + 1].CellData = null;
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
                                FRows[vR][vCurCol + 1].CellData.Dispose();
                                FRows[vR][vCurCol + 1].CellData = null;
                                FRows[vR][vCurCol + 1].ColSpan = -1;
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
                                    FRows[vR][vCurCol + 1].CellData.Dispose();
                                    FRows[vR][vCurCol + 1].CellData = null;
                                    FRows[vR][vCurCol + 1].ColSpan = vLeftCell.ColSpan - 1;
                                    FRows[vR][vDestCol].ColSpan = this[vR, vDestCol].ColSpan + 1;
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
                                    FRows[vR][vCurCol + 1].CellData.Dispose();
                                    FRows[vR][vCurCol + 1].CellData = null;
                                    FRows[vR][vCurCol + 1].RowSpan = vDestRow - vR;
                                    FRows[vR][vCurCol + 1].ColSpan = -1;
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

        public bool IsBreakRow(int ARow)
        {
            bool Result = false;
            for (int i = 0; i <= FPageBreaks.Count - 1; i++)
            {
                if (ARow == FPageBreaks[i].Row)
                {
                    Result = true;
                    break;
                }
            }

            return Result;
        }

        public bool IsFixRow(int ARow)
        {
            if (FFixRow >= 0)
                return (ARow >= FFixRow) && (ARow <= FFixRow + FFixRowCount - 1);
            else
                return false;
        }

        public bool IsFixCol(int ACol)
        {
            if (FFixCol >= 0)
                return (ACol >= FFixCol) && (ACol <= FFixCol + FFixColCount - 1);
            else
                return false;
        }

        public int GetFixRowHeight()
        {
            if (FFixRow < 0)
                return 0;
            else
            {
                int Result = FBorderWidth;
                for (int vR = FFixRow; vR <= FFixRow + FFixRowCount - 1; vR++)
                    Result = Result + FRows[vR].Height + FBorderWidth;

                return Result;
            }
        }

        public int GetFixColWidth()
        {
            if (FFixCol < 0)
                return 0;
            else
            {
                int Result = FBorderWidth;
                for (int vC = FFixCol; vC <= FFixCol + FFixColCount - 1; vC++)
                    Result = Result + FColWidths[vC] + FBorderWidth;

                return Result;
            }
        }

        public int GetFixColLeft()
        {
            if (FFixCol < 0)
                return 0;
            else
            {
                int Result = FBorderWidth;
                for (int vC = 0; vC <= FFixCol - 1; vC++)
                    Result = Result + FColWidths[vC] + FBorderWidth;

                return Result;
            }
        }

        public HCTableCell this[int aRow, int aCol]
        {
            get { return GetCells(aRow, aCol); }
        }

        public int ColWidth(int aIndex)
        {
            return GetColWidth(aIndex);
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

        public sbyte FixCol
        {
            get { return FFixCol; }
            set { FFixCol = value; }
        }

        public byte FixColCount
        {
            get { return FFixColCount; }
            set { FFixColCount = value; }
        }

        public sbyte FixRow
        {
            get { return FFixRow; }
            set { FFixRow = value; }
        }

        public byte FixRowCount
        {
            get { return FFixRowCount; }
            set { FFixRowCount = value; }
        }

        public HCCellPaintEventHandle OnCellPaintBK
        {
            get { return FOnCellPaintBK; }
            set { OnCellPaintBK = value; }
        }

        public HCCellPaintDataEventHandle OnCellPaintData
        {
            get { return OnCellPaintData; }
            set { OnCellPaintData = value; }
        }
    }
}
