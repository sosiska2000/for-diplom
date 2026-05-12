using System.Collections.Generic;

namespace Rockstar.API.Models
{
    public class Direction
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameKey { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>(); // Старая связь
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

        // 👇 НОВОЕ: связь с тренерами через промежуточную таблицу
        public ICollection<TrainerDirection> TrainerDirections { get; set; } = new List<TrainerDirection>();
    }
}