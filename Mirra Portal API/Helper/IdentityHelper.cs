using System.Security.Claims;

namespace Mirra_Portal_API.Helper
{
    public class IdentityHelper
    {

        private ClaimsPrincipal user;

        public void setUser(ClaimsPrincipal requestUser)
        {
            user = requestUser;
        }

        public int UserId()
        {
            return Convert.ToInt32(user.FindFirst(ClaimTypes.Sid).Value);
        }
    }
}
