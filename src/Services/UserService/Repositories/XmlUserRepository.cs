using System.Xml.Serialization;
using UserService.Models;
using UserService.Utils;

namespace UserService.Repositories
{
    // XML-based implementation of the IUserRepository interface
    public class XmlUserRepository : IUserRepository
    {
        private readonly string _filePath;
        private readonly object _lockObject = new object();
        private List<User> _users;

        // Constructor that initializes the XML file path and loads existing data
        public XmlUserRepository(IConfiguration configuration)
        {
            _filePath = configuration.GetValue<string>("XmlStorage:FilePath") ?? "users.xml";
            _users = LoadUsersFromXml();
        }

        // Retrieves an active user by ID
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => u.Id == id && u.IsActive));
        }

        // Retrieves an active user by username (case-insensitive)
        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return await Task.FromResult(_users.FirstOrDefault(u => 
                u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase) && u.IsActive));
        }

        // Retrieves an active user by email (case-insensitive)
        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await Task.FromResult(_users.FirstOrDefault(u => 
                u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase) && u.IsActive));
        }

        // Retrieves all users regardless of active status, ordered by username
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await Task.FromResult(_users.OrderBy(u => u.Username).ToList());
        }

        // Retrieves only active users, ordered by username
        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await Task.FromResult(_users.Where(u => u.IsActive).OrderBy(u => u.Username).ToList());
        }

        // Creates a new user with a hashed password and generated ID
        public async Task<User> CreateAsync(User user)
        {
            lock (_lockObject)
            {
                user.Id = Guid.NewGuid();
                user.CreatedAt = DateTime.UtcNow;

                // Hash password if not already hashed
                if (!string.IsNullOrEmpty(user.Password))
                {
                    user.Password = PasswordHasher.Hash(user.Password);
                }

                _users.Add(user);
                SaveUsersToXml();
            }
            
            return await Task.FromResult(user);
        }

        // Updates an existing user
        public async Task<User> UpdateAsync(User user)
        {
            lock (_lockObject)
            {
                var existingUserIndex = _users.FindIndex(u => u.Id == user.Id);
                if (existingUserIndex >= 0)
                {
                    _users[existingUserIndex] = user;
                    SaveUsersToXml();
                }
            }
            
            return await Task.FromResult(user);
        }

        // Permanently deletes a user from the XML storage by ID
        public async Task<bool> DeleteAsync(Guid id)
        {
            lock (_lockObject)
            {
                var user = _users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                    return false;

                _users.Remove(user);
                SaveUsersToXml();
                return true;
            }
        }

        // Soft-deletes a user by setting IsActive to false
        public async Task<bool> DeactivateAsync(Guid id)
        {
            lock (_lockObject)
            {
                var user = _users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                    return false;

                user.IsActive = false;
                SaveUsersToXml();
                return true;
            }
        }

        // Authenticates a user by validating username and password
        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await GetByUsernameAsync(username);
            if (user == null || !user.IsActive)
                return null;

            // Verify password
            if (PasswordHasher.Verify(password.Trim(), user.Password))
                return user;

            return null;
        }

        // Checks if the given username exists (case-insensitive)
        public async Task<bool> UsernameExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return await Task.FromResult(_users.Any(u => 
                u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase)));
        }

        // Checks if the given email exists (case-insensitive)
        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await Task.FromResult(_users.Any(u => 
                u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)));
        }

        // Loads users from the XML file, creates empty list if file doesn't exist
        private List<User> LoadUsersFromXml()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<User>();
                }

                var serializer = new XmlSerializer(typeof(UserCollection));
                using var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var userCollection = (UserCollection?)serializer.Deserialize(fileStream);
                return userCollection?.Users ?? new List<User>();
            }
            catch (Exception ex)
            {
                // Log the exception 
                Console.WriteLine($"Error loading users from XML: {ex.Message}");
                return new List<User>();
            }
        }

        // Saves the current users list to the XML file
        private void SaveUsersToXml()
        {
            try
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var userCollection = new UserCollection { Users = _users };
                var serializer = new XmlSerializer(typeof(UserCollection));
                
                using var fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                serializer.Serialize(fileStream, userCollection);
            }
            catch (Exception ex)
            {
                // Log the exception 
                Console.WriteLine($"Error saving users to XML: {ex.Message}");
                throw;
            }
        }
    }

    // Wrapper class for XML serialization of user collection
    [XmlRoot("Users")]
    public class UserCollection
    {
        [XmlElement("User")]
        public List<User> Users { get; set; } = new List<User>();
    }
}