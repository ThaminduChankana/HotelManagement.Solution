namespace HotelTool.Web.Models
{
    // Represents an error response returned by the API
    public class ApiErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

    // Represents a success response returned by the API
    public class ApiSuccessResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    // Represents the response for room availability
    public class AvailabilityResponse
    {
        public bool IsAvailable { get; set; }
        public int AvailableRoomCount { get; set; }
    }

    // Request model for creating a new reservation
    public class CreateReservationRequest
    {
        public string? UserID { get; set; }
        public Guid RoomTypeId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string BookFor { get; set; } = string.Empty;
        public bool IsWorkRelated { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string PayBy { get; set; } = string.Empty;
        public string FullOrHalfBoard { get; set; } = string.Empty;
        public string SpecialRequest { get; set; } = string.Empty;
        public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;
        public int RecurrenceCount { get; set; } = 0;
    }

    // Request model for updating an existing reservation
    public class UpdateReservationRequest
    {
        public Guid RoomTypeId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string BookFor { get; set; } = string.Empty;
        public bool IsWorkRelated { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string PayBy { get; set; } = string.Empty;
        public string FullOrHalfBoard { get; set; } = string.Empty;
        public string SpecialRequest { get; set; } = string.Empty;
    }
}