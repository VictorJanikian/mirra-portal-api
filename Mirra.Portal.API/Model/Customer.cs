namespace Mirra_Portal_API.Model
{
    public class Customer : Entity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Boolean IsEmailActivated { get; set; }
        public string? EmailActivationCode { get; set; }
        public int? EmailActivationFailedAttempts { get; set; }
        public List<CustomerPlatformConfiguration> PlatformsConfigurations { get; set; }
    }
}
