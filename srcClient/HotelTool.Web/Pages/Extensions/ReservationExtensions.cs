using HotelTool.Web.Models;

namespace HotelTool.Web.Extensions
{
    // Provides extension methods for Reservation and ReservationStatus to support UI rendering and business logic
    public static class ReservationExtensions
    {
        // Returns a user-friendly string representation of the reservation status
        public static string GetStatusDisplayName(this ReservationStatus status)
        {
            return status switch
            {
                ReservationStatus.Active => "Active",
                ReservationStatus.Canceled => "Canceled",
                ReservationStatus.CheckedIn => "Checked In",
                ReservationStatus.Completed => "Completed",
                _ => status.ToString()
            };
        }

        // Returns the appropriate CSS class name for a status badge based on the reservation status
        public static string GetStatusBadgeClass(this ReservationStatus status)
        {
            return status switch
            {
                ReservationStatus.Active => "badge bg-success",
                ReservationStatus.Canceled => "badge bg-danger",
                ReservationStatus.CheckedIn => "badge bg-primary",
                ReservationStatus.Completed => "badge bg-secondary",
                _ => "badge bg-light"
            };
        }

        // Determines whether the reservation can be canceled
        public static bool CanBeCanceled(this Reservation reservation)
        {
            return reservation.Status == ReservationStatus.Active &&
                   (reservation.CheckInDate - DateTime.Now).TotalHours >= 48;
        }

        // Determines whether the reservation can be modified
        public static bool CanBeModified(this Reservation reservation)
        {
            return reservation.Status == ReservationStatus.Active &&
                   reservation.CheckInDate.Date > DateTime.Today;
        }

        // Calculates the number of nights for a reservation
        public static int GetNumberOfNights(this Reservation reservation)
        {
            var nights = (reservation.CheckOutDate.Date - reservation.CheckInDate.Date).Days;
            return nights > 0 ? nights : 1;
        }

        // Concatenates and returns the guest's full name
        public static string GetGuestName(this Reservation reservation)
        {
            return $"{reservation.FirstName} {reservation.LastName}".Trim();
        }
    }
}
