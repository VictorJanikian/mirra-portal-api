using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("scheduling_status")]
    public class SchedulingStatusTableRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
