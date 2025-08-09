using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using HotelTool.Web.Models;

namespace HotelTool.Web.Pages.ChatBot;

public class ChatBotModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<ChatBotModel> _logger;

    // Initializes the ChatBot model with dependency injection 
    public ChatBotModel(IHttpClientFactory httpClientFactory, ILogger<ChatBotModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        // Configure JSON serialization for consistent microservice communication
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // JSON string containing room data from Room Service for client-side chatbot processing
    public string RoomsJson { get; set; } = "";
    // JSON string containing reservation data from Reservation Service for client-side chatbot processing
    public string ReservationsJson { get; set; } = "";
    // Status message for displaying any microservice connectivity issues
    public string StatusMessage { get; set; } = "";
    // Flag indicating if data was successfully loaded from microservices
    public bool IsDataLoaded { get; set; } = false;

    // Handles GET requests by fetching data 
    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Starting to load chatbot data from microservices");

            // Use parallel processing to fetch data 
            var roomsTask = GetRoomsFromMicroserviceAsync();
            var reservationsTask = GetReservationsFromMicroserviceAsync();

            // Wait for both microservice calls to complete concurrently
            await Task.WhenAll(roomsTask, reservationsTask);

            // Get results from completed tasks
            var rooms = await roomsTask;
            var reservations = await reservationsTask;

            // Transform and serialize data for client-side consumption
            await ProcessAndSerializeDataAsync(rooms, reservations);

            IsDataLoaded = true;
            _logger.LogInformation("Successfully loaded chatbot data from microservices - {RoomCount} rooms, {ReservationCount} reservations",
                rooms?.Count ?? 0, reservations?.Count ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load chatbot data from microservices");
            StatusMessage = "Unable to load hotel data at this time. Please try again later.";
            IsDataLoaded = false;

            // Provide empty JSON arrays to prevent client-side errors
            RoomsJson = "[]";
            ReservationsJson = "[]";
        }
    }

    // Fetches room data from the Room microservice
    private async Task<List<Room>> GetRoomsFromMicroserviceAsync()
    {
        try
        {
            _logger.LogDebug("Fetching rooms from Room microservice");

            // Create HTTP client configured for Room Service
            var roomClient = _httpClientFactory.CreateClient("RoomServiceAPI");

            // Make async HTTP call to Room Service - non-blocking I/O operation
            var response = await roomClient.GetAsync("/api/rooms");

            if (response.IsSuccessStatusCode)
            {
                // Read response content asynchronously
                var json = await response.Content.ReadAsStringAsync();

                // Deserialize JSON response into strongly-typed objects
                var rooms = JsonSerializer.Deserialize<List<Room>>(json, _jsonOptions);

                _logger.LogDebug("Successfully retrieved {Count} rooms from Room Service", rooms?.Count ?? 0);
                return rooms ?? new List<Room>();
            }
            else
            {
                _logger.LogWarning("Room Service returned non-success status: {StatusCode} - {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);
                throw new Exception($"Room Service returned status: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while communicating with Room Service");
            throw new Exception("Failed to communicate with Room Service", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while communicating with Room Service");
            throw new Exception("Room Service request timed out", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for Room Service response");
            throw new Exception("Invalid response format from Room Service", ex);
        }
    }


    // Fetches reservation data from the Reservation microservice
    private async Task<List<Reservation>> GetReservationsFromMicroserviceAsync()
    {
        try
        {
            _logger.LogDebug("Fetching reservations from Reservation microservice");

            // Create HTTP client configured for Reservation Service
            var reservationClient = _httpClientFactory.CreateClient("ReservationServiceAPI");

            // Make async HTTP call to Reservation Service 
            var response = await reservationClient.GetAsync("/api/reservation");

            if (response.IsSuccessStatusCode)
            {
                // Read response content asynchronously
                var json = await response.Content.ReadAsStringAsync();

                // Deserialize JSON response into strongly-typed objects
                var reservations = JsonSerializer.Deserialize<List<Reservation>>(json, _jsonOptions);

                _logger.LogDebug("Successfully retrieved {Count} reservations from Reservation Service", reservations?.Count ?? 0);
                return reservations ?? new List<Reservation>();
            }
            else
            {
                _logger.LogWarning("Reservation Service returned non-success status: {StatusCode} - {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);
                throw new Exception($"Reservation Service returned status: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while communicating with Reservation Service");
            throw new Exception("Failed to communicate with Reservation Service", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while communicating with Reservation Service");
            throw new Exception("Reservation Service request timed out", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for Reservation Service response");
            throw new Exception("Invalid response format from Reservation Service", ex);
        }
    }

    // Processes and serializes microservice data for client-side consumption
    private async Task ProcessAndSerializeDataAsync(List<Room> rooms, List<Reservation> reservations)
    {
        // Use Task.Run for CPU-intensive serialization to avoid blocking the request thread
        // This demonstrates proper threading for both I/O and CPU-bound operations
        var roomsJsonTask = Task.Run(() =>
        {
            _logger.LogDebug("Processing and serializing room data for chatbot consumption");

            // Transform room data to optimize it for chatbot processing
            var roomsForChatbot = rooms.Select(r => new
            {
                // Basic room information
                id = r.Id.ToString(),
                name = r.Name,
                features = r.Features ?? new List<string>(),
                numberOfGuests = r.NumberOfGuests,
                price = r.Price,
                discount = r.Discount,
                totalRooms = r.TotalRooms,
                roomNumbers = r.RoomNumbers ?? new List<string>(),

                // Meal pricing information
                breakfastPrice = r.BreakfastPrice,
                lunchPrice = r.LunchPrice,
                dinnerPrice = r.DinnerPrice,

                // Computed properties for easier chatbot processing
                basePriceAfterDiscount = r.BasePriceAfterDiscount,
                isAvailable = r.TotalRooms > 0,
                priceRange = GetPriceRange(r.BasePriceAfterDiscount),

                // Enhanced descriptions for better chatbot responses
                description = GenerateRoomDescription(r),
                amenitiesText = string.Join(", ", r.Features ?? new List<string>()),
                capacityText = $"Accommodates {r.NumberOfGuests} guests",

                // Pricing breakdown for detailed responses
                pricing = new
                {
                    basePrice = r.Price,
                    discountPercentage = r.Discount,
                    finalPrice = r.BasePriceAfterDiscount,
                    breakfast = r.BreakfastPrice,
                    lunch = r.LunchPrice,
                    dinner = r.DinnerPrice,
                    fullBoardTotal = r.BasePriceAfterDiscount + r.BreakfastPrice + r.LunchPrice + r.DinnerPrice,
                    halfBoardTotal = r.BasePriceAfterDiscount + r.BreakfastPrice + r.DinnerPrice
                }
            }).ToList();

            return JsonSerializer.Serialize(roomsForChatbot, _jsonOptions);
        });

        var reservationsJsonTask = Task.Run(() =>
        {
            _logger.LogDebug("Processing and serializing reservation data for chatbot consumption");

            // Transform reservation data to optimize it for chatbot processing
            var reservationsForChatbot = reservations.Select(r => new
            {
                // Basic reservation information
                id = r.Id.ToString(),
                roomTypeId = r.RoomTypeId.ToString(),

                // Guest information (formatted for privacy and readability)
                guestName = $"{r.FirstName} {r.LastName}",
                firstName = r.FirstName,
                lastName = r.LastName,
                email = r.Email,
                country = r.Country,
                phoneNumber = r.PhoneNumber,
                bookFor = r.BookFor,
                isWorkRelated = r.IsWorkRelated,

                // Date information (multiple formats for flexibility)
                checkInDate = r.CheckInDate.ToString("yyyy-MM-dd"),
                checkOutDate = r.CheckOutDate.ToString("yyyy-MM-dd"),
                checkInDateFormatted = r.CheckInDate.ToString("MMMM dd, yyyy"),
                checkOutDateFormatted = r.CheckOutDate.ToString("MMMM dd, yyyy"),
                checkInDateShort = r.CheckInDate.ToString("MMM dd"),
                checkOutDateShort = r.CheckOutDate.ToString("MMM dd"),

                // Financial information
                payBy = r.PayBy,
                fullOrHalfBoard = r.FullOrHalfBoard,
                totalCost = r.TotalCost,
                totalCostFormatted = $"LKR {r.TotalCost:N0}",

                // Special requests and notes
                specialRequest = r.SpecialRequest,
                adminNote = r.AdminNote,
                allocatedRoomNumber = r.AllocatedRoomNumber,

                // Status information (multiple formats)
                status = r.Status.ToString(),
                statusFormatted = FormatReservationStatus(r.Status),

                // Computed properties for chatbot logic
                durationNights = (r.CheckOutDate - r.CheckInDate).Days,
                hasSpecialRequest = !string.IsNullOrWhiteSpace(r.SpecialRequest),
                hasRoomAssignment = !string.IsNullOrWhiteSpace(r.AllocatedRoomNumber),

                // Time-based flags for easy filtering
                isUpcoming = r.CheckInDate.Date > DateTime.Today,
                isActive = r.CheckInDate.Date <= DateTime.Today && r.CheckOutDate.Date > DateTime.Today,
                isPast = r.CheckOutDate.Date <= DateTime.Today,
                isToday = r.CheckInDate.Date == DateTime.Today || r.CheckOutDate.Date == DateTime.Today,

                // Days calculations for chatbot responses
                daysUntilCheckIn = (r.CheckInDate.Date - DateTime.Today).Days,
                daysUntilCheckOut = (r.CheckOutDate.Date - DateTime.Today).Days,

                // Natural language descriptions
                timeStatus = GetTimeStatus(r.CheckInDate, r.CheckOutDate),
                durationText = $"{(r.CheckOutDate - r.CheckInDate).Days} nights",
                dateRangeText = $"{r.CheckInDate:MMM dd} - {r.CheckOutDate:MMM dd, yyyy}"
            }).ToList();

            return JsonSerializer.Serialize(reservationsForChatbot, _jsonOptions);
        });

        // Wait for both serialization tasks to complete
        await Task.WhenAll(roomsJsonTask, reservationsJsonTask);

        // Set the final JSON strings for client consumption
        RoomsJson = await roomsJsonTask;
        ReservationsJson = await reservationsJsonTask;

        _logger.LogDebug("Successfully processed and serialized data for chatbot consumption");
    }

    // Generates a comprehensive description for rooms to enhance chatbot responses
    private static string GenerateRoomDescription(Room room)
    {
        var features = room.Features?.Any() == true
            ? string.Join(", ", room.Features)
            : "Standard hotel amenities";

        var discountText = room.Discount > 0
            ? $" (discounted from LKR {room.Price:N0})"
            : "";

        return $"{room.Name} is designed for {room.NumberOfGuests} guests and features {features}. " +
               $"Priced at LKR {room.BasePriceAfterDiscount:N0} per night{discountText}. " +
               $"We have {room.TotalRooms} rooms of this type available.";
    }

    // Determines price range category for room pricing
    private static string GetPriceRange(decimal price)
    {
        return price switch
        {
            < 5000 => "Budget",
            < 10000 => "Standard",
            < 20000 => "Premium",
            _ => "Luxury"
        };
    }

    // Formats reservation status for user-friendly display
    private static string FormatReservationStatus(ReservationStatus status)
    {
        return status switch
        {
            ReservationStatus.Active => "Confirmed",
            ReservationStatus.Canceled => "Cancelled",
            ReservationStatus.CheckedIn => "Currently Checked In",
            ReservationStatus.Completed => "Stay Completed",
            _ => status.ToString()
        };
    }

    // Determines the time status of a reservation relative to today
    private static string GetTimeStatus(DateTime checkIn, DateTime checkOut)
    {
        var today = DateTime.Today;

        if (checkOut.Date < today)
            return "Past stay";

        if (checkIn.Date > today)
            return "Upcoming";

        if (checkIn.Date <= today && checkOut.Date > today)
            return "Currently staying";

        if (checkIn.Date == today)
            return "Checking in today";

        if (checkOut.Date == today)
            return "Checking out today";

        return "Active";
    }

}