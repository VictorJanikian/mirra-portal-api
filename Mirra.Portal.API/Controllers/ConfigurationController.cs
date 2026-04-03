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
        IScheduleService _scheduleService;
        IMapper _mapper;

        public ConfigurationController(IConfigurationService configurationService,
                                       IMapper mapper,
                                       IScheduleService scheduleService)
        {
            _configurationService = configurationService;
            _mapper = mapper;
            _scheduleService = scheduleService;
        }

        [HttpPost("{configurationId}/schedulings/")]
        public async Task<IActionResult> CreateScheduling([FromRoute] int configurationId, [FromBody] SchedulingRequest request)
        {
            try
            {
                var scheduling = _mapper.Map<Scheduling>(request);
                return Ok(_mapper.Map<SchedulingResponse>(await _scheduleService.CreateSchedule(configurationId, scheduling)));
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

        [HttpGet("{configurationId}")]
        public async Task<IActionResult> RecoverConfiguration([FromRoute] int configurationId)
        {
            try
            {
                var configuration = await _configurationService.GetConfiguration(configurationId);
                return Ok(_mapper.Map<ConfigurationResponse
                    >(configuration));
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

        [HttpGet()]
        public async Task<IActionResult> RecoverAllConfigurations()
        {
            try
            {
                var configurations = await _configurationService.GetAllConfigurations();

                return Ok(_mapper.Map<List<ConfigurationResponse>>(configurations));
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
                var scheduling = await _scheduleService.GetSchedule(configurationId, schedulingId);
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
                var updatedScheduling = await _scheduleService.UpdateSchedule(configurationId, schedulingId, scheduling);
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
                await _scheduleService.DeleteSchedule(configurationId, schedulingId);
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


        [HttpDelete("{configurationId}")]
        public async Task<IActionResult> DeleteConfiguration([FromRoute] int configurationId)
        {
            try
            {
                await _configurationService.DeleteConfiguration(configurationId);
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

        [HttpPut("{configurationId}")]
        public async Task<IActionResult> EditConfiguration([FromRoute] int configurationId, [FromBody] EditConfigurationRequest request)
        {
            try
            {
                var configuration = _mapper.Map<CustomerPlatformConfiguration>(request);
                var updated = await _configurationService.UpdateConfiguration(configurationId, configuration);
                return Ok(_mapper.Map<ConfigurationResponse>(updated));
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

        [HttpGet("{configurationId}/has-suspended-nopayment")]
        public async Task<IActionResult> HasSuspendedSchedulingsDueToLackPaymentAtThisConfiguration([FromRoute] int configurationId)
        {
            try
            {
                var hasInactive = await _configurationService.HasSuspendedSchedulingsDueToLackOfPayment(configurationId);
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

        [HttpGet("{configurationId}/has-suspended-downgrade")]
        public async Task<IActionResult> HasSuspendedSchedulingsDueToPlanDowngradeAtThisConfiguration([FromRoute] int configurationId)
        {
            try
            {
                var hasInactive = await _configurationService.HasSuspendedSchedulingsDueToPlanDowngrade(configurationId);
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
