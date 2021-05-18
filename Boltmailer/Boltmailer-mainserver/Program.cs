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

        static void Main(string[] args)
        {
            Console.WriteLine(GetTitleText());
            Console.WriteLine("Open help with '#help'");

            BoltReader boltReader = null;

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
                        boltReader = new BoltReader();
                        boltReader.StartTicking();

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Initialization successfull, reading started!");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Open help with '#help'");
                    }

                    if (input == "#stop")
                    {
                        if (boltReader != null)
                            boltReader = null;

                        Console.WriteLine("Reader stopped.");
                    }
                }
            }
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
