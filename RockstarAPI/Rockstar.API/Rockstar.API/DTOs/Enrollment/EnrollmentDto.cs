namespace Rockstar.API.DTOs.Enrollment
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int ScheduleId { get; set; }
        public DateTime EnrolledAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDisplay => Status switch
        {
            "enrolled" => "Записан",
            "attended" => "Посетил",
            "cancelled" => "Отменен",
            "no_show" => "Не явился",
            _ => Status
        };
        public object? ScheduleInfo { get; set; }
    }
}