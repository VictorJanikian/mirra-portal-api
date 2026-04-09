namespace Mirra_Portal_API.Integration.Interfaces
{
    public interface IRestClient
    {
        Task<HttpResponseMessage> get(string url);
    }
}
