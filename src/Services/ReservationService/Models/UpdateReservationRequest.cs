using System.ComponentModel.DataAnnotations;

namespace ReservationService.Models
{
    // Represents the data required to update an existing reservation.
    public class UpdateReservationRequest
    {
        // The ID of the room type being reserved.
        [Required(ErrorMessage = "Room Type ID is required.")]
        public Guid RoomTypeId { get; set; }
        // First name of the guest.
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(25, ErrorMessage = "First Name cannot exceed 25 characters.")]
        public string FirstName { get; set; } = string.Empty;
        // Last name of the guest.
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(25, ErrorMessage = "Last Name cannot exceed 25 characters.")]
        public string LastName { get; set; } = string.Empty;
        // Email address of the guest.
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;
        // Country of the guest.
        [Required(ErrorMessage = "Country is required.")]
        [StringLength(25, ErrorMessage = "Country Name cannot exceed 25 characters.")]
        public string Country { get; set; } = string.Empty;
        // Phone number of the guest
        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string PhoneNumber { get; set; } = string.Empty;
        // Field to specify whom the booking is for
        public string BookFor { get; set; } = string.Empty;
        // Indicates whether the reservation is work-related.
        public bool IsWorkRelated { get; set; }
        // Date of check-in.
        [Required(ErrorMessage = "Check-in date is required.")]
        public DateTime CheckInDate { get; set; }
        // Date of check-out.
        [Required(ErrorMessage = "Check-out date is required.")]
        public DateTime CheckOutDate { get; set; }
        // Method of payment (e.g., card, cash).
        [Required(ErrorMessage = "Payment method is required.")]
        public string PayBy { get; set; } = string.Empty;
        // Board type selected (e.g., Full Board, Half Board).
        public string FullOrHalfBoard { get; set; } = string.Empty;
        // Special requests by the guest (e.g., late check-in, extra pillows).
        public string SpecialRequest { get; set; } = string.Empty;
    }
}