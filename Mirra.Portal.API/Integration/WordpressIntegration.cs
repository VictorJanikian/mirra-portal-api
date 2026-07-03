using System.Text.Json;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Integration.Interfaces;

namespace Mirra_Portal_API.Integration
{
    public class WordpressIntegration : IWordpressIntegration
    {
        private const string WordPressCoreNamespace = "wp/v2";

        private readonly IRestClient _restClient;

        public WordpressIntegration(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task checkIfIsValidWordPressSite(string url)
        {
            url = url.TrimEnd('/');
            try
            {
                using var wordpressResponse = await _restClient.get(url + "/wp-json/");
                if (!await IsWordPressRestApiResponse(wordpressResponse))
                {
                    throw new BadRequestException("The provided URL is not a valid WordPress site.");
                }
            }
            catch (HttpRequestException)
            {
                throw new BadRequestException("The provided URL is not a valid WordPress site.");
            }
        }

        private static async Task<bool> IsWordPressRestApiResponse(HttpResponseMessage response)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }

            if (!IsJsonContent(response))
            {
                return false;
            }

            var body = await response.Content.ReadAsStringAsync();
            return ExposesWordPressCoreNamespace(body);
        }

        private static bool IsJsonContent(HttpResponseMessage response)
        {
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            return mediaType is not null
                && mediaType.Contains("json", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ExposesWordPressCoreNamespace(string body)
        {
            try
            {
                using var document = JsonDocument.Parse(body);

                return document.RootElement.TryGetProperty("namespaces", out var namespaces)
                    && namespaces.ValueKind == JsonValueKind.Array
                    && namespaces.EnumerateArray().Any(registeredNamespace =>
                        registeredNamespace.ValueKind == JsonValueKind.String
                        && registeredNamespace.GetString() == WordPressCoreNamespace);
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
