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
    public partial class frmTemplateList : Form
    {
        private int FTemplateID, FDesID;
        private string FRecordName;

        public frmTemplateList()
        {
            InitializeComponent();
        }

        private TreeNode GetParentNode(int aPID)
        {
            for (int i = 0; i < tvTemplate.Nodes.Count; i++)
            {
                if (tvTemplate.Nodes[i].Tag != null)
                {
                    if ((tvTemplate.Nodes[i].Tag as DataSetInfo).ID == aPID)
                    {
                        return tvTemplate.Nodes[i];
                    }
                }
            }

            return null;
        }

        private void SetParentNodeState(TreeNode aNode)
        {
            if (aNode.Nodes.Count == 0)
                aNode.Nodes.Add("正在加载...");
            else
            {
                for (int i = 0; i < aNode.Nodes.Count; i++)
                    SetParentNodeState(aNode.Nodes[i]);
            }
        }

        private void GetTemplateGroup()
        {
            tvTemplate.Nodes.Clear();
            DataTable dt = emrMSDB.DB.GetData(emrMSDB.Sql_GetDataSet);
            tvTemplate.BeginUpdate();
            try
            {
                int vUseRang = -1, vInOrOut = -1;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (!int.TryParse(dt.Rows[i]["UseRang"].ToString(), out vUseRang))
                        return;

                    if (!int.TryParse(dt.Rows[i]["InOrOut"].ToString(), out vInOrOut))
                        return;

                    if (((vUseRang == DataSetInfo.USERANG_CLINIC)
                             || (vUseRang == DataSetInfo.USERANG_CLINICANDNURSE))

                         && ((vInOrOut == DataSetInfo.INOROUT_IN)
                             || (vInOrOut == DataSetInfo.INOROUT_INOUT))
                        )
                    {
                        TreeNode vNode = null;

                        int vPID = (int)dt.Rows[i]["pid"];
                        if (vPID != 0)
                        {
                            vNode = GetParentNode(vPID);
                            if (vNode == null)
                                continue;
                        }

                        DataSetInfo vDataSetInfo = new DataSetInfo();
                        vDataSetInfo.ID = (int)dt.Rows[i]["id"];
                        vDataSetInfo.PID = (int)dt.Rows[i]["pid"];
                        vDataSetInfo.GroupClass = (int)dt.Rows[i]["class"];
                        vDataSetInfo.GroupType = (int)dt.Rows[i]["type"];
                        vDataSetInfo.GroupName = dt.Rows[i]["name"].ToString();
                        vDataSetInfo.UseRang = vUseRang;
                        vDataSetInfo.InOrOut = vInOrOut;


                        if (vNode != null)
                            vNode = vNode.Nodes.Add(dt.Rows[i]["name"].ToString());
                        else
                            vNode = tvTemplate.Nodes.Add(dt.Rows[i]["name"].ToString());
                            
                        vNode.Tag = vDataSetInfo;
                    }
                }
            }
            finally
            {
                tvTemplate.EndUpdate();
            }
        }

        private void frmTemplateList_Load(object sender, EventArgs e)
        {
            GetTemplateGroup();
        }

        private void ClearTemplateList()
        {
            dgvTempList.RowCount = 0;
            tbxRecordName.Clear();
            FDesID = 0;
            FRecordName = "";
        }

        private void tvTemplate_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ClearTemplateList();

            TreeNode vNode = tvTemplate.SelectedNode;
            if (vNode == null)
                return;

            if (vNode.Nodes.Count > 0)
                return;

            FDesID = (vNode.Tag as DataSetInfo).ID;
            FRecordName = vNode.Text;
            tbxRecordName.Text = FRecordName;

            DataTable dt = emrMSDB.DB.GetData(string.Format(emrMSDB.Sql_GetTemplateList, FDesID));
            dgvTempList.RowCount = dt.Rows.Count;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dgvTempList.Rows[i].Cells[0].Value = dt.Rows[i]["tname"];
                dgvTempList.Rows[i].Cells[1].Value = dt.Rows[i]["Owner"];
                dgvTempList.Rows[i].Cells[2].Value = dt.Rows[i]["OwnerID"];
                dgvTempList.Rows[i].Cells[3].Value = dt.Rows[i]["id"];
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (dgvTempList.SelectedRows.Count > 0)
            {
                int vSelectRow = dgvTempList.SelectedRows[0].Index;
                FTemplateID = (int)(dgvTempList.Rows[vSelectRow].Cells[3].Value);
                FRecordName = tbxRecordName.Text;

                if (FRecordName.Trim() != "")
                {
                    Close();
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("请填写正确的病历名称！");
                    tbxRecordName.Focus();
                }
            }
            else
                MessageBox.Show("请选择模板！");
        }

        public int TemplateID
        {
            get { return FTemplateID; }
            set { FTemplateID = value; }
        }

        public int DesID
        {
            get { return FDesID; }
            set { FDesID = value; }
        }

        public string RecordName
        {
            get { return FRecordName; }
            set { FRecordName = value; }
        }

        public DateTime RecordDateTime
        {
            get { return dtpDT.Value; }
        }
    }
}
