using Microsoft.AspNetCore.Mvc;
using ReservationService.Models;
using ReservationService.Services;
using ReservationService.DTOs;

namespace ReservationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        // GET: api/reservation
        // Returns all reservations
        [HttpGet]
        public ActionResult<IEnumerable<Reservation>> GetAll()
        {
            try
            {
                var reservations = _reservationService.GetAll();
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/reservation/{id}
        // Returns a specific reservation by ID
        [HttpGet("{id}")]
        public ActionResult<Reservation> GetById(Guid id)
        {
            try
            {
                var reservation = _reservationService.GetById(id);
                if (reservation == null)
                    return NotFound(new { message = "Reservation not found" });

                return Ok(reservation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/reservation/user/{userId}
        // Returns reservations for a specific user
        [HttpGet("user/{userId}")]
        public ActionResult<IEnumerable<Reservation>> GetByUserId(string userId)
        {
            try
            {
                var reservations = _reservationService.GetByUserId(userId);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // POST: api/reservation
        // Creates a new reservation
        [HttpPost]
        public async Task<ActionResult<Reservation>> Create([FromBody] CreateReservationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var reservation = await _reservationService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // PUT: api/reservation/{id}
        // Updates an existing reservation
        [HttpPut("{id}")]
        public async Task<ActionResult<Reservation>> Update(Guid id, [FromBody] UpdateReservationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var reservation = await _reservationService.UpdateAsync(id, request);
                if (reservation == null)
                    return NotFound(new { message = "Reservation not found" });

                return Ok(reservation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // PATCH: api/reservation/{id}/status
        // Updates the status of a reservation
        [HttpPatch("{id}/status")]
        public ActionResult UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var result = _reservationService.UpdateStatus(id, request.Status, request.AdminNote);
                if (!result)
                    return NotFound(new { message = "Reservation not found" });

                return Ok(new { message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // PATCH: api/reservation/{id}/cancel
        // Cancels a reservation
        [HttpPatch("{id}/cancel")]
        public ActionResult CancelReservation(Guid id)
        {
            try
            {
                var result = _reservationService.CancelReservation(id);
                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/reservation/availability
        // Checks availability for a given room type and date range
        [HttpGet("availability")]
        public async Task<ActionResult<AvailabilityResponse>> CheckAvailability([FromQuery] AvailabilityRequest request)
        {
            try
            {
                var availability = await _reservationService.CheckAvailabilityAsync(request);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/reservation/available-count
        // Gets the number of available rooms for a room type in a given date range
        [HttpGet("available-count")]
        public async Task<ActionResult<int>> GetAvailableCount([FromQuery] Guid roomTypeId, [FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut)
        {
            try
            {
                var count = await _reservationService.GetAvailableCountAsync(roomTypeId, checkIn, checkOut);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}

namespace ReservationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        // GET: api/health
        // Simple health check endpoint to verify the service is up
        [HttpGet]
        public ActionResult<object> GetHealth()
        {
            return Ok(new
            {
                service = "ReservationService",
                status = "healthy",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
