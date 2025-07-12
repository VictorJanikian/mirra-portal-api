namespace Mirra_Portal_API.Model
{
    public class Customer : Entity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Boolean IsEmailConfirmed { get; set; }

        public List<CustomerContentTypeConfiguration> ContentTypesConfigurations { get; set; }
    }
}
