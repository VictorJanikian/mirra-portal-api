using AutoMapper;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Model.Requests;
using Mirra_Portal_API.Model.Responses;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Mapper
{
    public class ConvertedIntervalResolver : IValueResolver<Scheduling, SchedulingResponse, string>
    {
        private readonly ICronService _cronService;

        public ConvertedIntervalResolver(ICronService cronService)
        {
            _cronService = cronService;
        }

        public string Resolve(Scheduling source, SchedulingResponse destination,
                              string destMember, ResolutionContext context)
        {
            return _cronService.ConvertCronToLocal(source.Interval, source.Timezone);
        }
    }

    public class SchedulingProfile : Profile
    {
        public SchedulingProfile()
        {
            CreateMap<SchedulingRequest, Scheduling>();
            CreateMap<Scheduling, SchedulingResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.SchedulingStatus != null ? src.SchedulingStatus.Id : (int?)null))
                .ForMember(dest => dest.ConvertedInterval, opt => opt.MapFrom<ConvertedIntervalResolver>());
        }
    }
}
