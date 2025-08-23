using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using HotelTool.Web.Models;

namespace HotelTool.Web.Pages.Rooms
{
    public class RoomListModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Constructor to inject the HTTP client factory
        public RoomListModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        // Stores the list of rooms retrieved from the API
        public List<Room> Rooms { get; set; } = new();
        // Indicates whether the logged-in user is an admin
        public bool IsAdmin { get; set; }
        // Bound property to hold the RoomId for deletion
        [BindProperty]
        public required string RoomId { get; set; }

        // Called when the page is first loaded (GET request)
        public async Task OnGetAsync()
        {
            // Check user role from session
            var role = HttpContext.Session.GetString("Role");
            IsAdmin = role == "Admin";

            try
            {
                // Create an HTTP client using the named client "RoomServiceAPI"
                var httpClient = _httpClientFactory.CreateClient("RoomServiceAPI");
                
                // Make a GET request to retrieve all rooms
                var response = await httpClient.GetAsync("/api/rooms");

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response content into a list of Rooms
                    var content = await response.Content.ReadAsStringAsync();
                    var rooms = JsonSerializer.Deserialize<List<Room>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    Rooms = rooms ?? new List<Room>();
                }
                else
                {
                    Rooms = new List<Room>();
                    TempData["ErrorMessage"] = "Unable to load rooms. Please try again later.";
                }
            }
            catch (HttpRequestException)
            {
                Rooms = new List<Room>();
                TempData["ErrorMessage"] = "Unable to connect to the room service. Please make sure the RoomService is running.";
            }
            catch (Exception ex)
            {
                Rooms = new List<Room>();
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }
        }

        // Called when a POST request is made to delete a room
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(RoomId))
                {
                    var httpClient = _httpClientFactory.CreateClient("RoomServiceAPI");
                    
                    // Send a DELETE request to remove the specified room
                    var response = await httpClient.DeleteAsync($"/api/rooms/{RoomId}");

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Room deleted successfully.";
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = "Failed to delete room. It may have active reservations.";
                    }
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Unable to connect to the room service.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToPage();
        }
    }

}