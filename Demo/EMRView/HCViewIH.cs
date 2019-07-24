using HC.View;
using HC.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EMRView
{
    public class HCViewIH : HCView
    {
        private HCInputHelper FInputHelper;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (FInputHelper.Enable && (e.Control) && (e.Shift) && (e.KeyCode == Keys.Space))
                FInputHelper.Show();
            else
               if (FInputHelper.Enable && (!e.Control) && (!e.Shift) && (e.KeyCode == Keys.Escape))
                FInputHelper.Close();
            else
                base.OnKeyDown(e);
        }

        protected virtual bool DoProcessIMECandi(string aCandi)
        {
            return true;
        }

        protected override void WndProc(ref Message Message)
        {
            if (FInputHelper.Enable)
            {
                switch (Message.WParam.ToInt32())
                {
                    case 0x000B:
                    case 0x0004:
                        FInputHelper.CompWndMove(this.Handle, Caret.X, Caret.Y + Caret.Height);
                        return;
                }
            }

            base.WndProc(ref Message);
        }

        private string GetCompositionStr(int aType)
        {
            string Result = "";
            if (this.hImc != IntPtr.Zero)
            {
                int vSize = Imm.ImmGetCompositionString(this.hImc, aType, null, 0);
                if (vSize > 0)
                {
                    byte[] vBuffer = new byte[vSize];
                    Imm.ImmGetCompositionString(this.hImc, Imm.GCS_RESULTSTR, vBuffer, vSize);
                    Result = System.Text.Encoding.Default.GetString(vBuffer);
                    
                }
            }

            return Result;
        }

        protected override void UpdateImeComposition(int aLParam)
        {
            if (FInputHelper.Enable && ((aLParam & Imm.GCS_COMPSTR) != 0))
            {
                string vS = GetCompositionStr(Imm.GCS_COMPSTR);
                FInputHelper.SetCompositionString(vS);

                if (FInputHelper.Resize)
                {
                    COMPOSITIONFORM vCF = new COMPOSITIONFORM();
                    if (Imm.ImmGetCompositionWindow(this.hImc, ref vCF))
                    {
                        if (FInputHelper.ResetImeCompRect(ref vCF.ptCurrentPos))
                            Imm.ImmSetCompositionWindow(this.hImc, ref vCF);
                    }
                }
            }

            if ((aLParam & Imm.GCS_RESULTSTR) != 0)
            {
                string vS = GetCompositionStr(Imm.GCS_RESULTSTR);
                if (vS != "")
                {
                    if (DoProcessIMECandi(vS))
                        InsertText(vS);
                }
            }
        }

        protected override void UpdateImePosition()
        {
            COMPOSITIONFORM vCF = new COMPOSITIONFORM();
            vCF.ptCurrentPos.X = Caret.X;
            vCF.ptCurrentPos.Y = Caret.Y + Caret.Height + 4;

            if (FInputHelper.Enable)
                FInputHelper.ResetImeCompRect(ref vCF.ptCurrentPos);

            vCF.dwStyle = 0x0020;
            Rectangle vr = this.ClientRectangle;
            vCF.rcArea = new RECT(vr.Left, vr.Top, vr.Right, vr.Bottom);
            Imm.ImmSetCompositionWindow(this.hImc, ref vCF);

            if (FInputHelper.Enable)
                FInputHelper.CompWndMove(this.Handle, Caret.X, Caret.Y + Caret.Height);
        }

        protected override bool DoInsertText(string aText)
        {
            bool vResult = base.DoInsertText(aText);
            HCCustomData vTopData = this.ActiveSectionTopLevelData();
            return vResult;
        }

        public HCViewIH() : base()
        {
            FInputHelper = new HCInputHelper();
        }

        ~HCViewIH()
        {
            
        }
    }
}
