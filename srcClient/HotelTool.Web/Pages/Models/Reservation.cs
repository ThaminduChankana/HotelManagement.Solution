using System.ComponentModel.DataAnnotations;

namespace HotelTool.Web.Models
{
    // Represents a reservation made by a guest
    public class Reservation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? UserID { get; set; }
        public Guid RoomTypeId { get; set; }
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(25, ErrorMessage = "First Name cannot exceed 25 characters.")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(25, ErrorMessage = "Last Name cannot exceed 25 characters.")]
        public string LastName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Country is required.")]
        [StringLength(25, ErrorMessage = "Country Name cannot exceed 25 characters.")]
        public string Country { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string PhoneNumber { get; set; } = string.Empty;
        public string BookFor { get; set; } = string.Empty;
        public bool IsWorkRelated { get; set; }
        [Required(ErrorMessage = "Check-in date is required.")]
        public DateTime CheckInDate { get; set; }
        [Required(ErrorMessage = "Check-out date is required.")]
        public DateTime CheckOutDate { get; set; }
        [Required(ErrorMessage = "Payment method is required.")]
        public string PayBy { get; set; } = string.Empty;
        public string FullOrHalfBoard { get; set; } = string.Empty;
        [Required(ErrorMessage = "Total Price is required.")]
        public decimal TotalCost { get; set; }
        public string SpecialRequest { get; set; } = string.Empty;
        public ReservationStatus Status { get; set; } = ReservationStatus.Active;
        public string AdminNote { get; set; } = string.Empty;
        public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;
        [Range(0, 365, ErrorMessage = "Recurrence must be between 0 and 365.")]
        public int RecurrenceCount { get; set; } = 0;
        public string? AllocatedRoomNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    // Enums for the reservation status
    public enum ReservationStatus
    {
        Active,
        Canceled,
        CheckedIn,
        Completed
    }

    // Enums for the recurrence types
    public enum RecurrenceType
    {
        None,        // For one-off bookings
        Daily,
        Weekly,
        Monthly
    }
}