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
using HC.Win32;

namespace EMRView
{
    public partial class frmPageSet : Form
    {
        private PaperInfos FPaperInfos;

        private void cbxPaper_DropDownClosed(object sender, EventArgs e)
        {
            int vIndex = GetPaperInfoIndexByName(cbxPaper.Text);
            if (vIndex > 0)  // 标准纸张大小
            {
                tbxWidth.Text = String.Format("{0:0.#}", FPaperInfos[vIndex].Width);
                tbxHeight.Text = String.Format("{0:0.#}", FPaperInfos[vIndex].Height);
                tbxWidth.ReadOnly = true;
                tbxHeight.ReadOnly = true;
            }
            else  // 自定义纸张大小
            {
                tbxWidth.ReadOnly = false;
                tbxHeight.ReadOnly = false;
            }
        }

        private int GetPaperInfoIndexByName(string aName)
        {
            for (int i = 0; i < FPaperInfos.Count; i++)
            {
                if (FPaperInfos[i].SizeName == aName)
                    return i;
            }

            return -1;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        public frmPageSet()
        {
            InitializeComponent();

            FPaperInfos = new PaperInfos();
            FPaperInfos.Append(GDI.DMPAPER_USER, "自定义", 210, 297);
            FPaperInfos.Append(GDI.DMPAPER_A3, "A3", 297, 420);
            FPaperInfos.Append(GDI.DMPAPER_A4, "A4", 210, 297);
            FPaperInfos.Append(GDI.DMPAPER_A5, "A5", 148, 210);
            FPaperInfos.Append(GDI.DMPAPER_B5, "B5", 182, 257);
            FPaperInfos.Append(HC.View.HC.DMPAPER_HC_16K, "16K", 195, 271);

            for (int i = 0; i < FPaperInfos.Count; i++)
                cbxPaper.Items.Add(FPaperInfos[i].SizeName);
        }

        public void SetHCView(HCView aHCView)
        {
            cbxPaper.SelectedIndex = cbxPaper.Items.IndexOf(HC.View.HC.GetPaperSizeStr((int)aHCView.ActiveSection.PaperSize));
            if (cbxPaper.SelectedIndex < 0)
                cbxPaper.SelectedIndex = 0;

            tbxWidth.ReadOnly = cbxPaper.SelectedIndex > 0;
            tbxHeight.ReadOnly = cbxPaper.SelectedIndex > 0;

            if (aHCView.ActiveSection.PaperOrientation == PaperOrientation.cpoPortrait)
            {
                cbxPaperOrientation.SelectedIndex = 0;
                tbxWidth.Text = string.Format("{0:0.#}", aHCView.ActiveSection.PaperWidth);
                tbxHeight.Text = string.Format("{0:0.#}", aHCView.ActiveSection.PaperHeight);
            }
            else
            {
                cbxPaperOrientation.SelectedIndex = 1;
                tbxWidth.Text = string.Format("{0:0.#}", aHCView.ActiveSection.PaperHeight);
                tbxHeight.Text = string.Format("{0:0.#}", aHCView.ActiveSection.PaperWidth);
            }

            tbxTop.Text = string.Format("{0:0.#}", aHCView.ActiveSection.PaperMarginTop);
            tbxLeft.Text = string.Format("{0:0.#}", aHCView.ActiveSection.PaperMarginLeft);
            tbxRight.Text = string.Format("{0:0.#}", aHCView.ActiveSection.PaperMarginRight);
            tbxBottom.Text = string.Format("{0:0.#}", aHCView.ActiveSection.PaperMarginBottom);

            cbxSymmetryMargin.Checked = aHCView.ActiveSection.SymmetryMargin;

            cbxPageNoVisible.Checked = aHCView.ActiveSection.PageNoVisible;
            cbxParaLastMark.Checked = aHCView.Style.ShowParaLastMark;
            cbxShowLineNo.Checked = aHCView.ShowLineNo;
            cbxShowLineActiveMark.Checked = aHCView.ShowLineActiveMark;
            cbxShowUnderLine.Checked = aHCView.ShowUnderLine;

            this.ShowDialog();
            if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                aHCView.BeginUpdate();
                try
                {
                    int vIndex = GetPaperInfoIndexByName(cbxPaper.Text);
                    if (vIndex == 0)  // 自定义尺寸
                        aHCView.ActiveSection.PaperSize = System.Drawing.Printing.PaperKind.Custom;
                    else
                        aHCView.ActiveSection.PaperSize = (System.Drawing.Printing.PaperKind)FPaperInfos[vIndex].Size;

                    if (cbxPaperOrientation.SelectedIndex == 0)  // 纵向
                        aHCView.ActiveSection.PaperOrientation = PaperOrientation.cpoPortrait;
                    else
                        aHCView.ActiveSection.PaperOrientation = PaperOrientation.cpoLandscape;

                    if (vIndex == 0)  // 自定义
                    {
                        if (cbxPaperOrientation.SelectedIndex == 0)  // 纵向
                        {
                            aHCView.ActiveSection.PaperWidth = float.Parse(tbxWidth.Text);
                            aHCView.ActiveSection.PaperHeight = float.Parse(tbxHeight.Text);
                        }
                        else  // 横向
                        {
                            aHCView.ActiveSection.PaperWidth = float.Parse(tbxHeight.Text);
                            aHCView.ActiveSection.PaperHeight = float.Parse(tbxWidth.Text);
                        }
                    }
                    else
                    {
                        if (cbxPaperOrientation.SelectedIndex == 0)  // 纵向
                        {
                            aHCView.ActiveSection.PaperWidth = FPaperInfos[vIndex].Width;
                            aHCView.ActiveSection.PaperHeight = FPaperInfos[vIndex].Height;
                        }
                        else  // 横向
                        {
                            aHCView.ActiveSection.PaperWidth = FPaperInfos[vIndex].Height;
                            aHCView.ActiveSection.PaperHeight = FPaperInfos[vIndex].Width;
                        }
                    }

                    aHCView.ActiveSection.PaperMarginTop = float.Parse(tbxTop.Text);
                    aHCView.ActiveSection.PaperMarginLeft = float.Parse(tbxLeft.Text);
                    aHCView.ActiveSection.PaperMarginRight = float.Parse(tbxRight.Text);
                    aHCView.ActiveSection.PaperMarginBottom = float.Parse(tbxBottom.Text);

                    aHCView.ActiveSection.SymmetryMargin = cbxSymmetryMargin.Checked;

                    aHCView.ActiveSection.PageNoVisible = cbxPageNoVisible.Checked;
                    aHCView.Style.ShowParaLastMark = cbxParaLastMark.Checked;
                    aHCView.ShowLineNo = cbxShowLineNo.Checked;
                    aHCView.ShowLineActiveMark = cbxShowLineActiveMark.Checked;
                    aHCView.ShowUnderLine = cbxShowUnderLine.Checked;
                    aHCView.ResetActiveSectionMargin();
                }
                finally
                {
                    aHCView.EndUpdate();
                }
            }
        }
    }

    class PaperInfo
    {
        public int Size;
        public string SizeName;
        public float Width, Height;
    }

    class PaperInfos : List<PaperInfo>
    {
        public void Append(int aSize, string aSizeName, float aWidth, float aHeight)
        {
            PaperInfo vPaperInfo = new PaperInfo();
            vPaperInfo.Size = aSize;
            vPaperInfo.SizeName = aSizeName;
            vPaperInfo.Width = aWidth;
            vPaperInfo.Height = aHeight;

            this.Add(vPaperInfo);
        }
    }
}
