using System.ComponentModel.DataAnnotations;

namespace ReservationService.Models
{
    public class Reservation
    {
        // Unique identifier for the reservation.
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        // The ID of the user making the reservation.
        public string? UserID { get; set; }

        // The ID of the room type being reserved.
        [Required(ErrorMessage = "Room Type ID is required.")]
        public Guid RoomTypeId { get; set; }

        // First name of the guest.
        [Required(ErrorMessage = "First name of the user is required.")]
        [StringLength(25, ErrorMessage = "First Name cannot exceed 25 characters.")]
        public string FirstName { get; set; } = string.Empty;

        // Last name of the guest.
        [Required(ErrorMessage = "Last name of the user is required.")]
        [StringLength(25, ErrorMessage = "Last Name cannot exceed 25 characters.")]
        public string LastName { get; set; } = string.Empty;

        // Guest's email address.
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        // Guest's country of residence.
        [Required(ErrorMessage = "Country is required.")]
        [StringLength(25, ErrorMessage = "Country Name cannot exceed 25 characters.")]
        public string Country { get; set; } = string.Empty;

        // Guest's contact number.
        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Who the booking is for (self or someone else).
        public string BookFor { get; set; } = string.Empty;

        // Whether the stay is work-related.
        public bool IsWorkRelated { get; set; }

        // Date of check-in.
        [Required(ErrorMessage = "Check-in date is required.")]
        public DateTime CheckInDate { get; set; }

        // Date of check-out.
        [Required(ErrorMessage = "Check-out date is required.")]
        public DateTime CheckOutDate { get; set; }

        // Payment method.
        [Required(ErrorMessage = "Payment method is required.")]
        public string PayBy { get; set; } = string.Empty;

        // Meal plan selected: Full or Half Board.
        public string FullOrHalfBoard { get; set; } = string.Empty;

        // Total price for the reservation.
        [Required(ErrorMessage = "Total Price is required.")]
        public decimal TotalCost { get; set; }

        // Special requests from the guest.
        public string SpecialRequest { get; set; } = string.Empty;

        // Current status of the reservation.
        [Required(ErrorMessage = "Status is required.")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Active;

        // Admin remarks or comments.
        public string AdminNote { get; set; } = string.Empty;

        // Whether the booking recurs and how often.
        public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;

        // How many times the booking should repeat if recurring.
        [Range(0, 365, ErrorMessage = "Recurrence must be between 0 and 365.")]
        public int RecurrenceCount { get; set; } = 0;

        // Allocated room number for the guest.
        public string? AllocatedRoomNumber { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    // Enum to represent the current status of a reservation.
    public enum ReservationStatus
    {
        Active,
        Canceled,
        CheckedIn,
        Completed
    }

    // Enum to define the recurrence type of a reservation.
    public enum RecurrenceType
    {
        None,        // For one-off bookings
        Daily,
        Weekly,
        Monthly
    }
}