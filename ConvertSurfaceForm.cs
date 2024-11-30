using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.LinearAlgebra;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using MathNet.Numerics.RootFinding;
using System.Text.RegularExpressions;

namespace SurfaceMaster
{
    public partial class ConvertSurfaceForm : Form
    {
        private SurfaceMaster _surfaceMaster;
        public ConvertSurfaceForm(SurfaceMaster surfaceMaster)
        {
            InitializeComponent();
            _surfaceMaster = surfaceMaster;
            ComboBoxSurfaceType.SelectedIndex = 0;
            InitializeTextBoxValidation();



            // Initialize the ComboBox for algorithm selection
            comboBoxAlgorithmSelection.Items.AddRange(new string[]
            {
            "leastsq", "least_squares", "nelder", "powell",
            });
            comboBoxAlgorithmSelection.SelectedIndex = 0; // Default selection

            // Set the text box values from SurfaceMaster
            SetTextBoxValuesFromSurfaceMaster();

        }
        // Define SomeMethod outside of the constructor
        private void InitializeTextBoxValidation()
        {
            // Attach the validation method to the TextChanged event for each relevant textbox
            textBoxEARadius.TextChanged += ValidateTextBoxInput;
            textBoxEAConic.TextChanged += ValidateTextBoxInput;
            textBoxOARadius.TextChanged += ValidateTextBoxInput;
            textBoxOAConic.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZRadius.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZe2.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZH.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnURadius.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUe2.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUH.TextChanged += ValidateTextBoxInput;
            textBoxPolyZA1.TextChanged += ValidateTextBoxInput;
            textBoxPolyZA2.TextChanged += ValidateTextBoxInput;
            // Add other textboxes as needed
        }

        private void ValidateTextBoxInput(object? sender, EventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox == null) return;

            // Define the regex pattern for acceptable input
            string pattern = @"^[0-9.,eE+-]*$";

            // Check if the input matches the pattern
            if (Regex.IsMatch(textBox.Text, pattern))
            {
                // Input is valid
                textBox.BackColor = SystemColors.Window; // Default background color
                textBox.ForeColor = SystemColors.ControlText; // Default text color
            }
            else
            {
                // Input is invalid
                textBox.BackColor = Color.FromArgb(255, 200, 200); // Light red background
                textBox.ForeColor = Color.DarkRed; // Dark red text color
            }
        }

        private void SomeMethod()
        {
            // Use the AppendToConsole method from SurfaceMaster
            _surfaceMaster.AppendToConsole("Message from ConvertSurfaceForm.");
        }

        private void SetTextBoxValuesFromSurfaceMaster()
        {
            if (_surfaceMaster == null) return;

            string activePanel = _surfaceMaster.GetActivePanel() ?? string.Empty; // Added null-coalescing operator
            string valueToSet = string.Empty;

            // Determine the value to set based on the active panel in SurfaceMaster
            switch (activePanel)
            {
                case "Even asphere":
                    valueToSet = _surfaceMaster.EARadiusValue ?? string.Empty; // Added null-coalescing operator
                    break;
                case "Odd asphere":
                    valueToSet = _surfaceMaster.OARadiusValue ?? string.Empty; // Added null-coalescing operator
                    break;
                case "Opal Universal Z":
                    valueToSet = _surfaceMaster.OpalUnZRadiusValue ?? string.Empty; // Added null-coalescing operator
                    break;
                case "Opal Universal U":
                    valueToSet = _surfaceMaster.OpalUnURadiusValue ?? string.Empty; // Added null-coalescing operator
                    break;
                case "Opal polynomial Z":
                    valueToSet = _surfaceMaster.PolyA1Value ?? string.Empty; // Added null-coalescing operator
                    break;
            }

            // Set the value to all relevant text boxes in ConvertSurfaceForm
            textBoxEARadius.Text = valueToSet;
            textBoxOARadius.Text = valueToSet;
            textBoxOpalUnZRadius.Text = valueToSet;
            textBoxOpalUnURadius.Text = valueToSet;
            textBoxPolyZA1.Text = valueToSet;
        }

        private List<(double r, double z)> sagData = new List<(double r, double z)>();

        private void ComboBoxSurfaceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Hide all panels initially
            panelConvertEvenAsphere.Visible = false;
            panelConvertOddAsphere.Visible = false;
            panelConvertOpalUnZ.Visible = false;
            panelConvertOpalUnU.Visible = false;
            panelConvertPoly.Visible = false;

            // Check if SelectedItem is not null before using it
            if (ComboBoxSurfaceType.SelectedItem != null)
            {
                // Show the selected panel
                switch (ComboBoxSurfaceType.SelectedItem.ToString())
                {
                    case "Even asphere":
                        panelConvertEvenAsphere.Visible = true;
                        break;
                    case "Odd asphere":
                        panelConvertOddAsphere.Visible = true;
                        break;
                    case "Opal Universal Z":
                        panelConvertOpalUnZ.Visible = true;
                        break;
                    case "Opal Universal U":
                        panelConvertOpalUnU.Visible = true;
                        break;
                    case "Opal polynomial Z":
                        panelConvertPoly.Visible = true;
                        break;
                }
            }
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            // Determine the selected surface type
            int surfaceType = ComboBoxSurfaceType.SelectedIndex + 1;

