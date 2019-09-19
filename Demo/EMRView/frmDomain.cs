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
using System.Data;
using System.Windows.Forms;

namespace EMRView
{
    public partial class frmDomain : Form
    {
        private frmDataElementDomain frmDataElementDomain;
        public frmDomain()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;

            frmDataElementDomain = new frmDataElementDomain();
            frmDataElementDomain.FormBorderStyle = FormBorderStyle.None;
            frmDataElementDomain.Dock = DockStyle.Fill;
            frmDataElementDomain.TopLevel = false;
            this.pnlDomainItem.Controls.Add(frmDataElementDomain);
            frmDataElementDomain.Show();
        }

        private void GetAllDomain()
        {
            DataTable dt = emrMSDB.DB.GetData("SELECT DID, DCode, DName FROM Comm_Dic_Domain");
            dgvDomain.RowCount = dt.Rows.Count;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dgvDomain.Rows[i].Cells[0].Value = dt.Rows[i]["DID"];
                dgvDomain.Rows[i].Cells[1].Value = dt.Rows[i]["DCode"];
                dgvDomain.Rows[i].Cells[2].Value = dt.Rows[i]["DName"];
            }

            DgvDomain_SelectionChanged(null, null);
        }

        private void frmDomain_Load(object sender, EventArgs e)
        {
            GetAllDomain();
        }

        private void mniNew_Click(object sender, EventArgs e)
        {
            frmDomainInfo vFrmDomainOper = new frmDomainInfo();
            vFrmDomainOper.SetDID(0, "", "");
            if (vFrmDomainOper.DialogResult == System.Windows.Forms.DialogResult.OK)
                GetAllDomain();
        }

        private void mniEdit_Click(object sender, EventArgs e)
        {
            if (dgvDomain.SelectedRows.Count > 0)
            {
                int vRow = dgvDomain.SelectedRows[0].Index;
                frmDomainInfo vFrmDomainOper = new frmDomainInfo();
                vFrmDomainOper.SetDID(int.Parse(dgvDomain.Rows[vRow].Cells[0].Value.ToString()),
                    dgvDomain.Rows[vRow].Cells[1].Value.ToString(),
                    dgvDomain.Rows[vRow].Cells[2].Value.ToString());

                if (vFrmDomainOper.DialogResult == System.Windows.Forms.DialogResult.OK)
                    GetAllDomain();
            }
        }

        private void mniDelete_Click(object sender, EventArgs e)
        {
            if (dgvDomain.SelectedRows.Count > 0)
            {
                int vRow = dgvDomain.SelectedRows[0].Index;

                MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                DialogResult dr = MessageBox.Show("确定要删除值域【 " + dgvDomain.Rows[vRow].Cells[1].Value.ToString() + "】以及其对应的选项及选项关联的内容吗？", "确认操作", messButton);
                if (dr == DialogResult.OK)
                {
                    int vDomainID = int.Parse(dgvDomain.Rows[vRow].Cells[0].Value.ToString());

                    DataTable dt = emrMSDB.DB.GetData(string.Format("SELECT DE.ID, DE.Code, DE.devalue, DE.PY, DC.Content FROM Comm_DataElementDomain DE LEFT JOIN Comm_DomainContent DC ON DE.ID = DC.DItemID WHERE DE.domainid = {0}", vDomainID));

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (!emrMSDB.DB.DeleteDomainItemContent(int.Parse(dt.Rows[i]["ID"].ToString())))
                        {
                            MessageBox.Show("删除值域选项关联内容失败，"+ emrMSDB.DB.ErrMsg);
                            return;
                        }
                    }

                    if (!emrMSDB.DB.DeleteDomainAllItem(vDomainID))  // 删除值域对应的所有选项
                    {
                        MessageBox.Show("删除值域选项关联内容失败，"+ emrMSDB.DB.ErrMsg);
                        return;
                    }

                    if (emrMSDB.DB.ExecSql(string.Format( "DELETE FROM Comm_Dic_Domain WHERE DID = {0}", vDomainID)))
                    {
                        GetAllDomain();
                        MessageBox.Show("删除值域成功");
                    }
                }
            }
        }

        private void DgvDomain_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDomain.SelectedRows.Count > 0)
            {
                DataGridViewRow vSelectedRow = dgvDomain.SelectedRows[0];
                if (vSelectedRow.Cells[0].Value != null)
                {
                    if (frmDataElementDomain.DomainID != int.Parse(vSelectedRow.Cells[0].Value.ToString()))
                        frmDataElementDomain.DomainID = int.Parse(vSelectedRow.Cells[0].Value.ToString());
                }
            };
        }
    }
}
