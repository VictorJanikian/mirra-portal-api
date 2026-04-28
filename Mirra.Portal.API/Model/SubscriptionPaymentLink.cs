namespace Mirra_Portal_API.Model
{
    public class SubscriptionPaymentLink
    {
        public int Id { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public string Country { get; set; }
        public string PaymentLink { get; set; }
    }
}
