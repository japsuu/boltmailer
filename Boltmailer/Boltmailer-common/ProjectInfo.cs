using System;

namespace Boltmailer_common
{
    public class ProjectInfo
    {
        public string ProjectName { get; set; }
        public string Deadline { get; set; }
        public string TimeEstimate { get; set; }
        public ProjectState State { get; set; }
    }

    public enum ProjectState
    {
        NotStarted,
        InProgress,
        Ready
    }
}