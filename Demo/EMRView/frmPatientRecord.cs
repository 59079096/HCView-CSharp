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
using System.Xml;
using System.Data.SqlClient;
using HC.Win32;

namespace EMRView
{
    public partial class frmPatientRecord : Form
    {
        private ServerInfo FServerInfo = new ServerInfo();
        private int FSyncDataDesID = -1;
        private DataTable FDataElementSetMacro;
        private List<StructDoc> FStructDocs = new List<StructDoc>();
        private emrCompiler FCompiler = new emrCompiler();

        private void frmPatientRecord_Load(object sender, EventArgs e)
        {
            this.Text = PatientInfo.BedNo + "床，" + PatientInfo.Name;
            lblPatientInfo.Text = PatientInfo.BedNo + "床，" + PatientInfo.Name + "，"
                + PatientInfo.Sex + "，" + PatientInfo.Age + "岁，"
                + string.Format("{0:yyyy-MM-dd HH:mm}", PatientInfo.InDeptDateTime) + "入科，"
                + PatientInfo.CareLevel.ToString() + "级护理";

            GetPatientRecordListUI();
        }

        private void 新建ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecordInfo vRecordInfo = null;
            int vTemplateID = -1;

            frmTemplateList vFrmTempList = new frmTemplateList();
            vFrmTempList.ShowDialog();
            if (vFrmTempList.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                vTemplateID = vFrmTempList.TemplateID;

                vRecordInfo = new RecordInfo();
                vRecordInfo.DesID = vFrmTempList.DesID;
                vRecordInfo.RecName = vFrmTempList.RecordName;
                vRecordInfo.DT = vFrmTempList.RecordDateTime;
            }
            else
                return;

            using (MemoryStream vSM = new MemoryStream())
            {
                emrMSDB.DB.GetTemplateContent(vTemplateID, vSM);  // 取模板内容(流)

                TabPage vPage = null;
                frmRecord vFrmRecord = null;

                NewPageAndRecord(vRecordInfo, ref vPage, ref vFrmRecord);
                emrMSDB.DB.GetDataSetElement(vRecordInfo.DesID);

                if (vSM.Length > 0)  // 模板内容不为空
                {
                    // 获取当前数据集有哪些数据可以被替换的数据
                    // 放到本地DataTable：FDataElementSetMacro中
                    PrepareSyncData(vRecordInfo.DesID);

                    // 赋值模板加载时替换数据元内容的方法
                    vFrmRecord.EmrView.OnSyncDeItem = DoSyncDeItem;
                    try
                    {
                        vFrmRecord.EmrView.BeginUpdate();
                        try
                        {
                            // 加载模板，加载过程会调用DoSyncDeItem
                            // 给每一个数据元到FDataElementSetMacro中找
                            // 自己要替换为什么内容的机会
                            vFrmRecord.EmrView.LoadFromStream(vSM);

                            // 替换数据组的内容
                            SyncDeGroupByStruct(vFrmRecord.EmrView);
                            vFrmRecord.EmrView.FormatData();
                            vFrmRecord.EmrView.IsChanged = true;
                        }
                        finally
                        {
                            vFrmRecord.EmrView.EndUpdate();
                        }
                    }
                    finally
                    {
                        vFrmRecord.EmrView.OnSyncDeItem = null;
                    }
                }

                tabRecord.SelectedTab = vPage;
            }
        }

        private void DoSetDeItemText(object sender, DeItem deItem, ref string text, ref bool cancel)
        {
            // 获取和此数据元相关的脚本
            string script = emrMSDB.DB.GetDeScript(int.Parse(deItem[DeProp.Index]));

            if (script == "")
                return;
            
            if (!FCompiler.RunScript(script, deItem, PatientInfo, ((sender as frmRecord).ObjectData as RecordInfo), ref text, ref cancel))
                MessageBox.Show("当前数据元有控制脚本，但运行错误，原因：\r\n" + FCompiler.ErrorMessage);
        }

        private bool DoDeItemPopup(DeItem aDeItem)
        {
            return true;
        }

        private void DoPrintPreview(object sender, EventArgs e)
        {
            frmRecordSet vFrmRecordSet = new frmRecordSet();
            vFrmRecordSet.ShowDialog(PatientInfo.PatID, PatientInfo.VisitID, (this.GetActiveRecord().ObjectData as RecordInfo).ID);
        }

        private void DoTraverseItem(HCCustomData aData, int aItemNo, int aTags, Stack<HCDomainInfo> aDomainStack, ref bool aStop)
        {
            if (!(aData.Items[aItemNo] is DeItem))
                return;

            DeItem vDeItem = aData.Items[aItemNo] as DeItem;

            if (TTravTag.Contains(aTags, TTravTag.WriteTraceInfo))
            {
                switch (vDeItem.StyleEx)
                {
                    case StyleExtra.cseNone:
                        vDeItem[DeProp.Trace] = "";
                         break;

                    case StyleExtra.cseDel:
                        if (vDeItem[DeProp.Trace] == "")
                            vDeItem[DeProp.Trace] = UserInfo.Name + "(" + UserInfo.ID + ") 删除 "
                            + string.Format("{0:yyyy-MM-dd}", FServerInfo.DateTime);
                        break;

                    case StyleExtra.cseAdd:
                        if (vDeItem[DeProp.Trace] == "")
                            vDeItem[DeProp.Trace] = UserInfo.Name + "(" + UserInfo.ID + ") 添加 "
                            + string.Format("{0:yyyy-MM-dd}", FServerInfo.DateTime);
                        break;
                }
            }
        }

        private void DoSyntaxCheck(HCCustomData aData, Stack<HCDomainInfo> aDomainStack, int aItemNo)
        {
            /*DeItem vDeItem = aData.Items[aItemNo] as DeItem;
            vDeItem.SyntaxClear();
            string vText = vDeItem.Text;

            if (PatientInfo.Sex == "男")
            {
                int vPos = vText.IndexOf("子宫");
                if (vPos >= 0)
                    vDeItem.SyntaxAdd(vPos + 1, 2);
            }*/
        }

        private void PrepareSyncData(int aDesID)
        {
            FDataElementSetMacro = emrMSDB.DB.GetData(string.Format("SELECT ObjID, MacroType, Macro FROM Comm_DataElementSetMacro WHERE DesID = {0}", aDesID));
            FServerInfo.DateTime = DateTime.Now;
        }

