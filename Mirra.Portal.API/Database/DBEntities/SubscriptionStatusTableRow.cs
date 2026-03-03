using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("subscription_status")]
    public class SubscriptionStatusTableRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
