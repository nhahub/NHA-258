using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartTransportation.Web.Pages
{
    public class TripDto
    {
        public int TripId { get; set; }
        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public string DepartureTime { get; set; } = string.Empty;
        public int MaxPassengers { get; set; }
        public int CurrentPassengers { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "Upcoming"; // Upcoming, Active, Completed
    }

    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class DriverDashboardModel : PageModel
    {
        public string DriverName { get; set; } = "Ahmed";
        public int TotalTrips { get; set; }
        public int UpcomingTrips { get; set; }
        public int ActiveBookings { get; set; }
        public decimal TotalEarnings { get; set; }

        public List<TripDto> Trips { get; set; } = new();
        public List<NotificationDto> Notifications { get; set; } = new();

        public void OnGet()
        {
            // TODO: Fetch from database via services
            // var driverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // var driver = await _driverService.GetDriverByIdAsync(driverId);
            // DriverName = driver.FirstName;

            // Mock data for now
            TotalTrips = 45;
            UpcomingTrips = 5;
            ActiveBookings = 12;
            TotalEarnings = 15750.00m;

            // Sample trips
            Trips = new List<TripDto>
            {
                new TripDto
                {
                    TripId = 1,
                    FromLocation = "Cairo",
                    ToLocation = "Alexandria",
                    DepartureDate = DateTime.Now.AddDays(1),
                    DepartureTime = "08:00 AM",
                    MaxPassengers = 4,
                    CurrentPassengers = 2,
                    Price = 150.00m,
                    Status = "Upcoming"
                },
                new TripDto
                {
                    TripId = 2,
                    FromLocation = "Giza",
                    ToLocation = "Luxor",
                    DepartureDate = DateTime.Now.AddDays(3),
                    DepartureTime = "06:00 PM",
                    MaxPassengers = 6,
                    CurrentPassengers = 4,
                    Price = 350.00m,
                    Status = "Upcoming"
                },
                new TripDto
                {
                    TripId = 3,
                    FromLocation = "Cairo",
                    ToLocation = "Hurghada",
                    DepartureDate = DateTime.Now.AddDays(-2),
                    DepartureTime = "09:00 AM",
                    MaxPassengers = 4,
                    CurrentPassengers = 4,
                    Price = 400.00m,
                    Status = "Completed"
                }
            };

            // Sample notifications
            Notifications = new List<NotificationDto>
            {
                new NotificationDto
                {
                    NotificationId = 1,
                    Message = "New booking for your Cairo → Alexandria trip",
                    CreatedAt = DateTime.Now.AddHours(-2),
                    IsRead = false
                },
                new NotificationDto
                {
                    NotificationId = 2,
                    Message = "Payment received: 150 EGP",
                    CreatedAt = DateTime.Now.AddHours(-5),
                    IsRead = false
                },
                new NotificationDto
                {
                    NotificationId = 3,
                    Message = "Trip reminder: Departure in 24 hours",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    IsRead = true
                }
            };
        }
    }
}