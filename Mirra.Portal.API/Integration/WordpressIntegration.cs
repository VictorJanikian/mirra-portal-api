using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Integration.Interfaces;

namespace Mirra_Portal_API.Integration
{
    public class WordpressIntegration : IWordpressIntegration
    {
        private readonly IRestClient _restClient;

        public WordpressIntegration(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task checkIfIsValidWordPressSite(string url)
        {
            url = url.TrimEnd('/');
            using var wordpressResponse = await _restClient.get(url + "/wp-json/");
            if (wordpressResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new BadRequestException("The provided URL is not a valid WordPress site.");
            }
        }
    }
}
