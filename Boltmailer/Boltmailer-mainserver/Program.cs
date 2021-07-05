using System.Configuration;
using System.Collections.Specialized;
using Boltmailer_common;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Boltmailer_mainserver
{
    class Program
    {
        static bool quit;
        static BoltReader boltReader = null;

        static void Main(string[] args)
        {
            ConfigManager.ReadConfig();

            Console.WriteLine(GetTitleText());
            Console.WriteLine("Open help with '#help'");
            string startOnOpen = ConfigurationManager.AppSettings.Get("StartReaderOnOpen");
            if (startOnOpen != "false")
            {
                Start();
            }

            while (!quit)
            {
                string input = Console.ReadLine();

                if (input.StartsWith('#'))
                {
                    if (input == "#quit")
                        quit = true;

                    if (input == "#help")
                        ShowHelp();

                    if (input == "#start")
                    {
                        Start();
                    }

                    if (input == "#stop")
                    {
                        if (boltReader != null)
                            boltReader = null;

                        Console.WriteLine("Reader stopped.");
                    }
                }
                else
                {
                    Console.WriteLine("Unknown command, please try #help .");
                }
            }
        }

        static void Start()
        {
            boltReader = new BoltReader();
            boltReader.StartTicking();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Initialization successful, reading started!");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Open help with '#help'");
        }

        static void ShowHelp()
        {
            string help =
                $@"
Boltmailer Server version {Versioning.GetServerVersion()}" + @"
-------------------------------------------------
|                                               |
|                                               |
|   #help   -   Show this box.                  |
|                                               |
|   #quit   -   Close the server.               |
|                                               |
|   #start  -   Starts reading mails            |
|                                               |
|   #stop   -   Stops reading mails             |
|                                               |
|                                               |
-------------------------------------------------
                ";

            Console.WriteLine(help);
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
    }
}
