using Boltmailer_common;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Timers;

namespace Boltmailer_mainserver
{
    class BoltReader
    {
        bool firstStart = true;

        static bool isParsingMail = false;

        System.Timers.Timer ticker;

        readonly Random rnd = new Random();

        public void StartTicking()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Gray;

            if (firstStart)
            {
                Console.WriteLine(GetTitleText());
                Console.WriteLine(GetStartingUpText());

                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Yellow;

                Console.WriteLine("\nRunning startup tests, please do not close this window at this time...\n");

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;

                Console.WriteLine("Initiating a connection with the mail server, please allow up to two minutes...");
            }

            ticker = new System.Timers.Timer();

            ticker.Elapsed += new ElapsedEventHandler(ReadInbox);

            int refreshFrequency = int.Parse(ConfigurationManager.AppSettings.Get("EmailRefreshFrequency"));

            if (refreshFrequency < 10000)
                refreshFrequency = 10000;

            ticker.Interval = refreshFrequency;

            ReadInbox(null, null);

            ticker.Start();
        }

        /// <summary>
        /// Reads the contents of the inbox.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void ReadInbox(object arg1, EventArgs arg2)
        {
            if (isParsingMail)
                return;

            isParsingMail = true;

            // Check if this was the first cycle, if it was, show info that everything is working and wait for next cycle
            if (!firstStart)
            {
                using var imapClient = GetImapClient();
                using var smtpClient = GetSmtpClient();

                // Check if connection was successful. If not, abort.
                if (imapClient == null || smtpClient == null)
                {
                    return;
                }

                // Try to open the Imap inbox
                try
                {
                    imapClient.Inbox.Open(FolderAccess.ReadWrite);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not open the inbox: " + ex);
                    return;
                }

                // Read the inbox contents
                var uids = imapClient.Inbox.Search(SearchQuery.NotSeen);

                // Handle the found messages
                HandleMessages(uids, imapClient, smtpClient);

                // Disconnect, to not leave the connection hanging
                imapClient.Disconnect(true);
                smtpClient.Disconnect(true);
            }
            else
            {
                if (!RunStartupChecks())
                {
                    ticker.Stop();
                    ticker.Interval = 1000000;
                    Console.WriteLine("\nInitialization process halted!");
                }
                else
                {
                    Thread.Sleep(5000);
                    firstStart = false;
                    Console.Clear();
                    Console.WriteLine(GetTitleText());
                    Console.WriteLine($"\n'Output Directory' is now set to {ConfigManager.OutputDirectory}.");
                    Console.WriteLine($"\n'Email Refresh Frequency' is now set to {ticker.Interval}.");
                    Console.WriteLine($"\n'Trusted Domain' is now set to {ConfigManager.TrustedDomain}.\n");
                    Console.WriteLine("Initialization successful, reading started!");
                }
            }

            isParsingMail = false;
        }

