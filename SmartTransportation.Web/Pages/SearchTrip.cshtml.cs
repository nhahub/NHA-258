using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace SmartTransportation.Web.Pages
{
    public class TripSearchResult
    {
        public int TripId { get; set; }
        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public string DepartureTime { get; set; } = string.Empty;
        public int MaxPassengers { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Price { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public double DriverRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class SearchTripModel : PageModel
    {
        [BindProperty]
        public string? FromLocation { get; set; }

        [BindProperty]
        public string? ToLocation { get; set; }

        [BindProperty]
        public DateTime? DepartureDate { get; set; }

        [BindProperty]
        public int Passengers { get; set; } = 1;

        public bool HasSearched { get; set; }
        public List<TripSearchResult> SearchResults { get; set; } = new();

        public void OnGet()
        {
            HasSearched = false;
        }

        public IActionResult OnPost()
        {
            HasSearched = true;

            // TODO: Implement actual search from database
            // var trips = await _tripService.SearchTripsAsync(FromLocation, ToLocation, DepartureDate, Passengers);
            // SearchResults = trips;

            // Mock data for now
            SearchResults = GetMockSearchResults();

            return Page();
        }

        private List<TripSearchResult> GetMockSearchResults()
        {
            var allTrips = new List<TripSearchResult>
            {
                new TripSearchResult
                {
                    TripId = 1,
                    FromLocation = "Cairo",
                    ToLocation = "Alexandria",
                    DepartureDate = DateTime.Now.AddDays(1),
                    DepartureTime = "08:00 AM",
                    MaxPassengers = 4,
                    AvailableSeats = 2,
                    Price = 150.00m,
                    VehicleType = "Sedan",
                    DriverName = "Ahmed Mohamed",
                    DriverRating = 4.8,
                    TotalReviews = 156
                },
                new TripSearchResult
                {
                    TripId = 2,
                    FromLocation = "Cairo",
                    ToLocation = "Alexandria",
                    DepartureDate = DateTime.Now.AddDays(1),
                    DepartureTime = "02:00 PM",
                    MaxPassengers = 6,
                    AvailableSeats = 4,
                    Price = 120.00m,
                    VehicleType = "SUV",
                    DriverName = "Mohamed Ali",
                    DriverRating = 4.6,
                    TotalReviews = 89
                },
                new TripSearchResult
                {
                    TripId = 3,
                    FromLocation = "Giza",
                    ToLocation = "Luxor",
                    DepartureDate = DateTime.Now.AddDays(2),
                    DepartureTime = "06:00 AM",
                    MaxPassengers = 4,
                    AvailableSeats = 3,
                    Price = 350.00m,
                    VehicleType = "Sedan",
                    DriverName = "Sara Ahmed",
                    DriverRating = 4.9,
                    TotalReviews = 234
                },
                new TripSearchResult
                {
                    TripId = 4,
                    FromLocation = "Cairo",
                    ToLocation = "Hurghada",
                    DepartureDate = DateTime.Now.AddDays(3),
                    DepartureTime = "10:00 PM",
                    MaxPassengers = 4,
                    AvailableSeats = 1,
                    Price = 400.00m,
                    VehicleType = "Sedan",
                    DriverName = "Khaled Hassan",
                    DriverRating = 4.7,
                    TotalReviews = 178
                }
            };

            // Filter based on search criteria
            var filtered = allTrips.AsQueryable();

            if (!string.IsNullOrEmpty(FromLocation))
            {
                filtered = filtered.Where(t => t.FromLocation.Contains(FromLocation, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(ToLocation))
            {
                filtered = filtered.Where(t => t.ToLocation.Contains(ToLocation, StringComparison.OrdinalIgnoreCase));
            }

            if (DepartureDate.HasValue)
            {
                filtered = filtered.Where(t => t.DepartureDate.Date == DepartureDate.Value.Date);
            }

            if (Passengers > 0)
            {
                filtered = filtered.Where(t => t.AvailableSeats >= Passengers);
            }

            return filtered.ToList();
        }
    }
}