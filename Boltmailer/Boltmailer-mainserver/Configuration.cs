using System;
using System.Collections.Generic;
using System.Text;

namespace Boltmailer_mainserver
{
    public class Configuration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int RefreshFrequency { get; set; }
        public bool StartReaderOnOpen { get; set; }
    }
}
