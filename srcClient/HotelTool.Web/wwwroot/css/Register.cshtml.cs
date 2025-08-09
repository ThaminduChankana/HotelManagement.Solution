using HotelTool.Web.Data;
using HotelTool.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelTool.Web.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        // Register the standard user
        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and Password are required.";
                return Page();
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return Page();
            }

            if (UserRepository.GetByUsername(Username) != null)
            {
                ErrorMessage = "Username already exists.";
                return Page();
            }

            var user = new User
            {
                Username = Username,
                Password = Password,
                Role = "User" // Default role when registering
            };

            UserRepository.Add(user);
            return RedirectToPage("../LoginPage/Login");
        }
    }
}
