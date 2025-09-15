using System.ComponentModel.DataAnnotations;

namespace Mirra_Portal_API.Model.Requests
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "Name can't be empty.")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Password can't be empty.")]
        public string Password { get; set; }
    }
}
