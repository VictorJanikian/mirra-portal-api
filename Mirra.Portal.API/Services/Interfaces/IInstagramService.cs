namespace Mirra_Portal_API.Services.Interfaces
{
    public interface IInstagramService
    {
        /// <summary>Creates a new Instagram configuration holding the CSRF state and returns the Instagram authorization URL.</summary>
        Task<string> StartAuthorization();

        /// <summary>Validates the state, turns the code into a 60 days token and returns the URL to redirect the user back to.</summary>
        Task<string> HandleCallback(string code, string state, string permissions);

        string HomeUrl();
    }
}
