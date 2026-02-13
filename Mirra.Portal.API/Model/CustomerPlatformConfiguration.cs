namespace Mirra_Portal_API.Model
{
    public class CustomerPlatformConfiguration : Entity
    {
        public Customer Customer { get; set; }
        public Platform Platform { get; set; }
        public string PlatformName { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<Scheduling> Schedulings { get; set; }
    }
}
