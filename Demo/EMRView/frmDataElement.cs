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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EMRView
{
    public delegate void InsertAsDeEventHandler(string aIndex, string aName);

    public partial class frmDataElement : Form
    {
        //private InsertAsDeEventHandler FOnInsertAsDE;

        public frmDataElement()
        {
            InitializeComponent();
        }

        //public InsertAsDeEventHandler OnInsertAsDE
        //{
        //    get { return FOnInsertAsDE; }
        //    set { FOnInsertAsDE = value; }
        //}

        private void ShowDataElement(DataRow[] aRows = null)
        {
            DataRow[] vRows = null;
            if (aRows == null)
            {
                vRows = new DataRow[emrMSDB.DB.DataElementDT.Rows.Count];
                emrMSDB.DB.DataElementDT.Rows.CopyTo(vRows, 0);
            }
            else
                vRows = aRows;

            dgvDE.RowCount = vRows.Length;
            for (int i = 0; i < vRows.Length; i++)
            {
                //TemplateInfo vTempInfo = new TemplateInfo();
                dgvDE.Rows[i].Cells[0].Value = vRows[i]["deid"];
                dgvDE.Rows[i].Cells[1].Value = vRows[i]["dename"];
                dgvDE.Rows[i].Cells[2].Value = vRows[i]["decode"];
                dgvDE.Rows[i].Cells[3].Value = vRows[i]["py"];
                dgvDE.Rows[i].Cells[4].Value = vRows[i]["frmtp"];
                dgvDE.Rows[i].Cells[5].Value = vRows[i]["domainid"];
            }
        }

        private void tbxPY_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {

                if (tbxPY.Text == "")
                {
                    ShowDataElement();
                }
                else
                {
                    DataRow[] vRows = null;
                    if (EMR.IsPY(tbxPY.Text[0]))
                        vRows = emrMSDB.DB.DataElementDT.Select("py like '%" + tbxPY.Text + "%'");
                    else
                        vRows = emrMSDB.DB.DataElementDT.Select("dename like '%" + tbxPY.Text + "%'");

                    ShowDataElement(vRows);
                }
            }
        }

        private void DoInsertAsDE(string aIndex, string aName)
        {
            //if (FOnInsertAsDE != null)
            //    FOnInsertAsDE(aIndex, aName);
        }

        private void mniInsertAsDE_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                int vRow = dgvDE.SelectedRows[0].Index;
                DoInsertAsDE(dgvDE.Rows[vRow].Cells[0].Value.ToString(), dgvDE.Rows[vRow].Cells[1].Value.ToString());
            }
        }

        private void dgvDE_DoubleClick(object sender, EventArgs e)
        {
            mniInsertAsDE_Click(sender, e);
        }
    }
}
