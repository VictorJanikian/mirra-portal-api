using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("subscription_plans")]
    public class SubscriptionPlanTableRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MaximumPosts { get; set; }
        public int? MaximumConfigurations { get; set; }
        public int Price { get; set; }
        public string? PaymentLink { get; set; }
    }
}
