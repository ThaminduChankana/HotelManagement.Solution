using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoomService.Models;

namespace RoomService.Data
{
    public class RoomContext : DbContext
    {
        public RoomContext(DbContextOptions<RoomContext> options) : base(options)
        {
        }
        // DbSet representing the Rooms table
        public DbSet<Room> Rooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure the Room entity
            modelBuilder.Entity<Room>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);
                // Unique index on Name
                entity.HasIndex(e => e.Name)
                      .IsUnique()
                      .HasDatabaseName("IX_Rooms_Name");
                // Name property constraints
                entity.Property(e => e.Name)
                      .HasMaxLength(100)
                      .IsRequired();
                // Convert Features list to comma-separated string in DB
                entity.Property(e => e.Features)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
                // Convert ImageUrls list to comma-separated string in DB
                entity.Property(e => e.ImageUrls)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
                // Convert Options list to comma-separated string in DB
                entity.Property(e => e.Options)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
                // Convert RoomNumbers list to comma-separated string in DB
                entity.Property(e => e.RoomNumbers)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
                // Configure decimal precision for price-related properties
                entity.Property(e => e.Price)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Discount)
                      .HasColumnType("decimal(5,2)");

                entity.Property(e => e.BreakfastPrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.LunchPrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.DinnerPrice)
                      .HasColumnType("decimal(18,2)");
                // Default timestamp values (created/updated)
                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
                // Set table name
                entity.ToTable("Rooms");
            });

            // Call method to seed initial data into the database
            SeedData(modelBuilder);
        }

        // Seeds predefined room data into the database
        private void SeedData(ModelBuilder modelBuilder)
        {
            var staticDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Room>().HasData(
                new Room
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Deluxe Double",
                    Features = new() { "WiFi", "19m²", "Air Conditioning", "Smart TV", "Sea View", "Private Balcony" },
                    NumberOfGuests = 2,
                    Price = 35000,
                    Discount = 10,
                     ImageUrls = new() { "https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751611214/Deluxe_Double_evv1bi.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751620602/512776456_kbl9gc.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634312/512785655_khsxtc.jpg", "https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634312/512785654_wckoc2.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634312/512785660_xsq4ki.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634312/512776444_bwh65n.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634312/512785661_d7hhnk.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634312/512785657_zwvltz.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634313/512776423_tdasme.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634314/512785656_rv4j9u.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634315/512785659_gf29av.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634315/512785662_oi04wx.jpg"},
                    Options = new() { "Breakfast Included", "Airport Pickup", "Non-refundable", "Pay the property before arrival" },
                    TotalRooms = 5,
                    BreakfastPrice = 5500,
                    LunchPrice = 7000,
                    DinnerPrice = 7500,
                    RoomNumbers = new() { "101", "102", "103", "104", "105" },
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Room
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Deluxe King",
                    Features = new() { "WiFi", "19m²", "Smart TV", "City View" },
                    NumberOfGuests = 2,
                    Price = 45000,
                    Discount = 5,
                   ImageUrls = new() { "https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634969/512657784_ugzgua.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634968/512657779_viohnb.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634968/512657665_o48np4.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634968/512657706_lqcku2.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634968/512657744_hf1ol5.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634969/512657710_u3czsh.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634969/512657404_gwaon4.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634970/512657641_hopnrb.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634970/512657636_gfdf8t.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634970/512657754_pbidju.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634970/512657797_rka41a.jpg", "https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634971/512657804_oeolbp.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634971/512657805_nne36c.jpg","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751634976/512657818_omxdgg.jpg"},
                    Options = new() { "Breakfast Included", "Non-refundable", "Pay the property before arrival" },
                    TotalRooms = 4,
                    BreakfastPrice = 6500,
                    LunchPrice = 7500,
                    DinnerPrice = 8000,
                    RoomNumbers = new() { "301", "302", "303", "304" },
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Room
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Standard Double",
                    Features = new() { "WiFi", "Smart TV", "Mini Fridge", "Room Service", "Garden View" },
                    NumberOfGuests = 2,
                    Price = 25000,
                    Discount = 15,
                    ImageUrls = new() { "https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751650546/26166157_gljocd.webp","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751650546/35365136_n3tgkc.webp","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751650547/37344774_lcgsvj.webp","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751650547/37344776_czwlvu.webp","https://res.cloudinary.com/dfnqhfdyw/image/upload/v1751650548/37344738_ziucqp.webp" },
                    Options = new() { "Breakfast Included", "Late Checkout", "Airport Pickup", "Non-refundable", "Pay the property before arrival" },
                    TotalRooms = 1,
                    BreakfastPrice = 5000,
                    LunchPrice = 6000,
                    DinnerPrice = 6500,
                    RoomNumbers = new() { "201" },
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                }
            );
        }
        // Overrides SaveChanges to update UpdatedAt timestamp
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }
        // Overrides SaveChangesAsync to update UpdatedAt timestamp
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }
        // Automatically sets the UpdatedAt field to current UTC time when entities are modified
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Room && (e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                ((Room)entry.Entity).UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}