namespace Rockstar.API.DTOs.Trainer
{
    public class TrainerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";

        // Старое поле (первое направление)
        public int? DirectionId { get; set; }
        public string DirectionName { get; set; } = string.Empty;
        public string DirectionKey { get; set; } = string.Empty;

        // 👇 НОВОЕ: список всех направлений
        public List<DirectionDto> Directions { get; set; } = new();

        public string? Email { get; set; }
        public string? PhotoBase64 { get; set; }
        public int Experience { get; set; }
        public string ExperienceDisplay => $"Стаж: {Experience} лет";
        public string? Description { get; set; }
    }

    public class DirectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameKey { get; set; } = string.Empty;
    }
}