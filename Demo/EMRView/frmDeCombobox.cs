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
    public partial class frmDeCombobox : Form
    {
        public frmDeCombobox()
        {
            InitializeComponent();
        }

        public void SetHCView(HCView aHCView, DeCombobox aCombobox)
        {
            if (aCombobox[DeProp.Name] != "")
                this.Text = aCombobox[DeProp.Name];

            cbxAutoSize.Checked = aCombobox.AutoSize;
            tbxWidth.Enabled = !cbxAutoSize.Checked;
            tbxHeight.Enabled = !cbxAutoSize.Checked;

            tbxWidth.Text = aCombobox.Width.ToString();
            tbxHeight.Text = aCombobox.Height.ToString();
            tbxText.Text = aCombobox.Text;
            cbxPrintOnlyText.Checked = aCombobox.PrintOnlyText;
            cbxDeleteAllow.Checked = aCombobox.DeleteAllow;
            cbxBorderLeft.Checked = aCombobox.BorderSides.Contains((byte)BorderSide.cbsLeft);
            cbxBorderTop.Checked = aCombobox.BorderSides.Contains((byte)BorderSide.cbsTop);
            cbxBorderRight.Checked = aCombobox.BorderSides.Contains((byte)BorderSide.cbsRight);
            cbxBorderBottom.Checked = aCombobox.BorderSides.Contains((byte)BorderSide.cbsBottom);

            dgvCombobox.RowCount = aCombobox.Propertys.Count + 1;
            if (aCombobox.Propertys.Count > 0)
            {
                int vRow = 0;
                foreach (KeyValuePair<string, string> keyValuePair in aCombobox.Propertys)
                {
                    dgvCombobox.Rows[vRow].Cells[0].Value = keyValuePair.Key;
                    dgvCombobox.Rows[vRow].Cells[1].Value = keyValuePair.Value;
                    vRow++;
                }
            }

            cbxSaveItem.Checked = aCombobox.SaveItem;
            dgvItem.Enabled = cbxSaveItem.Checked;
            dgvItem.RowCount = aCombobox.Items.Count + 1;
            if (aCombobox.Items.Count > 0)
            {
                for (int i = 0; i < aCombobox.Items.Count; i++)
                {
                    dgvItem.Rows[i].Cells[0].Value = aCombobox.Items[i].Text;
                    if (i < aCombobox.ItemValues.Count)
                        dgvItem.Rows[i].Cells[1].Value = aCombobox.ItemValues[i].Text;
                }
            }

            this.ShowDialog();
            if (this.DialogResult == DialogResult.OK)
            {
                aCombobox.AutoSize = cbxAutoSize.Checked;
                if (!cbxAutoSize.Checked)  // 自定义大小
                {
                    int vi = aCombobox.Width;
                    if (int.TryParse(tbxWidth.Text, out vi))
                        aCombobox.Width = vi;

                    vi = aCombobox.Height;
                    if (int.TryParse(tbxHeight.Text, out vi))
                        aCombobox.Height = vi;
                }

                aCombobox.Text = tbxText.Text;

                if (cbxBorderLeft.Checked)
                    aCombobox.BorderSides.InClude((byte)BorderSide.cbsLeft);
                else
                    aCombobox.BorderSides.ExClude((byte)BorderSide.cbsLeft);

                if (cbxBorderTop.Checked)
                    aCombobox.BorderSides.InClude((byte)BorderSide.cbsTop);
                else
                    aCombobox.BorderSides.ExClude((byte)BorderSide.cbsTop);

                if (cbxBorderRight.Checked)
                    aCombobox.BorderSides.InClude((byte)BorderSide.cbsRight);
                else
                    aCombobox.BorderSides.ExClude((byte)BorderSide.cbsRight);

                if (cbxBorderBottom.Checked)
                    aCombobox.BorderSides.InClude((byte)BorderSide.cbsBottom);
                else
                    aCombobox.BorderSides.ExClude((byte)BorderSide.cbsBottom);

                aCombobox.PrintOnlyText = cbxPrintOnlyText.Checked;
                aCombobox.DeleteAllow = cbxDeleteAllow.Checked;

                string vsValue = "";
                aCombobox.Propertys.Clear();
                for (int i = 0; i < dgvCombobox.RowCount; i++)
                {
                    if (dgvCombobox.Rows[i].Cells[0].Value == null)
                        continue;

                    if (dgvCombobox.Rows[i].Cells[1].Value == null)
                        vsValue = "";
                    else
                        vsValue = dgvCombobox.Rows[i].Cells[1].Value.ToString();

                    if (dgvCombobox.Rows[i].Cells[0].Value.ToString().Trim() != "")
                        aCombobox.Propertys.Add(dgvCombobox.Rows[i].Cells[0].Value.ToString(), vsValue);
                }

                aCombobox.SaveItem = cbxSaveItem.Checked;
                aCombobox.Items.Clear();
                aCombobox.ItemValues.Clear();
                for (int i = 0; i < dgvItem.RowCount; i++)
                {
                    if (dgvItem.Rows[i].Cells[0].Value == null)
                        continue;

                    if (dgvItem.Rows[i].Cells[1].Value == null)
                        vsValue = "";
                    else
                        vsValue = dgvItem.Rows[i].Cells[1].Value.ToString();

                    if (dgvItem.Rows[i].Cells[0].Value.ToString() != "")
                    {
                        aCombobox.Items.Add(new HCCbbItem(dgvItem.Rows[i].Cells[0].Value.ToString()));
                        aCombobox.ItemValues.Add(new HCCbbItem(vsValue));
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void cbxSaveItem_CheckedChanged(object sender, EventArgs e)
        {
            dgvItem.Enabled = cbxSaveItem.Checked;
        }
    }
}
