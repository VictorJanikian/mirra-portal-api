using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories
{
    public class SubscriptionPlanRepository : DefaultRepository, ISubscriptionPlanRepository
    {
        public SubscriptionPlanRepository(DatabaseContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<SubscriptionPlan> GetById(int id)
        {
            var row = await _context.SubscriptionPlans
                .AsNoTracking()
                .Where(subscriptionPlan => subscriptionPlan.Id == id)
                .FirstOrDefaultAsync();

            if (row != null)
                return _mapper.Map<SubscriptionPlan>(row);

            else
                return null;
        }

        public async Task<SubscriptionPlan> GetByPrice(int price)
        {
            var row = await _context.SubscriptionPlans
                .AsNoTracking()
                .Where(subscriptionPlan => subscriptionPlan.DefaultPrice == price)
                .FirstOrDefaultAsync();

            if (row != null)
                return _mapper.Map<SubscriptionPlan>(row);

            else
                return null;
        }

        public async Task<List<SubscriptionPlan>> GetAll()
        {
            var rows = await _context.SubscriptionPlans
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<SubscriptionPlan>>(rows);
        }
    }
}
