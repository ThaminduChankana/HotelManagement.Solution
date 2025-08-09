using UserService.Models;

namespace UserService.Services
{
    // Interface defining the contract for user-related operations such as creating, retrieving, updating, and deleting users.
    public interface IUserService
    {
        Task<UserDto?> GetUserAsync(Guid id);
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<UserDto>> GetActiveUsersAsync();
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
        Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> DeactivateUserAsync(Guid id);
        bool ValidateUserData(CreateUserRequest request);
    }
}