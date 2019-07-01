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
    public partial class frmTemplateInfo : Form
    {
        public frmTemplateInfo()
        {
            InitializeComponent();
        }

        public string TemplateName
        {
            get { return tbxName.Text; }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
