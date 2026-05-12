namespace Rockstar.API.DTOs.Service
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DirectionId { get; set; }
        public string DirectionName { get; set; } = string.Empty;
        public string DirectionKey { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int SessionsCount { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string PriceDisplay => $"{Price:N0} ₽";
        public string SessionsDisplay => SessionsCount > 1 ? $"{SessionsCount} занятий" : "Разовое";
    }
}