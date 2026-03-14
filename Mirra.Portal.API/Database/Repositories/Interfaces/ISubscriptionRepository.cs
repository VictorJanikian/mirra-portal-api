using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<SubscriptionPlan> GetByPrice(int price);
        Task<List<SubscriptionPlan>> GetAll();
    }
}
