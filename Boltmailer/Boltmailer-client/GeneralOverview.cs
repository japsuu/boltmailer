using Boltmailer_common;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Boltmailer_client
{
    public partial class GeneralOverview : Form
    {
        readonly string EMPLOYEES_ROOT_PATH = ConfigurationManager.AppSettings.Get("employeesRootPath");

        FileSystemWatcher allProjectsWatcher;
        FileSystemWatcher userProjectsWatcher;

        Dictionary<ProjectInfo, string> projectPaths = new Dictionary<ProjectInfo, string>();

        bool sendNotifications;
        bool showCompleted;
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

        void Initialize()
        {
            // Try to set the search term based on saved string
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

        private void SendCreatedNotification(object sender, FileSystemEventArgs e)
        {
            if (
                e.FullPath.ToLower().Contains(FilterEmployeesBox.Text) &&
                !(e.FullPath.Substring(e.FullPath.LastIndexOf('\\') + 1) == "lock") &&
                !(e.FullPath.Substring(e.FullPath.LastIndexOf('\\') + 1) == "notes") &&
                !(e.FullPath.Substring(e.FullPath.LastIndexOf('\\') + 1) == "info.json") &&
                !(e.FullPath.Substring(e.FullPath.LastIndexOf('.') + 1) == "eml"))
            {
                if (e != null && sendNotifications)
                {
                    new ToastContentBuilder()
                        .AddArgument("action", "openProj")
                        .AddArgument("path", e.FullPath)
                        .AddText("Uusi Projekti / Päivitys projektiin:")
                        .AddText(e.FullPath.Substring(e.FullPath.LastIndexOf('\\') + 1))
                        .Show();
                }
            }
        }

        void InvokeRefresh(object sender, FileSystemEventArgs e)
        {
            try
            {
                allProjectsWatcher.EnableRaisingEvents = false;

                // Don't update for lockfile changes
                if (e != null && e.FullPath.Contains("\\lock"))
                    return;

                if (sender is FileSystemWatcher)
                {
                    System.Diagnostics.Debug.WriteLine("FS kutsuu päivityksen koska: " + e.FullPath + "\n" + e.ChangeType.ToString());
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

        void InvokeUI(Action a)
        {
            this.BeginInvoke(new MethodInvoker(new Action(() => { RunDelayedAction(500, a); })));
        }

        void RefreshView()
        {
            ProjectsDataGrid.Rows.Clear();
            UpdateProjectGrid();
            NotifyRefresh();
        }

        void NotifyRefresh()
        {
            DebugLabel.Text = "Päivitetty: " + DateTime.Now.ToString("HH:mm");
        }

        void UpdateProjectGrid()
        {
            ProjectsDataGrid.Rows.Clear();

            // Reset the projectPaths dictionary
            projectPaths = new Dictionary<ProjectInfo, string>();

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

                // Add all the projects to GridView as rows
                for (int i = 0; i < projects.Count; i++)
                {
                    ProjectInfo info = projects[i];

                    // Get the employee name, in original format, from the folder name
                    string employee = directory.Substring(directory.LastIndexOf('\\') + 1).Replace('_', ' ').Replace('-', ' ').ToLower();

                    // Only add the divider for the last row
                    //if (i == projects.Count - 1)
                    //    ProjectsDataGrid.Rows.Add(GetRow(info, employee, true));
                    //else
                        ProjectsDataGrid.Rows.Add(GetRow(info, employee, false));
                }
            }
            FilterEmployees(FilterEmployeesBox.Text);
            ProjectsDataGrid.ClearSelection();
            ProjectsDataGrid.CurrentCell = null;
            //MessageBox.Show("Päivitetty");
        }

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
                /////if (showCompleted)  //TODO: Now why the fuck does this work without contents of this check?
                /////{
                /////    
                /////}
                /////else
                /////{
                /////    ProjectInfo info = (ProjectInfo)row.Cells[5].Value;
                /////    if ((row.Cells[0].Value.ToString().ToLower().Contains(query) || row.Cells[0].Value.ToString().ToLower().Contains("vapaa")) && info.Status != ProjectStatus.Palautettu)
                /////    {
                /////        row.Visible = true;
                /////        continue;
                /////    }
                /////    else
                /////    {
                /////        row.Visible = false;
                /////    }
                /////}
            }
            //if (!string.IsNullOrEmpty(query))
            //{
            //}
            //else
            //{
            //    foreach (DataGridViewRow row in ProjectsDataGrid.Rows)
            //    {
            //        row.Visible = true;
            //    }
            //}
        }

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

        DataGridViewRow GetRow(ProjectInfo info, string employee, bool last)
        {
            DataGridViewRow row = new DataGridViewRow();

            // Check if the project is unassigned
            if (employee == "vapaa")
            {
                row.DefaultCellStyle = GetUnassignedCellStyle();
            }
            else
            {
                row.DefaultCellStyle = GetDefaultCellStyle();
            }

            row.Cells.AddRange(new DataGridViewTextBoxCell[]
            {
                new DataGridViewTextBoxCell { Value = employee },
                new DataGridViewTextBoxCell { Value = info.ProjectName },
                new DataGridViewTextBoxCell { Value = info.Status.ToString() },
                new DataGridViewTextBoxCell { Value = info.Deadline },
                new DataGridViewTextBoxCell { Value = info.TimeEstimate },
                new DataGridViewTextBoxCell { ValueType = typeof(ProjectInfo), Value = info }
            });

            row.Cells[2].Style = GetStatusCellStyle(info.Status);

            if (last)
                row.DividerHeight = 10;

            return row;
        }

        DataGridViewCellStyle GetUnassignedCellStyle()
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle
            {
                BackColor = Color.LightSkyBlue
            };
            return style;
        }

        DataGridViewCellStyle GetDefaultCellStyle()
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle
            {

            };
            return style;
        }

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
            /*
             * DataGridViewCell cell = ProjectsDataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                ProjectInfo info = (ProjectInfo)ProjectsDataGrid.Rows[e.RowIndex].Cells[5].Value;
                string employee = (string)ProjectsDataGrid.Rows[e.RowIndex].Cells[0].Value;

                // Apply colors based on unassigned or not
                if (employee == "vapaa")
                {
                    ProjectsDataGrid.Rows[e.RowIndex].DefaultCellStyle = GetUnassignedCellStyle();
                }
                else
                {
                    ProjectsDataGrid.Rows[e.RowIndex].DefaultCellStyle = GetDefaultCellStyle();
                }

                // Apply colors based on status
                ProjectsDataGrid.Rows[e.RowIndex].Cells[2].Style = GetStatusCellStyle(info.Status);

                // Highlight the Searched cells
                if (!string.IsNullOrEmpty(SearchBox.Text) && e.Value.ToString().ToLower().Contains(SearchBox.Text.ToLower()))
                {
                    cell.Style = new DataGridViewCellStyle(cell.Style) { BackColor = Color.Chartreuse };
                }
             */

            e.AdvancedBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;
            if (e.RowIndex < 1 || e.ColumnIndex < 0)
                return;

            if (CompareCellValues(e.ColumnIndex, e.RowIndex) && e.ColumnIndex == 0)
            {
                e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
            }
            else
            {
                e.AdvancedBorderStyle.Top = ProjectsDataGrid.AdvancedCellBorderStyle.Top;
            }

            //if (ProjectsDataGrid.Rows.Count > e.RowIndex + 1)
            //{
            //    int comparedRow = 1;
            //    while ((ProjectsDataGrid.Rows.Count > e.RowIndex + comparedRow) && !ProjectsDataGrid.Rows[comparedRow].Visible)
            //    {
            //        if (ProjectsDataGrid.Rows.Count > e.RowIndex + comparedRow)
            //        {
            //            break;
            //        }
            //        comparedRow++;
            //    }
            //    //TODO: Try with .Equals on the two objects?
            //    if (ProjectsDataGrid.Rows[e.RowIndex].Cells[0].ToolTipText != ProjectsDataGrid.Rows[e.RowIndex + comparedRow].Cells[0].ToolTipText)
            //    {
            //        ProjectsDataGrid.Rows[e.RowIndex].DividerHeight = 10;
            //    }
            //}

            //if (ProjectsDataGrid.Rows.Count > e.RowIndex + 1)
            //{
            //    //TODO: It's a bit hacky, but it works!
            //    if (ProjectsDataGrid.Rows[e.RowIndex].Cells[0].ToolTipText != ProjectsDataGrid.Rows[e.RowIndex + 1].Cells[0].ToolTipText)
            //    {
            //
            //
            //        ProjectsDataGrid.Rows[e.RowIndex].DividerHeight = 10;
            //    }
            //}
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
            if (e.RowIndex == 0 || e.ColumnIndex != 0)
                return;

            if (CompareCellValues(e.ColumnIndex, e.RowIndex))
            {
                e.Value = "          \u21B3";
                e.FormattingApplied = true;
            }

            if(!CompareCellValues(e.ColumnIndex, e.RowIndex))
            {
                ProjectsDataGrid.Rows[e.RowIndex - 1].DividerHeight = 10;
            }
        }

        #endregion

        #region Events

        void ProjectsDataGrid_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                DataGridView.HitTestInfo hitTest = ProjectsDataGrid.HitTest(e.X, e.Y);

                if (hitTest.Type == DataGridViewHitTestType.None)
                {
                    ProjectsDataGrid.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Virhe: " + ex);
            }
        }

        private void ProjectsDataGrid_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void ProjectsDataGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Get the ProjectInfo
            ProjectInfo info = (ProjectInfo)ProjectsDataGrid.Rows[e.RowIndex].Cells[ProjectsDataGrid.Rows[e.RowIndex].Cells.Count - 1].Value;

            // Get the project path
            projectPaths.TryGetValue(info, out string path);

            ProjectOverview overview = new ProjectOverview(info, path);
            overview.TopMost = AlwaysOnTopCheckbox.Checked;
            overview.ShowDialog();
        }

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

        private void GeneralOverview_Load(object sender, EventArgs e)
        {

        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            ProjectsDataGrid.Refresh();
        }

        private void AlwaysOnTopCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = AlwaysOnTopCheckbox.Checked;

            ModifyConfig("alwaysOnTop", AlwaysOnTopCheckbox.Checked.ToString().ToLower());
        }

        private void SendNotificationsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            sendNotifications = SendNotificationsCheckbox.Checked;

            ModifyConfig("sendNotifications", SendNotificationsCheckbox.Checked.ToString().ToLower());
        }

        private void ShowCompletedProjectsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            showCompleted = ShowCompletedProjectsCheckbox.Checked;

            ModifyConfig("showCompletedProjects", ShowCompletedProjectsCheckbox.Checked.ToString().ToLower());

            if (initialized)
            {
                UpdateProjectGrid();
            }
        }

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
