﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Boltmailer_mainserver
{
    public static class ConfigManager
    {
        public static string EmailImapHost { get; set; }
        public static int EmailImapPort { get; set; }
        public static bool EmailImapUseSSL { get; set; }
        public static string EmailSmtpHost { get; set; }
        public static int EmailSmtpPort { get; set; }
        public static bool EmailSmtpUseSSL { get; set; }
        public static string EmailUsername { get; set; }
        public static string EmailPassword { get; set; }
        public static int EmailRefreshFrequency { get; set; }
        public static bool StartReaderOnOpen { get; set; }
        public static string TrustedDomain { get; set; }

        public static void ReadConfig()
        {
            EmailImapHost = ConfigurationManager.AppSettings.Get("EmailImapHost");
            EmailImapPort = int.Parse(ConfigurationManager.AppSettings.Get("EmailImapPort"));
            EmailImapUseSSL = bool.Parse(ConfigurationManager.AppSettings.Get("EmailImapUseSSL"));
            EmailSmtpHost = ConfigurationManager.AppSettings.Get("EmailSmtpHost");
            EmailSmtpPort = int.Parse(ConfigurationManager.AppSettings.Get("EmailSmtpPort"));
            EmailSmtpUseSSL = bool.Parse(ConfigurationManager.AppSettings.Get("EmailSmtpUseSSL"));
            EmailUsername = ConfigurationManager.AppSettings.Get("EmailUsername");
            EmailPassword = ConfigurationManager.AppSettings.Get("EmailPassword");
            TrustedDomain = ConfigurationManager.AppSettings.Get("TrustedDomain");
        }
    }
}