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
using System.IO;
using System.Linq;
using System.Threading;

namespace Boltmailer_mainserver
{
    internal class BoltReader
    {
        private bool firstStart = true;

        private static bool isParsingMail;

        private System.Timers.Timer ticker;

        private readonly Random rnd = new Random();

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

            ticker.Elapsed += ReadInbox;

            int refreshFrequency = int.Parse(ConfigurationManager.AppSettings.Get("EmailRefreshFrequency"));

            if (refreshFrequency < 10000)
                refreshFrequency = 10000;

            ticker.Interval = refreshFrequency;

            ReadInbox(null, null);
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
            if (firstStart)
            {
                if (RunStartupChecks())
                {
                    Thread.Sleep(5000);
                    firstStart = false;
                    Console.Clear();
                    Console.WriteLine(GetTitleText());
                    Console.WriteLine($"\n'Output Directory' is now set to {ConfigManager.OutputDirectory}.");
                    Console.WriteLine($"\n'Email Refresh Frequency' is now set to {ticker.Interval}.");
                    Console.WriteLine($"\n'Trusted Domain' is now set to {ConfigManager.TrustedDomain}.\n");
                    Console.WriteLine("Initialization successful, reading started!");
                    
                    // Instantly trigger a read event
                    ReadInbox(null, null);
                    ticker.Start();
                }
                else
                {
                    ticker.Stop();
                    ticker.Interval = 1000000;
                    Console.WriteLine("\nInitialization process halted!");
                }
            }
            else
            {
                Console.Write("*.*");
                using ImapClient imapClient = GetImapClient();
                using SmtpClient smtpClient = GetSmtpClient();

                // Check if connection was successful. If not, abort.
                if (imapClient == null || smtpClient == null)
                {
                    Console.WriteLine("Could not connect to mail provider!");
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
                IList<UniqueId> uIds = imapClient.Inbox.Search(SearchQuery.NotSeen);

                // Handle the found messages
                HandleMessages(uIds, imapClient);

                // Disconnect, to not leave the connection hanging
                imapClient.Disconnect(true);
                smtpClient.Disconnect(true);
                Console.Write(".*.");
            }

            isParsingMail = false;
        }

