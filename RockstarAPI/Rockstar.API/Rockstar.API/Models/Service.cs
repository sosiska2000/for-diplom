using System.Collections.Generic;

namespace Rockstar.API.Models
{
    public class Service
    {
        public int Id { get; set; }
        public int DirectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int SessionsCount { get; set; } = 1;
        public int? DurationMinutes { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public Direction Direction { get; set; } = null!;
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}