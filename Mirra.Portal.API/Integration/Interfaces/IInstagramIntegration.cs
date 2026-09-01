using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Integration.Interfaces
{
    public interface IInstagramIntegration
    {
        string BuildAuthorizationUrl(string state);
        Task<InstagramAccessToken> ExchangeCodeForShortLivedToken(string code);
        Task<InstagramAccessToken> ExchangeShortLivedForLongLivedToken(string shortLivedToken);
        Task<InstagramProfile> GetProfile(string accessToken);
    }
}
