/*******************************************************}
{                                                       }
{         基于HCView的电子病历程序  作者：荆通          }
{                                                       }
{ 此代码仅做学习交流使用，不可用于商业目的，由此引发的  }
{ 后果请使用者承担，加入QQ群 649023932 来获取更多的技术 }
{ 交流。                                                }
{                                                       }
{*******************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HC.View;

namespace EMRView
{
    public partial class frmTableBorderBackColor : Form
    {
        public frmTableBorderBackColor()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }

        private HCTableItem FTableItem;

        private void GetTableProperty()
        {
            HCTableCell vCell = FTableItem[0, 0];
            if (FTableItem.SelectCellRang.StartRow >= 0)
                vCell = FTableItem[FTableItem.SelectCellRang.StartRow, FTableItem.SelectCellRang.StartCol];

            pnlBackColor.BackColor = vCell.BackgroundColor;

            cbxLeft.Checked = vCell.BorderSides.Contains((byte)BorderSide.cbsLeft);
            cbxTop.Checked = vCell.BorderSides.Contains((byte)BorderSide.cbsTop);
            cbxRight.Checked = vCell.BorderSides.Contains((byte)BorderSide.cbsRight);
            cbxBottom.Checked = vCell.BorderSides.Contains((byte)BorderSide.cbsBottom);

            cbxLTRB.Checked = vCell.BorderSides.Contains((byte)BorderSide.cbsLTRB);
            cbxRTLB.Checked = vCell.BorderSides.Contains((byte)BorderSide.cbsRTLB);
        }

        #region 子方法
        private void SetCellBorderBackColor(int aRow, int aCol)
        {
            HCTableCell vCell = FTableItem[aRow, aCol];

            if (pnlBackColor.BackColor == HC.View.HC.HCTransparentColor)
                vCell.BackgroundColor = HC.View.HC.HCTransparentColor;
            else
                vCell.BackgroundColor = pnlBackColor.BackColor;

            if (cbxLeft.Checked)
                vCell.BorderSides.InClude((byte)BorderSide.cbsLeft);
            else
                vCell.BorderSides.ExClude((byte)BorderSide.cbsLeft);

            if (cbxTop.Checked)
                vCell.BorderSides.InClude((byte)BorderSide.cbsTop);
            else
                vCell.BorderSides.ExClude((byte)BorderSide.cbsTop);

            if (cbxRight.Checked)
                vCell.BorderSides.InClude((byte)BorderSide.cbsRight);
            else
                vCell.BorderSides.ExClude((byte)BorderSide.cbsRight);

            if (cbxBottom.Checked)
                vCell.BorderSides.InClude((byte)BorderSide.cbsBottom);
            else
                vCell.BorderSides.ExClude((byte)BorderSide.cbsBottom);

            if (cbxLTRB.Checked)
                vCell.BorderSides.InClude((byte)BorderSide.cbsLTRB);
            else
                vCell.BorderSides.ExClude((byte)BorderSide.cbsLTRB);

            if (cbxRTLB.Checked)
                vCell.BorderSides.InClude((byte)BorderSide.cbsRTLB);
            else
                vCell.BorderSides.ExClude((byte)BorderSide.cbsRTLB);
        }

        private void ApplyAllTable()
        {
            for (int vR = 0; vR < FTableItem.RowCount; vR++)
                for (int vC = 0; vC < FTableItem.ColCount; vC++)
                    SetCellBorderBackColor(vR, vC);
        }
        #endregion

        private void SetTableProperty()
        {
            if (cbbRang.SelectedIndex == 0)  // 单元格
            {
                if (FTableItem.SelectCellRang.EditCell())  // 在同一个单元格编辑
                    SetCellBorderBackColor(FTableItem.SelectCellRang.StartRow, FTableItem.SelectCellRang.StartCol);
                else  // 多选或者一个也没选  
                {
                    if (FTableItem.SelectCellRang.StartRow >= 0)  // 多选
                    {
                        for (int vR = FTableItem.SelectCellRang.StartRow; vR <= FTableItem.SelectCellRang.EndRow; vR++)
                            for (int vC = FTableItem.SelectCellRang.StartCol; vC <= FTableItem.SelectCellRang.EndCol; vC++)
                                SetCellBorderBackColor(vR, vC);
                    }
                    else  // 一个也没选，按整个表格处理
                        ApplyAllTable();
                }
            }
            else
                ApplyAllTable();
        }

        public void SetView(HCView aView)
        {
            FTableItem = aView.ActiveSection.ActiveData.GetActiveItem() as HCTableItem;
            GetTableProperty();

            this.ShowDialog();
            if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                aView.BeginUpdate();
                try
                {
                    SetTableProperty();
                    aView.Style.UpdateInfoRePaint();
                }
                finally
                {
                    aView.EndUpdate();
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnSelectColor_Click(object sender, EventArgs e)
        {
            ColorDialog vDlg = new ColorDialog();
            //vDlg.SolidColorOnly = true;
            vDlg.Color = pnlBackColor.BackColor;
            vDlg.ShowDialog();
            pnlBackColor.BackColor = vDlg.Color;
        }

        private void frmTableBorderBackColor_Load(object sender, EventArgs e)
        {
            cbbRang.SelectedIndex = 0;
        }
    }
}
