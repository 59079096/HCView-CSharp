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
using System.Windows.Forms;

namespace EMRView
{
    public partial class frmDomainInfo : Form
    {
        private int FDID;

        public frmDomainInfo()
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
