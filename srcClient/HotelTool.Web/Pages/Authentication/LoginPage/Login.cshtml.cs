using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Text;
namespace HotelTool.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Properties to bind input fields from the login form
        [BindProperty]
        public required string Username { get; set; }
        [BindProperty]
        public required string Password { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        // Handles the POST request for login
        public async Task<IActionResult> OnPostAsync()
        {
            // Validate user input
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and Password are required.";
                return Page();
            }

            try
            {
                // Create an HTTP client to call the UserService API
                var httpClient = _httpClientFactory.CreateClient("UserServiceAPI");

                // Create login request
                var loginRequest = new
                {
                    Username = Username.Trim(),
                    Password = Password.Trim()
                };

                // Serialize login request to JSON format
                var jsonContent = JsonConvert.SerializeObject(loginRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Call UserService authenticate API
                var response = await httpClient.PostAsync("/api/users/authenticate", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Deserialize the response into an AuthResponse object
                    var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseContent);

                    // Check if authentication was successful
                    if (authResponse?.Success == true && authResponse.User != null)
                    {
                        // Store user details in session
                        HttpContext.Session.SetString("Username", authResponse.User.Username);
                        HttpContext.Session.SetString("Role", authResponse.User.Role);
                        HttpContext.Session.SetString("UserId", authResponse.User.Id.ToString());
                        HttpContext.Session.SetString("Email", authResponse.User.Email);
                        HttpContext.Session.SetString("AuthToken", authResponse.Token);

                        return RedirectToPage("../../Index");
                    }
                    else
                    {
                        // Login failed; show the error message from API or generic message
                        ErrorMessage = authResponse?.Message ?? "Login failed.";
                        return Page();
                    }
                }
                else // If response status is not success 
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = "Invalid username or password.";
                    return Page();
                }
            }
            catch (HttpRequestException) // Handle connectivity issues
            {
                ErrorMessage = "Unable to connect to the server. Please make sure the UserService is running.";
                return Page();
            }
            catch (Exception ex)  // Handle unexpected errors
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }
    }

    // Model representing the authentication response from the API
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public UserDto? User { get; set; }
        public DateTime Expires { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // DTO representing user details received from the authentication API
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}