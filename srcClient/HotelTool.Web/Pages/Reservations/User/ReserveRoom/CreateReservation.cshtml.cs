using HotelTool.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text;

namespace HotelTool.Web.Pages.Reservations
{
    public class CreateReservationModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        // Constructor to initialize dependencies
        public CreateReservationModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Bound property to bind form data from the Razor Page
        [BindProperty]
        public Reservation Reservation { get; set; } = new();
        // List to hold room types for dropdown selection
        public List<SelectListItem> RoomTypes { get; set; } = new();
        // Property to hold detailed information about the selected room
        public Room? SelectedRoom { get; set; }
        // Message to display availability status
        public string AvailabilityMessage { get; set; } = string.Empty;

        // Called on GET request (initial page load)
        public async Task OnGetAsync(Guid? roomTypeId = null)
        {
            await LoadRoomTypesAsync();

            if (roomTypeId != null && roomTypeId != Guid.Empty)
            {
                Reservation.RoomTypeId = roomTypeId.Value;

                // Load the selected room details for display
                await LoadSelectedRoomAsync(roomTypeId.Value);
            }
        }

        // Called on POST request (when form is submitted)
        public async Task<IActionResult> OnPostAsync()
        {
            await LoadRoomTypesAsync();

            // If room is pre-selected, load its details again
            if (Reservation.RoomTypeId != Guid.Empty)
            {
                await LoadSelectedRoomAsync(Reservation.RoomTypeId);
            }

            if (!ModelState.IsValid)
                return Page();

            if (Reservation.CheckOutDate <= Reservation.CheckInDate)
            {
                ModelState.AddModelError(string.Empty, "Check-out date must be after check-in date.");
                return Page();
            }

            // Validate that check-in date is not in the past
            if (Reservation.CheckInDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("Reservation.CheckInDate", "Check-in date cannot be in the past.");
                return Page();
            }

            try
            {
                // Check availability before creating reservation
                var client = _httpClientFactory.CreateClient("ReservationServiceAPI");
                var availabilityQuery = $"?roomTypeId={Reservation.RoomTypeId}&checkInDate={Reservation.CheckInDate:yyyy-MM-dd}&checkOutDate={Reservation.CheckOutDate:yyyy-MM-dd}&boardType={Uri.EscapeDataString(Reservation.FullOrHalfBoard ?? "")}";
                Console.WriteLine(availabilityQuery);
                var availabilityResponse = await client.GetAsync($"/api/reservation/availability{availabilityQuery}");

                if (availabilityResponse.IsSuccessStatusCode)
                {
                    var availabilityJson = await availabilityResponse.Content.ReadAsStringAsync();
                    var availability = JsonSerializer.Deserialize<AvailabilityResponse>(availabilityJson, _jsonOptions);

                    if (availability?.IsAvailable != true)
                    {
                        ModelState.AddModelError("", "Selected room type is not available for the chosen dates. Please select different dates.");
                        return Page();
                    }
                }

                // Create the reservation request
                var request = new CreateReservationRequest
                {
                    UserID = HttpContext.Session.GetString("UserId"),
                    RoomTypeId = Reservation.RoomTypeId,
                    FirstName = Reservation.FirstName,
                    LastName = Reservation.LastName,
                    Email = Reservation.Email,
                    PhoneNumber = Reservation.PhoneNumber,
                    Country = Reservation.Country,
                    BookFor = Reservation.BookFor,
                    IsWorkRelated = Reservation.IsWorkRelated,
                    CheckInDate = Reservation.CheckInDate,
                    CheckOutDate = Reservation.CheckOutDate,
                    PayBy = Reservation.PayBy,
                    SpecialRequest = Reservation.SpecialRequest,
                    FullOrHalfBoard = Reservation.FullOrHalfBoard ?? string.Empty,
                    Recurrence = Reservation.Recurrence,
                    RecurrenceCount = Reservation.RecurrenceCount
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/reservation", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Reservation created successfully!";
                    return RedirectToPage("/Reservations/User/ReservationList/AllReservations");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, _jsonOptions);
                    ModelState.AddModelError(string.Empty, errorResponse?.Message ?? "Failed to create reservation.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the reservation. Please try again.");
                return Page();
            }
        }

        // Loads room types from Room API
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

                    var selectedRoom = RoomTypes.FirstOrDefault(r => r.Selected);
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

        // Load selected room details for display
        private async Task LoadSelectedRoomAsync(Guid roomId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("RoomServiceAPI");
                var response = await httpClient.GetAsync($"/api/rooms/{roomId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var room = JsonSerializer.Deserialize<Room>(json, _jsonOptions);

                    if (room != null)
                    {
                        RoomTypes = new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = room.Id.ToString(),
                        Text = room.Name,
                        Selected = true
                    }
                };
                    }
                    else
                    {
                        RoomTypes = new List<SelectListItem>();
                    }
                }
                else
                {
                    RoomTypes = new List<SelectListItem>();
                }
            }
            catch (Exception ex)
            {
                RoomTypes = new List<SelectListItem>();
            }
        }

    }
}