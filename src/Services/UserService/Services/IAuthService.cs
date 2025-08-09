using UserService.Models;

namespace UserService.Services
{
    // Interface that defines the contract for authentication-related services
    public interface IAuthService
    {
        Task<AuthResponse> AuthenticateAsync(AuthRequest request);
        Task<string> GenerateTokenAsync(User user);
        Task<bool> ValidateTokenAsync(string token);
    }
}