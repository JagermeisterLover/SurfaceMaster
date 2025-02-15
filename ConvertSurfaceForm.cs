using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SurfaceMaster;

public partial class ConvertSurfaceForm : Form
{
    private readonly SurfaceMaster _surfaceMaster;

    private List<(double r, double z)> sagData = new();

    public ConvertSurfaceForm(SurfaceMaster surfaceMaster)
    {
        InitializeComponent();
        _surfaceMaster = surfaceMaster;
        ComboBoxSurfaceType.SelectedIndex = 0;
        InitializeTextBoxValidation();


        // Initialize the ComboBox for algorithm selection
        comboBoxAlgorithmSelection.Items.AddRange(new[]
        {
            "leastsq", "least_squares", "nelder", "powell"
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
        var textBox = sender as TextBox;
        if (textBox == null) return;

        // Define the regex pattern for acceptable input
        var pattern = @"^[0-9.,eE+-]*$";

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

        var activePanel = _surfaceMaster.GetActivePanel() ?? string.Empty;
        var valueToSet = string.Empty;
        var isPolyPanel = activePanel == "Opal polynomial Z"; // Check if active panel is Poly

        switch (activePanel)
        {
            case "Even asphere":
                valueToSet = _surfaceMaster.EARadiusValue ?? string.Empty;
                break;
            case "Odd asphere":
                valueToSet = _surfaceMaster.OARadiusValue ?? string.Empty;
                break;
            case "Opal Universal Z":
                valueToSet = _surfaceMaster.OpalUnZRadiusValue ?? string.Empty;
                break;
            case "Opal Universal U":
                valueToSet = _surfaceMaster.OpalUnURadiusValue ?? string.Empty;
                break;
            case "Opal polynomial Z":
                valueToSet = _surfaceMaster.PolyA1Value ?? string.Empty;
                break;
        }

        if (isPolyPanel)
        {
            // For Poly panel: A1 is stored in valueToSet (radius * 2)
            if (double.TryParse(valueToSet, NumberStyles.Any, CultureInfo.InvariantCulture, out var a1))
            {
                // Set PolyZA1 to A1 (no multiplication)
                textBoxPolyZA1.Text = a1.ToString(CultureInfo.InvariantCulture);
                // Calculate radius (A1 / 2) and set other text boxes
                var radius = a1 / 2;
                var radiusString = radius.ToString(CultureInfo.InvariantCulture);
                textBoxEARadius.Text = radiusString;
                textBoxOARadius.Text = radiusString;
                textBoxOpalUnZRadius.Text = radiusString;
                textBoxOpalUnURadius.Text = radiusString;
            }
            else
            {
                // Clear if parsing fails
                textBoxPolyZA1.Text = string.Empty;
                textBoxEARadius.Text = string.Empty;
                textBoxOARadius.Text = string.Empty;
                textBoxOpalUnZRadius.Text = string.Empty;
                textBoxOpalUnURadius.Text = string.Empty;
            }
        }
        else
        {
            // Original logic for other panels
            textBoxEARadius.Text = valueToSet;
            textBoxOARadius.Text = valueToSet;
            textBoxOpalUnZRadius.Text = valueToSet;
            textBoxOpalUnURadius.Text = valueToSet;

            // Set PolyZA1 to valueToSet (radius) * 2
            if (double.TryParse(valueToSet, NumberStyles.Any, CultureInfo.InvariantCulture, out var numericValue))
            {
                textBoxPolyZA1.Text = (numericValue * 2).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                textBoxPolyZA1.Text = string.Empty;
            }
        }
    }

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

    private void buttonConvert_Click(object sender, EventArgs e)
    {
        // Determine the selected surface type
        var surfaceType = ComboBoxSurfaceType.SelectedIndex + 1;

        // Prepare variables to store the values from the controls
        double radius = 0;
        double h = 0;
        var e2IsVariable = 0;
        double e2 = 0;
        var termNumber = 0;
        var conicIsVariable = 0;
        double conic = 0;

        // Get the selected optimization algorithm
        var optimizationAlgorithm =
            comboBoxAlgorithmSelection.SelectedItem?.ToString() ??
            "default_algorithm"; // Added null-coalescing operator

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
        if (!checkBoxUseHigherOrderTerms.Checked) termNumber = 0;

        // Check if the required files exist
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var convertSettingsPath = Path.Combine(baseDirectory, "ConvertSettings.txt");
        var tempSurfaceDataPath = Path.Combine(baseDirectory, "tempsurfacedata.txt");

        // Create ConvertSettings.txt if it doesn't exist
        if (!File.Exists(convertSettingsPath)) File.Create(convertSettingsPath).Dispose();

        if (!File.Exists(tempSurfaceDataPath))
        {
            _surfaceMaster.AppendToConsole(
                ">> tempsurfacedata.txt is missing. Please create it by pressing Sag Data button or create it manually.");
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
        var autoAlgorithm = checkBoxAuto.Checked ? 1 : 0;

        // Create the settings string using InvariantCulture for formatting
        var settings = $"SurfaceType={surfaceType}\n" +
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
        MessageBox.Show("Everything is OK. Please check the console for results.", "Success", MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        _surfaceMaster.AppendToConsole(">>EquationFitter is starting...");
        // Execute fitter.exe
        var fitterPath = Path.Combine(baseDirectory, "EquationFitter.exe");
        try
        {
            Process.Start(fitterPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start equationfitter.exe: {ex.Message}", "Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            _surfaceMaster.AppendToConsole(">> Error: failed to start equationfitter.exe. Check if it's missing");
        }
    }

    private double ParseDouble(string input)
    {
        // Replace comma with dot and parse the double
        input = input.Replace(',', '.');
        double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
        return result;
    }

    private void checkBoxUseHigherOrderTerms_CheckedChanged(object sender, EventArgs e)
    {
        var isChecked = checkBoxUseHigherOrderTerms.Checked;

        // Disable all trackbars if the checkbox is unchecked
        trackBarEA.Enabled = isChecked;
        trackBarOA.Enabled = isChecked;
        trackBarOpalUnZ.Enabled = isChecked;
        trackBarOpalUnU.Enabled = isChecked;
        trackBarOpalPolyZ.Enabled = isChecked;
    }
}