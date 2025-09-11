using AutoMapper;
using Mirra_Portal_API.Enums;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Model.Requests;
using Mirra_Portal_API.Model.Responses;

namespace Mirra_Portal_API.Mapper
{
    public class PlatformConfigurationProfile : Profile
    {
        public PlatformConfigurationProfile()
        {
            CreateMap<PlatformConfigurationRequest, CustomerPlatformConfiguration>()
                .ForMember(entity => entity.ContentPlatform, options => options.MapFrom(request => new ContentPlatform().SetId((int)request.ContentPlatformId)));

            CreateMap<CustomerPlatformConfiguration, ConfigurationResponse>()
                .ForMember(response => response.ContentPlatformId, options => options.MapFrom(entity => (EContentPlatform)entity.ContentPlatform.Id));
        }
    }
}
