
namespace Boltmailer_client
{
    partial class ProjectOverview
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.NotesTitle = new System.Windows.Forms.Label();
            this.ProjectNotesBox = new System.Windows.Forms.TextBox();
            this.projectDeadlineLabel = new System.Windows.Forms.Label();
            this.projectTimeEstLabel = new System.Windows.Forms.Label();
            this.projectstatusLabel = new System.Windows.Forms.Label();
            this.FolderOpenButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // NotesTitle
            // 
            this.NotesTitle.AutoSize = true;
            this.NotesTitle.Location = new System.Drawing.Point(12, 109);
            this.NotesTitle.Name = "NotesTitle";
            this.NotesTitle.Size = new System.Drawing.Size(145, 15);
            this.NotesTitle.TabIndex = 0;
            this.NotesTitle.Text = "Muistiinpanot ja huomiot:";
            // 
            // ProjectNotesBox
            // 
            this.ProjectNotesBox.Location = new System.Drawing.Point(12, 127);
            this.ProjectNotesBox.Multiline = true;
            this.ProjectNotesBox.Name = "ProjectNotesBox";
            this.ProjectNotesBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ProjectNotesBox.Size = new System.Drawing.Size(294, 138);
            this.ProjectNotesBox.TabIndex = 1;
            // 
            // projectDeadlineLabel
            // 
            this.projectDeadlineLabel.AutoSize = true;
            this.projectDeadlineLabel.Location = new System.Drawing.Point(12, 39);
            this.projectDeadlineLabel.Name = "projectDeadlineLabel";
            this.projectDeadlineLabel.Size = new System.Drawing.Size(102, 15);
            this.projectDeadlineLabel.TabIndex = 3;
            this.projectDeadlineLabel.Text = "Projektin deadline";
            // 
            // projectTimeEstLabel
            // 
            this.projectTimeEstLabel.AutoSize = true;
            this.projectTimeEstLabel.Location = new System.Drawing.Point(12, 63);
            this.projectTimeEstLabel.Name = "projectTimeEstLabel";
            this.projectTimeEstLabel.Size = new System.Drawing.Size(109, 15);
            this.projectTimeEstLabel.TabIndex = 4;
            this.projectTimeEstLabel.Text = "Projektin aika-arvio";
            // 
            // projectstatusLabel
            // 
            this.projectstatusLabel.AutoSize = true;
            this.projectstatusLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.projectstatusLabel.Location = new System.Drawing.Point(12, 9);
            this.projectstatusLabel.Name = "projectstatusLabel";
            this.projectstatusLabel.Size = new System.Drawing.Size(122, 21);
            this.projectstatusLabel.TabIndex = 5;
            this.projectstatusLabel.Text = "Projektin status";
            // 
            // FolderOpenButton
            // 
            this.FolderOpenButton.Location = new System.Drawing.Point(211, 9);
            this.FolderOpenButton.Name = "FolderOpenButton";
            this.FolderOpenButton.Size = new System.Drawing.Size(95, 45);
            this.FolderOpenButton.TabIndex = 6;
            this.FolderOpenButton.Text = "Avaa projektin\r\nkansio";
            this.FolderOpenButton.UseVisualStyleBackColor = true;
            this.FolderOpenButton.Click += new System.EventHandler(this.FolderOpenButton_Click);
            // 
            // ProjectOverview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(318, 277);
            this.Controls.Add(this.FolderOpenButton);
            this.Controls.Add(this.projectstatusLabel);
            this.Controls.Add(this.projectTimeEstLabel);
            this.Controls.Add(this.projectDeadlineLabel);
            this.Controls.Add(this.ProjectNotesBox);
            this.Controls.Add(this.NotesTitle);
            this.Name = "ProjectOverview";
            this.Text = "Projektin katsaus";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProjectOverview_FormClosing);
            this.Load += new System.EventHandler(this.ProjectOverview_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label NotesTitle;
        private System.Windows.Forms.TextBox ProjectNotesBox;
        private System.Windows.Forms.Label projectDeadlineLabel;
        private System.Windows.Forms.Label projectTimeEstLabel;
        private System.Windows.Forms.Label projectstatusLabel;
        private System.Windows.Forms.Button FolderOpenButton;
    }
}