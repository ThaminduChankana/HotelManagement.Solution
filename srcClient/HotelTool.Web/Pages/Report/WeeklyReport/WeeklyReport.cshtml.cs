using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using HotelTool.Web.Models;

namespace HotelTool.Web.Pages.Reports;

public class WeeklyReportModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    // Constructor to inject HttpClientFactory and configure JSON deserialization
    public WeeklyReportModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // Represents summary data for a single day
    public class DailySummary
    {
        public DateTime Date { get; set; }
        public int BookingCount { get; set; }
        public decimal DailyIncome { get; set; }
        public List<string> SpecialRequests { get; set; } = new();
        public List<RequestWithRoom> RequestsWithRoomNumbers { get; set; } = new();

        public class RequestWithRoom
        {
            public string? SpecialRequest { get; set; }
            public string? RoomNumber { get; set; }
        }
    }

    // Properties bound to the Razor Page
    public DateTime StartOfWeek { get; set; }
    public DateTime EndOfWeek { get; set; }
    public List<DailySummary> DailySummaries { get; set; } = [];
    public decimal TotalWeeklyIncome => DailySummaries.Sum(d => d.DailyIncome);
    public string StatusMessage { get; set; } = string.Empty;

    // Function to get all the reservations that fall within the selected week
    public async Task OnGetAsync(DateTime? weekStartDate)
    {
        try
        {
            StartOfWeek = (weekStartDate ?? DateTime.Today).StartOfWeek(DayOfWeek.Monday);
            EndOfWeek = StartOfWeek.AddDays(6);

            await LoadWeeklyDataAsync();
        }
        catch (Exception)
        {
            StatusMessage = "Unable to load weekly report data at this time.";
            DailySummaries = [];
        }
    }

    // Core logic to load and structure the weekly report data
    private async Task LoadWeeklyDataAsync()
    {
        // Get all reservations from ReservationService
        var allReservations = await GetReservationsAsync();

        // Filter reservations for the selected week
        var weekReservations = allReservations
            .Where(r => r.CheckInDate.Date >= StartOfWeek && r.CheckInDate.Date <= EndOfWeek)
            .ToList();

        // Get unique room type IDs for the week
        var roomTypeIds = weekReservations.Select(r => r.RoomTypeId).Distinct().ToList();

        // Get room details from RoomService
        var roomCache = await GetRoomDetailsAsync(roomTypeIds);

        // Build daily summaries
        DailySummaries = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var date = StartOfWeek.AddDays(i);
                var dayReservations = weekReservations.Where(r => r.CheckInDate.Date == date).ToList();

                return new DailySummary
                {
                    Date = date,
                    BookingCount = dayReservations.Count,
                    DailyIncome = dayReservations.Sum(r => r.TotalCost),
                    RequestsWithRoomNumbers = dayReservations
                        .Where(r => !string.IsNullOrWhiteSpace(r.SpecialRequest))
                        .Select(r => new DailySummary.RequestWithRoom
                        {
                            SpecialRequest = r.SpecialRequest,
                            RoomNumber = r.AllocatedRoomNumber
                        }).ToList()
                };
            })
            .ToList();
    }

    // Fetches all reservations from the ReservationService API
    private async Task<List<Reservation>> GetReservationsAsync()
    {
        var client = _httpClientFactory.CreateClient("ReservationServiceAPI");
        var response = await client.GetAsync("/api/reservation");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var reservations = JsonSerializer.Deserialize<List<Reservation>>(json, _jsonOptions);
            return reservations ?? [];
        }

        throw new Exception("Failed to retrieve reservations from ReservationService");
    }

    // Fetches room details for a list of roomTypeIds using RoomService
    private async Task<Dictionary<Guid, Room>> GetRoomDetailsAsync(List<Guid> roomTypeIds)
    {
        var roomCache = new Dictionary<Guid, Room>();
        var roomClient = _httpClientFactory.CreateClient("RoomServiceAPI");

        // Use parallel processing to fetch multiple rooms simultaneously for better performance
        var roomTasks = roomTypeIds.Select(async roomId =>
        {
            try
            {
                var response = await roomClient.GetAsync($"/api/rooms/{roomId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var room = JsonSerializer.Deserialize<Room>(json, _jsonOptions);
                    if (room != null)
                    {
                        return new KeyValuePair<Guid, Room>(roomId, room);
                    }
                }
            }
            catch (Exception)
            {
                // Skip failed room requests and continue with others
            }
            return (KeyValuePair<Guid, Room>?)null;
        });

        var roomResults = await Task.WhenAll(roomTasks);

        foreach (var result in roomResults.Where(r => r.HasValue))
        {
            roomCache[result.Value.Key] = result.Value.Value;
        }

        return roomCache;
    }
}