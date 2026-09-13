using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("caption_sizes")]
    public class CaptionSizeTableRow : EntityTableRow
    {
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
