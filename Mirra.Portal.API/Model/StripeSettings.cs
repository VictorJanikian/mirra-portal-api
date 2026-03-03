namespace Mirra_Portal_API.Model
{
    public class StripeSettings
    {
        public string WebhookSecret { get; set; }
        public Dictionary<long, int> PriceToSubscriptionPlan { get; set; }
    }
}
