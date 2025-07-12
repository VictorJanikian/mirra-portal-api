namespace Mirra_Portal_API.Model
{
    public class ContentType : Entity
    {
        public string Name { get; set; }
        public string Prompt { get; set; }
        public string SummaryPrompt { get; set; }
        public string SystemPrompt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
