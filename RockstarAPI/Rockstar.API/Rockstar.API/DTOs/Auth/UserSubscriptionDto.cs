namespace Rockstar.API.DTOs.Auth
{
    public class UserSubscriptionDto
    {
        public int PurchaseId { get; set; }
        public int SubscriptionId { get; set; }
        public string SubscriptionName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TotalSessions { get; set; }
        public int SessionsUsed { get; set; }
        public int SessionsRemaining => TotalSessions - SessionsUsed;
        public DateTime PurchaseDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? DirectionId { get; set; }
        public string DirectionName { get; set; } = string.Empty;
    }
}
