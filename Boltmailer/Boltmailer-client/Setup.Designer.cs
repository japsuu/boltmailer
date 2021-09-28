
namespace Boltmailer_client
{
    partial class Setup
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
            this.components = new System.ComponentModel.Container();
            this.Title = new System.Windows.Forms.Label();
            this.usernameTextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ServerRoot = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CompleteBtn = new System.Windows.Forms.Button();
            this.ConfirmationTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.InfoTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // Title
            // 
            this.Title.AutoSize = true;
            this.Title.Font = new System.Drawing.Font("Segoe UI", 25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point);
            this.Title.Location = new System.Drawing.Point(68, 9);
            this.Title.Name = "Title";
            this.Title.Size = new System.Drawing.Size(275, 57);
            this.Title.TabIndex = 0;
            this.Title.Text = "Konfiguraatio";
            // 
            // usernameTextbox
            // 
            this.usernameTextbox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.usernameTextbox.Location = new System.Drawing.Point(12, 116);
            this.usernameTextbox.Name = "usernameTextbox";
            this.usernameTextbox.PlaceholderText = "Etunimi Sukunimi";
            this.usernameTextbox.Size = new System.Drawing.Size(409, 27);
            this.usernameTextbox.TabIndex = 1;
            this.InfoTooltip.SetToolTip(this.usernameTextbox, "Syötä koko nimesi käyttäjänimeksesi,\r\nmieluiten muodossa \"etunimi sukunimi\".\r\n\r\nE" +
        "sim: \"Testi Tepponen\".");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Koko nimi:";
            // 
            // ServerRoot
            // 
            this.ServerRoot.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ServerRoot.Location = new System.Drawing.Point(12, 179);
            this.ServerRoot.Name = "ServerRoot";
            this.ServerRoot.PlaceholderText = "Juurikirjaston täysi sijainti";
            this.ServerRoot.Size = new System.Drawing.Size(409, 27);
            this.ServerRoot.TabIndex = 3;
            this.ServerRoot.Text = "\\\\serveri\\Boltmailer";
            this.InfoTooltip.SetToolTip(this.ServerRoot, "Täysi sijainti Boltmailer serverin\r\njuurikirjastoon.\r\n\r\nEsim: Jos Boltmailer syöt" +
        "tää saapuneet\r\nprojektit verkkokansioon \\\\serveri\\Boltmailer,\r\nsyötä tähän \"\\\\se" +
        "rveri\\Boltmailer\"");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 156);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(146, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Serverin juurikirjasto:";
            // 
            // CompleteBtn
            // 
            this.CompleteBtn.Location = new System.Drawing.Point(142, 223);
            this.CompleteBtn.Name = "CompleteBtn";
            this.CompleteBtn.Size = new System.Drawing.Size(132, 58);
            this.CompleteBtn.TabIndex = 5;
            this.CompleteBtn.Text = "Valmis";
            this.ConfirmationTooltip.SetToolTip(this.CompleteBtn, "Varmistathan että kaikki syöttämäsi tiedot ovat oikeita!");
            this.CompleteBtn.UseVisualStyleBackColor = true;
            this.CompleteBtn.Click += new System.EventHandler(this.CompleteBtn_Click);
            // 
            // ConfirmationTooltip
            // 
            this.ConfirmationTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Warning;
            this.ConfirmationTooltip.ToolTipTitle = "Varoitus!";
            // 
            // InfoTooltip
            // 
            this.InfoTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 293);
            this.Controls.Add(this.CompleteBtn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ServerRoot);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.usernameTextbox);
            this.Controls.Add(this.Title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Setup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Setup";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Title;
        private System.Windows.Forms.TextBox usernameTextbox;
        private System.Windows.Forms.ToolTip InfoTooltip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ServerRoot;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button CompleteBtn;
        private System.Windows.Forms.ToolTip ConfirmationTooltip;
    }
}