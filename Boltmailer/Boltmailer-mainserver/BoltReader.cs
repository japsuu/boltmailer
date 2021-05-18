using Boltmailer_common;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        private void Read(object sender, EventArgs args)
        {
            // Create connection and log on to the mail server
            using var client = new ImapClient();
            try
            {
                client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
                client.Authenticate("0481664@gmail.com", "Thejapsu1");
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
                client.Inbox.Open(FolderAccess.ReadWrite);
                LOG("Opening inbox...", true);
            }
            catch (Exception ex)
            {
                LOG("Opening inbox...", false, ex.Message);
                return;
            }

            // Read the inbox contents
            var uids = client.Inbox.Search(SearchQuery.NotSeen);

            foreach (var uid in uids)
            {
                try
                {
                    var message = client.Inbox.GetMessage(uid);
                    string projectName = message.Subject;
                    string assignedEmployee;
                    string projectDeadline;

                    using (var reader = new StringReader(message.TextBody))
                    {
                        assignedEmployee = reader.ReadLine();
                        projectDeadline = reader.ReadLine();
                    }

                    Console.WriteLine($"\nWriting project '{projectName}' for '{assignedEmployee}' with deadline '{projectDeadline}' to file...");

                    projectName = FilenameFromTitle(projectName).ToLower();
                    assignedEmployee = FilenameFromTitle(assignedEmployee);

                    DirectoryInfo path = Directory.CreateDirectory("Projektit" + "\\" + assignedEmployee + "\\" + projectName);

                    // Create notes file
                    File.CreateText(path + "\\" + "notes").Close();

                    message.WriteTo($"{path}\\{projectName}_{rnd.Next(1000, 9999)}.eml");

                    // Create the info file
                    ProjectInfo info = new ProjectInfo() { ProjectName = message.Subject, Deadline = projectDeadline, TimeEstimate = "Ei annettu", Status = ProjectStatus.Aloittamaton };
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Converters =
                        {
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                        }
                    };
                    string json = JsonSerializer.Serialize(info, typeof(ProjectInfo), options);
                    File.WriteAllText(path.FullName + "\\info.json", json);

                    JsonTools.WriteJson(info, path.FullName);

                    //JsonSerializerOptions options = new JsonSerializerOptions
                    //{
                    //    WriteIndented = true,
                    //    Converters =
                    //    {
                    //    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                    //    }
                    //};
                    //string json = JsonSerializer.Serialize(info, typeof(ProjectInfo), options);
                    //File.WriteAllText(path.FullName + "\\info.json", json);
                }
                catch (Exception ex)
                {
                    DirectoryInfo directory = Directory.CreateDirectory("Virheelliset" + "\\");
                    string path = directory.FullName + DateTime.Now.ToString("dd.MM") + "_" + rnd.Next(1000, 9999);

                    Console.WriteLine($"\n\nFound an email that cannot be saved (full html?):\n{ex.Message}.\nSaving at: {path}\n\n");

                    try
                    {
                        var message = client.Inbox.GetMessage(uid);

                        message.WriteTo($"{path}.eml");
                    }
                    catch
                    {
                    }
                }

                client.Inbox.SetFlags(uid, MessageFlags.Seen, false);
            }
            LOG("Reading inbox contents...", true);

            client.Disconnect(true);

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
