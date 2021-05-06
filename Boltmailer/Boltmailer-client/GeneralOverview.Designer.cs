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
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ProjectsDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // DebugLabel
            // 
            this.DebugLabel.AutoSize = true;
            this.DebugLabel.Location = new System.Drawing.Point(47, 459);
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
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.button1.Location = new System.Drawing.Point(891, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(45, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Apua";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // GeneralOverview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(948, 487);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ProjectsDataGrid);
            this.Controls.Add(this.DebugLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "GeneralOverview";
            this.Text = "Boltmailer";
            ((System.ComponentModel.ISupportInitialize)(this.ProjectsDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label DebugLabel;
        private System.Windows.Forms.DataGridView ProjectsDataGrid;
        private System.Windows.Forms.Button button1;
    }
}

