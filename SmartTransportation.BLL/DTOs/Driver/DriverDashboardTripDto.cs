using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTransportation.BLL.DTOs.Driver
{
    public class DriverDashboardTripDto
    {
        public int TripId { get; set; }
        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public string DepartureTime { get; set; } = string.Empty;
        public int MaxPassengers { get; set; }
        public int CurrentPassengers { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "Upcoming";
    }

    public class DriverDashboardNotificationDto
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class DriverDashboardDto
    {
        public string DriverName { get; set; } = string.Empty;
        public int TotalTrips { get; set; }
        public int UpcomingTrips { get; set; }
        public int ActiveBookings { get; set; }
        public decimal TotalEarnings { get; set; }

        public List<DriverDashboardTripDto> Trips { get; set; } = new();
        public List<DriverDashboardNotificationDto> Notifications { get; set; } = new();
    }
}
