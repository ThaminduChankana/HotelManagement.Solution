using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Text.Json;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HotelTool.Web.Models;

namespace HotelTool.Web.Pages.Rooms
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Cloudinary _cloudinary;

        // Inject HttpClient and configure Cloudinary for image uploads
        public CreateModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;

            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]);
            _cloudinary = new Cloudinary(account);
        }

        // Properties bound to form input
        [BindProperty] public RoomCreateRequest Room { get; set; } = new();
        [BindProperty] public List<string> SelectedFeatures { get; set; } = new();
        [BindProperty] public List<string> SelectedOptions { get; set; } = new();
        [BindProperty] public IFormFileCollection UploadedImages { get; set; } = default!;
        [BindProperty] public List<string> RoomNumbers { get; set; } = new();

        // Dropdown data sources
        public List<SelectListItem> AllFeatures { get; set; } = new();
        public List<SelectListItem> AllOptions { get; set; } = new();

        // TempData to persist success message after redirect
        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        // Handles GET request to render form and load dropdown data
        public void OnGet()
        {
            LoadDropdowns();
            StatusMessage = string.Empty;
        }

        // Handles POST request when form is submitted
        public async Task<IActionResult> OnPostAsync()
        {
            // Validate the form model
            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return Page();
            }

            try
            {
                // Upload each image file to Cloudinary and store the secure URLs
                var imageUrls = new List<string>();
                if (UploadedImages != null)
                {
                    foreach (var file in UploadedImages)
                    {
                        if (file.Length > 0)
                        {
                            await using var stream = file.OpenReadStream();
                            var uploadParams = new ImageUploadParams
                            {
                                File = new FileDescription(file.FileName, stream),
                                Folder = "hotel_rooms"
                            };
                            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                imageUrls.Add(uploadResult.SecureUrl.ToString());
                            }
                        }
                    }
                }

                // Create payload for the room creation API request
                var createRoomRequest = new
                {
                    Name = Room.Name.Trim(),
                    Features = SelectedFeatures,
                    NumberOfGuests = Room.NumberOfGuests,
                    Price = Room.Price,
                    Discount = Room.Discount,
                    ImageUrls = imageUrls,
                    Options = SelectedOptions,
                    TotalRooms = Room.TotalRooms,
                    RoomNumbers = RoomNumbers.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
                    BreakfastPrice = Room.BreakfastPrice,
                    LunchPrice = Room.LunchPrice,
                    DinnerPrice = Room.DinnerPrice
                };

                // Send POST request to backend API
                var httpClient = _httpClientFactory.CreateClient("RoomServiceAPI");
                var jsonContent = JsonSerializer.Serialize(createRoomRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                // API call to create a new room
                var response = await httpClient.PostAsync("/api/rooms", content);

                // Check API response status
                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = "Room created successfully!";
                    return RedirectToPage("../ListRoom/RoomList");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "Failed to create room. Please try again.");
                    LoadDropdowns();
                    return Page();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "Unable to connect to the room service.");
                LoadDropdowns();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                LoadDropdowns();
                return Page();
            }
        }

        // Load all available features and options into dropdown lists
        private void LoadDropdowns()
        {
            AllFeatures = new List<SelectListItem>
            {
                new("WiFi", "WiFi"),
                new("Smart TV", "Smart TV"),
                new("Air Conditioning", "Air Conditioning"),
                new("Mini Fridge", "Mini Fridge"),
                new("Room Service", "Room Service"),
                new("Private Bathroom", "Private Bathroom"),
                new("Soundproofing", "Soundproofing"),
                new("Flat-screen TV", "Flat-screen TV"),
                new("Sea View", "Sea View"),
                new("City View", "City View"),
                new("Garden View", "Garden View"),
                new("Private Balcony", "Private Balcony"),
            };

            AllOptions = new List<SelectListItem>
            {
                new("Free Cancellation", "Free Cancellation"),
                new("Non-Refundable", "Non-Refundable"),
                new("Pay the property before arrival", "Pay the property before arrival"),
                new("Breakfast Included", "Breakfast Included"),
                new("Airport Pickup", "Airport Pickup"),
                new("Parking", "Parking"),
                new("Late Checkout", "Late Checkout")
            };
        }
    }
}