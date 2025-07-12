using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("schedulings")]
    public class SchedulingTableRow : EntityTableRow
    {
        public int CustomerContentTypeConfigurationId { get; set; }
        public CustomerContentTypeConfigurationTableRow CustomerContentTypeConfigurations { get; set; }
        public int ParameterId { get; set; }
        public ParametersTableRow Parameters { get; set; }
        public string Interval { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
