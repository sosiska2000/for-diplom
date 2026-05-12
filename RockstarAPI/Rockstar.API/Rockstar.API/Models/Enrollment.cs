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

        // Навигационные свойства
        public User? User { get; set; }
        public Schedule? Schedule { get; set; }
    }
}