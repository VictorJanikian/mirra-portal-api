using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class SubscriptionService : ISubscriptionService
    {

        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerPlatformConfigurationRepository _configurationRepository;
        private readonly ISubscriptionPlanEvaluator _subscriptionPlanEvaluator;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository,
            ISubscriptionPlanEvaluator subscriptionPlanEvaluator,
            ICustomerRepository customerRepository,
            ICustomerPlatformConfigurationRepository configurationRepository)
        {
            _subscriptionRepository = subscriptionRepository;
            _subscriptionPlanEvaluator = subscriptionPlanEvaluator;
            _configurationRepository = configurationRepository;
            _customerRepository = customerRepository;
        }

        public async Task<SubscriptionPlan> GetSubscriptionPlanByPrice(int price)
        {
            return await _subscriptionRepository.GetByPrice(price);
        }

        public async Task<List<SubscriptionPlan>> GetAllSubscriptionPlans()
        {
            return await _subscriptionRepository.GetAll();
        }

        public async Task<int> GetRemainingConfigurationsAllowed(int customerId)
        {
            var customer = await _customerRepository.GetById(customerId);
            var customerConfigurations = await _configurationRepository.GetAllForCustomer(customerId);
            var reamining = await _subscriptionPlanEvaluator.getRemainingConfigurationsAllowed(customer, customerConfigurations.Count);
            return reamining ?? -1;

        }
    }
}
