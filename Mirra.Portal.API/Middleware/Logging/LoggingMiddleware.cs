using System.Text;

namespace Mirra_Portal_API.Middleware.Logging
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {

            logEndpoint(context);

            await logRequest(context);

            var originalStream = clientStream(context);

            overwriteContextStreamToAllowReading(context);

            await _next(context);

            await logResponse(context);

            await copyResponseToOriginalStream(context, originalStream);
        }

        private void logEndpoint(HttpContext context)
        {
            _logger.LogInformation($"Request Endpoint: {context.Request.Method} {context.Request.Path} {context.Request.QueryString}");
        }

        private async Task logRequest(HttpContext context)
        {
            context.Request.EnableBuffering();
            var requestBody = await ReadStreamAsync(context.Request.Body);
            if (!string.IsNullOrEmpty(requestBody))
                _logger.LogInformation($"Request Body: {requestBody}");
        }

        private Stream clientStream(HttpContext context)
        {
            return context.Response.Body;
        }

        private void overwriteContextStreamToAllowReading(HttpContext context)
        {
            context.Response.Body = new MemoryStream();
        }

        private async Task logResponse(HttpContext context)
        {
            var responseBody = await ReadStreamAsync(context.Response.Body);

            if (!string.IsNullOrEmpty(responseBody))
            {
                _logger.LogInformation($"Response Body: {responseBody}");
            }
        }

        private async Task copyResponseToOriginalStream(HttpContext context, Stream clientStream)
        {
            await context.Response.Body.CopyToAsync(clientStream);
        }

        private static async Task<string> ReadStreamAsync(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var text = await reader.ReadToEndAsync();
            stream.Seek(0, SeekOrigin.Begin);
            return text;
        }
    }
}
