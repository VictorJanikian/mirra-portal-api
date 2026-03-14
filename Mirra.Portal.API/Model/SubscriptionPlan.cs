namespace Mirra_Portal_API.Model
{
    public class SubscriptionPlan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MaximumPosts { get; set; }
        public int? MaximumConfigurations { get; set; }
        public int Price { get; set; } = 0;
        public string? PaymentLink { get; set; }

    }
}
