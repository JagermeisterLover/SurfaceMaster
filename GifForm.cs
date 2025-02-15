using System;
using System.Drawing;
using System.Windows.Forms;

namespace SurfaceMaster
{
    public partial class GifForm : Form
    {
        public GifForm()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Restart GIF animation
            pictureBox1.Image = null;  // Reset image
            pictureBox1.Image = Properties.Resources.pepeG; // Use the correct resource name
        }
    }
}