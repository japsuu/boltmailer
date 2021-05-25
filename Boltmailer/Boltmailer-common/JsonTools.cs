using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Boltmailer_common
{
    public static class JsonTools
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
        /// Writes the info.json file to the path provided. If the file exists, it is overwritten.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="path">Path where to write, without the file name.</param>
        public static void WriteJson(ProjectInfo info, string path)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
            string json = JsonSerializer.Serialize(info, typeof(ProjectInfo), options);
            File.WriteAllText(path + "\\info.json", json);
        }
    }
}
