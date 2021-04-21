using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Boltmailer_mainserver
{
    class BoltReader
    {

        public void Read()
        {
            string outputPath = Directory.GetCurrentDirectory();
            string streamPath = GetMailboxPath();
            Log("READER", "Locking to directory: " + streamPath + "\n\t\twith the output path: " + outputPath);

            FileStream stream;
            // Open the storage file with FileStream
            try
            {
                stream = new FileStream(streamPath + "Inbox", FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                Log("ERROR", "Could not read the mailbox: " + ex.Message);
                return;
            }

            // Load every message from a Unix mbox
            var parser = new MimeParser(stream, MimeFormat.Mbox);
            while (!parser.IsEndOfStream)
            {
                var message = parser.ParseMessage();

                Log("READER", message.Subject);
            }
        }

        string GetMailboxPath()
        {
            string rootPath = "C:\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\Thunderbird\\Profiles\\";

            string[] mailBoxes = Directory.GetDirectories(rootPath, "*default-release*");

            if (mailBoxes.Length < 1)
            {
                Log("ERROR", $"No mailbox found at '{rootPath}'!");
                return null;
            }
            else if (mailBoxes.Length > 1)
            {
                Log("READER", "Multiple mailboxes found. Please select the one to use:");
                for (int i = 0; i < mailBoxes.Length; i++)
                {
                    Console.WriteLine($"[{i}] " + mailBoxes[i]);
                }
                Log("READER", "Input the ID of the mailbox you want to select: ");
                return mailBoxes[Convert.ToInt32(Console.ReadLine())] + "\\Mail\\pop.gmail.com\\";
            }
            else
            {

                return mailBoxes[0] + "\\Mail\\pop.gmail.com\\";
            }
        }

        void Log(string prefix, string data)
        {
            Console.WriteLine($"[{prefix}]\t{data}");
        }
    }
}
