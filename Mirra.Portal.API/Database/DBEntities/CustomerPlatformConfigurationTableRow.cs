using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("customer_platforms_configurations")]
    public class CustomerPlatformConfigurationTableRow : EntityTableRow
    {
        public int CustomerId { get; set; }
        public CustomerTableRow Customer { get; set; }
        public int PlatformId { get; set; }
        public string PlatformName { get; set; }
        public PlatformTableRow Platform { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<SchedulingTableRow> Schedulings { get; set; }
    }
}
