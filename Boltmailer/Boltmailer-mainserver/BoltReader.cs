﻿using Boltmailer_common;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;

namespace Boltmailer_mainserver
{
    class BoltReader
    {
        Timer ticker;
        Random rnd = new Random();

        public void StartTicking()
        {
            Console.WriteLine("Started monitoring for incoming mails");
            ticker = new Timer();
            ticker.Elapsed += new ElapsedEventHandler(Read);
            ticker.Interval = 10000;
            ticker.Start();
            Read(null, null);
        }

        public void Read(object sender, EventArgs args)
        {
            using var client = new ImapClient();
            Console.Write(".");
            client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
            client.Authenticate("0481664@gmail.com", "Thejapsu1");

            client.Inbox.Open(FolderAccess.ReadWrite);

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

                    StreamWriter notesStream = File.CreateText(path + "\\" + "notes");
                    notesStream.Close();

                    message.WriteTo($"{path}\\{projectName}_{rnd.Next(1000, 9999)}.eml");

                    // Create the info file
                    ProjectInfo info = new ProjectInfo() { ProjectName = message.Subject, Deadline = projectDeadline, TimeEstimate = "Ei annettu", State = ProjectState.Aloittamaton };
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n\nFound an email that cannot be saved (full html?). Marking it as read anyways to prevent future exceptions:\n{ex.Message}\n\n");
                }

                client.Inbox.SetFlags(uid, MessageFlags.Seen, false);
            }

            client.Disconnect(true);
        }

        public static string FilenameFromTitle(string name)
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
    }
}
