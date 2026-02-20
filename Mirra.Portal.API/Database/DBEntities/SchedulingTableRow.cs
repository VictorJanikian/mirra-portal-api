using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("schedulings")]
    public class SchedulingTableRow : EntityTableRow
    {
        public int CustomerPlatformConfigurationId { get; set; }
        public CustomerPlatformConfigurationTableRow CustomerPlatformConfiguration { get; set; }
        public int ParametersId { get; set; }
        public ParametersTableRow Parameters { get; set; }
        public string Interval { get; set; }
        public int RunsPerWeek { get; set; }
        public SchedulingStatusTableRow SchedulingStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
