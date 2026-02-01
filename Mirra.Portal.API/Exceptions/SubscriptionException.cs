namespace Mirra_Portal_API.Exceptions
{
    public class SubscriptionException : BadRequestException
    {
        public SubscriptionException(string message) : base(message)
        {
        }
    }
}
