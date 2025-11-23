using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartTransportation.Web.Helpers;

namespace SmartTransportation.Web.Pages.Driver.Trips
{
    public class RouteDetailsVm
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
    }

    public class RouteListItemVm
    {
        public int RouteId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    public class VehicleVm
    {
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public int SeatsCount { get; set; }
        public bool IsVerified { get; set; }
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

        public List<RouteListItemVm> Routes { get; set; } = new();
        public VehicleVm? Vehicle { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please select a route.")]
        public int SelectedRouteId { get; set; }

        [BindProperty]
        [Required]
        [DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; }

        [BindProperty]
        [Required]
        [DataType(DataType.Time)]
        public TimeSpan DepartureTime { get; set; }

        [BindProperty]
        [Range(1, 100)]
        public int AvailableSeats { get; set; }

        [BindProperty]
        [Range(0.01, 999999)]
        public decimal PricePerSeat { get; set; }

        [BindProperty]
        [StringLength(255)]
        public string? Notes { get; set; }

        private int GetCurrentUserId()
        {
            var id = ClaimsHelper.GetUserId(User);
            if (id == null)
                throw new Exception("Cannot resolve logged-in user ID.");
            return id.Value;
        }

        private string ApiBase =>
            _config["ApiBaseUrl"] ?? throw new Exception("ApiBaseUrl missing in config.");

        private async Task<bool> LoadRoutesAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{ApiBase}/api/Routes");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Failed to load routes (status {response.StatusCode}).";
                    return false;
                }

                var dto = await response.Content.ReadFromJsonAsync<List<RouteDetailsVm>>();
                if (dto == null || !dto.Any())
                {
                    ErrorMessage = "No routes found.";
                    return false;
                }

                Routes = dto.Select(r => new RouteListItemVm
                {
                    RouteId = r.RouteId,
                    DisplayName = !string.IsNullOrWhiteSpace(r.RouteName)
                        ? $"{r.RouteName} ({r.StartLocation} → {r.EndLocation})"
                        : $"{r.StartLocation} → {r.EndLocation}"
                })
                .OrderBy(r => r.DisplayName)
                .ToList();

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading routes: {ex.Message}";
                return false;
            }
        }

        private async Task<bool> LoadVehicleAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // Attach JWT token
                if (Request.Cookies.ContainsKey("AuthToken"))
                {
                    var token = Request.Cookies["AuthToken"];
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                Vehicle = await client.GetFromJsonAsync<VehicleVm>($"{ApiBase}/api/Driver/vehicle");
                return Vehicle != null;
            }
            catch
            {
                Vehicle = null;
                return false;
            }
        }





        public async Task<IActionResult> OnGetAsync()
        {
            await LoadRoutesAsync();
            await LoadVehicleAsync();

            DepartureDate = DateTime.Today.AddDays(1);
            DepartureTime = TimeSpan.FromHours(9);

            AvailableSeats = Vehicle?.SeatsCount > 0 ? Vehicle.SeatsCount : 1;
            PricePerSeat = 50m;

            if (Routes.Any())
                SelectedRouteId = Routes.First().RouteId;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadRoutesAsync();
            await LoadVehicleAsync();

            if (!ModelState.IsValid)
                return Page();

            var client = _httpClientFactory.CreateClient();

            // Attach JWT token from cookie if available
            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                var token = Request.Cookies["AuthToken"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var startLocal = DepartureDate.Date + DepartureTime;
            var startUtc = DateTime.SpecifyKind(startLocal, DateTimeKind.Local).ToUniversalTime();

            var payload = new
            {
                DriverId = GetCurrentUserId(),
                RouteId = SelectedRouteId,
                StartTime = startUtc,
                EndTime = (DateTime?)null,
                PricePerSeat,
                AvailableSeats,
                Notes
            };

            try
            {
                var res = await client.PostAsJsonAsync($"{ApiBase}/api/Trips", payload);
                if (!res.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Failed to create trip ({res.StatusCode}).";
                    return Page();
                }

                SuccessMessage = "Trip created successfully!";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating trip: {ex.Message}";
                return Page();
            }
        }

    }
}
