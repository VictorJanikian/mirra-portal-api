using AutoMapper;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Mapper
{
    public class PlatformTableRowProfile : Profile
    {
        public PlatformTableRowProfile()
        {
            CreateMap<ContentPlatformTableRow, ContentPlatform>();
            CreateMap<ContentPlatform, ContentPlatformTableRow>();
        }
    }
}
