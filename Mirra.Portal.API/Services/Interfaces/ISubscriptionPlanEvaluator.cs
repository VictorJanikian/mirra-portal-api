using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface ISubscriptionPlanEvaluator
    {
        public Task<bool> checkIfRunsPerWeekAreAllowedInCustomerCurrentPlan(Customer customer, int runsPerWeek);

        public Task<bool> checkIfNumberOfConfigurationsAreAllowedInCustomerCurrentPlan(Customer customer, int numberOfConfigurations);

        public Task<int?> getRemainingConfigurationsAllowed(Customer customer, int currentNumberOfConfigurations);
    }
}
