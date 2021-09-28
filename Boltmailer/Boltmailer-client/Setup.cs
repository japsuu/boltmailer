using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Tulpep.NotificationWindow;

namespace Boltmailer_client
{
    public partial class Setup : Form
    {
        public Setup()
        {
            InitializeComponent();
        }

        private void CompleteBtn_Click(object sender, EventArgs e)
        {
            string username = usernameTextbox.Text;
            string rootPath = ServerRoot.Text;

            if(username == "admin")
            {
                ModifyConfig("adminMode", "true");
                ModifyConfig("employeesRootPath", rootPath);

                PopupNotifier popup = new PopupNotifier
                {
                    TitleText = "Admin tila",
                    ContentText = "Admin tila aktivoitu."
                };
                popup.Popup();
            }
            else
            {
                ModifyConfig("employeeSearchTerm", username);
                ModifyConfig("employeesRootPath", rootPath);

                PopupNotifier popup = new PopupNotifier();

                try
                {
                    // Create the user directory
                    DirectoryInfo dir = Directory.CreateDirectory(rootPath + "\\Projektit\\" + Boltmailer_common.NamingConventions.FilenameFromTitle(username));

                    popup.TitleText = "Luotiin uusi käyttäjä!";
                    popup.ContentText = "Antamallasi tiedoilla luotiin uusi käyttäjähakemisto sijaintiin " + dir.FullName;
                }
                catch (Exception ex)
                {
                    popup.TitleText = "Varoitus";
                    popup.ContentText = "Käyttäjän luomisessa annettuun sijaintiin tapahtui virhe:\n" + ex.Message;
                }

                popup.Popup();
            }

            // Save the setup as completed
            ModifyConfig("setupCompleted", "true");

            // Close current window and open the main view.
            Exit();
        }

        private void Exit()
        {
            Hide();

            GeneralOverview general = new GeneralOverview();

            general.Closed += (s, args) => this.Close();
            general.Show();
        }

        private static void ModifyConfig(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings[key].Value = value;

            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
