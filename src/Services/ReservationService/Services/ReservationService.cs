using ReservationService.DTOs;
using ReservationService.Models;
using ReservationService.Repositories;

namespace ReservationService.Services
{
    // Handles business logic related to reservations
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IRoomService _roomService;

        public ReservationService(IReservationRepository reservationRepository, IRoomService roomService)
        {
            _reservationRepository = reservationRepository;
            _roomService = roomService;
        }

        // Get all reservations
        public IEnumerable<Reservation> GetAll()
        {
            return _reservationRepository.GetAll();
        }

        // Get reservation by ID
        public Reservation? GetById(Guid id)
        {
            return _reservationRepository.GetById(id);
        }

        // Get all reservations by user ID
        public IEnumerable<Reservation> GetByUserId(string userId)
        {
            return _reservationRepository.GetByUserId(userId);
        }

        // Creates one or more reservations, considering recurrence and availability
        public async Task<Reservation> CreateAsync(CreateReservationRequest request)
        {
            // Validate room exists
            var room = await _roomService.GetByIdAsync(request.RoomTypeId);
            if (room == null)
                throw new InvalidOperationException("Room type not found");

            // Handle recurrence
            var reservations = new List<Reservation>();
            int count = request.RecurrenceCount > 0 ? request.RecurrenceCount : 1;

            for (int i = 0; i < count; i++)
            {
                var checkIn = CalculateCheckInDate(request.CheckInDate, request.Recurrence, i);
                var checkOut = CalculateCheckOutDate(request.CheckOutDate, request.Recurrence, i);

                // Check availability
                if (!await IsRoomTypeAvailableAsync(request.RoomTypeId, checkIn, checkOut, request.FullOrHalfBoard))
                {
                    throw new InvalidOperationException($"Room is not available for {checkIn:yyyy-MM-dd} to {checkOut:yyyy-MM-dd}");
                }

                var reservation = CreateSingleReservation(request, room, checkIn, checkOut);
                _reservationRepository.Add(reservation);
                reservations.Add(reservation);
            }

            return reservations.First(); // Return the first reservation created
        }

        // Creates a reservation object with calculated cost and room number.
        private Reservation CreateSingleReservation(CreateReservationRequest request, RoomDto room, DateTime checkIn, DateTime checkOut)
        {
            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                UserID = request.UserID,
                RoomTypeId = room.Id,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Country = request.Country.Trim(),
                BookFor = request.BookFor,
                IsWorkRelated = request.IsWorkRelated,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                PayBy = request.PayBy,
                FullOrHalfBoard = request.FullOrHalfBoard,
                SpecialRequest = request.SpecialRequest?.Trim() ?? string.Empty,
                Recurrence = request.Recurrence,
                RecurrenceCount = request.RecurrenceCount,
                Status = ReservationStatus.Active
            };

            // Allocate room number
            AllocateRoomNumber(reservation, room);

            // Calculate total cost
            CalculateTotalCost(reservation, room);

