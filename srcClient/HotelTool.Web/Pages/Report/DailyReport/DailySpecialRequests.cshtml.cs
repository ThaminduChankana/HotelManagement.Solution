using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPDF.Fluent;
using System.Text.Json;
using HotelTool.Web.Models;

namespace HotelTool.Web.Pages.Reports;

public class DailySpecialRequestsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    // Constructor: initializes dependencies
    public DailySpecialRequestsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // Model representing a single special request entry for the report
    public class RequestItem
    {
        public string GuestName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string RoomType { get; set; } = "";
        public string SpecialRequest { get; set; } = "";
        public string AllocatedRoomNumber { get; set; } = "";
    }


    // Date selected for generating the report, defaulting to today
    [BindProperty(SupportsGet = true)]
    public DateTime SelectedDate { get; set; } = DateTime.Today;
    // Final list of requests shown in the report or PDF
    public List<RequestItem> Requests { get; set; } = [];
    // TempData to show status messages
    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    // Handles the GET request to load special requests for the selected date
    public async Task OnGetAsync()
    {
        try
        {
            await LoadRequestsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = "Unable to load special requests at this time.";
            Requests = [];
        }
    }

    // Handles the GET request to export the special requests to a PDF file
    public async Task<IActionResult> OnGetExportPdfAsync()
    {
        try
        {
            await LoadRequestsAsync();

            var document = new DailyRequestsDocument(Requests, SelectedDate);
            var pdf = document.GeneratePdf();

            return File(pdf, "application/pdf", $"DailyRequests_{SelectedDate:yyyyMMdd}.pdf");
        }
        catch (Exception)
        {
            StatusMessage = "Unable to generate PDF at this time.";
            return RedirectToPage();
        }
    }

    // Loads and prepares the data for display or export
    private async Task LoadRequestsAsync()
    {
        // Get all reservations from ReservationService
        var reservations = await GetReservationsAsync();

        // Filter reservations for selected date with special requests
        var filteredReservations = reservations
            .Where(r => r.CheckInDate.Date == SelectedDate && !string.IsNullOrWhiteSpace(r.SpecialRequest))
            .ToList();

        // Get unique room type IDs
        var roomTypeIds = filteredReservations.Select(r => r.RoomTypeId).Distinct().ToList();

        // Get room details from RoomService
        var roomCache = await GetRoomDetailsAsync(roomTypeIds);

        // Build the request items
        Requests = filteredReservations.Select(r => new RequestItem
        {
            GuestName = $"{r.FirstName} {r.LastName}",
            Phone = r.PhoneNumber,
            RoomType = roomCache.TryGetValue(r.RoomTypeId, out var room) ? room.Name : "Unknown",
            SpecialRequest = r.SpecialRequest,
            AllocatedRoomNumber = r.AllocatedRoomNumber ?? "Not Assigned"
        }).ToList();
    }

    // Retrieves all reservations from the ReservationService API
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

    // Retrieves room details for a given list of roomTypeIds
    private async Task<Dictionary<Guid, Room>> GetRoomDetailsAsync(List<Guid> roomTypeIds)
    {
        var roomCache = new Dictionary<Guid, Room>();
        var roomClient = _httpClientFactory.CreateClient("RoomServiceAPI");

        // Use parallel processing to fetch multiple rooms simultaneously
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
                // Skip failed room requests
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