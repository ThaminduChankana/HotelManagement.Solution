using System.ComponentModel.DataAnnotations;

namespace ReservationService.Models
{
    // DTO used to create a new reservation
    public class CreateReservationRequest
    {
        // User ID for authenticated users
        public string? UserID { get; set; }
        // The ID of the selected room type
        [Required(ErrorMessage = "Room Type ID is required.")]
        public Guid RoomTypeId { get; set; }
        // First name of the person making the reservation
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(25, ErrorMessage = "First Name cannot exceed 25 characters.")]
        public string FirstName { get; set; } = string.Empty;
        // Last name of the person making the reservation
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(25, ErrorMessage = "Last Name cannot exceed 25 characters.")]
        public string LastName { get; set; } = string.Empty;
        // Valid email address
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;
        // Country of the guest
        [Required(ErrorMessage = "Country is required.")]
        [StringLength(25, ErrorMessage = "Country Name cannot exceed 25 characters.")]
        public string Country { get; set; } = string.Empty;
        // Valid phone number
        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string PhoneNumber { get; set; } = string.Empty;
        // Name of the person for whom the booking is made
        public string BookFor { get; set; } = string.Empty;
        // Indicates whether the reservation is for a work-related purpose
        public bool IsWorkRelated { get; set; }
        // Check-in date
        [Required(ErrorMessage = "Check-in date is required.")]
        public DateTime CheckInDate { get; set; }
        // Check-out date
        [Required(ErrorMessage = "Check-out date is required.")]
        public DateTime CheckOutDate { get; set; }
        // Payment method 
        [Required(ErrorMessage = "Payment method is required.")]
        public string PayBy { get; set; } = string.Empty;
        // Board type (e.g., Full Board, Half Board)
        public string FullOrHalfBoard { get; set; } = string.Empty;
        // Any special requests 
        public string SpecialRequest { get; set; } = string.Empty;
        // Recurrence type (e.g., None, Daily, Weekly)
        public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;
        // Number of recurrences
        [Range(0, 365, ErrorMessage = "Recurrence must be between 0 and 365.")]
        public int RecurrenceCount { get; set; } = 0;
    }
}