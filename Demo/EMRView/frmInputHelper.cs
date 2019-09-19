/*******************************************************}
{                                                       }
{         基于HCView的电子病历程序  作者：荆通          }
{                                                       }
{ 此代码仅做学习交流使用，不可用于商业目的，由此引发的  }
{ 后果请使用者承担，加入QQ群 649023932 来获取更多的技术 }
{ 交流。                                                }
{                                                       }
{*******************************************************/
using HC.Win32;
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
    public partial class frmInputHelper : Form
    {
        private bool FEnableEx = false;
        private bool FSizeChange = false;
        private bool FActiveEx = false;
        private string FCompStr = "";
        private string FBeforText = "";
        private string FAfterText = "";
        private Color FBorderColor;

        private void UpdateView()
        {
            if (this.Visible)
            {
                RECT vRect = ClientRect();
                User.InvalidateRect(Handle, ref vRect, 0);
            }
        }

        public frmInputHelper()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            User.MoveWindow(Handle, Left, Top, 400, 24, 0);
            this.TopMost = true;
            FSizeChange = false;
            FEnableEx = false;
            FActiveEx = false;
            FBorderColor = Color.FromArgb(0xB5, 0xC5, 0xD2);
            FCompStr = "";
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == User.WM_MOUSEACTIVATE)
            {
                m.Result = new IntPtr(User.MA_NOACTIVATE);
                return;
            }
            else 
            if (m.Msg == User.WM_NCACTIVATE)
            {
                if (((int)m.WParam & 0xFFFF) != User.WA_INACTIVE)
                {
                    if (m.LParam != IntPtr.Zero)
                    {
                        User.SetActiveWindow(m.LParam);
                    }
                    else
                    {
                        User.SetActiveWindow(IntPtr.Zero);
                    }
                }
            }

            base.WndProc(ref m);
        }

        public RECT ClientRect()
        {
            return new RECT(0, 0, Width, Height);
        }

        public string GetCandiText(uint aIndex)
        {
            return "第" + (aIndex + 1).ToString() + "项";
        }

        public void SetCompositionString(string s)
        {
            FSizeChange = false;

            if (FCompStr != s)
            {
                FCompStr = s;
                // TODO : 新的输入，重新匹配知识词条 

                if (FCompStr != "")  // 有知识
                {
                    if (!Visible)
                        ShowEx();
                    else
                        UpdateView();
                }
                else  // 无知识
                    CloseEx();
            }
        }

        public void SetCaretString(string aBeforText, string aAfterText)
        {
            FBeforText = aBeforText;
            FAfterText = aAfterText;
            UpdateView();
        }

        public void CompWndMove(IntPtr aHandle, int aCaretX, int aCaretY)
        {
            POINT vPt = new POINT(aCaretX, aCaretY);
            User.ClientToScreen(aHandle, ref vPt);

            this.Location = new Point(vPt.X + 2, vPt.Y + 2);
            if (FCompStr != "")
            {
                FActiveEx = false;
                ShowEx();
            }
        }

        public bool ResetImeCompRect(ref POINT aImePosition)
        {
            bool vResult = false;

            if (true)
            {
                aImePosition.Y += Height + 2;
                vResult = true;
            }

            return vResult;
        }

        public void ShowEx()
        {
            FActiveEx = false;

            if (!Visible)
                User.ShowWindow(Handle, User.SW_SHOWNOACTIVATE);

            //User.MoveWindow(Handle, FLeft, FTop, FWidth, FHeight, 0);
        }

        public void CloseEx()
        {
            User.ShowWindow(Handle, User.SW_HIDE);
        }

        public bool EnableEx
        {
            get { return FEnableEx; }
            set { FEnableEx = value; }
        }

        public bool ChangeSize
        {
            get { return FSizeChange; }
            set { FSizeChange = value; }
        }

        public bool ActiveEx
        {
            get { return FActiveEx; }
            set { SetActiveEx(value); }
        }

        private void SetActiveEx(bool value)
        {
            if (FActiveEx != value)
            {
                FActiveEx = value;
                UpdateView();
            }
        }

        private void HCInputHelper_Paint(object sender, PaintEventArgs e)
        {
            SolidBrush brush = new SolidBrush(HC.View.HC.clBtnFace);
            if (!FActiveEx)
                brush.Color = Color.FromArgb(0xFE, 0xFE, 0xFF);

            Rectangle vRect = e.ClipRectangle;
            
            e.Graphics.FillRectangle(brush, vRect);
            e.Graphics.DrawRectangle(new Pen(FBorderColor), new Rectangle(vRect.Left, vRect.Top, vRect.Right - 1, vRect.Bottom - 1));

            string vText = "";
            if (FCompStr != "")
                vText = "1.第1项 2.第2项 3.第3项 4.第4项 5.第5项";  // "[" + FBeforText +"]" + "[" + FAfterText + "]"
            else
                vText = "你好，我正在学习相关知识^_^";

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Center;
            brush.Color = Color.Black;
            vRect.Inflate(-5, -5);
            e.Graphics.DrawString(vText, this.Font, brush, vRect, sf);
        }
    }
}
