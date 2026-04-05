using System.ComponentModel.DataAnnotations.Schema;

namespace Mirra_Portal_API.Database.DBEntities
{
    [Table("customers")]
    public class CustomerTableRow : EntityTableRow
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<CustomerPlatformConfigurationTableRow> PlatformsConfigurations { get; set; }
        public Boolean IsEmailActivated { get; set; }
        public string? EmailActivationCode { get; set; }
        public int? EmailActivationFailedAttempts { get; set; }
        public SubscriptionPlanTableRow SubscriptionPlan { get; set; }
        public int SubscriptionPlanId { get; set; }
        public SubscriptionStatusTableRow SubscriptionStatus { get; set; }
        public int SubscriptionStatusId { get; set; }
        public string? StripeCustomerId { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public string? PasswordResetCode { get; set; }
        public DateTime? PasswordResetCodeExpiration { get; set; }
        public int? PasswordResetFailedAttempts { get; set; }
        public DateTime? CreatedAt { get; set; }


    }
}
