using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoomService.Models;

namespace RoomService.Repositories
{
    // Interface that defines the contract for room-related data access operations
    public interface IRoomRepository
    {
        Task<Room?> GetByIdAsync(Guid id);
        Task<Room?> GetByNameAsync(string name);
        Task<IEnumerable<Room>> GetAllAsync();
        Task<Room> CreateAsync(Room room);
        Task<Room> UpdateAsync(Room room);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByNameAsync(string name, Guid excludeId);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut);
        Task<int> GetAvailableCountAsync(Guid roomId, DateTime checkIn, DateTime checkOut);
    }
}