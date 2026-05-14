using System;

namespace Rockstar.Admin.WPF.Models
{
    public class SubscriptionPurchaseDto
    {
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        public string SubscriptionName { get; set; } = string.Empty;
        public int? DirectionId { get; set; }
        public string DirectionName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TotalSessions { get; set; }
        public int SessionsUsed { get; set; }
        public int SessionsRemaining { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}