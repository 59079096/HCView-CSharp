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
using HC.Win32;
using System.IO;

namespace EMRView
{
    public delegate void TextEventHandle(string aText);
    public delegate void StreamEventHandle(Stream aStream);

    public partial class frmRecordPop : Form
    {
        string FSnum1, FSnum2, FSmark, FOldUnit;
        double FNum1, FNum2;
        bool FFlag, FSign, FTemp, FTemplate, FConCalcValue;
        string FFrmtp;
        DeItem FDeItem;
        DataTable FDBDomain = new DataTable();

        private TextEventHandle FOnSetActiveItemText;
        private StreamEventHandle FOnSetActiveItemExtra;

        private void SetDeItemValue(string value)
        {
            if (FOnSetActiveItemText != null)
                FOnSetActiveItemText(value);
        }

        private void SetDeItemExtraValue(string aCVVID)
        {
            if (FOnSetActiveItemExtra != null)
            {
                DataRow[] vRows = FDBDomain.Select("id=" + aCVVID);
                if (vRows.Length > 0)
                {
                    using(MemoryStream vSM = new MemoryStream((byte[])vRows[0]["content"]))
                    {
                        vSM.Position = 0;
                        FOnSetActiveItemExtra(vSM);
                    }
                }
            }
        }

        private void SetValueFocus()
        {
            tbxValue.Focus();
            tbxValue.SelectionStart = tbxValue.Text.Length;
            tbxValue.SelectionLength = 0;
        }

        private void SetConCalcValue()
        {
            if (!FConCalcValue)
            {
                tbxValue.Clear();
                FConCalcValue = true;
            }
        }

        private void PutCalcNumber(int aNum)
        {
            if (FTemplate)
            {
                if (aNum != 0)
                    tbxValue.Text += "." + aNum.ToString();
            }
            else
            {
                SetConCalcValue();
                if (FTemp || FFlag)
                {
                    tbxValue.Text = aNum.ToString();
                    FTemp = false;
                }
                else
                    tbxValue.Text += aNum.ToString();
            }

            SetValueFocus();
        }

        public frmRecordPop()
        {
            InitializeComponent();
        }

        #region 子方法
        private void IniDomainUI()
        {
            dgvDomain.RowCount = FDBDomain.Rows.Count;

            for (int i = 0; i < FDBDomain.Rows.Count; i++)
            {
                dgvDomain.Rows[i].Cells[0].Value = FDBDomain.Rows[i]["devalue"];
                dgvDomain.Rows[i].Cells[1].Value = FDBDomain.Rows[i]["code"];
                dgvDomain.Rows[i].Cells[2].Value = FDBDomain.Rows[i]["id"];
                dgvDomain.Rows[i].Cells[3].Value = FDBDomain.Rows[i]["py"];
                dgvDomain.Rows[i].Cells[4].Value = "";

                if (FDBDomain.Rows[i]["content"].GetType() != typeof(System.DBNull))
                {
                    byte[] vbuffer = (byte[])FDBDomain.Rows[i]["content"];
                    if (vbuffer.Length > 0)
                        dgvDomain.Rows[i].Cells[4].Value = "...";
                }
            }
        }
        #endregion

