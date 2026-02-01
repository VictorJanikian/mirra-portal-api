using AutoMapper;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Mapper
{
    public class SubscriptionPlanProfile : Profile
    {
        public SubscriptionPlanProfile()
        {
            CreateMap<SubscriptionPlanTableRow, SubscriptionPlan>();
            CreateMap<SubscriptionPlan, SubscriptionPlanTableRow>();
        }
    }
}
