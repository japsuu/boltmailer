using Boltmailer_common;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;

namespace Boltmailer_client
{
    public partial class GeneralOverview : Form
    {
        readonly string EMPLOYEES_ROOT_PATH = ConfigurationManager.AppSettings.Get("employeesRootPath");

        FileSystemWatcher allProjectsWatcher;
        FileSystemWatcher userProjectsWatcher;

        Dictionary<ProjectInfo, string> projectPaths = new Dictionary<ProjectInfo, string>();

        public GeneralOverview()
        {
            InitializeComponent();

            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Virhe");
            }
        }

        void Initialize()
        {
            // Try to set the search term based on saved string
            string searchTerm = ConfigurationManager.AppSettings.Get("employeeSearchTerm");

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

            FilterEmployeesBox.Text = searchTerm;

            // Refresh the projects
            InvokeRefresh("Initial refresh", null);
        }

        private void SendCreatedNotification(object sender, FileSystemEventArgs e)
        {
            if (
                e.FullPath.ToLower().Contains(FilterEmployeesBox.Text) &&
                !(e.FullPath.Substring(e.FullPath.LastIndexOf('\\') + 1) == "lock") &&
                !(e.FullPath.Substring(e.FullPath.LastIndexOf('\\') + 1) == "notes") &&
                !(e.FullPath.Substring(e.FullPath.LastIndexOf('\\') + 1) == "info.json"))
            {
                if (e != null)
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
                if (e != null && e.FullPath.Contains("/lock"))
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
                    IProjectInfo info = JsonTools.ReadJson(projectPath);

                    // If it's the info
                    if (typeof(ProjectInfo).IsAssignableFrom(info.GetType()))
                    {
                        projects.Add((ProjectInfo)info);
                        projectPaths.Add((ProjectInfo)info, projectPath);
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
                    if (i == projects.Count - 1)
                        ProjectsDataGrid.Rows.Add(GetRow(info, employee, true));
                    else
                        ProjectsDataGrid.Rows.Add(GetRow(info, employee, false));
                }
            }
            FilterEmployees(FilterEmployeesBox.Text);
            DebugLabel.Text = "Projektit päivitetty";
            ProjectsDataGrid.ClearSelection();
            ProjectsDataGrid.CurrentCell = null;
        }

        void FilterEmployees(string employee)
        {
            string query = employee.ToLower();
            if (!string.IsNullOrEmpty(query))
            {
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
            else
            {
                foreach (DataGridViewRow row in ProjectsDataGrid.Rows)
                {
                    row.Visible = true;
                }
            }
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
                case ProjectStatus.Valmis:
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
            overview.ShowDialog();
        }

        private void MenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if(e.ClickedItem.Name == "HelpButton")
            {
                HelpBox hbox = new HelpBox();
                hbox.Show();
            }

            if(e.ClickedItem.Name == "AboutButton")
            {
                AboutBox abox = new AboutBox();
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
            if (AlwaysOnTopCheckbox.Checked)
                TopMost = true;
            else
                TopMost = false;
        }

        private void FilterEmployeesBox_TextChanged(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["employeeSearchTerm"].Value = FilterEmployeesBox.Text;
            config.Save(ConfigurationSaveMode.Modified);
            FilterEmployees(FilterEmployeesBox.Text);
        }

        #endregion
    }
}
