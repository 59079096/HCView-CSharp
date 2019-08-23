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
    public partial class frmPatientHisRecord : Form
    {
        frmImportRecord FfrmImportRecord;
        public frmPatientHisRecord()
        {
            InitializeComponent();

            FfrmImportRecord = new frmImportRecord();
            FfrmImportRecord.TopLevel = false;
            FfrmImportRecord.FormBorderStyle = FormBorderStyle.None;
            this.pnlRecord.Controls.Add(FfrmImportRecord);
            FfrmImportRecord.Dock = DockStyle.Fill;
            //FfrmImportRecord.Show();
        }

        public PatientInfo PatientInfo;

        private void FrmPatientHisInchRecord_Shown(object sender, EventArgs e)
        {
            FfrmImportRecord.Show();

            DataTable dt = emrMSDB.DB.GetPatientHisInchInfo(PatientInfo.PatID, PatientInfo.VisitID);

            tvRecord.BeginUpdate();
            try
            {
                tvRecord.Nodes.Clear();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    TreeNode vPatNode = tvRecord.Nodes.Add("第" + dt.Rows[i]["VisitID"].ToString() + "次 " 
                        + dt.Rows[i]["BedNo"].ToString() + " " + dt.Rows[i]["DeptName"].ToString());

                    PatientInfo vPatInfo = new PatientInfo();
                    vPatInfo.PatID = dt.Rows[i]["PatID"].ToString();
                    vPatInfo.VisitID = byte.Parse(dt.Rows[i]["VisitID"].ToString());
                    vPatNode.Tag = vPatInfo;
                    vPatNode.Nodes.Add("正在加载...");
                }
            }
            finally
            {
                tvRecord.EndUpdate();
            }
        }

        public HCImportAsTextEventHandler OnImportAsText
        {
            get { return FfrmImportRecord.OnImportAsText; }
            set { FfrmImportRecord.OnImportAsText = value; }
        }

        private void FrmPatientHisInchRecord_FormClosed(object sender, FormClosedEventArgs e)
        {
            FfrmImportRecord.Close();
        }

        private void TvRecord_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes[0].Text == "正在加载...")
            {
                DataTable dt = emrMSDB.DB.GetData(string.Format(emrMSDB.Sql_GetInchRecordList, 
                    (e.Node.Tag as PatientInfo).PatID, (e.Node.Tag as PatientInfo).VisitID));

                tvRecord.BeginUpdate();
                try
                {
                    int vDesPID = 0;
                    TreeNode vNode = null;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (vDesPID.ToString() != dt.Rows[i]["desPID"].ToString())
                        {
                            vDesPID = int.Parse(dt.Rows[i]["desPID"].ToString());
                            RecordDataSetInfo vRecordDataSetInfo = new RecordDataSetInfo();
                            vRecordDataSetInfo.DesPID = vDesPID;

                            DataSetInfo vDataSetInfo = emrMSDB.DB.GetDataSetInfo(vDesPID);
                            vNode = e.Node.Nodes.Add(vDataSetInfo.GroupName);
                            vNode.Tag = vRecordDataSetInfo;
                        }

                        RecordInfo vRecordInfo = new RecordInfo();
                        vRecordInfo.ID = int.Parse(dt.Rows[i]["ID"].ToString());
                        vRecordInfo.DesID = int.Parse(dt.Rows[i]["desID"].ToString());
                        vRecordInfo.RecName = dt.Rows[i]["name"].ToString();
                        vRecordInfo.LastDT = DateTime.Parse(dt.Rows[i]["LastDT"].ToString());

                        TreeNode vRecNode = vNode.Nodes.Add(vRecordInfo.RecName + "(" + string.Format("{0:yyyy-MM-dd HH:mm}", vRecordInfo.LastDT) + ")");
                        vRecNode.Tag = vRecordInfo;
                    }

                    if ((e.Node.GetNodeCount(false) > 0) && (e.Node.Nodes[0].Text == "正在加载..."))
                        e.Node.Nodes.RemoveAt(0);
                }
                finally
                {
                    tvRecord.EndUpdate();
                }
            }
        }

        private void TvRecord_DoubleClick(object sender, EventArgs e)
        {
            if (!EMR.TreeNodeIsRecord(tvRecord.SelectedNode))
                return;

            FfrmImportRecord.EmrView.Clear();

            int vDesPID = -1, vDesID = -1, vRecordID = -1;
            EMR.GetNodeRecordInfo(tvRecord.SelectedNode, ref vDesPID, ref vDesID, ref vRecordID);

            if (vRecordID > 0)
            {
                using (MemoryStream vSM = new MemoryStream())
                {
                    emrMSDB.DB.GetRecordContent((tvRecord.SelectedNode.Tag as RecordInfo).ID, vSM);
                    if (vSM.Length > 0)
                    {

                        FfrmImportRecord.EmrView.LoadFromStream(vSM);
                        FfrmImportRecord.EmrView.Focus();
                    }
                }
            }
        }
    }
}
