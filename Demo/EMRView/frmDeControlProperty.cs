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
    public partial class frmDeControlProperty : Form
    {
        public frmDeControlProperty()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }

        public void SetHCView(HC.View.HCView aHCView)
        {
            HCControlItem vControlItem = aHCView.ActiveSectionTopLevelData().GetActiveItem() as HCControlItem;

            cbxAutoSize.Checked = vControlItem.AutoSize;
            tbxWidth.Text = vControlItem.Width.ToString();
            tbxHeight.Text = vControlItem.Height.ToString();
            tbxText.Text = vControlItem.Text;

            pnlBorder.Visible = false;

            DeCheckBox vDeCheckBox = null;
            if (vControlItem is DeCheckBox)
            {
                this.Text = "DeCheckBox属性";
                vDeCheckBox = vControlItem as DeCheckBox;                
            }

            DeEdit vDeEdit = null;
            if (vControlItem is DeEdit)
            {
                this.Text = "DeEdit属性";
                vDeEdit = vControlItem as DeEdit;
                cbxBorderLeft.Checked = vDeEdit.BorderSides.Contains((byte)BorderSide.cbsLeft);
                cbxBorderTop.Checked = vDeEdit.BorderSides.Contains((byte)BorderSide.cbsTop);
                cbxBorderRight.Checked = vDeEdit.BorderSides.Contains((byte)BorderSide.cbsRight);
                cbxBorderBottom.Checked = vDeEdit.BorderSides.Contains((byte)BorderSide.cbsBottom);
                pnlBorder.Visible = true;

                dgvEdit.RowCount = vDeEdit.Propertys.Count + 1;
                if (vDeEdit.Propertys.Count > 0)
                {
                    int vRow = 0;
                    foreach (KeyValuePair<string, string> keyValuePair in vDeEdit.Propertys)
                    {
                        dgvEdit.Rows[vRow].Cells[0].Value = keyValuePair.Key;
                        dgvEdit.Rows[vRow].Cells[1].Value = keyValuePair.Value;
                        vRow++;
                    }
                }                
            }
            else
                pnlEdit.Visible = false;

            DeDateTimePicker vDeDateTimePicker = null;
            if (vControlItem is DeDateTimePicker)
            {
                this.Text = "DeDateTime属性";
                vDeDateTimePicker = vControlItem as DeDateTimePicker;
                cbxBorderLeft.Checked = vDeDateTimePicker.BorderSides.Contains((byte)BorderSide.cbsLeft);
                cbxBorderTop.Checked = vDeDateTimePicker.BorderSides.Contains((byte)BorderSide.cbsTop);
                cbxBorderRight.Checked = vDeDateTimePicker.BorderSides.Contains((byte)BorderSide.cbsRight);
                cbxBorderBottom.Checked = vDeDateTimePicker.BorderSides.Contains((byte)BorderSide.cbsBottom);
                pnlBorder.Visible = true;

                cbbDTFormat.Text = vDeDateTimePicker.Format;                
            }
            else
                pnlDateTime.Visible = false;

            int vHeight = 0;
            if (pnlSize.Visible)
                vHeight += pnlSize.Height;
            else
            if (pnlEdit.Visible)
                vHeight += pnlEdit.Height;
            else
            if (pnlDateTime.Visible)
                vHeight += pnlDateTime.Height;

            this.Height = vHeight;

            this.ShowDialog();
            if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                vControlItem.AutoSize = cbxAutoSize.Checked;
                if (!cbxAutoSize.Checked)  // 自定义大小
                {
                    int vValue = 0;
                    if (int.TryParse(tbxWidth.Text, out vValue))
                        vControlItem.Width = vValue;

                    if (int.TryParse(tbxHeight.Text, out vValue))
                        vControlItem.Height = vValue;
                }

                if (tbxText.Text != "")
                    vControlItem.Text = tbxText.Text;

                if (vDeEdit != null)
                {
                    if (cbxBorderLeft.Checked)
                        vDeEdit.BorderSides.InClude((byte)BorderSide.cbsLeft);
                    else
                        vDeEdit.BorderSides.ExClude((byte)BorderSide.cbsLeft);

                    if (cbxBorderTop.Checked)
                        vDeEdit.BorderSides.InClude((byte)BorderSide.cbsTop);
                    else
                        vDeEdit.BorderSides.ExClude((byte)BorderSide.cbsTop);

                    if (cbxBorderRight.Checked)
                        vDeEdit.BorderSides.InClude((byte)BorderSide.cbsRight);
                    else
                        vDeEdit.BorderSides.ExClude((byte)BorderSide.cbsRight);

                    if (cbxBorderBottom.Checked)
                        vDeEdit.BorderSides.InClude((byte)BorderSide.cbsBottom);
                    else
                        vDeEdit.BorderSides.ExClude((byte)BorderSide.cbsBottom);

                    string vsValue = "";
                    vDeEdit.Propertys.Clear();
                    for (int i = 0; i < dgvEdit.RowCount; i++)
                    {
                        if (dgvEdit.Rows[i].Cells[0].Value == null)
                            continue;

                        if (dgvEdit.Rows[i].Cells[1].Value == null)
                            vsValue = "";
                        else
                            vsValue = dgvEdit.Rows[i].Cells[1].Value.ToString();

                        if (dgvEdit.Rows[i].Cells[0].Value.ToString().Trim() != "")
                            vDeEdit.Propertys.Add(dgvEdit.Rows[i].Cells[0].Value.ToString(), vsValue);
                    }
                }

                if (vDeDateTimePicker != null)
                {
                    if (cbxBorderLeft.Checked)
                        vDeDateTimePicker.BorderSides.InClude((byte)BorderSide.cbsLeft);
                    else
                        vDeDateTimePicker.BorderSides.ExClude((byte)BorderSide.cbsLeft);

                    if (cbxBorderTop.Checked)
                        vDeDateTimePicker.BorderSides.InClude((byte)BorderSide.cbsTop);
                    else
                        vDeDateTimePicker.BorderSides.ExClude((byte)BorderSide.cbsTop);

                    if (cbxBorderRight.Checked)
                        vDeDateTimePicker.BorderSides.InClude((byte)BorderSide.cbsRight);
                    else
                        vDeDateTimePicker.BorderSides.ExClude((byte)BorderSide.cbsRight);

                    if (cbxBorderBottom.Checked)
                        vDeDateTimePicker.BorderSides.InClude((byte)BorderSide.cbsBottom);
                    else
                        vDeDateTimePicker.BorderSides.ExClude((byte)BorderSide.cbsBottom);

                    vDeDateTimePicker.Format = cbbDTFormat.Text;
                }

                aHCView.BeginUpdate();
                try
                {
                    aHCView.ActiveSection.ReFormatActiveItem();
                }
                finally
                {
                    aHCView.EndUpdate();
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
