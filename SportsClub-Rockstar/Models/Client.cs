using System;

namespace Rockstar.Admin.WPF.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PlainPassword { get; set; }
        public string? PasswordHash { get; set; }
        public string? Phone { get; set; }
        public int? Age { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string Role { get; set; } = "client";

        public string FullName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName))
                    return "Имя не указано";
                return $"{FirstName} {LastName}".Trim();
            }
        }
    }
}