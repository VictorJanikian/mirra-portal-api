using Stripe;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface IStripeWebhookService
    {
        Task HandleCheckoutSessionCompleted(Event stripeEvent);
        Task HandleInvoicePaymentSucceeded(Event stripeEvent);
        Task HandleSubscriptionUpdated(Event stripeEvent);
        Task HandleSubscriptionDeleted(Event stripeEvent);
    }
}
