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
    public partial class frmMain : Form
    {
        private frm_Template FfrmTemplate;

        public frmMain()
        {
            InitializeComponent();
        }

        private void 模板制作ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FfrmTemplate == null)
                FfrmTemplate = new frm_Template();

            FfrmTemplate.ShowDialog();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            LoadPatientList();
        }

        private void LoadPatientList()
        {
            lvPatient.Items.Clear();

            ListViewItem lvi = lvPatient.Items.Add("张三");
            lvi.SubItems.Add("男");
            lvi.SubItems.Add("28岁");
            lvi.SubItems.Add("2019-1-20 12:04");
            lvi.SubItems.Add("骨一科");
            lvi.SubItems.Add("骨一科");
            lvi.SubItems.Add("2019-1-20 12:18");
        }
    }
}
