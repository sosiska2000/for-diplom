using System;

namespace Rockstar.API.DTOs.Subscription
{
    public class PurchaseSubscriptionDto
    {
        public int UserId { get; set; }
        public int SubscriptionId { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}