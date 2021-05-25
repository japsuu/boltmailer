using Boltmailer_common;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
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
            }

            ticker = new System.Timers.Timer();
            ticker.Elapsed += new ElapsedEventHandler(Read);
            ticker.Interval = 10000;
            ticker.Start();
            Read(null, null);
        }

        private void Read(object arg1, EventArgs arg2)
        {
            // Create connection and log on to the mail server
            using var imapClient = new ImapClient();
            using var smtpClient = new SmtpClient();
            try
            {
                imapClient.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
                imapClient.Authenticate("0481664@gmail.com", "Thejapsu1");
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                smtpClient.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
                smtpClient.Authenticate("0481664@gmail.com", "Thejapsu1");
                LOG("Logging on... ", true);
            }
            catch (Exception ex)
            {
                LOG("Logging on... ", false, ex.Message);
                return;
            }

            // Open the inbox
            try
            {
                imapClient.Inbox.Open(FolderAccess.ReadWrite);
                LOG("Opening inbox...", true);
            }
            catch (Exception ex)
            {
                LOG("Opening inbox...", false, ex.Message);
                return;
            }

            // Read the inbox contents
            var uids = imapClient.Inbox.Search(SearchQuery.NotSeen);

            foreach (var uid in uids)
            {
                bool isUpdate = false;
                var message = imapClient.Inbox.GetMessage(uid);
                string sender = message.From.OfType<MailboxAddress>().Single().Address;
                string projectName = message.Subject;
                string assignedEmployee = "";
                string projectDeadline;
                string timeEstimate;

                try
                {
                    using (var reader = new StringReader(message.TextBody))
                    {
                        assignedEmployee = reader.ReadLine();
                        projectDeadline = reader.ReadLine();

                        if (string.IsNullOrEmpty(projectDeadline))
                        {
                            throw new Exception("Missing Parameters :(");
                        }

                        isUpdate = projectDeadline.ToLower().Contains("update");
                        if(!isUpdate)
                            timeEstimate = reader.ReadLine();
                    }

                    if (!isUpdate)
                    {
                        if (!projectDeadline.Contains("update"))
                        {
                            projectName = FilenameFromTitle(projectName).ToLower();
                            assignedEmployee = FilenameFromTitle(assignedEmployee);

                            DirectoryInfo path = Directory.CreateDirectory("Projektit" + "\\" + assignedEmployee + "\\" + projectName);

                            // Create notes file if needed
                            if (!File.Exists(path + "\\" + "notes"))
                            {
                                File.CreateText(path + "\\" + "notes").Close();
                            }

                            message.WriteTo($"{path}\\{projectName}_{rnd.Next(1000, 9999)}.eml");

                            // Create the info file if needed
                            if (!File.Exists(path + "\\info.json"))
                            {
                                ProjectInfo info = new ProjectInfo() { ProjectName = message.Subject, Deadline = projectDeadline, TimeEstimate = "Ei annettu", Status = ProjectStatus.Aloittamaton };
                                JsonTools.WriteJson(info, path.FullName);
                            }
                            Console.WriteLine($"\nWrote project '{projectName}' for '{assignedEmployee}' with deadline '{projectDeadline}' to file.");
                        }
                    }
                    else
                    {
                        projectName = FilenameFromTitle(projectName).ToLower();
                        assignedEmployee = FilenameFromTitle(assignedEmployee);

                        if(Directory.Exists("Projektit" + "\\" + assignedEmployee + "\\" + projectName))
                        {
                            DirectoryInfo path = Directory.CreateDirectory("Projektit" + "\\" + assignedEmployee + "\\" + projectName);

                            message.WriteTo($"{path}\\{projectName}_update_{rnd.Next(1000, 9999)}.eml");

                            Console.WriteLine($"\nWrote update to project '{projectName}' for '{assignedEmployee}' to file.");
                        }
                        else    // Project to update doesn't exist, notify sender.
                        {
                            SendErrorReply(smtpClient, sender, projectName, assignedEmployee, isUpdate);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DirectoryInfo directory = Directory.CreateDirectory("Virheelliset" + "\\");
                    string path = directory.FullName + DateTime.Now.ToString("dd.MM") + "_" + rnd.Next(1000, 9999);

                    Console.WriteLine($"\n\nError parsing a mail:\n{ex}.\nSaving at: {path}\nAnd notifying the sender of the mail.");

                    try
                    {
                        var mesg = imapClient.Inbox.GetMessage(uid);

                        mesg.WriteTo($"{path}.eml");
                    }
                    catch
                    {
                    }
                    SendErrorReply(smtpClient, sender, projectName, assignedEmployee, isUpdate, ex);
                }

                imapClient.Inbox.SetFlags(uid, MessageFlags.Seen, false);
            }
            LOG("Reading inbox contents...", true);

            imapClient.Disconnect(true);

            if (firstStart)
            {
                Thread.Sleep(1000);
                firstStart = false;
                Console.Clear();
                Console.WriteLine(GetTitleText());
            }
            //else
            //{
            //    Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
            //    Console.WriteLine("Messages refreshed " + DateTime.Now.ToString("HH:mm:ss"));
            //}
        }

        private void SendErrorReply(SmtpClient smtpClient, string Recipient, string projectName, string assignedEmployee, bool update, Exception ex = null)
        {
            Console.WriteLine($"\nTried to update non-existant project '{projectName}' for '{assignedEmployee}', notifying email sender ({Recipient}).");

            var reply = new MimeMessage();
            var bodyBuilder = new BodyBuilder();

            // from
            reply.From.Add(new MailboxAddress("Boltmailer Mainserver", "0481664@gmail.com"));
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

        private static string FilenameFromTitle(string name)
        {
            // first trim the raw string
            string safe = name.Trim();

            // replace spaces with hyphens
            safe = safe.Replace(" ", "-").ToLower();

            // replace any 'double spaces' with singles
            if (safe.IndexOf("--") > -1)
                while (safe.IndexOf("--") > -1)
                    safe = safe.Replace("--", "-");

            // trim out illegal characters
            safe = System.Text.RegularExpressions.Regex.Replace(safe, "[^a-ö0-9\\-]", "");

            // trim the length
            if (safe.Length > 50)
                safe = safe.Substring(0, 49);

            // clean the beginning and end of the filename
            char[] replace = { '-', '.' };
            safe = safe.TrimStart(replace);
            safe = safe.TrimEnd(replace);

            return safe;
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
