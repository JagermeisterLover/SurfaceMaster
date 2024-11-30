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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZMXdataDialogue));
            label1 = new Label();
            buttonSelectZMXsurface = new Button();
            tabControlSurfaceSelection = new TabControl();
            panelTabControlSurfaceSelection = new Panel();
            panelTabControlSurfaceSelection.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // buttonSelectZMXsurface
            // 
            resources.ApplyResources(buttonSelectZMXsurface, "buttonSelectZMXsurface");
            buttonSelectZMXsurface.Name = "buttonSelectZMXsurface";
            buttonSelectZMXsurface.UseVisualStyleBackColor = true;
            buttonSelectZMXsurface.Click += buttonSelectZMXsurface_Click;
            // 
            // tabControlSurfaceSelection
            // 
            resources.ApplyResources(tabControlSurfaceSelection, "tabControlSurfaceSelection");
            tabControlSurfaceSelection.Name = "tabControlSurfaceSelection";
            tabControlSurfaceSelection.SelectedIndex = 0;
            // 
            // panelTabControlSurfaceSelection
            // 
            resources.ApplyResources(panelTabControlSurfaceSelection, "panelTabControlSurfaceSelection");
            panelTabControlSurfaceSelection.Controls.Add(tabControlSurfaceSelection);
            panelTabControlSurfaceSelection.Name = "panelTabControlSurfaceSelection";
            // 
            // ZMXdataDialogue
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLightLight;
            Controls.Add(panelTabControlSurfaceSelection);
            Controls.Add(buttonSelectZMXsurface);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "ZMXdataDialogue";
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