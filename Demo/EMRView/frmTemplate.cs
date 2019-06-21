using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using HC.View;

namespace EMRView
{
    public partial class frmTemplate : Form
    {
        private frmRecord frmRecord;
        private emrDB emrDB;
        private string FOpenedTID;

        public frmTemplate()
        {
            InitializeComponent();
        }

        private void LoadTemplate()
        {
            tvTemplate.Nodes.Clear();
            DataTable dt = emrDB.ExecToDataTable(emrDB.Sql_GetTemplate);
            tvTemplate.BeginUpdate();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    TreeNode vNode = tvTemplate.Nodes.Add(dt.Rows[i]["tname"].ToString());
                    vNode.Tag = dt.Rows[i]["id"].ToString();
                }
            }
            finally
            {
                tvTemplate.EndUpdate();
            }
        }

        private void frm_Template_Load(object sender, EventArgs e)
        {
            if (emrDB == null)
                emrDB = new emrDB();

            LoadTemplate();
        }

        private void tvTemplate_DoubleClick(object sender, EventArgs e)
        {
            if (frmRecord == null)
            {
                frmRecord = new frmRecord();
                frmRecord.TopLevel = false;
                frmRecord.Dock = DockStyle.Fill;
                //this.splitContainer1.Panel2.Controls.Add(frmRecord);
                frmRecord.Parent = this.splitContainer1.Panel2;
                frmRecord.Show();
            }

            TreeNode vNode = tvTemplate.SelectedNode;

            if (vNode != null)
            {
                FOpenedTID = vNode.Tag.ToString();
                byte[] vFile = emrDB.GetTemplateContent(FOpenedTID);
                if (vFile != null)
                {
                    MemoryStream stream = new MemoryStream(vFile);
                    try
                    {
                        if (stream.Length > 0)
                            frmRecord.emrView.LoadFromStream(stream);
                        else
                            frmRecord.emrView.Clear();
                    }
                    finally
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
                else
                    frmRecord.emrView.Clear();
            }
            else
                FOpenedTID = "";
        }
    }
}
