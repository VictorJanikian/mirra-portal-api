namespace Mirra_Portal_API.Model.Responses
{
    public class ErrorResponse
    {
        public string Message { get; set; }

        public ErrorResponse(string message)
        {
            Message = message;
        }
    }
}
