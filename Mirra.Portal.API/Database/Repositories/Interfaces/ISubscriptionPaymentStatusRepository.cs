using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories.Interfaces
{
    public interface ISubscriptionPaymentLinkRepository
    {
        public Task<SubscriptionPaymentLink> GetBySubscriptionPlanIdAndCountry(int subscriptionPlanId, string country);
    }
}
