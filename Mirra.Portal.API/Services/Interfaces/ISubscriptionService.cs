using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface ISubscriptionService
    {
        public Task<SubscriptionPlan> GetSubscriptionPlanByPrice(int price);
        public Task<List<SubscriptionPlan>> GetAllSubscriptionPlans();
        public Task<int> GetRemainingConfigurationsAllowed(int customerId);


    }
}
