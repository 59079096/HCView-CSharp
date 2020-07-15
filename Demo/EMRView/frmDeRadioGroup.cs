using HC.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EMRView
{
    public partial class frmDeRadioGroup : Form
    {
        public frmDeRadioGroup()
        {
            InitializeComponent();
        }

        public void SetHCView(HCView aHCView, DeRadioGroup aRadioGroup)
        {
            if (aRadioGroup[DeProp.Name] != "")
                this.Text = aRadioGroup[DeProp.Name];

            cbxAutoSize.Checked = aRadioGroup.AutoSize;
            tbxWidth.Enabled = !cbxAutoSize.Checked;
            tbxHeight.Enabled = !cbxAutoSize.Checked;

            tbxWidth.Text = aRadioGroup.Width.ToString();
            tbxHeight.Text = aRadioGroup.Height.ToString();

            if (aRadioGroup.RadioStyle == HCRadioStyle.Radio)
                cbbRadioStyle.SelectedIndex = 0;
            else
                cbbRadioStyle.SelectedIndex = 1;

            cbxMulSelect.Checked = aRadioGroup.MultSelect;
            cbxDeleteAllow.Checked = aRadioGroup.DeleteAllow;

            tbxColume.Text = aRadioGroup.Columns.ToString();
            cbxColumnAlign.Checked = aRadioGroup.ColumnAlign;
            cbxItemHit.Checked = aRadioGroup.ItemHit;

            dgvRadioGroup.RowCount = aRadioGroup.Propertys.Count + 1;
            int vRow = 0;
            if (aRadioGroup.Propertys.Count > 0)
            {
                foreach (KeyValuePair<string, string> keyValuePair in aRadioGroup.Propertys)
                {
                    dgvRadioGroup.Rows[vRow].Cells[0].Value = keyValuePair.Key;
                    dgvRadioGroup.Rows[vRow].Cells[1].Value = keyValuePair.Value;
                    vRow++;
                }
            }

            dgvItem.RowCount = aRadioGroup.Items.Count + 1;
            vRow = 0;
            foreach (HCRadioButton vItem in aRadioGroup.Items)
            {
                dgvItem.Rows[vRow].Cells[0].Value = vItem.Text;
                dgvItem.Rows[vRow].Cells[1].Value = vItem.TextValue;
                vRow++;
            }

            this.ShowDialog();
            if (this.DialogResult == DialogResult.OK)
            {
                aRadioGroup.AutoSize = cbxAutoSize.Checked;
                if (!cbxAutoSize.Checked)  // 自定义大小
                {
                    int vi = aRadioGroup.Width;
                    if (int.TryParse(tbxWidth.Text, out vi))
                        aRadioGroup.Width = vi;

                    vi = aRadioGroup.Height;
                    if (int.TryParse(tbxHeight.Text, out vi))
                        aRadioGroup.Height = vi;
                }

                if (cbbRadioStyle.SelectedIndex == 0)
                    aRadioGroup.RadioStyle = HCRadioStyle.Radio;
                else
                    aRadioGroup.RadioStyle = HCRadioStyle.CheckBox;

                aRadioGroup.MultSelect = cbxMulSelect.Checked;
                aRadioGroup.DeleteAllow = cbxDeleteAllow.Checked;
                aRadioGroup.ItemHit = cbxItemHit.Checked;

                string vsValue = "";
                aRadioGroup.Propertys.Clear();
                for (int i = 0; i < dgvRadioGroup.RowCount; i++)
                {
                    if (dgvRadioGroup.Rows[i].Cells[0].Value == null)
                        continue;

                    if (dgvRadioGroup.Rows[i].Cells[1].Value == null)
                        vsValue = "";
                    else
                        vsValue = dgvRadioGroup.Rows[i].Cells[1].Value.ToString();

                    if (dgvRadioGroup.Rows[i].Cells[0].Value.ToString().Trim() != "")
                        aRadioGroup.Propertys.Add(dgvRadioGroup.Rows[i].Cells[0].Value.ToString(), vsValue);
                }

                aRadioGroup.BeginAdd();
                try
                {
                    byte vByte = 0;
                    if (byte.TryParse(tbxColume.Text, out vByte))
                        aRadioGroup.Columns = vByte;
                    else
                        aRadioGroup.Columns = 0;

                    aRadioGroup.ColumnAlign = cbxColumnAlign.Checked;

                    aRadioGroup.Items.Clear();
                    for (int i = 0; i < dgvItem.RowCount; i++)
                    {
                        if (dgvItem.Rows[i].Cells[0].Value == null)
                            continue;

                        if (dgvItem.Rows[i].Cells[1].Value == null)
                            vsValue = "";
                        else
                            vsValue = dgvItem.Rows[i].Cells[1].Value.ToString();

                        if (dgvItem.Rows[i].Cells[0].Value.ToString().Trim() != "")
                            aRadioGroup.AddItem(dgvItem.Rows[i].Cells[0].Value.ToString(), vsValue);
                    }
                }
                finally
                {
                    aRadioGroup.EndAdd();
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}