using Mirra_Portal_API.Enums;

namespace Mirra_Portal_API.Model.Requests
{
    public class CustomerContentPlatformsRequest
    {
        public ContentPlatformsEnum ContentPlatformId { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<SchedulingRequest> Schedulings { get; set; }
    }
}
