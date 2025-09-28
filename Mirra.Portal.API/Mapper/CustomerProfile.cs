using AutoMapper;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Model.Requests;
using Mirra_Portal_API.Model.Responses;

namespace Mirra_Portal_API.Mapper
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<RegisterRequest, Customer>();
            CreateMap<Customer, RegisterResponse>();
            CreateMap<Customer, LoginResponse>();
            CreateMap<Customer, ActivateEmailResponse>();

        }
    }
}
