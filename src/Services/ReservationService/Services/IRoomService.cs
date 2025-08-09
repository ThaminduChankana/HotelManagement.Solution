using ReservationService.DTOs;

namespace ReservationService.Services
{
    // Interface for the room operations in the reservation service
    public interface IRoomService
    {
        Task<RoomDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<RoomDto>> GetAllAsync();
        Task<bool> ExistsAsync(Guid roomTypeId);
        Task<IEnumerable<RoomDto>> GetByIdsAsync(IEnumerable<Guid> roomIds);
    }
}
