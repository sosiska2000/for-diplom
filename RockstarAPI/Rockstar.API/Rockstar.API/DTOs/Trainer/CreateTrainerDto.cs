public class CreateTrainerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Старое поле (для обратной совместимости)
    public int? DirectionId { get; set; }

    // 👇 НОВОЕ: список ID направлений
    public List<int>? DirectionIds { get; set; }

    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PhotoBase64 { get; set; }
    public int Experience { get; set; }
    public string? Description { get; set; }
}