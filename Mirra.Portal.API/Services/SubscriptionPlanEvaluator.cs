using Mirra_Portal_API.Enums;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class SubscriptionPlanEvaluator : ISubscriptionPlanEvaluator
    {


        public bool checkIfRunsPerWeekAreAllowedInCustomerCurrentPlan(Customer customer, int runsPerWeek)
        {
            if (customer.SubscriptionPlan.Id == (int)ESubscriptionPlan.FREE && runsPerWeek > 3)
                return false;

            else if (customer.SubscriptionPlan.Id == (int)ESubscriptionPlan.BASIC && runsPerWeek > 5)
                return false;

            else if (customer.SubscriptionPlan.Id == (int)ESubscriptionPlan.PREMIUM && runsPerWeek > 10)
                return false;

            return true;

        }

        public bool checkIfNumberOfConfigurationsAreAllowedInCustomerCurrentPlan(Customer customer, int numberOfConfigurations)
        {
            if (customer.SubscriptionPlan.Id == (int)ESubscriptionPlan.FREE && numberOfConfigurations > 1)
                return false;

            else if (customer.SubscriptionPlan.Id == (int)ESubscriptionPlan.BASIC && numberOfConfigurations > 2)
                return false;

            else if (customer.SubscriptionPlan.Id == (int)ESubscriptionPlan.PREMIUM && numberOfConfigurations > 5)
                return false;

            return true;
        }
    }
}
