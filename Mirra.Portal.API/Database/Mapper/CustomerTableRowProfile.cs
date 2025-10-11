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
