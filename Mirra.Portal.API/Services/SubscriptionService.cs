using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class SubscriptionService : ISubscriptionService
    {

        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<SubscriptionPlan> GetSubscriptionPlanByPrice(int price)
        {
            return await _subscriptionRepository.GetByPrice(price);
        }

        public async Task<List<SubscriptionPlan>> GetAllSubscriptionPlans()
        {
            return await _subscriptionRepository.GetAll();
        }
    }
}
