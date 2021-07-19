
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
            this.StatusComboBox = new System.Windows.Forms.ComboBox();
            this.TimeEstimateBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.EmployeeComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // NotesTitle
            // 
            this.NotesTitle.AutoSize = true;
            this.NotesTitle.Location = new System.Drawing.Point(12, 222);
            this.NotesTitle.Name = "NotesTitle";
            this.NotesTitle.Size = new System.Drawing.Size(145, 15);
            this.NotesTitle.TabIndex = 0;
            this.NotesTitle.Text = "Muistiinpanot ja huomiot:";
            // 
            // ProjectNotesBox
            // 
            this.ProjectNotesBox.Location = new System.Drawing.Point(13, 240);
            this.ProjectNotesBox.Multiline = true;
            this.ProjectNotesBox.Name = "ProjectNotesBox";
            this.ProjectNotesBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ProjectNotesBox.Size = new System.Drawing.Size(294, 138);
            this.ProjectNotesBox.TabIndex = 1;
            // 
            // projectDeadlineLabel
            // 
            this.projectDeadlineLabel.AutoSize = true;
            this.projectDeadlineLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point);
            this.projectDeadlineLabel.Location = new System.Drawing.Point(13, 32);
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
            this.projectTimeEstLabel.Size = new System.Drawing.Size(64, 15);
            this.projectTimeEstLabel.TabIndex = 4;
            this.projectTimeEstLabel.Text = "Aika-arvio:";
            // 
            // projectstatusLabel
            // 
            this.projectstatusLabel.AutoSize = true;
            this.projectstatusLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.projectstatusLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.projectstatusLabel.Location = new System.Drawing.Point(12, 9);
            this.projectstatusLabel.MinimumSize = new System.Drawing.Size(185, 0);
            this.projectstatusLabel.Name = "projectstatusLabel";
            this.projectstatusLabel.Size = new System.Drawing.Size(185, 23);
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
            // StatusComboBox
            // 
            this.StatusComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.StatusComboBox.FormattingEnabled = true;
            this.StatusComboBox.Location = new System.Drawing.Point(211, 81);
            this.StatusComboBox.Name = "StatusComboBox";
            this.StatusComboBox.Size = new System.Drawing.Size(95, 23);
            this.StatusComboBox.TabIndex = 7;
            this.StatusComboBox.SelectedValueChanged += new System.EventHandler(this.StatusComboBox_SelectedValueChanged);
            // 
            // TimeEstimateBox
            // 
            this.TimeEstimateBox.Location = new System.Drawing.Point(12, 81);
            this.TimeEstimateBox.Name = "TimeEstimateBox";
            this.TimeEstimateBox.Size = new System.Drawing.Size(185, 23);
            this.TimeEstimateBox.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(203, 9);
            this.label1.MinimumSize = new System.Drawing.Size(2, 153);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(2, 153);
            this.label1.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Location = new System.Drawing.Point(5, 57);
            this.label2.MaximumSize = new System.Drawing.Size(0, 2);
            this.label2.MinimumSize = new System.Drawing.Size(200, 2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(200, 2);
            this.label2.TabIndex = 10;
            // 
            // EmployeeComboBox
            // 
            this.EmployeeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.EmployeeComboBox.FormattingEnabled = true;
            this.EmployeeComboBox.Location = new System.Drawing.Point(211, 134);
            this.EmployeeComboBox.Name = "EmployeeComboBox";
            this.EmployeeComboBox.Size = new System.Drawing.Size(95, 23);
            this.EmployeeComboBox.TabIndex = 11;
            this.EmployeeComboBox.SelectedIndexChanged += new System.EventHandler(this.EmployeeComboBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(211, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 15);
            this.label3.TabIndex = 12;
            this.label3.Text = "Vaihda statusta:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(211, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 15);
            this.label4.TabIndex = 13;
            this.label4.Text = "Vaihda tekijää:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label5.Location = new System.Drawing.Point(203, 57);
            this.label5.MaximumSize = new System.Drawing.Size(0, 2);
            this.label5.MinimumSize = new System.Drawing.Size(110, 2);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 2);
            this.label5.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label6.Location = new System.Drawing.Point(203, 107);
            this.label6.MaximumSize = new System.Drawing.Size(0, 2);
            this.label6.MinimumSize = new System.Drawing.Size(110, 2);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(110, 2);
            this.label6.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label7.Location = new System.Drawing.Point(203, 160);
            this.label7.MaximumSize = new System.Drawing.Size(0, 2);
            this.label7.MinimumSize = new System.Drawing.Size(110, 2);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(110, 2);
            this.label7.TabIndex = 16;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label8.Location = new System.Drawing.Point(5, 107);
            this.label8.MaximumSize = new System.Drawing.Size(0, 2);
            this.label8.MinimumSize = new System.Drawing.Size(200, 2);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(200, 2);
            this.label8.TabIndex = 17;
            // 
            // ProjectOverview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(318, 388);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.EmployeeComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TimeEstimateBox);
            this.Controls.Add(this.StatusComboBox);
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
        private System.Windows.Forms.ComboBox StatusDropdown;
        private System.Windows.Forms.ComboBox StatusComboBox;
        private System.Windows.Forms.TextBox TimeEstimateBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox EmployeeComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
    }
}