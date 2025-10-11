namespace Mirra_Portal_API.Model
{
    public class Scheduling
    {
        public int Id { get; set; }
        public CustomerPlatformConfiguration CustomerPlatformConfiguration { get; set; }
        public Parameters Parameters { get; set; }
        public string Interval { get; set; }
    }
}
