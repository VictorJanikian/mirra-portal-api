namespace Mirra_Portal_API.Integration.Interfaces
{
    public interface IWordpressIntegration
    {
        public Task checkIfIsValidWordPressSite(string url);
    }
}
