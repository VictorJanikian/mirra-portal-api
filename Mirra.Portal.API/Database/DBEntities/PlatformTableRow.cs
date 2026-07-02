using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("platforms")]
    public class PlatformTableRow : EntityTableRow
    {
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
