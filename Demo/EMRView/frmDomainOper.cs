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
    public partial class frmDomainOper : Form
    {
        private int FDID;

        public frmDomainOper()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }

        public void SetDID(int aDID, string aCode, string aName)
        {
            FDID = aDID;
            tbxCode.Text = aCode;
            tbxName.Text = aName;
            this.ShowDialog();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (tbxName.Text.Trim() == "")
            {
                MessageBox.Show("错误，请填写值域名称！");
                return;
            }

            string vSql = "";
            if (FDID > 0)  // 修改
                vSql = string.Format("UPDATE Comm_Dic_Domain SET DCode = '{0}', DName = '{1}' WHERE DID = {2}",
                    tbxCode.Text, tbxName.Text, FDID);
            else
                vSql = string.Format("INSERT INTO Comm_Dic_Domain (DCode, DName) VALUES ('{0}', '{1}')",
                    tbxCode.Text, tbxName.Text);

            if (emrMSDB.DB.ExecSql(vSql))
                MessageBox.Show("保存成功！");
            else
            {
                MessageBox.Show("保存失败！" + emrMSDB.DB.ErrMsg);
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