            // Prepare variables to store the values from the controls
            double radius = 0;
            double h = 0;
            int e2IsVariable = 0;
            double e2 = 0;
            int termNumber = 0;
            int conicIsVariable = 0;
            double conic = 0;

            // Get the selected optimization algorithm
            string optimizationAlgorithm = comboBoxAlgorithmSelection.SelectedItem?.ToString() ?? "default_algorithm"; // Added null-coalescing operator

            // Check which panel is visible and get the values from the controls
            if (panelConvertEvenAsphere.Visible)
            {
                radius = ParseDouble(textBoxEARadius.Text);
                conicIsVariable = checkBoxEAVaryConic.Checked ? 1 : 0;
                conic = ParseDouble(textBoxEAConic.Text);
                termNumber = trackBarEA.Value;
            }
            if (panelConvertOddAsphere.Visible)
            {
                radius = ParseDouble(textBoxOARadius.Text);
                conicIsVariable = checkBoxOAVaryConic.Checked ? 1 : 0;
                conic = ParseDouble(textBoxOAConic.Text);
                termNumber = trackBarOA.Value;
            }
            if (panelConvertOpalUnZ.Visible)
            {
                radius = ParseDouble(textBoxOpalUnZRadius.Text);
                h = ParseDouble(textBoxOpalUnZH.Text);
                e2IsVariable = checkBoxOpalUnZVarye2.Checked ? 1 : 0;
                e2 = ParseDouble(textBoxOpalUnZe2.Text);
                termNumber = trackBarOpalUnZ.Value;
            }
            if (panelConvertOpalUnU.Visible)
            {
                radius = ParseDouble(textBoxOpalUnURadius.Text);
                h = ParseDouble(textBoxOpalUnUH.Text);
                e2IsVariable = checkBoxOpalUnUVarye2.Checked ? 1 : 0;
                e2 = ParseDouble(textBoxOpalUnUe2.Text);
                termNumber = trackBarOpalUnU.Value;
            }
            if (panelConvertPoly.Visible)
            {
                radius = ParseDouble(textBoxPolyZA1.Text) / 2;
                e2IsVariable = checkBoxPolyZVaryA2.Checked ? 1 : 0;
                e2 = ParseDouble(textBoxPolyZA2.Text) + 1;
                termNumber = trackBarOpalPolyZ.Value;
            }

            // If the checkbox is unchecked, set termNumber to 0
            if (!checkBoxUseHigherOrderTerms.Checked)
            {
                termNumber = 0;
            }

            // Check if the required files exist
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string convertSettingsPath = Path.Combine(baseDirectory, "ConvertSettings.txt");
            string tempSurfaceDataPath = Path.Combine(baseDirectory, "tempsurfacedata.txt");

            // Create ConvertSettings.txt if it doesn't exist
            if (!File.Exists(convertSettingsPath))
            {
                File.Create(convertSettingsPath).Dispose();
            }

            if (!File.Exists(tempSurfaceDataPath))
            {
                _surfaceMaster.AppendToConsole(">> tempsurfacedata.txt is missing. Please create it by pressing Sag Data button or create it manually.");
                return;
            }

            // Check if radius is zero
            if (radius == 0)
            {
                _surfaceMaster.AppendToConsole(">> Error: radius cannot be zero");
                MessageBox.Show("Radius cannot be zero.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Determine the state of the AutoAlgorithm checkbox
            int autoAlgorithm = checkBoxAuto.Checked ? 1 : 0;

            // Create the settings string using InvariantCulture for formatting
            string settings = $"SurfaceType={surfaceType}\n" +
                              $"Radius={radius.ToString(CultureInfo.InvariantCulture)}\n" +
                              $"H={h.ToString(CultureInfo.InvariantCulture)}\n" +
                              $"e2_isVariable={e2IsVariable}\n" +
                              $"e2={e2.ToString(CultureInfo.InvariantCulture)}\n" +
                              $"TermNumber={termNumber}\n" +
                              $"OptimizationAlgorithm={optimizationAlgorithm}\n" +
                              $"AutoAlgorithm={autoAlgorithm}\n" +
                              $"conic_isVariable={conicIsVariable}\n" +
                              $"conic={conic.ToString(CultureInfo.InvariantCulture)}";

            // Save the settings to a file
            File.WriteAllText(convertSettingsPath, settings);

            // Inform the user everything is okay
            MessageBox.Show("Everything is OK. Please check the console for results.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _surfaceMaster.AppendToConsole(">>EquationFitter is starting...");
            // Execute fitter.exe
            string fitterPath = Path.Combine(baseDirectory, "EquationFitter.exe");
            try
            {
                System.Diagnostics.Process.Start(fitterPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start equationfitter.exe: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _surfaceMaster.AppendToConsole(">> Error: failed to start equationfitter.exe. Check if it's missing");
            }
        }

        private double ParseDouble(string input)
        {
            // Replace comma with dot and parse the double
            input = input.Replace(',', '.');
            double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double result);
            return result;
        }

        private void checkBoxUseHigherOrderTerms_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = checkBoxUseHigherOrderTerms.Checked;

            // Disable all trackbars if the checkbox is unchecked
            trackBarEA.Enabled = isChecked;
            trackBarOA.Enabled = isChecked;
            trackBarOpalUnZ.Enabled = isChecked;
            trackBarOpalUnU.Enabled = isChecked;
            trackBarOpalPolyZ.Enabled = isChecked;
        }
    }
}