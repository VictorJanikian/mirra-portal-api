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
                .ForMember(entity => entity.Platform, options => options.MapFrom(request => new Platform().SetId((int)request.PlatformId)));

            CreateMap<CustomerPlatformConfiguration, ConfigurationResponse>()
                .ForMember(response => response.PlatformId, options => options.MapFrom(entity => (EPlatform)entity.Platform.Id));

        }
    }
}
