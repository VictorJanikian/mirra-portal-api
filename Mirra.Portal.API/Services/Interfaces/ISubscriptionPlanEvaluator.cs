using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface ISubscriptionPlanEvaluator
    {
        public Boolean checkIfRunsPerWeekAreAllowedInCustomerCurrentPlan(Customer customer, int runsPerWeek);
    }
}
