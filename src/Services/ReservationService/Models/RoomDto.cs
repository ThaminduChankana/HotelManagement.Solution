namespace ReservationService.DTOs
{
    // Data Transfer Object representing room details
    public class RoomDto
    {
        // Unique identifier of the room.
        public Guid Id { get; set; }
        // Name of the room type.
        public string Name { get; set; } = string.Empty;
        // List of features available in the room.
        public List<string> Features { get; set; } = new();
        // Maximum number of guests allowed in the room.
        public int NumberOfGuests { get; set; }
        // Base price of the room.
        public decimal Price { get; set; }
        // Discount percentage on the room price.
        public decimal Discount { get; set; }
        // List of image URLs for the room.
        public List<string> ImageUrls { get; set; } = new();
        // Additional options available for the room.
        public List<string> Options { get; set; } = new();
        // Total number of rooms available of this type.
        public int TotalRooms { get; set; }
        // List of room numbers associated with this room type.
        public List<string> RoomNumbers { get; set; } = new();
        // Price of breakfast for this room.
        public decimal BreakfastPrice { get; set; }
        // Price of lunch for this room.
        public decimal LunchPrice { get; set; }
        // Price of dinner for this room.
        public decimal DinnerPrice { get; set; }
        // Date and time when the room was created.
        public DateTime CreatedAt { get; set; }
        // Date and time when the room details were last updated.
        public DateTime UpdatedAt { get; set; }

        // Computed property for the base price after applying the discount.
        public decimal BasePriceAfterDiscount => Price * (1 - Discount / 100);
        // Calculates the price of the board type (Half/Full) based on selected meals.
        public decimal GetPriceAfterDiscountWithBoard(string boardType)
        {
            decimal mealTotal = 0;
            if (boardType == "Half Board")
                mealTotal = BreakfastPrice + DinnerPrice;
            else if (boardType == "Full Board")
                mealTotal = BreakfastPrice + LunchPrice + DinnerPrice;
            return mealTotal;
        }
        // Full price of the room including full board without applying any discount.
        public decimal NormalRoomPrice => Price + GetPriceAfterDiscountWithBoard("Full Board");

        // Full price of the room including full board after applying the discount.
        public decimal NormalRoomAfterDiscount => BasePriceAfterDiscount + GetPriceAfterDiscountWithBoard("Full Board");
    }
}