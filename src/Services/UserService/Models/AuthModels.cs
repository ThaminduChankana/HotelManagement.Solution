using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    // Model used for login/authentication requests
    public class AuthRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }

    // Model returned after a successful or failed authentication attempt
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();
        public DateTime Expires { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}