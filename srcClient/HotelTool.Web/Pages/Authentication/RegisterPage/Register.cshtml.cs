using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;


namespace HotelTool.Web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Constructor with dependency injection for HTTP client
        public RegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        // Properties bound to form inputs
        [BindProperty]
        public string Username { get; set; } = string.Empty;
        [BindProperty]
        public string Email { get; set; } = string.Empty;
        [BindProperty]
        public string Password { get; set; } = string.Empty;
        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Properties to display success or error messages to the user
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        // Handles POST request for registration form
        public async Task<IActionResult> OnPostAsync()
        {
            // Field validation
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = $"Missing fields - Username: '{Username}', Email: '{Email}', Password: {(string.IsNullOrWhiteSpace(Password) ? "Empty" : "Provided")}";
                return Page();
            }
            // Check if password and confirm password match
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return Page();
            }
            // Check password length
            if (Password.Length < 6)
            {
                ErrorMessage = "Password must be at least 6 characters long.";
                return Page();
            }
            // Validate email format
            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Please enter a valid email address.";
                return Page();
            }
            try
            {
                // Create the HTTP client instance for API calls
                var httpClient = _httpClientFactory.CreateClient("UserServiceAPI");

                // Build the registration request payload
                var registerRequest = new
                {
                    Username = Username.Trim(),
                    Email = Email.Trim(),
                    Password = Password.Trim(),
                    Role = "User" // Default role when registering
                };

                // Serialize request to JSON
                var jsonContent = JsonSerializer.Serialize(registerRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Send the request to the UserService API
                var response = await httpClient.PostAsync("/api/users", content);

                // Handle successful response
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var userResponse = JsonSerializer.Deserialize<UserDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // If deserialization was successful and user is returned
                    if (userResponse != null)
                    {
                        SuccessMessage = $"Registration successful! User '{userResponse.Username}' has been created.";

                        // Clear the form fields
                        Username = string.Empty;
                        Email = string.Empty;
                        Password = string.Empty;
                        ConfirmPassword = string.Empty;

                        // Wait a moment to show success message, then redirect
                        await Task.Delay(1000);
                        return RedirectToPage("../LoginPage/Login");
                    }
                }
                else
                {
                    // Handle failure response
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        // Try to parse error message from API
                        try
                        {
                            using var document = JsonDocument.Parse(errorContent);
                            var root = document.RootElement;

                            if (root.TryGetProperty("message", out var messageElement))
                            {
                                ErrorMessage = messageElement.GetString() ?? "Registration failed.";
                            }
                            else if (root.TryGetProperty("title", out var titleElement))
                            {
                                ErrorMessage = titleElement.GetString() ?? "Registration failed.";
                            }
                            else
                            {
                                ErrorMessage = "Username or email already exists.";
                            }
                        }
                        catch
                        {
                            ErrorMessage = "Username or email already exists.";
                        }
                    }
                    else
                    {
                        ErrorMessage = "Registration failed. Please try again.";
                    }
                    return Page();
                }
            }
            catch (HttpRequestException ex)  // Handle HTTP-related issues
            {
                Console.WriteLine($"HTTP Request Exception: {ex.Message}");
                ErrorMessage = "Unable to connect to the server. Please make sure the UserService is running on http://localhost:5000";
                return Page();
            }
            catch (Exception ex) // Handle any unexpected exceptions
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }

            return Page();
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}