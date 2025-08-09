using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RoomService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Features = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ImageUrls = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalRooms = table.Column<int>(type: "int", nullable: false),
                    RoomNumbers = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BreakfastPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LunchPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DinnerPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "BreakfastPrice", "CreatedAt", "DinnerPrice", "Discount", "Features", "ImageUrls", "LunchPrice", "Name", "NumberOfGuests", "Options", "Price", "RoomNumbers", "TotalRooms", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 5500m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7500m, 10m, "WiFi,19m²,Air Conditioning,Smart TV,Sea View,Private Balcony", "https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751611214/Deluxe_Double_evv1bi.jpg", 7000m, "Deluxe Double", 2, "Breakfast Included,Airport Pickup,Non-refundable,Pay the property before arrival", 35000m, "101,102,103,104,105", 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 6500m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8000m, 5m, "WiFi,19m²,Smart TV,City View", "https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634969/512657784_ugzgua.jpg", 7500m, "Deluxe King", 2, "Breakfast Included,Non-refundable,Pay the property before arrival", 45000m, "301,302,303,304", 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 5000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6500m, 15m, "WiFi,Smart TV,Mini Fridge,Room Service,Garden View", "https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751650546/26166157_gljocd.webp", 6000m, "Standard Double", 2, "Breakfast Included,Late Checkout,Airport Pickup,Non-refundable,Pay the property before arrival", 25000m, "201", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Name",
                table: "Rooms",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rooms");
        }
    }
}
