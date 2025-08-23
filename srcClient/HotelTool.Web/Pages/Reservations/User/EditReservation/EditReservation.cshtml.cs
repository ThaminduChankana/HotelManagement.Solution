using HotelTool.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text;

namespace HotelTool.Web.Pages.ReserveRoom.EditReservation;

public class EditReservationModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public EditReservationModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // Binds the reservation model from the form
    [BindProperty]
    public Reservation Reservation { get; set; } = new();
    // List of available room types to populate dropdown
    public List<SelectListItem> RoomTypes { get; set; } = new();
    // TempData to hold status messages across requests
    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    // Handles GET request to load an existing reservation for editing
    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ReservationServiceAPI");
            var response = await client.GetAsync($"/api/reservation/{id}");

            if (!response.IsSuccessStatusCode)
            {
                StatusMessage = "Reservation not found.";
                return RedirectToPage("../ReservationList/AllReservations");
            }

            var json = await response.Content.ReadAsStringAsync();
            var existing = JsonSerializer.Deserialize<Reservation>(json, _jsonOptions);

            if (existing == null)
            {
                StatusMessage = "Reservation not found.";
                return RedirectToPage("../ReservationList/AllReservations");
            }

            // Populate the Reservation property with data from API
            Reservation = new Reservation
            {
                Id = existing.Id,
                FirstName = existing.FirstName,
                LastName = existing.LastName,
                Email = existing.Email,
                PhoneNumber = existing.PhoneNumber,
                Country = existing.Country,
                RoomTypeId = existing.RoomTypeId,
                FullOrHalfBoard = existing.FullOrHalfBoard,
                CheckInDate = existing.CheckInDate,
                CheckOutDate = existing.CheckOutDate,
                SpecialRequest = existing.SpecialRequest,
                PayBy = existing.PayBy,
                IsWorkRelated = existing.IsWorkRelated,
                TotalCost = existing.TotalCost,
                BookFor = existing.BookFor
            };

            await LoadRoomTypesAsync();
            return Page();
        }
        catch (Exception)
        {
            StatusMessage = "Error loading reservation.";
            return RedirectToPage("../ReservationList/AllReservations");
        }
    }

    // Handles POST request to update the reservation
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadRoomTypesAsync();
            return Page();
        }

        if (Reservation.CheckOutDate <= Reservation.CheckInDate)
        {
            ModelState.AddModelError("Reservation.CheckOutDate", "Check-out date must be after check-in date.");
            await LoadRoomTypesAsync();
            return Page();
        }

        // Validate check-in date is not in the past
        if (Reservation.CheckInDate.Date < DateTime.Today)
        {
            ModelState.AddModelError("Reservation.CheckInDate", "Check-in date cannot be in the past.");
            await LoadRoomTypesAsync();
            return Page();
        }

        try
        {
            // Check availability first
            var client = _httpClientFactory.CreateClient("ReservationServiceAPI");
            var availabilityQuery = $"?roomTypeId={Reservation.RoomTypeId}&checkInDate={Reservation.CheckInDate:yyyy-MM-dd}&checkOutDate={Reservation.CheckOutDate:yyyy-MM-dd}&boardType={Uri.EscapeDataString(Reservation.FullOrHalfBoard)}&excludeReservationId={Reservation.Id}";
            var availabilityResponse = await client.GetAsync($"/api/reservation/availability{availabilityQuery}");

            // Handle room availability check
            if (availabilityResponse.IsSuccessStatusCode)
            {
                var availabilityJson = await availabilityResponse.Content.ReadAsStringAsync();
                var availability = JsonSerializer.Deserialize<AvailabilityResponse>(availabilityJson, _jsonOptions);

                if (availability?.IsAvailable != true)
                {
                    ModelState.AddModelError("", "Selected room type is not available for the new dates.");
                    await LoadRoomTypesAsync();
                    return Page();
                }
            }

            // Update reservation request
            var updateRequest = new UpdateReservationRequest
            {
                RoomTypeId = Reservation.RoomTypeId,
                FirstName = Reservation.FirstName?.Trim() ?? string.Empty,
                LastName = Reservation.LastName?.Trim() ?? string.Empty,
                Email = Reservation.Email?.Trim() ?? string.Empty,
                PhoneNumber = Reservation.PhoneNumber?.Trim() ?? string.Empty,
                Country = Reservation.Country?.Trim() ?? string.Empty,
                FullOrHalfBoard = Reservation.FullOrHalfBoard,
                CheckInDate = Reservation.CheckInDate,
                CheckOutDate = Reservation.CheckOutDate,
                SpecialRequest = Reservation.SpecialRequest?.Trim() ?? string.Empty,
                PayBy = Reservation.PayBy,
                IsWorkRelated = Reservation.IsWorkRelated,
                BookFor = Reservation.BookFor
            };
            // Serialize and send PUT request to update the reservation
            var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // API call to update a reservation
            var response = await client.PutAsync($"/api/reservation/{Reservation.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                StatusMessage = "Reservation updated successfully!";
                return RedirectToPage("../ReservationList/AllReservations");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, _jsonOptions);
                ModelState.AddModelError("", errorResponse?.Message ?? "Failed to update reservation.");
                await LoadRoomTypesAsync();
                return Page();
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "An error occurred while updating the reservation. Please try again.");
            await LoadRoomTypesAsync();
            return Page();
        }
    }

    // Loads the list of available room types from Room API
    private async Task LoadRoomTypesAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("RoomServiceAPI");
            var response = await client.GetAsync("/api/rooms");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var rooms = JsonSerializer.Deserialize<List<Room>>(json, _jsonOptions);

                RoomTypes = rooms?.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name,
                    Selected = r.Id == Reservation.RoomTypeId
                }).ToList() ?? new List<SelectListItem>();
            }
            else
            {
                RoomTypes = new List<SelectListItem>();
            }
        }
        catch (Exception)
        {
            RoomTypes = new List<SelectListItem>();
        }
    }
}