using System.Text.Json;
using Microsoft.Extensions.Options;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Integration.Interfaces;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Integration
{
    public class InstagramIntegration : IInstagramIntegration
    {
        public const string CallbackRoute = "/api/Configuration/instagram/callback";

        private const string AuthorizationUrl = "https://www.instagram.com/oauth/authorize";
        private const string ShortLivedTokenUrl = "https://api.instagram.com/oauth/access_token";
        private const string LongLivedTokenUrl = "https://graph.instagram.com/access_token";
        private const string ProfileUrl = "https://graph.instagram.com/me";
        private const string Scope = "instagram_business_basic,instagram_business_content_publish";

        private readonly IRestClient _restClient;
        private readonly InstagramSettings _settings;
        private readonly ApplicationSettings _applicationSettings;

        public InstagramIntegration(IRestClient restClient,
                                    IOptions<InstagramSettings> settings,
                                    IOptions<ApplicationSettings> applicationSettings)
        {
            _restClient = restClient;
            _settings = settings.Value;
            _applicationSettings = applicationSettings.Value;
        }

        public string BuildAuthorizationUrl(string state)
        {
            return AuthorizationUrl
                + "?client_id=" + Uri.EscapeDataString(_settings.AppId)
                + "&redirect_uri=" + Uri.EscapeDataString(RedirectUri())
                + "&response_type=code"
                + "&scope=" + Uri.EscapeDataString(Scope)
                + "&state=" + Uri.EscapeDataString(state);
        }

        public async Task<InstagramAccessToken> ExchangeCodeForShortLivedToken(string code)
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _settings.AppId,
                ["client_secret"] = _settings.AppSecret,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = RedirectUri(),
                ["code"] = code
            });

            using var response = await _restClient.post(ShortLivedTokenUrl, form);
            var root = await ReadJson(response, "Could not exchange the Instagram code for an access token.");

            // Depending on the API version the payload comes either flat or wrapped in "data".
            if (root.TryGetProperty("data", out var data)
                && data.ValueKind == JsonValueKind.Array
                && data.GetArrayLength() > 0)
            {
                root = data[0];
            }

            return new InstagramAccessToken
            {
                AccessToken = ReadString(root, "access_token")
                    ?? throw new BadRequestException("Instagram did not return an access token."),
                ExpiresIn = ReadInt64(root, "expires_in")
            };
        }

        public async Task<InstagramAccessToken> ExchangeShortLivedForLongLivedToken(string shortLivedToken)
        {
            var url = LongLivedTokenUrl
                + "?grant_type=ig_exchange_token"
                + "&client_secret=" + Uri.EscapeDataString(_settings.AppSecret)
                + "&access_token=" + Uri.EscapeDataString(shortLivedToken);

            using var response = await _restClient.get(url);
            var root = await ReadJson(response, "Could not exchange the Instagram token for a long lived one.");

            return new InstagramAccessToken
            {
                AccessToken = ReadString(root, "access_token")
                    ?? throw new BadRequestException("Instagram did not return a long lived access token."),
                ExpiresIn = ReadInt64(root, "expires_in")
            };
        }

        public async Task<InstagramProfile> GetProfile(string accessToken)
        {
            var url = ProfileUrl
                + "?fields=user_id,username"
                + "&access_token=" + Uri.EscapeDataString(accessToken);

            using var response = await _restClient.get(url);
            var root = await ReadJson(response, "Could not recover the Instagram profile.");

            return new InstagramProfile
            {
                UserId = ReadInt64(root, "user_id") is var userId && userId != 0 ? userId : ReadInt64(root, "id"),
                Username = ReadString(root, "username")
            };
        }

        /*---*/

        private string RedirectUri()
        {
            return _applicationSettings.ApiBaseUrl.TrimEnd('/') + CallbackRoute;
        }

        private static async Task<JsonElement> ReadJson(HttpResponseMessage response, string errorMessage)
        {
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new BadRequestException(errorMessage + " Instagram answered: " + body);

            try
            {
                return JsonDocument.Parse(body).RootElement.Clone();
            }
            catch (JsonException)
            {
                throw new BadRequestException(errorMessage);
            }
        }

        private static string ReadString(JsonElement element, string property)
        {
            if (!element.TryGetProperty(property, out var value)) return null;

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.GetRawText(),
                _ => null
            };
        }

        private static long ReadInt64(JsonElement element, string property)
        {
            if (!element.TryGetProperty(property, out var value)) return 0;

            return value.ValueKind switch
            {
                JsonValueKind.Number => value.GetInt64(),
                JsonValueKind.String => long.TryParse(value.GetString(), out var parsed) ? parsed : 0,
                _ => 0
            };
        }
    }
}