        private string GetDeValueFromStruct(string aPatID, int aDesID, string aDeIndex)
        {
            string Result = "";
            XmlElement vXmlNode = GetDeItemNodeFromStructDoc(aPatID, aDesID, aDeIndex);
            if (vXmlNode != null)
            {
                if (vXmlNode.InnerText != "")
                    Result = vXmlNode.InnerText;
            }

            return Result;
        }

        private XmlElement GetDeItemNodeFromStructDoc(string aPatID, int aDesID, string aDeIndex)
        {
            XmlElement Result = null;
            XmlDocument vXmlDoc = null;
            for (int i = 0; i < FStructDocs.Count; i++)
            {
                if ((FStructDocs[i].PatID == aPatID) && (FStructDocs[i].DesID == aDesID))
                {
                    vXmlDoc = FStructDocs[i].XmlDoc;
                    break;
                }
            }

            if (vXmlDoc == null)
            {
                DataTable dt = emrMSDB.DB.GetData(string.Format("SELECT strct.structure FROM Inch_Patient inpat LEFT JOIN Inch_RecordInfo inrec ON inpat.Patient_ID = inrec.PatID LEFT JOIN Inch_RecordStructure strct ON inrec.ID = strct.rid WHERE inpat.Patient_ID = {0} and inrec.desID = {1}", aPatID, aDesID));

                if (dt.Rows.Count > 0)
                {
                    byte[] vContent = (byte[])dt.Rows[0]["structure"];
                    using (MemoryStream vStream = new MemoryStream(vContent))
                    {
                        vStream.Position = 0;
                        StructDoc vStructDoc = new StructDoc(aPatID, aDesID);
                        vStructDoc.XmlDoc.Load(vStream);
                        vXmlDoc = vStructDoc.XmlDoc;
                        FStructDocs.Add(vStructDoc);
                    }
                }
            }

            if (vXmlDoc != null)
                Result = StructDoc.GetDeItemNode(aDeIndex, vXmlDoc);

            return Result;
        }

        private string GetMarcoSqlResult(string aObjID, string aMacro)
        {
            string vField = "";
            string vSqlResult = aMacro;

            while (true)
            {
                int vProPos = vSqlResult.IndexOf(":PatientInfo");
                if (vProPos > 0)
                {
                    int vFieldPos = vSqlResult.IndexOf(" ", vProPos + 13);  // :patientinfo.
                    if (vFieldPos < 0)
                        vFieldPos = vSqlResult.Length - 1;

                    vField = vSqlResult.Substring(vProPos + 13, vFieldPos - vProPos - 13 + 1);

                    vSqlResult = vSqlResult.Replace(":PatientInfo." + vField, PatientInfo.FieldByName(vField).ToString());
                    continue;
                }

                vProPos = vSqlResult.IndexOf(":UserInfo");
                if (vProPos > 0)
                {
                    int vFieldPos = vSqlResult.IndexOf(" ", vProPos + 9);  // :userinfo.
                    if (vFieldPos < 0)
                        vFieldPos = vSqlResult.Length - 1;

                    vField = vSqlResult.Substring(vProPos + 9, vFieldPos - vProPos - 9 + 1);

                    vSqlResult = vSqlResult.Replace(":UserInfo." + vField, UserInfo.FieldByName(vField).ToString());
                    continue;
                }

                vProPos = vSqlResult.IndexOf(":ServerInfo");
                if (vProPos > 0)
                {
                    int vFieldPos = vSqlResult.IndexOf(" ", vProPos + 11);  // :serverinfo.
                    if (vFieldPos < 0)
                        vFieldPos = vSqlResult.Length - 1;

                    vField = vSqlResult.Substring(vProPos + 11, vFieldPos - vProPos - 11 + 1);

                    vSqlResult = vSqlResult.Replace(":ServerInfo." + vField, FServerInfo.FieldByName(vField).ToString());
                    continue;
                }

                break;
            }

            DataTable dt = emrMSDB.DB.GetData(vSqlResult);
            if (dt.Rows.Count > 0)
            {
                vSqlResult = dt.Rows[0]["value"].ToString();
            }

            if (vSqlResult != "")
                return vSqlResult;
            else
                return "";
        }

        private string GetDeItemValueTry(string aDeIndex)
        {
            if (FDataElementSetMacro == null)
                return "";

            DataRow[] vDataRows = FDataElementSetMacro.Select("ObjID=" + aDeIndex);
            if (vDataRows.Length == 1)  // 有此数据元的替换信息
            {
                switch (vDataRows[0]["MacroType"].ToString())
                {
                    case "1":  // 患者信息(客户端处理)
                        return EMR.GetValueAsString(PatientInfo.FieldByName(vDataRows[0]["Macro"].ToString()));

                    case "2":  // 用户信息(客户端处理)
                        return EMR.GetValueAsString(UserInfo.FieldByName(vDataRows[0]["Macro"].ToString()));

                    case "3":  // 病历信息(服务端处理)
                        return GetDeValueFromStruct(PatientInfo.PatID, int.Parse(vDataRows[0]["Macro"].ToString()), aDeIndex);

                    case "4":  // 环境信息(服务端，如当前时间等)
                        return EMR.GetValueAsString(FServerInfo.FieldByName(vDataRows[0]["Macro"].ToString()));

                    case "5":  // SQL脚本(服务端处理)
                        return GetMarcoSqlResult(aDeIndex, vDataRows[0]["Macro"].ToString());

                    default:
                        break;
                }
            }

            return "";
        }

        /// <summary> 获取指定数据元同步的值 </summary>
        /// <param name="aDeItem"></param>
        /// <returns></returns>
        private string DoDeItemGetSyncValue(int aDesID, DeItem aDeItem)
        {
            string vDeIndex = aDeItem[DeProp.Index];
            if (vDeIndex != "")
            {
                if (FSyncDataDesID != aDesID)
                    this.PrepareSyncData(aDesID);

                return GetDeItemValueTry(vDeIndex);
            }

            return "";
        }

