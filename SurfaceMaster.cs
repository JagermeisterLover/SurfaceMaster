using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;
using WMPLib;
using System.Net.Http;
using System.Net.Http.Headers; // Make sure to include this at the top
using Newtonsoft.Json;
using System.IO.Compression;
using System.Net;

namespace SurfaceMaster;

public partial class SurfaceMaster : Form
{
    private AboutForm aboutForm;
    private ConvertSurfaceForm convertSurfaceForm;

    private DataGridView dataGridView;

    private string updateDownloadUrl = null;

    private List<(double r, double z, double asphericity, double slope, double normalAberration, double angle)> results
        = new List<(double r, double z, double asphericity, double slope, double normalAberration, double angle)>();
    // Class-level variable

    private readonly SurfaceCalculations surfaceCalculations;
    private ZMXdataDialogue zmxDataDialogue;

    private const string CurrentVersion = "2.0.9";

    // Add Windows Media Player instance
    private WindowsMediaPlayer mediaPlayer;
    private Button playStopButton;
    private bool isPlaying = false; // Track play/stop state

    public SurfaceMaster()
    {
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

        InitializeComponent();
        var _ = consoleTextBox.Handle;
        CheckFilesOnStartup();
        InitializeTextBoxValidation();

        // Initialize other forms
        convertSurfaceForm = new ConvertSurfaceForm(this);
        zmxDataDialogue = new ZMXdataDialogue();
        aboutForm = new AboutForm();
        surfaceCalculations = new SurfaceCalculations();

        // Initialize MP3 player
        InitializeMp3Player();

        // Default surface selection
        ComboBoxSurfaceType.SelectedIndex = 0;
        panelEvenAsphere.Visible = true;
        panelOddAsphere.Visible = false;
        panelOpalUnZ.Visible = false;
        panelOpalUnU.Visible = false;
        panelPoly.Visible = false;

        checkBoxStep.CheckedChanged += (sender, e) =>
        {
            textBoxStep.Enabled = checkBoxStep.Checked;
            if (!checkBoxStep.Checked)
            {
                textBoxStep.Text = "1"; // Default step value
            }
        };


        // Set default state
        textBoxStep.Enabled = false;
        textBoxStep.Text = "1"; // Default step value

    }

    public string EARadiusValue => textBoxEARadius.Text;
    public string OARadiusValue => textBoxOARadius.Text;
    public string OpalUnZRadiusValue => textBoxOpalUnZRadius.Text;
    public string OpalUnURadiusValue => textBoxOpalUnURadius.Text;
    public string PolyA1Value => textBoxPolyA1.Text;

    public void AppendToConsole(string message, bool highlight = false)
    {
        if (consoleTextBox.InvokeRequired)
        {
            consoleTextBox.Invoke(() => AppendToConsole(message, highlight));
        }
        else
        {
            if (highlight)
            {
                // Save the current selection start
                var start = consoleTextBox.TextLength;

                // Append the message
                consoleTextBox.AppendText(message + Environment.NewLine);

                // Select the newly added text
                consoleTextBox.Select(start, message.Length);

                // Apply formatting
                consoleTextBox.SelectionColor = Color.Red; // Change color to red
                consoleTextBox.SelectionFont = new Font(consoleTextBox.Font, FontStyle.Bold); // Make it bold

                // Deselect the text
                consoleTextBox.SelectionLength = 0;
            }
            else
            {
                consoleTextBox.AppendText(message + Environment.NewLine);
            }

            // Scroll to the end
            consoleTextBox.SelectionStart = consoleTextBox.Text.Length;
            consoleTextBox.ScrollToCaret();
        }
    }

    // Event handler for link clicks in the RichTextBox.
    private void ConsoleTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
    {
        if (e.LinkText.StartsWith("update://"))
        {
            string encodedUrl = e.LinkText.Substring("update://".Length);
            string downloadUrl = Uri.UnescapeDataString(encodedUrl);
            StartUpdate(downloadUrl);
        }
    }

    private void CheckFilesOnStartup()
    {
        AppendToConsole($"SurfaceMaster V{CurrentVersion} successfully loaded.");// UPDATE VERSION!!!!
        CheckForUpdatesAsync().ConfigureAwait(false); // Start update check 

        var programFolder = AppDomain.CurrentDomain.BaseDirectory;
        var equationFitterPath = Path.Combine(programFolder, "equationfitter.exe");
        var documentationPath = Path.Combine(programFolder, "documentation.pdf");

        if (File.Exists(equationFitterPath))
            AppendToConsole("EquationFitter V5 is loaded.");
        else
            AppendToConsole("equationfitter.exe is missing.");
    }










    #region updater
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("SurfaceMaster");
                var response = await client.GetAsync("https://api.github.com/repos/JagermeisterLover/SurfaceMaster/releases/latest");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    dynamic release = JsonConvert.DeserializeObject(json);

                    string originalTag = release.tag_name;
                    string cleanedTag = originalTag.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                        ? originalTag.Substring(1)
                        : originalTag;

