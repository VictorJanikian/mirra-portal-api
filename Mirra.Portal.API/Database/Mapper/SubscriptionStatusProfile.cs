using AutoMapper;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Mapper
{
    public class SubscriptionStatusProfile : Profile
    {
        public SubscriptionStatusProfile()
        {
            CreateMap<SubscriptionStatusTableRow, SubscriptionStatus>();
            CreateMap<SubscriptionStatus, SubscriptionStatusTableRow>();
        }
    }
}
