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
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EMRView
{
    public partial class frmItemContent : Form
    {
        private int FDomainItemID = 0;
        private HCEmrEdit FEmrEdit;
        private int FMouseDownTick;
        private frmRecordPop FfrmRecordPop;
        private DeItemSetTextEventHandler FOnSetDeItemText = null;
        private frmDataElement frmDataElement;

        private void DoSaveItemContent()
        {
            using (MemoryStream vSM = new MemoryStream())
            {
                FEmrEdit.SaveToStream(vSM);

                EMRView.emrMSDB.ExecCommandEventHanler vEvent = delegate(SqlCommand sqlComm)
                {
                    sqlComm.Parameters.AddWithValue("DItemID", FDomainItemID);
                    sqlComm.Parameters.AddWithValue("content", vSM.GetBuffer());
                };
                
                if (emrMSDB.DB.ExecSql(emrMSDB.Sql_SaveDomainItemContent, vEvent))
                    MessageBox.Show("保存成功！");
                else
                    MessageBox.Show(emrMSDB.DB.ErrMsg);
            }
        }

        private void SetDomainItemID(int value)
        {
            FDomainItemID = value;

            DataTable dt = emrMSDB.DB.GetData(string.Format(emrMSDB.Sql_GetDomainItemContent, FDomainItemID));
            if (dt.Rows.Count > 0)
            {
                using (MemoryStream vSM = new MemoryStream((byte[])dt.Rows[0]["content"]))
                {
                    if (vSM.Length > 0)
                        FEmrEdit.LoadFromStream(vSM);
                    else
                        FEmrEdit.Clear();
                }
            }
        }

        /// <summary> 设置当前数据元的文本内容 </summary>
        private void DoSetActiveDeItemText(DeItem aDeItem, string aText, ref bool aCancel)
        {
            if (FOnSetDeItemText != null)
            {
                string vText = aText;
                FOnSetDeItemText(this, aDeItem, ref vText, ref aCancel);
                if (!aCancel)
                    FEmrEdit.SetActiveItemText(vText);
            }
            else
                FEmrEdit.SetActiveItemText(aText);
        }

        /// <summary> 设置当前数据元的内容为扩展内容 </summary>
        private void DoSetActiveDeItemExtra(DeItem aDeItem, Stream aStream)
        {
            FEmrEdit.SetActiveItemExtra(aStream);
        }

        private frmRecordPop PopupForm()
        {
            //if (FfrmRecordPop == null)
            {
                FfrmRecordPop = new frmRecordPop();
                FfrmRecordPop.OnSetActiveItemText = DoSetActiveDeItemText;
                FfrmRecordPop.OnSetActiveItemExtra = DoSetActiveDeItemExtra;
            }

            return FfrmRecordPop;
        }

        private void PopupFormClose()
        {
            if ((FfrmRecordPop != null) && FfrmRecordPop.Visible)
                FfrmRecordPop.Close();
        }

        private void DoEmrEditMouseDown(object sender, MouseEventArgs e)
        {
            PopupFormClose();
            FMouseDownTick = Environment.TickCount;
        }

        private void DoEmrEditMouseUp(object sender, MouseEventArgs e)
        {
            string vInfo = "";

            HCCustomItem vActiveItem = FEmrEdit.Data.GetTopLevelItem();
            if (vActiveItem != null)
            {
                if (FEmrEdit.Data.ActiveDomain.BeginNo >= 0)
                {
                    DeGroup vDeGroup = FEmrEdit.Data.Items[FEmrEdit.Data.ActiveDomain.BeginNo] as DeGroup;

                    vInfo = vDeGroup[DeProp.Name];
                }

                if (vActiveItem is DeItem)
                {
                    DeItem vDeItem = vActiveItem as DeItem;
                    if (vDeItem.StyleEx != StyleExtra.cseNone)
                        vInfo += "-" + vDeItem.GetHint();
                    else
                    if (vDeItem.Active
                            && (vDeItem[DeProp.Index] != "")
                            && (!vDeItem.IsSelectComplate)
                            && (!vDeItem.IsSelectPart)
                            && (Environment.TickCount - FMouseDownTick < 500)
                        )
                    {
                        vInfo = vInfo + "元素(" + vDeItem[DeProp.Index] + ")";

                        if (FEmrEdit.Data.ReadOnly)
                        {
                            //tssDeInfo.Text = "";
                            return;
                        }

                        POINT vPt = FEmrEdit.Data.GetTopLevelDrawItemCoord();  // 得到相对EmrEdit的坐标
                        HCCustomDrawItem vActiveDrawItem = FEmrEdit.Data.GetTopLevelDrawItem();
                        RECT vDrawItemRect = vActiveDrawItem.Rect;
                        vDrawItemRect = HC.View.HC.Bounds(vPt.X, vPt.Y, vDrawItemRect.Width, vDrawItemRect.Height);

                        if (HC.View.HC.PtInRect(vDrawItemRect, new POINT(e.X, e.Y)))
                        {
                            vPt.Y = vPt.Y + vActiveDrawItem.Height;

                            Point vPoint = new Point(vPt.X, vPt.Y);
                            vPoint = FEmrEdit.PointToScreen(vPoint);

                            //HC.Win32.User.ClientToScreen(FEmrEdit.Handle, ref vPt);
                            vPt.X = vPoint.X;
                            vPt.Y = vPoint.Y;
                            PopupForm().PopupDeItem(vDeItem, vPt);
                        }
                    }
                }
                else
                if (vActiveItem is DeEdit)
                {

                }
                else
                if (vActiveItem is DeCombobox)
                {

                }
                else
                if (vActiveItem is DeDateTimePicker)
                {

                }
            }

            //tssDeInfo.Text = vInfo;
        }

        private void DoDEInsertAsDeItem(object sender, EventArgs e)
        {
            DeItem vDeItem = FEmrEdit.NewDeItem(frmDataElement.GetDeName());
            vDeItem[DeProp.Index] = frmDataElement.GetDeIndex();
            vDeItem[DeProp.Name] = frmDataElement.GetDeName();
            FEmrEdit.InsertDeItem(vDeItem);
        }

        private void DoDEInsertAsDeGroup(object sender, EventArgs e)
        {
            DeGroup vDeGroup = new DeGroup(FEmrEdit.TopLevelData());
            vDeGroup[DeProp.Index] = frmDataElement.GetDeIndex();
            vDeGroup[DeProp.Name] = frmDataElement.GetDeName();
            FEmrEdit.InsertDeGroup(vDeGroup);
        }

        private void DoDEInsertAsDeEdit(object sender, EventArgs e)
        {
            DeEdit vDeEdit = new DeEdit(FEmrEdit.TopLevelData(), frmDataElement.GetDeName());
            vDeEdit[DeProp.Index] = frmDataElement.GetDeIndex();
            vDeEdit[DeProp.Name] = frmDataElement.GetDeName();
            FEmrEdit.InsertItem(vDeEdit);
        }

        private void DoDEInsertAsDeCombobox(object sender, EventArgs e)
        {
            DeCombobox vDeCombobox = new DeCombobox(FEmrEdit.TopLevelData(), frmDataElement.GetDeName());
            vDeCombobox.SaveItem = false;
            vDeCombobox[DeProp.Index] = frmDataElement.GetDeIndex();
            vDeCombobox[DeProp.Name] = frmDataElement.GetDeName();
            FEmrEdit.InsertItem(vDeCombobox);
        }

        private void DoDEInsertAsDeDateTime(object sender, EventArgs e)
        {
            DeDateTimePicker vDeDateTime = new DeDateTimePicker(FEmrEdit.TopLevelData(), DateTime.Now);
            vDeDateTime[DeProp.Index] = frmDataElement.GetDeIndex();
            vDeDateTime[DeProp.Name] = frmDataElement.GetDeName();
            FEmrEdit.InsertItem(vDeDateTime);
        }

        private void DoDEInsertAsDeRadioGroup(object sender, EventArgs e)
        {
            DeRadioGroup vDeRadioGropu = new DeRadioGroup(FEmrEdit.TopLevelData());
            vDeRadioGropu[DeProp.Index] = frmDataElement.GetDeIndex();
            vDeRadioGropu[DeProp.Name] = frmDataElement.GetDeName();
            // 取数据元的选项，选项太多时提示是否都插入
            vDeRadioGropu.AddItem("选项1");
            vDeRadioGropu.AddItem("选项2");
            vDeRadioGropu.AddItem("选项3");

            FEmrEdit.InsertItem(vDeRadioGropu);
        }

        private void DoDEInsertAsDeCheckBox(object sender, EventArgs e)
        {
            DeCheckBox vDeCheckBox = new DeCheckBox(FEmrEdit.TopLevelData(), frmDataElement.GetDeName(), false);
            vDeCheckBox[DeProp.Index] = frmDataElement.GetDeIndex();
            vDeCheckBox[DeProp.Name] = frmDataElement.GetDeName();
            FEmrEdit.InsertItem(vDeCheckBox);
        }

        public frmItemContent()
        {
            HCTextItem.HCDefaultTextItemClass = typeof(DeItem);
            HCDomainItem.HCDefaultDomainItemClass = typeof(DeGroup);

            InitializeComponent();
            this.ShowInTaskbar = false;

            System.Drawing.Text.InstalledFontCollection fonts = new System.Drawing.Text.InstalledFontCollection();
            foreach (System.Drawing.FontFamily family in fonts.Families)
            {
                cbbFont.Items.Add(family.Name);
            }
            cbbFont.Text = "宋体";

            cbbFontSize.Text = "五号";

            if (FEmrEdit == null)
            {
                FEmrEdit = new HCEmrEdit();
                this.pnlEdit.Controls.Add(FEmrEdit);
                FEmrEdit.Dock = DockStyle.Fill;

                FEmrEdit.MouseDown += DoEmrEditMouseDown;
                FEmrEdit.MouseUp += DoEmrEditMouseUp;
                FEmrEdit.Show();
            }

            FDomainItemID = 0;

            frmDataElement = new frmDataElement();
            frmDataElement.FormBorderStyle = FormBorderStyle.None;
            frmDataElement.Dock = DockStyle.Fill;
            frmDataElement.TopLevel = false;

            frmDataElement.OnInsertAsDeItem = DoDEInsertAsDeItem;
            frmDataElement.OnInsertAsDeGroup = DoDEInsertAsDeGroup;
            frmDataElement.OnInsertAsDeEdit = DoDEInsertAsDeEdit;
            frmDataElement.OnInsertAsDeCombobox = DoDEInsertAsDeCombobox;
            frmDataElement.OnInsertAsDeDateTime = DoDEInsertAsDeDateTime;
            frmDataElement.OnInsertAsDeRadioGroup = DoDEInsertAsDeRadioGroup;
            frmDataElement.OnInsertAsDeCheckBox = DoDEInsertAsDeCheckBox;

            splitContainer.Panel1.Controls.Add(frmDataElement);
            frmDataElement.Show();
        }

        public int DomainItemID
        {
            get { return FDomainItemID; }
            set { SetDomainItemID(value); }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DoSaveItemContent();
        }

        private void cbbFont_DropDownClosed(object sender, EventArgs e)
        {
            FEmrEdit.ApplyTextFontName(cbbFont.Text);
        }

        private void cbbFontSize_DropDownClosed(object sender, EventArgs e)
        {
            FEmrEdit.ApplyTextFontSize(HC.View.HC.GetFontSize(cbbFontSize.Text));
        }

        private void btnBold_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripButton).Tag.ToString())
            {
                case "0":
                    FEmrEdit.ApplyTextStyle(HCFontStyle.tsBold);
                    break;

                case "1":
                    FEmrEdit.ApplyTextStyle(HCFontStyle.tsItalic);
                    break;

                case "2":
                    FEmrEdit.ApplyTextStyle(HCFontStyle.tsUnderline);
                    break;

                case "3":
                    FEmrEdit.ApplyTextStyle(HCFontStyle.tsStrikeOut);
                    break;

                case "4":
                    FEmrEdit.ApplyTextStyle(HCFontStyle.tsSuperscript);
                    break;

                case "5":
                    FEmrEdit.ApplyTextStyle(HCFontStyle.tsSubscript);
                    break;
            }
        }

        private void btnRightIndent_Click(object sender, EventArgs e)
        {
            switch ((sender as ToolStripButton).Tag.ToString())
            {
                case "0":
                    FEmrEdit.ApplyParaAlignHorz(ParaAlignHorz.pahLeft);
                    break;

                case "1":
                    FEmrEdit.ApplyParaAlignHorz(ParaAlignHorz.pahCenter);
                    break;

                case "2":
                    FEmrEdit.ApplyParaAlignHorz(ParaAlignHorz.pahRight);
                    break;

                case "3":
                    FEmrEdit.ApplyParaAlignHorz(ParaAlignHorz.pahJustify);  // 两端
                    break;

                case "4":
                    FEmrEdit.ApplyParaAlignHorz(ParaAlignHorz.pahScatter);  // 分散
                    break;
            }
        }
    }
}
