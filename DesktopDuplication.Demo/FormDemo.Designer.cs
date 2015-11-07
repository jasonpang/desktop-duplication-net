namespace DesktopDuplication.Demo
{
    partial class FormDemo
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
            this.UpdatedRegion = new System.Windows.Forms.Label();
            this.MovedRegion = new System.Windows.Forms.Label();
            this.LabelCursor = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // UpdatedRegion
            // 
            this.UpdatedRegion.BackColor = System.Drawing.Color.Orange;
            this.UpdatedRegion.Location = new System.Drawing.Point(37, 109);
            this.UpdatedRegion.Name = "UpdatedRegion";
            this.UpdatedRegion.Size = new System.Drawing.Size(1, 1);
            this.UpdatedRegion.TabIndex = 0;
            // 
            // MovedRegion
            // 
            this.MovedRegion.BackColor = System.Drawing.Color.Purple;
            this.MovedRegion.Location = new System.Drawing.Point(308, 215);
            this.MovedRegion.Name = "MovedRegion";
            this.MovedRegion.Size = new System.Drawing.Size(1, 1);
            this.MovedRegion.TabIndex = 1;
            // 
            // LabelCursor
            // 
            this.LabelCursor.BackColor = System.Drawing.Color.Red;
            this.LabelCursor.Location = new System.Drawing.Point(141, 97);
            this.LabelCursor.Name = "LabelCursor";
            this.LabelCursor.Size = new System.Drawing.Size(15, 15);
            this.LabelCursor.TabIndex = 2;
            // 
            // FormDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 431);
            this.Controls.Add(this.LabelCursor);
            this.Controls.Add(this.MovedRegion);
            this.Controls.Add(this.UpdatedRegion);
            this.DoubleBuffered = true;
            this.Name = "FormDemo";
            this.Text = "Desktop Duplication API Demo";
            this.Shown += new System.EventHandler(this.FormDemo_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label UpdatedRegion;
        private System.Windows.Forms.Label MovedRegion;
        private System.Windows.Forms.Label LabelCursor;
    }
}

