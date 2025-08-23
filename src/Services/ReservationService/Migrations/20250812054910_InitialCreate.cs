using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BookFor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsWorkRelated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PayBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullOrHalfBoard = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SpecialRequest = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    AdminNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Recurrence = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "None"),
                    RecurrenceCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AllocatedRoomNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.CheckConstraint("CK_Reservation_DateRange", "[CheckOutDate] > [CheckInDate]");
                    table.CheckConstraint("CK_Reservation_RecurrenceCount", "[RecurrenceCount] >= 0 AND [RecurrenceCount] <= 365");
                    table.CheckConstraint("CK_Reservation_TotalCost", "[TotalCost] >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Availability",
                table: "Reservations",
                columns: new[] { "RoomTypeId", "CheckInDate", "CheckOutDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_DateRange",
                table: "Reservations",
                columns: new[] { "CheckInDate", "CheckOutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Email",
                table: "Reservations",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_RoomTypeId",
                table: "Reservations",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Status",
                table: "Reservations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserID",
                table: "Reservations",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");
        }
    }
}
