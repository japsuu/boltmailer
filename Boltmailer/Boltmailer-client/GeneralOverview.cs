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

        readonly int checkInterval = 30;

        Dictionary<ProjectInfo, string> projectPaths = new Dictionary<ProjectInfo, string>();

        Timer checkTicker;
        Timer checkNotifyTicker;
        readonly Stopwatch checkNotifyTimer = new Stopwatch();

        public GeneralOverview()
        {
            InitializeComponent();
            Initialize();
        }

        void Initialize()
        {
            // Start a ticker that checks for changes every checkInterval seconds.
            checkTicker = new Timer();
            checkTicker.Tick += new EventHandler(Refresh);
            checkTicker.Interval = checkInterval * 1000;
            checkTicker.Start();
            checkNotifyTicker = new Timer();
            checkNotifyTicker.Tick += new EventHandler(NotifyRefresh);
            checkNotifyTicker.Interval = 1;
            checkNotifyTicker.Start();
            checkNotifyTimer.Start();

            // Setup the DataGridView
            ProjectsDataGrid.MultiSelect = false;
            ProjectsDataGrid.RowHeadersVisible = false;
            ProjectsDataGrid.AutoGenerateColumns = false;
            ProjectsDataGrid.EnableHeadersVisualStyles = false;
            ProjectsDataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ProjectsDataGrid.ColumnHeadersDefaultCellStyle = GetHeaderStyle();
            ProjectsDataGrid.Columns.AddRange(GetGridViewColumns());

            // Refresh the projects
            Refresh(null, null);
        }

        void Refresh(object sender, EventArgs args)
        {
            checkNotifyTicker.Start();
            ProjectsDataGrid.Rows.Clear();
            UpdateProjectGrid();
            checkNotifyTimer.Restart();
        }

        void NotifyRefresh(object sender, EventArgs args)
        {
            if(((checkInterval * 1000 - checkNotifyTimer.ElapsedMilliseconds) / 1000) < checkInterval / 3)
                DebugLabel.Text = "Päivitetään: " + ((checkInterval * 1000 - checkNotifyTimer.ElapsedMilliseconds) / 1000) + "s";
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
                    string json = File.ReadAllText(projectPath + "\\info.json");
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Converters =
                        {
                            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                        }
                    };
                    try
                    {
                        ProjectInfo info = JsonSerializer.Deserialize<ProjectInfo>(json, options);
                        projects.Add(info);
                        projectPaths.Add(info, projectPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Virheellinen info.json tiedosto, projekti skipataan, ja sitä ei ladata.\n\n" + ex.Message + "\n\n'" + projectPath + "'\n\n", "Virhe ladatessa projektia");
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

        DataGridViewRow GetRow(ProjectInfo info, string employee, bool last)
        {
            DataGridViewRow row = new DataGridViewRow();

            row.Cells.AddRange(new DataGridViewTextBoxCell[]
            {
                new DataGridViewTextBoxCell { Value = employee },
                new DataGridViewTextBoxCell { Value = info.ProjectName },
                new DataGridViewTextBoxCell { Value = info.State.ToString() },
                new DataGridViewTextBoxCell { Value = info.Deadline },
                new DataGridViewTextBoxCell { Value = info.TimeEstimate },
                new DataGridViewTextBoxCell { ValueType = typeof(ProjectInfo), Value = info }
            });

            switch (info.State)
            {
                case ProjectState.Aloittamaton:
                    {
                        row.Cells[2].Style = new DataGridViewCellStyle() { BackColor = Color.FromArgb(255, 132, 132) };
                    }
                    break;
                case ProjectState.Kesken:
                    {
                        row.Cells[2].Style = new DataGridViewCellStyle() { BackColor = Color.FromArgb(255, 255, 132) };
                    }
                    break;
                case ProjectState.Valmis:
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

        void CheckUnassignedRows()
        {
            //// Check if the "employee" is the 'Free' project folder
            //if (employee == "vapaa")
            //{
            //    row.DefaultCellStyle = GetUnassignedCellStyle();
            //}
            //else
            //{
            //    row.DefaultCellStyle = GetDefaultCellStyle();
            //}
        }

        DataGridViewCellStyle GetUnassignedCellStyle()
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle
            {
                BackColor = Color.LightSkyBlue
            };
            return style;
        }

        DataGridViewTextBoxColumn[] GetGridViewColumns()
        {
            DataGridViewTextBoxColumn[] cols = new DataGridViewTextBoxColumn[]
            {
                new DataGridViewTextBoxColumn()
                {
                    DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Työntekijä"
                },
                new DataGridViewTextBoxColumn()
                {
                    DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Projektin nimi"
                },
                new DataGridViewTextBoxColumn()
                {
                    DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Tila"
                },
                new DataGridViewTextBoxColumn()
                {
                    DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Deadline"
                },
                new DataGridViewTextBoxColumn()
                {
                    DefaultCellStyle = GetDefaultCellStyle(),
                    HeaderText = "Aika-arvio"
                },
                new DataGridViewTextBoxColumn()
                {
                    Visible = false
                }
            }; 

            return cols;
        }

        DataGridViewCellStyle GetDefaultCellStyle()
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle
            {

            };
            return style;
        }

        DataGridViewCellStyle GetHeaderStyle()
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle
            {
                BackColor = Color.LightGray
            };
            return style;
        }

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
    }
}
