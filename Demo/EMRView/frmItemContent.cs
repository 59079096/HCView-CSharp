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
using HC.View;
using System.IO;
using System.Data.SqlClient;

namespace EMRView
{
    public partial class frmItemContent : Form
    {
        private int FDomainItemID = 0;
        private HCEmrEdit FEmrEdit;

        private void DoSaveItemContent()
        {
            using (MemoryStream vSM = new MemoryStream())
            {
                FEmrEdit.SaveToStream(vSM);

                EMRView.emrMSDB.ExecCommandEventHanler vEvent = delegate(SqlCommand sqlComm)
                {
                    sqlComm.Parameters.AddWithValue("DItemID", FDomainItemID);
                    sqlComm.Parameters.AddWithValue("content", vSM.GetBuffer());
                };
                
                if (emrMSDB.DB.ExecSql(emrMSDB.Sql_SaveDomainItemContent, vEvent))
                    MessageBox.Show("保存成功！");
                else
                    MessageBox.Show(emrMSDB.DB.ErrMsg);
            }
        }

        private void SetDomainItemID(int value)
        {
            FDomainItemID = value;

            DataTable dt = emrMSDB.DB.GetData(string.Format(emrMSDB.Sql_GetDomainItemContent, FDomainItemID));
            if (dt.Rows.Count > 0)
            {
                using (MemoryStream vSM = new MemoryStream((byte[])dt.Rows[0]["content"]))
                {
                    if (vSM.Length > 0)
                        FEmrEdit.LoadFromStream(vSM);
                    else
                        FEmrEdit.Clear();
                }
            }
        }

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
                TemplateInfo vTempInfo = new TemplateInfo();
                dgvDE.Rows[i].Cells[0].Value = vRows[i]["deid"];
                dgvDE.Rows[i].Cells[1].Value = vRows[i]["dename"];
                dgvDE.Rows[i].Cells[2].Value = vRows[i]["decode"];
                dgvDE.Rows[i].Cells[3].Value = vRows[i]["py"];
                dgvDE.Rows[i].Cells[4].Value = vRows[i]["frmtp"];
                dgvDE.Rows[i].Cells[5].Value = vRows[i]["domainid"];
            }
        }

        public frmItemContent()
        {
            HCTextItem.HCDefaultTextItemClass = typeof(DeItem);
            HCDomainItem.HCDefaultDomainItemClass = typeof(DeGroup);

            InitializeComponent();
            this.ShowInTaskbar = false;

            if (FEmrEdit == null)
            {
                FEmrEdit = new HCEmrEdit();
                this.pnlEdit.Controls.Add(FEmrEdit);
                FEmrEdit.Dock = DockStyle.Fill;
                FEmrEdit.Show();
            }
        }

        public int DomainItemID
        {
            get { return FDomainItemID; }
            set { SetDomainItemID(value); }
        }

        private void frmItemContent_Load(object sender, EventArgs e)
        {
            ShowDataElement();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DoSaveItemContent();
        }

        private void cbbFont_DropDownClosed(object sender, EventArgs e)
        {
            FEmrEdit.ApplyTextFontName(cbbFont.Text);
        }

        private void cbbFontSize_DropDownClosed(object sender, EventArgs e)
        {
            FEmrEdit.ApplyTextFontSize(HC.View.HC.GetFontSize(cbbFontSize.Text));
        }

        private void btnBold_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripButton).Tag.ToString())
            {
                case "0":
                    FEmrEdit.ApplyTextStyle(HCFontStyle.tsBold);
                    break;

                case "1":
                    FEmrEdit.ApplyTextStyle(HCFontStyle.tsItalic);
                    break;

                case "2":
                    FEmrEdit.ApplyTextStyle(HCFontStyle.tsUnderline);
                    break;

                case "3":
                    FEmrEdit.ApplyTextStyle(HCFontStyle.tsStrikeOut);
                    break;

                case "4":
                    FEmrEdit.ApplyTextStyle(HCFontStyle.tsSuperscript);
                    break;

                case "5":
                    FEmrEdit.ApplyTextStyle(HCFontStyle.tsSubscript);
                    break;
            }
        }

        private void btnRightIndent_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripButton).Tag.ToString())
            {
                case "0":
                    FEmrEdit.ApplyParaAlignHorz(ParaAlignHorz.pahLeft);
                    break;

                case "1":
                    FEmrEdit.ApplyParaAlignHorz(ParaAlignHorz.pahCenter);
                    break;

                case "2":
                    FEmrEdit.ApplyParaAlignHorz(ParaAlignHorz.pahRight);
                    break;

                case "3":
                    FEmrEdit.ApplyParaAlignHorz(ParaAlignHorz.pahJustify);  // 两端
                    break;

                case "4":
                    FEmrEdit.ApplyParaAlignHorz(ParaAlignHorz.pahScatter);  // 分散
                    break;
            }
        }

        private void dgvDE_DoubleClick(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count == 0)
                return;

            int vRow = dgvDE.SelectedRows[0].Index;
            DeItem vDeItem = new DeItem(dgvDE.Rows[vRow].Cells[1].Value.ToString());
            if (FEmrEdit.CurStyleNo > HCStyle.Null)
                vDeItem.StyleNo = FEmrEdit.CurStyleNo;
            else
                vDeItem.StyleNo = 0;

            vDeItem.ParaNo = FEmrEdit.CurParaNo;

            vDeItem[DeProp.Name] = dgvDE.Rows[vRow].Cells[1].Value.ToString();
            vDeItem[DeProp.Index] = dgvDE.Rows[vRow].Cells[0].Value.ToString();

            FEmrEdit.InsertItem(vDeItem);
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
    }
}
