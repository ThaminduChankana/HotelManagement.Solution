using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using RoomService.Models;
using RoomService.Repositories;

namespace RoomService.Services
{
    // Implementation of the IRoomService interface for handling business logic related to room operations
    public class RoomServiceImpl : IRoomService
    {
        private readonly IRoomRepository _roomRepository;

        public RoomServiceImpl(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }
        // Retrieves a room by its ID
        public async Task<RoomDto?> GetRoomAsync(Guid id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            return room == null ? null : MapToDto(room);
        }
        // Retrieves a room by its name
        public async Task<RoomDto?> GetRoomByNameAsync(string name)
        {
            var room = await _roomRepository.GetByNameAsync(name);
            return room == null ? null : MapToDto(room);
        }
        // Retrieves all rooms from the database
        public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return rooms.Select(MapToDto);
        }
        // Creates a new room after validating the request data
        public async Task<RoomDto> CreateRoomAsync(CreateRoomRequest request)
        {
            // Validate request
            if (!ValidateRoomData(request))
                throw new InvalidOperationException("Invalid room data provided.");

            // Check for existing room name
            if (await _roomRepository.ExistsByNameAsync(request.Name))
                throw new InvalidOperationException("A room with this name already exists.");

            var room = new Room
            {
                Name = request.Name.Trim(),
                Features = request.Features,
                NumberOfGuests = request.NumberOfGuests,
                Price = request.Price,
                Discount = request.Discount,
                ImageUrls = request.ImageUrls,
                Options = request.Options,
                TotalRooms = request.TotalRooms,
                RoomNumbers = request.RoomNumbers.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
                BreakfastPrice = request.BreakfastPrice,
                LunchPrice = request.LunchPrice,
                DinnerPrice = request.DinnerPrice
            };

            var createdRoom = await _roomRepository.CreateAsync(room);
            return MapToDto(createdRoom);
        }
        // Updates an existing room's details
        public async Task<RoomDto> UpdateRoomAsync(Guid id, UpdateRoomRequest request)
        {
            var existingRoom = await _roomRepository.GetByIdAsync(id);
            if (existingRoom == null)
                throw new InvalidOperationException("Room not found.");

            // Check if name is being changed and if it already exists
            if (!existingRoom.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (await _roomRepository.ExistsByNameAsync(request.Name, id))
                    throw new InvalidOperationException("A room with this name already exists.");
            }

            // Update room properties
            existingRoom.Name = request.Name.Trim();
            existingRoom.Features = request.Features;
            existingRoom.NumberOfGuests = request.NumberOfGuests;
            existingRoom.Price = request.Price;
            existingRoom.Discount = request.Discount;
            existingRoom.ImageUrls = request.ImageUrls;
            existingRoom.Options = request.Options;
            existingRoom.TotalRooms = request.TotalRooms;
            existingRoom.RoomNumbers = request.RoomNumbers.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            existingRoom.BreakfastPrice = request.BreakfastPrice;
            existingRoom.LunchPrice = request.LunchPrice;
            existingRoom.DinnerPrice = request.DinnerPrice;

            var updatedRoom = await _roomRepository.UpdateAsync(existingRoom);
            return MapToDto(updatedRoom);
        }
        // Deletes a room by its ID
        public async Task<bool> DeleteRoomAsync(Guid id)
        {
            return await _roomRepository.DeleteAsync(id);
        }
        // Gets all available rooms for a specified check-in/check-out date range
        public async Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
        {
            var rooms = await _roomRepository.GetAvailableRoomsAsync(checkIn, checkOut);
            return rooms.Select(MapToDto);
        }
        // Gets the count of available rooms of a specific type within the given date range
        public async Task<int> GetAvailableCountAsync(Guid roomId, DateTime checkIn, DateTime checkOut)
        {
            return await _roomRepository.GetAvailableCountAsync(roomId, checkIn, checkOut);
        }
        // Validates the business rules for creating a room
        private bool ValidateRoomData(CreateRoomRequest request)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(request);

            bool isValid = Validator.TryValidateObject(request, context, validationResults, true);

            // Additional business logic validation
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3)
                return false;

            if (request.Price <= 0)
                return false;

            if (request.TotalRooms <= 0)
                return false;

            if (request.RoomNumbers.Count != request.TotalRooms)
                return false;

            return isValid;
        }
        // Maps a Room model to a RoomDto objec
        private static RoomDto MapToDto(Room room)
        {
            return new RoomDto
            {
                Id = room.Id,
                Name = room.Name,
                Features = room.Features,
                NumberOfGuests = room.NumberOfGuests,
                Price = room.Price,
                Discount = room.Discount,
                ImageUrls = room.ImageUrls,
                Options = room.Options,
                TotalRooms = room.TotalRooms,
                RoomNumbers = room.RoomNumbers,
                BreakfastPrice = room.BreakfastPrice,
                LunchPrice = room.LunchPrice,
                DinnerPrice = room.DinnerPrice,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt
            };
        }
    }
}