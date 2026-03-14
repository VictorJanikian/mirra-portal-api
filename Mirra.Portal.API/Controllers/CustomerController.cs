using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Helper;
using Mirra_Portal_API.Model.Responses;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Produces("application/json")]
    [Authorize("Bearer")]
    public class CustomerController : ControllerBase
    {
        ICustomerService _customerService;
        IMapper _mapper;
        IdentityHelper _identityHelper;

        public CustomerController(ICustomerService customerService,
                                  IMapper mapper,
                                  IdentityHelper identityHelper)
        {
            _customerService = customerService;
            _mapper = mapper;
            _identityHelper = identityHelper;
        }

        [HttpGet("subscription")]
        public async Task<IActionResult> GetSubscription()
        {
            try
            {
                var customer = await _customerService.GetCustomerById(_identityHelper.UserId());
                return Ok(_mapper.Map<CustomerResponse>(customer));
            }
            catch (NotFoundException e)
            {
                return NotFound(new ErrorResponse(e.Message));
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponse("Erro interno do servidor: " + e.Message));
            }
        }
    }
}
