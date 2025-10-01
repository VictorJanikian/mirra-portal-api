namespace Mirra_Portal_API.Model
{
    public record class Parameters
    {
        public int Id { get; set; }
        public string ThemeTitle { get; set; }
        public string Keywords { get; set; }
        public string TargetAudience { get; set; }
        public string Style { get; set; }
        public string Goal { get; set; }
        public string ApproximatedSize { get; set; }
        public string AdditionalInfo { get; set; }
        public string Categories { get; set; }
        public string Tags { get; set; }
        public string SEOAdditionalInformation { get; set; }
        public bool? IsDraft { get; set; }
        public string Language { get; set; }
    }

}
