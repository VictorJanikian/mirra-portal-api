namespace Mirra_Portal_API.Model.Responses
{
    public class SchedulingResponse
    {
        public int Id { get; set; }
        public string Interval { get; set; }
        public int? Status { get; set; }
        public ParametersResponse Parameters { get; set; }
    }
}
