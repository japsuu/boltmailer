using System;

namespace Boltmailer_common
{
    public class ProjectInfo : IProjectInfo
    {
        public string ProjectName { get; set; }
        public string Deadline { get; set; }
        public string TimeEstimate { get; set; }
        public ProjectStatus Status { get; set; }
    }

    public class ProjectInfoError : IProjectInfo
    {
        public string Error { get; set; }
        public string Exception { get; set; }
        public string Path { get; set; }

        public ProjectInfoError(string error, string exception, string path)
        {
            Error = error;
            Exception = exception;
            Path = path;
        }
    }

    public enum ProjectStatus
    {
        Aloittamaton,
        Kesken,
        Valmis
    }

    public interface IProjectInfo
    {

    }
}