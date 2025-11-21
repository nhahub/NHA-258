using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartTransportation.Web.Pages.Bookings
{
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


    // What the view uses to show each segment option
    public class SegmentOption
    {
        public int SegmentId { get; set; }
        public int Order { get; set; }
        public string Label { get; set; } = string.Empty; // e.g. "Alexandria → Tanta (80.0 km)"
    }

    public class BookingResponseVm
    {
        public int BookingId { get; set; }
    }

    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public CreateModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        // Trip & route data (for display)
        public TripDetailsVm? Trip { get; set; }
        public List<RouteSegmentVm> Segments { get; set; } = new();
        public List<SegmentOption> SegmentOptions { get; set; } = new();

        public string? ErrorMessage { get; set; }

        // Query-bound (trip & initial passengers from search page)
        [BindProperty(SupportsGet = true)]
        public int TripId { get; set; }

        [BindProperty(SupportsGet = true)]
        [Range(1, 20, ErrorMessage = "Seats must be at least 1.")]
        public int Passengers { get; set; } = 1;

        // Form-bound
        [BindProperty]
        [Range(1, 20, ErrorMessage = "Seats must be at least 1.")]
        public int SeatsCount { get; set; } = 1;

        // MULTI-SELECT: all chosen segment IDs come here
        [BindProperty]
        [Required(ErrorMessage = "Please select at least one segment.")]
        public List<int> SelectedSegmentIds { get; set; } = new();

        // Get current logged user id from claims
        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                         ?? User.FindFirst("sub")
                         ?? User.FindFirst("userId");

            if (idClaim != null && int.TryParse(idClaim.Value, out var id))
                return id;

            throw new InvalidOperationException("Logged-in user id not found in claims.");
        }

        // ---------- GET ----------
        public async Task<IActionResult> OnGetAsync()
        {
            // Fallback: read from query string manually if binding missed it
            if (TripId <= 0)
            {
                var qTrip = Request.Query["tripId"].ToString();
                if (int.TryParse(qTrip, out var parsedTripId))
                    TripId = parsedTripId;
            }

            if (TripId <= 0)
            {
                ErrorMessage = "Trip not specified.";
                return Page();
            }

            if (Passengers <= 0)
            {
                var qPassengers = Request.Query["passengers"].ToString();
                if (int.TryParse(qPassengers, out var parsedPassengers))
                    Passengers = parsedPassengers;
            }

            var loaded = await LoadTripAndSegmentsAsync();
            if (!loaded)
            {
                ErrorMessage = "Unable to load trip details.";
                return Page();
            }

            SeatsCount = Passengers > 0 ? Passengers : 1;

            // default selection: all segments (whole route)
            if (SegmentOptions.Any())
            {
                SelectedSegmentIds = SegmentOptions
                    .OrderBy(s => s.Order)
                    .Select(s => s.SegmentId)
                    .ToList();
            }

            return Page();
        }

        // ---------- POST ----------
        public async Task<IActionResult> OnPostAsync()
        {
            // Same TripId fallback on POST
            if (TripId <= 0)
            {
                var qTrip = Request.Query["tripId"].ToString();
                if (int.TryParse(qTrip, out var parsedTripId))
                    TripId = parsedTripId;
            }

            if (TripId <= 0)
            {
                ErrorMessage = "Trip not specified.";
                return Page();
            }

            var loaded = await LoadTripAndSegmentsAsync();
            if (!loaded)
            {
                ErrorMessage = "Unable to load trip details.";
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!SegmentOptions.Any())
            {
                ErrorMessage = "No route segments available for this trip.";
                return Page();
            }

            if (SelectedSegmentIds == null || !SelectedSegmentIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one segment.");
                return Page();
            }

            // Ensure all selected IDs actually exist on this route
            var validIds = SegmentOptions.Select(s => s.SegmentId).ToHashSet();
            var cleanedSegmentIds = SelectedSegmentIds
                .Where(id => validIds.Contains(id))
                .Distinct()
                .ToList();

            if (!cleanedSegmentIds.Any())
            {
                ErrorMessage = "No valid segments selected.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var apiBase = _config["ApiBaseUrl"]
                          ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

            var payload = new
            {
                tripId = TripId,
                bookerUserId = GetCurrentUserId(),
                seatsCount = SeatsCount,
                totalAmount = 0m,              // backend calculates later
                passengerUserIds = (List<int>?)null,
                segmentIds = cleanedSegmentIds
            };

            try
            {
                var response = await client.PostAsJsonAsync(
                    $"{apiBase}/api/bookings",
                    payload
                );

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Error while creating booking ({(int)response.StatusCode}).";
                    return Page();
                }

                var booking = await response.Content.ReadFromJsonAsync<BookingResponseVm>();
                if (booking == null || booking.BookingId <= 0)
                {
                    ErrorMessage = "Booking created but response was not understood.";
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

        // load trip + segments, and build SegmentOptions for the view
        private async Task<bool> LoadTripAndSegmentsAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var apiBase = _config["ApiBaseUrl"]
                          ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

            try
            {
                // Fallback: pull tripId from query if not bound
                if (TripId <= 0)
                {
                    var qTrip = Request.Query["tripId"].ToString();
                    if (int.TryParse(qTrip, out var parsedTripId))
                        TripId = parsedTripId;
                }

                if (TripId <= 0)
                    return false;

                Trip = await client.GetFromJsonAsync<TripDetailsVm>($"{apiBase}/api/trips/{TripId}");
                if (Trip?.Route == null)
                    return false;

                // Try both possible JSON property names
                var rawSegments = Trip.Route.RouteSegments;
                if (rawSegments == null || !rawSegments.Any())
                    rawSegments = Trip.Route.Segments;

                if (rawSegments == null || !rawSegments.Any())
                    return false;

                Segments = rawSegments
                    .OrderBy(s => s.SegmentOrder)
                    .ToList();

                SegmentOptions = Segments
                    .Select(s => new SegmentOption
                    {
                        SegmentId = s.SegmentId,
                        Order = s.SegmentOrder,
                        Label = $"{s.StartPoint} → {s.EndPoint} ({(s.DistanceKm ?? 0):0.0} km)"
                    })
                    .OrderBy(o => o.Order)
                    .ToList();

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}

