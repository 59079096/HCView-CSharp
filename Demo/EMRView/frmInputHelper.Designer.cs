namespace EMRView
{
    partial class frmInputHelper
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // HCInputHelper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 20);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "HCInputHelper";
            this.ShowInTaskbar = false;
            this.Text = "HCInputHelper";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.HCInputHelper_Paint);
            this.ResumeLayout(false);

        }

        #endregion
    }
}