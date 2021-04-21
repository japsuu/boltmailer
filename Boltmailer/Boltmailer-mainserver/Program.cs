using MimeKit;
using System;
using System.IO;

namespace Boltmailer_mainserver
{
    class Program
    {
        static void Main(string[] args)
        {
            BoltReader boltReader = new BoltReader();
            boltReader.StartTicking();

            Console.ReadKey();
        }
    }
}
