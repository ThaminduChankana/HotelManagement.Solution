using Microsoft.EntityFrameworkCore;
using ReservationService.Data;
using ReservationService.Models;

namespace ReservationService.Repositories
{
    // Provides concrete implementation for IReservationRepository using EF Core and ReservationContext
    public class ReservationRepository : IReservationRepository
    {
        private readonly ReservationContext _context;

        // The database context to access reservation
        public ReservationRepository(ReservationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        // Retrieves all reservations from the database
        public IEnumerable<Reservation> GetAll()
        {
            return _context.Reservations.ToList();
        }
        // Retrieves a reservation by its unique identifier
        public Reservation? GetById(Guid id)
        {
            return _context.Reservations.FirstOrDefault(r => r.Id == id);
        }
        // Retrieves all reservations for a specific user
        public IEnumerable<Reservation> GetByUserId(string userId)
        {
            return _context.Reservations.Where(r => r.UserID == userId).ToList();
        }
        // Retrieves reservations that overlap with the given date range for a specific room type
        // Excludes reservations that are canceled
        public IEnumerable<Reservation> GetOverlappingReservations(Guid roomTypeId, DateTime checkIn, DateTime checkOut)
        {
            return _context.Reservations.Where(r =>
                r.RoomTypeId == roomTypeId &&
                !(r.CheckOutDate <= checkIn || r.CheckInDate >= checkOut) &&
                r.Status != ReservationStatus.Canceled
            ).ToList();
        }
        // Adds a new reservation to the database 
        public void Add(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
            _context.SaveChanges();
        }
        // Updates an existing reservation 
        public void Update(Reservation reservation)
        {
            _context.Reservations.Update(reservation);
            _context.SaveChanges();
        }
        // Deletes a reservation by its ID if it exists
        public void Delete(Guid id)
        {
            var reservation = _context.Reservations.FirstOrDefault(r => r.Id == id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                _context.SaveChanges();
            }
        }
    }
}