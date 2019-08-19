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
using System.IO;
using HC.View;
using System.Data.SqlClient;

namespace EMRView
{
    public partial class frmTemplate : Form
    {
        //private frmRecord frmRecord;
        //private string FOpenedTID;
        private int FDomainID;

        public frmTemplate()
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

        private void ShowTemplateDeSet()
        {
            tvTemplate.Nodes.Clear();
            DataTable dt = emrMSDB.DB.GetData(emrMSDB.Sql_GetTemplate);
            tvTemplate.BeginUpdate();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataSetInfo vDataSetInfo = new DataSetInfo();
                    vDataSetInfo.ID = (int)dt.Rows[i]["id"];
                    vDataSetInfo.PID = (int)dt.Rows[i]["pid"];
                    vDataSetInfo.GroupClass = (int)dt.Rows[i]["class"];
                    vDataSetInfo.GroupType = (int)dt.Rows[i]["type"];
                    vDataSetInfo.GroupName = dt.Rows[i]["name"].ToString();

                    TreeNode vNode = null;
                    if (vDataSetInfo.PID != 0)
                        vNode = GetParentNode(vDataSetInfo.PID).Nodes.Add(dt.Rows[i]["name"].ToString());
                    else
                        vNode = tvTemplate.Nodes.Add(dt.Rows[i]["name"].ToString());
                        
                    vNode.Tag = vDataSetInfo;
                }

