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
    public partial class frmScriptIDE : Form
    {
        private EventHandler FOnSave;  // 保存脚本
        private EventHandler FOnCompile;  // 编译脚本

        public frmScriptIDE()
        {
            InitializeComponent();
            tbxEditor.Font = new System.Drawing.Font("Courier New", 10);
            //textEditorControl = new TextEditor();
            //splitContainer1.Panel1.Controls.Add(textEditorHost);
            //textEditorHost.BringToFront();
        }

        public void ClearScript()
        {
            lblMsg.Text = "Message";
            tbxEditor.Text = "";
            lstMessage.Items.Clear();
        }

        public string Script
        {
            get { return tbxEditor.Text.Trim(); }
            set { tbxEditor.Text = value; }
        }

        public void ClearDebugInfo()
        {
            lstMessage.Items.Clear();
        }

        public void SetDebugCaption(string text)
        {
            lblMsg.Text = text;
        }

        public void AddError(string error)
        {
            lstMessage.Items.Add(error);
        }

        public EventHandler OnSave
        {
            get { return FOnSave; }
            set { FOnSave = value; }
        }

        public EventHandler OnCompile
        {
            get { return FOnCompile; }
            set { FOnCompile = value; }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (FOnSave != null)
                FOnSave(sender, e);
        }

        private void BtnCompile_Click(object sender, EventArgs e)
        {
            if (FOnCompile != null)
                FOnCompile(sender, e);
        }
    }
}
