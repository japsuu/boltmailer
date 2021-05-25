using System;
using System.Collections.Generic;
using System.Text;

namespace Boltmailer_common
{
    public static class Versioning
    {
        private static readonly int SERVER_VERSION_NUMBER = 11;
        private static readonly int CLIENT_VERSION_NUMBER = 26;

        /// <summary>
        /// Current version number of the main server.
        /// Should follow the pattern "YEAR-MONTH.BUILD_NBR".
        /// Example: "21-05.21"
        /// </summary>
        public static string GetServerVersion()
        {
            string date = DateTime.Now.ToString("yy-MM");
            return date + "." + SERVER_VERSION_NUMBER;
        }

        /// <summary>
        /// Current version number of the client application.
        /// Should follow the pattern "YEAR-MONTH.BUILD_NBR".
        /// Example: "21-05.21"
        /// </summary>
        public static string GetClientVersion()
        {
            string date = DateTime.Now.ToString("yy-MM");
            return date + "." + CLIENT_VERSION_NUMBER;
        }
    }
}
