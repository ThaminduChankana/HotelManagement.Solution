using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReservationService.Models;

namespace ReservationService.Data
{
      public class ReservationContext : DbContext
      {
            // Constructor for injecting options
            public ReservationContext(DbContextOptions<ReservationContext> options) : base(options)
            {
            }
            // DbSet representing the Reservations table
            public DbSet<Reservation> Reservations { get; set; }

            // Model configuration using Fluent API
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  base.OnModelCreating(modelBuilder);

                  // Configure the Reservation entity
                  modelBuilder.Entity<Reservation>(entity =>
                  {
                        // Primary key
                        entity.HasKey(e => e.Id);
                        // Define indexes for better query performance
                        entity.HasIndex(e => e.UserID)
                        .HasDatabaseName("IX_Reservations_UserID");

                        entity.HasIndex(e => e.RoomTypeId)
                        .HasDatabaseName("IX_Reservations_RoomTypeId");

                        entity.HasIndex(e => new { e.CheckInDate, e.CheckOutDate })
                        .HasDatabaseName("IX_Reservations_DateRange");

                        entity.HasIndex(e => e.Status)
                        .HasDatabaseName("IX_Reservations_Status");

                        entity.HasIndex(e => e.Email)
                        .HasDatabaseName("IX_Reservations_Email");

                        entity.HasIndex(e => new { e.RoomTypeId, e.CheckInDate, e.CheckOutDate, e.Status })
                        .HasDatabaseName("IX_Reservations_Availability");

                        // Set max lengths and required fields for string properties
                        entity.Property(e => e.UserID)
                        .HasMaxLength(50);

                        entity.Property(e => e.FirstName)
                        .HasMaxLength(25)
                        .IsRequired();

                        entity.Property(e => e.LastName)
                        .HasMaxLength(25)
                        .IsRequired();

                        entity.Property(e => e.Email)
                        .HasMaxLength(100)
                        .IsRequired();

                        entity.Property(e => e.PhoneNumber)
                        .HasMaxLength(20)
                        .IsRequired();

                        entity.Property(e => e.Country)
                        .HasMaxLength(25)
                        .IsRequired();

                        entity.Property(e => e.BookFor)
                        .HasMaxLength(50);

                        entity.Property(e => e.PayBy)
                        .HasMaxLength(50)
                        .IsRequired();

                        entity.Property(e => e.FullOrHalfBoard)
                        .HasMaxLength(20);

                        entity.Property(e => e.SpecialRequest)
                        .HasMaxLength(500);

                        entity.Property(e => e.AdminNote)
                        .HasMaxLength(500);

                        entity.Property(e => e.AllocatedRoomNumber)
                        .HasMaxLength(10);

                        entity.Property(e => e.CreatedBy)
                        .HasMaxLength(50);

                        entity.Property(e => e.UpdatedBy)
                        .HasMaxLength(50);

                        // Configure decimal precision for price
                        entity.Property(e => e.TotalCost)
                        .HasColumnType("decimal(18,2)");

                        // Configure datetime columns
                        entity.Property(e => e.CheckInDate)
                        .HasColumnType("datetime2");

                        entity.Property(e => e.CheckOutDate)
                        .HasColumnType("datetime2");

                        entity.Property(e => e.CreatedAt)
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                        entity.Property(e => e.UpdatedAt)
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                        // Configure enums as strings
                        entity.Property(e => e.Status)
                        .HasConversion<string>()
                        .HasMaxLength(20)
                        .HasDefaultValue(ReservationStatus.Active);

                        entity.Property(e => e.Recurrence)
                        .HasConversion<string>()
                        .HasMaxLength(20)
                        .HasDefaultValue(RecurrenceType.None);

                        // Configure other default values
                        entity.Property(e => e.RecurrenceCount)
                        .HasDefaultValue(0);

                        entity.Property(e => e.IsWorkRelated)
                        .HasDefaultValue(false);

                        // Add check constraints
                        entity.HasCheckConstraint("CK_Reservation_DateRange",
                      "[CheckOutDate] > [CheckInDate]");

                        entity.HasCheckConstraint("CK_Reservation_TotalCost",
                      "[TotalCost] >= 0");

                        entity.HasCheckConstraint("CK_Reservation_RecurrenceCount",
                      "[RecurrenceCount] >= 0 AND [RecurrenceCount] <= 365");

                        // Set table name
                        entity.ToTable("Reservations");
                  });
            }


            // Override SaveChanges to automatically set timestamps before saving
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
                      .Where(e => e.Entity is Reservation && (e.State == EntityState.Added || e.State == EntityState.Modified));

                  foreach (var entry in entries)
                  {
                        var reservation = (Reservation)entry.Entity;

                        if (entry.State == EntityState.Added)
                        {
                              reservation.CreatedAt = DateTime.UtcNow;
                        }

                        reservation.UpdatedAt = DateTime.UtcNow;
                  }
            }
      }
}