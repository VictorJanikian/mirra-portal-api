using Mirra_Portal_API.Enums;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public class SubscriptionPlanEvaluator : ISubscriptionPlanEvaluator
    {
        public bool checkIfRunsPerWeekAreAllowedInCustomerCurrentPlan(Customer customer, int runsPerWeek)
        {
            if (customer.SubscriptionPlan.Id == (int)ESubscriptionPlan.FREE && runsPerWeek > 1)
                return false;

            else if (customer.SubscriptionPlan.Id == (int)ESubscriptionPlan.BASIC && runsPerWeek > 7)
                return false;

            else if (customer.SubscriptionPlan.Id == (int)ESubscriptionPlan.PREMIUM && runsPerWeek > 21)
                return false;

            return true;

        }
    }
}
