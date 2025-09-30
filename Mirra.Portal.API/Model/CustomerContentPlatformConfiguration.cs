namespace Mirra_Portal_API.Model
{
    public class CustomerContentPlatformConfiguration : Entity
    {
        public Customer Customer { get; set; }
        public ContentPlatform ContentPlatform { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<Scheduling> Schedulings { get; set; }
    }
}