        private void DoSyncDeItem(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            string vsResult = "";
            string vDeIndex = "";

            if (aItem is DeItem)
            {
                DeItem vDeItem = aItem as DeItem;
                if (vDeItem.IsElement)
                {
                    vDeIndex = vDeItem[DeProp.Index];
                    if (vDeIndex != "")
                    {
                        vsResult = GetDeItemValueTry(vDeIndex);
                        if (vsResult != "")
                            vDeItem.Text = vsResult;
                    }
                }
            }
            else
            if (aItem is DeEdit)
            {
                vDeIndex = (aItem as DeEdit)[DeProp.Index];
                if (vDeIndex != "")
                {
                    vsResult = GetDeItemValueTry(vDeIndex);
                    if (vsResult != "")
                        (aItem as DeEdit).Text = vsResult;
                }
            }
            else
            if (aItem is DeCombobox)
            {
                vDeIndex = (aItem as DeCombobox)[DeProp.Index];
                if (vDeIndex != "")
                {
                    vsResult = GetDeItemValueTry(vDeIndex);
                    if (vsResult != "")
                        (aItem as DeCombobox).Text = vsResult;
                }
            }
            else
            if (aItem is DeFloatBarCodeItem)
            {
                vDeIndex = (aItem as DeFloatBarCodeItem)[DeProp.Index];
                if (vDeIndex != "")  // 是数据元
                {
                    vsResult = GetDeItemValueTry(vDeIndex);
                    if (vsResult != "")
                        (aItem as DeFloatBarCodeItem).Text = vsResult;
                }
            }
            else
            if (aItem is DeImageItem)
            {
                vDeIndex = (aItem as DeImageItem)[DeProp.Index];
                if (vDeIndex != "")  // 是数据元
                {
                    //根据vDeIndex赋值不同的图片
                    // (AItem as TDeImageItem).LoadFromBmpFile('');
                }
            }
        }

        private void SyncDeGroupByStruct(HCEmrView aEmrView)
        {
            for (int i = 0; i < aEmrView.Sections.Count; i++)
            {
                HCViewData vData = aEmrView.Sections[0].Page;
                int vItemNo = vData.Items.Count - 1;

                while (vItemNo >= 0)
                {
                    if (HCDomainItem.IsBeginMark(vData.Items[vItemNo]))  // 是数据组开始位置
                    {
                        string vDeGroupIndex = (vData.Items[vItemNo] as DeGroup)[DeProp.Index];  // 数据组标识
                        DataRow[] vRows = FDataElementSetMacro.Select("MacroType=3 and ObjID=" + vDeGroupIndex);
                        if (vRows.Length > 0)  // 有该数据组的引用替换配置信息
                        {
                            // 得到指定的数据集对应的病历结构xml文档，并从xml中找该数据组的节点
                            XmlElement vXmlNode = GetDeItemNodeFromStructDoc(PatientInfo.PatID,
                                int.Parse(vRows[0]["Macro"].ToString()), vDeGroupIndex);

                            if (vXmlNode != null)  // 找到了该数据组对应的节点
                            {
                                string vText = "";
                                for (int j = 0; j < vXmlNode.ChildNodes.Count; j++)
                                    vText = vText + vXmlNode.ChildNodes[j].InnerText;

                                if (vText != "")  // 得到不为空的节点内容并赋值给数据组
                                    aEmrView.SetDeGroupText(vData, vItemNo, vText);
                            }
                        }
                        //else
                        //if (vDeGroupIndex == "197")
                        //{
                        //    aEmrView.SetDeGroupText(vData, vItemNo, "第一条医嘱\r\n    第一条医嘱子医嘱\r\n    第一条医嘱子医嘱\r\n第二条医嘱\r\n    第二条医嘱子医嘱\r\n    第二条医嘱子医嘱");
                        //}
                    }

                    vItemNo--;
                }
            }
        }

        private void DoSaveRecordContent(object sender, EventArgs e)
        {
            frmRecord vFrmRecord = sender as frmRecord;

            if (!vFrmRecord.EmrView.IsChanged)
            {
                MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                DialogResult dr = MessageBox.Show("未发生变化，确定要执行保存?", "确认操作", messButton);
                if (dr != DialogResult.OK)
                    return;
            }

            RecordInfo vRecordInfo = vFrmRecord.ObjectData as RecordInfo;

            if (vFrmRecord.EmrView.Trace)
            {
                FServerInfo.DateTime = DateTime.Now;

                HashSet<SectionArea> vAreas = new HashSet<SectionArea> { SectionArea.saPage };
                vFrmRecord.TraverseElement(DoTraverseItem, vAreas, TTravTag.WriteTraceInfo | TTravTag.HideTrace);
            }

            using (MemoryStream vSM = new MemoryStream())
            {
                vFrmRecord.EmrView.SaveToStream(vSM);

                if (vRecordInfo.ID > 0)  // 修改后保存
                {
                    EMRView.emrMSDB.ExecCommandEventHanler vEvent = delegate (SqlCommand sqlComm)
                    {
                        sqlComm.Parameters.AddWithValue("RID", vRecordInfo.ID);
                        sqlComm.Parameters.AddWithValue("LastUserID", UserInfo.ID);
                        sqlComm.Parameters.AddWithValue("content", vSM.GetBuffer());
                    };

                    if (emrMSDB.DB.ExecSql(emrMSDB.Sql_SaveRecordContent, vEvent))
                    {
                        vFrmRecord.EmrView.IsChanged = false;
                        SaveRecordStructure(vRecordInfo.ID, vFrmRecord, false);  // 提取并保存病历结构
                        MessageBox.Show("保存成功！");
                    }
                    else
                        MessageBox.Show("保存病历失败，请重试！\n" + emrMSDB.DB.ErrMsg);
                }
                else  // 保存新建的病历
                {
                    EMRView.emrMSDB.ExecCommandEventHanler vEvent = delegate (SqlCommand sqlComm)
                    {
                        sqlComm.CommandType = CommandType.StoredProcedure;
                        sqlComm.CommandText = "CreateInchRecord";
                        sqlComm.Parameters.AddWithValue("PatID", PatientInfo.PatID);
                        sqlComm.Parameters.AddWithValue("VisitID", PatientInfo.VisitID);
                        sqlComm.Parameters.AddWithValue("desid", vRecordInfo.DesID);
                        sqlComm.Parameters.AddWithValue("Name", vRecordInfo.RecName);
                        sqlComm.Parameters.AddWithValue("DT", vRecordInfo.DT);
                        sqlComm.Parameters.AddWithValue("DeptID", PatientInfo.DeptID);
                        sqlComm.Parameters.AddWithValue("CreateUserID", UserInfo.ID);
                        sqlComm.Parameters.AddWithValue("Content", vSM.GetBuffer());

                        //SqlParameter parOutput = sqlComm.Parameters.Add("@RecordID", SqlDbType.Int);
                        //parOutput.Direction = ParameterDirection.Output;
                        SqlParameter parRetrun = new SqlParameter("@RecordID", SqlDbType.Int);
                        parRetrun.Direction = ParameterDirection.ReturnValue;
                        sqlComm.Parameters.Add(parRetrun);

                        sqlComm.ExecuteNonQuery();
                        vRecordInfo.ID = int.Parse(parRetrun.Value.ToString());
                    };

                    if (emrMSDB.DB.ExecStoredProcedure(vEvent))
                    {
                        vFrmRecord.EmrView.IsChanged = false;
                        SaveRecordStructure(vRecordInfo.ID, vFrmRecord, true);  // 提取并保存病历结构
                        GetPatientRecordListUI();
                        MessageBox.Show("保存成功！");
                    }
                    else
                        MessageBox.Show("保存病历失败，请重试！\n" + emrMSDB.DB.ErrMsg);
                }
            }
            // 病历导出为图片
            //vFrmRecord.SaveToImage(@"c:\", vRecordInfo.ID.ToString() + "_" + vRecordInfo.DesID.ToString());
        }

