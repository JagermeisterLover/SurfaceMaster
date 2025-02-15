namespace SurfaceMaster
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            label1 = new Label();
            label3 = new Label();
            pictureBoxPepes = new PictureBox();
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPepes).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI Semibold", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 204);
            label1.ForeColor = Color.Black;
            label1.ImeMode = ImeMode.NoControl;
            label1.Location = new Point(156, 34);
            label1.Name = "label1";
            label1.Size = new Size(151, 25);
            label1.TabIndex = 1;
            label1.Text = "SurfaceMaster™";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.Black;
            label3.ImeMode = ImeMode.NoControl;
            label3.Location = new Point(131, 74);
            label3.Name = "label3";
            label3.Size = new Size(201, 21);
            label3.TabIndex = 3;
            label3.Text = "Version 2.0.9, Feb 15, 2025";
            // 
            // pictureBoxPepes
            // 
            pictureBoxPepes.Image = (Image)resources.GetObject("pictureBoxPepes.Image");
            pictureBoxPepes.Location = new Point(102, 115);
            pictureBoxPepes.Name = "pictureBoxPepes";
            pictureBoxPepes.Size = new Size(259, 214);
            pictureBoxPepes.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxPepes.TabIndex = 4;
            pictureBoxPepes.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.Black;
            label2.ImeMode = ImeMode.NoControl;
            label2.Location = new Point(133, 361);
            label2.Name = "label2";
            label2.Size = new Size(197, 21);
            label2.TabIndex = 5;
            label2.Text = "achapovskyai@gmail.com";
            // 
            // AboutForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLightLight;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(469, 404);
            Controls.Add(label2);
            Controls.Add(pictureBoxPepes);
            Controls.Add(label3);
            Controls.Add(label1);
            DoubleBuffered = true;
            ForeColor = SystemColors.ControlLightLight;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 2, 3, 2);
            Name = "AboutForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About";
            ((System.ComponentModel.ISupportInitialize)pictureBoxPepes).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private Label label3;
        private PictureBox pictureBoxPepes;
        private Label label2;
    }
}