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

            DeCombobox vDeCombobox = null;
            if (vControlItem is DeCombobox)
            {
                this.Text = "DeCombobox属性";
                vDeCombobox = vControlItem as DeCombobox;

                cbxBorderLeft.Checked = vDeCombobox.BorderSides.Contains((byte)BorderSide.cbsLeft);
                cbxBorderTop.Checked = vDeCombobox.BorderSides.Contains((byte)BorderSide.cbsTop);
                cbxBorderRight.Checked = vDeCombobox.BorderSides.Contains((byte)BorderSide.cbsRight);
                cbxBorderBottom.Checked = vDeCombobox.BorderSides.Contains((byte)BorderSide.cbsBottom);
                pnlBorder.Visible = true;

                foreach (HCCbbItem vItem in vDeCombobox.Items)
                    lstCombobox.Items.Add(vItem.Text);

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

            DeRadioGroup vDeRadioGroup = null;
            if (vControlItem is DeRadioGroup)
            {
                this.Text = "DeRadioGroup属性";
                vDeRadioGroup = vControlItem as DeRadioGroup;
                if (vDeRadioGroup.RadioStyle == HCRadioStyle.CheckBox)
                    cbbRadioStyle.SelectedIndex = 1;
                else
                    cbbRadioStyle.SelectedIndex = 0;

                foreach (HCRadioButton vItem in vDeRadioGroup.Items)
                    lstRadioItem.Items.Add(vItem.Text);

                dgvRadio.RowCount = vDeRadioGroup.Propertys.Count + 1;
                if (vDeRadioGroup.Propertys.Count > 0)
                {
                    int vRow = 0;
                    foreach (KeyValuePair<string, string> keyValuePair in vDeRadioGroup.Propertys)
                    {
                        dgvRadio.Rows[vRow].Cells[0].Value = keyValuePair.Key;
                        dgvRadio.Rows[vRow].Cells[1].Value = keyValuePair.Value;
                        vRow++;
                    }
                }
            }
            else
                pnlRadioGroup.Visible = false;

            int vHeight = 0;
            if (pnlSize.Visible)
                vHeight += pnlSize.Height;
            else
            if (pnlEdit.Visible)
                vHeight += pnlEdit.Height;
            else
            if (pnlCombobox.Visible)
                vHeight += pnlCombobox.Height;
            else
            if (pnlDateTime.Visible)
                vHeight += pnlDateTime.Height;
            else
            if (pnlRadioGroup.Visible)
                vHeight += pnlRadioGroup.Height;

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
                        vDeCombobox.Items.Add(new HCCbbItem(vobj.ToString()));

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
                            vDeCombobox.Propertys.Add(dgvCombobox.Rows[i].Cells[0].Value.ToString(), vsValue);
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
                    if (cbbRadioStyle.SelectedIndex == 1)
                        vDeRadioGroup.RadioStyle = HCRadioStyle.CheckBox;
                    else
                        vDeRadioGroup.RadioStyle = HCRadioStyle.Radio;

                    vDeRadioGroup.Items.Clear();
                    foreach (object vObj in lstRadioItem.Items)
                        vDeRadioGroup.AddItem(vObj.ToString());

                    string vsValue = "";
                    vDeRadioGroup.Propertys.Clear();
                    for (int i = 0; i < dgvRadio.RowCount; i++)
                    {
                        if (dgvRadio.Rows[i].Cells[0].Value == null)
                            continue;

                        if (dgvRadio.Rows[i].Cells[1].Value == null)
                            vsValue = "";
                        else
                            vsValue = dgvRadio.Rows[i].Cells[1].Value.ToString();

                        if (dgvRadio.Rows[i].Cells[0].Value.ToString().Trim() != "")
                            vDeRadioGroup.Propertys.Add(dgvRadio.Rows[i].Cells[0].Value.ToString(), vsValue);
                    }
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
            if (tbxComboboxValue.Text != "")
            {
                lstCombobox.Items.Add(tbxComboboxValue.Text);
                tbxComboboxValue.Clear();
            }
        }

        private void btnMod_Click(object sender, EventArgs e)
        {
            lstCombobox.SelectedItem = tbxComboboxValue.Text;
            tbxComboboxValue.Clear();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            lstCombobox.Items.Remove(lstCombobox.SelectedItem);
        }

        private void btnAddRadioItem_Click(object sender, EventArgs e)
        {
            lstRadioItem.Items.Add(tbxRadioValue.Text);
            tbxRadioValue.Clear();
        }

        private void btnModRadioItem_Click(object sender, EventArgs e)
        {
            lstRadioItem.SelectedItem = tbxRadioValue.Text;
            tbxRadioValue.Clear();
        }

        private void btnDeleteRadioItem_Click(object sender, EventArgs e)
        {
            lstRadioItem.Items.Remove(lstRadioItem.SelectedItem);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void lstRadioItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbxRadioValue.Text = lstRadioItem.SelectedItem.ToString();
        }

        private void lstCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbxComboboxValue.Text = lstCombobox.SelectedItem.ToString();
        }
    }
}
