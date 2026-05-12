using System.Collections.Generic;

namespace Rockstar.API.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? DirectionId { get; set; }
        public decimal Price { get; set; }
        public int SessionsCount { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public Direction? Direction { get; set; }
        public ICollection<SubscriptionPurchase> Purchases { get; set; } = new List<SubscriptionPurchase>();
    }
}