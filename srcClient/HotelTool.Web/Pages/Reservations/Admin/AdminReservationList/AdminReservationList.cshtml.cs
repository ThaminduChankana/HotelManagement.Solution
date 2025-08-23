using HotelTool.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;

namespace HotelTool.Web.Pages.Reservations;

public class AdminReservationListModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public AdminReservationListModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public List<Reservation> Reservations { get; set; } = [];
    [TempData]
    public string StatusMessage { get; set; } = "";

    // GET method to load all reservations if the user is an admin
    public async Task<IActionResult> OnGetAsync()
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
        {
            return RedirectToPage("/AccessDenied");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ReservationServiceAPI");

            // API call to get all the reservations
            var response = await client.GetAsync("/api/reservation");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var reservations = JsonSerializer.Deserialize<List<Reservation>>(json, _jsonOptions);
                Reservations = reservations ?? [];
            }
            else
            {
                StatusMessage = "Unable to load reservations at this time.";
            }
        }
        catch (Exception)
        {
            StatusMessage = "Unable to load reservations at this time.";
            Reservations = [];
        }

        return Page();
    }

    // POST method to allow the admin to update the reservation status and add notes
    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid reservationId, ReservationStatus newStatus, string adminNote)
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
        {
            return RedirectToPage("../../../AccessDenied/AccessDenied");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ReservationServiceAPI");
            var request = new { Status = newStatus, AdminNote = adminNote?.Trim() ?? "" };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // API call to update a reservation by the admin
            var response = await client.PatchAsync($"/api/reservation/{reservationId}/status", content);
            StatusMessage = response.IsSuccessStatusCode ? "Reservation updated successfully." : "Failed to update reservation.";
        }
        catch (Exception)
        {
            StatusMessage = "An error occurred while updating the reservation.";
        }

        return RedirectToPage();
    }
}
