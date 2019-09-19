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
    public partial class frmDomainItem : Form
    {
        private int FDomainID;
        private int FItemID;

        public frmDomainItem()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }

        public void SetInfo(int aDomainID, int aItemID)
        {
            FDomainID = aDomainID;
            FItemID = aItemID;

            if (FItemID > 0)  // 修改
            {
                DataTable dt = emrMSDB.DB.GetData(string.Format("SELECT devalue, py, code FROM Comm_DataElementDomain WHERE ID = {0}", FItemID));
                if (dt.Rows.Count > 0)
                {
                    tbxName.Text = dt.Rows[0]["devalue"].ToString();
                    tbxPY.Text = dt.Rows[0]["py"].ToString();
                    tbxCode.Text = dt.Rows[0]["code"].ToString();
                }
            }

            this.ShowDialog();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (tbxName.Text.Trim() == "")
            {
                MessageBox.Show("错误，填写项目名称！");
                return;
            }

            string vSql = "";
            if (FItemID > 0)  // 修改
                vSql = string.Format("UPDATE Comm_DataElementDomain SET devalue = '{0}', py = '{1}', code = '{2}' WHERE ID = {3}", 
                    tbxName.Text, tbxPY.Text, tbxCode.Text, FItemID);
            else
                vSql = string.Format("INSERT INTO Comm_DataElementDomain (domainid, code, devalue, py) VALUES ({0}, '{1}', '{2}', '{3}')",
                    FDomainID, tbxCode.Text, tbxName.Text, tbxPY.Text);

            if (emrMSDB.DB.ExecSql(vSql))
                MessageBox.Show("保存成功！");
            else
            {
                MessageBox.Show("保存失败！" + emrMSDB.DB.ErrMsg);
                return;
            }

            if (FItemID == 0)  // 新建后关闭
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