            return reservation;
        }

        // Allocates an available room number for a reservation
        private void AllocateRoomNumber(Reservation reservation, RoomDto room)
        {
            var overlappingReservations = _reservationRepository.GetOverlappingReservations(
                reservation.RoomTypeId,
                reservation.CheckInDate,
                reservation.CheckOutDate
            );

            var occupiedRoomNumbers = overlappingReservations
                .Where(r => r.Status != ReservationStatus.Canceled && r.AllocatedRoomNumber != null)
                .Select(r => r.AllocatedRoomNumber)
                .ToHashSet();

            var availableRoomNumber = room.RoomNumbers.FirstOrDefault(rn => !occupiedRoomNumbers.Contains(rn));
            if (availableRoomNumber == null)
            {
                throw new InvalidOperationException("No available room numbers for the selected type and dates.");
            }

            reservation.AllocatedRoomNumber = availableRoomNumber;
        }

        // Calculates the total cost based on board type, duration, and discounts
        private void CalculateTotalCost(Reservation reservation, RoomDto room)
        {
            int numberOfNights = (reservation.CheckOutDate.Date - reservation.CheckInDate.Date).Days;
            if (numberOfNights <= 0)
                numberOfNights = 1;

            decimal roomPricePerNight;
            decimal mealCostPerNight;

            if (numberOfNights == 1)
            {
                roomPricePerNight = room.BasePriceAfterDiscount;
                mealCostPerNight = room.GetPriceAfterDiscountWithBoard(reservation.FullOrHalfBoard);
                reservation.TotalCost = (roomPricePerNight + mealCostPerNight) * numberOfNights;
            }
            else
            {
                mealCostPerNight = room.DinnerPrice + room.BreakfastPrice +
                    (numberOfNights - 1) * (room.BreakfastPrice + room.LunchPrice + room.DinnerPrice);
                roomPricePerNight = room.BasePriceAfterDiscount * numberOfNights;
                reservation.TotalCost = mealCostPerNight + roomPricePerNight;
            }
        }

        // Updates an existing reservation if found and valid
        public async Task<Reservation?> UpdateAsync(Guid id, UpdateReservationRequest request)
        {
            var existing = _reservationRepository.GetById(id);
            if (existing == null)
                return null;

            var room = await _roomService.GetByIdAsync(request.RoomTypeId);
            if (room == null)
                throw new InvalidOperationException("Room type not found");

            // Check availability (exclude current reservation) 
            if (!await IsRoomTypeAvailableAsync(request.RoomTypeId, request.CheckInDate, request.CheckOutDate, request.FullOrHalfBoard, id))
            {
                throw new InvalidOperationException("Selected room type is not available for the new dates.");
            }

            // Update properties
            existing.FirstName = request.FirstName.Trim();
            existing.LastName = request.LastName.Trim();
            existing.Email = request.Email.Trim();
            existing.PhoneNumber = request.PhoneNumber.Trim();
            existing.Country = request.Country.Trim();
            existing.RoomTypeId = request.RoomTypeId;
            existing.FullOrHalfBoard = request.FullOrHalfBoard;
            existing.CheckInDate = request.CheckInDate;
            existing.CheckOutDate = request.CheckOutDate;
            existing.SpecialRequest = request.SpecialRequest?.Trim() ?? string.Empty;
            existing.PayBy = request.PayBy;
            existing.IsWorkRelated = request.IsWorkRelated;
            existing.BookFor = request.BookFor;

            // Recalculate total cost
            CalculateTotalCost(existing, room);

            _reservationRepository.Update(existing);
            return existing;
        }

        // Updates the status and admin note of a reservation
        public bool UpdateStatus(Guid id, ReservationStatus status, string? adminNote)
        {
            var reservation = _reservationRepository.GetById(id);
            if (reservation == null)
                return false;

            reservation.Status = status;
            reservation.AdminNote = adminNote?.Trim() ?? string.Empty;
            _reservationRepository.Update(reservation);
            return true;
        }

        // Cancels a reservation if it's more than 48 hours before check-in
        public CancellationResult CancelReservation(Guid id)
        {
            var reservation = _reservationRepository.GetById(id);
            if (reservation == null)
                return new CancellationResult { Success = false, Message = "Reservation not found." };

            // Check if cancellation is allowed (48 hours before check-in)
            if ((reservation.CheckInDate - DateTime.Now).TotalHours < 48)
            {
                return new CancellationResult { Success = false, Message = "Cannot cancel within 48 hours of check-in." };
            }

            reservation.Status = ReservationStatus.Canceled;
            _reservationRepository.Update(reservation);
            return new CancellationResult { Success = true, Message = "Reservation canceled successfully." };
        }

        // Returns availability and room count for the given parameters
        public async Task<AvailabilityResponse> CheckAvailabilityAsync(AvailabilityRequest request)
        {
            var isAvailable = await IsRoomTypeAvailableAsync(request.RoomTypeId, request.CheckInDate, request.CheckOutDate, request.BoardType, request.ExcludeReservationId);
            var availableCount = await GetAvailableCountAsync(request.RoomTypeId, request.CheckInDate, request.CheckOutDate);

            return new AvailabilityResponse
            {
                IsAvailable = isAvailable,
                AvailableRoomCount = availableCount
            };
        }

        // Returns the count of available rooms for the selected type and date range
        public async Task<int> GetAvailableCountAsync(Guid roomTypeId, DateTime checkIn, DateTime checkOut)
        {
            var room = await _roomService.GetByIdAsync(roomTypeId);
            if (room == null)
                return 0;

            var overlapping = _reservationRepository.GetOverlappingReservations(roomTypeId, checkIn, checkOut)
                .Count(r => r.Status != ReservationStatus.Canceled);

            return room.TotalRooms - overlapping;
        }

        // Determines if a room type is available for all days between check-in and check-out
        private async Task<bool> IsRoomTypeAvailableAsync(Guid roomTypeId, DateTime checkIn, DateTime checkOut, string boardType, Guid? excludeReservationId = null)
        {
            var room = await _roomService.GetByIdAsync(roomTypeId);
            Console.WriteLine("This is room");
            Console.WriteLine(room);
            Console.WriteLine(roomTypeId);
            Console.WriteLine(checkIn);
            Console.WriteLine(checkOut);
            Console.WriteLine(boardType);
            if (room == null) return false;

            var reservations = _reservationRepository.GetOverlappingReservations(roomTypeId, checkIn, checkOut)
                .Where(r => r.Status != ReservationStatus.Canceled && r.Id != excludeReservationId)
                .ToList();

            int totalRooms = room.TotalRooms;

            // Check for each day if enough rooms are available
            for (DateTime day = checkIn.Date; day < checkOut.Date; day = day.AddDays(1))
            {
                DateTime blockStart, blockEnd;
                bool isFirstDay = day == checkIn.Date;
                bool isLastDay = day == checkOut.Date.AddDays(-1);

                if (isFirstDay)
                {
                    blockStart = boardType == "Full Board" ? day.AddHours(19) : day.AddHours(14);
                    blockEnd = day.AddDays(1);
                }
                else if (isLastDay)
                {
                    blockStart = day;
                    blockEnd = boardType == "Full Board" ? checkOut.Date.AddHours(12) : checkOut.Date.AddHours(8);
                }
                else
                {
                    blockStart = day;
                    blockEnd = day.AddDays(1);
                }

                // Count overlapping reservations for this day
                int overlappingCount = reservations.Count(existing =>
                {
                    DateTime existingStart, existingEnd;
                    var existingCheckIn = existing.CheckInDate.Date;
                    var existingCheckOut = existing.CheckOutDate.Date;

                    if (day == existingCheckIn)
                        existingStart = existing.FullOrHalfBoard == "Full Board" ? existingCheckIn.AddHours(19) : existingCheckIn.AddHours(14);
                    else
                        existingStart = existingCheckIn;

                    if (day == existingCheckOut.AddDays(-1))
                        existingEnd = existing.FullOrHalfBoard == "Full Board" ? existingCheckOut.AddHours(12) : existingCheckOut.AddHours(8);
                    else
                        existingEnd = existingCheckOut;

                    return blockStart < existingEnd && blockEnd > existingStart;
                });

                if (overlappingCount >= totalRooms)
                    return false;
            }

            return true;
        }

        //  Helper Methods for Recurrence to get checkIn Date 
        private DateTime CalculateCheckInDate(DateTime baseDate, RecurrenceType recurrence, int iteration)
        {
            if (iteration == 0) return baseDate;

            return recurrence switch
            {
                RecurrenceType.Daily => baseDate.AddDays(iteration),
                RecurrenceType.Weekly => baseDate.AddDays(iteration * 7),
                RecurrenceType.Monthly => baseDate.AddMonths(iteration),
                _ => baseDate
            };
        }
        //  Helper Methods for Recurrence to get checkOut Date 
        private DateTime CalculateCheckOutDate(DateTime baseDate, RecurrenceType recurrence, int iteration)
        {
            if (iteration == 0) return baseDate;

            return recurrence switch
            {
                RecurrenceType.Daily => baseDate.AddDays(iteration),
                RecurrenceType.Weekly => baseDate.AddDays(iteration * 7),
                RecurrenceType.Monthly => baseDate.AddMonths(iteration),
                _ => baseDate
            };
        }
    }
}

