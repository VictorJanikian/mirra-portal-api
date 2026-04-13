namespace Mirra_Portal_API.Model
{
    public class Scheduling
    {
        public int Id { get; set; }
        public CustomerPlatformConfiguration CustomerPlatformConfiguration { get; set; }
        public Parameters Parameters { get; set; }
        public string Interval { get; set; }
        public string Timezone { get; set; }
        public int RunsPerWeek { get; set; }
        public SchedulingStatus SchedulingStatus { get; set; }
    }
}
