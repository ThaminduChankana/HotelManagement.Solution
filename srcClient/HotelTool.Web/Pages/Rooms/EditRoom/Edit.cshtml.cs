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
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Cloudinary _cloudinary;

        // Constructor initializes HttpClientFactory and Cloudinary account
        public EditModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;

            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]);
            _cloudinary = new Cloudinary(account);
        }

        // Bound properties for form inputs
        [BindProperty] public Room Room { get; set; } = new();
        [BindProperty] public List<string> SelectedFeatures { get; set; } = new();
        [BindProperty] public List<string> SelectedOptions { get; set; } = new();
        [BindProperty] public IFormFileCollection? UploadedImages { get; set; }
        [BindProperty] public List<string> RoomNumbers { get; set; } = new();
        [BindProperty] public List<string> ImagesToDelete { get; set; } = new();

        // Dropdown data sources
        public List<SelectListItem> AllFeatures { get; set; } = new();
        public List<SelectListItem> AllOptions { get; set; } = new();

        // Message to pass across redirects
        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        // GET: Fetch room details by ID and populate the form
        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            StatusMessage = string.Empty;

            try
            {
                var httpClient = _httpClientFactory.CreateClient("RoomServiceAPI");
                var response = await httpClient.GetAsync($"/api/rooms/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var room = JsonSerializer.Deserialize<Room>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (room != null)
                    {
                        Room = room;
                        RoomNumbers = Room.RoomNumbers;
                        SelectedFeatures = Room.Features;
                        SelectedOptions = Room.Options;

                        LoadDropdowns();
                        return Page();
                    }
                }

                return RedirectToPage("../ListRoom/RoomList");
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Unable to connect to the room service.";
                return RedirectToPage("../ListRoom/RoomList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToPage("../ListRoom/RoomList");
            }
        }

        // POST: Update the room with new data, handle image upload and deletion
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("RoomServiceAPI");

                // Fetch the existing room to get current images
                var existingResponse = await httpClient.GetAsync($"/api/rooms/{Room.Id}");
                if (!existingResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Failed to load existing room data.");
                    LoadDropdowns();
                    return Page();
                }

                var existingContent = await existingResponse.Content.ReadAsStringAsync();
                var existingRoom = JsonSerializer.Deserialize<Room>(existingContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (existingRoom == null)
                {
                    ModelState.AddModelError("", "Invalid room data received.");
                    LoadDropdowns();
                    return Page();
                }

                // Start with the real existing image list
                var imageUrls = existingRoom.ImageUrls?.ToList() ?? new List<string>();

                // Remove selected images
                if (ImagesToDelete != null && ImagesToDelete.Any())
                {
                    foreach (var imageUrl in ImagesToDelete)
                    {
                        imageUrls.Remove(imageUrl);

                        try
                        {
                            var publicId = GetPublicIdFromUrl(imageUrl);
                            await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to delete image: {imageUrl}. Error: {ex.Message}");
                        }
                    }
                }

                //  Upload new images
                if (UploadedImages != null && UploadedImages.Count > 0)
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

                // Ensure at least one image remains
                if (!imageUrls.Any())
                {
                    ModelState.AddModelError("", "At least one image is required for the room.");
                    LoadDropdowns();
                    return Page();
                }

                // Construct update request with new values
                var updateRoomRequest = new
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

                // Send update request to API
                var jsonContent = JsonSerializer.Serialize(updateRoomRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync($"/api/rooms/{Room.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = "Room updated successfully!";
                    return RedirectToPage("../ListRoom/RoomList");
                }

                // Handle API failure
                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Failed to update room. Server error: {errorContent}");
                LoadDropdowns();
                return Page();
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

        // Populates dropdown lists with available features and options
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

        // Extracts Cloudinary public ID from image URL for deletion
        private string GetPublicIdFromUrl(string url)
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
            var filename = Path.GetFileNameWithoutExtension(path);
            var folder = path.Split("/").Reverse().Skip(1).FirstOrDefault();
            return $"{folder}/{filename}";
        }
    }
}