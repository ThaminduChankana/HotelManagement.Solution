using ReservationService.Models;

namespace ReservationService.Repositories
{
    // Defines the contract for reservation data access operations
    public interface IReservationRepository
    {
        IEnumerable<Reservation> GetAll();
        Reservation? GetById(Guid id);
        IEnumerable<Reservation> GetByUserId(string userId);
        IEnumerable<Reservation> GetOverlappingReservations(Guid roomTypeId, DateTime checkIn, DateTime checkOut);
        void Add(Reservation reservation);
        void Update(Reservation reservation);
        void Delete(Guid id);
    }
}
