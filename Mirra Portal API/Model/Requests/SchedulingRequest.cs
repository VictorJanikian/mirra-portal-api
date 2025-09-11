namespace Mirra_Portal_API.Model.Requests
{
    public class SchedulingRequest
    {
        public string Interval { get; set; }

        public ParametersRequest Parameters { get; set; }
    }
}
