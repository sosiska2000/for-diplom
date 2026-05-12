using System;
using System.Collections.Generic;

namespace Rockstar.API.Models
{
    public class RecurringSchedule
    {
        public int Id { get; set; }
        public int TrainerId { get; set; }
        public int DirectionId { get; set; }
        public int? ServiceId { get; set; }
        public int DurationMinutes { get; set; }
        public int MaxParticipants { get; set; }
        public decimal? Price { get; set; }
        public bool IsGroup { get; set; }
        public string Pattern { get; set; } = string.Empty;
        public int Interval { get; set; } = 1;
        public string? WeekDays { get; set; }
        public int? DayOfMonth { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxOccurrences { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Trainer? Trainer { get; set; }
        public Direction? Direction { get; set; }
        public Service? Service { get; set; }
        public ICollection<Schedule> GeneratedSchedules { get; set; } = new List<Schedule>();
    }
}