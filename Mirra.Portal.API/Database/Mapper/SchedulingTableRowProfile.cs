using AutoMapper;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Mapper
{
    public class SchedulingTableRowProfile : Profile
    {
        public SchedulingTableRowProfile()
        {
            CreateMap<Scheduling, SchedulingTableRow>()
                 .ForMember(row => row.Parameters, options => options.Ignore())
                 .ForMember(row => row.CustomerContentPlatformConfiguration, options => options.Ignore())
                 .ForMember(row => row.ParametersId, options => options.Ignore())
                 .ForMember(row => row.CustomerContentPlatformConfigurationId, options => options.Ignore())
                 .ForMember(row => row.CreatedAt, options => options.Ignore())
                 .AfterMap((entity, row) => row.CreatedAt = row.CreatedAt ?? DateTime.Now);

            CreateMap<SchedulingTableRow, Scheduling>();
        }
    }
}
