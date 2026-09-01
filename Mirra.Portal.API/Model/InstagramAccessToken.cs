namespace Mirra_Portal_API.Model
{
    public class InstagramAccessToken
    {
        public string AccessToken { get; set; }
        public long ExpiresIn { get; set; }

        public DateTime ExpiresAt() => DateTime.Now.AddSeconds(ExpiresIn);
    }
}
