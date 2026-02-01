using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("subscription_plans")]
    public class SubscriptionPlanTableRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
