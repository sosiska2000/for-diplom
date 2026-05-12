// DTOs/Service/CreateServiceDto.cs
public class CreateServiceDto
{
    public int DirectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int SessionsCount { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Description { get; set; }
}