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
    public partial class frmMain : Form
    {
        private frmTemplate FfrmTemplate;
        private emrDB emrDB;

        public frmMain()
        {
            InitializeComponent();
        }

        private void 模板制作ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FfrmTemplate == null)
                FfrmTemplate = new frmTemplate();

            FfrmTemplate.ShowDialog();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            if (emrDB == null)
                emrDB = new EMRView.emrDB();

            LoadPatientList();
        }

        private void LoadPatientList()
        {
            lvPatient.Items.Clear();
            DataTable dt = emrDB.ExecToDataTable(emrDB.Sql_GetAllPatient);
            lvPatient.BeginUpdate();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ListViewItem lvi = lvPatient.Items.Add(dt.Rows[i]["id"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["vid"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["bedno"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["pname"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["sex"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["age"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["indt"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["indept"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["curdept"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["indeptdt"].ToString());
                }
            }
            finally
            {
                lvPatient.EndUpdate();
            }
        }

        private void lvPatient_DoubleClick(object sender, EventArgs e)
        {
            string vPID = lvPatient.SelectedItems[0].SubItems[0].Text;
            string vVID = lvPatient.SelectedItems[0].SubItems[1].Text;
            frmPatientRecord vfrmPatientRecord = new frmPatientRecord();
            vfrmPatientRecord.SetPatientID(vPID, vVID);
            vfrmPatientRecord.ShowDialog();
        }
    }
}
