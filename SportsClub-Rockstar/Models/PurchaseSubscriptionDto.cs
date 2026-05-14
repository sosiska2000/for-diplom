using System;

namespace Rockstar.Admin.WPF.Models
{
    public class PurchaseSubscriptionDto
    {
        public int UserId { get; set; }
        public int SubscriptionId { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}