using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("customers")]
    public class CustomerTableRow : EntityTableRow
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<CustomerContentTypeConfigurationTableRow> ContentTypesConfigurations { get; set; }
        public Boolean IsEmailActivated { get; set; }
        public string? EmailActivationCode { get; set; }
        public int? EmailActivationFailedAttempts { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
