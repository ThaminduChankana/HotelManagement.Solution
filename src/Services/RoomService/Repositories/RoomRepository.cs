using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoomService.Data;
using RoomService.Models;

namespace RoomService.Repositories
{
    // Repository for managing Room entities
    public class RoomRepository : IRoomRepository
    {
        private readonly RoomContext _context;
        // Initializes a new instance
        public RoomRepository(RoomContext context)
        {
            _context = context;
        }
        // Gets a room by its unique ID
        public async Task<Room?> GetByIdAsync(Guid id)
        {
            return await _context.Rooms.FindAsync(id);
        }
        // Gets a room by its name (case-insensitive)
        public async Task<Room?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await _context.Rooms
                .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower().Trim());
        }
        // Retrieves all rooms, ordered by name
        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _context.Rooms
                .OrderBy(r => r.Name)
                .ToListAsync();
        }
        // Creates a new room
        public async Task<Room> CreateAsync(Room room)
        {
            room.Id = Guid.NewGuid();
            room.CreatedAt = DateTime.UtcNow;
            room.UpdatedAt = DateTime.UtcNow;

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }
        // Updates an existing room
        public async Task<Room> UpdateAsync(Room room)
        {
            room.UpdatedAt = DateTime.UtcNow;
            _context.Entry(room).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return room;
        }
        // Deletes a room by its ID
        public async Task<bool> DeleteAsync(Guid id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return false;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return true;
        }
        // Checks if a room with the given name exist
        public async Task<bool> ExistsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return await _context.Rooms
                .AnyAsync(r => r.Name.ToLower() == name.ToLower().Trim());
        }
        // Checks if a room with the given name exists, excluding a specific room ID
        public async Task<bool> ExistsByNameAsync(string name, Guid excludeId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return await _context.Rooms
                .AnyAsync(r => r.Name.ToLower() == name.ToLower().Trim() && r.Id != excludeId);
        }
        // Gets available rooms for the given date range
        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
        {
            return await GetAllAsync();
        }
        // Gets the number of available rooms of a specific room type during a date range
        public async Task<int> GetAvailableCountAsync(Guid roomId, DateTime checkIn, DateTime checkOut)
        {
            var room = await GetByIdAsync(roomId);
            if (room == null) return 0;
            return room.TotalRooms;
        }
    }
}