        private void DoSaveRecordStructure(object sender, EventArgs e)
        {
            frmRecord vFrmRecord = sender as frmRecord;

            if (!vFrmRecord.EmrView.IsChanged)
            {
                MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                DialogResult dr = MessageBox.Show("未发生变化，确定要更新病历结构数据?", "确认操作", messButton);
                if (dr != DialogResult.OK)
                    return;
            }

            RecordInfo vRecordInfo = vFrmRecord.ObjectData as RecordInfo;
            SaveRecordStructure(vRecordInfo.ID, vFrmRecord, false);  // 更新病历结构内容
            MessageBox.Show("更新病历 " + vRecordInfo.RecName + " 结构成功！");
        }

        private void DoRecordChangedSwitch(object sender, EventArgs e)
        {
            if (sender is frmRecord)
            {
                if ((sender as frmRecord).Parent is TabPage)
                {
                    string vText = ((sender as frmRecord).ObjectData as RecordInfo).RecName;

                    if ((sender as frmRecord).EmrView.IsChanged)
                        vText = vText + "*";

                    ((sender as frmRecord).Parent as TabPage).Text = vText;
                }
            }
        }

        private void DoRecordReadOnlySwitch(object sender, EventArgs e)
        {

        }

        private bool DoRecordCopyRequest(int aFormat)
        {
            if (aFormat == HC.View.HC.HCExtFormat.Id)  // 复制为HC格式
                return true;
            else  // 不是复制为HC格式
            if (EMR.ServerParam.PasteOutSide)  // 允许复制到外面
                return true;

            return false;
        }

        private bool DoRecordPasteRequest(int aFormat)
        {
            if (aFormat == HC.View.HC.HCExtFormat.Id)  // 粘贴HC格式
                return true;  // 允许，具体是来源于哪个患者的数据在DoPasteDataBefor中判断
            else
            if (EMR.ServerParam.PasteDifferent)  // 允许不同患者之间粘贴数据
                return true;

            return false;
        }

        private bool DoRecordCopyAsStream(Stream aStream)
        {
            if (!EMR.ServerParam.PasteDifferent)  // 不允许不同患者之间粘贴数据
                HC.View.HC.HCSaveTextToStream(aStream, PatientInfo.PatID);  // 写入患者PatID

            return true;
        }

        private bool DoRecordPasteFromStream(Stream aStream)
        {
            if (!EMR.ServerParam.PasteDifferent)  // 不允许不同患者之间粘贴数据
            {
                string vPatID = "";
                HC.View.HC.HCLoadTextFromStream(aStream, ref vPatID, HC.View.HC.HC_FileVersionInt);
                if (vPatID == PatientInfo.PatID)
                    return true;
                else
                    MessageBox.Show("您要粘贴的内容来源于其他患者，当前系统禁止粘贴不同患者之间的病历！");
            }
            else
                return true;

            return false;
        }

        private frmRecord GetActiveRecord()
        {
            if (tabRecord.SelectedIndex >= 0)
                return GetPageRecord(tabRecord.SelectedIndex);
            else
                return null;
        }

        private int GetRecordPageIndex(int aRecordID)
        {
            for (int i = 0; i < tabRecord.TabPages.Count; i++)
            {
                if (int.Parse(tabRecord.TabPages[i].Tag.ToString()) == aRecordID)
                {
                    return i;
                }
            }

            return -1;
        }

        private frmRecord GetPageRecord(int aPageIndex)
        {
            for (int i = 0; i < tabRecord.TabPages[aPageIndex].Controls.Count; i++)
            {
                if (tabRecord.TabPages[aPageIndex].Controls[i] is frmRecord)
                    return tabRecord.TabPages[aPageIndex].Controls[i] as frmRecord;
            }

            return null;
        }

        private void CloseRecordPage(int aPageIndex, bool aSaveChange = true)
        {
            if (aPageIndex >= 0)
            {
                TabPage vPage = tabRecord.TabPages[aPageIndex];
                frmRecord vFrmRecord = GetPageRecord(aPageIndex);
                if (aSaveChange && vFrmRecord.EmrView.IsChanged && (int.Parse(vPage.Tag.ToString()) > 0))
                {
                    MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                    DialogResult dr = MessageBox.Show("是否保存病历 " + (vFrmRecord.ObjectData as RecordInfo).RecName + "？", "确认操作", messButton);
                    if (dr == DialogResult.OK)
                        DoSaveRecordContent(vFrmRecord, null);
                }

                tabRecord.TabPages.Remove(vPage);
            }
        }

