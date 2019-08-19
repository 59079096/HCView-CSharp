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
        private frmRecord FfrmRecord;
        private ImportEventHandler FOnImport;

        public frmImportRecord()
        {
            InitializeComponent();

            FfrmRecord = new frmRecord();
            FfrmRecord.EditToolVisible = false;
            FfrmRecord.TopLevel = false;
            this.pnlRecord.Controls.Add(FfrmRecord);
            FfrmRecord.Dock = DockStyle.Fill;
            FfrmRecord.Show();
        }

        private void BtnImportSelect_Click(object sender, EventArgs e)
        {
            if (FOnImport != null)
            {
                string vText = FfrmRecord.EmrView.ActiveSection.ActiveData.GetSelectText();
                FOnImport(vText);
            }
        }

        private void BtnImportAll_Click(object sender, EventArgs e)
        {
            if (FOnImport != null)
            {
                string vText = FfrmRecord.EmrView.SaveToText();
                FOnImport(vText);
            }
        }

        public HCEmrView EmrView
        {
            get { return FfrmRecord.EmrView; }
        }

        public ImportEventHandler OnImport
        {
            get { return FOnImport; }
            set { FOnImport = value; }
        }
    }
}
