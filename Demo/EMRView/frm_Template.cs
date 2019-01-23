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
    public partial class frm_Template : Form
    {
        private EmrView FEmrView;

        public frm_Template()
        {
            InitializeComponent();
        }

        private void frm_Template_Load(object sender, EventArgs e)
        {
            if (FEmrView == null)
            {
                FEmrView = new EmrView();
                this.Controls.Add(FEmrView);
                FEmrView.Parent = this.splitContainer1.Panel2;
                FEmrView.Dock = DockStyle.Fill;
                FEmrView.BringToFront();
            }
        }
    }
}
