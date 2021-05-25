using Boltmailer_common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boltmailer_client
{
    public partial class GeneralOverview : Form
    {
        const string EMPLOYEES_ROOT_PATH = "C:\\Users\\japsu\\Desktop\\Boltmailer\\Boltmailer\\Boltmailer-mainserver\\bin\\Debug\\netcoreapp3.1\\Projektit";

        FileSystemWatcher watcher;

        DataTable dataTable = new DataTable();

        Dictionary<ProjectInfo, string> projectPaths = new Dictionary<ProjectInfo, string>();

        public GeneralOverview()
        {
            InitializeComponent();
            Initialize();
        }

        void Initialize()
        {
            // Setup the DataGridView
            ProjectsDataGrid.MultiSelect = false;
            ProjectsDataGrid.RowHeadersVisible = false;
            ProjectsDataGrid.AutoGenerateColumns = false;
            ProjectsDataGrid.EnableHeadersVisualStyles = false;
            ProjectsDataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ProjectsDataGrid.ColumnHeadersDefaultCellStyle = GetHeaderStyle();
            ProjectsDataGrid.Columns.AddRange(GetGridViewColumns());

            // Setup the filesystemWatcher
            watcher = new FileSystemWatcher(EMPLOYEES_ROOT_PATH)
            {
                NotifyFilter = NotifyFilters.LastWrite
            };

            watcher.Changed += InvokeRefresh;
            watcher.Created += InvokeRefresh;
            watcher.Deleted += InvokeRefresh;
            watcher.Renamed += InvokeRefresh;

            watcher.Filter = "";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            // Refresh the projects
            InvokeRefresh("Initial refresh", null);
        }

        void InvokeRefresh(object sender, FileSystemEventArgs e)
        {
            try
            {
                watcher.EnableRaisingEvents = false;
                // Don't update for lockfile changes
                if (e != null && e.FullPath.Contains("/lock"))
                    return;

                //if (e != null)
                //    MessageBox.Show("List updated by: " + sender + " because: " + e.FullPath + " " + e.ChangeType);
                //else
                //    MessageBox.Show("List updated by: " + sender + " because: Initial refresh");

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
                watcher.EnableRaisingEvents = true;
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
            ProjectsDataGrid.ClearSelection();
            DebugLabel.Text = "Projektit päivitetty";
        }

        void Highlight(string _query)
        {
            string query = _query.ToLower();
            if (!string.IsNullOrEmpty(query))
            {
                foreach (DataGridViewRow row in ProjectsDataGrid.Rows)
                {
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        if (row.Cells[i].Value.ToString().ToLower().Contains(query) || row.Cells[i].Value.ToString().ToLower().Contains("vapaa"))
                        {
                            row.Visible = true;
                            break;
                        }
                        else
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

            //dt.DefaultView.RowFilter = string.Format("Field = '{0}'", _query);
            //string query = _query.ToLower();
            //foreach (DataGridViewRow row in ProjectsDataGrid.Rows)
            //{
            //    for (int i = 0; i < ProjectsDataGrid.Columns.Count; i++)
            //    {
            //        if (row.Cells[i].Value.ToString().ToLower().Contains(query))
            //        {
            //            DataGridViewCellStyle style = new DataGridViewCellStyle(row.Cells[i].Style) { BackColor = Color.Chartreuse };
            //            row.Cells[i].Style = style;
            //        }
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

            switch (info.Status)
            {
                case ProjectStatus.Aloittamaton:
                    {
                        row.Cells[2].Style = new DataGridViewCellStyle() { BackColor = Color.FromArgb(255, 132, 132) };
                    }
                    break;
                case ProjectStatus.Kesken:
                    {
                        row.Cells[2].Style = new DataGridViewCellStyle() { BackColor = Color.FromArgb(255, 255, 132) };
                    }
                    break;
                case ProjectStatus.Valmis:
                    {
                        row.Cells[2].Style = new DataGridViewCellStyle() { BackColor = Color.FromArgb(132, 255, 132) };
                    }
                    break;
                default:
                    break;
            }

            if (last)
                row.DividerHeight = 10;

            return row;
        }

        void SetDataRow(ProjectInfo info, string employee, bool isLast)
        {
            DataRow row = dataTable.NewRow();

            DataCell[] items = new DataCell[]
            {
                new DataCell(employee, employee, GetDefaultCellStyle()),
                new DataCell(info.ProjectName, info.ProjectName, GetDefaultCellStyle()),
                new DataCell(info.Status, info.Status.ToString(), GetStatusCellStyle(info.Status)),
                new DataCell(info.Deadline, info.Deadline, GetDefaultCellStyle()),
                new DataCell(info.TimeEstimate, info.TimeEstimate, GetDefaultCellStyle()),
                new DataCell(info, isLast.ToString(), GetDefaultCellStyle())
            };

            row.ItemArray = items;
            ProjectsDataGrid.Rows.Add(row);
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
                    DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Työntekijä",
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn()
                {
                    DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Projektin nimi",
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn()
                {
                    DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Tila",
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn()
                {
                    DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Deadline",
                    SortMode = DataGridViewColumnSortMode.NotSortable
                },
                new DataGridViewTextBoxColumn()
                {
                    DefaultCellStyle = GetDefaultCellStyle(),
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

        private void HelpButton_Click(object sender, EventArgs e)
        {

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
        }

        private void GeneralOverview_Load(object sender, EventArgs e)
        {

        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            Highlight(SearchBox.Text);
        }

        #endregion
    }

    public class DataCell
    {
        public object Value { get; set; }
        public string DisplayValue { get; set; }
        public DataGridViewCellStyle DisplayStyle { get; set; }

        public DataCell(object value, string displayValue, DataGridViewCellStyle displayStyle)
        {
            Value = value;
            DisplayValue = displayValue;
            DisplayStyle = displayStyle;
        }
    }
}
