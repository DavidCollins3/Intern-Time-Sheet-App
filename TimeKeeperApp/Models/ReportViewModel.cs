using System.Collections.Generic;

namespace TimeKeeperApp.Models
{
    public class ReportViewModel
    {
        public List<TimeEntryReportDto> Entries { get; set; } = new();
        public List<UserWeekTotalDto> Totals { get; set; } = new();
    }
}