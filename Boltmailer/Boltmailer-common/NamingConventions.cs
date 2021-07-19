using System;
using System.Collections.Generic;
using System.Text;

namespace Boltmailer_common
{
    public static class NamingConventions
    {
        public static string EmployeeFromPath(string path)
        {
            return path.Substring(path.LastIndexOf('\\') + 1).Replace('_', ' ').Replace('-', ' ').ToLower();
        }

        /// <summary>
        /// Retrieves the substring after the last '\'
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string FilenameFromPath(string fullPath, bool includeSlash = false)
        {
            if(!includeSlash)
                return fullPath.Substring(fullPath.LastIndexOf('\\') + 1);
            else
                return fullPath.Substring(fullPath.LastIndexOf('\\'));
        }

        public static string FilenameFromTitle(string name)
        {
            // first trim the raw string
            string safe = name.Trim();

            // replace spaces with hyphens
            safe = safe.Replace(" ", "-").ToLower();

            // replace any 'double spaces' with singles
            if (safe.IndexOf("--") > -1)
                while (safe.IndexOf("--") > -1)
                    safe = safe.Replace("--", "-");

            // trim out illegal characters
            safe = System.Text.RegularExpressions.Regex.Replace(safe, "[^a-ö0-9\\-]", "");

            // trim the length
            if (safe.Length > 50)
                safe = safe.Substring(0, 49);

            // clean the beginning and end of the filename
            char[] replace = { '-', '.' };
            safe = safe.TrimStart(replace);
            safe = safe.TrimEnd(replace);

            return safe;
        }

        public static string FileExtensionFromPath(string fullPath)
        {
            return fullPath.Substring(fullPath.LastIndexOf('.') + 1);
        }
    }
}
