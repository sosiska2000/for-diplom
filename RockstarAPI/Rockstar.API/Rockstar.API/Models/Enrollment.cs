using System;

namespace Rockstar.API.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ScheduleId { get; set; }
        public DateTime EnrolledAt { get; set; }
        public string Status { get; set; } = "enrolled";
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsCancelledByAdmin { get; set; } = false;
        public User? User { get; set; }
        public Schedule? Schedule { get; set; }
    }
}