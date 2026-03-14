using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Model.Responses;
using Mirra_Portal_API.Services.Interfaces;
using Stripe;

namespace Mirra_Portal_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Produces("application/json")]
    public class StripeController : ControllerBase
    {
        private readonly IStripeWebhookService _stripeWebhookService;
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<StripeController> _logger;

        public StripeController(
            IStripeWebhookService stripeWebhookService,
            IOptions<StripeSettings> stripeSettings,
            ILogger<StripeController> logger)
        {
            _stripeWebhookService = stripeWebhookService;
            _stripeSettings = stripeSettings.Value;
            _logger = logger;
        }




        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeSignature = Request.Headers["Stripe-Signature"];
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    _stripeSettings.WebhookSecret
                );

                _logger.LogInformation(
                    "Received Stripe webhook event: {EventType} ({EventId})",
                    stripeEvent.Type, stripeEvent.Id);

                switch (stripeEvent.Type)
                {
                    //case EventTypes.CheckoutSessionCompleted:
                    //    await _stripeWebhookService.HandleCheckoutSessionCompleted(stripeEvent);
                    //    break;

                    case EventTypes.InvoicePaid:
                        await _stripeWebhookService.HandleInvoicePaymentSucceeded(stripeEvent);
                        break;

                    case EventTypes.CustomerSubscriptionUpdated:
                        await _stripeWebhookService.HandleSubscriptionUpdated(stripeEvent);
                        break;

                    case EventTypes.CustomerSubscriptionDeleted:
                        await _stripeWebhookService.HandleSubscriptionDeleted(stripeEvent);
                        break;

                    case EventTypes.InvoicePaymentFailed:
                        await _stripeWebhookService.HandlePaymentFailed(stripeEvent);
                        break;

                    default:
                        _logger.LogInformation(
                            "Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogWarning(
                    "Stripe signature verification failed: {Message}", e.Message);
                return BadRequest(new ErrorResponse("Invalid Stripe webhook signature."));
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



        public async Task<IActionResult> TestStripe()
        {

            var stripeEvent = new Stripe.Event();
            stripeEvent.Data = new EventData();
            stripeEvent.Data.Object = new Stripe.Subscription();
            ((Stripe.Subscription)stripeEvent.Data.Object).CustomerId = "cus_U3fw7e7JZiAtT4";
            ((Stripe.Subscription)stripeEvent.Data.Object).Items = new StripeList<SubscriptionItem>();
            ((Stripe.Subscription)stripeEvent.Data.Object).Items.Data = new List<Stripe.SubscriptionItem>();
            ((Stripe.Subscription)stripeEvent.Data.Object).Items.Data.Add(new Stripe.SubscriptionItem());
            ((Stripe.Subscription)stripeEvent.Data.Object).Items.Data[0].Price = new Stripe.Price();
            ((Stripe.Subscription)stripeEvent.Data.Object).Items.Data[0].Price.UnitAmount = 1400;
            await _stripeWebhookService.HandleSubscriptionUpdated(stripeEvent);
            return Ok();
        }



    }
}
