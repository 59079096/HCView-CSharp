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
    public partial class frmDeProperty : Form
    {
        public frmDeProperty()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }

        public void SetHCView(HCView aHCView)
        {
            DeItem vDeItem = aHCView.ActiveSectionTopLevelData().GetActiveItem() as DeItem;
            dgvProperty.RowCount = vDeItem.Propertys.Count + 1;

            int vRow = 0;
            foreach (KeyValuePair<string, string> keyValuePair in vDeItem.Propertys)
            {
                dgvProperty.Rows[vRow].Cells[0].Value = keyValuePair.Key;
                dgvProperty.Rows[vRow].Cells[1].Value = keyValuePair.Value;
                vRow++;
            }

            cbxCanEdit.Checked = !vDeItem.EditProtect;
            cbxCanEdit.Checked = !vDeItem.CopyProtect;

            this.ShowDialog();
            if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                string vsValue = "";

                vDeItem.Propertys.Clear();
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
                        vDeItem.Propertys.Add(dgvProperty.Rows[i].Cells[0].Value.ToString(), vsValue);
                    }
                }

                vDeItem.EditProtect = !cbxCanEdit.Checked;
                vDeItem.CopyProtect = !cbxCanEdit.Checked;
            }

            //Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
