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
    internal static class Program
    {
        private static bool quit;
        private static BoltReader boltReader = null;

        private static void Main(string[] args)
        {
            ConfigManager.ReadConfig();

            Console.WriteLine(GetTitleText());
            Console.WriteLine("Open help with '#help'");

            Start();

            while (!quit)
            {
                string input = Console.ReadLine();

                if (input != null && input.StartsWith('#'))
                {
                    switch (input)
                    {
                        case "#quit":
                            quit = true;
                            break;
                        case "#help":
                            ShowHelp();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Unknown command, please try #help .");
                }
            }
        }

        private static void Start()
        {
            boltReader = new BoltReader();
            boltReader.StartTicking();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\nOpen help with '#help'");
        }

        private static void ShowHelp()
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
|                                               |
-------------------------------------------------
                ";

            Console.WriteLine(help);
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
    }
}
