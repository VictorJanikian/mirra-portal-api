using System.ComponentModel.DataAnnotations;
using Mirra_Portal_API.Validation;

namespace Mirra_Portal_API.Model.Requests
{
    public class SchedulingRequest
    {
        [Required]
        public string Interval { get; set; }
        [Required]
        public string Timezone { get; set; }
        [Required]
        [MinValue(1)]
        public int? ContentTypeId { get; set; }
        [Required]
        public ParametersRequest Parameters { get; set; }
    }
}
