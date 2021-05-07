using Boltmailer_common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Boltmailer_client
{
    public partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            ContentsLabel.Text = "Käyttöliittymän versio: " + Versioning.GetClientVersion();
            CopyrightLabel.Text = $"©Copyright {DateTime.Now:yyyy} Jasper Honkasalo (Honkasoft)";
        }

        private void CopyrightLabel_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "http://www.honkasoft.fi");
        }
    }
}