        public void PopupDeItem(DeItem aDeItem, POINT aPopupPt)
        {
            FFrmtp = "";
            FDeItem = aDeItem;
            string vDeUnit = "";
            int vCMV = -1;

            DataTable dt = emrMSDB.DB.GetData(string.Format("SELECT DeCode, PY, frmtp, deunit, domainid FROM Comm_DataElement WHERE DeID ={0}", FDeItem[DeProp.Index]));
            if (dt.Rows.Count > 0)
            {
                FFrmtp = dt.Rows[0]["frmtp"].ToString();
                vDeUnit = dt.Rows[0]["deunit"].ToString();
                vCMV = int.Parse(dt.Rows[0]["domainid"].ToString());
            }

            if (FFrmtp == DeFrmtp.Number)  // 数值
            {
                if (aDeItem[DeProp.Unit] != "")
                    tbxValue.Text = aDeItem.Text.Replace(aDeItem[DeProp.Unit], "");
                else
                    tbxValue.Text = aDeItem.Text;

                if (tbxValue.Text != "")
                    tbxValue.SelectAll();

                cbbUnit.Items.Clear();
                string[] vStrings = vDeUnit.Split(new string[] { "," }, StringSplitOptions.None);
                for (int i = 0; i < vStrings.Length; i++)
                {
                    cbbUnit.Items.Add(vStrings[i]);
                }
                if (cbbUnit.Items.Count > 0)
                {
                    cbbUnit.SelectedIndex = cbbUnit.Items.IndexOf(aDeItem[DeProp.Unit]);
                    if (cbbUnit.SelectedIndex < 0)
                        cbbUnit.SelectedIndex = 0;
                }
                else
                    cbbUnit.Text = aDeItem[DeProp.Unit];

                tabPop.SelectedIndex = 1;
                this.Width = 185;

                if (aDeItem[DeProp.Index] == "979")  // 体温
                {
                    tabQk.SelectedIndex = 0;
                    this.Height = 300;
                }
                else
                {
                    tabQk.SelectedIndex = -1;
                    this.Height = 215;
                }
            }
            else
            if ((FFrmtp == DeFrmtp.Date) || (FFrmtp == DeFrmtp.Time) || (FFrmtp == DeFrmtp.DateTime))  // 日期时间
            {
                tabPop.SelectedIndex = 3;
                this.Width = 260;
                this.Height = 170;

                pnlDate.Visible = FFrmtp != DeFrmtp.Time;
                pnlTime.Visible = FFrmtp != DeFrmtp.Date;
            }
            else
            if ((FFrmtp == DeFrmtp.Radio) || (FFrmtp == DeFrmtp.Multiselect))  // 单、多选
            {
                tbxSpliter.Clear();

                if (FDBDomain.Rows.Count > 0)
                    FDBDomain.Reset();

                dgvDomain.RowCount = 0;
                tabPop.SelectedIndex = 0;
                this.Width = 290;
                this.Height = 300;

                if (vCMV > 0)  // 有值域
                {
                    FDBDomain = emrMSDB.DB.GetData(string.Format("SELECT DE.ID, DE.Code, DE.devalue, DE.PY, DC.Content FROM Comm_DataElementDomain DE LEFT JOIN Comm_DomainContent DC ON DE.ID = DC.DItemID WHERE DE.domainid = {0}", vCMV));
                }

                if (FDBDomain.Rows.Count > 0)
                    IniDomainUI();
            }
            else
            if (FFrmtp == DeFrmtp.String)
            {
                tbxMemo.Clear();
                tabPop.SelectedIndex = 2;
                this.Width = 260;
                this.Height = 200;
            }

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(aPopupPt.X, aPopupPt.Y);

            this.Show();

            if (FFrmtp == DeFrmtp.Number)
                tbxValue.Focus();
        }

        public TextEventHandle OnSetActiveItemText
        {
            get { return FOnSetActiveItemText; }
            set { FOnSetActiveItemText = value; }
        }

        public StreamEventHandle OnSetActiveItemExtra
        {
            get { return FOnSetActiveItemExtra; }
            set { FOnSetActiveItemExtra = value; }
        }

        private void frmRecordPop_Load(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            tabPop.SizeMode = TabSizeMode.Fixed;
            tabPop.ItemSize = new Size(0, 1);
            cbbDate.SelectedIndex = 3;
            cbbTime.SelectedIndex = 3;
        }

        private void btnDomainOk_Click(object sender, EventArgs e)
        {
            if (dgvDomain.SelectedRows.Count > 0)
            {
                int vSelIndex = dgvDomain.SelectedRows[0].Index;
                FDeItem[DeProp.CMVVCode] = dgvDomain.Rows[vSelIndex].Cells[1].Value.ToString();
                if (dgvDomain.Rows[vSelIndex].Cells[4].Value.ToString() != "")
                    SetDeItemExtraValue(dgvDomain.Rows[vSelIndex].Cells[2].Value.ToString());
                else
                    SetDeItemValue(dgvDomain.Rows[vSelIndex].Cells[0].Value.ToString());

                this.Close();
            }
        }

        private void btnNumberOk_Click(object sender, EventArgs e)
        {
            string vText = tbxValue.Text;

            if (vText != "")
            {
                if (!cbxHideUnit.Checked)
                    vText += cbbUnit.Text;

                FDeItem[DeProp.Unit] = cbbUnit.Text;
                SetDeItemValue(vText);

                this.Close();
            }
        }

