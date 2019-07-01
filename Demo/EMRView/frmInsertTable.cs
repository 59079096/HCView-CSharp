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

namespace EMRView
{
    public partial class frmInsertTable : Form
    {
        private int FRowCount, FColCount;

        public frmInsertTable()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            FRowCount = -1;
            FColCount = -1;
            int.TryParse(tbxRow.Text, out FRowCount);
            int.TryParse(tbxCol.Text, out FColCount);

            if ((FRowCount > 0) && (FColCount > 0))
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            else
                MessageBox.Show("请填写正确的行、列数量！");
        }

        public int RowCount
        {
            get { return FRowCount; }
        }

        public int ColCount
        {
            get { return FColCount; }
        }
    }
}
