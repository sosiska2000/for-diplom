using System;
using System.Collections.Generic;

namespace Rockstar.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int? Age { get; set; }
        public string Role { get; set; } = "client";
        public bool IsActive { get; set; } = true;
        public DateTime? BirthDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<SubscriptionPurchase> Purchases { get; set; } = new List<SubscriptionPurchase>();
    }
}