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
        private frmDataElement frmDataElement;
        private frmDataElementDomain frmDataElementDomain;

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

        private void DoDESelectChange(object sender, EventArgs e)
        {
            frmDataElementDomain.DeName = frmDataElement.GetDeName();
            frmDataElementDomain.DomainID = frmDataElement.GetDomainID();
        }

        private void DoDEInsertAsDeItem(object sender, EventArgs e)
        {
            InsertAsProc vEvent = delegate (frmRecord frmRecord)
            {
                frmRecord.InsertDeItem(frmDataElement.GetDeIndex(), frmDataElement.GetDeName());
            };

            InsertDataElementAs(vEvent);
        }

        private void DoDEInsertAsDeGroup(object sender, EventArgs e)
        {
            InsertAsProc vEvent = delegate (frmRecord frmRecord)
            {
                frmRecord.InsertDeGroup(frmDataElement.GetDeIndex(), frmDataElement.GetDeName());
            };

            InsertDataElementAs(vEvent);
        }

        private void DoDEInsertAsDeEdit(object sender, EventArgs e)
        {
            InsertAsProc vEvent = delegate (frmRecord frmRecord)
            {
                frmRecord.InsertDeEdit(frmDataElement.GetDeIndex(), frmDataElement.GetDeName());
            };

            InsertDataElementAs(vEvent);
        }

        private void DoDEInsertAsDeCombobox(object sender, EventArgs e)
        {
            InsertAsProc vEvent = delegate (frmRecord frmRecord)
            {
                frmRecord.InsertDeCombobox(frmDataElement.GetDeIndex(), frmDataElement.GetDeName());
            };

            InsertDataElementAs(vEvent);
        }

        private void DoDEInsertAsDeDateTime(object sender, EventArgs e)
        {
            InsertAsProc vEvent = delegate (frmRecord frmRecord)
            {
                frmRecord.InsertDeDateTime(frmDataElement.GetDeIndex(), frmDataElement.GetDeName());
            };

            InsertDataElementAs(vEvent);
        }

        private void DoDEInsertAsDeRadioGroup(object sender, EventArgs e)
        {
            InsertAsProc vEvent = delegate (frmRecord frmRecord)
            {
                frmRecord.InsertDeRadioGroup(frmDataElement.GetDeIndex(), frmDataElement.GetDeName());
            };

            InsertDataElementAs(vEvent);
        }

        private void DoDEInsertAsDeCheckBox(object sender, EventArgs e)
        {
            InsertAsProc vEvent = delegate (frmRecord frmRecord)
            {
                frmRecord.InsertDeCheckBox(frmDataElement.GetDeIndex(), frmDataElement.GetDeName());
            };

            InsertDataElementAs(vEvent);
        }

        private void DoDEInsertAsDeImage(object sender, EventArgs e)
        {
            InsertAsProc vEvent = delegate (frmRecord frmRecord)
            {
                frmRecord.InsertDeCheckBox(frmDataElement.GetDeIndex(), frmDataElement.GetDeName());
            };

            InsertDataElementAs(vEvent);
        }

        private void DoDEInsertAsFloatBarCode(object sender, EventArgs e)
        {
            InsertAsProc vEvent = delegate (frmRecord frmRecord)
            {
                frmRecord.InsertDeFloatBarCode(frmDataElement.GetDeIndex(), frmDataElement.GetDeName());
            };

            InsertDataElementAs(vEvent);
        }

        private void frm_Template_Load(object sender, EventArgs e)
        {
            ShowTemplateDeSet();  // 获取并显示模板数据集信息

            // 选项窗体
            frmDataElementDomain = new frmDataElementDomain();
            frmDataElementDomain.FormBorderStyle = FormBorderStyle.None;
            frmDataElementDomain.Dock = DockStyle.Fill;
            frmDataElementDomain.TopLevel = false;
            this.splitContainerDataElement.Panel2.Controls.Add(frmDataElementDomain);
            frmDataElementDomain.Show();

            // 数据元窗体
            frmDataElement = new frmDataElement();
            frmDataElement.FormBorderStyle = FormBorderStyle.None;
            frmDataElement.Dock = DockStyle.Fill;
            frmDataElement.TopLevel = false;
            this.splitContainerDataElement.Panel1.Controls.Add(frmDataElement);
            frmDataElement.OnSelectRowChange = DoDESelectChange;
            frmDataElement.OnInsertAsDeItem = DoDEInsertAsDeItem;
            frmDataElement.OnInsertAsDeGroup = DoDEInsertAsDeGroup;
            frmDataElement.OnInsertAsDeEdit = DoDEInsertAsDeEdit;
            frmDataElement.OnInsertAsDeCombobox = DoDEInsertAsDeCombobox;
            frmDataElement.OnInsertAsDeDateTime = DoDEInsertAsDeDateTime;
            frmDataElement.OnInsertAsDeRadioGroup = DoDEInsertAsDeRadioGroup;
            frmDataElement.OnInsertAsDeCheckBox = DoDEInsertAsDeCheckBox;
            frmDataElement.OnInsertAsDeImage = DoDEInsertAsDeImage;
            frmDataElement.OnInsertAsDeFloatBarCode = DoDEInsertAsFloatBarCode;
            frmDataElement.Show();
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
                    //using (FileStream vfs = new FileStream(@"c:\问题模板.hcf", FileMode.Create))
                    //{
                    //    byte[] vBytes = vSM.GetBuffer();
                    //    vfs.Write(vBytes, 0, vBytes.Length);
                    //    vfs.Close();
                    //}

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

        private delegate void InsertAsProc(frmRecord frmrecord);

        private void InsertDataElementAs(InsertAsProc proc)
        {
            frmRecord vFrmRecord = GetActiveRecord();
            if (vFrmRecord != null)
            {
                if (!vFrmRecord.EmrView.Focused)  // 先给焦点，便于处理光标处域
                    vFrmRecord.EmrView.Focus();

                proc(vFrmRecord);
            }
            else
                MessageBox.Show("未发现打开的模板！");
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
    }
}
