using Boltmailer_common;

namespace Boltmailer_client
{
    public class StatusComboItem
    {
        public ProjectStatus Status { get; set; }
        public string DisplayText { get; set; }

        public StatusComboItem(ProjectStatus status, string text)
        {
            Status = status;
            DisplayText = text;
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
