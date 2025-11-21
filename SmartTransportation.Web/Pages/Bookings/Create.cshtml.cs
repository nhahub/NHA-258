using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartTransportation.Web.Pages.Bookings
{
    // View models for this page
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
        public List<RouteSegmentVm> RouteSegments { get; set; } = new();
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

    // STOP option (city/point), not segment
    public class StopOption
    {
        public int Index { get; set; }      // 0..N
        public string Name { get; set; } = string.Empty;
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

        // ---- Trip & route data ----
        public TripDetailsVm? Trip { get; set; }
        public List<RouteSegmentVm> Segments { get; set; } = new();
        public List<StopOption> Stops { get; set; } = new();

        public string? ErrorMessage { get; set; }

        // ---- Query-bound ----
        [BindProperty(SupportsGet = true)]
        public int TripId { get; set; }

        [BindProperty(SupportsGet = true)]
        [Range(1, 20, ErrorMessage = "Seats must be at least 1.")]
        public int Passengers { get; set; } = 1;

        // ---- Form-bound ----
        [BindProperty]
        [Range(1, 20, ErrorMessage = "Seats must be at least 1.")]
        public int SeatsCount { get; set; } = 1;

        [BindProperty]
        [Required(ErrorMessage = "Please select an origin.")]
        public int FromStopIndex { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please select a destination.")]
        public int ToStopIndex { get; set; }

        // TODO: replace with actual logged-in user id
        private int GetCurrentUserId() => 1;

        // ---------- GET ----------

        public async Task<IActionResult> OnGetAsync()
        {
            if (TripId <= 0)
            {
                ErrorMessage = "Trip not specified.";
                return Page();
            }

            var loaded = await LoadTripAndStopsAsync();
            if (!loaded)
            {
                ErrorMessage = "Unable to load trip details.";
                return Page();
            }

            SeatsCount = Passengers > 0 ? Passengers : 1;

            // Default route: full route (first stop -> last stop)
            if (Stops.Any())
            {
                FromStopIndex = 0;
                ToStopIndex = Stops.Count - 1;
            }

            return Page();
        }

        // ---------- POST ----------

        public async Task<IActionResult> OnPostAsync()
        {
            if (TripId <= 0)
            {
                ErrorMessage = "Trip not specified.";
                return Page();
            }

            var loaded = await LoadTripAndStopsAsync();
            if (!loaded)
            {
                ErrorMessage = "Unable to load trip details.";
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (FromStopIndex < 0 || ToStopIndex <= FromStopIndex || ToStopIndex >= Stops.Count)
            {
                ModelState.AddModelError(string.Empty, "Destination must come after origin.");
                return Page();
            }

            // segments[i] = between stop[i] and stop[i+1]
            // so from = i, to = j  -> segments i..(j-1)
            var orderedSegments = Segments.OrderBy(s => s.SegmentOrder).ToList();

            if (orderedSegments.Count + 1 != Stops.Count)
            {
                ErrorMessage = "Internal route mapping error.";
                return Page();
            }

            var selectedSegmentIds = orderedSegments
                .Skip(FromStopIndex)
                .Take(ToStopIndex - FromStopIndex)
                .Select(s => s.SegmentId)
                .ToList();

            if (!selectedSegmentIds.Any())
            {
                ErrorMessage = "No segments found for the selected route.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var apiBase = _config["ApiBaseUrl"]
                          ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

            var createBookingPayload = new
            {
                tripId = TripId,
                bookerUserId = GetCurrentUserId(),
                seatsCount = SeatsCount,
                totalAmount = 0m, // backend recalculates at payment time
                passengerUserIds = (List<int>?)null,
                segmentIds = selectedSegmentIds
            };

            try
            {
                var response = await client.PostAsJsonAsync(
                    $"{apiBase}/api/bookings",
                    createBookingPayload
                );

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Error while creating booking ({(int)response.StatusCode})";
                    return Page();
                }

                var booking = await response.Content.ReadFromJsonAsync<BookingResponseVm>();
                if (booking == null || booking.BookingId <= 0)
                {
                    ErrorMessage = "Booking created but response was not understood.";
                    return Page();
                }

                // Redirect to payment page
                return RedirectToPage("/Payment/Pay", new { bookingId = booking.BookingId });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error while creating booking: {ex.Message}";
                return Page();
            }
        }

        // ---------- Helpers ----------

        private async Task<bool> LoadTripAndStopsAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var apiBase = _config["ApiBaseUrl"]
                          ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

            try
            {
                Trip = await client.GetFromJsonAsync<TripDetailsVm>($"{apiBase}/api/trips/{TripId}");
                if (Trip?.Route?.RouteSegments == null || !Trip.Route.RouteSegments.Any())
                    return false;

                Segments = Trip.Route.RouteSegments
                    .OrderBy(s => s.SegmentOrder)
                    .ToList();

                // Build stops from segments:
                // stop[0] = first.StartPoint
                // stop[1] = first.EndPoint
                // stop[2] = second.EndPoint, etc.
                Stops = new List<StopOption>();

                var firstSeg = Segments.First();
                Stops.Add(new StopOption { Index = 0, Name = firstSeg.StartPoint });

                int index = 1;
                foreach (var seg in Segments)
                {
                    Stops.Add(new StopOption
                    {
                        Index = index,
                        Name = seg.EndPoint
                    });
                    index++;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