        private void NewPageAndRecord(RecordInfo aRecordInfo, ref TabPage aPage, ref frmRecord aFrmRecord)
        {
            aPage = new TabPage(aRecordInfo.RecName);
            aPage.Tag = aRecordInfo.ID;
            // 创建病历窗体
            aFrmRecord = new frmRecord();
            aFrmRecord.TopLevel = false;
            aFrmRecord.OnSave = DoSaveRecordContent;
            aFrmRecord.OnSaveStructure = DoSaveRecordStructure;
            aFrmRecord.OnChangedSwitch = DoRecordChangedSwitch;
            aFrmRecord.OnReadOnlySwitch = DoRecordReadOnlySwitch;
            aFrmRecord.OnSetDeItemText = DoSetDeItemText;
            aFrmRecord.OnDeItemPopup = DoDeItemPopup;
            aFrmRecord.OnPrintPreview = DoPrintPreview;
            aFrmRecord.OnDeItemGetSyncValue = DoDeItemGetSyncValue;
            aFrmRecord.OnSyntaxCheck = DoSyntaxCheck;

            aFrmRecord.OnCopyRequest = DoRecordCopyRequest;
            aFrmRecord.OnPasteRequest = DoRecordPasteRequest;
            aFrmRecord.OnCopyAsStream = DoRecordCopyAsStream;
            aFrmRecord.OnPasteFromStream = DoRecordPasteFromStream;

            aFrmRecord.ObjectData = aRecordInfo;

            aPage.Controls.Add(aFrmRecord);
            aFrmRecord.Dock = DockStyle.Fill;
            aFrmRecord.Show();

            tabRecord.TabPages.Add(aPage);
        }

        private TreeNode GetPatientNode()
        {
            return tvRecord.Nodes[0];
        }

