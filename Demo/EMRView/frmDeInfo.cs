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
    public partial class frmDeInfo : Form
    {
        private int FDeID;

        public frmDeInfo()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }

        #region 子方法
        private string GetFrmtpText(string aFrmtp)
        {
            if (aFrmtp == DeFrmtp.Radio)
                return "单选";
            else
                if (aFrmtp == DeFrmtp.Multiselect)
                    return "多选";
                else
                    if (aFrmtp == DeFrmtp.Number)
                        return "数值";
                    else
                        if (aFrmtp == DeFrmtp.String)
                            return "文本";
                        else
                            if (aFrmtp == DeFrmtp.Date)
                                return "日期";
                            else
                                if (aFrmtp == DeFrmtp.Time)
                                    return "时间";
                                else
                                    if (aFrmtp == DeFrmtp.DateTime)
                                        return "日期时间";
                                    else
                                        return "";
        }

        private string GetFrmtp(string aText)
        {
            if (aText == "单选")
                return DeFrmtp.Radio;
            else
                if (aText == "多选")
                    return DeFrmtp.Multiselect;
                else
                    if (aText == "数值")
                        return DeFrmtp.Number;
                    else
                        if (aText == "文本")
                            return DeFrmtp.String;
                        else
                            if (aText == "日期")
                                return DeFrmtp.Date;
                            else
                                if (aText == "时间")
                                    return DeFrmtp.Time;
                                else
                                    if (aText == "日期时间")
                                        return DeFrmtp.DateTime;
                                    else
                                        return "";
        }
        #endregion

        public void SetDeID(int aDeID)
        {
            FDeID = aDeID;

            if (FDeID > 0)  // 修改
            {
                this.Text = "数据元维护-" + FDeID.ToString();

                DataTable dt = emrMSDB.DB.GetData(string.Format("SELECT deid, decode, dename, py, dedefine, detype, deformat, frmtp, deunit, domainid FROM Comm_DataElement WHERE DeID = {0}", FDeID));
                if (dt.Rows.Count > 0)
                {
                    tbxName.Text = dt.Rows[0]["dename"].ToString();
                    tbxCode.Text = dt.Rows[0]["decode"].ToString();
                    tbxPY.Text = dt.Rows[0]["py"].ToString();
                    tbxDefine.Text = dt.Rows[0]["dedefine"].ToString();
                    tbxType.Text = dt.Rows[0]["detype"].ToString();
                    tbxFormat.Text = dt.Rows[0]["deformat"].ToString();
                    tbxUnit.Text = dt.Rows[0]["deunit"].ToString();
                    cbbFrmtp.SelectedIndex = cbbFrmtp.Items.IndexOf(GetFrmtpText(dt.Rows[0]["frmtp"].ToString()));
                    tbxDomainID.Text = dt.Rows[0]["domainid"].ToString();
                }
            }
            else
                this.Text = "新建数据元";

            this.ShowDialog();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (tbxName.Text.Trim() == "")
            {
                MessageBox.Show("错误，填写数据元名称！");
                return;
            }

            int vDomainID = 0;
            if (!int.TryParse(tbxDomainID.Text, out vDomainID))
            {
                MessageBox.Show("错误，填写的值域不能转为整数！");
                return;
            }

            string vSql = "";
            if (FDeID > 0)  // 修改
                vSql = string.Format("UPDATE Comm_DataElement SET decode = '{0}', dename = '{1}', py = '{2}', dedefine = '{3}', detype = '{4}', deformat = '{5}', frmtp = '{6}', deunit = '{7}', domainid = {8} WHERE DeID = {9}",
                    tbxCode.Text, tbxName.Text, tbxPY.Text, tbxDefine.Text, tbxType.Text, tbxFormat.TabIndex, GetFrmtp(cbbFrmtp.Text), tbxUnit.Text, tbxDomainID.Text, FDeID);
            else
                vSql = string.Format("INSERT INTO Comm_DataElement (decode, dename, py, dedefine, detype, deformat, frmtp, deunit, domainid) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')",
                    tbxCode.Text, tbxName.Text, tbxPY.Text, tbxDefine.Text, tbxType.Text, tbxFormat.TabIndex, GetFrmtp(cbbFrmtp.Text), tbxUnit.Text, tbxDomainID.Text);

            if (emrMSDB.DB.ExecSql(vSql))
                MessageBox.Show("保存成功！");
            else
                MessageBox.Show("保存失败！" + emrMSDB.DB.ErrMsg);
        }

        private void btnSaveClose_Click(object sender, EventArgs e)
        {
            btnSave_Click(sender, e);
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
