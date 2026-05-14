// Rockstar.Admin.WPF/Services/DTOs/ClientUpdateDto.cs
using System.Text.Json.Serialization;
using System;

namespace Rockstar.Admin.WPF.Services.DTOs
{
    public class ClientUpdateDto
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("age")]
        public int? Age { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("birthDate")]  
        public DateTime? BirthDate { get; set; }
    }
}