        private void GetPatientRecordListUI()
        {
            tvRecord.Nodes.Clear();
            // 本次住院节点
            TreeNode vPatNode = tvRecord.Nodes.Add("第" + PatientInfo.VisitID.ToString() + "次 " + PatientInfo.BedNo + "床 " + PatientInfo.Name
                + " " + string.Format("{0:yyyy-MM-dd}", PatientInfo.InDeptDateTime));

            DataTable dt = emrMSDB.DB.GetData(string.Format(emrMSDB.Sql_GetInchRecordList, PatientInfo.PatID, PatientInfo.VisitID));
            tvRecord.BeginUpdate();
            try
            {
                int vDesPID = 0;
                TreeNode vNode = vPatNode;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (vDesPID.ToString() != dt.Rows[i]["desPID"].ToString())
                    {
                        vDesPID = int.Parse(dt.Rows[i]["desPID"].ToString());
                        RecordDataSetInfo vRecordDataSetInfo = new RecordDataSetInfo();
                        vRecordDataSetInfo.DesPID = vDesPID;

                        DataSetInfo vDataSetInfo = emrMSDB.DB.GetDataSetInfo(vDesPID);
                        vNode = vPatNode.Nodes.Add(vDataSetInfo.GroupName);
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
            }
            finally
            {
                tvRecord.EndUpdate();
            }
        }

        private void LoadPatientDataSetContent(int aDeSetID)
        {
            DataTable dt = emrMSDB.DB.GetData(string.Format(emrMSDB.Sql_GetDataSetRecord, PatientInfo.PatID, PatientInfo.VisitID, aDeSetID));
            if (dt.Rows.Count >= 0)
            {
                int vIndex = 0;

                frmRecord vFrmRecord = new frmRecord();
                vFrmRecord.OnReadOnlySwitch = DoRecordReadOnlySwitch;

                vFrmRecord.TopLevel = false;
                TabPage vPage = new TabPage();
                vPage.Text = "病程记录";
                vPage.Tag = -aDeSetID;
                vPage.Controls.Add(vFrmRecord);
                vFrmRecord.Dock = DockStyle.Fill;

                vFrmRecord.EmrView.BeginUpdate();
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        byte[] vContent = (byte[])dt.Rows[i]["content"];
                        MemoryStream vStream = new MemoryStream(vContent);
                        if (vStream.Length > 0)
                        {
                            if (vIndex > 0)
                            {
                                vFrmRecord.EmrView.ActiveSection.ActiveData.SelectLastItemAfterWithCaret();
                                vFrmRecord.EmrView.InsertBreak();
                                vFrmRecord.EmrView.ApplyParaAlignHorz(ParaAlignHorz.pahLeft);
                            }

                            vFrmRecord.EmrView.InsertStream(vStream);
                            vIndex++;
                        }
                    }
                }
                finally
                {
                    vFrmRecord.EmrView.EndUpdate();
                }

                vFrmRecord.Show();
                tabRecord.TabPages.Add(vPage);
            }
        }

        private void LoadPatientRecordContent(RecordInfo aRecordInfo)
        {
            using (MemoryStream vSM = new MemoryStream())
            {
                emrMSDB.DB.GetRecordContent(aRecordInfo.ID, vSM);
                if (vSM.Length > 0)
                {
                    TabPage vPage = null;
                    frmRecord vFrmRecord = null;

                    NewPageAndRecord(aRecordInfo, ref vPage, ref vFrmRecord);

                    // 把文件在加载到编辑器前存下来
                    /*FileStream vStream = new FileStream(@"c:\上级查房.hcf", FileMode.CreateNew, FileAccess.Write);
                    byte[] vBuffer = new byte[vSM.Length];
                    vSM.Position = 0;
                    vSM.Read(vBuffer, 0, (int)vSM.Length);
                    vStream.Write(vBuffer, 0, vBuffer.Length);
                    vStream.Flush();
                    vStream.Close();*/

                    vFrmRecord.EmrView.LoadFromStream(vSM);
                    vFrmRecord.EmrView.ReadOnly = true;
           
                    tabRecord.SelectedTab = vPage;
                    vFrmRecord.EmrView.Focus();
                }
            }
        }

        private void DeletePatientRecord(int aRecordID)
        {
            emrMSDB.DB.ExecSql(string.Format("EXEC DeleteInchRecord {0}", aRecordID));
        }

        private TreeNode FindRecordNode(int aRecordID)
        {
            for (int i = 0; i < tvRecord.Nodes.Count; i++)
            {
                if (EMR.TreeNodeIsRecord(tvRecord.Nodes[i]))
                {
                    return tvRecord.Nodes[i];
                }
            }

            return null;
        }

        private void SaveStructureToXml(frmRecord aFrmRecord, string aFileName)
        {
            XmlDocument vXmlDoc = GetStructureToXml(aFrmRecord);
            vXmlDoc.Save(aFileName);
        }

        private XmlDocument GetStructureToXml(frmRecord aFrmRecord)
        {
            HCItemTraverse vItemTraverse = new HCItemTraverse();  // 准备存放遍历信息的对象
            XmlStruct vXmlStruct = new XmlStruct();  
            vItemTraverse.Areas.Add(SectionArea.saPage);  // 遍历正文中的信息
            vItemTraverse.Process = vXmlStruct.TraverseItem;  // 遍历到每一个文本对象是触发的事件

            vXmlStruct.XmlDoc.DocumentElement.SetAttribute("DesID", (aFrmRecord.ObjectData as RecordInfo).DesID.ToString());
            vXmlStruct.XmlDoc.DocumentElement.SetAttribute("DocName", (aFrmRecord.ObjectData as RecordInfo).RecName);

            aFrmRecord.EmrView.TraverseItem(vItemTraverse);  // 开始遍历
            return vXmlStruct.XmlDoc;
        }

        private void SaveRecordStructure(int aRecordID, frmRecord aFrmRecord, bool aInsert)
        {
            XmlDocument vXmlDoc = GetStructureToXml(aFrmRecord);
            if (vXmlDoc == null)
                return;

            using (MemoryStream vSM = new MemoryStream())
            {
                vXmlDoc.Save(vSM);

                EMRView.emrMSDB.ExecCommandEventHanler vEvent = delegate(SqlCommand sqlComm)
                {
                    sqlComm.Parameters.AddWithValue("rid", aRecordID);
                    sqlComm.Parameters.AddWithValue("structure", vSM.GetBuffer());
                };

                if (aInsert)
                {
                    if (emrMSDB.DB.ExecSql("INSERT INTO Inch_RecordStructure (rid, structure) VALUES (@rid, @structure)", vEvent))
                    {

                    }
                    else
                        MessageBox.Show("保存病历结构失败：" + emrMSDB.DB.ErrMsg);
                }
                else
                {
                    if (emrMSDB.DB.ExecSql("UPDATE Inch_RecordStructure SET structure = @structure WHERE rid = @rid", vEvent))
                    {

                    }
                    else
                        MessageBox.Show("保存病历结构失败：" + emrMSDB.DB.ErrMsg);
                }
            }
        }

        private void GetRecordStructure(int aRecordID, frmRecord aFrmRecord)
        {

        }

        public UserInfo UserInfo;
        public PatientInfo PatientInfo;

        public frmPatientRecord()
        {
            InitializeComponent();
        }

        private void tvRecord_DoubleClick(object sender, EventArgs e)
        {
            //if (frmRecord == null)
            //{
            //    frmRecord = new frmRecord();
            //    frmRecord.TopLevel = false;
            //    frmRecord.Dock = DockStyle.Fill;
            //    //this.splitContainer1.Panel2.Controls.Add(frmRecord);
            //    frmRecord.Parent = this.splitContainer1.Panel2;
            //    frmRecord.Show();
            //}
        }

        private void 查看ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int vDesPID = -1, vDesID = -1, vRecordID = -1;
            EMR.GetNodeRecordInfo(tvRecord.SelectedNode, ref vDesPID, ref vDesID, ref vRecordID);

            if (vRecordID > 0)
            {
                int vPageIndex = GetRecordPageIndex(vRecordID);
                if (vPageIndex < 0)
                {
                    LoadPatientRecordContent(tvRecord.SelectedNode.Tag as RecordInfo);
                    vPageIndex = GetRecordPageIndex(vRecordID);
                }
                else
                    tabRecord.SelectedIndex = vPageIndex;

                frmRecord vFrmRecord = null;
                try
                {
                    vFrmRecord = GetPageRecord(vPageIndex);
                }
                finally
                {
                    vFrmRecord.EmrView.ReadOnly = true;
                }

                vFrmRecord.EmrView.Trace = emrMSDB.DB.GetInchRecordSignature(vRecordID);
            }
        }

        private void 编辑ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int vDesPID = -1, vDesID = -1, vRecordID = -1;
            EMR.GetNodeRecordInfo(tvRecord.SelectedNode, ref vDesPID, ref vDesID, ref vRecordID);

            if (vRecordID > 0)
            {
                int vPageIndex = GetRecordPageIndex(vRecordID);
                if (vPageIndex < 0)
                {
                    LoadPatientRecordContent(tvRecord.SelectedNode.Tag as RecordInfo);
                    vPageIndex = GetRecordPageIndex(vRecordID);
                }
                else
                    tabRecord.SelectedIndex = vPageIndex;

                frmRecord vFrmRecord = GetPageRecord(vPageIndex);
                vFrmRecord.EmrView.ReadOnly = false;
                // 下面2行可实现只有正文可修改
                //vFrmRecord.EmrView.ActiveSection.Header.ReadOnly = true;
                //vFrmRecord.EmrView.ActiveSection.Footer.ReadOnly = true;
                vFrmRecord.EmrView.UpdateView();

                try
                {
                    vFrmRecord.EmrView.Trace = emrMSDB.DB.GetInchRecordSignature(vRecordID);
                    if (vFrmRecord.EmrView.Trace)
                        MessageBox.Show("病历已经签名，后续的修改将留下修改痕迹！");
                }
                catch (Exception exp)
                {
                    vFrmRecord.EmrView.ReadOnly = true;
                    MessageBox.Show("编辑病历失败！" + exp.Message + "\n\r" + emrMSDB.DB.ErrMsg);
                }
            }
        }

        private void 签名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int vDesPID = -1, vDesID = -1, vRecordID = -1;
            EMR.GetNodeRecordInfo(tvRecord.SelectedNode, ref vDesPID, ref vDesID, ref vRecordID);

            if (vRecordID > 0)
            {
                if (emrMSDB.DB.SignatureInchRecord(vRecordID, UserInfo.ID))
                    MessageBox.Show(UserInfo.Name + "，签名成功！后续的修改将留下修改痕迹！");

                int vPageIndex = GetRecordPageIndex(vRecordID);
                if (vPageIndex >= 0)
                {
                    frmRecord vFrmRecord = GetPageRecord(vPageIndex);
                    vFrmRecord.EmrView.Trace = true;
                }
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int vDesPID = -1, vDesID = -1, vRecordID = -1;
            EMR.GetNodeRecordInfo(tvRecord.SelectedNode, ref vDesPID, ref vDesID, ref vRecordID);

            if (vRecordID > 0)
            {
                MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                DialogResult dr = MessageBox.Show("删除病历 " + tvRecord.SelectedNode.Text + " ？", "确认操作", messButton);
                if (dr == DialogResult.OK)
                {
                    int vPageIndex = GetRecordPageIndex(vRecordID);
                    if (vPageIndex >= 0)
                        CloseRecordPage(tabRecord.SelectedIndex, false);

                    if (emrMSDB.DB.DeletePatientRecord(vRecordID))
                    {
                        tvRecord.Nodes.Remove(tvRecord.SelectedNode);
                        MessageBox.Show("删除成功！");
                    }
                }
            }
        }

        private void mniMergeRecord_Click(object sender, EventArgs e)
        {
            frmRecordSet vFrmRecordSet = new frmRecordSet();
            vFrmRecordSet.ShowDialog(PatientInfo.PatID, PatientInfo.VisitID);
            //if (!EMR.TreeNodeIsRecord(tvRecord.SelectedNode))
            //    return;

            //int vDesPID = -1, vDesID = -1, vRecordID = -1;
            //GetNodeRecordInfo(tvRecord.SelectedNode, ref vDesPID, ref vDesID, ref vRecordID);

            //if (vDesPID == DataSetInfo.Proc)
            //{
            //    int vPageIndex = GetRecordPageIndex(-vDesPID);
            //    if (vPageIndex < 0)
            //        LoadPatientDataSetContent(vDesPID);
            //    else
            //        tabRecord.SelectedIndex = vPageIndex;
            //}
        }

        private void 导出XML结构ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int vDesPID = -1, vDesID = -1, vRecordID = -1;
            EMR.GetNodeRecordInfo(tvRecord.SelectedNode, ref vDesPID, ref vDesID, ref vRecordID);

            if (vRecordID > 0)
            {
                SaveFileDialog vSaveDlg = new SaveFileDialog();
                vSaveDlg.Filter = "XML|*.xml";
                vSaveDlg.ShowDialog();
                if (vSaveDlg.FileName != "")
                {
                    int vPageIndex = GetRecordPageIndex(vRecordID);
                    if (vPageIndex < 0)
                    {
                        LoadPatientRecordContent(tvRecord.SelectedNode.Tag as RecordInfo);
                        vPageIndex = GetRecordPageIndex(vRecordID);
                    }
                    else
                        tabRecord.SelectedIndex = vPageIndex;

                    frmRecord vFrmRecord = GetPageRecord(vPageIndex);

                    string vFileName = Path.GetExtension(vSaveDlg.FileName);
                    if (vFileName.ToLower() != ".xml")
                        vFileName = vSaveDlg.FileName + ".xml";
                    else
                        vFileName = vSaveDlg.FileName;

                    SaveStructureToXml(vFrmRecord, vFileName);
                }
            }
        }

        private void tvRecord_DoubleClick_1(object sender, EventArgs e)
        {
            查看ToolStripMenuItem_Click(sender, e);
        }

        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseRecordPage(tabRecord.SelectedIndex);
        }

        private void DoImportAsText(string aText)
        {
            frmRecord vFrmRecord = GetActiveRecord();
            if (vFrmRecord != null)
                vFrmRecord.EmrView.InsertText(aText);
            else
                MessageBox.Show("未发现打开的病历！");
        }

        private void MniHisRecord_Click(object sender, EventArgs e)
        {
            frmPatientHisRecord vFrmHisRecord = new frmPatientHisRecord();
            vFrmHisRecord.PatientInfo = PatientInfo;
            vFrmHisRecord.OnImportAsText = DoImportAsText;  // 导入病历窗体中点击导入时触发的事件
            this.AddOwnedForm(vFrmHisRecord);
            vFrmHisRecord.Show();
        }

        private void mniReSync_Click(object sender, EventArgs e)
        {
            int vDesPID = -1, vDesID = -1, vRecordID = -1;
            EMR.GetNodeRecordInfo(tvRecord.SelectedNode, ref vDesPID, ref vDesID, ref vRecordID);

            if (vRecordID > 0)
            {
                int vPageIndex = GetRecordPageIndex(vRecordID);
                if (vPageIndex < 0)
                {
                    MessageBox.Show("请先打开病历并处于编辑状态！");
                    return;
                }
                else
                {
                    tabRecord.SelectedIndex = vPageIndex;
                    frmRecord vFrmRecord = GetPageRecord(vPageIndex);
                    if (vFrmRecord.EmrView.ReadOnly)
                    {
                        MessageBox.Show("请先将病历并处于编辑状态！");
                        return;
                    }

                    HCCustomItem vItem = null;
                    DeItem vDeItem = null;
                    HashSet<SectionArea> vAreas = new HashSet<SectionArea>() { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
                    TraverseItemEventHandle vTraverseItemEvent = delegate (HCCustomData aData, int aItemNo, int aTags, Stack<HCDomainInfo> aDomainStack, ref bool aStop)
                    {
                        vItem = aData.Items[aItemNo];
                        if (vItem.StyleNo < HCStyle.Null)
                            (vItem as HCCustomRectItem).FormatDirty();
                        else
                        if (vItem is DeItem)
                        {
                            vDeItem = vItem as DeItem;  // 每遍历到一个数据元

                            string vValue = DoDeItemGetSyncValue(vDesID, vDeItem);  // 取数据元的同步值
                            if (vValue != "")
                            {
                                vDeItem.Text = vValue;
                                //vDeItem[DeProp.CMVVCode] = ""; 值域编码
                            }
                        }
                    };

                    vFrmRecord.EmrView.BeginUpdate();
                    try
                    {
                        vFrmRecord.TraverseElement(vTraverseItemEvent, vAreas, 0);  // 遍历数据元
                    }
                    finally
                    {
                        vFrmRecord.EmrView.EndUpdate();
                    }
                }
            }
        }
    }

    public class StructDoc
    {
        private string FPatID;
        private int FDesID;
        private XmlDocument FXmlDoc;

        public StructDoc(string aPatID, int aDesID)
        {
            FPatID = aPatID;
            FDesID = aDesID;
            FXmlDoc = new XmlDocument();
        }

        ~StructDoc()
        {

        }

        #region 子方法
        private static XmlElement _GetDeNode(XmlNodeList aNodes, string aDeIndex)
        {
            XmlElement Result = null;

            for (int i = 0; i < aNodes.Count; i++)
            {
                if ((aNodes[i].Attributes != null) && (aNodes[i].Attributes["Index"] != null) && (aNodes[i].Attributes["Index"].Value == aDeIndex))
                {
                    Result = aNodes[i] as XmlElement;
                    break;
                }
                else
                {
                    Result = _GetDeNode(aNodes[i].ChildNodes, aDeIndex);
                    if (Result != null)
                        break;
                }
            }

            return Result;
        }
        #endregion

        public static XmlElement GetDeItemNode(string aDeIndex, XmlDocument aXmlDoc)
        {
            return _GetDeNode(aXmlDoc.DocumentElement.ChildNodes, aDeIndex);
        }

        public string PatID
        {
            get { return FPatID; }
            set { FPatID = value; }
        }

        public int DesID
        {
            get { return FDesID; }
            set { FDesID = value; }
        }

        public XmlDocument XmlDoc
        {
            get { return FXmlDoc; }
        }
    }

    public class XmlStruct
    {
        private XmlDocument FXmlDoc;
        private List<XmlElement> FDeGroupNodes;
        private DataTable FDETable;
        private bool FOnlyDeItem;  // 是否只存数据元，不存普通文本

        public XmlStruct()
        {
            FOnlyDeItem = false;

            FDETable = emrMSDB.DB.GetData("SELECT deid, decode, dename, py, frmtp, domainid FROM Comm_DataElement");

            FDeGroupNodes = new List<XmlElement>();
            FXmlDoc = new XmlDocument();

            XmlElement vNode = FXmlDoc.CreateElement("DocInfo");
            vNode.SetAttribute("SourceTool", "HCEMRView");
            FXmlDoc.AppendChild(vNode);
        }

        public void TraverseItem(HCCustomData aData, int aItemNo, int aTag, Stack<HCDomainInfo> aDomainStack, ref bool aStop)
        {
            if ((aData is HCHeaderData) || (aData is HCFooterData))
                return;

            if (aData.Items[aItemNo] is DeGroup)  // 数据组
            {
                DeGroup vDeGroup = aData.Items[aItemNo] as DeGroup;
                if (vDeGroup.MarkType == MarkType.cmtBeg)
                {
                    XmlElement vXmlNode = FXmlDoc.CreateElement("DeGroup");
                    if (FDeGroupNodes.Count > 0)
                        FDeGroupNodes[FDeGroupNodes.Count - 1].AppendChild(vXmlNode);
                    else
                        FXmlDoc.DocumentElement.AppendChild(vXmlNode);

                    vXmlNode.SetAttribute("Index", vDeGroup[DeProp.Index]);

                    DataRow[] vRows = FDETable.Select("DeID=" + vDeGroup[DeProp.Index]);
                    if (vRows.Length == 1)
                    {
                        vXmlNode.SetAttribute("Code", vRows[0]["decode"].ToString());
                        vXmlNode.SetAttribute("Name", vRows[0]["dename"].ToString());
                    }

                    FDeGroupNodes.Add(vXmlNode);
                }
                else
                {
                    if (FDeGroupNodes.Count > 0)
                        FDeGroupNodes.RemoveAt(FDeGroupNodes.Count - 1);
                }
            }
            else
            if (aData.Items[aItemNo].StyleNo > HCStyle.Null)  // 文本类
            {
                if ((aData.Items[aItemNo] as DeItem).IsElement)  // 是数据元
                {
                    DeItem vDeItem = aData.Items[aItemNo] as DeItem;
                    if (vDeItem[DeProp.Index] != "")
                    {
                        XmlElement vXmlNode = FXmlDoc.CreateElement("DeItem");
                        if (FDeGroupNodes.Count > 0)
                            FDeGroupNodes[FDeGroupNodes.Count - 1].AppendChild(vXmlNode);
                        else
                            FXmlDoc.DocumentElement.AppendChild(vXmlNode);

                        vXmlNode.InnerText = vDeItem.Text;
                        vXmlNode.SetAttribute("Index", vDeItem[DeProp.Index]);

                        DataRow[] vRows = FDETable.Select("DeID=" + vDeItem[DeProp.Index]);
                        if (vRows.Length == 1)
                        {
                            vXmlNode.SetAttribute("Code", vRows[0]["decode"].ToString());
                            vXmlNode.SetAttribute("Name", vRows[0]["dename"].ToString());
                        }
                    }
                }
                else
                if (!FOnlyDeItem)
                {
                    XmlElement vXmlNode = FXmlDoc.CreateElement("Text");
                    if (FDeGroupNodes.Count > 0)
                        FDeGroupNodes[FDeGroupNodes.Count - 1].AppendChild(vXmlNode);
                    else
                        FXmlDoc.DocumentElement.AppendChild(vXmlNode);

                    vXmlNode.InnerText = aData.Items[aItemNo].Text;
                }
            }
            else  // 非文本类
            {
                if (aData.Items[aItemNo] is EmrYueJingItem)
                {
                    XmlElement vXmlNode = FXmlDoc.CreateElement("DeItem");
                    if (FDeGroupNodes.Count > 0)
                        FDeGroupNodes[FDeGroupNodes.Count - 1].AppendChild(vXmlNode);
                    else
                        FXmlDoc.DocumentElement.AppendChild(vXmlNode);

                    (aData.Items[aItemNo] as EmrYueJingItem).ToXmlEmr(vXmlNode as XmlElement);
                }
                else
                if (aData.Items[aItemNo] is EmrToothItem)
                {
                    XmlElement vXmlNode = FXmlDoc.CreateElement("DeItem");
                    if (FDeGroupNodes.Count > 0)
                        FDeGroupNodes[FDeGroupNodes.Count - 1].AppendChild(vXmlNode);
                    else
                        FXmlDoc.DocumentElement.AppendChild(vXmlNode);

                    (aData.Items[aItemNo] as EmrToothItem).ToXmlEmr(vXmlNode as XmlElement);
                }
                else
                if (aData.Items[aItemNo] is EmrFangJiaoItem)
                {
                    XmlElement vXmlNode = FXmlDoc.CreateElement("DeItem");
                    if (FDeGroupNodes.Count > 0)
                        FDeGroupNodes[FDeGroupNodes.Count - 1].AppendChild(vXmlNode);
                    else
                        FXmlDoc.DocumentElement.AppendChild(vXmlNode);

                    (aData.Items[aItemNo] as EmrFangJiaoItem).ToXmlEmr(vXmlNode as XmlElement);
                }
            }
        }

        public XmlDocument XmlDoc
        {
            get { return FXmlDoc; }
        }
    }
}
