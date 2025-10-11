using Mirra_Portal_API.Enums;

namespace Mirra_Portal_API.Model.Responses
{
    public class ConfigurationResponse
    {
        public int Id { get; set; }
        public EPlatform PlatformId { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public List<SchedulingResponse> Schedulings { get; set; }
    }
}
