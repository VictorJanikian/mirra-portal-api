using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Model.Responses;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Produces("application/json")]
    [Authorize("Bearer")]
    public class SchedulingController : ControllerBase
    {

        IConfigurationService _configurationService;
        IMapper _mapper;
        public SchedulingController(IConfigurationService configurationService,
                                       IMapper mapper)
        {
            _configurationService = configurationService;
            _mapper = mapper;
        }

        [HttpGet("has-suspended")]
        public async Task<IActionResult> HasSuspendedSchedulingsDueToLackPayment()
        {
            try
            {
                var hasInactive = await _configurationService.HasSuspendedSchedulingsDueToLackOfPayment();
                return Ok(hasInactive);
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
