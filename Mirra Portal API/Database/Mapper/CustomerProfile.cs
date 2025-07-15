using AutoMapper;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Mapper
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerTableRow>()
               .ForMember(row => row.CreatedAt, options => options.Ignore())

                .ForMember(row => row.ContentTypesConfigurations,
                    options =>
                    {
                        options.Condition(entity => entity.ContentTypesConfigurations != null);
                        options.MapFrom(entity => entity.ContentTypesConfigurations);
                    })

               .AfterMap((entity, row) => row.CreatedAt = row.CreatedAt ?? DateTime.Now);

            CreateMap<CustomerTableRow, Customer>()
                .ForMember(entity => entity.ContentTypesConfigurations, options => options.Ignore());
        }
    }
}
