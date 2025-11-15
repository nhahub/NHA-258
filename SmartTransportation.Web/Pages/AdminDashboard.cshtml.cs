using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartTransportation.Web.Pages
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RouteDto
    {
        public int RouteId { get; set; }
        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public double Distance { get; set; }
        public string EstimatedDuration { get; set; } = string.Empty;
        public int TotalSegments { get; set; }
    }

    public class AdminTripDto
    {
        public int TripId { get; set; }
        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public int MaxPassengers { get; set; }
        public int CurrentPassengers { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class AdminDashboardModel : PageModel
    {
        // Overview Statistics
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int TotalTrips { get; set; }
        public int ActiveTrips { get; set; }
        public int TotalRoutes { get; set; }
        public int RouteSegments { get; set; }
        public decimal TotalRevenue { get; set; }

        // Reports Statistics
        public int TotalBookings { get; set; }
        public int CompletedTrips { get; set; }
        public double AverageRating { get; set; }
        public int ActiveDrivers { get; set; }

        // Data Lists
        public List<UserDto> Users { get; set; } = new();
        public List<RouteDto> Routes { get; set; } = new();
        public List<AdminTripDto> Trips { get; set; } = new();

        public void OnGet()
        {
            // TODO: Fetch from database via services
            // var stats = await _adminService.GetDashboardStatsAsync();
            // TotalUsers = stats.TotalUsers;
            // etc.

            // Mock data
            TotalUsers = 1250;
            NewUsersThisMonth = 87;
            TotalTrips = 3456;
            ActiveTrips = 45;
            TotalRoutes = 156;
            RouteSegments = 892;
            TotalRevenue = 125750.00m;

            TotalBookings = 8934;
            CompletedTrips = 3211;
            AverageRating = 4.7;
            ActiveDrivers = 342;

            // Sample users
            Users = new List<UserDto>
            {
                new UserDto
                {
                    UserId = 1,
                    FullName = "Ahmed Mohamed",
                    Email = "ahmed.m@example.com",
                    Role = "Driver",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddMonths(-6)
                },
                new UserDto
                {
                    UserId = 2,
                    FullName = "Sara Ali",
                    Email = "sara.ali@example.com",
                    Role = "Passenger",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddMonths(-3)
                },
                new UserDto
                {
                    UserId = 3,
                    FullName = "Mohamed Hassan",
                    Email = "m.hassan@example.com",
                    Role = "Driver",
                    IsActive = false,
                    CreatedAt = DateTime.Now.AddMonths(-8)
                },
                new UserDto
                {
                    UserId = 4,
                    FullName = "Layla Ibrahim",
                    Email = "layla.i@example.com",
                    Role = "Passenger",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-15)
                },
                new UserDto
                {
                    UserId = 5,
                    FullName = "Khaled Mahmoud",
                    Email = "khaled.m@example.com",
                    Role = "Driver",
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddMonths(-2)
                }
            };

            // Sample routes
            Routes = new List<RouteDto>
            {
                new RouteDto
                {
                    RouteId = 1,
                    FromLocation = "Cairo",
                    ToLocation = "Alexandria",
                    Distance = 220.5,
                    EstimatedDuration = "2h 30m",
                    TotalSegments = 5
                },
                new RouteDto
                {
                    RouteId = 2,
                    FromLocation = "Cairo",
                    ToLocation = "Luxor",
                    Distance = 670.0,
                    EstimatedDuration = "7h 15m",
                    TotalSegments = 12
                },
                new RouteDto
                {
                    RouteId = 3,
                    FromLocation = "Giza",
                    ToLocation = "Hurghada",
                    Distance = 455.0,
                    EstimatedDuration = "5h 45m",
                    TotalSegments = 8
                },
                new RouteDto
                {
                    RouteId = 4,
                    FromLocation = "Alexandria",
                    ToLocation = "Marsa Matrouh",
                    Distance = 290.0,
                    EstimatedDuration = "3h 20m",
                    TotalSegments = 6
                }
            };

            // Sample trips
            Trips = new List<AdminTripDto>
            {
                new AdminTripDto
                {
                    TripId = 1,
                    FromLocation = "Cairo",
                    ToLocation = "Alexandria",
                    DriverName = "Ahmed Mohamed",
                    DepartureDate = DateTime.Now.AddDays(1),
                    MaxPassengers = 4,
                    CurrentPassengers = 3,
                    Price = 150.00m,
                    Status = "Active"
                },
                new AdminTripDto
                {
                    TripId = 2,
                    FromLocation = "Giza",
                    ToLocation = "Luxor",
                    DriverName = "Sara Ali",
                    DepartureDate = DateTime.Now.AddDays(2),
                    MaxPassengers = 6,
                    CurrentPassengers = 4,
                    Price = 350.00m,
                    Status = "Pending"
                },
                new AdminTripDto
                {
                    TripId = 3,
                    FromLocation = "Cairo",
                    ToLocation = "Hurghada",
                    DriverName = "Khaled Mahmoud",
                    DepartureDate = DateTime.Now.AddDays(-1),
                    MaxPassengers = 4,
                    CurrentPassengers = 4,
                    Price = 400.00m,
                    Status = "Active"
                }
            };
        }
    }
}
