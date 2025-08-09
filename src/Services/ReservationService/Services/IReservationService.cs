using ReservationService.DTOs;
using ReservationService.Models;

namespace ReservationService.Services
{
    public interface IReservationService
    {
        // Synchronous methods that do not rely on RoomService
        IEnumerable<Reservation> GetAll();
        Reservation? GetById(Guid id);
        IEnumerable<Reservation> GetByUserId(string userId);
        bool UpdateStatus(Guid id, ReservationStatus status, string? adminNote);
        CancellationResult CancelReservation(Guid id);

        // Asynchronous methods that require communication with RoomService
        Task<Reservation> CreateAsync(CreateReservationRequest request);
        Task<Reservation?> UpdateAsync(Guid id, UpdateReservationRequest request);
        Task<AvailabilityResponse> CheckAvailabilityAsync(AvailabilityRequest request);
        Task<int> GetAvailableCountAsync(Guid roomTypeId, DateTime checkIn, DateTime checkOut);
    }
}