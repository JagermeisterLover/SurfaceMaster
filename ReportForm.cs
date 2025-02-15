using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SurfaceMaster
{
    public partial class ReportForm : Form
    {
        public ReportForm(string activePanelType, string reportText)
        {
            InitializeComponent();
            ActivePanelType = activePanelType;
            ReportText = reportText;
        }

        public string ActivePanelType { get; }
        public string ReportText { get; }

        private void ReportForm_Load(object sender, EventArgs e)
        {
            // Set all image panels invisible first
            panelImageEA.Visible = false;
            panelImageOA.Visible = false;
            panelImageUnZ.Visible = false;
            panelImageUnU.Visible = false;
            panelImagePoly.Visible = false;


            // Set report text FIRST
            reportTextBox.Text = ReportText;

            // Then handle panel visibility
            // Show correct image panel based on active surface type
            switch (ActivePanelType)
            {
                case "Even asphere":
                    panelImageEA.Visible = true;
                    break;
                case "Odd asphere":
                    panelImageOA.Visible = true;
                    break;
                case "Opal Universal Z":
                    panelImageUnZ.Visible = true;
                    break;
                case "Opal Universal U":
                    panelImageUnU.Visible = true;
                    break;
                case "Opal polynomial Z":
                    panelImagePoly.Visible = true;
                    break;
            }

            // Set report text
            reportTextBox.Text = ReportText;
        }


    }

}
