using System.ComponentModel.DataAnnotations;

namespace TimeKeeperApp.Models
{
    public class TimeEntry
    {
        // Primary key for identifying Time Entries
        public int TimeEntryId { get; set; }

        // Submitting User's ID (foreign key) from AspNetUser table
        public string? UserID { get; set; }

        // Pay Period/Week which the time entry falls into  
        public DateOnly Week { get; set; }

        // "Clock In" time
        public DateTime TimeIn { get; set; }

        // "Clock Out" time
        public DateTime? TimeOut { get; set; }

        // String parsed from TimeSpan value of TimeOut - TimeIn
        // HoursWorked = TimeOut.Subtract(TimeIn).ToString();
        public string? HoursWorked
        {
            get
            {
                if (TimeOut.HasValue)
                {
                    return (TimeOut.Value - TimeIn).ToString();
                }
                return null;
            }
            private set { }
        }

        // Has the time entry been supervisor approved?
        public bool ApprovalStatus { get; set; }
    }
}
