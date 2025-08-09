using System;
using System.Collections.Generic;

namespace RoomService.Models
{
    // Data Transfer Object representing a room with pricing and feature details
    public class RoomDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public int NumberOfGuests { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<string> Options { get; set; } = new();
        public int TotalRooms { get; set; }
        public List<string> RoomNumbers { get; set; } = new();
        public decimal BreakfastPrice { get; set; }
        public decimal LunchPrice { get; set; }
        public decimal DinnerPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal BasePriceAfterDiscount => Price * (1 - Discount / 100);
        public decimal GetPriceAfterDiscountWithBoard(string boardType)
        {
            decimal mealTotal = 0;

            if (boardType == "Half Board")
                mealTotal = BreakfastPrice + DinnerPrice;
            else if (boardType == "Full Board")
                mealTotal = BreakfastPrice + LunchPrice + DinnerPrice;

            return mealTotal;
        }
        public decimal NormalRoomPrice => Price + GetPriceAfterDiscountWithBoard("Full Board");
        public decimal NormalRoomAfterDicount => BasePriceAfterDiscount + GetPriceAfterDiscountWithBoard("Full Board");
    }
    // Model representing the request to create a new room
    public class CreateRoomRequest
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public int NumberOfGuests { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<string> Options { get; set; } = new();
        public int TotalRooms { get; set; }
        public List<string> RoomNumbers { get; set; } = new();
        public decimal BreakfastPrice { get; set; }
        public decimal LunchPrice { get; set; }
        public decimal DinnerPrice { get; set; }
    }

    // Model representing the request to update an existing room
    public class UpdateRoomRequest
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public int NumberOfGuests { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<string> Options { get; set; } = new();
        public int TotalRooms { get; set; }
        public List<string> RoomNumbers { get; set; } = new();
        public decimal BreakfastPrice { get; set; }
        public decimal LunchPrice { get; set; }
        public decimal DinnerPrice { get; set; }
    }
}