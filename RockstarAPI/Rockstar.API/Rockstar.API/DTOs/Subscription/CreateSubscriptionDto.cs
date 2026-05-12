namespace Rockstar.API.DTOs.Subscription
{
    public class CreateSubscriptionDto
    {
        public string Name { get; set; } = string.Empty;
        public int DirectionId { get; set; }
        public decimal Price { get; set; }
        public int SessionsCount { get; set; }
        public string? Description { get; set; }
    }
}