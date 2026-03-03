using AutoMapper;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Mapper
{
    public class CustomerTableRowProfile : Profile
    {
        public CustomerTableRowProfile()
        {
            CreateMap<Customer, CustomerTableRow>()
               .ForMember(row => row.CreatedAt, options => options.Ignore())
               .ForMember(row => row.SubscriptionPlan, options => options.Ignore())
               .ForMember(row => row.SubscriptionStatus, options => options.Ignore())
               .ForMember(row => row.SubscriptionPlanId,
                    options =>
                    {
                        options.Condition(entity => entity.SubscriptionPlan != null);
                        options.MapFrom(entity => entity.SubscriptionPlan.Id);
                    })
               .ForMember(row => row.SubscriptionStatusId,
                    options =>
                    {
                        options.Condition(entity => entity.SubscriptionStatus != null);
                        options.MapFrom(entity => entity.SubscriptionStatus.Id);
                    })
                .ForMember(row => row.PlatformsConfigurations,
                    options =>
                    {
                        options.Condition(entity => entity.PlatformsConfigurations != null);
                        options.MapFrom(entity => entity.PlatformsConfigurations);
                    })

               .AfterMap((entity, row) => row.CreatedAt = row.CreatedAt ?? DateTime.Now);

            CreateMap<CustomerTableRow, Customer>()
                .ForMember(entity => entity.PlatformsConfigurations, options => options.Ignore());
        }
    }
}
