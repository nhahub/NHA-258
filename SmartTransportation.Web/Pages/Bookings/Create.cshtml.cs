using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartTransportation.Web.Helpers;

namespace SmartTransportation.Web.Pages.Bookings
{
    // ---------------------------
    // View Models
    // ---------------------------
    public class RouteSegmentVm
    {
        public int SegmentId { get; set; }
        public int SegmentOrder { get; set; }
        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;
        public decimal? DistanceKm { get; set; }
    }

    public class RouteVm
    {
        public int RouteId { get; set; }
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public List<RouteSegmentVm>? RouteSegments { get; set; }
        public List<RouteSegmentVm>? Segments { get; set; }
    }

    public class TripDetailsVm
    {
        public int TripId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal PricePerSeat { get; set; }
        public int AvailableSeats { get; set; }
        public string Status { get; set; } = string.Empty;
        public RouteVm? Route { get; set; }
    }

    public class SegmentOption
    {
        public int SegmentId { get; set; }
        public int Order { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class BookingResponseVm
    {
        public int BookingId { get; set; }
    }

    // ---------------------------
    // PageModel
    // ---------------------------
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public CreateModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        // Trip & route data
        public TripDetailsVm? Trip { get; set; }
        public List<RouteSegmentVm> Segments { get; set; } = new();
        public List<SegmentOption> SegmentOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }

        // Query-bound
        [BindProperty(SupportsGet = true)]
        public int TripId { get; set; }

        [BindProperty(SupportsGet = true)]
        [Range(1, 20, ErrorMessage = "Seats must be at least 1.")]
        public int Passengers { get; set; } = 1;

        // Form-bound
        [BindProperty]
        [Range(1, 20, ErrorMessage = "Seats must be at least 1.")]
        public int SeatsCount { get; set; } = 1;

        [BindProperty]
        [Required(ErrorMessage = "Please select at least one segment.")]
        public List<int> SelectedSegmentIds { get; set; } = new();

        // Current logged-in user
        private int? CurrentUserId => ClaimsHelper.GetUserId(User);

        // ---------------------------
        // GET
        // ---------------------------
        public async Task<IActionResult> OnGetAsync()
        {
            if (!await LoadTripAndSegmentsAsync())
            {
                ErrorMessage = "Unable to load trip details.";
                return Page();
            }

            SeatsCount = Passengers > 0 ? Passengers : 1;

            // Default: select all segments
            SelectedSegmentIds = SegmentOptions
                .OrderBy(s => s.Order)
                .Select(s => s.SegmentId)
                .ToList();

            return Page();
        }

        // ---------------------------
        // POST
        // ---------------------------
        public async Task<IActionResult> OnPostAsync()
        {
            if (CurrentUserId == null)
            {
                ErrorMessage = "You must be logged in to book a trip.";
                return Page();
            }

            if (!await LoadTripAndSegmentsAsync())
            {
                ErrorMessage = "Unable to load trip details.";
                return Page();
            }

            if (!ModelState.IsValid)
                return Page();

            // Validate selected segments
            var validIds = SegmentOptions.Select(s => s.SegmentId).ToHashSet();
            SelectedSegmentIds = SelectedSegmentIds.Where(id => validIds.Contains(id)).Distinct().ToList();

            if (!SelectedSegmentIds.Any())
            {
                ErrorMessage = "No valid segments selected.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiBase = _config["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl not configured.");

                // Add JWT token from cookie
                var jwtToken = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(jwtToken))
                {
                    ErrorMessage = "User token not found. Please login.";
                    return Page();
                }
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                var payload = new
                {
                    tripId = TripId,
                    bookerUserId = CurrentUserId.Value,
                    seatsCount = SeatsCount,
                    totalAmount = 0m, // backend calculates
                    passengerUserIds = (List<int>?)null,
                    segmentIds = SelectedSegmentIds
                };

                var response = await client.PostAsJsonAsync($"{apiBase}/api/bookings", payload);
                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Error while creating booking ({(int)response.StatusCode}).";
                    return Page();
                }

                var booking = await response.Content.ReadFromJsonAsync<BookingResponseVm>();
                if (booking == null || booking.BookingId <= 0)
                {
                    ErrorMessage = "Booking created but response was invalid.";
                    return Page();
                }

                return RedirectToPage("/Payment/Pay", new { bookingId = booking.BookingId });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error while creating booking: {ex.Message}";
                return Page();
            }
        }

        // ---------------------------
        // Helper: load trip info & segments
        // ---------------------------
        private async Task<bool> LoadTripAndSegmentsAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiBase = _config["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl not configured.");

                // Add JWT token for GET requests
                var jwtToken = Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(jwtToken))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                Trip = await client.GetFromJsonAsync<TripDetailsVm>($"{apiBase}/api/trips/{TripId}");
                if (Trip?.Route == null) return false;

                var rawSegments = Trip.Route.RouteSegments ?? Trip.Route.Segments;
                if (rawSegments == null || !rawSegments.Any()) return false;

                // Populate segments and options
                Segments = rawSegments.OrderBy(s => s.SegmentOrder).ToList();
                SegmentOptions = Segments.Select(s => new SegmentOption
                {
                    SegmentId = s.SegmentId,
                    Order = s.SegmentOrder,
                    Label = $"{s.StartPoint} → {s.EndPoint} ({(s.DistanceKm ?? 0):0.0} km)"
                }).OrderBy(o => o.Order).ToList();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
