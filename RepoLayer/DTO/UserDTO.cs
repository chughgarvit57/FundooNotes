using System.ComponentModel.DataAnnotations;

namespace RepoLayer.DTO
{
    public class UserDTO
    {
        [Required(ErrorMessage = "First Name is required")]
        [RegularExpression(@"^[A-Z][a-zA-Z]{2,}$", ErrorMessage = "First Name should start with a capital letter and have at least 3 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [RegularExpression(@"^[A-Z][a-zA-Z]{2,}$", ErrorMessage = "Last Name should start with a capital letter and have at least 3 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$"
        ,ErrorMessage = "Password must be at least 6 characters long, contain at least one uppercase letter, one lowercase letter, and one number.")]
        public string Password { get; set; } = string.Empty;
    }
}
