namespace TimeKeeperApp.Models
{
    public class UserWeekTotalDto
    {
        public string? UserName { get; set; }
        public double TotalHours { get; set; }
        public string? TotalHoursFormatted { get; set; } // e.g. "38:30"
    }
}