using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace UserService.Models
{
    [XmlType("User")]
    public class User
    {
        // Unique identifier for the user.
        [XmlElement("Id")]
        [Required]
        public Guid Id { get; set; }

        // Username used for login.
        [XmlElement("Username")]
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string Username { get; set; } = string.Empty;

        // Email address
        [XmlElement("Email")]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        // Hashed password of the user.
        [XmlElement("Password")]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255, ErrorMessage = "Password hash cannot exceed 255 characters.")]
        public string Password { get; set; } = string.Empty;

        // Role of the user (e.g., Admin, User).
        [XmlElement("Role")]
        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(Admin|User)$", ErrorMessage = "Role must be Admin or User.")]
        public string Role { get; set; } = string.Empty;

        // When the user was created
        [XmlElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Whether the user account is active
        [XmlElement("IsActive")]
        public bool IsActive { get; set; } = true;

        // Parameterless constructor required for XML serialization
        public User()
        {
            Id = Guid.NewGuid();
        }

        // Constructor to initialize a user with required properties.
        public User(string username, string email, string password, string role)
        {
            Id = Guid.NewGuid();
            Username = username;
            Email = email;
            Password = password;
            Role = role;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }
}