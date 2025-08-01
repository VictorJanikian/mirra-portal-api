﻿using AutoMapper;
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

        [HttpPost("platforms")]
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
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponse("Erro interno do servidor: " + e.Message));
            }
        }
    }
}