        private bool RunStartupChecks()
        {
            using ImapClient imapClient = GetImapClient();

            if (imapClient == null)
                return false;

            using SmtpClient smtpClient = GetSmtpClient();

            if (smtpClient == null)
                return false;

            // Try to open the Imap inbox
            try
            {
                imapClient.Inbox.Open(FolderAccess.ReadWrite);
                Log("Opening inbox...", true);
            }
            catch (Exception ex)
            {
                Log("Opening inbox...", false, ex.Message);
                return false;
            }

            try
            {
                imapClient.Inbox.Search(SearchQuery.NotSeen);
                Log("Reading inbox contents...", true);
            }
            catch (Exception ex)
            {
                Log("Reading inbox contents...", false, ex.ToString());
                return false;
            }

            try
            {
                DirectoryInfo path = Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Test");

                File.CreateText(path.FullName + "\\testText.txt").Close();

                Directory.Delete(path.FullName, true);

                Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Projektit");

                Log("Checking output folder...", true);
            }
            catch (Exception ex)
            {
                Log("Checking output folder...", false, ex.ToString());
                return false;
            }

            try
            {
                File.Create(ConfigManager.OutputDirectory + "\\Test").Close();
                File.Delete(ConfigManager.OutputDirectory + "\\Test");
                Log("Checking file privileges...", true);
            }
            catch (Exception ex)
            {
                Log("Checking file privileges...", false, ex.ToString());
                return false;
            }

            try
            {
                imapClient.Disconnect(true);
                smtpClient.Disconnect(true);
                Log("Disconnecting...", true);
            }
            catch (Exception ex)
            {
                Log("Disconnecting...", false, ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles the list of uIds, sending them to correct folders.
        /// </summary>
        /// <param name="uIds"></param>
        /// <param name="imapClient"></param>
        private void HandleMessages(IEnumerable<UniqueId> uIds, ImapClient imapClient)
        {
            foreach (UniqueId uid in uIds)
            {
                MimeMessage message = imapClient.Inbox.GetMessage(uid);

                string sender = message.From.OfType<MailboxAddress>().Single().Address;
                string projectName = message.Subject;
                string assignedEmployee = "Vapaa";
                string projectDeadline = "Ei annettu";
                string timeEstimate = "Ei annettu";

                bool isUpdate = false;

                // Check if the message comes from the trusted domain
                if (sender[sender.LastIndexOf('@')..] != ConfigManager.TrustedDomain)
                {
                    Console.WriteLine("Received a msg from untrusted domain, skipping...");

                    imapClient.Inbox.SetFlags(uid, MessageFlags.Seen, false);

                    continue;
                }
                
                // Check if the sender is ourselves (to not get in an infinite loop)
                if (sender == ConfigManager.EmailUsername)
                {
                    imapClient.Inbox.SetFlags(uid, MessageFlags.Seen, false);
                    
                    continue;
                }

                // Check if message is a help-message
                if (message.Subject.ToLower() == "help")
                {
                    SendHelpReply(sender);

                    imapClient.Inbox.SetFlags(uid, MessageFlags.Seen, false);

                    continue;
                }

                // Start reading the MSG
                try
                {
                    if (message.TextBody == null)
                    {
                        Console.WriteLine("Error parsing a mail, there's no text body! Full HTML? Please no.");

                        imapClient.Inbox.SetFlags(uid, MessageFlags.Seen, false);
                        
                        continue;
                    }
                    
                    using (StringReader reader = new StringReader(message.TextBody))
                    {
                        // Set the employee if it's not null
                        string employeeLn = reader.ReadLine();
                        if (!string.IsNullOrEmpty(employeeLn)) assignedEmployee = employeeLn;

                        // Set the deadline if it's not null
                        string deadlineLn = reader.ReadLine();
                        if (!string.IsNullOrEmpty(deadlineLn)) projectDeadline = deadlineLn;

                        // Set the time estimate if it's not null
                        string timeEstimateLn = reader.ReadLine();
                        if (!string.IsNullOrEmpty(timeEstimateLn)) timeEstimate = timeEstimateLn;

                        string updateLn = reader.ReadLine();

                        // Set the update flag
                        isUpdate = projectDeadline.ToLower().Contains("update")
                                   || timeEstimate.ToLower().Contains("update")
                                   || updateLn.ToLower().Contains("update");
                    }

                    // Create necessary files (notes, msg, info), if the msg is not a update to existing project
                    if (isUpdate)
                    {
                        // It's an update
                        projectName = NamingConventions.FilenameFromTitle(projectName).ToLower();
                        assignedEmployee = NamingConventions.FilenameFromTitle(assignedEmployee);

                        // Check if the project we need to update exists, and write the email there
                        if (Directory.Exists(ConfigManager.OutputDirectory +
                                             "\\Projektit" + "\\" + assignedEmployee + "\\" + projectName))
                        {
                            DirectoryInfo projectRootPath = Directory.CreateDirectory(ConfigManager.OutputDirectory +
                                                                           "\\Projektit\\" + assignedEmployee + "\\" +
                                                                           projectName);

                            int suffix = 1;
                            while (File.Exists($"{projectRootPath}\\{projectName}_update_{suffix}.eml"))
                            {
                                suffix++;
                            }
                            message.WriteTo($"{projectRootPath}\\{projectName}_update_{suffix}.eml");

                            Console.WriteLine(
                                $"\nUpdated project '{projectName}' for '{assignedEmployee}'.");
                        }
                        else
                        {
                            // Project to update doesn't exist, notify sender.
                            SendErrorReply(sender, projectName, assignedEmployee, true);
                        }
                    }
                    else    //TODO: Check if the assigned user exists, if not, send error reply.
                    {
                        projectName = NamingConventions.FilenameFromTitle(projectName).ToLower();
                        assignedEmployee = NamingConventions.FilenameFromTitle(assignedEmployee);

                        DirectoryInfo projectRootPath = Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Projektit\\" +
                                                                       assignedEmployee + "\\" + projectName);

                        // Create notes file if needed
                        if (!File.Exists(projectRootPath + "\\" + "notes"))
                        {
                            File.CreateText(projectRootPath + "\\" + "notes").Close();
                            File.SetAttributes(projectRootPath + "\\" + "notes", FileAttributes.Hidden);
                        }

                        // Create the email file
                        int suffix = 1;
                        while (File.Exists($"{projectRootPath}\\{projectName}_{suffix}.eml"))
                        {
                            suffix++;
                        }
                        message.WriteTo($"{projectRootPath}\\{projectName}_{suffix}.eml");

                        // Create the info file if needed
                        if (!File.Exists(projectRootPath + "\\info.json"))
                        {
                            ProjectInfo info = new ProjectInfo
                            {
                                ProjectName = message.Subject, Deadline = projectDeadline, TimeEstimate = timeEstimate,
                                Status = ProjectStatus.Aloittamaton
                            };

                            if (FileTools.WriteInfo(info, projectRootPath.FullName, true))
                            {
                                File.SetAttributes(projectRootPath + "\\info.json", FileAttributes.Hidden);
                            }
                            else
                            {
                                Console.WriteLine(
                                    $"\nThe info file was not properly created. Removing the corrupt directory at {projectRootPath.FullName}\n");
                                Directory.Delete(projectRootPath.FullName, true);
                            }
                        }

                        Console.WriteLine(
                            $"\nSaved project '{projectName}' for '{assignedEmployee}' with deadline '{projectDeadline}'.");
                    }
                }
                catch (Exception ex)    // Error reading the MSG
                {
                    DirectoryInfo directory = Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Virheelliset" + "\\");

                    string path = directory.FullName + DateTime.Now.ToString("dd.MM") + "_" + rnd.Next(0, 99999);

                    Console.WriteLine($"\n\nError parsing a mail:\n{ex}.\n\nSaving at: {path}\n\nAnd notifying the sender of the error.");

                    // Try saving the email to file
                    try
                    {
                        MimeMessage msg = imapClient.Inbox.GetMessage(uid);

                        msg.WriteTo($"{path}.eml");
                    }
                    catch
                    {
                        // ignored
                        Console.WriteLine("Saving failed :(");
                    }

                    SendErrorReply(sender, projectName, assignedEmployee, isUpdate, ex);
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
            ImapClient imapClient = new ImapClient();
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

                Log("Logging on (Imap)... ", true);

                return imapClient;
            }
            catch (Exception ex)
            {
                Log("Logging on... ", false, ex.Message);

                return null;
            }
        }

        /// <summary>
        /// Creates and logs on a Smtp client
        /// </summary>
        /// <returns>Logged on client</returns>
        private SmtpClient GetSmtpClient()
        {
            SmtpClient smtpClient = new SmtpClient();

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

                Log("Logging on (Smtp)... ", true);

                return smtpClient;
            }
            catch (Exception ex)
            {
                Log("Logging on... ", false, ex.Message);

                return null;
            }
        }

        /// <summary>
        /// Replies to Recipient with an error message describing the error.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="projectName"></param>
        /// <param name="assignedEmployee"></param>
        /// <param name="update"></param>
        /// <param name="ex"></param>
        private void SendErrorReply(string recipient, string projectName, string assignedEmployee, bool update, Exception ex = null)
        {
            Console.WriteLine($"\nTried to update non-existent project '{projectName}' for '{assignedEmployee}', notifying email sender ({recipient}).");

            MimeMessage reply = new MimeMessage();
            BodyBuilder bodyBuilder = new BodyBuilder();

            // from
            reply.From.Add(new MailboxAddress("Boltmailer Mainserver", ConfigManager.EmailUsername));
            // to
            reply.To.Add(new MailboxAddress(recipient, recipient));
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
<h4>{ex?.Message}</h4>
</br></br>
<h1>In English:</h1>
<h4>This is an automated message sent by the Boltmailer Mainserver. <b>Please do not try to reply to this email.</b></h4>
<h3>There's been an error adding a project ({projectName}) for the user '{assignedEmployee}':</h3>
<h4>{ex?.Message}</h4>
                                ";
            }
            
            reply.Body = bodyBuilder.ToMessageBody();

            using SmtpClient smtpClient = GetSmtpClient();

            smtpClient.Send(reply);
            smtpClient.Disconnect(true);
        }

        /// <summary>
        /// Sends a help message to the Recipient.
        /// </summary>
        /// <param name="recipient"></param>
        private void SendHelpReply(string recipient)
        {
            Console.WriteLine($"\nUser ({recipient}) requested help. Replying with help MSG.");

            MimeMessage reply = new MimeMessage();
            BodyBuilder bodyBuilder = new BodyBuilder();

            // from
            reply.From.Add(new MailboxAddress("Boltmailer Mainserver", ConfigManager.EmailUsername));
            // to
            reply.To.Add(new MailboxAddress(recipient, recipient));
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
            
            
            using SmtpClient smtpClient = GetSmtpClient();

            smtpClient.Send(reply);
            smtpClient.Disconnect(true);
        }

        private void Log(string text, bool ok, string exception = null)
        {
            if (!firstStart) return;
            
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

        private static string GetTitleText()
        {
            const string title = @"
██████   ██████  ██   ████████ ███╗   ███╗ █████╗ ██╗██╗     ███████╗██████╗ 
██   ██ ██    ██ ██      ██    ████╗ ████║██╔══██╗██║██║     ██╔════╝██╔══██╗
██████  ██    ██ ██      ██    ██╔████╔██║███████║██║██║     █████╗  ██████╔╝
██   ██ ██    ██ ██      ██    ██║╚██╔╝██║██╔══██║██║██║     ██╔══╝  ██╔══██╗
██████   ██████  ███████ ██    ██║ ╚═╝ ██║██║  ██║██║███████╗███████╗██║  ██║
                               ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝╚══════╝╚══════╝╚═╝  ╚═╝
                ";

            return title;
        }

        private static string GetStartingUpText()
        {
            const string title = @"
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
















/*
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
using System.IO;
using System.Linq;
using System.Threading;

namespace Boltmailer_mainserver
{
    internal class BoltReader
    {
        private bool firstStart = true;

        private static bool isParsingMail;

        private System.Timers.Timer ticker;

        private readonly Random rnd = new Random();

        public void StartTicking()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Gray;

            if (firstStart)
            {
                DLog("Running first start", LogLevel.Everything);
                
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

            ticker.Elapsed += ReadInbox;

            int refreshFrequency = int.Parse(ConfigurationManager.AppSettings.Get("EmailRefreshFrequency"));

            if (refreshFrequency < 10000)
                refreshFrequency = 10000;

            ticker.Interval = refreshFrequency;
            
            DLog("Set ticker interval to " + ticker.Interval, LogLevel.Debug);

            ReadInbox(null, null);
            
            DLog("Inbox read complete", LogLevel.Debug);

            ticker.Start();
            
            DLog("Ticker started", LogLevel.Everything);
        }

        /// <summary>
        /// Reads the contents of the inbox.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void ReadInbox(object arg1, EventArgs arg2)
        {
            DLog("Inbox read started", LogLevel.Debug);
            
            if (isParsingMail)
            {
                DLog("Tried to parse mail while the last thread has not completed yet", LogLevel.Debug);
                return;
            }

            isParsingMail = true;
            DLog("Set parsing mail to true.", LogLevel.Everything);

            // Check if this was the first cycle, if it was, show info that everything is working and wait for next cycle
            if (firstStart)
            {
                DLog("Is first start", LogLevel.Debug);
                if (RunStartupChecks())
                {
                    DLog("Running startup checks", LogLevel.Debug);
                    Thread.Sleep(5000);
                    firstStart = false;
                    Console.Clear();
                    Console.WriteLine(GetTitleText());
                    Console.WriteLine($"\n'Output Directory' is now set to {ConfigManager.OutputDirectory}.");
                    Console.WriteLine($"\n'Email Refresh Frequency' is now set to {ticker.Interval}.");
                    Console.WriteLine($"\n'Trusted Domain' is now set to {ConfigManager.TrustedDomain}.\n");
                    Console.WriteLine("Initialization successful, reading started!");
                }
                else
                {
                    ticker.Stop();
                    Console.WriteLine("\nInitialization process halted!");
                }
            }
            else
            {
                DLog("Not first start. Creating Imap & Smtp clients...", LogLevel.Debug);

                // Check if connection was successful. If not, abort.
                if (GetImapClient() == null)
                {
                    DLog("[WARNING] IMAP-Connection was not successful, returning!", LogLevel.Debug);
                    isParsingMail = false;
                    return;
                }
                if (GetSmtpClient() == null)
                {
                    DLog("[WARNING] SMTP-connection was not successful, returning!", LogLevel.Debug);
                    isParsingMail = false;
                    return;
                }

                // Try to open the Imap inbox
                try
                {
                    GetImapClient().Inbox.Open(FolderAccess.ReadWrite);
                }
                catch (Exception ex)
                {
                    DLog("Could not read inbox: " + ex, LogLevel.Errors);
                    isParsingMail = false;
                    return;
                }
                DLog("Searching the inbox...", LogLevel.Debug);

                // Read the inbox contents
                IList<UniqueId> uIds = GetImapClient().Inbox.Search(SearchQuery.NotSeen);

                DLog($"Done. Found {uIds.Count} messages", LogLevel.Debug);
                // Handle the found messages
                HandleMessages(uIds);
                
                DLog("Messages handled.", LogLevel.Debug);

                // Disconnect, to not leave the connection hanging
                //GetImapClient().Disconnect(true);
                //GetSmtpClient().Disconnect(true);
                
                //DLog("Done.", LogLevel.Debug);
            }

            isParsingMail = false;
            DLog("Set parsing mail to false.", LogLevel.Everything);
        }

        private bool RunStartupChecks()
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
                Log("Opening inbox...", true);
            }
            catch (Exception ex)
            {
                Log("Opening inbox...", false, ex.Message);
                return false;
            }

            try
            {
                imapClient.Inbox.Search(SearchQuery.NotSeen);
                Log("Reading inbox contents...", true);
            }
            catch (Exception ex)
            {
                Log("Reading inbox contents...", false, ex.ToString());
                return false;
            }

            try
            {
                DirectoryInfo path = Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Test");

                File.CreateText(path.FullName + "\\testtext.txt").Close();

                Directory.Delete(path.FullName, true);

                Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Projektit");

                Log("Checking output folder...", true);
            }
            catch (Exception ex)
            {
                Log("Checking output folder...", false, ex.ToString());
                return false;
            }

            try
            {
                File.Create(ConfigManager.OutputDirectory + "\\Test").Close();
                File.Delete(ConfigManager.OutputDirectory + "\\Test");
                Log("Checking file priviledges...", true);
            }
            catch (Exception ex)
            {
                Log("Checking file priviledges...", false, ex.ToString());
                return false;
            }

            try
            {
                imapClient.Disconnect(true);
                smtpClient.Disconnect(true);
                Log("Disconnecting...", true);
            }
            catch (Exception ex)
            {
                Log("Disconnecting...", false, ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles the list of uIds, sending them to correct folders.
        /// </summary>
        /// <param name="uIds"></param>
        private void HandleMessages(IEnumerable<UniqueId> uIds)
        {
            foreach (UniqueId uid in uIds)
            {
                DLog("Getting the MSG info...", LogLevel.Everything);
                var message = GetImapClient().Inbox.GetMessage(uid);

                string sender = message.From.OfType<MailboxAddress>().Single().Address;
                string projectName = message.Subject;
                string assignedEmployee = "Vapaa";
                string projectDeadline = "Ei annettu";
                string timeEstimate = "Ei annettu";

                bool isUpdate = false;
                
                DLog("Done.", LogLevel.Everything);

                #region Check if the message comes from any trusted domain

                bool isTrusted = true;

                string[] domains = ConfigManager.TrustedDomain.Split(',');
                if (domains.Any(domain => sender[sender.LastIndexOf('@')..] != domain))
                {
                    isTrusted = false;
                }


                if (!isTrusted)
                {
                    DLog("Skipping a MSG from incorrect domain.", LogLevel.Debug);
                    
                    // Mark the email as seen to prevent reading it again
                    GetImapClient().Inbox.SetFlags(uid, MessageFlags.Seen, false);
                    
                    continue;
                }

                #endregion

                #region Check if message is a help-message

                if (message.Subject.ToLower() == "help")
                {
                    SendHelpReply(sender);

                    GetImapClient().Inbox.SetFlags(uid, MessageFlags.Seen, false);
                    
                    continue;
                }

                #endregion

                #region Read the MSG    //BUG: Fix the update messages reading as sender info

                try
                {
                    using (var reader = new StringReader(message.TextBody))
                    {
                        // Set the employee if it's not null
                        string employeeLn = reader.ReadLine();
                        if (!string.IsNullOrEmpty(employeeLn)) assignedEmployee = employeeLn;

                        // Set the deadline if it's not null
                        string deadlineLn = reader.ReadLine();
                        if (!string.IsNullOrEmpty(deadlineLn)) projectDeadline = deadlineLn;

                        // Set the time estimate if it's not null
                        string timeEstimateLn = reader.ReadLine();
                        if (!string.IsNullOrEmpty(timeEstimateLn)) timeEstimate = timeEstimateLn;

                        // Set the update flag
                        isUpdate = projectDeadline.ToLower().Contains("update") || timeEstimate.ToLower().Contains("update");
                    }

                    // Create necessary files (notes, msg, info), if the msg is not a update to existing project
                    if (!isUpdate)  //TODO: Check if the directory for the user exists, if not, notify the sender!
                    {
                        DLog("MSG is not an update.", LogLevel.Everything);
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
                        DLog("MSG is an update.", LogLevel.Everything);
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
                            SendErrorReply(sender, projectName, assignedEmployee, true);
                        }
                    }
                }
                catch (Exception ex)    // Error reading the MSG
                {
                    DirectoryInfo directory = Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Virheelliset" + "\\");

                    string path = directory.FullName + DateTime.Now.ToString("dd.MM") + "_" + rnd.Next(10000, 99999);

                    DLog($"\n\nError parsing a mail:\n{ex}.\n\nSaving at: {path}\n\nAnd notifying the sender of the mail.", LogLevel.Errors);

                    // Try saving the email to file
                    try
                    {
                        var msg = GetImapClient().Inbox.GetMessage(uid);

                        msg.WriteTo($"{path}.eml");
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }

                    SendErrorReply(sender, projectName, assignedEmployee, isUpdate, ex);
                }

                #endregion

                // Mark the email as seen to prevent reading it again
                GetImapClient().Inbox.SetFlags(uid, MessageFlags.Seen, false);
            }
        }

        private ImapClient imClient;
        /// <summary>
        /// Creates and logs on a Imap client
        /// </summary>
        /// <returns>Logged on client</returns>
        private ImapClient GetImapClient()
        {
            imClient = new ImapClient();
            DLog("Created new Imap-Client.", LogLevel.Everything);

            try
            {
                // Authenticate imap client
                if (ConfigManager.EmailImapUseSSL && !imClient.IsConnected)
                {
                    DLog("Connecting Imap-Client with SSL...", LogLevel.Debug);
                    imClient.CheckCertificateRevocation = false;
                    imClient.Connect(ConfigManager.EmailImapHost, ConfigManager.EmailImapPort, SecureSocketOptions.SslOnConnect);
                }
                else if(!imClient.IsConnected)
                {
                    DLog("Connecting Imap-Client without SSL...", LogLevel.Debug);
                    imClient.CheckCertificateRevocation = false;
                    imClient.Connect(ConfigManager.EmailImapHost, ConfigManager.EmailImapPort, SecureSocketOptions.None);
                }

                if (!imClient.IsAuthenticated)
                {
                    DLog("Authenticating Imap-Client", LogLevel.Debug);
                    imClient.Authenticate(ConfigManager.EmailUsername, ConfigManager.EmailPassword);
                }

                Log("Logging on (Imap)... ", true);

                return imClient;
            }
            catch (Exception ex)
            {
                Log("Logging on... ", false, ex.Message);

                return null;
            }
        }

        private SmtpClient smClient;

        /// <summary>
        /// Creates and logs on a Smtp client
        /// </summary>
        /// <returns>Logged on client</returns>
        private SmtpClient GetSmtpClient()
        {
            smClient = new SmtpClient();
            DLog("Created new Smtp-Client.", LogLevel.Everything);

            try
            {
                smClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                
                // Connect if needed
                if (ConfigManager.EmailSmtpUseSSL && !smClient.IsConnected)
                {
                    smClient.CheckCertificateRevocation = false;
                    smClient.Connect(ConfigManager.EmailSmtpHost, ConfigManager.EmailSmtpPort, SecureSocketOptions.SslOnConnect);
                }
                else if(!smClient.IsConnected)
                {
                    smClient.CheckCertificateRevocation = false;
                    smClient.Connect(ConfigManager.EmailSmtpHost, ConfigManager.EmailSmtpPort, SecureSocketOptions.None);
                }
                
                // Auth if needed
                if(!smClient.IsAuthenticated)
                    smClient.Authenticate(ConfigManager.EmailUsername, ConfigManager.EmailPassword);

                Log("Logging on (Smtp)... ", true);

                return smClient;
            }
            catch (Exception ex)
            {
                Log("Logging on... ", false, ex.Message);

                return null;
            }
        }

        /// <summary>
        /// Replies to Recipient with an error message describing the error.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="projectName"></param>
        /// <param name="assignedEmployee"></param>
        /// <param name="update"></param>
        /// <param name="ex"></param>
        private void SendErrorReply(string recipient, string projectName, string assignedEmployee, bool update, Exception ex = null)
        {
            DLog($"\nTried to update non-existent project '{projectName}' for '{assignedEmployee}', notifying email sender ({recipient}).", LogLevel.Debug);

            var reply = new MimeMessage();
            var bodyBuilder = new BodyBuilder();

            // from
            reply.From.Add(new MailboxAddress("Boltmailer Mainserver", ConfigManager.EmailUsername));
            // to
            reply.To.Add(new MailboxAddress(recipient, recipient));
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
<h4>{ex?.Message}</h4>

</br></br>

<h1>In English:</h1>
<h4>This is an automated message sent by the Boltmailer Mainserver. <b>Please do not try to reply to this email.</b></h4>
<h3>There's been an error adding a project ({projectName}) for the user '{assignedEmployee}':</h3>
<h4>{ex?.Message}</h4>
                                ";
            }
            
            reply.Body = bodyBuilder.ToMessageBody();

            GetSmtpClient().Send(reply);
        }

        /// <summary>
        /// Sends a help message to the Recipient.
        /// </summary>
        /// <param name="recipient"></param>
        private void SendHelpReply(string recipient)
        {
            DLog($"\nUser ({recipient}) requested help.", LogLevel.Debug);

            var reply = new MimeMessage();
            var bodyBuilder = new BodyBuilder();

            // from
            reply.From.Add(new MailboxAddress("Boltmailer Mainserver", ConfigManager.EmailUsername));
            // to
            reply.To.Add(new MailboxAddress(recipient, recipient));
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
            
            DLog("Replying with help MSG.", LogLevel.Debug);

            GetSmtpClient().Send(reply);
            //smtpClient.Disconnect(true);
            
            DLog("Done.", LogLevel.Debug);
        }

        public static void DLog(string text, LogLevel minLogLevel)
        {
            LogLevel level = ConfigManager.LogLevel;
            
            if (level >= minLogLevel)
            {
                Console.WriteLine(text);
            }
        }

        private void Log(string text, bool ok, string exception = null)
        {
            if (!firstStart) return;
            
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

        private static string GetTitleText()
        {
            const string title = @"
██████   ██████  ██   ████████ ███╗   ███╗ █████╗ ██╗██╗     ███████╗██████╗ 
██   ██ ██    ██ ██      ██    ████╗ ████║██╔══██╗██║██║     ██╔════╝██╔══██╗
██████  ██    ██ ██      ██    ██╔████╔██║███████║██║██║     █████╗  ██████╔╝
██   ██ ██    ██ ██      ██    ██║╚██╔╝██║██╔══██║██║██║     ██╔══╝  ██╔══██╗
██████   ██████  ███████ ██    ██║ ╚═╝ ██║██║  ██║██║███████╗███████╗██║  ██║
                               ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝╚══════╝╚══════╝╚═╝  ╚═╝
                ";

            return title;
        }

        private static string GetStartingUpText()
        {
            const string title = @"
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
}*/