        bool RunStartupChecks()
        {
            using var imapClient = GetImapClient();

            if (imapClient == null)
                return false;

            using var smtpClient = GetSmtpClient();

            if (smtpClient == null)
                return false;

            // Try to open the Imap inbox
            try
            {
                imapClient.Inbox.Open(FolderAccess.ReadWrite);
                LOG("Opening inbox...", true);
            }
            catch (Exception ex)
            {
                LOG("Opening inbox...", false, ex.Message);
                return false;
            }

            try
            {
                var uids = imapClient.Inbox.Search(SearchQuery.NotSeen);
                LOG("Reading inbox contents...", true);
            }
            catch (Exception ex)
            {
                LOG("Reading inbox contents...", false, ex.ToString());
                return false;
            }

            try
            {
                DirectoryInfo path = Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Test");

                File.CreateText(path.FullName + "\\testtext.txt").Close();

                Directory.Delete(path.FullName, true);

                Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Projektit");

                LOG("Checking output folder...", true);
            }
            catch (Exception ex)
            {
                LOG("Checking output folder...", false, ex.ToString());
                return false;
            }

            try
            {
                File.Create(ConfigManager.OutputDirectory + "\\Test").Close();
                File.Delete(ConfigManager.OutputDirectory + "\\Test");
                LOG("Checking file priviledges...", true);
            }
            catch (Exception ex)
            {
                LOG("Checking file priviledges...", false, ex.ToString());
                return false;
            }

            try
            {
                imapClient.Disconnect(true);
                smtpClient.Disconnect(true);
                LOG("Disconnecting...", true);
            }
            catch (Exception ex)
            {
                LOG("Disconnecting...", false, ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles the list of uids, sending them to correct folders.
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="imapClient"></param>
        /// <param name="smtpClient"></param>
        private void HandleMessages(IList<UniqueId> uids, ImapClient imapClient, SmtpClient smtpClient)
        {
            foreach (var uid in uids)
            {
                var message = imapClient.Inbox.GetMessage(uid);

                string sender = message.From.OfType<MailboxAddress>().Single().Address;
                string projectName = message.Subject;
                string assignedEmployee = "Vapaa";
                string projectDeadline = "Ei annettu";
                string timeEstimate = "Ei annettu";

                bool isUpdate = false;

                // Check if the message comes from the trusted domain
                if (sender.Substring(sender.LastIndexOf('@')) != ConfigManager.TrustedDomain)
                {
                    Console.WriteLine("Recieved a msg from untrusted domain, skipping...");

                    imapClient.Inbox.SetFlags(uid, MessageFlags.Seen, false);

                    return;
                }

                // Check if message is a help-message
                if (message.Subject == "help")
                {
                    SendHelpReply(smtpClient, sender);

                    imapClient.Inbox.SetFlags(uid, MessageFlags.Seen, false);

                    return;
                }

                // Start reading the MSG
                try
                {
                    using (var reader = new StringReader(message.TextBody))
                    {
                        // Set the employee if it's not null
                        string employeeLn = reader.ReadLine();
                        if (!string.IsNullOrEmpty(employeeLn))
                            assignedEmployee = employeeLn;

                        // Set the deadline if it's not null
                        string deadlineLn = reader.ReadLine();
                        if (!string.IsNullOrEmpty(deadlineLn))
                            projectDeadline = deadlineLn;

                        // Set the time estimate if it's not null
                        string timeEstimateLn = reader.ReadLine();
                        if (!string.IsNullOrEmpty(timeEstimateLn))
                            timeEstimate = timeEstimateLn;

                        // Set the update flag
                        isUpdate = projectDeadline.ToLower().Contains("update") || timeEstimate.ToLower().Contains("update");
                    }

                    // Create necessary files (notes, msg, info), if the msg is not a update to existing project
                    if (!isUpdate)
                    {
                        projectName = NamingConventions.FilenameFromTitle(projectName).ToLower();
                        assignedEmployee = NamingConventions.FilenameFromTitle(assignedEmployee);

                        DirectoryInfo path = Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Projektit\\" + assignedEmployee + "\\" + projectName);

                        // Create notes file if needed
                        if (!File.Exists(path + "\\" + "notes"))
                        {
                            File.CreateText(path + "\\" + "notes").Close();
                            File.SetAttributes(path + "\\" + "notes", FileAttributes.Hidden);
                        }

                        // Create the email file
                        message.WriteTo($"{path}\\{projectName}_{rnd.Next(1000, 9999)}.eml");

                        // Create the info file if needed
                        if (!File.Exists(path + "\\info.json"))
                        {
                            ProjectInfo info = new ProjectInfo() { ProjectName = message.Subject, Deadline = projectDeadline, TimeEstimate = timeEstimate, Status = ProjectStatus.Aloittamaton };

                            if (FileTools.WriteInfo(info, path.FullName, true))
                            {
                                File.SetAttributes(path + "\\info.json", FileAttributes.Hidden);
                            }
                            else
                            {
                                Console.WriteLine($"\nThe info file was not properly created. Removing the corrupt directory at {path.FullName}\n");
                                Directory.Delete(path.FullName, true);
                            }
                        }
                        Console.WriteLine($"\nWrote project '{projectName}' for '{assignedEmployee}' with deadline '{projectDeadline}' to file.");
                    }
                    else
                    {
                        // It's an update
                        projectName = NamingConventions.FilenameFromTitle(projectName).ToLower();
                        assignedEmployee = NamingConventions.FilenameFromTitle(assignedEmployee);

                        // Check if the project we need to update exists, and write the email there
                        if (Directory.Exists("Projektit" + "\\" + assignedEmployee + "\\" + projectName))
                        {
                            DirectoryInfo path = Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Projektit\\" + assignedEmployee + "\\" + projectName);

                            message.WriteTo($"{path}\\{projectName}_update_{rnd.Next(1000, 9999)}.eml");

                            Console.WriteLine($"\nWrote update to project '{projectName}' for '{assignedEmployee}' to file.");
                        }
                        else
                        {
                            // Project to update doesn't exist, notify sender.
                            SendErrorReply(smtpClient, sender, projectName, assignedEmployee, isUpdate);
                        }
                    }
                }
                catch (Exception ex)    // Error reading the MSG
                {
                    DirectoryInfo directory = Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Virheelliset" + "\\");

                    string path = directory.FullName + DateTime.Now.ToString("dd.MM") + "_" + rnd.Next(10000, 99999);

                    Console.WriteLine($"\n\nError parsing a mail:\n{ex}.\n\nSaving at: {path}\n\nAnd notifying the sender of the mail.");

                    // Try saving the email to file
                    try
                    {
                        var msg = imapClient.Inbox.GetMessage(uid);

                        msg.WriteTo($"{path}.eml");
                    }
                    catch
                    {
                    }

                    SendErrorReply(smtpClient, sender, projectName, assignedEmployee, isUpdate, ex);
                }

                // Mark the email as seen to prevent reading it again
                imapClient.Inbox.SetFlags(uid, MessageFlags.Seen, false);
            }
        }

        /// <summary>
        /// Creates and logs on a Imap client
        /// </summary>
        /// <returns>Logged on client</returns>
        private ImapClient GetImapClient()
        {
            var imapClient = new ImapClient();
            try
            {
                // Authenticate imap client
                if (ConfigManager.EmailImapUseSSL)
                {
                    imapClient.Connect(ConfigManager.EmailImapHost, ConfigManager.EmailImapPort, SecureSocketOptions.SslOnConnect);
                }
                else
                {
                    imapClient.Connect(ConfigManager.EmailImapHost, ConfigManager.EmailImapPort, SecureSocketOptions.None);
                }
                imapClient.Authenticate(ConfigManager.EmailUsername, ConfigManager.EmailPassword);

                LOG("Logging on (Imap)... ", true);

                return imapClient;
            }
            catch (Exception ex)
            {
                LOG("Logging on... ", false, ex.Message);

                return null;
            }
        }

        /// <summary>
        /// Creates and logs on a Smtp client
        /// </summary>
        /// <returns>Logged on client</returns>
        private SmtpClient GetSmtpClient()
        {
            var smtpClient = new SmtpClient();

            try
            {
                // Authenticate smtp client
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                if (ConfigManager.EmailSmtpUseSSL)
                {
                    smtpClient.Connect(ConfigManager.EmailSmtpHost, ConfigManager.EmailSmtpPort, SecureSocketOptions.SslOnConnect);
                }
                else
                {
                    smtpClient.Connect(ConfigManager.EmailSmtpHost, ConfigManager.EmailSmtpPort, SecureSocketOptions.None);
                }
                smtpClient.Authenticate(ConfigManager.EmailUsername, ConfigManager.EmailPassword);

                LOG("Logging on (Smtp)... ", true);

                return smtpClient;
            }
            catch (Exception ex)
            {
                LOG("Logging on... ", false, ex.Message);

                return null;
            }
        }

        /// <summary>
        /// Replies to Recipient with an error mesage describing the error.
        /// </summary>
        /// <param name="smtpClient"></param>
        /// <param name="Recipient"></param>
        /// <param name="projectName"></param>
        /// <param name="assignedEmployee"></param>
        /// <param name="update"></param>
        /// <param name="ex"></param>
        private void SendErrorReply(SmtpClient smtpClient, string Recipient, string projectName, string assignedEmployee, bool update, Exception ex = null)
        {
            Console.WriteLine($"\nTried to update non-existant project '{projectName}' for '{assignedEmployee}', notifying email sender ({Recipient}).");

            var reply = new MimeMessage();
            var bodyBuilder = new BodyBuilder();

            // from
            reply.From.Add(new MailboxAddress("Boltmailer Mainserver", ConfigManager.EmailUsername));
            // to
            reply.To.Add(new MailboxAddress(Recipient, Recipient));
            // reply to
            reply.ReplyTo.Add(new MailboxAddress("NoReply", "noReply"));

            reply.Subject = "Virhe / Error";
            if (update)
            {
                bodyBuilder.HtmlBody =
                $@"
<h1>Suomeksi:</h1>
<h4>Tämä on automaattinen viesti Boltmailer Serveriltä. <b>Ethän yritä vastata tähän viestiin.</b></h4>
<h3>Projektia jota yritit päivittää ({projectName}) ei ole olemassa käyttäjälle '{assignedEmployee}'.</h3>
<h4>Jos uskot tämän olevan virhe, ota yhteyttä ylläpitäjiin.</h4>

</br></br>

<h1>In English:</h1>
<h4>This is an automated message sent by the Boltmailer Mainserver. <b>Please do not try to reply to this email.</b></h4>
<h3>The project you have tried to update ({projectName}) does not exist for the user '{assignedEmployee}'.</h3>
<h4>If you believe this is an error, please contact the Administrators.</h4>
                                ";
            }
            else
            {
                bodyBuilder.HtmlBody =
                $@"
<h1>Suomeksi:</h1>
<h4>Tämä on automaattinen viesti Boltmailer Serveriltä. <b>Ethän yritä vastata tähän viestiin.</b></h4>
<h3>Projektin ({projectName}) lisäämisessä käyttäjälle '{assignedEmployee}' on tapahtunut virhe:</h3>
<h4>{ex.Message}</h4>

</br></br>

<h1>In English:</h1>
<h4>This is an automated message sent by the Boltmailer Mainserver. <b>Please do not try to reply to this email.</b></h4>
<h3>There's been an error adding a project ({projectName}) for the user '{assignedEmployee}':</h3>
<h4>{ex.Message}</h4>
                                ";
            }
            
            reply.Body = bodyBuilder.ToMessageBody();

            smtpClient.Send(reply);
            smtpClient.Disconnect(true);
        }

        /// <summary>
        /// Sends a help message to the Recipient.
        /// </summary>
        /// <param name="smtpClient"></param>
        /// <param name="Recipient"></param>
        private void SendHelpReply(SmtpClient smtpClient, string Recipient)
        {
            Console.WriteLine($"\nUser ({Recipient}) requested help. Replying with help MSG.");

            var reply = new MimeMessage();
            var bodyBuilder = new BodyBuilder();

            // from
            reply.From.Add(new MailboxAddress("Boltmailer Mainserver", ConfigManager.EmailUsername));
            // to
            reply.To.Add(new MailboxAddress(Recipient, Recipient));
            // reply to
            reply.ReplyTo.Add(new MailboxAddress("NoReply", "noReply"));

            reply.Subject = "Ohjeet/Instructions";

            bodyBuilder.HtmlBody =
                $@"
<h4>Tämä on automaattinen viesti Boltmailer Serveriltä. <b>Ethän yritä vastata tähän viestiin.</b></h4>
<h4>Muistathan varmistaa syöttämäsi tiedot oikeiksi ennen viestin lähettämistä!</h4>
<h3>Uuden projektin lisääminen:</h3>
<h4><pre>
    Otsikko: Projektin nimi
    Ensimmäinen viestin rivi: Työntekijän nimi jolle projektin annetaan (tai vapaa)
    Toinen viestin rivi: Deadline
    Kolmas viestin rivi: Aika-arvio.</pre>
</h4>

<h3>Olemassa-olevaan projektiin tiedostojen lisääminen:</h3>
<h4><pre>
    Otsikko: Projektin nimi
    Ensimmäinen viestin rivi: Työntekijä jolle projekti on annettu (tai vapaa)
    Toinen viestin rivi: ""update"" (ilman heittomerkkejä).</pre>
</h4>
                ";

            reply.Body = bodyBuilder.ToMessageBody();

            smtpClient.Send(reply);
            smtpClient.Disconnect(true);
        }

        private void LOG(string text, bool ok, string exception = null)
        {
            if (firstStart)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("{0, -40}", "\n" + text);

                if (ok)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("{0, -10}", "[OK]");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("{0, -10}", "[ERROR]: " + exception);
                }
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        static string GetTitleText()
        {
            string title =
                @"
██████   ██████  ██   ████████ ███╗   ███╗ █████╗ ██╗██╗     ███████╗██████╗ 
██   ██ ██    ██ ██      ██    ████╗ ████║██╔══██╗██║██║     ██╔════╝██╔══██╗
██████  ██    ██ ██      ██    ██╔████╔██║███████║██║██║     █████╗  ██████╔╝
██   ██ ██    ██ ██      ██    ██║╚██╔╝██║██╔══██║██║██║     ██╔══╝  ██╔══██╗
██████   ██████  ███████ ██    ██║ ╚═╝ ██║██║  ██║██║███████╗███████╗██║  ██║
                               ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝╚══════╝╚══════╝╚═╝  ╚═╝
                ";

            return title;
        }

        static string GetStartingUpText()
        {
            string title =
                @"
   _____ _             _   _                                
  / ____| |           | | (_)                               
 | (___ | |_ __ _ _ __| |_ _ _ __   __ _   _   _ _ __       
  \___ \| __/ _` | '__| __| | '_ \ / _` | | | | | '_ \      
  ____) | || (_| | |  | |_| | | | | (_| | | |_| | |_) | _ _ 
 |_____/ \__\__,_|_|   \__|_|_| |_|\__, |  \__,_| .__(_|_|_)
                                    __/ |       | |         
                                   |___/        |_|         
                ";

            return title;
        }
    }
}
