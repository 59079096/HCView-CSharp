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
        private EventHandler FOnSelectChange, FOnInsertAsDeItem, FOnInsertAsDeGroup, FOnInsertAsDeEdit,
            FOnInsertAsDeCombobox, FOnInsertAsDeDateTime, FOnInsertAsDeRadioGroup,
            FOnInsertAsDeCheckBox, FOnInsertAsDeImage, FOnInsertAsDeFloatBarCode;

        public frmDataElement()
        {
            InitializeComponent();
        }

        private void FrmDataElement_Load(object sender, EventArgs e)
        {
            ShowDataElement();  // 数据元列表
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
                //TemplateInfo vTempInfo = new TemplateInfo();
                dgvDE.Rows[i].Cells[0].Value = vRows[i]["deid"];
                dgvDE.Rows[i].Cells[1].Value = vRows[i]["dename"];
                dgvDE.Rows[i].Cells[2].Value = vRows[i]["decode"];
                dgvDE.Rows[i].Cells[3].Value = vRows[i]["py"];
                dgvDE.Rows[i].Cells[4].Value = vRows[i]["frmtp"];
                dgvDE.Rows[i].Cells[5].Value = vRows[i]["domainid"];
            }

            if (FOnSelectChange != null)
                FOnSelectChange(this, null);
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

        private void mniInsertAsDeItem_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                if (FOnInsertAsDeItem != null)
                    FOnInsertAsDeItem(sender, e);
            }
        }

        private void MniInsertAsRadioGroup_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                if (FOnInsertAsDeRadioGroup != null)
                    FOnInsertAsDeRadioGroup(sender, e);
            }
        }

        private void MniInsertAsDeGroup_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                if (FOnInsertAsDeGroup != null)
                    FOnInsertAsDeGroup(sender, e);
            }
        }

        private void MniInsertAsEdit_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                if (FOnInsertAsDeEdit != null)
                    FOnInsertAsDeEdit(sender, e);
            }
        }

        private void MniInsertAsDateTime_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                if (FOnInsertAsDeDateTime != null)
                    FOnInsertAsDeDateTime(sender, e);
            }
        }

        private void MniInsertAsCheckBox_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                if (FOnInsertAsDeCheckBox != null)
                    FOnInsertAsDeCheckBox(sender, e);
            }
        }

        private void MniNew_Click(object sender, EventArgs e)
        {
            frmDeInfo frmDeInfo = new frmDeInfo();
            frmDeInfo.SetDeID(0);
            if (frmDeInfo.DialogResult == DialogResult.OK)
                MniRefresh_Click(sender, e);
        }

        private void MniEdit_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                frmDeInfo frmDeInfo = new frmDeInfo();
                frmDeInfo.SetDeID(int.Parse(dgvDE.SelectedRows[0].Cells[0].Value.ToString()));
                if (frmDeInfo.DialogResult == DialogResult.OK)
                    MniRefresh_Click(sender, e);
            }
        }

        private void MniDelete_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                int vRow = dgvDE.SelectedRows[0].Index;

                MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                DialogResult dr = MessageBox.Show("确定要删除数据元【 " + dgvDE.Rows[vRow].Cells[1].Value.ToString() + "】吗？", "确认操作", messButton);
                if (dr == DialogResult.OK)
                {
                    if (int.Parse(dgvDE.Rows[vRow].Cells[5].Value.ToString()) != 0)
                    {
                        MessageBoxButtons messButton2 = MessageBoxButtons.OKCancel;
                        DialogResult dr2 = MessageBox.Show("如果【 " + dgvDE.Rows[vRow].Cells[1].Value.ToString() + "】对应的值域 "
                            + dgvDE.Rows[vRow].Cells[5].Value.ToString() + "不再使用，请注意及时删除，继续删除数据元？", "确认操作", messButton2);
                        if (dr2 != DialogResult.OK)
                            return;
                    }

                    if (emrMSDB.DB.ExecSql(string.Format("DELETE FROM Comm_DataElement WHERE DeID = {0}", dgvDE.Rows[vRow].Cells[0].Value.ToString())))
                    {
                        MessageBox.Show("删除成功！");
                        MniRefresh_Click(sender, e);
                    }
                    else
                        MessageBox.Show("删除失败！" + emrMSDB.DB.ErrMsg);
                }
            }
        }

        private void MniDomain_Click(object sender, EventArgs e)
        {
            frmDomain vFrmDomain = new frmDomain();
            vFrmDomain.ShowDialog();
        }

        private void MniRefresh_Click(object sender, EventArgs e)
        {
            tbxPY.Clear();
            emrMSDB.DB.GetDataElement();
            ShowDataElement();
        }

        private void dgvDE_DoubleClick(object sender, EventArgs e)
        {
            mniInsertAsDeItem_Click(sender, e);
        }

        public int GetDomainID()
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                DataGridViewRow vSelectedRow = dgvDE.SelectedRows[0];
                if (vSelectedRow.Cells[5].Value != null)
                    return int.Parse(vSelectedRow.Cells[5].Value.ToString());
                else
                    return 0;
            }
            else
                return 0;
        }

        public string GetDeName()
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                DataGridViewRow vSelectedRow = dgvDE.SelectedRows[0];
                if (vSelectedRow.Cells[1].Value != null)
                    return vSelectedRow.Cells[1].Value.ToString();
                else
                    return "";
            }
            else
                return "";
        }

        public string GetDeIndex()
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                DataGridViewRow vSelectedRow = dgvDE.SelectedRows[0];
                if (vSelectedRow.Cells[0].Value != null)
                    return vSelectedRow.Cells[0].Value.ToString();
                else
                    return "";
            }
            else
                return "";
        }

        public EventHandler OnSelectRowChange
        {
            get { return FOnSelectChange; }
            set { FOnSelectChange = value; }
        }

        private void MniInsertAsCombobox_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                if (FOnInsertAsDeCombobox != null)
                    FOnInsertAsDeCombobox(sender, e);
            }
        }

        private void MniInsertAsFloatBarCode_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                if (FOnInsertAsDeFloatBarCode != null)
                    FOnInsertAsDeFloatBarCode(sender, e);
            }
        }

        private void DgvDE_SelectionChanged(object sender, EventArgs e)
        {
            if (FOnSelectChange != null)
                FOnSelectChange(sender, e);
        }

        public EventHandler OnSelectChange
        {
            get { return FOnSelectChange; }
            set { FOnSelectChange = value; }
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            MniRefresh_Click(sender, e);
        }

        private void Pmde_Opening(object sender, CancelEventArgs e)
        {
            mniEdit.Enabled = dgvDE.SelectedRows.Count > 0;
            mniDelete.Enabled = dgvDE.SelectedRows.Count > 0;
            mniInsertAsDeItem.Visible = (FOnInsertAsDeItem != null) && (dgvDE.SelectedRows.Count > 0);
            mniInsertAsDeGroup.Visible = (FOnInsertAsDeGroup != null) && (dgvDE.SelectedRows.Count > 0);
            mniInsertAsEdit.Visible = (FOnInsertAsDeEdit != null) && (dgvDE.SelectedRows.Count > 0);
            mniInsertAsCombobox.Visible = (FOnInsertAsDeCombobox != null) && (dgvDE.SelectedRows.Count > 0);
            mniInsertAsDateTime.Visible = (FOnInsertAsDeDateTime != null) && (dgvDE.SelectedRows.Count > 0);
            mniInsertAsRadioGroup.Visible = (FOnInsertAsDeRadioGroup != null) && (dgvDE.SelectedRows.Count > 0);
            mniInsertAsCheckBox.Visible = (FOnInsertAsDeCheckBox != null) && (dgvDE.SelectedRows.Count > 0);
            mniInsertAsImage.Visible = (FOnInsertAsDeImage != null) && (dgvDE.SelectedRows.Count > 0);
            mniInsertAsFloatBarCode.Visible = (FOnInsertAsDeFloatBarCode != null) && (dgvDE.SelectedRows.Count > 0);
        }

        private void mniInsertAsImage_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                if (FOnInsertAsDeImage != null)
                    FOnInsertAsDeImage(sender, e);
            }
        }

        public EventHandler OnInsertAsDeItem
        {
            get { return FOnInsertAsDeItem; }
            set { FOnInsertAsDeItem = value; }
        }

        public EventHandler OnInsertAsDeGroup
        {
            get { return FOnInsertAsDeGroup; }
            set { FOnInsertAsDeGroup = value; }
        }

        public EventHandler OnInsertAsDeEdit
        {
            get { return FOnInsertAsDeEdit; }
            set { FOnInsertAsDeEdit = value; }
        }

        public EventHandler OnInsertAsDeCombobox
        {
            get { return FOnInsertAsDeCombobox; }
            set { FOnInsertAsDeCombobox = value; }
        }

        public EventHandler OnInsertAsDeDateTime
        {
            get { return FOnInsertAsDeDateTime; }
            set { FOnInsertAsDeDateTime = value; }
        }

        public EventHandler OnInsertAsDeRadioGroup
        {
            get { return FOnInsertAsDeRadioGroup; }
            set { FOnInsertAsDeRadioGroup = value; }
        }

        public EventHandler OnInsertAsDeCheckBox
        {
            get { return FOnInsertAsDeCheckBox; }
            set { FOnInsertAsDeCheckBox = value; }
        }

        public EventHandler OnInsertAsDeImage
        {
            get { return FOnInsertAsDeImage; }
            set { FOnInsertAsDeImage = value; }
        }

        public EventHandler OnInsertAsDeFloatBarCode
        {
            get { return FOnInsertAsDeFloatBarCode; }
            set { FOnInsertAsDeFloatBarCode = value; }
        }
    }
}
