namespace ReservationService.Models
{
    // DTO used to represent the result of a cancellation operation
    public class CancellationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}