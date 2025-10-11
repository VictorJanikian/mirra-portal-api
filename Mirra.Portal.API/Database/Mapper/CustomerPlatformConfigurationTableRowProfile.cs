using AutoMapper;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Mapper
{
    public class CustomerPlatformConfigurationTableRowProfile : Profile
    {
        public CustomerPlatformConfigurationTableRowProfile()
        {
            CreateMap<CustomerPlatformConfiguration, CustomerPlatformConfigurationTableRow>()
                .ForMember(row => row.Customer, options => options.Ignore())
                .ForMember(row => row.Platform, options => options.Ignore())

                .ForMember(row => row.PlatformId, options => options.MapFrom(entity => entity.Platform.Id))
                .ForMember(row => row.CustomerId, options => options.MapFrom(entity => entity.Customer.Id));

            CreateMap<CustomerPlatformConfigurationTableRow, CustomerPlatformConfiguration>();
        }
    }
}
