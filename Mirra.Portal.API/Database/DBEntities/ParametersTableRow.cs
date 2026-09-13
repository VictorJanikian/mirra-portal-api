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
        public string? Description { get; set; }
        public string? SearchIntent { get; set; }
        public bool? IsDraft { get; set; }
        public string Language { get; set; }
        public string? CTA { get; set; }
        public string? VisualHookInstructions { get; set; }
        public string? ColorPalette { get; set; }
        public string? VisualLayout { get; set; }
        public string? TextOnImage { get; set; }
        public int? CaptionSizeId { get; set; }
        public CaptionSizeTableRow? CaptionSize { get; set; }
        public string? HashtagsStrategy { get; set; }
        public string? AvoidTopics { get; set; }
        public string? ContentTone { get; set; }
        public string? CaptionInstructions { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
