using System;

namespace Rockstar.API.DTOs
{
    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public int? Age { get; set; }
        public string? PhotoBase64 { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}