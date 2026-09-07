namespace Mirra_Portal_API.Model
{
    public class CustomerPlatformConfiguration : Entity
    {
        public Customer Customer { get; set; }
        public Platform Platform { get; set; }
        public string PlatformName { get; set; }
        public string? Url { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int RemainingRunsPerWeek { get; set; }
        public string? InstagramState { get; set; }
        public long? InstagramUserId { get; set; }
        public string? InstagramAccessToken { get; set; }
        public DateTime? InstagramTokenExpiresAt { get; set; }
        public string? InstagramUsername { get; set; }
        public bool IsConfirmed { get; set; }
        public List<Scheduling> Schedulings { get; set; }
    }
}
