using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing; // Add this for Color
using System.Windows.Forms;

namespace SurfaceMaster
{
    public partial class ZMXdataDialogue : Form
    {
        private DataGridView dataGridView;
        public event Action DataSaved;

        public ZMXdataDialogue()
        {
            InitializeComponent();
            InitializeDataGridView();
        }

        private void InitializeDataGridView()
        {
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White // Set your desired background color here
            };
            // Assuming you have a panel to add the DataGridView to
            // Replace 'yourPanel' with the actual panel name
            panelTabControlSurfaceSelection.Controls.Add(dataGridView);
        }

        public void LoadData(List<Dictionary<string, string>> surfaces)
        {
            if (surfaces == null || surfaces.Count == 0)
            {
                MessageBox.Show("No data to display.");
                return;
            }

            foreach (var surface in surfaces)
            {
                var table = new DataTable();
                foreach (var key in surface.Keys)
                {
                    table.Columns.Add(key);
                }

                var row = table.NewRow();
                foreach (var kvp in surface)
                {
                    row[kvp.Key] = kvp.Value;
                }
                table.Rows.Add(row);

                var tabPage = new TabPage($"Surface {surface["SURF"]}");
                var surfaceDataGridView = new DataGridView
                {
                    DataSource = table,
                    Dock = DockStyle.Fill,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = Color.White // Set your desired background color here
                };
                tabPage.Controls.Add(surfaceDataGridView);
                tabControlSurfaceSelection.TabPages.Add(tabPage);
            }
        }

        private void buttonSelectZMXsurface_Click(object sender, EventArgs e)
        {
            // Check if there is a selected tab
            if (tabControlSurfaceSelection.SelectedTab != null)
            {
                // Get the DataGridView from the selected tab
                var selectedTab = tabControlSurfaceSelection.SelectedTab;
                var surfaceDataGridView = selectedTab.Controls[0] as DataGridView;

                if (surfaceDataGridView != null && surfaceDataGridView.DataSource is DataTable dataTable)
                {
                    // Prepare the data to be written to the file
                    var lines = new List<string>();

                    // Add column headers
                    var columnHeaders = string.Join("\t", dataTable.Columns.Cast<DataColumn>().Select(col => col.ColumnName));
                    lines.Add(columnHeaders);

                    // Add rows
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var rowData = string.Join("\t", row.ItemArray);
                        lines.Add(rowData);
                    }

                    // Write to tempZMXData.txt in the program folder
                    var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempZMXData.txt");
                    System.IO.File.WriteAllLines(filePath, lines);

                    // Close the form
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No data available in the selected tab.");
                }
            }
            else
            {
                MessageBox.Show("No tab is selected.");
            }
            // Raise the event to notify that data has been saved
            DataSaved?.Invoke();

            // Close the form
            this.Close();
        }
    }
}
