using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;

namespace Boltmailer_mainserver
{
    class BoltReader
    {
        private Timer ticker;
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
            try
            {
                using var client = new ImapClient();
                Console.WriteLine("Checking for mails...");
                client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
                client.Authenticate("0481664@gmail.com", "Thejapsu1");

                client.Inbox.Open(FolderAccess.ReadWrite);

                var uids = client.Inbox.Search(SearchQuery.NotSeen);

                foreach (var uid in uids)
                {
                    var message = client.Inbox.GetMessage(uid);
                    string name = message.Subject;
                    string employee;
                    string deadline;

                    using (var reader = new StringReader(message.TextBody))
                    {
                        employee = reader.ReadLine();
                        deadline = reader.ReadLine();
                    }

                    Console.WriteLine($"Writing project '{name}' for '{employee}' with deadline '{deadline}' to file...");

                    //make sure the filename is safe to use
                    foreach (char invalid in Path.GetInvalidPathChars())
                    {
                        if (invalid.ToString() == " ")
                            name = name.Replace(invalid.ToString(), "_");
                        else
                            name = name.Replace(invalid.ToString(), "");
                    }

                    message.WriteTo(Directory.CreateDirectory("Projects") + "\\" + $"{name}.eml");

                    client.Inbox.SetFlags(uid, MessageFlags.Seen, false);
                }

                client.Disconnect(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading the mailbox: " + ex);
            }
        }
    }
}
