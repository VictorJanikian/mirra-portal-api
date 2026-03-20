using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class SubscriptionPlanEvaluator : ISubscriptionPlanEvaluator
    {
        private ILogger<SubscriptionPlanEvaluator> _logger;
        private ISubscriptionRepository _subscriptionRepository;

        public SubscriptionPlanEvaluator(ILogger<SubscriptionPlanEvaluator> logger, ISubscriptionRepository subscriptionRepository)
        {
            _logger = logger;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<bool> checkIfRunsPerWeekAreAllowedInCustomerCurrentPlan(Customer customer, int runsPerWeek)
        {
            var plan = await _subscriptionRepository.GetById(customer.SubscriptionPlan.Id);

            if (plan == null || plan.MaximumPosts == null)
                return true;

            return runsPerWeek <= plan.MaximumPosts;
        }

        public async Task<bool> checkIfNumberOfConfigurationsAreAllowedInCustomerCurrentPlan(Customer customer, int numberOfConfigurations)
        {
            var plan = await _subscriptionRepository.GetById(customer.SubscriptionPlan.Id);

            if (plan == null || plan.MaximumConfigurations == null)
                return true;

            return numberOfConfigurations <= plan.MaximumConfigurations;
        }

        public async Task<int?> getRemainingConfigurationsAllowed(Customer customer, int currentNumberOfConfigurations)
        {
            var plan = await _subscriptionRepository.GetById(customer.SubscriptionPlan.Id);

            if (plan == null || plan.MaximumConfigurations == null)
                return null;

            return Math.Max(plan.MaximumConfigurations.Value - currentNumberOfConfigurations, 0);
        }

        public async Task<int?> getRemainingRunsPerWeekAllowed(Customer customer, int configurationId, int currentNumberOfSchedulings)
        {
            var plan = await _subscriptionRepository.GetById(customer.SubscriptionPlan.Id);

            if (plan == null || plan.MaximumPosts == null)
                return null;

            return Math.Max(plan.MaximumPosts.Value - currentNumberOfSchedulings, 0);
        }
    }
}
