using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoomService.Models;

namespace RoomService.Services
{
    // Interface defining the contract for room-related operations such as creating, retrieving, updating, and deleting rooms
    public interface IRoomService
    {
        Task<RoomDto?> GetRoomAsync(Guid id);
        Task<RoomDto?> GetRoomByNameAsync(string name);
        Task<IEnumerable<RoomDto>> GetAllRoomsAsync();
        Task<RoomDto> CreateRoomAsync(CreateRoomRequest request);
        Task<RoomDto> UpdateRoomAsync(Guid id, UpdateRoomRequest request);
        Task<bool> DeleteRoomAsync(Guid id);
        Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut);
        Task<int> GetAvailableCountAsync(Guid roomId, DateTime checkIn, DateTime checkOut);
    }
}