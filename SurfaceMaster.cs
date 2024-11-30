using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Text;
using SurfaceMaster;
using System.ComponentModel;
using System.Resources;


namespace SurfaceMaster

{
    public partial class SurfaceMaster : Form
    {
        public string EARadiusValue => textBoxEARadius.Text;
        public string OARadiusValue => textBoxOARadius.Text;
        public string OpalUnZRadiusValue => textBoxOpalUnZRadius.Text;
        public string OpalUnURadiusValue => textBoxOpalUnURadius.Text;
        public string PolyA1Value => textBoxPolyA1.Text;

        private DataGridView dataGridView;

        private SurfaceCalculations surfaceCalculations;
        private ConvertSurfaceForm convertSurfaceForm;
        private ZMXdataDialogue zmxDataDialogue;
        private AboutForm aboutForm;


        private List<(double r, double z)> results = new List<(double r, double z)>(); // Class-level variable
        public SurfaceMaster()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

            InitializeComponent();
            CheckFilesOnStartup();
            InitializeTextBoxValidation();

            // Initialize other forms
            convertSurfaceForm = new ConvertSurfaceForm(this);
            zmxDataDialogue = new ZMXdataDialogue();
            aboutForm = new AboutForm();

            surfaceCalculations = new SurfaceCalculations();

            // Default surface selection
            ComboBoxSurfaceType.SelectedIndex = 0;
            panelEvenAsphere.Visible = true;
            panelOddAsphere.Visible = false;
            panelOpalUnZ.Visible = false;
            panelOpalUnU.Visible = false;
            panelPoly.Visible = false;
        }




        #region ZMX import

