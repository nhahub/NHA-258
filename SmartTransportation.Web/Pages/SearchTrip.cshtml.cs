using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public SearchTripModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [BindProperty]
        public string? FromLocation { get; set; }

        [BindProperty]
        public string? ToLocation { get; set; }

        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime? DepartureDate { get; set; }

        [BindProperty]
        [Range(1, 20, ErrorMessage = "Passengers must be at least 1")]
        public int Passengers { get; set; } = 1;

        public bool HasSearched { get; set; }
        public List<TripSearchResult> SearchResults { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            HasSearched = false;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            HasSearched = true;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var apiBase = _config["ApiBaseUrl"]
                          ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

            try
            {
                // Use the form values to call the API
                var queryParams = new Dictionary<string, string?>
                {
                    ["from"] = FromLocation,
                    ["to"] = ToLocation,
                    ["date"] = DepartureDate?.ToString("yyyy-MM-dd"),
                    ["passengers"] = Passengers.ToString()
                };

                var url = QueryHelpers.AddQueryString(
                    $"{apiBase}/api/trips/search",
                    queryParams!
                );

                var trips = await client.GetFromJsonAsync<List<TripSearchResult>>(url);
                SearchResults = trips ?? new List<TripSearchResult>();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error while searching trips: {ex.Message}";
                SearchResults = new List<TripSearchResult>();
            }

            return Page();
        }
    }
}