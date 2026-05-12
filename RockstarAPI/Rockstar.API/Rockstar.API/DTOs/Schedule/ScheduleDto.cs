public class ScheduleDto
{
    public int Id { get; set; }
    public int? TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int DirectionId { get; set; }
    public string DirectionName { get; set; } = string.Empty;
    public string DirectionKey { get; set; } = string.Empty;
    public int? ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }
    public decimal? Price { get; set; }
    public bool IsGroup { get; set; }
    public bool IsAvailable => CurrentParticipants < MaxParticipants;
    public int AvailableSpots => MaxParticipants - CurrentParticipants;
    public string TimeRange => $"{DateTime:HH:mm} - {DateTime.AddMinutes(DurationMinutes):HH:mm}";
    public string DateDisplay => DateTime.ToString("dd.MM.yyyy");
}