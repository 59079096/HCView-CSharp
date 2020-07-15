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
using HC.View;

namespace EMRView
{
    public partial class frmParagraph : Form
    {
        public frmParagraph()
        {
            InitializeComponent();
        }

        private void btnSelectColor_Click(object sender, EventArgs e)
        {
            ColorDialog vDlg = new ColorDialog();
            //vDlg.SolidColorOnly = true;
            vDlg.Color = pnlBackColor.BackColor;
            vDlg.ShowDialog();
            pnlBackColor.BackColor = vDlg.Color;
        }

        public void SetView(HCView aHCView)
        {
            cbbSpaceMode.SelectedIndex = (byte)aHCView.Style.ParaStyles[aHCView.CurParaNo].LineSpaceMode;
            switch (aHCView.Style.ParaStyles[aHCView.CurParaNo].LineSpaceMode)
            {
                case ParaLineSpaceMode.plsFix:
                    tbxLineSpace.Text = string.Format("{0:0.#}", aHCView.Style.ParaStyles[aHCView.CurParaNo].LineSpace);
                    break;

                case ParaLineSpaceMode.plsMult:
                    tbxLineSpace.Text = string.Format("{0:0.#}", aHCView.Style.ParaStyles[aHCView.CurParaNo].LineSpace);
                    break;
            }
            cbbAlignHorz.SelectedIndex = (byte)aHCView.Style.ParaStyles[aHCView.CurParaNo].AlignHorz;
            cbbAlignVert.SelectedIndex = (byte)aHCView.Style.ParaStyles[aHCView.CurParaNo].AlignVert;
            pnlBackColor.BackColor = aHCView.Style.ParaStyles[aHCView.CurParaNo].BackColor;
            tbxFirstIndent.Text = string.Format("{0:0.#}", aHCView.Style.ParaStyles[aHCView.CurParaNo].FirstIndent);
            tbxLeftIndent.Text = string.Format("{0:0.#}", aHCView.Style.ParaStyles[aHCView.CurParaNo].LeftIndent);
            tbxRightIndent.Text = string.Format("{0:0.#}", aHCView.Style.ParaStyles[aHCView.CurParaNo].RightIndent);
            cbxBreakRough.Checked = aHCView.Style.ParaStyles[aHCView.CurParaNo].BreakRough;

            this.ShowDialog();
            if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                aHCView.BeginUpdate();
                try
                {
                    Single vFloat = 12;
                    if (cbbSpaceMode.SelectedIndex > 4)
                    {
                        if (Single.TryParse(tbxLineSpace.Text, out vFloat))
                            aHCView.ApplyParaLineSpace((ParaLineSpaceMode)cbbSpaceMode.SelectedIndex, vFloat);
                    }
                    else
                        aHCView.ApplyParaLineSpace((ParaLineSpaceMode)cbbSpaceMode.SelectedIndex, vFloat);

                    aHCView.ApplyParaAlignHorz((ParaAlignHorz)cbbAlignHorz.SelectedIndex);
                    aHCView.ApplyParaAlignVert((ParaAlignVert)cbbAlignVert.SelectedIndex);
                    aHCView.ApplyParaBackColor(pnlBackColor.BackColor);
                    if (Single.TryParse(tbxFirstIndent.Text, out vFloat))
                        aHCView.ApplyParaFirstIndent(vFloat);
                    else
                        aHCView.ApplyParaFirstIndent(0);

                    if (Single.TryParse(tbxLeftIndent.Text, out vFloat))
                        aHCView.ApplyParaLeftIndent(vFloat);
                    else
                        aHCView.ApplyParaLeftIndent(0);

                    if (Single.TryParse(tbxRightIndent.Text, out vFloat))
                        aHCView.ApplyParaRightIndent(vFloat);
                    else
                        aHCView.ApplyParaRightIndent(0);

                    aHCView.ApplyParaBreakRough(cbxBreakRough.Checked);
                }
                finally
                {
                    aHCView.EndUpdate();
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void cbbSpaceMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbSpaceMode.SelectedIndex == 5)  // 固定值
            {
                tbxLineSpace.Text = "12";
                tbxLineSpace.Visible = true;
                lblUnit.Text = "磅";
                lblUnit.Visible = true;
            }
            else
            if (cbbSpaceMode.SelectedIndex == 6)  // 多倍
            {
                tbxLineSpace.Text = "3";
                tbxLineSpace.Visible = true;
                lblUnit.Text = "倍";
                lblUnit.Visible = true;
            }
            else
            {
                tbxLineSpace.Text = "";
                tbxLineSpace.Visible = false;
                lblUnit.Visible = false;
            }
        }
    }
}
