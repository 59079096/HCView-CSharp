using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HCViewDemo
{
    public partial class frmInsertTable : Form
    {
        public frmInsertTable()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int vRowCount = 0, vColCount = 0;
            if (!int.TryParse(txbRow.Text, out vRowCount))
                MessageBox.Show("请输入正确的行数！");
            else
            if (!int.TryParse(txbCol.Text, out vColCount))
                MessageBox.Show("请输入正确的列数！");
            else
            if (vRowCount < 1)
                MessageBox.Show("行数至少为1！");
            else
            if (vRowCount > 256)
                MessageBox.Show("行数不能超过256行！");
            else
            if (vColCount < 1)
                MessageBox.Show("列数至少为1！");
            else
            if (vColCount > 32)
                MessageBox.Show("列数不能超过32列！");
            else
                this.DialogResult = DialogResult.OK;
        }
    }
}
