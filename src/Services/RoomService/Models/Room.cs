using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RoomService.Models
{
    // Represents a hotel room with all its details and pricing logic
    public class Room
    {
        // Primary key for the room (GUID)
        [Key]
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();
        // Room name with a maximum length of 100 characters
        [Required(ErrorMessage = "Room name is required.")]
        [StringLength(100, ErrorMessage = "Room name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;
        // List of main features 
        [Required(ErrorMessage = "Features are required.")]
        public List<string> Features { get; set; } = new();
        // Maximum number of guests allowed (between 2 and 4
        [Required(ErrorMessage = "Number of guests is required.")]
        [Range(2, 4, ErrorMessage = "Number of guests must be between 2 and 4.")]
        public int NumberOfGuests { get; set; }
        // Room base price (between 10,000 and 100,000)
        [Required(ErrorMessage = "Room price is required.")]
        [Range(10000, 100000, ErrorMessage = "Price must be between 10000 and 100000")]
        public decimal Price { get; set; }
        // Discount percentage (0 to 100)
        [Range(0, 100, ErrorMessage = "Discount must be between 0% and 100%.")]
        public decimal Discount { get; set; }
        // List of image URLs for the room
        [Required(ErrorMessage = "Images are required.")]
        public List<string> ImageUrls { get; set; } = new();
        // Additional room options
        [Required(ErrorMessage = "Options are required.")]
        public List<string> Options { get; set; } = new();
        // Total number of physical rooms available for this room type
        [Required(ErrorMessage = "Total rooms count is required.")]
        [Range(1, 100, ErrorMessage = "Total rooms must be between 1 and 100.")]
        public int TotalRooms { get; set; }
        // Specific room numbers
        [Required(ErrorMessage = "Room Numbers are required.")]
        public List<string> RoomNumbers { get; set; } = new();
        // Price of breakfast for this room type
        [Range(1000, 10000, ErrorMessage = "Meal price must be between 1000 and 10000.")]
        public decimal BreakfastPrice { get; set; }
        // Price of lunch for this room type
        [Range(1000, 10000, ErrorMessage = "Meal price must be between 1000 and 10000.")]
        public decimal LunchPrice { get; set; }
        // Price of dinner for this room type
        [Range(1000, 10000, ErrorMessage = "Meal price must be between 1000 and 10000.")]
        public decimal DinnerPrice { get; set; }
        // Timestamp for when the room was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Timestamp for when the room was last updated 
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        // Base price after applying discount (no meals included)
        public decimal BasePriceAfterDiscount => Price * (1 - Discount / 100);
        // Returns additional meal charges based on board type
        public decimal GetPriceAfterDiscountWithBoard(string boardType)
        {
            decimal mealTotal = 0;

            if (boardType == "Half Board")
                mealTotal = BreakfastPrice + DinnerPrice;
            else if (boardType == "Full Board")
                mealTotal = BreakfastPrice + LunchPrice + DinnerPrice;

            return mealTotal;
        }
        // Full room price without discount for the Full Board
        public decimal NormalRoomPrice => Price + GetPriceAfterDiscountWithBoard("Full Board");
        // Full room price after applying discount for the Full Board
        public decimal NormalRoomAfterDiscount => BasePriceAfterDiscount + GetPriceAfterDiscountWithBoard("Full Board");
    }
}