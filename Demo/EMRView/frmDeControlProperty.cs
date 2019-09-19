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

            pnlBorder.Visible = false;

            DeCheckBox vDeCheckBox = null;
            if (vControlItem is DeCheckBox)
            {
                vDeCheckBox = vControlItem as DeCheckBox;
                pnlEdit.Visible = false;
            }

            DeEdit vDeEdit = null;
            if (vControlItem is DeEdit)
            {
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

            DeCombobox vDeCombobox = null;
            if (vControlItem is DeCombobox)
            {
                vDeCombobox = vControlItem as DeCombobox;

                cbxBorderLeft.Checked = vDeCombobox.BorderSides.Contains((byte)BorderSide.cbsLeft);
                cbxBorderTop.Checked = vDeCombobox.BorderSides.Contains((byte)BorderSide.cbsTop);
                cbxBorderRight.Checked = vDeCombobox.BorderSides.Contains((byte)BorderSide.cbsRight);
                cbxBorderBottom.Checked = vDeCombobox.BorderSides.Contains((byte)BorderSide.cbsBottom);
                pnlBorder.Visible = true;

                foreach (string vItem in vDeCombobox.Items)
                    lstCombobox.Items.Add(vItem);

                dgvCombobox.RowCount = vDeCombobox.Propertys.Count + 1;
                if (vDeCombobox.Propertys.Count > 0)
                {
                    int vRow = 0;
                    foreach (KeyValuePair<string, string> keyValuePair in vDeCombobox.Propertys)
                    {
                        dgvCombobox.Rows[vRow].Cells[0].Value = keyValuePair.Key;
                        dgvCombobox.Rows[vRow].Cells[1].Value = keyValuePair.Value;
                        vRow++;
                    }
                }
            }
            else
                pnlCombobox.Visible = false;

            DeDateTimePicker vDeDateTimePicker = null;
            if (vControlItem is DeDateTimePicker)
            {
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

            DeRadioGroup vDeRadioGroup = null;
            if (vControlItem is DeRadioGroup)
            {
                vDeRadioGroup = vControlItem as DeRadioGroup;
                foreach (HCRadioButton vItem in vDeRadioGroup.Items)
                    lstRadioItem.Items.Add(vItem.Text);
            }
            else
                pnlRadioGroup.Visible = false;

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
                        {
                            vDeEdit.Propertys.Add(dgvEdit.Rows[i].Cells[0].Value.ToString(), vsValue);
                        }
                    }
                }

                if (vDeCombobox != null)
                {
                    if (cbxBorderLeft.Checked)
                        vDeCombobox.BorderSides.InClude((byte)BorderSide.cbsLeft);
                    else
                        vDeCombobox.BorderSides.ExClude((byte)BorderSide.cbsLeft);

                    if (cbxBorderTop.Checked)
                        vDeCombobox.BorderSides.InClude((byte)BorderSide.cbsTop);
                    else
                        vDeCombobox.BorderSides.ExClude((byte)BorderSide.cbsTop);

                    if (cbxBorderRight.Checked)
                        vDeCombobox.BorderSides.InClude((byte)BorderSide.cbsRight);
                    else
                        vDeCombobox.BorderSides.ExClude((byte)BorderSide.cbsRight);

                    if (cbxBorderBottom.Checked)
                        vDeCombobox.BorderSides.InClude((byte)BorderSide.cbsBottom);
                    else
                        vDeCombobox.BorderSides.ExClude((byte)BorderSide.cbsBottom);

                    vDeCombobox.Items.Clear();
                    foreach (string vobj in lstCombobox.Items)
                        vDeCombobox.Items.Add(vobj.ToString());

                    string vsValue = "";
                    vDeCombobox.Propertys.Clear();
                    for (int i = 0; i < dgvCombobox.RowCount; i++)
                    {
                        if (dgvCombobox.Rows[i].Cells[0].Value == null)
                            continue;

                        if (dgvCombobox.Rows[i].Cells[1].Value == null)
                            vsValue = "";
                        else
                            vsValue = dgvCombobox.Rows[i].Cells[1].Value.ToString();

                        if (dgvCombobox.Rows[i].Cells[0].Value.ToString().Trim() != "")
                        {
                            vDeCombobox.Propertys.Add(dgvCombobox.Rows[i].Cells[0].Value.ToString(), vsValue);
                        }
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

                if (vDeRadioGroup != null)
                {
                    vDeRadioGroup.Items.Clear();
                    foreach (object vObj in lstRadioItem.Items)
                        vDeRadioGroup.AddItem(vObj.ToString());
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (tbxValue.Text != "")
            {
                lstCombobox.Items.Add(tbxValue.Text);
                tbxValue.Clear();
            }
        }

        private void btnMod_Click(object sender, EventArgs e)
        {
            lstCombobox.SelectedItem = tbxValue.Text;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            lstCombobox.Items.Remove(lstCombobox.SelectedItem);
        }

        private void btnAddRadioItem_Click(object sender, EventArgs e)
        {
            if (tbxRadioValue.Text != "")
            {
                lstRadioItem.Items.Add(tbxRadioValue.Text);
                tbxRadioValue.Clear();
            }
        }

        private void btnModRadioItem_Click(object sender, EventArgs e)
        {
            lstRadioItem.SelectedItem = tbxRadioValue.Text;
        }

        private void btnDeleteRadioItem_Click(object sender, EventArgs e)
        {
            lstRadioItem.Items.Remove(lstRadioItem.SelectedItem);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
