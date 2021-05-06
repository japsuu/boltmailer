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

            // Set proj name
            projectNameLabel.Text = info.ProjectName;

            // Set state
            projectstatusLabel.Text = info.State.ToString();

            // Set deadline
            projectDeadlineLabel.Text = info.Deadline;

            // Set time est
            projectTimeEstLabel.Text = info.TimeEstimate;

            // Set notes
            try
            {
                ProjectNotesBox.Text = File.ReadAllText(projectPath + "\\notes");
            }
            catch (Exception)
            {
                MessageBox.Show($"Huomio-tiedosto ei ole olemassa.\n\nTiedosto luodaan.", "Virhe");
                StreamWriter writer = File.CreateText(projectPath + "\\notes");
                writer.Close();
            }
        }

        private void ProjectOverview_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(projectPath + "\\notes", ProjectNotesBox.Text);
        }
    }
}
