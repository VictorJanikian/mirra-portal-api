using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories
{
    public class SubscriptionPaymentLinkRepository : DefaultRepository, ISubscriptionPaymentLinkRepository
    {
        public SubscriptionPaymentLinkRepository(DatabaseContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<SubscriptionPaymentLink> GetBySubscriptionPlanIdAndCountry(int subscriptionPlanId, string country)
        {
            var row = await _context
                .SubscriptionPaymentLinks
                .FirstOrDefaultAsync(s => s.Country == country && s.SubscriptionPlanId == subscriptionPlanId);

            if (row == null) return null;

            return _mapper.Map<SubscriptionPaymentLink>(row);

        }

        public async Task<SubscriptionPaymentLink> GetByPriceAndCountry(int price, string country)
        {
            var row = await _context
                .SubscriptionPaymentLinks
                .FirstOrDefaultAsync(s => s.Country == country && s.Price == price);

            if (row == null) return null;

            return _mapper.Map<SubscriptionPaymentLink>(row);

        }
    }
}
