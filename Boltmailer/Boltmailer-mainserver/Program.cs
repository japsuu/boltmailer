using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Boltmailer_mainserver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(GetTitle());

            BoltReader boltReader = new BoltReader();
            boltReader.StartTicking();

            Console.WriteLine("CLI Ready for other actions.");
            Console.ReadKey();
        }

        static string GetTitle()
        {
            string title =
                @"
██████   ██████  ██   ████████ ███╗   ███╗ █████╗ ██╗██╗     ███████╗██████╗ 
██   ██ ██    ██ ██      ██    ████╗ ████║██╔══██╗██║██║     ██╔════╝██╔══██╗
██████  ██    ██ ██      ██    ██╔████╔██║███████║██║██║     █████╗  ██████╔╝
██   ██ ██    ██ ██      ██    ██║╚██╔╝██║██╔══██║██║██║     ██╔══╝  ██╔══██╗
██████   ██████  ███████ ██    ██║ ╚═╝ ██║██║  ██║██║███████╗███████╗██║  ██║
                               ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝╚══════╝╚══════╝╚═╝  ╚═╝

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
