﻿using AutoMapper;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Mapper
{
    public class CustomerContentPlatformConfigurationProfile : Profile
    {
        public CustomerContentPlatformConfigurationProfile()
        {
            CreateMap<CustomerContentPlatformsConfiguration, CustomerContentPlatformConfigurationTableRow>()
                .ForMember(row => row.Customer, options => options.Ignore())
                .ForMember(row => row.ContentPlatform, options => options.Ignore())
                .ForMember(row => row.ContentPlatformId, options => options.MapFrom(entity => entity.ContentPlatform.Id))
                .ForMember(row => row.CustomerId, options => options.MapFrom(entity => entity.Customer.Id));

            CreateMap<CustomerContentPlatformConfigurationTableRow, CustomerContentPlatformsConfiguration>();
        }
    }
}