                for (int i = 0; i < tvTemplate.Nodes.Count; i++)
                    SetParentNodeState(tvTemplate.Nodes[i]);
            }
            finally
            {
                tvTemplate.EndUpdate();
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

        private void ShowAllDataElement()
        {
            tbxPY.Clear();
            dgvDE.RowCount = 1;
            dgvCV.RowCount = 1;
            emrMSDB.DB.GetDataElement();
            ShowDataElement();
        }

        private void frm_Template_Load(object sender, EventArgs e)
        {
            ShowTemplateDeSet();  // 获取并显示模板数据集信息
            ShowAllDataElement();  // 显示数据元信息
            mniViewItem_Click(sender, e);
        }

        private bool TreeNodeIsTemplate(TreeNode aNode)
        {
            return (aNode != null) && (aNode.Tag is TemplateInfo);
        }

        private int GetRecordEditPageIndex(int aTempID)
        {
            for (int i = 0; i < tabTemplate.TabPages.Count; i++)
            {
                if ((int)(tabTemplate.TabPages[i].Tag) == aTempID)
                    return i;
            }

            return -1;
        }

        private void DoSaveTempContent(object sender, EventArgs e)
        {
            using (MemoryStream vSM = new MemoryStream())
            {
                (sender as frmRecord).EmrView.SaveToStream(vSM);  // 得到文档数据流

                int vTempID = ((sender as frmRecord).ObjectData as TemplateInfo).ID;

                EMRView.emrMSDB.ExecCommandEventHanler vEvent = delegate(SqlCommand sqlComm)
                {
                    sqlComm.Parameters.AddWithValue("tid", vTempID);
                    sqlComm.Parameters.AddWithValue("content", vSM.GetBuffer());
                };

                if (emrMSDB.DB.ExecSql(emrMSDB.Sql_SaveTemplateConent, vEvent))
                {
                    (sender as frmRecord).EmrView.IsChanged = false;  // 保存后文档标识为非修改
                    MessageBox.Show("保存成功！");
                }
                else
                    MessageBox.Show(emrMSDB.DB.ErrMsg);
            }
        }

        private void DoRecordChangedSwitch(object sender, EventArgs e)
        {
            if (sender is frmRecord)
            {
                if ((sender as frmRecord).Parent is TabPage)
                {
                    string vText = ((sender as frmRecord).ObjectData as TemplateInfo).Name;
                    if ((sender as frmRecord).EmrView.IsChanged)
                        vText = vText + "*";

                    ((sender as frmRecord).Parent as TabPage).Text = vText;
                }
            }
        }
        
        private void tvTemplate_DoubleClick(object sender, EventArgs e)
        {
            if (TreeNodeIsTemplate(tvTemplate.SelectedNode))
            {
                int vTempID = (tvTemplate.SelectedNode.Tag as TemplateInfo).ID;
                int vPageIndex = GetRecordEditPageIndex(vTempID);
                if (vPageIndex >= 0)
                {
                    tabTemplate.TabIndex = vPageIndex;
                    return;
                }

                frmRecord vFrmRecord = null;

                using (MemoryStream vSM = new MemoryStream())
                {
                    emrMSDB.DB.GetTemplateContent(vTempID, vSM);

                    vFrmRecord = new frmRecord();
                    vFrmRecord.EmrView.DesignModeEx = true;
                    vFrmRecord.ObjectData = tvTemplate.SelectedNode.Tag;
                    if (vSM.Length > 0)
                        vFrmRecord.EmrView.LoadFromStream(vSM);
                }

                if (vFrmRecord != null)
                {
                    TabPage vPage = new TabPage(tvTemplate.SelectedNode.Text);
                    vPage.Tag = vTempID;

                    vFrmRecord.TopLevel = false;
                    vFrmRecord.OnSave = DoSaveTempContent;
                    vFrmRecord.OnChangedSwitch = DoRecordChangedSwitch;
                    vPage.Controls.Add(vFrmRecord);
                    vFrmRecord.Dock = DockStyle.Fill;
                    vFrmRecord.Show();
                    tabTemplate.TabPages.Add(vPage);
                    tabTemplate.SelectedTab = vPage;
                    vFrmRecord.EmrView.Focus();
                }
            }
        }

        private void tvTemplate_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {

            if (e.Node.Nodes[0].Text == "正在加载...")
            {
                DataTable dt = emrMSDB.DB.GetData(string.Format(emrMSDB.Sql_GetTemplateList,
                    (e.Node.Tag as DataSetInfo).ID));

                tvTemplate.BeginUpdate();
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        TemplateInfo vTempInfo = new TemplateInfo();
                        vTempInfo.ID = (int)dt.Rows[i]["id"];
                        vTempInfo.DesID = (int)dt.Rows[i]["desid"];
                        vTempInfo.Owner = (int)dt.Rows[i]["Owner"];
                        vTempInfo.OwnerID = (int)dt.Rows[i]["OwnerID"];
                        vTempInfo.Name = dt.Rows[i]["tname"].ToString();

                        TreeNode vNode = e.Node.Nodes.Add(dt.Rows[i]["tname"].ToString());

                        vNode.Tag = vTempInfo;
                    }

                    if ((e.Node.GetNodeCount(false) > 0) && (e.Node.Nodes[0].Text == "正在加载..."))
                        e.Node.Nodes.RemoveAt(0);
                }
                finally
                {
                    tvTemplate.EndUpdate();
                }
            }
        }

        private frmRecord GetActiveRecord()
        {
            if (tabTemplate.TabPages.Count > 0)
            {
                TabPage vPage = tabTemplate.SelectedTab;
                for (int i = 0; i < vPage.Controls.Count; i++)
                {
                    if (vPage.Controls[i] is frmRecord)
                        return vPage.Controls[i] as frmRecord;
                }
            }

            return null;
        }

        private void mniInsertAsDE_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count == 0)
                return;

            frmRecord vFrmRecord = GetActiveRecord();
            if (vFrmRecord != null)
            {
                if (!vFrmRecord.EmrView.Focused)  // 先给焦点，便于处理光标处域
                    vFrmRecord.EmrView.Focus();

                DataGridViewRow vSelectedRow = dgvDE.SelectedRows[0];
                vFrmRecord.InsertDeItem(vSelectedRow.Cells[0].Value.ToString(), 
                    vSelectedRow.Cells[1].Value.ToString());
            }
            else
                MessageBox.Show("未发现打开的模板！");
        }

        private void mniInsertAsDG_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count == 0)
                return;

            frmRecord vFrmRecord = GetActiveRecord();
            if (vFrmRecord != null)
            {
                using (DeGroup vDeGroup = new DeGroup(vFrmRecord.EmrView.ActiveSectionTopLevelData()))
                {
                    vDeGroup[DeProp.Index] = dgvDE.SelectedRows[0].Cells[0].Value.ToString();
                    vDeGroup[DeProp.Name] = dgvDE.SelectedRows[0].Cells[0].Value.ToString();

                    if (!vFrmRecord.EmrView.Focused)  // 先给焦点，便于处理光标处域
                        vFrmRecord.EmrView.Focus();

                    vFrmRecord.EmrView.InsertDeGroup(vDeGroup);
                }
            }
            else
                MessageBox.Show("未发现打开的模板！");
        }

        private void mniInsertAsCombobox_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count == 0)
                return;

            frmRecord vFrmRecord = GetActiveRecord();
            if (vFrmRecord != null)
            {
                DeCombobox vDeCombobox = new DeCombobox(vFrmRecord.EmrView.ActiveSectionTopLevelData(),
                    dgvDE.SelectedRows[0].Cells[1].Value.ToString());

                vDeCombobox.SaveItem = false;
                vDeCombobox[DeProp.Index] = dgvDE.SelectedRows[0].Cells[0].Value.ToString();
                
                if (!vFrmRecord.EmrView.Focused)  // 先给焦点，便于处理光标处域
                    vFrmRecord.EmrView.Focus();

                vFrmRecord.EmrView.InsertItem(vDeCombobox);
            }
            else
                MessageBox.Show("未发现打开的模板！");
        }

        private void GetDomainItem(int aDomainID)
        {
            dgvCV.RowCount = 1;
            if (aDomainID > 0)
            {
                DataTable dt = emrMSDB.DB.GetData(string.Format(emrMSDB.Sql_GetDomainItem, (aDomainID)));

                dgvCV.RowCount = dt.Rows.Count;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dgvCV.Rows[i].Cells[0].Value = dt.Rows[i]["devalue"];
                    dgvCV.Rows[i].Cells[1].Value = dt.Rows[i]["code"];
                    dgvCV.Rows[i].Cells[2].Value = dt.Rows[i]["py"];
                    dgvCV.Rows[i].Cells[3].Value = dt.Rows[i]["id"];

                    dgvCV.Rows[i].Cells[4].Value = "";

                    if (dt.Rows[i]["content"].GetType() != typeof(System.DBNull))
                    {
                        byte[] vbuffer = (byte[])dt.Rows[i]["content"];
                        if (vbuffer.Length > 0)
                            dgvCV.Rows[i].Cells[4].Value = "...";
                    }
                }
            }
            else
                dgvCV.RowCount = 0;
        }

        private void mniViewItem_Click(object sender, EventArgs e)
        {
            if ((dgvDE.SelectedRows.Count > 0) && (dgvDE.SelectedRows[0].Cells[5].Value != null))
            {
                if (dgvDE.SelectedRows[0].Cells[5].Value.ToString() != "")
                    FDomainID = (int)(dgvDE.SelectedRows[0].Cells[5].Value);
                else
                    FDomainID = 0;

                GetDomainItem(FDomainID);
                lblDE.Text = dgvDE.SelectedRows[0].Cells[1].Value.ToString() + "(共 "
                    + dgvCV.RowCount.ToString() + " 条选项)";
            }
            else
                dgvCV.RowCount = 0;
        }

        private void dgvDE_DoubleClick(object sender, EventArgs e)
        {
            mniInsertAsDE_Click(sender, e);
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

                mniViewItem_Click(sender, e);
            }
        }

        private void mniRefresh_Click(object sender, EventArgs e)
        {
            ShowAllDataElement();  // 刷新数据元信息
        }

        private void label1_Click(object sender, EventArgs e)
        {
            mniRefresh_Click(sender, e);
        }

        private void mniEditItemLink_Click(object sender, EventArgs e)
        {
            if (dgvCV.SelectedRows.Count == 0)
                return;

            int vRow = dgvCV.SelectedRows[0].Index;
            if (dgvCV.Rows[vRow].Cells[3].Value.ToString() == "")
                return;

            frmItemContent vFrmItemContent = new frmItemContent();
            vFrmItemContent.DomainItemID = int.Parse(dgvCV.Rows[vRow].Cells[3].Value.ToString());
            vFrmItemContent.ShowDialog();
        }

        private void lblDE_Click(object sender, EventArgs e)
        {
            frmItemContent vFrmItemContent = new frmItemContent();
            vFrmItemContent.DomainItemID = 599;
            vFrmItemContent.ShowDialog();
        }

        private void mniNewTemplate_Click(object sender, EventArgs e)
        {
            frmTemplateInfo vFrmTemplateInfo = new frmTemplateInfo();
            vFrmTemplateInfo.ShowDialog();
            if (vFrmTemplateInfo.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                if (vFrmTemplateInfo.TemplateName.Trim() == "")
                    return;

                int vTemplateID = -1;

                EMRView.emrMSDB.ExecCommandEventHanler vEvent = delegate(SqlCommand sqlComm)
                {
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandText = "CreateTemplate";

                    if (TreeNodeIsTemplate(tvTemplate.SelectedNode))
                        sqlComm.Parameters.AddWithValue("desID", (tvTemplate.SelectedNode.Tag as TemplateInfo).DesID);
                    else
                        sqlComm.Parameters.AddWithValue("desID", (tvTemplate.SelectedNode.Tag as DataSetInfo).ID);

                    sqlComm.Parameters.AddWithValue("tname", vFrmTemplateInfo.TemplateName);
                    sqlComm.Parameters.AddWithValue("owner", 1);
                    sqlComm.Parameters.AddWithValue("ownerid", 0);

                    //SqlParameter parOutput = sqlComm.Parameters.Add("@RecordID", SqlDbType.Int);
                    //parOutput.Direction = ParameterDirection.Output;
                    SqlParameter parRetrun = new SqlParameter("@TempID", SqlDbType.Int);
                    parRetrun.Direction = ParameterDirection.ReturnValue;
                    sqlComm.Parameters.Add(parRetrun);

                    sqlComm.ExecuteNonQuery();
                    vTemplateID = int.Parse(parRetrun.Value.ToString());
                };

                if (emrMSDB.DB.ExecStoredProcedure(vEvent))
                {
                    TemplateInfo vTempInfo = new TemplateInfo();
                    vTempInfo.ID = vTemplateID;
                    vTempInfo.Owner = 1;
                    vTempInfo.OwnerID = 0;
                    vTempInfo.Name = vFrmTemplateInfo.TemplateName;

                    tvTemplate.SelectedNode = tvTemplate.SelectedNode.Nodes.Add(vTempInfo.Name);
                    tvTemplate.SelectedNode.Tag = vTempInfo;
                }
                else
                    MessageBox.Show("新建模板失败，请重试！\n" + emrMSDB.DB.ErrMsg);
            }
        }

        private void pmTemplate_Opening(object sender, CancelEventArgs e)
        {
            mniNewTemplate.Enabled = !TreeNodeIsTemplate(tvTemplate.SelectedNode);
            mniDeleteTemplate.Enabled = !mniNewTemplate.Enabled;
            mniInsertTemplate.Enabled = !mniNewTemplate.Enabled;
        }

        private void CloseTemplatePage(int aPageIndex, bool aSaveChange = true)
        {
            if (aPageIndex >= 0)
            {
                TabPage vPage = tabTemplate.TabPages[aPageIndex];

                if (aSaveChange)
                {
                    for (int i = 0; i < vPage.Controls.Count; i++)
                    {
                        if (vPage.Controls[i] is frmRecord)
                        {
                            frmRecord vFrmRecord = vPage.Controls[i] as frmRecord;
                            if (vFrmRecord.EmrView.IsChanged)
                            {
                                MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                                DialogResult dr = MessageBox.Show("是否保存模板 " + (vFrmRecord.ObjectData as TemplateInfo).Name + "？", "确认操作", messButton);
                                if (dr == DialogResult.OK)
                                    DoSaveTempContent(vFrmRecord, null);
                            }

                            break;
                        }
                    }
                }

                tabTemplate.TabPages.Remove(vPage);
            }
        }

        private void mniDeleteTemplate_Click(object sender, EventArgs e)
        {
            MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
            DialogResult dr = MessageBox.Show("确定要删除模板 " + (tvTemplate.SelectedNode.Tag as TemplateInfo).Name + "？", "确认操作", messButton);
            if (dr == DialogResult.OK)
            {
                int vTempID = (tvTemplate.SelectedNode.Tag as TemplateInfo).ID;

                if (emrMSDB.DB.ExecSql(string.Format("EXEC DeleteTemplate {0}", vTempID)))
                {
                    tvTemplate.Nodes.Remove(tvTemplate.SelectedNode);
                    vTempID = GetRecordEditPageIndex(vTempID);
                    if (vTempID >= 0)
                        CloseTemplatePage(vTempID, false);
                }
                else
                    MessageBox.Show("删除失败:" + emrMSDB.DB.ErrMsg);
            }
        }

        private void mniInsertTemplate_Click(object sender, EventArgs e)
        {
            if (TreeNodeIsTemplate(tvTemplate.SelectedNode))
            {
                frmRecord vFrmRecord = GetActiveRecord();
                if (vFrmRecord != null)
                {
                    TreeNode vNode = tvTemplate.SelectedNode;
                    using (MemoryStream vSM = new MemoryStream())
                    {
                        emrMSDB.DB.GetTemplateContent((vNode.Tag as TemplateInfo).ID, vSM);

                        while (vNode.Parent != null)
                            vNode = vNode.Parent;

                        int vGroupClass = (vNode.Tag as DataSetInfo).GroupClass;

                        if (vGroupClass == DataSetInfo.CLASS_PAGE)
                        {
                            vSM.Position = 0;
                            vFrmRecord.EmrView.InsertStream(vSM);
                        }
                        else
                        if ((vGroupClass == DataSetInfo.CLASS_HEADER) || (vGroupClass == DataSetInfo.CLASS_FOOTER))
                        {
                            HCEmrView vEmrView = new HCEmrView();
                            vEmrView.LoadFromStream(vSM);
                            vSM.SetLength(0);
                            vEmrView.Sections[0].Header.SaveToStream(vSM);
                            vSM.Position = 0;

                            if (vGroupClass == DataSetInfo.CLASS_HEADER)
                                vFrmRecord.EmrView.ActiveSection.Header.LoadFromStream(vSM, vEmrView.Style, HC.View.HC.HC_FileVersionInt);
                            else
                                vFrmRecord.EmrView.ActiveSection.Footer.LoadFromStream(vSM, vEmrView.Style, HC.View.HC.HC_FileVersionInt);

                            vFrmRecord.EmrView.IsChanged = true;
                            vFrmRecord.EmrView.UpdateView();
                        }
                    }
                }
            }
        }

        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseTemplatePage(tabTemplate.SelectedIndex);
        }

        private bool DeleteDomainItemContent(int aDominItemID)
        {
            return emrMSDB.DB.ExecSql(string.Format("DELETE FROM Comm_DomainContent WHERE DItemID = {0}", aDominItemID));
        }

        private void mniDeleteItemLink_Click(object sender, EventArgs e)
        {
            if (dgvCV.SelectedRows.Count > 0)
            {
                int vRow = dgvCV.SelectedRows[0].Index;

                MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                DialogResult dr = MessageBox.Show("确定要删除选项【 " + dgvCV.Rows[vRow].Cells[0].Value.ToString() + "】的扩展内容吗？", "确认操作", messButton);
                if (dr == DialogResult.OK)
                {
                    int vDomainItemID = int.Parse(dgvCV.Rows[vRow].Cells[3].Value.ToString());

                    if (DeleteDomainItemContent(vDomainItemID))
                        MessageBox.Show("删除值域选项扩展内容成功！");
                    else
                        MessageBox.Show("删除失败:" + emrMSDB.DB.ErrMsg);
                }
            }
        }

        private void mniNewItem_Click(object sender, EventArgs e)
        {
            frmDomainItem vFrmDomainItem = new frmDomainItem();
            vFrmDomainItem.SetInfo(FDomainID, 0);
            if (vFrmDomainItem.DialogResult == System.Windows.Forms.DialogResult.OK)
                GetDomainItem(FDomainID);
        }

        private void mniEditItem_Click(object sender, EventArgs e)
        {
            if ((dgvCV.SelectedRows.Count > 0) && (FDomainID > 0))
            {
                frmDomainItem vFrmDomainItem = new frmDomainItem();
                vFrmDomainItem.SetInfo(FDomainID, int.Parse(dgvCV.Rows[dgvCV.SelectedRows[0].Index].Cells[3].Value.ToString()));
                if (vFrmDomainItem.DialogResult == System.Windows.Forms.DialogResult.OK)
                    GetDomainItem(FDomainID);
            }
        }

        private bool DeleteDomainItem(int aDItemID)
        {
            return emrMSDB.DB.ExecSql(string.Format("DELETE FROM Comm_DataElementDomain WHERE ID = {0}", aDItemID));
        }

        private void mniDeleteItem_Click(object sender, EventArgs e)
        {
            if (dgvCV.SelectedRows.Count > 0)
            {
                int vRow = dgvCV.SelectedRows[0].Index;

                MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                DialogResult dr = MessageBox.Show("确定要删除选项【 " + dgvCV.Rows[vRow].Cells[0].Value.ToString() + "】和该选项对应的扩展内容吗？", "确认操作", messButton);
                if (dr == DialogResult.OK)
                {
                    // 删除扩展内容
                    if (DeleteDomainItemContent(int.Parse(dgvCV.Rows[vRow].Cells[3].Value.ToString())))
                    {
                        MessageBox.Show("删除选项扩展内容成功！");

                        // 删除选项
                        if (DeleteDomainItem(int.Parse(dgvCV.Rows[vRow].Cells[3].Value.ToString())))
                            MessageBox.Show("删除选项成功！");
                        else
                            MessageBox.Show("删除选项失败！" + emrMSDB.DB.ErrMsg);
                    }
                    else
                        MessageBox.Show("删除选项扩展内容失败！" + emrMSDB.DB.ErrMsg);
                }
            }
        }

        private void mniNew_Click(object sender, EventArgs e)
        {
            frmDeInfo vFrmDeInfo = new frmDeInfo();
            vFrmDeInfo.SetDeID(0);
            if (vFrmDeInfo.DialogResult == System.Windows.Forms.DialogResult.OK)
                mniRefresh_Click(sender, e);
        }

        private void mniEdit_Click(object sender, EventArgs e)
        {
            if (dgvDE.SelectedRows.Count > 0)
            {
                frmDeInfo vFrmDeInfo = new frmDeInfo();
                vFrmDeInfo.SetDeID(int.Parse(dgvDE.Rows[dgvDE.SelectedRows[0].Index].Cells[0].Value.ToString()));
                if (vFrmDeInfo.DialogResult == System.Windows.Forms.DialogResult.OK)
                    mniRefresh_Click(sender, e);
            }
        }

        private void mniDelete_Click(object sender, EventArgs e)
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
                        mniRefresh_Click(sender, e);
                    }
                    else
                        MessageBox.Show("删除失败！" + emrMSDB.DB.ErrMsg);
                }
            }
        }

        private void mniDomain_Click(object sender, EventArgs e)
        {
            frmDomain vFrmDomain = new frmDomain();
            vFrmDomain.ShowDialog();
        }

        private void DgvDE_SelectionChanged(object sender, EventArgs e)
        {
            mniViewItem_Click(sender, e);
        }
    }
}
