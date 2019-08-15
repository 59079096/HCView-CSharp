using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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

        public void ShowDialog(string aPatID, int aVisit)
        {
            GetPatientInchRecord(aPatID, aVisit);
            ShowDialog();
        }

        private void BtnShow_Click(object sender, EventArgs e)
        {
            FRecord.EmrView.Clear();
            FRecord.EmrView.PageNoFormat = tbxPageNoFmt.Text;
            FRecord.EmrView.HideTrace = !cbxShowTrace.Checked;
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
                            if ((dgvRecord.Rows[i].Cells[1].Value != null) &&(bool.Parse(dgvRecord.Rows[i].Cells[1].Value.ToString())))
                                FRecord.EmrView.InsertPageBreak();
                            else
                                FRecord.EmrView.InsertBreak();

                            FRecord.EmrView.ApplyParaAlignHorz(HC.View.ParaAlignHorz.pahLeft);
                            FRecord.EmrView.InsertStream(vStream);
                        }
                    }
                }
            }
        }

        private void FrmRecordSet_Load(object sender, EventArgs e)
        {
            FRecord = new frmRecord();
            FRecord.PrintToolVisible = true;
            FRecord.TopLevel = false;
            this.pnlRecord.Controls.Add(FRecord);
            FRecord.Dock = DockStyle.Fill;
            FRecord.Show();

            tbxPageNoFmt.Text = FRecord.EmrView.PageNoFormat;
        }
    }
}