        private void btnMemoOk_Click(object sender, EventArgs e)
        {
            if (tbxMemo.Text != "")
            {
                SetDeItemValue(tbxMemo.Text);
                this.Close();
            }
        }

        private void btnNow_Click(object sender, EventArgs e)
        {
            dtpDate.Value = DateTime.Now;
            dtpTime.Value = DateTime.Now;
        }

        private void btnDateTimeOk_Click(object sender, EventArgs e)
        {
            string vText = "";
            if (FFrmtp == DeFrmtp.Date)
            {
                vText = string.Format("{0:" + cbbDate.Text + "}", dtpDate.Value);
            }
            else
            if (FFrmtp == DeFrmtp.Time)
            {
                vText = string.Format("{0:" + cbbTime.Text + "}", dtpTime.Value);
            }
            else
            if (FFrmtp == DeFrmtp.DateTime)
            {
                vText = string.Format("{0:" + cbbDate.Text + "}", dtpDate.Value)
                    + " " + string.Format("{0:" + cbbTime.Text + "}", dtpTime.Value);
            }

            if (vText != "")
            {
                SetDeItemValue(vText);
                this.Close();
            }
        }

        private void dgvDomain_DoubleClick(object sender, EventArgs e)
        {
            btnDomainOk_Click(sender, e);
        }

        private void frmRecordPop_Deactivate(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmRecordPop_Leave(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmRecordPop_Shown(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void btnCE_Click(object sender, EventArgs e)
        {
            tbxValue.Text = "";
            FNum1 = 0;
            FSmark = "";
            FNum2 = 0;
            FTemplate = false;
            SetValueFocus();
        }

        private void btnC_Click(object sender, EventArgs e)
        {
            string vS = tbxValue.Text;
            if (FFlag)
                tbxValue.Text = "";
            else
            {
                vS = vS.Substring(0, vS.Length - 1);
                tbxValue.Text = vS;
            }
            SetValueFocus();
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            PutCalcNumber(int.Parse((sender as Button).Tag.ToString()));
        }

        private void btnDot_Click(object sender, EventArgs e)
        {
            tbxValue.Text = tbxValue.Text + ".";
            SetValueFocus();
        }

        private double Equal(double a, string m, double b)
        {
            double r = 0;

            if (m == "+")
                r = a + b;
            else
                if (m == "-")
                    r = a - b;
                else
                    if (m == "*")
                        r = a * b;
                    else
                        if (m == "/")
                            r = a / b;

            return r;
        }

        private void btnResult_Click(object sender, EventArgs e)
        {
            if (FFlag)
                tbxValue.Text = Equal(double.Parse(tbxValue.Text), FSmark, FNum2).ToString();
            else
            {
                if (FSmark == "")
                {

                }
                else
                {
                    FSnum2 = tbxValue.Text;
                    if (FSnum2 != "")
                        FNum2 = double.Parse(tbxValue.Text);
                    else
                        FNum2 = FNum1;

                    if ((FSmark == "/") && (FNum2 == 0))
                    {
                        FNum1 = 0;
                        FSmark = "";
                        FNum2 = 0;
                    }
                    else
                        tbxValue.Text = Equal(FNum1, FSmark, FNum2).ToString();

                    FFlag = true;
                    FSign = false;
                    FTemp = true;
                }
            }

            SetValueFocus();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (FSign)
            {
                FSnum2 = tbxValue.Text;
                if (FSnum2 != "")
                    FNum2 = double.Parse(tbxValue.Text);

                if ((FSmark == "/") && (FNum2 == 0))
                {
                    FNum1 = 0;
                    FSmark = "";
                    FNum2 = 0;
                }
                else
                    tbxValue.Text = Equal(FNum1, FSmark, FNum2).ToString();

                FFlag = true;
            }

            FSnum1 = tbxValue.Text;
            if (FSnum1 != "")
                FNum1 = double.Parse(FSnum1);
            else
                FNum1 = 0;

            FSmark = (sender as Button).Text;
            FFlag = false;
            FSign = true;
            FTemp = FSign;
            SetValueFocus();
        }

        private void btn35_Click(object sender, EventArgs e)
        {
            tbxValue.Text = (sender as Button).Tag.ToString();
            FTemp = false;
            FTemplate = true;
            FConCalcValue = true;
            SetValueFocus();
        }
    }
}
