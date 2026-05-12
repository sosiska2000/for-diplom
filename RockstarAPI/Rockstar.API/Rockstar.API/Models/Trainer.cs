using System.Collections.Generic;

namespace Rockstar.API.Models
{
    public class Trainer
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Старое поле (оставляем для обратной совместимости)
        public int? DirectionId { get; set; }

        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public byte[]? Photo { get; set; }
        public int Experience { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public Direction? Direction { get; set; }
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

        // 👇 НОВОЕ: несколько направлений
        public ICollection<TrainerDirection> TrainerDirections { get; set; } = new List<TrainerDirection>();
    }
}