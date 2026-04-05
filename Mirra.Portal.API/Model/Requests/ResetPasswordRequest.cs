using System.ComponentModel.DataAnnotations;

namespace Mirra_Portal_API.Model.Requests
{
    public class  ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Password can't be empty.")]
        public string NewPassword { get; set; }
    }
}
