using Boltmailer_common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Boltmailer_client
{
    public partial class ProjectOverview : Form
    {
        string projectPath;
        string lockPath;
        string notesPath;
        bool canEdit = false;
        FileSystemWatcher watcher;

        public ProjectOverview(ProjectInfo info, string notesFilePath)
        {
            InitializeComponent();
            SetInfo(info, notesFilePath);
        }

        private void ProjectOverview_Load(object sender, EventArgs e)
        {

        }

        void SetInfo(ProjectInfo info, string projectPath)
        {
            this.projectPath = projectPath;
            lockPath = projectPath + "\\lock";
            notesPath = projectPath + "\\notes";

            // Check if somebody is editing the project currently. If not, lock the directory, if is, disable editing and add a watcher for changes
            if (File.Exists(lockPath))
            {
                DisableEditing();
            }
            else
            {
                canEdit = true;
                FileStream stream = File.Create(lockPath);
                stream.Close();
            }

            // Set proj name to header
            Text = info.ProjectName;

            // Set Status selection ComboBox values
            StatusComboBox.DataSource = new StatusComboItem[]
            {
                new StatusComboItem(ProjectStatus.Aloittamaton, "Aloittamaton"),
                new StatusComboItem(ProjectStatus.Kesken, "Kesken"),
                new StatusComboItem(ProjectStatus.Valmis, "Valmis")
            };
            StatusComboBox.SelectedItem = new StatusComboItem(info.Status, info.Status.ToString());

            // Set status
            switch (info.Status)
            {
                case ProjectStatus.Aloittamaton:
                    projectstatusLabel.ForeColor = Color.Red;
                    break;
                case ProjectStatus.Kesken:
                    projectstatusLabel.ForeColor = Color.Orange;
                    break;
                case ProjectStatus.Valmis:
                    projectstatusLabel.ForeColor = Color.Green;
                    break;
                default:
                    break;
            }
            projectstatusLabel.Text = "Status: " + info.Status.ToString();

            // Set deadline
            projectDeadlineLabel.Text = "Deadline: " + info.Deadline;

            // Set time est
            projectTimeEstLabel.Text = "Aika-arvio: " + info.TimeEstimate;

            // Set notes
            try
            {
                ProjectNotesBox.Text = File.ReadAllText(notesPath);
            }
            catch (Exception)
            {
                MessageBox.Show($"Huomio-tiedostoa ei ole olemassa.\n\nTiedosto luodaan.", "Varoitus");
                StreamWriter writer = File.CreateText(notesPath);
                writer.Close();
            }
        }

        void DisableEditing()
        {
            MessageBox.Show("Joku muu muokkaa projektin tietoja parhaillaan.\nMuokkaamistila on sammutettu tämän ajaksi.", "Varoitus");
            ProjectNotesBox.Enabled = false;

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

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if(e.Name.ToLower() == "notes")
                ProjectNotesBox.Invoke(new Action(() => SetNotes(File.ReadAllText(e.FullPath))));
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            watcher.Changed -= OnChanged;
            watcher.Deleted -= OnDeleted;
        }

        void SetNotes(string content)
        {
            ProjectNotesBox.Text = content;
        }

        private void ProjectOverview_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (canEdit)
            {
                File.WriteAllText(notesPath, ProjectNotesBox.Text);
                try
                {
                    File.Delete(lockPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Virhe poistaessa kansion lukitustiedostoa:\n\n{ex.Message}", "Virhe");
                }
            }
        }

        private void FolderOpenButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", projectPath);
        }
    }

    public class StatusComboItem
    {
        public ProjectStatus Status { get; set; }
        public string Text { get; set; }

        public StatusComboItem(ProjectStatus status, string text)
        {
            Status = status;
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
