using Mirra_Portal_API.Integration.Interfaces;

namespace Mirra_Portal_API.Integration
{
    public class RestClient : IRestClient
    {

        public async Task<HttpResponseMessage> get(string url)
        {
            using var client = new HttpClient();
            return await client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> post(string url, HttpContent content)
        {
            using var client = new HttpClient();
            return await client.PostAsync(url, content);
        }
    }
}
