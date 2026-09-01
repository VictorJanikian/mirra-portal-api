using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("instagram_permissions")]
    public class InstagramPermissionTableRow : EntityTableRow
    {
        public string Permission { get; set; }
        public int CustomerPlatformConfigurationId { get; set; }
        public CustomerPlatformConfigurationTableRow CustomerPlatformConfiguration { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
