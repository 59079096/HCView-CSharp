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
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

namespace EMRView
{
    public partial class frmDataElementDomain : Form
    {
        private int FDomainID;
        private string FDeName;

        private void GetDomainItem()
        {
            dgvCV.RowCount = 1;
            if (FDomainID > 0)
            {
                DataTable dt = emrMSDB.DB.GetData(string.Format(emrMSDB.Sql_GetDomainItem, (FDomainID)));

                dgvCV.RowCount = dt.Rows.Count;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dgvCV.Rows[i].Cells[0].Value = dt.Rows[i]["devalue"];
                    dgvCV.Rows[i].Cells[1].Value = dt.Rows[i]["code"];
                    dgvCV.Rows[i].Cells[2].Value = dt.Rows[i]["py"];
                    dgvCV.Rows[i].Cells[3].Value = dt.Rows[i]["id"];

                    dgvCV.Rows[i].Cells[4].Value = "";

                    if (dt.Rows[i]["content"].GetType() != typeof(System.DBNull))
                    {
                        byte[] vbuffer = (byte[])dt.Rows[i]["content"];
                        if (vbuffer.Length > 0)
                            dgvCV.Rows[i].Cells[4].Value = "...";
                    }
                }
            }
            else
                dgvCV.RowCount = 0;
        }

        private void SetDomainID(int value)
        {
            if (FDomainID != value)
            {
                FDomainID = value;
                GetDomainItem();
            }
        }

        private void SetDeName(string value)
        {
            if (FDeName != value)
            {
                FDeName = value;
                lblDE.Text = "[" + FDeName + "] 选项如下(点击刷新)";
            }
        }

        public frmDataElementDomain()
        {
            InitializeComponent();
        }

        public int DomainID
        {
            get { return FDomainID; }
            set { SetDomainID(value); }
        }

        public string DeName
        {
            get { return FDeName; }
            set { SetDeName(value); }
        }

        private void PmCV_Opening(object sender, CancelEventArgs e)
        {
            mniNewItem.Visible = FDomainID > 0;
            mniEditItem.Visible = dgvCV.SelectedRows.Count > 0;
            mniDeleteItem.Visible = dgvCV.SelectedRows.Count > 0;
            mniEditItemLink.Visible = dgvCV.SelectedRows.Count > 0;
            mniDeleteItemLink.Visible = dgvCV.SelectedRows.Count > 0;
        }

        private void LblDE_Click(object sender, EventArgs e)
        {
            GetDomainItem();
        }

        private void MniNewItem_Click(object sender, EventArgs e)
        {
            frmDomainItem vFrmDomainItem = new frmDomainItem();
            vFrmDomainItem.SetInfo(FDomainID, 0);
            if (vFrmDomainItem.DialogResult == DialogResult.OK)
                GetDomainItem();
        }

        private void MniEditItem_Click(object sender, EventArgs e)
        {
            frmDomainItem vFrmDomainItem = new frmDomainItem();
            vFrmDomainItem.SetInfo(FDomainID, int.Parse(dgvCV.Rows[dgvCV.SelectedRows[0].Index].Cells[3].Value.ToString()));
            if (vFrmDomainItem.DialogResult == DialogResult.OK)
                GetDomainItem();
        }

        private void MniDeleteItem_Click(object sender, EventArgs e)
        {
            int vRow = dgvCV.SelectedRows[0].Index;

            MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
            DialogResult dr = MessageBox.Show("确定要删除选项【 " + dgvCV.Rows[vRow].Cells[0].Value.ToString() + "】和该选项对应的扩展内容吗？", "确认操作", messButton);
            if (dr == DialogResult.OK)
            {
                // 删除扩展内容
                if (emrMSDB.DB.DeleteDomainItemContent(int.Parse(dgvCV.Rows[vRow].Cells[3].Value.ToString())))
                {
                    MessageBox.Show("删除选项扩展内容成功！");

                    // 删除选项
                    if (emrMSDB.DB.DeleteDomainItem(int.Parse(dgvCV.Rows[vRow].Cells[3].Value.ToString())))
                    {
                        GetDomainItem();
                        MessageBox.Show("删除选项成功！");
                    }
                    else
                        MessageBox.Show("删除选项失败！" + emrMSDB.DB.ErrMsg);
                }
                else
                    MessageBox.Show("删除选项扩展内容失败！" + emrMSDB.DB.ErrMsg);
            }
        }

        private void MniEditItemLink_Click(object sender, EventArgs e)
        {
            if (dgvCV.SelectedRows.Count == 0)
                return;

            int vRow = dgvCV.SelectedRows[0].Index;
            if (dgvCV.Rows[vRow].Cells[3].Value.ToString() == "")
                return;

            frmItemContent vFrmItemContent = new frmItemContent();
            vFrmItemContent.DomainItemID = int.Parse(dgvCV.Rows[vRow].Cells[3].Value.ToString());
            vFrmItemContent.ShowDialog();
        }

        private void MniDeleteItemLink_Click(object sender, EventArgs e)
        {
            int vRow = dgvCV.SelectedRows[0].Index;

            MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
            DialogResult dr = MessageBox.Show("确定要删除选项【 " + dgvCV.Rows[vRow].Cells[0].Value.ToString() + "】的扩展内容吗？", "确认操作", messButton);
            if (dr == DialogResult.OK)
            {
                int vDomainItemID = int.Parse(dgvCV.Rows[vRow].Cells[3].Value.ToString());

                if (emrMSDB.DB.DeleteDomainItemContent(vDomainItemID))
                    MessageBox.Show("删除值域选项扩展内容成功！");
                else
                    MessageBox.Show("删除失败:" + emrMSDB.DB.ErrMsg);
            }
        }
    }
}
