using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Boltmailer_common
{
    public static class FileTools
    {
        /// <summary>
        /// Reads the info.json located at the path provided.
        /// </summary>
        /// <param name="path">Path to read, without the file name.</param>
        /// <returns></returns>
        public static IProjectInfo ReadJson(string path)
        {
            string json = File.ReadAllText(path + "\\info.json");
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

            try
            {
                ProjectInfo info = JsonSerializer.Deserialize<ProjectInfo>(json, options);
                return info;
            }
            catch (Exception ex)
            {
                return new ProjectInfoError($"Virheellinen info.json tiedosto sijainnissa '{path}'! Projekti skipataan, ja sitä ei ladata.", ex.Message, path);
            }
        }

        /// <summary>
        /// Writes the info.json file to the path provided. If the file exists, it is overwritten. If there is no changes, we don't save the file.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="path">Path where to write, without the file name.</param>
        public static bool WriteInfo(ProjectInfo info, string path, bool force = false)
        {
            path += "\\info.json";

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
            string json = JsonSerializer.Serialize(info, typeof(ProjectInfo), options);

            try
            {
                if (force)
                {
                    RemoveHidden(path);

                    File.WriteAllText(path, json);

                    ApplyHidden(path);

                    System.Diagnostics.Debug.WriteLine("\ninfo written\n");

                    return true;
                }
                else
                {
                    if (json != File.ReadAllText(path))
                    {
                        RemoveHidden(path);

                        File.WriteAllText(path, json);

                        ApplyHidden(path);

                        System.Diagnostics.Debug.WriteLine("\ninfo written\n");

                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("\no need to write, no changes\n");
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// WriteAllText that handles attributes correctly.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="path">Path where to write, WITH the file name.</param>
        public static void WriteAllText(string path, string contents)
        {
            if (contents != File.ReadAllText(path))
            {
                RemoveHidden(path);

                File.WriteAllText(path, contents);

                ApplyHidden(path);

                System.Diagnostics.Debug.WriteLine("\ninfo written\n");
            }
        }

        /// <summary>
        /// Removes "hidden" attribute.
        /// </summary>
        /// <param name="path"></param>
        private static void RemoveHidden(string path)
        {
            if(File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }
        }

        /// <summary>
        /// Applies "hidden" attribute.
        /// </summary>
        /// <param name="path"></param>
        private static void ApplyHidden(string path)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Hidden);
            }
        }
    }
}
