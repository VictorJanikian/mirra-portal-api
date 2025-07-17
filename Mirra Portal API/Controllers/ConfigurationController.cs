using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mirra_Portal_API.Model.Requests;

namespace Mirra_Portal_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Produces("application/json")]
    [Authorize("Bearer")]
    public class ConfigurationController : ControllerBase
    {
        [HttpPost("platforms")]
        public async Task<IActionResult> ConfigurePlatform([FromBody] CustomerContentPlatformsRequest request)
        {

            return Ok();
        }
    }
}
