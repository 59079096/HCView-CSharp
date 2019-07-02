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
    public partial class frmDomain : Form
    {
        public frmDomain()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
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
        }

        private void frmDomain_Load(object sender, EventArgs e)
        {
            GetAllDomain();
        }

        private void mniNew_Click(object sender, EventArgs e)
        {
            frmDomainOper vFrmDomainOper = new frmDomainOper();
            vFrmDomainOper.SetDID(0, "", "");
            if (vFrmDomainOper.DialogResult == System.Windows.Forms.DialogResult.OK)
                GetAllDomain();
        }

        private void mniEdit_Click(object sender, EventArgs e)
        {
            if (dgvDomain.SelectedRows.Count > 0)
            {
                int vRow = dgvDomain.SelectedRows[0].Index;
                frmDomainOper vFrmDomainOper = new frmDomainOper();
                vFrmDomainOper.SetDID(int.Parse(dgvDomain.Rows[vRow].Cells[0].Value.ToString()),
                    dgvDomain.Rows[vRow].Cells[1].Value.ToString(),
                    dgvDomain.Rows[vRow].Cells[2].Value.ToString());

                if (vFrmDomainOper.DialogResult == System.Windows.Forms.DialogResult.OK)
                    GetAllDomain();
            }
        }

        #region 子方法
        private bool DeleteDomainItemContent(int aDItemID)
        {
            return emrMSDB.DB.ExecSql(string.Format("DELETE FROM Comm_DomainContent WHERE DItemID = {0} ", aDItemID));
        }

        private bool DeleteDomainAllItem(int aDomainID)
        {
            return emrMSDB.DB.ExecSql(string.Format("DELETE FROM Comm_DataElementDomain WHERE DomainID = {0}", aDomainID));
        }
        #endregion

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
                        if (!DeleteDomainItemContent(int.Parse(dt.Rows[i]["ID"].ToString())))
                        {
                            MessageBox.Show("删除值域选项关联内容失败，"+ emrMSDB.DB.ErrMsg);
                            return;
                        }
                    }

                    if (!DeleteDomainAllItem(vDomainID))  // 删除值域对应的所有选项
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
    }
}
