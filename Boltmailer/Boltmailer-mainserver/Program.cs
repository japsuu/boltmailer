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
                        case "#createusr":
                        {
                            Console.WriteLine("Please input the name of the new user: ");
                            string name = NamingConventions.FilenameFromTitle(Console.ReadLine());
                            DirectoryInfo userDir = Directory.CreateDirectory(ConfigManager.OutputDirectory + "\\Projektit\\" + name);
                            
                            Console.WriteLine("User directory created at " + userDir.FullName);
                            
                            break;
                        }
                        case "#deleteusr":
                        {
                            Console.WriteLine("Please input the name of the user to delete: ");
                            string name = Console.ReadLine();
                            string dir = ConfigManager.OutputDirectory + "\\Projektit\\" + NamingConventions.FilenameFromTitle(name);
                            
                            Console.WriteLine("WARNING!!! Deleting a user will delete their user directory, and all data inside it!");
                            Console.WriteLine("The directory " + dir + " will be deleted, alongside of all of it's contents.");
                            Console.WriteLine("Do you want to continue? (Yes/No).");
                            
                            if(Console.ReadLine()?.ToLower() != "yes") break;
                            
                            Directory.Delete(dir, true);
                            
                            Console.WriteLine("User directory at " + dir + " deleted.");
                            
                            break;
                        }
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
|   #createusr  -   Create a new user.          |
|                                               |
|   #deleteusr  -   Delete a user.              |
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
