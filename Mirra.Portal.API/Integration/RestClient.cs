using Mirra_Portal_API.Integration.Interfaces;

namespace Mirra_Portal_API.Integration
{
    public class RestClient : IRestClient
    {

        public async Task<HttpResponseMessage> get(string url)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            return response;

        }
    }
}
