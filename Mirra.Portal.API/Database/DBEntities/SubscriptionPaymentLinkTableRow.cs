using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("subscription_payment_links")]

    public class SubscriptionPaymentLinkTableRow
    {
        public int Id { get; set; }
        public SubscriptionPlanTableRow SubscriptionPlan { get; set; }
        public int SubscriptionPlanId { get; set; }
        public string Country { get; set; }
        public string PaymentLink { get; set; }
        public int Price { get; set; }
    }
}
