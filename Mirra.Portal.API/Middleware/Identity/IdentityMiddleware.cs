using Mirra_Portal_API.Helper;

namespace Mirra_Portal_API.Middleware.Identity
{
    public class IdentityMiddleware
    {
        private readonly RequestDelegate _next;

        public IdentityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IdentityHelper identityHelper)
        {

            identityHelper.setUser(context.User);
            await _next(context);

        }
    }
}
