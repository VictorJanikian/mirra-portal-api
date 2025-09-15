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
    public class AccountController : ControllerBase
    {
        IMapper _mapper;
        ICustomerService _customerService;
        IEmailService _emailService;
        ILoginService _loginService;

        public AccountController(IMapper mapper, ICustomerService customerService,
                                  IEmailService emailService,
                                  ILoginService loginService)
        {
            _mapper = mapper;
            _customerService = customerService;
            _emailService = emailService;
            _loginService = loginService;
        }

        [HttpPost("register")]
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var loginResult = await _loginService.Login(request.Email, request.Password);
                var token = loginResult.token;
                var customer = loginResult.customer;
                var loginResponse = _mapper.Map<LoginResponse>(customer);
                loginResponse.Token = token;
                return Ok(loginResponse);
            }
            catch (BadRequestException e)
            {
                return BadRequest(new ErrorResponse(e.Message));
            }
            catch (NotFoundException e)
            {
                return NotFound(new ErrorResponse(e.Message));
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(new ErrorResponse(e.Message));
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponse("Erro interno do servidor: " + e.Message));
            }
        }
    }
}
