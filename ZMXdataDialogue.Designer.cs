namespace SurfaceMaster
{
    partial class ZMXdataDialogue
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
            label1 = new Label();
            buttonSelectZMXsurface = new Button();
            tabControlSurfaceSelection = new TabControl();
            panelTabControlSurfaceSelection = new Panel();
            panelTabControlSurfaceSelection.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Archivo", 9F, FontStyle.Bold);
            label1.Location = new Point(242, 21);
            label1.Name = "label1";
            label1.Size = new Size(189, 18);
            label1.TabIndex = 0;
            label1.Text = "Choose aspheric surface to load";
            // 
            // buttonSelectZMXsurface
            // 
            buttonSelectZMXsurface.Font = new Font("Archivo", 9F, FontStyle.Bold);
            buttonSelectZMXsurface.Location = new Point(256, 307);
            buttonSelectZMXsurface.Margin = new Padding(3, 2, 3, 2);
            buttonSelectZMXsurface.Name = "buttonSelectZMXsurface";
            buttonSelectZMXsurface.Size = new Size(196, 22);
            buttonSelectZMXsurface.TabIndex = 1;
            buttonSelectZMXsurface.Text = "Select this aspheric surface";
            buttonSelectZMXsurface.UseVisualStyleBackColor = true;
            buttonSelectZMXsurface.Click += buttonSelectZMXsurface_Click;
            // 
            // tabControlSurfaceSelection
            // 
            tabControlSurfaceSelection.Location = new Point(3, 3);
            tabControlSurfaceSelection.Margin = new Padding(3, 2, 3, 2);
            tabControlSurfaceSelection.Name = "tabControlSurfaceSelection";
            tabControlSurfaceSelection.SelectedIndex = 0;
            tabControlSurfaceSelection.Size = new Size(668, 241);
            tabControlSurfaceSelection.TabIndex = 2;
            // 
            // panelTabControlSurfaceSelection
            // 
            panelTabControlSurfaceSelection.Controls.Add(tabControlSurfaceSelection);
            panelTabControlSurfaceSelection.Location = new Point(10, 48);
            panelTabControlSurfaceSelection.Margin = new Padding(3, 2, 3, 2);
            panelTabControlSurfaceSelection.Name = "panelTabControlSurfaceSelection";
            panelTabControlSurfaceSelection.Size = new Size(673, 246);
            panelTabControlSurfaceSelection.TabIndex = 3;
            // 
            // ZMXdataDialogue
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLightLight;
            ClientSize = new Size(700, 338);
            Controls.Add(panelTabControlSurfaceSelection);
            Controls.Add(buttonSelectZMXsurface);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 2, 3, 2);
            Name = "ZMXdataDialogue";
            Text = "Zemax Parser";
            panelTabControlSurfaceSelection.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button buttonSelectZMXsurface;
        private TabControl tabControlSurfaceSelection;
        private Panel panelTabControlSurfaceSelection;
    }
}