                    if (new Version(cleanedTag) > new Version(CurrentVersion))
                    {
                        // Get the download URL from the release.
                        string downloadUrl = GetDownloadUrlFromRelease(release);
                        if (!string.IsNullOrEmpty(downloadUrl))
                        {
                            // Store the URL and enable the update button.
                            updateDownloadUrl = downloadUrl;
                            AppendToConsole($"New version {originalTag} available. Click the Update button to download the update.");

                            if (buttonUpdate.InvokeRequired)
                            {
                                buttonUpdate.Invoke(() => buttonUpdate.Enabled = true);
                            }
                            else
                            {
                                buttonUpdate.Enabled = true;
                            }
                        }
                        else
                        {
                            AppendToConsole("Update asset not found in release.");
                        }
                    }
                    else
                    {
                        AppendToConsole("No updates available.");
                    }
                }
                else
                {
                    AppendToConsole("Update check failed: " + response.ReasonPhrase);
                }
            }
        }
        catch (Exception ex)
        {
            AppendToConsole($"Update check failed: {ex.Message}");
        }
    }

    private string GetDownloadUrlFromRelease(dynamic release)
    {
        foreach (var asset in release.assets)
        {
            if (asset.name.ToString().Equals("SurfaceMaster.zip", StringComparison.OrdinalIgnoreCase))
            {
                return asset.browser_download_url.ToString();
            }
        }
        return release.assets.Count > 0
            ? release.assets[0].browser_download_url.ToString()
            : null;
    }

    private void buttonUpdate_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(updateDownloadUrl))
        {
            // Optionally disable the button to prevent multiple clicks.
            buttonUpdate.Enabled = false;
            StartUpdate(updateDownloadUrl);
        }
    }
    private bool IsNewVersion(string latestVersion)
    {
        // Remove any leading 'v' (or 'V') from the version string.
        if (latestVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            latestVersion = latestVersion.Substring(1);
        }
        Version current = new Version(CurrentVersion);
        Version latest = new Version(latestVersion);
        return latest > current;
    }

    private void InsertLink(string displayText, string link)
    {
        // RTF field for a hyperlink.
        string linkRtf = $@"{{\field{{\*\fldinst HYPERLINK ""{link}""}}{{\fldrslt {displayText}}}}}";
        consoleTextBox.SelectedRtf = linkRtf;
    }

    // Appends a message with a clickable update link to the console.
    private void AppendUpdateLink(string latestVersion, string downloadUrl)
    {
        string messageBefore = $"New version {latestVersion} available. Click ";
        string messageAfter = " to update.";

        string encodedUrl = Uri.EscapeDataString(downloadUrl);
        string link = $"update://{encodedUrl}";

        if (consoleTextBox.InvokeRequired)
        {
            consoleTextBox.Invoke(() =>
            {
                consoleTextBox.AppendText(messageBefore);
                InsertLink("here", link);
                consoleTextBox.AppendText(messageAfter + Environment.NewLine);
            });
        }
        else
        {
            consoleTextBox.AppendText(messageBefore);
            InsertLink("here", link);
            consoleTextBox.AppendText(messageAfter + Environment.NewLine);
        }
    }

    private async void StartUpdate(string downloadUrl)
    {
        try
        {
            string tempDir = Path.GetTempPath();
            string zipPath = Path.Combine(tempDir, "SurfaceMasterUpdate.zip");
            string extractPath = Path.Combine(tempDir, "SurfaceMasterUpdate");

            // Cleanup previous updates.
            if (File.Exists(zipPath)) File.Delete(zipPath);
            if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);

            using (var client = new HttpClient(new HttpClientHandler
            {
                UseProxy = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = false
            }))
            {
                client.Timeout = TimeSpan.FromMinutes(5);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("SurfaceMasterUpdater/1.0");

                AppendToConsole($"Starting download from: {downloadUrl}");

                // Configure download with a large buffer.
                const int bufferSize = 1048576; // 1MB buffer.
                using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous))
                {
                    var sw = Stopwatch.StartNew();
                    long totalBytes = response.Content.Headers.ContentLength ?? -1;
                    long totalRead = 0L;
                    var buffer = new byte[bufferSize];
                    int read;
                    int lastReportedProgress = -1;

                    while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fs.WriteAsync(buffer, 0, read);
                        totalRead += read;

                        // Progress reporting with speed.
                        if (totalBytes > 0)
                        {
                            int progressPercentage = (int)((totalRead * 100) / totalBytes);
                            int currentReport = progressPercentage / 10;

                            if (currentReport > lastReportedProgress)
                            {
                                double mbps = (totalRead / (1024.0 * 1024.0)) / sw.Elapsed.TotalSeconds;
                                AppendToConsole($"Download progress: {currentReport * 10}% ({mbps:0.0} MB/s)");
                                lastReportedProgress = currentReport;
                            }
                        }
                    }

                    AppendToConsole($"Download completed in {sw.Elapsed.TotalSeconds:0.0}s");
                }

                // Verify ZIP file.
                using (var file = File.OpenRead(zipPath))
                {
                    byte[] header = new byte[4];
                    await file.ReadAsync(header, 0, 4);
                    if (BitConverter.ToString(header) != "50-4B-03-04")
                    {
                        AppendToConsole("Error: Invalid ZIP file format");
                        return;
                    }
                }

                // Extract files.
                AppendToConsole("Extracting update package...");
                ZipFile.ExtractToDirectory(zipPath, extractPath);

                // Start the updater process.
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string updaterPath = Path.Combine(appDir, "Updater.exe");

                if (!File.Exists(updaterPath))
                {
                    AppendToConsole("Error: Updater.exe not found");
                    return;
                }

                ProcessStartInfo psi = new ProcessStartInfo(updaterPath)
                {
                    Arguments = $"\"{Application.ExecutablePath}\" \"{extractPath}\"",
                    UseShellExecute = true,
                    Verb = "runas" // Request admin privileges.
                };

                AppendToConsole("Launching updater...");
                Process.Start(psi);
                Application.Exit();
            }
        }
        catch (Exception ex)
        {
            AppendToConsole($"Update failed: {ex.Message}");
            Debug.WriteLine($"Update error: {ex}");
        }
    }

    #endregion


    #region duranduran
    private void InitializeMp3Player()
    {
        try
        {
            // Create and configure Windows Media Player instance
            mediaPlayer = new WindowsMediaPlayer();

            // Set MP3 file path
            string mp3FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "invisible.mp3");
            if (File.Exists(mp3FilePath))
            {
                mediaPlayer.URL = mp3FilePath;

                // Stop playback immediately after setting URL to prevent auto-play
                mediaPlayer.controls.stop();

                // Set volume to 50%
                mediaPlayer.settings.volume = 10;
            }
            else
            {
                AppendToConsole("MP3 file not found.");
                AppendToConsole("MP3 file 'invisible.mp3' was not found.");
            }
        }
        catch (Exception ex)
        {
            AppendToConsole($"Error initializing media player: {ex.Message}");
        }
    }

    private void playButton_Click(object sender, EventArgs e)
    {
        try
        {
            if (mediaPlayer == null)
            {
                MessageBox.Show("Media player not initialized.");
                return;
            }

            if (isPlaying)
            {
                // Stop playback
                mediaPlayer.controls.stop();
                playButton.Text = "Play";
                isPlaying = false;
            }
            else
            {
                // Start playback
                mediaPlayer.controls.play();
                playButton.Text = "Stop";
                isPlaying = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }

        if (playButton == null)
        {
            MessageBox.Show("Play/Stop button not initialized.");
            return;
        }

    }
    #endregion



    #region panels
    private void ComboBoxSurfaceType_SelectedIndexChanged(object sender, EventArgs e)
    {
        panelEvenAsphere.Visible = false;
        panelOddAsphere.Visible = false;
        panelOpalUnZ.Visible = false;
        panelOpalUnU.Visible = false;
        panelPoly.Visible = false;


        // Check if SelectedItem is not null before using it
        if (ComboBoxSurfaceType.SelectedItem != null)
            switch (ComboBoxSurfaceType.SelectedItem.ToString())
            {
                case "Even asphere":
                    panelEvenAsphere.Visible = true;
                    AppendToConsole(">>Surface type Even Asphere was selected");
                    break;
                case "Odd asphere":
                    panelOddAsphere.Visible = true;
                    AppendToConsole(">> Surface type Odd Asphere was selected");
                    break;
                case "Opal Universal Z":
                    panelOpalUnZ.Visible = true;
                    AppendToConsole(">> SurfaceType Opal Universal Z was selected");
                    break;
                case "Opal Universal U":
                    panelOpalUnU.Visible = true;
                    AppendToConsole(">> SurfaceType Opal Universal U was selected");
                    break;
                case "Opal polynomial Z":
                    panelPoly.Visible = true;
                    AppendToConsole(">> SurfaceType Opal Polynomial Z was selected");
                    break;
                    // more surface types
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
    #endregion



    // A helper class to hold common input parameters and a delegate for sag calculation.
    private class SagCalculationSettings
    {
        public double R { get; set; }         // Radius (if needed)
        public double k { get; set; }         // Conic constant (if needed)
        public double E2 { get; set; }        // Additional parameter for Opal types
        public double H { get; set; }         // Additional parameter for Opal types
        public double MinR { get; set; }      // Minimum r value (start)
        public double MaxR { get; set; }      // Maximum r value (end)
        public double Step { get; set; }      // Calculation step
                                              // Delegate to calculate sag given an r value.
        public Func<double, double> SagCalculator { get; set; }

        public Func<double, double> SlopeCalculator { get; set; }
    }

    private void toolStripButtonSag_Click(object sender, EventArgs e)
    {
        // Clear previous results.
        results.Clear();

        if (surfaceCalculations == null)
        {
            AppendToConsole("Surface calculations object is not initialized.");
            return;
        }

        // Retrieve input settings (including sag and slope calculation functions) for the visible panel.
        SagCalculationSettings settings = GetSagCalculationSettings();
        if (settings == null)
        {
            // (Error message already reported.)
            return;
        }

        // Calculate sag at the minimum and maximum radial heights.
        double zmin = settings.SagCalculator(settings.MinR);
        double zmax = settings.SagCalculator(settings.MaxR);

        // Use a custom step value if the checkbox is checked.
        double step = checkBoxStep.Checked ? settings.Step : 1.0;
        if (step <= 0)
        {
            AppendToConsole("Step value must be greater than 0.");
            return;
        }

        // Determine best-fit sphere parameters.
        double? R3 = null;
        double? R4 = null;
        double zm = 0, rm = 0, g = 0, Lz = 0;
        if (settings.MinR == 0)
        {
            R3 = surfaceCalculations.CalculateBestFitSphereRadius3Points(settings.MaxR, zmax);
        }
        else
        {
            (R4, zm, rm, g, Lz) =
                surfaceCalculations.CalculateBestFitSphereRadius4Points(settings.MinR, settings.MaxR, zmin, zmax);
        }

        // Loop from minR to maxR (inclusive) and compute sag, asphericity, slope, and aberration of normals.
        for (double r = settings.MinR; r <= settings.MaxR + step / 2; r += step)
        {
            // Ensure we do not overshoot the max due to floating-point issues.
            if (r > settings.MaxR)
                r = settings.MaxR;

            double z = settings.SagCalculator(r);
            if (double.IsNaN(z) || double.IsInfinity(z))
            {
                AppendToConsole($"Invalid calculation at r = {r}. Skipping this value.");
                continue;
            }

            double asphericity = settings.MinR == 0
                ? surfaceCalculations.CalculateAsphericityForR3(r, z, R3.Value, settings.R)
                : surfaceCalculations.CalculateAsphericityForR4(r, z, R4.Value, zm, rm, g, Lz);

            // Calculate the local surface slope using the provided slope calculator delegate.
            double slope = settings.SlopeCalculator(r);
            double angle = Math.Atan(slope) * (180 / Math.PI); // Convert slope to degrees

            // Compute the aberration of normals using your provided method.
            double bestFitR = (settings.MinR == 0) ? R3.Value : R4.Value;
            double normalAberration = 0;
            try
            {
                normalAberration = surfaceCalculations.CalculateAberrationOfNormals(z, r, slope, settings.R);
            }
            catch (DivideByZeroException ex)
            {
                AppendToConsole($"Error calculating aberration of normals at r = {r}: {ex.Message}");
                normalAberration = 0;
            }

            results.Add((Math.Round(r, 1), z, asphericity, slope, normalAberration, angle));
        }

        if (R3.HasValue)
            AppendToConsole($"Best fit sphere radius (3 points): {R3.Value}", true);
        if (R4.HasValue)
        {
            string sign = settings.R < 0 ? "-" : "";
            AppendToConsole($"Best fit sphere radius (4 points): {sign}{R4.Value}", true);
        }

        // Show the results and also save to file (if needed).
        ShowResults(results, settings.MinR);
        SaveResultsToFile(results);
    }
    // Modified GetSagCalculationSettings to assign both SagCalculator and SlopeCalculator.
    private SagCalculationSettings GetSagCalculationSettings()
    {
        // Note: The ParseInput method converts the textbox text to a double,
        // accepting either dot or comma as the decimal separator.
        if (panelEvenAsphere.Visible)
        {
            double R = ParseInput(textBoxEARadius.Text);
            double k = ParseInput(textBoxEAConic.Text);
            double minR = ParseInput(textBoxEAminheight.Text);
            double maxR = ParseInput(textBoxEAHeight.Text);
            double A4 = ParseInput(textBoxEA4.Text);
            double A6 = ParseInput(textBoxEA6.Text);
            double A8 = ParseInput(textBoxEA8.Text);
            double A10 = ParseInput(textBoxEA10.Text);
            double A12 = ParseInput(textBoxEA12.Text);
            double A14 = ParseInput(textBoxEA14.Text);
            double A16 = ParseInput(textBoxEA16.Text);
            double A18 = ParseInput(textBoxEA18.Text);
            double A20 = ParseInput(textBoxEA20.Text);
            double step = ParseInput(textBoxStep.Text);

            return new SagCalculationSettings
            {
                R = R,
                k = k,
                MinR = minR,
                MaxR = maxR,
                Step = step,
                SagCalculator = r => surfaceCalculations.CalculateEvenAsphereSag(r, R, k,
                    A4, A6, A8, A10, A12, A14, A16, A18, A20),
                SlopeCalculator = r => surfaceCalculations.CalculateEvenAsphereSlope(r, R, k,
                    A4, A6, A8, A10, A12, A14, A16, A18, A20)
            };
        }
        else if (panelOddAsphere.Visible)
        {
            double R = ParseInput(textBoxOARadius.Text);
            double k = ParseInput(textBoxOAConic.Text);
            double minR = ParseInput(textBoxOAminheight.Text);
            double maxR = ParseInput(textBoxOAHeight.Text);
            double A3 = ParseInput(textBoxOA3.Text);
            double A4 = ParseInput(textBoxOA4.Text);
            double A5 = ParseInput(textBoxOA5.Text);
            double A6 = ParseInput(textBoxOA6.Text);
            double A7 = ParseInput(textBoxOA7.Text);
            double A8 = ParseInput(textBoxOA8.Text);
            double A9 = ParseInput(textBoxOA9.Text);
            double A10 = ParseInput(textBoxOA10.Text);
            double A11 = ParseInput(textBoxOA11.Text);
            double A12 = ParseInput(textBoxOA12.Text);
            double A13 = ParseInput(textBoxOA13.Text);
            double A14 = ParseInput(textBoxOA14.Text);
            double A15 = ParseInput(textBoxOA15.Text);
            double A16 = ParseInput(textBoxOA16.Text);
            double A17 = ParseInput(textBoxOA17.Text);
            double A18 = ParseInput(textBoxOA18.Text);
            double A19 = ParseInput(textBoxOA19.Text);
            double A20 = ParseInput(textBoxOA20.Text);
            double step = ParseInput(textBoxStep.Text);

            return new SagCalculationSettings
            {
                R = R,
                k = k,
                MinR = minR,
                MaxR = maxR,
                Step = step,
                SagCalculator = r => surfaceCalculations.CalculateOddAsphereSag(r, R, k,
                    A3, A4, A5, A6, A7, A8, A9, A10, A11, A12,
                    A13, A14, A15, A16, A17, A18, A19, A20),
                SlopeCalculator = r => surfaceCalculations.CalculateOddAsphereSlope(r, R, k,
                    A3, A4, A5, A6, A7, A8, A9, A10, A11, A12,
                    A13, A14, A15, A16, A17, A18, A19, A20)
            };
        }
        else if (panelOpalUnZ.Visible)
        {
            double R = ParseInput(textBoxOpalUnZRadius.Text);
            double e2 = ParseInput(textBoxOpalUnZe2.Text);
            double minR = ParseInput(textBoxOpalUnZminheight.Text);
            double maxR = ParseInput(textBoxOpalUnZHeight.Text);
            double H = ParseInput(textBoxOpalUnZH.Text);
            double A3 = ParseInput(textBoxOpalUnZA3.Text);
            double A4 = ParseInput(textBoxOpalUnZA4.Text);
            double A5 = ParseInput(textBoxOpalUnZA5.Text);
            double A6 = ParseInput(textBoxOpalUnZA6.Text);
            double A7 = ParseInput(textBoxOpalUnZA7.Text);
            double A8 = ParseInput(textBoxOpalUnZA8.Text);
            double A9 = ParseInput(textBoxOpalUnZA9.Text);
            double A10 = ParseInput(textBoxOpalUnZA10.Text);
            double A11 = ParseInput(textBoxOpalUnZA11.Text);
            double A12 = ParseInput(textBoxOpalUnZA12.Text);
            double A13 = ParseInput(textBoxOpalUnZA13.Text);
            double step = ParseInput(textBoxStep.Text);

            return new SagCalculationSettings
            {
                R = R,
                E2 = e2,
                H = H,
                MinR = minR,
                MaxR = maxR,
                Step = step,
                SagCalculator = r => surfaceCalculations.CalculateOpalUnZSag(r, R, e2, H,
                    A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13),
                SlopeCalculator = r => surfaceCalculations.CalculateOpalUnZSlope(r, R, e2, H,
                    A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13)
            };
        }
        else if (panelOpalUnU.Visible)
        {
            double R = ParseInput(textBoxOpalUnURadius.Text);
            double e2 = ParseInput(textBoxOpalUnUe2.Text);
            double minR = ParseInput(textBoxOpalUnUminheight.Text);
            double maxR = ParseInput(textBoxOpalUnUHeight.Text);
            double H = ParseInput(textBoxOpalUnUH.Text);
            double A2 = ParseInput(textBoxOpalUnUA2.Text);
            double A3 = ParseInput(textBoxOpalUnUA3.Text);
            double A4 = ParseInput(textBoxOpalUnUA4.Text);
            double A5 = ParseInput(textBoxOpalUnUA5.Text);
            double A6 = ParseInput(textBoxOpalUnUA6.Text);
            double A7 = ParseInput(textBoxOpalUnUA7.Text);
            double A8 = ParseInput(textBoxOpalUnUA8.Text);
            double A9 = ParseInput(textBoxOpalUnUA9.Text);
            double A10 = ParseInput(textBoxOpalUnUA10.Text);
            double A11 = ParseInput(textBoxOpalUnUA11.Text);
            double A12 = ParseInput(textBoxOpalUnUA12.Text);
            double step = ParseInput(textBoxStep.Text);

            return new SagCalculationSettings
            {
                R = R,
                E2 = e2,
                H = H,
                MinR = minR,
                MaxR = maxR,
                Step = step,
                SagCalculator = r => surfaceCalculations.CalculateOpalUnUSag(r, R, e2, H,
                    A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12),
                SlopeCalculator = r => surfaceCalculations.CalculateOpalUnUSlope(r, R, e2, H,
                    A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12)
            };
        }
        else if (panelPoly.Visible)
        {
            double polyRadiusInput = ParseInput(textBoxPolyA1.Text);
            double minR = ParseInput(textBoxPolyminheight.Text);
            double maxR = ParseInput(textBoxPolyHeight.Text);
            double A1 = ParseInput(textBoxPolyA1.Text);
            double A2 = ParseInput(textBoxPolyA2.Text);
            double A3 = ParseInput(textBoxPolyA3.Text);
            double A4 = ParseInput(textBoxPolyA4.Text);
            double A5 = ParseInput(textBoxPolyA5.Text);
            double A6 = ParseInput(textBoxPolyA6.Text);
            double A7 = ParseInput(textBoxPolyA7.Text);
            double A8 = ParseInput(textBoxPolyA8.Text);
            double A9 = ParseInput(textBoxPolyA9.Text);
            double A10 = ParseInput(textBoxPolyA10.Text);
            double A11 = ParseInput(textBoxPolyA11.Text);
            double A12 = ParseInput(textBoxPolyA12.Text);
            double A13 = ParseInput(textBoxPolyA13.Text);
            double step = ParseInput(textBoxStep.Text);

            return new SagCalculationSettings
            {
                // For a polynomial surface, use half of the A1 textbox value as the radius.
                R = polyRadiusInput / 2,
                MinR = minR,
                MaxR = maxR,
                Step = step,
                SagCalculator = r => surfaceCalculations.CalculatePolySag(r,
                    A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13),
                SlopeCalculator = r => surfaceCalculations.CalculatePolySlope(r,
                    A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13)
            };
        }

        AppendToConsole("No valid calculation panel is visible.");
        return null;
    }

    // Helper method that parses a string into a double accepting both dots and commas.
    private double ParseInput(string input)
    {
        input = input.Replace(',', '.');
        if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        return 0;
    }

    // Modified SaveResultsToFile to include slope and normals aberration.
    private void SaveResultsToFile(List<(double r, double z, double asphericity, double slope, double normalAberration, double angle)> results)
    {
        try
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempsurfacedata.txt");
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var result in results)
                {
                    // Format r with one digit after the decimal point and z, slope, aberration with 13 decimal places.
                    var formattedR = result.r.ToString("0.0", CultureInfo.InvariantCulture);
                    var formattedZ = result.z.ToString("0.0000000000000", CultureInfo.InvariantCulture);
                    var formattedSlope = result.slope.ToString("0.0000000000000", CultureInfo.InvariantCulture);
                    var formattedAberration = result.normalAberration.ToString("0.0000000000000", CultureInfo.InvariantCulture);
                    var formattedAngle = result.angle.ToString("0.0000000000000", CultureInfo.InvariantCulture);
                    writer.WriteLine($"{formattedR} {formattedZ} {formattedSlope} {formattedAberration} {formattedAngle}");
                }
            }
            AppendToConsole($">> Sag Data was saved to {filePath}");
        }
        catch (Exception ex)
        {
            AppendToConsole($">> An error occurred while saving the file: {ex.Message}");
        }
    }
    private void ShowResults(List<(double r, double z, double asphericity, double slope, double normalAberration, double angle)> results, double minR)
    {
        var resultsForm = new Form();
        var dataGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText // Allow copying headers.
        };

        // Create numeric columns with proper ValueType and formatting.
        DataGridViewTextBoxColumn colHeight = new DataGridViewTextBoxColumn
        {
            Name = "Height",
            HeaderText = "Surface Height (r)",
            ValueType = typeof(double),
            DefaultCellStyle = { Format = "0.0" }
        };
        dataGridView.Columns.Add(colHeight);

        DataGridViewTextBoxColumn colSag = new DataGridViewTextBoxColumn
        {
            Name = "Sag",
            HeaderText = "Surface Sag (z)",
            ValueType = typeof(double),
            DefaultCellStyle = { Format = "0.0000000000000" }
        };
        dataGridView.Columns.Add(colSag);

        DataGridViewTextBoxColumn colAsphericity = new DataGridViewTextBoxColumn
        {
            Name = "Asphericity",
            HeaderText = (minR == 0 ? "a3" : "a4"),
            ValueType = typeof(double),
            DefaultCellStyle = { Format = "0.0000000000000" }
        };
        dataGridView.Columns.Add(colAsphericity);

        DataGridViewTextBoxColumn colSlope = new DataGridViewTextBoxColumn
        {
            Name = "Slope",
            HeaderText = "Surface Slope",
            ValueType = typeof(double),
            DefaultCellStyle = { Format = "0.0000000000000" }
        };
        dataGridView.Columns.Add(colSlope);

        DataGridViewTextBoxColumn colAngle = new DataGridViewTextBoxColumn
        {
            Name = "Angle",
            HeaderText = "Angle (degrees)",
            ValueType = typeof(double),
            DefaultCellStyle = { Format = "0.0000000000000" }
        };
        dataGridView.Columns.Add(colAngle);

        // DMS angle column stays as text.
        DataGridViewTextBoxColumn colDMSAngle = new DataGridViewTextBoxColumn
        {
            Name = "DMSAngle",
            HeaderText = "Angle (DMS)",
            ValueType = typeof(string)
        };
        dataGridView.Columns.Add(colDMSAngle);

        DataGridViewTextBoxColumn colNormalAberration = new DataGridViewTextBoxColumn
        {
            Name = "NormalAberration",
            HeaderText = "Aberration of Normals",
            ValueType = typeof(double),
            DefaultCellStyle = { Format = "0.0000000000000" }
        };
        dataGridView.Columns.Add(colNormalAberration);

        // Add rows using the raw numeric values.
        foreach (var result in results)
        {
            dataGridView.Rows.Add(
                result.r,
                result.z,
                result.asphericity,
                result.slope,
                result.angle,
                DegreesToDMS(result.angle),
                result.normalAberration);
        }

        resultsForm.Controls.Add(dataGridView);
        resultsForm.Text = "Calculation Results";
        resultsForm.Size = new Size(500, 800);
        resultsForm.Show();
    }
    /// <summary>
    /// Converts an angle in decimal degrees to a string in DMS (degrees, minutes, seconds) format.
    /// </summary>
    private string DegreesToDMS(double angle)
    {
        // Handle negative angles by preserving the sign.
        int sign = angle < 0 ? -1 : 1;
        angle = Math.Abs(angle);

        int degrees = (int)Math.Floor(angle);
        double fraction = angle - degrees;
        int minutes = (int)Math.Floor(fraction * 60);
        double seconds = (fraction * 60 - minutes) * 60;

        string signStr = sign < 0 ? "-" : "";
        return $"{signStr}{degrees}° {minutes}' {seconds:0.000}\"";
    }


    private void toolStripButtonGraph_Click(object sender, EventArgs e)
    {
        // Check if results list is empty
        if (results == null || !results.Any())
        {
            AppendToConsole(">> No data available to plot the graph. You should press Sag Data first.");
            return;
        }

        // Check if all r and z values are zero
        if (results.All(result => result.r == 0 && result.z == 0))
        {
            AppendToConsole(
                ">> All data points are zero. Unable to plot a meaningful graph. Make sure that Radius or Height aren't set to 0, Min Height is less than Max height, and there are no special symbols or letters in text boxes.");
            return;
        }

        // Filter out any NaN values
        var validResults = results.Where(result => !double.IsNaN(result.r) && !double.IsNaN(result.z)).ToList();

        // Check if there are valid data points after filtering
        if (!validResults.Any())
        {
            AppendToConsole(
                ">> No valid data points available to plot the graph. Check if Radius or Height are set to 0");
            return;
        }


        // new form for the graph
        var graphForm = new Form();
        graphForm.Text = "Surface Graph";
        graphForm.Size = new Size(800, 600);

        // new chart
        var chart = new Chart
        {
            Dock = DockStyle.Fill
        };

        // chart area
        var chartArea = new ChartArea();
        chart.ChartAreas.Add(chartArea);

        // series for both negative and positive values
        var positiveSeries = new Series
        {
            ChartType = SeriesChartType.Line,
            XValueType = ChartValueType.Double,
            YValueType = ChartValueType.Double
        };

        var negativeSeries = new Series
        {
            ChartType = SeriesChartType.Line,
            XValueType = ChartValueType.Double,
            YValueType = ChartValueType.Double
        };

        // sort by r value
        var sortedResults = validResults.OrderBy(result => result.r).ToList();

        // Add data points for positive r values
        foreach (var result in sortedResults) positiveSeries.Points.AddXY(result.z, result.r);

        // Add data points for negative r values
        foreach (var result in sortedResults) negativeSeries.Points.AddXY(result.z, -result.r);

        // Add the series to the chart
        chart.Series.Add(positiveSeries);
        chart.Series.Add(negativeSeries);

        // Set axis properties for 1:1 scale
        chartArea.AxisX.IsStartedFromZero = true;
        chartArea.AxisY.IsStartedFromZero = true;
        chartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
        chartArea.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;

        // Calculate the maximum range for both axes
        var maxRange = Math.Max(sortedResults.Max(r => Math.Abs(r.r)), sortedResults.Max(r => Math.Abs(r.z)));

        // Set the same range for both axes
        chartArea.AxisX.Minimum = -maxRange;
        chartArea.AxisX.Maximum = maxRange;
        chartArea.AxisY.Minimum = -maxRange;
        chartArea.AxisY.Maximum = maxRange;

        // Add the chart to the form
        graphForm.Controls.Add(chart);

        AppendToConsole(">> The surface graph was successfully generated");
        // Show the graph form
        graphForm.Show();
    }

    private void saveSagDataToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (results == null || results.Count == 0)
        {
            AppendToConsole(">> No sag data available to save. Please calculate the sag data first.");
            return;
        }

        using (var saveFileDialog = new SaveFileDialog())
        {
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.Title = "Save Sag Data";
            saveFileDialog.FileName = "SagData.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        // Write header with all calculated fields.
                        writer.WriteLine("R\tZ\tAsphericity\tSlope\tNormal Aberration\tAngle (deg)");

                        foreach (var result in results)
                        {
                            var formattedR = result.r.ToString("0.0", CultureInfo.InvariantCulture).PadRight(8);
                            var formattedZ = result.z.ToString("0.0000000000000", CultureInfo.InvariantCulture);
                            var formattedAsphericity = result.asphericity.ToString("0.0000000000000", CultureInfo.InvariantCulture);
                            var formattedSlope = result.slope.ToString("0.0000000000000", CultureInfo.InvariantCulture);
                            var formattedNormalAberration = result.normalAberration.ToString("0.0000000000000", CultureInfo.InvariantCulture);
                            var formattedAngle = result.angle.ToString("0.0000000000000", CultureInfo.InvariantCulture);
                            writer.WriteLine($"{formattedR}\t{formattedZ}\t{formattedAsphericity}\t{formattedSlope}\t{formattedNormalAberration}\t{formattedAngle}");
                        }
                    }

                    AppendToConsole(">> Sag data saved successfully.");
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
        var convertSurfaceForm = new ConvertSurfaceForm(this);
        convertSurfaceForm.Show();
        AppendToConsole(">> Surface converter was succesfully loaded");
    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var aboutForm = new AboutForm();
        aboutForm.ShowDialog();
    }

    private void documentationToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            // Get the path to the application's folder
            var appFolderPath = AppDomain.CurrentDomain.BaseDirectory;

            // Combine the folder path with the PDF file name
            var pdfFilePath = Path.Combine(appFolderPath, "Documentation.pdf");

            // Use ProcessStartInfo to specify the file and use the default application
            var processStartInfo = new ProcessStartInfo
            {
                FileName = pdfFilePath,
                UseShellExecute = true // This ensures the file is opened with the default application
            };

            // Start the process
            Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            // Handle any exceptions that may occur
            AppendToConsole($">> An error occurred while trying to open the documentation: {ex.Message}");
        }
    }


    #region ZMX import

    // This method is triggered when the "Import ZMX" button is clicked.
    private void buttonImportZMX_Click(object sender, EventArgs e)
    {
        // Open a file dialog to select a ZMX file.
        using (var openFileDialog = new OpenFileDialog())
        {
            // Set the file filter to ZMX files and all files.
            openFileDialog.Filter = "ZMX files (*.zmx)|*.zmx|All files (*.*)|*.*";

            // Show the dialog and check if the user selected a file.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file path.
                var filePath = openFileDialog.FileName;

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
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempZMXData.txt");
        AppendToConsole($"Checking file at: {filePath}");

        // Check if the file exists.
        if (File.Exists(filePath))
        {
            // Read all lines from the file.
            var lines = File.ReadAllLines(filePath);
            AppendToConsole("File contents:");
            foreach (var line in lines) AppendToConsole(line);

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
            for (var i = 0; i < headers.Length; i++) data[headers[i]] = values[i];

            AppendToConsole("Parsed data:");
            foreach (var kvp in data) AppendToConsole($"{kvp.Key}: {kvp.Value}");

            // Clear textboxes before filling them with new data.
            ClearTextboxes();

            // Check if the data contains a "TYPE" key and update the UI accordingly.
            if (data.TryGetValue("TYPE", out var type))
            {
                AppendToConsole($"Surface type: {type}");
                if (type == "EVENASPH")
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
                else if (type == "STANDARD")
                    UpdateUI(() =>
                    {
                        ComboBoxSurfaceType.SelectedItem = "Even asphere"; // Set ComboBox selection
                        panelEvenAsphere.Visible = true;
                        textBoxEARadius.Text = data.GetValueOrDefault("RADIUS", "0");
                        textBoxEAHeight.Text = data.GetValueOrDefault("DIAM", "0");
                        textBoxEAConic.Text = data.GetValueOrDefault("CONI", "0");
                    });
                else if (type == "IRREGULA")
                    UpdateUI(() =>
                    {
                        ComboBoxSurfaceType.SelectedItem = "Even asphere"; // Set ComboBox selection
                        panelEvenAsphere.Visible = true;
                        textBoxEARadius.Text = data.GetValueOrDefault("RADIUS", "0");
                        textBoxEAHeight.Text = data.GetValueOrDefault("DIAM", "0");
                        textBoxEAConic.Text = data.GetValueOrDefault("CONI", "0");
                    });
                else if (type == "ODDASPHE")
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
            // Check if the current control is a TextBox.
            if (control is TextBox textBox)
            {
                // Check if the TextBox is not the consoleTextBox.
                if (textBox.Name != "consoleTextBox")
                    // If it is a TextBox and not the consoleTextBox, set its Text property to "0".
                    textBox.Text = "0";
            }
            else
            {
                // If the control is not a TextBox, it might be a container (like a Panel or GroupBox).
                // Recursively call this method to check its child controls.
                ClearTextboxesRecursive(control);
            }
    }

    // This method ensures that UI updates are performed on the UI thread.
    private void UpdateUI(Action action)
    {
        // Check if the current thread is not the UI thread.
        if (InvokeRequired)
            // If not, invoke the action on the UI thread.
            Invoke(action);
        else
            // If already on the UI thread, execute the action directly.
            action();
    }

    // This method parses a ZMX file to extract aspheric surface data.
    private List<Dictionary<string, string>> ParseZmxFile(string filePath)
    {
        var asphericSurfaces = new List<Dictionary<string, string>>();
        Dictionary<string, string> currentSurface = null;
        var isAspheric = false;

        try
        {
            // Try reading the file with UTF-8 encoding first.
            foreach (var line in File.ReadLines(filePath, Encoding.UTF8))
                ProcessLine(line, ref currentSurface, ref isAspheric, asphericSurfaces);
        }
        catch (Exception)
        {
            try
            {
                // If UTF-8 fails, try reading with UTF-16 encoding.
                foreach (var line in File.ReadLines(filePath, Encoding.Unicode))
                    ProcessLine(line, ref currentSurface, ref isAspheric, asphericSurfaces);
            }
            catch (Exception)
            {
                try
                {
                    // If UTF-16 fails, try reading with the system's default encoding (ANSI).
                    foreach (var line in File.ReadLines(filePath, Encoding.Default))
                        ProcessLine(line, ref currentSurface, ref isAspheric, asphericSurfaces);
                }
                catch (Exception ex)
                {
                    // Show an error message if all attempts to read the file fail.
                    MessageBox.Show($"Failed to read the file: {ex.Message}");
                }
            }
        }

        // Add the last surface if it is aspheric.
        if (currentSurface != null && isAspheric) asphericSurfaces.Add(currentSurface);

        return asphericSurfaces;
    }

    // This method processes each line of the ZMX file to extract relevant data.
    private void ProcessLine(string line, ref Dictionary<string, string> currentSurface, ref bool isAspheric,
        List<Dictionary<string, string>> asphericSurfaces)
    {
        var trimmedLine = line.Trim();

        // Check if the line starts a new surface.
        if (trimmedLine.StartsWith("SURF"))
        {
            // If the previous surface was aspheric, add it to the list.
            if (currentSurface != null && isAspheric) asphericSurfaces.Add(currentSurface);
            // Start a new surface.
            currentSurface = new Dictionary<string, string>
            {
                { "SURF", trimmedLine.Split()[1] }
            };
            isAspheric = false;
        }

        // Check if the line indicates an aspheric surface.
        if (trimmedLine.Contains("CONI") || trimmedLine.Contains("PARM")) isAspheric = true;

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
                var curvString = trimmedLine.Split()[1];
                if (double.TryParse(curvString, NumberStyles.Float, CultureInfo.InvariantCulture, out var curvValue) &&
                    curvValue != 0)
                {
                    var radiusValue = 1.0 / curvValue;
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
        textBoxNormalizeUnZ.TextChanged += ValidateTextBoxInput;
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

    #endregion

    private void pictureBoxChangeSign_Click(object sender, EventArgs e)
    {
        // Helper function to change the sign of a textbox value
        void ChangeSign(TextBox textBox)
        {
            // Try parsing with CurrentCulture first (to handle commas or dots based on user locale)
            if (double.TryParse(textBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value))
            {
                // Negate the value
                value = -value;

                // Convert back to string using CurrentCulture to preserve formatting
                textBox.Text = value.ToString(CultureInfo.CurrentCulture);
            }
            else if (double.TryParse(textBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                // Fallback: Parse with InvariantCulture in case of dot-based input
                value = -value;

                // Convert back to string using InvariantCulture to preserve formatting
                textBox.Text = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        // Determine which panel is visible and change signs accordingly
        if (panelEvenAsphere.Visible || panelOddAsphere.Visible || panelOpalUnU.Visible)
        {
            // Change sign of R
            if (panelEvenAsphere.Visible)
            {
                ChangeSign(textBoxEARadius);
                ChangeSign(textBoxEA4);
                ChangeSign(textBoxEA6);
                ChangeSign(textBoxEA8);
                ChangeSign(textBoxEA10);
                ChangeSign(textBoxEA12);
                ChangeSign(textBoxEA14);
                ChangeSign(textBoxEA16);
                ChangeSign(textBoxEA18);
                ChangeSign(textBoxEA20);
            }
            else if (panelOddAsphere.Visible)
            {
                ChangeSign(textBoxOARadius);
                ChangeSign(textBoxOA3);
                ChangeSign(textBoxOA4);
                ChangeSign(textBoxOA5);
                ChangeSign(textBoxOA6);
                ChangeSign(textBoxOA7);
                ChangeSign(textBoxOA8);
                ChangeSign(textBoxOA9);
                ChangeSign(textBoxOA10);
                ChangeSign(textBoxOA11);
                ChangeSign(textBoxOA12);
                ChangeSign(textBoxOA13);
                ChangeSign(textBoxOA14);
                ChangeSign(textBoxOA15);
                ChangeSign(textBoxOA16);
                ChangeSign(textBoxOA17);
                ChangeSign(textBoxOA18);
                ChangeSign(textBoxOA19);
                ChangeSign(textBoxOA20);
            }
            else if (panelOpalUnU.Visible)
            {
                ChangeSign(textBoxOpalUnURadius);
                ChangeSign(textBoxOpalUnUA2);
                ChangeSign(textBoxOpalUnUA3);
                ChangeSign(textBoxOpalUnUA4);
                ChangeSign(textBoxOpalUnUA5);
                ChangeSign(textBoxOpalUnUA6);
                ChangeSign(textBoxOpalUnUA7);
                ChangeSign(textBoxOpalUnUA8);
                ChangeSign(textBoxOpalUnUA9);
                ChangeSign(textBoxOpalUnUA10);
                ChangeSign(textBoxOpalUnUA11);
                ChangeSign(textBoxOpalUnUA12);
            }
        }
        else if (panelOpalUnZ.Visible)
        {
            // Change sign of R and even coefficients
            ChangeSign(textBoxOpalUnZRadius);
            ChangeSign(textBoxOpalUnZA4);
            ChangeSign(textBoxOpalUnZA6);
            ChangeSign(textBoxOpalUnZA8);
            ChangeSign(textBoxOpalUnZA10);
            ChangeSign(textBoxOpalUnZA12);
        }
        else if (panelPoly.Visible)
        {
            // Change sign of R and odd coefficients
            ChangeSign(textBoxPolyA1);
            ChangeSign(textBoxPolyA3);
            ChangeSign(textBoxPolyA5);
            ChangeSign(textBoxPolyA7);
            ChangeSign(textBoxPolyA9);
            ChangeSign(textBoxPolyA11);
            ChangeSign(textBoxPolyA13);
        }
    }



    private async void SurfaceMaster_FormClosing(object sender, FormClosingEventArgs e)
    {
        e.Cancel = true; // Prevent immediate closing
        Hide(); // Hide the main form

        // Show the GIF form
        GifForm gifForm = new GifForm();
        gifForm.Show();

        gifForm.Refresh(); // Force the UI to update and play GIF

        await Task.Delay(1800); // Wait for 2 seconds

        gifForm.Close(); // Close the GIF form

        Environment.Exit(0); // Exit the application
    }

    private void buttonNormalizeUnZ_Click(object sender, EventArgs e)
    {
        // Parse normalization value
        double normalizationValue = ParseInput(textBoxNormalizeUnZ.Text);
        if (normalizationValue <= 0)
        {
            AppendToConsole("Normalization value must be a positive number.", true);
            return;
        }

        // Parse current H value
        double currentH = ParseInput(textBoxOpalUnZH.Text);
        if (currentH <= 0)
        {
            AppendToConsole("Current H value must be a positive number.", true);
            return;
        }

        if (currentH == normalizationValue)
        {
            AppendToConsole("H is already equal to the normalization value. No changes made.");
            return;
        }

        // Calculate the ratio (normalizationValue / currentH)
        double ratio = normalizationValue / currentH;

        // Define coefficients and their exponents (A3 to A13)
        var coefficients = new[]
        {
        new { TextBox = textBoxOpalUnZA3, Exponent = 3 },
        new { TextBox = textBoxOpalUnZA4, Exponent = 4 },
        new { TextBox = textBoxOpalUnZA5, Exponent = 5 },
        new { TextBox = textBoxOpalUnZA6, Exponent = 6 },
        new { TextBox = textBoxOpalUnZA7, Exponent = 7 },
        new { TextBox = textBoxOpalUnZA8, Exponent = 8 },
        new { TextBox = textBoxOpalUnZA9, Exponent = 9 },
        new { TextBox = textBoxOpalUnZA10, Exponent = 10 },
        new { TextBox = textBoxOpalUnZA11, Exponent = 11 },
        new { TextBox = textBoxOpalUnZA12, Exponent = 12 },
        new { TextBox = textBoxOpalUnZA13, Exponent = 13 },
    };

        foreach (var coeff in coefficients)
        {
            double currentA = ParseInput(coeff.TextBox.Text);
            double factor = Math.Pow(ratio, coeff.Exponent); // (normalizationValue / currentH)^i
            double newA = currentA * factor;

            // Update textbox with normalized value (use invariant culture)
            coeff.TextBox.Text = newA.ToString("0.###############", CultureInfo.InvariantCulture);
        }

        // Update H to the normalization value
        textBoxOpalUnZH.Text = normalizationValue.ToString(CultureInfo.InvariantCulture);
        AppendToConsole(">> Coefficients normalized successfully. H updated.");
    }

    private void buttonConvertToUnZ_Click(object sender, EventArgs e)
    {
        // 1. Parse the Poly coefficients.
        //    Note: textBoxPolyA1 holds a value that when divided by 2 gives R.
        double polyA1;
        try
        {
            polyA1 = ParseInput(textBoxPolyA1.Text);
        }
        catch (Exception ex)
        {
            AppendToConsole("Error parsing Poly A1: " + ex.Message, true);
            return;
        }

        // Calculate R from Poly A1 (R = textBoxPolyA1 / 2).
        double R = polyA1 / 2.0;

        // According to the instructions H is set to 1 for this conversion.
        double H = 1.0;

        // 2. Parse the higher–order coefficients A3 ... A13 from the Poly form.
        //    (We ignore A2 here, as only orders >=3 are converted.)
        Dictionary<int, double> coeffs_eq1 = new Dictionary<int, double>();
        try
        {
            coeffs_eq1[3] = ParseInput(textBoxPolyA3.Text);
            coeffs_eq1[4] = ParseInput(textBoxPolyA4.Text);
            coeffs_eq1[5] = ParseInput(textBoxPolyA5.Text);
            coeffs_eq1[6] = ParseInput(textBoxPolyA6.Text);
            coeffs_eq1[7] = ParseInput(textBoxPolyA7.Text);
            coeffs_eq1[8] = ParseInput(textBoxPolyA8.Text);
            coeffs_eq1[9] = ParseInput(textBoxPolyA9.Text);
            coeffs_eq1[10] = ParseInput(textBoxPolyA10.Text);
            coeffs_eq1[11] = ParseInput(textBoxPolyA11.Text);
            coeffs_eq1[12] = ParseInput(textBoxPolyA12.Text);
            coeffs_eq1[13] = ParseInput(textBoxPolyA13.Text);
        }
        catch (Exception ex)
        {
            AppendToConsole("Error parsing Poly coefficients: " + ex.Message, true);
            return;
        }

        // 3. Convert the coefficients from Equation 1 to Equation 2.
        //    The conversion formula is:
        //         A_n^(2) = -(H^n/(2R)) * A_n^(1)   for n >= 3.
        //    With H=1, this simplifies to:
        //         A_n^(2) = -(1/(2R)) * A_n^(1)
        Dictionary<int, double> coeffs_eq2 = new Dictionary<int, double>();
        foreach (var pair in coeffs_eq1)
        {
            int n = pair.Key;
            double a_n = pair.Value;
            if (n < 3)
            {
                AppendToConsole($"Order must be >= 3, got {n}.", true);
                return;
            }
            double converted = -(Math.Pow(H, n) / (2 * R)) * a_n;
            coeffs_eq2[n] = converted;
        }

        // 3a. Also parse textBoxPolyA2 and convert it for the UnZ panel.
        //     The conversion is: UnZ order 2 = (Poly order 2) + 1.
        double polyA2;
        try
        {
            polyA2 = ParseInput(textBoxPolyA2.Text);
        }
        catch (Exception ex)
        {
            AppendToConsole("Error parsing Poly A2: " + ex.Message, true);
            return;
        }
        double unZe2 = polyA2 + 1;

        // 4. Switch panels (make the UnZ panel visible and hide the Poly panel).
        panelOpalUnZ.Visible = true;
        panelPoly.Visible = false;

        // 5. Update the UnZ panel textboxes.
        //    Set H and R (R remains the same) and update A3..A13 with the new values.
        textBoxOpalUnZH.Text = H.ToString(CultureInfo.InvariantCulture);
        textBoxOpalUnZRadius.Text = R.ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxOpalUnZA3.Text = coeffs_eq2[3].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxOpalUnZA4.Text = coeffs_eq2[4].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxOpalUnZA5.Text = coeffs_eq2[5].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxOpalUnZA6.Text = coeffs_eq2[6].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxOpalUnZA7.Text = coeffs_eq2[7].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxOpalUnZA8.Text = coeffs_eq2[8].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxOpalUnZA9.Text = coeffs_eq2[9].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxOpalUnZA10.Text = coeffs_eq2[10].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxOpalUnZA11.Text = coeffs_eq2[11].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxOpalUnZA12.Text = coeffs_eq2[12].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxOpalUnZA13.Text = coeffs_eq2[13].ToString("0.###############", CultureInfo.InvariantCulture);

        // Also update the order 2 textbox using the Poly A2 value (plus 1).
        textBoxOpalUnZe2.Text = unZe2.ToString("0.###############", CultureInfo.InvariantCulture);

        AppendToConsole("Conversion to UnZ coefficients successful.");
    }


    private void buttonConvertToPoly_Click(object sender, EventArgs e)
    {
        // 1. Parse R and H from the UnZ panel.
        double R;
        try
        {
            R = ParseInput(textBoxOpalUnZRadius.Text);
        }
        catch (Exception ex)
        {
            AppendToConsole("Error parsing UnZ Radius: " + ex.Message, true);
            return;
        }

        double H;
        try
        {
            H = ParseInput(textBoxOpalUnZH.Text);
        }
        catch (Exception ex)
        {
            AppendToConsole("Error parsing UnZ H value: " + ex.Message, true);
            return;
        }
        if (H <= 0)
        {
            AppendToConsole("UnZ H must be a positive number.", true);
            return;
        }

        // 2. Parse the higher–order coefficients A3 ... A13 from the UnZ panel.
        Dictionary<int, double> coeffs_eq2 = new Dictionary<int, double>();
        try
        {
            coeffs_eq2[3] = ParseInput(textBoxOpalUnZA3.Text);
            coeffs_eq2[4] = ParseInput(textBoxOpalUnZA4.Text);
            coeffs_eq2[5] = ParseInput(textBoxOpalUnZA5.Text);
            coeffs_eq2[6] = ParseInput(textBoxOpalUnZA6.Text);
            coeffs_eq2[7] = ParseInput(textBoxOpalUnZA7.Text);
            coeffs_eq2[8] = ParseInput(textBoxOpalUnZA8.Text);
            coeffs_eq2[9] = ParseInput(textBoxOpalUnZA9.Text);
            coeffs_eq2[10] = ParseInput(textBoxOpalUnZA10.Text);
            coeffs_eq2[11] = ParseInput(textBoxOpalUnZA11.Text);
            coeffs_eq2[12] = ParseInput(textBoxOpalUnZA12.Text);
            coeffs_eq2[13] = ParseInput(textBoxOpalUnZA13.Text);
        }
        catch (Exception ex)
        {
            AppendToConsole("Error parsing UnZ coefficients: " + ex.Message, true);
            return;
        }

        // 2a. Also parse textBoxOpalUnZe2 and convert it for the Poly panel.
        //     The conversion is: Poly order 2 = (UnZ order 2) - 1.
        double unZe2;
        try
        {
            unZe2 = ParseInput(textBoxOpalUnZe2.Text);
        }
        catch (Exception ex)
        {
            AppendToConsole("Error parsing UnZ order 2: " + ex.Message, true);
            return;
        }
        double polyA2 = unZe2 - 1;

        // 3. Convert the coefficients from Equation 2 to Equation 1.
        //    The conversion formula is:
        //         A_n^(1) = -(2R/(H^n)) * A_n^(2)   for n >= 3.
        Dictionary<int, double> coeffs_eq1 = new Dictionary<int, double>();
        foreach (var pair in coeffs_eq2)
        {
            int n = pair.Key;
            double a_n = pair.Value;
            if (n < 3)
            {
                AppendToConsole($"Order must be >= 3, got {n}.", true);
                return;
            }
            double converted = -(2 * R / Math.Pow(H, n)) * a_n;
            coeffs_eq1[n] = converted;
        }

        // 4. Switch panels (make the Poly panel visible and hide the UnZ panel).
        panelPoly.Visible = true;
        panelOpalUnZ.Visible = false;

        // 5. Update the Poly panel textboxes.
        //    Set textBoxPolyA1 to 2*R (matching the original Poly input)
        //    Set textBoxPolyA2 from the converted UnZ order 2 value (i.e., unZe2 - 1)
        //    Set textBoxPolyA3...A13 from our converted coefficients.
        textBoxPolyA1.Text = (2 * R).ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxPolyA2.Text = polyA2.ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxPolyA3.Text = coeffs_eq1[3].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxPolyA4.Text = coeffs_eq1[4].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxPolyA5.Text = coeffs_eq1[5].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxPolyA6.Text = coeffs_eq1[6].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxPolyA7.Text = coeffs_eq1[7].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxPolyA8.Text = coeffs_eq1[8].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxPolyA9.Text = coeffs_eq1[9].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxPolyA10.Text = coeffs_eq1[10].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxPolyA11.Text = coeffs_eq1[11].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxPolyA12.Text = coeffs_eq1[12].ToString("0.###############", CultureInfo.InvariantCulture);
        textBoxPolyA13.Text = coeffs_eq1[13].ToString("0.###############", CultureInfo.InvariantCulture);

        // Also (again) set textBoxPolyA1 to 2*R if needed.
        textBoxPolyA1.Text = (2 * R).ToString("0.###############", CultureInfo.InvariantCulture);

        AppendToConsole("Conversion to Poly coefficients successful.");
    }

    private void toolStripButtonReport_Click(object sender, EventArgs e)
    {
        string activePanel = GetActivePanel();
        string reportText = GenerateReportText(activePanel);

        // Debug output to verify report content
        Debug.WriteLine("Generated Report:");
        Debug.WriteLine(reportText);

        var reportForm = new ReportForm(activePanel, reportText);
        reportForm.Show();
    }

    private string GenerateReportText(string activePanel)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Surface Master Report - {DateTime.Now}");
        sb.AppendLine($"Surface Type: {activePanel}");
        sb.AppendLine();

        switch (activePanel)
        {
            case "Even asphere":
                AppendEvenAsphereReport(sb);
                break;
            case "Odd asphere":
                AppendOddAsphereReport(sb);
                break;
            case "Opal Universal Z":
                AppendOpalUnZReport(sb);
                break;
            case "Opal Universal U":
                AppendOpalUnUReport(sb);
                break;
            case "Opal polynomial Z":
                AppendPolyReport(sb);
                break;
            default:
                sb.AppendLine("No parameters available for this surface type.");
                break;
        }

        return sb.ToString();
    }
    private void AppendEvenAsphereReport(StringBuilder sb)
    {
        sb.AppendLine("Even Asphere Parameters:");
        sb.AppendLine($"Radius: {textBoxEARadius.Text}");
        sb.AppendLine($"Conic Constant: {textBoxEAConic.Text}");
        sb.AppendLine($"Height Range: {textBoxEAminheight.Text} - {textBoxEAHeight.Text}");
        sb.AppendLine("\nAspheric Coefficients:");
        sb.AppendLine($"A4: {textBoxEA4.Text}");
        sb.AppendLine($"A6: {textBoxEA6.Text}");
        sb.AppendLine($"A8: {textBoxEA8.Text}");
        sb.AppendLine($"A10: {textBoxEA10.Text}");
        sb.AppendLine($"A12: {textBoxEA12.Text}");
        sb.AppendLine($"A14: {textBoxEA14.Text}");
        sb.AppendLine($"A16: {textBoxEA16.Text}");
        sb.AppendLine($"A18: {textBoxEA18.Text}");
        sb.AppendLine($"A20: {textBoxEA20.Text}");
    }

    private void AppendOddAsphereReport(StringBuilder sb)
    {
        sb.AppendLine("Odd Asphere Parameters:");
        sb.AppendLine($"Radius: {textBoxOARadius.Text}");
        sb.AppendLine($"Conic Constant: {textBoxOAConic.Text}");
        sb.AppendLine($"Height Range: {textBoxOAminheight.Text} - {textBoxOAHeight.Text}");
        sb.AppendLine("\nAspheric Coefficients:");
        for (int i = 3; i <= 20; i++)
        {
            var textBox = Controls.Find($"textBoxOA{i}", true).FirstOrDefault() as TextBox;
            if (textBox != null) sb.AppendLine($"A{i}: {textBox.Text}");
        }
    }

    private void AppendOpalUnZReport(StringBuilder sb)
    {
        sb.AppendLine("Opal Universal Z Parameters:");
        sb.AppendLine($"Radius: {textBoxOpalUnZRadius.Text}");
        sb.AppendLine($"e2: {textBoxOpalUnZe2.Text}");
        sb.AppendLine($"H: {textBoxOpalUnZH.Text}");
        sb.AppendLine($"Height Range: {textBoxOpalUnZminheight.Text} - {textBoxOpalUnZHeight.Text}");
        sb.AppendLine("\nCoefficients:");
        for (int i = 3; i <= 13; i++)
        {
            var textBox = Controls.Find($"textBoxOpalUnZA{i}", true).FirstOrDefault() as TextBox;
            if (textBox != null) sb.AppendLine($"A{i}: {textBox.Text}");
        }
    }

    private void AppendOpalUnUReport(StringBuilder sb)
    {
        sb.AppendLine("Opal Universal U Parameters:");
        sb.AppendLine($"Radius: {textBoxOpalUnURadius.Text}");
        sb.AppendLine($"e2: {textBoxOpalUnUe2.Text}");
        sb.AppendLine($"H: {textBoxOpalUnUH.Text}");
        sb.AppendLine($"Height Range: {textBoxOpalUnUminheight.Text} - {textBoxOpalUnUHeight.Text}");
        sb.AppendLine("\nCoefficients:");
        for (int i = 2; i <= 12; i++)
        {
            var textBox = Controls.Find($"textBoxOpalUnUA{i}", true).FirstOrDefault() as TextBox;
            if (textBox != null) sb.AppendLine($"A{i}: {textBox.Text}");
        }
    }

    private void AppendPolyReport(StringBuilder sb)
    {
        // Parse key values
        double a1 = ParseInput(textBoxPolyA1.Text);
        double a2 = ParseInput(textBoxPolyA2.Text);
        double radius = a1 / 2;
        double eSquared = a2 + 1;

        sb.AppendLine("Polynomial Parameters:");
        sb.AppendLine($"A1 (2R): {a1.ToString("0.############", CultureInfo.GetCultureInfo("fr-FR"))}");
        sb.AppendLine($"Radius: {radius.ToString("0.############", CultureInfo.GetCultureInfo("fr-FR"))}");
        sb.AppendLine($"A2 (e2 - 1): {a2.ToString("0.############", CultureInfo.GetCultureInfo("fr-FR"))}");
        sb.AppendLine($"e2: {eSquared.ToString("0.############", CultureInfo.GetCultureInfo("fr-FR"))}");
        sb.AppendLine($"Height Range: {textBoxPolyminheight.Text} - {textBoxPolyHeight.Text}");

        sb.AppendLine("\nHigher order coefficients:");
        for (int i = 3; i <= 13; i++)
        {
            var textBox = Controls.Find($"textBoxPolyA{i}", true).FirstOrDefault();
            var value = textBox != null ? textBox.Text : "0";
            sb.AppendLine($"A{i}: {value.Replace(".", ",")}"); // Force comma decimal separator
        }

        // Build polynomial equation with safe formatting
        var equation = new StringBuilder("y^2 = ");
        equation.Append(FormatPolyCoefficient(a1) + "z");

        // Add A2 term
        if (a2 != 0)
        {
            equation.Append(a2 > 0 ? " + " : " - ");
            equation.Append($"{FormatPolyCoefficient(Math.Abs(a2))}z^2");
        }

        // Add remaining terms
        for (int i = 3; i <= 13; i++)
        {
            var textBox = Controls.Find($"textBoxPolyA{i}", true).FirstOrDefault();
            var coeff = textBox != null ? ParseInput(textBox.Text) : 0;

            if (coeff == 0) continue;

            equation.Append(coeff > 0 ? " + " : " - ");
            equation.Append($"{FormatPolyCoefficient(Math.Abs(coeff))}z^{i}");
        }

        sb.AppendLine("\nPolynomial Equation:");
        sb.AppendLine(equation.ToString());
    }

    private string FormatPolyCoefficient(double value)
    {
        if (value == 0) return "0";

        // Use scientific notation only for values < 0.001
        if (Math.Abs(value) < 0.001 && Math.Abs(value) > 1e-20)
        {
            return value.ToString("0.#####e-0", CultureInfo.GetCultureInfo("fr-FR"));
        }

        // Decimal format with comma and trimmed zeros
        string formatted = value.ToString("0.################", CultureInfo.GetCultureInfo("fr-FR"));

        // Trim trailing zeros after comma
        if (formatted.Contains(','))
        {
            formatted = formatted.TrimEnd('0').TrimEnd(',');
        }

        // Ensure at least one decimal place for values between 0 and 1
        if (formatted.StartsWith("0,") && !formatted.Contains("e"))
        {
            formatted = formatted.PadRight(formatted.Length + 1, '0');
        }

        return formatted;
    }

    private void buttonLoadFittedEquation_Click(object sender, EventArgs e)
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FitReport.txt");
        if (!File.Exists(filePath))
        {
            AppendToConsole("FitReport.txt not found.", true);
            return;
        }

        var parameters = ParseFitReport(filePath);
        if (parameters == null)
        {
            AppendToConsole("Failed to parse FitReport.txt.", true);
            return;
        }

        UpdateUI(() =>
        {
            ComboBoxSurfaceType.SelectedIndex = -1;
            switch (parameters["Type"])
            {
                case "EA":
                    LoadEvenAsphere(parameters);
                    break;
                case "OA":
                    LoadOddAsphere(parameters);
                    break;
                case "OUZ":
                    LoadOpalUnZ(parameters);
                    break;
                case "OUU":
                    LoadOpalUnU(parameters);
                    break;
                case "OP":
                    LoadOpalPolynomial(parameters);
                    break;
                default:
                    AppendToConsole($"Unknown surface type: {parameters["Type"]}", true);
                    break;
            }
        });
    }

    private Dictionary<string, string> ParseFitReport(string filePath)
    {
        var parameters = new Dictionary<string, string>();
        foreach (var line in File.ReadAllLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split('=');
            if (parts.Length != 2) continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();
            parameters[key] = value;
        }
        return parameters;
    }

    private void LoadEvenAsphere(Dictionary<string, string> parameters)
    {
        ComboBoxSurfaceType.SelectedItem = "Even asphere";
        panelEvenAsphere.Visible = true;

        textBoxEARadius.Text = parameters.GetValueOrDefault("R", "0");
        textBoxEAConic.Text = parameters.GetValueOrDefault("k", "0");

        // Map coefficients A4, A6, A8...
        for (int i = 4; parameters.ContainsKey($"A{i}"); i += 2)
        {
            var textBox = Controls.Find($"textBoxEA{i}", true).FirstOrDefault() as TextBox;
            if (textBox != null) textBox.Text = parameters[$"A{i}"];
        }
    }

    private void LoadOddAsphere(Dictionary<string, string> parameters)
    {
        ComboBoxSurfaceType.SelectedItem = "Odd asphere";
        panelOddAsphere.Visible = true;

        textBoxOARadius.Text = parameters.GetValueOrDefault("R", "0");
        textBoxOAConic.Text = parameters.GetValueOrDefault("k", "0");

        // Map coefficients A3, A4, A5...
        for (int i = 3; parameters.ContainsKey($"A{i}"); i++)
        {
            var textBox = Controls.Find($"textBoxOA{i}", true).FirstOrDefault() as TextBox;
            if (textBox != null) textBox.Text = parameters[$"A{i}"];
        }
    }

    private void LoadOpalUnZ(Dictionary<string, string> parameters)
    {
        ComboBoxSurfaceType.SelectedItem = "Opal Universal Z";
        panelOpalUnZ.Visible = true;

        textBoxOpalUnZRadius.Text = parameters.GetValueOrDefault("R", "0");
        textBoxOpalUnZH.Text = parameters.GetValueOrDefault("H", "0");
        textBoxOpalUnZe2.Text = parameters.GetValueOrDefault("e2", "0");

        // Map coefficients A3, A4, A5...
        for (int i = 3; parameters.ContainsKey($"A{i}"); i++)
        {
            var textBox = Controls.Find($"textBoxOpalUnZA{i}", true).FirstOrDefault() as TextBox;
            if (textBox != null) textBox.Text = parameters[$"A{i}"];
        }
    }

    private void LoadOpalUnU(Dictionary<string, string> parameters)
    {
        ComboBoxSurfaceType.SelectedItem = "Opal Universal U";
        panelOpalUnU.Visible = true;

        textBoxOpalUnURadius.Text = parameters.GetValueOrDefault("R", "0");
        textBoxOpalUnUH.Text = parameters.GetValueOrDefault("H", "0");
        textBoxOpalUnUe2.Text = parameters.GetValueOrDefault("e2", "0");

        // Map coefficients A2, A3, A4...
        for (int i = 2; parameters.ContainsKey($"A{i}"); i++)
        {
            var textBox = Controls.Find($"textBoxOpalUnUA{i}", true).FirstOrDefault() as TextBox;
            if (textBox != null) textBox.Text = parameters[$"A{i}"];
        }
    }

    private void LoadOpalPolynomial(Dictionary<string, string> parameters)
    {
        ComboBoxSurfaceType.SelectedItem = "Opal polynomial Z";
        panelPoly.Visible = true;

        textBoxPolyA1.Text = parameters.GetValueOrDefault("A1", "0");
        textBoxPolyA2.Text = parameters.GetValueOrDefault("A2", "0");

        // Map coefficients A3, A4, A5...
        for (int i = 3; parameters.ContainsKey($"A{i}"); i++)
        {
            var textBox = Controls.Find($"textBoxPolyA{i}", true).FirstOrDefault() as TextBox;
            if (textBox != null) textBox.Text = parameters[$"A{i}"];
        }
    }


}


