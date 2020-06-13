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
using System.Windows.Forms;

namespace EMRView
{
    public partial class frmImportRecord : Form
    {
        private HCEmrView FEmrViewLite;
        private HCImportAsTextEventHandler FOnImportAsText;

        public frmImportRecord()
        {
            InitializeComponent();

            FEmrViewLite = new HCEmrView();
            this.pnlRecord.Controls.Add(FEmrViewLite);
            FEmrViewLite.Dock = DockStyle.Fill;
            FEmrViewLite.Show();
        }

        private void BtnImportSelect_Click(object sender, EventArgs e)
        {
            if (FOnImportAsText != null)
                FOnImportAsText(FEmrViewLite.ActiveSection.ActiveData.GetSelectText());
        }

        private void BtnImportAll_Click(object sender, EventArgs e)
        {
            if (FOnImportAsText != null)
                FOnImportAsText(FEmrViewLite.SaveToText());
        }

        public HCEmrView EmrView
        {
            get { return FEmrViewLite; }
        }

        public HCImportAsTextEventHandler OnImportAsText
        {
            get { return FOnImportAsText; }
            set { FOnImportAsText = value; }
        }
    }
}
