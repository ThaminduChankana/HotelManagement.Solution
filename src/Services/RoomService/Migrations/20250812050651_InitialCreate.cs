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
                    { new Guid("11111111-1111-1111-1111-111111111111"), 5500m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7500m, 10m, "WiFi,19m²,Air Conditioning,Smart TV,Sea View,Private Balcony", "https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751611214/Deluxe_Double_evv1bi.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751620602/512776456_kbl9gc.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634312/512785655_khsxtc.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634312/512785654_wckoc2.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634312/512785660_xsq4ki.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634312/512776444_bwh65n.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634312/512785661_d7hhnk.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634312/512785657_zwvltz.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634313/512776423_tdasme.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634314/512785656_rv4j9u.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634315/512785659_gf29av.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634315/512785662_oi04wx.jpg", 7000m, "Deluxe Double", 2, "Breakfast Included,Airport Pickup,Non-refundable,Pay the property before arrival", 35000m, "101,102,103,104,105", 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 6500m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8000m, 5m, "WiFi,19m²,Smart TV,City View", "https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634969/512657784_ugzgua.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634968/512657779_viohnb.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634968/512657665_o48np4.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634968/512657706_lqcku2.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634968/512657744_hf1ol5.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634969/512657710_u3czsh.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634969/512657404_gwaon4.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634970/512657641_hopnrb.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634970/512657636_gfdf8t.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634970/512657754_pbidju.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634970/512657797_rka41a.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634971/512657804_oeolbp.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634971/512657805_nne36c.jpg,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634976/512657818_omxdgg.jpg", 7500m, "Deluxe King", 2, "Breakfast Included,Non-refundable,Pay the property before arrival", 45000m, "301,302,303,304", 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 5000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6500m, 15m, "WiFi,Smart TV,Mini Fridge,Room Service,Garden View", "https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751650546/26166157_gljocd.webp,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751650546/35365136_n3tgkc.webp,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751650547/37344774_lcgsvj.webp,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751650547/37344776_czwlvu.webp,https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751650548/37344738_ziucqp.webp", 6000m, "Standard Double", 2, "Breakfast Included,Late Checkout,Airport Pickup,Non-refundable,Pay the property before arrival", 25000m, "201", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
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
