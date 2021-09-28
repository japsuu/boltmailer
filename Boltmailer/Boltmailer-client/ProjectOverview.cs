using Boltmailer_common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Boltmailer_client
{
    public partial class ProjectOverview : Form
    {
        readonly string projectPath;
        readonly string currentEmployee;
        readonly string lockPath;
        readonly string notesPath;
        readonly string projectsRootPath;

        string projectMoveDestination;

        bool canEdit = false;
        bool employeeHasChanged = false;
        bool setupDone = false;

        readonly ProjectInfo info;

        FileSystemWatcher watcher;

        readonly List<string> employeePaths;

        public ProjectOverview(ProjectInfo info, string notesFilePath, List<string> employeePaths, string currentEmployee, string projectsRootPath)
        {
            InitializeComponent();

            // Set proj name to header
            Text = info.ProjectName;

            // Prepare all paths
            this.currentEmployee = currentEmployee;

            this.employeePaths = employeePaths;

            this.info = info;

            this.projectsRootPath = projectsRootPath;

            projectPath = notesFilePath;

            lockPath = projectPath + "\\lock";

            notesPath = projectPath + "\\notes";

            projectMoveDestination = "none";

            Setup();
        }

        void ProjectOverview_Load(object sender, EventArgs e)
        {
        }

        void Setup()
        {
            // Check if somebody is editing the project currently. If not, lock the directory, if is, disable editing and add a watcher for changes
            CheckEditPerms();

            // Prepare all the UI elements
            PrepareUI();

            setupDone = true;
        }

        void PrepareUI()
        {
            // Set Status selection ComboBox values
            StatusComboBox.DataSource = new StatusComboItem[]
            {
                new StatusComboItem(ProjectStatus.Aloittamaton, "Aloittamaton"),
                new StatusComboItem(ProjectStatus.Kesken, "Kesken"),
                new StatusComboItem(ProjectStatus.Palautettu, "Palautettu")
            };

            // Set the currently selected status
            int statusSelectionIndex = info.Status switch
            {
                ProjectStatus.Aloittamaton => 0,
                ProjectStatus.Kesken => 1,
                ProjectStatus.Palautettu => 2,
                _ => 0,
            };
            StatusComboBox.SelectedIndex = statusSelectionIndex;


            bool isAdminModeEnabled = bool.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("adminMode"));

            if (isAdminModeEnabled)
            {
                EmployeeComboBox.Visible = true;
                AssignToLabel.Visible = true;
                AssignToSelfBtn.Visible = false;

                // Set Employee selection ComboBox values
                List<EmployeeComboItem> employeeComboItems = new List<EmployeeComboItem>();

                foreach (string employeePath in employeePaths)
                {
                    string employeeName = NamingConventions.EmployeeFromPath(employeePath);

                    employeeComboItems.Add(new EmployeeComboItem(employeePath, employeeName));
                }
                EmployeeComboBox.DataSource = employeeComboItems;

                int employeeSelectionIndex = employeePaths.FindIndex(p =>
                {
                    string employee = NamingConventions.EmployeeFromPath(p);

                    return employee.Contains(currentEmployee);
                });

                if (employeeSelectionIndex > -1)
                    EmployeeComboBox.SelectedIndex = employeeSelectionIndex;
            }
            else
            {
                EmployeeComboBox.Visible = false;
                AssignToLabel.Visible = false;
                AssignToSelfBtn.Visible = true;

                if(currentEmployee == System.Configuration.ConfigurationManager.AppSettings.Get("employeeSearchTerm"))
                {
                    AssignToSelfBtn.Text = "Aseta vapaaksi";
                }
            }



            // Set status
            switch (info.Status)
            {
                case ProjectStatus.Aloittamaton:
                    projectstatusLabel.ForeColor = Color.Red;
                    break;
                case ProjectStatus.Kesken:
                    projectstatusLabel.ForeColor = Color.Orange;
                    break;
                case ProjectStatus.Palautettu:
                    projectstatusLabel.ForeColor = Color.Green;
                    break;
                default:
                    break;
            }
            projectstatusLabel.Text = "Status: " + info.Status.ToString();

            // Set deadline
            projectDeadlineLabel.Text = "Deadline: " + info.Deadline;

            // Set time est
            TimeEstimateBox.Text = info.TimeEstimate;

            // Set notes
            HandleNotes();
        }

        void HandleNotes()
        {
            try
            {
                ProjectNotesBox.Text = File.ReadAllText(notesPath);
            }
            catch (Exception)
            {
                MessageBox.Show($"Muistiinpanot-tiedostoa ei ole olemassa.\n\nTiedosto luodaan.", "Varoitus");
                File.CreateText(notesPath).Close();
                try
                {
                    File.SetAttributes(notesPath, FileAttributes.Hidden);
                }
                catch
                {
                }
            }
        }

        void CheckEditPerms()
        {
            if (File.Exists(lockPath))
            {
                DisableEditing();
            }
            else
            {
                canEdit = true;

                File.Create(lockPath).Close();

                try
                {
                    File.SetAttributes(lockPath, FileAttributes.Hidden);
                }
                catch
                {
                }
                Program.LastCreatedLockfilePath = lockPath;
            }
        }

        void DisableEditing()
        {
            MessageBox.Show("Joku muu muokkaa projektin tietoja parhaillaan.\nMuokkaamistila on sammutettu tämän ajaksi.", "Varoitus");
            ProjectNotesBox.Enabled = false;
            TimeEstimateBox.Enabled = false;

            watcher = new FileSystemWatcher(projectPath)
            {
                NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size
            };

            watcher.Changed += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.Filter = "";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        void OnChanged(object sender, FileSystemEventArgs e)
        {
            if(e.Name.ToLower() == "notes")
            {
                if (IsHandleCreated)
                {
                    ProjectNotesBox.BeginInvoke(new Action(() => SetNotes(File.ReadAllText(e.FullPath))));
                }
                else
                {
                    //MessageBox.Show("Not executing on UI thread", "Virhe");
                }

                //ProjectNotesBox.Invoke(new Action(() => SetNotes(File.ReadAllText(e.FullPath))));
            }

            if (e.Name.ToLower() == "info.json")
            {
                if (IsHandleCreated)
                {
                    ProjectNotesBox.BeginInvoke(new Action(() => SetTimeEstimate(File.ReadAllText(e.FullPath))));
                }
                else
                {
                    //MessageBox.Show("Not executing on UI thread", "Virhe");
                }

                //ProjectNotesBox.Invoke(new Action(() => SetTimeEstimate(File.ReadAllText(e.FullPath))));
            }
        }

        void OnDeleted(object sender, FileSystemEventArgs e)
        {
            watcher.Changed -= OnChanged;
            watcher.Deleted -= OnDeleted;
        }

        void SetNotes(string content)
        {
            ProjectNotesBox.Text = content;
        }

        void SetTimeEstimate(string content)
        {
            TimeEstimateBox.Text = content;
        }

        void ProjectOverview_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (canEdit)
            {
                // Change info's time estimate
                info.TimeEstimate = TimeEstimateBox.Text;

                // Write the info
                FileTools.WriteInfo(info, projectPath);

                // Write the notes
                FileTools.WriteAllText(notesPath, ProjectNotesBox.Text);

                // Delete the lockfile
                try
                {
                    File.Delete(lockPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Virhe poistaessa kansion lukitustiedostoa:\n\n{ex.Message}", "Virhe");
                }

                // Move project if needed
                if (employeeHasChanged)
                    MoveProject();

                // Move to free/to self if needed
                if(projectMoveDestination != "none")
                {
                    try
                    {
                        if (File.Exists(projectMoveDestination)) throw new Exception("File already exists!");

                        Directory.Move(projectPath, projectMoveDestination);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not move the project:\n\n" + ex);
                    }
                }
            }
        }

        void MoveProject()
        {
            string employeeToMoveTo = employeePaths[EmployeeComboBox.SelectedIndex] + NamingConventions.FilenameFromPath(projectPath, true);

            try
            {
                Directory.Move(projectPath, employeeToMoveTo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not move the project:\n\n" + ex);
            }
        }

        void FolderOpenButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", projectPath);
        }

        void StatusComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!setupDone)
                return;

            StatusComboItem item = (StatusComboItem)StatusComboBox.SelectedValue;

            switch (item.Status)
            {
                case ProjectStatus.Aloittamaton:
                    projectstatusLabel.ForeColor = Color.Red;
                    break;
                case ProjectStatus.Kesken:
                    projectstatusLabel.ForeColor = Color.Orange;
                    break;
                case ProjectStatus.Palautettu:
                    projectstatusLabel.ForeColor = Color.Green;
                    break;
                default:
                    break;
            }
            projectstatusLabel.Text = "Status: " + item.DisplayText;
            info.Status = item.Status;
            //FileTools.WriteInfo(info, projectPath);
        }

        void EmployeeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!setupDone)
                return;

            if (EmployeeComboBox.SelectedValue.ToString() != currentEmployee)
                employeeHasChanged = true;
        }

        private void AssignToSelfBtn_Click(object sender, EventArgs e)
        {
            if (currentEmployee == System.Configuration.ConfigurationManager.AppSettings.Get("employeeSearchTerm"))
            {
                projectMoveDestination = projectsRootPath + "\\vapaa" + NamingConventions.FilenameFromPath(projectPath, true);
            }
            else
            {
                projectMoveDestination = projectsRootPath + "\\" + NamingConventions.FilenameFromTitle(System.Configuration.ConfigurationManager.AppSettings.Get("employeeSearchTerm")) + NamingConventions.FilenameFromPath(projectPath, true);
            }

            Close();
        }
    }
}
