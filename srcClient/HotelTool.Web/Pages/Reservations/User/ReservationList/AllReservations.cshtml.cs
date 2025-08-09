using HotelTool.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace HotelTool.Web.Pages.Reservations
{
    public class AllReservationsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public AllReservationsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Stores the list of reservations fetched for the logged-in user
        public List<Reservation> Reservations { get; set; } = [];
        // Caches room data to avoid repeated API calls for the same room type
        public Dictionary<Guid, Room> RoomCache { get; set; } = new();
        // Used to display status messages
        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        // Handle GET request to retrieve user reservations
        public async Task OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                Reservations = [];
                return;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ReservationServiceAPI");
                var response = await client.GetAsync($"/api/reservation/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var reservations = JsonSerializer.Deserialize<List<Reservation>>(json, _jsonOptions);
                    Reservations = reservations ?? [];

                    // Load room details for display
                    await LoadRoomDetailsAsync();
                }
                else
                {
                    Reservations = [];
                    StatusMessage = "Unable to load reservations at this time.";
                }
            }
            catch (Exception)
            {
                Reservations = [];
                StatusMessage = "Unable to load reservations at this time.";
            }
        }

        // Handles reservation cancellation requests
        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ReservationServiceAPI");
                var response = await client.PatchAsync($"/api/reservation/{id}/cancel", null);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiSuccessResponse>(json, _jsonOptions);
                    StatusMessage = result?.Message ?? "Reservation canceled successfully.";
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    var errorResult = JsonSerializer.Deserialize<ApiErrorResponse>(errorJson, _jsonOptions);
                    StatusMessage = errorResult?.Message ?? "Failed to cancel reservation.";
                }
            }
            catch (Exception)
            {
                StatusMessage = "An error occurred while canceling the reservation.";
            }

            return RedirectToPage();
        }

        // Helper method to fetch room details by room type ID
        public Room? GetRoomByTypeId(Guid roomTypeId)
        {
            return RoomCache.TryGetValue(roomTypeId, out var room) ? room : null;
        }

        // Load room details for all reservations
        private async Task LoadRoomDetailsAsync()
        {
            try
            {
                var roomClient = _httpClientFactory.CreateClient("RoomServiceAPI");
                var roomIds = Reservations.Select(r => r.RoomTypeId).Distinct().ToList();

                foreach (var roomId in roomIds)
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
                                RoomCache[roomId] = room;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to load room {roomId}: {response.StatusCode} - {response.ReasonPhrase}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception loading room {roomId}: {ex.Message}");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in LoadRoomDetailsAsync: {ex.Message}");
            }
        }
    }
}