using System;

namespace Rockstar.API.Models
{
    public class SubscriptionPurchase
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SubscriptionId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int SessionsUsed { get; set; }
        public string Status { get; set; } = "active";

        // Навигационные свойства
        public User? User { get; set; }
        public Subscription? Subscription { get; set; }
    }
}