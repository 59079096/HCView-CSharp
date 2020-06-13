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
    public partial class frmDoctorStation : Form
    {
        private UserInfo FUserInfo;
        private PatientInfo FPatientInfo;

        public frmDoctorStation()
        {
            InitializeComponent();
        }

        private void 模板制作ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTemplate vfrmTemplate = new frmTemplate();
            vfrmTemplate.ShowDialog();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            FPatientInfo = new PatientInfo();

            FUserInfo = new UserInfo();
            FUserInfo.ID = "jt";
            FUserInfo.Name = "张医";
            EMR.ServerParam = new ServerParam();

            LoadPatientList();
        }

        private void LoadPatientList()
        {
            lvPatient.Items.Clear();
            DataTable dt = emrMSDB.DB.GetData("SELECT PI.Patient_ID AS PatID, PI.Visit_ID AS VisitID, PI.INP_NO AS InpNo, PI.Name, "
                + "SX.Name AS Sex, SX.Code AS SexCode, PI.AgeYear AS Age, BedNo, PI.Link_TEL AS LinkPhone, PI.Diagnosis, "
                + "PI.IN_Dept_DT as InDate,  Dept.ID AS DeptID, Dept.Name AS DeptName, "
                + "PI.Allergic_Drug AS AllergicDrug,   (CASE Nurs.Name WHEN '特级护理' THEN '特' "
                + "WHEN 'Ⅰ级护理' THEN 'Ⅰ' WHEN 'Ⅱ级护理' THEN 'Ⅱ'   "
                + "WHEN 'Ⅲ级护理' THEN 'Ⅲ' ELSE '' END) AS CareLevel,   "
                + "(CASE PC.Name WHEN '一般' THEN '' ELSE PC.Name END) AS IllState,     "
                + "HU.UserID AS OneDrID   FROM Inch_Patient PI   "
                + "LEFT JOIN Comm_Dept Dept ON PI.DeptID = Dept.ID   "
                + "LEFT JOIN Comm_Dic_Patcond PC ON PI.PAT_ConditionID = PC.id   "
                + "LEFT JOIN Comm_Dic_Sex SX ON PI.SexCode = SX.Code   "
                + "LEFT JOIN Comm_Dic_NursingLevel Nurs ON PI.NursingLevel_ID = Nurs.id  "
                + "LEFT JOIN Comm_User HU ON PI.ONE_DrID = HU.id    "
                + "WHERE PI.InflagID = 1 ORDER BY Dept.id");
            lvPatient.BeginUpdate();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ListViewItem lvi = lvPatient.Items.Add(dt.Rows[i]["PatID"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["VisitID"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["bedno"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["Name"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["sex"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["age"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["InDate"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["DeptName"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["DeptID"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["IllState"].ToString());   
                    lvi.SubItems.Add(dt.Rows[i]["Diagnosis"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["SexCode"].ToString());
                }
            }
            finally
            {
                lvPatient.EndUpdate();
            }
        }

        private void lvPatient_DoubleClick(object sender, EventArgs e)
        {
            FPatientInfo.PatID = lvPatient.SelectedItems[0].SubItems[0].Text;
            FPatientInfo.VisitID = byte.Parse(lvPatient.SelectedItems[0].SubItems[1].Text);
            FPatientInfo.BedNo = lvPatient.SelectedItems[0].SubItems[2].Text;
            FPatientInfo.Name = lvPatient.SelectedItems[0].SubItems[3].Text;
            FPatientInfo.Sex = lvPatient.SelectedItems[0].SubItems[4].Text;
            FPatientInfo.Age = lvPatient.SelectedItems[0].SubItems[5].Text;
            FPatientInfo.DeptName = lvPatient.SelectedItems[0].SubItems[7].Text;
            FPatientInfo.DeptID = int.Parse(lvPatient.SelectedItems[0].SubItems[8].Text);
            FPatientInfo.InDeptDateTime = DateTime.Parse(lvPatient.SelectedItems[0].SubItems[6].Text);
            FPatientInfo.SexCode = byte.Parse(lvPatient.SelectedItems[0].SubItems[11].Text);


            frmPatientRecord vfrmPatientRecord = new frmPatientRecord();
            vfrmPatientRecord.UserInfo = FUserInfo;
            vfrmPatientRecord.PatientInfo = FPatientInfo;
            vfrmPatientRecord.ShowDialog();
        }
    }
}
