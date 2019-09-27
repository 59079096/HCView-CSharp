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
using System.IO;
using System.Windows.Forms;

namespace EMRView
{
    public partial class frmRecordSet : Form
    {
        private frmRecord FRecord;

        private void GetPatientInchRecord(string aPatID, int aVisit)
        {
            dgvRecord.Rows.Clear();
            DataTable dt = emrMSDB.DB.GetData(string.Format(emrMSDB.Sql_GetInchRecordList, aPatID, aVisit));

            dgvRecord.RowCount = dt.Rows.Count;
            dgvRecord.ColumnCount = 5;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dgvRecord.Rows[i].Cells[2].Value = dt.Rows[i]["Name"].ToString();
                dgvRecord.Rows[i].Cells[3].Value = dt.Rows[i]["DT"].ToString();
                dgvRecord.Rows[i].Cells[4].Value = dt.Rows[i]["ID"].ToString();
            }
        }

        public frmRecordSet()
        {
            InitializeComponent();
        }

        public void ShowDialog(string aPatID, int aVisit, int aRecordID = -1)
        {
            GetPatientInchRecord(aPatID, aVisit);

            for (int i = 0; i < dgvRecord.Rows.Count; i++)
            {
                if (dgvRecord.Rows[i].Cells[4].Value.ToString() == aRecordID.ToString())
                {
                    dgvRecord.Rows[i].Cells[0].Value = true;
                    break;
                }
            }

            ShowDialog();
        }

        private void BtnShow_Click(object sender, EventArgs e)
        {
            FRecord.EmrView.BeginUpdate();
            try
            {
                FRecord.EmrView.ReadOnly = false;  // 防止多次加载上一次只读影响下一次加载
                FRecord.EmrView.HideTrace = false;  // 默认为不显示痕迹
                FRecord.EmrView.Clear();
                FRecord.EmrView.PageNoFormat = tbxPageNoFmt.Text;

                if (cbxPageBlankTip.Checked)
                    FRecord.EmrView.PageBlankTip = tbxPageBlankTip.Text;
                else
                    FRecord.EmrView.PageBlankTip = "";

                bool vFirst = true;
                for (int i = 0; i < dgvRecord.RowCount; i++)
                {
                    if ((dgvRecord.Rows[i].Cells[0].Value != null) && (bool.Parse(dgvRecord.Rows[i].Cells[0].Value.ToString())))
                    {
                        using (MemoryStream vStream = new MemoryStream())
                        {
                            emrMSDB.DB.GetRecordContent(int.Parse(dgvRecord.Rows[i].Cells[4].Value.ToString()), vStream);
                            vStream.Position = 0;

                            if (vFirst)
                            {
                                FRecord.EmrView.LoadFromStream(vStream);
                                FRecord.EmrView.Sections[0].PageNoFrom = int.Parse(tbxPageNo.Text);
                                vFirst = false;
                            }
                            else
                            {
                                FRecord.EmrView.ActiveSection.ActiveData.SelectLastItemAfterWithCaret();
                                if ((dgvRecord.Rows[i].Cells[1].Value != null) && (bool.Parse(dgvRecord.Rows[i].Cells[1].Value.ToString())))
                                    FRecord.EmrView.InsertPageBreak();
                                else
                                    FRecord.EmrView.InsertBreak();

                                FRecord.EmrView.ApplyParaAlignHorz(HC.View.ParaAlignHorz.pahLeft);
                                FRecord.EmrView.InsertStream(vStream);
                            }
                        }
                    }
                }

                FRecord.HideTrace = !cbxShowTrace.Checked;
                if (!FRecord.EmrView.ReadOnly)
                    FRecord.EmrView.ReadOnly = true;
            }
            finally
            {
                FRecord.EmrView.EndUpdate();
            }
        }

        private void FrmRecordSet_Load(object sender, EventArgs e)
        {
            FRecord = new frmRecord();
            FRecord.PrintToolVisible = true;
            FRecord.EditToolVisible = false;
            FRecord.TopLevel = false;
            this.pnlRecord.Controls.Add(FRecord);
            FRecord.Dock = DockStyle.Fill;
            FRecord.Show();

            tbxPageNoFmt.Text = FRecord.EmrView.PageNoFormat;
        }

        private void CbxShowTrace_CheckedChanged(object sender, EventArgs e)
        {
            FRecord.HideTrace = !FRecord.EmrView.HideTrace;
        }

        private void CbxPageBlankTip_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxPageBlankTip.Checked)
                FRecord.EmrView.PageBlankTip = tbxPageBlankTip.Text;
            else
                FRecord.EmrView.PageBlankTip = "";
        }

        private void TbxPageBlankTip_TextChanged(object sender, EventArgs e)
        {
            if (cbxPageBlankTip.Checked)
                FRecord.EmrView.PageBlankTip = tbxPageBlankTip.Text;
        }
    }
}
