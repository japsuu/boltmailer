namespace Boltmailer_client
{
    public class EmployeeComboItem
    {
        public string EmployeePath { get; set; }
        public string DisplayText { get; set; }

        public EmployeeComboItem(string employeePath, string text)
        {
            EmployeePath = employeePath;
            DisplayText = text;
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
