using AutoMapper;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Mapper
{
    public class SubscriptionPaymentLinkTableRowProfile : Profile
    {
        public SubscriptionPaymentLinkTableRowProfile()
        {

            CreateMap<SubscriptionPaymentLinkTableRow, SubscriptionPaymentLink>();
            CreateMap<SubscriptionPaymentLink, SubscriptionPaymentLinkTableRow>();
        }
    }
}
