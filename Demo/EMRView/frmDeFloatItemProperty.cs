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
    public partial class frmDeFloatItemProperty : Form
    {
        public frmDeFloatItemProperty()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        public void SetHCView(HC.View.HCView aHCView)
        {
            HCCustomFloatItem vFloatItem = aHCView.ActiveSection.ActiveData.GetActiveFloatItem();

            tbxWidth.Text = vFloatItem.Width.ToString();
            tbxHeight.Text = vFloatItem.Height.ToString();

            DeFloatBarCodeItem vFloatBarCode = null;
            if (vFloatItem is DeFloatBarCodeItem)
            {
                vFloatBarCode = vFloatItem as DeFloatBarCodeItem;

                dgvProperty.RowCount = vFloatBarCode.Propertys.Count + 1;
                if (vFloatBarCode.Propertys.Count > 0)
                {
                    int vRow = 0;
                    foreach (KeyValuePair<string, string> keyValuePair in vFloatBarCode.Propertys)
                    {
                        dgvProperty.Rows[vRow].Cells[0].Value = keyValuePair.Key;
                        dgvProperty.Rows[vRow].Cells[1].Value = keyValuePair.Value;
                        vRow++;
                    }
                }
            }

            this.ShowDialog();
            if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                int vValue = vFloatItem.Width;
                if (int.TryParse(tbxWidth.Text, out vValue))
                    vFloatItem.Width = vValue;

                vValue = vFloatItem.Height;
                if (int.TryParse(tbxHeight.Text, out vValue))
                        vFloatItem.Height = vValue;

                if (vFloatBarCode != null)
                {
                    string vsValue = "";
                    vFloatBarCode.Propertys.Clear();
                    for (int i = 0; i < dgvProperty.RowCount; i++)
                    {
                        if (dgvProperty.Rows[i].Cells[0].Value == null)
                            continue;

                        if (dgvProperty.Rows[i].Cells[1].Value == null)
                            vsValue = "";
                        else
                            vsValue = dgvProperty.Rows[i].Cells[1].Value.ToString();

                        if (dgvProperty.Rows[i].Cells[0].Value.ToString().Trim() != "")
                        {
                            vFloatBarCode.Propertys.Add(dgvProperty.Rows[i].Cells[0].Value.ToString(), vsValue);
                        }
                    }
                }
            }
        }
    }
}
