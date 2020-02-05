/*******************************************************}
{                                                       }
{         基于HCView的电子病历程序  作者：荆通          }
{                                                       }
{ 此代码仅做学习交流使用，不可用于商业目的，由此引发的  }
{ 后果请使用者承担，加入QQ群 649023932 来获取更多的技术 }
{ 交流。                                                }
{                                                       }
{*******************************************************/
using HC.View;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EMRView
{
    public partial class frmDeTableProperty : Form
    {
        public frmDeTableProperty()
        {
            InitializeComponent();
            FReFormat = false;
            tabTableInfo.SelectedIndex = 0;
            this.ShowInTaskbar = false;
        }

        private bool FReFormat;
        private HCView FHCView;

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        public void SetHCView(HCView aHCView)
        {
            FHCView = aHCView;
            HCRichData vData = FHCView.ActiveSection.ActiveData;
            DeTable vTable = vData.GetActiveItem() as DeTable;

            // 表格
            tbxCellHPadding.Text = String.Format("{0:0.##}", vTable.CellHPaddingMM);
            tbxCellVPadding.Text = String.Format("{0:0.##}", vTable.CellVPaddingMM);
            cbxBorderVisible.Checked = vTable.BorderVisible;
            tbxBorderWidth.Text = String.Format("{0:0.##}", vTable.BorderWidthPt);

            tbxFixRowFirst.Text = (vTable.FixRow + 1).ToString();
            tbxFixRowLast.Text = (vTable.FixRow + 1 + vTable.FixRowCount).ToString();
            tbxFixColFirst.Text = (vTable.FixCol + 1).ToString();
            tbxFixColLast.Text = (vTable.FixCol + 1 + vTable.FixColCount).ToString();

            // 行
            if (vTable.SelectCellRang.StartRow >= 0)
            {
                tabRow.Text = "行(" + (vTable.SelectCellRang.StartRow + 1).ToString() + ")";
                if (vTable.SelectCellRang.EndRow > 0)
                    tabRow.Text += " - (" + (vTable.SelectCellRang.EndRow + 1).ToString() + ")";

                tbxRowHeight.Text = vTable.Rows[vTable.SelectCellRang.StartRow].Height.ToString();  // 行高
            }
            else
                tabTableInfo.TabPages.Remove(tabRow);

            // 单元格
            if ((vTable.SelectCellRang.StartRow >= 0) && (vTable.SelectCellRang.StartCol >= 0))
            {
                HCAlignVert vAlignVer = HCAlignVert.cavTop;

                if (vTable.SelectCellRang.EndRow >= 0)  // 多选
                {
                    vAlignVer = vTable[vTable.SelectCellRang.StartRow, vTable.SelectCellRang.StartCol].AlignVert;
                    tabCell.Text = "单元格(" + (vTable.SelectCellRang.StartRow + 1).ToString() + ","
                        + (vTable.SelectCellRang.StartCol + 1).ToString() + ") - ("
                        + (vTable.SelectCellRang.EndRow + 1).ToString() + ","
                        + (vTable.SelectCellRang.EndCol + 1).ToString() + ")";
                }
                else
                {
                    vAlignVer = vTable.GetEditCell().AlignVert;
                    tabCell.Text = "单元格(" + (vTable.SelectCellRang.StartRow + 1).ToString() + ","
                        + (vTable.SelectCellRang.StartCol + 1).ToString() + ")";
                }

                cbbCellAlignVert.SelectedIndex = (int)vAlignVer;
            }
            else
                tabTableInfo.TabPages.Remove(tabCell);

            dgvTable.RowCount = vTable.Propertys.Count + 1;
            if (vTable.Propertys.Count > 0)
            {
                int vRow = 0;
                foreach (KeyValuePair<string, string> keyValuePair in vTable.Propertys)
                {
                    dgvTable.Rows[vRow].Cells[0].Value = keyValuePair.Key;
                    dgvTable.Rows[vRow].Cells[1].Value = keyValuePair.Value;
                    vRow++;
                }
            }

            this.ShowDialog();
            if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                FHCView.BeginUpdate();
                try
                {
                    // 表格
                    vTable.CellHPaddingMM = Single.Parse(tbxCellHPadding.Text);
                    vTable.CellVPaddingMM = Single.Parse(tbxCellVPadding.Text);
                    vTable.BorderWidthPt = float.Parse(tbxBorderWidth.Text);
                    vTable.BorderVisible = cbxBorderVisible.Checked;

                    vTable.FixRow = (sbyte)(int.Parse(tbxFixRowFirst.Text, 0) - 1);
                    vTable.FixRowCount = (byte)(int.Parse(tbxFixRowLast.Text, 0) - vTable.FixRow);
                    vTable.FixCol = (sbyte)(int.Parse(tbxFixColFirst.Text, 0) - 1);
                    vTable.FixColCount = (byte)(int.Parse(tbxFixColLast.Text, 0) - vTable.FixCol);

                    // 行
                    if (vTable.SelectCellRang.StartRow >= 0)
                    {
                        int viValue = int.Parse(tbxRowHeight.Text);
                        if (vTable.SelectCellRang.EndRow > 0)
                        {
                            for (int vR = vTable.SelectCellRang.StartRow; vR <= vTable.SelectCellRang.EndRow; vR++)
                                vTable.Rows[vR].Height = viValue;
                        }
                        else
                            vTable.Rows[vTable.SelectCellRang.StartRow].Height = viValue;
                    }

                    // 单元格
                    if ((vTable.SelectCellRang.StartRow >= 0) && (vTable.SelectCellRang.StartCol >= 0))
                    {
                        if (vTable.SelectCellRang.EndCol > 0)
                        {
                            for (int vR = vTable.SelectCellRang.StartRow; vR <= vTable.SelectCellRang.EndRow; vR++)
                            {
                                for (int vC = vTable.SelectCellRang.StartCol; vC <= vTable.SelectCellRang.EndCol; vC++)
                                    vTable[vR, vC].AlignVert = (HCAlignVert)cbbCellAlignVert.SelectedIndex;
                            }
                        }
                        else
                            vTable.GetEditCell().AlignVert = (HCAlignVert)cbbCellAlignVert.SelectedIndex;
                    }

                    vTable.Propertys.Clear();
                    string vsValue = "";
                    for (int i = 0; i < dgvTable.RowCount; i++)
                    {
                        if (dgvTable.Rows[i].Cells[0].Value == null)
                            continue;

                        if (dgvTable.Rows[i].Cells[1].Value == null)
                            vsValue = "";
                        else
                            vsValue = dgvTable.Rows[i].Cells[1].Value.ToString();

                        if (dgvTable.Rows[i].Cells[0].Value.ToString().Trim() != "")
                        {
                            vTable.Propertys.Add(dgvTable.Rows[i].Cells[0].Value.ToString(), vsValue);
                        }
                    }

                    if (FReFormat)
                        FHCView.ActiveSection.ReFormatActiveItem();
                }
                finally
                {
                    FHCView.EndUpdate();
                }
            }
        }

        private void tbxCellHPadding_TextChanged(object sender, EventArgs e)
        {
            FReFormat = true;
        }

        private void btnBorderBackColor_Click(object sender, EventArgs e)
        {
            frmTableBorderBackColor vFrmBorderBackColor = new frmTableBorderBackColor();
            vFrmBorderBackColor.SetView(FHCView);
        }

        private void frmDeTableProperty_Load(object sender, EventArgs e)
        {
            //IntPtr HIme = Imm.ImmGetContext(this.tabTableInfo.Handle);
            //Imm.ImmSetOpenStatus(HIme, true);
            tabTableInfo.ImeMode = ImeMode.OnHalf;
        }
    }
}
