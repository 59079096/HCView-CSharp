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
    public partial class frmPatientRecord : Form
    {
        private EmrView FEmrView;
        private emrDB emrDB;
        private string FPID;
        private string FVID;

        public frmPatientRecord()
        {
            InitializeComponent();
        }

        private void frmPatientRecord_Load(object sender, EventArgs e)
        {
            if (FEmrView == null)
            {
                FEmrView = new EmrView();
                this.Controls.Add(FEmrView);
                FEmrView.Parent = this.splitContainer1.Panel2;
                FEmrView.Dock = DockStyle.Fill;
                FEmrView.BringToFront();
            }

            if (emrDB == null)
                emrDB = new emrDB();

            LoadRecordList();
        }

        private void LoadRecordList()
        {
            tvRecord.Nodes.Clear();
            DataTable dt = emrDB.ExecToDataTable(string.Format(emrDB.Sql_GetPatInRecord, FPID, FVID));
            tvRecord.BeginUpdate();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    TreeNode vNode = tvRecord.Nodes.Add(dt.Rows[i]["rname"].ToString());
                    vNode.Tag = dt.Rows[i]["id"].ToString();
                }
            }
            finally
            {
                tvRecord.EndUpdate();
            }
        }

        public void SetPatientID(string aPID, string aVID)
        {
            FPID = aPID;
            FVID = aVID;
        }

        private void 新建ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
