using System;

namespace Rockstar.API.DTOs
{
    public class UserListDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? Phone { get; set; }
        public int? Age { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ActiveEnrollmentsCount { get; set; }
        public int ActiveSubscriptionsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}