using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

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

    // This matches /api/Driver/dashboard response (DriverDashboardDto)
    public class DriverDashboardVm
    {
        public string DriverName { get; set; } = string.Empty;
        public int TotalTrips { get; set; }
        public int UpcomingTrips { get; set; }
        public int ActiveBookings { get; set; }
        public decimal TotalEarnings { get; set; }
        public List<TripDto> Trips { get; set; } = new();
        public List<NotificationDto> Notifications { get; set; } = new();
    }

    public class DriverDashboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public DriverDashboardModel(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        private string ApiBase =>
            _config["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl missing in config.");

        public string DriverName { get; set; } = "Driver";
        public int TotalTrips { get; set; }
        public int UpcomingTrips { get; set; }
        public int ActiveBookings { get; set; }
        public decimal TotalEarnings { get; set; }

        public List<TripDto> Trips { get; set; } = new();
        public List<NotificationDto> Notifications { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDashboardAsync();
        }

        private async Task LoadDashboardAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // 🔐 Same auth pattern as other pages: JWT from AuthToken cookie
                string? token = null;

                if (Request.Cookies.ContainsKey("AuthToken"))
                {
                    token = Request.Cookies["AuthToken"];
                }
                else
                {
                    // Optional fallback if you ever keep JWT in session
                    token = HttpContext.Session.GetString("JwtToken");
                }

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"{ApiBase}/api/Driver/dashboard");
                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Failed to load dashboard (status {response.StatusCode}).";
                    return;
                }

                var dto = await response.Content.ReadFromJsonAsync<DriverDashboardVm>();
                if (dto == null)
                {
                    ErrorMessage = "Failed to parse dashboard data.";
                    return;
                }

                DriverName = dto.DriverName;
                TotalTrips = dto.TotalTrips;
                UpcomingTrips = dto.UpcomingTrips;
                ActiveBookings = dto.ActiveBookings;
                TotalEarnings = dto.TotalEarnings;
                Trips = dto.Trips ?? new();
                Notifications = dto.Notifications ?? new();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading dashboard: {ex.Message}";
            }
        }
    }
}
