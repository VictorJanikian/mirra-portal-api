using AutoMapper;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Model.Requests;
using Mirra_Portal_API.Model.Responses;

namespace Mirra_Portal_API.Mapper
{
    public class ParametersProfile : Profile
    {
        public ParametersProfile()
        {
            CreateMap<ParametersRequest, Parameters>();
            CreateMap<Parameters, ParametersResponse>();
        }
    }
}
