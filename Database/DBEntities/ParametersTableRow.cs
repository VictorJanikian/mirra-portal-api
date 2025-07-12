using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("parameters")]
    public class ParametersTableRow : EntityTableRow
    {
        public string ThemeTitle { get; set; }
        public string? Keywords { get; set; }
        public string? TargetAudience { get; set; }
        public string? Style { get; set; }
        public string? Goal { get; set; }
        public string? ApproximatedSize { get; set; }
        public string? AdditionalInfo { get; set; }
        public string? Categories { get; set; }
        public string? Tags { get; set; }
        public string? SEOAdditionalInformation { get; set; }
        public bool? IsDraft { get; set; }
        public string Language { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
