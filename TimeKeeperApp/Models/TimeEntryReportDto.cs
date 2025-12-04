namespace TimeKeeperApp.Models
{
    public class TimeEntryReportDto
    {
        public string? UserName { get; set; }
        public DateOnly Week { get; set; }
        public DateTime TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public string? HoursWorked { get; set; }   // formatted per-entry "H:mm"
        public double HoursDecimal { get; set; }   // numeric hours for summing
    }
}