namespace ReservationService.Models
{
    // DTO used to request availability for a room type
    public class AvailabilityRequest
    {
        public Guid RoomTypeId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string BoardType { get; set; } = string.Empty;
        public Guid? ExcludeReservationId { get; set; }
    }
}