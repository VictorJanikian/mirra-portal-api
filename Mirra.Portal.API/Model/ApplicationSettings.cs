namespace Mirra_Portal_API.Model
{
    public class ApplicationSettings
    {
        /// <summary>Public address of this API, used to build integration redirect URLs.</summary>
        public string ApiBaseUrl { get; set; }

        /// <summary>Front-end home the user is sent back to once an authorization flow finishes.</summary>
        public string HomeUrl { get; set; }
    }
}
