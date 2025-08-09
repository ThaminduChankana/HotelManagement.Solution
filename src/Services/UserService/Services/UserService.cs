using System.ComponentModel.DataAnnotations;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    // This class implements the IUserService interface and provides business logic for managing users
    public class UserServiceImpl : IUserService
    {
        private readonly IUserRepository _userRepository;

        // Constructor injection of the user repository
        public UserServiceImpl(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Fetch a single user by ID and return as a DTO, or null if not found
        public async Task<UserDto?> GetUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : MapToDto(user);
        }
        
        // Fetch a user by their username and return as a DTO, or null if not found
        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user == null ? null : MapToDto(user);
        }
        
        // Retrieve all users and return them as a list of DTOs
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto);
        }
        
        // Retrieve all active users and return them as a list of DTOs
        public async Task<IEnumerable<UserDto>> GetActiveUsersAsync()
        {
            var users = await _userRepository.GetActiveUsersAsync();
            return users.Select(MapToDto);
        }
        
        // Create a new user based on the incoming request
        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            // Validate request
            if (!ValidateUserData(request))
                throw new InvalidOperationException("Invalid user data provided.");

            // Check for existing username
            if (await _userRepository.UsernameExistsAsync(request.Username))
                throw new InvalidOperationException("Username already exists.");

            // Check for existing email
            if (await _userRepository.EmailExistsAsync(request.Email))
                throw new InvalidOperationException("Email already exists.");

            var user = new User(
                request.Username.Trim(),
                request.Email.Trim(),
                request.Password.Trim(),
                request.Role.Trim()
            );

            var createdUser = await _userRepository.CreateAsync(user);
            return MapToDto(createdUser);
        }

        // Update an existing user with new data
        public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
                throw new InvalidOperationException("User not found.");

            // Check if username is being changed and if it already exists
            if (!existingUser.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase))
            {
                if (await _userRepository.UsernameExistsAsync(request.Username))
                    throw new InvalidOperationException("Username already exists.");
            }

            // Check if email is being changed and if it already exists
            if (!existingUser.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _userRepository.EmailExistsAsync(request.Email))
                    throw new InvalidOperationException("Email already exists.");
            }

            // Update user properties
            existingUser.Username = request.Username.Trim();
            existingUser.Email = request.Email.Trim();
            existingUser.Role = request.Role.Trim();
            existingUser.IsActive = request.IsActive;

            var updatedUser = await _userRepository.UpdateAsync(existingUser);
            return MapToDto(updatedUser);
        }
        
        // Delete a user by ID
        public async Task<bool> DeleteUserAsync(Guid id)
        {
            return await _userRepository.DeleteAsync(id);
        }
        
        // Deactivate a user without deleting the record
        public async Task<bool> DeactivateUserAsync(Guid id)
        {
            return await _userRepository.DeactivateAsync(id);
        }

        // Validates user data using data annotations and additional business rules
        public bool ValidateUserData(CreateUserRequest request)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(request);

            bool isValid = Validator.TryValidateObject(request, context, validationResults, true);

            // Additional business logic validation
            if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
                return false;

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                return false;

            if (!IsValidEmail(request.Email))
                return false;

            return isValid;
        }
        
        // Maps the User entity to a UserDto
        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
        
        // Validates email format
        private static bool IsValidEmail(string email)
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