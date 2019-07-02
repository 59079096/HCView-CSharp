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
            cbbAlignHorz.SelectedIndex = (byte)aHCView.Style.ParaStyles[aHCView.CurParaNo].AlignHorz;
            cbbAlignVert.SelectedIndex = (byte)aHCView.Style.ParaStyles[aHCView.CurParaNo].AlignVert;
            pnlBackColor.BackColor = aHCView.Style.ParaStyles[aHCView.CurParaNo].BackColor;

            this.ShowDialog();
            if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                aHCView.BeginUpdate();
                try
                {
                    aHCView.ApplyParaLineSpace((ParaLineSpaceMode)cbbSpaceMode.SelectedIndex);
                    aHCView.ApplyParaAlignHorz((ParaAlignHorz)cbbAlignHorz.SelectedIndex);
                    aHCView.ApplyParaAlignVert((ParaAlignVert)cbbAlignVert.SelectedIndex);
                    aHCView.ApplyParaBackColor(pnlBackColor.BackColor);
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
    }
}
