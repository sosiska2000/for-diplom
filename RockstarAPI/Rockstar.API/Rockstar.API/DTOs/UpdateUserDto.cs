using System;

namespace Rockstar.API.DTOs
{
    public class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? Age { get; set; }
        public bool IsActive { get; set; } = true;
        public string? NewPassword { get; set; }
        public DateTime? BirthDate { get; set; }  
    }
}