        // This method is triggered when the "Import ZMX" button is clicked.
        private void buttonImportZMX_Click(object sender, EventArgs e)
        {
            // Open a file dialog to select a ZMX file.
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Set the file filter to ZMX files and all files.
                openFileDialog.Filter = "ZMX files (*.zmx)|*.zmx|All files (*.*)|*.*";

                // Show the dialog and check if the user selected a file.
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the selected file path.
                    string filePath = openFileDialog.FileName;

                    // Parse the ZMX file to extract aspheric surface data.
                    var asphericSurfaces = ParseZmxFile(filePath);

                    // Open a new form to display the parsed data.
                    var dataDialogue = new ZMXdataDialogue();
                    dataDialogue.LoadData(asphericSurfaces);

                    // Subscribe to the DataSaved event to handle data saving.
                    dataDialogue.DataSaved += OnDataSaved;

                    // Show the dialogue form.
                    dataDialogue.ShowDialog();
                }
            }
        }

        // This method is called when data is saved from the ZMXdataDialogue form.
        private void OnDataSaved()
        {
            // Define the path to a temporary file where data is saved.
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempZMXData.txt");
            AppendToConsole($"Checking file at: {filePath}");

            // Check if the file exists.
            if (File.Exists(filePath))
            {
                // Read all lines from the file.
                var lines = File.ReadAllLines(filePath);
                AppendToConsole("File contents:");
                foreach (var line in lines)
                {
                    AppendToConsole(line);
                }

                // Ensure there are at least two lines (header and data).
                if (lines.Length < 2)
                {
                    AppendToConsole("Not enough data in file.");
                    return;
                }

                // Parse the header and data lines.
                var headers = lines[0].Split('\t');
                var values = lines[1].Split('\t');

                // Check if the number of headers matches the number of values.
                if (headers.Length != values.Length)
                {
                    AppendToConsole("Header and value count mismatch.");
                    return;
                }

                // Create a dictionary to store the parsed data.
                var data = new Dictionary<string, string>();
                for (int i = 0; i < headers.Length; i++)
                {
                    data[headers[i]] = values[i];
                }

                AppendToConsole("Parsed data:");
                foreach (var kvp in data)
                {
                    AppendToConsole($"{kvp.Key}: {kvp.Value}");
                }

                // Clear textboxes before filling them with new data.
                ClearTextboxes();

                // Check if the data contains a "TYPE" key and update the UI accordingly.
                if (data.TryGetValue("TYPE", out string type))
                {
                    AppendToConsole($"Surface type: {type}");
                    if (type == "EVENASPH")
                    {
                        UpdateUI(() =>
                        {
                            ComboBoxSurfaceType.SelectedItem = "Even asphere"; // Set ComboBox selection
                            panelEvenAsphere.Visible = true;
                            textBoxEARadius.Text = data.GetValueOrDefault("RADIUS", "0");
                            textBoxEAHeight.Text = data.GetValueOrDefault("DIAM", "0");
                            textBoxEAConic.Text = data.GetValueOrDefault("CONI", "0");
                            textBoxEA4.Text = data.GetValueOrDefault("PARM_2", "0");
                            textBoxEA6.Text = data.GetValueOrDefault("PARM_3", "0");
                            textBoxEA8.Text = data.GetValueOrDefault("PARM_4", "0");
                            textBoxEA10.Text = data.GetValueOrDefault("PARM_5", "0");
                            textBoxEA12.Text = data.GetValueOrDefault("PARM_6", "0");
                            textBoxEA14.Text = data.GetValueOrDefault("PARM_7", "0");
                            textBoxEA16.Text = data.GetValueOrDefault("PARM_8", "0");
                            textBoxEA18.Text = data.GetValueOrDefault("PARM_9", "0");
                            textBoxEA20.Text = data.GetValueOrDefault("PARM_10", "0");
                        });
                    }
                    else if (type == "STANDARD")
                    {
                        UpdateUI(() =>
                        {
                            ComboBoxSurfaceType.SelectedItem = "Even asphere"; // Set ComboBox selection
                            panelEvenAsphere.Visible = true;
                            textBoxEARadius.Text = data.GetValueOrDefault("RADIUS", "0");
                            textBoxEAHeight.Text = data.GetValueOrDefault("DIAM", "0");
                            textBoxEAConic.Text = data.GetValueOrDefault("CONI", "0");
                        });
                    }
                    else if (type == "IRREGULA")
                    {
                        UpdateUI(() =>
                        {
                            ComboBoxSurfaceType.SelectedItem = "Even asphere"; // Set ComboBox selection
                            panelEvenAsphere.Visible = true;
                            textBoxEARadius.Text = data.GetValueOrDefault("RADIUS", "0");
                            textBoxEAHeight.Text = data.GetValueOrDefault("DIAM", "0");
                            textBoxEAConic.Text = data.GetValueOrDefault("CONI", "0");
                        });
                    }
                    else if (type == "ODDASPHE")
                    {
                        UpdateUI(() =>
                        {
                            ComboBoxSurfaceType.SelectedItem = "Odd asphere"; // Set ComboBox selection
                            panelOddAsphere.Visible = true;
                            textBoxOARadius.Text = data.GetValueOrDefault("RADIUS", "0");
                            textBoxOAHeight.Text = data.GetValueOrDefault("DIAM", "0");
                            textBoxOAConic.Text = data.GetValueOrDefault("CONI", "0");
                            textBoxOA3.Text = data.GetValueOrDefault("PARM_3", "0");
                            textBoxOA4.Text = data.GetValueOrDefault("PARM_4", "0");
                            textBoxOA5.Text = data.GetValueOrDefault("PARM_5", "0");
                            textBoxOA6.Text = data.GetValueOrDefault("PARM_6", "0");
                            textBoxOA7.Text = data.GetValueOrDefault("PARM_7", "0");
                            textBoxOA8.Text = data.GetValueOrDefault("PARM_8", "0");
                            textBoxOA9.Text = data.GetValueOrDefault("PARM_9", "0");
                            textBoxOA10.Text = data.GetValueOrDefault("PARM_10", "0");
                            textBoxOA11.Text = data.GetValueOrDefault("PARM_11", "0");
                            textBoxOA12.Text = data.GetValueOrDefault("PARM_12", "0");
                            textBoxOA13.Text = data.GetValueOrDefault("PARM_13", "0");
                            textBoxOA14.Text = data.GetValueOrDefault("PARM_14", "0");
                            textBoxOA15.Text = data.GetValueOrDefault("PARM_15", "0");
                            textBoxOA16.Text = data.GetValueOrDefault("PARM_16", "0");
                            textBoxOA17.Text = data.GetValueOrDefault("PARM_17", "0");
                            textBoxOA18.Text = data.GetValueOrDefault("PARM_18", "0");
                            textBoxOA19.Text = data.GetValueOrDefault("PARM_19", "0");
                            textBoxOA20.Text = data.GetValueOrDefault("PARM_20", "0");
                        });
                    }
                }
                else
                {
                    AppendToConsole("TYPE not found in parsed data.");
                }

                // Trigger the toolStripButtonSag_Click event.
                toolStripButtonSag_Click(this, EventArgs.Empty);
            }
            else
            {
                AppendToConsole("File does not exist.");
            }
        }

        // This method clears the textboxes to avoid data mixing.
        private void ClearTextboxes()
        {
            // This method is called to clear all textboxes in the form.
            // It uses the UpdateUI method to ensure that UI updates are performed on the UI thread.
            UpdateUI(() =>
            {
                // Start the recursive clearing process from the top-level form (this).
                ClearTextboxesRecursive(this);
            });
        }

        private void ClearTextboxesRecursive(Control parent)
        {
            // Iterate over each control within the parent control.
            foreach (Control control in parent.Controls)
            {
                // Check if the current control is a TextBox.
                if (control is TextBox textBox)
                {
                    // Check if the TextBox is not the consoleTextBox.
                    if (textBox.Name != "consoleTextBox")
                    {
                        // If it is a TextBox and not the consoleTextBox, set its Text property to "0".
                        textBox.Text = "0";
                    }
                }
                else
                {
                    // If the control is not a TextBox, it might be a container (like a Panel or GroupBox).
                    // Recursively call this method to check its child controls.
                    ClearTextboxesRecursive(control);
                }
            }
        }

        // This method ensures that UI updates are performed on the UI thread.
        private void UpdateUI(Action action)
        {
            // Check if the current thread is not the UI thread.
            if (InvokeRequired)
            {
                // If not, invoke the action on the UI thread.
                Invoke(action);
            }
            else
            {
                // If already on the UI thread, execute the action directly.
                action();
            }
        }

        // This method parses a ZMX file to extract aspheric surface data.
        private List<Dictionary<string, string>> ParseZmxFile(string filePath)
        {
            var asphericSurfaces = new List<Dictionary<string, string>>();
            Dictionary<string, string> currentSurface = null;
            bool isAspheric = false;

            try
            {
                // Try reading the file with UTF-8 encoding first.
                foreach (var line in File.ReadLines(filePath, Encoding.UTF8))
                {
                    ProcessLine(line, ref currentSurface, ref isAspheric, asphericSurfaces);
                }
            }
            catch (Exception)
            {
                try
                {
                    // If UTF-8 fails, try reading with UTF-16 encoding.
                    foreach (var line in File.ReadLines(filePath, Encoding.Unicode))
                    {
                        ProcessLine(line, ref currentSurface, ref isAspheric, asphericSurfaces);
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        // If UTF-16 fails, try reading with the system's default encoding (ANSI).
                        foreach (var line in File.ReadLines(filePath, Encoding.Default))
                        {
                            ProcessLine(line, ref currentSurface, ref isAspheric, asphericSurfaces);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Show an error message if all attempts to read the file fail.
                        MessageBox.Show($"Failed to read the file: {ex.Message}");
                    }
                }
            }

            // Add the last surface if it is aspheric.
            if (currentSurface != null && isAspheric)
            {
                asphericSurfaces.Add(currentSurface);
            }

            return asphericSurfaces;
        }

        // This method processes each line of the ZMX file to extract relevant data.
        private void ProcessLine(string line, ref Dictionary<string, string> currentSurface, ref bool isAspheric, List<Dictionary<string, string>> asphericSurfaces)
        {
            var trimmedLine = line.Trim();

            // Check if the line starts a new surface.
            if (trimmedLine.StartsWith("SURF"))
            {
                // If the previous surface was aspheric, add it to the list.
                if (currentSurface != null && isAspheric)
                {
                    asphericSurfaces.Add(currentSurface);
                }
                // Start a new surface.
                currentSurface = new Dictionary<string, string>
        {
            { "SURF", trimmedLine.Split()[1] }
        };
                isAspheric = false;
            }

            // Check if the line indicates an aspheric surface.
            if (trimmedLine.Contains("CONI") || trimmedLine.Contains("PARM"))
            {
                isAspheric = true;
            }

            // If a current surface is being processed, extract relevant data.
            if (currentSurface != null)
            {
                if (trimmedLine.StartsWith("TYPE"))
                {
                    currentSurface["TYPE"] = trimmedLine.Split(new[] { ' ' }, 2)[1];
                }
                else if (trimmedLine.StartsWith("CURV"))
                {
                    // Parse the CURV value and calculate its reciprocal.
                    string curvString = trimmedLine.Split()[1];
                    if (double.TryParse(curvString, NumberStyles.Float, CultureInfo.InvariantCulture, out double curvValue) && curvValue != 0)
                    {
                        double radiusValue = 1.0 / curvValue;
                        currentSurface["RADIUS"] = radiusValue.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        currentSurface["RADIUS"] = "N/A"; // Handle division by zero or parsing error.
                    }
                }
                else if (trimmedLine.StartsWith("CONI"))
                {
                    currentSurface["CONI"] = trimmedLine.Split()[1];
                }
                else if (trimmedLine.StartsWith("PARM"))
                {
                    var parts = trimmedLine.Split();
                    currentSurface[$"PARM_{parts[1]}"] = parts[2];
                }
                else if (trimmedLine.StartsWith("DIAM"))
                {
                    currentSurface["DIAM"] = trimmedLine.Split()[1];
                }
            }
        }

        // This method saves the parsed aspheric surface data to a file.
        private void SaveParsedData(List<Dictionary<string, string>> asphericSurfaces, string outputPath)
        {
            using (var writer = new StreamWriter(outputPath))
            {
                foreach (var surface in asphericSurfaces)
                {
                    writer.WriteLine($"Surface {surface["SURF"]}:");
                    writer.WriteLine($"  TYPE: {surface.GetValueOrDefault("TYPE", "N/A")}");
                    writer.WriteLine($"  CURV: {surface.GetValueOrDefault("CURV", "N/A")}");
                    writer.WriteLine($"  CONI: {surface.GetValueOrDefault("CONI", "N/A")}");
                    writer.WriteLine($"  DIAM: {surface.GetValueOrDefault("DIAM", "N/A")}");
                    for (int i = 1; i <= 8; i++)
                    {
                        string parmKey = $"PARM_{i}";
                        if (surface.ContainsKey(parmKey))
                        {
                            writer.WriteLine($"  {parmKey}: {surface[parmKey]}");
                        }
                    }
                    writer.WriteLine();
                }
            }
        }
        #endregion

        #region Textbox data inputs validation
        private void InitializeTextBoxValidation()
        {
            // Attach the validation method to the TextChanged event for each relevant textbox
            textBoxEAConic.TextChanged += ValidateTextBoxInput;
            textBoxEAHeight.TextChanged += ValidateTextBoxInput;
            textBoxEARadius.TextChanged += ValidateTextBoxInput;
            textBoxEA12.TextChanged += ValidateTextBoxInput;
            textBoxEA10.TextChanged += ValidateTextBoxInput;
            textBoxEA8.TextChanged += ValidateTextBoxInput;
            textBoxEA6.TextChanged += ValidateTextBoxInput;
            textBoxEA4.TextChanged += ValidateTextBoxInput;
            textBoxOA7.TextChanged += ValidateTextBoxInput;
            textBoxOA6.TextChanged += ValidateTextBoxInput;
            textBoxOA5.TextChanged += ValidateTextBoxInput;
            textBoxOA4.TextChanged += ValidateTextBoxInput;
            textBoxOA3.TextChanged += ValidateTextBoxInput;
            textBoxOAConic.TextChanged += ValidateTextBoxInput;
            textBoxOAHeight.TextChanged += ValidateTextBoxInput;
            textBoxOARadius.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZA7.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZA6.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZH.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZA4.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZA3.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZe2.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZHeight.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZRadius.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZA12.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZA11.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZA10.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZA9.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZA8.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZA5.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUA4.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUA11.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUA10.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUA9.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUA8.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUA7.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUA6.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUA5.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUH.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUA3.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUA2.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUe2.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUHeight.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnURadius.TextChanged += ValidateTextBoxInput;
            textBoxPolyA6.TextChanged += ValidateTextBoxInput;
            textBoxPolyA11.TextChanged += ValidateTextBoxInput;
            textBoxPolyA10.TextChanged += ValidateTextBoxInput;
            textBoxPolyA9.TextChanged += ValidateTextBoxInput;
            textBoxPolyA8.TextChanged += ValidateTextBoxInput;
            textBoxPolyA7.TextChanged += ValidateTextBoxInput;
            textBoxPolyA3.TextChanged += ValidateTextBoxInput;
            textBoxPolyA5.TextChanged += ValidateTextBoxInput;
            textBoxPolyA4.TextChanged += ValidateTextBoxInput;
            textBoxPolyA2.TextChanged += ValidateTextBoxInput;
            textBoxPolyHeight.TextChanged += ValidateTextBoxInput;
            textBoxPolyA1.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUA12.TextChanged += ValidateTextBoxInput;
            textBoxPolyA13.TextChanged += ValidateTextBoxInput;
            textBoxPolyA12.TextChanged += ValidateTextBoxInput;
            textBoxEA18.TextChanged += ValidateTextBoxInput;
            textBoxEA16.TextChanged += ValidateTextBoxInput;
            textBoxEA14.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZA13.TextChanged += ValidateTextBoxInput;
            textBoxEA20.TextChanged += ValidateTextBoxInput;
            textBoxOA18.TextChanged += ValidateTextBoxInput;
            textBoxOA17.TextChanged += ValidateTextBoxInput;
            textBoxOA16.TextChanged += ValidateTextBoxInput;
            textBoxOA15.TextChanged += ValidateTextBoxInput;
            textBoxOA14.TextChanged += ValidateTextBoxInput;
            textBoxOA13.TextChanged += ValidateTextBoxInput;
            textBoxOA12.TextChanged += ValidateTextBoxInput;
            textBoxOA11.TextChanged += ValidateTextBoxInput;
            textBoxOA10.TextChanged += ValidateTextBoxInput;
            textBoxOA9.TextChanged += ValidateTextBoxInput;
            textBoxOA8.TextChanged += ValidateTextBoxInput;
            textBoxOA20.TextChanged += ValidateTextBoxInput;
            textBoxOA19.TextChanged += ValidateTextBoxInput;
            textBoxPolyminheight.TextChanged += ValidateTextBoxInput;
            textBoxEAminheight.TextChanged += ValidateTextBoxInput;
            textBoxOAminheight.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnZminheight.TextChanged += ValidateTextBoxInput;
            textBoxOpalUnUminheight.TextChanged += ValidateTextBoxInput;
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
        #endregion

        public void AppendToConsole(string message)
        {
            if (consoleTextBox.InvokeRequired)
            {
                consoleTextBox.Invoke(new Action(() => AppendToConsole(message)));
            }
            else
            {
                consoleTextBox.AppendText(message + Environment.NewLine);
                consoleTextBox.SelectionStart = consoleTextBox.Text.Length;
                consoleTextBox.ScrollToCaret();
            }
        }

        private void CheckFilesOnStartup()
        {
            AppendToConsole("SurfaceMaster V1.2 successfully loaded");

            string programFolder = AppDomain.CurrentDomain.BaseDirectory;
            string equationFitterPath = Path.Combine(programFolder, "equationfitter.exe");
            string documentationPath = Path.Combine(programFolder, "documentation.pdf");

            if (File.Exists(equationFitterPath))
            {
                AppendToConsole("EquationFitter V3 is loaded.");
            }
            else
            {
                AppendToConsole("equationfitter.exe is missing.");
            }

        }
        private void ComboBoxSurfaceType_SelectedIndexChanged(object sender, EventArgs e)
        {

            panelEvenAsphere.Visible = false;
            panelOddAsphere.Visible = false;
            panelOpalUnZ.Visible = false;
            panelOpalUnU.Visible = false;
            panelPoly.Visible = false;


            // Check if SelectedItem is not null before using it
            if (ComboBoxSurfaceType.SelectedItem != null)
            {
                switch (ComboBoxSurfaceType.SelectedItem.ToString())
                {
                    case "Even asphere":
                        panelEvenAsphere.Visible = true;
                        AppendToConsole($">>Surface type Even Asphere was selected");
                        break;
                    case "Odd asphere":
                        panelOddAsphere.Visible = true;
                        AppendToConsole($">> Surface type Odd Asphere was selected");
                        break;
                    case "Opal Universal Z":
                        panelOpalUnZ.Visible = true;
                        AppendToConsole($">> SurfaceType Opal Universal Z was selected");
                        break;
                    case "Opal Universal U":
                        panelOpalUnU.Visible = true;
                        AppendToConsole($">> SurfaceType Opal Universal U was selected");
                        break;
                    case "Opal polynomial Z":
                        panelPoly.Visible = true;
                        AppendToConsole($">> SurfaceType Opal Polynomial Z was selected");
                        break;
                        // more surface types
                }
            }
        }

        public string GetActivePanel()
        {
            if (panelEvenAsphere.Visible) return "Even asphere";
            if (panelOddAsphere.Visible) return "Odd asphere";
            if (panelOpalUnZ.Visible) return "Opal Universal Z";
            if (panelOpalUnU.Visible) return "Opal Universal U";
            if (panelPoly.Visible) return "Opal polynomial Z";
            return string.Empty;
        }

        private void toolStripButtonSag_Click(object sender, EventArgs e)
        {
            results.Clear(); // clear previous results
            double R = 0, e2 = 0, k = 0, minR = 0, maxR = 0, H = 0;
            double A1 = 0, A2 = 0, A3 = 0, A4 = 0, A5 = 0, A6 = 0, A7 = 0, A8 = 0, A9 = 0, A10 = 0, A11 = 0, A12 = 0, A13 = 0, A14 = 0, A15 = 0, A16 = 0, A17 = 0, A18 = 0, A19 = 0, A20 = 0;

            // Helper function to parse text with either dot or comma as decimal separator
            double ParseInput(string input)
            {
                input = input.Replace(',', '.'); // Replace comma with dot
                double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double result);
                return result;
            }

            if (surfaceCalculations == null)
            {
                AppendToConsole("Surface calculations object is not initialized.");
                return;
            }

            if (panelEvenAsphere.Visible)
            {
                // parse inputs even asphere
                R = ParseInput(textBoxEARadius.Text);
                k = ParseInput(textBoxEAConic.Text);
                minR = ParseInput(textBoxEAminheight.Text);
                maxR = ParseInput(textBoxEAHeight.Text);
                A4 = ParseInput(textBoxEA4.Text);
                A6 = ParseInput(textBoxEA6.Text);
                A8 = ParseInput(textBoxEA8.Text);
                A10 = ParseInput(textBoxEA10.Text);
                A12 = ParseInput(textBoxEA12.Text);
                A14 = ParseInput(textBoxEA14.Text);
                A16 = ParseInput(textBoxEA16.Text);
                A18 = ParseInput(textBoxEA18.Text);
                A20 = ParseInput(textBoxEA18.Text);
            }
            else if (panelOddAsphere.Visible)
            {
                // parse inputs Odd Asphere
                R = ParseInput(textBoxOARadius.Text);
                k = ParseInput(textBoxOAConic.Text);
                minR = ParseInput(textBoxOAminheight.Text);
                maxR = ParseInput(textBoxOAHeight.Text);
                A3 = ParseInput(textBoxOA3.Text);
                A4 = ParseInput(textBoxOA4.Text);
                A5 = ParseInput(textBoxOA5.Text);
                A6 = ParseInput(textBoxOA6.Text);
                A7 = ParseInput(textBoxOA7.Text);
                A8 = ParseInput(textBoxOA8.Text);
                A9 = ParseInput(textBoxOA9.Text);
                A10 = ParseInput(textBoxOA10.Text);
                A11 = ParseInput(textBoxOA11.Text);
                A12 = ParseInput(textBoxOA12.Text);
                A13 = ParseInput(textBoxOA13.Text);
                A14 = ParseInput(textBoxOA14.Text);
                A15 = ParseInput(textBoxOA15.Text);
                A16 = ParseInput(textBoxOA16.Text);
                A17 = ParseInput(textBoxOA17.Text);
                A18 = ParseInput(textBoxOA18.Text);
                A19 = ParseInput(textBoxOA19.Text);
                A20 = ParseInput(textBoxOA20.Text);
            }
            else if (panelOpalUnZ.Visible)
            {
                // parse inputs opal universal Z
                R = ParseInput(textBoxOpalUnZRadius.Text);
                e2 = ParseInput(textBoxOpalUnZe2.Text);
                minR = ParseInput(textBoxOpalUnZminheight.Text);
                maxR = ParseInput(textBoxOpalUnZHeight.Text);
                H = ParseInput(textBoxOpalUnZH.Text);
                A3 = ParseInput(textBoxOpalUnZA3.Text);
                A4 = ParseInput(textBoxOpalUnZA4.Text);
                A5 = ParseInput(textBoxOpalUnZA5.Text);
                A6 = ParseInput(textBoxOpalUnZA6.Text);
                A7 = ParseInput(textBoxOpalUnZA7.Text);
                A8 = ParseInput(textBoxOpalUnZA8.Text);
                A9 = ParseInput(textBoxOpalUnZA9.Text);
                A10 = ParseInput(textBoxOpalUnZA10.Text);
                A11 = ParseInput(textBoxOpalUnZA11.Text);
                A12 = ParseInput(textBoxOpalUnZA12.Text);
                A13 = ParseInput(textBoxOpalUnZA13.Text);
            }
            else if (panelOpalUnU.Visible)
            {
                // parse inputs opal universal U
                R = ParseInput(textBoxOpalUnURadius.Text);
                e2 = ParseInput(textBoxOpalUnUe2.Text);
                minR = ParseInput(textBoxOpalUnUminheight.Text);
                maxR = ParseInput(textBoxOpalUnUHeight.Text);
                H = ParseInput(textBoxOpalUnUH.Text);
                A2 = ParseInput(textBoxOpalUnUA2.Text);
                A3 = ParseInput(textBoxOpalUnUA3.Text);
                A4 = ParseInput(textBoxOpalUnUA4.Text);
                A5 = ParseInput(textBoxOpalUnUA5.Text);
                A6 = ParseInput(textBoxOpalUnUA6.Text);
                A7 = ParseInput(textBoxOpalUnUA7.Text);
                A8 = ParseInput(textBoxOpalUnUA8.Text);
                A9 = ParseInput(textBoxOpalUnUA9.Text);
                A10 = ParseInput(textBoxOpalUnUA10.Text);
                A11 = ParseInput(textBoxOpalUnUA11.Text);
                A12 = ParseInput(textBoxOpalUnUA12.Text);
            }
            else if (panelPoly.Visible)
            {
                // parse inputs opal polynomial
                minR = ParseInput(textBoxPolyminheight.Text);
                maxR = ParseInput(textBoxPolyHeight.Text);
                A1 = ParseInput(textBoxPolyA1.Text);
                A2 = ParseInput(textBoxPolyA2.Text);
                A3 = ParseInput(textBoxPolyA3.Text);
                A4 = ParseInput(textBoxPolyA4.Text);
                A5 = ParseInput(textBoxPolyA5.Text);
                A6 = ParseInput(textBoxPolyA6.Text);
                A7 = ParseInput(textBoxPolyA7.Text);
                A8 = ParseInput(textBoxPolyA8.Text);
                A9 = ParseInput(textBoxPolyA9.Text);
                A10 = ParseInput(textBoxPolyA10.Text);
                A11 = ParseInput(textBoxPolyA11.Text);
                A12 = ParseInput(textBoxPolyA12.Text);
                A13 = ParseInput(textBoxPolyA13.Text);
            }

            for (double r = minR; r <= maxR; r += 0.1)
            {
                double z = 0;
                if (panelEvenAsphere.Visible)
                {
                    z = surfaceCalculations.CalculateEvenAsphereSag(r, R, k, A4, A6, A8, A10, A12, A14, A16, A18, A20);
                }
                else if (panelOddAsphere.Visible)
                {
                    z = surfaceCalculations.CalculateOddAsphereSag(r, R, k, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15, A16, A17, A18, A19, A20);
                }
                else if (panelOpalUnZ.Visible)
                {
                    z = surfaceCalculations.CalculateOpalUnZSag(r, R, e2, H, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13);
                }
                else if (panelOpalUnU.Visible)
                {
                    z = surfaceCalculations.CalculateOpalUnUSag(r, R, e2, H, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12);
                }
                else if (panelPoly.Visible)
                {
                    z = surfaceCalculations.CalculatePolySag(r, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13);
                }
                results.Add((r, z));
            }

            ShowResults(results);

            // Save results to tempsurfacedata.txt in the program directory
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempsurfacedata.txt");
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var result in results)
                    {
                        // Format the r value as a decimal with one digit after the decimal point, using fixed width
                        string formattedR = result.r.ToString("0.0", CultureInfo.InvariantCulture).PadRight(1);
                        // Format the z value as a decimal with eight digits after the decimal point
                        string formattedZ = result.z.ToString("0.0000000000000", CultureInfo.InvariantCulture);
                        writer.WriteLine($"{formattedR} {formattedZ}");
                    }
                }
                AppendToConsole($">> Sag Data was saved to {filePath}/tempsurfacedata.txt");

            }
            catch (Exception ex)
            {
                AppendToConsole($">> An error occurred while saving the file: {ex.Message}");
            }
        }

        private void ShowResults(List<(double r, double z)> results)
        {
            Form resultsForm = new Form();
            DataGridView dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dataGridView.Columns.Add("Height", "Surface Height (r)");
            dataGridView.Columns.Add("Sag", "Surface Sag (z)");

            foreach (var result in results)
            {
                dataGridView.Rows.Add(result.r, result.z);
            }

            resultsForm.Controls.Add(dataGridView);
            resultsForm.Text = "Calculation Results";
            resultsForm.Size = new Size(400, 800);
            resultsForm.Show();
        }

        private void toolStripButtonGraph_Click(object sender, EventArgs e)
        {
            // Check if results list is empty
            if (results == null || !results.Any())
            {
                AppendToConsole($">> No data available to plot the graph. You should press Sag Data first.");
                return;
            }

            // Check if all r and z values are zero
            if (results.All(result => result.r == 0 && result.z == 0))
            {
                AppendToConsole($">> All data points are zero. Unable to plot a meaningful graph. Make sure that Radius or Height aren't set to 0, Min Height is less than Max height, and there are no special symbols or letters in text boxes.");
                return;
            }

            // Filter out any NaN values
            var validResults = results.Where(result => !double.IsNaN(result.r) && !double.IsNaN(result.z)).ToList();

            // Check if there are valid data points after filtering
            if (!validResults.Any())
            {
                AppendToConsole($">> No valid data points available to plot the graph. Check if Radius or Height are set to 0");
                return;
            }


            // new form for the graph
            Form graphForm = new Form();
            graphForm.Text = "Surface Graph";
            graphForm.Size = new Size(800, 600);

            // new chart
            Chart chart = new Chart
            {
                Dock = DockStyle.Fill
            };

            // chart area
            ChartArea chartArea = new ChartArea();
            chart.ChartAreas.Add(chartArea);

            // series for both negative and positive values
            Series positiveSeries = new Series
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.Double,
                YValueType = ChartValueType.Double
            };

            Series negativeSeries = new Series
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.Double,
                YValueType = ChartValueType.Double
            };

            // sort by r value
            var sortedResults = validResults.OrderBy(result => result.r).ToList();

            // Add data points for positive r values
            foreach (var result in sortedResults)
            {
                positiveSeries.Points.AddXY(result.z, result.r);
            }

            // Add data points for negative r values
            foreach (var result in sortedResults)
            {
                negativeSeries.Points.AddXY(result.z, -result.r);
            }

            // Add the series to the chart
            chart.Series.Add(positiveSeries);
            chart.Series.Add(negativeSeries);

            // Set axis properties for 1:1 scale
            chartArea.AxisX.IsStartedFromZero = true;
            chartArea.AxisY.IsStartedFromZero = true;
            chartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chartArea.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;

            // Calculate the maximum range for both axes
            double maxRange = Math.Max(sortedResults.Max(r => Math.Abs(r.r)), sortedResults.Max(r => Math.Abs(r.z)));

            // Set the same range for both axes
            chartArea.AxisX.Minimum = -maxRange;
            chartArea.AxisX.Maximum = maxRange;
            chartArea.AxisY.Minimum = -maxRange;
            chartArea.AxisY.Maximum = maxRange;

            // Add the chart to the form
            graphForm.Controls.Add(chart);

            AppendToConsole($">> The surface graph was successfully generated");
            // Show the graph form
            graphForm.Show();
        }

        private void saveSagDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (results.Count == 0)
            {
                AppendToConsole($">> No sag data available to save. Please calculate the sag data first.");
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.Title = "Save Sag Data";
                saveFileDialog.FileName = "SagData.txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                        {
                            // Use fixed-width formatting for headers
                            writer.WriteLine("Y-coord\tSag");
                            writer.WriteLine("       \t");

                            foreach (var result in results)
                            {
                                // Format the r value as a decimal with one digit after the decimal point, using fixed width
                                string formattedR = result.r.ToString("0.0", CultureInfo.InvariantCulture).PadRight(8);
                                // Format the z value as a decimal with eight digits after the decimal point
                                string formattedZ = result.z.ToString("0.000000000000", CultureInfo.InvariantCulture);
                                writer.WriteLine($"{formattedR}\t{formattedZ}");
                            }
                        }
                        AppendToConsole($">> Sag data saved successfully.");
                    }
                    catch (Exception ex)
                    {
                        AppendToConsole($">> An error occurred while saving the file: {ex.Message}");
                    }
                }
            }
        }

        private void toolStripButtonConvert_Click(object sender, EventArgs e)
        {
            ConvertSurfaceForm convertSurfaceForm = new ConvertSurfaceForm(this);
            convertSurfaceForm.Show();
            AppendToConsole($">> Surface converter was succesfully loaded");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

        private void documentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the path to the application's folder
                string appFolderPath = AppDomain.CurrentDomain.BaseDirectory;

                // Combine the folder path with the PDF file name
                string pdfFilePath = System.IO.Path.Combine(appFolderPath, "Documentation.pdf");

                // Use ProcessStartInfo to specify the file and use the default application
                var processStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = pdfFilePath,
                    UseShellExecute = true // This ensures the file is opened with the default application
                };

                // Start the process
                System.Diagnostics.Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                AppendToConsole($">> An error occurred while trying to open the documentation: {ex.Message}");
            }
        }

        private void russianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeLanguage(new CultureInfo("ru"));
        }

        private void defaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeLanguage(new CultureInfo(""));
        }

        private void ChangeLanguage(CultureInfo cultureInfo)
        {
            // Set the current UI culture
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            // Update the main form
            ApplyResourceToControl(this, new ComponentResourceManager(typeof(SurfaceMaster)), cultureInfo);

            // Update other forms
            ApplyResourceToForm(convertSurfaceForm, cultureInfo);
            ApplyResourceToForm(zmxDataDialogue, cultureInfo);
            ApplyResourceToForm(aboutForm, cultureInfo);

            // Reinitialize panels based on the current selection
            ComboBoxSurfaceType_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void ApplyResourceToForm(Form form, CultureInfo cultureInfo)
        {
            ComponentResourceManager cmp = new ComponentResourceManager(form.GetType());
            ApplyResourceToControl(form, cmp, cultureInfo);
        }

        private void ApplyResourceToControl(
            Control control,
            ComponentResourceManager cmp,
            CultureInfo cultureInfo)
        {
            cmp.ApplyResources(control, control.Name, cultureInfo);

            foreach (Control child in control.Controls)
            {
                ApplyResourceToControl(child, cmp, cultureInfo);
            }

            // Special handling for MenuStrip items
            if (control is MenuStrip menuStrip)
            {
                foreach (ToolStripMenuItem item in menuStrip.Items)
                {
                    ApplyResourceToToolStripItem(item, cmp, cultureInfo);
                }
            }
        }

        private void ApplyResourceToToolStripItem(
            ToolStripItem item,
            ComponentResourceManager cmp,
            CultureInfo cultureInfo)
        {
            cmp.ApplyResources(item, item.Name, cultureInfo);

            if (item is ToolStripMenuItem menuItem)
            {
                foreach (ToolStripItem subItem in menuItem.DropDownItems)
                {
                    ApplyResourceToToolStripItem(subItem, cmp, cultureInfo);
                }
            }
        }

    }
}
