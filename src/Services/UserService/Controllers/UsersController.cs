using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        // Constructor to inject user and auth services
        public UsersController(IUserService userService, IAuthService authService, IConfiguration configuration)
        {
            _userService = userService;
            _authService = authService;
            _configuration = configuration;
        }

        // POST: api/users/authenticate
        // Authenticates a user with username and password
        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.AuthenticateAsync(request);

            if (!response.Success)
                return Unauthorized(response);

            return Ok(response);
        }

        // GET: api/users
        // Retrieves all active users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetActiveUsersAsync();
            return Ok(users);
        }

        // GET: api/users/{id}
        // Retrieves a user by their unique ID
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(Guid id)
        {
            var user = await _userService.GetUserAsync(id);
            if (user == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(user);
        }

        // GET: api/users/username/{username}
        // Retrieves a user by username
        [HttpGet("username/{username}")]
        public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return NotFound($"User with username '{username}' not found.");

            return Ok(user);
        }

        // POST: api/users
        // Creates a new user
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userService.CreateUserAsync(request);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/users/{id}
        // Updates an existing user
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userService.UpdateUserAsync(id, request);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/users/{id}
        // Permanently deletes a user
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound($"User with ID {id} not found.");

            return NoContent();
        }

        // POST: api/users/{id}/deactivate
        // Soft deletes or deactivates a user 
        [HttpPost("{id}/deactivate")]
        public async Task<ActionResult> DeactivateUser(Guid id)
        {
            var result = await _userService.DeactivateUserAsync(id);
            if (!result)
                return NotFound($"User with ID {id} not found.");

            return Ok(new { message = "User deactivated successfully." });
        }

        // GET: api/users/test-storage
        // Simple endpoint to verify XML storage connectivity and metadata
        [HttpGet("test-storage")]
        public async Task<ActionResult> TestStorage()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var activeUsers = await _userService.GetActiveUsersAsync();
                var xmlFilePath = _configuration.GetValue<string>("XmlStorage:FilePath") ?? "users.xml";
                var fileExists = System.IO.File.Exists(xmlFilePath);
                var fileSize = fileExists ? new FileInfo(xmlFilePath).Length : 0;

                return Ok(new
                {
                    StorageType = "XML File",
                    FilePath = xmlFilePath,
                    FileExists = fileExists,
                    FileSizeBytes = fileSize,
                    TotalUserCount = users.Count(),
                    ActiveUserCount = activeUsers.Count(),
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }
    }
}