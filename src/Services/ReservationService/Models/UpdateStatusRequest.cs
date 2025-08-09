namespace ReservationService.Models
{
    // Represents a request to update the status of a reservation.
    public class UpdateStatusRequest
    {
        public ReservationStatus Status { get; set; }
        public string? AdminNote { get; set; }
    }
}