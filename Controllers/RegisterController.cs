using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Model.Requests;
using Mirra_Portal_API.Model.Responses;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Produces("application/json")]
    public class RegisterController : ControllerBase
    {
        IMapper _mapper;
        ICustomerService _customerService;
        IEmailService _emailService;

        public RegisterController(IMapper mapper, ICustomerService customerService,
                                  IEmailService emailService)
        {
            _mapper = mapper;
            _customerService = customerService;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCustomer([FromBody] Model.Requests.RegisterRequest request)
        {
            try
            {
                var customer = _mapper.Map<Customer>(request);
                customer = await _customerService.RegisterCustomer(customer);
                return Ok(_mapper.Map<RegisterResponse>(customer));
            }
            catch (BadRequestException e)
            {
                return BadRequest(new ErrorResponse(e.Message));
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponse("Erro interno do servidor: " + e.Message));
            }
        }

        [HttpPost("activate")]
        public async Task<IActionResult> ActivateCustomerEmail([FromBody] ActivateEmailRequest request)
        {
            try
            {
                await _emailService.ActivateEmail(request.Email, request.Code);
                return Ok();
            }
            catch (BadRequestException e)
            {
                return BadRequest(new ErrorResponse(e.Message));
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
