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
        private EmrView FEmrView;
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
            if (FEmrView == null)
            {
                FEmrView = new EmrView();
                this.Controls.Add(FEmrView);
                FEmrView.Parent = this.splitContainer1.Panel2;
                FEmrView.Dock = DockStyle.Fill;
                FEmrView.BringToFront();
            }

            if (emrDB == null)
                emrDB = new emrDB();

            LoadTemplate();
        }

        private void tvTemplate_DoubleClick(object sender, EventArgs e)
        {
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
                            FEmrView.LoadFromStream(stream);
                        else
                            FEmrView.Clear();
                    }
                    finally
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
                else
                    FEmrView.Clear();
            }
            else
                FOpenedTID = "";
        }

        private void tsbNew_Click(object sender, EventArgs e)
        {
            FEmrView.FileName = "";
            FEmrView.Clear();
        }

        private void tsbOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog vOpenDlg = new OpenFileDialog();
            try
            {
                vOpenDlg.Filter = "文件|*" + HC.View.HC.HC_EXT;
                if (vOpenDlg.ShowDialog() == DialogResult.OK)
                {
                    if (vOpenDlg.FileName != "")
                    {
                        Application.DoEvents();
                        FEmrView.LoadFromFile(vOpenDlg.FileName);
                    }
                }
            }
            finally
            {
                vOpenDlg.Dispose();
                GC.Collect();
            }
        }

        private void tsbUndo_Click(object sender, EventArgs e)
        {
            FEmrView.Undo();
        }

        private void tsbRedo_Click(object sender, EventArgs e)
        {
            FEmrView.Redo();
        }

        private void tsbSave_Click(object sender, EventArgs e)
        {
            HashSet<SectionArea> vParts = new HashSet<SectionArea> { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
            MemoryStream stream = new MemoryStream();
            try
            {
                FEmrView.SaveToStream(stream, false, vParts);
                emrDB.SaveTemplateContent(FOpenedTID, stream);
            }
            finally
            {
                stream.Close();//关闭文件对象
                stream.Dispose();
            }
            
        }
    }
}
