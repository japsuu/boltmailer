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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Boltmailer_client
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Root path to the folder that contains the employee folders.
        /// </summary>
        const string EMPLOYEES_ROOT_PATH = "C:\\Users\\japsu\\Desktop\\Boltmailer\\Boltmailer\\Boltmailer-mainserver\\bin\\Debug\\netcoreapp3.1\\Projektit";

        Timer checkTicker;
        Timer checkNotifyTicker;
        Stopwatch checkNotifyTimer = new Stopwatch();

        /// <summary>
        /// How often we want to check for new projects. In seconds.
        /// </summary>
        readonly int checkInterval = 30;

        public Form1()
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

            // Setup the Projects View
            ProjectsList.View = View.Details;

            // Make the headers
            ColumnHeader header_employees = new ColumnHeader
            {
                Name = "employees",
                Text = "Työntekijät",
                Width = 200
            };
            ColumnHeader header_projects = new ColumnHeader
            {
                Name = "projects",
                Text = "Projektit",
                Width = 800
            };

            // Add the headers
            ProjectsList.Columns.Add(header_employees);
            ProjectsList.Columns.Add(header_projects);

            // Refresh the projects
            Refresh(null, null);

            checkNotifyTimer.Start();
        }

        /// <summary>
        /// Refreshes the projects view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Refresh(object sender, EventArgs args)
        {
            checkNotifyTicker.Start();
            ProjectsList.Items.Clear();
            InstantiateProjects();
            checkNotifyTimer.Restart();
        }

        /// <summary>
        /// Notifies the user about the refresh that's about to happen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void NotifyRefresh(object sender, EventArgs args)
        {
            if(((checkInterval * 1000 - checkNotifyTimer.ElapsedMilliseconds) / 1000) < checkInterval / 3)
                DebugLabel.Text = "Päivitetään: " + ((checkInterval * 1000 - checkNotifyTimer.ElapsedMilliseconds) / 1000) + "s";
        }

        /// <summary>
        /// Gets all projects and shows them in the projectsView.
        /// </summary>
        void InstantiateProjects()
        {
            // Instantiate all projects
            foreach (string directory in Directory.GetDirectories(EMPLOYEES_ROOT_PATH))
            {
                ListViewItem entry = new ListViewItem();
                List<ProjectInfo> projects = new List<ProjectInfo>();

                // Get all the projects the employee has
                foreach (string projectPath in Directory.GetDirectories(directory))
                {
                    //projects.Add(projectPath.Replace('_', ' ').Replace('-', ' ').Substring(directory.LastIndexOf('\\') + 1));
                    string json = File.ReadAllText(projectPath + "\\info.txt");
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    ProjectInfo info = (ProjectInfo)JsonSerializer.Deserialize(json, typeof(ProjectInfo));
                    projects.Add(info);
                }

                // Get the employee name, in original format
                string employeeName = directory.Substring(directory.LastIndexOf('\\') + 1).Replace('_', ' ').Replace('-', ' ');

                // Check if the "employee" is the 'Free' project folder
                if (employeeName.ToLower() == "vapaa")
                {
                    entry.Text = "Vapaat Projektit";
                    entry.ForeColor = Color.Red;
                }
                else
                {
                    entry.Text = employeeName;
                }

                // Add the projects to the employee
                entry.SubItems.AddRange(projects.Select(n => n.ProjectName).ToArray());

                // Instantiate the row to the list
                ProjectsList.Items.Add(entry);
            }
            DebugLabel.Text = "Projektit päivitetty";
        }
    }
}
