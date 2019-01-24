using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HC.Win32;

namespace EMRView
{
    public partial class frmRecordPop : Form
    {
        private EventHandler FOnActiveItemChange;

        public frmRecordPop()
        {
            InitializeComponent();
        }

        public void PopupDeItem(DeItem aDeItem, POINT aPopupPt)
        {

        }

        public EventHandler OnActiveItemChange
        {
            get { return FOnActiveItemChange; }
            set { FOnActiveItemChange = value; }
        }
    }
}
