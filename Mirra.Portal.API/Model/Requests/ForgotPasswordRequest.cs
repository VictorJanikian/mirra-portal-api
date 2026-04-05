using System.ComponentModel.DataAnnotations;

namespace Mirra_Portal_API.Model.Requests
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
