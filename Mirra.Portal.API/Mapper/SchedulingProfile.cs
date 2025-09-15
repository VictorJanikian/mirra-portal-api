using AutoMapper;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Model.Requests;
using Mirra_Portal_API.Model.Responses;

namespace Mirra_Portal_API.Mapper
{
    public class SchedulingProfile : Profile
    {
        public SchedulingProfile()
        {
            CreateMap<SchedulingRequest, Scheduling>();
            CreateMap<Scheduling, SchedulingResponse>();
        }
    }
}
