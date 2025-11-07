using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize("Bearer")]
    public class ConfigurationController : ControllerBase
    {
        IConfigurationService _configurationService;
        IMapper _mapper;
        public ConfigurationController(IConfigurationService configurationService,
                                       IMapper mapper)
        {
            _configurationService = configurationService;
            _mapper = mapper;
        }

        [HttpPost("{configurationId}/schedulings/")]
        public async Task<IActionResult> CreateScheduling([FromRoute] int configurationId, [FromBody] SchedulingRequest request)
        {
            try
            {
                var scheduling = _mapper.Map<Scheduling>(request);
                return Ok(_mapper.Map<SchedulingResponse>(await _configurationService.CreateScheduling(configurationId, scheduling)));
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

        [HttpPost()]
        public async Task<IActionResult> ConfigurePlatform([FromBody] PlatformConfigurationRequest request)
        {
            try
            {
                var platformConfiguration = _mapper.Map<CustomerPlatformConfiguration>(request);
                return Ok(_mapper.Map<ConfigurationResponse>(await _configurationService.CreateConfiguration(platformConfiguration)));
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

        [HttpGet("{configurationId}/schedulings")]
        public async Task<IActionResult> RecoverConfigurationSchedulings([FromRoute] int configurationId)
        {
            try
            {
                var schedulings = await _configurationService.GetConfigurationSchedulings(configurationId);
                return Ok(_mapper.Map<List<SchedulingResponse>>(schedulings));
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

        [HttpGet("{configurationId}/schedulings/{schedulingId}")]
        public async Task<IActionResult> RecoverScheduling([FromRoute] int configurationId, [FromRoute] int schedulingId)
        {
            try
            {
                var scheduling = await _configurationService.GetScheduling(configurationId, schedulingId);
                return Ok(_mapper.Map<SchedulingResponse>(scheduling));
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

        [HttpPut("{configurationId}/schedulings/{schedulingId}")]
        public async Task<IActionResult> UpdateScheduling([FromRoute] int configurationId, [FromRoute] int schedulingId, [FromBody] SchedulingRequest schedulingRequest)
        {
            try
            {
                var scheduling = _mapper.Map<Scheduling>(schedulingRequest);
                var updatedScheduling = await _configurationService.UpdateScheduling(configurationId, schedulingId, scheduling);
                return Ok(_mapper.Map<SchedulingResponse>(updatedScheduling));
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

        [HttpDelete("{configurationId}/schedulings/{schedulingId}")]
        public async Task<IActionResult> RemoveScheduling([FromRoute] int configurationId, [FromRoute] int schedulingId)
        {
            try
            {
                await _configurationService.DeleteScheduling(configurationId, schedulingId);
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
