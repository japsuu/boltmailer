using Boltmailer_common;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Boltmailer_client
{
    public partial class GeneralOverview : Form
    {
        /// <summary>
        /// Root of where all the employees' folders are located.
        /// </summary>
        readonly string EMPLOYEES_ROOT_PATH = ConfigurationManager.AppSettings.Get("employeesRootPath");

        /// <summary>
        /// Called when any project updates.
        /// </summary>
        FileSystemWatcher allProjectsWatcher;
        /// <summary>
        /// Called when a project which name matches the search updates.
        /// </summary>
        FileSystemWatcher userProjectsWatcher;

        /// <summary>
        /// Key = Info, Value = path for that info
        /// </summary>
        readonly Dictionary<ProjectInfo, string> projectPaths = new Dictionary<ProjectInfo, string>();

        /// <summary>
        /// Key = path, Value = employee name
        /// </summary>
        readonly List<string> employeeDirectories = new List<string>();

        bool sendNotifications;
        bool showCompleted;

        /// <summary>
        /// True if initialization is fully and successfully completed.
        /// </summary>
        bool initialized;

        public GeneralOverview()
        {
            InitializeComponent();

            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ohjelman käynnistämisessä tapahtui virhe:\n\n" + ex.ToString(), "Virhe");
            }
        }

        /// <summary>
        /// Initializes all paths, UI elements, listeners, and reads the config.
        /// </summary>
        void Initialize()
        {
            // Read the config
            try
            {
                FilterEmployeesBox.Text = ConfigurationManager.AppSettings.Get("employeeSearchTerm");

                AlwaysOnTopCheckbox.Checked = bool.Parse(ConfigurationManager.AppSettings.Get("alwaysOnTop"));

                sendNotifications = bool.Parse(ConfigurationManager.AppSettings.Get("sendNotifications"));
                SendNotificationsCheckbox.Checked = sendNotifications;

                showCompleted = bool.Parse(ConfigurationManager.AppSettings.Get("showCompletedProjects"));
                ShowCompletedProjectsCheckbox.Checked = showCompleted;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Virheellinen Config-tiedosto:\n\n" + ex, "Virhe");
            }

            // Setup the DataGridView
            ProjectsDataGrid.MultiSelect = false;
            ProjectsDataGrid.RowHeadersVisible = false;
            ProjectsDataGrid.AutoGenerateColumns = false;
            ProjectsDataGrid.EnableHeadersVisualStyles = false;
            ProjectsDataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ProjectsDataGrid.ColumnHeadersDefaultCellStyle = GetHeaderStyle();
            ProjectsDataGrid.Columns.AddRange(GetGridViewColumns());

            // Setup the filesystemWatcher for all projects
            allProjectsWatcher = new FileSystemWatcher(EMPLOYEES_ROOT_PATH)
            {
                NotifyFilter = NotifyFilters.LastWrite
            };

            // Setup FSW filters
            allProjectsWatcher.Filters.Add("*.eml");

            allProjectsWatcher.Created += InvokeRefresh;
            allProjectsWatcher.Changed += InvokeRefresh;
            allProjectsWatcher.Deleted += InvokeRefresh;
            allProjectsWatcher.Renamed += InvokeRefresh;

            allProjectsWatcher.Filter = "";
            allProjectsWatcher.IncludeSubdirectories = true;
            allProjectsWatcher.EnableRaisingEvents = true;

            // Setup the filesystemwatcher for user's projects
            userProjectsWatcher = new FileSystemWatcher(EMPLOYEES_ROOT_PATH)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size
            };
            userProjectsWatcher.Created += SendCreatedNotification;

            // Set the initialization flag, to get around the "Checkbox checked -called" -error
            initialized = true;

            // Refresh the projects
            InvokeRefresh("Initial refresh", null);
        }

        /// <summary>
        /// Sends a notification about a created/modified project.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SendCreatedNotification(object sender, FileSystemEventArgs e)
        {
            if (
                e.FullPath.ToLower().Contains(FilterEmployeesBox.Text) &&
                !(NamingConventions.FilenameFromPath(e.FullPath) == "lock") &&
                !(NamingConventions.FilenameFromPath(e.FullPath) == "notes") &&
                !(NamingConventions.FilenameFromPath(e.FullPath) == "info.json") &&
                !(NamingConventions.FileExtensionFromPath(e.FullPath) == "eml"))
            {
                if (e != null && sendNotifications)
                {
                    new ToastContentBuilder()
                        .AddArgument("action", "openProj")
                        .AddArgument("path", e.FullPath)
                        .AddText("Uusi Projekti / Päivitys projektiin:")
                        .AddText(NamingConventions.FilenameFromPath(e.FullPath))
                        .Show();
                }
            }
        }

        /// <summary>
        /// Invokes a refresh on the projects grid on the UI thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InvokeRefresh(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Disable event raising to not get multiple calls
                allProjectsWatcher.EnableRaisingEvents = false;

                // Don't update for lockfile changes
                if (e != null && e.FullPath.Contains("\\lock"))
                    return;

                if (sender is FileSystemWatcher)
                {
                    InvokeUI(RefreshView);
                }
                else
                {
                    RefreshView();
                }
            }
            finally
            {
                allProjectsWatcher.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// Calls an action on the UI thread.
        /// </summary>
        /// <param name="a">Action to call</param>
        void InvokeUI(Action a)
        {
            this.BeginInvoke(new MethodInvoker(new Action(() => { RunDelayedAction(500, a); })));
        }

        /// <summary>
        /// Refreshes the projects view. Should not be called manually.
        /// </summary>
        void RefreshView()
        {
            // Clear the rows to not get duplicates
            ProjectsDataGrid.Rows.Clear();

            // Update the grid
            UpdateProjectGrid();

            // Log down the refresh
            NotifyRefresh();
        }

        /// <summary>
        /// Changes the debuglabel to show the time of the last refresh.
        /// </summary>
        void NotifyRefresh()
        {
            DebugLabel.Text = "Päivitetty: " + DateTime.Now.ToString("HH:mm");
        }

        /// <summary>
        /// Updates the projects grid. Should not be called manually.
        /// </summary>
        void UpdateProjectGrid()
        {
            // Clear all the remaining entries to not get duplicates
            ProjectsDataGrid.Rows.Clear();
            employeeDirectories.Clear();
            projectPaths.Clear();

            // Go through all employers' folders
            foreach (string directory in Directory.GetDirectories(EMPLOYEES_ROOT_PATH))
            {
                // Reset the projects list
                List<ProjectInfo> projects = new List<ProjectInfo>();

                // Get all the projects the employee has
                foreach (string projectPath in Directory.GetDirectories(directory))
                {
                    IProjectInfo info = FileTools.ReadJson(projectPath);

                    // If it's the info
                    if (typeof(ProjectInfo).IsAssignableFrom(info.GetType()))
                    {
                        // Only add Completed projects, if user wants so
                        if (showCompleted)
                        {
                            projects.Add((ProjectInfo)info);
                            projectPaths.Add((ProjectInfo)info, projectPath);
                        }
                        else
                        {
                            // Get the info object and compare if the state is Completed
                            ProjectInfo pinfo = (ProjectInfo)info;

                            if(pinfo.Status != ProjectStatus.Palautettu)
                            {
                                projects.Add((ProjectInfo)info);
                                projectPaths.Add((ProjectInfo)info, projectPath);
                            }
                        }

                    }
                    else // It's the error
                    {
                        ProjectInfoError error = (ProjectInfoError)info;
                        MessageBox.Show(error.Error);
                    }
                }

                List<DataGridViewRow> rows = new List<DataGridViewRow>();

                // Add all the projects to GridView as rows
                for (int i = 0; i < projects.Count; i++)
                {
                    ProjectInfo info = projects[i];

                    // Get the employee name, in original format, from the folder name
                    string employee = NamingConventions.EmployeeFromPath(directory);

                    DataGridViewRow row = GetRow(info, employee);

                    rows.Add(row);
                }
                ProjectsDataGrid.Rows.AddRange(rows.ToArray());

                employeeDirectories.Add(directory);
            }

            // Filter the results
            FilterEmployees(FilterEmployeesBox.Text);

            // Clear the automatic selection
            ProjectsDataGrid.ClearSelection();
            ProjectsDataGrid.CurrentCell = null;
        }

        /// <summary>
        /// Hides all rows not matching the searched term.
        /// </summary>
        /// <param name="employee"></param>
        void FilterEmployees(string employee)
        {
            string query = employee.ToLower();
            foreach (DataGridViewRow row in ProjectsDataGrid.Rows)
            {
                if (row.Cells[0].Value.ToString().ToLower().Contains(query) || row.Cells[0].Value.ToString().ToLower().Contains("vapaa"))
                {
                    row.Visible = true;
                    continue;
                }
                else
                {
                    row.Visible = false;
                }
            }
        }

        /// <summary>
        /// Runs an action after 'millisecond' * ms has passed.
        /// </summary>
        /// <param name="millisecond"></param>
        /// <param name="action"></param>
        void RunDelayedAction(int millisecond, Action action)
        {
            var timer = new Timer();
            timer.Tick += delegate
            {
                timer.Stop();
                action.Invoke();
            };

            timer.Interval = millisecond;
            timer.Start();
        }

        /// <summary>
        /// Creates and returns a new Row according to the info provided.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        DataGridViewRow GetRow(ProjectInfo info, string employee)
        {
            DataGridViewRow row = new DataGridViewRow();

            // Check if the project is unassigned for styling
            if (employee == "vapaa")
            {
                row.DefaultCellStyle = GetUnassignedCellStyle();
            }
            else
            {
                row.DefaultCellStyle = GetDefaultCellStyle();
            }

            // Create the cells of the row
            row.Cells.AddRange(new DataGridViewTextBoxCell[]
            {
                new DataGridViewTextBoxCell { Value = employee },
                new DataGridViewTextBoxCell { Value = info.ProjectName },
                new DataGridViewTextBoxCell { Value = info.Status.ToString() },
                new DataGridViewTextBoxCell { Value = info.Deadline },
                new DataGridViewTextBoxCell { Value = info.TimeEstimate },
                new DataGridViewTextBoxCell { ValueType = typeof(ProjectInfo), Value = info }
            });

            // Set the style of the status cell
            row.Cells[2].Style = GetStatusCellStyle(info.Status);

            return row;
        }

        /// <summary>
        /// Gets the style of a cell that has no specific style set.
        /// </summary>
        /// <returns></returns>
        DataGridViewCellStyle GetUnassignedCellStyle()
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle
            {
                BackColor = Color.LightSkyBlue
            };
            return style;
        }

        /// <summary>
        /// Sets default style of all cells without styling.
        /// </summary>
        /// <returns></returns>
        DataGridViewCellStyle GetDefaultCellStyle()
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle
            {

            };
            return style;
        }

        /// <summary>
        /// Gets the styling of a status cell according to the status.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        DataGridViewCellStyle GetStatusCellStyle(ProjectStatus status)
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle(GetDefaultCellStyle());
            switch (status)
            {
                case ProjectStatus.Aloittamaton:
                    {
                        style = new DataGridViewCellStyle() { BackColor = Color.FromArgb(255, 132, 132) };
                    }
                    break;
                case ProjectStatus.Kesken:
                    {
                        style = new DataGridViewCellStyle() { BackColor = Color.FromArgb(255, 255, 132) };
                    }
                    break;
                case ProjectStatus.Palautettu:
                    {
                        style = new DataGridViewCellStyle() { BackColor = Color.FromArgb(132, 255, 132) };
                    }
                    break;
                default:
                    break;
            }
            return style;
        }

        /// <summary>
        /// Creates and returns all the columns for the gridView.
        /// </summary>
        /// <returns></returns>
        DataGridViewTextBoxColumn[] GetGridViewColumns()
        {
            DataGridViewTextBoxColumn[] cols = new DataGridViewTextBoxColumn[]
            {
                new DataGridViewTextBoxColumn()
                {
                    //DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Työntekijä",
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn()
                {
                    //DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Projektin nimi",
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn()
                {
                    //DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Tila",
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn()
                {
                    //DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Deadline",
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn()
                {
                    //DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Aika-arvio",
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn()
                {
                    Visible = false
                }
            }; 

            return cols;
        }

        /// <summary>
        /// Returns column header default style.
        /// </summary>
        /// <returns></returns>
        DataGridViewCellStyle GetHeaderStyle()
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle
            {
                BackColor = Color.LightGray
            };
            return style;
        }

        #region Cell merging stuff

        private void ProjectsDataGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Set cell styling to none
            e.AdvancedBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;

            // Safety checks
            if (e.RowIndex < 1 || e.ColumnIndex < 0)
                return;

            // Hide borders between matching cells
            if (CompareCellValues(e.ColumnIndex, e.RowIndex) && e.ColumnIndex == 0)
            {
                e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
            }
            else
            {
                e.AdvancedBorderStyle.Top = ProjectsDataGrid.AdvancedCellBorderStyle.Top;
            }
        }

        /// <summary>
        /// Compares only visible cells. Given cell and the one below it.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        bool CompareCellValues(int column, int row)
        {
            DataGridViewCell cell1 = ProjectsDataGrid[column, row];
            DataGridViewCell cell2 = ProjectsDataGrid[column, row - 1];

            if (cell1.Value == null || cell2.Value == null)
            {
                return false;
            }

            return cell1.Value.ToString() == cell2.Value.ToString();
        }

        private void ProjectsDataGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Safety checks
            if (e.RowIndex == 0 || e.ColumnIndex != 0)
                return;

            // For matching cells set the bottom one's text to an arrow.
            if (CompareCellValues(e.ColumnIndex, e.RowIndex))
            {
                e.Value = "          \u21B3";
                e.FormattingApplied = true;
            }

            // For not maching cells create a divider between them
            if(!CompareCellValues(e.ColumnIndex, e.RowIndex))
            {
                ProjectsDataGrid.Rows[e.RowIndex - 1].DividerHeight = 10;
            }
        }

        #endregion

        #region Events

        // Unselecting cells
        void ProjectsDataGrid_MouseDown(object sender, MouseEventArgs e)
        {
            // Handles clicking outside the gridView to unselect items
            try
            {
                DataGridView.HitTestInfo hitTest = ProjectsDataGrid.HitTest(e.X, e.Y);

                if (hitTest.Type == DataGridViewHitTestType.None)
                {
                    ProjectsDataGrid.ClearSelection();
                }
            }
            catch
            {
            }
        }

        // Un-used
        private void ProjectsDataGrid_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // un-used
        }

        // Opening project overviews
        private void ProjectsDataGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Get the ProjectInfo
            ProjectInfo info = (ProjectInfo)ProjectsDataGrid.Rows[e.RowIndex].Cells[ProjectsDataGrid.Rows[e.RowIndex].Cells.Count - 1].Value;

            // Get the project path
            projectPaths.TryGetValue(info, out string path);

            // Open an overview of the project
            ProjectOverview overview = new ProjectOverview(info, path, employeeDirectories, (string)ProjectsDataGrid.Rows[e.RowIndex].Cells[0].Value);

            overview.TopMost = AlwaysOnTopCheckbox.Checked;
            overview.ShowDialog();
        }

        // Main window menu strip handling
        private void MenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if(e.ClickedItem.Name == "HelpButton")
            {
                HelpBox hbox = new HelpBox();
                hbox.TopMost = AlwaysOnTopCheckbox.Checked;
                hbox.Show();
            }

            if(e.ClickedItem.Name == "AboutButton")
            {
                AboutBox abox = new AboutBox();
                abox.TopMost = AlwaysOnTopCheckbox.Checked;
                abox.ShowDialog();
            }

            if(e.ClickedItem.Name == "ConfigButton")
            {
                MessageBox.Show("Muokkaathan konfiguraatiota vain, jos tiedät mitä teet.", "Muistutus!");
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                System.Diagnostics.Process.Start("explorer.exe", config.FilePath);
            }
        }

        // Un-used
        private void GeneralOverview_Load(object sender, EventArgs e)
        {

        }

        // Refreshing the proj grid
        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            ProjectsDataGrid.Refresh();
        }

        // Handles alwaysOnTop and saving it to config
        private void AlwaysOnTopCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = AlwaysOnTopCheckbox.Checked;

            ModifyConfig("alwaysOnTop", AlwaysOnTopCheckbox.Checked.ToString().ToLower());
        }

        // Handles sendNotifications and saving it to config
        private void SendNotificationsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            sendNotifications = SendNotificationsCheckbox.Checked;

            ModifyConfig("sendNotifications", SendNotificationsCheckbox.Checked.ToString().ToLower());
        }

        // Handles showCompletedProjects and saving it to config and updating proj view
        private void ShowCompletedProjectsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            showCompleted = ShowCompletedProjectsCheckbox.Checked;

            ModifyConfig("showCompletedProjects", ShowCompletedProjectsCheckbox.Checked.ToString().ToLower());

            if (initialized)
            {
                UpdateProjectGrid();
            }
        }

        // Handles filterEmployees and saving it to config
        private void FilterEmployeesBox_TextChanged(object sender, EventArgs e)
        {
            ModifyConfig("employeeSearchTerm", FilterEmployeesBox.Text);

            FilterEmployees(FilterEmployeesBox.Text);
        }

        #endregion

        /// <summary>
        /// Assigns the value to key in the config file.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static void ModifyConfig(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings[key].Value = value;

            config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
