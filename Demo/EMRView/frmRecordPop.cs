/*******************************************************}
{                                                       }
{         基于HCView的电子病历程序  作者：荆通          }
{                                                       }
{ 此代码仅做学习交流使用，不可用于商业目的，由此引发的  }
{ 后果请使用者承担，加入QQ群 649023932 来获取更多的技术 }
{ 交流。                                                }
{                                                       }
{*******************************************************/
using HC.View;
using HC.Win32;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EMRView
{
    public delegate void TextEventHandler(DeItem aDeItem, string aText, ref bool aCancel);
    public delegate void StreamEventHandler(DeItem aDeItem, Stream aStream);
    public delegate void DeItemSetTextEventHandler(object sender, DeItem aDeItem, ref string aText, ref bool aCancel);

    public partial class frmRecordPop : Form
    {
        string FSnum1, FSnum2, FSmark, FOldUnit;
        double FNum1, FNum2;
        bool FFlag, FSign, FTemp, FTemplate, FConCalcValue;
        bool FMultSelect;
        string FFrmtp;
        DeItem FDeItem;
        DataTable FDBDomain = new DataTable();

        private TextEventHandler FOnSetActiveItemText;
        private StreamEventHandler FOnSetActiveItemExtra;

        private void SetDeItemValue(string value, ref bool aCancel)
        {
            if (FOnSetActiveItemText != null)
                FOnSetActiveItemText(FDeItem, value, ref aCancel);
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
                        FOnSetActiveItemExtra(FDeItem, vSM);
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

        private void SetMultSelect(bool value)
        {
            if (FMultSelect != value)
            {
                FMultSelect = value;
                if (FMultSelect)
                {
                    dgvDomain.Columns[0].Width = 20;
                    btnRM.Text = "单选";
                }
                else
                {
                    dgvDomain.Columns[0].Width = 0;
                    btnRM.Text = "多选";
                }
            }
        }

        protected bool MultSelect
        {
            get { return FMultSelect; }
            set { SetMultSelect(value); }
        }

        public frmRecordPop()
        {
            InitializeComponent();

            this.ShowInTaskbar = false;
            tabPop.SizeMode = TabSizeMode.Fixed;
            tabPop.ItemSize = new Size(0, 1);
            cbbDate.SelectedIndex = 3;
            cbbTime.SelectedIndex = 3;
            FMultSelect = false;
        }

        #region 子方法
        private void IniDomainUI()
        {
            dgvDomain.RowCount = FDBDomain.Rows.Count;

            for (int i = 0; i < FDBDomain.Rows.Count; i++)
            {
                dgvDomain.Rows[i].Cells[0].Value = false;
                dgvDomain.Rows[i].Cells[1].Value = FDBDomain.Rows[i]["devalue"];
                dgvDomain.Rows[i].Cells[2].Value = FDBDomain.Rows[i]["code"];
                dgvDomain.Rows[i].Cells[3].Value = FDBDomain.Rows[i]["id"];
                dgvDomain.Rows[i].Cells[4].Value = FDBDomain.Rows[i]["py"];
                dgvDomain.Rows[i].Cells[5].Value = "";

                if (FDBDomain.Rows[i]["content"].GetType() != typeof(System.DBNull))
                {
                    byte[] vbuffer = (byte[])FDBDomain.Rows[i]["content"];
                    if (vbuffer.Length > 0)
                        dgvDomain.Rows[i].Cells[5].Value = "...";
                }
            }
        }
        #endregion

        public void PopupDeCombobox(DeCombobox aDeCombobox)
        {
            aDeCombobox.Items.Clear();
            int vCMV = -1;

            DataTable dt = emrMSDB.DB.GetData(string.Format("SELECT DeCode, domainid "
                + "FROM Comm_DataElement WHERE DeID ={0}", aDeCombobox[DeProp.Index]));
            if (dt.Rows.Count > 0)
            {
                vCMV = int.Parse(dt.Rows[0]["domainid"].ToString());
                if (vCMV > 0)  // 有值域
                {
                    DataTable FDBDomain = emrMSDB.DB.GetData(string.Format("SELECT DE.Code, DE.devalue "
                        + "FROM Comm_DataElementDomain DE "
                        + "WHERE DE.domainid = {0}", vCMV));

                    if (FDBDomain.Rows.Count > 0)
                    {
                        for (int i = 0; i < FDBDomain.Rows.Count; i++)
                            aDeCombobox.Items.Add(new HCCbbItem(FDBDomain.Rows[i]["devalue"].ToString(), FDBDomain.Rows[i]["code"].ToString()));
                    }
                }
            }
        }

        public bool PopupDeItem(DeItem aDeItem, POINT aPopupPt)
        {
            FFrmtp = "";
            FDeItem = aDeItem;
            string vDeUnit = "";
            int vCMV = -1;

            DataTable dt = emrMSDB.DB.GetData(string.Format("SELECT DeCode, PY, frmtp, deunit, domainid "
                + "FROM Comm_DataElement WHERE DeID ={0}", FDeItem[DeProp.Index]));
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
                string[] vStrings = vDeUnit.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
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
                btnNow_Click(null, null);
                pnlDate.Visible = FFrmtp != DeFrmtp.Time;
                pnlTime.Visible = FFrmtp != DeFrmtp.Date;
            }
            else
            if ((FFrmtp == DeFrmtp.Radio) || (FFrmtp == DeFrmtp.Multiselect))  // 单、多选
            {
                MultSelect = FFrmtp == DeFrmtp.Multiselect;
                tbxSpliter.Clear();

                if (FDBDomain.Rows.Count > 0)
                    FDBDomain.Reset();

                dgvDomain.RowCount = 0;
                tabPop.SelectedIndex = 0;
                this.Width = 290;
                this.Height = 300;

                if (vCMV > 0)  // 有值域
                {
                    FDBDomain = emrMSDB.DB.GetData(string.Format("SELECT DE.ID, DE.Code, DE.devalue, DE.PY, DC.Content "
                        + "FROM Comm_DataElementDomain DE LEFT JOIN Comm_DomainContent DC ON DE.ID = DC.DItemID "
                        + "WHERE DE.domainid = {0}", vCMV));
                }

                if (FDBDomain.Rows.Count > 0)  // 有选项
                    IniDomainUI();
                else  // 没选项
                    return false;
            }
            else
            if (FFrmtp == DeFrmtp.String)
            {
                return false;  // 文本的不弹了，使用直接在元素上修改的方式
                tbxMemo.Clear();
                tabPop.SelectedIndex = 2;
                this.Width = 260;
                this.Height = 200;
            }
            else  // 不认识的类型不弹
                return false;

            System.Drawing.Rectangle vRect = Screen.GetWorkingArea(this);
            if (aPopupPt.X + Width > vRect.Right)
                aPopupPt.X = vRect.Right - Width;

            if (aPopupPt.Y + Height > vRect.Bottom)
                aPopupPt.Y = vRect.Bottom - Height;

            if (aPopupPt.X < vRect.Left)
                aPopupPt.X = vRect.Left;

            if (aPopupPt.Y < vRect.Top)
                aPopupPt.Y = vRect.Top;

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(aPopupPt.X, aPopupPt.Y);

            this.Show();
            //User.ShowWindow(this.Handle, User.SW_SHOWNOACTIVATE);

            if (FFrmtp == DeFrmtp.Number)
                tbxValue.Focus();

            return true;
        }

        public TextEventHandler OnSetActiveItemText
        {
            get { return FOnSetActiveItemText; }
            set { FOnSetActiveItemText = value; }
        }

        public StreamEventHandler OnSetActiveItemExtra
        {
            get { return FOnSetActiveItemExtra; }
            set { FOnSetActiveItemExtra = value; }
        }

        private void btnDomainOk_Click(object sender, EventArgs e)
        {
            if (dgvDomain.SelectedRows.Count > 0)
            {
                bool vCancel = false;
                if (FMultSelect)  // 多选
                {
                    string vS = "";
                    string vDID = "";
                    for (int i = 0; i < dgvDomain.RowCount; i++)
                    {
                        if (bool.Parse(dgvDomain.Rows[i].Cells[0].Value.ToString()))
                        {
                            vDID = vDID + dgvDomain.Rows[i].Cells[2].Value.ToString() + ",";
                            vS = vS + dgvDomain.Rows[i].Cells[1].Value.ToString() + "；";
                        }
                    }

                    if (vS != "")
                    {
                        vS = vS.Remove(vS.Length - 1, 1);
                        vDID = vDID.Remove(vDID.Length - 1, 1);
                        FDeItem[DeProp.CMVVCode] = vDID;
                        SetDeItemValue(vS, ref vCancel);
                    }
                }
                else  // 单选
                {
                    int vSelIndex = dgvDomain.SelectedRows[0].Index;
                    FDeItem[DeProp.CMVVCode] = dgvDomain.Rows[vSelIndex].Cells[2].Value.ToString();
                    if (dgvDomain.Rows[vSelIndex].Cells[5].Value.ToString() != "")
                        SetDeItemExtraValue(dgvDomain.Rows[vSelIndex].Cells[3].Value.ToString());
                    else
                        SetDeItemValue(dgvDomain.Rows[vSelIndex].Cells[1].Value.ToString(), ref vCancel);
                }

                if (!vCancel)
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

                bool vCancel = false;
                SetDeItemValue(vText, ref vCancel);

                if (!vCancel)
                    this.Close();
            }
        }

        private void btnMemoOk_Click(object sender, EventArgs e)
        {
            if (tbxMemo.Text != "")
            {
                bool vCancel = false;
                SetDeItemValue(tbxMemo.Text, ref vCancel);

                if (!vCancel)
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
                bool vCancel = false;
                SetDeItemValue(vText, ref vCancel);
                if (!vCancel)
                    this.Close();
            }
        }

        private void dgvDomain_DoubleClick(object sender, EventArgs e)
        {
            btnDomainOk_Click(sender, e);
        }

        private void frmRecordPop_Deactivate(object sender, EventArgs e)
        {
            //this.Close();
            User.ShowWindow(this.Handle, User.SW_HIDE);
        }

        private string ConversionValueByUnit(string aValue, string aOldUnit, string aNewUnit)
        {
            return aValue;
        }

        private void CbbUnit_DropDownClosed(object sender, EventArgs e)
        {
            if (tbxValue.Text.Trim() != "")
                tbxValue.Text = ConversionValueByUnit(tbxValue.Text, FOldUnit, cbbUnit.Text);  // 数据单位换算
        }

        private void CbbUnit_DropDown(object sender, EventArgs e)
        {
            FOldUnit = cbbUnit.Text;
        }

        private void FrmRecordPop_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                User.ShowWindow(this.Handle, User.SW_HIDE);
        }

        private void frmRecordPop_Load(object sender, EventArgs e)
        {
            tabPop.ImeMode = ImeMode.OnHalf;
        }

        private void btnRM_Click(object sender, EventArgs e)
        {
            MultSelect = !FMultSelect;
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
