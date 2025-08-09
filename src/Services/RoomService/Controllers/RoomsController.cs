using Microsoft.AspNetCore.Mvc;
using RoomService.Models;
using RoomService.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace RoomService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        // Inject the IRoomService via constructor
        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // GET: api/rooms
        // Get all rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return Ok(rooms);
        }

        // GET: api/rooms/{id}
        // Get a room by its ID
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDto>> GetRoom(Guid id)
        {
            var room = await _roomService.GetRoomAsync(id);
            if (room == null)
                return NotFound($"Room with ID {id} not found.");

            return Ok(room);
        }

        // GET: api/rooms/name/{name}
        // Get a room by its name
        [HttpGet("name/{name}")]
        public async Task<ActionResult<RoomDto>> GetRoomByName(string name)
        {
            var room = await _roomService.GetRoomByNameAsync(name);
            if (room == null)
                return NotFound($"Room with name '{name}' not found.");

            return Ok(room);
        }

        // POST: api/rooms
        // Create a new room
        [HttpPost]
        public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var room = await _roomService.CreateRoomAsync(request);
                return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/rooms/{id}
        // Update an existing room
        [HttpPut("{id}")]
        public async Task<ActionResult<RoomDto>> UpdateRoom(Guid id, [FromBody] UpdateRoomRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var room = await _roomService.UpdateRoomAsync(id, request);
                return Ok(room);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/rooms/{id}
        // Delete a room by its ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRoom(Guid id)
        {
            var result = await _roomService.DeleteRoomAsync(id);
            if (!result)
                return NotFound($"Room with ID {id} not found.");

            return NoContent();
        }

        // GET: api/rooms/available?checkIn={date}&checkOut={date}
        // Get rooms available for a given date range
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAvailableRooms(
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut)
        {
            var rooms = await _roomService.GetAvailableRoomsAsync(checkIn, checkOut);
            return Ok(rooms);
        }

        // GET: api/rooms/{id}/availability?checkIn={date}&checkOut={date}
        // Get available count of a specific room for a given date range
        [HttpGet("{id}/availability")]
        public async Task<ActionResult<int>> GetAvailableCount(
            Guid id,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut)
        {
            var count = await _roomService.GetAvailableCountAsync(id, checkIn, checkOut);
            return Ok(new { AvailableCount = count });
        }

        // GET: api/rooms/test-db
        // Test the database connection and return diagnostic info
        [HttpGet("test-db")]
        public async Task<ActionResult> TestDatabase([FromServices] Data.RoomContext context)
        {
            try
            {
                var canConnect = await context.Database.CanConnectAsync();
                var roomCount = await context.Rooms.CountAsync();
                var databaseName = context.Database.GetDbConnection().Database;

                return Ok(new
                {
                    CanConnect = canConnect,
                    RoomCount = roomCount,
                    DatabaseName = databaseName,
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }
    }
}