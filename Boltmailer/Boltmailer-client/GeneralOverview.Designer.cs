namespace Boltmailer_client
{
    partial class GeneralOverview
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DebugLabel = new System.Windows.Forms.Label();
            this.ProjectsDataGrid = new System.Windows.Forms.DataGridView();
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.HelpButton = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutButton = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.ProjectsDataGrid)).BeginInit();
            this.MenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // DebugLabel
            // 
            this.DebugLabel.AutoSize = true;
            this.DebugLabel.Location = new System.Drawing.Point(47, 429);
            this.DebugLabel.Name = "DebugLabel";
            this.DebugLabel.Size = new System.Drawing.Size(65, 15);
            this.DebugLabel.TabIndex = 1;
            this.DebugLabel.Text = "Debug text";
            // 
            // ProjectsDataGrid
            // 
            this.ProjectsDataGrid.AllowUserToAddRows = false;
            this.ProjectsDataGrid.AllowUserToDeleteRows = false;
            this.ProjectsDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ProjectsDataGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.ProjectsDataGrid.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.ProjectsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ProjectsDataGrid.Cursor = System.Windows.Forms.Cursors.Cross;
            this.ProjectsDataGrid.Location = new System.Drawing.Point(47, 41);
            this.ProjectsDataGrid.MultiSelect = false;
            this.ProjectsDataGrid.Name = "ProjectsDataGrid";
            this.ProjectsDataGrid.ReadOnly = true;
            this.ProjectsDataGrid.RowHeadersWidth = 51;
            this.ProjectsDataGrid.RowTemplate.Height = 25;
            this.ProjectsDataGrid.Size = new System.Drawing.Size(850, 369);
            this.ProjectsDataGrid.TabIndex = 2;
            this.ProjectsDataGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.ProjectsDataGrid_CellFormatting);
            this.ProjectsDataGrid.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ProjectsDataGrid_CellMouseDoubleClick);
            this.ProjectsDataGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.ProjectsDataGrid_CellPainting);
            this.ProjectsDataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ProjectsDataGrid_MouseDown);
            // 
            // MenuStrip
            // 
            this.MenuStrip.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.HelpButton,
            this.AboutButton});
            this.MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.MenuStrip.Size = new System.Drawing.Size(948, 24);
            this.MenuStrip.TabIndex = 4;
            this.MenuStrip.Text = "menuStrip1";
            this.MenuStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.MenuStrip_ItemClicked);
            // 
            // HelpButton
            // 
            this.HelpButton.AutoToolTip = true;
            this.HelpButton.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.HelpButton.CheckOnClick = true;
            this.HelpButton.ForeColor = System.Drawing.SystemColors.Window;
            this.HelpButton.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.HelpButton.Name = "HelpButton";
            this.HelpButton.Size = new System.Drawing.Size(47, 20);
            this.HelpButton.Text = "Apua";
            this.HelpButton.ToolTipText = "Täältä löydät apua sovelluksen käyttämiseen";
            // 
            // AboutButton
            // 
            this.AboutButton.AutoToolTip = true;
            this.AboutButton.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.AboutButton.CheckOnClick = true;
            this.AboutButton.ForeColor = System.Drawing.SystemColors.Window;
            this.AboutButton.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(51, 20);
            this.AboutButton.Text = "Tietoa";
            this.AboutButton.ToolTipText = "Tietoa sovelluksesta ja sen tekijöistä";
            // 
            // GeneralOverview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(948, 453);
            this.Controls.Add(this.ProjectsDataGrid);
            this.Controls.Add(this.DebugLabel);
            this.Controls.Add(this.MenuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.MenuStrip;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "GeneralOverview";
            this.Text = "Boltmailer";
            this.Load += new System.EventHandler(this.GeneralOverview_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ProjectsDataGrid)).EndInit();
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label DebugLabel;
        private System.Windows.Forms.DataGridView ProjectsDataGrid;
        private System.Windows.Forms.MenuStrip MenuStrip;
        private System.Windows.Forms.ToolStripMenuItem HelpButton;
        private System.Windows.Forms.ToolStripMenuItem AboutButton;
    }
}

