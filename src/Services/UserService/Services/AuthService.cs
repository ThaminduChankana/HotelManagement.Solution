using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    // AuthService provides authentication logic (login, token generation/validation)
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        // Constructor injection of the user repository
        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        // Authenticates a user and returns a response containing user info and token
        public async Task<AuthResponse> AuthenticateAsync(AuthRequest request)
        {
            try
            {
                // Try to authenticate the user using username and password
                var user = await _userRepository.AuthenticateAsync(request.Username, request.Password);

                // Authentication failed: user not found or invalid password
                if (user == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid username or password."
                    };
                }

                // Generate a token for the authenticated user
                var token = await GenerateTokenAsync(user);
                
                // Return authentication success response with user data and token
                return new AuthResponse
                {
                    Success = true,
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt,
                        IsActive = user.IsActive
                    },
                    Expires = DateTime.UtcNow.AddHours(24), // Token validity duration
                    Message = "Authentication successful."
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Authentication failed: {ex.Message}"
                };
            }
        }

        // Generates a simple base64-encoded token from user data
        public async Task<string> GenerateTokenAsync(User user)
        {
            var token = $"{user.Id}:{user.Username}:{DateTime.UtcNow.Ticks}";
            return await Task.FromResult(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token)));
        }
        
        // Validates the token by decoding it and checking the user exists and is active
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decoded.Split(':');

                // Ensure the token has exactly 3 parts: userId, username, timestamp
                if (parts.Length != 3)
                    return false;
                    
                // Try to parse user ID from token and check if the user still exists and is active
                var userId = Guid.Parse(parts[0]);
                var user = await _userRepository.GetByIdAsync(userId);

                return user != null && user.IsActive;
            }
            catch
            {
                return false;
            }
        }
    }
}