using AutoMapper;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Mapper
{
    public class CustomerContentTypeConfigurationProfile : Profile
    {
        public CustomerContentTypeConfigurationProfile()
        {
            CreateMap<CustomerContentTypeConfiguration, CustomerContentTypeConfigurationTableRow>()
                .ForMember(row => row.Customer, options => options.Ignore())
                .ForMember(row => row.ContentType, options => options.Ignore())
                .ForMember(row => row.ContentTypeId, options => options.MapFrom(entity => entity.ContentType.Id))
                .ForMember(row => row.CustomerId, options => options.MapFrom(entity => entity.Customer.Id));

            CreateMap<CustomerContentTypeConfigurationTableRow, CustomerContentTypeConfiguration>();
        }
    }
}
