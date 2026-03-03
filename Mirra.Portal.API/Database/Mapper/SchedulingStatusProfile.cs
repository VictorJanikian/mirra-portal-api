using AutoMapper;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Mapper
{
    public class SchedulingStatusProfile : Profile
    {
        public SchedulingStatusProfile()
        {
            CreateMap<SchedulingStatusTableRow, SchedulingStatus>();
            CreateMap<SchedulingStatus, SchedulingStatusTableRow>();
        }
    }
}
