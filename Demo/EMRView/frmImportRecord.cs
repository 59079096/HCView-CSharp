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
    public partial class frmImportRecord : Form
    {
        private HCEmrViewLite FEmrViewLite;
        private HCImportAsTextEventHandler FOnImportAsText;

        public frmImportRecord()
        {
            InitializeComponent();

            FEmrViewLite = new HCEmrViewLite();
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

        public HCEmrViewLite EmrView
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
