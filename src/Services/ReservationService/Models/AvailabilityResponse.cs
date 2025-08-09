namespace ReservationService.Models
{
    // DTO used to respond with availability information
    public class AvailabilityResponse
    {
        public bool IsAvailable { get; set; }
        public int AvailableRoomCount { get; set; }
    }
}