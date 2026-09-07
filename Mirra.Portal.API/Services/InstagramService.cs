using Microsoft.Extensions.Options;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Enums;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Helper;
using Mirra_Portal_API.Integration.Interfaces;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Model.Responses;
using Mirra_Portal_API.Services.Interfaces;
using System.Security.Cryptography;

namespace Mirra_Portal_API.Services
{
    public class InstagramService : IInstagramService
    {
        private const string PlatformName = "Instagram";

        ICustomerPlatformConfigurationRepository _configurationRepository;
        IInstagramIntegration _instagramIntegration;
        IdentityHelper _identityHelper;
        ApplicationSettings _applicationSettings;

        public InstagramService(ICustomerPlatformConfigurationRepository configurationRepository,
                                IInstagramIntegration instagramIntegration,
                                IdentityHelper identityHelper,
                                IOptions<ApplicationSettings> applicationSettings)
        {
            _configurationRepository = configurationRepository;
            _instagramIntegration = instagramIntegration;
            _identityHelper = identityHelper;
            _applicationSettings = applicationSettings.Value;
        }

        public async Task<StartInstagranIntegrationResponse> StartAuthorization()
        {
            var state = generateState();

            await _configurationRepository.Create(new CustomerPlatformConfiguration
            {
                Customer = new Customer { Id = _identityHelper.UserId() },
                Platform = new Platform { Id = (int)EPlatform.INSTAGRAM },
                PlatformName = PlatformName,
                Url = string.Empty,
                Username = string.Empty,
                Password = string.Empty,
                InstagramState = state,
                IsConfirmed = false
            });

            return new StartInstagranIntegrationResponse { RedirectUrl = _instagramIntegration.BuildAuthorizationUrl(state) };
        }

        public async Task<string> HandleCallback(string code, string state, string? permissions)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new BadRequestException("The Instagram authorization code is missing.");

            if (string.IsNullOrWhiteSpace(state))
                throw new BadRequestException("The Instagram state is missing.");

            var configuration = await _configurationRepository.GetByInstagramState(state);
            if (configuration == null)
                throw new UnauthorizedException("Invalid Instagram state.");

            var shortLivedToken = await _instagramIntegration.ExchangeCodeForShortLivedToken(sanitizeCode(code));
            var longLivedToken = await _instagramIntegration.ExchangeShortLivedForLongLivedToken(shortLivedToken.AccessToken);
            var profile = await _instagramIntegration.GetProfile(longLivedToken.AccessToken);

            await _configurationRepository.SaveInstagramCredentials(
                configuration.Id,
                profile,
                longLivedToken,
                splitPermissions(permissions));

            return HomeUrl();
        }

        public string HomeUrl()
        {
            return _applicationSettings.HomeUrl;
        }

        /*---*/

        private static string generateState()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        /// <summary>Instagram appends "#_" to the code when it redirects the browser back.</summary>
        private static string sanitizeCode(string code)
        {
            var fragmentIndex = code.IndexOf('#');
            return fragmentIndex >= 0 ? code.Substring(0, fragmentIndex) : code;
        }

        private static List<string> splitPermissions(string? permissions)
        {
            if (string.IsNullOrWhiteSpace(permissions)) return new List<string>();

            return permissions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToList();
        }
    }
}
