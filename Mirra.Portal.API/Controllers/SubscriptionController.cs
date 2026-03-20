using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Helper;
using Mirra_Portal_API.Model.Responses;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Produces("application/json")]
    [Authorize("Bearer")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISubscriptionPlanEvaluator _subscriptionPlanEvaluator;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerPlatformConfigurationRepository _configurationRepository;
        private readonly IdentityHelper _identityHelper;

        public SubscriptionController(ISubscriptionService subscriptionService,
                                      ISubscriptionPlanEvaluator subscriptionPlanEvaluator,
                                      ICustomerRepository customerRepository,
                                      ICustomerPlatformConfigurationRepository configurationRepository,
                                      IdentityHelper identityHelper)
        {
            _subscriptionService = subscriptionService;
            _subscriptionPlanEvaluator = subscriptionPlanEvaluator;
            _customerRepository = customerRepository;
            _configurationRepository = configurationRepository;
            _identityHelper = identityHelper;
        }

        [HttpGet("plans")]
        public async Task<IActionResult> GetAllPlans()
        {
            try
            {
                var plans = await _subscriptionService.GetAllSubscriptionPlans();
                return Ok(plans);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponse("Erro interno do servidor: " + e.Message));
            }
        }

        [HttpGet("remaining-configurations")]
        public async Task<IActionResult> GetRemainingConfigurations()
        {
            try
            {
                var remaining = await _subscriptionService.GetRemainingConfigurationsAllowed(_identityHelper.UserId());
                return Ok(new { remainingConfigurations = remaining });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponse("Erro interno do servidor: " + e.Message));
            }
        }
    }
}
